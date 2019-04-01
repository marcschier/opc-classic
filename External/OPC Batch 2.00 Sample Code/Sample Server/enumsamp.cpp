// enumsamp.cpp
//
// (c) Copyright 1999 The OPC Foundation
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
//  Will Irwin, Fisher-Rosemount Systems Inc.
//
// CONTENTS:
//  This file contains code and data to provide some enumeration sets
//  that are made available through the sample OPC server's 
//  IOPCEnumerationSets interface.  This is purely for the sample
//  server and not intended to show how a real server would do it.
//  In particular this code is not space-efficient nor does it provide
//  for release of memory when the server exits.
//
//  The enumeration sets here include the standard ones defined by
//  the OPC-Batch Specification version 1.0, plus an additional one
//  which illustrates that real servers may add there own.  Also,
//  one of the standard sets has a vendor-defined extension for the
//  purposes of illustration.
//
//
// Modification Log:
// Vers    Date   By    Notes
// ----  -------- ---   -----
//
//
// Define the data that the OPC-Batch sample server will return 
// via the IOPCEnumerationSets interface.
// 

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCBatchDef.h"
#include "enumsamp.h"

DWORD EnumerationSet::M_cEnumSets = 0;

EnumerationSet* allEnumSets[maxEnumSets];  // allow for a fixed number of sets
                                  // the class has a static member that
                                  // tracks how many really exist

void createEnumSets()
{
  // First check if we have already done this.  This method
  // will be called each time a tear-off object is created, 
  // which happens every time a client obtains a IOPCEnumerationSets
  // interface pointer.  We only need to have this data set up 
  // once for the life of the server, and in fact it is an error
  // to do it over again.  So just exit from here if we have
  // done this before.
  if (EnumerationSet::M_cEnumSets > 0)
  {
    return;
  }

  // First, null out all the enum set pointers
  for (int i = 0; i < maxEnumSets; i++)
  {
    allEnumSets[i] = NULL;
  }

  EnumerationSet* phys = new EnumerationSet ( L"PHYSICAL", OPCB_ENUM_PHYS, 8 );
  phys->AddEnum ( OPCB_PHYS_ENTERPRISE, L"Enterprise" );
  phys->AddEnum ( OPCB_PHYS_SITE, L"Site" );
  phys->AddEnum ( OPCB_PHYS_AREA, L"Area" );
  phys->AddEnum ( OPCB_PHYS_PROCESSCELL, L"Process Cell" );
  phys->AddEnum ( OPCB_PHYS_UNIT, L"Unit" );
  phys->AddEnum ( OPCB_PHYS_EQUIPMENTMODULE, L"Equipment Module" );
  phys->AddEnum ( OPCB_PHYS_CONTROLMODULE, L"Control Module" );
  phys->AddEnum ( OPCB_PHYS_EPE, L"Equipment Procedural Element" );

  EnumerationSet* proc = new EnumerationSet ( L"PROCEDURAL", OPCB_ENUM_PROC, 10 );
  proc->AddEnum ( OPCB_PROC_PROCEDURE, L"Procedure" );
  proc->AddEnum ( OPCB_PROC_UNITPROCEDURE, L"Unit Procedure" );
  proc->AddEnum ( OPCB_PROC_OPERATION, L"Operation" );
  proc->AddEnum ( OPCB_PROC_PHASE, L"Phase" );
  proc->AddEnum ( OPCB_PROC_PARAMETER_COLLECTION, L"Parameter Collection" );
  proc->AddEnum ( OPCB_PROC_PARAMETER, L"Parameter" );
  proc->AddEnum ( OPCB_PROC_RESULT_COLLECTION, L"Result Collection" );
  proc->AddEnum ( OPCB_PROC_RESULT, L"Result" );
  proc->AddEnum ( OPCB_PROC_BATCH, L"Batch" );
  proc->AddEnum ( OPCB_PROC_CAMPAIGN, L"Campaign" );

  EnumerationSet* state = new EnumerationSet ( L"STATE", OPCB_ENUM_STATE, 13 );
  state->AddEnum ( OPCB_STATE_IDLE, L"Idle" );
  state->AddEnum ( OPCB_STATE_RUNNING, L"Running" );
  state->AddEnum ( OPCB_STATE_COMPLETE, L"Complete" );
  state->AddEnum ( OPCB_STATE_PAUSING, L"Pausing" );
  state->AddEnum ( OPCB_STATE_PAUSED, L"Paused" );
  state->AddEnum ( OPCB_STATE_HOLDING, L"Holding" );
  state->AddEnum ( OPCB_STATE_HELD, L"Held" );
  state->AddEnum ( OPCB_STATE_RESTARTING, L"Restarting" );
  state->AddEnum ( OPCB_STATE_STOPPING, L"Stopping" );
  state->AddEnum ( OPCB_STATE_STOPPED, L"Stopped" );
  state->AddEnum ( OPCB_STATE_ABORTING, L"Aborting" );
  state->AddEnum ( OPCB_STATE_ABORTED, L"Aborted" );
  state->AddEnum ( OPCB_STATE_UNKNOWN, L"Unknown" );

  EnumerationSet* mode = new EnumerationSet ( L"MODE", OPCB_ENUM_MODE, 4 );
  mode->AddEnum ( OPCB_MODE_AUTOMATIC, L"Auto" );
  mode->AddEnum ( OPCB_MODE_SEMIAUTOMATIC, L"Semi-Auto" );
  mode->AddEnum ( OPCB_MODE_MANUAL, L"Manual" );
  mode->AddEnum ( OPCB_MODE_UNKNOWN, L"Unknown" );

  EnumerationSet* param = new EnumerationSet ( L"PARAM", OPCB_ENUM_PARAM, 3 );
  param->AddEnum ( OPCB_PARAM_PROCESSINPUT, L"Process Input" );
  param->AddEnum ( OPCB_PARAM_PROCESSPARAMETER, L"Process Parameter" );
  param->AddEnum ( OPCB_PARAM_PROCESSOUTPUT, L"Process Output" );

  // Add a vendor specific enum set
  EnumerationSet* vendorSpecific1 = new EnumerationSet ( L"VENDORSPECIFIC", 100, 29 );
  vendorSpecific1->AddEnum ( 25, L"Open" );
  vendorSpecific1->AddEnum ( 26, L"Closed" );
  vendorSpecific1->AddEnum ( 27, L"In Transition" );
  vendorSpecific1->AddEnum ( 28, L"Failed" );

  allEnumSets[0] = phys;
  allEnumSets[1] = proc;
  allEnumSets[2] = state;
  allEnumSets[3] = mode;
  allEnumSets[4] = param;
  allEnumSets[5] = vendorSpecific1;
}

void destroyEnumSets ()
{
  for (int i = 0; i < maxEnumSets; i++)
  {
    if (allEnumSets[i] != NULL)
    {
      delete allEnumSets[i];
    }
  }
}


EnumerationSet* getEnumSetWithId ( DWORD id )
{
  EnumerationSet* pResult = NULL;  // return NULL if not found

  for (DWORD i = 0; i < maxEnumSets; i++)
  {
    EnumerationSet* pSet = allEnumSets[i];
    if ( (pSet != NULL) && (id == pSet->GetEnumSetId()) )
    {
      pResult = pSet;
      break;
    }
  }
  return pResult;
}
