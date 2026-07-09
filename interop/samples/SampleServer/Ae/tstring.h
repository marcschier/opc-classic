// tstring.h
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
//   $Workfile: tstring.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 41 $
//       $Date: 9/21/01 5:17p $
//    $Archive: /AWX32/AWXLog32/tstring.h $
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
/*   $History: tstring.h $
 * 
 * *****************  Version 41  *****************
 * User: Jiml         Date: 9/21/01    Time: 5:17p
 * Updated in $/AWX32/AWXLog32
 * Added code to create ODBC DSN on the fly.
 * 
 * *****************  Version 40  *****************
 * User: Jiml         Date: 5/22/00    Time: 5:47p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 39  *****************
 * User: Jiml         Date: 4/13/00    Time: 2:54p
 * Updated in $/AWX32/AlarmOle
 * 
 * *****************  Version 38  *****************
 * User: Jiml         Date: 12/13/99   Time: 6:38p
 * Updated in $/GenTray
 * 
 * *****************  Version 37  *****************
 * User: Jiml         Date: 7/02/99    Time: 6:29p
 * Updated in $/OPC/AlarmEvents/SampleServer/CE
 * 
 * *****************  Version 36  *****************
 * User: Jiml         Date: 6/23/99    Time: 5:18p
 * Updated in $/Expressions/IcoExpressionEngine
 * 
 * *****************  Version 35  *****************
 * User: Jiml         Date: 6/18/99    Time: 2:53p
 * Updated in $/AWX32/AWXTray
 * 
 * *****************  Version 34  *****************
 * User: Jiml         Date: 5/19/99    Time: 6:32p
 * Updated in $/AWX32/AlarmOle
 * Fixed bug in CString::AllocSysString
 * 
 * *****************  Version 33  *****************
 * User: Jiml         Date: 5/06/99    Time: 5:39p
 * Updated in $/Expressions/IcoExpressionEngine
 * 
 * *****************  Version 32  *****************
 * User: Jiml         Date: 5/06/99    Time: 5:15p
 * Updated in $/Expressions/IcoExpressionEngine
 * IcoExpressionEngine DLL ported to CE
 * 
 * *****************  Version 31  *****************
 * User: Jiml         Date: 5/06/99    Time: 12:03p
 * Updated in $/AWX32/AlarmOle
 * First CE Version
 * 
 * *****************  Version 30  *****************
 * User: Jiml         Date: 4/14/99    Time: 2:37p
 * Updated in $/GenRegistrar
 * VC 6.0 compatible
 * 
 * *****************  Version 29  *****************
 * User: Jiml         Date: 4/07/99    Time: 6:30p
 * Updated in $/Expressions/IcoExpressionEngine
 * 
 * *****************  Version 28  *****************
 * User: Alaa         Date: 1/07/99    Time: 1:06p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 27  *****************
 * User: Alaa         Date: 12/10/98   Time: 7:05p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 26  *****************
 * User: Jiml         Date: 11/30/98   Time: 5:45p
 * Updated in $/GenRegistrar
 * 
 * *****************  Version 25  *****************
 * User: Jiml         Date: 11/16/98   Time: 6:41p
 * Updated in $/AWX32/AWXLog32/AWXLogCfg
 * 
 * *****************  Version 24  *****************
 * User: Jiml         Date: 11/12/98   Time: 3:08p
 * Updated in $/AWX32/AWXLog32
 * 
 * *****************  Version 23  *****************
 * User: Jiml         Date: 11/09/98   Time: 4:34p
 * Updated in $/AWX32/AWXLog32
 * 
 * *****************  Version 22  *****************
 * User: Jiml         Date: 11/09/98   Time: 11:21a
 * Updated in $/AWX32/AWXLog32
 * Added safety for assign/append NULL pointer
 * 
 * *****************  Version 21  *****************
 * User: Jiml         Date: 10/29/98   Time: 4:41p
 * Updated in $/AWX32/server
 * Added expressions
 * 
 * *****************  Version 20  *****************
 * User: Jiml         Date: 10/13/98   Time: 3:39p
 * Updated in $/GenRegistrar
 * 
 * *****************  Version 19  *****************
 * User: Jiml         Date: 8/19/98    Time: 12:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 18  *****************
 * User: Jiml         Date: 8/18/98    Time: 10:28a
 * Updated in $/GenRegistrar
 * 
 * *****************  Version 17  *****************
 * User: Jiml         Date: 8/17/98    Time: 12:03p
 * Updated in $/OPC/AlarmEvents/SampleClient
 * 
 * *****************  Version 16  *****************
 * User: Jiml         Date: 8/17/98    Time: 11:56a
 * Updated in $/GenRegistrar
 * Added CString workalike
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 7/27/98    Time: 5:01p
 * Updated in $/GenRegistrar
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 5/15/98    Time: 6:48p
 * Updated in $/AWX32/AlarmOle
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 5/13/98    Time: 7:42p
 * Updated in $/AWX32/AlarmOle
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 4/22/98    Time: 6:55p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 4/17/98    Time: 3:40p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 3/31/98    Time: 6:33p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 3/31/98    Time: 2:15p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 3/31/98    Time: 12:43p
 * Updated in $/AWX32/server
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 3/30/98    Time: 6:02p
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
#pragma once

#define __TSTRING_H

#include <string>
#pragma warning( disable : 4786 )

#pragma warning( push )
#pragma warning( disable : 4804 )


#include <vector>
using namespace std;


// ignore case versions
wchar_t *wcsstri( const wchar_t *string, const wchar_t *strCharSet );
char *strstri( const char *string, const char *strCharSet );
#ifdef _UNICODE 
	#define _tcsstri wcsstri
#else
	#define _tcsstri strstri
#endif
 


// protect against error using NULL pointers
struct safe_char_traits : public char_traits<char>
{
    static size_t __cdecl length(const char *_U)
	{ return( _U ? char_traits<char>::length(_U) : 0 ); }
};


struct safe_wchar_traits : public char_traits<wchar_t>
{
    static size_t __cdecl length(const wchar_t *_U)
	{ return( _U ? char_traits<wchar_t>::length(_U) : 0 ); }
};


typedef basic_string<char, safe_char_traits >
// typedef basic_string<char, safe_char_traits, allocator<char> >
Ostring;

typedef basic_string<wchar_t, safe_wchar_traits > 
// typedef basic_string<wchar_t, safe_wchar_traits, allocator<wchar_t> > 
Owstring;


#ifdef _UNICODE 
typedef Owstring tstring_base;
#else
typedef Ostring tstring_base;
#endif


class tstring : public tstring_base
{
private:
	bool PrivFormatMessage( DWORD dwFlags, DWORD dwMessageId, DWORD dwLanguageId, ... ); 

public:
	tstring() : tstring_base() {};
	tstring( const tstring& src ) : tstring_base( (const tstring_base&)src ){}
	tstring( const tstring_base& src ) : tstring_base( src ){}
	tstring( LPCTSTR src ) : tstring_base( src ) {}
	tstring( LPCTSTR src, size_type n ) : tstring_base( src, n ) {}
	tstring( size_type n, TCHAR c ) : tstring_base(n, c ) {}

	bool LoadString( UINT uID, HINSTANCE hInst = NULL );
	BOOL GetComputerName();
	bool FormatMessage( UINT nID, HINSTANCE hInst, ...);
	bool FormatMessage( DWORD dwMessageId, DWORD dwLanguageId, ... ); 
	bool FormatSystemMessage( DWORD dwMessageId, DWORD dwLanguageId ); 
	int CompareNoCase( LPCTSTR sz ) const { return _tcsicmp( data(), sz ); }
	int CompareNoCase( const tstring& ts ) const
										{ return CompareNoCase( ts.data() ); }
	int CompareNoCase( const tstring_base& ts ) const
										{ return CompareNoCase( ts.data() ); }
	bool GetWindowText( HWND hWnd );
	bool GetDlgItemText( HWND hWndDlg, int nCtlID )
				{ return GetWindowText( GetDlgItem( hWndDlg, nCtlID ) ); }

	LONG RegQueryValue(  HKEY hKey,       // handle to key to query
						 LPCTSTR lpSubKey, LPCTSTR lpValueName = NULL);
	LONG RegQueryValueEx(  HKEY hKey,       // handle to key to query
						 LPCTSTR lpValueName );
	bool GetModuleFileName( HMODULE hMod = NULL );
	// upper/lower/reverse conversion
	void MakeUpper();
	void MakeLower();
	void MakeReverse();

};


extern LPWSTR CoTaskAlloc( LPCTSTR lpStr );


extern LPWSTR CoTaskAlloc( LPCWSTR lpStr );


// a drop in replacement for CoTaskMemAlloc
extern PVOID CoTaskAlloc( ULONG cb );



class ResourceString : public tstring
{
public:
	ResourceString( UINT uID, HINSTANCE hInst = NULL )
				{ LoadString( uID, hInst ); }
};


class ResourceStringVector : public vector<tstring>
{
private:
	DWORD	m_nBaseOffset;

	void Fill();
public:
	ResourceStringVector( DWORD BaseOffset ) :
	  m_nBaseOffset( BaseOffset ){};
	
	const tstring& operator[](size_type pos );
	size_type size();
	DWORD BaseOffset() const { return m_nBaseOffset; }
};

#ifndef __AFX_H__  // don't define in an MFC project
// MFC CString workalike

#ifndef AFXAPI
	#define AFXAPI __stdcall
#endif

class CString : public tstring
{
public:
// Constructors
	CString() : tstring() {}
	CString(const CString& stringSrc) { *this = stringSrc; }
//	CString(TCHAR ch, int nRepeat = 1);
	CString(LPCTSTR lpsz) { *this = lpsz; }
#ifndef _UNICODE
	CString(LPCWSTR lpsz) { *this = lpsz; }
#endif
//	CString(LPCTSTR lpch, int nLength);
//	CString(const unsigned char* psz);

// Attributes & Operations
	// as an array of characters
	int GetLength() const { return size(); }
	BOOL IsEmpty() const { return empty(); }
	void Empty() { erase(); }                       // free up the data

	TCHAR GetAt(int nIndex) const { return at(nIndex); }      // 0 based
	TCHAR operator[](int nIndex) const { return at(nIndex); } // same as GetAt
	void SetAt(int nIndex, TCHAR ch) { at(nIndex) = ch; }
	operator LPCTSTR() const { return data(); }           // as a C string

	// overloaded assignment
	const CString& operator=(const CString& stringSrc) { assign( stringSrc ); return *this; }
	const CString& operator=(TCHAR ch) { assign(1,ch); return *this; }
	const CString& operator=(const tstring& stringSrc) { assign( stringSrc ); return *this; }
#ifdef _UNICODE
//	const CString& operator=(char ch);
#endif
	const CString& operator=(LPCTSTR lpsz) { assign( lpsz ); return *this; }
#ifndef _UNICODE
	const CString& operator=(LPCWSTR lpsz) { USES_CONVERSION; assign( W2T(lpsz) ); return *this; }
#endif
//	const CString& operator=(const unsigned char* psz);

	// string concatenation
	const CString& operator+=(const CString& string) { append( string ); return *this; }
	const CString& operator+=(TCHAR ch) { append( 1, ch ); return *this; }
#ifdef _UNICODE
//	const CString& operator+=(char ch);
#endif
	const CString& operator+=(LPCTSTR lpsz) { append( lpsz ); return *this; }

	friend CString AFXAPI operator+(const CString& string1,
		const CString& string2);
	friend CString AFXAPI operator+(const CString& string, TCHAR ch);
	friend CString AFXAPI operator+(TCHAR ch, const CString& string);
#ifdef _UNICODE
//	friend CString AFXAPI operator+(const CString& string, char ch);
//	friend CString AFXAPI operator+(char ch, const CString& string);
#endif
	friend CString AFXAPI operator+(const CString& string, LPCTSTR lpsz);
	friend CString AFXAPI operator+(LPCTSTR lpsz, const CString& string);
	// string comparison
	int Compare(LPCTSTR lpsz) const { return compare(lpsz); }         // straight character
//	int CompareNoCase(LPCTSTR lpsz) const;   // ignore case
//	int Collate(LPCTSTR lpsz) const;         // NLS aware

	// simple sub-string extraction
	CString Mid(int nFirst, int nCount) const { return (CString&)substr(nFirst,nCount); }
	CString Mid(int nFirst) const { return (CString&)substr( nFirst ); }
	CString Left(int nCount) const { return (CString&)substr( 0, nCount ); }
	CString Right(int nCount) const { return (CString&)substr( size() - nCount ); }

	CString SpanIncluding(LPCTSTR lpszCharSet) const;
	CString SpanExcluding(LPCTSTR lpszCharSet) const;

	// upper/lower/reverse conversion
//	void MakeUpper();
//	void MakeLower();
//	void MakeReverse();

	// trimming whitespace (either side)
	void TrimRight();
	void TrimLeft();
	void TrimLeft( TCHAR c );
	void TrimRight( TCHAR c );
	void TrimLeft( LPCTSTR lpszTargets );
	void TrimRight( LPCTSTR lpszTargets );

	// searching (return starting index, or -1 if not found)
	// look for a single character match
	int Find(TCHAR ch) const { return find( ch ); }              // like "C" strchr
	int Find(TCHAR ch, int nStart ) const { return find( ch, nStart ); }
	int ReverseFind(TCHAR ch) const { return rfind( ch ); }
	int FindOneOf(LPCTSTR lpszCharSet) const { return find_first_of( lpszCharSet ); }

	// look for a specific sub-string
	int Find(LPCTSTR lpszSub) const { return find( lpszSub );  }       // like "C" strstr
	int Find(LPCTSTR lpszSub, int nStart ) const { return find( lpszSub, nStart );  }       // like "C" strstr

	// simple formatting
	void Format(LPCTSTR lpszFormat, ...);
	void Format(UINT nFormatID, ...);

#ifndef _MAC
	// formatting for localization (uses FormatMessage API)
//	void AFX_CDECL FormatMessage(LPCTSTR lpszFormat, ...);
//	void AFX_CDECL FormatMessage(UINT nFormatID, ...);
#endif

	// input and output
#ifdef _DEBUG
//	friend CDumpContext& AFXAPI operator<<(CDumpContext& dc,
//				const CString& string);
#endif
//	friend CArchive& AFXAPI operator<<(CArchive& ar, const CString& string);
//	friend CArchive& AFXAPI operator>>(CArchive& ar, CString& string);

	// Windows support
//	BOOL LoadString(UINT nID);          // load from string resource
										// 255 chars max
#ifndef _UNICODE
	// ANSI <-> OEM support (convert string in place)
//	void AnsiToOem();
//	void OemToAnsi();
#endif

#ifndef _AFX_NO_BSTR_SUPPORT
	// OLE BSTR support (use for OLE automation)
	BSTR AllocSysString() const { USES_CONVERSION; return SysAllocString( T2CW(data()) ); }
//	BSTR SetSysString(BSTR* pbstr) const;
#endif

	// Access to string implementation buffer as "C" character array
	LPTSTR GetBuffer(int nMinBufLength) { resize(size()+1); resize( size()-1); if( nMinBufLength > (int)size() ) resize( nMinBufLength ); return (LPTSTR)data(); }
	void ReleaseBuffer(int nNewLength = -1) { resize( nNewLength == -1 ? _tcslen(data()) : nNewLength ); }
	LPTSTR GetBufferSetLength(int nNewLength) { resize( nNewLength+1); resize(nNewLength); return (LPTSTR)data(); }
//	void FreeExtra();

	// Use LockBuffer/UnlockBuffer to turn refcounting off
//	LPTSTR LockBuffer();
//	void UnlockBuffer();

// Implementation
public:
//	int GetAllocLength() const;

};

// Compare helpers
inline bool AFXAPI operator==(const CString& s1, const CString& s2)
	{ return( (tstring&)s1 == (tstring&)s2 ); }
inline bool AFXAPI operator==(const CString& s1, LPCTSTR s2)
	{ return( (tstring&)s1 == s2 ); }
inline bool AFXAPI operator==(LPCTSTR s1, const CString& s2)
	{ return( s1 == (tstring&)s2 ); }
inline bool AFXAPI operator!=(const CString& s1, const CString& s2)
	{ return( (tstring&)s1 != (tstring&)s2 ); }
inline bool AFXAPI operator!=(const CString& s1, LPCTSTR s2)
	{ return( !((tstring&)s1 == s2 )); }
inline bool AFXAPI operator!=(LPCTSTR s1, const CString& s2)
	{ return( !(s1 == (tstring&)s2 )); }
inline bool AFXAPI operator<(const CString& s1, const CString& s2)
	{ return( (tstring&)s1 < (tstring&)s2 ); }
inline bool AFXAPI operator<(const CString& s1, LPCTSTR s2)
	{ return( (tstring&)s1 < s2 ); }
inline bool AFXAPI operator<(LPCTSTR s1, const CString& s2)
	{ return( s1 < (tstring&)s2 ); }
inline bool AFXAPI operator>(const CString& s1, const CString& s2)
	{ return( (tstring&)s1 > (tstring&)s2 ); }
inline bool AFXAPI operator>(const CString& s1, LPCTSTR s2)
	{ return(  s2 < (tstring&)s1 ); }
inline bool AFXAPI operator>(LPCTSTR s1, const CString& s2)
	{ return(  (tstring&)s2 < s1 ); }
inline bool AFXAPI operator<=(const CString& s1, const CString& s2)
	{ return( (tstring&)s1 <= (tstring&)s2 ); }
inline bool AFXAPI operator<=(const CString& s1, LPCTSTR s2)
	{ return(   !(s2 < (tstring&)s1) ); }
inline bool AFXAPI operator<=(LPCTSTR s1, const CString& s2)
	{ return( !((tstring&)s2 < s1)); }
inline bool AFXAPI operator>=(const CString& s1, const CString& s2)
	{ return( !((tstring&)s1 < (tstring&)s2) ); }
inline bool AFXAPI operator>=(const CString& s1, LPCTSTR s2)
	{ return( !((tstring&)s1 < s2) ); }
inline bool AFXAPI operator>=(LPCTSTR s1, const CString& s2)
	{ return( !(s1 < (tstring&)s2) ); }

#define LOCALE_ENGLISH_US      MAKELCID(MAKELANGID(LANG_ENGLISH,  SUBLANG_ENGLISH_US), SORT_DEFAULT)
#define LOCALE_ENGLISH_UK      MAKELCID(MAKELANGID(LANG_ENGLISH,  SUBLANG_ENGLISH_UK), SORT_DEFAULT)
#define LOCALE_ENGLISH_NEUTRAL MAKELCID(MAKELANGID(LANG_ENGLISH,  SUBLANG_NEUTRAL),    SORT_DEFAULT)
#define LOCALE_GERMAN_GERMANY  MAKELCID(MAKELANGID(LANG_GERMAN,   SUBLANG_DEFAULT),    SORT_DEFAULT)
#define LOCALE_JAPANESE_JAPAN  MAKELCID(MAKELANGID(LANG_JAPANESE, SUBLANG_DEFAULT),    SORT_DEFAULT)

LPWSTR OpcLocalizeText(LCID lcid, LPCWSTR szString);
LPWSTR OpcDeLocalizeText(LCID lcid, LPCWSTR szString);
void OpcLocalizeVARIANT(LCID lcid, VARIANT* pvData);
void OpcLocalizeVARIANTs(LCID lcid, DWORD dwCount, VARIANT* pvData);
void OpcLocalizeONEVENTSTRUCTs(LCID lcid, DWORD dwCount, ONEVENTSTRUCT* pvData);

#endif  // CString 

#pragma warning( pop )

