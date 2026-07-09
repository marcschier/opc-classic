// ConditionMap.h
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
//   $Workfile: ConditionMap.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 6 $
//       $Date: 8/19/98 1:53p $
//    $Archive: /OPC/AlarmEvents/SampleServer/ConditionMap.h $
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
/*   $History: ConditionMap.h $
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 8/19/98    Time: 1:53p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 3/04/98    Time: 4:14p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:06a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 12/18/97   Time: 6:29p
 * Updated in $/OPC/AlarmEvents/SampleServer
*/
//
//
//*************************************************************************          

#ifndef __CONDITIONMAP_H
#define __CONDITIONMAP_H

#include "condition.h"
#pragma warning( disable : 4786 )
#include <map>

using namespace std;


// map condition name to OPCCondition
class ConditionMap : public  map<wstring, OPCCondition, less<wstring> >
{
};



class SourceMapPtr;

// map source name to CONDITION_MAP
class SourceMap : 
	public CComAutoCriticalSection,
	public map< wstring, ConditionMap, less<wstring> > 
{
private:
	friend SourceMapPtr;
	static SourceMap theSourceMap;
	SourceMap(){};
	SourceMap( const SourceMap& src );   // disallow copy constructor -- no implementation
};



class SourceMapPtr
{
private:
	SourceMapPtr( const SourceMapPtr& src );   // disallow copy constructor -- no implementation
public:
	SourceMapPtr() { SourceMap::theSourceMap.Lock(); }
	virtual ~SourceMapPtr() { SourceMap::theSourceMap.Unlock(); }
	
	SourceMap * operator->() { return &SourceMap::theSourceMap; }
};


#endif

