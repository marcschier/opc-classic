// StdAfx.h
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
//   $Workfile: StdAfx.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 14 $
//       $Date: 8/14/01 12:50p $
//    $Archive: /OPC/AlarmEvents/SampleServer/StdAfx.h $
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
/*   $History: StdAfx.h $
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 8/14/01    Time: 12:50p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 8/20/98    Time: 9:56a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * moved tstring.h out of pre-compiled header to work around compiler bug
 * on WIN 95.
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 8/19/98    Time: 12:01p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 7/24/98    Time: 11:32a
 * Updated in $/OPC/AlarmEvents/SampleServer
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
 * User: Jiml         Date: 2/26/98    Time: 11:04a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 1/02/98    Time: 6:29p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 12/29/97   Time: 7:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:10a
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
// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#if !defined(AFX_STDAFX_H__EA35CE5C_59D6_11D1_84A0_00608CB8A7E9__INCLUDED_)
#define AFX_STDAFX_H__EA35CE5C_59D6_11D1_84A0_00608CB8A7E9__INCLUDED_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

#define STRICT


#define _WIN32_WINNT 0x0400
#define _ATL_FREE_THREADED


#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
class CExeModule : public CComModule
{
public:
	LONG Unlock();
	DWORD dwThreadID;
};
extern CExeModule _Module;
#include <atlcom.h>

#define TRACE	ATLTRACE

// mappings for MatchPattern.h
#define MCHAR		WCHAR
#define _M(x)		L ## x
#define _ismdigit   iswdigit
#define _tomupper	towupper




#include "opc_ae.h"
#include "opcae_er.h"
#include "opccomn.h"
#include "opcaedef.h"


//{{AFX_INSERT_LOCATION}}
// Microsoft Developer Studio will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_STDAFX_H__EA35CE5C_59D6_11D1_84A0_00608CB8A7E9__INCLUDED)
