// i_do.cpp
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
//  the IDataObject interface for groups in the XXX server.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.00  11/18/96 ACC
// 0.90  04/08/97 ACC   add async logic
//       05/20/97 ACC   fix comments in UnAdvise
//       06/12/98 acc   cosmetic cleanup
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"


/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXDO()
//   Constructor for this Interface
//
///////////////////////////////////////
IXXXDO::IXXXDO( LPUNKNOWN parent )
{
	m_Parent = (XXXGroup *)parent;
	m_dataCallback = 0;
	m_datatimeCallback = 0;
	m_writeCallback = 0;

	m_data = RegisterClipboardFormat("OPCSTMFORMATDATA");
	m_datatime = RegisterClipboardFormat("OPCSTMFORMATDATATIME");
	m_write = RegisterClipboardFormat("OPCSTMFORMATWRITECOMPLETE");
}



///////////////////////////////////////
// ~IXXXDO()
//   Destructor for this Interface
//
///////////////////////////////////////
IXXXDO::~IXXXDO( void)
{
	m_Parent->m_pIXXXDO = 0;

	// A well behaved client should already have released these:
	//
	if(m_dataCallback) m_dataCallback->Release();
	if(m_datatimeCallback) m_datatimeCallback->Release();
	if(m_writeCallback) m_writeCallback->Release();
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
//

STDMETHODIMP_(ULONG) IXXXDO::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXDO::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXDO::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}



/////////////////////////////////////////////////////////////////////////////
// IXXXDO (IDataObject) interface functions
//


///////////////////////////////////////
// IDataObject::DAdvise
///////////////////////////////////////
        
STDMETHODIMP IXXXDO::DAdvise( 
	FORMATETC *pfe,
	DWORD advf,
	IAdviseSink *pAdvSink,
	DWORD *pdwConnection) 
{
	HRESULT hr;

	// Basic Sanity Checks
	//
	if(!pdwConnection) return E_POINTER;
	if(!pAdvSink) return E_POINTER;
	if(!pfe) return E_POINTER;

	// Figure out what type of connection he wants
	//
	if(pfe->cfFormat == m_data)
	{
		// if already in use, return error
		// Else connect it up
		//
		if(m_dataCallback) return CONNECT_E_ADVISELIMIT;
		hr = pAdvSink->QueryInterface( IID_IAdviseSink, (LPVOID*) &m_dataCallback);
		if(FAILED(hr)) return hr;
	} 
	else if(pfe->cfFormat == m_datatime)
	{
		// if already in use, return error
		// Else connect it up
		//
		if(m_datatimeCallback) return CONNECT_E_ADVISELIMIT;
		hr = pAdvSink->QueryInterface( IID_IAdviseSink, (LPVOID*) &m_datatimeCallback);
		if(FAILED(hr)) return hr;
	} 
	else if(pfe->cfFormat == m_write)
	{
		// if already in use, return error
		// Else connect it up
		//
		if(m_writeCallback) return CONNECT_E_ADVISELIMIT;
		hr = pAdvSink->QueryInterface( IID_IAdviseSink, (LPVOID*) &m_writeCallback);
		if(FAILED(hr)) return hr;
	}
	else
	{
		// Bad stream format
		//
		return DV_E_FORMATETC;
	}

	// Cookie used to indicate connection type
	//
	*pdwConnection = pfe->cfFormat;

	return S_OK;
}

///////////////////////////////////////
// IDataObject::DUnadvise
///////////////////////////////////////
        
STDMETHODIMP IXXXDO::DUnadvise( 
			DWORD dwConnection) 
{
	// Figure out what type of connection it is
	//
	if((UINT)dwConnection == m_data)
	{
		// if not in use, return error
		//
		if(!m_dataCallback) return OLE_E_NOCONNECTION;
		m_dataCallback->Release();
		m_dataCallback = 0;
	} 
	else if((UINT)dwConnection == m_datatime)
	{
		// if not in use, return error
		//
		if(!m_datatimeCallback) return OLE_E_NOCONNECTION;
		m_datatimeCallback->Release();
		m_datatimeCallback = 0;
	} 
	else if((UINT)dwConnection == m_write)
	{
		// if not in use, return error
		//
		if(!m_writeCallback) return OLE_E_NOCONNECTION;
		m_writeCallback->Release();
		m_writeCallback = 0;
	}
	else
	{
		// Bad stream format
		//
		return OLE_E_NOCONNECTION;
	}
	return S_OK;
}
        
///////////////////////////////////////
// IDataObject::GetData
///////////////////////////////////////

STDMETHODIMP IXXXDO::GetData( 
			FORMATETC *pformatetcIn,
			STGMEDIUM *pmedium) 
{
	return E_NOTIMPL;
}


///////////////////////////////////////
// IDataObject::GetDataHere
///////////////////////////////////////

STDMETHODIMP IXXXDO::GetDataHere( 
			FORMATETC *pformatetc,
			STGMEDIUM *pmedium) 
{
	return E_NOTIMPL;
}



///////////////////////////////////////
// IDataObject::QueryGetData
///////////////////////////////////////
        
STDMETHODIMP IXXXDO::QueryGetData( 
			FORMATETC *pformatetc) 
{
	return E_NOTIMPL;
}


///////////////////////////////////////
// IDataObject::GetCanonicalFormatEtc
///////////////////////////////////////
        
STDMETHODIMP IXXXDO::GetCanonicalFormatEtc( 
			FORMATETC *pformatectIn,
			FORMATETC *pformatetcOut) 
{
	return E_NOTIMPL;
}


///////////////////////////////////////
// IDataObject::SetData

        
STDMETHODIMP IXXXDO::SetData( 
			FORMATETC *pformatetc,
			STGMEDIUM *pmedium,
			BOOL fRelease) 
{
	return E_NOTIMPL;
}


///////////////////////////////////////
// IDataObject::EnumFormatEtc
///////////////////////////////////////
        
STDMETHODIMP IXXXDO::EnumFormatEtc( 
			DWORD dwDirection,
			IEnumFORMATETC **ppenumFormatEtc) 
{
	return E_NOTIMPL;
}


///////////////////////////////////////
// IDataObject::EnumDAdvise
///////////////////////////////////////
STDMETHODIMP IXXXDO::EnumDAdvise( 
			IEnumSTATDATA **ppenumAdvise) 
{
	return E_NOTIMPL;
}

