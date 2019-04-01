// Server.cpp : functions to access server
//



#include "stdafx.h"

#include "server.h"
#include "OPCBClient.h"


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// Server functions







/////////////////////////////////////////////////////////////////////////////
// CreateServer INPROC
// Create the requested OPC Server
IUnknown *CreateInprocOPCServer(LPCOLESTR pszProgID)
{
	CLSID clsid;
	IClassFactory *pCF;
	HRESULT r1, r2, r3;
	IUnknown * pOPC;

	// Get the CLSID from the Name
	r1 = CLSIDFromProgID(pszProgID, &clsid);
	if(FAILED(r1))
	{
		printf("CLSIDFromProgID failed for %ls\n", pszProgID);
		return NULL;
	}

	// Create an OPC Sample Server Class Factory
	//
	r2 = CoGetClassObject(clsid, CLSCTX_INPROC_SERVER , //try inproc first
		NULL, IID_IClassFactory, (void**)&pCF);

	if (FAILED(r2))
	{
		printf("CoGetClassObject- no InProc server for (%lx)\n", r2);
		return NULL;
	}

	// And use the class factory to create the OPC Server
	// Request an IUnknown Interface to it
	// and release the class factory which is no longer needed
	//
	r3 = pCF->CreateInstance(NULL, IID_IUnknown, (void**)&pOPC);

	pCF->Release();

	if (FAILED(r3))
	{
		printf("Error from CreateInstance (%lx)\n", r3);
		return NULL;
	}
	printf("Inproc Object (with IUnknown) Created for %ls\n", pszProgID);
	return pOPC;
}

/////////////////////////////////////////////////////////////////////////////
// CreateServer LOCAL
// Create the requested OPC Server
IUnknown *CreateLocalOPCServer(LPCOLESTR pszProgID)
{
	CLSID clsid;
	IClassFactory *pCF;
	HRESULT r1, r2, r3;
	IUnknown * pOPC;

	// Get the CLSID from the Name
	r1 = CLSIDFromProgID(pszProgID, &clsid);
	if(FAILED(r1))
	{
		printf("CLSIDFromProgID failed for %ls\n", pszProgID);
		return NULL;
	}

	// Create an OPC Sample Server Class Factory
	//
	r2 = CoGetClassObject(clsid, CLSCTX_LOCAL_SERVER , //try inproc first
		NULL, IID_IClassFactory, (void**)&pCF);

	if (FAILED(r2))
	{
		printf("CoGetClassObject- no Local server for (%lx)\n", r2);
		return NULL;
	}

	// And use the class factory to create the OPC Server
	// Request an IUnknown Interface to it
	// and release the class factory which is no longer needed
	//
	r3 = pCF->CreateInstance(NULL, IID_IUnknown, (void**)&pOPC);

	pCF->Release();

	if (FAILED(r3))
	{
		printf("Error from CreateInstance (%lx)\n", r3);
		return NULL;
	}
	printf("Local Object (with IUnknown) Created for %ls\n", pszProgID);
	return pOPC;
}


//---------------------------------------------------------
// CreateServer REMOTE
// Create the requested OPC Server - DCOM enabled!
IUnknown*	CreateRemoteOPCServer(LPCOLESTR pszProgID, LPOLESTR pszNodeName)
{
	CLSID clsid;
	HRESULT r1, r2;
	MULTI_QI mqi;
	COSERVERINFO	sin, *sinptr;
	DWORD clsctx;

	// Get the CLSID from the Name
	// zzz NOTE this is a call to the LOCAL registry!
	//
	r1 = CLSIDFromProgID(pszProgID, &clsid);
	if(FAILED(r1))
	{
		printf("CLSIDFromProgID failed for %ls (%lx)\n", pszProgID, r1);
		return NULL;
	}

	// set up server info
	//
	if(*pszNodeName)
	{
		sinptr = &sin;
		sin.dwReserved1 = 0;
		sin.dwReserved2 = 0;
		sin.pwszName = pszNodeName;
		sin.pAuthInfo = 0;
		clsctx = CLSCTX_REMOTE_SERVER;
	} else
	{
		// If NODE is Nul then try local server
		sinptr = 0;		// pointer should be NULL if local
		clsctx = CLSCTX_LOCAL_SERVER;
	}

	// set up mqi
	//
	mqi.pIID = &IID_IUnknown;
	mqi.hr = 0;
	mqi.pItf = 0;

	// Note you must define _WIN32_DCOM in 'Settings'
	r2 = CoCreateInstanceEx(clsid, NULL, 
		clsctx, sinptr, 1, &mqi);

	if (FAILED(r2) || FAILED(mqi.hr))
	{
		printf("CoCreateInstanceEx - failed for node:%ls ProgID:%ls (%lx)\n", pszNodeName, pszProgID, r2);
		return NULL;
	}

	printf("Remote Object (with IUnknown) Created for %ls\n", pszProgID);
	return (IUnknown*)mqi.pItf;
}
