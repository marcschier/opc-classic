// ServerStatus.h
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
//   $Workfile: ServerStatus.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 5 $
//       $Date: 8/20/98 9:56a $
//    $Archive: /OPC/AlarmEvents/SampleServer/ServerStatus.h $
//
//      System: OPC Alarm & Events
//   Subsystem: 
//
//
// Description: C++ version of OPCEVENTSERVERSTATUS
//
// Functions:   
//
//
//
//
//
/*   $History: ServerStatus.h $
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 8/20/98    Time: 9:56a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * moved tstring.h out of pre-compiled header to work around compiler bug
 * on WIN 95.
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 8/19/98    Time: 12:01p
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
#include "FileTime.h"
#include "tstring.h"

class OpcEventServerStatus
{
private:
	OPCEVENTSERVERSTATUS	m_data;
	wstring					m_wsVendorInfo;
public:
	OpcEventServerStatus();
	OpcEventServerStatus( const OpcEventServerStatus& src );
	~OpcEventServerStatus();

	OpcEventServerStatus& operator=( const OpcEventServerStatus& src );

	void StartTime( FILETIME s ) { (FileTimeClass)m_data.ftStartTime = (FileTimeClass)s; }
	void CurrentTime( FILETIME s ) { (FileTimeClass)m_data.ftCurrentTime = (FileTimeClass)s; }
	void LastUpdateTime( FILETIME s ) { (FileTimeClass)m_data.ftLastUpdateTime = (FileTimeClass)s; }
	void ServerState( OPCEVENTSERVERSTATE dwState ) { m_data.dwServerState = dwState; }
	void MajorVersion( WORD w ) { m_data.wMajorVersion = w; }
	void MinorVersion( WORD w ) { m_data.wMinorVersion = w; }
	void BuildNumber( WORD w ) { m_data.wBuildNumber = w; }
	void VendorInfo( LPCWSTR sz );

	FileTimeClass StartTime() const { return (FileTimeClass)m_data.ftStartTime; }
	FileTimeClass CurrentTime() const { return (FileTimeClass)m_data.ftCurrentTime; }
	FileTimeClass LastUpdateTime() const { return (FileTimeClass)m_data.ftLastUpdateTime; }
	OPCEVENTSERVERSTATE ServerState() const { return m_data.dwServerState; }
	WORD MajorVersion() const { return m_data.wMajorVersion; }
	WORD MinorVersion() const { return m_data.wMinorVersion; }
	WORD BuildNumber() const { return m_data.wBuildNumber; }
	LPCWSTR VendorInfo() const { return m_data.szVendorInfo; }
	const OPCEVENTSERVERSTATUS& StatusStruct() const { return m_data; }
	OPCEVENTSERVERSTATUS *CoTaskDup() const;
};

