// dllmain.cpp
//
// (c) Copyright 1997 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Data Access Specification and may be used
//  as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by
//  Al Chisholm, Intellution Inc.
//
// CONTENTS:
//
//  This source file contains the DLL Mainline code for 
//  this sample in-process OPC server.
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.00  11/18/96 ACC
// 0.90  04/08/97 ACC   add async logic
// 2.00  04/09/98 acc   add component catagories and self registration
// 2.01  07/29/99 acc   add CoInitialize/Uninitialize to SvrThread
//
//

#define INITGUID
#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCfact.h"
#include "OPCTHRD.h"
#include "process.h"

// Globals (Note there is a copy per instance of the server)
//
DWORD			objectCount = 0;		// Number of servers created
DWORD			lockCount = 0;			// Standard COM Server lock count
HINSTANCE		serverInstance = 0;		// DLL instance (e.g. for LoadString)
FILETIME		serverStartTime;		// OPC specific server start time
IMalloc			*pIMalloc = 0;			// Common memory allocator

// Async/Background Thread Stuff
//
HANDLE hThread = 0;
BOOL KeepRunning = TRUE;
CRITICAL_SECTION	CritSec;		// In case we need it...

void	SvrThread(void FAR * param);

extern void	OPCQuickReg(GUID clsid, TCHAR * path, TCHAR *progid,TCHAR *Type, TCHAR *desc);
extern void	OPCQuickUnReg(GUID clsid, TCHAR *progid, TCHAR *Type);
extern void RegOPCDACat(GUID clsid);
extern void UnRegOPCDACat(GUID clsid);
extern void CreateOPCDACat(void);


//
// The following code supports the DLL version 
//  of the OPC Sample server
//


///////////////////////////////////////
// DllMain()
// Main entry point for the DLL.
///////////////////////////////////////
BOOL WINAPI DllMain( HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	if ( fdwReason == DLL_PROCESS_ATTACH) 
	{
		InitializeCriticalSection( &CritSec);
		serverInstance = hinstDLL;
		CoFileTimeNow( &serverStartTime);
		CoGetMalloc(MEMCTX_TASK, &pIMalloc);	// 'TASK' ok for DLL use

		// Init the server list for the background thread
		// and start the thread
		//
		InitServerSlots();
		_beginthread(SvrThread, 0, 0);
	
	}
	else if ( fdwReason == DLL_PROCESS_DETACH)
	{
		// Kill bgn thread
		//
		KeepRunning = FALSE;
		Sleep(2000);

        DeleteCriticalSection( &CritSec);

		pIMalloc->Release();
	}

	return TRUE;
}



///////////////////////////////////////
// DllGetClassObject()
//  This is the standard COM function for 
//  returning our class factory to the caller.
//
///////////////////////////////////////
STDAPI DllGetClassObject( REFCLSID rclsid, REFIID riid, LPVOID* ppv)
{
	if ( rclsid == CLSID_OPCBatchSampleServer) 
	{
		OPCClassFactory* pClassFactory = new OPCClassFactory( rclsid);
		if ( pClassFactory == NULL)
			return E_OUTOFMEMORY;

		HRESULT hr = pClassFactory->QueryInterface( riid, ppv);
		if ( FAILED( hr))
			delete pClassFactory;

		return hr;
	}
	return CLASS_E_CLASSNOTAVAILABLE;
}



///////////////////////////////////////
// DllCanUnloadNow()
//  This is the standard function that is called 
//  to determine whether or not this DLL is
//  in use.
//
///////////////////////////////////////
STDAPI DllCanUnloadNow( void)
{
	SCODE sc;
	BOOL canUnload;

	// If we have no objects and we are not locked
	// then we can be unloaded (e.g. by VB).
	//
	canUnload = ( objectCount == 0 && lockCount == 0);
	sc = canUnload ? S_OK : S_FALSE;
	return sc;
}

///////////////////////////////////////
// OPCServerUnload()
//  This function is called by the ClassFactory
//  whenever a server is destroyed in case it is the last one.
// This allows an EXE implementation to exit.
// In the case of a DLL there is nothing to do.
//
///////////////////////////////////////
void		OPCServerUnload(void)
{
}


///////////////////////////////////////
// SvrThread()
//
///////////////////////////////////////
void	SvrThread(void FAR * param)
{
	CoInitialize( NULL);
	while(KeepRunning)
	{
		// Periodically
		// Update all servers for the attached process
		// (Most will just have 1)
		//
		Sleep(250);	//This could be made faster

		UpdateServers(250);
	}
	CoUninitialize();
	_endthread();
}


STDAPI DllRegisterServer(void)
{

	HMODULE  hMod ;
	TCHAR szModulePath[MAX_PATH];

	CoInitialize( NULL);

	// Create the OPC Data Access Server Catagories
	// (of they do not already exist)
	//
	CreateOPCDACat();

	// Then Register the Server COM Object
	//
	// Obtain the path to this module's executable file
	//

	// Obtain the path to this module's file for later use.
	// This is a bit trickey for a DLL.
	//
	// BE SURE TO CHANGE THIS FOR YOUR SERVER
	//
	hMod = GetModuleHandle("OPC_BATCH_DLL.dll");	
	if (!GetModuleFileName( hMod, szModulePath, sizeof(szModulePath)/sizeof(TCHAR)))
		return E_UNEXPECTED;

	// BE SURE TO CHANGE THIS FOR YOUR SERVER
	//
	OPCQuickReg(CLSID_OPCBatchSampleServer, szModulePath, TEXT("OPC.BatchSample.1"), TEXT("InProcServer32"), TEXT("OPC Batch Sample Server"));

	// And After registering the server
	// Add it to the component catagories.
	//
	RegOPCDACat(CLSID_OPCBatchSampleServer);
	  
	CoUninitialize();
	return S_OK;
}
	
	
STDAPI DllUnregisterServer(void)
{
	CoInitialize( NULL);

	// *BEFORE* UnRegistering the server
	// Remove it from the component catagories.
	//
	UnRegOPCDACat(CLSID_OPCBatchSampleServer);
	  
	// Then deregister the COM object
	//
	// BE SURE TO CHANGE THIS FOR YOUR SERVER
	//
	OPCQuickUnReg(CLSID_OPCBatchSampleServer, TEXT("OPC.BatchSample.1"), TEXT("InProcServer32"));

	CoUninitialize();
	return S_OK;
}
