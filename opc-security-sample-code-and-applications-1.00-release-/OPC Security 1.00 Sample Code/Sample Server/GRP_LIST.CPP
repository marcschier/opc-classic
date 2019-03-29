// Grp_list.cpp
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
//  the XXX Group Object ITEM LIST management functions
//  for the OPC Sample server.
//
//
// Note - this logic supports a fixed list of N_ITEMS
// But is designed to be easily modified to support a varaiable length list
//
// Modification Log:
// Vers    Date   By    Notes
// ----  -------- ---   -----
// 0.90  04/08/97 ACC   
//

#define WIN32_LEAN_AND_MEAN
#define NOCOMM

#include "OPCXXX.h"



///////////////////////////////////////
// XXXGroup::ItemValid
// Verify that a handle is valid
//
BOOL XXXGroup::ItemValid(OPCHANDLE h)
{
	if (h >= N_ITEMS) return 0;
	if(!m_items[h].inuse) return 0;
	return 1;
}


///////////////////////////////////////
// XXXGroup::ItemPtr
// Return ptr to item, given handle
//
XXXItem* XXXGroup::ItemPtr(OPCHANDLE h)
{
	if (!ItemValid(h)) return 0;
	return m_items[h].pItem;
}


///////////////////////////////////////
// XXXGroup::ItemHandles
// Return # item handles (for use in loops)
// Note that some item handles may be empty (See ItemValid)
//
int XXXGroup::ItemHandles(void)
{
	return N_ITEMS;
}


///////////////////////////////////////
// XXXGroup::ItemAlloc
// Return next available item handle
//
HRESULT XXXGroup::ItemAlloc(OPCHANDLE *h)
{
	int j;

	// find a place to put the item
	//
	for(j=0; j<N_ITEMS; j++)
	{
		if(m_items[j].inuse == 0) break;
	}
	if(j >= N_ITEMS)
	{
		return E_OUTOFMEMORY;
	}
	*h = j;
	return S_OK;
}


///////////////////////////////////////
// XXXGroup::ItemReAlloc
// Make sure a specific handle is available
// (used only by Clone - you can assume the handle is free)
//
HRESULT XXXGroup::ItemReAlloc(OPCHANDLE h)
{
	if(h < N_ITEMS) return S_OK;
	return E_OUTOFMEMORY;
}



///////////////////////////////////////
// XXXGroup::ItemSet
// Assign an Item to a specific handle
//
void XXXGroup::ItemSet(OPCHANDLE h, XXXItem * p)
{
	m_items[h].pItem = p;
	m_items[h].inuse = 1;
}


///////////////////////////////////////
// XXXGroup::ItemFree
// Free a specific Item Handle 
// (caller is assumed to have freed the item itself)
//
void XXXGroup::ItemFree(OPCHANDLE h)
{
	m_items[h].pItem = 0;
	m_items[h].inuse = 0;
}


