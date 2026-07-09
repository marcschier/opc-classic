/*
 * SPDX-License-Identifier: MIT
 * Copyright (c) 2026 Opc.Classic .NET Contributors
 * Minimal native OPC DA 2.05a out-of-process smoke server for docker interop runs.
 */
#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <windows.h>
#include <comcat.h>
#include <oleauto.h>
#include <strsafe.h>
#include <algorithm>
#include <atomic>
#include <chrono>
#include <cmath>
#include <cstdio>
#include <cstring>
#include <cwchar>
#include <iterator>
#include <mutex>
#include <new>
#include <random>
#include <string>
#include <thread>
#include <utility>
#include <vector>
#include "opcda.h"
#include "opccomn.h"
#include "opcerror.h"

namespace {
constexpr wchar_t kProgId[] = L"OPC.SampleServer.1";
constexpr wchar_t kVerProgId[] = L"OPC.SampleServer";
constexpr wchar_t kDescription[] = L"OPC Classic Docker Sample Server";
constexpr LCID kLocale = MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US), SORT_DEFAULT);
const CLSID kClsid = {0xe53b21c7,0x990e,0x11d3,{0xb3,0xe4,0x00,0xc0,0x4f,0x8e,0xce,0xaa}};
const CATID kCatDa10 = {0x63d5f430,0xcfe4,0x11d1,{0xb2,0xc8,0x00,0x60,0x08,0x3b,0xa1,0xfb}};
const CATID kCatDa20 = {0x63d5f432,0xcfe4,0x11d1,{0xb2,0xc8,0x00,0x60,0x08,0x3b,0xa1,0xfb}};

struct CSampleItem {
    std::wstring id; VARTYPE type = VT_EMPTY; VARIANT value; WORD quality = OPC_QUALITY_GOOD; FILETIME timestamp{};
    CSampleItem() { VariantInit(&value); }
    ~CSampleItem() { VariantClear(&value); }
    CSampleItem(const CSampleItem&) = delete; CSampleItem& operator=(const CSampleItem&) = delete;
};
CSampleItem g_tags[3]; std::mutex g_tagLock; std::atomic<bool> g_stopTags{false}; std::thread g_tagThread;
FILETIME g_startTime{}; HANDLE g_shutdownEvent = nullptr;

LPWSTR DupString(const std::wstring& s) {
    auto bytes = static_cast<SIZE_T>((s.size() + 1) * sizeof(wchar_t));
    auto* p = static_cast<LPWSTR>(CoTaskMemAlloc(bytes)); if (p) memcpy(p, s.c_str(), bytes); return p;
}
std::wstring GuidString(REFGUID id) { wchar_t b[40]{}; StringFromGUID2(id, b, static_cast<int>(std::size(b))); return b; }
HRESULT AllocErrors(DWORD n, HRESULT** e) {
    if (!e) return E_POINTER; *e = static_cast<HRESULT*>(CoTaskMemAlloc(sizeof(HRESULT) * n)); if (!*e) return E_OUTOFMEMORY;
    for (DWORD i = 0; i < n; ++i) (*e)[i] = S_OK; return S_OK;
}
HRESULT CopyCoerce(const VARIANT& src, VARTYPE requested, VARIANT* dst) {
    VariantInit(dst); HRESULT hr = VariantCopy(dst, const_cast<VARIANT*>(&src)); if (FAILED(hr)) return hr;
    if (requested == VT_EMPTY || requested == src.vt) return S_OK;
    VARIANT coerced; VariantInit(&coerced); hr = VariantChangeType(&coerced, dst, 0, requested); VariantClear(dst);
    if (FAILED(hr)) return OPC_E_BADTYPE; *dst = coerced; return S_OK;
}
void PutTag(CSampleItem& tag, double sinValue, bool squareValue, int randomValue) {
    VariantClear(&tag.value); VariantInit(&tag.value);
    if (tag.type == VT_R8) { tag.value.vt = VT_R8; tag.value.dblVal = sinValue; }
    else if (tag.type == VT_BOOL) { tag.value.vt = VT_BOOL; tag.value.boolVal = squareValue ? VARIANT_TRUE : VARIANT_FALSE; }
    else if (tag.type == VT_I4) { tag.value.vt = VT_I4; tag.value.lVal = randomValue; }
    tag.quality = OPC_QUALITY_GOOD; GetSystemTimeAsFileTime(&tag.timestamp);
}
void InitializeTags() {
    std::lock_guard<std::mutex> g(g_tagLock);
    g_tags[0].id = L"Sin"; g_tags[0].type = VT_R8; g_tags[1].id = L"Square"; g_tags[1].type = VT_BOOL; g_tags[2].id = L"Random"; g_tags[2].type = VT_I4;
    PutTag(g_tags[0], 0.0, false, 0); PutTag(g_tags[1], 0.0, false, 0); PutTag(g_tags[2], 0.0, false, 0);
}
int FindTag(LPCWSTR id) {
    if (!id || !*id) return -1;
    for (int i = 0; i < 3; ++i) if (_wcsicmp(g_tags[i].id.c_str(), id) == 0) return i;
    return -1;
}
HRESULT ReadTag(int index, VARTYPE requested, VARIANT* value, WORD* quality, FILETIME* timestamp) {
    std::lock_guard<std::mutex> g(g_tagLock); HRESULT hr = CopyCoerce(g_tags[index].value, requested, value); if (FAILED(hr)) return hr;
    if (quality) *quality = g_tags[index].quality; if (timestamp) *timestamp = g_tags[index].timestamp; return S_OK;
}
HRESULT CheckType(int index, VARTYPE requested) { if (requested == VT_EMPTY) return S_OK; VARIANT v; HRESULT hr = ReadTag(index, requested, &v, nullptr, nullptr); if (SUCCEEDED(hr)) VariantClear(&v); return hr; }
HRESULT WriteTag(int index, VARIANT* value) {
    std::lock_guard<std::mutex> g(g_tagLock); VARIANT coerced; VariantInit(&coerced);
    HRESULT hr = VariantChangeType(&coerced, value, 0, g_tags[index].type); if (FAILED(hr)) return OPC_E_BADTYPE;
    VariantClear(&g_tags[index].value); g_tags[index].value = coerced; g_tags[index].quality = OPC_QUALITY_GOOD; GetSystemTimeAsFileTime(&g_tags[index].timestamp); return S_OK;
}
void StartTagThread() {
    GetSystemTimeAsFileTime(&g_startTime); g_stopTags = false;
    g_tagThread = std::thread([] {
        auto start = std::chrono::steady_clock::now(); std::mt19937 rng{std::random_device{}()}; std::uniform_int_distribution<int> dist(-100, 100);
        while (!g_stopTags.load()) {
            double seconds = std::chrono::duration<double>(std::chrono::steady_clock::now() - start).count();
            { std::lock_guard<std::mutex> g(g_tagLock);
              PutTag(g_tags[0], std::sin(seconds * 2.0 * 3.14159265358979323846 / 10.0), false, 0);
              PutTag(g_tags[1], 0.0, (static_cast<int>(seconds) % 2) == 0, 0); PutTag(g_tags[2], 0.0, false, dist(rng)); }
            Sleep(100);
        }
    });
}
void StopTagThread() { g_stopTags = true; if (g_tagThread.joinable()) g_tagThread.join(); }
std::wstring ErrorText(HRESULT hr) {
    switch (hr) {
    case S_OK: return L"The operation completed successfully."; case S_FALSE: return L"The operation completed with per-item errors.";
    case E_INVALIDARG: return L"One or more arguments are invalid."; case E_NOTIMPL: return L"The requested optional OPC feature is not implemented.";
    case OPC_E_INVALIDHANDLE: return L"The item or group server handle is invalid."; case OPC_E_BADTYPE: return L"The requested data type cannot be returned.";
    case OPC_E_BADRIGHTS: return L"The item does not support the requested access rights."; case OPC_E_UNKNOWNITEMID: return L"The item ID is unknown.";
    case OPC_E_INVALIDITEMID: return L"The item ID is invalid."; default: wchar_t b[128]{}; StringCchPrintfW(b, std::size(b), L"HRESULT 0x%08X", static_cast<unsigned int>(hr)); return b;
    }
}

struct GroupItem { OPCHANDLE server = 0, client = 0; std::wstring id; int tag = -1; BOOL active = TRUE; VARTYPE requested = VT_EMPTY; };
class CSampleGroup final : public IOPCGroupStateMgt, public IOPCItemMgt, public IOPCSyncIO {
public:
    CSampleGroup(OPCHANDLE handle, std::wstring name, BOOL active, DWORD rate, OPCHANDLE client, DWORD lcid)
        : _handle(handle), _name(std::move(name)), _active(active), _rate(rate), _client(client), _lcid(lcid) {}
    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER; *ppv = nullptr;
        if (riid == IID_IUnknown || riid == __uuidof(IOPCGroupStateMgt)) *ppv = static_cast<IOPCGroupStateMgt*>(this);
        else if (riid == __uuidof(IOPCItemMgt)) *ppv = static_cast<IOPCItemMgt*>(this);
        else if (riid == __uuidof(IOPCSyncIO)) *ppv = static_cast<IOPCSyncIO*>(this); else return E_NOINTERFACE;
        AddRef(); return S_OK;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return static_cast<ULONG>(InterlockedIncrement(&_refs)); }
    ULONG STDMETHODCALLTYPE Release() override { ULONG r = static_cast<ULONG>(InterlockedDecrement(&_refs)); if (!r) delete this; return r; }
    ~CSampleGroup() = default;
    OPCHANDLE Handle() const { return _handle; }
    bool NameIs(LPCWSTR name) { std::lock_guard<std::mutex> g(_lock); return name && _wcsicmp(_name.c_str(), name) == 0; }
    HRESULT STDMETHODCALLTYPE GetState(DWORD* rate, BOOL* active, LPWSTR* name, LONG* bias, FLOAT* deadband, DWORD* lcid, OPCHANDLE* client, OPCHANDLE* server) override {
        if (!rate || !active || !name || !bias || !deadband || !lcid || !client || !server) return E_POINTER;
        std::lock_guard<std::mutex> g(_lock); *rate = _rate; *active = _active; *name = DupString(_name); if (!*name) return E_OUTOFMEMORY;
        *bias = 0; *deadband = 0.0f; *lcid = _lcid; *client = _client; *server = _handle; return S_OK;
    }
    HRESULT STDMETHODCALLTYPE SetState(DWORD* requestedRate, DWORD* revisedRate, BOOL* active, LONG*, FLOAT*, DWORD* lcid, OPCHANDLE* client) override {
        std::lock_guard<std::mutex> g(_lock); if (requestedRate) _rate = *requestedRate ? *requestedRate : 100; if (revisedRate) *revisedRate = _rate;
        if (active) _active = *active; if (lcid) _lcid = *lcid; if (client) _client = *client; return S_OK;
    }
    HRESULT STDMETHODCALLTYPE SetName(LPCWSTR name) override { if (!name || !*name) return E_INVALIDARG; std::lock_guard<std::mutex> g(_lock); _name = name; return S_OK; }
    HRESULT STDMETHODCALLTYPE CloneGroup(LPCWSTR, REFIID, LPUNKNOWN*) override { return E_NOTIMPL; }
    HRESULT STDMETHODCALLTYPE AddItems(DWORD n, OPCITEMDEF* defs, OPCITEMRESULT** results, HRESULT** errors) override {
        if (!n || !defs || !results || !errors) return E_INVALIDARG; *results = nullptr; *errors = nullptr;
        auto* r = static_cast<OPCITEMRESULT*>(CoTaskMemAlloc(sizeof(OPCITEMRESULT) * n)); if (!r) return E_OUTOFMEMORY; ZeroMemory(r, sizeof(OPCITEMRESULT) * n);
        HRESULT hr = AllocErrors(n, errors); if (FAILED(hr)) { CoTaskMemFree(r); return hr; } *results = r; bool failed = false; std::lock_guard<std::mutex> g(_lock);
        for (DWORD i = 0; i < n; ++i) { (*errors)[i] = FillResult(defs[i], &r[i]); if (FAILED((*errors)[i])) { failed = true; continue; }
            GroupItem item{_nextItem++, defs[i].hClient, defs[i].szItemID, FindTag(defs[i].szItemID), defs[i].bActive, defs[i].vtRequestedDataType}; _items.push_back(item); r[i].hServer = item.server; }
        return failed ? S_FALSE : S_OK;
    }
    HRESULT STDMETHODCALLTYPE ValidateItems(DWORD n, OPCITEMDEF* defs, BOOL, OPCITEMRESULT** results, HRESULT** errors) override {
        if (!n || !defs || !results || !errors) return E_INVALIDARG; *results = nullptr; *errors = nullptr;
        auto* r = static_cast<OPCITEMRESULT*>(CoTaskMemAlloc(sizeof(OPCITEMRESULT) * n)); if (!r) return E_OUTOFMEMORY; ZeroMemory(r, sizeof(OPCITEMRESULT) * n);
        HRESULT hr = AllocErrors(n, errors); if (FAILED(hr)) { CoTaskMemFree(r); return hr; } *results = r; bool failed = false;
        for (DWORD i = 0; i < n; ++i) { (*errors)[i] = FillResult(defs[i], &r[i]); r[i].hServer = 0; failed = failed || FAILED((*errors)[i]); } return failed ? S_FALSE : S_OK;
    }
    HRESULT STDMETHODCALLTYPE RemoveItems(DWORD n, OPCHANDLE* handles, HRESULT** errors) override {
        return ForItems(n, handles, errors, [this](GroupItem& item, DWORD) { _items.erase(Find(item.server)); return S_OK; });
    }
    HRESULT STDMETHODCALLTYPE SetActiveState(DWORD n, OPCHANDLE* handles, BOOL active, HRESULT** errors) override {
        return ForItems(n, handles, errors, [active](GroupItem& item, DWORD) { item.active = active; return S_OK; });
    }
    HRESULT STDMETHODCALLTYPE SetClientHandles(DWORD n, OPCHANDLE* handles, OPCHANDLE* clients, HRESULT** errors) override {
        if (!clients) return E_INVALIDARG; return ForItems(n, handles, errors, [clients](GroupItem& item, DWORD i) { item.client = clients[i]; return S_OK; });
    }
    HRESULT STDMETHODCALLTYPE SetDatatypes(DWORD n, OPCHANDLE* handles, VARTYPE* types, HRESULT** errors) override {
        if (!types) return E_INVALIDARG; return ForItems(n, handles, errors, [types](GroupItem& item, DWORD i) { HRESULT hr = CheckType(item.tag, types[i]); if (SUCCEEDED(hr)) item.requested = types[i]; return hr; });
    }
    HRESULT STDMETHODCALLTYPE CreateEnumerator(REFIID, LPUNKNOWN*) override { return E_NOTIMPL; }
    HRESULT STDMETHODCALLTYPE Read(OPCDATASOURCE, DWORD n, OPCHANDLE* handles, OPCITEMSTATE** values, HRESULT** errors) override {
        if (!n || !handles || !values || !errors) return E_INVALIDARG; *values = nullptr; *errors = nullptr;
        auto* v = static_cast<OPCITEMSTATE*>(CoTaskMemAlloc(sizeof(OPCITEMSTATE) * n)); if (!v) return E_OUTOFMEMORY; ZeroMemory(v, sizeof(OPCITEMSTATE) * n);
        HRESULT hr = AllocErrors(n, errors); if (FAILED(hr)) { CoTaskMemFree(v); return hr; } *values = v; bool failed = false; std::lock_guard<std::mutex> g(_lock);
        for (DWORD i = 0; i < n; ++i) { VariantInit(&v[i].vDataValue); auto it = Find(handles[i]); if (it == _items.end()) { (*errors)[i] = OPC_E_INVALIDHANDLE; failed = true; continue; }
            v[i].hClient = it->client; (*errors)[i] = ReadTag(it->tag, it->requested, &v[i].vDataValue, &v[i].wQuality, &v[i].ftTimeStamp); failed = failed || FAILED((*errors)[i]); }
        return failed ? S_FALSE : S_OK;
    }
    HRESULT STDMETHODCALLTYPE Write(DWORD n, OPCHANDLE* handles, VARIANT* values, HRESULT** errors) override {
        if (!n || !handles || !values || !errors) return E_INVALIDARG; HRESULT hr = AllocErrors(n, errors); if (FAILED(hr)) return hr; bool failed = false; std::lock_guard<std::mutex> g(_lock);
        for (DWORD i = 0; i < n; ++i) { auto it = Find(handles[i]); (*errors)[i] = (it == _items.end()) ? OPC_E_INVALIDHANDLE : WriteTag(it->tag, &values[i]); failed = failed || FAILED((*errors)[i]); }
        return failed ? S_FALSE : S_OK;
    }
private:
    using Iter = std::vector<GroupItem>::iterator;
    Iter Find(OPCHANDLE handle) { return std::find_if(_items.begin(), _items.end(), [handle](const GroupItem& i) { return i.server == handle; }); }
    HRESULT FillResult(const OPCITEMDEF& def, OPCITEMRESULT* r) {
        int tag = FindTag(def.szItemID); if (tag < 0) return OPC_E_INVALIDITEMID; HRESULT hr = CheckType(tag, def.vtRequestedDataType); if (FAILED(hr)) return hr;
        r->vtCanonicalDataType = g_tags[tag].type; r->dwAccessRights = OPC_READABLE | OPC_WRITEABLE; r->dwBlobSize = 0; r->pBlob = nullptr; return S_OK;
    }
    template<class F> HRESULT ForItems(DWORD n, OPCHANDLE* handles, HRESULT** errors, F action) {
        if (!n || !handles || !errors) return E_INVALIDARG; HRESULT hr = AllocErrors(n, errors); if (FAILED(hr)) return hr; bool failed = false; std::lock_guard<std::mutex> g(_lock);
        for (DWORD i = 0; i < n; ++i) { auto it = Find(handles[i]); (*errors)[i] = (it == _items.end()) ? OPC_E_INVALIDHANDLE : action(*it, i); failed = failed || FAILED((*errors)[i]); } return failed ? S_FALSE : S_OK;
    }
    volatile LONG _refs = 1; OPCHANDLE _handle, _client, _nextItem = 1; std::wstring _name; BOOL _active; DWORD _rate, _lcid; std::vector<GroupItem> _items; std::mutex _lock;
};

class CSampleServer final : public IOPCServer, public IOPCCommon {
public:
    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override {
        if (!ppv) return E_POINTER; *ppv = nullptr; if (riid == IID_IUnknown || riid == __uuidof(IOPCServer)) *ppv = static_cast<IOPCServer*>(this);
        else if (riid == __uuidof(IOPCCommon)) *ppv = static_cast<IOPCCommon*>(this); else return E_NOINTERFACE; AddRef(); return S_OK;
    }
    ULONG STDMETHODCALLTYPE AddRef() override { return static_cast<ULONG>(InterlockedIncrement(&_refs)); }
    ULONG STDMETHODCALLTYPE Release() override { ULONG r = static_cast<ULONG>(InterlockedDecrement(&_refs)); if (!r) delete this; return r; }
    HRESULT STDMETHODCALLTYPE AddGroup(LPCWSTR name, BOOL active, DWORD requestedRate, OPCHANDLE client, LONG*, FLOAT*, DWORD lcid, OPCHANDLE* server, DWORD* revisedRate, REFIID riid, LPUNKNOWN* unk) override {
        if (!server || !revisedRate || !unk) return E_POINTER; *unk = nullptr; { std::lock_guard<std::mutex> g(_lock); *server = _nextGroup++; }
        *revisedRate = requestedRate ? requestedRate : 100; std::wstring groupName = (name && *name) ? name : (L"Group" + std::to_wstring(*server));
        auto* group = new (std::nothrow) CSampleGroup(*server, groupName, active, *revisedRate, client, lcid); if (!group) return E_OUTOFMEMORY;
        HRESULT hr = group->QueryInterface(riid, reinterpret_cast<void**>(unk)); if (FAILED(hr)) { group->Release(); return hr; }
        std::lock_guard<std::mutex> g(_lock); _groups.push_back(group); return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetErrorString(HRESULT error, LCID, LPWSTR* text) override { return ErrorString(error, text); }
    HRESULT STDMETHODCALLTYPE GetGroupByName(LPCWSTR name, REFIID riid, LPUNKNOWN* unk) override {
        if (!name || !unk) return E_POINTER; *unk = nullptr; std::lock_guard<std::mutex> g(_lock);
        for (auto* group : _groups) if (group->NameIs(name)) return group->QueryInterface(riid, reinterpret_cast<void**>(unk)); return E_INVALIDARG;
    }
    HRESULT STDMETHODCALLTYPE GetStatus(OPCSERVERSTATUS** status) override {
        if (!status) return E_POINTER; *status = static_cast<OPCSERVERSTATUS*>(CoTaskMemAlloc(sizeof(OPCSERVERSTATUS))); if (!*status) return E_OUTOFMEMORY; ZeroMemory(*status, sizeof(OPCSERVERSTATUS));
        (*status)->ftStartTime = g_startTime; GetSystemTimeAsFileTime(&(*status)->ftCurrentTime); (*status)->ftLastUpdateTime = (*status)->ftCurrentTime; (*status)->dwServerState = OPC_STATUS_RUNNING;
        (*status)->wMajorVersion = 2; (*status)->wMinorVersion = 5; (*status)->wBuildNumber = 1; (*status)->szVendorInfo = DupString(L"Opc.Classic native Docker MVP"); if (!(*status)->szVendorInfo) { CoTaskMemFree(*status); *status = nullptr; return E_OUTOFMEMORY; }
        std::lock_guard<std::mutex> g(_lock); (*status)->dwGroupCount = static_cast<DWORD>(_groups.size()); return S_OK;
    }
    HRESULT STDMETHODCALLTYPE RemoveGroup(OPCHANDLE handle, BOOL) override {
        std::lock_guard<std::mutex> g(_lock); auto it = std::find_if(_groups.begin(), _groups.end(), [handle](CSampleGroup* x) { return x->Handle() == handle; });
        if (it == _groups.end()) return OPC_E_INVALIDHANDLE; (*it)->Release(); _groups.erase(it); return S_OK;
    }
    HRESULT STDMETHODCALLTYPE CreateGroupEnumerator(OPCENUMSCOPE, REFIID, LPUNKNOWN*) override { return E_NOTIMPL; }
    HRESULT STDMETHODCALLTYPE SetLocaleID(LCID lcid) override { if (lcid && lcid != kLocale && lcid != LOCALE_SYSTEM_DEFAULT && lcid != LOCALE_USER_DEFAULT) return E_INVALIDARG; _locale = lcid; return S_OK; }
    HRESULT STDMETHODCALLTYPE GetLocaleID(LCID* lcid) override { if (!lcid) return E_POINTER; *lcid = _locale; return S_OK; }
    HRESULT STDMETHODCALLTYPE QueryAvailableLocaleIDs(DWORD* count, LCID** lcids) override {
        if (!count || !lcids) return E_POINTER; *count = 1; *lcids = static_cast<LCID*>(CoTaskMemAlloc(sizeof(LCID))); if (!*lcids) return E_OUTOFMEMORY; (*lcids)[0] = kLocale; return S_OK;
    }
    HRESULT STDMETHODCALLTYPE GetErrorString(HRESULT error, LPWSTR* text) override { return ErrorString(error, text); }
    HRESULT STDMETHODCALLTYPE SetClientName(LPCWSTR name) override { _clientName = name ? name : L""; return S_OK; }
    ~CSampleServer() { std::vector<CSampleGroup*> groups; { std::lock_guard<std::mutex> g(_lock); groups.swap(_groups); } for (auto* group : groups) group->Release(); }
private:
    HRESULT ErrorString(HRESULT error, LPWSTR* text) { if (!text) return E_POINTER; *text = DupString(ErrorText(error)); return *text ? S_OK : E_OUTOFMEMORY; }
    volatile LONG _refs = 1; std::mutex _lock; std::vector<CSampleGroup*> _groups; OPCHANDLE _nextGroup = 1; LCID _locale = kLocale; std::wstring _clientName;
};

class CSampleClassFactory final : public IClassFactory {
public:
    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void** ppv) override { if (!ppv) return E_POINTER; *ppv = nullptr; if (riid != IID_IUnknown && riid != IID_IClassFactory) return E_NOINTERFACE; *ppv = static_cast<IClassFactory*>(this); AddRef(); return S_OK; }
    ULONG STDMETHODCALLTYPE AddRef() override { return static_cast<ULONG>(InterlockedIncrement(&_refs)); }
    ULONG STDMETHODCALLTYPE Release() override { ULONG r = static_cast<ULONG>(InterlockedDecrement(&_refs)); if (!r) delete this; return r; }
    HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown* outer, REFIID riid, void** ppv) override { if (outer) return CLASS_E_NOAGGREGATION; auto* server = new (std::nothrow) CSampleServer(); if (!server) return E_OUTOFMEMORY; HRESULT hr = server->QueryInterface(riid, ppv); server->Release(); return hr; }
    HRESULT STDMETHODCALLTYPE LockServer(BOOL) override { return S_OK; }
    ~CSampleClassFactory() = default;
private: volatile LONG _refs = 1;
};

HRESULT SetRegString(HKEY root, const std::wstring& subkey, const wchar_t* valueName, const std::wstring& value) {
    HKEY key{}; LONG e = RegCreateKeyExW(root, subkey.c_str(), 0, nullptr, REG_OPTION_NON_VOLATILE, KEY_WRITE, nullptr, &key, nullptr); if (e != ERROR_SUCCESS) return HRESULT_FROM_WIN32(e);
    e = RegSetValueExW(key, valueName, 0, REG_SZ, reinterpret_cast<const BYTE*>(value.c_str()), static_cast<DWORD>((value.size() + 1) * sizeof(wchar_t))); RegCloseKey(key); return HRESULT_FROM_WIN32(e);
}
HRESULT CreateRegKey(HKEY root, const std::wstring& subkey) { HKEY key{}; LONG e = RegCreateKeyExW(root, subkey.c_str(), 0, nullptr, REG_OPTION_NON_VOLATILE, KEY_WRITE, nullptr, &key, nullptr); if (e == ERROR_SUCCESS) RegCloseKey(key); return HRESULT_FROM_WIN32(e); }
HRESULT RegisterCategories() {
    ICatRegister* reg{}; HRESULT hr = CoCreateInstance(CLSID_StdComponentCategoriesMgr, nullptr, CLSCTX_INPROC_SERVER, IID_ICatRegister, reinterpret_cast<void**>(&reg)); if (FAILED(hr)) return hr;
    CATEGORYINFO info[2]{}; info[0].catid = kCatDa10; info[0].lcid = kLocale; info[1].catid = kCatDa20; info[1].lcid = kLocale;
    StringCchCopyW(info[0].szDescription, std::size(info[0].szDescription), L"OPC Data Access Servers Version 1.0"); StringCchCopyW(info[1].szDescription, std::size(info[1].szDescription), L"OPC Data Access Servers Version 2.0");
    hr = reg->RegisterCategories(2, info); if (SUCCEEDED(hr)) { CATID cats[2] = {kCatDa10, kCatDa20}; hr = reg->RegisterClassImplCategories(kClsid, 2, cats); } reg->Release(); return hr;
}
void UnregisterCategories() { ICatRegister* reg{}; if (SUCCEEDED(CoCreateInstance(CLSID_StdComponentCategoriesMgr, nullptr, CLSCTX_INPROC_SERVER, IID_ICatRegister, reinterpret_cast<void**>(&reg)))) { CATID cats[2] = {kCatDa10, kCatDa20}; reg->UnRegisterClassImplCategories(kClsid, 2, cats); reg->Release(); } }
HRESULT RegisterServer() {
    wchar_t path[MAX_PATH]{}; if (!GetModuleFileNameW(nullptr, path, static_cast<DWORD>(std::size(path)))) return HRESULT_FROM_WIN32(GetLastError());
    std::wstring clsid = GuidString(kClsid), ck = L"CLSID\\" + clsid, quoted = L"\"" + std::wstring(path) + L"\""; HRESULT hr;
    if (FAILED(hr = SetRegString(HKEY_CLASSES_ROOT, ck, nullptr, kDescription))) return hr;
    if (FAILED(hr = SetRegString(HKEY_CLASSES_ROOT, ck + L"\\LocalServer32", nullptr, quoted))) return hr;
    if (FAILED(hr = SetRegString(HKEY_CLASSES_ROOT, ck + L"\\ProgID", nullptr, kProgId))) return hr;
    if (FAILED(hr = SetRegString(HKEY_CLASSES_ROOT, ck + L"\\VersionIndependentProgID", nullptr, kVerProgId))) return hr;
    if (FAILED(hr = SetRegString(HKEY_CLASSES_ROOT, kProgId, nullptr, kDescription))) return hr;
    if (FAILED(hr = SetRegString(HKEY_CLASSES_ROOT, std::wstring(kProgId) + L"\\CLSID", nullptr, clsid))) return hr;
    if (FAILED(hr = SetRegString(HKEY_CLASSES_ROOT, kVerProgId, nullptr, kDescription))) return hr;
    if (FAILED(hr = SetRegString(HKEY_CLASSES_ROOT, std::wstring(kVerProgId) + L"\\CLSID", nullptr, clsid))) return hr;
    if (FAILED(hr = SetRegString(HKEY_CLASSES_ROOT, std::wstring(kVerProgId) + L"\\CurVer", nullptr, kProgId))) return hr;
    if (FAILED(hr = CreateRegKey(HKEY_CLASSES_ROOT, ck + L"\\Implemented Categories\\{63D5F430-CFE4-11d1-B2C8-0060083BA1FB}"))) return hr;
    if (FAILED(hr = CreateRegKey(HKEY_CLASSES_ROOT, ck + L"\\Implemented Categories\\{63D5F432-CFE4-11d1-B2C8-0060083BA1FB}"))) return hr;
    return RegisterCategories();
}
HRESULT DeleteTree(HKEY root, const std::wstring& subkey) { LONG e = RegDeleteTreeW(root, subkey.c_str()); return (e == ERROR_FILE_NOT_FOUND || e == ERROR_PATH_NOT_FOUND) ? S_OK : HRESULT_FROM_WIN32(e); }
HRESULT UnregisterServer() { UnregisterCategories(); HRESULT a = DeleteTree(HKEY_CLASSES_ROOT, L"CLSID\\" + GuidString(kClsid)), b = DeleteTree(HKEY_CLASSES_ROOT, kProgId), c = DeleteTree(HKEY_CLASSES_ROOT, kVerProgId); return FAILED(a) ? a : (FAILED(b) ? b : c); }
bool IsSwitch(const wchar_t* arg, const wchar_t* name) { return arg && (*arg == L'/' || *arg == L'-') && _wcsicmp(arg + 1, name) == 0; }
BOOL WINAPI ConsoleHandler(DWORD type) { if (type == CTRL_C_EVENT || type == CTRL_CLOSE_EVENT || type == CTRL_BREAK_EVENT || type == CTRL_SHUTDOWN_EVENT) { if (g_shutdownEvent) SetEvent(g_shutdownEvent); return TRUE; } return FALSE; }
} // namespace

int wmain(int argc, wchar_t** argv) {
    HRESULT hr = CoInitializeEx(nullptr, COINIT_MULTITHREADED); if (FAILED(hr)) { std::fwprintf(stderr, L"CoInitializeEx failed: 0x%08X\n", static_cast<unsigned int>(hr)); return 1; }
    for (int i = 1; i < argc; ++i) {
        if (IsSwitch(argv[i], L"RegServer")) { hr = RegisterServer(); CoUninitialize(); return SUCCEEDED(hr) ? 0 : 1; }
        if (IsSwitch(argv[i], L"UnregServer") || IsSwitch(argv[i], L"UnRegServer")) { hr = UnregisterServer(); CoUninitialize(); return SUCCEEDED(hr) ? 0 : 1; }
    }
    hr = CoInitializeSecurity(nullptr, -1, nullptr, nullptr, RPC_C_AUTHN_LEVEL_NONE, RPC_C_IMP_LEVEL_IDENTIFY, nullptr, EOAC_NONE, nullptr);
    if (FAILED(hr) && hr != RPC_E_TOO_LATE) { std::fwprintf(stderr, L"CoInitializeSecurity failed: 0x%08X\n", static_cast<unsigned int>(hr)); CoUninitialize(); return 1; }
    InitializeTags(); StartTagThread(); g_shutdownEvent = CreateEventW(nullptr, TRUE, FALSE, nullptr);
    if (!g_shutdownEvent) { StopTagThread(); CoUninitialize(); return 1; } SetConsoleCtrlHandler(ConsoleHandler, TRUE);
    auto* factory = new (std::nothrow) CSampleClassFactory(); if (!factory) { CloseHandle(g_shutdownEvent); StopTagThread(); CoUninitialize(); return 1; }
    DWORD registration = 0; hr = CoRegisterClassObject(kClsid, static_cast<IClassFactory*>(factory), CLSCTX_LOCAL_SERVER, REGCLS_MULTIPLEUSE, &registration);
    if (FAILED(hr)) { factory->Release(); std::fwprintf(stderr, L"CoRegisterClassObject failed: 0x%08X\n", static_cast<unsigned int>(hr)); CloseHandle(g_shutdownEvent); StopTagThread(); CoUninitialize(); return 1; }
    std::wprintf(L"OPC.SampleServer.1 running. Press Ctrl+C to stop.\n"); WaitForSingleObject(g_shutdownEvent, INFINITE);
    CoRevokeClassObject(registration); factory->Release(); CloseHandle(g_shutdownEvent); g_shutdownEvent = nullptr; StopTagThread(); CoUninitialize(); return 0;
}
