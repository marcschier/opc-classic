//==============================================================================
// TITLE: COpcDa30Server.cpp
//
// CONTENTS:
// 
// The implements the OPC remote server activator.
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
#include "COpcDa30Cache.h"

//==============================================================================
// COpcDa30Server

// FinalConstruct
HRESULT COpcDa30Server::FinalConstruct()
{
    if (!Initialize())
    {
        return E_FAIL;
    }

    return COpcDaServer::FinalConstruct();
}

// FinalRelease
bool COpcDa30Server::FinalRelease()
{
    COpcDaServer::FinalRelease();
    Uninitialize();
	return true;
}
