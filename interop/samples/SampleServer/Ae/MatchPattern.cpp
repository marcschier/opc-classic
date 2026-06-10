// MatchPattern.cpp
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
//      System: Many
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
/*   $History: MatchPattern.cpp $
 * 
 * *****************  Version 13  *****************
 * User: David        Date: 7/17/00    Time: 5:46p
 * Updated in $/Trending 32/TwxViewer
 * 
 * *****************  Version 12  *****************
 * User: Chris        Date: 3/18/99    Time: 12:57p
 * Updated in $/GWX32/Gwx32CE
 * modified "toupper" to compile for WinCE
 * 
 * *****************  Version 11  *****************
 * User: Alaa         Date: 1/07/99    Time: 1:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 10  *****************
 * User: Alaa         Date: 12/10/98   Time: 7:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:54a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 2/18/98    Time: 11:55a
 * Updated in $/Security/Server
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 12/31/97   Time: 11:47a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 12/29/97   Time: 7:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 10/29/97   Time: 6:37p
 * Updated in $/Security/Server
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 10/27/97   Time: 5:50p
 * Updated in $/Security/Server
*/
//
//
//*************************************************************************          
#include "stdafx.h"
#include "MatchPattern.h"



inline int ConvertCase( MCHAR c, BOOL bCaseSensitive )
{
	return bCaseSensitive ? c : _tomupper(c);
}



//*************************************************************************          
// return TRUE if String Matches Pattern -- 
// -- uses Visual Basic LIKE operator syntax
// CAUTION: Function is recursive
//*************************************************************************          
BOOL MatchPattern( const MCHAR *String, const MCHAR *Pattern, BOOL bCaseSensitive )
{ 
	if( !String )
		return FALSE;
	if( !Pattern )
		return TRUE;
	MCHAR   c, p, l;
	for (; ;)
	{
		switch (p = ConvertCase( *Pattern++, bCaseSensitive ) )
		{
		case 0:                             // end of pattern
			return *String ? FALSE : TRUE;  // if end of string TRUE

		case _M('*'):
			while (*String) 
			{               // match zero or more char
				if (MatchPattern (String++, Pattern, bCaseSensitive))
					return TRUE; 
			}
			return MatchPattern (String, Pattern, bCaseSensitive );

		case _M('?'):
			if (*String++ == 0)             // match any one char 
				return FALSE;                   // not end of string 
			break; 

		case _M('['): 
			if ( (c = ConvertCase( *String++, bCaseSensitive) ) == 0)      // match char set 
				return FALSE;                   // syntax 
			l = 0; 
			if( *Pattern == _M('!') )  // match a char if NOT in set []
			{
				++Pattern;

				while( (p = ConvertCase( *Pattern++, bCaseSensitive) ) != _M('\0') ) 
				{
					if (p == _M(']'))               // if end of char set, then 
						break;           // no match found 

					if (p == _M('-')) 
					{            // check a range of chars? 
						p = ConvertCase( *Pattern, bCaseSensitive );   // get high limit of range 
						if (p == 0  ||  p == _M(']')) 
							return FALSE;           // syntax 

						if (c >= l  &&  c <= p) 
							return FALSE;              // if in range, return FALSE 
					} 
					l = p;
					if (c == p)                 // if char matches this element 
						return FALSE;                  // return false 
				} 
			}
			else	// match if char is in set []
			{
				while( (p = ConvertCase( *Pattern++, bCaseSensitive) ) != _M('\0') ) 
				{
					if (p == _M(']'))               // if end of char set, then 
						return FALSE;           // no match found 

					if (p == _M('-')) 
					{            // check a range of chars? 
						p = ConvertCase( *Pattern, bCaseSensitive );   // get high limit of range 
						if (p == 0  ||  p == _M(']')) 
							return FALSE;           // syntax 

						if (c >= l  &&  c <= p) 
							break;              // if in range, move on 
					} 
					l = p;
					if (c == p)                 // if char matches this element 
						break;                  // move on 
				} 

				while (p  &&  p != _M(']'))         // got a match in char set 
					p = *Pattern++;             // skip to end of set 
			}

			break; 

		case _M('#'):
			c = *String++; 
			if( !_ismdigit( c ) )
				return FALSE;		// not a digit

			break;

		default: 
			c = ConvertCase( *String++, bCaseSensitive ); 
			if( c != p )            // check for exact char 
				return FALSE;                   // not a match 

			break; 
		} 
	} 
} 

