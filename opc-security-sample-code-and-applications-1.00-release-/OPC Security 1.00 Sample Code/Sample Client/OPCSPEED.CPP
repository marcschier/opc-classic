// opcspeed.cpp
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
// Simple test of OPC Server Performance.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//  0.90  05/03/97 acc   
//

#include <time.h>
#include <stdio.h>
#include <conio.h>
#include "opcda.h"	// Include the GENERIC OPC header file
#include "wcsutil.h"
//#include "OLECTL.h"


//---------------------------------------------------------
// Local Functions

void LocalInit(void);
void LocalCleanup(void);

IUnknown * CreateRemoteOPCServer(WCHAR* szProgID, WCHAR*szNodeName);
IUnknown * CreateInprocOPCServer(WCHAR* szProgID);
IUnknown * CreateLocalOPCServer(WCHAR* szProgID);

void TryOPCServer(IOPCServer * pOPC);
void TryAddGroup(IOPCServer * pOPC);
void TryRemoveGroup(IOPCServer * pOPC);

void TryGSM( IOPCGroupStateMgt * pGRP1 );
void TryIM( IOPCItemMgt * pIM);
void TryAddItem(IOPCItemMgt * pIM);
void TryValidateItem(IOPCItemMgt * pIM);
void TestGroup(void);

void TryItemFunctions(IOPCGroupStateMgt * pGRP);

void TryItemReadWrite(IOPCSyncIO *pSIO, int nItem, OPCHANDLE *sh);

void	report( char*n, HRESULT r);

// Global interface to the COM memory manager
IMalloc *pIMalloc;

// Some Global Strings for use in Group/Item Testing
//
WCHAR	*ItemIDs[2];
WCHAR	*AccessPaths[2];
VARTYPE	dtype[2] ;
OPCHANDLE g_sh[100];		// Returned Server Handles for the Items
static int add_done = 0;


// Global list of available server interfaces
//
IUnknown *gpOPC = 0;
IOPCServer *gpOPCS = 0;



//---------------------------------------------------------
// main
void main(void)
{
	HRESULT r2;

	printf("Al Chisholm's OPC Performace Test Program V0.90\n");
	printf("Provided by Intellution Inc.\n");

	LocalInit();

	// Allow the user to run the test several time...
	//
	while(1)
	{
		CHAR c;
		CHAR buffer[82] = {80};
		CHAR *nptr, *pptr;
		WCHAR *node = 0;
		WCHAR *progid = 0;

		while(kbhit())getch();

		printf("Enter Server type I(inproc),L(local),R(remote), X(exit)\n");
		pptr = gets(buffer);
		c = *pptr;
		if(c == 'x') break;
		if(c == 'X') break;

		// read the common info we need for all of the tests
		//
		printf("Enter Server PROGID\n");
		pptr = gets(buffer);
		progid = WSTRFromSBCS(pptr, 0);
		

		while(kbhit())getch();
		switch(c)
		{
		case 'i':
		case 'I':
			gpOPC = CreateInprocOPCServer(progid);
			break;

		case 'l':
		case 'L':
			gpOPC = CreateLocalOPCServer(progid);
			break;

		case 'r':
		case 'R':
			printf("Enter Server NodeName\n");
			nptr = gets(buffer);
			node = WSTRFromSBCS(nptr, 0);
		
			gpOPC = CreateRemoteOPCServer(progid, node);
			break;

		default:
			break;
		}

		if(gpOPC)
		{
			printf("\nChecking Supported Server Interfaces...\n");
			// See what interfaces are supported
			//
			r2 = gpOPC->QueryInterface(IID_IOPCServer, (void**)&gpOPCS);

			report("IOPCServer     ", r2);

			printf("\n");
		}

		if(gpOPC)
		{
			TryOPCServer( gpOPCS );
		}

		// Free the interfaces
		//
		if(gpOPC) gpOPC->Release();
		if(gpOPCS) gpOPCS->Release();


		WSTRFree(progid, 0);
		progid = 0;
		WSTRFree(node, 0);
		node = 0;

		WSTRFree(ItemIDs[0], 0);
		ItemIDs[0] = 0;

		WSTRFree(AccessPaths[0], 0);
		AccessPaths[0] = 0;
	}

	LocalCleanup();

	printf("Done...\n");
	getch();
	exit(0);
}



//---------------------------------------------------------
//
//
void	report( char*n, HRESULT r)
{
	printf("\t%s - ", n);
	if(FAILED(r)) printf("NOT SUPPORTED(%lx)\n", r);
	else printf("Supported\n");
}


//---------------------------------------------------------
// LocalInit													z
// This is generic initialization for a task using COM
void LocalInit(void)
{
	HRESULT	r1;
	// General COM initialization...
	//
	r1 = CoInitialize(NULL);

	if (FAILED(r1))
	{
		printf("Error from CoInitialize\n");
		exit(1);
	}


	// Also get access to the COM memory manager
	//
	r1 = CoGetMalloc(MEMCTX_TASK, &pIMalloc);

	if (FAILED(r1))
	{
		printf("GetMalloc failed\n");
		CoUninitialize();
		exit(1);
	}
}



//---------------------------------------------------------
// CreateServer REMOTE
// Create the requested OPC Server - DCOM enabled!
IUnknown*	CreateRemoteOPCServer(WCHAR*szProgID, WCHAR*szNodeName)
{
	CLSID clsid;
	HRESULT r1, r2;
	MULTI_QI mqi;
	COSERVERINFO	sin, *sinptr;
	DWORD clsctx;

	// Get the CLSID from the Name
	// NOTE this is a call to the LOCAL registry!
	//
	r1 = CLSIDFromProgID(szProgID, &clsid);
	if(FAILED(r1))
	{
		printf("CLSIDFromProgID failed for %ls (%lx)\n", szProgID, r1);
		return NULL;
	}

	// set up server info
	//
	if(*szNodeName)
	{
		sinptr = &sin;
		sin.dwReserved1 = 0;
		sin.dwReserved2 = 0;
		sin.pwszName = szNodeName;
		sin.pAuthInfo = 0;
		clsctx = CLSCTX_REMOTE_SERVER;
	} else
	{
		// If NODE is Nul then try local server
		sinptr = 0;		// pointer should be NULL if local
		clsctx = CLSCTX_LOCAL_SERVER;
	}

	// set up mqi
	//
	mqi.pIID = &IID_IUnknown;
	mqi.hr = 0;
	mqi.pItf = 0;

	// Note you must define _WIN32_DCOM in 'Settings'
	r2 = CoCreateInstanceEx(clsid, NULL, 
		clsctx, sinptr, 1, &mqi);

	if (FAILED(r2) || FAILED(mqi.hr))
	{
		printf("CoCreateInstanceEx - failed for node:%ls ProgID:%ls (%lx)\n", szNodeName, szProgID, r2);
		return NULL;
	}

	printf("Remote Object (with IUnknown) Created for %ls\n", szProgID);
	return (IUnknown*)mqi.pItf;
}


//---------------------------------------------------------
// CreateServer INPROC
// Create the requested OPC Server
IUnknown *CreateInprocOPCServer(WCHAR*szName)
{
	CLSID clsid;
	IClassFactory *pCF;
	HRESULT r1, r2, r3;
	IUnknown * pOPC;

	// Get the CLSID from the Name
	r1 = CLSIDFromProgID(szName, &clsid);
	if(FAILED(r1))
	{
		printf("CLSIDFromProgID failed for %ls\n", szName);
		return NULL;
	}

	// Create an OPC Sample Server Class Factory
	//
	r2 = CoGetClassObject(clsid, CLSCTX_INPROC_SERVER , //try inproc first
		NULL, IID_IClassFactory, (void**)&pCF);

	if (FAILED(r2))
	{
		printf("CoGetClassObject- no InProc server for (%lx)\n", r2);
		return NULL;
	}

	// And use the class factory to create the OPC Server
	// Request an IUnknown Interface to it
	// and release the class factory which is no longer needed
	//
	r3 = pCF->CreateInstance(NULL, IID_IUnknown, (void**)&pOPC);

	pCF->Release();

	if (FAILED(r3))
	{
		printf("Error from CreateInstance (%lx)\n", r3);
		return NULL;
	}
	printf("Inproc Object (with IUnknown) Created for %ls\n", szName);
	return pOPC;
}

//---------------------------------------------------------
// CreateServer LOCAL
// Create the requested OPC Server
IUnknown *CreateLocalOPCServer(WCHAR*szName)
{
	CLSID clsid;
	IClassFactory *pCF;
	HRESULT r1, r2, r3;
	IUnknown * pOPC;

	// Get the CLSID from the Name
	r1 = CLSIDFromProgID(szName, &clsid);
	if(FAILED(r1))
	{
		printf("CLSIDFromProgID failed for %ls\n", szName);
		return NULL;
	}

	// Create an OPC Sample Server Class Factory
	//
	r2 = CoGetClassObject(clsid, CLSCTX_LOCAL_SERVER , //try inproc first
		NULL, IID_IClassFactory, (void**)&pCF);

	if (FAILED(r2))
	{
		printf("CoGetClassObject- no Local server for (%lx)\n", r2);
		return NULL;
	}

	// And use the class factory to create the OPC Server
	// Request an IUnknown Interface to it
	// and release the class factory which is no longer needed
	//
	r3 = pCF->CreateInstance(NULL, IID_IUnknown, (void**)&pOPC);

	pCF->Release();

	if (FAILED(r3))
	{
		printf("Error from CreateInstance (%lx)\n", r3);
		return NULL;
	}
	printf("Local Object (with IUnknown) Created for %ls\n", szName);
	return pOPC;
}




//---------------------------------------------------------
// TryOPCServer
// Use the OPCServer to perform some functions
void TryOPCServer(IOPCServer *pOPC)
{
	TryAddGroup(pOPC);
	TestGroup();
	TryRemoveGroup(pOPC);
}




// Globals for Group interfaces and handles
//
DWORD	hServerGroup1;
IUnknown          * pGRP1U = 0;
IOPCGroupStateMgt * pGRP1GSM = 0;
IOPCSyncIO        * pGRP1SIO = 0;
IOPCItemMgt       * pGRP1IM = 0;



//---------------------------------------------------------
// TryAddGroup
// Use the OPCServer to add some groups
// and then try out the interfaces on the groups
//
void TryAddGroup(IOPCServer * pOPC)
{
	HRESULT r1;
	DWORD	RevisedRate1;
	float	DeadBand1 = (float)0.0;
	HRESULT r2, r4, r6;

	// Try to create 2 groups (later ask user for names)
	//
	if(pGRP1U == 0)
	{
		printf("Creating groups...\n");

		r1 = pOPC->AddGroup( L"TestGroup1", TRUE, 0, 0, 0, &DeadBand1,
			0, &hServerGroup1, &RevisedRate1, IID_IUnknown, 
			(LPUNKNOWN*)&pGRP1U);

		if (FAILED(r1))
		{
			printf("Error from AddGroup (TestGroup1) (%lx)\n", r1);
		} 
		else
		{
			printf("Checking Interfaces on Group 1...\n");

			r2 = pGRP1U->QueryInterface(IID_IOPCGroupStateMgt, (void**)&pGRP1GSM);
			r4 = pGRP1U->QueryInterface(IID_IOPCSyncIO, (void**)&pGRP1SIO);
			r6 = pGRP1U->QueryInterface(IID_IOPCItemMgt, (void**)&pGRP1IM);

			report("IUnknown               ", S_OK);
			report("IOPCGroupStateMgt      ", r2);
			report("IOPCSyncIO             ", r4);
			report("IOPCItemMgt            ", r6);
			printf("\n\n");
		}
	}
	else
	{
		printf("AddGroup already done...\n");
		return;
	}

	printf("AddGroup test complete...\n");
}

//---------------------------------------------------------
// TestGroup
//
void TestGroup(void)
{

	int loop;
	char buffer[40];

	loop = 1;
	while(loop)
	{

		printf("\nTest OPCGroup Interfaces...\n");
		printf("1= ItemMgt (Add some items)...\n");
		printf("2= SyncIO...\n");
		printf("x= Exit...\n\n");

		switch(*gets(buffer))
		{
		case '1':
			if(pGRP1IM) TryAddItem(pGRP1IM);	// Add Items
			break;
		case '2':
			if(pGRP1SIO) TryItemReadWrite(pGRP1SIO, 2, g_sh);
			break;
		case 'x':
			loop = 0;
			break;
		}
	}
	printf("IOPCGroup test complete\n");
}

//---------------------------------------------------------
// TryRemoveGroup
// Note - this can sometimes cause crash of the Inproc sample server
// because that server is not currently threadsafe
//
void TryRemoveGroup(IOPCServer * pOPC)
{
	HRESULT r1;
	// When done
	// Release the Group interface 
	// and remove the group from the server
	// (Which should take care of deleting any items)
	//
	if(!pGRP1U)
	{
		printf("Add needs to be done first\n");
		return;
	}

	if(pGRP1GSM) pGRP1GSM->Release();
	if(pGRP1SIO) pGRP1SIO->Release();
	if(pGRP1IM) pGRP1IM->Release();

	pGRP1GSM = 0;
	pGRP1SIO = 0;
	pGRP1IM = 0;

	if(pGRP1U)
	{
		pGRP1U->Release();
		r1 = pOPC->RemoveGroup(hServerGroup1, FALSE);
		if (FAILED(r1))
		{
			printf("Error from RemoveGroup (TestGroup1) (%lx)\n", r1);
		} 
		pGRP1U = 0;
		add_done = 0;
	}

	printf("Remove Groups test complete\n");
}





void GetItemIDs(void)
{
	char *pptr, buffer[80];
	printf("Enter ItemID AccessPath\n");

	printf("ItemID\n");
	pptr = gets(buffer);
	ItemIDs[0] = WSTRFromSBCS(pptr, 0);

	printf("AccessPath\n");
	pptr = gets(buffer);
	AccessPaths[0] = WSTRFromSBCS(pptr, 0);


try_again:
	printf("Enter Requested Datatype (F=VT_R4, R=VT_R8, I=VT_I2, L=VT_I4, S=VT_BSTR\n");
	pptr = gets(buffer);
	switch(*pptr)
	{
	case 'f':
	case 'F':
		dtype[0] = VT_R4;
		break;
	case 'r':
	case 'R':
		dtype[0] = VT_R8;
		break;
	case 'i':
	case 'I':
		dtype[0] = VT_I2;
		break;
	case 'l':
	case 'L':
		dtype[0] = VT_I4;
		break;
	case 's':
	case 'S':
		dtype[0] = VT_BSTR;
		dtype[1] = VT_BSTR;
		break;
	default:
		goto try_again;
	}
}



void TryAddItem(IOPCItemMgt * pIM)
{
	HRESULT	r1;
	OPCITEMDEF id[2];
	HRESULT *ih;
	OPCITEMRESULT *ir;
	int j;


	if(add_done)
	{
		printf("add already done\n");
		return;
	}

	GetItemIDs();


	// Define two items
	//
	id[0].szItemID = ItemIDs[0];
	id[0].szAccessPath = AccessPaths[0];
	id[0].bActive = TRUE;
	id[0].hClient = 100;
	id[0].dwBlobSize = 0;
	id[0].pBlob = NULL;
	id[0].vtRequestedDataType = dtype[0];

	// Add then items and check the hresults
	//
	printf("Adding 100 Items (1 at a time)...\n");

	for(j=0; j<100; j++)
	{
		r1 = pIM->AddItems(1, id, &ir, &ih);
		if (FAILED(r1))
		{
			printf("Error from AddItems(%lx)\n", r1);
			return;
		}
		else
		{
			add_done = 1;

			// Check the results of each item
			int i = 0;

		 	if (FAILED(ih[i]))
		 	{
		 		printf("Error from AddItems[%d] / hr[%d](%lx)\n", j, i, ih[i]);
		 		// more robust error here would be nice 
		 		// (to prevent use of bad items later)
				//
		 	} else
 		 	{
				// Store handle into global
				g_sh[j] = ir[i].hServer;

				// also Important to free blob if it is returned!
				//
				if(ir[i].pBlob) pIMalloc->Free(ir[i].pBlob);
			}

			// Free the returned HRESULTs and ITEMRESULTS
			//
			pIMalloc->Free(ir);
			pIMalloc->Free(ih);
		}
	}
	printf("AddItems Test Complete...\n");
}



//---------------------------------------------------------
// TryItemReadWrite
// 
void TryItemReadWrite(IOPCSyncIO *pSIO, int nItem, OPCHANDLE *sh)
{
	HRESULT	r1;
	HRESULT *hr;
	OPCITEMSTATE *is;
	char buffer[40];
	char tbuffer[9];
	int j, n, icount = 100;

	if(!add_done)
	{
		printf("Must do Add first\n");
		return;
	}

	// try to read each item
	//

	printf("How many items per read (1-100)\n");
	*gets(buffer);
	icount = atoi(buffer);
	if (icount > 100) icount = 100;

	printf("How many Reads of %d items?\n", icount);
	*gets(buffer);
	n = atoi(buffer);
	_strtime(tbuffer);
	printf("Starting Read loop(%d) at %s...\n", n, tbuffer);

	for(j=0; j<n; j++)
	{
		r1 = pSIO->Read(OPC_DS_CACHE, icount, sh, &is, &hr);
		if (FAILED(r1))
		{
			printf("Error from Read(%lx)\n", r1);
		} 
		else
		{
			// if the read worked then 
			// Free returned items
			//
			int k;
			pIMalloc->Free(hr);
			for(k=0; k<icount; k++)
			{
				VariantClear(&is[k].vDataValue);
			}
			pIMalloc->Free(is);
		}
	}
	_strtime(tbuffer);
	printf("Done reading %ld items at %s...\n", (long)(n*icount), tbuffer);

}



//---------------------------------------------------------
// LocalCleanup
// This is generic cleanup for any task using COM.
void LocalCleanup(void)
{
	// Finally, release the memory manager
	// as well as COM
	//
	pIMalloc->Release();
	CoUninitialize();
}



