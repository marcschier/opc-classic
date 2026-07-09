//============================================================================
// TITLE: COpcHdaHistorian.cpp
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

#include "StdAfx.h"
#include "COpcHdaHistorian.h"
#include "COpcHdaTime.h"
#include "COpcHdaSubscriptionMgr.h"

//============================================================================
// Local Declarations

#define MAX_SAMPLING_RATE 100

#define TAG_CONFIG         _T("Config")
#define TAG_SEPARATOR      _T("/")
#define TAG_BROWSE_ELEMENT _T("BrowseElement")
#define TAG_ELEMENT_NAME   _T("ElementName")
#define TAG_ITEM           _T("Item")
#define TAG_CHILD_ELEMENTS _T("Children")

//============================================================================
// COpcHdaHistorian

// Constructor
COpcHdaHistorian::COpcHdaHistorian()
{
    m_pAddressSpace = new COpcBrowseElement(NULL);
	m_ipSelfRegInfo = NULL;
	m_eStatus       = OPCHDA_DOWN;
	m_ftStartTime   = OpcMinDate();
}

// Destructor
COpcHdaHistorian::~COpcHdaHistorian()
{
}

// Start
bool COpcHdaHistorian::Start()
{
    COpcLock cLock(*this);

	if (m_eStatus != OPCHDA_DOWN)
	{
		return false;
	}

	bool bResult = true;

	TRY
	{
		m_eStatus = OPCHDA_INDETERMINATE;

		// get the executable version information.
		if (!OpcGetModuleVersion(m_cVersionInfo))
		{
			m_cVersionInfo.cFileDescription = _T("Unknown");
			m_cVersionInfo.wMajorVersion    = 0;
			m_cVersionInfo.wMinorVersion    = 0;
			m_cVersionInfo.wBuildNumber     = 0;
			m_cVersionInfo.wRevisionNumber  = 0;
		}

		// add the revision number - if possible.
		if (m_cVersionInfo.wBuildNumber < 0x0100 && m_cVersionInfo.wRevisionNumber > 0 && m_cVersionInfo.wRevisionNumber < 100)
		{
			m_cVersionInfo.wBuildNumber *= 100;
			m_cVersionInfo.wBuildNumber += m_cVersionInfo.wRevisionNumber;
		}

		cLock.Unlock();

		// create the thread pool.
		if (!m_cThreadPool.Start())
		{
			THROW_(bResult, false);
		}

		// start the transaction queue.
		if (!m_cTransactionQueue.Start())
		{
			THROW_(bResult, false);
		}  

		// start the subscription manager.
		if (!m_cSubscriptionMgr.Start())
		{
			THROW_(bResult, false);
		}  

		cLock.Lock();

		// construct configuration file name.
		COpcString cFileName;
		
		cFileName += OpcGetModulePath();
		cFileName += _T("\\");
		cFileName += OpcGetModuleName();
		cFileName += _T(".config.xml");

		// load the configuration file.
		if (!Load(cFileName))
		{
			THROW_(bResult, false);
		}

		// add items to address space.
		if (!BuildAddressSpace())
		{
			return false;
		}

		m_eStatus     = OPCHDA_UP;
		m_ftStartTime = OpcUtcNow();
	}
	CATCH
	{
		Stop();
	}

	return bResult;
}

// Stop
void COpcHdaHistorian::Stop()
{
    COpcLock cLock(*this);

	if (m_eStatus != OPCHDA_UP)
	{
		return;
	}

	// update state.
	m_eStatus = OPCHDA_INDETERMINATE;

	// delete all outstanding transactions.
	OPC_POS pos = m_cTransactions.GetStartPosition();

	while (pos != NULL)
	{
		DWORD dwID = 0;
		COpcHdaTransaction* pTransaction = NULL;
		m_cTransactions.GetNextAssoc(pos, dwID, pTransaction);

		if (pTransaction != NULL)
		{
			delete pTransaction;
		}
	}

	// clear address space.
	ClearAddressSpace();

	cLock.Unlock();

	m_cThreadPool.Stop();
	m_cTransactionQueue.Stop();
	m_cSubscriptionMgr.Stop();

	cLock.Lock();

	m_eStatus     = OPCHDA_DOWN;
	m_ftStartTime = OpcMinDate();
}

// Load
bool COpcHdaHistorian::Load(const COpcString& cFileName)
{
    COpcLock cLock(*this);

    // load the configuration file.
    COpcXmlDocument cConfigFile;

    if (!cConfigFile.Load(cFileName))
    {
        return false;
    }

    COpcXmlElement cRoot = cConfigFile.GetRoot();

	// release existing server self registration information.
	if (m_ipSelfRegInfo != NULL)
	{
		m_ipSelfRegInfo->Release();
		m_ipSelfRegInfo = NULL;
	}

	// extract server self registration information.
	HRESULT hResult = OpcExtractSelfRegInfo((IXMLDOMElement*)cRoot, &m_ipSelfRegInfo);

	if (FAILED(hResult))
	{
		m_ipSelfRegInfo = NULL;
	}
    
	// parse the xml document.
	if (!Read(cRoot))
	{
		return false;
	}

    return true;
}

// GetHistorianStatus
HRESULT COpcHdaHistorian::GetHistorianStatus(
	OPCHDA_SERVERSTATUS* pwStatus,
	FILETIME**           pftCurrentTime,
	FILETIME**           pftStartTime,
	WORD*                pwMajorVersion,
	WORD*                pwMinorVersion,
	WORD*                pwBuildNumber,
	DWORD*               pdwMaxReturnValues,
	LPWSTR*              ppszStatusString,
	LPWSTR*              ppszVendorInfo
)
{
    COpcLock cLock(*this); 

	*pwStatus           = m_eStatus;
	*ppszStatusString   = NULL;
	*pftCurrentTime     = (FILETIME*)OpcAlloc(sizeof(FILETIME));
	**pftCurrentTime    = OpcUtcNow();  	
	*pftStartTime       = (FILETIME*)OpcAlloc(sizeof(FILETIME));
	**pftStartTime      = m_ftStartTime;  
	*ppszVendorInfo     = OpcStrDup((LPCWSTR)m_cVersionInfo.cFileDescription);
	*pwMajorVersion     = m_cVersionInfo.wMajorVersion;
	*pwMinorVersion     = m_cVersionInfo.wMinorVersion;
	*pwBuildNumber      = m_cVersionInfo.wBuildNumber;
	*pdwMaxReturnValues = OPCHDA_MAX_VALUES;

    return S_OK;
}

//==============================================================================
// Item Access

// AddLink
bool COpcHdaHistorian::AddLink(const COpcString& cBrowsePath)
{
    COpcLock cLock(*this);

    // check for a unique browse path.
    if (m_pAddressSpace->Find(cBrowsePath) != NULL)
    {
        return false;
    }

    // create new browse element.
    m_pAddressSpace->Insert(cBrowsePath);

    return true;
}

// AddLink
bool COpcHdaHistorian::AddLink(const COpcString& cBrowsePath, const COpcString& cItemID)
{
    COpcLock cLock(*this);

    // lookup up item.
    COpcHdaItem* pItem = NULL;

    if (!m_cItems.Lookup(cItemID, pItem))
    {
        return false;
    }

    // check for a unique browse path.
    if (m_pAddressSpace->Find(cBrowsePath) != NULL)
    {
        return false;
    }

    // create new browse element.
    m_pAddressSpace->Insert(cBrowsePath, cItemID);

    return true;
}

// RemoveLink
bool COpcHdaHistorian::RemoveLink(const COpcString& cBrowsePath)
{
    COpcLock cLock(*this);

    COpcBrowseElement* pChild = m_pAddressSpace->Find(cBrowsePath);

    if (pChild == NULL)
    {
        return false;
    }

    pChild->Remove();

    return true;
}

// RemoveEmptyLink
bool COpcHdaHistorian::RemoveEmptyLink(const COpcString& cBrowsePath)
{
    COpcLock cLock(*this);

    COpcBrowseElement* pChild = m_pAddressSpace->Find(cBrowsePath);

    if (pChild == NULL)
    {
        return false;
    }

    if (pChild->GetChild(0) == NULL)
    {
        pChild->Remove();
    }   

    return true;
}


//==============================================================================
// Browsing Functions

// BrowseUp
bool COpcHdaHistorian::BrowseUp(
    const COpcString& cOldPath, 
    COpcString&       cNewPath
)
{
    COpcLock cLock(*this);

    cNewPath.Empty();

    // can't browse up from root.
    if (cOldPath.IsEmpty())
    {
        return false;
    }

    // search for current node - error if missing or invalid.
    if (m_pAddressSpace != NULL)
    {
        COpcBrowseElement* pNode = m_pAddressSpace->Find(cOldPath);

        if (pNode != NULL)
        {
            // parent should never be missing if browse path is not empty.
            if (pNode->GetParent() == NULL)
            {
                return false;       
            }

            cNewPath = pNode->GetParent()->GetBrowsePath();
            return true;
        }
    }

    return false;
}

// BrowseDown
bool COpcHdaHistorian::BrowseDown(
    const COpcString& cOldPath, 
    const COpcString& cName, 
    COpcString&       cNewPath
)
{
    COpcLock cLock(*this);

    cNewPath.Empty();

    // no where to go to.
    if (cName.IsEmpty())
    {
        cNewPath = cOldPath;
        return true;
    }

    // nothing to search.
    if (m_pAddressSpace == NULL)
    {
        return false;
    }
    
    // search for current node - error if missing or invalid.
    COpcBrowseElement* pNode = m_pAddressSpace;

    if (!cOldPath.IsEmpty())
    {
        pNode = m_pAddressSpace->Find(cOldPath);

        if (pNode == NULL)
        {
            return false;
        }
    }

    // search for child - error if missing.
    COpcBrowseElement* pChild = pNode->Find(cName);

    if (pChild == NULL)
    {
        return false;
    }

    // check for a leaf node.
    if (pChild->GetChild(0) == NULL)
    {
        return false;
    }

    cNewPath = pChild->GetBrowsePath();
    return true;
}

// BrowseTo
bool COpcHdaHistorian::BrowseTo(
    const COpcString& cItemID, 
    COpcString&       cNewPath
)
{
    COpcLock cLock(*this);

    cNewPath.Empty();

    // nothing to search.
    if (m_pAddressSpace == NULL)
    {
        return false;
    }

    // move to root.
    if (cItemID.IsEmpty())
    {
        return true;
    }

    // search assuming the browse and item id are the same.
    COpcBrowseElement* pNode = m_pAddressSpace->Find(cItemID);

    if (pNode == NULL)
    {
        return false;
    }

    // check for a leaf node.
    if (pNode->GetChild(0) == NULL)
    {
        return false;
    }

    cNewPath = pNode->GetBrowsePath();
    return true;
}

// GetItemID
bool COpcHdaHistorian::GetItemID(
    const COpcString& cPath, 
    const COpcString& cName, 
    COpcString&       cItemID
)
{
    COpcLock cLock(*this);

    cItemID.Empty();

    // nothing to search.
    if (m_pAddressSpace == NULL)
    {
        return false;
    }

    COpcBrowseElement* pNode = m_pAddressSpace;

    // search for current node - error if missing or invalid.
    if (!cPath.IsEmpty())
    {
        pNode = m_pAddressSpace->Find(cPath);

        if (pNode == NULL)
        {
            return false;
        }
    }

    // return the node item id if child name not specified. 
    if (cName.IsEmpty())
    {
        cItemID = pNode->GetItemID();
        return true;
    }

    // search for child - error if missing.
    COpcBrowseElement* pChild = pNode->Find(cName);

    if (pChild == NULL)
    {
        return false;
    }

    cItemID = pChild->GetItemID();
    return true;
}

// Browse
bool COpcHdaHistorian::Browse(
    const COpcString&        cPath, 
    OPCHDA_BROWSETYPE        eType,
	COpcHdaBrowseFilterList& cFilters,
    COpcStringList&          cNodes
)
{
    COpcLock cLock(*this);

    cNodes.RemoveAll();

    // nothing to search.
    if (m_pAddressSpace == NULL)
    {
        return false;
    }

    COpcStringList cHits;
    COpcBrowseElement* pNode = NULL;

    // check if currently at root.
    if (cPath.IsEmpty())
    { 
        pNode = m_pAddressSpace;
        m_pAddressSpace->Browse(OPC_EMPTY_STRING, (eType == OPCHDA_FLAT), cHits);
    }

    // search for current node - error if missing or invalid.
    else
    {
        pNode = m_pAddressSpace->Find(cPath);

        if (pNode == NULL)
        {
            return false;
        }

        // find all names at the current level and below (if requested). 
        pNode->Browse(OPC_EMPTY_STRING, (eType == OPCHDA_FLAT), cHits);
    }

    // apply filters.
    OPC_POS pos = cHits.GetHeadPosition();

    while (pos != NULL)
    {
        COpcString cName = cHits.GetNext(pos);

        COpcBrowseElement* pChild = pNode->Find(cName);
                
		COpcHdaItem* pItem = NULL;
        bool bIsItem = m_cItems.Lookup(pChild->GetItemID(), pItem);
        bool bHasChildren = (pChild->GetChild(0) != NULL); 
        
		// apply filter based on the type of item.
		if (
			  (eType == OPCHDA_FLAT   && !bIsItem)      ||
			  (eType == OPCHDA_ITEMS  && !bIsItem)      || 
			  (eType == OPCHDA_BRANCH && !bHasChildren) || 
			  (eType == OPCHDA_LEAF   && bHasChildren)
		   )
		{
			continue;
		}

		// apply attribute filters
		if (pItem != NULL)
		{
			bool bMatch = true;

			for (UINT ii = 0; ii < cFilters.GetSize(); ii++)
			{
				// ignore invalid filters.
				if (FAILED(cFilters[ii]->GetError()))
				{
					continue;
				}

				// the item id attribute simply checks the name of the leaf in the address space.
				if (cFilters[ii]->GetID() == OPCHDA_ITEMID)
				{
					if (!OpcMatchPattern(cName, cFilters[ii]->GetValue().bstrVal, false))
					{
						bMatch = false;
						break;
					}
				}

				// check for match on any other attribute.
				else if (!pItem->Match(*(cFilters[ii])))
				{
					bMatch = false;
					break;
				}
			}

			// skip item if it did not match the filters.
			if (!bMatch)
			{
				continue;
			}
		}        

		// return entire item id when browsing a flat address space.
		if (eType == OPCHDA_FLAT)
		{
			cNodes.AddTail(pChild->GetItemID());
		}

		// only return the element name.
		else
		{
			cNodes.AddTail(cName);
		}
    }

    return true;
}

//========================================================================
// IOpcHdaDatabase

// BuildAddressSpace
bool COpcHdaHistorian::BuildAddressSpace()
{
    COpcLock cLock(*this);
	
    // items to address space.
    OPC_POS pos = m_cItemList.GetHeadPosition();

    while (pos != NULL)
    {
		COpcString cItemID = m_cItemList.GetNext(pos);

		COpcHdaItem* pItem = NULL;

		if (m_cItems.Lookup(cItemID, pItem))
		{
			if (!pItem->BuildAddressSpace())
			{
				return false;
			}
		}
    }

    return true;
}

// ClearAddressSpace
void COpcHdaHistorian::ClearAddressSpace()
{
    COpcLock cLock(*this);

	// items to address space.
    OPC_POS pos = m_cItemList.GetHeadPosition();

    while (pos != NULL)
    {
		COpcString cItemID = m_cItemList.GetNext(pos);

		COpcHdaItem* pItem = NULL;

		if (m_cItems.Lookup(cItemID, pItem))
		{
			pItem->ClearAddressSpace();
		}
    }
}

//========================================================================
// IOpcXmlSerialize

// Init
void COpcHdaHistorian::Init()
{
    m_cItems.RemoveAll();
	m_cItemList.RemoveAll();
    m_cBranches.RemoveAll();
}

// Clear
void COpcHdaHistorian::Clear()
{
    OPC_POS pos = NULL;

    // clear all items.
    pos = m_cItems.GetStartPosition();

    while (pos != NULL)
    {
        COpcString cItemID;
        COpcHdaItem* pItem = NULL;
        m_cItems.GetNextAssoc(pos, cItemID, pItem);

        delete pItem;
    }

    m_cItems.RemoveAll();
	m_cItemList.RemoveAll();

    // clear all branches.
    pos = m_cBranches.GetStartPosition();

    while (pos != NULL)
    {
        COpcString cItemID;
        COpcStringList* pBranch = NULL;
        m_cBranches.GetNextAssoc(pos, cItemID, pBranch);

        delete pBranch;
    }

    m_cBranches.RemoveAll();

    Init();
}

// Read
bool COpcHdaHistorian::Read(COpcXmlElement& cElement)
{
    COpcLock cLock(*this);

    // read top level items.
    COpcXmlElementList cItems;

    UINT uCount = cElement.GetChildren(cItems);

    for (UINT ii = 0; ii < uCount; ii++)
    {
		Read((LPTSTR)NULL, cItems[ii]);
    }

	return true;
}

// Read
bool COpcHdaHistorian::Read(const COpcString& cElementPath, COpcXmlElement& cElement)
{
    // read element name.
    COpcString cElementName;

	READ_ATTRIBUTE(TAG_ELEMENT_NAME, cElementName);

    if (cElementName.IsEmpty())
    {
        return false;
    }

    // index element by path
	COpcStringList* pBranch = NULL;

	if (!m_cBranches.Lookup(cElementPath, pBranch))
	{
		m_cBranches[cElementPath] = pBranch = new COpcStringList();
	}

	pBranch->AddTail(cElementName);

	// build item id.
	COpcString cItemID;

	cItemID += cElementPath;
    cItemID += (cElementPath.IsEmpty())?_T(""):TAG_SEPARATOR;
	cItemID += cElementName;

	// check if current element is an item.
	COpcXmlElement cItem = cElement.GetChild(TAG_ITEM);

	if (cItem != NULL)
	{
        // check for duplicate item ids.
        if (m_cItems.Lookup(cItemID))
        {
			return false;
        }

		COpcHdaItem* pItem = new COpcHdaItem(cItemID);

		// initialize item.
		if (!pItem->Read(cItem))
		{
			delete pItem;
			return false;
		}

        // index item by item id.
        m_cItems[cItemID] = pItem;
		m_cItemList.AddTail(cItemID);
	}

    // read child elements.
	COpcXmlElement cChildren = cElement.GetChild(TAG_CHILD_ELEMENTS);

	if (cChildren != NULL)
	{
		COpcXmlElementList cItems;

		UINT uCount = cChildren.GetChildren(cItems);

		for (UINT ii = 0; ii < uCount; ii++)
		{
			Read(cItemID, cItems[ii]);
		}
	}

    return true;
}

// Write
bool COpcHdaHistorian::Write(COpcXmlElement& cElement)
{
	OPC_ASSERT(false);  
	
	// not implemented.
	
	return false;
}

//=========================================================================
// Item Access

// GetItemHandles
OPCHANDLE* COpcHdaHistorian::GetItemHandles(DWORD dwCount, LPWSTR* pItemIDs)
{
    COpcLock cLock(*this);

	// handle trivial case.
	if (dwCount == 0)
	{
		return NULL;
	}

	// allocate array (zero handle means the item does not exist).
	OPCHANDLE* phHandles = OpcArrayAlloc(OPCHANDLE, dwCount);
	memset(phHandles, 0, sizeof(OPCHANDLE)*dwCount);

	for (DWORD ii = 0; ii < dwCount; ii++)
	{
		// lookup item id.
		COpcHdaItem* pItem = NULL;

		if (m_cItems.Lookup(pItemIDs[ii], pItem))
		{
			phHandles[ii] = (OPCHANDLE)pItem;
		}
	}

	// return handle array.
	return phHandles;
}

// ReadRaw
HRESULT COpcHdaHistorian::ReadRaw(
	LONGLONG      llStartTime,
	LONGLONG      llEndTime,
	UINT          uNumValues,
	bool          bIncludeBounds,
	UINT          uStartIndex,
	OPCHANDLE     hServer,
	OPCHDA_ITEM&  cItem
)
{
    COpcLock cLock(*this);

	// lookup item from server handle.
	COpcHdaItem* pItem = (COpcHdaItem*)hServer;

	if (pItem == NULL)
	{
		return OPC_E_INVALIDHANDLE;
	}

	UINT uValuesSent = 0;

	// read data from item.
	HRESULT hResult = pItem->ReadRaw(
		llStartTime,
		llEndTime,
		uNumValues,
		bIncludeBounds,
		uStartIndex,
		cItem
	);

	return hResult;
}

// ReadProcessed
HRESULT COpcHdaHistorian::ReadProcessed(
	LONGLONG      llStartTime,
	LONGLONG      llEndTime,
	LONGLONG      llResampleInterval,
	DWORD         haAggregate, 
	OPCHANDLE     hServer,
	OPCHDA_ITEM&  cItem
)
{
    COpcLock cLock(*this);

	// lookup item from server handle.
	COpcHdaItem* pItem = (COpcHdaItem*)hServer;

	if (pItem == NULL)
	{
		return OPC_E_INVALIDHANDLE;
	}

	// read data from item.
	HRESULT hResult = pItem->ReadProcessed(
		llStartTime,
		llEndTime,
		llResampleInterval,
		haAggregate,
		cItem
	);

	return hResult;
}

// ReadAtTime
HRESULT COpcHdaHistorian::ReadAtTime(
	COpcArray<LONGLONG>& cTimestamps,
	OPCHANDLE            hServer,
	OPCHDA_ITEM&         cItem
)
{
    COpcLock cLock(*this);

	// lookup item from server handle.
	COpcHdaItem* pItem = (COpcHdaItem*)hServer;

	if (pItem == NULL)
	{
		return OPC_E_INVALIDHANDLE;
	}

	// read data from item.
	return pItem->ReadAtTime(cTimestamps, cItem);
}

// ReadModified
HRESULT COpcHdaHistorian::ReadModified(
	LONGLONG             llStartTime,
	LONGLONG             llEndTime,
	UINT                 uNumValues,
	OPCHANDLE            hServer,
	OPCHDA_MODIFIEDITEM& cItem
)
{
    COpcLock cLock(*this);

	// lookup item from server handle.
	COpcHdaItem* pItem = (COpcHdaItem*)hServer;

	if (pItem == NULL)
	{
		return OPC_E_INVALIDHANDLE;
	}

	// read data from item.
	HRESULT hResult = pItem->ReadModified(
		llStartTime,
		llEndTime,
		uNumValues,
		0,
		cItem
	);

	return hResult;
}

// ReadAttribute
HRESULT COpcHdaHistorian::ReadAttribute(
	LONGLONG          llStartTime,
	LONGLONG          llEndTime,
	OPCHANDLE         hServer,
	DWORD             dwAttributeID, 
	OPCHDA_ATTRIBUTE& cAttribute
)
{
    COpcLock cLock(*this);

	// lookup item from server handle.
	COpcHdaItem* pItem = (COpcHdaItem*)hServer;

	if (pItem == NULL)
	{
		return OPC_E_INVALIDHANDLE;
	}

	// read data from item.
	HRESULT hResult = pItem->ReadAttribute(
		llStartTime,
		llEndTime,
		dwAttributeID,
		cAttribute
	);

	return hResult;
}

// ReadAnnotations
HRESULT COpcHdaHistorian::ReadAnnotations(
	LONGLONG           llStartTime,
	LONGLONG           llEndTime,
	OPCHANDLE          hServer,
	OPCHDA_ANNOTATION& cAnnotation
)
{
    COpcLock cLock(*this);

	// lookup item from server handle.
	COpcHdaItem* pItem = (COpcHdaItem*)hServer;

	if (pItem == NULL)
	{
		return OPC_E_INVALIDHANDLE;
	}

	// read data from item.
	HRESULT hResult = pItem->ReadAnnotations(
		llStartTime,
		llEndTime,
		cAnnotation
	);

	return hResult;
}

// InsertAnnotations
HRESULT COpcHdaHistorian::InsertAnnotations(
	UINT               uCount,
	OPCHANDLE*         phServer,
	FILETIME*          pftTimestamps,
	OPCHDA_ANNOTATION* pAnnotations,
	HRESULT**          ppErrors 
)
{
    COpcLock cLock(*this);

	// allocate array.
	*ppErrors = OpcArrayAlloc(HRESULT, uCount);
	memset(*ppErrors, 0, sizeof(HRESULT)*uCount);

	// keep track of items that have changed.
	COpcMap<OPCHANDLE,COpcHdaItem*> cItems;

	bool bError = false;

	for (UINT ii = 0; ii < uCount; ii++)
	{
		// lookup item from server handle.
		COpcHdaItem* pItem = (COpcHdaItem*)phServer[ii];

		// item does not exist,
		if (pItem == NULL)
		{
			(*ppErrors)[ii] = OPC_E_INVALIDHANDLE;
			bError = true;
			continue;
		}

		// update the item.
		else
		{
			// remember item to save changes to disk later.
			cItems[phServer[ii]] = pItem;

			(*ppErrors)[ii] = pItem->InsertAnnotation(
				OpcHdaInt64FromFILETIME(pftTimestamps[ii]),
				pAnnotations[ii]
			);
		}

		// check for error.
		if (FAILED((*ppErrors)[ii]))
		{
			bError = true;
		}
	}

	// save changes to disk.
	OPC_POS pos = cItems.GetStartPosition();

	while (pos != NULL)
	{
		OPCHANDLE hServer = NULL;
		COpcHdaItem* pItem = NULL;
		cItems.GetNextAssoc(pos, hServer, pItem);

		pItem->SaveData();
	}

	// all done.
	return (bError)?S_FALSE:S_OK;
}

// Update
HRESULT COpcHdaHistorian::Update(
	OPCHDA_EDITTYPE eEditType,
	UINT            uCount,
	OPCHANDLE*      phServer,
	FILETIME*       pftTimestamps,
	VARIANT*        pvValues,
	DWORD*          pdwQualities,
	HRESULT**       ppErrors 
)
{
    COpcLock cLock(*this);

	// allocate array.
	*ppErrors = OpcArrayAlloc(HRESULT, uCount);
	memset(*ppErrors, 0, sizeof(HRESULT)*uCount);

	// keep track of items that have changed.
	COpcMap<OPCHANDLE,COpcHdaItem*> cItems;

	bool bError = false;

	for (UINT ii = 0; ii < uCount; ii++)
	{
		// lookup item from server handle.
		COpcHdaItem* pItem = (COpcHdaItem*)phServer[ii];

		// item does not exist,
		if (pItem == NULL)
		{
			(*ppErrors)[ii] = OPC_E_INVALIDHANDLE;
			bError = true;
			continue;
		}

		// update the item.
		else
		{
			// remember item to save changes to disk later.
			cItems[phServer[ii]] = pItem;

			if (eEditType != OPCHDA_DELETE)
			{
				(*ppErrors)[ii] = pItem->Update(
					eEditType,
					OpcHdaInt64FromFILETIME(pftTimestamps[ii]),
					pvValues[ii],
					pdwQualities[ii]
				);
			}
			else
			{
				VARIANT vEmpty;
				VariantInit(&vEmpty);

				(*ppErrors)[ii] = pItem->Update(
					eEditType,
					OpcHdaInt64FromFILETIME(pftTimestamps[ii]),
					vEmpty,
					0
				);
			}
		}

		// check for error.
		if (FAILED((*ppErrors)[ii]))
		{
			bError = true;
		}
	}

	// save changes to disk.
	OPC_POS pos = cItems.GetStartPosition();

	while (pos != NULL)
	{
		OPCHANDLE hServer = NULL;
		COpcHdaItem* pItem = NULL;
		cItems.GetNextAssoc(pos, hServer, pItem);

		pItem->SaveData();
	}

	// all done.
	return (bError)?S_FALSE:S_OK;
}

// Delete
HRESULT COpcHdaHistorian::Delete(
	LONGLONG  llStartTime,
	LONGLONG  llEndTime,
	OPCHANDLE hServer
)
{
    COpcLock cLock(*this);

	// lookup item from server handle.
	COpcHdaItem* pItem = (COpcHdaItem*)hServer;

	if (pItem == NULL)
	{
		return OPC_E_INVALIDHANDLE;
	}

	// read data from item.
	HRESULT hResult = pItem->Delete(
		llStartTime,
		llEndTime
	);

	return hResult;
}

//==============================================================================
// Transactions

// QueueTransaction
bool COpcHdaHistorian::QueueTransaction(COpcHdaTransaction* pTransaction, bool bCopy)
{
    COpcLock cLock(*this);

	// check if server is running.
	if (m_eStatus != OPCHDA_UP || pTransaction == NULL)
	{
		return false;
	}

	// check if this a continuation of a single transaction.
	if (bCopy)
	{		
		// check if transaction was cancelled.
		COpcHdaTransaction* pExisting = NULL;

		if (!m_cTransactions.Lookup(pTransaction->GetID(), pExisting))
		{
			return false;
		}

		// should never happen.
		if (pExisting != NULL)
		{
			OPC_ASSERT(false);
			return false;
		}
	}

	// must be a new transaction.
	else
	{
		// check if transaction already exists (should never happen).
		if (m_cTransactions.Lookup(pTransaction->GetID()))
		{
			OPC_ASSERT(false);
			return false;
		}
	}

	// save transaction.
	m_cTransactions[pTransaction->GetID()] = pTransaction;

	// add transaction to queue.
	m_cTransactionQueue.QueueTransaction(*pTransaction);

	// add done.
	return true;
}
 
// CancelTransaction
bool COpcHdaHistorian::CancelTransaction(DWORD dwCancelID)
{
    COpcLock cLock(*this);

	// check if server is running.
	if (m_eStatus != OPCHDA_UP)
	{
		return false;
	}

	bool bFound = false;
	
	// check for subscription.
	COpcHdaTransaction* pTransaction = NULL;

	if (m_cSubscriptions.Lookup(dwCancelID, pTransaction))
	{
		m_cSubscriptionMgr.CancelSubscription(dwCancelID);
		m_cSubscriptions.RemoveKey(dwCancelID);

		delete pTransaction;
		pTransaction = NULL;

		bFound = true;
	}

	// lookup transaction (including pending replies for subscriptions).
	if (m_cTransactions.Lookup(dwCancelID, pTransaction))
	{
		m_cTransactions.RemoveKey(dwCancelID);

		// will be deleted when the thread pool processes the reply message.
		if (pTransaction != NULL)
		{
			delete pTransaction;
		}

		bFound = true;
	}

	// return whether transaction was found.
	return bFound;
}

// TransactionComplete
bool COpcHdaHistorian::TransactionComplete(DWORD dwID, bool bMoreData)
{
	COpcLock cLock(*this);

	// check if server is running.
	if (m_eStatus != OPCHDA_UP)
	{
		return false;
	}

	// verify that transaction still exists (i.e. was not cancelled).
	if (!m_cTransactions.Lookup(dwID))
	{
		return false;
	}

	// remove key if no more data.
	if (!bMoreData)
	{
		m_cTransactions.RemoveKey(dwID);
	}

	// transaction found.
	return true;
}

// DispatchTransaction
void COpcHdaHistorian::DispatchTransaction(MSG& cMsg)
{
    COpcLock cLock(*this);

	// lookup the transaction.
	COpcHdaTransaction* pTransaction = NULL;

	if (!m_cTransactions.Lookup(cMsg.wParam, pTransaction))
	{
		return;
	}

	// should never happen - transactions set to NULL should not be in the queue.
	if (pTransaction == NULL)
	{
		OPC_ASSERT(false);
		return;
	}

	// dispatch transaction.
	switch (pTransaction->GetType())
	{
		// create a new subscription.
		case OPC_TRANSACTION_ADVISE_RAW:
		case OPC_TRANSACTION_ADVISE_PROCESSED:
		case OPC_TRANSACTION_PLAYBACK_RAW:
		case OPC_TRANSACTION_PLAYBACK_PROCESSED:
		{
			// move transaction to subscriptions table.
			m_cTransactions[pTransaction->GetID()] = NULL;
			m_cSubscriptions[pTransaction->GetID()] = pTransaction;

			// register subscription with subscription manager.
			m_cSubscriptionMgr.CreateSubscription(*pTransaction);
			break;
		}
		
		// handle simple transactions.
		case OPC_TRANSACTION_READ_RAW:
		case OPC_TRANSACTION_READ_PROCESSED:
		case OPC_TRANSACTION_READ_AT_TIME:
		case OPC_TRANSACTION_READ_MODIFIED:
		case OPC_TRANSACTION_INSERT:
		case OPC_TRANSACTION_REPLACE:
		case OPC_TRANSACTION_INSERT_REPLACE:
		case OPC_TRANSACTION_DELETE:
		case OPC_TRANSACTION_DELETE_AT_TIME:
		case OPC_TRANSACTION_READ_ANNOTATION:
		case OPC_TRANSACTION_INSERT_ANNOTATION:
		{
			DispatchTransaction(*pTransaction);
			break;
		}

		// handle attribute read.
		case OPC_TRANSACTION_READ_ATTRIBUTE:
		{
			COpcHdaAttributeTransaction& cLocal = (COpcHdaAttributeTransaction&)*pTransaction;

			// lookup item from server handle.
			COpcHdaItem* pItem = (COpcHdaItem*)cLocal.cServerHandles[0];

			if (pItem == NULL)
			{
				break;
			}

			bool bError = false;

			// read attributes.
			for (UINT ii = 0; ii < cLocal.cAtributeIDs.GetSize(); ii++)
			{
				cLocal.cErrors[ii] = pItem->ReadAttribute(
					cLocal.llStartTime,
					cLocal.llEndTime,
					cLocal.cAtributeIDs[ii],
					cLocal.cAttributes[ii]
				);

				if (FAILED(cLocal.cErrors[ii]))
				{
					bError = true;
				}

				// save client handle.
				cLocal.cAttributes[ii].hClient = cLocal.cClientHandles[0];
			}

			// set transaction result.
			cLocal.hResult = (bError)?S_FALSE:S_OK;

			// pass ownership of the transaction to the thread pool which will reply to the client.
			m_cTransactions[cLocal.GetID()] = NULL;
			m_cThreadPool.QueueMessage(pTransaction);
			break;
		}

		// unsupported transaction type.
		default:
		{
			delete pTransaction;
			break;
		}
	}
}

// DispatchTransaction
void COpcHdaHistorian::DispatchTransaction(COpcHdaTransaction& cTransaction)
{
    COpcLock cLock(*this);

	bool bError = false;

	for (UINT ii = 0; ii < cTransaction.GetNoOfItems(); ii++)
	{
		// lookup item from server handle.
		COpcHdaItem* pItem = (COpcHdaItem*)cTransaction.cServerHandles[ii];

		if (pItem == NULL)
		{
			cTransaction.cErrors[ii] = OPC_E_INVALIDHANDLE;
			bError = true;
			continue;
		}

		// process request.
		switch (cTransaction.GetType())
		{
			// read raw values.
			case OPC_TRANSACTION_READ_RAW:
			{
				COpcHdaReadTransaction& cLocal = (COpcHdaReadTransaction&)cTransaction;

				cLocal.cErrors[ii] = pItem->ReadRaw(
					cLocal.llStartTime,
					cLocal.llEndTime,
					cLocal.uNumValues,
					cLocal.bIncludeBounds,
					cLocal.uValuesSent,
					cLocal.cItems[ii]
				);

				// save item client handle.
				cLocal.cItems[ii].hClient = cTransaction.cClientHandles[ii];
				break;
			}

			// read processed values.
			case OPC_TRANSACTION_READ_PROCESSED:
			{
				COpcHdaReadTransaction& cLocal = (COpcHdaReadTransaction&)cTransaction;

				cLocal.cErrors[ii] = pItem->ReadProcessed(
					cLocal.llStartTime,
					cLocal.llEndTime,
					cLocal.llResampleInterval,
					cLocal.cAggregates[ii],
					cLocal.cItems[ii]
				);

				// save item client handle.
				cLocal.cItems[ii].hClient = cTransaction.cClientHandles[ii];
				break;
			}
			
			// read modified values.
			case OPC_TRANSACTION_READ_MODIFIED:
			{
				COpcHdaModifiedTransaction& cLocal = (COpcHdaModifiedTransaction&)cTransaction;

				cLocal.cErrors[ii] = pItem->ReadModified(
					cLocal.llStartTime,
					cLocal.llEndTime,
					cLocal.uNumValues,
					cLocal.uValuesSent,
					cLocal.cItems[ii]
				);

				// save item client handle.
				cLocal.cItems[ii].hClient = cTransaction.cClientHandles[ii];
				break;
			}

			// read values at time.
			case OPC_TRANSACTION_READ_AT_TIME:
			{
				COpcHdaReadTransaction& cLocal = (COpcHdaReadTransaction&)cTransaction;

				cLocal.cErrors[ii] = pItem->ReadAtTime(
					cLocal.cTimestamps,
					cLocal.cItems[ii]
				);

				// save item client handle.
				cLocal.cItems[ii].hClient = cTransaction.cClientHandles[ii];
				break;
			}

			// read annotations.
			case OPC_TRANSACTION_READ_ANNOTATION:
			{
				COpcHdaAnnotationTransaction& cLocal = (COpcHdaAnnotationTransaction&)cTransaction;

				cLocal.cErrors[ii] = pItem->ReadAnnotations(
					cLocal.llStartTime,
					cLocal.llEndTime,
					cLocal.cAnnotations[ii]
				);

				// save item client handle.
				cLocal.cAnnotations[ii].hClient = cTransaction.cClientHandles[ii];
				break;
			}

			// insert annotations.
			case OPC_TRANSACTION_INSERT_ANNOTATION:
			{
				COpcHdaAnnotationTransaction& cLocal = (COpcHdaAnnotationTransaction&)cTransaction;

				cLocal.cErrors[ii] = pItem->InsertAnnotation(
					cLocal.cTimestamps[ii],
					cLocal.cAnnotations[ii]
				);

				break;
			}

			// insert values.
			case OPC_TRANSACTION_INSERT:
			{
				COpcHdaUpdateTransaction& cLocal = (COpcHdaUpdateTransaction&)cTransaction;

				cLocal.cErrors[ii] = pItem->Update(
					OPCHDA_INSERT,
					cLocal.cTimestamps[ii],
					cLocal.cValues[ii],
					cLocal.cQualities[ii]
				);

				break;
			}

			// replace values.
			case OPC_TRANSACTION_REPLACE:
			{
				COpcHdaUpdateTransaction& cLocal = (COpcHdaUpdateTransaction&)cTransaction;

				cLocal.cErrors[ii] = pItem->Update(
					OPCHDA_REPLACE,
					cLocal.cTimestamps[ii],
					cLocal.cValues[ii],
					cLocal.cQualities[ii]
				);

				break;
			}

			// replace values.
			case OPC_TRANSACTION_INSERT_REPLACE:
			{
				COpcHdaUpdateTransaction& cLocal = (COpcHdaUpdateTransaction&)cTransaction;

				cLocal.cErrors[ii] = pItem->Update(
					OPCHDA_INSERTREPLACE,
					cLocal.cTimestamps[ii],
					cLocal.cValues[ii],
					cLocal.cQualities[ii]
				);

				break;
			}

			// delete values.
			case OPC_TRANSACTION_DELETE:
			{
				COpcHdaUpdateTransaction& cLocal = (COpcHdaUpdateTransaction&)cTransaction;

				cLocal.cErrors[ii] = pItem->Delete(
					cLocal.llStartTime,
					cLocal.llEndTime
				);

				break;
			}

			// delete values.
			case OPC_TRANSACTION_DELETE_AT_TIME:
			{
				COpcHdaUpdateTransaction& cLocal = (COpcHdaUpdateTransaction&)cTransaction;

				VARIANT vtEmptyValue;
				VariantInit(&vtEmptyValue);

				cLocal.cErrors[ii] = pItem->Update(
					OPCHDA_DELETE,
					cLocal.cTimestamps[ii],
					vtEmptyValue,
					0
				);

				break;
			}
		}

		// check for item level error.
		if (FAILED(cTransaction.cErrors[ii]))
		{
			bError = true;
		}

	}

	// set transaction result.
	cTransaction.hResult = (bError)?S_FALSE:S_OK;

	// pass ownership of the transaction to the thread pool which will reply to the client.
	m_cTransactions[cTransaction.GetID()] = NULL;
	m_cThreadPool.QueueMessage(&cTransaction);
}

// SubscriptionUpdate
void COpcHdaHistorian::SubscriptionUpdate(LONGLONG llTicks, COpcList<DWORD>& cTransactionIDs)
{
    COpcLock cLock(*this);

	while (cTransactionIDs.GetCount() > 0)
	{
		// lookup the transaction.
		COpcHdaTransaction* pSubscription = NULL;

		if (m_cSubscriptions.Lookup(cTransactionIDs.RemoveHead(), pSubscription))
		{
			// should never happen - check just in case.
			if (pSubscription == NULL)
			{
				OPC_ASSERT(false);
				continue;
			}

			COpcHdaReadTransaction& cSubscription = (COpcHdaReadTransaction&)*pSubscription;

			LONGLONG llNow = OpcHdaInt64FromFILETIME(OpcUtcNow());

			// check if it is time for an update.
			if (llNow < cSubscription.llLastUpdate + cSubscription.llUpdateInterval)
			{
				continue;
			}

			// do not process if the end time is in the future.
			if (cSubscription.GetType() != OPC_TRANSACTION_PLAYBACK_RAW && cSubscription.GetType() != OPC_TRANSACTION_PLAYBACK_PROCESSED)
			{		
				if (llNow <= cSubscription.llEndTime)
				{
					continue;
				}
			}

			// read the data the current interval.
			ReadSubscription(cSubscription);

			// check if a playback request has run out of data.
			bool bAllDone = false;

			switch (cSubscription.GetType())
			{
				case OPC_TRANSACTION_PLAYBACK_RAW:
				case OPC_TRANSACTION_PLAYBACK_PROCESSED:
				{					
					bAllDone = true;

					for (UINT ii = 0; ii < cSubscription.GetNoOfItems(); ii++)
					{
						if (cSubscription.cItems[ii].dwCount > 0)
						{
							bAllDone = false;
							break;
						}
					}
					
					break;
				}
			}

			// remove a playback subscription since there is no more data.
			if (bAllDone)
			{
				m_cSubscriptionMgr.CancelSubscription(pSubscription->GetID());
				m_cSubscriptions.RemoveKey(pSubscription->GetID());
			}

			// create a copy of the subscription to use for the next update cycle.
			else
			{
				COpcHdaReadTransaction* pCopy = new COpcHdaReadTransaction(cSubscription);

				pCopy->llStartTime  = cSubscription.llEndTime;
				pCopy->llLastUpdate = llNow; 
				
				pCopy->SetNoOfItems(cSubscription.GetNoOfItems());

				for (UINT ii = 0; ii < cSubscription.GetNoOfItems(); ii++)
				{
					pCopy->cServerHandles[ii] = cSubscription.cServerHandles[ii];
					pCopy->cClientHandles[ii] = cSubscription.cClientHandles[ii];
					pCopy->cAggregates[ii]    = cSubscription.cAggregates[ii];
				}

				switch (cSubscription.GetType())
				{
					case OPC_TRANSACTION_ADVISE_RAW:
					case OPC_TRANSACTION_ADVISE_PROCESSED:
					{					
						pCopy->llEndTime = cSubscription.llEndTime + cSubscription.llUpdateInterval;
						break;
					}

					case OPC_TRANSACTION_PLAYBACK_RAW:
					{		
						pCopy->llEndTime = cSubscription.llEndTime + cSubscription.llUpdateDuration;

						for (DWORD ii = 0; ii < cSubscription.GetNoOfItems(); ii++)
						{
							if (cSubscription.cItems[ii].dwCount > 0)
							{
								pCopy->cLastTimestamps[ii] = OpcHdaInt64FromFILETIME(cSubscription.cItems[ii].pftTimeStamps[cSubscription.cItems[ii].dwCount-1]);
							}
							else
							{
								pCopy->cLastTimestamps[ii] = pCopy->llStartTime;
							}
						}

						break;				
					}

					case OPC_TRANSACTION_PLAYBACK_PROCESSED:
					{		
						pCopy->llEndTime = cSubscription.llEndTime + cSubscription.llUpdateDuration;
						break;				
					}
				}

				m_cSubscriptions[pCopy->GetID()] = pCopy;
			}
			
			// pass ownership of the current to the thread pool which will reply to the client.
			m_cTransactions[pSubscription->GetID()] = NULL;
			m_cThreadPool.QueueMessage(pSubscription);
		}
	}
}

// ReadSubscription
void COpcHdaHistorian::ReadSubscription(COpcHdaReadTransaction& cSubscription)
{
    COpcLock cLock(*this);

	bool bError = false;

	for (UINT ii = 0; ii < cSubscription.GetNoOfItems(); ii++)
	{
		// lookup item from server handle.
		COpcHdaItem* pItem = (COpcHdaItem*)cSubscription.cServerHandles[ii];

		if (pItem == NULL)
		{
			cSubscription.cErrors[ii] = OPC_E_INVALIDHANDLE;
			bError = true;
			continue;
		}

		// process request.
		switch (cSubscription.GetType())
		{
			// read raw values.
			case OPC_TRANSACTION_ADVISE_RAW:
			{
				cSubscription.cErrors[ii] = pItem->ReadRaw(
					cSubscription.llStartTime,
					cSubscription.llEndTime,
					cSubscription.uNumValues,
					false,
					0,
					cSubscription.cItems[ii]
				);

				break;
			}

			// playback raw values.
			case OPC_TRANSACTION_PLAYBACK_RAW:
			{
				cSubscription.cErrors[ii] = pItem->ReadRaw(
					cSubscription.cLastTimestamps[ii],
					cSubscription.llEndTime,
					cSubscription.uNumValues,
					false,
					0,
					cSubscription.cItems[ii]
				);

				// supress more data signals for playback.
				if (cSubscription.cErrors[ii] == OPC_S_MOREDATA)
				{
					cSubscription.cErrors[ii] = S_OK;
				}

				break;
			}

			// read processed values.
			case OPC_TRANSACTION_ADVISE_PROCESSED:
			case OPC_TRANSACTION_PLAYBACK_PROCESSED:
			{				
				cSubscription.cErrors[ii] = pItem->ReadProcessed(
					cSubscription.llStartTime,
					cSubscription.llEndTime,
					cSubscription.llResampleInterval,
					cSubscription.cAggregates[ii],
					cSubscription.cItems[ii]
				);

				break;
			}
		}

		// check for item level error.
		if (FAILED(cSubscription.cErrors[ii]))
		{
			bError = true;
		}

		// save item client handle.
		cSubscription.cItems[ii].hClient = cSubscription.cClientHandles[ii];
	}

	// set transaction result.
	cSubscription.hResult = (bError)?S_FALSE:S_OK;	
}