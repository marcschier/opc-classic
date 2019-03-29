// ServerStatus.cpp
//
// © Copyright 1997,1998 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Alarms and Events Specification and may be used 
//  as set forth in the License Grant section of the OPC Specification.  
//  This code is provided as-is and without warranty or support of any sort 
//  and is subject to the Warranty and Liability Disclaimers which appear 
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by 
//  ICONICS, Inc.  http://www.iconics.com
//
// CONTENTS:
//
//  
//
//
//      System: OPC Alarm & Events
//   Subsystem: 
//
//
// Description: C++ version of OPCEVENTSERVERSTATUS
//
// Functions:   
//
//
//
//
//
/*   $History: ServerStatus.cpp $
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:58a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 4/27/98    Time: 11:18a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 3/30/98    Time: 6:02p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 3/18/98    Time: 7:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 3/10/98    Time: 6:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
*/
//
//
//*************************************************************************          

#include "stdafx.h"
#include "ServerStatus.h"



OpcEventServerStatus::OpcEventServerStatus()
{
	// set some defaults
	memset( &m_data, 0, sizeof( OPCEVENTSERVERSTATUS ) );
	GetSystemTimeAsFileTime( &m_data.ftStartTime );
}


OpcEventServerStatus::OpcEventServerStatus( const OpcEventServerStatus& src )
{
	*this = src;
}



OpcEventServerStatus::~OpcEventServerStatus()
{
}


void OpcEventServerStatus::VendorInfo( LPCWSTR s )
{
	if( s )
		m_wsVendorInfo = s;
	m_data.szVendorInfo = const_cast<LPWSTR>( m_wsVendorInfo.data() );
}


OpcEventServerStatus& OpcEventServerStatus::operator=( const OpcEventServerStatus& src )
{
	m_data = src.m_data;
	m_wsVendorInfo = src.m_wsVendorInfo;
	return *this;
}


OPCEVENTSERVERSTATUS *OpcEventServerStatus::CoTaskDup() const
{
	OPCEVENTSERVERSTATUS *pStatus = NULL;

	try
	{
		pStatus = (OPCEVENTSERVERSTATUS *)CoTaskAlloc( sizeof(OPCEVENTSERVERSTATUS) );
		*pStatus = m_data;		// copy struct
		pStatus->szVendorInfo = CoTaskAlloc( m_wsVendorInfo.data() );
	}
	catch( ... )
	{
		if( pStatus )
			CoTaskMemFree( pStatus->szVendorInfo );
		CoTaskMemFree( pStatus );
		pStatus = NULL;
	}
	return pStatus;
}
