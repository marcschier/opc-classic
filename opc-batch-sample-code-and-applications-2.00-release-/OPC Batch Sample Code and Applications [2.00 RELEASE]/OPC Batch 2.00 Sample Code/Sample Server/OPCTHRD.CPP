// OpcThrd.cpp
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
//   This file contains a sample implementation of 
//   Background/Async handling for the OPC sample server.
//
// Note that this is a VERY simple implementation intended
//   (like the rest of this code) to show the basic principles.
//   A 'real' imlementation would probably be much more
//   sophisticated and use the apartment or free threading model
//
// Also this code is currently NOT threadsafe at this time when used InProc
//   It has some minimal critical section logic to deal with the more
//   obvious issues.
//   But the client can still change things inside the server and groups 
//   while this thread is running which can potentially cause problems.
//   I real implementation would need to be more robust than this.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
// 0.90  04/08/97 ACC   add async logic
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCTHRD.h"



#define N_SERVERS	10

///////////////////////////////////////
// The list of server objects being managed
//
struct {
	BOOL	inuse;
	XXXServer	*pServer;
}slist[N_SERVERS];


///////////////////////////////////////
// Initialization at startup (EXE start or DLL load)
//
void InitServerSlots(void)
{
	int i;

	for(i=0; i<N_SERVERS; i++)
	{
		slist[i].inuse = 0;
		slist[i].pServer = NULL;
	}
}

///////////////////////////////////////
// 'Register' a server with the background helper thread
//
BOOL FindServerSlot(int *slot, XXXServer * pServer)
{
	int i;

	// By convention, start with 1
	// so that 0 can indicate NO SLOT
	//
	*slot = 0;
	for(i=1; i<N_SERVERS; i++)
	{
		if(!slist[i].inuse)
		{
			slist[i].inuse = 1;
			slist[i].pServer = pServer;
			*slot = i;
			return TRUE;
		}
	}
	return FALSE;
}

///////////////////////////////////////
// 'UnRegister' a server
//
void ClearServerSlot(int i)
{

	if(slist[i].inuse)
	{
		slist[i].inuse = 0;
		slist[i].pServer = NULL;
	}
}


///////////////////////////////////////
// Invoke the Async State logic in each server object
//
void UpdateServers(DWORD tics)
{
	int i;

	for(i=0; i<N_SERVERS; i++)
	{
		if(slist[i].inuse)
		{
			if(slist[i].pServer) 
			{
				slist[i].pServer->UpdateData(tics);
			}
		}
	}
}

///////////////////////////////////////
// Request Client Shutdown
//
void ShutdownClients(void)
{
	int i;

	for(i=0; i<N_SERVERS; i++)
	{
		if(slist[i].inuse)
		{
			if(slist[i].pServer) 
			{
				slist[i].pServer->ShutdownClients();
			}
		}
	}
}

