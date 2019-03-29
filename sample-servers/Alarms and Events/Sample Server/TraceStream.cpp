// TraceStream.cpp
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
/*   $History: TraceStream.cpp $
 * 
 * *****************  Version 10  *****************
 * User: Alaa         Date: 1/07/99    Time: 1:06p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 9  *****************
 * User: Alaa         Date: 12/10/98   Time: 7:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 11/18/98   Time: 3:08p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:59a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 4/06/98    Time: 10:44a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 4/02/98    Time: 6:52p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 3/12/98    Time: 4:55p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 12/26/97   Time: 6:55p
 * Updated in $/OPC/AlarmEvents/SampleServer
*/
//
//
//*************************************************************************          
#include "stdafx.h"
#include <comdef.h>
#include "TraceStream.h"

#ifdef _DEBUG

TraceStream trace;



int TraceStreambuf::sync()
{
	USES_CONVERSION;
	sputc( '\0' ); // null terminate the string
	char *p = str();
#ifdef _DEBUG
	OutputDebugString( A2T(p) );
#endif
	freeze( FALSE );
	seekpos( 0 );
	// call base class
	return strstreambuf::sync();
}


#endif


ostream& operator<<( ostream& os, const VARIANT& v )
{
	if( v.vt == VT_NULL )
		os << "{VT_NULL}";
	else if( v.vt == VT_EMPTY )
		os << "{VT_EMPTY}";
	else
	{
		try
		{
			_variant_t s( v );
			os << (LPCTSTR)((_bstr_t)s);
		}
		catch( ... )
		{
			os << "{Non convertable VARTYPE " << v.vt << " }";
		}
	}
	return os;
}

