// opc_gsm.cpp - OPCGroupStateMgt test code
//
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
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
// 2.00   01/14/98 acc   
//

#include <stdio.h>
#include <conio.h>
#include "opcda.h"
#include "wcsutil.h"

extern	IMalloc *pIMalloc;
extern	DWORD	hServerGroup3;		// the cloned group
extern	int bGroup3;		// do we need to remove it?


OPCHANDLE TryGetGroupState(IOPCGroupStateMgt * pGRP);
void TrySetGroupState(IOPCGroupStateMgt * pGRP);
void TrySetGroupName(IOPCGroupStateMgt * pGRP);
void TryCloneGroup(IOPCGroupStateMgt * pGRP);

extern	void TryEnumItemAttributes(IOPCItemMgt * pOPC);


void TryGSM( IOPCGroupStateMgt * pGRP1 )
{
	int loop;
	char buffer[40];

	loop = 1;
	while(loop)
	{

		printf("\nTest OPCGroupStateMgt Methods...\n");
		printf("0= GetState...\n");
		printf("1= SetState...\n");
		printf("2= SetName...\n");
		printf("3= CloneGroup...\n");
		printf("x= Exit...\n\n");

		switch(*gets(buffer))
		{
		case '0':
			TryGetGroupState(pGRP1);
			break;
		case '1':
			TrySetGroupState(pGRP1);
			break;
		case '2':
			TrySetGroupName(pGRP1);
			break;
		case '3':
			TryCloneGroup(pGRP1);
			break;
		case 'x':
			loop = 0;
			break;
		}
	}
	printf("GroupStateMgt test complete\n");
}

//---------------------------------------------------------
// TryGetByName
// 
void TryGetByName(IOPCServer * pOPC, LPOLESTR name)
{
	HRESULT   r1;
	IOPCGroupStateMgt *pGRP;

	r1 = pOPC->GetGroupByName(name, IID_IOPCGroupStateMgt, (IUnknown**)&pGRP);
	if (FAILED(r1))
	{
		printf("Error from GetGroupByName(%lx)\n", r1);
	} 
	else
	{
		TryGetGroupState(pGRP);
		pGRP->Release();
	}
	printf( "GetGroupByName test complete\n");
}


//---------------------------------------------------------
// TryGetGroupState
// Perform some functions
OPCHANDLE TryGetGroupState(IOPCGroupStateMgt * pGRP)
{
	HRESULT   r1;
	DWORD     UpdateRate; 
	BOOL      Active;
	LPWSTR    pName;
	LONG      TimeBias;
	FLOAT     PercentDeadband;
	DWORD     LCID;
	OPCHANDLE hClientGroup;
	OPCHANDLE hServerGroup;

	// Get it's status
	//
	r1 = pGRP->GetState(&UpdateRate, &Active, 
			&pName, &TimeBias, &PercentDeadband,
			&LCID, &hClientGroup, &hServerGroup);

	if (FAILED(r1))
	{
		printf("Error from GetState(%lx)\n", r1);
	} 
	else
	{
		// Print the name (to verify it worked)
		// And don't forget to Free the returned string!
		//
		printf("GetState Succeeded for %ls\n", pName);
		printf("UpdateRate = %d, Active = %d, TimeBias=%d, Deadband=%5.2f\n",
			UpdateRate, (DWORD)Active, TimeBias, PercentDeadband);
		printf("LCID=%d ClientHandle=%d, ServerHandle=%d\n", 
			LCID, hClientGroup, hServerGroup);
		
		pIMalloc->Free(pName);
	}


	return hServerGroup;
}

//---------------------------------------------------------
// TrySetGroupState
// 
void TrySetGroupState(IOPCGroupStateMgt * pGRP)
{
	HRESULT   r1;
	DWORD     UpdateRate; 
	DWORD     RevisedRate; 
	BOOL      Active;
	LPWSTR    pName;
	LONG      TimeBias;
	FLOAT     PercentDeadband;
	DWORD     LCID;
	OPCHANDLE hClientGroup;
	OPCHANDLE hServerGroup;

	// Get it's status
	//
	r1 = pGRP->GetState(&UpdateRate, &Active, 
			&pName, &TimeBias, &PercentDeadband,
			&LCID, &hClientGroup, &hServerGroup);

	if (FAILED(r1))
	{
		printf("Error from GetState(%lx)\n", r1);
		return;
	} 

	// Then write it back out
	// zzz improve this test later...
	//
	r1 = pGRP->SetState(&UpdateRate, &RevisedRate, &Active, 
			&TimeBias, &PercentDeadband,
			&LCID, &hClientGroup);

	if (FAILED(r1))
	{
		printf("Error from SetState(%lx)\n", r1);
		return;
	} 
	printf("SetState completed...\n");
}

//---------------------------------------------------------
// TryCloneGroup
// 
void TryCloneGroup(IOPCGroupStateMgt * pGRP)
{
	HRESULT   r1;
	IOPCGroupStateMgt *pGRP3GSM;
	IOPCItemMgt *pGRP3IM;

	// Get it's status
	//
	r1 = pGRP->CloneGroup(L"ClonedGroup1",
		IID_IOPCGroupStateMgt,
		(LPUNKNOWN*)&pGRP3GSM);

	if (FAILED(r1))
	{
		printf("Error from Clone(%lx)\n", r1);
		return;
	} 

	// Then check it out...
	//
	printf("CloneGroup completed; Calling GetState on new group...\n");
	hServerGroup3 = TryGetGroupState(pGRP3GSM);
	bGroup3 = 1;	// flag that we need to remove it later

	printf("\nCalling EnumItemAttributes on new group...\n");
	r1 = pGRP3GSM->QueryInterface(IID_IOPCItemMgt, (void**)&pGRP3IM);
	TryEnumItemAttributes(pGRP3IM);

	pGRP3IM->Release();
	pGRP3GSM->Release();
	// Note the group does NOT (and should not) go away when I do the releases!!
	// I would have to do a RemoveGroup (see TryRemoveGroup)!
	// The user can verify this by backing out to CreateGroupEnumerator
	//
	printf("CloneGroup test completed...\n");
}

//---------------------------------------------------------
// TrySetGroupName
// 
void TrySetGroupName(IOPCGroupStateMgt * pGRP)
{
	HRESULT   r1;

	r1 = pGRP->SetName(L"TestGroup1/Revised");

	if (FAILED(r1))
	{
		printf("Error from SetName(%lx)\n", r1);
		return;
	} 
	// Use GetState to see if it worked
	printf("SetName completed...\n");
}
