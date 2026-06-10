// MatchPattern.h
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
//   $Workfile: MatchPattern.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 14 $
//       $Date: 10/16/00 11:48a $
//    $Archive: /AWX32/server/MatchPattern.h $
//
//      System: GENESIS-32
//   Subsystem: Security
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
/*   $History: MatchPattern.h $
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 10/16/00   Time: 11:48a
 * Updated in $/AWX32/server
 * 
 * *****************  Version 13  *****************
 * User: David        Date: 7/17/00    Time: 5:46p
 * Updated in $/Trending 32/TwxViewer
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 3/30/99    Time: 5:08p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 11  *****************
 * User: Chris        Date: 3/18/99    Time: 12:57p
 * Updated in $/GWX32/Gwx32CE
 * modified "toupper" to compile for WinCE
 * 
 * *****************  Version 10  *****************
 * User: Alaa         Date: 1/07/99    Time: 1:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 9  *****************
 * User: Alaa         Date: 12/10/98   Time: 7:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 8/19/98    Time: 1:52p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 3/17/98    Time: 5:04p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 2/18/98    Time: 11:55a
 * Updated in $/Security/Server
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 12/29/97   Time: 7:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 10/27/97   Time: 5:50p
 * Updated in $/Security/Server
*/
//
//
//*************************************************************************          

#ifndef __MATCHPATTERN_H
#define __MATCHPATTERN_H

// By redefining MCHAR, _M, _ismdigit, and _tomupper you may alter the type
// of string MatchPattern() works with. For example to operate on
// wide strings, make the following definitions:
// #define MCHAR		WCHAR
// #define _M(x)		L ## x
// #define _ismdigit   iswdigit
// #define _tomupper   towupper



#ifndef MCHAR

#define MCHAR		TCHAR
#define _M(a)		_T(a)
#define _ismdigit   _istdigit
#define _tomupper   _totupper
#define T2CM			
#define A2CM		A2CT
#define W2CM		W2CT

#endif



extern BOOL  MatchPattern( const MCHAR* String, const MCHAR * Pattern, BOOL bCaseSensitive = FALSE );


// For OPC NULL or empty string is always a match !!!!
inline int MatchPatternOPC( const MCHAR* String, const MCHAR * Pattern, BOOL bCaseSensitive = FALSE )
{
	if( !Pattern )
		return TRUE;
	if( *Pattern == '\0' )
		return TRUE;
	return MatchPattern( String, Pattern, bCaseSensitive );
}



#endif

