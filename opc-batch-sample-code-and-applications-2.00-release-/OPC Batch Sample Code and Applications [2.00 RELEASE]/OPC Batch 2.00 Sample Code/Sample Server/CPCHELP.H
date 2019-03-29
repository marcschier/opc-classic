// CPCHELP.h
//
// (c) Copyright 1998 The OPC Foundation
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
//
// This file contains ConnectionPoint helper classes
// (These are limited to a single connection point and callback)
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//  2.00  01/12/98 acc   
//        10/13/99 acc   add SetParent to CP and AddChild to CPC
//	                     and change constructor footprint to clear up
//                       some initialiation problems.
//


#ifndef CPCHELP_H
#define CPCHELP_H

class IXXXCP;	// forward declaration

#include "OLECTL.h"	// For ConnectionPoint Stuff

/////////////////////////////////////////////////////////////////////////////
// IXXXCPC
//   This class is an implementation of an XXX specific CPC Class.
//   This generic/reusable class supports any SINGLE IID.
//   (It could be modified fairly easily to support multiple IIDs)
//
/////////////////////////////////////////////////////////////////////////////
class IXXXCPC : public IConnectionPointContainer
{
  public:
    IXXXCPC( IUnknown *Parent, REFIID iid);
    ~IXXXCPC( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// Member Functions 
	STDMETHODIMP		EnumConnectionPoints( 
		IEnumConnectionPoints **ppEnum
		);
	STDMETHODIMP		FindConnectionPoint( 
		REFIID riid,
		IConnectionPoint **ppCP
		);

	STDMETHODIMP		AddChild( 
		 IXXXCP			*pCP
		);

  private:
	// Member Variables
	IUnknown	*m_Parent;
	GUID		 m_iid;		// The IID for this CPC (only 1 for now)
	IXXXCP		*m_pCP;		// The ConnectionPoint for this CPC (only 1)
};

/////////////////////////////////////////////////////////////////////////////
// IXXXCP
//   This class is an implementation of a generic CP Class.
//   It supports registering only a single callback
//
/////////////////////////////////////////////////////////////////////////////
class IXXXCP : public IConnectionPoint
{
  public:
    IXXXCP(REFIID iid);
    ~IXXXCP( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// Member Functions 
	STDMETHODIMP	GetConnectionInterface( 
		IID *pIID
		);
	STDMETHODIMP	GetConnectionPointContainer( 
		IConnectionPointContainer **ppCPC
		);
	STDMETHODIMP	Advise( 
		IUnknown *pUnkSink,
		DWORD *pdwCookie
		);
	STDMETHODIMP	Unadvise( 
		DWORD dwCookie
		);
	STDMETHODIMP	EnumConnections( 
		IEnumConnections **ppEnum		//out
		);

	STDMETHODIMP	SetParent( 
		IConnectionPointContainer *ParentContainer
		);

	// Note Callback is public so caller can easily see if anything is registered
	// (this would not work if we supported multiple connections
	// in that case caller should use EnumConnections)
	IUnknown        *m_Callback;		// Only 1 is supported for now

  private:
	// Member Variables
    ULONG			mRefCount;
	GUID			m_iid;		// The IID for this CP
	IConnectionPointContainer *m_pCPC;
};
#endif
