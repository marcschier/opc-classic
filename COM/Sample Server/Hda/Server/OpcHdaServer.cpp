//==============================================================================
// TITLE: OpcHdaServer.cpp
//
// CONTENTS:
// 
// Implements the required COM server functions.
//
// (c) Copyright 2003-2004 The OPC Foundation
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
// 2003/12/17 RSA   Initial implementation.

#include "StdAfx.h"
#include "COpcHdaServerImpl.h"

#pragma warning(disable:4192)

//================================================================================
// COM Module Declarations

OPC_DECLARE_APPLICATION(OPCSample, OpcHdaServer, "OPC Historical Data Access 1.20 Sample Server")

OPC_BEGIN_CLASS_TABLE()
    OPC_CLASS_TABLE_ENTRY(COpcHdaServerImpl, OpcHdaServer, 1, "OPC Historical Data Access 1.20 Sample Server")
OPC_END_CLASS_TABLE()

OPC_BEGIN_CATEGORY_TABLE()
    OPC_CATEGORY_TABLE_ENTRY(OpcHdaServer, CATID_OPCHDAServer10, OPC_CATEGORY_DESCRIPTION_HDA10)
OPC_END_CATEGORY_TABLE()

#ifndef _USRDLL

// {778F4D15-8401-43d8-A88D-FBFD083538B3}
OPC_IMPLEMENT_LOCAL_SERVER(0x778f4d15, 0x8401, 0x43d8, 0xa8, 0x8d, 0xfb, 0xfd, 0x8, 0x35, 0x38, 0xb3);

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
