// OPCUtil.cpp
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
//  This file contains some string utility functions
//  for the OPC Sample server.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.01  12/31/96 ACC
// 0.02  10/27/97 acc   remove bstrfromwstr and bstrfree 
//                      which were unused (and also wrong).
//
//


#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "string.h"


///////////////////////////////////////
// Utility function to malloc a WSTR 
// and copy a BSTR into it
//
///////////////////////////////////////
WCHAR * WSTRFromBSTR(BSTR bstr, IMalloc *pmem)
{
	WCHAR *wstr;

	bstr--;
	bstr--;
	if(pmem) wstr = (WCHAR*) pmem->Alloc( bstr[0] );
	else wstr = new WCHAR[bstr[0]];
	
	if(wstr == NULL) return NULL;

	wcscpy(wstr, &bstr[2]);

	return wstr;
}

///////////////////////////////////////
// Clone a Wide String 
//
///////////////////////////////////////
WCHAR * WSTRClone(const WCHAR *oldstr, IMalloc *pmem)
{
	WCHAR *newstr;

	if(pmem) newstr = (WCHAR*)pmem->Alloc(sizeof(WCHAR) * (wcslen(oldstr) + 1));
	else newstr = new WCHAR[wcslen(oldstr) + 1];

	if(newstr) wcscpy(newstr, oldstr);
	return newstr;
}

///////////////////////////////////////
// Free a Wide String 
//
///////////////////////////////////////
void WSTRFree(WCHAR * c, IMalloc *pmem)
{
	if(c == NULL) return;

	if(pmem)  pmem->Free(c);
	else  delete c;
}


