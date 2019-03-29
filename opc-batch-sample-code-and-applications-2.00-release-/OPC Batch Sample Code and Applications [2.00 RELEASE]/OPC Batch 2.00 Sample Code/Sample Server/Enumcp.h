// ENUMCP.h
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
// This file contains a generic EnumConnectionPoints class
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	X.00  03/17/97 ACC
//
//


#ifndef ENUMCP_H
#define ENUMCP_H

/////////////////////////////////////////////////////////////////////////////
// XXX specific implementation of an IConnectionPoints Interface for the Server
//
class IXXXEnumConnectionPoints : public IEnumConnectionPoints
{
  public:
	IXXXEnumConnectionPoints(LPUNKNOWN, ULONG, IConnectionPoint**);
    ~IXXXEnumConnectionPoints( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IConnectionPoints Functions
	STDMETHODIMP         Next (
		ULONG celt,
		IConnectionPoint **rgelt,
		ULONG *pceltFetched
		);
        
	STDMETHODIMP         Skip (
		ULONG celt
		);
        
	STDMETHODIMP         Reset(
		void
		);
        
	STDMETHODIMP         Clone( 
		IEnumConnectionPoints **ppenum
		);

	// Member Variables
  private:
	ULONG           m_cRef;     //Object reference count
	LPUNKNOWN       m_pUnkRef;  //IUnknown for ref counting
	ULONG           m_iCur;     //Current element
	ULONG           m_cUnk;     //Number of interfaces in list
	IConnectionPoint      **m_prgUnk;   //list of interfaces
};


#endif
