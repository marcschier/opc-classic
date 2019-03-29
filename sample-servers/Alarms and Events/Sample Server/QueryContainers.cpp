// QueryContainers.cpp
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
// Description: global queryable containers that are initialized at startup
//
// Functions:   
//
//
//
//
//
/*   $History: QueryContainers.cpp $
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 8/02/02    Time: 4:47p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 11/18/98   Time: 3:08p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:58a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 8/10/98    Time: 6:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 *  1. Added AckComment attribute
 *  2. Added Areas attribute
 *  3. revised ::GetConditionState() to include attributes
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 7/24/98    Time: 11:32a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 3/11/98    Time: 4:34p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 3/04/98    Time: 4:14p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 2/26/98    Time: 11:04a
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
#include "stdafx.h"
#include "QueryContainers.h"
#include <Commctrl.h>

LPCWSTR wszStdAttrNames[] = {L"AckComment", L"Areas" };
size_t nStdAttrs() { return sizeof(wszStdAttrNames) / sizeof(wszStdAttrNames[0]); }


ATTRIBUTES_VECTOR theAttributesVector;
CATEGORIES_MAP theCategoriesMap;
ConditionNameSet theConditionNameSet;
CONDITIONNAME_MAP theConditionNameMap;
AreaNode theAreaRootNode;
AreaNodeSet theEmptyAreaNodeSet;




// creates the root node i.e. no name and no parent
AreaNode::AreaNode()
{
	m_pChildren = NULL;
	m_pParent = NULL;
	m_bIsSource = false;
	m_bEnabled = true;
}

AreaNode::AreaNode( LPCWSTR wszName, AreaNode *pParent, bool bIsSource )
{
	m_pChildren = NULL;
	m_pParent = pParent;
	m_wsName = wszName;
	m_bIsSource = bIsSource;
	m_bEnabled = true;

	_ASSERT( !m_wsName.empty() );
	_ASSERT( m_pParent != NULL );
}


AreaNode::AreaNode( LPCSTR szName, AreaNode *pParent, bool bIsSource )
{
	USES_CONVERSION;
	m_pChildren = NULL;
	m_pParent = pParent;
	m_wsName = A2W(szName);
	m_bIsSource = bIsSource;
	m_bEnabled = true;

	_ASSERT( !m_wsName.empty() );
	_ASSERT( m_pParent != NULL );
}


AreaNode::~AreaNode()
{
	delete m_pChildren;
}


AreaNodeSet& AreaNode::Children()
{
	if( !m_pChildren )
		m_pChildren = new AreaNodeSet();

	return *m_pChildren;
}


const AreaNodeSet& AreaNode::ConstChildren() const
{
	if( m_pChildren )
		return *m_pChildren;

	return theEmptyAreaNodeSet;
}


// returned the area node using the fully qualified Area Name
AreaNode *AreaNode::find( LPCWSTR szQualifiedAreaName, LPCWSTR seps )
{
		AreaNode *pNode = this;
		LPWSTR dup = _wcsdup( szQualifiedAreaName );
		for( LPWSTR token = wcstok( dup, seps );
				token != NULL;
				token = wcstok( NULL, seps ) )

		{
			AreaNode test( token, pNode );
			AreaNodeSet::iterator it = pNode->Children().find( test );
			if( it == pNode->Children().end() )
			{
				free( dup );
				return NULL;	// can't find szString
			}										
			if( (*it).IsSource() )
			{
				free( dup );
				return NULL; // node is a Source, not an area
			}										
			pNode = const_cast<AreaNode*>(&(*it));
		}
		free( dup );
		return pNode;
	
}

BOOL AreaNode::IsEffectivelyEnabled() const
{
	if( IsEnabled() )
	{
		if( m_pParent )
		{
			return m_pParent->IsEffectivelyEnabled();
		}
		else
			return TRUE;
	}
	return FALSE;
}
