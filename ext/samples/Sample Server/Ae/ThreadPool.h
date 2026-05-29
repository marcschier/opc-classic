// ThreadPool.h
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
//   $Workfile: ThreadPool.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 15 $
//       $Date: 11/27/00 6:43p $
//    $Archive: /Security/Server/ThreadPool.h $
//
//      System: GENESIS-32
//   Subsystem: Security
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
/*   $History: ThreadPool.h $
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 11/27/00   Time: 6:43p
 * Updated in $/Security/Server
 * Fixed JCI PT 108123 issue with hanging DDE apps by not servicing
 * message loop is Thread Pool Threads.
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 12/14/99   Time: 11:13a
 * Updated in $/AWX32/server
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 11/11/99   Time: 4:01p
 * Updated in $/Security/Server
 * Eliminated the Mutex in ThreadPool that was held during "ThreadWork"
 * 
 * *****************  Version 12  *****************
 * User: Alaa         Date: 1/07/99    Time: 1:06p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 11  *****************
 * User: Alaa         Date: 12/14/98   Time: 5:55p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 10  *****************
 * User: Alaa         Date: 12/10/98   Time: 7:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 8/19/98    Time: 12:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 7/24/98    Time: 12:09p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 7/24/98    Time: 11:32a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 4/01/98    Time: 5:06p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 2/18/98    Time: 11:55a
 * Updated in $/Security/Server
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 11/07/97   Time: 10:55a
 * Updated in $/Security/Server
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 11/05/97   Time: 6:36p
 * Updated in $/Security/Server
 * 
 * *****************  Version 1  *****************
 * User: Jiml         Date: 11/04/97   Time: 3:21p
 * Created in $/Security/Server
*/
//
//
//*************************************************************************          

// BEGIN ThreadPool.h
#ifndef __THREADPOOL_H
#define __THREADPOOL_H

#ifdef NO_CRTL
	typedef DWORD THREADRTN;
	typedef DWORD THREADIDTYPE;
	#define MyCreateThread	CreateThread
#else
	#include <process.h>
	typedef unsigned THREADRTN;
	typedef unsigned THREADIDTYPE;
	#define MyCreateThread (HANDLE)_beginthreadex
#endif



#include <atlbase.h>

#pragma warning( disable : 4786 )
#include <deque>
#include <set>

using namespace std;



class ComThreadPool;

class CTPClient
{
private:
	friend ComThreadPool;
	enum state { IDLE, QUEUED, RUNNING, RUN_AGAIN };
	
	static ComThreadPool*		s_pDefaultPool;
	ComThreadPool*				m_pPool;
	state						m_state;
	HANDLE						m_hIdle;
public:
	CTPClient();
	virtual ~CTPClient();

	virtual void ThreadWork() = 0;

	void SetPool( ComThreadPool& pool );
	void QueueThreadWork();
	void RemoveThread();
};


typedef set<CTPClient*> CLIENTSET;
typedef deque< CTPClient* >  POOLQUE;

class ComThreadPool : public CComAutoCriticalSection
{
protected:
	class ThreadData
	{
	public:
		THREADIDTYPE dwThreadID;
		HANDLE hThread;
		CTPClient* pActiveClient;
		ComThreadPool*	pPool;	
		
		ThreadData();
		~ThreadData();

	};


	ThreadData*		m_pThreadData;

	POOLQUE		m_q;
	CLIENTSET	m_ClientSet;

	HANDLE	m_hDoWork;
	BOOL	m_bDie;

	int		m_nThreadCount;
	int		m_nThreadsInUse;
	int		m_nThreadPeakUse;
	int		m_nQueuePeakUse;

	static THREADRTN WINAPI ThreadStub( LPVOID pThreadData );
	THREADRTN Thread( ThreadData& data );
	CTPClient *GetWorkTodo();

	friend CTPClient;
	void Queue( CTPClient& client );
	void Remove( CTPClient& client );
	void RegisterClient( CTPClient& client );
	void UnregisterClient( CTPClient& client );

	virtual void ThreadInit();
	virtual void ThreadUnInit();

	static DWORD WaitForSingleObjectMsgLoop( HANDLE hHandle, DWORD dwMilliseconds );

public:
	ComThreadPool( int nThreadCount, int nPriority = THREAD_PRIORITY_NORMAL, UINT nStackSize = 0, DWORD dwCreateFlags = 0, LPSECURITY_ATTRIBUTES lpSecurityAttrs = NULL  );
	~ComThreadPool();

	int ThreadCount() const { return m_nThreadCount; }
	int ThreadsInUse() const { return m_nThreadsInUse; }
	int ThreadsAvailable() const { return m_nThreadCount - m_nThreadsInUse; }
	int ThreadPeakUse() const { return m_nThreadPeakUse; }
	int QueuePeakUse() const { return m_nQueuePeakUse; }
};



#endif
// END ThreadPool.h

