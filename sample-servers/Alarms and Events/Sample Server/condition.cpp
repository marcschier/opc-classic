// condition.cpp
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
// Description: 
//
// Functions:   
//
//
//
//
//
/*   $History: condition.cpp $
 * 
 * *****************  Version 22  *****************
 * User: Jiml         Date: 1/07/03    Time: 5:41p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * minor source changes to build in VC7
 * 
 * *****************  Version 21  *****************
 * User: Jiml         Date: 8/02/02    Time: 4:47p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 20  *****************
 * User: Jiml         Date: 8/14/01    Time: 12:50p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 19  *****************
 * User: Jiml         Date: 3/29/99    Time: 2:49p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 18  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:52a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 17  *****************
 * User: Jiml         Date: 8/10/98    Time: 6:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 *  1. Added AckComment attribute
 *  2. Added Areas attribute
 *  3. revised ::GetConditionState() to include attributes
 * 
 * *****************  Version 16  *****************
 * User: Jiml         Date: 7/10/98    Time: 3:31p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * Changed ONEVENTSTRUCT.ftActiveTime to use cond.SubconditionTime instead
 * of ConditionTime.
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 4/06/98    Time: 5:47p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 3/31/98    Time: 11:43a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 3/30/98    Time: 6:02p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 3/18/98    Time: 7:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 3/11/98    Time: 4:34p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 3/10/98    Time: 6:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 3/06/98    Time: 6:02p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 3/04/98    Time: 4:14p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 2/26/98    Time: 11:04a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 12/30/97   Time: 6:46p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:04a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 12/19/97   Time: 6:40p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 12/18/97   Time: 6:29p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 1  *****************
 * User: Jiml         Date: 12/15/97   Time: 11:11a
 * Created in $/OPC/AlarmEvents/SampleServer
*/
//
//
//************************************************************************          





#include "stdafx.h"
#include "condition.h"
#include "QueryContainers.h"
#include "OPCEventAreaBrowser.h"



#define NullOrCoTaskAlloc( a )	((a && wcslen(a)) ? CoTaskAlloc(a) : NULL)



OPCCondition::OPCCondition( BOOL bEnabled, WORD wInitialQuality)
{
	ClearChangeMask();
	m_dwEventCategory = 0;
	m_dwSeverity = 0;
	m_dwEventType = 0;
	m_dwEventCategory = 0;
	m_dwSeverity = 0;
	m_bEnabled = bEnabled;
	m_wNewState = bEnabled ? OPC_CONDITION_ENABLED : 0;   // bit fields for active, enabled, acked 
	m_wQuality = wInitialQuality;
	m_bAckRequired = TRUE;
	m_dwCookie = 0;	
}


BOOL OPCCondition::UpdateEffectiveState()
{
	if( IsEffectivelyEnabled() )
	{
		if( (m_wNewState & OPC_CONDITION_ENABLED) == 0  )
		{
			m_wNewState |= OPC_CONDITION_ENABLED;
			m_wChangeMask |= OPC_CHANGE_ENABLE_STATE;
			return TRUE;  // state has changed
		}
	}
	else
	{
		if( (m_wNewState & OPC_CONDITION_ENABLED) != 0  )
		{
			m_wNewState &= ~OPC_CONDITION_ENABLED;
			m_wChangeMask |= OPC_CHANGE_ENABLE_STATE;
			return TRUE;  // state has changed
		}
	}
	return FALSE;
}

void OPCCondition::Enable( BOOL bEnable )
{
	m_bEnabled = bEnable;
	UpdateEffectiveState();
}



void OPCCondition::Active( BOOL bActive )
{
	if( bActive != IsActive() )
	{
		m_wChangeMask |= OPC_CHANGE_ACTIVE_STATE;

		if( bActive )   // transitioned into alarm
			m_wNewState |= OPC_CONDITION_ACTIVE;
		else  // return to normal
			m_wNewState &= ~OPC_CONDITION_ACTIVE;
	}
}


BOOL OPCCondition::Ack( LPCWSTR szActor, LPCWSTR szComment )
{
	if( !IsAcked() )
	{
		m_wNewState |= OPC_CONDITION_ACKED;
		m_wsAckComment = szComment;
		m_wsActorID = szActor;
		m_wChangeMask = OPC_CHANGE_ACK_STATE;
		Time();
		m_ftLastAckTime = m_ftTime;
		return TRUE;
	}
	return FALSE;
}



void OPCCondition::Message( LPCWSTR wszString )
{ 
	if( m_wsMessage != wszString )
	{
		m_wsMessage = wszString;
		m_wChangeMask |= OPC_CHANGE_MESSAGE;
	}
}



void OPCCondition::Time( FILETIME ftTime )
{
	m_ftTime = ftTime;
	if( IsActive() )
	{
		if( (m_wChangeMask & OPC_CHANGE_ACTIVE_STATE) )
		{
			m_ftActiveTime = m_ftTime;
			m_ftSubconditionTime = m_ftTime;
		}
		else if( (m_wChangeMask & OPC_CHANGE_SUBCONDITION) )
			m_ftSubconditionTime = m_ftTime;
	}
	else
	{
		if( (m_wChangeMask & OPC_CHANGE_ACTIVE_STATE) )
			m_ftRTNTime = m_ftTime;  // Return to Normal
	}
}



void OPCCondition::Time()  // time of event is now
{
	FILETIME ftNow;
	GetSystemTimeAsFileTime( &ftNow );
	Time( ftNow );
}



void OPCCondition::Severity( DWORD dwSeverity )
{
	if( dwSeverity != m_dwSeverity )
	{
		m_dwSeverity = dwSeverity;
		m_wChangeMask |= OPC_CHANGE_SEVERITY;
	}
}

void OPCCondition::SubconditionName( LPCWSTR wszSubconditionName )
{ 
	if( m_wsSubconditionName != wszSubconditionName )
	{
		m_wsSubconditionName = wszSubconditionName; 
		m_wChangeMask |= OPC_CHANGE_SUBCONDITION;
	}
}

void OPCCondition::Quality( WORD wQuality )
{
	if( m_wQuality != m_wQuality )
	{
		m_wQuality = m_wQuality;
		m_wChangeMask |= OPC_CHANGE_QUALITY;
	}
}



void OPCCondition::AckRequired( BOOL bAckReq )
{
	m_bAckRequired = bAckReq; 
	if( !m_bAckRequired && !IsAcked() ) // ack the condition
	{
		m_wNewState |= OPC_CONDITION_ACKED;
		m_wChangeMask |= OPC_CHANGE_ACK_STATE;
	}
	if( m_bAckRequired && IsAcked() ) //un ack the condition
	{
		m_wNewState &= ~OPC_CONDITION_ACKED;
		m_wChangeMask |= OPC_CHANGE_ACK_STATE;
	}

}



OPCCONDITIONSTATE *OPCCondition::CoTaskAllocConditionState( DWORD dwNumAttrs, DWORD *pdwAttrIDs ) const
{
	OPCCONDITIONSTATE *rtn = NULL;
	CComVariant *pComVar = NULL;
	try
	{
		rtn = (OPCCONDITIONSTATE *)CoTaskAlloc( sizeof(OPCCONDITIONSTATE) );
		rtn->wState = NewState();


		OPCSubcondition test( SubconditionName() );
		SUBCONDITION_VECTOR::const_iterator it = find( 
						m_Subconditions.begin(), m_Subconditions.end(), test );
		_ASSERTE( it != m_Subconditions.end() );

		const OPCSubcondition& sub = *it;

		if( NewState() & OPC_CONDITION_ACTIVE )
		{
			rtn->szActiveSubCondition = NullOrCoTaskAlloc( SubconditionName() );
			rtn->szASCDefinition = NullOrCoTaskAlloc( sub.Definition() );
			rtn->dwASCSeverity = sub.Severity();
			rtn->szASCDescription = NullOrCoTaskAlloc( sub.Message() );
		}
		else
		{
			rtn->szActiveSubCondition = NULL;
			rtn->szASCDefinition = NULL;
			rtn->dwASCSeverity = 0;
			rtn->szASCDescription = NULL;
		}
		rtn->wQuality = Quality();
		(FileTimeClass&)rtn->ftLastAckTime = LastAckTime();
		(FileTimeClass&)rtn->ftSubCondLastActive = SubconditionTime();
		(FileTimeClass&)rtn->ftCondLastActive = ActiveTime();
		(FileTimeClass&)rtn->ftCondLastInactive = RTNTime();
		
		rtn->szAcknowledgerID = NullOrCoTaskAlloc( ActorID() );
		
		rtn->szComment = NullOrCoTaskAlloc( AckComment() );
		rtn->dwNumSCs = m_Subconditions.size();
		rtn->pszSCNames = (LPWSTR *)CoTaskAlloc( rtn->dwNumSCs * sizeof(LPWSTR) );
		rtn->pszSCDefinitions = (LPWSTR *)CoTaskAlloc( rtn->dwNumSCs * sizeof( LPWSTR ) );
		rtn->pdwSCSeverities = (DWORD *)CoTaskAlloc( rtn->dwNumSCs * sizeof(DWORD) );
		rtn->pszSCDescriptions = (LPWSTR *)CoTaskAlloc( rtn->dwNumSCs * sizeof(LPWSTR) );
		for( DWORD i = 0; i < rtn->dwNumSCs; i++ )
		{
			rtn->pszSCNames[i] = NullOrCoTaskAlloc( m_Subconditions[i].Name() );
			rtn->pszSCDescriptions[i] = NullOrCoTaskAlloc( m_Subconditions[i].Message() );
			rtn->pszSCDefinitions[i] = NullOrCoTaskAlloc( m_Subconditions[i].Definition() );
			rtn->pdwSCSeverities[i] = m_Subconditions[i].Severity();
		}

	    pComVar = new CComVariant[dwNumAttrs];
		rtn->dwNumEventAttrs = dwNumAttrs;
		rtn->pEventAttributes = pComVar;
		rtn->pErrors = (HRESULT *)CoTaskAlloc( dwNumAttrs * sizeof(HRESULT) );
		for( i = 0; i < dwNumAttrs; i++ )
		{
			switch( pdwAttrIDs[i] )
			{
			case ixAckComment:
				pComVar[i] = AckComment();
				rtn->pErrors[i] = S_OK;
				break;
			case ixAreas:
				{
				SAFEARRAY* s = SafeArrayCreateVector( VT_BSTR, 0, Areas().size() );
				long ix[1];
				for( ix[0] = 0; ix[0] < (long)Areas().size(); (ix[0])++ )
				{
					CComBSTR bs( Areas()[ix[0]].data() );
					SafeArrayPutElement( s, ix, bs.Detach() ); 
				}
				pComVar[i].vt = VT_ARRAY | VT_BSTR;	
				pComVar[i].parray = s;
				}
				rtn->pErrors[i] = S_OK;
				break;
			default:
				if( pdwAttrIDs[i] >= nStdAttrs() 
						&& pdwAttrIDs[i] < sizeEventAttributes() + nStdAttrs() )
				{
					pComVar[i] = operator[]( pdwAttrIDs[i] - nStdAttrs()) ;  // copy the optional attributes
					rtn->pErrors[i] = S_OK;
				}
				else
					rtn->pErrors[i] = E_FAIL;
			}
		}
	}
	catch( ... )
	{
		if( rtn )
		{
			CoTaskMemFree( rtn->szActiveSubCondition );
			CoTaskMemFree( rtn->szASCDefinition );
			CoTaskMemFree( rtn->szASCDescription );
			CoTaskMemFree( rtn->szAcknowledgerID );
			CoTaskMemFree( rtn->szComment );
			for( DWORD i = 0; i < rtn->dwNumSCs; i++ )
			{
				CoTaskMemFree( rtn->pszSCNames[i]  );
				CoTaskMemFree( rtn->pszSCDescriptions[i] );
				CoTaskMemFree( rtn->pszSCDefinitions[i] );
			}
			CoTaskMemFree( rtn->pszSCNames );
			CoTaskMemFree( rtn->pszSCDescriptions );
			CoTaskMemFree( rtn->pszSCDefinitions );
			CoTaskMemFree( rtn->pdwSCSeverities );
			CoTaskMemFree( rtn->pErrors );
			CoTaskMemFree( rtn );
			rtn = NULL;
		}
		delete [] pComVar;
	}

	return rtn;
}


BOOL OPCCondition::AreAreasEffectivelyEnabled() const 
{
	AREA_VECTOR::const_iterator it;
	for( it = m_Areas.begin(); it != m_Areas.end(); it++ )
	{
		const AreaNode *pNode = theAreaRootNode.find( (*it).data(), COPCEventAreaBrowser::seps );
		ATLASSERT( pNode );
		if( pNode )
		{
			if( !pNode->IsEffectivelyEnabled() )
				return FALSE;
		}
	}
	return TRUE; // all of the areas are enabled
}


BOOL OPCCondition::IsEffectivelyEnabled() const
{
	if( IsEnabled() ) // the condition is enabled
	{
		return AreAreasEffectivelyEnabled();
	}
	return FALSE;
}




OnEventClass::OnEventClass( LPCWSTR wszSource, LPCWSTR wszCondition, const OPCCondition& cond )
{
	m_RefCount = 1;
    

	wChangeMask = cond.ChangeMask();
    wNewState = cond.NewState();
    szSource = _wcsdup( wszSource );
    ftTime = cond.Time();
    szMessage = _wcsdup( cond.Message() );
	dwEventType = cond.EventType();
    dwEventCategory = cond.EventCategory();
    dwSeverity = cond.Severity();
    szConditionName = _wcsdup( wszCondition );
    szSubconditionName = _wcsdup( cond.SubconditionName() );
    wQuality = cond.Quality();
    bAckRequired = cond.AckRequired();
    ftActiveTime = cond.SubconditionTime();
    dwCookie = cond.Cookie();
	szActorID = _wcsdup( cond.ActorID() );

	// attributes
    dwNumEventAttrs = cond.sizeEventAttributes() + nStdAttrs();
    CComVariant *pComVar = new CComVariant[dwNumEventAttrs];
	
	pComVar[ixAckComment] = cond.AckComment();

	// create the safe array of areas

	SAFEARRAY* s = SafeArrayCreateVector( VT_BSTR, 0, cond.Areas().size() );
	long ix[1];
	for( ix[0] = 0; ix[0] < (long)cond.Areas().size(); (ix[0])++ )
	{
		CComBSTR bs( cond.Areas()[ix[0]].data() );
		SafeArrayPutElement( s, ix, bs ); 
	}

	pComVar[ixAreas].vt = VT_ARRAY | VT_BSTR;	
	pComVar[ixAreas].parray = s;


	for( DWORD i = nStdAttrs(); i < dwNumEventAttrs; i++ )
		pComVar[i] = cond[i - nStdAttrs()];  // copy the optional attributes

	pEventAttributes = pComVar;
}


OnEventClass::~OnEventClass()
{
	_ASSERT( m_RefCount == 0 );
	delete [] (CComVariant *)pEventAttributes;
	free( szActorID );
	free( szSubconditionName );
	free( szConditionName );
	free( szMessage );
	free( szSource );
}
