//============================================================================
// TITLE: COpcHdaServerImpl.h
//
// CONTENTS:
// 
// A concrete instance of a OPC Data Server object.
//
// (c) Copyright 2003-2004 The OPC Foundation
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
// 2003/12/17 RSA   Initial implementation.

#ifndef _COpcHdaServerImpl_H_
#define _COpcHdaServerImpl_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

#include "OpcHdaServer.h"
#include "COpcHdaServer.h"

//============================================================================
// CLASS:   COpcHdaServerImpl
// PURPOSE: A class that implements the IOPCServer interface.

class COpcHdaServerImpl :
    public COpcComObject,
    public COpcHdaServer
{
    OPC_CLASS_NEW_DELETE()

    OPC_BEGIN_INTERFACE_TABLE(COpcHdaServerImpl)
        OPC_INTERFACE_ENTRY(IOPCCommon)
        OPC_INTERFACE_ENTRY(IConnectionPointContainer)
        OPC_INTERFACE_ENTRY(IOPCHDA_Server)
        OPC_INTERFACE_ENTRY(IOPCHDA_SyncRead)
        OPC_INTERFACE_ENTRY(IOPCHDA_SyncUpdate)
        OPC_INTERFACE_ENTRY(IOPCHDA_SyncAnnotations)
        OPC_INTERFACE_ENTRY(IOPCHDA_AsyncRead)
        OPC_INTERFACE_ENTRY(IOPCHDA_AsyncUpdate)
        OPC_INTERFACE_ENTRY(IOPCHDA_AsyncAnnotations)
        OPC_INTERFACE_ENTRY(IOPCHDA_Playback)
    OPC_END_INTERFACE_TABLE()

public:

    //==========================================================================
    // Operators

    // Constructor
    COpcHdaServerImpl() {}

    // Destructor 
    ~COpcHdaServerImpl() {}

    //=========================================================================
    // Public Methods

    // FinalConstruct
    virtual HRESULT FinalConstruct();

    // FinalRelease
    virtual bool FinalRelease();
};

#endif // _COpcDaSampleServer_H_