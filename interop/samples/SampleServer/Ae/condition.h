// condition.h
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
//   $Workfile: condition.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 20 $
//       $Date: 8/02/02 4:47p $
//    $Archive: /OPC/AlarmEvents/SampleServer/condition.h $
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
/*   $History: condition.h $
 * 
 * *****************  Version 20  *****************
 * User: Jiml         Date: 8/02/02    Time: 4:47p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 19  *****************
 * User: Jiml         Date: 3/29/99    Time: 2:49p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 18  *****************
 * User: Jiml         Date: 8/20/98    Time: 9:56a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * moved tstring.h out of pre-compiled header to work around compiler bug
 * on WIN 95.
 * 
 * *****************  Version 17  *****************
 * User: Jiml         Date: 8/19/98    Time: 12:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 16  *****************
 * User: Jiml         Date: 8/10/98    Time: 6:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 *  1. Added AckComment attribute
 *  2. Added Areas attribute
 *  3. revised ::GetConditionState() to include attributes
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 3/31/98    Time: 11:43a
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
 * User: Jiml         Date: 12/31/97   Time: 2:47p
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
 * User: Jiml         Date: 12/24/97   Time: 10:06a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 12/19/97   Time: 6:40p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 12/18/97   Time: 6:29p
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
#ifndef __CONDITION_H
#define __CONDITION_H

#include "FileTime.h"
#include "tstring.h"

#pragma warning( disable : 4786 )
#include <vector>
#include <algorithm>

using namespace std;

/*

class OPCConditionKey
{
private:
    wstring			m_wsSource;
    wstring			m_wsConditionName;
public:
	OPCConditionKey() {};
	OPCConditionKey( LPCWSTR wszSource, LPCWSTR wszConditionName )
		{ 
			m_wsSource = wszSource;
			m_wsConditionName = wszConditionName; 
		}
	OPCConditionKey( LPCSTR szSource, LPCSTR szConditionName )
		{
			USES_CONVERSION;
			m_wsSource = A2W( szSource );
			m_wsConditionName = A2W( szConditionName );
		}

	LPCWSTR Source() const { return m_wsSource.data(); }
	LPCWSTR ConditionName() const { return m_wsConditionName.data(); }

	int compare( const OPCConditionKey& test ) const
		{
			int rtn = m_wsSource.compare( test.m_wsSource );
			if( rtn == 0 )
				rtn = m_wsConditionName.compare( test.m_wsConditionName );
			return rtn;
		}

	bool operator<( const OPCConditionKey& test ) const 
						{ return (compare( test ) < 0 ); }
	bool operator<=( const OPCConditionKey& test ) const 
						{ return (compare( test ) <= 0 ); }
	bool operator>( const OPCConditionKey& test ) const 
						{ return (compare( test ) > 0 ); }
	bool operator>=( const OPCConditionKey& test ) const 
						{ return (compare( test ) >= 0 ); }
	bool operator==( const OPCConditionKey& test ) const 
						{ return (compare( test ) == 0 ); }
	bool operator!=( const OPCConditionKey& test ) const 
						{ return (compare( test ) != 0 ); }
};

*/


class OPCSubcondition
{
private:
	wstring		m_wsName;
	wstring		m_wsMessage;
	wstring		m_wsDefinition;
	DWORD		m_dwSeverity;
public:
	OPCSubcondition( LPCWSTR wszName ) : m_wsName( wszName ) {}
	OPCSubcondition( LPCWSTR wszName, LPCWSTR wszMessage, LPCWSTR wszDefinition,
					DWORD dwSeverity );
	LPCWSTR Name() const { return m_wsName.data(); }
	LPCWSTR Message() const { return m_wsMessage.data(); }
	LPCWSTR Definition() const { return m_wsDefinition.data(); }
	DWORD Severity() const { return m_dwSeverity; }

	bool operator==( const OPCSubcondition& test ) const
								{ return( m_wsName == test.m_wsName ); }
};



template< class T> class unique_vector : public vector<T>
{
public:
	void push_back_unique( const T& toAdd )
	{
		if( find( begin(), end(), toAdd ) == end() )
			push_back( toAdd );
	}
};



typedef vector<CComVariant> VARIANT_VECTOR; 
typedef unique_vector<wstring> AREA_VECTOR;
typedef unique_vector<OPCSubcondition> SUBCONDITION_VECTOR;


class OPCCondition
{
private:

    WORD			m_wChangeMask;
    WORD			m_wNewState;
    FileTimeClass	m_ftTime;
	FileTimeClass	m_ftLastAckTime;
    wstring			m_wsMessage;
    DWORD			m_dwEventType;
    DWORD			m_dwEventCategory;
    DWORD			m_dwSeverity;
    wstring			m_wsSubconditionName;
    WORD			m_wQuality;
    BOOL			m_bAckRequired;
    FileTimeClass	m_ftActiveTime;
	FileTimeClass	m_ftSubconditionTime;
	FileTimeClass	m_ftRTNTime;
    DWORD			m_dwCookie;
	VARIANT_VECTOR	m_EventAttributes;
    wstring			m_wsActorID;
	wstring			m_wsAckComment;
	BOOL			m_bEnabled;

	AREA_VECTOR				m_Areas;
	SUBCONDITION_VECTOR		m_Subconditions;

public:
	OPCCondition( BOOL bEnabled = TRUE, WORD wInitialQuality = OPC_QUALITY_GOOD);  // constructor sets reasonable defaults

	WORD ChangeMask() const { return m_wChangeMask; }
	WORD NewState() const { return m_wNewState; }
	FileTimeClass Time() const { return m_ftTime; }
	LPCWSTR Message() const { return m_wsMessage.data(); }
	DWORD EventType() const { return m_dwEventType; }
	DWORD EventCategory() const { return m_dwEventCategory; }
	DWORD Severity() const { return m_dwSeverity; }
	LPCWSTR SubconditionName() const { return m_wsSubconditionName.data(); }
	WORD Quality() const { return m_wQuality; }
	BOOL AckRequired() const { return m_bAckRequired; }
	FileTimeClass ActiveTime() const { return m_ftActiveTime; }
	FileTimeClass SubconditionTime() const { return m_ftSubconditionTime; }
	FileTimeClass LastAckTime() const { return m_ftLastAckTime; }
	FileTimeClass RTNTime() const { return m_ftRTNTime; }
	
	DWORD Cookie() const { return (DWORD)this; }
	DWORD sizeEventAttributes() const { return m_EventAttributes.size(); }
	const CComVariant& operator[](DWORD i) const { return m_EventAttributes[i]; }
	LPCWSTR ActorID() const { return m_wsActorID.data(); }
	LPCWSTR AckComment() const { return m_wsAckComment.data(); }
	
	BOOL IsEnabled() const { return m_bEnabled; }
	BOOL IsActive() const { return (m_wNewState & OPC_CONDITION_ACTIVE) ? TRUE : FALSE; }
	BOOL IsAcked() const { return (m_wNewState & OPC_CONDITION_ACKED) ? TRUE : FALSE; }
	BOOL IsEffectivelyEnabled() const;
	BOOL AreAreasEffectivelyEnabled() const;
	BOOL UpdateEffectiveState();


	const AREA_VECTOR& Areas() const { return m_Areas; }
	const SUBCONDITION_VECTOR& Subconditions() const { return m_Subconditions; }


	void ClearChangeMask() { m_wChangeMask = 0; }

	// set m_wNewState
	void Enable( BOOL bEnable );
	void Active( BOOL bActive );// TRUE == in alarm, FALSE == Return to Normal
	BOOL Ack( LPCWSTR szActor, LPCWSTR szComment = NULL ); // Acknowlege 
	BOOL Ack( LPCSTR szActor, LPCSTR szComment = NULL )
			{ USES_CONVERSION; return Ack( A2W(szActor), A2W(szComment) ); }


	// set TimeOfEvent
	virtual void Time( FILETIME ftTime );
	virtual void Time();  // time of event is now

	void Message( LPCWSTR wszString );
	void Message( LPCSTR szString ) { USES_CONVERSION; m_wsMessage = A2W(szString); }
	
	void EventType( DWORD dwEventType ) { m_dwEventType = dwEventType; }
	void EventCategory( DWORD dwEventCategory ) { m_dwEventCategory = dwEventCategory; }
	void Severity( DWORD dwSeverity );

//	void ConditionName( LPCWSTR wszConditionName ) { m_wsConditionName = wszConditionName; }
//	void ConditionName( LPCSTR szConditionName ) { USES_CONVERSION; ConditionName( A2W(szConditionName) ); }
	
	void SubconditionName( LPCWSTR wszSubconditionName );
	void SubconditionName( LPCSTR szSubconditionName ) { USES_CONVERSION; SubconditionName( A2W(szSubconditionName) ); }

	void Quality( WORD wQuality );
	void AckRequired( BOOL bAckReq );

	void ActorID( LPCWSTR wszString ) { m_wsActorID = wszString; }
	void ActorID( LPCSTR szString ) { USES_CONVERSION; ActorID( A2W(szString) ); }

	void push_back( const CComVariant& AnEventAttribute ) { m_EventAttributes.push_back( AnEventAttribute ); }
	void clear() { m_EventAttributes.clear(); }

	void SetChangeAttribute() { m_wChangeMask |= OPC_CHANGE_ATTRIBUTE; }

	void push_back_area( const wstring& sArea ) { m_Areas.push_back_unique( sArea ); }

	void push_back_subcondition( const OPCSubcondition& sub )
								{ m_Subconditions.push_back_unique( sub ); }

	OPCCONDITIONSTATE *CoTaskAllocConditionState( DWORD dwNumAttrs, DWORD *pdwAttrIDs ) const;
};




class OnEventClass : 
		public ONEVENTSTRUCT
{
private:
	LONG		m_RefCount;
	~OnEventClass(); // destructor only called by Release
public:
//	OnEventClass( const OPCConditionKey& key, const OPCCondition cond );
	OnEventClass( LPCWSTR wszSource, LPCWSTR wszCondition, const OPCCondition& cond );
	void Release()		// works like COM Release
		{
			if( InterlockedDecrement( &m_RefCount ) == 0 )
				delete this;
		};


	void AddRef()      // works like COM AddRef
		{
			InterlockedIncrement( &m_RefCount );
		};
};




inline 	OPCSubcondition::OPCSubcondition( LPCWSTR wszName, LPCWSTR wszMessage, 
			LPCWSTR wszDefinition, DWORD dwSeverity )
			: m_wsName( wszName ), m_wsMessage( wszMessage ),
			  m_wsDefinition( wszDefinition ), m_dwSeverity( dwSeverity )
			{}




#endif

