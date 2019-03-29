//============================================================================
// TITLE: COpcHdaHistorian.h
//
// CONTENTS:
// 
// Implements an OPC Historical Data Access database.
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

#ifndef _COpcHdaHistorian_H_
#define _COpcHdaHistorian_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

#include "COpcHdaItem.h"
#include "COpcHdaAttribute.h"
#include "COpcHdaSubscriptionMgr.h"

//============================================================================
// CLASS:   COpcHdaHistorian
// PURPOSE: A class that implements the IOPCServer interface.
// NOTES:

class COpcHdaHistorian : public COpcSynchObject
{
    OPC_CLASS_NEW_DELETE()

public:

	//=========================================================================
    // Operators

    // Constructor
    COpcHdaHistorian();

    // Destructor 
    ~COpcHdaHistorian();

	//=========================================================================
    // Configuration Methods

	// Start
	virtual bool Start();
	
	// Stop
	virtual void Stop();

	// Load
	virtual bool Load(const COpcString& cFileName);

	//=========================================================================
    // Status Methods

	// GetHistorianStatus
	HRESULT GetHistorianStatus(
		OPCHDA_SERVERSTATUS* pwStatus,
		FILETIME**           pftCurrentTime,
		FILETIME**           pftStartTime,
		WORD*                pwMajorVersion,
		WORD*                pwMinorVersion,
		WORD*                pwBuildNumber,
		DWORD*               pdwMaxReturnValues,
		LPWSTR*              ppszStatusString,
		LPWSTR*              ppszVendorInfo
	);
	
	//=========================================================================
    // Address Space Methods

    // AddLink
    bool AddLink(const COpcString& cBrowsePath);

    // AddLink
    bool AddLink(const COpcString& cBrowsePath, const COpcString& cItemID);
    
    // RemoveLink
    bool RemoveLink(const COpcString& cBrowsePath);
	
	// RemoveEmptyLink
	bool RemoveEmptyLink(const COpcString& cBrowsePath);

	// BuildAddressSpace
	virtual bool BuildAddressSpace();

	// ClearAddressSpace
	virtual void ClearAddressSpace();

	//=========================================================================
    // Browsing Functions

    // BrowseUp
    bool BrowseUp(
        const COpcString& cOldPath, 
        COpcString&       cNewPath
    );

    // BrowseDown
    bool BrowseDown(
        const COpcString& cOldPath, 
        const COpcString& cName, 
        COpcString&       cNewPath
    );

    // BrowseTo
    bool BrowseTo(
        const COpcString& cItemID, 
        COpcString&       cNewPath
    );

    // GetItemID
    bool GetItemID(
        const COpcString& cPath, 
        const COpcString& cName, 
        COpcString&       cItemID
    );

	// Browse
	bool Browse(
		const COpcString&        cPath, 
		OPCHDA_BROWSETYPE        eType, 
		COpcHdaBrowseFilterList& cFilters,
		COpcStringList&          cNodes
	);
	
	//=========================================================================
    // Item Access

	// returns the server handles for the specified items.
	OPCHANDLE* GetItemHandles(DWORD dwCount, LPWSTR* pItemIDs);

	// ReadRaw
	HRESULT ReadRaw(
		LONGLONG      llStartTime,
		LONGLONG      llEndTime,
		UINT          uNumValues,
		bool          bIncludeBounds,
		UINT          uStartIndex,
		OPCHANDLE     hServer,
		OPCHDA_ITEM&  cItem
	);

	// ReadProcessed
	HRESULT ReadProcessed(
		LONGLONG      llStartTime,
		LONGLONG      llEndTime,
		LONGLONG      llResampleInterval,
		DWORD         haAggregate, 
		OPCHANDLE     hServer,
		OPCHDA_ITEM&  cItem
	);

	// ReadAtTime
	HRESULT ReadAtTime(
		COpcArray<LONGLONG>& cTimestamps,
		OPCHANDLE            hServer,
		OPCHDA_ITEM&         cItem
	);
	
	// ReadModified
	HRESULT ReadModified(
		LONGLONG             llStartTime,
		LONGLONG             llEndTime,
		UINT                 uNumValues,
		OPCHANDLE            hServer,
		OPCHDA_MODIFIEDITEM& cItem
	);

	// ReadAttribute
	HRESULT ReadAttribute(
		LONGLONG          llStartTime,
		LONGLONG          llEndTime,
		OPCHANDLE         hServer,
		DWORD             dwAttributeID, 
		OPCHDA_ATTRIBUTE& cAttribute
	);

	// ReadAnnotations
	HRESULT ReadAnnotations(
		LONGLONG           llStartTime,
		LONGLONG           llEndTime,
		OPCHANDLE          hServer,
		OPCHDA_ANNOTATION& cAnnotation
	);

	// InsertAnnotations
	HRESULT InsertAnnotations(
		UINT               uCount,
		OPCHANDLE*         phServer,
		FILETIME*          pftTimestamps,
		OPCHDA_ANNOTATION* pAnnotations,
		HRESULT**          ppErrors 
	);

	// Update
	HRESULT Update(
		OPCHDA_EDITTYPE eEditType,
		UINT            uCount,
		OPCHANDLE*      phServer,
		FILETIME*       pftTimestamps,
		VARIANT*        pvValues,
		DWORD*          pdwQualities,
		HRESULT**       ppErrors 
	);

	// Delete
	HRESULT Delete(
		LONGLONG  llStartTime,
		LONGLONG  llEndTime,
		OPCHANDLE hServer
	);

    //========================================================================
    // Transactions

	// adds a transaction to the queue.
	bool QueueTransaction(COpcHdaTransaction* pTransaction, bool bCopy = false);

	// removes a transaction to the queue.
	bool CancelTransaction(DWORD dwCancelID);

	// creates a new subscription for an item.
	void CreateSubscription(COpcHdaTransaction* pTransaction);
	
	// sends the results of a transaction back to the client.
	void ReplyToClient(COpcHdaTransaction* pTransaction);

	// handles a message from the transaction queue. 
	void DispatchTransaction(MSG& cMsg);
	
	// processes a subscription and sends an update to the client.
	void SubscriptionUpdate(LONGLONG llTicks, COpcList<DWORD>& cTransactionIDs);

	// removes a transaction when the reply is sent to the client.
	bool TransactionComplete(DWORD dwID, bool bMoreData);

    //========================================================================
    // Serialization
    
    // Init
    virtual void Init();

    // Clear
    virtual void Clear();

    // Read
    virtual bool Read(COpcXmlElement& cElement);

    // Write
    virtual bool Write(COpcXmlElement& cElement);

private:

	//========================================================================
    // Private Methods

	// Read
	bool Read(const COpcString& cElementPath, COpcXmlElement& cElement);
        	
	// processes a transaction.
	void DispatchTransaction(COpcHdaTransaction& cTransaction);

	// reads data for the current interval.
	void ReadSubscription(COpcHdaReadTransaction& cSubscription);

    //==========================================================================
    // Private Members

    COpcBrowseElement*   m_pAddressSpace;
	COpcHdaItemMap       m_cItems;

	IXMLDOMElement*      m_ipSelfRegInfo;

	OPCHDA_SERVERSTATUS  m_eStatus;
	OpcVersionInfo       m_cVersionInfo;
	FILETIME             m_ftStartTime;

	COpcThreadPool          m_cThreadPool;
	COpcHdaTransactionQueue m_cTransactionQueue;
	COpcHdaSubscriptionMgr  m_cSubscriptionMgr;
	COpcHdaTransactionTable m_cTransactions;
	COpcHdaTransactionTable m_cSubscriptions;

	COpcStringList                      m_cItemList;
	COpcMap<COpcString,COpcStringList*> m_cBranches;
};

//============================================================================
// FUNCTION: GetHistorian
// PURPOSE:  Returns a reference (must never be null) to the historian database.

extern COpcHdaHistorian& GetHistorian();

#endif // _COpcHdaHistorian_H_