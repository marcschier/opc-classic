// ENUMHELP.h
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
// This file contains generic Enumerator classes
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.00  11/18/96 ACC
//
//


#ifndef ENUMHELP_H
#define ENUMHELP_H

/////////////////////////////////////////////////////////////////////////////
// XXX specific implementation of an IEnumUnknown Interface for the Server
//
class IXXXEnumUnknown : public IEnumUnknown
{
  public:
	IXXXEnumUnknown(LPUNKNOWN, ULONG, LPUNKNOWN*, IMalloc*);
    ~IXXXEnumUnknown( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IEnumUnknown Functions
	STDMETHODIMP         Next (
		ULONG celt,
		IUnknown **rgelt,
		ULONG *pceltFetched
		);
        
	STDMETHODIMP         Skip (
		ULONG celt
		);
        
	STDMETHODIMP         Reset(
		void
		);
        
	STDMETHODIMP         Clone( 
		IEnumUnknown **ppenum
		);

	// Member Variables
  private:
	ULONG           m_cRef;     //Object reference count
	LPUNKNOWN       m_pUnkRef;  //IUnknown for ref counting
	ULONG           m_iCur;     //Current element
	ULONG           m_cUnk;     //Number of unknowns in us
	LPUNKNOWN       *m_prgUnk;   //Source of unknowns
	IMalloc			*m_pmem;	// memory allocator to use
};


/////////////////////////////////////////////////////////////////////////////
// XXX specific implementation of an IEnumString Interface for the Server
//
class IXXXEnumString : public IEnumString
{
  public:
	IXXXEnumString(LPUNKNOWN, ULONG, LPOLESTR*, IMalloc*);
    ~IXXXEnumString( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IEnumString Functions
	STDMETHODIMP         Next (
		ULONG celt,
		LPOLESTR *rgelt,
		ULONG *pceltFetched
		);
        
	STDMETHODIMP         Skip (
		ULONG celt
		);
        
	STDMETHODIMP         Reset(
		void
		);
        
	STDMETHODIMP         Clone( 
		IEnumString **ppenum
		);

	// Member Variables
  private:
	ULONG           m_cRef;     //Object reference count
	LPUNKNOWN       m_pUnkRef;  //IUnknown for ref counting
	ULONG           m_iCur;     //Current element
	ULONG           m_cstr;     //Number of strings in us
	LPOLESTR        *m_prgstr;   //copy of the strings
	IMalloc			*m_pmem;	// memory allocator to use
};


#endif
