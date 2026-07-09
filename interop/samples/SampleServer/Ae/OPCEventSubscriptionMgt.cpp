// OPCEventSubscriptionMgt.cpp
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
/*   $History: OPCEventSubscriptionMgt.cpp $
 * 
 * *****************  Version 35  *****************
 * User: Jiml         Date: 12/08/02   Time: 3:40p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * Added Keep-alive
 * 
 * *****************  Version 34  *****************
 * User: Jiml         Date: 9/14/02    Time: 8:55p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 33  *****************
 * User: Jiml         Date: 7/19/02    Time: 6:52p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 32  *****************
 * User: Jiml         Date: 2/01/02    Time: 5:16p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * Interop changes
 * 
 * *****************  Version 31  *****************
 * User: Jiml         Date: 1/18/02    Time: 3:35p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 30  *****************
 * User: Jiml         Date: 8/14/01    Time: 12:50p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 29  *****************
 * User: Jiml         Date: 11/18/98   Time: 3:08p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 28  *****************
 * User: Jiml         Date: 9/04/98    Time: 2:13p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * Fixed bug - m_ftcLastUpdate was not getting updated.
 * 
 * *****************  Version 27  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:56a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 26  *****************
 * User: Jiml         Date: 8/18/98    Time: 2:15p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * Fixed Refresh() -- was not working at all 
 * 
 * *****************  Version 25  *****************
 * User: Jiml         Date: 8/10/98    Time: 6:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 *  1. Added AckComment attribute
 *  2. Added Areas attribute
 *  3. revised ::GetConditionState() to include attributes
 * 
 * *****************  Version 24  *****************
 * User: Jiml         Date: 7/24/98    Time: 12:09p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 23  *****************
 * User: Jiml         Date: 7/24/98    Time: 11:32a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 22  *****************
 * User: Jiml         Date: 5/01/98    Time: 10:04a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 21  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 20  *****************
 * User: Jiml         Date: 4/15/98    Time: 4:50p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 19  *****************
 * User: Jiml         Date: 4/03/98    Time: 2:42p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 18  *****************
 * User: Jiml         Date: 3/30/98    Time: 6:02p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 17  *****************
 * User: Jiml         Date: 3/18/98    Time: 7:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 16  *****************
 * User: Jiml         Date: 3/17/98    Time: 5:04p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 3/11/98    Time: 4:34p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 3/06/98    Time: 6:02p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 3/04/98    Time: 4:14p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 2/26/98    Time: 11:04a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 2/18/98    Time: 10:45a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 1/02/98    Time: 6:29p
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
 * User: Jiml         Date: 12/24/97   Time: 5:39p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:08a
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
// OPCEventSubscriptionMgt.cpp : Implementation of COPCEventSubscriptionMgt
#include "stdafx.h"
#include "global.h"
#include "OpcAeServer.h"
#include "OPCEventSubscriptionMgt.h"
#include "OPCEventServer.h"
#include "ConditionMap.h"
#include "MatchPattern.h"
#include "QueryContainers.h"
#include "OPCEventAreaBrowser.h"
#include "simulation.h"


#define WILD_CHARS	L"*?#["


SubscribedEventClass::~SubscribedEventClass()
{
	// delete the Variant Array created by copy()
	delete [] (CComVariant *)pEventAttributes;
}
	

void SubscribedEventClass::copy( const OnEventClass& src, const RETURNED_ATTRIBUTES_SET& RASet )
{
	// make a shallow copy of everything except the variant array.
	// make a new variant array with only the attributes that are 
	// subscribed to.

	*(ONEVENTSTRUCT *)this = src;   // shallow copy

	// assume no extra attributes
	dwNumEventAttrs = 0;
	pEventAttributes = NULL;
	
	RETURNED_ATTRIBUTES_SET::const_iterator RASit;
	ReturnedAttributes test( dwEventCategory );
	RASit = RASet.find( test );
	if( RASit != RASet.end() )
	{
		dwNumEventAttrs = (*RASit).AttributeVector().size();
	    CComVariant *pComVar = new CComVariant[dwNumEventAttrs];
		ATTRIBUTE_VECTOR::const_iterator it = (*RASit).AttributeVector().begin();
		for( DWORD i = 0; i < dwNumEventAttrs; i++, it++ )
		{
			if( (*it) < src.dwNumEventAttrs )
				pComVar[i] = src.pEventAttributes[ (*it) ];  // copy the subscribed variants
		}			

		pEventAttributes = pComVar;
	}
}







/////////////////////////////////////////////////////////////////////////////
// COPCEventSubscriptionMgt

void COPCEventSubscriptionMgt::Init( COPCEventServer& Parent,
			BOOL bActive,
            DWORD dwBufferTime,
            DWORD dwMaxSize,
            OPCHANDLE hClientSubscription )
{

	m_pParent = &Parent;
//	ATLTRACE( _T("COPCEventSubscriptionMgt::Init() %Xh %Xh\n"), m_pParent, this );
	dwMaxSize = dwMaxSize ? dwMaxSize : Registry::MaxSize();
	m_dwMaxSize = min( dwMaxSize, Registry::MaxSize() );

	m_bActive = bActive;
	m_dwBufferTime = dwBufferTime;
	m_hClientSubscription = hClientSubscription;

	// set default filter values to include all
	m_dwEventType = OPC_ALL_EVENTS;
	m_dwLowSeverity = 1;
	m_dwHighSeverity = 1000;

	m_bCancelRefresh = FALSE;

	m_dwKeepAliveModulo  = 0;
	m_dwNonUpdates  = 0;

	CoFileTimeNow( &m_ftcLastUpdate );
}


COPCEventSubscriptionMgt::~COPCEventSubscriptionMgt()
{
//	ATLTRACE( _T("Entering 	COPCEventSubscriptionMgt::~COPCEventSubscriptionMgt() %Xh %Xh\n"), m_pParent, this );
//	ATLTRACE( _T("Calling RemoveThread()\n") );
	RemoveThread();  // stop the thread pool thread
//	ATLTRACE( _T("RemoveThread() done\n") );
	
	m_csData.Lock();
	// call Release for everything left in my queues
	while( !m_RefreshQ.empty() )
	{
		m_RefreshQ.front()->Release();
		m_RefreshQ.pop();
	}

	while( !m_OnEventQ.empty() )
	{
		m_OnEventQ.front()->Release();
		m_OnEventQ.pop();
	}

	
	m_csData.Unlock();
	if( m_pParent )
		m_pParent->Remove( this );
//	ATLTRACE( _T("Leaving 	COPCEventSubscriptionMgt::~COPCEventSubscriptionMgt()\n") );
}


bool COPCEventSubscriptionMgt::MatchesFilter( OnEventClass* pOnEventClass, const AREA_VECTOR& areas  ) const
{
	DWORD i;

	if( !(pOnEventClass->dwEventType & m_dwEventType) ) 	
		return FALSE;

	if( pOnEventClass->dwSeverity < m_dwLowSeverity ) 
		return FALSE;

	if( pOnEventClass->dwSeverity > m_dwHighSeverity ) 
		return FALSE;

	if( !m_EventCategoryVector.empty() )
	{
		for( i = 0; i < m_EventCategoryVector.size(); i++ )
		{
			if( m_EventCategoryVector[i] == pOnEventClass->dwEventCategory )
				break; // found
		}

		if( i >= m_EventCategoryVector.size() )
			return FALSE;	// category not found
	}



	if( !m_AreaVector.empty() )
	{
		register BOOL bAreaMatch = FALSE;
		for( i = 0; !bAreaMatch && i < m_AreaVector.size(); i++ )
		{
			for( DWORD j = 0; !bAreaMatch && j < areas.size(); j++ )
				bAreaMatch = MatchPattern( areas[j].data(), m_AreaVector[i].data() );
		}

		if( !bAreaMatch )
			return FALSE;	// no area match
	}
	

	if( !m_GenIDVector.empty() )
	{
		register BOOL bGenIDMatch = FALSE;
		for( i = 0; !bGenIDMatch && i < m_GenIDVector.size(); i++ )
		{
			bGenIDMatch = MatchPattern( pOnEventClass->szSource, m_GenIDVector[i].data() );
		}
		if( !bGenIDMatch )
			return FALSE;     // no GenID match
	}

	return TRUE;
}



void COPCEventSubscriptionMgt::ProcessNewEvent( OnEventClass* pOnEventClass, const AREA_VECTOR& areas  )
{
	ThreadSafe<CComAutoCriticalSection> AutoLock(m_csData);  // calls Lock() and Unlock() in constructor/destructor
	
	if( Active())
	{
		// compare to my filter, if it matches
		// call pOnEventClass->AddRef and add it to my queue

		if( MatchesFilter( pOnEventClass, areas ) )
		{
			// A robust server would have an upper bound on the number
			// of events it would cue for a client and check here if the limit
			// was reached.  If so an Event of category "OPC SERVER ERROR"
			// would be placed in the queue instead of the the one that is passed
			// in to this function.  This would indicate to the client that 
			// data has been thrown on the floor and a refresh should be 
			// requested by the client to stay in sync.
	
			// As an alternative in this implementation, if a client hangs, 
			// and no longer accepts callbacks, we just keep adding events 
			// to the queue until we run out of memory and crash!  
			// Have a nice day!

			pOnEventClass->AddRef();
			m_OnEventQ.push( pOnEventClass );
		}

		// Even if this event was filtered out, check to see if it 
		// is time to send anything that is already in my queue.

		if( !m_OnEventQ.empty() )
		{
			FileTimeClass now;
			CoFileTimeNow( &now );
			FileTimeClass ftcNextUpdate( FileTimeClass::milliseconds( m_dwBufferTime ) );
			ftcNextUpdate += m_ftcLastUpdate;
			FileTimeClass ftcNextSimulation( FileTimeClass::milliseconds( Registry::SimulationPeriod() ));
			ftcNextSimulation += now;
			if( m_OnEventQ.size() >= m_dwMaxSize ||
				ftcNextUpdate < ftcNextSimulation )
			{
				QueueThreadWork();
			}
		}
		else // m_OnEventQ.empty()
		{
			if( m_dwKeepAliveModulo )
			{
				++m_dwNonUpdates;
				if( m_dwNonUpdates >= m_dwKeepAliveModulo )
				{
					m_dwNonUpdates = 0;
					QueueThreadWork();  // send keep alive callback
				}
			}
		}
	}
}


// send data from one of the queues to the client 
void COPCEventSubscriptionMgt::SendEvents( ON_EVENT_Q& q, BOOL bRefresh )
{
	m_csData.Lock();
	OPCHANDLE hClientSubscription = m_hClientSubscription;
	DWORD dwCount = min( q.size(), m_dwMaxSize );
	BOOL bLastRefresh = ( dwCount == q.size() ) && bRefresh;
	SubscribedEventClass *pEvents = new SubscribedEventClass[ dwCount ];
	OnEventClass **ppDequed = new OnEventClass*[ dwCount ];
    DWORD i;
	for( i = 0; i < dwCount; i++ )
	{
		pEvents[i].copy(*q.front(), m_ReturnedAttributesSet);  // copy the struct to array to be sent
		ppDequed[i] = q.front();  // save pointer in our private array so we can call Release after data is sent
		q.pop(); // remove the pointer from the queue
	}

	// localize the events.
	OpcLocalizeONEVENTSTRUCTs(m_lcid, dwCount, pEvents);

	m_csData.Unlock();

	bool bUpdated = false;
	Lock();  // lock to use m_vec
	for( IUnknown** ppUnk = m_vec.begin(); // m_vec is a member  of IConnectionPointImpl
			 ppUnk < m_vec.end();
			 ppUnk++ )
	{
		if( *ppUnk != NULL )
		{
			if( dwCount ) // don't update time for a keep alive callback
			{
				CoFileTimeNow( &m_ftcLastUpdate );
				bUpdated = true;
			}

			IOPCEventSink *pSink = (IOPCEventSink *)*ppUnk;
			pSink->OnEvent( hClientSubscription, bRefresh, bLastRefresh,
							dwCount, pEvents );
			ATLTRACE( "Called OnEvent() in client %Xh %Xh\n", m_pParent, this );
		}
	}
	Unlock();

	if( m_pParent && bUpdated )
		m_pParent->LastUpdateTime( m_ftcLastUpdate );


	// call release for each of the OnEventClass objects we just sent
	for( i = 0; i < dwCount; i++ )
		ppDequed[i]->Release();

	delete [] pEvents;
	delete [] ppDequed;
	if( bRefresh && bLastRefresh )
		ATLTRACE( _T("done SendEvents() for Last Refresh\n") );
}

void COPCEventSubscriptionMgt::SendCancelRefresh()
{
	m_csData.Lock();
	OPCHANDLE hClientSubscription = m_hClientSubscription;
	DWORD dwCount = 0;
	BOOL bRefresh = TRUE;
	BOOL bLastRefresh = TRUE;
	HRESULT hr = S_OK;
	DWORD dummy = 0;

	while( !m_RefreshQ.empty() )
	{
		m_RefreshQ.front()->Release();
		m_RefreshQ.pop();
	}

	m_bCancelRefresh = FALSE;
	m_csData.Unlock();

	Lock();  // lock to use m_vec
	for( IUnknown** ppUnk = m_vec.begin(); // m_vec is a member  of IConnectionPointImpl
			 ppUnk < m_vec.end();
			 ppUnk++ )
	{
		if( *ppUnk != NULL )
		{
			IOPCEventSink *pSink = (IOPCEventSink *)*ppUnk;
			hr = pSink->OnEvent( hClientSubscription, bRefresh, bLastRefresh,
						dwCount, (SubscribedEventClass *)&dummy  );
		}
	}
	Unlock();
	ATLTRACE( _T("done SendCancelRefresh()\n") );
}



void COPCEventSubscriptionMgt::ThreadWork()
{
	m_csData.Lock();
	  DWORD dwEventQSize = m_OnEventQ.size();
	  if( dwEventQSize )	
		m_dwNonUpdates = 0;  // reset non-update counter

	  BOOL bCancelRefresh = m_bCancelRefresh;
	  DWORD dwRefreshQSize = m_RefreshQ.size();
	  m_bCancelRefresh = FALSE;
	m_csData.Unlock();

	// new events have priority over refresh events.
	if( bCancelRefresh )
		SendCancelRefresh();
	if( dwEventQSize )
		SendEvents( m_OnEventQ, FALSE );
	if( dwRefreshQSize )
		SendEvents( m_RefreshQ, TRUE );

	if( !bCancelRefresh && !dwEventQSize && !dwRefreshQSize )
		SendEvents( m_OnEventQ, FALSE );  // keep-alive callback


	m_csData.Lock();
	  if(!m_OnEventQ.empty() || !m_RefreshQ.empty() )
		  QueueThreadWork();
	m_csData.Unlock();
}

        
        
        
        

HRESULT COPCEventSubscriptionMgt::SetFilter( 
            /* [in] */ DWORD dwEventType,
            /* [in] */ DWORD dwNumCategories,
            /* [size_is][in] */ DWORD __RPC_FAR *pdwEventCategories,
            /* [in] */ DWORD dwLowSeverity,
            /* [in] */ DWORD dwHighSeverity,
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszAreaList,
            /* [in] */ DWORD dwNumSources,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszSourceList)
{
	DWORD i;
	// these locals are here just to lock the critical sections
	SourceMapPtr pSourceMap;  // get a pointer to the sources/conditions and lock it with a critical section
	ThreadSafeGlobal tsg;

	ThreadSafe<CComAutoCriticalSection> AutoLock(m_csData);  // calls Lock() and Unlock() in constructor/destructor

	if( !( (dwEventType & OPC_SIMPLE_EVENT) ||
		   (dwEventType & OPC_TRACKING_EVENT) ||
		   (dwEventType & OPC_CONDITION_EVENT) ) )
	{
		return E_INVALIDARG;  // dwEventType does not specify any known event types
	}

	
	for( i = 0; i < dwNumAreas; i++ )
	{
		if( pszAreaList[i] == NULL )
			return E_INVALIDARG;
		if( wcspbrk( pszAreaList[i], WILD_CHARS ) == NULL )
		{   // if there are no wildcard chars, then make sure there is an exact match
			if( !theAreaRootNode.find( pszAreaList[i], COPCEventAreaBrowser::seps ))
				return E_INVALIDARG;
		}
	}

	for( i = 0; i < dwNumSources; i++ )
	{
		if( pszSourceList[i] == NULL )
			return E_INVALIDARG;
		if( wcspbrk( pszSourceList[i], WILD_CHARS ) == NULL )
		{   // if there are no wildcard chars, then make sure there is an exact match
			SourceMap::iterator itSource = pSourceMap->find( pszSourceList[i] );
			if( itSource == pSourceMap->end() )
				return E_INVALIDARG;		// szSource was not found
		}
	}
	if( dwLowSeverity < 1 || dwHighSeverity > 1000 )
		return E_INVALIDARG;

	if( dwLowSeverity > dwHighSeverity )
		return E_INVALIDARG;

		for( i = 0; i < dwNumCategories; i++ )
	{
		if( pdwEventCategories[i] > theCategoriesMap.size() )
			return E_INVALIDARG;
	}


	if( !m_RefreshQ.empty() )
		return OPC_E_BUSY;
	
	m_dwEventType = dwEventType;
	m_EventCategoryVector.clear();
	for( i = 0; i < dwNumCategories; i++ )
	{
		m_EventCategoryVector.push_back( pdwEventCategories[i] );
	}

	m_dwLowSeverity = dwLowSeverity;
	m_dwHighSeverity = dwHighSeverity;

	m_AreaVector.clear();
	for( i = 0; i < dwNumAreas; i++ )
		m_AreaVector.push_back( wstring( pszAreaList[i] ) );

	m_GenIDVector.clear();
	for( i = 0; i < dwNumSources; i++ )
		m_GenIDVector.push_back( wstring( pszSourceList[i] ) );
	

	return S_OK;
}
  


HRESULT COPCEventSubscriptionMgt::GetFilter( 
            /* [out] */ DWORD __RPC_FAR *pdwEventType,
            /* [out] */ DWORD __RPC_FAR *pdwNumCategories,
            /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppdwEventCategories,
            /* [out] */ DWORD __RPC_FAR *pdwLowSeverity,
            /* [out] */ DWORD __RPC_FAR *pdwHighSeverity,
            /* [out] */ DWORD __RPC_FAR *pdwNumAreas,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszAreaList,
            /* [out] */ DWORD __RPC_FAR *pdwNumSources,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszSourceList)
{
	ThreadSafe<CComAutoCriticalSection> AutoLock(m_csData);  // calls Lock() and Unlock() in constructor/destructor
	
	*ppdwEventCategories = NULL;
	*ppszAreaList = NULL;
	*ppszSourceList = NULL;

	*pdwEventType = m_dwEventType;
	*pdwNumCategories = m_EventCategoryVector.size();
	*pdwLowSeverity = m_dwLowSeverity;
	*pdwHighSeverity = m_dwHighSeverity;
	*pdwNumAreas = m_AreaVector.size();
	*pdwNumSources = m_GenIDVector.size();
    DWORD i;

	try
	{
		if( *pdwNumCategories )
			*ppdwEventCategories = (DWORD *)CoTaskAlloc( *pdwNumCategories * sizeof(DWORD) );
		if( *pdwNumAreas )
			*ppszAreaList = (LPWSTR *)CoTaskAlloc( *pdwNumAreas * sizeof(LPWSTR) );
		if( *pdwNumSources )
			*ppszSourceList = (LPWSTR *)CoTaskAlloc( *pdwNumSources * sizeof(LPWSTR) );

		for( i = 0; i < *pdwNumCategories; i++ )
			(*ppdwEventCategories)[i] = m_EventCategoryVector[i];
		
		for( i = 0; i < *pdwNumAreas; i++ )
			(*ppszAreaList)[i] = CoTaskAlloc( m_AreaVector[i].data() );

		for( i = 0; i < *pdwNumSources; i++ )
			(*ppszSourceList)[i] = CoTaskAlloc( m_GenIDVector[i].data() );
	}
	catch( ... )
	{
		if( *ppszAreaList )
		{
			for( DWORD i = 0; i < *pdwNumAreas; i++ )
				CoTaskMemFree( (*ppszAreaList)[i] );
			CoTaskMemFree( *ppszAreaList );
		}
		if( *ppszSourceList )
		{
			for( DWORD i = 0; i < *pdwNumAreas; i++ )
				CoTaskMemFree( (*ppszSourceList)[i] );
			CoTaskMemFree( *ppszSourceList );
		}
		CoTaskMemFree( *ppdwEventCategories );
		return E_OUTOFMEMORY;
	}


	return S_OK;
}

       
HRESULT COPCEventSubscriptionMgt::SelectReturnedAttributes( 
            /* [in] */ DWORD dwEventCategory,
            /* [in] */ DWORD dwCount,
            /* [size_is][in] */ DWORD __RPC_FAR *dwAttributeIDs)
{
	ThreadSafe<CComAutoCriticalSection> AutoLock(m_csData);  // calls Lock() and Unlock() in constructor/destructor
    DWORD i;
    if( dwEventCategory == 0 || dwEventCategory > theCategoriesMap.size() )
		return E_INVALIDARG;

	for( i = 0; i < dwCount; i++ )
	{
		if( dwAttributeIDs[i] >= (theAttributesVector.size() + nStdAttrs()) )
			return E_INVALIDARG;
	}

	ReturnedAttributes test(  dwEventCategory );

	// remove the element from the set
	RETURNED_ATTRIBUTES_SET::iterator it = m_ReturnedAttributesSet.find( test );
	if( it != m_ReturnedAttributesSet.end() )
		m_ReturnedAttributesSet.erase( it );

	pair <RETURNED_ATTRIBUTES_SET::iterator, bool> rasPair;
	rasPair = m_ReturnedAttributesSet.insert( test );
	// need to cast away const
	ReturnedAttributes& ra = (ReturnedAttributes&)*(rasPair.first);
	ra.AttributeVector().resize( dwCount );
	for(  i = 0; i < dwCount; i++ )
		ra.AttributeVector()[i] =  dwAttributeIDs[i];

	return S_OK;
}


HRESULT STDMETHODCALLTYPE COPCEventSubscriptionMgt::GetReturnedAttributes( 
            /* [in] */ DWORD dwEventCategory,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppdwAttributeIDs)
{
	ThreadSafe<CComAutoCriticalSection> AutoLock(m_csData);  // calls Lock() and Unlock() in constructor/destructor
	if( dwEventCategory == 0 || dwEventCategory > theCategoriesMap.size() )
		return E_INVALIDARG;
	ReturnedAttributes test(  dwEventCategory );

	RETURNED_ATTRIBUTES_SET::iterator it;

	*pdwCount = 0;

	it = m_ReturnedAttributesSet.find( test );
	if( it != m_ReturnedAttributesSet.end() )
	{
		*pdwCount = (*it).AttributeVector().size();
		ATLTRY(*ppdwAttributeIDs = (DWORD *)CoTaskAlloc( *pdwCount * sizeof(DWORD) ));
		if( !*ppdwAttributeIDs )
			return E_OUTOFMEMORY;
		for( DWORD i = 0; i < *pdwCount; i++ )
			(*ppdwAttributeIDs)[i] = (*it).AttributeVector()[i];
	}


	return S_OK;
}






void ForEachCondition::operator()( ConditionMap::value_type & Value )
{
	if( Value.second.IsEnabled() &&
				(Value.second.IsActive() || !Value.second.IsAcked()) )
	{
		OnEventClass *pOnEventClass = new OnEventClass( m_SubMgt.m_wsTempSource.data(), Value.first.data(), 
													Value.second );
		if( m_SubMgt.MatchesFilter( pOnEventClass, Value.second.Areas() ) )
			m_SubMgt.m_RefreshQ.push( pOnEventClass );
		else
			pOnEventClass->Release();
	}
}



void ForEachSource::operator()( SourceMap::value_type & Value )
{
	// save the source name in a private data member
	m_SubMgt.m_wsTempSource = Value.first;

	// call operator()( ConditionMap::value_type & Value ) for each condition
	for_each( Value.second.begin(), Value.second.end(), ForEachCondition(m_SubMgt) );
}



        
HRESULT COPCEventSubscriptionMgt::Refresh( 
            /* [in] */ DWORD dwConnection)
{
	ATLTRACE( _T("Refresh( %lu )\n"), dwConnection );

	SourceMapPtr pSourceMap;  // get a pointer to the sources/conditions and lock it with a critical section
	ThreadSafe<CComAutoCriticalSection> AutoLock(m_csData);  // calls Lock() and Unlock() in constructor/destructor


	if( !m_RefreshQ.empty() )
		return OPC_E_BUSY;

	// call operator()( SourceMap::value_type & Value ) for each source
	for_each( pSourceMap->begin(), pSourceMap->end(), ForEachSource(*this) );
	if( !m_RefreshQ.empty() )
		QueueThreadWork();
	else
		CancelRefresh( dwConnection );  // nothing to send so cancel

	return S_OK;
}
        
HRESULT COPCEventSubscriptionMgt::CancelRefresh( 
            /* [in] */ DWORD dwConnection)
{
	HRESULT rtn = E_FAIL;
	ATLTRACE( _T("CancelRefresh( %lu )\n"), dwConnection );

	ThreadSafe<CComAutoCriticalSection> AutoLock(m_csData);  // calls Lock() and Unlock() in constructor/destructor
	if( !m_bCancelRefresh )
	{
		m_bCancelRefresh = TRUE;
		QueueThreadWork();
		rtn = S_OK;
	}

	return rtn;
}



        
HRESULT COPCEventSubscriptionMgt::GetState( 
            /* [out] */ BOOL __RPC_FAR *pbActive,
            /* [out] */ DWORD __RPC_FAR *pdwBufferTime,
            /* [out] */ DWORD __RPC_FAR *pdwMaxSize,
            /* [out] */ OPCHANDLE __RPC_FAR *phClientSubscription)
{
	ThreadSafe<CComAutoCriticalSection> AutoLock(m_csData);  // calls Lock() and Unlock() in constructor/destructor

	*pbActive = Active();
	*pdwBufferTime = BufferTime();
	*pdwMaxSize = MaxSize();
	*phClientSubscription = ClientSubscription();

	return S_OK;
}
        


HRESULT COPCEventSubscriptionMgt::SetState( 
            /* [in][unique] */ BOOL __RPC_FAR *pbActive,
            /* [in][unique] */ DWORD __RPC_FAR *pdwBufferTime,
            /* [in][unique] */ DWORD __RPC_FAR *pdwMaxSize,
            /* [in] */ OPCHANDLE hClientSubscription,
            /* [out] */ DWORD __RPC_FAR *pdwRevisedBufferTime,
            /* [out] */ DWORD __RPC_FAR *pdwRevisedMaxSize)
{
	ThreadSafe<CComAutoCriticalSection> AutoLock(m_csData);  // calls Lock() and Unlock() in constructor/destructor

	if( pdwMaxSize )
	{
		if( *pdwMaxSize == 0 )
			MaxSize( Registry::MaxSize() );
		else
			MaxSize( min( Registry::MaxSize(), *pdwMaxSize) );
	}

	if( pbActive )
		Active( *pbActive );

	if( pdwBufferTime )
		BufferTime( *pdwBufferTime );

	ClientSubscription( hClientSubscription );

	*pdwRevisedBufferTime = BufferTime();
	*pdwRevisedMaxSize = MaxSize();

	if( pdwBufferTime )
	{
		if( *pdwRevisedBufferTime != *pdwBufferTime )
			return OPC_S_INVALIDBUFFERTIME;
	}

	if( pdwMaxSize )
	{
		if( *pdwRevisedMaxSize != *pdwMaxSize )
			return OPC_S_INVALIDMAXSIZE;
	}

	return S_OK;
}


HRESULT COPCEventSubscriptionMgt::SetKeepAlive( 
            /* [in] */ DWORD dwKeepAliveTime,
            /* [out] */ DWORD __RPC_FAR *pdwRevisedKeepAliveTime)
{
	// In this server the dwKeepAliveTime must be a multiple of the simulation period

	m_dwKeepAliveModulo = dwKeepAliveTime / Simulation::theSimulation.SimulationMsecs();
	if( m_dwKeepAliveModulo == 0  && dwKeepAliveTime != 0 )
		m_dwKeepAliveModulo = 1;

	*pdwRevisedKeepAliveTime = m_dwKeepAliveModulo * Simulation::theSimulation.SimulationMsecs();

	return *pdwRevisedKeepAliveTime == dwKeepAliveTime ? S_OK : OPC_S_INVALIDKEEPALIVETIME;
}
        
HRESULT COPCEventSubscriptionMgt::GetKeepAlive( 
            /* [out] */ DWORD __RPC_FAR *pdwKeepAliveTime)
{
	*pdwKeepAliveTime = m_dwKeepAliveModulo * Simulation::theSimulation.SimulationMsecs();
	return S_OK;
}
