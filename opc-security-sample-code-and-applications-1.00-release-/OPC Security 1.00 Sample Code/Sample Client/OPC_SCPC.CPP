// opc_scpc.cpp - server connection point test code
//
// (c) Copyright 1998 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Specifications and may be used
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
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
   
//       04/08/98 acc   update per final spec
//       06/19/98 acc   fix typo in ShutdownRequest (LPCWSTR)
//

#include <stdio.h>
#include <conio.h>
#include "opccomn.h"
#include "opcda.h"
#include "OLECTL.h"

extern	IMalloc *pIMalloc;
int TrySvrCPC(IConnectionPointContainer * pCPC);

int sdr = 0;	// shutdown request

// Define an actual implementation of the IOPCShutdown interface
// based on the 'virtual' definition in the header file
//
class IXXXShutdown : public IOPCShutdown
{
public:
	IXXXShutdown( void );
	~IXXXShutdown( void );

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// Member Functions 
	STDMETHODIMP ShutdownRequest( LPCWSTR szReason);

private:
	DWORD mRefCount;
};



//---------------------------------------------------------
// TryConectionPoint
// Function to exercise the ConntionPoint logic for a server
// Return '1' if Shutdown Request
// 
int TrySvrCPC(IConnectionPointContainer * pCPC)
{
	IConnectionPoint *pCallbackCP = 0;
	DWORD MyCookie = 0L;
	IXXXShutdown *MyCallback;
	HRESULT r1;
	
	cputs("Hit Enter to Monitor for ShutdownRequest...\n");
	getch();

	// Create a callback instance
	//
	MyCallback = new IXXXShutdown;
	MyCallback -> AddRef();

	// Establish a connection
	//
	r1 = pCPC->FindConnectionPoint(IID_IOPCShutdown, &pCallbackCP);
	if (FAILED(r1))
	{
		printf("No ConnectionPoint for OPCShhutdown:(%lx)\n", r1);
		return 0;
	}

	r1 = pCallbackCP->Advise(MyCallback, &MyCookie);
	if (FAILED(r1))
	{
		printf("Advise Failed:(%lx)\n", r1);
		pCallbackCP->Release();
		return 0;
	}

	printf("Callback is Connected - monitoring for activity...\n");
	printf("press 'x' to disconnect\n");

	// Run test until terminated by user.
	// We must dispatch messages (in single apartment model)
	// for callbacks to work!
	//
	sdr = 0;
	while(sdr == 0)
	{
		if(kbhit())
		{
			char c;
			c = getch();
			if (c == 'x') 
				break;
		}else
		{
			MSG msg;
			while(PeekMessage(&msg,NULL,NULL,NULL,PM_REMOVE))
			{
				TranslateMessage(&msg);
				DispatchMessage(&msg);
			}
		}
	}


	// User has terminated the test
	//
	printf("Disconnecting...\n");
	r1 = pCallbackCP->Unadvise(MyCookie);
	if (FAILED(r1))
	{
		printf("UnAdvise Failed:(%lx)\n", r1);
	}

	// Release the interfaces
	//
	pCallbackCP->Release();

	// Free my shutdown object
	//
	MyCallback -> Release();

	return sdr;		// return 1 if shutdown was requested
}




///////////////////////////////////////
// The rest of this file contains the 
// Client specific impementation of
// his IOPCShutdown Methods
//
///////////////////////////////////////
//
///////////////////////////////////////
IXXXShutdown:: IXXXShutdown()
{
	mRefCount = 0;
}

///////////////////////////////////////
//
///////////////////////////////////////
IXXXShutdown:: ~IXXXShutdown()
{
}


///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP_(ULONG) IXXXShutdown:: AddRef( void)
{
	return ++mRefCount;
}



///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP_(ULONG) IXXXShutdown:: Release( void)
{
	ULONG currentCount = --mRefCount; 

	// If no references left for this object
	if ( currentCount == 0)
	{
		// Then delete this group.
		delete this;
	}
	return currentCount;
}


///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP IXXXShutdown:: QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	if ( ppInterface == NULL)
		return E_INVALIDARG;

	if ( iid == IID_IUnknown )
		*ppInterface = (IUnknown*) this;

	else if ( iid == IID_IOPCShutdown)
	{
		*ppInterface = (IUnknown*) this;
	}
	else
	{
		*ppInterface = NULL;
	}
	
	if ( *ppInterface == NULL)
		return E_NOINTERFACE;

	AddRef();
	return S_OK;
}


///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP IXXXShutdown:: ShutdownRequest(
	LPCWSTR szReason
)
{
	printf("Shutdown Request Called\n");
	if(szReason) printf("Reason is:%ls\n", szReason);


	sdr = 1;		// set shutdown request - will cause pgm to exit

	return S_OK;
}
