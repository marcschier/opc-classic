// item.cpp
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
//  an XXX Item Object for the OPC Sample server.
//
//  Note that there is nothing in the Custom interface definition
//  that actually requires the creation of a COM based Item Object.
//  However doing so is consistant with the approach taken for the 
//  vendor specific Server and Group objects and, 
//  although not required, is certainly a reasonable approach.
//  In addition it will provide a convenient place to attach
//  the Item Automation interface.
//
//  As always there are many ways to address the implementation and
//  many tradeoffs in complexity vs clarity vs speed.
//  Please note that the intent of THIS code is to emphasize clarity
//  over everthing else.
//
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.00  12/18/96 ACC
// 0.02  03/01/97 acc    fix constructor to init item&accesspath ptrs
//                       add support for Variant Conversions
//                       other minor reorganizations for clarity
//                       make the data change on each read for better demos
// 0.90  04/08/97 ACC    add async logic
//                       and simulate function
//                       Fix SetDataType to store vtRequested
//  2.00  01/19/98 acc   add check for bad pointer itemid and access pointers
//        07/24/98 acc   fix minor glitch in simulated data.
//        08/19/99 acc   fix minor casting problem to eliminate warning
//        02/02/00 acc   add ability to simulate bad or uncertain quality
//        07/13/00 acc   fix SetDataType to validate the VARTYPE.
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCerror.h"

#include <math.h>


/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// XXXItem()
//   Constructor for the Sample Item.
//
///////////////////////////////////////
XXXItem::XXXItem( XXXGroup *parent)
{
	mRefCount = 0;

	// Record Parent Group
	//
	m_Parent = parent;

	// init pointers
	//
	m_szItemID = NULL;		//acc002
	m_szAccessPath = NULL;

	// Default state is to generate simulated data
	m_GenData = 1;
	m_LastWriteError = S_OK;
	m_AsyncMask = 0;
}



///////////////////////////////////////
// ~XXXItem()
//   Destructor for the OPC Sample Item.
//
///////////////////////////////////////
XXXItem::~XXXItem( void)
{
	// Free memory associated with the Item
	//
	if(m_szItemID) delete [] m_szItemID;
	if(m_szAccessPath) delete [] m_szAccessPath;
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions
//

///////////////////////////////////////
// IUnknown::AddRef()
//   Standard IUnknown implementation
//
///////////////////////////////////////
STDMETHODIMP_(ULONG) XXXItem::AddRef( void)
{
	return ++mRefCount;
}



///////////////////////////////////////
// IUnknown::Release()
//   Standard IUnknown implementation
//
///////////////////////////////////////
STDMETHODIMP_(ULONG) XXXItem::Release( void)
{
	ULONG currentCount = --mRefCount;

	// If no references left for this Item
	if ( currentCount == 0)
	{
		// Then delete this Item.
		delete this;
	}
	return currentCount;
}


///////////////////////////////////////
// IUnknown::QueryInterface()
//   Standard IUnknown implementation
//
///////////////////////////////////////
STDMETHODIMP XXXItem::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	if ( ppInterface == NULL)
		return E_INVALIDARG;

	if ( iid == IID_IUnknown )
		*ppInterface = (IUnknown*) this;

	else
	{
		*ppInterface = NULL;
	}
	
	if ( *ppInterface == NULL)
		return E_NOINTERFACE;

	AddRef();
	return S_OK;
}


/////////////////////////////////////////////////////////////////////////////
// Member functions
//

///////////////////////////////////////
// Init 
// Initialize the ITEM per the OPCITEMDEF		acc002
// This is the least 'real' function in this sample server
//
// Normally this logic would 'connect' the XXXItem to a value
// in the 'real' data source; PLC, DCS, etc
// If the specified ItemID did not exist in the 'real' source
// we would return an error.
//
// However this sample code just accepts any ItemID
// and creates some 'dummy' data for it.
//
///////////////////////////////////////
HRESULT XXXItem::Init( int j, OPCITEMDEF *def, OPCITEMRESULT *ir)
{
	HRESULT hr;
	SYSTEMTIME SystemTime;

	// Initialize some dummy values for testing		acc002
	//
	m_vtCanonical = VT_R4;
	m_Value = (float) 0.0;
	m_SimValue = (float) (0.0 + ((FLOAT) (j%61)) / 10.0);
	GetSystemTime(&SystemTime);		// Get current UTC Time
	SystemTimeToFileTime(&SystemTime, &m_TimeStamp); // and store it
	
	// Now make sure requested data type is OK
	//
	hr = SetDatatype(def->vtRequestedDataType);		//acc002
	if(FAILED(hr)) return hr;

	// If so then copy the ItemID and Accesspath
	// Note a 'real' server would validate the ITEMID here...
	// Also, memory allocation errors are possible here and should be
	// checked by a real server...
	//
	if (def->szItemID)		// in case caller program is buggy
		m_szItemID = WSTRClone(def->szItemID, NULL);
	else
		return OPC_E_INVALIDITEMID;

	if (def->szAccessPath)	// note accesspath is optional
		m_szAccessPath = WSTRClone(def->szAccessPath, NULL);

	// Set Item Quality default value
	// Note special values for testing purposes...
	// Any 'Q' tag detaults to uncertain
	// Any 'X' tag detaults to bad
	//
	switch(*m_szItemID)
	{
	case L'Q':
		m_Quality = OPC_QUALITY_UNCERTAIN;
		break;
	case L'X':
		m_Quality = OPC_QUALITY_BAD;
		break;
	default:
		m_Quality = OPC_QUALITY_GOOD;
		break;
	}

	// Copy other Itemdef info to Item object
	// (Note Blob stuff is not supported by this example)
	//
	m_hServerItem = j;
	m_hClientItem = def->hClient;
	m_bActive = def->bActive;

	// Mark data as 'changed' for OnDataChange
	//
	MarkAsChanged(OPC_ODC_ANY);

	// Set ITEM RESULT
	//
	ir->hServer = m_hServerItem;
	ir->vtCanonicalDataType = m_vtCanonical;
	ir->dwAccessRights = OPC_WRITEABLE | OPC_READABLE;
	ir->pBlob = NULL;
	ir->dwBlobSize = 0;

	return S_OK;
}

///////////////////////////////////////
// SetActive
//
///////////////////////////////////////
void XXXItem::SetActive(BOOL a)
{
	m_bActive = a;

	// If going to active then also queue an OnDataChange
	// else if going inactive then clear DataChange
	//
	if(m_bActive) MarkAsChanged(OPC_ODC_ANY);
	else ClearChanged(OPC_ODC_ANY);
}

///////////////////////////////////////
// GetActive
//
///////////////////////////////////////
BOOL XXXItem::GetActive(void)
{
	return m_bActive;
}

///////////////////////////////////////
// SetHandle
//
///////////////////////////////////////
void XXXItem::SetHandle(OPCHANDLE h)
{
	m_hClientItem = h;
}

///////////////////////////////////////
// GetHandle
//
///////////////////////////////////////
OPCHANDLE XXXItem::GetHandle(void)
{
	return m_hClientItem ;
}

///////////////////////////////////////
// SetDataType
//
///////////////////////////////////////
HRESULT XXXItem::SetDatatype(VARTYPE vt)
{
	// Ideally (per the spec) we should verify here that the requested type
	// is compatible with the canonical type
	// (e.g. by making a copy of the value and trying 'VariantChangeType').
	//

	// Validate the datatype (note this sample does not support arrays)
	//
	switch(vt)
	{
	case VT_EMPTY:
	case VT_UI1:
	case VT_I2:
	case VT_I4:
	case VT_R4:
	case VT_R8:
	case VT_CY:
	case VT_DATE:
	case VT_BSTR:
	case VT_BOOL:
		m_vtRequested = vt;
		return S_OK;
		break;
	default:
		return OPC_E_BADTYPE;
	}
}

///////////////////////////////////////
// GetDataType
//
///////////////////////////////////////
VARTYPE XXXItem::GetDatatype(void)
{
	return m_vtRequested;
}


///////////////////////////////////////
// Clone and item
// (this is similar to a copy constructor)
//
///////////////////////////////////////
XXXItem * XXXItem::Clone(XXXGroup * newparent, OPCHANDLE newserveritemhandle)
{
	XXXItem * item;
	item = new XXXItem(newparent);

	item->m_hServerItem = newserveritemhandle;

	item->m_szItemID = WSTRClone(m_szItemID, NULL);
	item->m_szAccessPath = WSTRClone(m_szAccessPath, NULL);
	item->m_hClientItem = m_hClientItem;
	item->m_vtRequested = m_vtRequested;
	item->m_bActive = m_bActive;
	
	return item;
}


///////////////////////////////////////
// Fill in an OPCITEMATTRIBUTE
// This is similar to IAClone() in itemutil.cpp
// Caller should delete contents of the IA 
// using IAFree()
//
///////////////////////////////////////
void XXXItem::IAGet(OPCITEMATTRIBUTES *newIA)
{
	// Note Blob and EUInfo not supported at present

	//
	// the easy stuff...
	//
	newIA->bActive = m_bActive;
	newIA->hServer = m_hServerItem;
	newIA->hClient = m_hClientItem;
	newIA->dwAccessRights = OPC_READABLE | OPC_WRITEABLE;
	newIA->dwBlobSize = 0;	// not supported
	newIA->pBlob = NULL;
	newIA->vtRequestedDataType = m_vtRequested;
	newIA->vtCanonicalDataType = m_vtCanonical;
	newIA->dwEUType = OPC_NOENUM;		// not supported
	newIA->vEUInfo.vt = VT_EMPTY;

	// strings...
	// Use local memory
	//
	newIA->szAccessPath = WSTRClone( m_szAccessPath, NULL);
	newIA->szItemID = WSTRClone(m_szItemID, NULL);

}



///////////////////////////////////////
// Get Item Value
// 
//
///////////////////////////////////////
HRESULT XXXItem::GetValue( OPCDATASOURCE ds, VARIANT * v, WORD *q, FILETIME*t)
{
	HRESULT r1 = S_OK;

	// First handle Quality 
	//
	if(q) switch(ds)
	{
	case OPC_DS_CACHE:
		if(m_bActive) *q = m_Quality;
		else *q = QUAL_NOTACTIVE;
		break;

	case OPC_DS_DEVICE:
		*q = m_Quality;
		break;
	}

	// Then Timestamp
	//
	if(t) *t = m_TimeStamp;

	// Then return the data
	//
	if(v)
	{
		double temp;
  		temp = m_Value;

		// First set the target to the canonical type (VT_R4)
		// If caller has requested a conversion
		//	Then try to convert to requested type (this should succeed).
		// else 
		//	just return the canonical type
		//

		v->vt = m_vtCanonical;		// note passed variant is known to be empty.
		v->fltVal= (float) temp;
		r1 = S_OK;
		if(m_vtRequested != VT_EMPTY)
		{
			// Convert to requested type
			// and check error
			//
			r1 = VariantChangeType(v, v, 0, m_vtRequested);
			if(FAILED(r1)) 
			{
				if(q) *q = OPC_QUALITY_BAD;		// BAD
			}
		}
	}

	return r1;
}


///////////////////////////////////////
// Set Item Value
//
//
///////////////////////////////////////
HRESULT XXXItem::SetValue(VARIANT * v)
{
	HRESULT r1;

	// First change the data to the canonical type (VT_R4)
	//
	r1 = VariantChangeType(v, v, 0, m_vtCanonical);
	if(FAILED(r1)) return r1;

	// Store the new value into the data item value.
	// A real server would need to trigger some sort
	// of write to the hardware.
	//
	m_Value = v->fltVal;

	// On Write, this sample code marks the data as changed 
	// for any OnDataChange operations.
	//
	MarkAsChanged(OPC_ODC_ANY);

	// If we ever WRITE to this value - stop generating simulated data
	//
	m_GenData = 0;

	// This TimeStamp update on 'write' is for the sample code only.
	// A real server would update the time when the value
	// was read from the real hardware device
	//
	SYSTEMTIME SystemTime;
	GetSystemTime(&SystemTime);		// Get current UTC Time
	SystemTimeToFileTime(&SystemTime, &m_TimeStamp); // and store it
	
	return S_OK;
}


///////////////////////////////////////
// Simulate Item Value read from device
// 
//
///////////////////////////////////////
void XXXItem::Simulate( void )
{
	double temp;
#define pi 3.14159
	if(m_GenData & m_bActive)
	{
		// Generate some interesting data that increments over time...
		//
		m_SimValue += (float)(pi/60.0);
		if(m_SimValue > (2.0 * pi)) m_SimValue =  0.0;
		
		temp = sin(m_SimValue);
		m_Value = (float)((temp + 1.0) * 50.0);	// scale = 0.0 to 100.0

		// Mark the data as changed for any OnDataChange operations.
		// A real server would set this on any change to
		// Value or Quality.
		//
		MarkAsChanged(OPC_ODC_ANY);
	}
}

///////////////////////////////////////
// Queue an Async Read
//
///////////////////////////////////////
void XXXItem::QueDeviceRead( short mask )
{
	// For this sample server nothing is actually required here
	// A real server might initiate some sort of transaction
	// with the hardware device.
	//
}

///////////////////////////////////////
// Check Async Read
// return 1 if complete
//
///////////////////////////////////////
BOOL XXXItem::CheckDeviceRead( HRESULT *r )
{
	// For this sample server read completes instantly
	//
	if(r) *r = S_OK;
	return 1;
}

///////////////////////////////////////
// Queue an Async Write
//
///////////////////////////////////////
void XXXItem::QueDeviceWrite( VARIANT *v )
{
	m_LastWriteError = SetValue(v);
}

///////////////////////////////////////
// Check Async Write compete
// return 1 is complete
//
///////////////////////////////////////
BOOL XXXItem::CheckDeviceWrite( HRESULT *r )
{
	// For this sample server Write completes instantly
	//
	if(r) *r = m_LastWriteError;
	return 1;
}

///////////////////////////////////////
// Mark cached data as changed
//
///////////////////////////////////////
void XXXItem::MarkAsChanged( WORD flg )
{
	// If item is active then mark it for callback
	//
	if(m_bActive) m_AsyncMask |= flg; 
}

///////////////////////////////////////
// Determine if cached item has changed
//
///////////////////////////////////////
BOOL XXXItem::Changed( WORD flg)
{
	if(m_bActive && (m_AsyncMask & flg)) return 1; 
	return 0;
}

///////////////////////////////////////
// Clear cached item changed
//
///////////////////////////////////////
void XXXItem::ClearChanged( WORD flg )
{
	m_AsyncMask &= ~flg;
}

