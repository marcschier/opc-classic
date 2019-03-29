//==============================================================================
// TITLE: COpcHdaServerImpl.cpp
//
// CONTENTS:
// 
// The implements the OPC remote server activator.
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
#include "COpcHdaHistorian.h"

//==============================================================================
// Static Data

static COpcCriticalSection g_cLock;
static UINT                g_uRefs      = 0;
static COpcHdaHistorian*   g_pHistorian = NULL;

//==============================================================================
// Static Functions

// GetCache
COpcHdaHistorian& GetHistorian()
{
    return *g_pHistorian;
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
		g_pHistorian = new COpcHdaHistorian();

        if (!g_pHistorian->Start())
        {
            THROW_(bResult, false);
        }
    }
    CATCH
    {
		if (g_pHistorian != NULL)
		{
			g_pHistorian->Stop();
			delete g_pHistorian;
			g_pHistorian = NULL;
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
		if (g_pHistorian != NULL)
		{
			g_pHistorian->Stop();
			delete g_pHistorian;
			g_pHistorian = NULL;
		}

        cLock.Unlock();
        COpcComModule::ExitProcess(S_OK);
    }
}

//==============================================================================
// COpcHdaServerImpl

// FinalConstruct
HRESULT COpcHdaServerImpl::FinalConstruct()
{
	if (!Initialize())
	{
		return E_FAIL;
	}

	return COpcHdaServer::FinalConstruct();
}

// FinalRelease
bool COpcHdaServerImpl::FinalRelease()
{
    bool bDelete = COpcHdaServer::FinalRelease();
	Uninitialize();
	return bDelete;
}
