// OPCBrowseConditions.h
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
//   $Workfile: OPCBrowseConditions.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 7 $
//       $Date: 8/19/98 1:52p $
//    $Archive: /OPC/AlarmEvents/SampleServer/OPCBrowseConditions.h $
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
/*   $History: OPCBrowseConditions.h $
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 8/19/98    Time: 1:52p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:07a
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
// OPCBrowseConditions.h : Declaration of the COPCBrowseConditions

#ifndef __OPCBROWSECONDITIONS_H_
#define __OPCBROWSECONDITIONS_H_

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// COPCBrowseConditions
class ATL_NO_VTABLE COPCBrowseConditions : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COPCBrowseConditions, &CLSID_OPCBrowseConditions>,
	public IOPCBrowseConditions
{
public:
	COPCBrowseConditions()
	{
	}

// DECLARE_REGISTRY_RESOURCEID(IDR_OPCBROWSECONDITIONS)
DECLARE_NOT_AGGREGATABLE(COPCBrowseConditions)

BEGIN_COM_MAP(COPCBrowseConditions)
	COM_INTERFACE_ENTRY(IOPCBrowseConditions)
END_COM_MAP()

// IOPCBrowseConditions
public:
};

#endif //__OPCBROWSECONDITIONS_H_
