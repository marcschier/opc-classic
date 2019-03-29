// OPCTHRD.h
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
// This file contains function prototyles
// for the OPC Sample Code Background Thread Manger
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//  0.03  04/01/97 acc 
//  2.00  01/19/98 acc   add ShutdownClients  
//


void InitServerSlots(void);
BOOL FindServerSlot(int *slot, XXXServer * pServer);
void ClearServerSlot(int i);
void UpdateServers(DWORD tics);
void ShutdownClients(void);
