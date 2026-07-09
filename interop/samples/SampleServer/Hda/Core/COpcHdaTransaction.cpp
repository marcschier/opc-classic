//============================================================================
// TITLE: COpcHdaTransaction.cpp
//
// CONTENTS:
// 
// Contains all information required to process an asynchronous transaction.
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
#include "COpcHdaTransaction.h"
#include "COpcHdaHistorian.h"
#include "COpcHdaTime.h"

//============================================================================
// Local Variables

static DWORD g_dwLastID = 0;

//==============================================================================
// Local Functions

// ThreadProc
static DWORD WINAPI ThreadProc(LPVOID lpParameter)
{
    ((COpcHdaTransactionQueue*)lpParameter)->Run();
	return 0;
}

// ZeroArray
template<class TYPE> void ZeroArray(COpcArray<TYPE>& cArray, UINT uSize)
{
	cArray.SetSize(uSize);
	memset(cArray.GetData(), 0, sizeof(TYPE)*uSize);
}
    
//============================================================================
// COpcHdaTransactionQueue

// Constructor
COpcHdaTransactionQueue::COpcHdaTransactionQueue()
{
	m_dwID   = NULL;
	m_hEvent = NULL;
}

// Destructor
COpcHdaTransactionQueue::~COpcHdaTransactionQueue()
{
}

// Start
bool COpcHdaTransactionQueue::Start()
{
	COpcLock cLock(*this);

	// check that the thread is not already running.
	if (m_dwID != NULL)
	{
		return false;
	}

	// create event to wait for thread.
	m_hEvent = CreateEvent(
		NULL,
		TRUE,
		FALSE,
		NULL
	);

	if (m_hEvent == NULL)
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

	cLock.Unlock();

	// wait for update thread to start.
	WaitForSingleObject(m_hEvent, INFINITE);

	cLock.Lock();
	ResetEvent(m_hEvent);

	return true;
}

// Stop
void COpcHdaTransactionQueue::Stop()
{
	COpcLock cLock(*this);

	if (m_dwID != NULL)
	{
		// post a quit message.
		PostThreadMessage(m_dwID, WM_QUIT, NULL, NULL);

		// clear thread id to signal shutdown.
		m_dwID = NULL;

		cLock.Unlock();

		// wait for update thread to shutdown.
		WaitForSingleObject(m_hEvent, INFINITE);

		cLock.Lock();
		
		CloseHandle(m_hEvent);
		m_hEvent = NULL;
	}
}

// Run
void COpcHdaTransactionQueue::Run()
{	
	COpcLock cLock(*this);

	// create the message queue.
	MSG cMsg; memset(&cMsg, 0, sizeof(MSG));
	PeekMessage(&cMsg, NULL, NULL, NULL, PM_NOREMOVE);

	// signal startup.
	SetEvent(m_hEvent);

	cLock.Unlock();

	// enter the message loop.
	while (GetMessage(&cMsg, NULL, NULL, NULL))
	{
		GetHistorian().DispatchTransaction(cMsg);
	}

	cLock.Lock();

	// signal shutdown.
	SetEvent(m_hEvent);
}

// QueueTransaction
bool COpcHdaTransactionQueue::QueueTransaction(COpcHdaTransaction& cTransaction)
{
	COpcLock cLock(*this);

	// queue is not running.
	if (m_dwID == NULL)
	{
		return false;
	}

	// the order of transaction is maintained via the thread message queue.
	if (!PostThreadMessage(m_dwID, cTransaction.GetType(), cTransaction.GetID(), NULL))
	{
		return false;
	}

	// all done.
	return true;
}

//============================================================================
// COpcHdaTransaction

// Constructor
COpcHdaTransaction::COpcHdaTransaction(DWORD dwType, IOpcMessageCallback* ipCallback)
: 
	COpcMessage(dwType, ipCallback)
{
	dwClientID  = 0;
	hResult     = S_OK;
	llStartTime = 0;
	llEndTime   = 0;
	uNumValues  = 0;
}

// Copy Constructor
COpcHdaTransaction::COpcHdaTransaction(const COpcHdaTransaction& cTransaction)
:
	COpcMessage(cTransaction)
{
	// copy input parameters.
	dwClientID     = cTransaction.dwClientID;
	hResult        = cTransaction.hResult;
	llStartTime    = cTransaction.llStartTime;
	llEndTime      = cTransaction.llEndTime;
	uNumValues     = cTransaction.uNumValues;
	cTimestamps    = cTransaction.cTimestamps;
	cServerHandles = cTransaction.cServerHandles;
	cClientHandles = cTransaction.cClientHandles;
}

// Destructor 
COpcHdaTransaction::~COpcHdaTransaction() 
{
	// nothing to do.
}

// GetNoOfItems
UINT COpcHdaTransaction::GetNoOfItems() const
{
	return cServerHandles.GetSize();
}

// SetNoOfItems
void COpcHdaTransaction::SetNoOfItems(UINT uCount)
{
	ZeroArray(cServerHandles, uCount);
	ZeroArray(cClientHandles, uCount);
	ZeroArray(cErrors, uCount);
}

//============================================================================
// COpcHdaReadTransaction

// Constructor
COpcHdaReadTransaction::COpcHdaReadTransaction(DWORD dwType, IOpcMessageCallback* ipCallback)
:
	COpcHdaTransaction(dwType, ipCallback)
{
	bIncludeBounds     = false;
	llResampleInterval = 0;
	llUpdateDuration   = 0;
	llUpdateInterval   = 0;
	llLastUpdate       = 0;
	uValuesSent        = 0;
}

// Copy Constructor
COpcHdaReadTransaction::COpcHdaReadTransaction(const COpcHdaReadTransaction& cTransaction)
:
	COpcHdaTransaction(cTransaction)
{
	// copy input parameters.
	bIncludeBounds     = cTransaction.bIncludeBounds;
	llResampleInterval = cTransaction.llResampleInterval;
	llUpdateDuration   = cTransaction.llUpdateDuration;
	llUpdateInterval   = cTransaction.llUpdateInterval;
	llLastUpdate       = cTransaction.llLastUpdate;
	uValuesSent        = cTransaction.uValuesSent;
	cAggregates        = cTransaction.cAggregates;
	cLastTimestamps    = cTransaction.cLastTimestamps;
}

// Destructor
COpcHdaReadTransaction::~COpcHdaReadTransaction()
{
	for (UINT ii = 0; ii < cItems.GetSize(); ii++)
	{
		OPCHDA_ITEM& cItem = cItems[ii];

		for (UINT jj = 0; jj < cItem.dwCount; jj++)
		{
			VariantClear(&cItem.pvDataValues[jj]);
		}

		OpcFree(cItem.pdwQualities);
		OpcFree(cItem.pftTimeStamps);
		OpcFree(cItem.pvDataValues);
	}
}

// SetNoOfItems
void COpcHdaReadTransaction::SetNoOfItems(UINT uCount)
{
	COpcHdaTransaction::SetNoOfItems(uCount);

	ZeroArray(cAggregates, uCount);
	ZeroArray(cLastTimestamps, uCount);
	ZeroArray(cItems, uCount);
}

// ReadRaw
COpcHdaTransaction* COpcHdaReadTransaction::ReadRaw(
	IOpcMessageCallback* ipCallback,
	DWORD                dwTransactionID,
	LONGLONG             llStartTime,
	LONGLONG             llEndTime,
	DWORD                dwNumValues,
	BOOL                 bBounds,
	DWORD                dwNumItems,
	OPCHANDLE*           phServer,
	OPCHANDLE*           phClient,
	DWORD*               pdwCancelID
)
{
	COpcHdaReadTransaction* pTransaction = new COpcHdaReadTransaction(OPC_TRANSACTION_READ_RAW, ipCallback);

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID     = dwTransactionID;
	pTransaction->llStartTime    = llStartTime;
	pTransaction->llEndTime      = llEndTime;
	pTransaction->uNumValues     = dwNumValues;
	pTransaction->bIncludeBounds = (bBounds)?true:false;

	ZeroArray(pTransaction->cServerHandles, dwNumItems);
	ZeroArray(pTransaction->cClientHandles, dwNumItems);
	ZeroArray(pTransaction->cAggregates, dwNumItems);

	memcpy(pTransaction->cServerHandles.GetData(), phServer, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cClientHandles.GetData(), phClient, sizeof(OPCHANDLE)*dwNumItems);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumItems);
	ZeroArray(pTransaction->cItems, dwNumItems);

	// return the new transaction.
	return pTransaction;
}

// AdviseRaw
COpcHdaTransaction* COpcHdaReadTransaction::AdviseRaw(
	IOpcMessageCallback* ipCallback,
	DWORD                dwTransactionID,
	LONGLONG             llStartTime,
	LONGLONG             llUpdateInterval,
	DWORD                dwNumItems,
	OPCHANDLE*           phServer,
	OPCHANDLE*           phClient,
	DWORD*               pdwCancelID
)
{
	COpcHdaReadTransaction* pTransaction = new COpcHdaReadTransaction(OPC_TRANSACTION_ADVISE_RAW, ipCallback);

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID       = dwTransactionID;
	pTransaction->llStartTime      = llStartTime;
	pTransaction->llEndTime        = llStartTime + llUpdateInterval;
	pTransaction->uNumValues       = 0;
	pTransaction->llUpdateDuration = 0;
	pTransaction->llUpdateInterval = llUpdateInterval;
	pTransaction->llLastUpdate     = 0;

	ZeroArray(pTransaction->cServerHandles, dwNumItems);
	ZeroArray(pTransaction->cClientHandles, dwNumItems);
	ZeroArray(pTransaction->cAggregates, dwNumItems);

	memcpy(pTransaction->cServerHandles.GetData(), phServer, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cClientHandles.GetData(), phClient, sizeof(OPCHANDLE)*dwNumItems);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumItems);
	ZeroArray(pTransaction->cItems, dwNumItems);

	// return the new transaction.
	return pTransaction;
}

// ReadProcessed
COpcHdaTransaction* COpcHdaReadTransaction::ReadProcessed(
	IOpcMessageCallback* ipCallback,
	DWORD                dwTransactionID,
	LONGLONG             llStartTime,
	LONGLONG             llEndTime,
	LONGLONG             llResampleInterval,
	DWORD                dwNumItems,
	OPCHANDLE*           phServer,
	OPCHANDLE*           phClient,
	DWORD*               haAggregate,
	DWORD*               pdwCancelID
)
{
	COpcHdaReadTransaction* pTransaction = new COpcHdaReadTransaction(OPC_TRANSACTION_READ_PROCESSED, ipCallback);

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID         = dwTransactionID;
	pTransaction->llStartTime        = llStartTime;
	pTransaction->llEndTime          = llEndTime;
	pTransaction->uNumValues         = 0;
	pTransaction->llResampleInterval = llResampleInterval;

	ZeroArray(pTransaction->cServerHandles, dwNumItems);
	ZeroArray(pTransaction->cClientHandles, dwNumItems);
	ZeroArray(pTransaction->cAggregates, dwNumItems);

	memcpy(pTransaction->cServerHandles.GetData(), phServer, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cClientHandles.GetData(), phClient, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cAggregates.GetData(), haAggregate, sizeof(DWORD)*dwNumItems);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumItems);
	ZeroArray(pTransaction->cItems, dwNumItems);

	// return the new transaction.
	return pTransaction;
}

// AdviseProcessed
COpcHdaTransaction* COpcHdaReadTransaction::AdviseProcessed(
	IOpcMessageCallback* ipCallback,
	DWORD                dwTransactionID,
	LONGLONG             llStartTime,
	LONGLONG             llEndTime,
	LONGLONG             llResampleInterval,
	LONGLONG             llUpdateInterval,
	DWORD                dwNumItems,
	OPCHANDLE*           phServer,
	OPCHANDLE*           phClient,
	DWORD*               haAggregate,
	DWORD*               pdwCancelID
)
{
	COpcHdaReadTransaction* pTransaction = new COpcHdaReadTransaction(OPC_TRANSACTION_ADVISE_PROCESSED, ipCallback);

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID         = dwTransactionID;
	pTransaction->llStartTime        = llStartTime;
	pTransaction->llEndTime          = llEndTime;
	pTransaction->uNumValues         = 0;
	pTransaction->llResampleInterval = llResampleInterval;
	pTransaction->llUpdateDuration   = 0;
	pTransaction->llUpdateInterval   = llUpdateInterval;
	pTransaction->llLastUpdate       = 0;

	ZeroArray(pTransaction->cServerHandles, dwNumItems);
	ZeroArray(pTransaction->cClientHandles, dwNumItems);
	ZeroArray(pTransaction->cAggregates, dwNumItems);

	memcpy(pTransaction->cServerHandles.GetData(), phServer, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cClientHandles.GetData(), phClient, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cAggregates.GetData(), haAggregate, sizeof(DWORD)*dwNumItems);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumItems);
	ZeroArray(pTransaction->cItems, dwNumItems);

	// return the new transaction.
	return pTransaction;
}

// ReadAtTime
COpcHdaTransaction* COpcHdaReadTransaction::ReadAtTime(
	IOpcMessageCallback* ipCallback,
	DWORD                dwTransactionID,
	DWORD                dwNumTimeStamps,
	FILETIME*            ftTimeStamps,
	DWORD                dwNumItems,
	OPCHANDLE*           phServer, 
	OPCHANDLE*           phClient,
	DWORD*               pdwCancelID
)
{
	COpcHdaReadTransaction* pTransaction = new COpcHdaReadTransaction(OPC_TRANSACTION_READ_AT_TIME, ipCallback);

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID  = dwTransactionID;
	pTransaction->llStartTime = 0;
	pTransaction->llEndTime   = 0;
	pTransaction->uNumValues  = 0;

	ZeroArray(pTransaction->cTimestamps, dwNumTimeStamps);

	for (DWORD ii = 0; ii < dwNumTimeStamps; ii++)
	{
		pTransaction->cTimestamps[ii] = OpcHdaInt64FromFILETIME(ftTimeStamps[ii]);
	}
	
	ZeroArray(pTransaction->cServerHandles, dwNumItems);
	ZeroArray(pTransaction->cClientHandles, dwNumItems);

	memcpy(pTransaction->cServerHandles.GetData(), phServer, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cClientHandles.GetData(), phClient, sizeof(OPCHANDLE)*dwNumItems);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumItems);
	ZeroArray(pTransaction->cItems, dwNumItems);

	// return the new transaction.
	return pTransaction;
}

// PlaybackRaw
COpcHdaTransaction* COpcHdaReadTransaction::PlaybackRaw(
	IOpcMessageCallback* ipCallback,
	DWORD                dwTransactionID,
	LONGLONG             llStartTime,
	LONGLONG             llEndTime,
	DWORD                dwNumValues,
	LONGLONG             llUpdateDuration,
	LONGLONG             llUpdateInterval,
	DWORD                dwNumItems,
	OPCHANDLE*           phServer,
	OPCHANDLE*           phClient,
	DWORD*               pdwCancelID
)
{
	COpcHdaReadTransaction* pTransaction = new COpcHdaReadTransaction(OPC_TRANSACTION_PLAYBACK_RAW, ipCallback);

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID       = dwTransactionID;
	pTransaction->llStartTime      = llStartTime;
	pTransaction->llEndTime        = llEndTime;
	pTransaction->uNumValues       = dwNumValues;
	pTransaction->llUpdateDuration = llUpdateDuration;
	pTransaction->llUpdateInterval = llUpdateInterval;
	pTransaction->llLastUpdate     = 0;

	ZeroArray(pTransaction->cServerHandles, dwNumItems);
	ZeroArray(pTransaction->cClientHandles, dwNumItems);
	ZeroArray(pTransaction->cAggregates, dwNumItems);
	ZeroArray(pTransaction->cLastTimestamps, dwNumItems);

	for (DWORD ii = 0; ii < dwNumItems; ii++)
	{
		pTransaction->cLastTimestamps[ii] = llStartTime;
	}

	memcpy(pTransaction->cServerHandles.GetData(), phServer, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cClientHandles.GetData(), phClient, sizeof(OPCHANDLE)*dwNumItems);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumItems);
	ZeroArray(pTransaction->cItems, dwNumItems);

	// return the new transaction.
	return pTransaction;
}

// PlaybackProcessed
COpcHdaTransaction* COpcHdaReadTransaction::PlaybackProcessed(
	IOpcMessageCallback* ipCallback,
	DWORD                dwTransactionID,
	LONGLONG             llStartTime,
	LONGLONG             llEndTime,
	LONGLONG             llResampleInterval,
	DWORD                dwNumIntervals,
	LONGLONG             llUpdateInterval,
	DWORD                dwNumItems,
	OPCHANDLE*           phServer,
	OPCHANDLE*           phClient,
	DWORD*               haAggregate,
	DWORD*               pdwCancelID
)
{
	COpcHdaReadTransaction* pTransaction = new COpcHdaReadTransaction(OPC_TRANSACTION_PLAYBACK_PROCESSED, ipCallback);

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID         = dwTransactionID;
	pTransaction->llStartTime        = llStartTime;
	pTransaction->llEndTime          = llEndTime;
	pTransaction->llResampleInterval = llResampleInterval;
	pTransaction->uNumValues         = 0;
	pTransaction->llUpdateDuration   = llResampleInterval*dwNumIntervals;
	pTransaction->llUpdateInterval   = llUpdateInterval;
	pTransaction->llLastUpdate       = 0;

	ZeroArray(pTransaction->cServerHandles, dwNumItems);
	ZeroArray(pTransaction->cClientHandles, dwNumItems);
	ZeroArray(pTransaction->cAggregates, dwNumItems);

	memcpy(pTransaction->cServerHandles.GetData(), phServer, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cClientHandles.GetData(), phClient, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cAggregates.GetData(), haAggregate, sizeof(DWORD)*dwNumItems);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumItems);
	ZeroArray(pTransaction->cItems, dwNumItems);

	// return the new transaction.
	return pTransaction;
}

//============================================================================
// COpcHdaModifiedTransaction

// Constructor
COpcHdaModifiedTransaction::COpcHdaModifiedTransaction(DWORD dwType, IOpcMessageCallback* ipCallback)
:
	COpcHdaTransaction(dwType, ipCallback)
{
	uValuesSent = 0;
}

// Copy Constructor
COpcHdaModifiedTransaction::COpcHdaModifiedTransaction(const COpcHdaModifiedTransaction& cTransaction)
:
	COpcHdaTransaction(cTransaction)
{
	uValuesSent = cTransaction.uValuesSent;
}

// Destructor
COpcHdaModifiedTransaction::~COpcHdaModifiedTransaction()
{
	for (UINT ii = 0; ii < cItems.GetSize(); ii++)
	{
		OPCHDA_MODIFIEDITEM& cItem = cItems[ii];

		for (UINT jj = 0; jj < cItem.dwCount; jj++)
		{
			VariantClear(&cItem.pvDataValues[jj]);
			OpcFree(cItem.szUser[jj]);
		}

		OpcFree(cItem.pvDataValues);
		OpcFree(cItem.pdwQualities);
		OpcFree(cItem.pftTimeStamps);
		OpcFree(cItem.pftModificationTime);
		OpcFree(cItem.pEditType);
		OpcFree(cItem.szUser);
	}
}
 
// SetNoOfItems
void COpcHdaModifiedTransaction::SetNoOfItems(UINT uCount)
{
	COpcHdaTransaction::SetNoOfItems(uCount);
	ZeroArray(cItems, uCount);
}

// ReadModified
COpcHdaTransaction* COpcHdaModifiedTransaction::ReadModified(
	IOpcMessageCallback* ipCallback,
	DWORD                dwTransactionID,
	LONGLONG             llStartTime,
	LONGLONG             llEndTime,
	DWORD                dwNumValues,
	DWORD                dwNumItems,
	OPCHANDLE*           phServer,
	OPCHANDLE*           phClient,
	DWORD*               pdwCancelID
)
{
	COpcHdaModifiedTransaction* pTransaction = new COpcHdaModifiedTransaction(OPC_TRANSACTION_READ_MODIFIED, ipCallback);

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID  = dwTransactionID;
	pTransaction->llStartTime = llStartTime;
	pTransaction->llEndTime   = llEndTime;
	pTransaction->uNumValues  = dwNumValues;

	ZeroArray(pTransaction->cServerHandles, dwNumItems);
	ZeroArray(pTransaction->cClientHandles, dwNumItems);

	memcpy(pTransaction->cServerHandles.GetData(), phServer, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cClientHandles.GetData(), phClient, sizeof(OPCHANDLE)*dwNumItems);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumItems);
	ZeroArray(pTransaction->cItems, dwNumItems);

	// return the new transaction.
	return pTransaction;
}

//============================================================================
// COpcHdaAttributeTransaction

// Constructor
COpcHdaAttributeTransaction::COpcHdaAttributeTransaction(DWORD dwType, IOpcMessageCallback* ipCallback)
:
	COpcHdaTransaction(dwType, ipCallback)
{
}

// Copy Constructor
COpcHdaAttributeTransaction::COpcHdaAttributeTransaction(const COpcHdaAttributeTransaction& cTransaction)
:
	COpcHdaTransaction(cTransaction)
{
	// copy input parameters.
	cAtributeIDs = cAtributeIDs;
}

// Destructor
COpcHdaAttributeTransaction::~COpcHdaAttributeTransaction()
{
	for (UINT ii = 0; ii < cAttributes.GetSize(); ii++)
	{
		OPCHDA_ATTRIBUTE& cAttribute = cAttributes[ii];

		for (UINT jj = 0; jj < cAttribute.dwNumValues; jj++)
		{
			VariantClear(&cAttribute.vAttributeValues[jj]);
		}

		OpcFree(cAttribute.ftTimeStamps);
	}
}

// ReadAttributes
COpcHdaTransaction* COpcHdaAttributeTransaction::ReadAttributes(
	IOpcMessageCallback* ipCallback,
	DWORD                dwTransactionID,
	LONGLONG             llStartTime,
	LONGLONG             llEndTime,
	OPCHANDLE            hServer,
	OPCHANDLE            hClient,
	DWORD                dwNumAttributes,
	DWORD*               pdwAttributeIDs,
	DWORD*               pdwCancelID
)
{
	COpcHdaAttributeTransaction* pTransaction = new COpcHdaAttributeTransaction(OPC_TRANSACTION_READ_ATTRIBUTE, ipCallback);

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID  = dwTransactionID;
	pTransaction->llStartTime = llStartTime;
	pTransaction->llEndTime   = llEndTime;
	pTransaction->uNumValues  = 0;

	ZeroArray(pTransaction->cServerHandles, 1);
	ZeroArray(pTransaction->cClientHandles, 1);
	ZeroArray(pTransaction->cAtributeIDs, dwNumAttributes);

	memcpy(pTransaction->cServerHandles.GetData(), &hServer, sizeof(OPCHANDLE)*1);
	memcpy(pTransaction->cClientHandles.GetData(), &hClient, sizeof(OPCHANDLE)*1);
	memcpy(pTransaction->cAtributeIDs.GetData(), pdwAttributeIDs, sizeof(DWORD)*dwNumAttributes);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumAttributes);
	ZeroArray(pTransaction->cAttributes, dwNumAttributes);

	// return the new transaction.
	return pTransaction;
}

//============================================================================
// COpcHdaAnnotationTransaction

// Constructor
COpcHdaAnnotationTransaction::COpcHdaAnnotationTransaction(DWORD dwType, IOpcMessageCallback* ipCallback)
:
	COpcHdaTransaction(dwType, ipCallback)
{
}

// Copy Constructor
COpcHdaAnnotationTransaction::COpcHdaAnnotationTransaction(const COpcHdaAnnotationTransaction& cTransaction)
:
	COpcHdaTransaction(cTransaction)
{
}

// Destructor
COpcHdaAnnotationTransaction::~COpcHdaAnnotationTransaction()
{
	for (UINT ii = 0; ii < cAnnotations.GetSize(); ii++)
	{
		OPCHDA_ANNOTATION& cAnnotation = cAnnotations[ii];

		for (UINT jj = 0; jj < cAnnotation.dwNumValues; jj++)
		{
			OpcFree(cAnnotation.szAnnotation[jj]);
			OpcFree(cAnnotation.szUser[jj]);
		}

		OpcFree(cAnnotation.ftTimeStamps);
		OpcFree(cAnnotation.szAnnotation);
		OpcFree(cAnnotation.ftAnnotationTime);
		OpcFree(cAnnotation.szUser);
	}
}

// SetNoOfItems
void COpcHdaAnnotationTransaction::SetNoOfItems(UINT uCount)
{
	COpcHdaTransaction::SetNoOfItems(uCount);
	ZeroArray(cAnnotations, uCount);
}

// ReadAnnotations
COpcHdaTransaction* COpcHdaAnnotationTransaction::ReadAnnotations(
	IOpcMessageCallback* ipCallback,
	DWORD                dwTransactionID,
	LONGLONG             llStartTime,
	LONGLONG             llEndTime,
	DWORD                dwNumItems,
	OPCHANDLE*           phServer,
	OPCHANDLE*           phClient,
	DWORD*               pdwCancelID
)
{
	COpcHdaAnnotationTransaction* pTransaction = new COpcHdaAnnotationTransaction(OPC_TRANSACTION_READ_ANNOTATION, ipCallback);

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID  = dwTransactionID;
	pTransaction->llStartTime = llStartTime;
	pTransaction->llEndTime   = llEndTime;
	pTransaction->uNumValues  = 0;

	ZeroArray(pTransaction->cServerHandles, dwNumItems);
	ZeroArray(pTransaction->cClientHandles, dwNumItems);

	memcpy(pTransaction->cServerHandles.GetData(), phServer, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cClientHandles.GetData(), phClient, sizeof(OPCHANDLE)*dwNumItems);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumItems);
	ZeroArray(pTransaction->cAnnotations, dwNumItems);

	// return the new transaction.
	return pTransaction;
}

// InsertAnnotations
COpcHdaTransaction* COpcHdaAnnotationTransaction::InsertAnnotations(
	IOpcMessageCallback* ipCallback,
	DWORD                dwTransactionID,
	DWORD                dwNumItems,
	OPCHANDLE*           phServer, 
	OPCHANDLE*           phClient,
	LONGLONG*            pllTimestamps,
	OPCHDA_ANNOTATION*   pAnnotationValues,
	DWORD*               pdwCancelID
)
{
	COpcHdaAnnotationTransaction* pTransaction = new COpcHdaAnnotationTransaction(OPC_TRANSACTION_INSERT_ANNOTATION, ipCallback);

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID  = dwTransactionID;
	pTransaction->llStartTime = 0;
	pTransaction->llEndTime   = 0;
	pTransaction->uNumValues  = 0;

	ZeroArray(pTransaction->cServerHandles, dwNumItems);
	ZeroArray(pTransaction->cClientHandles, dwNumItems);
	ZeroArray(pTransaction->cTimestamps, dwNumItems);
	ZeroArray(pTransaction->cAnnotations, dwNumItems);

	memcpy(pTransaction->cServerHandles.GetData(), phServer, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cClientHandles.GetData(), phClient, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cTimestamps.GetData(), pllTimestamps, sizeof(LONGLONG)*dwNumItems);
	memcpy(pTransaction->cAnnotations.GetData(), pAnnotationValues, sizeof(OPCHDA_ANNOTATION)*dwNumItems);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumItems);

	// return the new transaction.
	return pTransaction;
}

//============================================================================
// COpcHdaUpdateTransaction

// Constructor
COpcHdaUpdateTransaction::COpcHdaUpdateTransaction(DWORD dwType, IOpcMessageCallback* ipCallback)
:
	COpcHdaTransaction(dwType, ipCallback)
{
}

// Copy Constructor
COpcHdaUpdateTransaction::COpcHdaUpdateTransaction(const COpcHdaUpdateTransaction& cTransaction)
:
	COpcHdaTransaction(cTransaction)
{
}

// Destructor
COpcHdaUpdateTransaction::~COpcHdaUpdateTransaction()
{
	for (UINT ii = 0; ii < cValues.GetSize(); ii++)
	{
		VariantClear(&cValues[ii]);
	}
}

// SetNoOfItems
void COpcHdaUpdateTransaction::SetNoOfItems(UINT uCount)
{
	COpcHdaTransaction::SetNoOfItems(uCount);

	ZeroArray(cValues, uCount);
	ZeroArray(cQualities, uCount);
	ZeroArray(cTimestamps, uCount);
}

// Update
COpcHdaTransaction* COpcHdaUpdateTransaction::Update(
	IOpcMessageCallback* ipCallback,
	OPCHDA_EDITTYPE      eEditType,
	DWORD                dwTransactionID,
	DWORD                dwNumItems,
	OPCHANDLE*           phServer,
	OPCHANDLE*           phClient,
	VARIANT*             pvValues,
	DWORD*               pdwQualities,
	LONGLONG*            pllTimestamps,
	DWORD*               pdwCancelID
)
{
	// create transaction.
	COpcHdaUpdateTransaction* pTransaction = NULL;
	
	switch (eEditType)
	{
		case OPCHDA_INSERT:
		{
			pTransaction = new COpcHdaUpdateTransaction(OPC_TRANSACTION_INSERT, ipCallback);
			break;
		}

		case OPCHDA_REPLACE:
		{
			pTransaction = new COpcHdaUpdateTransaction(OPC_TRANSACTION_REPLACE, ipCallback);
			break;
		}

		case OPCHDA_INSERTREPLACE:
		{
			pTransaction = new COpcHdaUpdateTransaction(OPC_TRANSACTION_INSERT_REPLACE, ipCallback);
			break;
		}
	}

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID  = dwTransactionID;
	pTransaction->llStartTime = 0;
	pTransaction->llEndTime   = 0;
	pTransaction->uNumValues  = 0;

	ZeroArray(pTransaction->cServerHandles, dwNumItems);
	ZeroArray(pTransaction->cClientHandles, dwNumItems);
	ZeroArray(pTransaction->cValues, dwNumItems);
	ZeroArray(pTransaction->cQualities, dwNumItems);
	ZeroArray(pTransaction->cTimestamps, dwNumItems);

	memcpy(pTransaction->cServerHandles.GetData(), phServer, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cClientHandles.GetData(), phClient, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cValues.GetData(), pvValues, sizeof(VARIANT)*dwNumItems);
	memcpy(pTransaction->cQualities.GetData(), pdwQualities, sizeof(DWORD)*dwNumItems);
	memcpy(pTransaction->cTimestamps.GetData(), pllTimestamps, sizeof(LONGLONG)*dwNumItems);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumItems);

	// return the new transaction.
	return pTransaction;
}

// Delete
COpcHdaTransaction* COpcHdaUpdateTransaction::Delete(
	IOpcMessageCallback* ipCallback,
	DWORD                dwTransactionID,
	LONGLONG             llStartTime,
	LONGLONG             llEndTime,
	DWORD                dwNumItems,
	OPCHANDLE*           phServer,
	OPCHANDLE*           phClient,
	DWORD*               pdwCancelID
)
{
	COpcHdaUpdateTransaction* pTransaction = new COpcHdaUpdateTransaction(OPC_TRANSACTION_DELETE, ipCallback);

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID  = dwTransactionID;
	pTransaction->llStartTime = llStartTime;
	pTransaction->llEndTime   = llEndTime;
	pTransaction->uNumValues  = 0;

	ZeroArray(pTransaction->cServerHandles, dwNumItems);
	ZeroArray(pTransaction->cClientHandles, dwNumItems);

	memcpy(pTransaction->cServerHandles.GetData(), phServer, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cClientHandles.GetData(), phClient, sizeof(OPCHANDLE)*dwNumItems);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumItems);

	// return the new transaction.
	return pTransaction;
}

// DeleteAtTime
COpcHdaTransaction* COpcHdaUpdateTransaction::DeleteAtTime(
	IOpcMessageCallback* ipCallback,
	DWORD                dwTransactionID,
	DWORD                dwNumItems,
	OPCHANDLE*           phServer,
	OPCHANDLE*           phClient,
	LONGLONG*            pllTimestamps,
	DWORD*               pdwCancelID
)
{
	COpcHdaUpdateTransaction* pTransaction = new COpcHdaUpdateTransaction(OPC_TRANSACTION_DELETE_AT_TIME, ipCallback);

	// get unique id for transaction.
	*pdwCancelID = pTransaction->GetID();

	// copy input parameters.
	pTransaction->dwClientID  = dwTransactionID;
	pTransaction->llStartTime = 0;
	pTransaction->llEndTime   = 0;
	pTransaction->uNumValues  = 0;
	
	ZeroArray(pTransaction->cServerHandles, dwNumItems);
	ZeroArray(pTransaction->cClientHandles, dwNumItems);
	ZeroArray(pTransaction->cTimestamps, dwNumItems);

	memcpy(pTransaction->cServerHandles.GetData(), phServer, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cClientHandles.GetData(), phClient, sizeof(OPCHANDLE)*dwNumItems);
	memcpy(pTransaction->cTimestamps.GetData(), pllTimestamps, sizeof(LONGLONG)*dwNumItems);

	// initialize output parameters.
	pTransaction->hResult = S_OK;
	ZeroArray(pTransaction->cErrors, dwNumItems);

	// return the new transaction.
	return pTransaction;
}


