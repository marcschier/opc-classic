// ThreadPool.cpp
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
//      System: Many
//   Subsystem: 
//
//
// Description: Thread Pool Classes
//
// Functions:   
//
//
//
//
//
/*   $History: ThreadPool.cpp $
 * 
 * *****************  Version 21  *****************
 * User: Jiml         Date: 1/07/03    Time: 5:41p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * minor source changes to build in VC7
 * 
 * *****************  Version 20  *****************
 * User: Jiml         Date: 11/27/00   Time: 6:43p
 * Updated in $/Security/Server
 * Fixed JCI PT 108123 issue with hanging DDE apps by not servicing
 * message loop is Thread Pool Threads.
 * 
 * *****************  Version 19  *****************
 * User: Jiml         Date: 12/14/99   Time: 11:13a
 * Updated in $/AWX32/server
 * 
 * *****************  Version 18  *****************
 * User: Jiml         Date: 11/19/99   Time: 2:28p
 * Updated in $/Security/Server
 * Build 78.2  -- Fixed deadlock in AddItems()
 * 
 * *****************  Version 17  *****************
 * User: Jiml         Date: 11/11/99   Time: 4:01p
 * Updated in $/Security/Server
 * Eliminated the Mutex in ThreadPool that was held during "ThreadWork"
 * 
 * *****************  Version 16  *****************
 * User: Jiml         Date: 7/07/99    Time: 5:06p
 * Updated in $/OPC/AlarmEvents/SampleServer/CE
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 5/07/99    Time: 5:52p
 * Updated in $/Security/Server
 * Fixed bug in Thread Pool 
 * Added Internation capability to APPSEC.DLL
 * Added delay on process exit
 * 
 * *****************  Version 14  *****************
 * User: Alaa         Date: 1/07/99    Time: 1:06p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 13  *****************
 * User: Alaa         Date: 12/14/98   Time: 5:55p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 12  *****************
 * User: Alaa         Date: 12/10/98   Time: 7:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:59a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 7/24/98    Time: 12:09p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 7/24/98    Time: 11:32a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
*/
//
//
//*************************************************************************          

#include "stdafx.h"
#include <process.h>
#include "ThreadPool.h"



#define TIMEOUT	30000		// milliseconds


ComThreadPool*	CTPClient::s_pDefaultPool = NULL;

// static
DWORD ComThreadPool::WaitForSingleObjectMsgLoop( HANDLE hHandle, DWORD dwMilliseconds )
{

	MSG		msg;
	DWORD	dwBeginTime = GetTickCount();
	DWORD	dwWaitThisTime = INFINITE;
	while ( TRUE )
	{
		// Try to process messages
		while( PeekMessage( &msg, NULL, NULL, NULL, PM_REMOVE ))
		{
			TranslateMessage( &msg );
			DispatchMessage( &msg );
		}

		
		if( dwMilliseconds != INFINITE )
		{
			DWORD dwElapsed = GetTickCount() - dwBeginTime;
			if( dwElapsed >= dwMilliseconds )
				dwWaitThisTime = 0;
			else
				dwWaitThisTime = dwMilliseconds - dwElapsed;
		}


		// Test if the event is signaled
		// Wait no more than 100 msec
		DWORD rtn = WaitForSingleObject( hHandle, __min( 100, dwWaitThisTime ));

		if( rtn != WAIT_TIMEOUT || dwWaitThisTime < 100 )
			return rtn;
	}
}






ComThreadPool::ThreadData::ThreadData()
{
	pPool = NULL;
	pActiveClient = NULL;
	hThread = NULL;
	dwThreadID = 0;
}


ComThreadPool::ThreadData::~ThreadData()
{
//	TerminateThread( hThread, 0 );  // thread is normally dead by now, but just in case
	CloseHandle( hThread );
}


ComThreadPool::ComThreadPool( int nThreadCount, 
							 int nPriority, 
							 UINT nStackSize, 
							 DWORD dwCreateFlags, 
							 LPSECURITY_ATTRIBUTES lpSecurityAttrs  )
{
	_ASSERT( nThreadCount > 0 && nThreadCount < 500 );
	m_nThreadCount = nThreadCount;
	m_nThreadsInUse = 0;
	m_nThreadPeakUse = 0;
	m_nQueuePeakUse = 0;

	m_bDie = FALSE;
	m_hDoWork = CreateEvent( NULL, FALSE, FALSE, NULL );

	// make this the default pool for all clients
	CTPClient::s_pDefaultPool = this;

	m_pThreadData = new ThreadData[ m_nThreadCount ];
	for( int i = 0; i < m_nThreadCount; i++ )
	{
		m_pThreadData[i].pPool = this;
		m_pThreadData[i].hThread = MyCreateThread(
				lpSecurityAttrs, nStackSize, ThreadStub, &(m_pThreadData[i]), 
				dwCreateFlags, &(m_pThreadData[i].dwThreadID) );
		_ASSERT( m_pThreadData[i].hThread );
		if( nPriority != THREAD_PRIORITY_NORMAL )
		{
			BOOL bThreadPrioritySet = SetThreadPriority( m_pThreadData[i].hThread, nPriority );
			_ASSERT( bThreadPrioritySet );
		}
	}
}


ComThreadPool::~ComThreadPool()
{

#ifdef TRACE
	TRACE( _T("Entering ComThreadPool::~ComThreadPool()\n") );
#endif
	if( CTPClient::s_pDefaultPool == this )
		CTPClient::s_pDefaultPool = NULL;
	
	// remove all registered clients
	Lock();
	CLIENTSET::iterator it;
	it = m_ClientSet.begin();
	while( it != m_ClientSet.end() )
	{
		CTPClient *p = (*it);
		p->m_pPool = NULL;
		CLIENTSET::iterator erase_it = it;
		++it;
		m_ClientSet.erase( erase_it );
	}
	Unlock();


	m_bDie = TRUE;
	HANDLE *pHandle = new HANDLE[ m_nThreadCount ];
	for( int i = 0; i < m_nThreadCount; i ++ )
	{
		pHandle[i] = m_pThreadData[i].hThread;
		SetEvent( m_hDoWork );
		Sleep(0);
	}


	
	if( WaitForMultipleObjects( m_nThreadCount, pHandle, TRUE, TIMEOUT ) == WAIT_TIMEOUT )
	{
		// TODO this is bad
#ifdef TRACE
		TRACE(_T("WaitForMultipleObjects() timed out in ComThreadPool::~ComThreadPool()\n"));
#endif
		// terminate the threads
		for( int i = 0; i < m_nThreadCount; i ++ )
		{
			if( WaitForSingleObject( pHandle[i], 0 ) == WAIT_TIMEOUT )
				TerminateThread( pHandle[i], 1 );
		}


		;
	}
	delete [] pHandle;
	delete [] m_pThreadData;

	CloseHandle( m_hDoWork );

#ifdef TRACE
	TRACE( _T("Thread Pool max threads used = %i/%i\n"), ThreadPeakUse(), ThreadCount() );
	TRACE( _T("Thread Pool max queue used = %i\n"), QueuePeakUse() );
#endif
}


// override to add/alter per thread initialization
void ComThreadPool::ThreadInit()
{
	if(FAILED(CoInitializeEx(NULL, COINIT_MULTITHREADED)) )
	{
#ifdef TRACE
		TRACE(_T("CoInitializeEx failed\n"));
#endif
		;
	}
}



// override to add/alter per thread de-initialization
void ComThreadPool::ThreadUnInit()
{
   CoUninitialize();
}





THREADRTN WINAPI ComThreadPool::ThreadStub( LPVOID pThreadData )
{
	ThreadData *pData = (ThreadData *)pThreadData;
	THREADRTN rtn = pData->pPool->Thread( *pData );
	return rtn;
}


THREADRTN ComThreadPool::Thread( ThreadData& data )
{
	ThreadInit();

	while( !m_bDie )
	{
//		WaitForSingleObject( m_hDoWork, INFINITE );
		WaitForSingleObjectMsgLoop( m_hDoWork, INFINITE );
		if( m_bDie )
			break;
		CTPClient* pClient;
		Lock();
		while( !m_bDie && (pClient = GetWorkTodo()) != NULL )
		{	
			data.pActiveClient = pClient;
			ResetEvent( pClient->m_hIdle );
			pClient->m_state = CTPClient::RUNNING;
			++m_nThreadsInUse;
			m_nThreadPeakUse = max( m_nThreadPeakUse, m_nThreadsInUse );
			Unlock();
			pClient->ThreadWork();
			Sleep(0);
			Lock();
			data.pActiveClient = NULL;
			--m_nThreadsInUse;
			bool bAgain = (pClient->m_state == CTPClient::RUN_AGAIN);
			pClient->m_state = CTPClient::IDLE;
			SetEvent( pClient->m_hIdle );
			if( bAgain )
				pClient->QueueThreadWork();
		}
		Unlock();
	}
	
	ThreadUnInit();
	return ERROR_SUCCESS;
}


CTPClient *ComThreadPool::GetWorkTodo()
{
	Lock();
	CTPClient *rtn = NULL;
	if( !m_q.empty() )
	{
		rtn = m_q.front();
		CLIENTSET::iterator it;
		it = m_ClientSet.find( rtn );
		_ASSERT( it != m_ClientSet.end() );
		m_q.pop_front();
	}
	Unlock();
	return rtn;
}


void ComThreadPool::Queue( CTPClient& client )
{
	if( !m_bDie )
	{
		Lock();
		switch( client.m_state )
		{
		case CTPClient::IDLE:
			{
				CLIENTSET::iterator it;
				it = m_ClientSet.find( &client );
				if( it != m_ClientSet.end() )
				{
					m_q.push_back( &client );
					client.m_state = CTPClient::QUEUED;
					m_nQueuePeakUse = max( m_nQueuePeakUse, (int)m_q.size() );
					SetEvent( m_hDoWork );
				}
			}
			break;
		case CTPClient::RUNNING:
			client.m_state = CTPClient::RUN_AGAIN;
			break;
		}
		Unlock();
		Sleep(0);
	}
}

void ComThreadPool::Remove( CTPClient& client )
{
	Lock();
	UnregisterClient( client );  // prevent any more adding to Q
	// remove from Q

	if( client.m_state == CTPClient::QUEUED)
	{
		POOLQUE::iterator pi;
		for(pi= m_q.begin();  pi !=m_q.end(); pi++)
		{
			if( *pi == &client )
			{
				m_q.erase( pi );
				client.m_state = CTPClient::IDLE;
				break;
			}
		}
	}

	Unlock();

#ifdef TRACE
	TRACE(_T("WaitForSingleObject() %Xh\n") , &client);
#endif
	if( WaitForSingleObject( client.m_hIdle, TIMEOUT ) == WAIT_TIMEOUT )
	{
		// TODO This is bad
#ifdef TRACE
		TRACE(_T("WaitForSingleObject() timed out in ComThreadPool::Remove()\n") );
		_ASSERTE( 1 == 0);
#endif

		;
	}
#ifdef TRACE
	TRACE(_T("done WaitForSingleObject() %Xh\n") , &client);
#endif

#ifdef TRACE
	Lock();
	if( m_ClientSet.size() == 0 )
	{
		for( int i = 0; i < m_nThreadCount; i++ )
			_ASSERTE( m_pThreadData[i].pActiveClient == NULL );
	}
	Unlock();
#endif
}


void ComThreadPool::RegisterClient( CTPClient& client )
{
	Lock();
	m_ClientSet.insert(&client);
	Unlock();
}

void ComThreadPool::UnregisterClient( CTPClient& client )
{
	Lock();
	CLIENTSET::iterator it;
	it = m_ClientSet.find( &client );
//	_ASSERT( it != m_ClientSet.end() );
	if( it != m_ClientSet.end() )
	{
		client.m_pPool = NULL;
		m_ClientSet.erase( it );
	}
	Unlock();
}




CTPClient::CTPClient() 
{ 
	m_hIdle = CreateEvent( NULL, TRUE, TRUE, NULL );
	m_state = IDLE;
	m_pPool = s_pDefaultPool; 
	if( m_pPool )
		m_pPool->RegisterClient( *this );
}


CTPClient::~CTPClient()
{
	RemoveThread();
	CloseHandle( m_hIdle );
}


void CTPClient::QueueThreadWork()
{
	if( m_pPool )
		m_pPool->Queue( *this );
}


void CTPClient::SetPool( ComThreadPool& pool )
{
	if( m_pPool )
		m_pPool->UnregisterClient( *this );
	m_pPool = &pool; 
	m_pPool->RegisterClient( *this );
}

void CTPClient::RemoveThread()
{
	if( m_pPool )
		m_pPool->Remove( *this );
}

