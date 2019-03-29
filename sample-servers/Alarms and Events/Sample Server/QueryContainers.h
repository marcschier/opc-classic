// QueryContainers.h
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
//   $Workfile: QueryContainers.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 14 $
//       $Date: 8/02/02 4:47p $
//    $Archive: /OPC/AlarmEvents/SampleServer/QueryContainers.h $
//
//      System: OPC Alarm & Events
//   Subsystem: 
//
//
// Description: global queryable containers that are initialized at startup
//
// Functions:   
//
//
//
//
//
/*   $History: QueryContainers.h $
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 8/02/02    Time: 4:47p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 8/20/98    Time: 9:56a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * moved tstring.h out of pre-compiled header to work around compiler bug
 * on WIN 95.
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 8/19/98    Time: 12:02p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 8/10/98    Time: 6:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 *  1. Added AckComment attribute
 *  2. Added Areas attribute
 *  3. revised ::GetConditionState() to include attributes
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 3/13/98    Time: 6:46p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 3/11/98    Time: 4:34p
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
 * User: Jiml         Date: 12/31/97   Time: 11:47a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 12/30/97   Time: 6:46p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:09a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 12/23/97   Time: 12:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
*/
//
//
//*************************************************************************          
#ifndef  __QUERYCONTAINERS_H
#define  __QUERYCONTAINERS_H

#include "tstring.h"
#pragma warning( disable : 4786 )
#pragma warning( disable : 4503 )
#include <set>
#include <map>
#include <vector>
#include <string>


using namespace std;


// array of names for the standard attributes
extern LPCWSTR wszStdAttrNames[];
size_t nStdAttrs(); 
#define ixAckComment 	0
#define ixAreas			1




// array of text names for the optional attributes
typedef vector<wstring> ATTRIBUTES_VECTOR;
extern ATTRIBUTES_VECTOR theAttributesVector;





struct CategoryData
{
	DWORD	dwID;
	DWORD	dwEventType;
};



typedef map<wstring, CategoryData, less<wstring> >  CATEGORIES_MAP;
extern CATEGORIES_MAP theCategoriesMap;


typedef set<wstring> SUBCONDITIONNAME_SET;


class ConditionName : public wstring
{
public:
	ConditionName( const wstring& s ) : wstring( s ) {}
	ConditionName( LPCWSTR s ) : wstring( s ) {}
	SUBCONDITIONNAME_SET	m_SubconditionNameSet;

	bool operator<( const ConditionName& test ) const 
						{ return (compare( test ) < 0 ); }
	bool operator<=( const ConditionName& test ) const 
						{ return (compare( test ) <= 0 ); }
	bool operator>( const ConditionName& test ) const 
						{ return (compare( test ) > 0 ); }
	bool operator>=( const ConditionName& test ) const 
						{ return (compare( test ) >= 0 ); }
	bool operator==( const ConditionName& test ) const 
						{ return (compare( test ) == 0 ); }
	bool operator!=( const ConditionName& test ) const 
						{ return (compare( test ) != 0 ); }


};



// typedef set<wstring> CONDITIONNAME_SET;
class ConditionNameSet : public set<ConditionName>
{
};


extern ConditionNameSet theConditionNameSet;


typedef set<LPCWSTR> CONDTIONNAMELPCWSTR_SET;

typedef map<DWORD, CONDTIONNAMELPCWSTR_SET, less<DWORD> > CONDITIONNAME_MAP;
extern CONDITIONNAME_MAP theConditionNameMap;


class AreaNodeSet;

class AreaNode
{
private: 
	wstring			m_wsName;
	AreaNodeSet*	m_pChildren;
	AreaNode*		m_pParent;
	bool			m_bIsSource;
	BOOL			m_bEnabled;
public:
	AreaNode();
	AreaNode( LPCWSTR wszName, AreaNode *pParent, bool bIsSource = false );
	AreaNode( LPCSTR szName,  AreaNode *pParent, bool bIsSource = false );
	~AreaNode();

	AreaNode* Parent() const { return m_pParent; }
	const AreaNodeSet& ConstChildren() const;
	AreaNodeSet& Children();
	LPCWSTR Name() const { return m_wsName.data(); }	
	const wstring wsName() const { return m_wsName; }	
	bool IsSource() const { return m_bIsSource; }

	AreaNode *find( LPCWSTR szQualifiedAreaName, LPCWSTR szSeps );

	BOOL IsEnabled() const { return m_bEnabled; }
	void Enable( BOOL b ) { m_bEnabled = b; }
	BOOL IsEffectivelyEnabled() const;

	int compare( const AreaNode& test ) const
		{
			return m_wsName.compare( test.m_wsName );
		}

	bool operator<( const AreaNode& test ) const 
						{ return (compare( test ) < 0 ); }
	bool operator<=( const AreaNode& test ) const 
						{ return (compare( test ) <= 0 ); }
	bool operator>( const AreaNode& test ) const 
						{ return (compare( test ) > 0 ); }
	bool operator>=( const AreaNode& test ) const 
						{ return (compare( test ) >= 0 ); }
	bool operator==( const AreaNode& test ) const 
						{ return (compare( test ) == 0 ); }
	bool operator!=( const AreaNode& test ) const 
						{ return (compare( test ) != 0 ); }


};


class AreaNodeSet : public set<AreaNode>
{
};


extern AreaNode theAreaRootNode;



#endif