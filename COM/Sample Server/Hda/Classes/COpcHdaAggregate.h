//============================================================================
// TITLE: COpcHdaAggregate.h
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

#ifndef _COpcHdaAggregate_H_
#define _COpcHdaAggregate_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

#include "COpcMap.h"
#include "COpcXmlElement.h"

class COpcHdaAggregate;

//============================================================================
// TYPE:    COpcHdaAggregateList
// PURPOSE: An list of item properties.

typedef COpcArray<COpcHdaAggregate*> COpcHdaAggregateList;
template class COpcArray<COpcHdaAggregate*>;

//============================================================================
// CLASS:   COpcHdaAggregate
// PURPOSE: A class that describes an aggregate.

class COpcHdaAggregate
{
    OPC_CLASS_NEW_DELETE_ARRAY()

public:

	//==========================================================================
	// Public Operators

	// Constructor
	COpcHdaAggregate(DWORD dwID);

	// Copy Constructor
	COpcHdaAggregate(const COpcHdaAggregate& cAggregate);

	// Destructor
	~COpcHdaAggregate();

	// Assignment.
	COpcHdaAggregate& operator=(const COpcHdaAggregate& cAggregate);

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
	
	//==========================================================================
	// Static Methods

	// creates an array of the known attributes
	static void Create(
		DWORD&    dwCount,
		DWORD*&   pdwAggrID,
		LPWSTR*&  pszAggrName,
		LPWSTR*&  pszAggrDesc
	);

private:

	DWORD       m_dwID;
	COpcString  m_cName;
	COpcString  m_cDescription;
};

#endif // _COpcHdaAggregate_H_