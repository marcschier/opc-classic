//==============================================================================
// TITLE: COpcHdaAttribute.cpp
//
// CONTENTS:
// 
// Tables that describe all well known item attributes.
//
// (c) Copyright 2004-2004 The OPC Foundation
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

#include "StdAfx.h"
#include "COpcHdaAttribute.h"
#include "COpcVariant.h"

//==============================================================================
// Local Declarations

struct OpcHdaAttributeDesc
{
    DWORD   dwID;
    VARTYPE vtDataType;
    LPCWSTR szName;
    LPCWSTR szDescription;
};

OpcHdaAttributeDesc g_AttributeTable[] = 
{
    { OPCHDA_DATA_TYPE,          VT_I2,   OPCHDA_ATTRNAME_DATA_TYPE,          L"Specifies the data type for the item." },
    { OPCHDA_DESCRIPTION,        VT_BSTR, OPCHDA_ATTRNAME_DESCRIPTION,        L"Describes the item." },
    { OPCHDA_ENG_UNITS,          VT_BSTR, OPCHDA_ATTRNAME_ENG_UNITS,          L"The label to use in displays to define the units for the item (e.g., kg/sec)." },
    { OPCHDA_STEPPED,            VT_BOOL, OPCHDA_ATTRNAME_STEPPED,            L"Whether data from the history repository should be displayed as interpolated or stepped."  },
	{ OPCHDA_ARCHIVING,          VT_BOOL, OPCHDA_ATTRNAME_ARCHIVING,          L"Indicates whether historian is recording data for this item."  },
    { OPCHDA_DERIVE_EQUATION,    VT_BSTR, OPCHDA_ATTRNAME_DERIVE_EQUATION,    L"Specifies the equation to be used by a derived item to calculate its value."  },
    { OPCHDA_NODE_NAME,          VT_BSTR, OPCHDA_ATTRNAME_NODE_NAME,          L"Specifies the machine which is the source for the item."  },
    { OPCHDA_PROCESS_NAME,       VT_BSTR, OPCHDA_ATTRNAME_PROCESS_NAME,       L"Specifies the process which is the source for the item."  },
    { OPCHDA_SOURCE_NAME,        VT_BSTR, OPCHDA_ATTRNAME_SOURCE_NAME,        L"Specifies the name (e.g. the OPC-DA Item ID) of the item on the source."  },
    { OPCHDA_SOURCE_TYPE,        VT_BSTR, OPCHDA_ATTRNAME_SOURCE_TYPE,        L"Specifies what sort of source produces the data for the item (e.g. 'OPC')."  },
    { OPCHDA_NORMAL_MAXIMUM,     VT_R8,   OPCHDA_ATTRNAME_NORMAL_MAXIMUM,     L"Specifies the upper limit for the normal value range for the item."  },
    { OPCHDA_NORMAL_MINIMUM,     VT_R8,   OPCHDA_ATTRNAME_NORMAL_MINIMUM,     L"Specifies the lower limit for the normal value range for the item."  },
    { OPCHDA_ITEMID,             VT_BSTR, OPCHDA_ATTRNAME_ITEMID,             L"Specifies the leaf name portion the item id."  },
    { OPCHDA_MAX_TIME_INT,       VT_CY,   OPCHDA_ATTRNAME_MAX_TIME_INT,       L"Specifies the maximum interval between data points in the history repository."  },
    { OPCHDA_MIN_TIME_INT,       VT_CY,   OPCHDA_ATTRNAME_MIN_TIME_INT,       L"Specifies the minimum interval between data points in the history repository."  },
    { OPCHDA_EXCEPTION_DEV,      VT_R8,   OPCHDA_ATTRNAME_EXCEPTION_DEV,      L"Specifies the minimum amount that the data for the item must change in order for the change to be reported to the history database."  },
	{ OPCHDA_EXCEPTION_DEV_TYPE, VT_I2,   OPCHDA_ATTRNAME_EXCEPTION_DEV_TYPE, L"Specifies whether the exception deviation is given as an absolute value, percent of span, or percent of value." },
    { OPCHDA_HIGH_ENTRY_LIMIT,   VT_R8,   OPCHDA_ATTRNAME_HIGH_ENTRY_LIMIT,   L"Specifies the highest valid value for the item." },
    { OPCHDA_LOW_ENTRY_LIMIT,    VT_R8,   OPCHDA_ATTRNAME_LOW_ENTRY_LIMIT,    L"Specifies the lowest valid value for the item." },
    { OPCHDA_UNCERTAIN_VALUES,   VT_BOOL, OPCHDA_ATTRNAME_UNCERTAIN_VALUES,   L"Whether data with an uncertain quality is included in aggregate calculations." },
	{ 0, VT_EMPTY, NULL, NULL }
};

//============================================================================
// OpcHdaBrowseFilter

// Constructor
COpcHdaBrowseFilter::COpcHdaBrowseFilter(DWORD dwID)
{
	m_dwID      = dwID;
	m_eOperator = OPCHDA_EQUAL;
	m_hError    = S_OK;
}

// Copy Constructor
COpcHdaBrowseFilter::COpcHdaBrowseFilter(const COpcHdaBrowseFilter& cFilter)
{
	*this = cFilter;
}

// Destructor
COpcHdaBrowseFilter::~COpcHdaBrowseFilter()
{
}

// Assignment.
COpcHdaBrowseFilter& COpcHdaBrowseFilter::operator=(const COpcHdaBrowseFilter& cFilter)
{
	m_dwID      = cFilter.m_dwID;
	m_eOperator = cFilter.m_eOperator;
	m_cValue    = cFilter.m_cValue;
	m_hError    = cFilter.m_hError;

	return *this;
}

//==========================================================================
// Static Methods

// COpcHdaBrowseFilter::Create
void COpcHdaBrowseFilter::Create(
    DWORD                    dwCount,
	DWORD*                   pdwAttrID,
	OPCHDA_OPERATORCODES*    peOperators,
	VARIANT*                 pvFilters,
	COpcHdaBrowseFilterList& cFilters
)
{
	// initialize list of filters.
	cFilters.SetSize(dwCount);

	for (DWORD ii = 0; ii < dwCount; ii++)
	{
		cFilters[ii] = new COpcHdaBrowseFilter(pdwAttrID[ii]);

		// check that the attrbute is valid.
		COpcHdaAttribute cAttribute(pdwAttrID[ii]);

		if (cAttribute.GetName().IsEmpty())
		{
			cFilters[ii]->SetError(OPC_E_UNKNOWNATTRID);
			continue;
		}

		// convert filter value to the canonical data type for the filter.
		HRESULT hResult = COpcVariant::ChangeType(cFilters[ii]->GetValue(), pvFilters[ii], NULL, cAttribute.GetDataType());

		if (FAILED(hResult))
		{
			cFilters[ii]->SetError(OPC_E_INVALIDDATATYPE);
			continue;
		}

		// validate operator code against attribute id.
		switch (pdwAttrID[ii])
		{
			case OPCHDA_DATA_TYPE:
			case OPCHDA_DESCRIPTION:
			case OPCHDA_ENG_UNITS:
			case OPCHDA_STEPPED:
			case OPCHDA_ARCHIVING:
			case OPCHDA_DERIVE_EQUATION:
			case OPCHDA_EXCEPTION_DEV_TYPE:
			{
				if (peOperators[ii] != OPCHDA_EQUAL && peOperators[ii] != OPCHDA_NOTEQUAL)
				{
					cFilters[ii]->SetError(E_INVALIDARG);
					continue;
				}

				break;
			}
		}

		// set operator, value and error code.
		cFilters[ii]->SetOperator(peOperators[ii]);
		cFilters[ii]->SetError(S_OK);
	}
}

//============================================================================
// COpcHdaAttribute

// Constructor
COpcHdaAttribute::COpcHdaAttribute(DWORD dwID)
{
	m_dwID       = dwID;
	m_vtDataType = VT_EMPTY;

    for (DWORD ii = 0; g_AttributeTable[ii].dwID != 0; ii++)
    {
        if (g_AttributeTable[ii].dwID == dwID)
        {
			m_cName        = g_AttributeTable[ii].szName;
			m_cDescription = g_AttributeTable[ii].szDescription;
			m_vtDataType   = g_AttributeTable[ii].vtDataType;
			break;
        }
    }
}

// Copy Constructor
COpcHdaAttribute::COpcHdaAttribute(const COpcHdaAttribute& cAttribute)
{
	*this = cAttribute;
}

// Destructor
COpcHdaAttribute::~COpcHdaAttribute()
{
}

// Assignment.
COpcHdaAttribute& COpcHdaAttribute::operator=(const COpcHdaAttribute& cAttribute)
{
	m_dwID         = cAttribute.m_dwID;
	m_cName        = cAttribute.m_cName;
	m_cDescription = cAttribute.m_cDescription;
	m_vtDataType   = cAttribute.m_vtDataType;

	return *this;
}

// COpcHdaAttribute::Create
void COpcHdaAttribute::Create(
	DWORD&    dwCount,
	DWORD*&   pdwAttrID,
	LPWSTR*&  pszAttrName,
	LPWSTR*&  pszAttrDesc,
	VARTYPE*& pvtAttrDataType
)
{
	dwCount = 0;

	// determine the number of known attributes.
	while (g_AttributeTable[dwCount].dwID != 0) dwCount++;

	// allocate arrays.
	pdwAttrID       = (DWORD*)OpcArrayAlloc(DWORD, dwCount);
	pszAttrName     = (LPWSTR*)OpcArrayAlloc(LPWSTR, dwCount);
	pszAttrDesc     = (LPWSTR*)OpcArrayAlloc(LPWSTR, dwCount);
	pvtAttrDataType = (VARTYPE*)OpcArrayAlloc(VARTYPE, dwCount);

	for (DWORD ii = 0; ii < dwCount; ii++)
	{
		pdwAttrID[ii]       = g_AttributeTable[ii].dwID;
		pszAttrName[ii]     = OpcStrDup(g_AttributeTable[ii].szName);
		pszAttrDesc[ii]     = OpcStrDup(g_AttributeTable[ii].szDescription);
		pvtAttrDataType[ii] = g_AttributeTable[ii].vtDataType;
	}
}

// COpcHdaAttribute::LookupName
DWORD COpcHdaAttribute::LookupName(const COpcString& cName)
{
    for (DWORD ii = 0; g_AttributeTable[ii].dwID != 0; ii++)
    {
		if (_tcscmp((LPCWSTR)cName, g_AttributeTable[ii].szName) == 0)
        {
			return g_AttributeTable[ii].dwID;
        }
    }

	return -1;
}