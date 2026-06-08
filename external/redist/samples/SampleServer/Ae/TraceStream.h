// TraceStream.h
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
//   $Workfile: TraceStream.h $
//
//
// Org. Author: Jim Luth
//     $Author: Alaa $
//   $Revision: 7 $
//       $Date: 1/07/99 1:06p $
//    $Archive: /AWX32/server/TraceStream.h $
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
/*   $History: TraceStream.h $
 * 
 * *****************  Version 7  *****************
 * User: Alaa         Date: 1/07/99    Time: 1:06p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 6  *****************
 * User: Alaa         Date: 12/10/98   Time: 7:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 8/19/98    Time: 12:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 4/02/98    Time: 6:52p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 12/26/97   Time: 6:55p
 * Updated in $/OPC/AlarmEvents/SampleServer
*/
//
//
//*************************************************************************          
#ifndef __TRACESTREAM_H
#define __TRACESTREAM_H


#include <strstream>
using namespace std;


#ifdef _DEBUG



class TraceStreambuf : public strstreambuf
{
public:
	explicit TraceStreambuf(streamsize _N = 0) : strstreambuf( _N ) {}

protected:
	virtual int sync();
};



class TraceStream : public ostream 
{
public:
	TraceStream()
		: ostream(&_Sb), _Sb() {}
	strstreambuf *rdbuf() const
		{return ((strstreambuf *)&_Sb); }
	void freeze(bool _F = true)
		{_Sb.freeze(_F); }
	char *str()
		{return (_Sb.str()); }
	streamsize pcount() const
		{return (_Sb.pcount()); }
private:
	TraceStreambuf _Sb;
};


extern TraceStream trace;
#define TRACESTREAM( a )  trace << a

#else

#define TRACESTREAM( a )  0


#endif

ostream& operator<<( ostream& os, const VARIANT& v );


#endif
