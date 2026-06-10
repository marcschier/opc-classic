//============================================================================
// TITLE: COpcHdaSubscriptionMgr.cpp
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

#include "StdAfx.h"
#include "COpcHdaSubscriptionMgr.h"
#include "COpcHdaHistorian.h"

//==============================================================================
// Local Functions

static DWORD WINAPI ThreadProc(LPVOID lpParameter)
{
    ((COpcHdaSubscriptionMgr*)lpParameter)->Run();
	return 0;
}

//============================================================================
// COpcHdaSubscriptionMgr

// Constructor
COpcHdaSubscriptionMgr::COpcHdaSubscriptionMgr()
{
	m_dwID   = NULL;
	m_hEvent = NULL;
}

// Destructor
COpcHdaSubscriptionMgr::~COpcHdaSubscriptionMgr()
{
	OPC_ASSERT(m_cSubscriptions.GetCount() == 0);
}

// Start
bool COpcHdaSubscriptionMgr::Start()
{
	COpcLock cLock(*this);

	// check that the thread is not already running.
	if (m_dwID != NULL)
	{
		return false;
	}

	// start thread.
	HANDLE hThread = CreateThread(
		NULL,
		NULL,
		ThreadProc,
		(void*)this,
		NULL,
		&m_dwID);

	if (hThread == NULL)
	{
		return false;
	}

	// close thread handle.
	CloseHandle(hThread);
	return true;
}

// Stop
void COpcHdaSubscriptionMgr::Stop()
{
	COpcLock cLock(*this);

	// check if already stopped.
	if (m_dwID == NULL)
	{
		return;
	}

	// delete all existing subscriptions.
	m_cSubscriptions.RemoveAll();

	// create event to wait for shutdown.
	m_hEvent = CreateEvent(
		NULL,
		TRUE,
		FALSE,
		NULL
	);

	// clear thread id to signal shutdown.
	m_dwID = NULL;

	cLock.Unlock();

	// wait for update thread to shutdown.
	WaitForSingleObject(m_hEvent, INFINITE);

	cLock.Lock();
	
	CloseHandle(m_hEvent);
	m_hEvent = NULL;
}

// Run
void COpcHdaSubscriptionMgr::Run()
{
	LONGLONG llEnd   = 0;
	LONGLONG llStart = 0;
	LONGLONG llFreq  = 0;
	
	QueryPerformanceFrequency((LARGE_INTEGER*)&llFreq);

	LONGLONG uTicks = 0;

	do 
	{
		QueryPerformanceCounter((LARGE_INTEGER*)&llEnd);

		UINT uWaitTime = MAX_UPDATE_RATE;

		if (llStart != 0)
		{
			double delay = (((double)(llEnd - llStart))/((double)llFreq))*1000 - MAX_UPDATE_RATE;

			if (delay > 0)
			{
				uWaitTime = (delay < uWaitTime)?(uWaitTime - (UINT)delay):0;
			}
		}

		QueryPerformanceCounter((LARGE_INTEGER*)&llStart);

		// wait until next update.
		Sleep(uWaitTime);

		COpcLock cLock(*this);

		// check for shutdown.
		if (m_dwID != GetCurrentThreadId())
		{
			SetEvent(m_hEvent);
			break;
		}

		// copy the current set of subscriptions.
		COpcList<DWORD> cSubscriptions = m_cSubscriptions;

		cLock.Unlock();

		// do updates for subscriptions.
		GetHistorian().SubscriptionUpdate(uTicks, cSubscriptions);
		
		// increment counter.
		uTicks++;
	}
	while (true);
}

// CreateSubscription
void COpcHdaSubscriptionMgr::CreateSubscription(COpcHdaTransaction& cTransaction)
{
	COpcLock cLock(*this);
	
	// check if running.
	if (m_dwID == NULL)
	{
		return;
	}

	// add transaction.
	m_cSubscriptions.AddTail(cTransaction.GetID());
}

// CancelSubscription
void COpcHdaSubscriptionMgr::CancelSubscription(DWORD dwID)
{
	COpcLock cLock(*this);
	
	// check if running.
	if (m_dwID == NULL)
	{
		return;
	}

	// remove subscription from list.
	OPC_POS pos = m_cSubscriptions.GetHeadPosition();

	while (pos != NULL)
	{
		if (dwID == m_cSubscriptions[pos])
		{
			m_cSubscriptions.RemoveAt(pos);
			break;
		}

		m_cSubscriptions.GetNext(pos);
	}
}
