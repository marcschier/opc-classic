//============================================================================
// TITLE: COpcHdaBrowser.cpp
//
// CONTENTS:
// 
// Implements an OPC Historical Data Access browser object.
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
#include "COpcHdaBrowser.h"
#include "COpcHdaHistorian.h"

//============================================================================
// COpcHdaBrowser

// Constructor
COpcHdaBrowser::COpcHdaBrowser()
{
}

// Constructor
COpcHdaBrowser::COpcHdaBrowser(COpcHdaBrowseFilterList& cFilters)
{
	// takes ownership of the individual filter objects.
	m_cFilters = cFilters;
}

// Destructor
COpcHdaBrowser::~COpcHdaBrowser()
{
	// free memory allocated for the filter objects.
	for (UINT ii = 0; ii < m_cFilters.GetSize(); ii++)
	{
		delete m_cFilters[ii];
	}

	m_cFilters.RemoveAll();
}

//=========================================================================
// IOPCHDA_Browser

// GetEnum
HRESULT COpcHdaBrowser::GetEnum(
	OPCHDA_BROWSETYPE dwBrowseType,
	LPENUMSTRING*     ppIEnumString
)
{
	COpcLock cLock(*this);

	// validate arguments.
	if (ppIEnumString == NULL)
	{
		return E_INVALIDARG;
	}

	// validate browse type.
	switch (dwBrowseType)
	{
		case OPCHDA_FLAT:
		case OPCHDA_BRANCH:
		case OPCHDA_LEAF:
		case OPCHDA_ITEMS:
		{
			break;
		}

		default:
		{
			return E_INVALIDARG;
		}
	}

	// fetch the set of nodes meeting the filter criteria at the current position.
	COpcStringList cNodes;

	bool bResult = GetHistorian().Browse(
		m_cBrowsePath,
		dwBrowseType, 
		m_cFilters,
		cNodes
	);
	
    if (!bResult)
    {
        return E_FAIL;
    }

    // allocate enumerator.
    COpcEnumString* pEnum = NULL;

    UINT uCount = cNodes.GetCount();
    
    if (uCount == 0)
    {
        pEnum = new COpcEnumString();
    }
    else
    {
        LPWSTR* pNodes = OpcArrayAlloc(LPWSTR, uCount);
        memset(pNodes, 0, uCount*sizeof(LPWSTR));

        OPC_POS pos = cNodes.GetHeadPosition();

        for (UINT ii = 0; pos != NULL; ii++)
        {
            COpcString cElement = cNodes.GetNext(pos);
            pNodes[ii] = OpcStrDup((LPCWSTR)cElement);
        }

        pEnum = new COpcEnumString(uCount, pNodes);
    }

    // query for the desired interface.
    HRESULT hResult = pEnum->QueryInterface(IID_IEnumString, (void**)ppIEnumString);
    
    pEnum->Release();

    if (FAILED(hResult))
    {
        return hResult;
    }

    return (uCount > 0)?S_OK:S_FALSE;
}

// ChangeBrowsePosition
HRESULT COpcHdaBrowser::ChangeBrowsePosition(
	OPCHDA_BROWSEDIRECTION dwBrowseDirection,
	LPCWSTR                szString
)
{
	COpcLock cLock(*this);

    switch (dwBrowseDirection)
    {
        case OPCHDA_BROWSE_UP:
        {
            // not allowed to browse up from root.
            if (m_cBrowsePath.IsEmpty())
            {
                return E_FAIL;
            }

            // get new browse position.
            COpcString cBrowsePath;

            if (!::GetHistorian().BrowseUp(m_cBrowsePath, cBrowsePath))
            {
				// reset position to root on error (i.e. bad browse position).
				m_cBrowsePath.Empty();
                return E_FAIL;
            }
           
            m_cBrowsePath = cBrowsePath;
            return S_OK;
        }

        case OPCHDA_BROWSE_DOWN:
        {
            // nothing to do - ignore.
            if (szString == NULL || wcslen(szString) == 0)
            {
                return E_INVALIDARG;
            }

            // get new browse position.
            COpcString cBrowsePath;

            if (!::GetHistorian().BrowseDown(m_cBrowsePath, szString, cBrowsePath))
            {
                return E_INVALIDARG;
            }

            m_cBrowsePath = cBrowsePath;
            return S_OK;
        }

        case OPCHDA_BROWSE_DIRECT:
        {
            // check for browse to root.
            if (szString == NULL || wcslen(szString) == 0)
            {
                m_cBrowsePath = OPC_EMPTY_STRING;
                return S_OK;
            }

            // get new browse position.
            COpcString cBrowsePath;

            if (!::GetHistorian().BrowseTo(szString, cBrowsePath))
            {
                return E_INVALIDARG;
            }

            m_cBrowsePath = cBrowsePath;
            return S_OK;
        }
    }

    return E_INVALIDARG;
}

// GetItemID
HRESULT COpcHdaBrowser::GetItemID(
	LPCWSTR szNode,
	LPWSTR* pszItemID
)
{
	COpcLock cLock(*this);
	
	// check for invalid arguments
    if (pszItemID == NULL || wcslen(szNode) == 0)
    {
        return E_FAIL;
    }
        
    *pszItemID = NULL;

    // handle request for root item id.
    if (m_cBrowsePath.IsEmpty())
    {
        if (szNode == NULL || wcslen(szNode) == 0)
        {
            *pszItemID = OpcStrDup(L"");
            return S_OK;
        }
    }

    // lookup item id.
    COpcString cItemID;

    if (!::GetHistorian().GetItemID(m_cBrowsePath, szNode, cItemID))
    {
        return E_INVALIDARG;
    }

    // copy item id.
    *pszItemID = OpcStrDup((LPCWSTR)cItemID);

    return S_OK;
}

// GetBranchPosition
HRESULT COpcHdaBrowser::GetBranchPosition(
	LPWSTR* pszBranchPos
)
{
	COpcLock cLock(*this);
	
	// check for invalid arguments
    if (pszBranchPos == NULL)
    {
        return E_INVALIDARG;
    }

	// check if at root.
    if (m_cBrowsePath.IsEmpty())
    {
        *pszBranchPos = OpcStrDup(L"");
        return S_OK;
    }

    // copy current browse path (removing leading '/' if necessary).
	if (m_cBrowsePath.Find(_T("/")) == 0)
	{
		*pszBranchPos = OpcStrDup((LPCWSTR)m_cBrowsePath.SubStr(1));
	}
	else
	{
		*pszBranchPos = OpcStrDup((LPCWSTR)m_cBrowsePath);
	}


    return S_OK;
}

