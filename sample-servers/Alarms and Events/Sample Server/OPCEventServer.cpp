// OPCEventServer.cpp
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
/*   $History: OPCEventServer.cpp $
 * 
 * *****************  Version 31  *****************
 * User: Jiml         Date: 1/07/03    Time: 5:41p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * minor source changes to build in VC7
 * 
 * *****************  Version 30  *****************
 * User: Jiml         Date: 12/08/02   Time: 3:40p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * Added Keep-alive
 * 
 * *****************  Version 28  *****************
 * User: Jiml         Date: 8/02/02    Time: 4:47p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 27  *****************
 * User: Jiml         Date: 1/18/02    Time: 3:35p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 26  *****************
 * User: Jiml         Date: 8/14/01    Time: 12:50p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 25  *****************
 * User: Jiml         Date: 11/18/98   Time: 3:08p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 24  *****************
 * User: Jiml         Date: 11/12/98   Time: 11:43a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * Modified to reflect interface change in TranslateToItemIDs()
 * 
 * *****************  Version 23  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:56a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 22  *****************
 * User: Jiml         Date: 8/10/98    Time: 6:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 *  1. Added AckComment attribute
 *  2. Added Areas attribute
 *  3. revised ::GetConditionState() to include attributes
 * 
 * *****************  Version 21  *****************
 * User: Jiml         Date: 7/17/98    Time: 10:32a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 20  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 19  *****************
 * User: Jiml         Date: 4/06/98    Time: 6:22p
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
 * User: Jiml         Date: 3/13/98    Time: 6:46p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 3/11/98    Time: 4:34p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 3/10/98    Time: 6:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 3/06/98    Time: 6:02p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 3/04/98    Time: 4:14p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 2/26/98    Time: 11:04a
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
// OPCEventServer.cpp : Implementation of COPCEventServer
#include "stdafx.h"
#include "OPC_AE_SampleServer.h"
#include "OPCEventServer.h"
#include "global.h"
#include "QueryContainers.h"
#include "OPCEventSubscriptionMgt.h"
#include "ConditionMap.h"
#include "OPCEventAreaBrowser.h"

#define OPC_AE_PROXY	_T("opc_aeps")

/////////////////////////////////////////////////////////////////////////////
// COPCEventServer

COPCEventServer::COPCEventServer()
{
	m_lcid = LOCALE_SYSTEM_DEFAULT;
	
	ThreadSafeGlobal tsg;

	// add myselft to the global set
	tsg->m_EvServerSet.insert( this );
}


COPCEventServer::~COPCEventServer()
{
	ThreadSafeGlobal tsg;

	// remove myself from the global list
	tsg->m_EvServerSet.erase( tsg->m_EvServerSet.find( this ) );
}



void COPCEventServer::Remove( COPCEventSubscriptionMgt *pMgt )
{
	ThreadSafe<COPCEventServer> AutoLock(*this);  // calls Lock() and Unlock() in constructor/destructor

	m_ActiveSubscriptionSet.erase( 
				m_ActiveSubscriptionSet.find( pMgt ) );

}


void COPCEventServer::ProcessNewEvent( OnEventClass* pOnEventClass, const AREA_VECTOR& areas  )
{
	ThreadSafe<COPCEventServer> AutoLock(*this);  // calls Lock() and Unlock() in constructor/destructor

	EVSUBMGT_SET::iterator it;
	for( it = m_ActiveSubscriptionSet.begin(); 
				it != m_ActiveSubscriptionSet.end();
				it++ )
	{
		(*it)->ProcessNewEvent( pOnEventClass, areas );
	}

}



 

FileTimeClass COPCEventServer::LastUpdateTime()
{
	ThreadSafe<COPCEventServer> AutoLock(*this);  // calls Lock() and Unlock() in constructor/destructor
	return m_ftLastUpdateTime;
}


void COPCEventServer::LastUpdateTime( const FILETIME& ft )
{
	ThreadSafe<COPCEventServer> AutoLock(*this);  // calls Lock() and Unlock() in constructor/destructor
	m_ftLastUpdateTime = ft;
}
    



        
HRESULT  COPCEventServer::CreateEventSubscription( 
            /* [in] */ BOOL bActive,
            /* [in] */ DWORD dwBufferTime,
            /* [in] */ DWORD dwMaxSize,
            /* [in] */ OPCHANDLE hClientSubscription,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ LPUNKNOWN __RPC_FAR *ppUnk,
            /* [out] */ DWORD __RPC_FAR *pdwRevisedBufferTime,
            /* [out] */ DWORD __RPC_FAR *pdwRevisedMaxSize)
{

	CComObject<COPCEventSubscriptionMgt>* pSubMgt = NULL;
	ATLTRY(pSubMgt = new CComObject<COPCEventSubscriptionMgt>);
	pSubMgt->_AtlInitialConstruct();
	pSubMgt->SetLocaleID(m_lcid);
	
	if( !pSubMgt )
		return E_OUTOFMEMORY;

	pSubMgt->Init(
		*this, bActive, dwBufferTime, dwMaxSize, hClientSubscription );

	*pdwRevisedBufferTime = pSubMgt->BufferTime();
	*pdwRevisedMaxSize = pSubMgt->MaxSize();


	Lock();
	m_ActiveSubscriptionSet.insert( pSubMgt );
	Unlock();

	HRESULT rtn = pSubMgt->_InternalQueryInterface( riid, (LPVOID*)ppUnk );
	if( FAILED(rtn) )
		delete pSubMgt;
	else
	{
		if( pSubMgt->BufferTime() != dwBufferTime )
			rtn = OPC_S_INVALIDBUFFERTIME;
		else if( pSubMgt->MaxSize() != dwMaxSize )
			rtn = OPC_S_INVALIDMAXSIZE;
	}

	return rtn;
}
        
HRESULT  COPCEventServer::QueryAvailableFilters( 
            /* [out] */ DWORD __RPC_FAR *pdwFilterMask)
{
	*pdwFilterMask = OPC_FILTER_BY_EVENT |
					OPC_FILTER_BY_CATEGORY |
					OPC_FILTER_BY_SEVERITY |
					OPC_FILTER_BY_AREA |
					OPC_FILTER_BY_SOURCE;
	

	return S_OK;
}
        
HRESULT  COPCEventServer::QueryEventCategories( 
            /* [in] */ DWORD dwEventType,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppdwEventCategories,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppEventCategoryDescs)
{
	if( dwEventType == 0 || dwEventType > (OPC_SIMPLE_EVENT | OPC_TRACKING_EVENT | OPC_CONDITION_EVENT) )
		return E_INVALIDARG;

	*pdwCount = theCategoriesMap.size();
	*ppdwEventCategories = NULL;
	*ppEventCategoryDescs = NULL;
	try 
	{
		*ppdwEventCategories = (DWORD *)CoTaskAlloc( *pdwCount * sizeof(DWORD) );
		*ppEventCategoryDescs = (LPWSTR *)CoTaskAlloc( *pdwCount * sizeof( LPWSTR ) );
		int i = 0;
		CATEGORIES_MAP::iterator it;
		for( it = theCategoriesMap.begin(); it != theCategoriesMap.end(); it++ )
		{
			if( ((*it).second.dwEventType & dwEventType) != 0 )  // one or more event types match
			{
				// (*it).first is a wstring (EventCategory Description)
				(*ppEventCategoryDescs)[i] = CoTaskAlloc( (*it).first.data() );
				// (*it).second.dwID is a DWORD (EventCategory)
				(*ppdwEventCategories)[i] = (*it).second.dwID;
				++i;
			}
		}
		if( i != *pdwCount )	// shorten the arrays
		{		
			*pdwCount = i;
			*ppdwEventCategories = (DWORD *)CoTaskMemRealloc( *ppdwEventCategories, *pdwCount * sizeof(DWORD) );
			*ppEventCategoryDescs = (LPWSTR *)CoTaskMemRealloc( *ppEventCategoryDescs, *pdwCount * sizeof( LPWSTR ) );
		}
	}
	catch( ... )	// catch any allocation errors
	{
		if( *ppdwEventCategories )
			CoTaskMemFree( *ppdwEventCategories );
		if( *ppEventCategoryDescs )
		{
			for( DWORD i = 0; i < *pdwCount; i++ )
				CoTaskMemFree( (*ppEventCategoryDescs)[i] );
			CoTaskMemFree( *ppEventCategoryDescs );
		}
		return E_OUTOFMEMORY;
	}
	
	return S_OK;
}
        
HRESULT  COPCEventServer::QueryEventAttributes( 
            /* [in] */ DWORD dwEventCategory,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppAttrIDs,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszAttrDescs,
            /* [size_is][size_is][out] */ VARTYPE __RPC_FAR *__RPC_FAR *ppAttrTypes)
{
	// In this implementation, all dwEventCategories have the same Event
	// Attributes and they are all BSTRs

	*pdwCount = 0;
	*ppAttrIDs =  NULL;
	*ppszAttrDescs = NULL;
	*ppAttrTypes = NULL;

	
	// just to be picky, make sure we are passed a valid dwEventCategory
	CONDITIONNAME_MAP::iterator cidmIt;
	cidmIt = theConditionNameMap.find( dwEventCategory );
	if( cidmIt == theConditionNameMap.end() )
		return E_INVALIDARG;

	*pdwCount = theAttributesVector.size() + nStdAttrs();

	try
	{
		*ppAttrIDs =  (DWORD *)CoTaskAlloc( *pdwCount * sizeof(DWORD) );
		*ppszAttrDescs = (LPWSTR *)CoTaskAlloc( *pdwCount * sizeof( LPWSTR ) );
		*ppAttrTypes = (VARTYPE *)CoTaskAlloc( *pdwCount * sizeof( VARTYPE ) );

			
		(*ppAttrTypes)[ixAckComment] = VT_BSTR;
		(*ppAttrIDs)[ixAckComment] = ixAckComment;
		(*ppszAttrDescs)[ixAckComment] = CoTaskAlloc( wszStdAttrNames[ ixAckComment ] );
				
		(*ppAttrTypes)[ixAreas] = VT_ARRAY | VT_BSTR;
		(*ppAttrIDs)[ixAreas] = ixAreas;
		(*ppszAttrDescs)[ixAreas] = CoTaskAlloc( wszStdAttrNames[ ixAreas ] );


		for( DWORD i = nStdAttrs(); i < *pdwCount; i++ )
		{
			(*ppAttrTypes)[i] = VT_BSTR;
			(*ppAttrIDs)[i] = i;
			(*ppszAttrDescs)[i] = CoTaskAlloc( theAttributesVector[i - nStdAttrs()].data() );
		}
	}
	catch( ... )		// allocation error
	{
		if( *ppszAttrDescs )
		{
			for( DWORD i = 0; i < *pdwCount; i++ )
				CoTaskMemFree( (*ppszAttrDescs)[i] );
		}
		CoTaskMemFree( *ppszAttrDescs );
		CoTaskMemFree( *ppAttrIDs );
		CoTaskMemFree( *ppAttrTypes );

		return E_OUTOFMEMORY; 
	}
	return S_OK;
}
        
HRESULT  COPCEventServer::TranslateToItemIDs( 
            /* [in] */ LPWSTR szSource,
            /* [in] */ DWORD dwEventCategory,
            /* [in] */ LPWSTR szConditionName,
            /* [in] */ LPWSTR szSubconditionName,
            /* [in] */ DWORD dwCount,
            /* [size_is][in] */ DWORD __RPC_FAR *pdwAssocAttrIDs,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszAttrItemIDs,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszNodeNames,
            /* [size_is][out] */ CLSID __RPC_FAR **ppCLSIDs)
{
	// validate parameters.
	wstring wsSource( szSource );
	wstring wsConditionName( szConditionName );
	SourceMapPtr pSourceMap;  // get a pointer to the sources/conditions and lock it with a critical section

	DWORD i = 0;
	bool bFound = false;
	CATEGORIES_MAP::iterator it;

	for( it = theCategoriesMap.begin(); it != theCategoriesMap.end(); it++ )
	{
		if ((*it).second.dwID == dwEventCategory)
		{
			bFound = true;
			break;
		}
	}

	if (!bFound)
	{
		return E_INVALIDARG; // dwEventCategory was not found
	}

	SourceMap::iterator itSource = pSourceMap->find( wsSource );
	if( itSource == pSourceMap->end() )
		return E_INVALIDARG;		// szSource was not found

	ConditionMap& CondMap = (*itSource).second;

	ConditionMap::iterator itCond1 = CondMap.find( wsConditionName );
	if( itCond1 == CondMap.end() )
		return E_INVALIDARG;		// szConditionName was not found

	ConditionNameSet::iterator itCond2 = theConditionNameSet.find( wsConditionName );
	if( itCond2 == theConditionNameSet.end() )
		return E_INVALIDARG;	// szConditionName was not found

	bFound = false;
	DWORD dwNoOfSubconditions = (*itCond2).m_SubconditionNameSet.size();

	try
	{
		SUBCONDITIONNAME_SET::iterator itSub = (*itCond2).m_SubconditionNameSet.begin();

		for (i = 0; i < dwNoOfSubconditions; i++, itSub++)
		{
			if (wcscmp((*itSub).data(), szSubconditionName) == 0)
			{
				bFound = true;
				break;
			}
		}
	}
	catch( ... )
	{
		bFound = false;
	}

	if (!bFound)
	{
		return E_INVALIDARG; // szSubconditionName was not found
	}

	// create arbitrary item names for purposes of testing the client.
	*ppszAttrItemIDs = (LPWSTR*)CoTaskMemAlloc(sizeof(LPWSTR)*dwCount);
	*ppszNodeNames   = (LPWSTR*)CoTaskMemAlloc(sizeof(LPWSTR)*dwCount);
	*ppCLSIDs        = (CLSID*)CoTaskMemAlloc(sizeof(CLSID)*dwCount);

	for (DWORD ii = 0; ii < dwCount; ii++)
	{
		WCHAR szBuffer[MAX_PATH+1];

		swprintf(szBuffer, L"%s/%s/%d", szSource, szConditionName, pdwAssocAttrIDs[ii]);
		(*ppszAttrItemIDs)[ii] = (LPWSTR)CoTaskMemAlloc((sizeof(WCHAR)+1)*wcslen(szBuffer));
		wcscpy((*ppszAttrItemIDs)[ii], szBuffer);

		swprintf(szBuffer, L"localhost");
		(*ppszNodeNames)[ii] = (LPWSTR)CoTaskMemAlloc((sizeof(WCHAR)+1)*wcslen(szBuffer));
		wcscpy((*ppszNodeNames)[ii], szBuffer);

		(*ppCLSIDs)[ii] = CLSID_OPCEventServer;
	}

	return S_OK;
}
        

HRESULT  COPCEventServer::GetConditionState( 
            /* [in] */ LPWSTR szSource,
            /* [in] */ LPWSTR szConditionName,
            /* [in] */ DWORD dwNumEventAttrs,
            /* [size_is][in] */ DWORD __RPC_FAR *pdwAttributeIDs,
            /* [out] */ OPCCONDITIONSTATE __RPC_FAR *__RPC_FAR *ppConditionState)
{
	wstring wsSource( szSource );
	wstring wsConditionName( szConditionName );
	SourceMapPtr pSourceMap;  // get a pointer to the sources/conditions and lock it with a critical section

	SourceMap::iterator itSource = pSourceMap->find( wsSource );
	if( itSource == pSourceMap->end() )
		return E_INVALIDARG;		// szSource was not found

	ConditionMap& CondMap = (*itSource).second;

	ConditionMap::iterator itCond = CondMap.find( wsConditionName );
	if( itCond == CondMap.end() )
		return E_INVALIDARG;		// szConditionName was not found

	OPCCondition& OPCCond = (*itCond).second;
	

	*ppConditionState = OPCCond.CoTaskAllocConditionState( dwNumEventAttrs, pdwAttributeIDs );

	return *ppConditionState ? S_OK : E_OUTOFMEMORY;
}


HRESULT	 COPCEventServer::EnableSource( BOOL bEnable, LPCWSTR szSource )
{
	SourceMapPtr pSourceMap;  // get a pointer to the sources/conditions and lock it with a critical section
	ThreadSafeGlobal tsg;

	wstring wsSource( szSource );
	SourceMap::iterator itSource = pSourceMap->find( wsSource );
	if( itSource == pSourceMap->end() )
		return E_INVALIDARG;		// szSource was not found

	ConditionMap& CondMap = (*itSource).second;
	ConditionMap::iterator itCond;
	for( itCond = CondMap.begin(); itCond != CondMap.end(); itCond++ )
	{
		OPCCondition& OPCCond = (*itCond).second;
		if( OPCCond.IsEnabled() == bEnable )
			continue; // nothing has changed
		if( OPCCond.AreAreasEffectivelyEnabled() )
		{
			OPCCond.ClearChangeMask();
			OPCCond.Enable( bEnable );

			// only notify clients if the condition is Active or not Acked
			if( OPCCond.IsActive() || !OPCCond.IsAcked() )
				tsg->NotifyClients( szSource, (*itCond).first.data(), OPCCond );
		}
		else
		{ // silently update the source state

			OPCCond.Enable( bEnable );
		}
	}
	return S_OK;
}



void COPCEventServer::NotifyStateChange( LPCWSTR szSource )
{
	SourceMapPtr pSourceMap;  // get a pointer to the sources/conditions and lock it with a critical section
	ThreadSafeGlobal tsg;

//	wstring wsSource( szSource );
	SourceMap::iterator itSource = pSourceMap->find( szSource );
	if( itSource == pSourceMap->end() )
		return;		// szSource was not found

	ConditionMap& CondMap = (*itSource).second;
	ConditionMap::iterator itCond;
	for( itCond = CondMap.begin(); itCond != CondMap.end(); itCond++ )
	{
		OPCCondition& OPCCond = (*itCond).second;
		OPCCond.ClearChangeMask();
		if( OPCCond.UpdateEffectiveState() )
		{
			// only notify clients if the condition is Active or not Acked
			if( OPCCond.IsActive() || !OPCCond.IsAcked() )
				tsg->NotifyClients( szSource, (*itCond).first.data(), OPCCond );
		}
	}
}


void COPCEventServer::RecursiveNotifyStateChange( AreaNode& rNode )
{
	AreaNodeSet::iterator it;
	for( it = rNode.Children().begin();
					it !=  rNode.Children().end();
					it++ )
	{
		if( (*it).IsSource() )
			NotifyStateChange( (*it).Name() );
		else
			RecursiveNotifyStateChange(const_cast<AreaNode&>(*it) );
	}
}


HRESULT  COPCEventServer::EnableByArea( BOOL bEnable,
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszAreas,
            /* [size_is][size_is][out] */ HRESULT **ppErrors)

{
	HRESULT rtn = S_OK;
	if( ppErrors )
		*ppErrors = NULL;

	if( dwNumAreas == 0 )
		return E_INVALIDARG;

	if( ppErrors )
	{
		// allocate ppErrors 
		ATLTRY( *ppErrors = (HRESULT *)CoTaskAlloc( dwNumAreas * sizeof( HRESULT ) ) );
		if( !*ppErrors )
			return E_OUTOFMEMORY;
	}


	// these locals are here just to lock the critical sections
	SourceMapPtr pSourceMap;  // get a pointer to the sources/conditions and lock it with a critical section	ThreadSafeGlobal tsg;
	ThreadSafeGlobal tsg;

	DWORD i;
	// make sure all areas are valid
	if( !ppErrors )
	{
		for( i = 0; i < dwNumAreas; i++ )
		{
			const AreaNode *pNode = theAreaRootNode.find( pszAreas[i], COPCEventAreaBrowser::seps );
			if( pNode && wcslen( pszAreas[i] ) )  // found the area
				continue;
			return E_INVALIDARG;
		}
	}
	

	for( i = 0; i < dwNumAreas; i++ )
	{
		AreaNode *pNode = theAreaRootNode.find( pszAreas[i], COPCEventAreaBrowser::seps );
		if( pNode && wcslen( pszAreas[i] ) )  // found the area
		{
			pNode->Enable( bEnable );
			RecursiveNotifyStateChange( *pNode );
		}
		else
		{
			if( ppErrors )
				(*ppErrors)[i] = E_INVALIDARG;
			rtn = S_FALSE;
		}
	}
	return  rtn;
}


HRESULT  COPCEventServer::EnableBySource( BOOL bEnable,
            /* [in] */ DWORD dwNumSources,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszSources,
            /* [size_is][size_is][out] */ HRESULT **ppErrors)

{
	HRESULT rtn = S_OK;
	if( ppErrors )
		*ppErrors = NULL;
	if( dwNumSources == 0 )
		return E_INVALIDARG;

	if( ppErrors )
	{
		// allocate ppErrors 
		ATLTRY( *ppErrors = (HRESULT *)CoTaskAlloc( dwNumSources * sizeof( HRESULT ) ) );
		if( !*ppErrors )
			return E_OUTOFMEMORY;
	}


	// these locals are here just to lock the critical sections
	SourceMapPtr pSourceMap;  // get a pointer to the sources/conditions and lock it with a critical section
	ThreadSafeGlobal tsg;
	
	DWORD i;

	// make sure all sources are valid
	if( !ppErrors )
	{
		for( i = 0; i < dwNumSources; i++ )
		{
			SourceMap::iterator itSource = pSourceMap->find( pszSources[i] );
			if( itSource == pSourceMap->end() )
					return E_INVALIDARG;		// szSource was not found
		}
	}


	for( i = 0; i < dwNumSources; i++ )
	{
		 HRESULT hr = EnableSource( bEnable, pszSources[i] );
		 if( ppErrors )
		 {
			 (*ppErrors)[i] = hr;
			 if( FAILED(hr) )
				 rtn = S_FALSE;
		 }
	}

	return rtn;
}


HRESULT  COPCEventServer::EnableConditionByArea( 
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszAreas)
{
	return EnableByArea( TRUE, dwNumAreas, pszAreas, NULL );
}





        
HRESULT  COPCEventServer::EnableConditionBySource( 
            /* [in] */ DWORD dwNumSources,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszSources)
{
	return EnableBySource( TRUE, dwNumSources, pszSources, NULL );
}


        
HRESULT  COPCEventServer::DisableConditionByArea( 
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszAreas)
{
	return EnableByArea( FALSE, dwNumAreas, pszAreas, NULL );
}


        
HRESULT  COPCEventServer::DisableConditionBySource( 
            /* [in] */ DWORD dwNumSources,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszSources)
{
	return EnableBySource( FALSE, dwNumSources, pszSources, NULL );
}




HRESULT  COPCEventServer::AckCondition( 
             LPWSTR szAcknowledgerID,
             LPWSTR szComment,
             LPWSTR szSource,
             LPWSTR szConditionName,
             FILETIME ftActiveTime,
             DWORD dwCookie )
{
	wstring wsSource( szSource );
	wstring wsConditionName( szConditionName );
	SourceMapPtr pSourceMap;  // get a pointer to the sources/conditions and lock it with a critical section


	SourceMap::iterator itSource = pSourceMap->find( wsSource );
	if( itSource == pSourceMap->end() )
		return E_INVALIDARG;		// szSource was not found

	ConditionMap& CondMap = (*itSource).second;

	ConditionMap::iterator itCond = CondMap.find( wsConditionName );
	if( itCond == CondMap.end() )
		return E_INVALIDARG;		// szConditionName was not found

	OPCCondition& OPCCond = (*itCond).second;

	if( OPCCond.Cookie() != dwCookie )
		return E_INVALIDARG;

	if( OPCCond.SubconditionTime() != ftActiveTime )
		return OPC_E_INVALIDTIME;

	if( OPCCond.IsAcked() )
		return OPC_S_ALREADYACKED;

	if( !OPCCond.IsEffectivelyEnabled() )
		return E_INVALIDARG;  // found the unacked condition but it is disabled


	OPCCond.ClearChangeMask();
	if( OPCCond.Ack( szAcknowledgerID, szComment ) )
	{
		// send to all clients
		ThreadSafeGlobal tsg;
		tsg->NotifyClients( szSource, szConditionName, OPCCond );
		return S_OK;
	}

	return E_FAIL;	// this should never happen!
}


HRESULT  COPCEventServer::AckCondition( 
            /* [in] */ DWORD dwCount,
            /* [string][in] */ LPWSTR szAcknowledgerID,
            /* [string][in] */ LPWSTR szComment,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszSource,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszConditionName,
            /* [size_is][in] */ FILETIME __RPC_FAR *pftActiveTime,
            /* [size_is][in] */ DWORD __RPC_FAR *pdwCookie,
            /* [size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors)
{
	// make sure szAcknowledgerID is not empty or NULL
	if( !szAcknowledgerID )
		return E_INVALIDARG;
	if( *szAcknowledgerID == 0 )
		return E_INVALIDARG;

	if( dwCount == 0 )
		return E_INVALIDARG;

	// allocate ppErrors 
	ATLTRY( *ppErrors = (HRESULT *)CoTaskAlloc( dwCount * sizeof( HRESULT ) ) );
	if( !*ppErrors )
		return E_OUTOFMEMORY;

	HRESULT rtn = S_OK;		// assume all acks will succeed

	for( DWORD i = 0; i < dwCount; i++ )	
	{
		(*ppErrors)[i] = AckCondition( szAcknowledgerID,
										szComment,
										pszSource[i],
										pszConditionName[i],
            							pftActiveTime[i],
										pdwCookie[i] );

		if( (*ppErrors)[i] != S_OK )
			rtn = S_FALSE;
	}


	return rtn;
}
        
HRESULT  COPCEventServer::CreateAreaBrowser( 
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ LPUNKNOWN __RPC_FAR *ppUnk)
{

	CComObject<COPCEventAreaBrowser>* pBrowser = NULL;
	ATLTRY(pBrowser = new CComObject<COPCEventAreaBrowser>)
	pBrowser->_AtlInitialConstruct();
	pBrowser->SetLocaleID(m_lcid);
	
	HRESULT rtn = pBrowser->_InternalQueryInterface( riid, (LPVOID*)ppUnk );
	if( FAILED(rtn) )
		delete pBrowser;

	return rtn;
}
        

HRESULT  COPCEventServer::QueryConditionNames( 
            /* [in] */ DWORD dwEventCategory,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszConditionNames)
{
	*pdwCount = 0;
	*ppszConditionNames = NULL;
	CONDITIONNAME_MAP::iterator cidmIt;
	bool bValidNames = false;
	// the ConditionNameMap maps DWORDS (EventCategories) to a set of 
	// strings (ConditionNames )
	cidmIt = theConditionNameMap.find( dwEventCategory );
	if( cidmIt != theConditionNameMap.end() )
	{
		// (*cidmIt).second is the CONDTIONNAMELPCWSTR_SET container
		*pdwCount = (*cidmIt).second.size();
		try
		{
			*ppszConditionNames = (LPWSTR *)CoTaskAlloc( *pdwCount * sizeof(LPWSTR) );
			CONDTIONNAMELPCWSTR_SET::iterator cidsIt;
			int i = 0;
			for( cidsIt = (*cidmIt).second.begin();
					cidsIt != (*cidmIt).second.end(); 
					cidsIt++ )
			{
				wstring cn( *cidsIt );							
				if( !cn.empty() )
					bValidNames = true;
				(*ppszConditionNames)[i++] = CoTaskAlloc( *cidsIt );
			}
		}
		catch( ... )  // catch allocation errrors
		{
			if( *ppszConditionNames )
			{
				for( DWORD i = 0; i < *pdwCount; i++ )
					CoTaskMemFree( (*ppszConditionNames)[i] );
				CoTaskMemFree( *ppszConditionNames );
				*ppszConditionNames = NULL;
				*pdwCount = 0;
				return E_OUTOFMEMORY;
			}
		}
		if( !bValidNames )
		{
			for( DWORD i = 0; i < *pdwCount; i++ )
				CoTaskMemFree( (*ppszConditionNames)[i] );
			CoTaskMemFree( *ppszConditionNames );
			*ppszConditionNames = NULL;
			*pdwCount = 0;
			return E_INVALIDARG;

		}
		return S_OK;
	}

	return E_INVALIDARG;
}


HRESULT  COPCEventServer::QuerySubConditionNames( 
            /* [in] */ LPWSTR szConditionName,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszSubConditionNames)
{
	*ppszSubConditionNames = NULL;
	ConditionName test( szConditionName );

	if( test.empty() )
		return E_INVALIDARG;

	ConditionNameSet::iterator it = theConditionNameSet.find( test );
	if( it == theConditionNameSet.end() )
		return E_INVALIDARG;	// szConditionName was not found

	*pdwCount = (*it).m_SubconditionNameSet.size();

	try
	{
		*ppszSubConditionNames = (LPWSTR *)CoTaskAlloc( *pdwCount * sizeof( LPWSTR ) );

		SUBCONDITIONNAME_SET::iterator itSub = (*it).m_SubconditionNameSet.begin();

		for( DWORD i = 0; i < *pdwCount; i++, itSub++ )
			(*ppszSubConditionNames)[i] = CoTaskAlloc( (*itSub).data() );
	}
	catch( ... )
	{
		if( *ppszSubConditionNames )
		{
			for( DWORD i = 0; i < *pdwCount; i++ )
				CoTaskMemFree( (*ppszSubConditionNames)[i] );
			CoTaskMemFree( *ppszSubConditionNames );
			return E_OUTOFMEMORY;
		}
	}


	return S_OK;
}


HRESULT  COPCEventServer::QuerySourceConditions( 
            /* [in] */ LPWSTR szSource,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszConditionNames)
{
	*ppszConditionNames =  NULL;
	wstring wsSource( szSource );
	SourceMapPtr pSourceMap;  // get a pointer to the sources/conditions and lock it with a critical section

	SourceMap::iterator itSource = pSourceMap->find( wsSource );
	if( itSource == pSourceMap->end() )
		return E_INVALIDARG;		// szSource was not found

	ConditionMap& CondMap = (*itSource).second;

	*pdwCount = CondMap.size();

	try
	{
		*ppszConditionNames = (LPWSTR *)CoTaskAlloc( *pdwCount * sizeof( LPWSTR ) );

		ConditionMap::iterator itCond = CondMap.begin();
		for( DWORD i = 0; i < *pdwCount; i++, itCond++ )
			(*ppszConditionNames)[i] = CoTaskAlloc( (*itCond).first.data() );
	}
	catch( ... )
	{
		if( *ppszConditionNames )
		{
			for( DWORD i = 0; i < *pdwCount; i++ )
				CoTaskMemFree( (*ppszConditionNames)[i] );
			CoTaskMemFree( *ppszConditionNames );								
		}
		return E_OUTOFMEMORY;
	}

	return S_OK;
}
 



HRESULT  COPCEventServer::GetStatus( 
            /* [out] */ OPCEVENTSERVERSTATUS __RPC_FAR **ppStatus)
{
	{
		ThreadSafeGlobal tsg;

		*ppStatus = tsg->m_ServerStatus.CoTaskDup();
	}  // this brace will free the lock on tsg

	if( !*ppStatus )
		return E_OUTOFMEMORY;

	GetSystemTimeAsFileTime( &((*ppStatus)->ftCurrentTime) );

	(FileTimeClass&)((*ppStatus)->ftLastUpdateTime) = LastUpdateTime();

	return S_OK;
}



// IOPCCommon Methods
HRESULT  COPCEventServer::GetErrorString( 
            /* [in] */ HRESULT dwError,
            /* [string][out] */ LPWSTR __RPC_FAR *ppString)
{
	DWORD rtn = 0;
	LPVOID lpMsgBuf;
	*ppString = NULL;

	// standard error strings should be in the resource of the proxy/stub
	rtn = FormatMessage( 
			 FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_IGNORE_INSERTS |FORMAT_MESSAGE_FROM_HMODULE,
             GetModuleHandle( OPC_AE_PROXY ), 
             GetScode( dwError ), 
             m_lcid, 
             (LPTSTR) &lpMsgBuf, 0, NULL ); 
	if( !rtn )
	{
		// couldn't get it in the language of choice
		rtn = FormatMessage( 
			FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_IGNORE_INSERTS |FORMAT_MESSAGE_FROM_HMODULE,
			GetModuleHandle( OPC_AE_PROXY ), 
			GetScode( dwError ), 
			LOCALE_ENGLISH_US, 
			(LPTSTR) &lpMsgBuf, 0, NULL ); 
	}
	if( !rtn )
	{
		// FormatMessage from system
		rtn = FormatMessage( 
			FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_IGNORE_INSERTS |FORMAT_MESSAGE_FROM_SYSTEM,
			NULL,
			GetScode( dwError ),
			MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), // Default language
			(LPTSTR) &lpMsgBuf, 0, NULL  );

	}

	if( rtn )
	{
		ATLTRY( *ppString = CoTaskAlloc( (TCHAR *)lpMsgBuf ) );
		LocalFree( lpMsgBuf );
	}

	return *ppString ? S_OK : E_FAIL;
}

        
HRESULT  COPCEventServer::SetLocaleID( 
            /* [in] */ LCID dwLocale)
{	
	switch (dwLocale)
	{
		case LOCALE_ENGLISH_US:
		case LOCALE_ENGLISH_UK:
		case LOCALE_GERMAN_GERMANY:
		case LOCALE_JAPANESE_JAPAN:
		case LOCALE_SYSTEM_DEFAULT:
		{
			m_lcid = dwLocale;
			return S_OK;
		}
	}

	return E_INVALIDARG;
}
        
HRESULT  COPCEventServer::GetLocaleID( 
            /* [out] */ LCID __RPC_FAR *pdwLocale)
{

	*pdwLocale = m_lcid;

	return S_OK;
}
        
HRESULT  COPCEventServer::QueryAvailableLocaleIDs( 
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ LCID __RPC_FAR *__RPC_FAR *ppdwLcid)
{
	*pdwCount = 4;
	ATLTRY(*ppdwLcid = (LCID*)CoTaskAlloc(*pdwCount * sizeof(LCID)));

	if (!*ppdwLcid)
	{
		return E_OUTOFMEMORY;
	}

	(*ppdwLcid)[0] = LOCALE_ENGLISH_US;
	(*ppdwLcid)[1] = LOCALE_ENGLISH_UK;
	(*ppdwLcid)[2] = LOCALE_GERMAN_GERMANY;
	(*ppdwLcid)[3] = LOCALE_JAPANESE_JAPAN;

	return S_OK;
}
                
HRESULT  COPCEventServer::SetClientName( 
            /* [string][in] */ LPCWSTR szName)
{
	// The string could be stored in a member variable in the Server
	// for diagnostic purposes. (This sample does not do anything with it)
	//

	return S_OK;
}


// IOPCEventServer2

HRESULT  COPCEventServer::EnableConditionByArea2( 
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][string][in] */ LPWSTR __RPC_FAR *pszAreas,
            /* [size_is][size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors)
{
	return EnableByArea( TRUE, dwNumAreas, pszAreas, ppErrors );
}
        
HRESULT  COPCEventServer::EnableConditionBySource2( 
            /* [in] */ DWORD dwNumSources,
            /* [size_is][string][in] */ LPWSTR __RPC_FAR *pszSources,
            /* [size_is][size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors)
{
	return EnableBySource( TRUE, dwNumSources, pszSources, ppErrors );
}
        
HRESULT  COPCEventServer::DisableConditionByArea2( 
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][string][in] */ LPWSTR __RPC_FAR *pszAreas,
            /* [size_is][size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors)
{
	return EnableByArea( FALSE, dwNumAreas, pszAreas, ppErrors );
}
        
        
HRESULT  COPCEventServer::DisableConditionBySource2( 
            /* [in] */ DWORD dwNumSources,
            /* [size_is][string][in] */ LPWSTR __RPC_FAR *pszSources,
            /* [size_is][size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors)
{
	return EnableBySource( FALSE, dwNumSources, pszSources, ppErrors );
}
        
        
HRESULT  COPCEventServer::GetEnableStateByArea( 
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][string][in] */ LPWSTR __RPC_FAR *pszAreas,
            /* [size_is][size_is][out] */ BOOL __RPC_FAR *__RPC_FAR *pbEnabled,
            /* [size_is][size_is][out] */ BOOL __RPC_FAR *__RPC_FAR *pbEffectivelyEnabled,
            /* [size_is][size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors)
{
	HRESULT rtn = S_OK;
	if( dwNumAreas == 0 )
		return E_INVALIDARG;
	*pbEnabled = NULL;
	ATLTRY( *pbEnabled = (BOOL *)CoTaskAlloc( dwNumAreas * sizeof(BOOL) ) );
	if( !*pbEnabled )
		return E_OUTOFMEMORY;

	*pbEffectivelyEnabled = NULL;
	ATLTRY( *pbEffectivelyEnabled = (BOOL *)CoTaskAlloc( dwNumAreas * sizeof(BOOL) ) );
	if( !*pbEffectivelyEnabled )
	{
		CoTaskMemFree( *pbEnabled  );
		return E_OUTOFMEMORY;
	}

	*ppErrors = NULL;
	ATLTRY( *ppErrors = (HRESULT *)CoTaskAlloc( dwNumAreas * sizeof(HRESULT) ) );
	if( !*ppErrors )
	{
		CoTaskMemFree( *pbEnabled  );
		CoTaskMemFree( *pbEffectivelyEnabled  );
		return E_OUTOFMEMORY;
	}
	// these locals are here just to lock the critical sections
	SourceMapPtr pSourceMap;  // get a pointer to the sources/conditions and lock it with a critical section	ThreadSafeGlobal tsg;
	ThreadSafeGlobal tsg;

	for( DWORD i = 0; i < dwNumAreas; i++ )
	{
		AreaNode *pNode = theAreaRootNode.find( pszAreas[i], COPCEventAreaBrowser::seps );
		if( pNode && wcslen( pszAreas[i] ) )  // found the area
		{
			(*ppErrors)[i] = S_OK;
			(*pbEnabled)[i] = pNode->IsEnabled();
			(*pbEffectivelyEnabled)[i] = pNode->IsEffectivelyEnabled();
		}
		else
		{
			(*ppErrors)[i] = E_INVALIDARG;
			rtn = S_FALSE;
		}
	}
	return  rtn;
}
        
        
HRESULT  COPCEventServer::GetEnableStateBySource( 
            /* [in] */ DWORD dwNumSources,
            /* [size_is][string][in] */ LPWSTR __RPC_FAR *pszSources,
            /* [size_is][size_is][out] */ BOOL __RPC_FAR *__RPC_FAR *pbEnabled,
            /* [size_is][size_is][out] */ BOOL __RPC_FAR *__RPC_FAR *pbEffectivelyEnabled,
            /* [size_is][size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors)
{
	HRESULT rtn = S_OK;
	if( dwNumSources == 0 )
		return E_INVALIDARG;
	*pbEnabled = NULL;
	ATLTRY( *pbEnabled = (BOOL *)CoTaskAlloc( dwNumSources * sizeof(BOOL) ) );
	if( !*pbEnabled )
		return E_OUTOFMEMORY;

	*pbEffectivelyEnabled = NULL;
	ATLTRY( *pbEffectivelyEnabled = (BOOL *)CoTaskAlloc( dwNumSources * sizeof(BOOL) ) );
	if( !*pbEffectivelyEnabled )
	{
		CoTaskMemFree( *pbEnabled  );
		return E_OUTOFMEMORY;
	}

	*ppErrors = NULL;
	ATLTRY( *ppErrors = (HRESULT *)CoTaskAlloc( dwNumSources * sizeof(HRESULT) ) );
	if( !*ppErrors )
	{
		CoTaskMemFree( *pbEnabled  );
		CoTaskMemFree( *pbEffectivelyEnabled  );
		return E_OUTOFMEMORY;
	}
	
	SourceMapPtr pSourceMap;  // get a pointer to the sources/conditions and lock it with a critical section
	ThreadSafeGlobal tsg;
	for( DWORD i = 0; i < dwNumSources; i++ )
	{
		SourceMap::iterator itSource = pSourceMap->find( pszSources[i] );
		if( itSource == pSourceMap->end() )
		{
			rtn = S_FALSE;
			(*ppErrors)[i] = E_INVALIDARG;		// szSource was not found
		}
		else
		{
			ConditionMap& CondMap = (*itSource).second;
			ConditionMap::iterator itCond;
			itCond = CondMap.begin();
			if( itCond == CondMap.end() )
			{
				rtn = S_FALSE;
				(*ppErrors)[i] = E_INVALIDARG;		// Source has no conditions !!!
			}
			else
			{
				OPCCondition& OPCCond = (*itCond).second;
				(*ppErrors)[i] = S_OK;
				(*pbEnabled)[i] = OPCCond.IsEnabled();
				(*pbEffectivelyEnabled)[i] = OPCCond.IsEffectivelyEnabled();
			}
		}
	}
	return rtn;

}
        
        
