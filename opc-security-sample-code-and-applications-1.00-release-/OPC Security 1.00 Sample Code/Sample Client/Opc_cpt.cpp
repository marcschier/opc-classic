// opc_cpt.cpp - connection point test code
//
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
// 2.00  04/03/97 acc   
//       01/14/98 acc   update per final spec
//       02/06/98 acc   fix to not free 'in' parameters - the server does that.
//       06/17/98 acc   renamed some asyncio2 methods;cancel2,setenable,getenable
//       10/14/98 acc   rename refresh to refresh2
//

#include <stdio.h>
#include <conio.h>
#include "opcda.h"
#include "OLECTL.h"

extern	IMalloc *pIMalloc;
void TryConnectionPoint(IOPCGroupStateMgt * pGRP, int nItem, OPCHANDLE *sh);
//void TryConnectionPoint(IOPCGroupStateMgt * pGRP);

static void flip_mode(OPCDATASOURCE *mode);
static void	DumpValue(int i, VARIANT *v, WORD q, OPCHANDLE h, HRESULT err);
extern void	DumpVariant(VARIANT *v);

static	DWORD tid = 0;

// Define an actual implementation of the IOPCCallback interface
// based on the 'virtual' definition in the header file
//
class IXXXCallback : public IOPCDataCallback
{
public:
	IXXXCallback( void );
	~IXXXCallback( void );

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// Member Functions 
	STDMETHODIMP OnDataChange(
	 DWORD       Transid, 
	 OPCHANDLE   grphandle, 
	 HRESULT     masterquality,
	 HRESULT     mastererror,
	 DWORD       count, 
	 OPCHANDLE * clienthandles, 
	 VARIANT   * values, 
	 WORD      * quality,
	 FILETIME  * time,
	 HRESULT   * errors
	);

	STDMETHODIMP OnReadComplete(
	 DWORD       Transid, 
	 OPCHANDLE   grphandle, 
	 HRESULT     masterquality,
	 HRESULT     mastererror,
	 DWORD       count, 
	 OPCHANDLE * clienthandles, 
	 VARIANT   * values, 
	 WORD      * quality,
	 FILETIME  * time,
	 HRESULT   * errors
	);

	STDMETHODIMP OnWriteComplete(
	 DWORD       Transid, 
	 OPCHANDLE   grphandle, 
	 HRESULT     mastererr, 
	 DWORD       count, 
	 OPCHANDLE * clienthandles, 
	 HRESULT   * error
	);

	STDMETHODIMP OnCancelComplete(
	 DWORD       transid, 
	 OPCHANDLE   grphandle
	);

private:
	DWORD mRefCount;
};



// Utility help function used below
void	test1help(void)
{
	printf("Press 'a' to set active, i to set inactive\n");
	printf("'r to Read, w to Write\n");
	printf("'f to Refresh, c to cancel\n");
	printf("'d' to disable callbacks, 'e' to enable callbacks\n");
	printf("'m' to change Cache/Device mode, 'x' to disconnect\n");
	printf("'?' to repeat this message\n");
}

//---------------------------------------------------------
// TryConectionPoint
// Function to exercise the ConntionPoint logic for a group
// 
//void TryConnectionPoint(IOPCGroupStateMgt * pGRP)
void TryConnectionPoint(IOPCGroupStateMgt * pGRP, int nItem, OPCHANDLE *sh)
{
	IConnectionPointContainer *pCPC = 0;
	IConnectionPoint *pCallbackCP = 0;
	OPCDATASOURCE mode = OPC_DS_CACHE;
	IOPCAsyncIO2 *pASIO = 0;
	DWORD MyCookie = 0L;
	IXXXCallback *MyCallback;
	HRESULT r1;
	BOOL Active = 1;
	BOOL InActive = 0;
	DWORD RequestedRate = 2123;
	DWORD Rate;
	DWORD cid;
	HRESULT *hr;
	
	cputs("Hit Enter to Try OnDataChange...\n");
	getch();

	// Create a callback instance
	//
	MyCallback = new IXXXCallback;
	MyCallback -> AddRef();

	// Establish a connection
	//
	r1 = pGRP->QueryInterface(IID_IConnectionPointContainer, (void**)&pCPC);
	if (FAILED(r1))
	{
		printf("No ConnectionPointContainer:(%lx)\n", r1);
		return;
	}

	r1 = pGRP->QueryInterface(IID_IOPCAsyncIO2, (void**)&pASIO);
	if (FAILED(r1))
	{
		printf("No AsyncIO2 interface:(%lx)\n", r1);
		pCPC->Release();
		return;
	}

	r1 = pCPC->FindConnectionPoint(IID_IOPCDataCallback, &pCallbackCP);
	if (FAILED(r1))
	{
		printf("No ConnectionPoint for OPCCallback:(%lx)\n", r1);
		pASIO->Release();
		pCPC->Release();
		return;
	}

	r1 = pCallbackCP->Advise(MyCallback, &MyCookie);
	if (FAILED(r1))
	{
		printf("Advise Failed:(%lx)\n", r1);
		pCallbackCP->Release();
		pASIO->Release();
		pCPC->Release();
		return;
	}

	printf("Callback is Connected - monitoring for activity...\n");
	printf("Mode = DEVICE\n");
	test1help();


	// Run test until terminated by user.
	// We must dispatch messages (in single apartment model)
	// for callbacks to work!
	//
	pGRP->SetState(&RequestedRate, &Rate, &Active,0,0,0,0);
	printf("Requested rate: %ld, Revised rate: %ld\n", RequestedRate, Rate);
	while(1)
	{
		if(kbhit())
		{
			char c;
			c = getch();
			if (c == 'x') 
				break;
			if (c == 'a') 
				pGRP->SetState(0, &Rate, &Active, 0,0,0,0);
			if (c == 'i') 
				pGRP->SetState(0, &Rate, &InActive, 0,0,0,0);

			if (c == 'd') 
				pASIO->SetEnable(0);
			if (c == 'e') 
				pASIO->SetEnable(1);

			if (c == 'f') 
			{
				r1= pASIO->Refresh2(mode, tid, &cid);
				if (FAILED(r1))	printf("Refresh Error:%lx\n", r1);
				else printf("Refresh started/TID=%ld, CID=%ld\n", tid, cid);
				tid++;
			}
			if (c == 'c') 
			{
				r1 = pASIO->Cancel2(cid);
				if(FAILED(r1)) printf("Cancel Error=%lx\n", r1);
			}
			if (c == 'm') 
				flip_mode(&mode);

			if(c == '?') 
				test1help();

			if (c == 'r') 
			{
				r1 = pASIO->Read(nItem, 
					sh, tid++, &cid, &hr);
				// check r1 and hr array and also free hr
				if(r1 != S_OK)
				{
					printf("Async ReadFailed:(%lx)\n", r1);
				}
				if(hr) pIMalloc->Free(hr);
			}

			if (c == 'w') 
			{
				VARIANT v[2];

				v[0].vt = VT_R4;
				v[0].fltVal = (float)12.34;
				v[1].vt = VT_R4;
				v[1].fltVal = (float)56.78;

				r1 = pASIO->Write(nItem, 
					sh, v, tid++, &cid, &hr);

				// check r1 and hr array and also free hr
				if(r1 != S_OK)
				{
					printf("Async WriteFailed:(%lx)\n", r1);
				}
				if(hr) pIMalloc->Free(hr);
			}
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
	pASIO->Release();
	pCallbackCP->Release();
	pCPC->Release();

	MyCallback -> Release();
}

static void flip_mode(OPCDATASOURCE *mode)
{
	switch(*mode)
	{
	case OPC_DS_CACHE:
		*mode = OPC_DS_DEVICE;
		printf("Mode changed to DEVICE\n");
		break;
	case OPC_DS_DEVICE:
		*mode = OPC_DS_CACHE;
		printf("Mode changed to CACHE\n");
		break;
	}
}



///////////////////////////////////////
// The rest of this file contains the 
// Client specific impementation of
// his IOPCCallback Methods
//
///////////////////////////////////////
//
///////////////////////////////////////
IXXXCallback:: IXXXCallback()
{
	mRefCount = 0;
}

///////////////////////////////////////
//
///////////////////////////////////////
IXXXCallback:: ~IXXXCallback()
{
}


///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP_(ULONG) IXXXCallback:: AddRef( void)
{
	return ++mRefCount;
}



///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP_(ULONG) IXXXCallback:: Release( void)
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
STDMETHODIMP IXXXCallback:: QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	if ( ppInterface == NULL)
		return E_INVALIDARG;

	if ( iid == IID_IUnknown )
		*ppInterface = (IUnknown*) this;

	else if ( iid == IID_IOPCDataCallback)
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
STDMETHODIMP IXXXCallback:: OnDataChange(
	 DWORD       Transid, 
	 OPCHANDLE   grphandle, 
	 HRESULT     masterquality,
	 HRESULT     mastererror,
	 DWORD       count, 
	 OPCHANDLE * clienthandles, 
	 VARIANT   * value, 
	 WORD      * quality,
	 FILETIME  * time,
	 HRESULT   * errors
)
{
	unsigned int i;

	printf("OnDataChange: Transid=%ld count=%ld GrpHandle=%ld\n", Transid, count, grphandle);
	for(i=0; i<count; i++)
	{
		DumpValue(i, &value[i], quality[i], clienthandles[i], errors[i]);
	}

	return S_OK;
}

///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP IXXXCallback:: OnReadComplete(
	 DWORD       Transid, 
	 OPCHANDLE   grphandle, 
	 HRESULT     masterquality,
	 HRESULT     mastererror,
	 DWORD       count, 
	 OPCHANDLE * clienthandles, 
	 VARIANT   * value, 
	 WORD      * quality,
	 FILETIME  * time,
	 HRESULT   * errors
)
{
	unsigned int i;

	printf("OnReadComplete: Transid=%ld count=%ld GrpHandle=%ld\n", Transid, count, grphandle);
	for(i=0; i<count; i++)
	{
		DumpValue(i, &value[i], quality[i], clienthandles[i], errors[i]);
	}

	return S_OK;
}

///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP IXXXCallback:: OnWriteComplete(
	 DWORD       Transid, 
	 OPCHANDLE   grphandle, 
	 HRESULT     mastererr, 
	 DWORD       count, 
	 OPCHANDLE * clienthandles, 
	 HRESULT   * error
)
{
	DWORD i;

	printf("OnWriteComplete: Transid=%ld count=%ld GrpHandle=%ld\n", Transid, count, grphandle);
	for(i=0; i<count; i++)
	{
		printf("  [%d]-handle=%d, HR=%x\n", i, clienthandles[i], error[i]);
	}
	
	return S_OK;
}

///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP IXXXCallback:: OnCancelComplete(
	 DWORD       transid, 
	 OPCHANDLE   grphandle
)
{
	printf("OnCancelComplete: Transid=%ld GrpHandle=%ld\n", transid, grphandle);

	return S_OK;
}

//---------------------------------------------------------
// DumpValue
// This shows the value of a Variant
static void	DumpValue(int i, VARIANT *v, WORD q, OPCHANDLE h, HRESULT err)
{
	printf("  [%d]-handle=%d, Qual=%2x, Err=%8lx", i, h, q, err);
	DumpVariant(v);
	printf("\n");
}
