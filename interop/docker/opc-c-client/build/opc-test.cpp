/*
 * SPDX-License-Identifier: MIT
 * Copyright (c) 2026 Opc.Classic .NET Contributors
 *
 * opc-test.exe — minimum-viable headless OPC DA client.
 *
 * Activates a remote OPC DA server (via CLSCTX_REMOTE_SERVER), creates a
 * group, adds one item, reads its value, and cleans up. Exits 0 on
 * success, non-zero on failure with the HRESULT printed to stderr.
 *
 * Usage:
 *   opc-test.exe <prog-id> <target-host> [item-id]
 *
 *   prog-id      e.g. "Opc.Classic.DaSample.1" or "OPC.SampleServer.1"
 *   target-host  Hostname or IP of the remote DCOM server
 *   item-id      Optional; defaults to "Sin"
 *
 * Used by the docker test fleet's c-client container to smoke-test
 * cross-platform interop against the managed Opc.Classic server.
 */

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <objbase.h>
#include <comdef.h>
#include <oleauto.h>
#include <cstdio>
#include <cwchar>

#include "opcda.h"

#pragma comment(lib, "ole32.lib")
#pragma comment(lib, "oleaut32.lib")
#pragma comment(lib, "advapi32.lib")
#pragma comment(lib, "uuid.lib")

namespace {

void LogError(const wchar_t* operation, HRESULT hr)
{
    fwprintf(stderr, L"opc-test: %ls failed: HRESULT=0x%08lX\n",
        operation, static_cast<unsigned long>(hr));
}

HRESULT ParseClsid(const wchar_t* progId, CLSID& outClsid)
{
    return CLSIDFromProgID(progId, &outClsid);
}

HRESULT CreateRemoteServer(REFCLSID clsid, const wchar_t* host, REFIID riid, void** ppv)
{
    COSERVERINFO server = {};
    server.pwszName = const_cast<wchar_t*>(host);
    MULTI_QI mqi = { &riid, nullptr, S_OK };
    HRESULT hr = CoCreateInstanceEx(clsid, nullptr,
        CLSCTX_REMOTE_SERVER, &server, 1, &mqi);
    if (FAILED(hr))
    {
        return hr;
    }
    if (FAILED(mqi.hr))
    {
        return mqi.hr;
    }
    *ppv = mqi.pItf;
    return S_OK;
}

HRESULT AddTestGroup(IOPCServer* server, OPCHANDLE& outServerHandle,
    IOPCItemMgt** outItemMgt, IOPCSyncIO** outSyncIo)
{
    DWORD revisedRate = 0;
    GUID iid = IID_IOPCItemMgt;
    IUnknown* groupUnk = nullptr;
    HRESULT hr = server->AddGroup(L"opc-test-group", TRUE, 1000, 1, nullptr,
        nullptr, 0, &outServerHandle, &revisedRate, iid, &groupUnk);
    if (FAILED(hr))
    {
        return hr;
    }
    hr = groupUnk->QueryInterface(IID_IOPCItemMgt, reinterpret_cast<void**>(outItemMgt));
    if (SUCCEEDED(hr))
    {
        hr = groupUnk->QueryInterface(IID_IOPCSyncIO, reinterpret_cast<void**>(outSyncIo));
    }
    groupUnk->Release();
    return hr;
}

HRESULT AddTestItem(IOPCItemMgt* itemMgt, const wchar_t* itemId,
    OPCHANDLE& outItemHandle)
{
    OPCITEMDEF def = {};
    def.szAccessPath = L"";
    def.szItemID = const_cast<wchar_t*>(itemId);
    def.bActive = TRUE;
    def.hClient = 1;
    def.vtRequestedDataType = VT_EMPTY;

    OPCITEMRESULT* results = nullptr;
    HRESULT* errors = nullptr;
    HRESULT hr = itemMgt->AddItems(1, &def, &results, &errors);
    if (FAILED(hr))
    {
        return hr;
    }
    if (errors != nullptr && errors[0] != S_OK)
    {
        HRESULT itemError = errors[0];
        if (results != nullptr) { CoTaskMemFree(results); }
        CoTaskMemFree(errors);
        return itemError;
    }
    outItemHandle = results[0].hServer;
    if (results[0].pBlob != nullptr) { CoTaskMemFree(results[0].pBlob); }
    CoTaskMemFree(results);
    CoTaskMemFree(errors);
    return S_OK;
}

HRESULT ReadAndReport(IOPCSyncIO* syncIo, OPCHANDLE itemHandle)
{
    OPCITEMSTATE* states = nullptr;
    HRESULT* errors = nullptr;
    HRESULT hr = syncIo->Read(OPC_DS_CACHE, 1, &itemHandle, &states, &errors);
    if (FAILED(hr))
    {
        return hr;
    }
    if (errors[0] != S_OK)
    {
        HRESULT itemError = errors[0];
        CoTaskMemFree(states);
        CoTaskMemFree(errors);
        return itemError;
    }
    wprintf(L"opc-test: read OK, quality=0x%04X vt=%u\n",
        states[0].wQuality, states[0].vDataValue.vt);
    VariantClear(&states[0].vDataValue);
    CoTaskMemFree(states);
    CoTaskMemFree(errors);
    return S_OK;
}

} // namespace

extern "C" int wmain(int argc, wchar_t* argv[])
{
    if (argc < 3)
    {
        fwprintf(stderr,
            L"Usage: opc-test.exe <prog-id> <target-host> [item-id]\n");
        return 1;
    }
    const wchar_t* progId = argv[1];
    const wchar_t* host = argv[2];
    const wchar_t* itemId = argc >= 4 ? argv[3] : L"Sin";

    HRESULT hr = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    if (FAILED(hr))
    {
        LogError(L"CoInitializeEx", hr);
        return 1;
    }

    int rc = 0;
    CLSID clsid = {};
    IOPCServer* server = nullptr;
    IOPCItemMgt* itemMgt = nullptr;
    IOPCSyncIO* syncIo = nullptr;
    OPCHANDLE groupHandle = 0;
    OPCHANDLE itemHandle = 0;

    hr = ParseClsid(progId, clsid);
    if (FAILED(hr)) { LogError(L"CLSIDFromProgID", hr); rc = 2; goto cleanup; }

    hr = CreateRemoteServer(clsid, host, IID_IOPCServer,
        reinterpret_cast<void**>(&server));
    if (FAILED(hr)) { LogError(L"CoCreateInstanceEx", hr); rc = 3; goto cleanup; }
    wprintf(L"opc-test: connected to %ls on %ls\n", progId, host);

    hr = AddTestGroup(server, groupHandle, &itemMgt, &syncIo);
    if (FAILED(hr)) { LogError(L"AddGroup", hr); rc = 4; goto cleanup; }

    hr = AddTestItem(itemMgt, itemId, itemHandle);
    if (FAILED(hr)) { LogError(L"AddItems", hr); rc = 5; goto cleanup; }

    hr = ReadAndReport(syncIo, itemHandle);
    if (FAILED(hr)) { LogError(L"Read", hr); rc = 6; goto cleanup; }

cleanup:
    if (syncIo != nullptr) { syncIo->Release(); }
    if (itemMgt != nullptr) { itemMgt->Release(); }
    if (server != nullptr)
    {
        if (groupHandle != 0) { server->RemoveGroup(groupHandle, TRUE); }
        server->Release();
    }
    CoUninitialize();
    return rc;
}
