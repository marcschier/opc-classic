// exemain.cpp
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
//  This source file contains the EXE Mainline code for 
//  this sample OPC server.
//
//
// Modification Log:
// Vers    Date   By    Notes
// ----  -------- ---   -----
// 0.00  11/18/96 ACC
// 0.90  04/08/97 ACC   add async logic
//       09/11/97 ACC   move init/delete critical section to fix shutdown crash
// 2.00  02/06/98 acc   add ShutdownRequest logic
//       04/08/98 acc   add component catagories and self registration
// 2.02  05/28/99 dj    enhance with OPC security
// 2.03  08/03/00 dj    security spec release
//

#define INITGUID
#define WIN32_LEAN_AND_MEAN
#include "windows.h"
#include "OLE2.h"

#include "OPCXXX.h"
#include "OPCfact.h"
#include "OPCTHRD.h"

// %OPC_SEC_MODIFIED%
// include the definitions of the IIDs:
#include "opcComn_i.c"
#include "opcDA_i.c"
#include "opcSec_i.c"
// %END_OPC_SEC_MODIFIED%


#define TimerRes	1000		// This sample updates 1 per second max
// Globals 
//
DWORD			objectCount = 0;		// Number of servers created
DWORD			lockCount = 0;			// Standard COM Server lock count
HINSTANCE		serverInstance = 0;		// DLL instance (e.g. for LoadString)
FILETIME		serverStartTime;		// OPC specific server start time
IMalloc			*pIMalloc = 0;			// Common memory allocator
CRITICAL_SECTION	CritSec;			// For compatability with DLL version

HWND	serverWindow = NULL;
UINT	TimerID;
BOOL	Register( IUnknown *Fact, DWORD regCls);
BOOL	Unregister( void);
STDAPI	ServerCanUnloadNow( void);

extern void	OPCQuickReg(GUID clsid, TCHAR * path, TCHAR *progid,TCHAR *Type, TCHAR *desc);
extern void	OPCQuickUnReg(GUID clsid, TCHAR *progid, TCHAR *Type);
extern void RegOPCDACat(GUID clsid);
extern void UnRegOPCDACat(GUID clsid);
extern void CreateOPCDACat(void);

//
// The following code supports the EXE version 
//  of the OPC Sample server
//


// OPCWndProc()
//   This is the main windows procedure for the OPC Sample LocalServer.

LRESULT CALLBACK OPCWndProc( HWND hWnd, UINT iMsg, WPARAM wParam, LPARAM lParam)
{
	int i;

  switch ( iMsg) {
    case WM_CLOSE:
	  // Exit request
	  // Tell all OPC clients to exit 
	  // and wait up to 10 seconds for them to do so
	  //
	  ShutdownClients();
	  for( i = 0; i<100; i++)
	  {
		MSG msg;

		if(ServerCanUnloadNow() == S_OK) break;
		
		// We need this PeekMessage for the DCOM/RPC messages to work!
		//
		while(PeekMessage(&msg,NULL,NULL,NULL,PM_REMOVE))
		{
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
		Sleep(100);	// sleep 0.1 second x 100 times = 10 seconds

	  }
	  DestroyWindow(serverWindow);
	  break;

    case WM_DESTROY:
      KillTimer(serverWindow, TimerID);
      PostQuitMessage( 0);
      break;

	case WM_TIMER:
		UpdateServers(TimerRes);
		break;
    default:
      return DefWindowProc( hWnd, iMsg, wParam, lParam);
  }
  return 0;
}



// WinMain()
//   This is the entry point for the EXE.

int WINAPI WinMain( HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpszCmdLine, int nCmdShow)
{
  BOOL success = TRUE;
  MSG  msg;
  TCHAR szModulePath[MAX_PATH];

  CoInitialize( NULL);

  CoInitializeSecurity(
      NULL, 
      -1,	     // Use DCOM default authentication service
      NULL,		 // Use DCOM default authentication service 
      NULL,		 // reserved for future use - must be NULL
      RPC_C_AUTHN_LEVEL_CONNECT,        // user is authenticated
      RPC_C_IMP_LEVEL_IMPERSONATE,      // server would like to impersonate the client
      NULL,		// reserved for future use - must be NULL
      EOAC_NONE,	// first parameter is a security descriptor
      NULL 		// reserved for future use - must be NULL
      );

  

  serverInstance = hInstance;
 

  // Create the OPC Data Access Server Catagories
  // (of they do not already exist)
  //
  CreateOPCDACat();

  msg.wParam = 0;

  // Check for Register/Unregister
  //
  // If REGISTER requests
  if(
	  (lstrcmpiA(lpszCmdLine,
	  "-RegServer") == 0) ||
	  (lstrcmpiA(lpszCmdLine,
	  "/RegServer") == 0)
	  )
  {
	// Then Register the Server COM Object
	//
	// Obtain the path to this module's executable file
	//
	GetModuleFileName( 
		hInstance,
		szModulePath, 
		sizeof(szModulePath)/sizeof(TCHAR)); 
    // %OPC_SEC_MODIFIED%
	OPCQuickReg(CLSID_OPCSampleServer, szModulePath, TEXT("OPC.NT.1"), TEXT("LocalServer32"), TEXT("OPC Sample Server w/ NT Security"));
    // %END_OPC_SEC_MODIFIED%

	// And After registering the server
	// Add it to the component catagories.
	//
	RegOPCDACat(CLSID_OPCSampleServer);
	  
	CoUninitialize();
	return msg.wParam;
  }

  // Else if Unregister
  //
  if(
	  (lstrcmpiA(lpszCmdLine,
	  "-UnregServer") == 0) ||
	  (lstrcmpiA(lpszCmdLine,
	  "/UnregServer") == 0)
	  )
  {
	// *BEFORE* UnRegistering the server
	// Remove it from the component catagories.
	//
	UnRegOPCDACat(CLSID_OPCSampleServer);
	  
	// Then deregister the COM object
	//
    // %OPC_SEC_MODIFIED%
	OPCQuickUnReg(CLSID_OPCSampleServer, TEXT("OPC.NT.1"), TEXT("LocalServer32"));
    // %END_OPC_SEC_MODIFIED%

	CoUninitialize();
	return msg.wParam;
  }


  // Else we actually want to run...
  //
  InitializeCriticalSection(&CritSec);

  IClassFactory* serverFactory = new OPCClassFactory( CLSID_OPCSampleServer);
  if ( serverFactory != NULL && Register(serverFactory,REGCLS_MULTIPLEUSE)) 
	{
      if ( !hPrevInstance) 
	  {
        WNDCLASS wc;
        wc.style = CS_HREDRAW | CS_VREDRAW;
        wc.lpfnWndProc = OPCWndProc;
        wc.cbClsExtra = 0;
        wc.cbWndExtra = 0;
        wc.hInstance = serverInstance;
        wc.hIcon = 0;
        wc.hCursor = LoadCursor( NULL, IDC_ARROW);
        wc.hbrBackground = (HBRUSH)(COLOR_WINDOW+1);
        wc.lpszMenuName = 0;
        wc.lpszClassName = "OPC Sample";

        success = (RegisterClass( &wc) != 0);
      }

      if ( success && ( serverWindow = CreateWindow( "OPC Sample", "OPC Security Sample Server", WS_OVERLAPPEDWINDOW,
                                                     CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT, CW_USEDEFAULT,
                                                     NULL, NULL, serverInstance, NULL)) != NULL) 
	  {
        CoGetMalloc(MEMCTX_TASK, &pIMalloc);	// 'TASK' ok for DLL use
        ShowWindow( serverWindow, SW_MINIMIZE);
        UpdateWindow( serverWindow);

        CoFileTimeNow( &serverStartTime);

		// Start a psuedo-thread for Async Handling
		//
		InitServerSlots();
		TimerID = SetTimer(serverWindow, 0, TimerRes, 0);	// timer could be faster...

        while ( GetMessage( &msg, NULL, 0, 0)) 
		{
          TranslateMessage( &msg);
          DispatchMessage( &msg);
        }

        pIMalloc->Release();
      }
      Unregister();
  }
  else 
  {
      // Delete the factory if we did not successfully register it.  If it was registered, the factory
      // will be deleted by the Unregister() function.
      if ( serverFactory != NULL)
        delete serverFactory;
  }
  CoUninitialize();
  DeleteCriticalSection(&CritSec);
  
  return msg.wParam;
}




// OPCServerUnload()
//   This function is called when an object inside the server is destroyed.  If the server
//   can legally unload, this function posts a WM_QUIT message to the main window.  

void OPCServerUnload( void)
{
  if ( serverWindow != NULL && ServerCanUnloadNow() == S_OK)
    PostMessage( serverWindow, WM_CLOSE, 0, 0);
}




DWORD mRegisterID;

// Register()
//   This function is called by the EXE version of the server to register this class factory
//   with the OLE libraries.  It returns TRUE if it is successful.

BOOL Register( IUnknown *Fact, DWORD regCls)
{
    HRESULT hr =  CoRegisterClassObject( CLSID_OPCSampleServer, 
				Fact, CLSCTX_LOCAL_SERVER, regCls, &mRegisterID);
  return SUCCEEDED(hr);
}



// Unregister()
//   Unregisters this class factory with the OLE libraries.

BOOL Unregister( void)
{
  BOOL success = SUCCEEDED( CoRevokeClassObject( mRegisterID));
  mRegisterID = 0;
  return success;
}

///////////////////////////////////////
// ServerCanUnloadNow()
//  This is the standard function that is called 
//  to determine whether or not this Server is
//  in use.
//
///////////////////////////////////////
STDAPI ServerCanUnloadNow( void)
{
	SCODE sc;
	BOOL canUnload;

	// If we have no objects and we are not locked
	// then we can be unloaded (e.g. by VB).
	//
	canUnload = (( objectCount == 0) && (lockCount == 0));
	sc = canUnload ? S_OK : S_FALSE;
	return sc;
}



