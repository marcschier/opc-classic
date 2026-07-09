//============================================================================
// TITLE: COpcHdaBrowser.h
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

#ifndef _COpcHdaBrowser_H_
#define _COpcHdaBrowser_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

#include "COpcHdaAttribute.h"

//============================================================================
// CLASS:   COpcHdaBrowser
// PURPOSE: A class that implements the IOPCServer interface.
// NOTES:

class COpcHdaBrowser :
    public COpcComObject,
    public IOPCHDA_Browser,
    public COpcSynchObject
{
    OPC_CLASS_NEW_DELETE()

    OPC_BEGIN_INTERFACE_TABLE(COpcHdaBrowser)
        OPC_INTERFACE_ENTRY(IOPCHDA_Browser)
    OPC_END_INTERFACE_TABLE()

public:

	//=========================================================================
    // Operators

    // Constructor
    COpcHdaBrowser();
    COpcHdaBrowser(COpcHdaBrowseFilterList& cFilters);

    // Destructor 
    ~COpcHdaBrowser();

    //=========================================================================
    // IOPCHDA_Browser

	STDMETHODIMP GetEnum(
		OPCHDA_BROWSETYPE dwBrowseType,
		LPENUMSTRING*     ppIEnumString
	);

	STDMETHODIMP ChangeBrowsePosition(
		OPCHDA_BROWSEDIRECTION dwBrowseDirection,
		LPCWSTR                szString
	);

	STDMETHODIMP GetItemID(
		LPCWSTR szNode,
		LPWSTR* pszItemID
	);


	STDMETHODIMP GetBranchPosition(
		LPWSTR* pszBranchPos
	);

private:
        
    //==========================================================================
    // Private Members

    COpcString              m_cBrowsePath;
	COpcHdaBrowseFilterList m_cFilters;
};

#endif // _COpcHdaBrowser_H_