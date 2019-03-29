// WCSUtil.cpp
//
// (c) Copyright 1998 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Specifications and may be used
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
//	0.02  05/06/97 ACC
//
//


#define WIN32_LEAN_AND_MEAN

#include "windows.h"
#include "OLE2.h"
#include "string.h"

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

///////////////////////////////////////
// Free a SBCS String 
//
///////////////////////////////////////
void SBCSFree(CHAR * c, IMalloc *pmem)
{
	if(c == NULL) return;

	if(pmem)  pmem->Free(c);
	else  delete c;
}



///////////////////////////////////////
// Clone a WSTR into a SBCS String 
//
///////////////////////////////////////
CHAR * SBCSFromWSTR(const WCHAR *wcbuf, IMalloc * pmem)
{
	int	length, i;
	CHAR	*temp;

	length = wcslen(wcbuf) + 1;
	if(pmem) temp = (CHAR *) pmem->Alloc( length );
	else temp = new CHAR[ length];
	
	if(temp)
	{
		for(i=0; i<length; i++) temp[i] = (CHAR) wcbuf[i];
	}
	return temp;
}

///////////////////////////////////////
// Clone a SBCS into a WSTR String 
//
///////////////////////////////////////
WCHAR * WSTRFromSBCS(const CHAR *buf, IMalloc * pmem)
{
	int	length, i;
	WCHAR	*temp;

	length = strlen(buf) + 1;
	if(pmem) temp = (WCHAR*)pmem->Alloc(sizeof(WCHAR) * (strlen(buf) + 1));
	else temp = new WCHAR[strlen(buf) + 1];
	
	if(temp)
	{
		for(i=0; i<length; i++) temp[i] = (WCHAR) buf[i];
	}
	return temp;
}



