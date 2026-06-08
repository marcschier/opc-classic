//==============================================================================
// TITLE: COpcHdaItem.cpp
//
// CONTENTS:
// 
// A collection of historical values for an item.
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
#include "COpcHdaItem.h"
#include "COpcHdaAttribute.h"
#include "COpcHdaHistorian.h"
#include "COpcHdaTime.h"

//============================================================================
// Local Declarations

#define TAG_NAME       _T("Name")
#define TAG_START_TIME _T("StartTime")
#define TAG_TIMESTAMP  _T("Timestamp")

#define TICKS_PER_SECOND 10000000

//============================================================================
// COpcHdaItem

//========================================================================
// IOpcXmlSerialize


// Init
void COpcHdaItem::Init()
{
	m_vtDataType        = VT_R8;
	m_cDescription      = _T("A description of an item.");
	m_cEngUnits         = _T("Units");
	m_bStepped          = false;
	m_bArchiving        = false;
	m_cDeriveEquation   = _T("Direct");
	m_cNodeName         = _T("Localhost");
	m_cProcessName      = _T("Internal");
	m_cSourceType       = _T("File");
	m_dblNormalMax      = 1000;
	m_dblNormalMin      = 0;
	m_llMaxInterval     = 10000000;
	m_llMinInterval     = 10000000;
	m_dblDeviation      = 0;
	m_eDeviationType    = 0;
	m_dblLowEntryLimit  = DBL_MIN;
	m_dblHighEntryLimit = DBL_MAX;
	m_bUncertainValues  = false;

	m_uWaveform         = 1;
	m_dblPeriod         = 30;
	
	m_bLoaded           = false;
	m_llStartTime       = 0;
	m_llEndTime         = _I64_MAX;
	m_uFirstUseable     = -1;
	m_uLastUseable      = -1;

	// initialize source name and item name from the item id.
	m_cItemName = m_cItemID;

	int iIndex = m_cItemID.ReverseFind(_T("/"));

	if (iIndex != -1)
	{
		m_cItemName = m_cItemID.SubStr(iIndex+1);
	}

	m_cSourceName = m_cItemName;
	m_cSourceName += _T(".csv");
}
         
// Clear
void COpcHdaItem::Clear()
{
	m_cValues.RemoveAll();
	m_cModifiedValues.RemoveAll();
	m_cAttributeValues.RemoveAll();
	m_cAnnotations.RemoveAll();

    Init();
}

// LoadDataFromFile
bool COpcHdaItem::LoadDataFromFile(const COpcString& cFileName)
{
	// contruct file path.
	COpcString cFilePath;

	cFilePath += OpcGetModulePath();
	cFilePath += _T("\\");
	cFilePath += cFileName;

	// open file.
	COpcFile cFile;

	if (!cFile.Open(cFilePath))
	{
		return false;
	}

	// get pointer to file.
	BYTE* pBuffer = cFile.GetMemoryMapping();

	if (pBuffer == NULL)
	{
		return false;
	}

	// copy the file into a buffer for parsing.
	COpcTextReader cReader((LPCSTR)pBuffer, cFile.GetFileSize());

	// close file.
	cFile.Close();

	// read data.
	COpcText cText;

	COpcList<COpcHdaItemValue*>      cValues;
	COpcList<COpcHdaModifiedValue*>  cModifiedValues;
	COpcList<COpcHdaAttributeValue*> cAttributeValues;
	COpcList<COpcHdaAnnotation*>     cAnnotations;

	while (!cText.GetEof())
	{	
		// read next line.
		cText.SetType(COpcText::Delimited);
		cText.SetNewLineDelim();
		cText.SetEofDelim();

		if (!cReader.GetNext(cText))
		{
			break;
		}

		COpcString cRow = cText;

		if (cRow.Find(OPCHDA_TAG_VALUE) == 0)
		{
			// parse value.
			COpcHdaItemValue* pValue = new COpcHdaItemValue();

			if (!COpcHdaItemValue::Parse(cRow, *pValue))
			{
				delete pValue;
				continue;
			}

			cValues.AddTail(pValue);
		}
		
		else if (cRow.Find(OPCHDA_TAG_MODIFIED) == 0)
		{
			// parse value.
			COpcHdaModifiedValue* pValue = new COpcHdaModifiedValue();

			if (!COpcHdaModifiedValue::Parse(cRow, *pValue))
			{
				delete pValue;
				continue;
			}

			cModifiedValues.AddTail(pValue);
		}
		
		else if (cRow.Find(OPCHDA_TAG_ATTRIBUTE) == 0)
		{
			// parse value.
			COpcHdaAttributeValue* pValue = new COpcHdaAttributeValue();

			if (!COpcHdaAttributeValue::Parse(cRow, *pValue))
			{
				delete pValue;
				continue;
			}

			cAttributeValues.AddTail(pValue);
		}
		
		else if (cRow.Find(OPCHDA_TAG_ANNOTATION) == 0)
		{
			// parse value.
			COpcHdaAnnotation* pValue = new COpcHdaAnnotation();

			if (!COpcHdaAnnotation::Parse(cRow, *pValue))
			{
				delete pValue;
				continue;
			}

			cAnnotations.AddTail(pValue);
		}
	}

	if (cValues.GetCount() > 0)
	{
		m_cValues.SetCapacity(cValues.GetCount());

		// copy list of values into an indexable array of values.
		while (cValues.GetCount() > 0)
		{
			COpcHdaItemValue* pValue = cValues.RemoveHead();
			m_cValues.Append(pValue->llTimestamp, *pValue);
			delete pValue;
		}
	}

	if (cModifiedValues.GetCount() > 0)
	{
		m_cModifiedValues.SetCapacity(cModifiedValues.GetCount());

		// copy list of values into an indexable array of values.
		while (cModifiedValues.GetCount() > 0)
		{
			COpcHdaModifiedValue* pValue = cModifiedValues.RemoveHead();
			m_cModifiedValues.Append(pValue->llTimestamp, *pValue);
			delete pValue;
		}
	}
	
	if (cAttributeValues.GetCount() > 0)
	{
		m_cAttributeValues.SetCapacity(cAttributeValues.GetCount());

		// copy list of values into an indexable array of values.
		while (cAttributeValues.GetCount() > 0)
		{
			COpcHdaAttributeValue* pValue = cAttributeValues.RemoveHead();
			m_cAttributeValues.Append(pValue->llTimestamp, *pValue);
			delete pValue;
		}
	}

	if (cAnnotations.GetCount() > 0)
	{
		m_cAnnotations.SetCapacity(cAnnotations.GetCount());

		// copy list of values into an indexable array of values.
		while (cAnnotations.GetCount() > 0)
		{
			COpcHdaAnnotation* pValue = cAnnotations.RemoveHead();
			m_cAnnotations.Append(pValue->llTimestamp, *pValue);
			delete pValue;
		}
	}

	return true;
}

// LoadData
void COpcHdaItem::LoadData()
{
	// load initial values from file.
	if (!m_bLoaded)
	{
		// delete existing values.
		m_cValues.RemoveAll();
		m_cModifiedValues.RemoveAll();
		m_cAttributeValues.RemoveAll();
		m_cAnnotations.RemoveAll();

		m_llStartTime = 0;
		m_llEndTime   = 0;

		// load data from CSV file.
		if (LoadDataFromFile(m_cSourceName))
		{
			FixBoundingTimes();
		}

		m_bLoaded = true;
	}

	// dynamically generate new data.
	if (m_bArchiving)
	{
		LONGLONG llNow = OpcHdaInt64FromFILETIME(OpcUtcNow());

		// determine range of points to create.
		LONGLONG llStartTime = m_llEndTime + m_llMaxInterval;
		LONGLONG llEndTime   = llNow;

		if (m_llEndTime == 0)
		{
			llStartTime = llNow - (LONGLONG)(m_dblPeriod*TICKS_PER_SECOND);
		}

		// fill in points from the end time until now.
		for (LONGLONG llTimestamp = llStartTime; llTimestamp <= llEndTime; llTimestamp += m_llMaxInterval)
		{
			COpcHdaItemValue cValue;

			cValue.dblValue    = CreateValue(llTimestamp);
			cValue.llTimestamp = llTimestamp;
			cValue.dwQuality   = OPCHDA_RAW | OPC_QUALITY_GOOD;
				
			m_cValues.Insert(cValue.llTimestamp, cValue);
		}

		// fixup bounding times.
		FixBoundingTimes();
	}
}

// ParseDeriveEquation
void COpcHdaItem::ParseDeriveEquation(const COpcString& cEquation)
{
	// set reasonable defaults.
	m_uWaveform = 0;
	m_dblPeriod = 30;

	COpcTextReader cReader(cEquation);

	COpcText cText;

	// get the waveform type.
	cText.SetType(COpcText::Delimited);
	cText.SetDelims(_T("["));
	cText.SetSkipWhitespace();
	cText.SetEofDelim();

	if (!cReader.GetNext(cText))
	{
		return;
	}

	if (!OpcXml::Read(cText, m_uWaveform))
	{
		m_uWaveform = 0;
		return;
	}

	// get the waveform period.
	cText.SetType(COpcText::Delimited);
	cText.SetDelims(_T("]"));
	cText.SetSkipWhitespace();
	cText.SetEofDelim();

	if (!cReader.GetNext(cText))
	{
		return;
	}

	if (!OpcXml::Read(cText, m_dblPeriod))
	{
		m_dblPeriod = 30;	
		return;
	}
}

// CreateValue
double COpcHdaItem::CreateValue(LONGLONG llTimestamp)
{
	// the 'index' in period.
	double dblIndex = (double)(llTimestamp%((LONGLONG)(m_dblPeriod*TICKS_PER_SECOND)));
			
	// the 'fraction' of the period.
	double dblFraction = (dblIndex/(m_dblPeriod*TICKS_PER_SECOND));

	// the 'amplitude' of the waveform.
	double dblAmplitude = m_dblNormalMax - m_dblNormalMin;
	
	// the 'offset' of the waveform.
	double dblOffset = m_dblNormalMin;

	// generate value based on equation type.
	switch (m_uWaveform)
	{
		// ramp (X = period, Y = peak value).
		case 1:
		{
			return dblFraction*dblAmplitude + dblOffset;
		}

		// square (X = period, Y = peak value).
		case 2:
		{
			return (dblFraction < 0.5)?dblOffset:dblAmplitude + dblOffset;
		}

		// sine (X = period, Y = peak value).
		case 3:
		{
			// simple math.
			return sin(2*M_PI*dblFraction)*dblAmplitude + dblOffset;
		}

		// constant (X = value).
		default:
		{
			return dblAmplitude - dblOffset;
		}
	}
}

// SaveData
void COpcHdaItem::SaveData()
{
}

// Read
bool COpcHdaItem::Read(COpcXmlElement& cElement)
{
    Clear();

    // get item properties.
    COpcXmlElementList cChildren;

    UINT uCount = cElement.GetChildren(cChildren);

    // must have at least a value specified.
    if (uCount == 0)
    {
        return false;
    }

	// ensure attribute values are initialized to default values.
	Clear();
	
	for (UINT ii = 0; ii < uCount; ii++)
	{
		// read attribute name.
        COpcString cName;

        if (!OpcXml::ReadXml(cChildren[ii].GetAttribute(TAG_NAME), cName)) 
        {
            return false;
        }

		// lookup attribute id.
		DWORD dwID = COpcHdaAttribute::LookupName(cName);

		if (dwID == -1)
		{
			return false;
		}

        // read attribute value.
		OpcXml::AnyType cValue;
        
        if (!OpcXml::ReadXml(cChildren[ii], cValue)) 
        {
            return false;
		}

		// store attribute value in the appropriate place.
		switch (dwID)
		{	
			case OPCHDA_DATA_TYPE: 
			{
				m_vtDataType = OpcXml::GetVarType(cValue.eType);
				break;
			}
			
			case OPCHDA_DERIVE_EQUATION:    
			{ 
				if (!cValue.Get(m_cDeriveEquation))   
				{
					return false;
				}

				ParseDeriveEquation(m_cDeriveEquation);
				break; 
			}

			case OPCHDA_DESCRIPTION:        { if (!cValue.Get(m_cDescription))      return false; break; }
			case OPCHDA_ENG_UNITS:          { if (!cValue.Get(m_cEngUnits))         return false; break; }
			case OPCHDA_STEPPED:            { if (!cValue.Get(m_bStepped))          return false; break; }				
			case OPCHDA_ARCHIVING:          { if (!cValue.Get(m_bArchiving))        return false; break; }
			case OPCHDA_NODE_NAME:          { if (!cValue.Get(m_cNodeName))         return false; break; }
			case OPCHDA_PROCESS_NAME:       { if (!cValue.Get(m_cProcessName))      return false; break; }	
			case OPCHDA_SOURCE_NAME:        { if (!cValue.Get(m_cSourceName))       return false; break; }	
			case OPCHDA_SOURCE_TYPE:        { if (!cValue.Get(m_cSourceType))       return false; break; }	
			case OPCHDA_NORMAL_MAXIMUM:     { if (!cValue.Get(m_dblNormalMax))      return false; break; }	
			case OPCHDA_NORMAL_MINIMUM:     { if (!cValue.Get(m_dblNormalMin))      return false; break; }	
			case OPCHDA_ITEMID:             { if (!cValue.Get(m_cItemName))         return false; break; }	
			case OPCHDA_MAX_TIME_INT:       { if (!cValue.Get(m_llMaxInterval))     return false; break; }	
			case OPCHDA_MIN_TIME_INT:       { if (!cValue.Get(m_llMinInterval))     return false; break; }		
			case OPCHDA_EXCEPTION_DEV:      { if (!cValue.Get(m_dblDeviation))      return false; break; }	
			case OPCHDA_EXCEPTION_DEV_TYPE: { if (!cValue.Get(m_eDeviationType))    return false; break; }	
			case OPCHDA_HIGH_ENTRY_LIMIT:   { if (!cValue.Get(m_dblHighEntryLimit)) return false; break; }
			case OPCHDA_LOW_ENTRY_LIMIT:    { if (!cValue.Get(m_dblLowEntryLimit))  return false; break; }
			case OPCHDA_UNCERTAIN_VALUES:   { if (!cValue.Get(m_bUncertainValues))  return false; break; }		
				
			default:
			{
				OpcXml::AnyType* pAttribute = NULL;

				if (!m_cAttributes.Lookup(dwID, pAttribute))
				{
					m_cAttributes[dwID] = pAttribute = new OpcXml::AnyType();
				}
				
				*pAttribute = cValue;
				break;
			}
		}
	}

	return true;
}

// Write
bool COpcHdaItem::Write(COpcXmlElement& cElement)
{
	OPC_ASSERT(false);  
	
	// not implemented.
	
	return false;
}
	
// BuildAddressSpace
bool COpcHdaItem::BuildAddressSpace()
{
	if (!::GetHistorian().AddLink(m_cItemID))
	{
		return false;
	}

	return true;
}

// ClearAddressSpace
void COpcHdaItem::ClearAddressSpace()
{
	::GetHistorian().RemoveLink(m_cItemID);
}

// GetAttribute
bool COpcHdaItem::GetAttribute(DWORD dwID, OpcXml::AnyType& cValue)
{
	// store attribute value in the appropriate place.
	switch (dwID)
	{	
		case OPCHDA_DATA_TYPE: 	        { cValue.Set((short)m_vtDataType); return true; }
		case OPCHDA_DESCRIPTION:        { cValue.Set(m_cDescription);      return true; }
		case OPCHDA_ENG_UNITS:   		{ cValue.Set(m_cEngUnits);         return true; }	
		case OPCHDA_ARCHIVING:          { cValue.Set(m_bArchiving);        return true; }
		case OPCHDA_NODE_NAME:          { cValue.Set(m_cNodeName);         return true; } 
		case OPCHDA_PROCESS_NAME:       { cValue.Set(m_cProcessName);      return true; } 
		case OPCHDA_SOURCE_NAME:        { cValue.Set(m_cSourceName);       return true; } 
		case OPCHDA_SOURCE_TYPE:        { cValue.Set(m_cSourceType);       return true; } 
		case OPCHDA_NORMAL_MAXIMUM:     { cValue.Set(m_dblNormalMax);      return true; } 
		case OPCHDA_NORMAL_MINIMUM:     { cValue.Set(m_dblNormalMin);      return true; } 
		case OPCHDA_ITEMID:  	        { cValue.Set(m_cItemName);         return true; }	
		case OPCHDA_EXCEPTION_DEV:      { cValue.Set(m_dblDeviation);      return true; } 
		case OPCHDA_EXCEPTION_DEV_TYPE: { cValue.Set(m_eDeviationType);    return true; } 
		case OPCHDA_HIGH_ENTRY_LIMIT:   { cValue.Set(m_dblHighEntryLimit); return true; } 
		case OPCHDA_LOW_ENTRY_LIMIT:    { cValue.Set(m_dblLowEntryLimit);  return true; } 
		case OPCHDA_STEPPED:            { cValue.Set(m_bStepped);          return true; }	
		case OPCHDA_UNCERTAIN_VALUES:   { cValue.Set(m_bUncertainValues);  return true; }	
		case OPCHDA_DERIVE_EQUATION:    { cValue.Set(m_cDeriveEquation);   return true; }	

		case OPCHDA_MAX_TIME_INT:       
		{ 
			OpcXml::Decimal decimalValue;
			decimalValue.int64 = m_llMaxInterval;
			cValue.Set(decimalValue); 
			return true; 
		}

		case OPCHDA_MIN_TIME_INT:  
		{ 
			OpcXml::Decimal decimalValue;
			decimalValue.int64 = m_llMinInterval;
			cValue.Set(decimalValue); 
			return true; 
		} 

		default:
		{
			OpcXml::AnyType* pAttribute = NULL;

			if (!m_cAttributes.Lookup(dwID, pAttribute))
			{
				return false;
			}
			
			cValue = *pAttribute;
			return true;
		}
	}
}

// Match
bool COpcHdaItem::Match(COpcHdaBrowseFilter& cFilter)
{
	// always return no match if attribute does not exist.
	OpcXml::AnyType* pValue = NULL;

	if (!m_cAttributes.Lookup(cFilter.GetID(), pValue))
	{
		return false;
	}

	OpcXml::AnyType cFilterValue(cFilter.GetValue());

	// check for type mis-match.
	if (cFilterValue.eType != pValue->eType)
	{
		return false;
	}

	// always attempt to match patterns if data type is a string.
	if (pValue->eType == OpcXml::XML_STRING && pValue->iLength < 0 && cFilterValue.iLength < 0)
	{
		return OpcMatchPattern(pValue->stringValue, cFilterValue.stringValue, true);
	}
    
	// apply the operator.
	switch (cFilter.GetOperator())
	{
		case OPCHDA_EQUAL:        { return (pValue->Compare(cFilterValue) == 0); }
		case OPCHDA_LESS:         { return (pValue->Compare(cFilterValue) <  0); }
		case OPCHDA_LESSEQUAL:    { return (pValue->Compare(cFilterValue) <= 0); }
		case OPCHDA_GREATER:      { return (pValue->Compare(cFilterValue) >  0); }
		case OPCHDA_GREATEREQUAL: { return (pValue->Compare(cFilterValue) >= 0); }
		case OPCHDA_NOTEQUAL:     { return (pValue->Compare(cFilterValue) != 0); }
	}

	// no match.
	return false;
}

// GetTimestampAtIndex
LONGLONG COpcHdaItem::GetTimestampAtIndex(UINT uIndex)
{
	if (m_cValues.IsValidIndex(uIndex))
	{
		return m_cValues[uIndex].llTimestamp;
	}

	return 0;
}

// ReadIndicies
bool COpcHdaItem::ReadIndicies(
	LONGLONG llStartTime,
	LONGLONG llEndTime,
	UINT     uMaxValues,
	bool     bIncludeBounds,
	UINT*    pIndicies,
	UINT&    uCount
)
{
	uCount = 0;

	// exactly one value requested.
	if (llStartTime == llEndTime)
	{
		// get the start index.
		UINT uIndex = m_cValues.FindIndex(llStartTime);

		// add single value.
		if (uIndex != -1)
		{
			pIndicies[uCount++] = uIndex;
		}

		return true;
	}

	// ascending series requested.
	else if (llEndTime == 0 || (llStartTime != 0 && llStartTime < llEndTime))
	{
		// get the start index.
		UINT uIndex = m_cValues.FindIndex(llStartTime);

		if (uIndex == -1)
		{			
			if (bIncludeBounds && llStartTime != 0)
			{
				pIndicies[uCount++] = m_cValues.FindIndexBefore(llStartTime);

				// check if space for data.
				if (uCount >= uMaxValues)
				{
					return false;
				}
			}
            
			uIndex = m_cValues.FindIndexAfter(llStartTime);
		}

		// check if no data exists after start time and before end time.
		if (uIndex == -1 || (llEndTime != 0  && llEndTime <=  GetTimestampAtIndex(uIndex)))
		{
			// no data to return.
			if (!bIncludeBounds)
			{
				return true;
			}

			// check if space for end bound. 
			if (uCount >= uMaxValues)
			{
				return false;
			}

			// add end bound.
			pIndicies[uCount++] = uIndex;
			return true;
		}

		// add start value to list.
		pIndicies[uCount++] = uIndex++;
		
		LONGLONG llTimestamp = 0;

		while (uCount < uMaxValues)
		{						
			// get timestamp of next sample.
			llTimestamp = GetTimestampAtIndex(uIndex);

			// check if no more data.
			if (llTimestamp == 0)
			{
				break;
			}

			// check for end of time range.
			if (llEndTime != 0 && llTimestamp >= llEndTime)
			{
				break;
			}
			
			// save index of sample an increment index.
			pIndicies[uCount++] = uIndex++;
		}

		// all data within the range found. add end bound before return.
		if (uCount < uMaxValues)
		{
			if (bIncludeBounds && llEndTime != 0)
			{
				pIndicies[uCount++] = (llTimestamp != 0)?uIndex:-1;
			}

			return true;
		}

		// all requested data returned.
		if (llEndTime == 0)
		{
			return true;
		}

		// check if next sample is the bound and bounds are not required.
		if (!bIncludeBounds)
		{
			if (llEndTime == 0 || llEndTime <=  GetTimestampAtIndex(uIndex))
			{
				return true;
			}
		}
		
		// not all data could be returned.
		return (llEndTime == 0);
	}

	// descending series requested.
	else if (llStartTime == 0 || llEndTime < llStartTime)
	{
		// swap end time and start time if start time is zero.
		if (llStartTime == 0)
		{
			llStartTime = llEndTime;
			llEndTime   = 0;
		}

		// get the start index.
		UINT uIndex = m_cValues.FindIndex(llStartTime);

		if (uIndex == -1)
		{			
			if (bIncludeBounds)
			{
				pIndicies[uCount++] = m_cValues.FindIndexAfter(llStartTime);

				// check if space for data.
				if (uCount >= uMaxValues)
				{
					return false;
				}
			}
            
			uIndex = m_cValues.FindIndexBefore(llStartTime);
		}

		// check if no data exists before start time and after end time.
		if (uIndex == -1 || (llEndTime != 0  && llEndTime >=  GetTimestampAtIndex(uIndex)))
		{
			// no data to return.
			if (!bIncludeBounds)
			{
				return true;
			}

			// check if space for end bound. 
			if (uCount >= uMaxValues)
			{
				return false;
			}

			// add end bound.
			pIndicies[uCount++] = uIndex;
			return true;
		}

		// add start value to list.
		pIndicies[uCount++] = uIndex--;
		
		LONGLONG llTimestamp = 0;

		while (uCount < uMaxValues)
		{						
			// get timestamp of next sample.
			llTimestamp =  GetTimestampAtIndex(uIndex);

			// check if no more data.
			if (llTimestamp == 0)
			{
				break;
			}

			// check for end of time range.
			if (llEndTime != 0  && llEndTime >=  GetTimestampAtIndex(uIndex))
			{
				break;
			}
			
			// save index of sample an increment index.
			pIndicies[uCount++] = uIndex--;
		}

		// all data within the range found. add end bound before return.
		if (uCount < uMaxValues)
		{
			if (bIncludeBounds && llEndTime != 0)
			{
				pIndicies[uCount++] = (llTimestamp != 0)?uIndex:-1;
			}

			return true;
		}

		// check if next sample is the bound and bounds are not required.
		if (!bIncludeBounds)
		{
			if (llEndTime == 0 || llEndTime >=  GetTimestampAtIndex(uIndex))
			{
				return true;
			}
		}
		
		// not all data could be returned.
		return (llEndTime == 0);
	}

	// no data to return;
	return true;
}

// ReadRaw
HRESULT COpcHdaItem::ReadRaw(
	LONGLONG     llStartTime,
	LONGLONG     llEndTime,
	UINT         uNumValues,
	bool         bIncludeBounds,
	UINT         uStartIndex,
	OPCHDA_ITEM& cItem
)
{
	// ensure data is loaded for the item.
	LoadData();

	SYSTEMTIME stStartTime;
	FILETIME   ftStartTime = ::OpcHdaFILETIMEFromInt64(llStartTime);
	FileTimeToSystemTime(&ftStartTime, &stStartTime);

	SYSTEMTIME stEndTime;
	FILETIME   ftEndTime = ::OpcHdaFILETIMEFromInt64(llEndTime);
	FileTimeToSystemTime(&ftEndTime, &stEndTime);

	SYSTEMTIME m_stStartTime;
	FILETIME   m_ftStartTime = ::OpcHdaFILETIMEFromInt64(m_llStartTime);
	FileTimeToSystemTime(&m_ftStartTime, &m_stStartTime);

	SYSTEMTIME m_stEndTime;
	FILETIME   m_ftEndTime = ::OpcHdaFILETIMEFromInt64(m_llEndTime);
	FileTimeToSystemTime(&m_ftEndTime, &m_stEndTime);

	// initialize empty item object.
	cItem.dwCount       = 0;
	cItem.haAggregate   = OPCHDA_NOAGGREGATE;
	cItem.hClient       = NULL;
	cItem.pvDataValues  = NULL;
	cItem.pdwQualities  = NULL;
	cItem.pftTimeStamps = NULL;

	// calculate the maximum number of values that could be returned.
	UINT uMaxValues = uNumValues + uStartIndex;

	if (uMaxValues == 0)
	{
		LONGLONG llUpperBound = (llEndTime >= llStartTime)?llEndTime:llStartTime;
		LONGLONG llLowerBound = (llEndTime <= llStartTime)?llEndTime:llStartTime;

		if (llUpperBound > m_llEndTime)	  { llUpperBound = m_llEndTime;   }
		if (llUpperBound < m_llStartTime) {	llUpperBound = m_llStartTime; }
		if (llLowerBound > m_llEndTime)   { llLowerBound = m_llEndTime;   }
		if (llLowerBound < m_llStartTime) {	llLowerBound = m_llStartTime; }

		// find the number of values in the interval.
		UINT uLower = 0;
		UINT uBound = 0;
		UINT uUpper = 0;

		m_cValues.FindBounds(llLowerBound, uLower, uBound);
		m_cValues.FindBounds(llUpperBound, uBound, uUpper);

		if (uUpper == -1)
		{
			uUpper = m_cValues.GetCount()-1;
		}

		if (uLower == -1)
		{
			uLower = 0;
		}

		// minimum is three values: one value plus two bounds.
		uMaxValues = uUpper - uLower + 3;
	}

	// apply any server defined limit on the number of values that can be returned.
	if (OPCHDA_MAX_VALUES > 0 && uMaxValues > OPCHDA_MAX_VALUES)
	{
		uMaxValues = OPCHDA_MAX_VALUES;
	}

	// lookup indicies of values within the time range.
	UINT* pIndicies = OpcArrayAlloc(UINT, uMaxValues);
	memset(pIndicies, 0, sizeof(UINT)*uMaxValues);

	UINT uCount = 0;
	
	bool bNoMoreData = ReadIndicies(
		llStartTime,
		llEndTime,
		uMaxValues,
		bIncludeBounds,
		pIndicies,
		uCount
	);

	// no data found.
	if (uCount == 0)
	{
		OpcFree(pIndicies);
		return OPC_S_NODATA;
	}

	cItem.dwCount = uCount-uStartIndex;

	// allocate arrays.
	cItem.pvDataValues  = OpcArrayAlloc(VARIANT, cItem.dwCount);
	cItem.pdwQualities  = OpcArrayAlloc(DWORD, cItem.dwCount);
	cItem.pftTimeStamps = OpcArrayAlloc(FILETIME, cItem.dwCount);

	memset(cItem.pvDataValues, 0, sizeof(VARIANT)*cItem.dwCount);
	memset(cItem.pdwQualities, 0, sizeof(DWORD)*cItem.dwCount);
	memset(cItem.pftTimeStamps, 0, sizeof(FILETIME)*cItem.dwCount);

	// fill in values.
	for (UINT ii = 0; ii < cItem.dwCount; ii++)
	{
		UINT uIndex = pIndicies[ii + uStartIndex];

		// handle case where bounding values are outside the range of valid values.
		if (uIndex == -1)
		{
			cItem.pdwQualities[ii] = OPCHDA_NOBOUND | OPC_QUALITY_BAD;

			if (ii == 0)
			{
				cItem.pftTimeStamps[ii] = OpcHdaFILETIMEFromInt64(llStartTime);
			}
			else if (ii == uCount-1)
			{				
				cItem.pftTimeStamps[ii] = OpcHdaFILETIMEFromInt64(llEndTime);
			}
		}

		// fetch the value at the index.
		else
		{
			cItem.pftTimeStamps[ii] = OpcHdaFILETIMEFromInt64(m_cValues[uIndex].llTimestamp);
			cItem.pvDataValues[ii]  = GetValue(uIndex, cItem.pdwQualities[ii]);
		}
	}

	// free memory.
	OpcFree(pIndicies);

	// return result.
	return (bNoMoreData)?S_OK:OPC_S_MOREDATA;
}

// GetValue
VARIANT COpcHdaItem::GetValue(UINT uIndex, DWORD& dwQuality)
{
	// check for invalid index.
	if (!m_cValues.IsValidIndex(uIndex))
	{
		VARIANT vValue;
		VariantInit(&vValue);

		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return vValue;
	}

	dwQuality = m_cValues[uIndex].dwQuality;
	return GetValue(m_cValues[uIndex].dblValue, dwQuality);
}

// GetValue
VARIANT COpcHdaItem::GetValue(double dblValue, DWORD& dwQuality)
{
	COpcVariant cVariant(dblValue);

	VARIANT vValue;
	VariantInit(&vValue);

	HRESULT hResult = COpcVariant::ChangeType(vValue, cVariant.GetRef(), NULL, m_vtDataType);

	if (FAILED(hResult))
	{
		VariantClear(&vValue);
		dwQuality = OPCHDA_CONVERSION | OPC_QUALITY_BAD;
	}

	return vValue;
}

// IsUseableData
bool COpcHdaItem::IsUseableData(UINT uIndex)
{
	if (m_cValues.IsValidIndex(uIndex))
	{
		return IsUseableData(m_cValues[uIndex]);
	}

	return false;
}

// IsUseableData
bool COpcHdaItem::IsUseableData(const COpcHdaItemValue& cValue)
{
	if (!m_bUncertainValues)
	{
		return OPC_MASK_ISSET(cValue.dwQuality, OPC_QUALITY_GOOD);
	}
	else
	{
		return OPC_MASK_ISSET(cValue.dwQuality, OPC_QUALITY_UNCERTAIN);
	}

	return false;
}

// FixBoundingTimes
void COpcHdaItem::FixBoundingTimes()
{	
	m_llStartTime   = 0;
	m_llEndTime     = 0;
	m_uFirstUseable = -1;
	m_uLastUseable  = -1;

	if (m_cValues.GetCount() > 0)
	{
        UINT ii;
		m_llStartTime = m_cValues[0].llTimestamp;
		m_llEndTime   = m_cValues.GetLastValue().llTimestamp;

		// determine first useable value.
		for (ii = 0; ii < m_cValues.GetCount(); ii++)
		{
			if (IsUseableData(m_cValues[ii]))
			{
				m_uFirstUseable = ii;
				break;
			}
		}

		// determine last useable value.
		for (ii = m_cValues.GetCount()-1; ii >= m_uFirstUseable; ii--)
		{
			if (IsUseableData(m_cValues[ii]))
			{
				m_uLastUseable = ii;
				break;
			}
		}
	}
}

// Interpolate
double COpcHdaItem::Interpolate(LONGLONG llTime, DWORD& dwQuality)
{
	UINT uLower = 0;
	UINT uUpper = 0;

	// look of the item values that bound this value.
	if (m_cValues.FindBounds(llTime, uLower, uUpper))
	{
		if (IsUseableData(uLower))
		{
			dwQuality = m_cValues[uLower].dwQuality;
			return m_cValues[uLower].dblValue;
		}
	}

	bool bValuesSkipped = false;

	// search for good quality lower bound.
	while (uLower != -1 && !IsUseableData(uLower))
	{
		// no useable data found prior to the  start range.
		if (uLower == 0)
		{
			uLower = -1;
			break;
		}

		uLower--;
		bValuesSkipped = true;
	}

	// cannot interpolate if there is no beginning bound.
	if (uLower == -1)
	{
		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return 0;
	}

	// search for good quality upper bound.
	while (uUpper != -1 && !IsUseableData(uUpper))
	{
		// no useable data found prior to the  start range.
		if (uUpper == m_cValues.GetCount()-1)
		{
			uUpper = -1;
			break;
		}

		uUpper++;
		bValuesSkipped = true;
	}

	// extrapolate the last good value if there is no end bound.
	if (uUpper == -1)
	{	
		dwQuality = OPCHDA_INTERPOLATED | OPC_QUALITY_SUB_NORMAL;
		return m_cValues[uLower].dblValue;
	}

	// calculate interpolated value.
	COpcHdaItemValue& cLower = m_cValues[uLower];
	COpcHdaItemValue& cUpper = m_cValues[uUpper];

	// set quality from the quality of the values used in the calculation.
	dwQuality = OPCHDA_INTERPOLATED;

	if (OPC_MASK_ISSET(cLower.dwQuality, OPC_QUALITY_GOOD) && OPC_MASK_ISSET(cUpper.dwQuality, OPC_QUALITY_GOOD))
	{
		dwQuality |= OPC_QUALITY_GOOD;
	}
	else
	{
		dwQuality |= OPC_QUALITY_SUB_NORMAL;
	}

	double dblValue = 0;

	// use linear interpolation.
	if (!m_bStepped)
	{
		double dx = (cUpper.dblValue - cLower.dblValue);
		double dt = (double)(cUpper.llTimestamp - cLower.llTimestamp);
		double t  = (double)(llTime - cLower.llTimestamp);

		dblValue = ((dx*t)/dt) + cLower.dblValue;
	}

	// use stepped interpolation.
	else
	{
		dblValue = cLower.dblValue;
	}

	// override the quality if values had to be skipped to find the bounds.
	if (bValuesSkipped)
	{
		dwQuality = OPCHDA_INTERPOLATED | OPC_QUALITY_SUB_NORMAL;
	}

	// return value.
	return dblValue;
}

// TimeAverage
double COpcHdaItem::TimeAverage(LONGLONG llStartBound, LONGLONG llEndBound, DWORD& dwQuality)
{	
	// swap the start and end times if end is earlier than start.
	if (llStartBound > llEndBound)
	{
		LONGLONG llTime = llEndBound;
		llEndBound = llStartBound;
		llStartBound = llTime;
	}

	// use interpolation to find the value at the start.
	double dblStart = Interpolate(llStartBound, dwQuality);

	// no start an the start but it is still possible to calculate a partial aggregate.
	bool bNoStartValue = false;
	bool bPartial      = false;

	if (OPC_MASK_ISSET(dwQuality, OPCHDA_NODATA))
	{
		dwQuality = OPCHDA_PARTIAL | OPC_QUALITY_SUB_NORMAL;
		bNoStartValue = true;
		bPartial = true;
	}

	// preserve the data quality bits for the start value.
	else 
	{
		dwQuality = (OPCHDA_CALCULATED | (0x0000FFFF & dwQuality));
	}

	// get index of first data point in interval.
	UINT uIndex = m_cValues.FindIndexAfter(llStartBound);

	if (uIndex == -1)
	{
		// no data if no start bound either.
		if (bNoStartValue)
		{
			dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
			return 0;
		}

		// entire interval is after the last data point - extrapolate the start value.
		dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_SUB_NORMAL;
		return dblStart;
	}

	// the interval is calculated from the first valid data point.
	double dblInterval = ((double)(llEndBound - llStartBound))/TICKS_PER_SECOND;

	// find all useable raw data points in the the interval.
	bool bValuesSkipped = false;		

	LONGLONG llLastTime = llStartBound;
	double dblLastValue = dblStart;
		
	double dblTotal = 0;

	while (m_cValues.IsValidIndex(uIndex) && m_cValues[uIndex].llTimestamp < llEndBound)
	{
		if (!IsUseableData(uIndex))
		{
			uIndex++;
			bValuesSkipped = true;
			continue;
		}

		// calculate the time average as soon as a valid start value is found.
		if (!bNoStartValue)
		{
			double dblSpan = ((double)(m_cValues[uIndex].llTimestamp - llLastTime))/TICKS_PER_SECOND;
			double dblRise = m_cValues[uIndex].dblValue - dblLastValue;

			if (dblSpan > 0)
			{
				dblTotal += dblRise*dblSpan/2 + dblLastValue*dblSpan;
			}
		}

		// shorten the interval so it starts from the first good value.
		else
		{
			dblInterval = ((double)(llEndBound - m_cValues[uIndex].llTimestamp))/TICKS_PER_SECOND;
		}

		// uses the current value as the base value for the next interval.
		llLastTime   = m_cValues[uIndex].llTimestamp;
		dblLastValue = m_cValues[uIndex].dblValue;
		bNoStartValue = false;

		uIndex++;
	}

	// no start value and no good data in the interval.
	if (bNoStartValue)
	{
		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return 0;
	}

	// went past the end of the data - extrapolate the last useable data.
	if (!m_cValues.IsValidIndex(uIndex))
	{
		// add final area.
		dblTotal += dblLastValue*(((double)(llEndBound - llLastTime))/TICKS_PER_SECOND);
		
		// set quality.
		dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_SUB_NORMAL;
	}
	
	// use the intepolated end value to calculate the final area.
	else
	{
		DWORD dwEndQuality = 0;
		double dbEndValue = Interpolate(llEndBound, dwEndQuality);

		// check if good quality is overridden by an uncertain quality at the end point.
		if (!OPC_MASK_ISSET(dwQuality, OPC_QUALITY_GOOD) || !OPC_MASK_ISSET(dwEndQuality, OPC_QUALITY_GOOD))
		{
			dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_SUB_NORMAL;
		}

		// add final area.
		double dblSpan = ((double)(llEndBound - llLastTime))/TICKS_PER_SECOND;
		double dblRise = dbEndValue - dblLastValue;

		if (dblSpan > 0)
		{
			dblTotal += dblRise*dblSpan/2 + dblLastValue*dblSpan;
		}
	}

	// divide area under curve by interval. 
	dblTotal /= dblInterval;
	
	// set partial quality bit.
	if (bPartial && OPC_MASK_ISSET(dwQuality, OPCHDA_CALCULATED))
	{
		dwQuality = OPCHDA_PARTIAL | (0x0000FFFF & dwQuality);
	}

	return dblTotal;
}

// Total
double COpcHdaItem::Total(LONGLONG llStartBound, LONGLONG llEndBound, DWORD& dwQuality)
{
	// calculate the time average.
	double dblValue = TimeAverage(llStartBound, llEndBound, dwQuality);

	double dblInterval = ((double)(llEndBound - llStartBound))/TICKS_PER_SECOND;

	// must determine the actual interval.
	if (OPC_MASK_ISSET(dwQuality, OPCHDA_PARTIAL))
	{
		// swap bounds if start time is larger than end time.
		if (llStartBound > llEndBound)
		{
			LONGLONG llTimestamp = llStartBound;
			llStartBound = llEndBound;
			llEndBound = llTimestamp;
		}

		// find the first good value.
		UINT uIndex = m_cValues.FindIndexAfter(llStartBound);

		while (m_cValues.IsValidIndex(uIndex) && m_cValues[uIndex].llTimestamp < llEndBound)
		{		
			if (IsUseableData(uIndex))
			{				
				dblInterval = ((double)(llEndBound - m_cValues[uIndex].llTimestamp))/TICKS_PER_SECOND;
				break;
			}
			
			uIndex++;
		}
	}
	
	// multiple by the interval in seconds.
	dblValue *= dblInterval;
	
	// return result.
	return dblValue;
}

// FindRawValues
bool COpcHdaItem::FindRawValues(LONGLONG llStartTime, LONGLONG llEndTime, COpcList<COpcHdaItemValue*>& cValues)
{
	// determine if series is ascending or descending.
	bool bAscending = (llStartTime < llEndTime);

	// get lower and upper bounding times for the series.
	LONGLONG llLower = (bAscending)?llStartTime:llEndTime;
	LONGLONG llUpper = (bAscending)?llEndTime:llStartTime;

	// find indicies.
	UINT uLower = m_cValues.FindIndexAfter(llLower);
	UINT uUpper = m_cValues.FindIndexBefore(llUpper);

	// exclude values that are on the end bound.
	if (bAscending)
	{
		while (m_cValues.IsValidIndex(uUpper) && m_cValues[uUpper].llTimestamp >= llUpper)
		{
			uUpper--;
		}
	}
	else
	{
		while (m_cValues.IsValidIndex(uLower) && m_cValues[uLower].llTimestamp <= llLower)
		{
			uLower++;
		}
	}

	// check for data within range.
	if (uLower == -1 || uUpper == -1 || uLower > uUpper)
	{
		return false;
	}

	// add values all values within interval.
	for (UINT ii = uLower; ii <= uUpper; ii++)
	{
		cValues.AddTail(&m_cValues[ii]);
	}

	// all done.
	return true;
}

// FindUsableValues
DWORD COpcHdaItem::FindUseableValues(LONGLONG llStartTime, LONGLONG llEndTime, COpcList<COpcHdaItemValue*>& cValues)
{
	// find raw values in interval.
	COpcList<COpcHdaItemValue*> cRawValues;

	if (!FindRawValues(llStartTime, llEndTime, cRawValues))
	{
		return OPCHDA_NODATA | OPC_QUALITY_BAD;
	}

	// initialize quality.
	DWORD dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_GOOD;

	// remove bad values.
	while (cRawValues.GetCount() > 0)
	{
		COpcHdaItemValue* pValue = cRawValues.RemoveHead();

		if (!IsUseableData(*pValue))
		{
			dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_SUB_NORMAL;
			continue;
		}

		cValues.AddTail(pValue);
	}

	// no usable data found.
	if (cValues.GetCount() == 0)
	{
		return OPCHDA_NODATA | OPC_QUALITY_BAD;
	}

	// check for partial intervals.
	if (llStartTime < m_cValues[m_uFirstUseable].llTimestamp)
	{
		dwQuality = OPCHDA_PARTIAL | (0x0000FFFF & dwQuality);
	}

	// all done.
	return dwQuality;
}

// Average
double COpcHdaItem::Average(LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality)
{
	// find usable values in interval.
	COpcList<COpcHdaItemValue*> cValues;

	dwQuality = FindUseableValues(llStartTime, llEndTime, cValues);

	if (OPC_MASK_ISSET(dwQuality, OPCHDA_NODATA))
	{
		return 0;
	}

	// calculate sum and count.
	double dblSum   = 0;
	double dblCount = 0;
	
	while (cValues.GetCount() > 0)
	{
		COpcHdaItemValue* pValue = cValues.RemoveHead();

		dblSum += pValue->dblValue;
		dblCount++;
	}

	// should never happen.
	if (dblCount == 0)
	{
		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return 0;
	}

	// calculate average.
	return dblSum/dblCount;
}

// Count
double COpcHdaItem::Count(LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality)
{
	// find raw values in interval.
	COpcList<COpcHdaItemValue*> cValues;

	if (!FindRawValues(llStartTime, llEndTime, cValues))
	{
		dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_GOOD;
		return 0;
	}

	// initialize quality.
	dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_GOOD;

	// count the number of good values.
	double dblCount = 0;
	
	while (cValues.GetCount() > 0)
	{
		COpcHdaItemValue* pValue = cValues.RemoveHead();

		if (!IsUseableData(*pValue))
		{
			dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_SUB_NORMAL;
			continue;
		}

		dblCount++;
	}

	// calculate average.
	return dblCount;
}

// StdDev
double COpcHdaItem::StdDev(bool bVariance, LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality)
{
	// find usable values in interval.
	COpcList<COpcHdaItemValue*> cValues;

	dwQuality = FindUseableValues(llStartTime, llEndTime, cValues);

	if (OPC_MASK_ISSET(dwQuality, OPCHDA_NODATA))
	{
		return 0;
	}

	// should never happen.
	if (cValues.GetCount() == 0)
	{
		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return 0;
	}

	// calculate sum and count.
	double dblSum   = 0;
	double dblCount = 0;

	OPC_POS pos = cValues.GetHeadPosition();

	while (pos != NULL)
	{
		COpcHdaItemValue* pValue = cValues.GetNext(pos);
		
		dblSum += pValue->dblValue;
		dblCount++;
	}

	// check for trivial case.
	if (dblCount == 1)
	{
		dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_GOOD;
		return 0;
	}

	double dblAverage = dblSum/dblCount;
	
	dblSum = 0;

	// calculate sum of differences
	while (cValues.GetCount() > 0)
	{
		COpcHdaItemValue* pValue = cValues.RemoveHead();

		dblSum += pow((pValue->dblValue-dblAverage), 2);
	}

	// return the variance.
	if (bVariance)
	{
		return dblSum/(dblCount-1);
	}

	// calculate standard deviation.
	return sqrt(dblSum/(dblCount-1));
}

// MinMax
double COpcHdaItem::MinMax(
    bool      bMinimum,
	LONGLONG  llStartTime, 
	LONGLONG  llEndTime, 
	DWORD&    dwQuality,
	FILETIME* pftTimestamp
)
{
	// find usable values in interval.
	COpcList<COpcHdaItemValue*> cValues;

	dwQuality = FindRawValues(llStartTime, llEndTime, cValues);

	if (OPC_MASK_ISSET(dwQuality, OPCHDA_NODATA))
	{
		return 0;
	}

	// should never happen.
	if (cValues.GetCount() == 0)
	{
		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return 0;
	}

	// initialize quality.
	dwQuality = OPCHDA_RAW | OPC_QUALITY_GOOD;

	// find minimum value.

	COpcHdaItemValue* pGood = NULL;
	COpcHdaItemValue* pBad  = NULL;

	bool bExtraData = false;

	while (cValues.GetCount() > 0)
	{
		COpcHdaItemValue* pValue = cValues.RemoveHead();

		// check for bad data.
		if (!IsUseableData(*pValue))
		{
			// no data values cannot be used in a comparison.
			if (!OPC_MASK_ISSET(pValue->dwQuality, OPCHDA_NODATA))
			{
				if ((pBad == NULL) || (bMinimum && pValue->dblValue < pBad->dblValue) || (!bMinimum && pValue->dblValue > pBad->dblValue))
				{
					pBad = pValue;
				}
			}

			continue;
		}

		// check if the current value is a new minimum/maximum.
		if ((pGood == NULL) || (bMinimum && pValue->dblValue < pGood->dblValue) || (!bMinimum && pValue->dblValue > pGood->dblValue))
		{
			pGood      = pValue;
			bExtraData = false;
		}

		// check if the current value is equal to the minimum/maximum.
		else if (pValue->dblValue == pGood->dblValue)
		{
			bExtraData  = true;
		}
	}
	
	// check if no good data exists - use bad value instead.
	if (pGood == NULL)
	{
		pGood = pBad;
	
	}
	// should never happen.
	if (pGood == NULL)
	{
		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return 0;
	}

	// initialize quality.
	dwQuality = pGood->dwQuality;

	// check if a bad value exists and exceeded the good value.
	if (pBad != NULL)
	{
		if ((bMinimum && pBad->dblValue < pGood->dblValue) || (!bMinimum && pBad->dblValue > pGood->dblValue))
		{
			dwQuality = OPCHDA_RAW | OPC_QUALITY_SUB_NORMAL;
		}
	}

	// return actual time if required.
	if (pftTimestamp != NULL)
	{
		if (bExtraData)
		{
			dwQuality = (OPCHDA_EXTRADATA | (0x0000FFFF & dwQuality));
		}

		*pftTimestamp = OpcHdaFILETIMEFromInt64(pGood->llTimestamp);
	}

	// use start of interval.
	else
	{
		if (pGood->llTimestamp == llStartTime)
		{
			dwQuality = (OPCHDA_RAW | (0x0000FFFF & dwQuality));
		}
		else
		{
			dwQuality = (OPCHDA_CALCULATED | (0x0000FFFF & dwQuality));
		}
	}

	// return minimum value.
	return pGood->dblValue;
}

// StartEnd
double COpcHdaItem::StartEnd(
    bool      bStart,
	LONGLONG  llStartTime, 
	LONGLONG  llEndTime, 
	DWORD&    dwQuality,
	FILETIME& ftTimestamp
)
{
	// find usable values in interval.
	COpcList<COpcHdaItemValue*> cValues;

	dwQuality = FindRawValues(llStartTime, llEndTime, cValues);

	if (OPC_MASK_ISSET(dwQuality, OPCHDA_NODATA))
	{
		return 0;
	}

	// get the start or end raw value.
	COpcHdaItemValue* pValue = NULL;
	
	while (cValues.GetCount() > 0 && pValue == NULL)
	{
		pValue = (bStart)?cValues.RemoveHead():cValues.RemoveTail();

		if (OPC_MASK_ISSET(pValue->dwQuality, OPCHDA_NODATA))
		{
			pValue = NULL;
		}
	}

	// no data found in the interval.
	if (pValue == NULL)
	{
		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return 0;
	}

	// set raw quality and timestamp.
	dwQuality   = pValue->dwQuality;
	ftTimestamp = OpcHdaFILETIMEFromInt64(pValue->llTimestamp);

	// return value.
	return pValue->dblValue;
}

// Regression
double COpcHdaItem::Regression(bool bSlope, LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality)
{
	// calculate linear regression.
	double dblSlope    = 0;
	double dblConstant = 0;

	dwQuality = LinearRegression(llStartTime, llEndTime, dblSlope, dblConstant);

	// return requested value.
	return (bSlope)?dblSlope:dblConstant;
}

// RegressionDeviation
double COpcHdaItem::RegressionDeviation(LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality)
{
	// calculate linear regression.
	double A = 0;
	double B = 0;

	// determine if series is ascending or descending.
	bool bAscending = ((llStartTime != 0 && llStartTime <= llEndTime) || llEndTime == 0);

	// get lower and upper bounding times for the series.
	LONGLONG llLower = (bAscending)?llStartTime:llEndTime;
	LONGLONG llUpper = (bAscending)?llEndTime:llStartTime;

	dwQuality = LinearRegression(llStartTime, llEndTime, A, B);

	// check for set of unchanging points - this implies a zero error term.
	if (A == 0)
	{
		return 0;
	}	
	
	// find usable values in interval.
	COpcList<COpcHdaItemValue*> cValues;

	dwQuality = FindUseableValues(llStartTime, llEndTime, cValues);

	if (cValues.GetCount() == 0)
	{
		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return 0;
	}

	// calculate the total error term.
	double dblError = 0;
	double dblCount = 0;

	while (cValues.GetCount() > 0)
	{
		COpcHdaItemValue* pValue = cValues.RemoveHead();
		
		// get next point.
		double x = ((double)(pValue->llTimestamp - llLower))/TICKS_PER_SECOND;
		double y = pValue->dblValue;

		// add to sums.
		dblError += pow((y - (A*x + B)), 2);
		dblCount += 1;
	}

	// can be no error for regressions calculted with 2 or fewer points.
	if (dblCount < 3)
	{
		return 0;
	}

	// return square root of the error term.
	return sqrt(dblError/(dblCount-2));
}

// LinearRegression
DWORD COpcHdaItem::LinearRegression(
	LONGLONG llStartTime, 
	LONGLONG llEndTime, 
	double&  dblSlope,
	double&  dblConstant)
{	
	dblSlope    = 0;
	dblConstant = 0;

	// determine if series is ascending or descending.
	bool bAscending = ((llStartTime != 0 && llStartTime <= llEndTime) || llEndTime == 0);

	// get lower and upper bounding times for the series.
	LONGLONG llLower = (bAscending)?llStartTime:llEndTime;
	LONGLONG llUpper = (bAscending)?llEndTime:llStartTime;

	// find usable values in interval.
	COpcList<COpcHdaItemValue*> cValues;

	DWORD dwQuality = FindUseableValues(llStartTime, llEndTime, cValues);

	if (cValues.GetCount() == 0)
	{
		return OPCHDA_NODATA | OPC_QUALITY_BAD;
	}
	
	// handle the special case of one point.
	if (cValues.GetCount() == 1)
	{
		return OPCHDA_CALCULATED | OPC_QUALITY_GOOD;
	}

	//==========================================
	// Linear regression uses the formula:
	//
	// Slope    = (XY*N -  X*Y)/(XX*N - X*X)
	// Constant = (XX*Y - XY*X)/(XX*N - X*X)
	//
	// Where X  = Sum of Timestamps;
	//       Y  = Sum of Values;
	//       XY = Sum of Timestamp*Value;
	//       XX = Sum of Timestamp*Timestamp;
	//       N  = Number of Values;
	//==========================================

	double X  = 0;
	double Y  = 0;
	double XY = 0;
	double XX = 0;
	double N  = 0;

	while (cValues.GetCount() > 0)
	{
		COpcHdaItemValue* pValue = cValues.RemoveHead();
		
		// get next point.
		double x = ((double)(pValue->llTimestamp-llLower))/TICKS_PER_SECOND;
		double y = pValue->dblValue;

		// add to sums.
		X  += x;
		Y  += y;
		XX += x*x;
		XY += x*y;
		N  += 1;
	}

	// check if regression can be calculated.
	double dblDivisor = (XX*N - X*X);

	if (dblDivisor == 0)
	{
		dblSlope    = 0;
		dblConstant = (double)0xFFF8000000000000;
		return dwQuality;
	}

	// calculate the slope and constant.
	dblSlope    = (XY*N -  X*Y)/(XX*N - X*X);
	dblConstant = (XX*Y - XY*X)/(XX*N - X*X);

	// return quality.
	return dwQuality;
}

// Delta
double COpcHdaItem::Delta(LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality)
{	
	// find raw values in interval.
	COpcList<COpcHdaItemValue*> cValues;

	dwQuality = FindRawValues(llStartTime, llEndTime, cValues);

	if (cValues.GetCount() == 0)
	{
		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return 0;
	}

	bool bValuesSkipped = false;

	// find the start value.
	COpcHdaItemValue* pHead = NULL;

	while (cValues.GetCount() > 0)
	{
		COpcHdaItemValue* pValue = cValues.RemoveHead();

		if (IsUseableData(*pValue))
		{
			pHead = pValue;
			break;
		}

		bValuesSkipped = true;
	}

	// find the last value.
	COpcHdaItemValue* pTail = pHead;

	while (cValues.GetCount() > 0)
	{
		COpcHdaItemValue* pValue = cValues.RemoveTail();

		if (IsUseableData(*pValue))
		{
			pTail = pValue;
			break;
		}

		bValuesSkipped = true;
	}

	// no good data found.
	if (pHead == NULL || pTail == NULL)
	{
		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return 0;
	}

	// initialize the quality.
	dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_GOOD;

	if (bValuesSkipped)
	{
		dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_SUB_NORMAL;
	}

	// check for partial intervals.
	if (llStartTime < m_cValues[m_uFirstUseable].llTimestamp)
	{
		dwQuality = OPCHDA_PARTIAL | (0x0000FFFF & dwQuality);
	}

	// return the difference between the end and the start.
	return (pTail->dblValue - pHead->dblValue);
}

// Range
double COpcHdaItem::Range(LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality)
{	
	// find usable values in interval.
	COpcList<COpcHdaItemValue*> cValues;

	dwQuality = FindUseableValues(llStartTime, llEndTime, cValues);

	if (cValues.GetCount() == 0)
	{
		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return 0;
	}

	// find the min and max.
	COpcHdaItemValue* pMin = NULL;
	COpcHdaItemValue* pMax = NULL;

	while (cValues.GetCount() > 0)
	{
		COpcHdaItemValue* pValue = cValues.RemoveHead();

		// check for new minimum.
		if (pMin == NULL || pMin->dblValue > pValue->dblValue)
		{
			pMin = pValue;
		}
				
		// check for new maximum.
		if (pMax == NULL || pMax->dblValue < pValue->dblValue)
		{
			pMax = pValue;
		}
	}

	// check for valid data.
	if (pMin == NULL || pMax == NULL)
	{
		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return 0;
	}

	// return the difference between the minium and the maximum.
	return (pMax->dblValue - pMin->dblValue);
}

// calculates the time or percentage of an interval that is either good/bad.
double COpcHdaItem::Duration(
	bool      bGood,
	bool      bPercent,
	LONGLONG  llStartTime, 
	LONGLONG  llEndTime, 
	DWORD&    dwQuality
)
{
	double dblGood = 0;
	double dblBad  = 0;

	// initialize quality.
	dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_GOOD;

	// determine if series is ascending or descending.
	bool bAscending = ((llStartTime != 0 && llStartTime <= llEndTime) || llEndTime == 0);

	// get lower and upper bounding times for the series.
	LONGLONG llLower = (bAscending)?llStartTime:llEndTime;
	LONGLONG llUpper = (bAscending)?llEndTime:llStartTime;

	// find indicies.
	UINT uLower = m_cValues.FindIndexAfter(llLower);
	UINT uUpper = m_cValues.FindIndexBefore(llUpper);

	// check for range out of bounds.
	if (uLower == -1 || uUpper == -1)
	{
		if ((bAscending && uUpper == -1) || (!bAscending && uLower == -1))
		{
			dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
			return 0;
		}

		UINT uBound = (uUpper != -1)?uUpper:uLower;

		if (OPC_MASK_ISSET(m_cValues[uBound].dwQuality, OPC_QUALITY_GOOD))
		{
			dblGood = (double)(llUpper - llLower);
		}	

		if (OPC_MASK_ISSET(~m_cValues[uBound].dwQuality, OPC_QUALITY_GOOD))
		{
			dblBad = (double)(llUpper - llLower);
		}
	}

	// calculate durations.
	else if (bAscending)
	{		
		// find bounding value at the start of the interval.
		DWORD    dwLastQuality   = OPC_QUALITY_GOOD;
		LONGLONG llLastTimestamp = llStartTime;

		if (m_cValues[uLower].llTimestamp > llStartTime)
		{
			dwLastQuality = (uLower > 0)?m_cValues[uLower-1].dwQuality:OPC_QUALITY_BAD;
		}

		// move through each point in the interval.
		for (UINT ii = uLower; ii <= uUpper; ii++)
		{
			// check if range is good.
			if (OPC_MASK_ISSET(dwLastQuality, OPC_QUALITY_GOOD))
			{
				dblGood += (double)(m_cValues[ii].llTimestamp - llLastTimestamp);
			}

			// check if range is bad.
			if (OPC_MASK_ISSET(~dwLastQuality, OPC_QUALITY_GOOD))
			{
				dblBad += (double)(m_cValues[ii].llTimestamp - llLastTimestamp);
			}

			dwLastQuality   = m_cValues[ii].dwQuality; 
			llLastTimestamp = m_cValues[ii].llTimestamp;
		}

		// add remainder at end.
		if (llLastTimestamp < llEndTime)
		{
			// check if range is good.
			if (OPC_MASK_ISSET(dwLastQuality, OPC_QUALITY_GOOD))
			{
				dblGood += (double)(llEndTime - llLastTimestamp);
			}

			// check if range is bad.
			if (OPC_MASK_ISSET(~dwLastQuality, OPC_QUALITY_GOOD))
			{
				dblBad += (double)(llEndTime - llLastTimestamp);
			}
		}
	}
	else
	{
		// find bounding value at the start of the interval.
		DWORD    dwLastQuality   = OPC_QUALITY_GOOD;
		LONGLONG llLastTimestamp = llStartTime;

		if (m_cValues[uUpper].llTimestamp < llStartTime)
		{
			dwLastQuality = (uUpper < m_cValues.GetCount()-1)?m_cValues[uUpper+1].dwQuality:OPC_QUALITY_BAD;
		}

		// move through each point in the interval.
		for (UINT ii = uUpper; ii >= uLower && ii != -1; ii--)
		{
			// check if range is good.
			if (OPC_MASK_ISSET(dwLastQuality, OPC_QUALITY_GOOD))
			{
				dblGood += (double)(llLastTimestamp - m_cValues[ii].llTimestamp);
			}

			// check if range is bad.
			if (OPC_MASK_ISSET(~dwLastQuality, OPC_QUALITY_GOOD))
			{
				dblBad += (double)(llLastTimestamp - m_cValues[ii].llTimestamp);
			}

			dwLastQuality   = m_cValues[ii].dwQuality; 
			llLastTimestamp = m_cValues[ii].llTimestamp;
		}

		// add remainder at end.
		if (llLastTimestamp > llEndTime)
		{
			// check if range is good.
			if (OPC_MASK_ISSET(dwLastQuality, OPC_QUALITY_GOOD))
			{
				dblGood += (double)(llLastTimestamp - llEndTime);
			}

			// check if range is bad.
			if (OPC_MASK_ISSET(~dwLastQuality, OPC_QUALITY_GOOD))
			{
				dblBad += (double)(llLastTimestamp - llEndTime);
			}
		}
	}

	// calculate requested values.
	if (bPercent)
	{
		dblGood /= (double)(llUpper - llLower);
		dblBad  /= (double)(llUpper - llLower);
	}
	else
	{
		dblGood /= (double)TICKS_PER_SECOND;
		dblBad  /= (double)TICKS_PER_SECOND;
	}

	// return requested value.
	return (bGood)?dblGood:dblBad;
}

// WorstQuality
double COpcHdaItem::WorstQuality(LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality)
{
	// initialize quality.
	dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_GOOD;

	// find raw values in interval.
	COpcList<COpcHdaItemValue*> cValues;

	if (!FindRawValues(llStartTime, llEndTime, cValues))
	{
		dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;
		return 0;
	}

	WORD wWorstQuality = OPC_QUALITY_GOOD;

	while (cValues.GetCount() > 0)
	{
		COpcHdaItemValue* pValue = cValues.RemoveHead();

		// check if quality is bad.
		if (OPC_MASK_ISSET(~pValue->dwQuality, OPC_QUALITY_GOOD))
		{
			return OPC_QUALITY_BAD;
		}

		// check if quality is uncertain.
		if (!OPC_MASK_ISSET(pValue->dwQuality, OPC_QUALITY_GOOD))
		{
			wWorstQuality = OPC_QUALITY_UNCERTAIN;
		}
	}

	// return result.
	return wWorstQuality;
}

// returns the number of annotations in the interval.
double COpcHdaItem::AnnotationCount(LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality)
{
	// initialize quality.
	dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_GOOD;

	// determine if series is ascending or descending.
	bool bAscending = ((llStartTime != 0 && llStartTime <= llEndTime) || llEndTime == 0);

	// get lower and upper bounding times for the series.
	LONGLONG llLower = (bAscending)?llStartTime:llEndTime;
	LONGLONG llUpper = (bAscending)?llEndTime:llStartTime;

	// find bounding indices.
	UINT uLower = m_cAnnotations.FindIndexAfter(llLower);
	UINT uUpper = m_cAnnotations.FindIndexBefore(llUpper);

	// no annotations found.
	if (uLower == -1 || uUpper == -1)
	{
		return 0;
	}

	// return count.
	return (uUpper - uLower + 1);
}

// ReadProcessed
HRESULT COpcHdaItem::ReadProcessed(
	LONGLONG     llStartTime,
	LONGLONG     llEndTime,
	LONGLONG     llResampleInterval,
	DWORD        haAggregate, 
	OPCHDA_ITEM& cItem
)
{
	// ensure data is loaded for the item.
	LoadData();

	LONGLONG llDelta = (llEndTime < llStartTime)?-llResampleInterval:+llResampleInterval;

	LONGLONG llStartBound = llStartTime;
	LONGLONG llEndBound   = llStartTime + llDelta;

	UINT uNumValues = 1;

	// a resample interval of zero is a special case.
	if (llResampleInterval == 0)
	{
		llEndBound = llEndTime;
		uNumValues = 1;
	}

	// calculate number of values.
	else
	{
		LONGLONG llUpperBound = (llEndTime > llStartTime)?llEndTime:llStartTime;
		LONGLONG llLowerBound = (llEndTime < llStartTime)?llEndTime:llStartTime;

		uNumValues = (UINT)(((double)(llUpperBound - llLowerBound))/llResampleInterval);

		// add a partial interval at the if range is not evenly divisible by the interval.
		if ((llUpperBound - llLowerBound)%llResampleInterval != 0)
		{
			uNumValues++;
		}
	}

	// initialize item object.
	cItem.dwCount     = uNumValues;
	cItem.haAggregate = haAggregate;
	cItem.hClient     = NULL;

	// allocate arrays.
	cItem.pvDataValues  = OpcArrayAlloc(VARIANT, uNumValues);
	cItem.pdwQualities  = OpcArrayAlloc(DWORD, uNumValues);
	cItem.pftTimeStamps = OpcArrayAlloc(FILETIME, uNumValues);

	memset(cItem.pvDataValues, 0, sizeof(VARIANT)*uNumValues);
	memset(cItem.pdwQualities, 0, sizeof(DWORD)*uNumValues);
	memset(cItem.pftTimeStamps, 0, sizeof(FILETIME)*uNumValues);

	// calculate values.
	HRESULT hResult = S_OK;

	for (UINT ii = 0; ii < uNumValues; ii++)
	{
		// check for partial aggregate.
		bool bPartial = false;

		if ((llEndTime > llStartTime && llEndBound > llEndTime) || (llEndTime < llStartTime && llEndBound < llEndTime))
		{
			llEndBound = llEndTime;
			bPartial   = true;
		}

		// default timestamp is the beginning of the interval. 
		cItem.pftTimeStamps[ii] = OpcHdaFILETIMEFromInt64(llStartBound);

		double dblValue  = 0;
		DWORD  dwQuality = OPCHDA_NODATA | OPC_QUALITY_BAD;

		// calculate aggregate for the interval.
		switch (haAggregate)
		{
			case OPCHDA_INTERPOLATIVE:
			{ 
				dblValue = Interpolate(llStartBound, dwQuality);
				break;
			}

			case OPCHDA_TIMEAVERAGE:
			{ 
				dblValue = TimeAverage(llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_TOTAL:
			{ 
				dblValue = Total(llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_AVERAGE:
			{ 
				dblValue = Average(llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_COUNT:
			{ 
				dblValue = Count(llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_STDEV:
			{ 
				dblValue = StdDev(false, llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_VARIANCE:
			{ 
				dblValue = StdDev(true, llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_MINIMUM:
			{ 
				dblValue = MinMax(true, llStartBound, llEndBound, dwQuality, NULL);
				break;
			}
			
			case OPCHDA_MAXIMUM:
			{ 
				dblValue = MinMax(false, llStartBound, llEndBound, dwQuality, NULL);
				break;
			}

			case OPCHDA_MINIMUMACTUALTIME:
			{ 
				dblValue = MinMax(true, llStartBound, llEndBound, dwQuality, &cItem.pftTimeStamps[ii]);
				break;
			}
			
			case OPCHDA_MAXIMUMACTUALTIME:
			{ 
				dblValue = MinMax(false, llStartBound, llEndBound, dwQuality, &cItem.pftTimeStamps[ii]);
				break;
			}

			case OPCHDA_START:
			{ 
				dblValue = StartEnd(true, llStartBound, llEndBound, dwQuality, cItem.pftTimeStamps[ii]);
				break;
			}
			
			case OPCHDA_END:
			{ 
				dblValue = StartEnd(false, llStartBound, llEndBound, dwQuality, cItem.pftTimeStamps[ii]);
				break;
			}
			
			case OPCHDA_DELTA:
			{ 
				dblValue = Delta(llStartBound, llEndBound, dwQuality);
				break;
			}
			
			case OPCHDA_REGSLOPE:
			{ 
				dblValue = Regression(true, llStartBound, llEndBound, dwQuality);
				break;
			}
			
			case OPCHDA_REGCONST:
			{ 
				dblValue = Regression(false, llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_REGDEV:
			{ 
				dblValue = RegressionDeviation(llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_RANGE:
			{ 
				dblValue = Range(llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_DURATIONGOOD:
			{ 
				dblValue = Duration(true, false, llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_DURATIONBAD:
			{ 
				dblValue = Duration(false, false, llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_PERCENTGOOD:
			{ 
				dblValue = Duration(true, true, llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_PERCENTBAD:
			{ 
				dblValue = Duration(false, true, llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_WORSTQUALITY:
			{ 
				dblValue = WorstQuality(llStartBound, llEndBound, dwQuality);
				break;
			}

			case OPCHDA_ANNOTATIONS:
			{ 
				dblValue = AnnotationCount(llStartBound, llEndBound, dwQuality);
				break;
			}

			default:
			{
				dwQuality = OPCHDA_CALCULATED | OPC_QUALITY_BAD;
				dblValue  = 0;
				break;
			}	
		}

		// always return VT_EMPTY is no data found.
		if (OPC_MASK_ISSET(dwQuality, OPCHDA_NODATA))
		{
			cItem.pvDataValues[ii].vt = VT_EMPTY;
		}

		// convert double to item's canonical data type.
		else
		{
			cItem.pvDataValues[ii] = GetValue(dblValue, dwQuality);
		}

		// add partial flag to quality.
		if (bPartial)
		{
			dwQuality = OPCHDA_PARTIAL | (0x0000FFFF & dwQuality);
		}

		// set the item quality.
		cItem.pdwQualities[ii] = dwQuality;

		// move to next interval.
		llStartBound += llDelta;
		llEndBound   += llDelta;
	}

	// return result.
	return hResult;
}

// ReadAtTime
HRESULT COpcHdaItem::ReadAtTime(
	COpcArray<LONGLONG>& cTimestamps,
	OPCHDA_ITEM&         cItem
)
{
	// ensure data is loaded for the item.
	LoadData();

	// initialize empty item object.
	cItem.dwCount     = cTimestamps.GetSize();
	cItem.haAggregate = OPCHDA_NOAGGREGATE;
	cItem.hClient     = NULL;

	// allocate arrays.
	cItem.pvDataValues  = OpcArrayAlloc(VARIANT, cItem.dwCount);
	cItem.pdwQualities  = OpcArrayAlloc(DWORD, cItem.dwCount);
	cItem.pftTimeStamps = OpcArrayAlloc(FILETIME, cItem.dwCount);

	memset(cItem.pvDataValues, 0, sizeof(VARIANT)*cItem.dwCount);
	memset(cItem.pdwQualities, 0, sizeof(DWORD)*cItem.dwCount);
	memset(cItem.pftTimeStamps, 0, sizeof(FILETIME)*cItem.dwCount);

	HRESULT hResult = S_OK;

	bool bDataFound = false;

	for (UINT ii = 0; ii < cItem.dwCount; ii++)
	{
		cItem.pvDataValues[ii].vt = VT_EMPTY;

		// fetch the raw or interpolated value for the aggregate.
		double dblValue = Interpolate(cTimestamps[ii], cItem.pdwQualities[ii]);

		if (!OPC_MASK_ISSET(cItem.pdwQualities[ii], OPCHDA_NODATA))
		{
			cItem.pvDataValues[ii] = GetValue(dblValue, cItem.pdwQualities[ii]);
		}

		// set the timestamp.
		cItem.pftTimeStamps[ii] = OpcHdaFILETIMEFromInt64(cTimestamps[ii]);

		// set the no data flag.
		if (!OPC_MASK_ISSET(cItem.pdwQualities[ii], OPCHDA_NODATA))
		{
			bDataFound = true;
		}
	}

	// return result.
	return (bDataFound)?S_OK:OPC_S_NODATA;
}

// ReadModified
HRESULT COpcHdaItem::ReadModified(
	LONGLONG             llStartTime,
	LONGLONG             llEndTime,
	UINT                 uNumValues,
	UINT                 uStartIndex,
	OPCHDA_MODIFIEDITEM& cItem
)
{
	// ensure data is loaded for the item.
	LoadData();

	// initialize empty item object.
	cItem.dwCount             = 0;
	cItem.hClient             = NULL;
	cItem.pvDataValues        = NULL;
	cItem.pdwQualities        = NULL;
	cItem.pftTimeStamps       = NULL;
	cItem.pftModificationTime = NULL;
	cItem.pEditType           = NULL;
	cItem.szUser              = NULL;

	// determine if series is ascending or descending.
	bool bAscending = ((llStartTime != 0 && llStartTime <= llEndTime) || llEndTime == 0);

	// get lower and upper bounding times for the series.
	LONGLONG llLower = (bAscending)?llStartTime:llEndTime;
	LONGLONG llUpper = (bAscending)?llEndTime:llStartTime;

	if (llStartTime == 0)
	{
		llLower = 0;
		llUpper = llEndTime;
	}

	if (llEndTime == 0)
	{
		llUpper = _I64_MAX;
	}

	// find indicies.
	UINT uLower = m_cModifiedValues.FindIndexAfter(llLower);
	UINT uUpper = m_cModifiedValues.FindIndexBefore(llUpper);

	// adjust for start index.
	if (bAscending)
	{
		uLower += uStartIndex;
	}
	else
	{
		if (uStartIndex <= uUpper)
		{
			uUpper -= uStartIndex;
		}
		else
		{
			uUpper = -1;
		}
	}

	// check for valid indexes.
	if (uLower == -1 || uUpper == -1 || uLower > uUpper)
	{
		return OPC_S_NODATA;
	}

	HRESULT hResult = S_OK;

	// calculate number of values to return.
	cItem.dwCount = uUpper - uLower + 1;

	if (uNumValues != 0 && cItem.dwCount > uNumValues)
	{
		cItem.dwCount = uNumValues;
		
		if (llStartTime != 0 && llEndTime != 0)
		{
			hResult = OPC_S_MOREDATA;
		}
	}

	// allocate arrays.
	cItem.pvDataValues        = OpcArrayAlloc(VARIANT, cItem.dwCount);
	cItem.pdwQualities        = OpcArrayAlloc(DWORD, cItem.dwCount);
	cItem.pftTimeStamps       = OpcArrayAlloc(FILETIME, cItem.dwCount);
	cItem.pftModificationTime = OpcArrayAlloc(FILETIME, cItem.dwCount);
	cItem.pEditType           = OpcArrayAlloc(OPCHDA_EDITTYPE, cItem.dwCount);
	cItem.szUser              = OpcArrayAlloc(LPWSTR, cItem.dwCount);

	memset(cItem.pvDataValues, 0, sizeof(VARIANT)*cItem.dwCount);
	memset(cItem.pdwQualities, 0, sizeof(DWORD)*cItem.dwCount);
	memset(cItem.pftTimeStamps, 0, sizeof(FILETIME)*cItem.dwCount);
	memset(cItem.pftModificationTime, 0, sizeof(FILETIME)*cItem.dwCount);
	memset(cItem.pEditType, 0, sizeof(OPCHDA_EDITTYPE)*cItem.dwCount);
	memset(cItem.szUser, 0, sizeof(LPWSTR)*cItem.dwCount);

	// fill arrays.
	for (UINT ii = 0; ii < cItem.dwCount; ii++)
	{
		COpcHdaModifiedValue& cValue = m_cModifiedValues[((bAscending)?uLower+ii:uUpper-ii)];

		cItem.pdwQualities[ii]        = cValue.dwQuality;
		cItem.pvDataValues[ii]        = GetValue(cValue.dblValue, cItem.pdwQualities[ii]);
		cItem.pftTimeStamps[ii]       = OpcHdaFILETIMEFromInt64(cValue.llTimestamp);
		cItem.pftModificationTime[ii] = OpcHdaFILETIMEFromInt64(cValue.llModificationTime);
		cItem.pEditType[ii]           = cValue.eEditType;
		cItem.szUser[ii]              = OpcStrDup((LPCWSTR)cValue.cUser);
	}

	// return result.
	return hResult;
}

// GetCurrentValue
HRESULT COpcHdaItem::GetCurrentValue(OPCHDA_ATTRIBUTE& cAttribute)
{
	// get the current value.
	OpcXml::AnyType cCurrentValue;

	if (!GetAttribute(cAttribute.dwAttributeID, cCurrentValue))
	{
		return OPC_E_INVALIDATTRID;
	}

	cAttribute.dwNumValues      = 1;
	cAttribute.vAttributeValues = OpcArrayAlloc(VARIANT, cAttribute.dwNumValues);
	cAttribute.ftTimeStamps     = OpcArrayAlloc(FILETIME, cAttribute.dwNumValues);

	memset(cAttribute.vAttributeValues, 0, sizeof(VARIANT)*cAttribute.dwNumValues);
	memset(cAttribute.ftTimeStamps, 0, sizeof(FILETIME)*cAttribute.dwNumValues);

	// set timestamp.
	cAttribute.ftTimeStamps[0] = OpcUtcNow();

	// copy value.
	if (!cCurrentValue.Get(cAttribute.vAttributeValues[0]))
	{
		cAttribute.vAttributeValues[0].vt = VT_EMPTY;
		return E_FAIL;
	}

	// all done.
	return S_OK;
}

// ReadAttribute
HRESULT COpcHdaItem::ReadAttribute(
	LONGLONG          llStartTime,
	LONGLONG          llEndTime,
	DWORD             dwAttributeID, 
	OPCHDA_ATTRIBUTE& cAttribute
)
{
	// ensure data is loaded for the item.
	LoadData();

	// initialize empty item object.
	cAttribute.dwNumValues      = 0;
	cAttribute.dwAttributeID    = dwAttributeID;
	cAttribute.hClient          = NULL;
	cAttribute.vAttributeValues = NULL;
	cAttribute.ftTimeStamps     = NULL;

	// get the current time.
	LONGLONG llNow = OpcHdaInt64FromFILETIME(OpcUtcNow());

	// check for a request for the current value.
	if (llEndTime == 0 && ((llNow - llStartTime) < 10*TICKS_PER_SECOND))
	{
		return GetCurrentValue(cAttribute);
	}

	// determine if series is ascending or descending.
	bool bAscending = ((llStartTime != 0 && llStartTime <= llEndTime) || llEndTime == 0);

	// get lower and upper bounding times for the series.
	LONGLONG llLower = (bAscending)?llStartTime:llEndTime;
	LONGLONG llUpper = (bAscending)?llEndTime:llStartTime;

	if (llStartTime == 0)
	{
		llLower = 0;
		llUpper = llEndTime;
	}

	if (llEndTime == 0 || llUpper >= llNow)
	{
		llUpper = _I64_MAX;
	}

	// find indicies.
	UINT uLower = m_cAttributeValues.FindIndexAfter(llLower);
	UINT uUpper = m_cAttributeValues.FindIndexBefore(llUpper);

	// nothing after the lower bound and no upper bound - return the current value.
	if (uLower == -1 && uUpper != -1 && llUpper == _I64_MAX)
	{
		return GetCurrentValue(cAttribute);
	}

	// no historial data exists for the item. 
	if (uLower == -1 && uUpper == -1)
	{
		HRESULT hResult = GetCurrentValue(cAttribute);

		if (SUCCEEDED(hResult))
		{
			return OPC_S_CURRENTVALUE;
		}

		return hResult;
	}	

	// no data within the specified range.
	if (uUpper == -1)
	{
		return OPC_S_NODATA;
	}

	bool bNoDataAbove = (uLower == -1);

	// return value value before the lower bound.
	if (uLower == -1)
	{
		uLower = uUpper;
	}

	// count the number of values.
	COpcList<COpcHdaAttributeValue*> cValues;

	// find values within the range.
	for (UINT ii = uLower; ii <= uUpper; ii++)
	{
		if (m_cAttributeValues[ii].dwAttributeID == dwAttributeID)
		{
			cValues.AddTail(&(m_cAttributeValues[ii]));
		}
	}

	// check if it is necessary to search for an earlier value.
	bool bBoundAdded = false;

	if (cValues.GetCount() == 0 || cValues.GetHead()->llTimestamp > llLower)
	{
		while (uLower > 0)
		{
			uLower--;

			if (m_cAttributeValues[uLower].dwAttributeID == dwAttributeID)
			{
				cValues.AddHead(&(m_cAttributeValues[uLower]));
				bBoundAdded = true;
				break;
			}
		}
	}

	// determine the number of values.
	cAttribute.dwNumValues = cValues.GetCount();

	// add current value.
	if (llUpper == _I64_MAX)
	{
		cAttribute.dwNumValues++;
	}

	// no data.
	if (cAttribute.dwNumValues == 0)
	{
		// no data at all for the attribute.
		if (bNoDataAbove)
		{
			HRESULT hResult = GetCurrentValue(cAttribute);

			if (SUCCEEDED(hResult))
			{
				return OPC_S_CURRENTVALUE;
			}

			return hResult;
		}

		// no data in range.
		return OPC_S_NODATA;
	}

	// allocate arrays.
	cAttribute.vAttributeValues = OpcArrayAlloc(VARIANT, cAttribute.dwNumValues);
	cAttribute.ftTimeStamps     = OpcArrayAlloc(FILETIME, cAttribute.dwNumValues);

	memset(cAttribute.vAttributeValues, 0, sizeof(VARIANT)*cAttribute.dwNumValues);
	memset(cAttribute.ftTimeStamps, 0, sizeof(DWORD)*cAttribute.dwNumValues);

	UINT uFirst = 0;
	UINT uLast  = cAttribute.dwNumValues-1;

	// add current value.
	if (llUpper == _I64_MAX)
	{
		cAttribute.ftTimeStamps[uLast] = OpcUtcNow();

		OpcXml::AnyType cCurrentValue;

		if (GetAttribute(cAttribute.dwAttributeID, cCurrentValue))
		{
			cCurrentValue.Get(cAttribute.vAttributeValues[uLast]);
		}

		if (uLast > 0)
		{
			uLast--;
		}
	}

	// fill arrays.
	while (uFirst <= uLast && cValues.GetCount() > 0)
	{
		COpcHdaAttributeValue* pValue = (bAscending)?cValues.RemoveHead():cValues.RemoveTail();
		
		// adjust timestamps up to the lower bound.
		if (pValue->llTimestamp < llLower)
		{
			cAttribute.ftTimeStamps[uFirst] = OpcHdaFILETIMEFromInt64(llLower);
		}
		else
		{
			cAttribute.ftTimeStamps[uFirst] = OpcHdaFILETIMEFromInt64(pValue->llTimestamp);
		}

		// copy value.
		VariantCopy(&cAttribute.vAttributeValues[uFirst], pValue->cValue.GetPtr());

		uFirst++;
	}

	// all done.
	return S_OK;
}

// ReadAnnotations
HRESULT COpcHdaItem::ReadAnnotations(
	LONGLONG           llStartTime,
	LONGLONG           llEndTime,
	OPCHDA_ANNOTATION& cAnnotation
)
{
	// ensure data is loaded for the item.
	LoadData();

	// initialize empty item object.
	cAnnotation.dwNumValues      = 0;
	cAnnotation.hClient          = NULL;
	cAnnotation.ftTimeStamps     = NULL;
	cAnnotation.szAnnotation     = NULL;
	cAnnotation.ftAnnotationTime = NULL;
	cAnnotation.szUser           = NULL;

	// determine if series is ascending or descending.
	bool bAscending = ((llStartTime != 0 && llStartTime <= llEndTime) || llEndTime == 0);

	// get lower and upper bounding times for the series.
	LONGLONG llLower = (bAscending)?llStartTime:llEndTime;
	LONGLONG llUpper = (bAscending)?llEndTime:llStartTime;

	if (llStartTime == 0)
	{
		llLower = 0;
		llUpper = llEndTime;
	}

	if (llEndTime == 0)
	{
		llUpper = _I64_MAX;
	}

	// find indicies.
	UINT uLower = m_cAnnotations.FindIndexAfter(llLower);
	UINT uUpper = m_cAnnotations.FindIndexBefore(llUpper);

	if (uLower == -1 || uUpper == -1 || uLower > uUpper)
	{
		return OPC_S_NODATA;
	}

	HRESULT hResult = S_OK;

	// calculate number of values to return.
	cAnnotation.dwNumValues = uUpper - uLower + 1;

	// allocate arrays.
	cAnnotation.ftTimeStamps     = OpcArrayAlloc(FILETIME, cAnnotation.dwNumValues);
	cAnnotation.szAnnotation     = OpcArrayAlloc(LPWSTR, cAnnotation.dwNumValues);
	cAnnotation.ftAnnotationTime = OpcArrayAlloc(FILETIME, cAnnotation.dwNumValues);
	cAnnotation.szUser           = OpcArrayAlloc(LPWSTR, cAnnotation.dwNumValues);

	memset(cAnnotation.ftTimeStamps, 0, sizeof(FILETIME)*cAnnotation.dwNumValues);
	memset(cAnnotation.szAnnotation, 0, sizeof(LPWSTR)*cAnnotation.dwNumValues);
	memset(cAnnotation.ftAnnotationTime, 0, sizeof(FILETIME)*cAnnotation.dwNumValues);
	memset(cAnnotation.szUser, 0, sizeof(LPWSTR)*cAnnotation.dwNumValues);

	// fill arrays.
	for (UINT ii = 0; ii < cAnnotation.dwNumValues; ii++)
	{
		COpcHdaAnnotation& cValue = m_cAnnotations[((bAscending)?uLower+ii:uUpper-ii)];

		cAnnotation.ftTimeStamps[ii]     = OpcHdaFILETIMEFromInt64(cValue.llTimestamp);
		cAnnotation.szAnnotation[ii]     = OpcStrDup((LPCWSTR)cValue.cAnnotation);
		cAnnotation.ftAnnotationTime[ii] = OpcHdaFILETIMEFromInt64(cValue.llCreationTime);
		cAnnotation.szUser[ii]           = OpcStrDup((LPCWSTR)cValue.cUser);
	}

	// return result.
	return hResult;
}

// InsertAnnotations
HRESULT COpcHdaItem::InsertAnnotation(
	LONGLONG           llTimestamp,
	OPCHDA_ANNOTATION& cAnnotation
)
{
	// ensure data is loaded for the item.
	LoadData();

	for (DWORD ii = 0; ii < cAnnotation.dwNumValues; ii++)
	{
		// add new annotation.
		COpcHdaAnnotation cNewValue;

		cNewValue.llTimestamp    = llTimestamp;
		cNewValue.cAnnotation    = cAnnotation.szAnnotation[ii];
		cNewValue.llCreationTime = OpcHdaInt64FromFILETIME(cAnnotation.ftAnnotationTime[ii]);
		cNewValue.cUser          = cAnnotation.szUser[ii];

		m_cAnnotations.Insert(llTimestamp, cNewValue);
	}

	// all done.
	return S_OK;
}

// CreateModifiedValue
void COpcHdaItem::CreateModifiedValue(OPCHDA_EDITTYPE eEditType, COpcHdaItemValue& cValue)
{
	// create modified value.
	COpcHdaModifiedValue cModifiedValue;

	cModifiedValue.dblValue           = cValue.dblValue;
	cModifiedValue.dwQuality          = cValue.dwQuality;
	cModifiedValue.llTimestamp        = cValue.llTimestamp;
	cModifiedValue.llModificationTime = OpcHdaInt64FromFILETIME(OpcUtcNow());
	cModifiedValue.eEditType          = eEditType;

	// lookup user name.
	TCHAR tsUserName[_MAX_PATH+1];
	memset(tsUserName, 0, sizeof(tsUserName));

	DWORD dwLength = _MAX_PATH;

	if (GetUserName(tsUserName, &dwLength))
	{
		cModifiedValue.cUser =  tsUserName;
	}

	// update modified value table.
	m_cModifiedValues.Insert(cModifiedValue.llTimestamp, cModifiedValue);
}

// Update
HRESULT COpcHdaItem::Update(
	OPCHDA_EDITTYPE eEditType,
	LONGLONG        llTimestamp,
	VARIANT&        vValue,
	DWORD           dwQuality
)
{
	// ensure data is loaded for the item.
	LoadData();

	// convert value to double.
	COpcVariant cVariant;

	if (eEditType != OPCHDA_DELETE)
	{
		HRESULT hResult = COpcVariant::ChangeType(cVariant.GetRef(), vValue, NULL, VT_R8);

		if (FAILED(hResult))
		{
			return hResult;
		}
	}

	// lookup existing item.
	COpcHdaItemValue* pExisting = NULL;

	UINT uIndex = m_cValues.FindIndex(llTimestamp);

	if (uIndex != -1)
	{
		pExisting = &m_cValues[uIndex];
	}

	// insert a new item value.
	if (eEditType == OPCHDA_INSERT || (eEditType == OPCHDA_INSERTREPLACE && pExisting == NULL))
	{
		// check for existing data.
		if (pExisting != NULL)
		{
			return OPC_E_DATAEXISTS;
		}

		// add new value.
		COpcHdaItemValue cNewValue;

		cNewValue.dblValue    = cVariant;
		cNewValue.dwQuality   = OPCHDA_RAW | (dwQuality & 0x0000FFFF);
		cNewValue.llTimestamp = llTimestamp;

		m_cValues.Insert(llTimestamp, cNewValue);

		// create modified value.
		CreateModifiedValue(OPCHDA_INSERT, cNewValue); 
	}

	// replace an existing item value.
	else if (eEditType == OPCHDA_REPLACE || (eEditType == OPCHDA_INSERTREPLACE && pExisting != NULL))
	{
		// check for existing data.
		if (pExisting == NULL)
		{
			return OPC_E_NODATAEXISTS;
		}

		// create modified value if data actually changed.
		if (pExisting->dblValue != (double)cVariant || pExisting->dwQuality != dwQuality)
		{
			CreateModifiedValue(OPCHDA_REPLACE, *pExisting); 
		}

		// update existing record.
		pExisting->dblValue    = cVariant;
		pExisting->dwQuality   = OPCHDA_RAW | (dwQuality & 0x0000FFFF);
		pExisting->llTimestamp = llTimestamp;
	}

	// delete an existing value.
	else if (eEditType == OPCHDA_DELETE)
	{
		// check for existing data.
		if (pExisting == NULL)
		{
			return OPC_E_NODATAEXISTS;
		}

		// create modified value.
		CreateModifiedValue(OPCHDA_DELETE, *pExisting); 

		// delete value.
		m_cValues.RemoveAt(uIndex);
	}

	// update start/end time.
	FixBoundingTimes();

	// all done.
	return S_OK;
}

// Delete
HRESULT COpcHdaItem::Delete(
	LONGLONG llStartTime,
	LONGLONG llEndTime
)
{
	// ensure data is loaded for the item.
	LoadData();

	// determine if series is ascending or descending.
	bool bAscending = ((llStartTime != 0 && llStartTime <= llEndTime) || llEndTime == 0);

	// get lower and upper bounding times for the series.
	LONGLONG llLower = (bAscending)?llStartTime:llEndTime;
	LONGLONG llUpper = (bAscending)?llEndTime:llStartTime;

	if (llStartTime == 0)
	{
		llLower = 0;
		llUpper = llEndTime;
	}

	if (llEndTime == 0)
	{
		llUpper = _I64_MAX;
	}

	// find indicies.
	UINT uLower = m_cValues.FindIndexAfter(llLower);
	UINT uUpper = m_cValues.FindIndexBefore(llUpper);

	if (uLower == -1 || uUpper == -1 || uLower > uUpper)
	{
		return OPC_S_NODATA;
	}

	// create modified values.
	for (UINT ii = uLower; ii <= uUpper; ii++)
	{
		CreateModifiedValue(OPCHDA_DELETE, m_cValues[ii]); 
	}

	// delete value.
	m_cValues.RemoveAt(uLower, uUpper-uLower+1);

	// update start/end time.
	FixBoundingTimes();

	// save data.
	SaveData();

	// all done.
	return S_OK;
}
