//==============================================================================
// TITLE: CXxxDaCache.cpp
//
// CONTENTS:
// 
// A concrete instance of a OPC Data Server cache.
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
#include "CXxxDaCache.h"

//==============================================================================
// Local Declarations

// Start
bool CXxxDaCache::Start()
{
    COpcLock cLock(*this);

    bool bResult = true;

    TRY
    {
		// check if server is running.
		if (GetState() != OPC_STATUS_SUSPENDED)
		{
			THROW_(bResult, false);
		}

		// start the update thread.
		if (!COpcDaCache::Start())
        {
            SetState(OPC_STATUS_FAILED);
			GOTOFINALLY();
        }

		// construct configuration file name.
		COpcString cFileName;
		
		cFileName += OpcDaGetModulePath();
		cFileName += _T("\\");
		cFileName += OpcDaGetModuleName();
		cFileName += _T(".config.xml");

		// load the configuration file.
		if (!COpcDaCache::Load(cFileName))
		{
            SetState(OPC_STATUS_NOCONFIG);
			GOTOFINALLY();
		}

		// startup complete.
		SetState(OPC_STATUS_RUNNING);
    }
    CATCH_FINALLY
    {
    }

    return bResult;
}

// Stop
void CXxxDaCache::Stop()
{
    COpcLock cLock(*this);

    // stop device.
    m_cDevice.Stop();

	// stop update thread.
	COpcDaCache::Stop();

	// place in suspended state.
	SetState(OPC_STATUS_SUSPENDED);
}

// Load
bool CXxxDaCache::Load(COpcXmlElement& cRoot)
{
    COpcLock cLock(*this);

	// initialize device.
	if (!m_cDevice.Start(cRoot))
	{
		return false;
	}

	return true;
}

