// global.h
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
//-------------------------------------------------------------------------
//
//   $Workfile: global.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 15 $
//       $Date: 8/19/98 1:52p $
//    $Archive: /OPC/AlarmEvents/SampleServer/global.h $
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
/*   $History: global.h $
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 8/19/98    Time: 1:52p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 4/16/98    Time: 3:51p
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
 * User: Jiml         Date: 12/30/97   Time: 6:46p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 12/29/97   Time: 7:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 12/26/97   Time: 6:55p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 12/24/97   Time: 5:39p
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

#include "ThreadSafe.h"
#include "OPCEventServer.h"
#include "ServerStatus.h"



#pragma warning( disable : 4786 )
#pragma warning( disable : 4503 )
#include <set>

using namespace std;


typedef set<COPCEventServer*>  EVSERVER_SET;

extern HINSTANCE g_hinst;

// alarm server globals protected by a critical section

class ThreadSafeGlobal;

class CGlobal : public CComAutoCriticalSection
{
	private:
		friend ThreadSafeGlobal;
		static  CGlobal theGlobal;
		LCID	m_lcid;			
	public:
		CGlobal();
		~CGlobal();

		void NotifyClients( LPCWSTR wszSource, LPCWSTR szCondition, const OPCCondition& cond  );


		EVSERVER_SET		m_EvServerSet;	

		OpcEventServerStatus	m_ServerStatus;

		LCID Lcid() const { return m_lcid; }
		void Lcid( LCID dwLCID ) { m_lcid = dwLCID; }
};


class ThreadSafeGlobal : public ThreadSafe<CGlobal>
{
public:
	ThreadSafeGlobal() : ThreadSafe<class CGlobal>( CGlobal::theGlobal ){}
};



class Registry
{
public:
	static DWORD SimulationPeriod();
	static DWORD ThreadPoolThreads();
	static DWORD MaxSize();

};

