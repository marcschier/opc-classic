//==============================================================================
// TITLE: COpcHdaItem.h
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

#ifndef _COpcHdaItem_H_
#define _COpcHdaItem_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

#include "COpcHdaAttribute.h"
#include "COpcHdaItemValue.h"

//============================================================================
// MACRO:   OPCHDA_MAX_VALUES
// PURPOSE: Defines the maximum number of values per item that can be returned in a single call.

#define OPCHDA_MAX_VALUES 2000

//============================================================================
// MACRO:   OPCHDA_MAX_UPDATE_INTERVAL
// PURPOSE: Defines the maximum number of values per item that can be returned in a single call.

#define OPCHDA_MAX_UPDATE_INTERVAL 10000000

//==============================================================================
// CLASS:   COpcHdaItem
// PURPOSE: A source server for data exchange data.

class COpcHdaItem : public IOpcXmlSerialize
{
    OPC_CLASS_NEW_DELETE()

public:

    //==========================================================================
    // Operators

    // Constructor
    COpcHdaItem(const COpcString& cItemID) { Init(); m_cItemID = cItemID; }

    // Destructor 
    ~COpcHdaItem() { Clear(); }
    
    //==========================================================================
    // Public Methods

    // GetItemID
    COpcString GetItemID() { return m_cItemID; }

	// BuildAddressSpace
	bool BuildAddressSpace();

	// ClearAddressSpace
	void ClearAddressSpace();

	// returns the current value of an attribute.
	bool GetAttribute(DWORD dwID, OpcXml::AnyType& cValue);

	// checks whether the item meets the criteria specified by the filter.
	bool Match(COpcHdaBrowseFilter& cFilter);

	// saves the data for the item.
	void SaveData();

	// reads the raw data for the specified interval.
	HRESULT ReadRaw(
		LONGLONG     llStartTime,
		LONGLONG     llEndTime,
		UINT         uNumValues,
		bool         bIncludeBounds,
		UINT         uStartIndex,
		OPCHDA_ITEM& cItem
	);

	// reads the processed data for the specified interval.
	HRESULT ReadProcessed(
		LONGLONG     llStartTime,
		LONGLONG     llEndTime,
		LONGLONG     llResampleInterval,
		DWORD        haAggregate, 
		OPCHDA_ITEM& cItem
	);

	// reads the data at the specified times.
	HRESULT ReadAtTime(
		COpcArray<LONGLONG>& cTimestamps,
		OPCHDA_ITEM&         cItem
	);

	// reads and modified values within the specified interval.
	HRESULT ReadModified(
		LONGLONG             llStartTime,
		LONGLONG             llEndTime,
		UINT                 uNumValues,
		UINT                 uStartIndex,
		OPCHDA_MODIFIEDITEM& cItem
	);

	// reads item attributes over the specified interval.
	HRESULT ReadAttribute(
		LONGLONG          llStartTime,
		LONGLONG          llEndTime,
		DWORD             dwAttributeID, 
		OPCHDA_ATTRIBUTE& cAttribute
	);

	// reads the item annotations over the specified interval.
	HRESULT ReadAnnotations(
		LONGLONG           llStartTime,
		LONGLONG           llEndTime,
		OPCHDA_ANNOTATION& cAnnotation
	);

	// inserts the item annoations at the specified timestamp.
	HRESULT InsertAnnotation(
		LONGLONG           llTimestamp,
		OPCHDA_ANNOTATION& cAnnotation
	);

	// updates the item values.
	HRESULT Update(
		OPCHDA_EDITTYPE eEditType,
		LONGLONG        llTimestamp,
		VARIANT&        vValue,
		DWORD           dwQuality
	);
	
	// deletes the values within the specified interval.
	HRESULT Delete(
		LONGLONG llStartTime,
		LONGLONG llEndTime
	);

    //==========================================================================
    // IOpcXmlSerialize

    // Init
    virtual void Init();

    // Clear
    virtual void Clear();

    // Read
    virtual bool Read(COpcXmlElement& cElement);

    // Write
    virtual bool Write(COpcXmlElement& cElement);

private:
    
	//==========================================================================
    // Private Methods

	// loads the data for the item (if it is not already loaded).
	void LoadData();
		
	// loads the data for the item from a CSV file.
	bool LoadDataFromFile(const COpcString& cFileName);

	// checks whether the value at index has good quality.
	bool IsUseableData(UINT uIndex);
	bool IsUseableData(const COpcHdaItemValue& cValue);

	// gets the timestamp of the sample at the specified index.
	LONGLONG GetTimestampAtIndex(UINT uIndex);

	// returns the value at the specified time by interpolating between the bounding values.
	double Interpolate(LONGLONG llTime, DWORD& dwQuality);

	// calculates the time average by interpolating the values at the start and end.
	double TimeAverage(LONGLONG llStartBound, LONGLONG llEndBound, DWORD& dwQuality);

	// calculates the total area under the curve.
	double Total(LONGLONG llStartBound, LONGLONG llEndBound, DWORD& dwQuality);

	// calculates the arthimatic average of all good values in the interval.
	double Average(LONGLONG llStartBound, LONGLONG llEndBound, DWORD& dwQuality);

	// counts the number of good values in the interval.
	double Count(LONGLONG llStartBound, LONGLONG llEndBound, DWORD& dwQuality);

	// calculates the standard deviation for the set of values in the interval.
	double StdDev(bool bVariance, LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality);
		
	// finds the minimum or maximum good value in the interval.
	double MinMax(
		bool      bMinimum,
		LONGLONG  llStartTime, 
		LONGLONG  llEndTime, 
		DWORD&    dwQuality,
		FILETIME* pftTimestamp
	);

	// finds the start or end value within the interval.
	double StartEnd(
		bool      bStart,
		LONGLONG  llStartTime, 
		LONGLONG  llEndTime, 
		DWORD&    dwQuality,
		FILETIME& ftTimestamp
	);

	// calculates the difference between the first and last good values in the interval.
	double Delta(LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality);
	
	// calculates the absolute difference between the maximum and minimum values in the interval.
	double Range(LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality);

	// calculates a linear regression for all points in the interval.
	double Regression(
		bool      bSlope,
		LONGLONG  llStartTime, 
		LONGLONG  llEndTime, 
		DWORD&    dwQuality
	);

	// calculates the error term for a linear regression for all points in the interval.
	double RegressionDeviation(
		LONGLONG  llStartTime, 
		LONGLONG  llEndTime, 
		DWORD&    dwQuality
	);

	// calculates a linear regression on all points in the interval.
	DWORD LinearRegression(
		LONGLONG llStartTime, 
		LONGLONG llEndTime, 
		double&  dblSlope,
		double&  dblConstant
	);

	// calculates the time or percentage of an interval that is either good/bad.
	double Duration(
		bool      bGood,
		bool      bPercent,
		LONGLONG  llStartTime, 
		LONGLONG  llEndTime, 
		DWORD&    dwQuality
	);

	// returns the worst quality in the interval.
	double WorstQuality(LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality);

	// returns the number of annotations in the interval.
	double AnnotationCount(LONGLONG llStartTime, LONGLONG llEndTime, DWORD& dwQuality);
		
	// finds all values within the specified interval. returns false if no data exists.
	bool FindRawValues(LONGLONG llStartTime, LONGLONG llEndTime, COpcList<COpcHdaItemValue*>& cValues);

	// finds all usuable values within the specified interval. returns the uncertain quality if bad data exists.
	DWORD FindUseableValues(LONGLONG llStartTime, LONGLONG llEndTime, COpcList<COpcHdaItemValue*>& cValues);

	// fills an array with the indicies of the selected values.
	bool ReadIndicies(
		LONGLONG llStartTime,
		LONGLONG llEndTime,
		UINT     uMaxValues,
		bool     bIncludeBounds,
		UINT*    pIndicies,
		UINT&    uCount
	);

	// converts and returns the value at the specified index.
	VARIANT GetValue(UINT uIndex, DWORD& dwQuality);

	// converts and returns the specified value.
	VARIANT GetValue(double dblValue, DWORD& dwQuality);

	// fills the structure in with the current value of the specified attribute.
	HRESULT GetCurrentValue(OPCHDA_ATTRIBUTE& cAttribute);

	// adds a new record to the modified value list.
	void CreateModifiedValue(OPCHDA_EDITTYPE eEditType, COpcHdaItemValue& cValue);

	// updates the start/end times after data was modified.
	void FixBoundingTimes();

	// creates a new value based on the derive equation and the timestamp.
	double CreateValue(LONGLONG llTimestamp);

	// extracts the simulation parameeters from the derive equation.
	void ParseDeriveEquation(const COpcString& cEquation);

    //==========================================================================
    // Private Members

	// identity attributes.
    COpcString m_cItemID;	
    COpcString m_cItemName;	
	COpcString m_cDescription;
	COpcString m_cEngUnits;
	COpcString m_cDeriveEquation;
	COpcString m_cNodeName;
	COpcString m_cProcessName;
	COpcString m_cSourceName;
	COpcString m_cSourceType;
	
	// data description attributes.
	VARTYPE    m_vtDataType;
	double     m_dblNormalMax;
	double     m_dblNormalMin;
	LONGLONG   m_llMaxInterval;
	LONGLONG   m_llMinInterval;
	double     m_dblDeviation;
	short      m_eDeviationType;
	double     m_dblLowEntryLimit;
	double     m_dblHighEntryLimit;

	// data processing attributes.
	bool       m_bArchiving;
	bool       m_bStepped;
	bool       m_bUncertainValues;
	UINT       m_uWaveform;
	double     m_dblPeriod;

	// all other attributes.
	COpcMap<DWORD,OpcXml::AnyType*> m_cAttributes;

	// actual data.
	bool                       m_bLoaded;
    LONGLONG                   m_llStartTime;
	LONGLONG                   m_llEndTime;
	UINT                       m_uFirstUseable;
	UINT                       m_uLastUseable;
	COpcHdaItemValueArray      m_cValues;
	COpcHdaModifiedValueArray  m_cModifiedValues;
	COpcHdaAttributeValueArray m_cAttributeValues;
	COpcHdaAnnotationArray     m_cAnnotations;
};

//============================================================================
// TYPEDEF: COpcHdaItemMap
// PURPOSE: A table of device items indexed by item id.

typedef COpcMap<COpcString,COpcHdaItem*> COpcHdaItemMap;

#endif // _COpcHdaItem_H_
