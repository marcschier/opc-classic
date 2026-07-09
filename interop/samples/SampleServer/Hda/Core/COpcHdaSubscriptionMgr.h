//============================================================================
// TITLE: COpcHdaSubscriptionMgr.h
//
// CONTENTS:
// 
// Periodically updates all active subscriptions.
//
// (c) Copyright 2002-2004 The OPC Foundation
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
// 2004/01/29 RSA   Initial implementation.

#ifndef _COpcHdaSubscriptionMgr_H_
#define _COpcHdaSubscriptionMgr_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

#include "COpcHdaTransaction.h"

#define MAX_UPDATE_RATE 100

//============================================================================
// CLASS:   COpcHdaSubscriptionMgr
// PURPOSE: Manages subscriptions created by all clients.

class COpcHdaSubscriptionMgr : public COpcSynchObject
{
    OPC_CLASS_NEW_DELETE()

public:

    //========================================================================
    // Public Operators

    // Constructor
    COpcHdaSubscriptionMgr();

    // Destructor
    ~COpcHdaSubscriptionMgr();
    
	//=========================================================================
    // Public Methods

	// Start
	bool Start();
	
	// Stop
	void Stop();
    
	// Update
    void Run();

	// CreateSubscription
	void CreateSubscription(COpcHdaTransaction& cTransaction);

	// CancelSubscription
	void CancelSubscription(DWORD dwID);

private:

    //========================================================================
    // Private Members

	DWORD           m_dwID;
	HANDLE          m_hEvent;
	COpcList<DWORD> m_cSubscriptions;
};

#endif // _COpcHdaSubscriptionMgr_H_