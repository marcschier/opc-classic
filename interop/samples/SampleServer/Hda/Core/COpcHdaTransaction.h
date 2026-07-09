//============================================================================
// TITLE: COpcHdaTransaction.h
//
// CONTENTS:
// 
// Contains all information required to process an asynchronous transaction.
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
// 2004/01/29 RSA   Initial implementation.

#ifndef _COpcHdaTransaction_H_
#define _COpcHdaTransaction_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

//============================================================================
// MACROS:  OPC_TRANSACTION_XXX
// PURPOSE: Defines unique ids for transaction types.

#define OPC_TRANSACTION_READ_RAW           WM_APP+0x01
#define OPC_TRANSACTION_ADVISE_RAW         WM_APP+0x02
#define OPC_TRANSACTION_PLAYBACK_RAW       WM_APP+0x03
#define OPC_TRANSACTION_READ_PROCESSED     WM_APP+0x04
#define OPC_TRANSACTION_ADVISE_PROCESSED   WM_APP+0x05
#define OPC_TRANSACTION_PLAYBACK_PROCESSED WM_APP+0x06
#define OPC_TRANSACTION_READ_AT_TIME       WM_APP+0x07
#define OPC_TRANSACTION_READ_ATTRIBUTE     WM_APP+0x08
#define OPC_TRANSACTION_READ_MODIFIED      WM_APP+0x09
#define OPC_TRANSACTION_INSERT             WM_APP+0x0A
#define OPC_TRANSACTION_REPLACE            WM_APP+0x0B
#define OPC_TRANSACTION_INSERT_REPLACE     WM_APP+0x0C
#define OPC_TRANSACTION_DELETE             WM_APP+0x0D
#define OPC_TRANSACTION_DELETE_AT_TIME     WM_APP+0x0E
#define OPC_TRANSACTION_READ_ANNOTATION    WM_APP+0x0F
#define OPC_TRANSACTION_INSERT_ANNOTATION  WM_APP+0x10

//============================================================================
// CLASS:   COpcHdaTransaction
// PURPOSE: Stores the arguments and results for an asynchronous transaction.

class COpcHdaTransaction : public COpcMessage
{
    OPC_CLASS_NEW_DELETE()

public:

    //=========================================================================
    // Public Properties

	// request parameters.
    DWORD                dwClientID;
	HRESULT              hResult;
    
	// time domain definition.
	LONGLONG             llStartTime;
	LONGLONG             llEndTime;
	UINT                 uNumValues;
	COpcArray<LONGLONG>  cTimestamps;
	
	// item identifiers.
	COpcArray<OPCHANDLE> cServerHandles;
	COpcArray<OPCHANDLE> cClientHandles;

	// item results.
	COpcArray<HRESULT>   cErrors;

    //=========================================================================
    // Public Operators

    // Constructor
	COpcHdaTransaction(DWORD dwType, IOpcMessageCallback* ipCallback);

	// Copy Constructor
	COpcHdaTransaction(const COpcHdaTransaction& cTransaction);

    // Destructor 
    ~COpcHdaTransaction();

	//=========================================================================
    // Public Methods

	// GetNoOfItems
	UINT GetNoOfItems() const;

	// SetNoOfItems
	virtual void SetNoOfItems(UINT uCount);
};

//============================================================================
// TYPE:    COpcHdaTransactionTable
// PURPOSE: A table of transactions indexed by id.

typedef COpcMap<DWORD,COpcHdaTransaction*> COpcHdaTransactionTable;

//============================================================================
// CLASS:   COpcHdaTransactionQueue
// PURPOSE: Manages the processing of asynchronous transactions.

class COpcHdaTransactionQueue : public COpcSynchObject
{
    OPC_CLASS_NEW_DELETE()

public:

    //========================================================================
    // Public Operators

    // Constructor
    COpcHdaTransactionQueue();

    // Destructor
    ~COpcHdaTransactionQueue();
    
	//=========================================================================
    // Public Methods

	// Start
	bool Start();
	
	// Stop
	void Stop();
    
	// Run
    void Run();

	// QueueTransaction
	bool QueueTransaction(COpcHdaTransaction& pTransaction);

private:

    //========================================================================
    // Private Members

	DWORD  m_dwID;
	HANDLE m_hEvent;
};

//============================================================================
// CLASS:   COpcHdaReadTransaction
// PURPOSE: Stores the arguments and results for an read transaction.

class COpcHdaReadTransaction : public COpcHdaTransaction
{
    OPC_CLASS_NEW_DELETE()
	
public:

    //=========================================================================
    // Public Properties

	// request arguments.
	bool                 bIncludeBounds;
	LONGLONG            llResampleInterval;
	LONGLONG            llUpdateDuration;
	LONGLONG            llUpdateInterval;
	LONGLONG            llLastUpdate;
	COpcArray<DWORD>    cAggregates;
	COpcArray<LONGLONG> cLastTimestamps;
	UINT                uValuesSent;

	// request results
	COpcArray<OPCHDA_ITEM> cItems;

    //=========================================================================
    // Public Operators

	// Constructor
	COpcHdaReadTransaction(DWORD dwType, IOpcMessageCallback* ipCallback);

	// Copy Constructor
	COpcHdaReadTransaction(const COpcHdaReadTransaction& cTransaction);

	// Destructor
	~COpcHdaReadTransaction();
	
	//=========================================================================
    // Public Methods

	// SetNoOfItems
	virtual void SetNoOfItems(UINT uCount);

	//=========================================================================
    // Static Methods

	// ReadRaw
	static COpcHdaTransaction* ReadRaw(
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
	);

	// AdviseRaw
	static COpcHdaTransaction* AdviseRaw(
		IOpcMessageCallback* ipCallback,
		DWORD                dwTransactionID,
		LONGLONG             llStartTime,
		LONGLONG             llUpdateInterval,
		DWORD                dwNumItems,
		OPCHANDLE*           phServer,
		OPCHANDLE*           phClient,
		DWORD*               pdwCancelID
	);

	// ReadProcessed
	static COpcHdaTransaction* ReadProcessed(
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
	);

	// AdviseProcessed
	static COpcHdaTransaction* AdviseProcessed(
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
	);

	// ReadAtTime
	static COpcHdaTransaction* ReadAtTime(
		IOpcMessageCallback* ipCallback,
		DWORD                dwTransactionID,
		DWORD                dwNumTimeStamps,
		FILETIME*            ftTimeStamps,
		DWORD                dwNumItems,
		OPCHANDLE*           phServer, 
		OPCHANDLE*           phClient,
		DWORD*               pdwCancelID
	);

	// PlaybackRaw
	static COpcHdaTransaction* PlaybackRaw(
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
	);

	// PlaybackProcessed
	static COpcHdaTransaction* PlaybackProcessed(
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
	);
};

//============================================================================
// CLASS:   COpcHdaModifiedTransaction
// PURPOSE: Stores the arguments and results for an read modified transaction.

class COpcHdaModifiedTransaction : public COpcHdaTransaction
{
    OPC_CLASS_NEW_DELETE()
	
public:

    //=========================================================================
    // Public Properties

	UINT uValuesSent;

	COpcArray<OPCHDA_MODIFIEDITEM> cItems;

    //=========================================================================
    // Public Operators

	// Constructor
	COpcHdaModifiedTransaction(DWORD dwType, IOpcMessageCallback* ipCallback);

	// Copy Constructor
	COpcHdaModifiedTransaction(const COpcHdaModifiedTransaction& cTransaction);

	// Destructor
	~COpcHdaModifiedTransaction();

	//=========================================================================
    // Public Methods

	// SetNoOfItems
	virtual void SetNoOfItems(UINT uCount);

	//=========================================================================
    // Static Methods

	// ReadRaw
	static COpcHdaTransaction* ReadModified(
		IOpcMessageCallback* ipCallback,
		DWORD                dwTransactionID,
		LONGLONG             llStartTime,
		LONGLONG             llEndTime,
		DWORD                dwNumValues,
		DWORD                dwNumItems,
		OPCHANDLE*           phServer,
		OPCHANDLE*           phClient,
		DWORD*               pdwCancelID
	);
};

//============================================================================
// CLASS:   COpcHdaAttributeTransaction
// PURPOSE: Stores the arguments and results for an read attributes transaction.

class COpcHdaAttributeTransaction : public COpcHdaTransaction
{
    OPC_CLASS_NEW_DELETE()
	
public:

    //=========================================================================
    // Public Properties

	COpcArray<DWORD>            cAtributeIDs;
	COpcArray<OPCHDA_ATTRIBUTE> cAttributes;

    //=========================================================================
    // Public Operators

	// Constructor
	COpcHdaAttributeTransaction(DWORD dwType, IOpcMessageCallback* ipCallback);

	// Copy Constructor
	COpcHdaAttributeTransaction(const COpcHdaAttributeTransaction& cTransaction);

	// Destructor
	~COpcHdaAttributeTransaction();

    //=========================================================================
    // Static Methods

	// ReadAttributes
	static COpcHdaTransaction* ReadAttributes(
		IOpcMessageCallback* ipCallback,
		DWORD                dwTransactionID,
		LONGLONG             llStartTime,
		LONGLONG             llEndTime,
		OPCHANDLE            hServer,
		OPCHANDLE            hClient,
		DWORD                dwNumAttributes,
		DWORD*               pdwAttributeIDs,
		DWORD*               pdwCancelID
	);
};

//============================================================================
// CLASS:   COpcHdaAnnotationTransaction
// PURPOSE: Stores the arguments and results for an read\insert annotations transaction.

class COpcHdaAnnotationTransaction : public COpcHdaTransaction
{
    OPC_CLASS_NEW_DELETE()
	
public:

    //=========================================================================
    // Public Properties

	COpcArray<OPCHDA_ANNOTATION> cAnnotations;

    //=========================================================================
    // Public Operators

	// Constructor
	COpcHdaAnnotationTransaction(DWORD dwType, IOpcMessageCallback* ipCallback);

	// Copy Constructor
	COpcHdaAnnotationTransaction(const COpcHdaAnnotationTransaction& cTransaction);

	// Destructor
	~COpcHdaAnnotationTransaction();

	//=========================================================================
    // Public Methods

	// SetNoOfItems
	virtual void SetNoOfItems(UINT uCount);

	//=========================================================================
    // Static Methods

	// ReadAnnotations
	static COpcHdaTransaction* ReadAnnotations(
		IOpcMessageCallback* ipCallback,
		DWORD                dwTransactionID,
		LONGLONG             llStartTime,
		LONGLONG             llEndTime,
		DWORD                dwNumItems,
		OPCHANDLE*           phServer,
		OPCHANDLE*           phClient,
		DWORD*               pdwCancelID
	);

	// InsertAnnotations
	static COpcHdaTransaction* InsertAnnotations(
		IOpcMessageCallback* ipCallback,
		DWORD                dwTransactionID,
		DWORD                dwNumItems,
		OPCHANDLE*           phServer, 
		OPCHANDLE*           phClient,
		LONGLONG*            pllTimestamps,
		OPCHDA_ANNOTATION*   pAnnotationValues,
		DWORD*               pdwCancelID
	);
};

//============================================================================
// CLASS:   COpcHdaUpdateTransaction
// PURPOSE: Stores the arguments and results for an insert\replace\delete transaction.

class COpcHdaUpdateTransaction : public COpcHdaTransaction
{
    OPC_CLASS_NEW_DELETE()
	
public:

    //=========================================================================
    // Public Properties

	COpcArray<VARIANT> cValues;
	COpcArray<DWORD>   cQualities;

    //=========================================================================
    // Public Operators

	// Constructor
	COpcHdaUpdateTransaction(DWORD dwType, IOpcMessageCallback* ipCallback);

	// Copy Constructor
	COpcHdaUpdateTransaction(const COpcHdaUpdateTransaction& cTransaction);

	// Destructor
	~COpcHdaUpdateTransaction();

	//=========================================================================
    // Public Methods

	// SetNoOfItems
	virtual void SetNoOfItems(UINT uCount);

    //=========================================================================
    // Static Methods

	// Update
	static COpcHdaTransaction* Update(
		IOpcMessageCallback* ipCallback,
		OPCHDA_EDITTYPE      eEditType,
		DWORD                dwTransactionID,
		DWORD                dwNumItems,
		OPCHANDLE*           phServer,
		OPCHANDLE*           phClient,
		VARIANT*             pvValues,
		DWORD*               pdwQualities,
		LONGLONG*            llTimestamps,
		DWORD*               pdwCancelID
	);

	// Delete
	static COpcHdaTransaction* Delete(
		IOpcMessageCallback* ipCallback,
		DWORD                dwTransactionID,
		LONGLONG             llStartTime,
		LONGLONG             llEndTime,
		DWORD                dwNumItems,
		OPCHANDLE*           phServer,
		OPCHANDLE*           phClient,
		DWORD*               pdwCancelID
	);

	// DeleteAtTime
	static COpcHdaTransaction* DeleteAtTime(
		IOpcMessageCallback* ipCallback,
		DWORD                dwTransactionID,
		DWORD                dwNumItems,
		OPCHANDLE*           phServer,
		OPCHANDLE*           phClient,
		LONGLONG*            pllTimestamps,
		DWORD*               pdwCancelID
	);
};

#endif // _COpcHdaTransaction_H_