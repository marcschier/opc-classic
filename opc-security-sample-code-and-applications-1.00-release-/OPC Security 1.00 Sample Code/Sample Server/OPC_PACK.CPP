// OPC_PACK.cpp
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
//  This file contains utility functions which
//  the server and client can use to 
//  'pack' or 'Marshall' VARIANTS into the HGLOBAL 
//  used by the IDataObject.
//
// Note - this logic currently supports only numeric and VT_BSTR 
// data types which are those most commonly used.
// Arrays are not currently supported (although they should be).
//
// Modification Log:
//  Vers    Date   By    Notes
//  ----  -------- ---   -----
//  0.90  05/02/97 acc   
//        05/21/97 acc   fix per spec to include NUL in BSTR
//                       Note we include 2 bytes of NUL as in the
//                       proposed OPC 1.0A spec rather than 1 byte.
//                       (This area is currently under review)
//                       Note that the UnPack function does not care
//                       how many NUL bytes are in the stream.
//                       Also use Microsoft fcns to get size.
//

#define WIN32_LEAN_AND_MEAN

#include "OPCDA.h"
#include "OPC_PACK.h"


// NOTE: This version has limited functionality 
// It handles all numeric types plus BSTR. 
// It does NOT handle arrays at this time
// but will be enhanced to do so in the near future. 
//
///////////////////////////////////////
// OPCVariantPack - used by Server
// 'Marshalls' the Variant into the 'HGLOBAL'
// Returns the size of the marshalled data
//
int	OPCVariantPack( char*dest, VARIANT *vp)
{
	int extra = 0;
	unsigned short *bp;
	DWORD len;

	// Copy the 'core' of the variant
	// Note that for anything with a 'pointer'
	// the pointer in the 'dest' memory will be meaningless
	// (See Unpack below)
	//
	memcpy(dest, vp, sizeof(VARIANT));

	// determine where extra (if any) will go
	//
	dest += sizeof(VARIANT);

	switch(vp->vt)
	{
	case VT_BSTR:
		bp = vp->bstrVal;
		len = SysStringByteLen(bp);
		*(DWORD*)dest = len;		// Store byte count in stream
		dest += sizeof(DWORD);
		memcpy(dest, bp, len + 2);	// Store string & NUL
		extra = len + sizeof(DWORD) + 2;	// DWORD prefix plus byte count plus NUL
		break;

	default:
		extra = 0;
		break;
	}

	return sizeof(VARIANT) + extra;
}

///////////////////////////////////////
// OPCVariantSize - Used by Server
// Return the number of bytes needed to marshall a variant.
// (Must be the same as that returned by OPCVariantPack)
// This can be used to compute the total size required for the stream.
//
int	OPCVariantSize(VARIANT *vp)
{
	int extra = 0;
	unsigned short *bp;
	int len;

	switch(vp->vt)
	{
	case VT_BSTR:
		bp = vp->bstrVal;
		len = SysStringByteLen(bp);
		extra = len + sizeof(DWORD) + 2;	// DWORD prefix plus byte count plus NUL
		break;

	default:
		extra = 0;
		break;
	}

	return sizeof(VARIANT) + extra;
}



///////////////////////////////////////
// OPCVariantUnPack - Used by Client
// 'Marshalls' the Variant OUT of the 'HGLOBAL'
// The passed variant is assumed to be EMPTY 
// (see VariantInit or VariantClear)
//
void	OPCVariantUnpack( VARIANT *vp, char * src)
{
	unsigned short *bp;

	// copy the 'body' of the variant
	// This works OK for any of the numeric values 
	// within the union itself
	//
	memcpy(vp, src, sizeof(VARIANT));		
	
	// Check for special cases (things with pointers)
	//
	switch(vp->vt)
	{
	case VT_BSTR:
		DWORD strlen;
		// invalidate the pointer which is from the server's context
		//
		vp->bstrVal = NULL;

		src += sizeof(VARIANT);		// skip past the Variant 'core'
		strlen = *(DWORD*) src;		// get byte count (char count x 2)
		src += sizeof(DWORD);		// skip to the string (WIDECHAR)
		bp = (unsigned short *) src;	// get string pointer
		// Allocate memory, store the string, append the NUL
		//
		vp->bstrVal = SysAllocStringLen( (unsigned short*) bp, strlen/2);
	break;

	default:
		break;
	}
}

