//============================================================================
// TITLE: COpcDaEnumItemWrapper.h
//
// CONTENTS:
// 
// An a wrapper for the IEnumOPCItemAttributes interface.
//
// (c) Copyright 2004 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This code is provided by the OPC Foundation solely to assist in 
//  understanding and use of the appropriate OPC Specification(s) and may be 
//  used as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// MODIFICATION LOG:
//
// Date       By    Notes
// ---------- ---   -----
// 2004/04/27 RSA   Initial implementation.

#ifndef _COpcDaEnumItemWrapper_H_
#define _COpcDaEnumItemWrapper_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

//============================================================================
// CLASS:   COpcDaEnumItemWrapper
// PURPOSE: A class to implement the IEnumString interface.
// NOTES:

class COpcDaEnumItemWrapper :
    public COpcComObject,
    public IEnumOPCItemAttributes,
	public COpcSynchObject
{     
    OPC_CLASS_NEW_DELETE()

    OPC_BEGIN_INTERFACE_TABLE(COpcDaEnumItemWrapper)
        OPC_INTERFACE_ENTRY(IEnumOPCItemAttributes)
    OPC_END_INTERFACE_TABLE()

public:

    //========================================================================
    // Operators

    // Constructor
    COpcDaEnumItemWrapper();

    // Constructor
    COpcDaEnumItemWrapper(IUnknown* ipUnknown);

    // Destructor 
    ~COpcDaEnumItemWrapper();

    //========================================================================
    // IEnumOPCItemAttributes

    // Next
	STDMETHODIMP Next( 
		ULONG               celt,
		OPCITEMATTRIBUTES** ppItemArray,
		ULONG*              pceltFetched 
	);

    // Skip
	STDMETHODIMP Skip(ULONG celt);

    // Reset
	STDMETHODIMP Reset();

    // Clone
	STDMETHODIMP Clone(IEnumOPCItemAttributes** ppEnumGroupAttributes);

private:

    //=========================================================================
    // Private Members

	IUnknown* m_ipUnknown;
};

#endif // _COpcDaEnumItemWrapper_H_