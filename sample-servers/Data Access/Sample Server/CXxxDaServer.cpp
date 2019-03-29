//==============================================================================
// TITLE: CXxxDaServer.cpp
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
#include "CXxxDaServer.h"
#include "CXxxDaCache.h"

//==============================================================================
// Static Data

static COpcCriticalSection g_cLock;
static UINT                g_uRefs   = 0;
static CXxxDaCache*        g_pCache  = NULL;
static COpcDaDevice*       g_pDevice = NULL;

//==============================================================================
// Static Functions

// GetCache
COpcDaCache& GetCache()
{
    return *g_pCache;
}

// GetDevice
IOpcDaDevice* GetDevice(const COpcString& cItemID)
{
    return (IOpcDaDevice*)g_pDevice;
}

// Initialize
static bool Initialize()
{
    COpcLock cLock(g_cLock);
    
    g_uRefs++;
    
	if (g_uRefs > 1)
    {
        return true;
    }

    bool bResult = true;

    TRY
    {        
		g_pDevice = new COpcDaDevice();

		g_pCache = new CXxxDaCache(*g_pDevice);

        if (!g_pCache->Start())
        {
            THROW_(bResult, false);
        }
    }
    CATCH
    {
		if (g_pCache != NULL)
		{
			g_pCache->Stop();
			delete g_pCache;
			g_pCache = NULL;
		}

		if (g_pDevice != NULL)
		{
			delete g_pDevice;
			g_pDevice = NULL;
		}
    }

    return bResult;
}

// Uninitialize
static void Uninitialize()
{
    COpcLock cLock(g_cLock);

    g_uRefs--;
    
    if (g_uRefs == 0)
    {
		if (g_pCache != NULL)
		{
			g_pCache->Stop();
			delete g_pCache;
			g_pCache = NULL;
		}
	
		if (g_pDevice != NULL)
		{
			delete g_pDevice;
			g_pDevice = NULL;
		}

        cLock.Unlock();
        COpcComModule::ExitProcess(S_OK);
    }
}

//==============================================================================
// CXxxDaServer

// FinalConstruct
HRESULT CXxxDaServer::FinalConstruct()
{
    if (!Initialize())
    {
        return E_FAIL;
    }

    return COpcDaServer::FinalConstruct();
}

// FinalRelease
bool CXxxDaServer::FinalRelease()
{
    COpcDaServer::FinalRelease();
    Uninitialize();
	return true;
}
