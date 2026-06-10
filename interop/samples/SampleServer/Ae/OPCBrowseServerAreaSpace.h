//*************************************************************************
//
//   Copyright (c) 1997 OPC FOUNDATION AND ICONICS INC. 
//        (copyrighted as an unpublished work)
//
//-------------------------------------------------------------------------
//
//   $Workfile: OPCBrowseServerAreaSpace.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 5 $
//       $Date: 12/24/97 10:07a $
//    $Archive: /OPC/AlarmEvents/SampleServer/OPCBrowseServerAreaSpace.h $
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
/*   $History: OPCBrowseServerAreaSpace.h $
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:07a
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
// OPCEventAreaBrowser.h : Declaration of the COPCEventAreaBrowser

#ifndef __OPCEVENTAREABROWSER_H_
#define __OPCEVENTAREABROWSER_H_

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// COPCEventAreaBrowser
class ATL_NO_VTABLE COPCEventAreaBrowser : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COPCEventAreaBrowser, &CLSID_OPCEventAreaBrowser>,
	public IOPCEventAreaBrowser
{
public:
	COPCEventAreaBrowser()
	{
	}

// DECLARE_REGISTRY_RESOURCEID(IDR_OPCEVENTAREABROWSER)
DECLARE_NOT_AGGREGATABLE(COPCEventAreaBrowser)

BEGIN_COM_MAP(COPCEventAreaBrowser)
	COM_INTERFACE_ENTRY(IOPCEventAreaBrowser)
END_COM_MAP()

// IOPCEventAreaBrowser
public:
};

#endif //__OPCEVENTAREABROWSER_H_
