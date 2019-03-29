// ThreadSafe.h
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
//   $Workfile: ThreadSafe.h $
//
//
// Org. Author: Jim Luth
//     $Author: James $
//   $Revision: 13 $
//       $Date: 5/31/01 1:15p $
//    $Archive: /AWX32/AWXMMX32/MMXAgents/MMXFax/ThreadSafe.h $
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
/*   $History: ThreadSafe.h $
 * 
 * *****************  Version 13  *****************
 * User: James        Date: 5/31/01    Time: 1:15p
 * Updated in $/AWX32/AWXMMX32/MMXAgents/MMXFax
 * made it possible to do a shallow copy of CComAutoCriticalSection.  The
 * state of the critical section is re-initialized to open by creating a
 * new critical section on the copy
 * 
 * *****************  Version 12  *****************
 * User: Alaa         Date: 1/07/99    Time: 1:06p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 11  *****************
 * User: Alaa         Date: 12/10/98   Time: 7:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 8/19/98    Time: 12:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 7/17/98    Time: 6:10p
 * Updated in $/GenRegistrar
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 2/18/98    Time: 11:55a
 * Updated in $/Security/Server
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 12/29/97   Time: 7:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:10a
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
#ifndef __THREADSAFE_H
#define __THREADSAFE_H

class OComAutoCriticalSection : public CComAutoCriticalSection
{
public:
	OComAutoCriticalSection(){}
	~OComAutoCriticalSection(){}
	OComAutoCriticalSection(const OComAutoCriticalSection& s)
	{
		*this = s;
	}
	OComAutoCriticalSection& operator=(const OComAutoCriticalSection& s)
	{
		return *this;
	}
};



template <class T> class ThreadSafe
{
protected:
	T& m_ref;
public:
	ThreadSafe( T& ref ) : m_ref( ref ){ m_ref.Lock(); }
	~ThreadSafe() { m_ref.Unlock(); }
	T* operator->() { return &m_ref; }
	operator T*() { return &m_ref; }
};


template <class Z> class ThreadSafeClass
{
private:
	Z	m_z;
public:
	typedef ThreadSafe<Z> acsType;
		
	
	acsType ThreadSafeCall() { return ThreadSafe<Z>(m_z); }
	operator acsType() { return ThreadSafeCall(); }
	acsType operator->() { return ThreadSafeCall(); }
};


template <class T> class ThreadSafeValue : public OComAutoCriticalSection
{
private:
	T	m_t;
public:
	void Set( const T& src )
	{
		Lock();
		m_t = src;
		Unlock();
	};

	T Get()
	{
		Lock();
		T rtn = m_t;
		Unlock();
		return rtn;
	};
};




#endif

