// OPCClassFactory.h
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
//   This is an implementation of a standard COM IClassFactory interface. 
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.00  11/18/96 ACC
//
//



// Items shared by the class factory and the DLL or EXE mainline
//
//extern DWORD		clientCount;
//extern HINSTANCE	serverInstance;
extern	DWORD		objectCount;
extern	DWORD		lockCount;

extern	void		OPCServerUnload(void);


/////////////////////////////////////////////////////////////////////////////
class OPCClassFactory : public IClassFactory
{
  public:
    OPCClassFactory( REFCLSID classID);

    BOOL Register( DWORD regCls = REGCLS_MULTIPLEUSE);
    BOOL Unregister( void);

    STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
    STDMETHODIMP_(ULONG) AddRef( void);
    STDMETHODIMP_(ULONG) Release( void);

    STDMETHODIMP         CreateInstance( LPUNKNOWN pUnkOuter, REFIID riid, LPVOID* ppvObject);
    STDMETHODIMP         LockServer( BOOL fLock);

  protected:
    ULONG mRefCount;
};

