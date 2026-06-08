/* ========================================================================
 * Copyright (c) 2002-2026 OPC Foundation. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <tchar.h>
#include <stdio.h>
#include <objbase.h>

// OPC interface declarations (MIDL-generated)
#include "opccomn.h"
#include "opcda.h"

// OPC GUIDs (MIDL-generated)
#include "opccomn_i.c"
#include "opcda_i.c"

// CLSID for OpcEnum (OpcServerList) — not in opccomn.idl, defined in OpcEnum.idl
// {13486D51-4821-11D2-A494-3CB306C10000}
static const CLSID CLSID_OpcServerList =
    {0x13486D51, 0x4821, 0x11D2, {0xA4, 0x94, 0x3C, 0xB3, 0x06, 0xC1, 0x00, 0x00}};

//==============================================================================
// Helper: OPCSERVERSTATE to string

static LPCTSTR ServerStateString(OPCSERVERSTATE state)
{
    switch (state)
    {
    case OPC_STATUS_RUNNING:    return _T("Running");
    case OPC_STATUS_FAILED:     return _T("Failed");
    case OPC_STATUS_NOCONFIG:   return _T("No Config");
    case OPC_STATUS_SUSPENDED:  return _T("Suspended");
    case OPC_STATUS_TEST:       return _T("Test");
    case OPC_STATUS_COMM_FAULT: return _T("Comm Fault");
    default:                    return _T("Unknown");
    }
}

//==============================================================================
// Helper: FILETIME to printable string

static void FormatFileTime(const FILETIME& ft, TCHAR* buf, int bufLen)
{
    SYSTEMTIME st;
    FileTimeToSystemTime(&ft, &st);
    _sntprintf_s(buf, bufLen, _TRUNCATE,
        _T("%04d-%02d-%02d %02d:%02d:%02d"),
        st.wYear, st.wMonth, st.wDay,
        st.wHour, st.wMinute, st.wSecond);
}

//==============================================================================
// Helper: VARIANT to printable string

static void FormatVariant(const VARIANT& v, TCHAR* buf, int bufLen)
{
    switch (v.vt)
    {
    case VT_I1:    _sntprintf_s(buf, bufLen, _TRUNCATE, _T("%d (I1)"),    v.cVal); break;
    case VT_UI1:   _sntprintf_s(buf, bufLen, _TRUNCATE, _T("%u (UI1)"),   v.bVal); break;
    case VT_I2:    _sntprintf_s(buf, bufLen, _TRUNCATE, _T("%d (I2)"),    v.iVal); break;
    case VT_UI2:   _sntprintf_s(buf, bufLen, _TRUNCATE, _T("%u (UI2)"),   v.uiVal); break;
    case VT_I4:    _sntprintf_s(buf, bufLen, _TRUNCATE, _T("%ld (I4)"),   v.lVal); break;
    case VT_UI4:   _sntprintf_s(buf, bufLen, _TRUNCATE, _T("%lu (UI4)"),  v.ulVal); break;
    case VT_R4:    _sntprintf_s(buf, bufLen, _TRUNCATE, _T("%f (R4)"),    v.fltVal); break;
    case VT_R8:    _sntprintf_s(buf, bufLen, _TRUNCATE, _T("%f (R8)"),    v.dblVal); break;
    case VT_BSTR:  _sntprintf_s(buf, bufLen, _TRUNCATE, _T("'%ls' (BSTR)"), v.bstrVal ? v.bstrVal : L""); break;
    case VT_BOOL:  _sntprintf_s(buf, bufLen, _TRUNCATE, _T("%s (BOOL)"),  v.boolVal ? _T("True") : _T("False")); break;
    case VT_EMPTY: _sntprintf_s(buf, bufLen, _TRUNCATE, _T("(empty)")); break;
    default:       _sntprintf_s(buf, bufLen, _TRUNCATE, _T("(vt=0x%04x)"), v.vt); break;
    }
}

//==============================================================================
// Helper: OPCITEMSTATE printer

static void PrintItemState(int idx, LPCWSTR itemId, const OPCITEMSTATE& state)
{
    TCHAR valueStr[128];
    FormatVariant(state.vDataValue, valueStr, 128);
    _tprintf(_T("    [%d] %-12ls = %-32s q=0x%04x hr=0x%08x\n"),
        idx, itemId ? itemId : L"?", valueStr, state.wQuality, S_OK);
}

//==============================================================================
// RunLifecycle (Track AB6 / Opc.Classic divergence):
//   AddGroup -> AddItems(Test.Int32, Test.Float, Test.String) -> SyncRead ->
//   SyncWrite Test.Int32=100 -> SyncRead-Verify -> RemoveItems -> RemoveGroup
// Mirrors the managed mcp_driver.py --testserver exerciser so TestServer +
// TestClient form an in-tree symmetric loopback.

static void RunLifecycle(IOPCServer* pServer)
{
    _tprintf(_T("  Running full DA 2.x lifecycle exerciser:\n"));

    // ---- AddGroup ----
    OPCHANDLE serverGroupHandle = 0;
    DWORD revisedUpdateRate = 0;
    IUnknown* pGroupUnk = NULL;
    HRESULT hr = pServer->AddGroup(
        L"TestGroup",
        TRUE,           // active
        1000,           // requested update rate (ms)
        1,              // client group handle
        NULL,           // time bias
        NULL,           // percent deadband
        0,              // locale id (default)
        &serverGroupHandle,
        &revisedUpdateRate,
        IID_IUnknown,
        &pGroupUnk);
    if (FAILED(hr))
    {
        _tprintf(_T("    AddGroup failed: 0x%08x\n"), hr);
        return;
    }
    _tprintf(_T("    AddGroup OK: serverHandle=%lu revisedRate=%lums\n"),
        serverGroupHandle, revisedUpdateRate);

    IOPCItemMgt* pItemMgt = NULL;
    IOPCSyncIO* pSyncIO = NULL;
    hr = pGroupUnk->QueryInterface(IID_IOPCItemMgt, (void**)&pItemMgt);
    if (SUCCEEDED(hr))
        hr = pGroupUnk->QueryInterface(IID_IOPCSyncIO, (void**)&pSyncIO);
    pGroupUnk->Release();
    if (FAILED(hr))
    {
        _tprintf(_T("    QI IOPCItemMgt/IOPCSyncIO failed: 0x%08x\n"), hr);
        if (pItemMgt) pItemMgt->Release();
        pServer->RemoveGroup(serverGroupHandle, FALSE);
        return;
    }

    // ---- AddItems ----
    LPWSTR itemIds[3] = { L"Test.Int32", L"Test.Float", L"Test.String" };
    OPCITEMDEF itemDefs[3];
    ZeroMemory(itemDefs, sizeof(itemDefs));
    for (int i = 0; i < 3; i++)
    {
        itemDefs[i].szItemID = itemIds[i];
        itemDefs[i].bActive = TRUE;
        itemDefs[i].hClient = (OPCHANDLE)(100 + i);
        itemDefs[i].vtRequestedDataType = VT_EMPTY;
    }
    OPCITEMRESULT* pAddResults = NULL;
    HRESULT* pAddErrors = NULL;
    hr = pItemMgt->AddItems(3, itemDefs, &pAddResults, &pAddErrors);
    if (FAILED(hr))
    {
        _tprintf(_T("    AddItems call failed: 0x%08x\n"), hr);
        pItemMgt->Release();
        pSyncIO->Release();
        pServer->RemoveGroup(serverGroupHandle, FALSE);
        return;
    }

    OPCHANDLE serverItemHandles[3] = { 0 };
    bool addOk = true;
    for (int i = 0; i < 3; i++)
    {
        if (SUCCEEDED(pAddErrors[i]))
        {
            serverItemHandles[i] = pAddResults[i].hServer;
            _tprintf(_T("    AddItem[%d] %-12ls OK serverHandle=%lu canonVt=0x%04x\n"),
                i, itemIds[i], pAddResults[i].hServer, pAddResults[i].vtCanonicalDataType);
        }
        else
        {
            addOk = false;
            _tprintf(_T("    AddItem[%d] %-12ls FAILED hr=0x%08x\n"), i, itemIds[i], pAddErrors[i]);
        }
        if (pAddResults[i].pBlob) CoTaskMemFree(pAddResults[i].pBlob);
    }
    CoTaskMemFree(pAddResults);
    CoTaskMemFree(pAddErrors);

    if (!addOk)
    {
        _tprintf(_T("    Skipping read/write — items not added.\n"));
        pItemMgt->Release();
        pSyncIO->Release();
        pServer->RemoveGroup(serverGroupHandle, FALSE);
        return;
    }

    // ---- SyncRead (CACHE) ----
    OPCITEMSTATE* pReadStates = NULL;
    HRESULT* pReadErrors = NULL;
    hr = pSyncIO->Read(OPC_DS_CACHE, 3, serverItemHandles, &pReadStates, &pReadErrors);
    if (SUCCEEDED(hr))
    {
        _tprintf(_T("  SyncRead OK:\n"));
        for (int i = 0; i < 3; i++) PrintItemState(i, itemIds[i], pReadStates[i]);
        for (int i = 0; i < 3; i++) VariantClear(&pReadStates[i].vDataValue);
        CoTaskMemFree(pReadStates);
        CoTaskMemFree(pReadErrors);
    }
    else
    {
        _tprintf(_T("  SyncRead failed: 0x%08x\n"), hr);
    }

    // ---- SyncWrite Test.Int32 = 100 ----
    VARIANT writeValues[1];
    VariantInit(&writeValues[0]);
    writeValues[0].vt = VT_I4;
    writeValues[0].lVal = 100;
    OPCHANDLE writeHandles[1] = { serverItemHandles[0] };
    HRESULT* pWriteErrors = NULL;
    hr = pSyncIO->Write(1, writeHandles, writeValues, &pWriteErrors);
    if (SUCCEEDED(hr) && SUCCEEDED(pWriteErrors[0]))
    {
        _tprintf(_T("  SyncWrite Test.Int32=100 OK\n"));
    }
    else
    {
        _tprintf(_T("  SyncWrite failed: hr=0x%08x perItem=0x%08x\n"), hr,
            pWriteErrors ? pWriteErrors[0] : E_FAIL);
    }
    if (pWriteErrors) CoTaskMemFree(pWriteErrors);
    VariantClear(&writeValues[0]);

    // ---- SyncRead Test.Int32 to verify (DEVICE to bypass cache) ----
    OPCHANDLE verifyHandles[1] = { serverItemHandles[0] };
    hr = pSyncIO->Read(OPC_DS_DEVICE, 1, verifyHandles, &pReadStates, &pReadErrors);
    if (SUCCEEDED(hr))
    {
        TCHAR valueStr[128];
        FormatVariant(pReadStates[0].vDataValue, valueStr, 128);
        _tprintf(_T("  Verify Read Test.Int32 = %s\n"), valueStr);
        VariantClear(&pReadStates[0].vDataValue);
        CoTaskMemFree(pReadStates);
        CoTaskMemFree(pReadErrors);
    }
    else
    {
        _tprintf(_T("  Verify Read failed: 0x%08x\n"), hr);
    }

    // ---- RemoveItems ----
    HRESULT* pRemoveErrors = NULL;
    hr = pItemMgt->RemoveItems(3, serverItemHandles, &pRemoveErrors);
    if (SUCCEEDED(hr))
    {
        _tprintf(_T("  RemoveItems OK\n"));
        CoTaskMemFree(pRemoveErrors);
    }
    else
    {
        _tprintf(_T("  RemoveItems failed: 0x%08x\n"), hr);
    }

    pItemMgt->Release();
    pSyncIO->Release();

    // ---- RemoveGroup ----
    hr = pServer->RemoveGroup(serverGroupHandle, FALSE);
    if (SUCCEEDED(hr))
        _tprintf(_T("  RemoveGroup OK\n"));
    else
        _tprintf(_T("  RemoveGroup failed: 0x%08x\n"), hr);
}

//==============================================================================
// ConnectAndGetStatus — CoCreateInstance the server, call GetStatus, run
// the AddGroup/AddItems/SyncIO/Remove lifecycle, release.
//
// Track AB6 / Opc.Classic divergence: upstream version of this function
// only called GetStatus. Re-syncing with upstream OPC-Classic-CoreComponents
// requires re-applying the RunLifecycle call below — see
// ext/redist/CoreComponents/VENDORED.md.

static void ConnectAndGetStatus(REFCLSID clsid)
{
    IOPCServer* pServer = NULL;
    HRESULT hr = CoCreateInstance(clsid, NULL, CLSCTX_LOCAL_SERVER,
                                  IID_IOPCServer, (void**)&pServer);
    if (FAILED(hr))
    {
        _tprintf(_T("  CoCreateInstance failed: 0x%08X\n"), hr);
        return;
    }

    _tprintf(_T("  Connected. Calling GetStatus...\n"));

    OPCSERVERSTATUS* pStatus = NULL;
    hr = pServer->GetStatus(&pStatus);

    if (SUCCEEDED(hr) && pStatus != NULL)
    {
        TCHAR szTime[64];

        FormatFileTime(pStatus->ftStartTime, szTime, 64);
        _tprintf(_T("  Start Time:   %s\n"), szTime);

        FormatFileTime(pStatus->ftCurrentTime, szTime, 64);
        _tprintf(_T("  Current Time: %s\n"), szTime);

        _tprintf(_T("  State:        %s\n"), ServerStateString(pStatus->dwServerState));
        _tprintf(_T("  Version:      %d.%d.%d\n"),
            pStatus->wMajorVersion, pStatus->wMinorVersion, pStatus->wBuildNumber);
        _tprintf(_T("  Vendor:       %ls\n"),
            pStatus->szVendorInfo ? pStatus->szVendorInfo : L"(none)");
        _tprintf(_T("  Groups:       %lu\n"), pStatus->dwGroupCount);
        _tprintf(_T("  Bandwidth:    %lu\n"), pStatus->dwBandWidth);

        if (pStatus->szVendorInfo) CoTaskMemFree(pStatus->szVendorInfo);
        CoTaskMemFree(pStatus);
    }
    else
    {
        _tprintf(_T("  GetStatus failed: 0x%08X\n"), hr);
    }

    pServer->Release();
    _tprintf(_T("  Released.\n"));
}

//==============================================================================
// ConnectAndExercise — full lifecycle (Track AB6): GetStatus + AddGroup +
// AddItems + SyncIO Read/Write + Remove. Wraps ConnectAndGetStatus.

static void ConnectAndExercise(REFCLSID clsid)
{
    IOPCServer* pServer = NULL;
    HRESULT hr = CoCreateInstance(clsid, NULL, CLSCTX_LOCAL_SERVER,
                                  IID_IOPCServer, (void**)&pServer);
    if (FAILED(hr))
    {
        _tprintf(_T("  CoCreateInstance failed: 0x%08x\n"), hr);
        return;
    }

    _tprintf(_T("  Connected. Calling GetStatus...\n"));
    OPCSERVERSTATUS* pStatus = NULL;
    hr = pServer->GetStatus(&pStatus);
    if (SUCCEEDED(hr) && pStatus != NULL)
    {
        TCHAR szTime[64];
        FormatFileTime(pStatus->ftStartTime, szTime, 64);
        _tprintf(_T("  Start Time:   %s\n"), szTime);
        FormatFileTime(pStatus->ftCurrentTime, szTime, 64);
        _tprintf(_T("  Current Time: %s\n"), szTime);
        _tprintf(_T("  State:        %s\n"), ServerStateString(pStatus->dwServerState));
        _tprintf(_T("  Version:      %d.%d.%d\n"),
            pStatus->wMajorVersion, pStatus->wMinorVersion, pStatus->wBuildNumber);
        _tprintf(_T("  Vendor:       %ls\n"),
            pStatus->szVendorInfo ? pStatus->szVendorInfo : L"(none)");
        _tprintf(_T("  Groups:       %lu\n"), pStatus->dwGroupCount);
        _tprintf(_T("  Bandwidth:    %lu\n"), pStatus->dwBandWidth);
        if (pStatus->szVendorInfo) CoTaskMemFree(pStatus->szVendorInfo);
        CoTaskMemFree(pStatus);
    }
    else
    {
        _tprintf(_T("  GetStatus failed: 0x%08x\n"), hr);
    }

    RunLifecycle(pServer);

    pServer->Release();
    _tprintf(_T("  Released.\n"));
}

//==============================================================================
// main

int _tmain(int /* argc */, TCHAR* /* argv */[])
{
    _tprintf(_T("OPC DA 2.05a Test Client\n"));
    _tprintf(_T("=======================\n\n"));

    HRESULT hr = CoInitializeEx(NULL, COINIT_MULTITHREADED);
    if (FAILED(hr))
    {
        _tprintf(_T("CoInitializeEx failed: 0x%08X\n"), hr);
        return 1;
    }

    CoInitializeSecurity(NULL, -1, NULL, NULL,
        RPC_C_AUTHN_LEVEL_CONNECT, RPC_C_IMP_LEVEL_IMPERSONATE,
        NULL, EOAC_NONE, NULL);

    // Connect to OpcEnum via IOPCServerList.
    IOPCServerList* pServerList = NULL;
    hr = CoCreateInstance(CLSID_OpcServerList, NULL, CLSCTX_ALL,
                          IID_IOPCServerList, (void**)&pServerList);
    if (FAILED(hr))
    {
        _tprintf(_T("Could not connect to OpcEnum (IOPCServerList): 0x%08X\n"), hr);
        _tprintf(_T("Make sure the OpcEnum service is running.\n"));
        CoUninitialize();
        return 1;
    }

    _tprintf(_T("Connected to OpcEnum.\n\n"));

    // Enumerate OPC DA 2.0 servers.
    IEnumGUID* pEnum = NULL;
    CATID catid = CATID_OPCDAServer20;
    hr = pServerList->EnumClassesOfCategories(1, &catid, 0, NULL, &pEnum);
    if (FAILED(hr))
    {
        _tprintf(_T("EnumClassesOfCategories failed: 0x%08X\n"), hr);
        pServerList->Release();
        CoUninitialize();
        return 1;
    }

    _tprintf(_T("OPC DA 2.0 Servers\n"));
    _tprintf(_T("------------------\n"));

    CLSID clsid;
    ULONG fetched = 0;
    int count = 0;

    while (pEnum->Next(1, &clsid, &fetched) == S_OK)
    {
        count++;

        // Server info from OpcEnum.
        LPOLESTR wszProgID   = NULL;
        LPOLESTR wszUserType = NULL;
        pServerList->GetClassDetails(clsid, &wszProgID, &wszUserType);

        LPOLESTR wszClsid = NULL;
        StringFromCLSID(clsid, &wszClsid);

        _tprintf(_T("\nServer %d:\n"), count);
        _tprintf(_T("  CLSID:       %ls\n"), wszClsid   ? wszClsid   : L"?");
        _tprintf(_T("  ProgID:      %ls\n"), wszProgID  ? wszProgID  : L"?");
        _tprintf(_T("  Description: %ls\n"), wszUserType? wszUserType: L"?");

        // Connect, GetStatus, run lifecycle, release.
        ConnectAndExercise(clsid);

        if (wszClsid)    CoTaskMemFree(wszClsid);
        if (wszProgID)   CoTaskMemFree(wszProgID);
        if (wszUserType) CoTaskMemFree(wszUserType);
    }

    if (count == 0)
        _tprintf(_T("\n(none found)\n"));
    else
        _tprintf(_T("\nTotal: %d server(s).\n"), count);

    pEnum->Release();
    pServerList->Release();
    CoUninitialize();
    return 0;
}
