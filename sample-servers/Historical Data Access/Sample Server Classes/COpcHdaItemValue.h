//==============================================================================
// TITLE: COpcHdaItemValue.h
//
// CONTENTS:
// 
// A single value for an item.
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

#ifndef _COpcHdaItemValue_H_
#define _COpcHdaItemValue_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

//==============================================================================
// CLASS:   COpcHdaItemValue
// PURPOSE: A single value in the historian database.

class COpcHdaItemValue
{
    OPC_CLASS_NEW_DELETE_ARRAY()

public:
    
	//==========================================================================
    // Properties

	double     dblValue;
	LONGLONG   llTimestamp;
	DWORD      dwQuality;

    //==========================================================================
    // Public Operators

	// Constructor
	COpcHdaItemValue();

	// Destructor
	~COpcHdaItemValue();

	// Copy Constructor
	COpcHdaItemValue(const COpcHdaItemValue& cValue);

	// Assignment
	COpcHdaItemValue& operator=(const COpcHdaItemValue& cValue);
		
    //==========================================================================
    // Static Methods

	// Parse
	static bool Parse(const COpcString& cBuffer, COpcHdaItemValue& cValue);
};

//==============================================================================
// VALUE:   COpcHdaItemValueArray
// PURPOSE: An array of item values sorted by timestamp.

typedef COpcSortedArray<LONGLONG,COpcHdaItemValue> COpcHdaItemValueArray;

//==============================================================================
// CLASS:   COpcHdaModifiedValue
// PURPOSE: A single modified value in the historian database.

class COpcHdaModifiedValue  
{
    OPC_CLASS_NEW_DELETE_ARRAY()

public:
    
	//==========================================================================
    // Properties

	double          dblValue;
	LONGLONG        llTimestamp;
	DWORD           dwQuality;
	LONGLONG        llModificationTime;
	OPCHDA_EDITTYPE eEditType;
	COpcString      cUser;

    //==========================================================================
    // Public Operators

	// Constructor
	COpcHdaModifiedValue();

	// Destructor
	~COpcHdaModifiedValue();

	// Copy Constructor
	COpcHdaModifiedValue(const COpcHdaModifiedValue& cValue);

	// Assignment
	COpcHdaModifiedValue& operator=(const COpcHdaModifiedValue& cValue);

    //==========================================================================
    // Static Methods

	// Parse
	static bool Parse(const COpcString& cBuffer, COpcHdaModifiedValue& cValue);
};

//==============================================================================
// VALUE:   COpcHdaModifiedValueArray
// PURPOSE: An array of modified item values sorted by timestamp.

typedef COpcSortedArray<LONGLONG,COpcHdaModifiedValue> COpcHdaModifiedValueArray;

//==============================================================================
// CLASS:   COpcHdaAttributeValue
// PURPOSE: A single attribute value in the historian database.

class COpcHdaAttributeValue
{
    OPC_CLASS_NEW_DELETE_ARRAY()

public:
    
	//==========================================================================
    // Properties

	DWORD       dwAttributeID;
	COpcVariant cValue;
	LONGLONG    llTimestamp;

    //==========================================================================
    // Public Operators

	// Constructor
	COpcHdaAttributeValue();

	// Destructor
	~COpcHdaAttributeValue();

	// Copy Constructor
	COpcHdaAttributeValue(const COpcHdaAttributeValue& cAttribute);

	// Assignment
	COpcHdaAttributeValue& operator=(const COpcHdaAttributeValue& cAttribute);
		
    //==========================================================================
    // Static Methods

	// Parse
	static bool Parse(const COpcString& cBuffer, COpcHdaAttributeValue& cAttribute);
};

//==============================================================================
// VALUE:   COpcHdaAttributeValueArray
// PURPOSE: An array of item values sorted by timestamp.

typedef COpcSortedArray<LONGLONG,COpcHdaAttributeValue> COpcHdaAttributeValueArray;

//==============================================================================
// CLASS:   COpcHdaAnnotation
// PURPOSE: A single annotation value in the historian database.

class COpcHdaAnnotation
{
    OPC_CLASS_NEW_DELETE_ARRAY()

public:
    
	//==========================================================================
    // Properties

	LONGLONG   llTimestamp;
	COpcString cAnnotation;
	LONGLONG   llCreationTime;
	COpcString cUser;

    //==========================================================================
    // Public Operators

	// Constructor
	COpcHdaAnnotation();

	// Destructor
	~COpcHdaAnnotation();

	// Copy Constructor
	COpcHdaAnnotation(const COpcHdaAnnotation& cAnnotation);

	// Assignment
	COpcHdaAnnotation& operator=(const COpcHdaAnnotation& cAnnotation);
		
    //==========================================================================
    // Static Methods

	// Parse
	static bool Parse(const COpcString& cBuffer, COpcHdaAnnotation& cAnnotation);
};

//==============================================================================
// VALUE:   COpcHdaAnnotationArray
// PURPOSE: An array of item values sorted by timestamp.

typedef COpcSortedArray<LONGLONG,COpcHdaAnnotation> COpcHdaAnnotationArray;

//==============================================================================
// MACRO:   OPCHDA_TAG_XXX
// PURPOSE: Defines constants used to identify data types in CSV files.

#define OPCHDA_TAG_VALUE      _T("VAL")
#define OPCHDA_TAG_MODIFIED   _T("MOD")
#define OPCHDA_TAG_ATTRIBUTE  _T("ATT")
#define OPCHDA_TAG_ANNOTATION _T("ANN")

#endif // _COpcHdaItem_H_
