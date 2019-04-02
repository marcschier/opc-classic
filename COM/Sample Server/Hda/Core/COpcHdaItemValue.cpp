//==============================================================================
// TITLE: COpcHdaItemValue.cpp
//
// CONTENTS:
// 
// A single value for an item.
//
// (c) Copyright 2004 The OPC Foundation
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
// 2004/01/25 RSA   Initial implementation.

#include "StdAfx.h"
#include "COpcHdaItemValue.h"
#include "COpcHdaTime.h"
#include "COpcHdaAttribute.h"

//==============================================================================
// Local Functions

static bool ParseDateTime(COpcTextReader& cReader, OpcXml::DateTime& cDateTime)
{
	COpcText cText;

	// read timestamp.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");
	cText.SetEofDelim();

	if (!cReader.GetNext(cText))
	{
		return false;
	}

	COpcString cBuffer = cText;
	cBuffer.Trim();

	// insert a 'T' to make the string a valid XML date.
	for (UINT ii = 0; ii < cBuffer.GetLength(); ii++)
	{
		if (cBuffer[ii] == _T(' '))
		{
			cBuffer[ii] = _T('T');
			break;
		}
	}

	// parse the timestamp field.
	if (!OpcXml::Read(cBuffer, cDateTime))
	{
		return false;
	}

	return true;
}

//==============================================================================
// COpcHdaItemValue

// Constructor
COpcHdaItemValue::COpcHdaItemValue()
{
	dblValue    = 0;
	llTimestamp = 0;
	dwQuality   = OPCHDA_NODATA | OPC_QUALITY_BAD;
}

// Destructor
COpcHdaItemValue::~COpcHdaItemValue() 
{
}

// Copy Constructor
COpcHdaItemValue::COpcHdaItemValue(const COpcHdaItemValue& cValue)
{
	*this = cValue;
}

// Assignment
COpcHdaItemValue& COpcHdaItemValue::operator=(const COpcHdaItemValue& cValue)
{
	dblValue    = cValue.dblValue;
	llTimestamp = cValue.llTimestamp;
	dwQuality   = cValue.dwQuality;

	return *this;
}

// Parse
bool COpcHdaItemValue::Parse(const COpcString& cBuffer, COpcHdaItemValue& cValue)
{
	COpcTextReader cReader(cBuffer);

	COpcText cText;

	// read data tag.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");

	if (!cReader.GetNext(cText))
	{
		return false;
	}

	// not an item value.
	if (((COpcString&)cText) != OPCHDA_TAG_VALUE)
	{
		return false;
	}

	// store value locally until parsing completes successfully.
	OpcXml::Double   dblValue    = 0;
	OpcXml::DateTime ftTimestamp = OpcMinDate();
	OpcXml::UInt     dwQuality   = 0;

	if (!ParseDateTime(cReader, ftTimestamp))
	{
		return false;
	}

	// read value.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");
	cText.SetEofDelim();

	if (!cReader.GetNext(cText))
	{
		cValue.dblValue    = 0;
		cValue.dwQuality   = OPCHDA_NODATA | OPC_QUALITY_BAD;
		cValue.llTimestamp = OpcHdaInt64FromFILETIME(ftTimestamp);
		return true;
	}

	// parse the value field.
	if (!OpcXml::Read(cText, dblValue))
	{
		return false;
	}
	
	// read quality.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");
	cText.SetEofDelim();

	if (!cReader.GetNext(cText))
	{
		cValue.dblValue    = dblValue;
		cValue.dwQuality   = OPCHDA_RAW | OPC_QUALITY_GOOD;
		cValue.llTimestamp = OpcHdaInt64FromFILETIME(ftTimestamp);
		return true;
	}

	// parse the quality field.
	if (!OpcXml::Read(cText, dwQuality))
	{
		return false;
	}

	// parsing complete.
	cValue.dblValue    = dblValue;
	cValue.dwQuality   = dwQuality;
	cValue.llTimestamp = OpcHdaInt64FromFILETIME(ftTimestamp);
	
	SYSTEMTIME stTimestamp;
	FileTimeToSystemTime(&::OpcHdaFILETIMEFromInt64(cValue.llTimestamp), &stTimestamp);

	return true;
}

//==============================================================================
// COpcHdaModifiedValue

// Constructor
COpcHdaModifiedValue::COpcHdaModifiedValue()
{
	dblValue           = 0;
	llTimestamp        = 0;
	dwQuality          = OPCHDA_NODATA | OPC_QUALITY_BAD;
	llModificationTime = 0;
	eEditType          = OPCHDA_INSERT;
}

// Destructor
COpcHdaModifiedValue::~COpcHdaModifiedValue() 
{
}

// Copy Constructor
COpcHdaModifiedValue::COpcHdaModifiedValue(const COpcHdaModifiedValue& cValue)
{
	*this = cValue;
}

// Assignment
COpcHdaModifiedValue& COpcHdaModifiedValue::operator=(const COpcHdaModifiedValue& cValue)
{
	dblValue           = cValue.dblValue;
	llTimestamp        = cValue.llTimestamp;
	dwQuality          = cValue.dwQuality;
	llModificationTime = cValue.llModificationTime;
	eEditType          = cValue.eEditType;
	cUser              = cValue.cUser;

	return *this;
}

// Parse
bool COpcHdaModifiedValue::Parse(const COpcString& cBuffer, COpcHdaModifiedValue& cValue)
{
	COpcTextReader cReader(cBuffer);

	COpcText cText;

	// read data tag.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");

	if (!cReader.GetNext(cText))
	{
		return false;
	}

	// not a modified item value.
	if (((COpcString&)cText) != OPCHDA_TAG_MODIFIED)
	{
		return false;
	}

	// store value locally until parsing completes successfully.
	OpcXml::Double   dblValue           = 0;
	OpcXml::DateTime ftTimestamp        = OpcMinDate();
	OpcXml::UInt     dwQuality          = 0;
	OpcXml::DateTime ftModificationTime = OpcMinDate();
	OpcXml::UInt     uEditType          = 0;

	if (!ParseDateTime(cReader, ftTimestamp))
	{
		return false;
	}

	// read value.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");
	cText.SetEofDelim();

	if (!cReader.GetNext(cText))
	{
		return false;
	}

	// parse the value field.
	if (!OpcXml::Read(cText, dblValue))
	{
		return false;
	}
	
	// read quality.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");
	cText.SetEofDelim();

	if (!cReader.GetNext(cText))
	{
		return true;
	}

	// parse the quality field.
	if (!OpcXml::Read(cText, dwQuality))
	{
		return false;
	}
	
	// parse modification time.
	if (!ParseDateTime(cReader, ftModificationTime))
	{
		return false;
	}

	cValue.dblValue           = dblValue;
	cValue.dwQuality          = dwQuality;
	cValue.llTimestamp        = OpcHdaInt64FromFILETIME(ftTimestamp);
	cValue.llModificationTime = OpcHdaInt64FromFILETIME(ftModificationTime); 
	cValue.eEditType          = OPCHDA_INSERT;
	cValue.cUser              = (LPCWSTR)NULL;

	// read edit type.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");
	cText.SetEofDelim();

	if (!cReader.GetNext(cText))
	{
		return true;
	}

	// parse the edit type field.
	if (!OpcXml::Read(cText, uEditType))
	{
		return false;
	}	

	cValue.eEditType = (OPCHDA_EDITTYPE)uEditType;

	// read user id.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");
	cText.SetEofDelim();

	if (!cReader.GetNext(cText))
	{
		return true;
	}

	cValue.cUser = cText;
	return true;
}

//==============================================================================
// COpcHdaAttributeValue

// Constructor
COpcHdaAttributeValue::COpcHdaAttributeValue()
{
	llTimestamp   = 0;
	dwAttributeID = 0;
}

// Destructor
COpcHdaAttributeValue::~COpcHdaAttributeValue() 
{
}

// Copy Constructor
COpcHdaAttributeValue::COpcHdaAttributeValue(const COpcHdaAttributeValue& cAttribute)
{
	*this = cAttribute;
}

// Assignment
COpcHdaAttributeValue& COpcHdaAttributeValue::operator=(const COpcHdaAttributeValue& cAttribute)
{
	dwAttributeID = cAttribute.dwAttributeID;
	cValue        = cAttribute.cValue;
	llTimestamp   = cAttribute.llTimestamp;

	return *this;
}

// Parse
bool COpcHdaAttributeValue::Parse(const COpcString& cBuffer, COpcHdaAttributeValue& cAttribute)
{
	COpcTextReader cReader(cBuffer);

	COpcText cText;

	// read data tag.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");

	if (!cReader.GetNext(cText))
	{
		return false;
	}

	// not an item value.
	if (((COpcString&)cText) != OPCHDA_TAG_ATTRIBUTE)
	{
		return false;
	}

	// store value locally until parsing completes successfully.
	COpcString       cValue;
	OpcXml::DateTime ftTimestamp   = OpcMinDate();
	OpcXml::UInt     dwAttributeID = 0;

	if (!ParseDateTime(cReader, ftTimestamp))
	{
		return false;
	}

	// read value.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");
	cText.SetEofDelim();

	if (!cReader.GetNext(cText))
	{
		return false;
	}

	cValue = cText;
	
	// read attribute id.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");
	cText.SetEofDelim();

	if (!cReader.GetNext(cText))
	{
		return false;
	}

	// parse the attribute id field.
	if (!OpcXml::Read(cText, dwAttributeID))
	{
		return false;
	}

	// validate attribute.
	COpcHdaAttribute cDescription(dwAttributeID);

	if (cDescription.GetName().IsEmpty())
	{
		return false;
	}

	
	cAttribute.dwAttributeID = dwAttributeID;
	cAttribute.llTimestamp   = OpcHdaInt64FromFILETIME(ftTimestamp);

	// convert to attribute datatype.
	COpcVariant cSrc(cValue);
	COpcVariant cDst;

	HRESULT hResult = COpcVariant::ChangeType(cAttribute.cValue.GetRef(), cSrc.GetRef(), NULL, cDescription.GetDataType());

	if (FAILED(hResult))
	{
		return false;
	}

	return true;
}

//==============================================================================
// COpcHdaAnnotation

// Constructor
COpcHdaAnnotation::COpcHdaAnnotation()
{
	llTimestamp    = 0;
	llCreationTime = 0;
}

// Destructor
COpcHdaAnnotation::~COpcHdaAnnotation() 
{
}

// Copy Constructor
COpcHdaAnnotation::COpcHdaAnnotation(const COpcHdaAnnotation& cValue)
{
	*this = cValue;
}

// Assignment
COpcHdaAnnotation& COpcHdaAnnotation::operator=(const COpcHdaAnnotation& cValue)
{
	cAnnotation    = cValue.cAnnotation;
	llTimestamp    = cValue.llTimestamp;
	llCreationTime = cValue.llCreationTime;
	cUser          = cValue.cUser;

	return *this;
}

// Parse
bool COpcHdaAnnotation::Parse(const COpcString& cBuffer, COpcHdaAnnotation& cValue)
{
	COpcTextReader cReader(cBuffer);

	COpcText cText;

	// read data tag.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");

	if (!cReader.GetNext(cText))
	{
		return false;
	}

	// not a modified item value.
	if (((COpcString&)cText) != OPCHDA_TAG_ANNOTATION)
	{
		return false;
	}

	// store value locally until parsing completes successfully.
	OpcXml::DateTime ftTimestamp    = OpcMinDate();
	COpcString       cAnnotation    = (LPCWSTR)NULL;
	OpcXml::DateTime ftCreationTime = OpcMinDate();
	COpcString       cUser          = (LPCWSTR)NULL;

	if (!ParseDateTime(cReader, ftTimestamp))
	{
		return false;
	}

	// read annotation.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");
	cText.SetEofDelim();

	if (!cReader.GetNext(cText))
	{
		return false;
	}

	cAnnotation = cText;
    	
	// read user.
	cText.SetType(COpcText::Delimited);
    cText.SetDelims(L",");
	cText.SetEofDelim();

	if (!cReader.GetNext(cText))
	{
		return true;
	}

	cUser = cText;

	// parse creation time.
	if (!ParseDateTime(cReader, ftCreationTime))
	{
		return false;
	}

	cValue.llTimestamp    = OpcHdaInt64FromFILETIME(ftTimestamp);
	cValue.cAnnotation    = cAnnotation;
	cValue.llCreationTime = OpcHdaInt64FromFILETIME(ftCreationTime); 
	cValue.cUser          = cUser;
	return true;
}

