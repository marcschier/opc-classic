// I_CPC.cpp
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
//  This file contains the implementation of 
//  a generic IConnectionPointContainer tear-off interface 
//  This implementation supports a single interface IID.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	      03/17/97 ACC   proposed OPC 2.0 implementation
//       01/12/98 acc   made it generic and reusable
//       06/12/98 acc   cosmetic cleanup
//       01/22/99 acc   cosmetic cleanup - remove opcxxx.h include
//       10/13/99 acc	fix up initialization problem - add AddChild
//
//

#define WIN32_LEAN_AND_MEAN

#include "CPCHELP.h"
#include "ENUMCP.h"


/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXCPC()
//   Constructor for this Interface
//
///////////////////////////////////////
IXXXCPC::IXXXCPC( IUnknown *parent , REFIID iid)
{
	m_Parent = parent;
	m_iid = iid;
	m_pCP = 0;
}



///////////////////////////////////////
// ~IXXXCPC()
//   Destructor for this Interface
//
///////////////////////////////////////
IXXXCPC::~IXXXCPC( void)
{
	if(m_pCP) m_pCP->Release();
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
//

STDMETHODIMP_(ULONG) IXXXCPC::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXCPC::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXCPC::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}



/////////////////////////////////////////////////////////////////////////////
// IXXXCPC (IConnectionPointContainer) interface functions
//


///////////////////////////////////////
// IXXXCPC:: 
///////////////////////////////////////
STDMETHODIMP	IXXXCPC::EnumConnectionPoints( 
		IEnumConnectionPoints **ppEnum
		)
{
	IConnectionPoint *EnumList;

	// Create a list of ConnectionPoints for the generic enumerator
	// Note we do NOT addref them here, The enumerator will do it internally.
	// Note the enumerator is 'generic'.
	//
	EnumList = m_pCP;

	// Enuerator will also addref the Parent to insure it stays around
	//
	*ppEnum = new IXXXEnumConnectionPoints(m_Parent, 1, &EnumList);
	if(!*ppEnum) return E_OUTOFMEMORY;

	(*ppEnum)->AddRef();
	return S_OK;
}


///////////////////////////////////////
// IXXXCPC::
///////////////////////////////////////
STDMETHODIMP	IXXXCPC::FindConnectionPoint( 
		REFIID riid,
		IConnectionPoint **ppCP
		)
{
	if(!ppCP) return E_POINTER;

	*ppCP = NULL;

	if(riid == m_iid)
	{
		*ppCP = m_pCP;
	}
	else
	{
		return CONNECT_E_NOCONNECTION;
	}

	(*ppCP)->AddRef();		// caller must call Release
	return S_OK;
}

        

///////////////////////////////////////
// IXXXCPC::
///////////////////////////////////////
STDMETHODIMP	IXXXCPC::AddChild( 
		IXXXCP *pCP
		)
{
	// This simple example supports only one child
	// A more complex CPC could support multiple child CPs.
	//
	if (m_pCP) return E_FAIL;

	m_pCP = pCP;
	if(pCP) pCP->AddRef();
	return S_OK;
}
