// tstring.cpp
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
/*   $History: tstring.cpp $
 * 
 * *****************  Version 43  *****************
 * User: James        Date: 8/06/02    Time: 3:26p
 * Updated in $/AWX32/AWXview32
 * 
 * *****************  Version 42  *****************
 * User: Jiml         Date: 9/21/01    Time: 5:17p
 * Updated in $/AWX32/AWXLog32
 * Added code to create ODBC DSN on the fly.
 * 
 * *****************  Version 41  *****************
 * User: Jiml         Date: 2/02/01    Time: 11:29a
 * Updated in $/AWX32/AlarmOle
 * 
 * *****************  Version 40  *****************
 * User: Jiml         Date: 2/01/01    Time: 5:57p
 * Updated in $/IcoComn
 * 
 * *****************  Version 39  *****************
 * User: Jiml         Date: 1/31/01    Time: 4:37p
 * Updated in $/IcoComn
 * 
 * *****************  Version 38  *****************
 * User: Jiml         Date: 11/03/00   Time: 3:00p
 * Updated in $/GenTray
 * 
 * *****************  Version 37  *****************
 * User: Jiml         Date: 1/14/00    Time: 11:18a
 * Updated in $/AWX32/AWXLog32
 * 
 * *****************  Version 36  *****************
 * User: Jiml         Date: 1/12/00    Time: 6:30p
 * Updated in $/GenRegistrar
 * Added code the tstring::GetModuleFileName to insure a long file name is
 * returned.  
 * 
 * *****************  Version 35  *****************
 * User: Jiml         Date: 9/13/99    Time: 6:04p
 * Updated in $/GenRegistrar
 * 
 * *****************  Version 34  *****************
 * User: Jiml         Date: 9/09/99    Time: 6:21p
 * Updated in $/GenRegistrar/GenRegMon/GenRegMonCE
 * 
 * *****************  Version 33  *****************
 * User: Jiml         Date: 6/18/99    Time: 2:53p
 * Updated in $/AWX32/AWXTray
 * 
 * *****************  Version 32  *****************
 * User: Jiml         Date: 5/06/99    Time: 12:03p
 * Updated in $/AWX32/AlarmOle
 * First CE Version
 * 
 * *****************  Version 31  *****************
 * User: Jiml         Date: 4/07/99    Time: 6:30p
 * Updated in $/Expressions/IcoExpressionEngine
 * 
 * *****************  Version 30  *****************
 * User: Alaa         Date: 1/07/99    Time: 1:06p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 29  *****************
 * User: Alaa         Date: 12/10/98   Time: 7:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 28  *****************
 * User: Jiml         Date: 11/30/98   Time: 5:45p
 * Updated in $/GenRegistrar
 * 
 * *****************  Version 27  *****************
 * User: Jiml         Date: 11/18/98   Time: 3:08p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 26  *****************
 * User: Jiml         Date: 11/16/98   Time: 6:41p
 * Updated in $/AWX32/AWXLog32/AWXLogCfg
 * 
 * *****************  Version 25  *****************
 * User: Chris        Date: 11/13/98   Time: 3:13p
 * Updated in $/AWX32/server
 * fixed bug in SpanIncluding()
 * 
 * *****************  Version 24  *****************
 * User: Jiml         Date: 11/12/98   Time: 3:08p
 * Updated in $/AWX32/AWXLog32
 * 
 * *****************  Version 23  *****************
 * User: Jiml         Date: 11/11/98   Time: 6:25p
 * Updated in $/AWX32/AWXLog32
 * 
 * *****************  Version 22  *****************
 * User: Jiml         Date: 11/11/98   Time: 6:03p
 * Updated in $/AWX32/AWXLog32
 * 
 * *****************  Version 21  *****************
 * User: Jiml         Date: 11/09/98   Time: 4:34p
 * Updated in $/AWX32/AWXLog32
 * 
 * *****************  Version 20  *****************
 * User: Jiml         Date: 10/29/98   Time: 4:41p
 * Updated in $/AWX32/server
 * Added expressions
 * 
 * *****************  Version 19  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:59a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 18  *****************
 * User: Jiml         Date: 8/17/98    Time: 11:56a
 * Updated in $/GenRegistrar
 * Added CString workalike
 * 
 * *****************  Version 17  *****************
 * User: Jiml         Date: 7/27/98    Time: 5:01p
 * Updated in $/GenRegistrar
 * 
 * *****************  Version 16  *****************
 * User: Jiml         Date: 5/20/98    Time: 12:06p
 * Updated in $/AWX32/AlarmOle
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 5/15/98    Time: 6:48p
 * Updated in $/AWX32/AlarmOle
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 5/13/98    Time: 7:42p
 * Updated in $/AWX32/AlarmOle
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 4/22/98    Time: 6:55p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 4/17/98    Time: 3:40p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 4/16/98    Time: 11:44a
 * Updated in $/AWX32/server
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 3/31/98    Time: 6:33p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 3/31/98    Time: 12:43p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 3/30/98    Time: 6:02p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 3/12/98    Time: 4:55p
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
#include "stdafx.h"
#include "tstring.h"
#include <stdexcept>

#ifdef USE_REMOTE_COMPUTER_NAME
#ifdef REPLACE_COMP_NAME_FILE
#	include "RemoteComputerName.h"
#else
#	include "RemoteComputerName.cpp"
#endif
#endif

// determine number of elements in an array (not bytes)
#ifndef _countof
#define _countof(array) (sizeof(array)/sizeof(array[0]))
#endif

#ifndef UNDER_CE

typedef DWORD (WINAPI* LONGPATHPROC)( LPCTSTR, LPTSTR, DWORD );

DWORD MyGetLongPathName(
  LPCTSTR lpszShortPath, // file name
  LPTSTR lpszLongPath,   // path buffer
  DWORD cchBuffer        // size of path buffer 
)
{
	static LONGPATHPROC farProc = NULL;
	if( !farProc )
	{
		HMODULE hMod = GetModuleHandle( _T("KERNEL32") );
#ifdef _UNICODE
		farProc = (LONGPATHPROC)GetProcAddress( hMod, "GetLongPathNameW" );
#else
		farProc = (LONGPATHPROC)GetProcAddress( hMod, "GetLongPathNameA" );
#endif
	}
	return (farProc)( lpszShortPath, lpszLongPath, cchBuffer );
}



runtime_error xCoTaskAlloc( "CoTaskAlloc" );
#endif

wchar_t *wcsstri( const wchar_t *string, const wchar_t *strCharSet )
{
	size_t n = wcslen( strCharSet );
	for( const wchar_t *p = string; *p != 0; p++ )
	{
		if( _wcsnicmp( p, strCharSet, n ) == 0 )
			return (wchar_t *)p;
	}
	return NULL;
}


#ifndef UNDER_CE
char *strstri( const char *string, const char *strCharSet )
{
	size_t n = strlen( strCharSet );
	for( const char *p = string; *p != 0; p++ )
	{
		if( strnicmp( p, strCharSet, n ) == 0 )
			return (char *)p;
	}
	return NULL;
}
#endif


PVOID CoTaskAlloc( ULONG cb )
{
	PVOID rtn = CoTaskMemAlloc( cb );
#ifndef UNDER_CE
	if( !rtn )
		throw xCoTaskAlloc;
	memset( rtn, 0, cb );	// clear memory allocated
#endif
	return rtn;	
}



LPWSTR CoTaskAlloc( LPCWSTR lpStr ) 
{
	int nLen = wcslen( lpStr );
	LPWSTR rtn = (LPWSTR)CoTaskMemAlloc(2*(nLen+1));
	if( rtn )
	{
		memcpy( rtn, lpStr, 2 * nLen );
		rtn[ nLen ] = 0;
	}
#ifndef UNDER_CE
	else
		throw xCoTaskAlloc;
#endif
	return rtn;
}

#ifndef _UNICODE
LPWSTR CoTaskAlloc( LPCTSTR  lpStr ) 
{
	int nLen = _tcslen( lpStr );
#ifdef OLE2ANSI 
	LPWSTR rtn = (LPWSTR)CoTaskMemAlloc(2*(nLen+1));
	if( rtn )
	{
		memcpy( rtn, lpStr, 2 * nLen );
		rtn[ nLen ] = 0;
	}
#else
	int nOutLen = 1 + MultiByteToWideChar(CP_ACP, 0, lpStr,
		nLen, NULL, NULL);
	LPWSTR rtn = (LPWSTR)CoTaskAlloc(2*nOutLen );
	if( rtn )
	{
		MultiByteToWideChar(CP_ACP, 0, lpStr, nLen,
							rtn, nOutLen);
		rtn[--nOutLen] = 0;
	}
#endif
#ifndef UNDER_CE
	else
		throw xCoTaskAlloc;
#endif
	return rtn;
}
#endif



#ifdef _UNICODE
#define CHAR_FUDGE 1    // one TCHAR unused is good enough
#else
#define CHAR_FUDGE 2    // two BYTES unused for case of DBC last char
#endif


bool tstring::LoadString( UINT nID,  HINSTANCE hInst )
{
	tstring& ts = *this;

#ifndef NO_ATLMODULE
	if( !hInst )
		hInst = _Module.GetResourceInstance();
#endif

#ifdef __AFX_H__
	if( !hInst )
		hInst = AfxGetResourceHandle();
#endif

	// try fixed buffer first (to avoid wasting space in the heap)
	TCHAR szTemp[256];
	int nLen = ::LoadString( hInst, nID, szTemp, _countof(szTemp));
	if (_countof(szTemp) - nLen > CHAR_FUDGE)
	{
		ts = szTemp;
		return nLen > 0;
	}

	// try buffer size of 512, then larger size until entire string is retrieved
	int nSize = 256;
	do
	{
		nSize += 256;
		ts.reserve( nSize );
		nLen = ::LoadString( hInst, nID, const_cast<LPTSTR>(ts.data()), nSize);
	} while (nSize - nLen <= CHAR_FUDGE);

	ts.resize( nLen + 1, 0 );

	return nLen > 0;
}






bool tstring::FormatMessage( UINT nID, HINSTANCE hInst, ...)
{
	tstring& ts = *this;
	// get format string from string table
	ResourceString strFormat( nID, hInst);
	if( strFormat.empty() )
		return false;

	// format message into temporary buffer lpszTemp
	va_list argList;
	va_start(argList, hInst);
	LPTSTR lpszTemp;
	if (::FormatMessage(FORMAT_MESSAGE_FROM_STRING|FORMAT_MESSAGE_ALLOCATE_BUFFER,
		strFormat.data(), 0, 0, (LPTSTR)&lpszTemp, 0, &argList) == 0 ||
		lpszTemp == NULL)
	{
		va_end(argList);
		return false;
	}
		
	// assign lpszTemp into the resulting string and free lpszTemp
	ts = lpszTemp;
	LocalFree(lpszTemp);
	va_end(argList);
	return true;
}



bool tstring::FormatMessage( DWORD dwMessageId, DWORD dwLanguageId, ... )
{
	va_list argList;
	va_start(argList, dwLanguageId );
	bool rtn = PrivFormatMessage( FORMAT_MESSAGE_FROM_HMODULE, dwMessageId, dwLanguageId, &argList );
	va_end(argList);
	if( !rtn && dwLanguageId )  // try again with any language
	{
		va_start(argList, dwLanguageId );
		rtn = PrivFormatMessage( FORMAT_MESSAGE_FROM_HMODULE, dwMessageId, 0, &argList );
		va_end(argList);
	}
	return rtn;
}

bool tstring::FormatSystemMessage( DWORD dwMessageId, DWORD dwLanguageId )
{
	bool rtn = PrivFormatMessage( FORMAT_MESSAGE_FROM_SYSTEM, dwMessageId, dwLanguageId );
	if( !rtn && dwLanguageId )  // try again with any language
	{
		rtn = PrivFormatMessage( FORMAT_MESSAGE_FROM_SYSTEM, dwMessageId, 0 );
	}
	return rtn;
}



bool tstring::PrivFormatMessage( DWORD dwFlags, DWORD dwMessageId, DWORD dwLanguageId, ... )
{
	// format message into temporary buffer lpszTemp
	va_list argList;
	va_start(argList, dwLanguageId );
	LPTSTR lpszTemp = NULL;
	if(::FormatMessage( FORMAT_MESSAGE_ALLOCATE_BUFFER |
						dwFlags,
//						FORMAT_MESSAGE_FROM_HMODULE |
//						FORMAT_MESSAGE_FROM_SYSTEM,
						NULL,
						dwMessageId,
						dwLanguageId,
						(LPTSTR)&lpszTemp, 
						0, 
						&argList ) == 0 || lpszTemp == NULL )

	{
		va_end(argList);
		return false;
	}
		
	// assign lpszTemp into the resulting string and free lpszTemp
	*this = lpszTemp;
	LocalFree(lpszTemp);
	va_end(argList);
	return true;
}



BOOL tstring::GetComputerName()
{
#ifdef USE_REMOTE_COMPUTER_NAME
	_bstr_t bstrRemName = RemoteComputerName();
	if( bstrRemName.length() )
	{
		*this = static_cast<LPCTSTR>(bstrRemName);
		return TRUE;
	}
#endif
#ifdef GetComputerName
	TCHAR szNodeName[MAX_COMPUTERNAME_LENGTH + 2];
	DWORD size = MAX_COMPUTERNAME_LENGTH + 1;
	if( ::GetComputerName( szNodeName, &size ) )
	{
		*this = szNodeName;
		return TRUE;
	}
#endif
	return FALSE;
}


bool tstring::GetModuleFileName( HMODULE hMod )
{
	TCHAR sz[ _MAX_PATH + 1 ];
	sz[0] = _T('\0');
	DWORD size = ::GetModuleFileName( hMod, sz, _MAX_PATH );
#ifndef UNDER_CE
	MyGetLongPathName( sz, sz, _MAX_PATH );
	*this = sz;
#else
	// make sure we get the long file name !!!
	WIN32_FIND_DATA data;
	HANDLE h=FindFirstFile(sz,&data);
	if(h!=INVALID_HANDLE_VALUE)
	{
		TCHAR *p = _tcsrchr( sz, _T('\\') );
		if( p )
		{
			++p;
			*p = _T('\0');
			*this = sz;
			*this += data.cFileName;
		}
		else
			*this = sz;

		BOOL b=FindClose(h);
	}
	else
		*this = sz;
#endif
	return( size != 0 );
}



LONG tstring::RegQueryValue(  HKEY hKey,       // handle to key to query
						 LPCTSTR lpSubKey, LPCTSTR lpValueName )
{
	DWORD n = 0;
	erase();

	HKEY hSubKey;
	LONG rtn = ::RegOpenKeyEx( hKey, lpSubKey, 0, KEY_READ, &hSubKey  );
	if( rtn == ERROR_SUCCESS )
	{
		rtn = ::RegQueryValueEx(  hSubKey, lpValueName, NULL, NULL, NULL, &n );
		if( rtn == ERROR_SUCCESS )
		{
			resize( n/sizeof(TCHAR) );
			rtn = ::RegQueryValueEx(  hSubKey, lpValueName, NULL, NULL, (LPBYTE)data(), &n );
			resize( (n/sizeof(TCHAR)) - 1 );
		}
		::RegCloseKey( hSubKey );
	}
	return rtn;
}


LONG tstring::RegQueryValueEx(  HKEY hKey,       // handle to key to query
						 LPCTSTR lpValueName )
{
	DWORD n = 0;
	erase();
	LONG rtn = ::RegQueryValueEx(  hKey, lpValueName, NULL,NULL, NULL, &n );
	if( rtn == ERROR_SUCCESS )
	{
		resize( n/sizeof(TCHAR) );
		rtn = ::RegQueryValueEx(  hKey, lpValueName, NULL,NULL,(LPBYTE)data(), &n );
		resize( (n/sizeof(TCHAR)) - 1 );
	}
	return rtn;
}


bool tstring::GetWindowText( HWND hWnd )
{
	SetLastError(0);
	int n = GetWindowTextLength( hWnd );
	if( !n && GetLastError() )
		return false;

	resize( n + 1 );
	SetLastError(0);
	int nn = ::GetWindowText( hWnd, (LPTSTR)data(), n+1 );
	if( !nn && GetLastError() )
	{
		erase();
		return false;
	}
	resize(nn);
	
	return true;
}


void tstring::MakeUpper()
{
	_tcsupr( (TCHAR *)data() );
}


void tstring::MakeLower()
{
	_tcslwr( (TCHAR *)data() );
}


void tstring::MakeReverse()
{
	_tcsrev( (TCHAR *)data() );
}


void ResourceStringVector::Fill()
{
	for( DWORD i = 0; ; i++ )
	{
		ResourceString s( m_nBaseOffset + i );
		if( s.empty() )
			break;
		push_back( s );
	}
}



const tstring& ResourceStringVector::operator[](size_type pos )
{
	if( empty() )
		Fill();
	return vector<tstring>::operator[](pos);
}

ResourceStringVector::size_type ResourceStringVector::size()
{
	if( empty() )
		Fill();
	return vector<tstring>::size();	
}


#ifndef __AFX_H__  // don't define in an MFC project
// MFC CString workalike


void CString::TrimRight()
{
	// find beginning of trailing spaces by starting at beginning (DBCS aware)
	LPCTSTR lpsz = data();
	LPCTSTR lpszLast = NULL;
	while (*lpsz != '\0')
	{
		if (_istspace(*lpsz))
		{
			if (lpszLast == NULL)
				lpszLast = lpsz;
		}
		else
			lpszLast = NULL;
#ifdef UNDER_CE
		++lpsz;
#else
		lpsz = _tcsinc(lpsz);
#endif
	}

	if (lpszLast != NULL)
	{
		// truncate at trailing space start
		assign( substr( 0, lpszLast - data() ) );
	}
}

void CString::TrimLeft()
{

	// find first non-space character
	LPCTSTR lpsz = data();
	while (_istspace(*lpsz))
	{
#ifdef UNDER_CE
		++lpsz;
#else
		lpsz = _tcsinc(lpsz);
#endif
	}
	if( lpsz != data() )
	{
		assign( substr( lpsz - data() ));
	}

}


void CString::TrimLeft( LPCTSTR lpszTargets )
{
	// find first non-space character
	LPCTSTR lpsz = data();

	while( _tcschr(lpszTargets, *lpsz) )
	{
#ifdef UNDER_CE
		++lpsz;
#else
		lpsz = _tcsinc(lpsz);
#endif
	}
	if( lpsz != data() )
	{
		assign( substr( lpsz - data() ));
	}
}



void CString::TrimRight( LPCTSTR lpszTargets )
{
	// find beginning of trailing spaces by starting at beginning (DBCS aware)
	LPCTSTR lpsz = data();
	LPCTSTR lpszLast = NULL;
	while (*lpsz != '\0')
	{
		if( _tcschr(lpszTargets, *lpsz) )
		{
			if (lpszLast == NULL)
				lpszLast = lpsz;
		}
		else
			lpszLast = NULL;
#ifdef UNDER_CE
		++lpsz;
#else
		lpsz = _tcsinc(lpsz);
#endif
	}

	if (lpszLast != NULL)
	{
		// truncate at trailing space start
		assign( substr( 0, lpszLast - data() ) );
	}
}



void CString::TrimLeft( TCHAR c )
{
	// find first non-space character
	LPCTSTR lpsz = data();

	while( c == *lpsz )
	{
#ifdef UNDER_CE
		++lpsz;
#else
		lpsz = _tcsinc(lpsz);
#endif
	}
	if( lpsz != data() )
	{
		assign( substr( lpsz - data() ));
	}
}




void CString::TrimRight( TCHAR c )
{
	// find beginning of trailing spaces by starting at beginning (DBCS aware)
	LPCTSTR lpsz = data();
	LPCTSTR lpszLast = NULL;
	while (*lpsz != '\0')
	{
		if( c == *lpsz )
		{
			if (lpszLast == NULL)
				lpszLast = lpsz;
		}
		else
			lpszLast = NULL;
#ifdef UNDER_CE
		++lpsz;
#else
		lpsz = _tcsinc(lpsz);
#endif
	}

	if (lpszLast != NULL)
	{
		// truncate at trailing space start
		assign( substr( 0, lpszLast - data() ) );
	}
}



CString CString::SpanIncluding(LPCTSTR lpszCharSet) const
{
	CString rtn;
	size_type pos = find_first_of( lpszCharSet );
	if (pos != 0) //if the first character in the string is not in the set, return an empty string
		return rtn;
	pos = find_first_not_of( lpszCharSet );
	if( pos != npos )
		rtn.assign( substr( 0, pos ) );
	else
		rtn = *this;
	return rtn;
}


CString CString::SpanExcluding(LPCTSTR lpszCharSet) const
{
	CString rtn;
	size_type pos = find_first_of( lpszCharSet );
	if( pos != npos )
		rtn.assign( substr( 0, pos ) );
	else
		rtn = *this;
	return rtn;
}



CString AFXAPI operator+(const CString& string1, const CString& string2)
{
	CString s(string1);
	s += string2;
	return s;
}

CString AFXAPI operator+(const CString& string, LPCTSTR lpsz)
{
	_ASSERTE(lpsz == NULL);
	CString s(string);
	s += lpsz;
	return s;
}

CString AFXAPI operator+(LPCTSTR lpsz, const CString& string)
{
	_ASSERTE(lpsz == NULL );
	CString s( lpsz );
	s += string;
	return s;
}

CString AFXAPI operator+(const CString& string, TCHAR ch )
{
	CString s( string );
	s += ch;
	return s;
}

CString AFXAPI operator+(TCHAR ch, const CString& string)
{
	CString s;
	s = ch;
	s += string;
	return s;
}


void CString::Format(LPCTSTR lpszFormat, ...)
{
	TCHAR buf[ 4095 ];
	va_list argList;
	va_start(argList, lpszFormat );
	_vstprintf( buf, lpszFormat, argList );
	va_end(argList);
	*this = buf;
}

void CString::Format(UINT nFormatID, ...)
{
	TCHAR buf[ 4095 ];
	ResourceString tsFormat( nFormatID );
	va_list argList;
	va_start(argList, nFormatID );
	_vstprintf( buf, tsFormat.data(), argList );
	va_end(argList);
	*this = buf;
}

// OpcStrDup
WCHAR* OpcStrDup(LPCWSTR szValue)
{
    WCHAR* pCopy = NULL;

    if (szValue != NULL)
    {
        pCopy = (WCHAR*)CoTaskMemAlloc(sizeof(WCHAR)*(wcslen(szValue)+1));
        wcscpy(pCopy, szValue);
    }

    return pCopy;
}

// OpcFree
void OpcFree(void* pBlock)
{
    if (pBlock != NULL) 
	{
		CoTaskMemFree(pBlock);
	}
}

// OpcLocalizeText
LPWSTR OpcLocalizeText(LCID lcid, LPCWSTR szString)
{ 
	if (szString == NULL)
	{
		return NULL;
	}

	if (lcid == LOCALE_SYSTEM_DEFAULT || lcid == LOCALE_ENGLISH_US)
	{
		return ::OpcStrDup(szString);
	}

	WCHAR szBuffer[4096];
	swprintf(szBuffer, 4096, L"%s [%04X]", szString, lcid);
	return ::OpcStrDup(szBuffer);
}

// OpcDeLocalizeText
LPWSTR OpcDeLocalizeText(LCID lcid, LPCWSTR szString)
{ 
	if (szString == NULL)
	{
		return NULL;
	}

	if (lcid == LOCALE_SYSTEM_DEFAULT || lcid == LOCALE_ENGLISH_US)
	{
		return ::OpcStrDup(szString);
	}

	WCHAR szBuffer[4096];
	swprintf(szBuffer, 4096, L" [%04X]", lcid);

	for (int ii = wcslen(szString)-1; ii >= 0; ii--)
	{
		if (wcscmp(szString+ii, szBuffer) == 0)
		{
			wcsncpy(szBuffer, szString, ii);
			szBuffer[ii] = 0;
			return ::OpcStrDup(szBuffer);
		}
	}

	return ::OpcStrDup(szString);
}

// OpcLocalizeVARIANT
void OpcLocalizeVARIANT(LCID lcid, VARIANT* pvData)
{
	if (pvData != NULL && pvData->vt == VT_BSTR)
	{
		if (lcid != LOCALE_SYSTEM_DEFAULT && lcid != LOCALE_ENGLISH_US)
		{
			LPWSTR szName = OpcLocalizeText(lcid, pvData->bstrVal);
			SysFreeString(pvData->bstrVal);
			pvData->bstrVal = SysAllocString(szName);
			OpcFree(szName);
		}
	}
}

// OpcLocalizeVARIANTs
void OpcLocalizeVARIANTs(LCID lcid, DWORD dwCount, VARIANT* pvData)
{
	for (DWORD ii = 0; ii < dwCount; ii++)
	{
		OpcLocalizeVARIANT(lcid, &(pvData[ii]));
	}
}

// OpcLocalizeONEVENTSTRUCTs
void OpcLocalizeONEVENTSTRUCTs(LCID lcid, DWORD dwCount, ONEVENTSTRUCT* pvData)
{
	for (DWORD ii = 0; ii < dwCount; ii++)
	{
		LPWSTR szText = OpcLocalizeText(lcid, pvData[ii].szMessage);
		// free(pvData[ii].szMessage);
		pvData[ii].szMessage = _wcsdup(szText);
		OpcFree(szText);

		OpcLocalizeVARIANTs(lcid, pvData[ii].dwNumEventAttrs, pvData[ii].pEventAttributes);
	}
}
#endif 
