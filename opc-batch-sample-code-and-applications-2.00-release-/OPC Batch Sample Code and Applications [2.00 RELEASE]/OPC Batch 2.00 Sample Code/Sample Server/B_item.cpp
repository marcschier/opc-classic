// B_item.cpp
//
// (c) Copyright 1999 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Batch Specification and may be used
//  as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by
//  Will Irwin, Fisher-Rosemount Systems Inc.
//
// CONTENTS:
//
//  This file contains the implementation of 
//  a Batch Item Object for the OPC Batch Sample server.
//
//  This is derived from the XXXItem object used in the 
//  Data Access Sample server.  It overrides only the minimum
//  methods from XXXItem that is necessary to provide basic
//  item functionality for the batch sample server.
//
//  Its purpose is to allow a client to do 'AddItems' calls
//  that specify one of the item IDs in the Batch namespace.

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCProps.h"
#include "OPCerror.h"



XXXBatchItem::XXXBatchItem ( XXXGroup *parent ) : XXXItem(parent)
{
  VariantInit(&m_vValue);

} 

XXXBatchItem::~XXXBatchItem ()
{
  // Free anything we created 
  VariantClear(&m_vValue);

}

// Determine what data the item references so that
// subsequent reads/writes can get to it.
// In a real server this would set up a link to
// something in the batch execution system.  Here
// we simply copy the value that is referenced
// and subsequent reads will get that value.

HRESULT XXXBatchItem::Init( int j, OPCITEMDEF *def, OPCITEMRESULT *ir)
{
	HRESULT hr;
	SYSTEMTIME SystemTime;

	m_Quality = 0xC0;					// Quality = 'Good' (11000000)
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

  // There are four possible kinds of item ID we may encounter:
  // 1) itemID is the ID of a branch within our hierarchy -
  //    e.g. "OPCBBatchModel.BATCH1"
  // 2) itemID is the ID of a leaf within our hierarchy -  
  //    e.g. "OPCBBatchModel.BATCH1.Description"
  //    In this case the last part of the item ID (".Description")
  //    matches a property name, and the preceding portion 
  //    "OPCBBatchModel.BATCH1" is the item id of a branch.
  // 3) itemID is for a valid branch but the property name
  //    suffix is bad.  This is a bad itemID
  // 4) itemID is none of the above, so it is invalid (in
  //    a real server it would be possible for other non-batch
  //    items to be supported, but this sample server does not
  //    do this.)

  WCHAR branchID[200];  // (not used here, but ParseItemID requires it)
  WCHAR propName[50];   // (not used here, but ParseItemID requires it)
  DWORD dwPropertyID = 0;

  m_pBatchObj = ParseItemID ( m_szItemID, branchID, propName, dwPropertyID );
  if (m_pBatchObj == NULL)
  {
    // itemID is bad
    return OPC_E_UNKNOWNITEMID;
  }

  // If we get to here, we have a valid item id.  It could either
  // be of a branch object (no propertyName like ".Value" appended)
  // or a leaf item (with a propertyName).   If there isn't a 
  // property name we assume they want ".Value" as a default.

  if (dwPropertyID == 0) 
    dwPropertyID = OPC_PROP_VALUE;

  VARIANT v;
  VariantInit(&v);

  // Get the value of our item
  hr = m_pBatchObj->GetPropertyAtID(dwPropertyID, v);
  VariantCopy ( &m_vValue, &v );

  // Get the canonical data type of our item
 	m_vtCanonical = V_VT(&v);

	if (def->szAccessPath)	// note accesspath is optional
		m_szAccessPath = WSTRClone(def->szAccessPath, NULL);

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
	ir->dwAccessRights = OPC_READABLE;      // This OPC Batch sample server doesn't support writes
                                          // (per the spec, a Batch server is not required to.) 
	ir->pBlob = NULL;
	ir->dwBlobSize = 0;

	return S_OK;
}


HRESULT XXXBatchItem::GetValue( OPCDATASOURCE ds, VARIANT * v, WORD *q, FILETIME*t)
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

  // Special handling in the OPC Batch Sample Server for
  // items that represent data for which the value is not
  // known at the time it is requested.  An example is the
  // ActualEndTime property of a batch which has not ended
  // yet - the property exists but we must let the client
  // know that the value needs to be interpreted with care
  // (possibly it should be displayed differently).  This 
  // case is handled in the sample server by storing a 
  // special value, and when we see this value we return 
  // the special error code that the OPC Batch Spec defines
  // for this purpose.
  if (m_vtCanonical == VT_DATE)
  {
    m_vValue;

    return OPCB_E_NOT_MEANINGFUL;
  }
  
  // Then return the data
	//
	if(v)
	{
    // This is where we do something different for Batch
    VariantCopy(v, &m_vValue);

		// If caller has requested a conversion
		//	Then try to convert to requested type (this should succeed).
		// else 
		//	just return the canonical type
		//

		r1 = S_OK;
		if(m_vtRequested != VT_EMPTY)
		{
			// Convert to requested type
			// and check error
			//
			r1 = VariantChangeType(v, v, 0, m_vtRequested);
			if(FAILED(r1)) 
			{
				if(q) *q = 0;		// BAD
			}
		}
	}

	return r1;
}

///////////////////////////////////////
HRESULT XXXBatchItem::SetValue(VARIANT * v)
{
  // Batch items don't support writes in the sample server.
  //
	return OPC_E_BADRIGHTS;
}
