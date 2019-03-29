// OPCEventServer.h
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
//   $Workfile: OPCEventServer.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 17 $
//       $Date: 8/02/02 4:47p $
//    $Archive: /OPC/AlarmEvents/SampleServer/OPCEventServer.h $
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
/*   $History: OPCEventServer.h $
 * 
 * *****************  Version 17  *****************
 * User: Jiml         Date: 8/02/02    Time: 4:47p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 16  *****************
 * User: Jiml         Date: 11/12/98   Time: 11:43a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * Modified to reflect interface change in TranslateToItemIDs()
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 8/19/98    Time: 12:03p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 8/10/98    Time: 6:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 *  1. Added AckComment attribute
 *  2. Added Areas attribute
 *  3. revised ::GetConditionState() to include attributes
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 3/18/98    Time: 7:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 3/11/98    Time: 4:34p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 3/10/98    Time: 6:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 2/26/98    Time: 11:04a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 2/09/98    Time: 4:53p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 12/29/97   Time: 7:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 12/26/97   Time: 6:55p
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
// OPCEventServer.h : Declaration of the COPCEventServer

#ifndef __OPCEVENTSERVER_H_
#define __OPCEVENTSERVER_H_

#include "resource.h"       // main symbols
#include "OPCEventSubscriptionMgt.h"
#include "QueryContainers.h"


class OnEventClass;


#pragma warning( disable : 4786 )
#pragma warning( disable : 4503 )
#include <set>

using namespace std;


typedef set<COPCEventSubscriptionMgt*>  EVSUBMGT_SET;


#ifdef __cplusplus
EXTERN_C const CLSID CLSID_OPCEventServer;

class DECLSPEC_UUID("65168852-5783-11D1-84A0-00608CB8A7E9")
OPCEventServer;
#endif



/////////////////////////////////////////////////////////////////////////////
// COPCEventServer
class ATL_NO_VTABLE COPCEventServer : 
	public IOPCEventServer2,
	public IOPCCommon,
	public IConnectionPointContainerImpl<COPCEventServer>,
	public IConnectionPointImpl<COPCEventServer, &IID_IOPCShutdown>,
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COPCEventServer, &CLSID_OPCEventServer>

{
private:
	// a set of pointers to active subscriptions
	EVSUBMGT_SET m_ActiveSubscriptionSet;

	
	LCID			m_lcid;
	FileTimeClass	m_ftLastUpdateTime;

	HRESULT  AckCondition( 
             LPWSTR szAcknowledgerID,
             LPWSTR szComment,
             LPWSTR szSource,
             LPWSTR szConditionName,
             FILETIME ftActiveTime,
             DWORD dwCookie );

	HRESULT  EnableBySource( BOOL bEnable,
            /* [in] */ DWORD dwNumSources,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszSources,
            /* [size_is][size_is][out] */ HRESULT **ppErrors);


	HRESULT  EnableByArea( BOOL bEnable,
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszAreas,
            /* [size_is][size_is][out] */ HRESULT **ppErrors);

	HRESULT	 EnableSource( BOOL bEnable, LPCWSTR szSource );

	void NotifyStateChange( LPCWSTR szSource );
	void RecursiveNotifyStateChange( AreaNode& rNode );


public:
	COPCEventServer();
	~COPCEventServer();

	void Remove( COPCEventSubscriptionMgt *pMgt );

	void ProcessNewEvent( OnEventClass* pOnEventClass, const AREA_VECTOR& areas );

	FileTimeClass LastUpdateTime();
	void LastUpdateTime( const FILETIME& ft );


//IOPCEventServer Methods

        virtual HRESULT STDMETHODCALLTYPE GetStatus( 
            /* [out] */ OPCEVENTSERVERSTATUS __RPC_FAR **ppEventServerStatus);
        
        virtual HRESULT STDMETHODCALLTYPE CreateEventSubscription( 
            /* [in] */ BOOL bActive,
            /* [in] */ DWORD dwBufferTime,
            /* [in] */ DWORD dwMaxSize,
            /* [in] */ OPCHANDLE hClientSubscription,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ LPUNKNOWN __RPC_FAR *ppUnk,
            /* [out] */ DWORD __RPC_FAR *pdwRevisedBufferTime,
            /* [out] */ DWORD __RPC_FAR *pdwRevisedMaxSize);
        
        virtual HRESULT STDMETHODCALLTYPE QueryAvailableFilters( 
            /* [out] */ DWORD __RPC_FAR *pdwFilterMask);
        
        virtual HRESULT STDMETHODCALLTYPE QueryEventCategories( 
            /* [in] */ DWORD dwEventType,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppdwEventCategories,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszEventCategoryDescs);
        
        virtual HRESULT STDMETHODCALLTYPE QueryConditionNames( 
            /* [in] */ DWORD dwEventCategory,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszConditionNames);
        
        virtual HRESULT STDMETHODCALLTYPE QuerySubConditionNames( 
            /* [in] */ LPWSTR szConditionName,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszSubConditionNames);
        
        virtual HRESULT STDMETHODCALLTYPE QuerySourceConditions( 
            /* [in] */ LPWSTR szSource,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszConditionNames);
        
        virtual HRESULT STDMETHODCALLTYPE QueryEventAttributes( 
            /* [in] */ DWORD dwEventCategory,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppdwAttrIDs,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszAttrDescs,
            /* [size_is][size_is][out] */ VARTYPE __RPC_FAR *__RPC_FAR *ppvtAttrTypes);
        
        virtual HRESULT STDMETHODCALLTYPE TranslateToItemIDs( 
            /* [in] */ LPWSTR szSource,
            /* [in] */ DWORD dwEventCategory,
            /* [in] */ LPWSTR szConditionName,
            /* [in] */ LPWSTR szSubconditionName,
            /* [in] */ DWORD dwCount,
            /* [size_is][in] */ DWORD __RPC_FAR *pdwAssocAttrIDs,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszAttrItemIDs,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszNodeNames,
            /* [size_is][out] */ CLSID __RPC_FAR **ppCLSIDs);
        
        virtual HRESULT STDMETHODCALLTYPE GetConditionState( 
            /* [in] */ LPWSTR szSource,
            /* [in] */ LPWSTR szConditionName,
            /* [in] */ DWORD dwNumEventAttrs,
            /* [size_is][in] */ DWORD __RPC_FAR *dwAttributeIDs,
            /* [out] */ OPCCONDITIONSTATE __RPC_FAR *__RPC_FAR *ppConditionState);
                
        virtual HRESULT STDMETHODCALLTYPE EnableConditionByArea( 
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszAreas);
        
        virtual HRESULT STDMETHODCALLTYPE EnableConditionBySource( 
            /* [in] */ DWORD dwNumSources,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszSources);
        
        virtual HRESULT STDMETHODCALLTYPE DisableConditionByArea( 
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszAreas);
        
        virtual HRESULT STDMETHODCALLTYPE DisableConditionBySource( 
            /* [in] */ DWORD dwNumSources,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszSources);
        
        virtual HRESULT STDMETHODCALLTYPE AckCondition( 
            /* [in] */ DWORD dwCount,
            /* [string][in] */ LPWSTR szAcknowledgerID,
            /* [string][in] */ LPWSTR szComment,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszSource,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszConditionName,
            /* [size_is][in] */ FILETIME __RPC_FAR *pftActiveTime,
            /* [size_is][in] */ DWORD __RPC_FAR *pdwCookie,
            /* [size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors);
        
        virtual HRESULT STDMETHODCALLTYPE CreateAreaBrowser( 
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ LPUNKNOWN __RPC_FAR *ppUnk);
        


// IOPCCommon Methods
        virtual HRESULT STDMETHODCALLTYPE SetLocaleID( 
            /* [in] */ LCID dwLcid);
        
        virtual HRESULT STDMETHODCALLTYPE GetLocaleID( 
            /* [out] */ LCID __RPC_FAR *pdwLcid);
        
        virtual HRESULT STDMETHODCALLTYPE QueryAvailableLocaleIDs( 
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ LCID __RPC_FAR *__RPC_FAR *pdwLcid);
        
        virtual HRESULT STDMETHODCALLTYPE GetErrorString( 
            /* [in] */ HRESULT dwError,
            /* [string][out] */ LPWSTR __RPC_FAR *ppString);
        
        virtual HRESULT STDMETHODCALLTYPE SetClientName( 
            /* [string][in] */ LPCWSTR szName);

// IOPCEventServer2

        virtual HRESULT STDMETHODCALLTYPE EnableConditionByArea2( 
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][string][in] */ LPWSTR __RPC_FAR *pszAreas,
            /* [size_is][size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors);
        
        virtual HRESULT STDMETHODCALLTYPE EnableConditionBySource2( 
            /* [in] */ DWORD dwNumSources,
            /* [size_is][string][in] */ LPWSTR __RPC_FAR *pszSources,
            /* [size_is][size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors);
        
        virtual HRESULT STDMETHODCALLTYPE DisableConditionByArea2( 
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][string][in] */ LPWSTR __RPC_FAR *pszAreas,
            /* [size_is][size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors);
        
        virtual HRESULT STDMETHODCALLTYPE DisableConditionBySource2( 
            /* [in] */ DWORD dwNumSources,
            /* [size_is][string][in] */ LPWSTR __RPC_FAR *pszSources,
            /* [size_is][size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors);
        
        virtual HRESULT STDMETHODCALLTYPE GetEnableStateByArea( 
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][string][in] */ LPWSTR __RPC_FAR *pszAreas,
            /* [size_is][size_is][out] */ BOOL __RPC_FAR *__RPC_FAR *pbEnabled,
            /* [size_is][size_is][out] */ BOOL __RPC_FAR *__RPC_FAR *pbEffectivelyEnabled,
            /* [size_is][size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors);
        
        virtual HRESULT STDMETHODCALLTYPE GetEnableStateBySource( 
            /* [in] */ DWORD dwNumSources,
            /* [size_is][string][in] */ LPWSTR __RPC_FAR *pszSources,
            /* [size_is][size_is][out] */ BOOL __RPC_FAR *__RPC_FAR *pbEnabled,
            /* [size_is][size_is][out] */ BOOL __RPC_FAR *__RPC_FAR *pbEffectivelyEnabled,
            /* [size_is][size_is][out] */ HRESULT __RPC_FAR *__RPC_FAR *ppErrors);
        


DECLARE_REGISTRY_RESOURCEID(IDR_OPCEVENTSERVER)
DECLARE_NOT_AGGREGATABLE(COPCEventServer)

BEGIN_COM_MAP(COPCEventServer)
	COM_INTERFACE_ENTRY(IOPCEventServer2)
	COM_INTERFACE_ENTRY(IOPCEventServer)
	COM_INTERFACE_ENTRY(IOPCCommon)
	COM_INTERFACE_ENTRY_IMPL(IConnectionPointContainer)
END_COM_MAP()

BEGIN_CONNECTION_POINT_MAP(COPCEventServer)
	CONNECTION_POINT_ENTRY(IID_IOPCShutdown)
END_CONNECTION_POINT_MAP()
	
	
};

#endif //__OPCEVENTSERVER_H_
