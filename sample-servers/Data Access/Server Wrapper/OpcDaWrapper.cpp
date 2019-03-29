//==============================================================================
// TITLE: XxxDaServer.cpp
//
// CONTENTS:
// 
// Implements the required COM server functions.
//
// (c) Copyright 2002-2003 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This code is provided by the OPC Foundation solely to assist in 
//  understanding and use of the appropriate OPC Specification(s) and may be 
//  used as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// MODIFICATION LOG:
//
// Date       By    Notes
// ---------- ---   -----
// 2002/11/16 RSA   First release.
//

#include "StdAfx.h"
#include "COpcDaServerWrapper.h"

#pragma warning(disable:4192)

// {132B3E2B-0E92-4816-972B-E42AA9532529}
static const GUID CATID_DotNetOpcServerWrapper = 
{ 0x132b3e2b, 0xe92, 0x4816, { 0x97, 0x2b, 0xe4, 0x2a, 0xa9, 0x53, 0x25, 0x29 } };

//================================================================================
// COM Module Declarations

OPC_DECLARE_APPLICATION(OPCSample, OpcDaWrapper, "OPC Data Access .NET Server Wrapper")

OPC_BEGIN_CLASS_TABLE()
    OPC_CLASS_TABLE_ENTRY(COpcDaServerWrapper, OpcDaWrapper, 1, "OPC Data Access .NET Server Wrapper")
OPC_END_CLASS_TABLE()

OPC_BEGIN_CATEGORY_TABLE()
    OPC_CATEGORY_TABLE_ENTRY(OpcDaWrapper, CATID_OPCDAServer20, OPC_CATEGORY_DESCRIPTION_DA20)
    OPC_CATEGORY_TABLE_ENTRY(OpcDaWrapper, CATID_OPCDAServer30, OPC_CATEGORY_DESCRIPTION_DA30)
    OPC_CATEGORY_TABLE_ENTRY(OpcDaWrapper, CATID_DotNetOpcServerWrapper, _T(".NET OPC Server Wrapper Process"))
OPC_END_CATEGORY_TABLE()

#ifndef _USRDLL

// {12E2FD32-D48E-4b66-9AF5-8C409AC2C481}
OPC_IMPLEMENT_LOCAL_SERVER(0x12e2fd32, 0xd48e, 0x4b66, 0x9a, 0xf5, 0x8c, 0x40, 0x9a, 0xc2, 0xc4, 0x81);

//================================================================================
// WinMain

extern "C" int WINAPI _tWinMain(
    HINSTANCE hInstance, 
	HINSTANCE hPrevInstance, 
    LPTSTR    lpCmdLine, 
    int       nShowCmd
)
{
    OPC_START_LOCAL_SERVER_EX(hInstance, lpCmdLine);
    return 0;
}

#else

OPC_IMPLEMENT_INPROC_SERVER();

//==============================================================================
// DllMain

extern "C"
BOOL WINAPI DllMain( 
    HINSTANCE hModule, 
    DWORD     dwReason, 
    LPVOID    lpReserved)
{
    OPC_START_INPROC_SERVER(hModule, dwReason);
    return TRUE;
}

#endif // _USRDLL
