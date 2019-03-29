// OPCEventAreaBrowser.h 
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
//   $Workfile: OPCEventAreaBrowser.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 11 $
//       $Date: 8/02/02 4:47p $
//    $Archive: /OPC/AlarmEvents/SampleServer/OPCEventAreaBrowser.h $
//
//      System: OPC Alarm & Events
//   Subsystem: Sample Server
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
/*   $History: OPCEventAreaBrowser.h $
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 8/02/02    Time: 4:47p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 8/19/98    Time: 1:51p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 3/13/98    Time: 6:46p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 3/11/98    Time: 4:34p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 2/26/98    Time: 11:04a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 1/02/98    Time: 6:29p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 12/31/97   Time: 11:47a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 12/30/97   Time: 6:46p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:08a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 1  *****************
 * User: Jiml         Date: 12/15/97   Time: 11:11a
 * Created in $/OPC/AlarmEvents/SampleServer
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
// OPCEventAreaBrowser.h : Declaration of the COPCEventAreaBrowser

#ifndef __OPCEVENTAREABROWSER_H_
#define __OPCEVENTAREABROWSER_H_

#include "resource.h"       // main symbols

class AreaNode;

/////////////////////////////////////////////////////////////////////////////
// COPCEventAreaBrowser
class ATL_NO_VTABLE COPCEventAreaBrowser : 
	public CComObjectRootEx<CComMultiThreadModel>,
//	public CComCoClass<COPCEventAreaBrowser, &CLSID_OPCEventAreaBrowser>,
	public IOPCEventAreaBrowser
{
private:


	AreaNode	*m_pAreaNode;		// the current node -- starts at root

	// temporary member vars for recursive FillStringArray()
	DWORD			m_dwCount;
	LPCOLESTR *		m_pLPOLESTR;
	OPCAEBROWSETYPE	m_dwBrowseFilterType;
	LPCWSTR			m_szFilterCriteria;
	LCID            m_lcid;

	void FillStringArray( const AreaNode& rAreaNode );

public:

	static const WCHAR		seps[];

	typedef CComEnum<IEnumString, &IID_IEnumString, LPOLESTR,
		_Copy<LPOLESTR> > CComEnumString;

	void SetLocaleID(LCID lcid) { m_lcid = lcid; }

	COPCEventAreaBrowser();


// DECLARE_REGISTRY_RESOURCEID(IDR_OPCEVENTAREABROWSER)
DECLARE_NOT_AGGREGATABLE(COPCEventAreaBrowser)

BEGIN_COM_MAP(COPCEventAreaBrowser)
	COM_INTERFACE_ENTRY(IOPCEventAreaBrowser)
END_COM_MAP()

// IOPCEventAreaBrowser
public:
        

        virtual HRESULT STDMETHODCALLTYPE ChangeBrowsePosition( 
            /* [in] */ OPCAEBROWSEDIRECTION dwBrowseDirection,
            /* [string][in] */ LPCWSTR szString);
        
        virtual HRESULT STDMETHODCALLTYPE BrowseOPCAreas( 
            /* [in] */ OPCAEBROWSETYPE dwBrowseFilterType,
            /* [string][in] */ LPCWSTR szFilterCriteria,
            /* [out] */ LPENUMSTRING __RPC_FAR *ppIEnumString);
        
        virtual HRESULT STDMETHODCALLTYPE GetQualifiedAreaName( 
            /* [in] */ LPCWSTR szAreaName,
            /* [string][out] */ LPWSTR __RPC_FAR *pszQualifiedAreaName);
        
        virtual HRESULT STDMETHODCALLTYPE GetQualifiedSourceName( 
            /* [in] */ LPCWSTR szSourceName,
            /* [string][out] */ LPWSTR __RPC_FAR *pszQualifiedSourceName);

      


};

#endif //__OPCEVENTAREABROWSER_H_
