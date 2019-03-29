//==============================================================================
// TITLE: COpcHdaAggregate.cpp
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
#include "COpcHdaAggregate.h"
#include "COpcVariant.h"

//==============================================================================
// Local Declarations

struct OpcHdaAggregateDesc
{
    DWORD   dwID;
    LPCWSTR szName;
    LPCWSTR szDescription;
};

OpcHdaAggregateDesc g_AggregateTable[] = 
{
	{ OPCHDA_INTERPOLATIVE,     OPCHDA_AGGRNAME_INTERPOLATIVE,     L"Do not retrieve an aggregate. This is used for retrieving interpolated values." },
	{ OPCHDA_TOTAL,             OPCHDA_AGGRNAME_TOTAL,             L"Retrieve the totalized value (time integral) of the data over the resample interval." }, 
	{ OPCHDA_AVERAGE,           OPCHDA_AGGRNAME_AVERAGE,           L"Retrieve the average data over the resample interval." },
	{ OPCHDA_TIMEAVERAGE,       OPCHDA_AGGRNAME_TIMEAVERAGE,       L"Retrieve the time weighted average data over the resample interval." }, 
	{ OPCHDA_COUNT,             OPCHDA_AGGRNAME_COUNT,             L"Retrieve the number of raw values over the resample interval." }, 
	{ OPCHDA_STDEV,             OPCHDA_AGGRNAME_STDEV,             L"Retrieve the standard deviation over the resample interval." }, 
	{ OPCHDA_MINIMUMACTUALTIME, OPCHDA_AGGRNAME_MINIMUMACTUALTIME, L"Retrieve the minimum value in the resample interval and the timestamp of the minimum value." },
	{ OPCHDA_MINIMUM,           OPCHDA_AGGRNAME_MINIMUM,           L"Retrieve the minimum value in the resample interval." },
	{ OPCHDA_MAXIMUMACTUALTIME, OPCHDA_AGGRNAME_MAXIMUMACTUALTIME, L"Retrieve the maximum value in the resample interval and the timestamp of the maximum value." },
	{ OPCHDA_MAXIMUM,           OPCHDA_AGGRNAME_MAXIMUM,           L"Retrieve the maximum value in the resample interval." },
	{ OPCHDA_START,             OPCHDA_AGGRNAME_START,             L"Retrieve the value at the beginning of the resample interval. The time stamp is the time stamp of the beginning of the interval." },
	{ OPCHDA_END,               OPCHDA_AGGRNAME_END,               L"Retrieve the value at the end of the resample interval. The timestamp is the time stamp of the end of the interval." },
	{ OPCHDA_DELTA,             OPCHDA_AGGRNAME_DELTA,             L"Retrieve the difference between the first and last value in the interval." },
	{ OPCHDA_REGSLOPE,          OPCHDA_AGGRNAME_REGSLOPE,          L"Retrieve the slope of the regression line over the resample interval." },
	{ OPCHDA_REGCONST,          OPCHDA_AGGRNAME_REGCONST,          L"Retrieve the intercept of the regression line over the resample interval. This is the value of the regression line at the start of the interval." }, 
	{ OPCHDA_REGDEV,            OPCHDA_AGGRNAME_REGDEV,            L"Retrieve the standard deviation of the regression line over the resample interval." },
	{ OPCHDA_VARIANCE,          OPCHDA_AGGRNAME_VARIANCE,          L"Retrieve the variance over the sample interval ." },
	{ OPCHDA_RANGE,             OPCHDA_AGGRNAME_RANGE,             L"Retrieve the difference between the minimum and maximum value over the sample interval." }, 
	{ OPCHDA_DURATIONGOOD,      OPCHDA_AGGRNAME_DURATIONGOOD,      L"Retrieve the duration (in seconds) of time in the interval during which the data is good." },
	{ OPCHDA_DURATIONBAD,       OPCHDA_AGGRNAME_DURATIONBAD,       L"Retrieve the duration (in seconds) of time in the interval during which the data is bad." },
	{ OPCHDA_PERCENTGOOD,       OPCHDA_AGGRNAME_PERCENTGOOD,       L"Retrieve the percent of data (1 equals 100 percent) in the interval which has good quality." },
	{ OPCHDA_PERCENTBAD,        OPCHDA_AGGRNAME_PERCENTBAD,        L"Retrieve the percent of data (1 equals 100 percent) in the interval which has bad quality." },
	{ OPCHDA_WORSTQUALITY,      OPCHDA_AGGRNAME_WORSTQUALITY,      L"Retrieve the worst quality of data in the interval." },
	{ OPCHDA_ANNOTATIONS,       OPCHDA_AGGRNAME_ANNOTATIONS,       L"Retrieve the number of annotations in the interval." },
    { 0, NULL, NULL }
};

//============================================================================
// COpcHdaAggregate

// Constructor
COpcHdaAggregate::COpcHdaAggregate(DWORD dwID)
{
	m_dwID = dwID;

    for (DWORD ii = 0; g_AggregateTable[ii].dwID != 0; ii++)
    {
        if (g_AggregateTable[ii].dwID == dwID)
        {
			m_cName        = g_AggregateTable[ii].szName;
			m_cDescription = g_AggregateTable[ii].szDescription;
        }
    }
}

// Copy Constructor
COpcHdaAggregate::COpcHdaAggregate(const COpcHdaAggregate& cAggregate)
{
	*this = cAggregate;
}

// Destructor
COpcHdaAggregate::~COpcHdaAggregate()
{
}

// Assignment.
COpcHdaAggregate& COpcHdaAggregate::operator=(const COpcHdaAggregate& cAggregate)
{
	m_dwID         = cAggregate.m_dwID;
	m_cName        = cAggregate.m_cName;
	m_cDescription = cAggregate.m_cDescription;

	return *this;
}

// COpcHdaAggregate::Create
void COpcHdaAggregate::Create(
	DWORD&    dwCount,
	DWORD*&   pdwAggrID,
	LPWSTR*&  pszAggrName,
	LPWSTR*&  pszAggrDesc
)
{
	dwCount = 0;

	// determine the number of known attributes.
	while (g_AggregateTable[dwCount].dwID != 0) dwCount++;

	// allocate arrays.
	pdwAggrID   = (DWORD*)OpcArrayAlloc(DWORD, dwCount);
	pszAggrName = (LPWSTR*)OpcArrayAlloc(LPWSTR, dwCount);
	pszAggrDesc = (LPWSTR*)OpcArrayAlloc(LPWSTR, dwCount);

	for (DWORD ii = 0; ii < dwCount; ii++)
	{
		pdwAggrID[ii]   = g_AggregateTable[ii].dwID;
		pszAggrName[ii] = OpcStrDup(g_AggregateTable[ii].szName);
		pszAggrDesc[ii] = OpcStrDup(g_AggregateTable[ii].szDescription);
	}
}
