// OpcFact.cpp
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
//   This file contains the implementation of the OPCClassFactory 
//   for the OPC sample server.
//
// Note: the combination of 'ServerDestroyed' and 'OPCServerUnload'
//   Allow this class factory to work with both EXE and DLL versions.
//   There are many ways to provide this functionality and all of them
//	 are a bit convoluted. This is modeled on BrockSchmidt 2nd edition.
//	 See Chap 5, 'The Class Factory and the Unloading Mechanism'
//	 See also DLLMAIN.CPP and EXEMAIN.CPP
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.00  11/18/96 ACC
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCFact.h"

void ServerDestroyed(void);




///////////////////////////////////////
// OPCClassFactory()
//   Constructor for the OPC class factory.
///////////////////////////////////////
OPCClassFactory::OPCClassFactory(REFCLSID rid)
{
	mRefCount = 0;
}




///////////////////////////////////////
// QueryInterface()
//   Implementation of the standard IUnknown QueryInterface()
//   for the OPC class factory.
///////////////////////////////////////
STDMETHODIMP OPCClassFactory::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	if ( ppInterface == NULL)
		return E_INVALIDARG;

	if ( iid == IID_IUnknown || iid == IID_IClassFactory) 
	{
		*ppInterface = this;
		AddRef();
		return S_OK;
	}

	*ppInterface = NULL;
	return E_NOINTERFACE;
}



///////////////////////////////////////
// AddRef()
//   Implementation of the standard IUnknown AddRef() 
//   for the OPC class factory.
///////////////////////////////////////
STDMETHODIMP_(ULONG) OPCClassFactory::AddRef( void)
{
	return ++mRefCount;
}



///////////////////////////////////////
// Release()
//   Implementation of the standard IUnknown Release() 
//   for the OPC class factory.
///////////////////////////////////////
STDMETHODIMP_(ULONG) OPCClassFactory::Release( void)
{
	ULONG currentCount = --mRefCount;
	if ( currentCount == 0)
		delete this;
	return currentCount;
}



///////////////////////////////////////
// CreateInstance()
//   This function creates an instance of the OPC sample server.
///////////////////////////////////////
STDMETHODIMP OPCClassFactory::CreateInstance( LPUNKNOWN pUnkOuter, REFIID riid, LPVOID* ppvObject)
{
	if ( pUnkOuter != NULL)
		return CLASS_E_NOAGGREGATION;	// Aggregation is not supported by this code

	// Create a new server instance
	// We keep a count of the number created by the calling ap.
	// If that number goes to 0  (when the last interface 
	//  is released and the last server is destroyed)
	//  then the DLL or EXE can be unloaded.
	//
	XXXServer* pServer = new XXXServer(pUnkOuter, ServerDestroyed);
	if ( pServer == NULL) 
	{
		OPCServerUnload(); // used only for EXE version...
		return E_OUTOFMEMORY;
	}

	HRESULT hr = pServer->QueryInterface( riid, ppvObject);
	if ( FAILED( hr))
	{
		delete pServer;
		OPCServerUnload(); // used only for EXE version...
	}
	else
	{
		objectCount++;
	}

	return hr;
}


///////////////////////////////////////
// Called by server objects when they are destroyed
//  so that we can tell if there are any left 
//  for the DLL or EXE to manage.
//  See DLLCanUnloadNow for DLLs or OPCServerUnload for EXEs
//
///////////////////////////////////////
void ServerDestroyed(void)
{
		objectCount--;
		OPCServerUnload(); // used only for EXE version...
}

///////////////////////////////////////
// LockServer()
//   This is a standard implementation of the IClassFactory::LockServer() function that either
//   adds or removes a lock on the server.
//
///////////////////////////////////////
STDMETHODIMP OPCClassFactory::LockServer( BOOL fLock)
{
	if ( fLock)
		lockCount++;
	else
		lockCount--;

	return S_OK;
}
