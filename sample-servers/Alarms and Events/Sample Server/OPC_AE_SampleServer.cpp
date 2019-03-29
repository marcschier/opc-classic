// OPC_AE_SampleServer.cpp
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
/*   $History: OPC_AE_SampleServer.cpp $
 * 
 * *****************  Version 20  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:55a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 19  *****************
 * User: Jiml         Date: 7/06/98    Time: 12:13p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 18  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 17  *****************
 * User: Jiml         Date: 4/15/98    Time: 6:05p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 16  *****************
 * User: Jiml         Date: 4/15/98    Time: 4:50p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 3/30/98    Time: 6:02p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 3/17/98    Time: 5:04p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 3/13/98    Time: 6:46p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 3/10/98    Time: 6:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 2/26/98    Time: 11:04a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 2/09/98    Time: 4:35p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 12/24/97   Time: 5:39p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:06a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 12/23/97   Time: 12:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 12/19/97   Time: 6:40p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 12/18/97   Time: 6:29p
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
// OPC_AE_SampleServer.cpp : Implementation of WinMain


#include "stdafx.h"

#include "opc_ae_i.c"
#include "opccomn_i.c"

#include "resource.h"
#include "initguid.h"
#include "OPCEventServer.h"
#include "OPCEventSubscriptionMgt.h"
#include "OPCEventAreaBrowser.h"
#include "simulation.h"
#include "global.h"
#include "cathelp.h"


const CLSID CLSID_OPCEventServer = {0x65168852,0x5783,0x11D1,{0x84,0xA0,0x00,0x60,0x8C,0xB8,0xA7,0xE9}};



// Init app here
void Init()
{
	Simulation::Init();
}


// shutdown app here
void Shutdown()
{
	Simulation::Shutdown();
}






LONG CExeModule::Unlock()
{
	LONG l = CComModule::Unlock();
	if (l == 0)
	{
#if _WIN32_WINNT >= 0x0400
		if (CoSuspendClassObjects() == S_OK)
			PostThreadMessage(dwThreadID, WM_QUIT, 0, 0);
#else
		PostThreadMessage(dwThreadID, WM_QUIT, 0, 0);
#endif
	}
	return l;
}

CExeModule _Module;

BEGIN_OBJECT_MAP(ObjectMap)
	OBJECT_ENTRY(CLSID_OPCEventServer, COPCEventServer)
//	OBJECT_ENTRY(CLSID_OPCEventSubscriptionMgt, COPCEventSubscriptionMgt)
//	OBJECT_ENTRY(CLSID_OPCBrowseServerAreaSpace, COPCBrowseServerAreaSpace)
//	OBJECT_ENTRY(CLSID_OPCBrowseConditions, COPCBrowseConditions)
END_OBJECT_MAP()


LPCTSTR FindOneOf(LPCTSTR p1, LPCTSTR p2)
{
	while (*p1 != NULL)
	{
		LPCTSTR p = p2;
		while (*p != NULL)
		{
			if (*p1 == *p++)
				return p1+1;
		}
		p1++;
	}
	return NULL;
}



void RegisterServer()
{
	// register component categories
	
	HRESULT hr;
	// IID_OPCEventServerCATID is the Category ID (a GUID) defined in opc_ae.idl.
	// OPC_EVENTSERVER_CAT_DESC is the category description defined in opcaedef.h
	// All servers should register the categogy this way 
	hr = CreateComponentCategory( IID_OPCEventServerCATID, 
											OPC_EVENTSERVER_CAT_DESC);
	
	// CLSID_OPCEventServer is the CLSID for this sample server.  Each server
	// will need to register its own unique CLSID here with the component manager.
	hr = RegisterCLSIDInCategory( CLSID_OPCEventServer, IID_OPCEventServerCATID );
}



void UnregisterServer()
{
	UnRegisterCLSIDInCategory( CLSID_OPCEventServer, IID_OPCEventServerCATID );
}


/////////////////////////////////////////////////////////////////////////////
//
extern "C" int WINAPI _tWinMain(HINSTANCE hInstance, 
	HINSTANCE /*hPrevInstance*/, LPTSTR lpCmdLine, int /*nShowCmd*/)
{
	g_hinst = hInstance;
	lpCmdLine = GetCommandLine(); //this line necessary for _ATL_MIN_CRT
//	HRESULT hRes = CoInitialize(NULL);
//  If you are running on NT 4.0 or higher you can use the following call
//	instead to make the EXE free threaded.
//  This means that calls come in on a random RPC thread
	HRESULT hRes = CoInitializeEx(NULL, COINIT_MULTITHREADED);
	_ASSERTE(SUCCEEDED(hRes));
	_Module.Init(ObjectMap, hInstance);
	_Module.dwThreadID = GetCurrentThreadId();
	TCHAR szTokens[] = _T("-/");




	int nRet = 0;
	BOOL bRun = TRUE;
	LPCTSTR lpszToken = FindOneOf(lpCmdLine, szTokens);
	while (lpszToken != NULL)
	{
		if (lstrcmpi(lpszToken, _T("UnregServer"))==0)
		{
//			_Module.UpdateRegistryFromResource(IDR_OPCEVENTSERVER, FALSE);
			nRet = _Module.UnregisterServer();
			bRun = FALSE;
			UnregisterServer();
			break;
		}
		if (lstrcmpi(lpszToken, _T("RegServer"))==0)
		{
//			_Module.UpdateRegistryFromResource(IDR_OPCEVENTSERVER, TRUE);
			nRet = _Module.RegisterServer(TRUE);
			bRun = FALSE;
			RegisterServer();
			break;
		}
		lpszToken = FindOneOf(lpszToken, szTokens);
	}

	if (bRun)
	{
		hRes = _Module.RegisterClassObjects(CLSCTX_LOCAL_SERVER, 
			REGCLS_MULTIPLEUSE);
		_ASSERTE(SUCCEEDED(hRes));

		Init();  // Initialize app

		MSG msg;
		while (GetMessage(&msg, 0, 0, 0))
			DispatchMessage(&msg);

		_Module.RevokeClassObjects();
		Shutdown();  // Shutdown app
	}

	CoUninitialize();
	return nRet;
}
