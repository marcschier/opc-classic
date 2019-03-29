// FileTime.h
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
//   $Workfile: FileTime.h $
//
//
// Org. Author: Jim Luth
//     $Author: Andrew $
//   $Revision: 17 $
//       $Date: 7/13/00 3:45p $
//    $Archive: /AWX32/AWXMMX32/Agent Toolkit/MMX Mapi Agent Wizard/Template/FileTime.h $
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
/*   $History: FileTime.h $
 * 
 * *****************  Version 17  *****************
 * User: Andrew       Date: 7/13/00    Time: 3:45p
 * Updated in $/AWX32/AWXMMX32/Agent Toolkit/MMX Mapi Agent Wizard/Template
 * 
 * *****************  Version 16  *****************
 * User: Jiml         Date: 2/21/00    Time: 6:07p
 * Updated in $/AWX32/AWXMMX32/Server
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 7/02/99    Time: 6:29p
 * Updated in $/OPC/AlarmEvents/SampleServer/CE
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 5/28/99    Time: 10:38a
 * Updated in $/AWX32/server
 * 
 * *****************  Version 13  *****************
 * User: Alaa         Date: 1/07/99    Time: 1:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 12/15/98   Time: 6:35p
 * Updated in $/GenRegistrar
 * Version 5,2,50,1 --  Demo mode timer is now started when client units
 * issued goes non-zero
 * 
 * *****************  Version 11  *****************
 * User: Alaa         Date: 12/10/98   Time: 7:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 8/19/98    Time: 1:53p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 7/28/98    Time: 6:34p
 * Updated in $/GenRegistrar/GenRegMon
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 7/24/98    Time: 6:14p
 * Updated in $/GenRegistrar
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 7/22/98    Time: 7:29p
 * Updated in $/GenRegistrar
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 7/16/98    Time: 4:41p
 * Updated in $/GenRegistrar
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 3/18/98    Time: 7:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 12/29/97   Time: 7:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 12/26/97   Time: 6:55p
 * Updated in $/OPC/AlarmEvents/SampleServer
*/
//
//
//*************************************************************************          
#pragma once
#ifndef __FILETIME_H
#define __FILETIME_H

#ifdef FTC_FORMAT
	#include "tstring.h"
#endif

#define MAXFILETIME                      (DWORDLONG(0xffffffffffffffff))

class FileTimeClass : public FILETIME
{
public:
	FileTimeClass() { dwLowDateTime = 0; dwHighDateTime = 0; }
	FileTimeClass( const FileTimeClass& ftc ) { operator=( (FILETIME&)ftc); }
	FileTimeClass( const FILETIME& ft ) { operator=(ft); }
	FileTimeClass( DWORDLONG dwl ) { operator=(dwl); }

	FileTimeClass& operator+=( const FILETIME& ft );
	FileTimeClass& operator+=( DWORDLONG dwl );
	FileTimeClass& operator-=( const FILETIME& ft );
	FileTimeClass& operator-=( DWORDLONG dwl );

	FileTimeClass operator+( const FILETIME& ft ) const;
	FileTimeClass operator+( DWORDLONG dwl ) const;
	FileTimeClass operator-( const FILETIME& ft ) const;
	FileTimeClass operator-( DWORDLONG dwl ) const;

	FileTimeClass& operator=( const FILETIME& ft );
	FileTimeClass& operator=( DWORDLONG dwl );

	void clear() { dwLowDateTime = 0; dwHighDateTime = 0; }
	bool empty() const { return (dwLowDateTime == 0 && dwHighDateTime == 0); }
	bool operator==( const FILETIME& ft ) const;
	bool operator!=( const FILETIME& ft ) const;
	bool operator<( const FILETIME& ft ) const;
	bool operator<=( const FILETIME& ft ) const;
	bool operator>( const FILETIME& ft ) const;
	bool operator>=( const FILETIME& ft ) const;

	operator DWORDLONG() const { return  *((DWORDLONG *)this); }

	FileTimeClass& GetSystemTime();

	static FILETIME milliseconds( DWORDLONG dwlMilliseconds );
	static FILETIME seconds( DWORDLONG dwlSeconds );
	static FILETIME minutes( DWORDLONG dwlMinutes );
	static FILETIME hours( DWORDLONG dwlHours );

#ifdef FTC_FORMAT
	bool Format( tstring& rtn, DWORD dwDateFlags = 0,DWORD dwTimeFlags = 0, bool bUTC = false, LCID lcid = LANG_USER_DEFAULT ) const;
	bool Format( tstring& rtn, LPCTSTR lpszDateFormat, LPCTSTR lpszTimeFormat, bool bUTC = false ) const;
	bool Format( tstring& rtn, UINT nDateFormatID, UINT nTimeFormatID, bool bUTC = false ) const;
#endif
};


inline 	FileTimeClass& FileTimeClass::GetSystemTime()
{
	CoFileTimeNow( this ); 
	return *this;
}


inline 	FileTimeClass& FileTimeClass::operator+=( const FILETIME& ft )
{
	*((DWORDLONG *)this) += *((DWORDLONG *)(&ft));
	return *this;
}


inline 	FileTimeClass FileTimeClass::operator+( const FILETIME& ft ) const
{
	FileTimeClass rtn = *this;
	rtn += ft;
	return rtn;
}

inline 	FileTimeClass& FileTimeClass::operator+=( DWORDLONG dwl )
{
	*((DWORDLONG *)this) += dwl;
	return *this;
}


inline 	FileTimeClass FileTimeClass::operator+( DWORDLONG dwl ) const
{
	FileTimeClass rtn = *this;
	rtn += dwl;
	return rtn;
}


inline 	FileTimeClass& FileTimeClass::operator-=( const FILETIME& ft )
{
	*((DWORDLONG *)this) -= *((DWORDLONG *)(&ft));
	return *this;
}


inline 	FileTimeClass FileTimeClass::operator-( const FILETIME& ft ) const
{
	FileTimeClass rtn = *this;
	rtn -= ft;
	return rtn;
}

inline 	FileTimeClass& FileTimeClass::operator-=( DWORDLONG dwl )
{
	*((DWORDLONG *)this) -= dwl;
	return *this;
}


inline 	FileTimeClass FileTimeClass::operator-( DWORDLONG dwl ) const
{
	FileTimeClass rtn = *this;
	rtn -= dwl;
	return rtn;
}



inline FileTimeClass& FileTimeClass::operator=( const FILETIME& ft )
{
	*((DWORDLONG *)this) = *((DWORDLONG *)(&ft));
	return *this;
}

inline FileTimeClass& FileTimeClass::operator=( DWORDLONG dwl )
{
	*((DWORDLONG *)this) = dwl;
	return *this;
}


inline bool FileTimeClass::operator==( const FILETIME& ft ) const
{
	return( *((DWORDLONG *)this) == *((DWORDLONG *)(&ft)) );
}


inline bool FileTimeClass::operator!=( const FILETIME& ft ) const
{
	return( *((DWORDLONG *)this) != *((DWORDLONG *)(&ft)) );
}


inline bool FileTimeClass::operator>( const FILETIME& ft ) const
{
	return( *((DWORDLONG *)this) > *((DWORDLONG *)(&ft)) );
}

inline bool FileTimeClass::operator>=( const FILETIME& ft ) const
{
	return( *((DWORDLONG *)this) >= *((DWORDLONG *)(&ft)) );
}

inline bool FileTimeClass::operator<( const FILETIME& ft ) const
{
	return( *((DWORDLONG *)this) < *((DWORDLONG *)(&ft)) );
}


inline bool FileTimeClass::operator<=( const FILETIME& ft ) const
{
	return( *((DWORDLONG *)this) <= *((DWORDLONG *)(&ft)) );
}



inline FILETIME FileTimeClass::milliseconds( DWORDLONG dwlMilliseconds )
{
	FILETIME rtn;
	*((DWORDLONG *)(&rtn)) = dwlMilliseconds * 10000;
	return rtn;
}

inline FILETIME FileTimeClass::seconds( DWORDLONG dwlSeconds )
{
	return milliseconds( dwlSeconds * 1000 );
}

inline FILETIME FileTimeClass::minutes( DWORDLONG dwlMinutes )
{
	return seconds( dwlMinutes * 60 );
}


inline FILETIME FileTimeClass::hours( DWORDLONG dwlHours )
{
	return minutes( dwlHours * 60 );
}

#endif
