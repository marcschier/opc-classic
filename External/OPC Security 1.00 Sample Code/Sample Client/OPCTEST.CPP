// opctest.cpp
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
// Very simple example of an OPC Client program.
// It shows the proper use of most of the interfaces and methods
// including use of Variants and proper Freeing of returned memory!
//
// The main goal of the program is to try to call every method
// on every interface to show proper use of the method and proper freeing
// of returned parameters.
//
// The secondary goal of this program is to serve as a general purpose
// test program for OPC Custom Servers and to try to allow the User
// to perform a reasonable (but certainly not exhaustive) test of 
// the server behavior. It is NOT very user friendly in this regard.
// But it should still be fairly useful.
//
// This test program can be used to connect to and 
// test the basic functionality of any OPC Custom Server.
//
// Note that it creates 2 groups, adds 2 tags to each group,
// and reads the data for both tags.
// The idea is that if 1 works it proves very little
// but if '2' work then probably 'n' will work.
//
// What is missing from this version:
//   Some of the OPTIONAL interfaces are missing.
//   A few of the less commonly used methods are not tested.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.00  11/18/96 ACC
//	      12/01/96 acc   add PersistFile, Enumerators, Item fuctions
//                       Separate INPROC and LOCAL server tests
//	0.01  02/08/97 acc   Fix memory leaks (see acc001)
//	0.02  02/28/97 acc   Add ability to enter server and node name
//                      and to enter ItemID and AccessPaths
//	                     and to connect via DCOM (CreateRemoteServer)
//                       Correct some error messages
// 0.90  05/03/97 acc   Add ConnectionPoint Capability (early)
//                       & many additional tests
// 1.0a  08/01/97 acc   add TryBrowse, remove server::enumunknown
// 2.00  09/01/97 acc   add TryParms, finalize connection points, ASYNCIO2
//                      fix bugs in TryItemReadWrite
//       11/10/97 acc   test sync read from both cache and device
//       01/14/98 acc   update per final spec.
//
//                      todo: add tests for NULL pointers and Strings where allowed
//                      to insure they marshall properly.
//

#include <stdio.h>
#include <conio.h>
#include "opccomn.h"	// Include the GENERIC OPC header file
#include "opcda.h"	// Include the GENERIC OPC header file
#include "wcsutil.h"
#include "OLECTL.h"
#include "COMCAT.h"

// %OPC_SEC_MODIFIED%
#include "opcSec.h"     // the standard OPC security interface definition
#include "opcSec_i.c"

// global authorization info structure used by proxies
SEC_WINNT_AUTH_IDENTITY_W* g_pauthIdentity = NULL;
// %END_OPC_SEC_MODIFIED%


//---------------------------------------------------------
// Local Functions

void LocalInit(void);
void LocalCleanup(void);

IUnknown * CreateRemoteOPCServer(WCHAR* szProgID, WCHAR*szNodeName);
IUnknown * CreateInprocOPCServer(WCHAR* szProgID);
IUnknown * CreateLocalOPCServer(WCHAR* szProgID);

void ShowDAList(CATID);
void TryIUnknown(IUnknown * pOPC);
void TryOPCServer(IOPCServer * pOPC);
void TryPersistFile(IPersistFile * pOPC);
void TryGetErrorString(IOPCServer * pOPC);
void TryGetStatus(IOPCServer * pOPC);
void TryAddGroup(IOPCServer * pOPC);
void TryRemoveGroup(IOPCServer * pOPC);
void TryEnumString(IOPCServer * pOPC);
void TryGetByName(IOPCServer * pOPC, LPOLESTR name);
void TryEnumUnknown1(IOPCServer * pOPC);

void TryGSM( IOPCGroupStateMgt * pGRP1 );
extern	OPCHANDLE TryGetGroupState(IOPCGroupStateMgt * pGRP);

void TestGroup(void);

void TryItemFunctions(IOPCGroupStateMgt * pGRP);

void TryItemReadWrite(IOPCSyncIO *pSIO, int nItem, OPCHANDLE *sh);
void TryEnumItemAttributes(IOPCItemMgt * pOPC);

extern void TryIM( IOPCItemMgt * pIM);
extern void TryConnectionPoint(IOPCGroupStateMgt * pGRP, int nItem, OPCHANDLE *sh);	//V2.0
extern void TryDataObject(IOPCGroupStateMgt * pGRP, int nItem, OPCHANDLE *sh);
extern void TryBrowse(IOPCBrowseServerAddressSpace * pBAS);
extern void TryParams(IOPCItemProperties * pPRM);
extern void TryCOMN(IOPCCommon * pPRM);
extern int  TrySvrCPC(IConnectionPointContainer * pPRM);

// %OPC_SEC_MODIFIED%
void TrySecurity();
//void TryGetSecurityInfo(IOPCSecurity *pSec);
void TryChangeNTCredential(IOPCSecurityNT *pSec, DWORD dwMinImpLevel);
void TryPrivateCredential(IOPCSecurityPrivate *pSec);
HRESULT SetProxyBlanket(IUnknown *pUnk, LPCSTR pszPrincipal, LPCSTR pszPassword, DWORD dwMinImpLevel);
void CleanupAuthIdentity(SEC_WINNT_AUTH_IDENTITY_W* pauth);
// %END_OPC_SEC_MODIFIED%

void	report( char*n, HRESULT r);
void	DumpVariant(VARIANT *v);



// Global interface to the COM memory manager
IMalloc *pIMalloc;

// Some Global Strings for use in Group/Item Testing
//
WCHAR	*ItemIDs[2] = {0,0};
WCHAR	*AccessPaths[2] = {0,0};
VARTYPE	dtype[2];
OPCHANDLE g_sh[2];		// Returned Server Handles for the Items
BOOL add_done = 0;


// Global list of available server interfaces
//
IUnknown *gpOPC = 0;
IOPCServer *gpOPCS = 0;
IConnectionPointContainer *gpOPC_CPC = 0;
IOPCCommon *gpOPCCOMN = 0;
IOPCBrowseServerAddressSpace *gpOPCBA = 0;
IOPCItemProperties *gpOPCPRM = 0;
IOPCServerPublicGroups *gpOPCPG = 0;
//IEnumUnknown * gpOPCEU = 0;
IPersistFile * gpOPCPF = 0;
// %OPC_SEC_MODIFIED%
//IOPCSecurity    *gpOPC_Sec = NULL;
IOPCSecurityNT    *gpOPC_SecNT = NULL;
IOPCSecurityPrivate   *gpOPC_SecPrivate = NULL;
// %END_OPC_SEC_MODIFIED%


// Catagory IDs (in opc_cats.c)
//
EXTERN_C const CATID CATID_OPCDAServer10;
EXTERN_C const CATID CATID_OPCDAServer20;

//---------------------------------------------------------
// main
void main(void)
{
	HRESULT r2, r3, r5, r6, r7, r8, r9;
	int loop, sdr;

	printf("Al Chisholm's OPC Client Demo/OPC Custom Server Test Program V2.xx\n");
	printf("Provided by Intellution Inc.\n");

	LocalInit();

	// Check for and show registered Data Access Servers
	//
	printf("DA 1.0 Servers...\n");
	ShowDAList(CATID_OPCDAServer10);
	printf("DA 2.0 Servers...\n");
	ShowDAList(CATID_OPCDAServer20);

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
			r3 = gpOPC->QueryInterface(IID_IPersistFile, (void**)&gpOPCPF);
			r5 = gpOPC->QueryInterface(IID_IOPCServerPublicGroups, (void**)&gpOPCPG);
			r6 = gpOPC->QueryInterface(IID_IOPCBrowseServerAddressSpace, (void**)&gpOPCBA);
			r7 = gpOPC->QueryInterface(IID_IOPCItemProperties, (void**)&gpOPCPRM);
			r8 = gpOPC->QueryInterface(IID_IOPCCommon, (void**)&gpOPCCOMN);
			r9 = gpOPC->QueryInterface(IID_IConnectionPointContainer, (void**)&gpOPC_CPC);
// %OPC_SEC_MODIFIED%
            // check if server supports extended security!
			HRESULT r10;
            r10 = gpOPC->QueryInterface(IID_IOPCSecurityNT, (void**)&gpOPC_SecNT);
			HRESULT r11;
            r11 = gpOPC->QueryInterface(IID_IOPCSecurityPrivate, (void**)&gpOPC_SecPrivate);
// %END_OPC_SEC_MODIFIED%

			report("IUnknown.................... ", S_OK);
			report("IOPCServer.................. ", r2);
			report("IPersistFile................ ", r3);
			report("IOPCServerPublicGroups...... ", r5);
			report("IOPCBrowseServerAddressSpace ", r6);
			report("IOPCItemProperties.......... ", r7);
			report("IOPCCommon.................. ", r8);
			report("IConnectionPointContainer... ", r9);
// %OPC_SEC_MODIFIED%
			report("IOPCSecurityNT................ ", r10);
			report("IOPCSecurityPrivate................ ", r11);
// %END_OPC_SEC_MODIFIED%

			printf("\n");
		}

		loop = 1;
		if(gpOPC) while(loop)
		{

			// Try using the custom IOPCServer interface if present
			// and also the IDispatch interface if present
			printf("\nTest Server Interfaces...\n");
			printf("0= IUnknown...\n");
			printf("1= OPCServer...\n");
			printf("2= PersistFile...\n");
//			printf("3= IEnumUnknown...\n");
			printf("4= OPCServerPublicGroups...\n");
			printf("5= OPCBrowseServerAddressSpace...\n");
			printf("6= OPCItemProperties...\n");
			printf("7= OPCCommon...\n");
			printf("8= ConnectionPointContianer...\n");
// %OPC_SEC_MODIFIED%
			printf("s= OPCSecurity...\n");
// %END_OPC_SEC_MODIFIED%
			printf("x= Exit...\n\n");

			pptr = gets(buffer);
			c = *pptr;
			switch(c)
			{
			case '0':
				if(gpOPC)TryIUnknown( gpOPC );
				break;
			case '1':
				if(gpOPCS)TryOPCServer( gpOPCS );
				break;
			case '2':
				if(gpOPCPF)TryPersistFile( gpOPCPF );
				break;
//			case '3':

			case '4':
				printf("This test not yet implemented\n");
				break;
			case '5':
				if(gpOPCBA)TryBrowse( gpOPCBA);
				break;
			case '6':
				if(gpOPCPRM)TryParams( gpOPCPRM);
				break;
			case '7':
				if(gpOPCCOMN)TryCOMN( gpOPCCOMN);
				break;
			case '8':
				if(gpOPC_CPC) sdr = TrySvrCPC( gpOPC_CPC);
				if(sdr) 
				{
					// If shutdown request - remove groups and release server
					// Sample server will wait up to 10 seconds for this then exit anyway.
					// This 'getch' allows us to test this feature.
					//
					printf("Hit Enter to Remove Groups and release all interfaces\n");
					getch();
					TryRemoveGroup(gpOPCS);
					loop = 0;
				}
				break;
// %OPC_SEC_MODIFIED%
			case 's':
				TrySecurity();
				break;
// %END_OPC_SEC_MODIFIED%
			case 'x':
				loop = 0;
				break;
			}
		}

		// Free the interfaces
		//
		if(gpOPC) gpOPC->Release();
		if(gpOPCS) gpOPCS->Release();
		if(gpOPCPF) gpOPCPF->Release();
		if(gpOPCBA) gpOPCBA->Release();
		if(gpOPCPRM) gpOPCPRM->Release();
		if(gpOPCPG) gpOPCPG->Release();
		if(gpOPCCOMN) gpOPCCOMN->Release();
		if(gpOPC_CPC) gpOPC_CPC->Release();
// %OPC_SEC_MODIFIED%
		if(gpOPC_SecNT) gpOPC_SecNT->Release();
        if (g_pauthIdentity != NULL) {
            CleanupAuthIdentity(g_pauthIdentity);
            g_pauthIdentity = NULL;
        }
// %END_OPC_SEC_MODIFIED%


		WSTRFree(progid, 0);
		progid = 0;
		WSTRFree(node, 0);
		node = 0;

		WSTRFree(ItemIDs[0], 0);
		ItemIDs[0] = 0;
		WSTRFree(ItemIDs[1], 0);
		ItemIDs[1] = 0;

		WSTRFree(AccessPaths[0], 0);
		AccessPaths[0] = 0;
		WSTRFree(AccessPaths[1], 0);
		AccessPaths[1] = 0;
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

	// This is for DCOM
	//
	r1 = CoInitializeSecurity(
			NULL,   //Points to security descriptor 
			-1,     //Count of entries in asAuthSvc 
			NULL,   //Array of names to register 
			NULL,   //Reserved for future use 
			RPC_C_AUTHN_LEVEL_CONNECT,    //The default authentication level for proxies
			RPC_C_IMP_LEVEL_IMPERSONATE,//The default impersonation level for proxies
			NULL,                      //Reserved; must be set to  NULL 
			EOAC_NONE,                 //Additional client or server-side capabilities
			NULL                       //Reserved for future use 
			);
	if (FAILED(r1))
	{
		printf("Error from CoInitializeSecurity: %lx\n", r1);
//		CoUninitialize();
//		exit(1);
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

	// rai
	// Get the CLSID from the Name
	// zzz NOTE this is a call to the LOCAL registry!
	//
    printf("trying local registry lookup for CLSID...!\n");
	r1 = CLSIDFromProgID(szProgID, &clsid);
	if(FAILED(r1))
	{
        printf("local registry lookup for CLSID failed, trying remote...!\n");
        //added rai (version doesn't need local registry)
        HKEY					hCLSID;
        HKEY					hkRoot = HKEY_CLASSES_ROOT;
        const int				MAX_CLSID_KEY_LEN = 256;
        LONG					clsid_size = MAX_CLSID_KEY_LEN;
        TCHAR					clsidString[MAX_CLSID_KEY_LEN];
        HRESULT					hr = S_OK;
        
        CHAR* spProgID = SBCSFromWSTR(szProgID,0);
        
        if(*szNodeName)
        {
            DWORD dwR = RegOpenKey(hkRoot, (LPTSTR)(LPCTSTR)spProgID, &hCLSID );
            if( dwR != ERROR_SUCCESS )
            {		
                CHAR* spNode = SBCSFromWSTR(szNodeName,0);
                DWORD dwR = RegConnectRegistry ((LPTSTR)(LPCTSTR)spNode, HKEY_CLASSES_ROOT, &hkRoot);
                SBCSFree(spNode, 0);
                if( dwR != ERROR_SUCCESS ) {
                    printf("Error connecting to remote registry: (0x%08X)\n", HRESULT_FROM_WIN32(dwR));
                    return FALSE;
                }
            }
        }
        
        DWORD dwR = RegOpenKey(hkRoot, (LPTSTR)(LPCTSTR)spProgID, &hCLSID );
        if( dwR != ERROR_SUCCESS ) {
            printf("Error opening ProgID on remote registry: (0x%08X)\n", HRESULT_FROM_WIN32(dwR));
            return FALSE;
        }
        SBCSFree(spProgID, 0);
        
        dwR = RegQueryValue(hCLSID, "CLSID", clsidString, &clsid_size );
        if( dwR != ERROR_SUCCESS ) {
            printf("Error opening ProgID on remote registry: (0x%08X)\n", HRESULT_FROM_WIN32(dwR));
            return FALSE;
        }
        
        WCHAR *ws_clsidString = 0;
        ws_clsidString = WSTRFromSBCS(clsidString, 0);
        
        hr = CLSIDFromString(ws_clsidString, &clsid);
        WSTRFree(ws_clsidString,0);
        if( FAILED(hr))
        {
            return FALSE;
        }
        // end change of rai
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
// TryIUnknown
//
void TryIUnknown(IUnknown *pOPC)
{
	HRESULT r4;

	// Try basic interface to see if it works at all...
	//
	r4 = pOPC->AddRef();
	if (FAILED(r4))
	{
		printf("Error from IUnknown AddRef(%lx)\n", r4);
		return;
	}
	pOPC->Release();
	printf("IUnknown test complete\n\n");
}


//---------------------------------------------------------
// TryOPCServer
// Use the OPCServer to perform some functions
void TryOPCServer(IOPCServer *pOPC)
{
	int loop;
	char buffer[40];

	loop = 1;
	while(loop)
	{

		printf("\n\nTest OPCServer Methods...\n");
		printf("0= AddGroup...\n");
		printf("1= GetErrorString...\n");
		printf("2= GetGroupByName...\n");
		printf("3= GetStatus...\n");
		printf("4= RemoveGroup...\n");
		printf("5= CreateGroupEnumerator(Unknown and String)...\n");
		printf("9= *Test the Group Interfaces*...\n");
		printf("x= Exit...\n\n");

		switch(*gets(buffer))
		{
		case '0':
			TryAddGroup(pOPC);
			break;
		case '1':
			TryGetErrorString(pOPC);
			break;
		case '2':
			//Note - Group1 will fail if you run the SetGroupName test!
			TryGetByName(pOPC, L"TestGroup1");	
			TryGetByName(pOPC, L"TestGroup2");
			break;
		case '3':
			TryGetStatus(pOPC);
			break;
		case '4':
			TryRemoveGroup(pOPC);
			break;
		case '5':
			TryEnumUnknown1(pOPC);
			TryEnumString(pOPC);
			break;

		case '9':
			TestGroup();
			break;

		case 'x':
			loop = 0;
			break;
		}
	}
	printf("IOPCServer test complete\n\n");
}


//---------------------------------------------------------
// TryGetStatus
// Use the OPCServer to perform some functions
void TryGetStatus(IOPCServer * pOPC)
{
	OPCSERVERSTATUS	*pss;
	HRESULT r1;

	// Invoke a method on the OPCServer interface
	//
	r1 = pOPC->GetStatus(&pss);

	if (FAILED(r1))
	{
		printf("Error from GetStatus(%lx)\n", r1);
	} 
	else
	{
		// And print some of the results of the method
		//
		printf("Status.szVendorInfo = %ls\n", pss->szVendorInfo);

		// Dont forget to release the memory returned by the method
		//
		pIMalloc->Free(pss->szVendorInfo);
		pIMalloc->Free(pss);
	}
}



//---------------------------------------------------------
// TryGetErrorString
// Use the OPCServer to perform some functions
void TryGetErrorString(IOPCServer * pOPC)
{
	HRESULT r1;
	LPWSTR s = 0;

	// Invoke a method on the OPCServer interface
	// zzz later ask user for error code
	//
	r1 = pOPC->GetErrorString(0, 0, &s);

	if (FAILED(r1))
	{
		printf("Error from GetErrorString(%lx)\n", r1);
	} 
	else
	{
		// And print some of the results of the method
		//
		printf("GetErrorString returned \"%ls\"\n", s);

		// Dont forget to release the memory returned by the method
		//
		pIMalloc->Free(s);
	}
}



// Globals for Group interfaces and handles
//
DWORD	hServerGroup1;
IUnknown          * pGRP1U = 0;
IOPCGroupStateMgt * pGRP1GSM = 0;
IOPCPublicGroupStateMgt * pGRP1PGSM = 0;
IOPCSyncIO        * pGRP1SIO = 0;
IOPCAsyncIO       * pGRP1ASIO = 0;
IOPCAsyncIO2      * pGRP1ASIO2 = 0;
IOPCItemMgt       * pGRP1IM = 0;
IDataObject       * pGRP1DO = 0;
IConnectionPointContainer * pGRP1CPC = 0;

DWORD	hServerGroup2;
IOPCGroupStateMgt * pGRP2 = 0;

DWORD	hServerGroup3;		// the cloned group
int bGroup3 = 0;		// do we need to remove it?


//---------------------------------------------------------
// TryAddGroup
// Use the OPCServer to add some groups
// and then try out the interfaces on the groups
//
void TryAddGroup(IOPCServer * pOPC)
{
	HRESULT r1;
	DWORD	RevisedRate1;
	DWORD	RevisedRate2;
	float	DeadBand1 = (float)0.0;
	float	DeadBand2 = (float)0.0;
	HRESULT r2, r3, r4, r5, r6, r8;
	HRESULT r9, r5a;

	// Try to create 2 groups (zzz later ask user for names)
	//
	if(pGRP1U == 0)
	{
		printf("First time through: Creating groups...\n");

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
			r3 = pGRP1U->QueryInterface(IID_IOPCPublicGroupStateMgt, (void**)&pGRP1PGSM);
			r4 = pGRP1U->QueryInterface(IID_IOPCSyncIO, (void**)&pGRP1SIO);
			r5 = pGRP1U->QueryInterface(IID_IOPCAsyncIO, (void**)&pGRP1ASIO);
			r5a = pGRP1U->QueryInterface(IID_IOPCAsyncIO2, (void**)&pGRP1ASIO2);
			r6 = pGRP1U->QueryInterface(IID_IOPCItemMgt, (void**)&pGRP1IM);
			r8 = pGRP1U->QueryInterface(IID_IDataObject, (void**)&pGRP1DO);
			r9 = pGRP1U->QueryInterface(IID_IConnectionPointContainer, (void**)&pGRP1CPC);

			report("IUnknown               ", S_OK);
			report("IOPCGroupStateMgt      ", r2);
			report("IOPCPublicGroupStateMgt", r3);
			report("IOPCSyncIO             ", r4);
			report("IOPCAsyncIO            ", r5);
			report("IOPCAsyncIO2           ", r5a);
			report("IOPCItemMgt            ", r6);
			report("IDataObject            ", r8);
			report("IConnectionPointContainer", r9);
			printf("\n\n");
		}
	}
	else
	{
		printf("AddGroup already done...\n");
		return;
	}

	if( pGRP2 == 0)
	{
		r2 = pOPC->AddGroup( L"TestGroup2", TRUE, 0, 0, 0, &DeadBand2,
			0, &hServerGroup2, &RevisedRate2, IID_IOPCGroupStateMgt, 
			(LPUNKNOWN*)&pGRP2);

		if (FAILED(r2))
		{
			printf("Error from AddGroup (TestGroup2) (%lx)\n", r2);
		} 
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
		printf("0= GroupStateMgt...\n");
		printf("1= ItemMgt (Add some items)...\n");
		printf("2= SyncIO...\n");
		printf("3= AsyncIO, AsyncIO2...\n");
		printf("4= PublicGroupStateMgt...\n");
		printf("5= DataObject...\n");
		printf("6= ConnectionPoint...\n");
		printf("x= Exit...\n\n");

		switch(*gets(buffer))
		{
		case '0':
			if(pGRP1GSM) TryGSM( pGRP1GSM );
			break;
		case '1':
			if(pGRP1IM) TryIM( pGRP1IM );	// (Add items)
			break;
		case '2':
			if(pGRP1SIO) TryItemReadWrite(pGRP1SIO, 2, g_sh);
//			if(pGRP1SIO) TrySIO( pGRP1SIO );
			break;
		case '3':
			printf("This Interface is tested along with DataObject or ConnectionPoints\n");
//			if(pGRP1ASIO) TryASIO( pGRP1ASIO ); // need dataobj - maybe combine with trydo/trycpc
			break;
		case '4':
			printf("This test not yet implemented\n");
			break;
		case '5':
			if( pGRP1GSM ) TryDataObject(pGRP1GSM, 2, g_sh);
//			if( pGRP1DO ) TryDO( pGRP1DO );
			break;
		case '6':
			if( pGRP1GSM ) TryConnectionPoint(pGRP1GSM, 2, g_sh);	// Try ConnectionPoint for 1st group V2.0
//			if( pGRP1CPC ) TryCPC( pGRP1CPC );
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
	if(pGRP1PGSM) pGRP1PGSM->Release();
	if(pGRP1SIO) pGRP1SIO->Release();
	if(pGRP1ASIO) pGRP1ASIO->Release();
	if(pGRP1ASIO2) pGRP1ASIO2->Release();
	if(pGRP1IM) pGRP1IM->Release();
	if(pGRP1DO) pGRP1DO->Release();
	if(pGRP1CPC) pGRP1CPC->Release();

	pGRP1GSM = 0;
	pGRP1PGSM = 0;
	pGRP1SIO = 0;
	pGRP1ASIO = 0;
	pGRP1ASIO2 = 0;
	pGRP1IM = 0;
	pGRP1DO = 0;
	pGRP1CPC = 0;

	if(pGRP1U)
	{
		pGRP1U->Release();
		r1 = pOPC->RemoveGroup(hServerGroup1, FALSE);
		if (FAILED(r1))
		{
			printf("Error from RemoveGroup (TestGroup1) (%lx)\n", r1);
		} else printf("Group 1 removed\n"); 
		pGRP1U = 0;
	}
	if(pGRP2)
	{
		pGRP2->Release();
		r1 = pOPC->RemoveGroup(hServerGroup2, FALSE);
		if (FAILED(r1))
		{
			printf("Error from RemoveGroup (TestGroup2) (%lx)\n", r1);
		} else printf("Group 2 (empty) removed\n"); 
		pGRP2 = 0;
	}

	if(bGroup3)
	{
		r1 = pOPC->RemoveGroup(hServerGroup3, FALSE);	// the 'cloned' group
		if (FAILED(r1))
		{
			printf("Error from RemoveGroup (TestGroup3 - the cloned group) (%lx)\n", r1);
		} else printf("Group 3 (clone) removed\n"); 
		bGroup3 = 0;
	}


	add_done = 0;	// clear the 'items added' flag.
	printf("Remove Groups test complete\n");
}



//---------------------------------------------------------
// TryEnumUnknown1
// 
void TryEnumUnknown1(IOPCServer * pOPC)
{
	HRESULT r1;
	IEnumUnknown * pEnum;

	r1 = pOPC->CreateGroupEnumerator(OPC_ENUM_ALL, IID_IEnumUnknown, (IUnknown**)&pEnum);
	if (FAILED(r1))
	{
		printf("Error from CreateGroupEnumerator for EnumUnknown (%lx)\n", r1);
	} 
	else
	{
		HRESULT r2 = S_OK;
		IUnknown *pUnk;
		IOPCGroupStateMgt * pGRP;
		ULONG actual;

		printf("Got an IEnumUnknown from CreateGroupEnumerator()...\n");

		while((r2 = pEnum->Next(1, &pUnk, &actual)) == S_OK)
		{
			HRESULT r3;

			r3 = pUnk->QueryInterface(IID_IOPCGroupStateMgt, (void**)&pGRP);
			
			if (FAILED(r3))
			{
				printf("Error from QI after Next(%lx)\n", r3);
			}
			else
			{
				TryGetGroupState(pGRP);
				pGRP->Release();
			}

			pUnk->Release();
		}
		if (FAILED(r2))
		{
			printf("Error from Next(%lx)\n", r2);
		}

		// Release the interface
		//
		pEnum->Release();
	}
}



//---------------------------------------------------------
// TryEnumString
// 
void TryEnumString(IOPCServer * pOPC)
{
	HRESULT r1;
	IEnumString * pEnum;

	r1 = pOPC->CreateGroupEnumerator(OPC_ENUM_ALL, IID_IEnumString, (IUnknown **)&pEnum);
	if (FAILED(r1))
	{
		printf("Error from CreateGroupEnumerator for EnumString (%lx)\n", r1);
	} 
	else
	{
		HRESULT r2 = S_OK;
		LPOLESTR pStr;
		ULONG actual;

		printf("Got an IEnumString...\n");

		while((r2 = pEnum->Next(1, &pStr, &actual)) == S_OK)
		{
			printf("EnumString returns: %ls\n", pStr);

			// Free the string(s) that Next Returned
			//
			pIMalloc->Free(pStr);
		}
		if (FAILED(r2))
		{
			printf("Error from Next(%lx)\n", r2);
		}
		
		// Release the interface
		//
		pEnum->Release();
	}
}


//---------------------------------------------------------
// TryItemReadWrite
// 
void TryItemReadWrite(IOPCSyncIO *pSIO, int nItem, OPCHANDLE *sh)
{
	HRESULT	r1;
	HRESULT *hr;
	VARIANT	v[2];
	OPCITEMSTATE *is;
	char buffer[40];

	if(!add_done)
	{
		printf("Must do Add first\n");
		return;
	}

	// try to write and then read each item
	//

	v[0].vt = VT_R8;
	v[0].dblVal = 1.11;
	v[1].vt = VT_R8;
	v[1].dblVal = 2.22;

	printf("Do you want to try ItemWrite[y/n]\n");
	switch(*gets(buffer))
	{
	case 'y':
	case 'Y':
		printf("We will write VT_R8 / 1.11 and 2.22 to the 2 items\n");
		r1 = pSIO->Write(2, sh, v, &hr);
		if (FAILED(r1))
		{
			printf("Error from Write(%lx)\n", r1);
		} else
		{
			printf("HResult[0] = %lx)\n", hr[0]);
			printf("HResult[1] = %lx)\n", hr[1]);
			pIMalloc->Free(hr);		//acc001
		}
		break;
	}
	

	printf("Read from CACHE:\n");
	r1 = pSIO->Read(OPC_DS_CACHE, 2, sh, &is, &hr);
	if (FAILED(r1))
	{
		printf("Error from Read(%lx)\n", r1);
	} 
	else
	{
		// if the read worked then show the results
		// (Which are assumed to be VT_R4 for now!)
		//

		printf("Read[0] hr=%lx, Quality = %02x Value=", hr[0], (BYTE) is[0].wQuality); //zzz add DumpQuality()
		if(!FAILED(hr[0])) DumpVariant(&is[0].vDataValue);
		printf("\n");

		printf("Read[1] hr=%lx, Quality = %02x Value=", hr[1], (BYTE) is[1].wQuality);
		if(!FAILED(hr[1])) DumpVariant(&is[1].vDataValue);
		printf("\n");

		// Free returned items
		//
		pIMalloc->Free(hr);
		VariantClear(&is[0].vDataValue);	//acc0997
		VariantClear(&is[1].vDataValue);
		pIMalloc->Free(is);
	}


	printf("Read from DEVICE:\n");
	r1 = pSIO->Read(OPC_DS_DEVICE, 2, sh, &is, &hr);
	if (FAILED(r1))
	{
		printf("Error from Read(%lx)\n", r1);
	} 
	else
	{
		// if the read worked then show the results
		// (Which are assumed to be VT_R4 for now!)
		//

		printf("Read[0] hr=%lx, Quality = %02x Value=", hr[0], (BYTE) is[0].wQuality); //zzz add DumpQuality()
		if(!FAILED(hr[0])) DumpVariant(&is[0].vDataValue);
		printf("\n");

		printf("Read[1] hr=%lx, Quality = %02x Value=", hr[1], (BYTE) is[1].wQuality);
		if(!FAILED(hr[1])) DumpVariant(&is[1].vDataValue);
		printf("\n");

		// Free returned items
		//
		pIMalloc->Free(hr);
		VariantClear(&is[0].vDataValue);	//acc0997
		VariantClear(&is[1].vDataValue);
		pIMalloc->Free(is);
	}
}


//---------------------------------------------------------
// TryEnumItemAttributes
// 
void TryEnumItemAttributes(IOPCItemMgt * pOPC)
{
	HRESULT r1;
	IEnumOPCItemAttributes * pEnum;

	r1 = pOPC->CreateEnumerator(IID_IEnumOPCItemAttributes, (IUnknown **)&pEnum);
	if (FAILED(r1))
	{
		printf("Error from CreateEnumerator for EnumItemAttributes (%lx)\n", r1);
	} 
	else
	{
		HRESULT r2 = S_OK;
		OPCITEMATTRIBUTES *pIA;
		ULONG actual;

		printf("Got an IEnumItemAttr...\n");

		while((r2 = pEnum->Next(1, &pIA, &actual)) == S_OK)
		{
			printf("EnumItemAttr returns ItemID:%ls, hServer=%d hClient=%d\n", 
				pIA->szItemID, 
				pIA->hServer, 
				pIA->hClient);
			// zzz later print out other stuff...

			// Free all the stuff inside the ITEMATTR
			// that Next Returned
			//
			if(pIA->szAccessPath) pIMalloc->Free(pIA->szAccessPath);
			if(pIA->szItemID) pIMalloc->Free(pIA->szItemID);
			if(pIA->pBlob) pIMalloc->Free(pIA->pBlob);
			VariantClear(&pIA->vEUInfo);

			// And finally, free the ITEMATTR itself
			//
			pIMalloc->Free(pIA);
			
		}
		if (FAILED(r2))
		{
			printf("Error from Next(%lx)\n", r2);
		}
 
		// Release the enumerator
		//
		pEnum->Release();
	}
}



//---------------------------------------------------------
// TryPersistFile
// Use the OPCServer to perform some functions just to insure
// that the interface pointer really works.
//
void TryPersistFile(IPersistFile* pOPC)
{
	HRESULT r4;
	CLSID	cls;

	// Request an IPersistFile interface from the object.
	//
	
	// Try out a couple of methods on the interface
	//
	r4 = pOPC->GetClassID(&cls);
	if (FAILED(r4))
	{
		printf("Error from GetClassID (%lx)\n", r4);
	} else
	{
		printf("GetClassID worked\n");
	}
	printf("IPersistFile test complete\n\n");
}


// %OPC_SEC_MODIFIED%
//---------------------------------------------------------
void TrySecurity()
{
	int loop;
	char buffer[40];
    HRESULT hr;
    DWORD dwMinImpLevel = 0;

    printf("\n\nCurrent availability of OPCSecurity credential interfaces...\n");
    BOOL bIsAvailable;
    if (gpOPC_SecNT)
    {
        hr = gpOPC_SecNT->IsAvailableNT(&bIsAvailable);
        if (FAILED(hr)) {
		    printf("Error from IsAvailableNT (%lx)\n", hr);
            return;
        }
        printf("IsAvailableNT returned %s\n", bIsAvailable ? "YES" : "NO");
        hr = gpOPC_SecNT->QueryMinImpersonationLevel(&dwMinImpLevel);
        if (FAILED(hr)) {
		    printf("Error from QueryMinImpersonationLevel (%lx)\n", hr);
            return;
        }
        LPCSTR pszLevel;
        switch(dwMinImpLevel) {
//        case RPC_C_IMP_LEVEL_DEFAULT:       pszLevel = "Default"; break;
        case RPC_C_IMP_LEVEL_ANONYMOUS:     pszLevel = "Anonymous"; break;
        case RPC_C_IMP_LEVEL_IDENTIFY:      pszLevel = "Identify"; break;
        case RPC_C_IMP_LEVEL_IMPERSONATE:   pszLevel = "Impersonate"; break;
        case RPC_C_IMP_LEVEL_DELEGATE:      pszLevel = "Delegate"; break;
        default:                            pszLevel = ">Unknown<"; break;
        }
        printf("minimal required impersonation level is %s\n", pszLevel);
 
    }
    if (gpOPC_SecPrivate)
    {
        hr = gpOPC_SecPrivate->IsAvailablePriv(&bIsAvailable);
        if (FAILED(hr)) {
		    printf("Error from IsAvailablePriv (%lx)\n", hr);
            return;
        }
        printf("IsAvailablePriv returned %s\n", bIsAvailable ? "YES" : "NO");
    }

	loop = 1;
	while(loop)
	{

		printf("\n\nTest OPCSecurity Methods...\n");
		//printf("0= GetSecurityTypeInfo...\n");
		printf("1= Change NT Credential...\n");
		printf("2= Change Private Credential...\n");
		printf("x= Exit...\n\n");

		switch(*gets(buffer))
		{
		//case '0':
		//	TryGetSecurityInfo(pSec);
		//	break;
		case '1':
			TryChangeNTCredential(gpOPC_SecNT, dwMinImpLevel);
			break;
		case '2':
			TryPrivateCredential(gpOPC_SecPrivate);
			break;
		case 'x':
			loop = 0;
			break;
		}
	}
	printf("Security test complete\n\n");
}

//---------------------------------------------------------
void TryPrivateCredential(IOPCSecurityPrivate *pSec)
{
	int loop;
	char buffer[40];
	HRESULT hr;	
	char szPrincipal[255], szPassword[50];

	loop = 1;
	while(loop)
	{

		printf("\n\nTest OPCSecurityPrivate Methods...\n");
		printf("0= Logon...\n");
		printf("1= Logoff...\n");
		printf("x= Exit...\n\n");

		switch(*gets(buffer))
		{
		case '0':
			printf("Enter user ID and password:\n");
			printf("user ID \n");
			gets(szPrincipal);
			printf("password; BEWARE: password will show in cleartext while typing!!!!!\n");
			gets(szPassword);

			LPWSTR  pszUser, pszPassword;
			
			pszUser = WSTRFromSBCS(szPrincipal, 0);
			pszPassword = WSTRFromSBCS(szPassword, 0);

			hr = pSec->Logon(pszUser, pszPassword);
			if (FAILED(hr)) {
				printf("Error from Logon (%lx)\n", hr);
			}
			break;
		case '1':
			hr = pSec->Logoff();
			if (FAILED(hr)) {
				printf("Error from Logoff (%lx)\n", hr);
			}
			break;
		case 'x':
			loop = 0;
			break;
		}
	}
	printf("IOPCSecurityPrivate test complete\n\n");
}
//---------------------------------------------------------
/*
void TryGetSecurityInfo(IOPCSecurity *pSec)
{
    HRESULT hr;
    OPC_SECURITY_TYPE   type;
    hr = pSec->GetSecurityInfo(&type);

	if (FAILED(hr)) {
		printf("Error from GetSecurityInfo (%lx)\n", hr);
	} else
	{
		printf("GetSecurityInfo returned type:\n* ");
        switch(type) {
        case OPC_SECTYPE_NO_EXT_SECURITY:       printf("no extended security is enabled or implemented"); break;
        case OPC_SECTYPE_NT_CREDENTIAL:         printf("server implements access control using NT credentials"); break;
        case OPC_SECTYPE_PRIVATE_CREDENTIAL:    printf("server implements access control using vendor specific user credentials"); break;
        case OPC_SECTYPE_NT_AND_PRIVATE_CREDENTIAL: printf("server implements both NT and private credentials"); break;
        default:                                printf("!! Unknown security type info !!"); break;
        }
        printf("\n");
	}
	printf("GetSecurityInfo test complete\n\n");
}
*/

//---------------------------------------------------------
void TryChangeNTCredential(IOPCSecurityNT *pSec, DWORD dwMinImpLevel)
{
    HRESULT hr;
	char szPrincipal[255], szPassword[50];
	printf("Enter user ID and password:\n");

	printf("user ID (Format: [domain_name\\]userID)\n");
	gets(szPrincipal);
	
    printf("password; BEWARE: password will show in cleartext while typing!!!!!\n");
	gets(szPassword);

    if (pGRP1SIO == NULL) {
        printf("Must add groups and items first!\n");
        return;
    }
    hr = SetProxyBlanket(pSec, szPrincipal, szPassword, dwMinImpLevel);
    if (FAILED(hr)) {
		printf("Error from SetProxyBlanket (%lx)\n", hr);
        return;
	}

	hr = pSec->ChangeUser();
	if (FAILED(hr)) {
		printf("Error from ChangeUser (%lx)\n", hr);
        return;
	}

	printf("Change NT Credential complete\n\n");
}

//---------------------------------------------------------
HRESULT SetProxyBlanket(IUnknown *pUnk, LPCSTR pszPrincipal, LPCSTR pszPassword, DWORD dwMinImpLevel)
{
    IClientSecurity* pcs = 0;
    HRESULT hr = pUnk->QueryInterface(IID_IClientSecurity, (void**)&pcs);
    if (SUCCEEDED(hr))
    {
	    DWORD nAuthnLevel , nAuthnSvc, nAuthzSvc, nImpLevel, grfCap;
        RPC_AUTH_IDENTITY_HANDLE    hAuth = NULL;
	    OLECHAR* pszServerPrincipal;
	    hr = pcs->QueryBlanket(pUnk, &nAuthnSvc, &nAuthzSvc, &pszServerPrincipal,		                   &nAuthnLevel, NULL /*&nImpLevel*/, &hAuth, &grfCap );
        if (FAILED(hr)) return hr;

        LPWSTR  pszBuffer, pszPass;
        LPWSTR  pszDomain;
        LPWSTR  pszUser;
        // split principal into domain and user name:
        pszBuffer = WSTRFromSBCS(pszPrincipal, 0);
        pszPass = WSTRFromSBCS(pszPassword, 0);
        if (wcschr(pszBuffer, L'\\') != NULL)
        {
            LPWSTR psz = wcstok(pszBuffer, L"\\");
            pszDomain = WSTRClone(psz, NULL);
            psz = wcstok(NULL, L"\\");
            pszUser = WSTRClone(psz, NULL);
        }
        else {
            pszDomain = WSTRClone(L"", NULL);
            pszUser = WSTRClone(pszBuffer, NULL);
        }
        WSTRFree(pszBuffer, 0);

        // using NTLMSSP, e.g. WinNT authorization
        nAuthnSvc = RPC_C_AUTHN_WINNT;
		nImpLevel = dwMinImpLevel;
        if (g_pauthIdentity != NULL)
        {
            // first reset identity...
            hr = pcs->SetBlanket(pUnk, nAuthnSvc, nAuthzSvc, pszServerPrincipal,
		        nAuthnLevel, nImpLevel, NULL, grfCap );
            // .. now clean up authorization info memory
            CleanupAuthIdentity(g_pauthIdentity);
            g_pauthIdentity = NULL;
        }
        g_pauthIdentity = new SEC_WINNT_AUTH_IDENTITY_W;
        // pauthIdentity must remain allocated until we set a different (or NULL) identity
        // to proxy (see also documentation to CoSetProxyBlanket() )
        if (g_pauthIdentity == NULL) return E_OUTOFMEMORY;
	    g_pauthIdentity->Domain = pszDomain;
	    g_pauthIdentity->User = pszUser;
	    g_pauthIdentity->Password = pszPass;// Clear text password
	    g_pauthIdentity->DomainLength = wcslen(pszDomain);
	    g_pauthIdentity->UserLength = wcslen(pszUser);
	    g_pauthIdentity->PasswordLength = wcslen(pszPass);
	    g_pauthIdentity->Flags = SEC_WINNT_AUTH_IDENTITY_UNICODE;

        hr = pcs->SetBlanket(pUnk, nAuthnSvc, nAuthzSvc, pszServerPrincipal,
		    nAuthnLevel, nImpLevel, g_pauthIdentity, grfCap );
		pcs->Release();
        if (pszServerPrincipal != NULL) CoTaskMemFree(pszServerPrincipal);
    }
    return hr;
}

void CleanupAuthIdentity(SEC_WINNT_AUTH_IDENTITY_W* pauth)
{
    if (pauth->Domain != NULL)      WSTRFree(pauth->Domain, NULL);
    if (pauth->User != NULL)        WSTRFree(pauth->User, NULL);
    if (pauth->Password != NULL)    WSTRFree(pauth->Password, NULL);
    delete pauth;
}
// %END_OPC_SEC_MODIFIED%



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



//---------------------------------------------------------
// DumpVariant
// This shows the value of a Variant
void	DumpVariant(VARIANT *v)
{
	switch(v->vt)
	{
	case VT_I2:
		printf(" value(VT_I2)   = %d ", v->iVal );
		break;
	case VT_I4:
		printf(" value(VT_I4)   = %ld ", v->lVal );
		break;
	case VT_R4:
		printf(" value(VT_R4)   = %5.2f ", v->fltVal );
		break;
	case VT_R8:
		printf(" value(VT_R8)   = %9.4f ", v->dblVal );
		break;
	case VT_BSTR:
		printf(" value(VT_BSTR) = %ls ", v->bstrVal );
		break;
	default:
		printf(" value(unknown type:%d) ", v->vt );
		break;
	}
}
