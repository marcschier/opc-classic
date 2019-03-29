// wcsutil.h
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


BSTR BSTRFromWSTR(const WCHAR * s, IMalloc *pmem);	// Aloc BSTR and copy WSTR into it
void BSTRFree(BSTR bstr, IMalloc *pmem);			// Free a BSTR
WCHAR * WSTRFromBSTR(const BSTR bstr, IMalloc *pmem);		// Aloc a WSTR and copy a BSTR into it
WCHAR * WSTRClone(const WCHAR *oldstr, IMalloc *pmem);
void WSTRFree(WCHAR * c, IMalloc *pmem);

WCHAR * WSTRFromSBCS(const CHAR *temp, IMalloc *p);
CHAR * SBCSFromWSTR(const WCHAR *temp, IMalloc *p);
void SBCSFree(CHAR *temp, IMalloc *p);


