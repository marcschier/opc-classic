// ResourceVer.h
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
//   $Workfile: ResourceVer.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 8 $
//       $Date: 7/02/99 6:29p $
//    $Archive: /OPC/AlarmEvents/SampleServer/CE/ResourceVer.h $
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
/*   $History: ResourceVer.h $
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 7/02/99    Time: 6:29p
 * Updated in $/OPC/AlarmEvents/SampleServer/CE
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
 * User: Jiml         Date: 8/19/98    Time: 12:02p
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

#ifndef UNDER_CE

class ResourceVersion
{
private:
	LPVOID m_pInfo;
	DWORD m_dwLangCodepage;
	bool m_bFromFile;

	void Init( LPCTSTR szPath );
public:
	ResourceVersion( HMODULE hMod = NULL );  // by default read from ourself
	ResourceVersion( LPCTSTR szPath );
	~ResourceVersion();

	void LangCodepage( DWORD dw ) { m_dwLangCodepage = dw; }
	DWORD LangCodepage() const { return m_dwLangCodepage; }

	const VS_FIXEDFILEINFO* FixedFileInfo() const;

	LPCTSTR StringFileInfo( LPCTSTR str ) const;	
	
	LPCTSTR CompanyName() const { return StringFileInfo( _T("CompanyName") ); }
	LPCTSTR FileDescription() const { return StringFileInfo( _T("FileDescription") ); }
	LPCTSTR FileVersion() const { return StringFileInfo( _T("FileVersion") ); }
	LPCTSTR InternalName() const { return StringFileInfo( _T("InternalName") ); }
	LPCTSTR LegalCopyright() const { return StringFileInfo( _T("LegalCopyright") ); }
	LPCTSTR OriginalFilename() const { return StringFileInfo( _T("OriginalFilename") ); }
	LPCTSTR ProductName() const { return StringFileInfo( _T("ProductName") ); }
	LPCTSTR ProductVersion() const { return StringFileInfo( _T("ProductVersion") ); }
	LPCTSTR Comments() const { return StringFileInfo( _T("Comments") ); }
	LPCTSTR LegalTrademarks() const { return StringFileInfo( _T("LegalTrademarks") ); }
	LPCTSTR PrivateBuild() const { return StringFileInfo( _T("PrivateBuild") ); }
	LPCTSTR SpecialBuild() const { return StringFileInfo( _T("SpecialBuild") ); }
};

#endif //UNDER_CE



