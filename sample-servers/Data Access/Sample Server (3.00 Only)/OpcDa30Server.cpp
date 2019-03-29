//==============================================================================
// TITLE: OpcDa30Server.cpp
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
#include "COpcDa30Server.h"
#pragma warning(disable:4192)

//================================================================================
// COM Module Declarations

OPC_DECLARE_APPLICATION(OPCSample, OpcDa30Server, "OPC Data Access 3.00 Sample Server")

OPC_BEGIN_CLASS_TABLE()
    OPC_CLASS_TABLE_ENTRY(COpcDa30Server, OpcDa30Server, 1, "OPC Data Access 3.00 Source Server")
OPC_END_CLASS_TABLE()

OPC_BEGIN_CATEGORY_TABLE()
    OPC_CATEGORY_TABLE_ENTRY(OpcDa30Server, CATID_OPCDAServer30, OPC_CATEGORY_DESCRIPTION_DA30)
OPC_END_CATEGORY_TABLE()

#ifndef _USRDLL

// {F270ED87-CBBF-4138-81E7-018CFCD73911}
OPC_IMPLEMENT_LOCAL_SERVER(0xf270ed87, 0xcbbf, 0x4138, 0x81, 0xe7, 0x1, 0x8c, 0xfc, 0xd7, 0x39, 0x11);

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
