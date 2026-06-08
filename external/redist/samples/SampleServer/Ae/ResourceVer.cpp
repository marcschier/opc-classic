// ResourceVer.cpp
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
// Description: Class for reading version info from resource
//
// Functions:   
//
//
//
//
//
/*   $History: ResourceVer.cpp $
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 7/02/99    Time: 6:29p
 * Updated in $/OPC/AlarmEvents/SampleServer/CE
 * 
 * *****************  Version 9  *****************
 * User: Alaa         Date: 1/07/99    Time: 1:06p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 8  *****************
 * User: Alaa         Date: 12/10/98   Time: 7:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:58a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 7/24/98    Time: 1:54p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 7/22/98    Time: 5:17p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 4/28/98    Time: 5:43p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 3/10/98    Time: 6:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
*/
//
//
//*************************************************************************          

#include "stdafx.h"
#include "ResourceVer.h"

#ifndef UNDER_CE

ResourceVersion::ResourceVersion( HMODULE hMod )
{
	m_bFromFile = false;
	m_pInfo = NULL;
	m_dwLangCodepage = 0;

	TCHAR path[_MAX_PATH+1];
	if( GetModuleFileName( hMod, path, _MAX_PATH ) )
		Init( path );
}




ResourceVersion::ResourceVersion( LPCTSTR szPath )
{
	Init( szPath );

}


ResourceVersion::~ResourceVersion()
{
	if( m_bFromFile && m_pInfo )
		free( m_pInfo );
}


void ResourceVersion::Init( LPCTSTR szPath )
{
	m_bFromFile = true;
	m_pInfo = NULL;
	m_dwLangCodepage = 0;

	DWORD dwHandle = 0;

	DWORD len = GetFileVersionInfoSize( const_cast<LPTSTR>(szPath), &dwHandle );
	if( len )
	{
		m_pInfo = malloc( len );
		if( m_pInfo )
		{
			if( !GetFileVersionInfo( const_cast<LPTSTR>(szPath), dwHandle, len, m_pInfo ) )
			{
				free( m_pInfo );
				m_pInfo  = NULL;
			}
		}
	}


	m_dwLangCodepage = 0x080004b0;	// UNICODE Language Neutral

/*
	if( m_pInfo )
	{

		WORD * lpBuffer = NULL;
		UINT dwBytes = 0;
		if( VerQueryValue(m_pInfo,
				              TEXT("\\VarFileInfo\\Translation"),
								(void**)&lpBuffer, &dwBytes) )
		{
			if( dwBytes >= sizeof(DWORD) )
			{
				m_dwLangCodepage = MAKELPARAM( lpBuffer[1], lpBuffer[0]) ; // use first lang-codepage
			}
		}
	}
*/
}


const VS_FIXEDFILEINFO* ResourceVersion::FixedFileInfo() const
{
	if( m_pInfo )
	{

	PVOID lpBuffer = NULL;
	UINT dwBytes = 0;
	if( VerQueryValue(m_pInfo,
				          TEXT("\\"),
							&lpBuffer, &dwBytes) )
		return (VS_FIXEDFILEINFO *)lpBuffer;
	}
	return NULL;
}


LPCTSTR ResourceVersion::StringFileInfo( LPCTSTR str ) const
{
	if( m_pInfo )
	{
		TCHAR buf[256];
		wsprintf( buf, _T("\\StringFileInfo\\%08lx\\%s"), m_dwLangCodepage, str );

		PVOID lpBuffer = NULL;
		UINT dwBytes = 0;
		if( VerQueryValue(m_pInfo,
							  buf,
								&lpBuffer, &dwBytes) )
			return (LPCTSTR)lpBuffer;
	}
	return NULL;
}

#endif UNDER_CE

