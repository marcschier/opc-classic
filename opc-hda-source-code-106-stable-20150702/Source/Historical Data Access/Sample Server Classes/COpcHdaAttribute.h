//============================================================================
// TITLE: COpcHdaAttribute.h
//
// CONTENTS:
// 
// Constants that describe all well known item attributes.
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
// 2003/12/19 RSA   Initial implementation.

#ifndef _COpcHdaAttribute_H_
#define _COpcHdaAttribute_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

#include "COpcMap.h"
#include "COpcXmlElement.h"

class COpcHdaAttribute;
class COpcHdaBrowseFilter;

//==============================================================================
// TYPEDEF: COpcHdaBrowseFilterList
// PURPOSE: A list of attribute filters used to browse.

typedef COpcArray<COpcHdaBrowseFilter*> COpcHdaBrowseFilterList;

//==============================================================================
// CLASS:   OpcHdaBrowseFilter
// PURPOSE: A filter to apply to an item when browsing.

class COpcHdaBrowseFilter
{
	OPC_CLASS_NEW_DELETE_ARRAY()

public:

	//==========================================================================
	// Public Operators

	// Constructor
	COpcHdaBrowseFilter(DWORD dwID);

	// Copy Constructor
	COpcHdaBrowseFilter(const COpcHdaBrowseFilter& cFilter);

	// Destructor
	~COpcHdaBrowseFilter();

	// Assignment.
	COpcHdaBrowseFilter& operator=(const COpcHdaBrowseFilter& cFilter);

	//==========================================================================
	// Public Methods

	// ID
	DWORD GetID() const { return m_dwID; }
	void SetID(DWORD dwID) { m_dwID = dwID; }

	// Operator
	OPCHDA_OPERATORCODES GetOperator() const { return m_eOperator; }
	void SetOperator(OPCHDA_OPERATORCODES eOperator) { m_eOperator = eOperator; }

	// Value
	VARIANT& GetValue() { return m_cValue.GetRef(); }
	void SetValue(const VARIANT& cValue) { m_cValue = cValue; }

	// Error
	HRESULT GetError() const { return m_hError; }
	void SetError(HRESULT HRESULT) { m_hError = HRESULT; }

	//==========================================================================
	// Static Methods

	// creates a list of attribute browse filters.
	static void Create(
        DWORD                    dwCount,
		DWORD*                   pdwAttrID,
		OPCHDA_OPERATORCODES*    peOperators,
		VARIANT*                 pvFilters,
		COpcHdaBrowseFilterList& cFilters
	);

private:

	//==========================================================================
	// Private Members

	DWORD                m_dwID;
	OPCHDA_OPERATORCODES m_eOperator;
	COpcVariant          m_cValue;
	HRESULT              m_hError;
};

//============================================================================
// TYPE:    COpcHdaAttributeList
// PURPOSE: An list of item properties.

typedef COpcArray<COpcHdaAttribute*> COpcHdaAttributeList;
template class COpcArray<COpcHdaAttribute*>;

//============================================================================
// CLASS:   COpcHdaAttribute
// PURPOSE: A class that describes an attribute.

class COpcHdaAttribute
{
    OPC_CLASS_NEW_DELETE_ARRAY()

public:

	//==========================================================================
	// Public Operators

	// Constructor
	COpcHdaAttribute(DWORD dwID);

	// Copy Constructor
	COpcHdaAttribute(const COpcHdaAttribute& cAttribute);

	// Destructor
	~COpcHdaAttribute();

	// Assignment.
	COpcHdaAttribute& operator=(const COpcHdaAttribute& cAttribute);

	//==========================================================================
	// Public Methods

	// ID
	DWORD GetID() const { return m_dwID; }
	void SetID(DWORD dwID) { m_dwID = dwID; }

	// Name
	const COpcString& GetName() const { return m_cName; }
	void SetName(const COpcString& cName) { m_cName = cName; }

	// Description
	const COpcString& GetDescription() const { return m_cDescription; }
	void SetDescription(const COpcString& cDescription) { m_cDescription = cDescription; }

	// DataType
	VARTYPE GetDataType() const { return m_vtDataType; }
	void SetDataType(VARTYPE vtDataType) { m_vtDataType = vtDataType; }
	
	//==========================================================================
	// Static Methods

	// creates an array of the known attributes
	static void Create(
		DWORD&    dwCount,
		DWORD*&   pdwAttrID,
		LPWSTR*&  pszAttrName,
		LPWSTR*&  pszAttrDesc,
		VARTYPE*& pvtAttrDataType
	);

	// looks of the attribute id from an attribute name.
	static DWORD LookupName(const COpcString& cName);

private:

	DWORD       m_dwID;
	COpcString  m_cName;
	COpcString  m_cDescription;
	VARTYPE     m_vtDataType;
};

//============================================================================
// MACRO:   OPCHDA_ATTR_XXX
// PURPOSE: Vendor defined attribute types.

#define OPCHDA_UNCERTAIN_VALUES          0x80000001
#define OPCHDA_ATTRNAME_UNCERTAIN_VALUES _T("Uncertain Values")

#endif // _COpcHdaAttribute_H_