//============================================================================
// TITLE: COpcDa30Server.h
//
// CONTENTS:
// 
// A concrete instance of a OPC Data Server object.
//
// (c) Copyright 2002-2003 The OPC Foundation
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
// 2002/11/16 RSA   First release.
//

#ifndef _COpcDa30Server_H_
#define _COpcDa30Server_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

#include "COpcDaServer.h"

#include "OpcDa30Server.h"

//============================================================================
// CLASS:   COpcDa30Server
// PURPOSE: A class that implements the IOPCServer interface.

class COpcDa30Server :
    public COpcComObject,
    public COpcDaServer
{
    OPC_CLASS_NEW_DELETE()

    OPC_BEGIN_INTERFACE_TABLE(COpcDa30Server)
        OPC_INTERFACE_ENTRY(IOPCCommon)
        OPC_INTERFACE_ENTRY(IConnectionPointContainer)
        OPC_INTERFACE_ENTRY(IOPCServer)
        OPC_INTERFACE_ENTRY(IOPCBrowse)
        OPC_INTERFACE_ENTRY(IOPCItemIO)
        OPC_INTERFACE_ENTRY(IOPCSecurityNT)
        OPC_INTERFACE_ENTRY(IOPCSecurityPrivate)
    OPC_END_INTERFACE_TABLE()

public:

    //==========================================================================
    // Operators

    // Constructor
    COpcDa30Server() {}

    // Destructor 
    ~COpcDa30Server() {}

    //=========================================================================
    // Public Methods

    // FinalConstruct
    virtual HRESULT FinalConstruct();

    // FinalRelease
    virtual bool FinalRelease();
};

#endif // _COpcDa30Server_H_
