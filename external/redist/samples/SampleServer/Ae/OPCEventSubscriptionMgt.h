// OPCEventSubscriptionMgt.h
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
//   $Workfile: OPCEventSubscriptionMgt.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 23 $
//       $Date: 12/08/02 3:40p $
//    $Archive: /OPC/AlarmEvents/SampleServer/OPCEventSubscriptionMgt.h $
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
/*   $History: OPCEventSubscriptionMgt.h $
 * 
 * *****************  Version 23  *****************
 * User: Jiml         Date: 12/08/02   Time: 3:40p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * Added Keep-alive
 * 
 * *****************  Version 22  *****************
 * User: Jiml         Date: 8/19/98    Time: 12:03p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 21  *****************
 * User: Jiml         Date: 8/18/98    Time: 2:17p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 20  *****************
 * User: Jiml         Date: 8/18/98    Time: 2:15p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * Fixed Refresh() -- was not working at all 
 * 
 * *****************  Version 19  *****************
 * User: Jiml         Date: 7/24/98    Time: 12:09p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 18  *****************
 * User: Jiml         Date: 7/24/98    Time: 11:32a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 17  *****************
 * User: Jiml         Date: 5/01/98    Time: 10:04a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 16  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 3/18/98    Time: 7:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 3/13/98    Time: 6:46p
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
 * User: Jiml         Date: 1/02/98    Time: 6:29p
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
// OPCEventSubscriptionMgt.h : Declaration of the COPCEventSubscriptionMgt

#ifndef __OPCEVENTSUBSCRIPTIONMGT_H_
#define __OPCEVENTSUBSCRIPTIONMGT_H_

#include "resource.h"       // main symbols
#include "condition.h"
#include "ThreadPool.h"
#include "FileTime.h"
#include "ConditionMap.h"

#pragma warning( disable : 4786 )
#pragma warning( disable : 4503 )
#include <set>
#include <queue>
#include <vector>

using namespace std;



typedef queue<OnEventClass*> ON_EVENT_Q;
typedef vector<DWORD> EVENT_CATEGORY_VECTOR;
typedef vector<wstring> GEN_ID_VECTOR;

typedef vector<DWORD> ATTRIBUTE_VECTOR;


class ReturnedAttributes
{
private:
	// "key" parameters for the set
//	DWORD		m_dwEventType;
	DWORD		m_dwEventCategory;

	// non-key parameters
	ATTRIBUTE_VECTOR	m_AttributeVector;

public:
	ReturnedAttributes( DWORD dwEventCategory )
			{ m_dwEventCategory = dwEventCategory; }

//	DWORD EventType() const { return m_dwEventType; }
	DWORD EventCategory() const { return m_dwEventCategory; }

	ATTRIBUTE_VECTOR& AttributeVector() { return m_AttributeVector; }
	const ATTRIBUTE_VECTOR& AttributeVector() const { return m_AttributeVector; }

	bool operator<( const ReturnedAttributes& test ) const;
	bool operator<=( const ReturnedAttributes& test ) const; 
	bool operator>( const ReturnedAttributes& test ) const; 
	bool operator>=( const ReturnedAttributes& test ) const; 
	bool operator==( const ReturnedAttributes& test ) const; 
	bool operator!=( const ReturnedAttributes& test ) const; 
};

typedef set<ReturnedAttributes> RETURNED_ATTRIBUTES_SET;


// no data members and no virtual functions makes
// sizeof(SubscribedEventClass) == sizeof(ONEEVENTSTRUC)
class SubscribedEventClass : public ONEVENTSTRUCT 
{
public:
	~SubscribedEventClass();
	void copy( const OnEventClass& src, const RETURNED_ATTRIBUTES_SET& RASet );
};






class OnEventClass;
class COPCEventServer;
class ForEachCondition;
class ForEachSource;


/////////////////////////////////////////////////////////////////////////////
// COPCEventSubscriptionMgt
class COPCEventSubscriptionMgt : 
	public IOPCEventSubscriptionMgt2,
	public IConnectionPointContainerImpl<COPCEventSubscriptionMgt>,
	public IConnectionPointImpl<COPCEventSubscriptionMgt, &IID_IOPCEventSink>,
	public CComObjectRootEx<CComMultiThreadModel>,
//	public CComCoClass<COPCEventSubscriptionMgt, &CLSID_OPCEventSubscriptionMgt>,
	public CTPClient   // Thread Pool Client
{
private:
	friend ForEachCondition;
	friend ForEachSource;
	CComAutoCriticalSection	m_csData;   // crtical section for locking data members

	COPCEventServer*		m_pParent;
	BOOL					m_bActive;
    DWORD					m_dwBufferTime;
    DWORD					m_dwMaxSize;
    OPCHANDLE				m_hClientSubscription;
	ON_EVENT_Q				m_OnEventQ;
	ON_EVENT_Q				m_RefreshQ;
	FileTimeClass			m_ftcLastUpdate;


	// data members for Set/Get Filter
	DWORD					m_dwEventType;
	EVENT_CATEGORY_VECTOR   m_EventCategoryVector;
	DWORD					m_dwLowSeverity;
	DWORD					m_dwHighSeverity;
	AREA_VECTOR				m_AreaVector;
	GEN_ID_VECTOR			m_GenIDVector;

	// data member for SelectReturnAttributes()
	RETURNED_ATTRIBUTES_SET			m_ReturnedAttributesSet;

	// data member flag for CancelRefresh();
	BOOL					m_bCancelRefresh;

	// temp storage for Source
	wstring					m_wsTempSource;


	// data member for get/set KeepAlive
	DWORD					m_dwKeepAliveModulo;

	DWORD					m_dwNonUpdates;  
	LCID                    m_lcid;

	void SendCancelRefresh();
	void SendEvents( ON_EVENT_Q& q, BOOL bRefresh );
	bool MatchesFilter( OnEventClass* pOnEventClass, const AREA_VECTOR& areas  ) const;

public:

	
	COPCEventSubscriptionMgt(){ m_pParent = NULL;}
	

	~COPCEventSubscriptionMgt();

	void Init( COPCEventServer& Parent, BOOL bActive,
            DWORD dwBufferTime,
            DWORD dwMaxSize,
            OPCHANDLE hClientSubscription );


	void ProcessNewEvent( OnEventClass* pOnEventClass, const AREA_VECTOR& areas  );

	virtual void ThreadWork();

	void Active( BOOL bActive ) { m_bActive = bActive; }
	BOOL Active() const { return m_bActive; }
	void BufferTime( DWORD dwBufferTime ) { m_dwBufferTime = dwBufferTime; }
	DWORD BufferTime() const { return m_dwBufferTime; }
	void MaxSize( DWORD dwMaxSize ) { m_dwMaxSize = dwMaxSize; }
	DWORD MaxSize() const { return m_dwMaxSize; }
	void ClientSubscription( OPCHANDLE hClientSubscription )
				{ m_hClientSubscription = hClientSubscription; }
	OPCHANDLE ClientSubscription() const { return m_hClientSubscription; }
	void SetLocaleID(LCID lcid) { m_lcid = lcid; }

// DECLARE_REGISTRY_RESOURCEID(IDR_OPCEVENTSUBSCRIPTIONMGT)
   DECLARE_NOT_AGGREGATABLE(COPCEventSubscriptionMgt)

BEGIN_COM_MAP(COPCEventSubscriptionMgt)
	COM_INTERFACE_ENTRY(IOPCEventSubscriptionMgt)
	COM_INTERFACE_ENTRY(IOPCEventSubscriptionMgt2)
	COM_INTERFACE_ENTRY_IMPL(IConnectionPointContainer)
END_COM_MAP()

BEGIN_CONNECTION_POINT_MAP(COPCEventSubscriptionMgt)
	CONNECTION_POINT_ENTRY(IID_IOPCEventSink)
END_CONNECTION_POINT_MAP()




// IOPCEventSubscriptionMgt
public:

        virtual HRESULT STDMETHODCALLTYPE SetFilter( 
            /* [in] */ DWORD dwEventType,
            /* [in] */ DWORD dwNumCategories,
            /* [size_is][in] */ DWORD __RPC_FAR *pdwEventCategories,
            /* [in] */ DWORD dwLowSeverity,
            /* [in] */ DWORD dwHighSeverity,
            /* [in] */ DWORD dwNumAreas,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszAreaList,
            /* [in] */ DWORD dwNumSources,
            /* [size_is][in] */ LPWSTR __RPC_FAR *pszSourceList);
        
        virtual HRESULT STDMETHODCALLTYPE GetFilter( 
            /* [out] */ DWORD __RPC_FAR *pdwEventType,
            /* [out] */ DWORD __RPC_FAR *pdwNumCategories,
            /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppdwEventCategories,
            /* [out] */ DWORD __RPC_FAR *pdwLowSeverity,
            /* [out] */ DWORD __RPC_FAR *pdwHighSeverity,
            /* [out] */ DWORD __RPC_FAR *pdwNumAreas,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszAreaList,
            /* [out] */ DWORD __RPC_FAR *pdwNumSources,
            /* [size_is][size_is][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszSourceList);
        
        virtual HRESULT STDMETHODCALLTYPE SelectReturnedAttributes( 
            /* [in] */ DWORD dwEventCategory,
            /* [in] */ DWORD dwCount,
            /* [size_is][in] */ DWORD __RPC_FAR *dwAttributeIDs);
        
        virtual HRESULT STDMETHODCALLTYPE GetReturnedAttributes( 
            /* [in] */ DWORD dwEventCategory,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppdwAttributeIDs);
        
        virtual HRESULT STDMETHODCALLTYPE Refresh( 
            /* [in] */ DWORD dwConnection);
        
        virtual HRESULT STDMETHODCALLTYPE CancelRefresh( 
            /* [in] */ DWORD dwConnection);
        
        virtual HRESULT STDMETHODCALLTYPE GetState( 
            /* [out] */ BOOL __RPC_FAR *pbActive,
            /* [out] */ DWORD __RPC_FAR *pdwBufferTime,
            /* [out] */ DWORD __RPC_FAR *pdwMaxSize,
            /* [out] */ OPCHANDLE __RPC_FAR *phClientSubscription);
        
        virtual HRESULT STDMETHODCALLTYPE SetState( 
            /* [in][unique] */ BOOL __RPC_FAR *pbActive,
            /* [in][unique] */ DWORD __RPC_FAR *pdwBufferTime,
            /* [in][unique] */ DWORD __RPC_FAR *pdwMaxSize,
            /* [in] */ OPCHANDLE hClientSubscription,
            /* [out] */ DWORD __RPC_FAR *pdwRevisedBufferTime,
            /* [out] */ DWORD __RPC_FAR *pdwRevisedMaxSize);


// IOPCEventSubscriptionMgt2

        virtual HRESULT STDMETHODCALLTYPE SetKeepAlive( 
            /* [in] */ DWORD dwKeepAliveTime,
            /* [out] */ DWORD __RPC_FAR *pdwRevisedKeepAliveTime);
        
        virtual HRESULT STDMETHODCALLTYPE GetKeepAlive( 
            /* [out] */ DWORD __RPC_FAR *pdwKeepAliveTime);


        
};


class ForEachCondition
{
private:
	COPCEventSubscriptionMgt& m_SubMgt;
public:

	ForEachCondition( COPCEventSubscriptionMgt& SubMgt ) : m_SubMgt(SubMgt) {};
	void operator()( ConditionMap::value_type & Value );
};

class ForEachSource
{
private:
	COPCEventSubscriptionMgt& m_SubMgt;
public:

	ForEachSource( COPCEventSubscriptionMgt& SubMgt ) : m_SubMgt(SubMgt) {};
	void operator()( SourceMap::value_type & Value );
};




inline bool ReturnedAttributes::operator<( const ReturnedAttributes& test ) const
{  
	return m_dwEventCategory < test.m_dwEventCategory;
}


inline bool ReturnedAttributes::operator<=( const ReturnedAttributes& test ) const 
{  
	return m_dwEventCategory <= test.m_dwEventCategory;
}
inline bool ReturnedAttributes::operator>( const ReturnedAttributes& test ) const 
{  
	return m_dwEventCategory > test.m_dwEventCategory;
}
inline bool ReturnedAttributes::operator>=( const ReturnedAttributes& test ) const 
{  
	return m_dwEventCategory >= test.m_dwEventCategory;
}
inline bool ReturnedAttributes::operator==( const ReturnedAttributes& test ) const 
{  
	return m_dwEventCategory == test.m_dwEventCategory;
}
inline bool ReturnedAttributes::operator!=( const ReturnedAttributes& test ) const 
{  
	return m_dwEventCategory != test.m_dwEventCategory;
}



#endif //__OPCEVENTSUBSCRIPTIONMGT_H_
