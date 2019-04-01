// global.cpp
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
//   Subsystem: Sample Server
//
//
// Description: 
//
// Functions:   
//
//
//
//
//
/*   $History: global.cpp $
 * 
 * *****************  Version 20  *****************
 * User: Jiml         Date: 1/07/03    Time: 5:41p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * minor source changes to build in VC7
 * 
 * *****************  Version 19  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:54a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 18  *****************
 * User: Jiml         Date: 7/24/98    Time: 11:32a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 17  *****************
 * User: Jiml         Date: 4/24/98    Time: 3:04p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 16  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 4/16/98    Time: 3:51p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 3/13/98    Time: 6:45p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 3/12/98    Time: 4:55p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 3/10/98    Time: 6:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 3/04/98    Time: 4:14p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 12/31/97   Time: 11:47a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 12/30/97   Time: 6:46p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 12/29/97   Time: 7:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 12/26/97   Time: 6:55p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:06a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 12/23/97   Time: 12:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 12/15/97   Time: 10:45a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 11/24/97   Time: 10:01a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 11/14/97   Time: 6:39p
 * Updated in $/OPC/AlarmEvents/SampleServer
*/
//
//
//*************************************************************************          
#include "stdafx.h"
#include "global.h"
#include "ThreadPool.h"
#include "ResourceVer.h"

#define REGISTRY_APP_KEY _T("OPCSample.OPCEventServer")


#if _MSC_VER < 1300
#	define QueryDWORDValue( a, b ) QueryValue( b, a )
#endif



// the one and only global class
CGlobal CGlobal::theGlobal;

ComThreadPool theThreadPool( Registry::ThreadPoolThreads() );


HINSTANCE g_hinst = 0;





CGlobal::CGlobal()
{
	USES_CONVERSION;
	m_lcid = 0;
	
	// set some of OPCEVENTSERVERSTATUS

	FILETIME now;
	GetSystemTimeAsFileTime( &now );

	m_ServerStatus.StartTime( now );
	m_ServerStatus.CurrentTime( now );
	m_ServerStatus.ServerState( OPCAE_STATUS_TEST ); // a "real" server should return OPC_STATUS_RUNNING

	// init the rest of the OPCEVENTSERVERSTATUS struct with strings from the resource

	ResourceVersion rv;

	m_ServerStatus.VendorInfo( T2CW(rv.Comments()) );
	const VS_FIXEDFILEINFO* vs_ffi = rv.FixedFileInfo();
	if( vs_ffi )
	{
		m_ServerStatus.MajorVersion( HIWORD(vs_ffi->dwProductVersionMS) );
		m_ServerStatus.MinorVersion( LOWORD(vs_ffi->dwProductVersionMS) );
		m_ServerStatus.BuildNumber( LOWORD(vs_ffi->dwProductVersionLS) );
	}

}



CGlobal::~CGlobal()
{
}



void CGlobal::NotifyClients( LPCWSTR wszSource, LPCWSTR wszCondition, const OPCCondition& cond  )
{
	OnEventClass *pOnEventClass = new OnEventClass( wszSource, wszCondition, cond );


	EVSERVER_SET::iterator it;
	for( it = m_EvServerSet.begin();
					it != m_EvServerSet.end();
					it++ )
	{
		(*it)->ProcessNewEvent( pOnEventClass, cond.Areas() );
	}

	pOnEventClass->Release();  // release my reference
}





DWORD Registry::SimulationPeriod()
{
	static bool bRead = FALSE;
	static DWORD val = 5000;  // default is 5 seconds (5000 milliseconds)
	if( !bRead )
	{
		bRead = TRUE;
		CRegKey reg;
		LONG rtn = reg.Create( HKEY_CLASSES_ROOT, REGISTRY_APP_KEY );
		_ASSERT( rtn == ERROR_SUCCESS );
		reg.QueryDWORDValue( _T("SimulationPeriod"), val );
	}
	return val;		
}

DWORD Registry::ThreadPoolThreads()
{
	static bool bRead = FALSE;
	static DWORD val = 10;		// default is 10 threads
	if( !bRead )
	{
		bRead = TRUE;
		CRegKey reg;
		LONG rtn = reg.Create( HKEY_CLASSES_ROOT, REGISTRY_APP_KEY );
		_ASSERT( rtn == ERROR_SUCCESS );
		reg.QueryDWORDValue(  _T("ThreadPoolThreads"), val );
	}

	return val;
}




DWORD Registry::MaxSize()
{
	static bool bRead = FALSE;
	static DWORD val = 1024;		// default is 1024 events in one client callback 
	if( !bRead )
	{
		bRead = TRUE;
		CRegKey reg;
		LONG rtn = reg.Create( HKEY_CLASSES_ROOT, REGISTRY_APP_KEY );
		_ASSERT( rtn == ERROR_SUCCESS );
		reg.QueryDWORDValue(  _T("MaxSize"), val );
	}

	return val;
}
