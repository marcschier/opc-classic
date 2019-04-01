// opc_data.cpp - IDataObject test code
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
// 0.90  04/03/97 acc   
//       05/20/97 ACC   HandleWrite should use GROUPHEADERWRITE, not GROUPHEADER
//       01/14/98 acc   update per final spec
//
//

#include <stdio.h>
#include <conio.h>
#include "opcda.h"
#include "OLECTL.h"

#include "OPC_PACK.h"


void HandleData(STGMEDIUM *pSTM);
void HandleDataTime(STGMEDIUM *pSTM);
void HandleWrite(STGMEDIUM *pSTM);


// Define an actual implementation of the IAdviseSink interface
//
class IXXXAS : public IAdviseSink
{
public:
	IXXXAS( void );
	~IXXXAS( void );

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// Member Functions 
	STDMETHODIMP_(void) OnDataChange(
	 FORMATETC *pFmt,
	 STGMEDIUM *pStg
	);

	STDMETHODIMP_(void) OnViewChange(
	 DWORD dwA,
	 LONG lindex
	);

	STDMETHODIMP_(void) OnRename(IMoniker* m);

	STDMETHODIMP_(void) OnSave(void);

	STDMETHODIMP_(void) OnClose(void);

private:
	DWORD mRefCount;
	UINT m_data;
	UINT m_datatime;
	UINT m_write;
};

///////////////////////////////////////
// The first section of this file contains the 
// Client specific impementation of
// his IOPCCallback Methods
//
///////////////////////////////////////
//
///////////////////////////////////////
IXXXAS:: IXXXAS()
{
	mRefCount = 0;

	m_data = RegisterClipboardFormat("OPCSTMFORMATDATA");
	m_datatime = RegisterClipboardFormat("OPCSTMFORMATDATATIME");
	m_write = RegisterClipboardFormat("OPCSTMFORMATWRITECOMPLETE");
}

///////////////////////////////////////
//
///////////////////////////////////////
IXXXAS:: ~IXXXAS()
{
}


///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP_(ULONG) IXXXAS:: AddRef( void)
{
	return ++mRefCount;
}



///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP_(ULONG) IXXXAS:: Release( void)
{
	ULONG currentCount = --mRefCount; 

	// If no references left for this object
	if ( currentCount == 0)
	{
		// Then delete this group.
		delete this;
	}
	return currentCount;
}


///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP IXXXAS:: QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	if ( ppInterface == NULL)
		return E_INVALIDARG;

	if ( iid == IID_IUnknown )
		*ppInterface = (IUnknown*) this;

	else if ( iid == IID_IAdviseSink)
	{
		*ppInterface = (IUnknown*) this;
	}
	else
	{
		*ppInterface = NULL;
	}
	
	if ( *ppInterface == NULL)
		return E_NOINTERFACE;

	AddRef();
	return S_OK;
}


///////////////////////////////////////
//
///////////////////////////////////////
void IXXXAS::OnDataChange(LPFORMATETC pFE
	, LPSTGMEDIUM pSTM)
{

	printf("IAdviseSink::OnDataChange...\n");

	// Sanity checks...
	// If wrong storage medium or null pointer 
	// we can't handle it
	//
	if( TYMED_HGLOBAL != pFE->tymed )
	{
		printf("ERROR:Bad tymed (not HGLOBAL)\n");
		return;
	}
	if( pSTM->hGlobal == 0 )
	{
		printf("ERROR:Bad hGlobal\n");
		return;
	}

	if(pFE->cfFormat == m_data)
	{
		printf("\tFormat:data...\n");
		HandleData(pSTM);
	} else if(pFE->cfFormat == m_datatime)
	{
		printf("\tFormat:data with time...\n");
		HandleDataTime(pSTM);
	} else if(pFE->cfFormat == m_write)
	{
		printf("\tFormat:WriteComplete...\n");
		HandleWrite(pSTM);
	} else
	{
		printf("\tERROR: Format unknown\n");
		return;
	}
}



///////////////////////////////////////
//
///////////////////////////////////////
void IXXXAS::OnViewChange(
	 DWORD dwA,
	 LONG lindex
	)
{
	printf("OnViewChange...\n");
}

///////////////////////////////////////
//
///////////////////////////////////////
void IXXXAS::OnSave(void)
{
	printf("OnSave...\n");
}

///////////////////////////////////////
//
///////////////////////////////////////
void IXXXAS::OnRename(IMoniker*)
{
	printf("OnRename...\n");
}

///////////////////////////////////////
//
///////////////////////////////////////
void IXXXAS::OnClose(void)
{
	printf("OnClose...\n");
}




///////////////////////////////////////
// The rest of this file contains the 
// test program
//
///////////////////////////////////////

void OnDataChange(
	 DWORD       Transid, 
	 OPCHANDLE   grphandle, 
	 DWORD       count, 
	 OPCHANDLE * clienthandles, 
	 VARIANT   * value, 
	 BOOL        masterquality,
	 WORD      * quality,
	 FILETIME  * time
);
void OnWriteComplete(
	 DWORD       Transid, 
	 OPCHANDLE   grphandle, 
	 DWORD       count, 
	 OPCHANDLE * clienthandles, 
	 HRESULT   * hrs, 
	 BOOL        masterhr
);
extern	IMalloc *pIMalloc;
void TryDataObject(IOPCGroupStateMgt * pGRP, int nItem, OPCHANDLE *sh);
static void flip_mode(OPCDATASOURCE *mode);
static void	DumpValue(int i, VARIANT *v, WORD q, OPCHANDLE h);
extern void	DumpVariant(VARIANT *v);


//---------------------------------------------------------
// TryDataObject
// Function to exercise the DataObject logic for a group
// 
//void TryDataObject(IOPCGroupStateMgt * pGRP)
void TryDataObject(IOPCGroupStateMgt * pGRP, int nItem, OPCHANDLE *sh)
{
	IDataObject *pDO;
	OPCDATASOURCE mode = OPC_DS_CACHE;
	IOPCAsyncIO *pASIO = 0;
	DWORD MyCookie = 0L;
	DWORD MyCookiew = 0L;
	IXXXAS *MyCallback;
	HRESULT r1;
	BOOL Active = 1;
	BOOL InActive = 0;
	DWORD RequestedRate = 2123;
	DWORD Rate;
	DWORD tid;
	
	cputs("Hit Enter to Try OnDataChange...\n");
	getch();

	// Create a callback instance
	//
	MyCallback = new IXXXAS;
	MyCallback -> AddRef();

	// Establish a connection
	//
	r1 = pGRP->QueryInterface(IID_IDataObject, (void**)&pDO);
	if (FAILED(r1))
	{
		printf("No DataObject:(%lx)\n", r1);
		return;
	}

	r1 = pGRP->QueryInterface(IID_IOPCAsyncIO, (void**)&pASIO);
	if (FAILED(r1))
	{
		printf("No AsyncIO interface:(%lx)\n", r1);
		pDO->Release();
		return;
	}

	// todo: add test for data without timestamp	zzz
	// (Note the callback handler is already here - 
	// I just never subscribe to it)

	FORMATETC fe;
	fe.lindex = -1;
	fe.tymed = TYMED_HGLOBAL;
	fe.ptd = NULL;
	fe.dwAspect = DVASPECT_CONTENT;
	fe.cfFormat = RegisterClipboardFormat("OPCSTMFORMATDATATIME");

	r1 = pDO->DAdvise(&fe, ADVF_PRIMEFIRST, MyCallback, &MyCookie);
	if (FAILED(r1))
	{
		printf("Data/Time Advise Failed:(%lx)\n", r1);
		pASIO->Release();
		pDO->Release();
		return;
	}

	fe.lindex = -1;
	fe.tymed = TYMED_HGLOBAL;
	fe.ptd = NULL;
	fe.dwAspect = DVASPECT_CONTENT;
	fe.cfFormat = RegisterClipboardFormat("OPCSTMFORMATWRITECOMPLETE");

	r1 = pDO->DAdvise(&fe, ADVF_PRIMEFIRST, MyCallback, &MyCookiew);
	if (FAILED(r1))
	{
		pDO->DUnadvise(MyCookie);
		printf("WriteComplete Advise Failed:(%lx)\n", r1);
		pASIO->Release();
		pDO->Release();
		return;
	}


	printf("Callback is Connected - monitoring for activity...\n");
	printf("Mode = DEVICE\n");
	printf("Press 'a' to set active, f to Refresh, c to cancel, i to set inactive\n");
	printf("'m' to change mode, 'x' to disconnect\n");

	// Run test until terminated by user.
	// We must dispatch messages (in single apartment model)
	// for callbacks to work!
	//
	pGRP->SetState(&RequestedRate, &Rate, &Active,0,0,0,0);
	printf("Requested rate: %ld, Revised rate: %ld\n", RequestedRate, Rate);
	while(1)
	{
		if(kbhit())
		{
			char c;
			c = getch();
			if (c == 'x') 
				break;
			if (c == 'a') 
				pGRP->SetState(0, &Rate, &Active, 0,0,0,0);
			if (c == 'i') 
				pGRP->SetState(0, &Rate, &InActive, 0,0,0,0);
			if (c == 'f') 
			{
				r1= pASIO->Refresh(MyCookie, mode, &tid);
				if (FAILED(r1))	printf("Refresh Error:%lx\n", r1);
				else printf("RefreshTID=%lx\n", tid);
			}
			if (c == 'c') 
			{
				r1 = pASIO->Cancel(tid);
				if(FAILED(r1)) printf("Cancel Error=%lx\n", r1);
			}
			if (c == 'm') 
				flip_mode(&mode);
		}else
		{
			MSG msg;
			while(PeekMessage(&msg,NULL,NULL,NULL,PM_REMOVE))
			{
				TranslateMessage(&msg);
				DispatchMessage(&msg);
			}
		}
	}
	// Done - set group Inactive to stop ondatachange
	//
	pGRP->SetState(0, &Rate, &InActive, 0,0,0,0);


	// Next, test Async Read/Write
	//
	cputs("Hit Enter to Try AsyncRead/Write...\n");
	getch();

	mode = OPC_DS_DEVICE;
	printf("Mode = DEVICE\n");
	printf("Press 'r' to read, 'w' to write\n");
	printf("'f' to Refresh, 'c' to Cancel\n");
	printf("'m' to change mode, 'x' to disconnect\n");

	while(1)
	{
		HRESULT *hr;

		if(kbhit())
		{
			char c;
			c = getch();

			if (c == 'x') 
				break;
			if (c == 'r') 
			{
				r1 = pASIO->Read(MyCookie, mode, nItem, 
				sh, &tid, &hr);
				// check r1 and hr array and also free hr
				if(r1 != S_OK)
				{
					printf("Async ReadFailed:(%lx)\n", r1);
				}
				if(hr) pIMalloc->Free(hr);
			}
			if (c == 'w') 
			{
				VARIANT v[2];

				v[0].vt = VT_R4;
				v[0].fltVal = (float)12.34;
				v[1].vt = VT_R4;
				v[1].fltVal = (float)56.78;

				r1 = pASIO->Write(MyCookiew, nItem, 
				sh, v, &tid, &hr);

				// check r1 and hr array and also free hr
				if(r1 != S_OK)
				{
					printf("Async WriteFailed:(%lx)\n", r1);
				}
				if(hr) pIMalloc->Free(hr);
			}
			if (c == 'f') 
			{
				r1 = pASIO->Refresh(MyCookie, mode, &tid);
				if (FAILED(r1))	printf("Refresh Error:%lx\n", r1);
				else printf("RefreshTID=%lx\n", tid);
			}
			if (c == 'c') 
			{
				r1 = pASIO->Cancel(tid);
				if(FAILED(r1)) printf("Cancel Error=%lx\n", r1);
			}
			if (c == 'm') 
				flip_mode(&mode);
		}else
		{
			MSG msg;
			while(PeekMessage(&msg,NULL,NULL,NULL,PM_REMOVE))
			{
				TranslateMessage(&msg);
				DispatchMessage(&msg);
			}
		}
	}


	// User has terminated the test
	//
	printf("Disconnecting...\n");
	r1 = pDO->DUnadvise(MyCookie);
	if (FAILED(r1))
	{
		printf("Read UnAdvise Failed:(%lx)\n", r1);
	}

	r1 = pDO->DUnadvise(MyCookiew);
	if (FAILED(r1))
	{
		printf("Write UnAdvise Failed:(%lx)\n", r1);
	}

	// Release the interfaces
	//
	pASIO->Release();
	pDO->Release();

	MyCallback -> Release();
}

static void flip_mode(OPCDATASOURCE *mode)
{
	switch(*mode)
	{
	case OPC_DS_CACHE:
		*mode = OPC_DS_DEVICE;
		printf("Mode changed to DEVICE\n");
		break;
	case OPC_DS_DEVICE:
		*mode = OPC_DS_CACHE;
		printf("Mode changed to CACHE\n");
		break;
	}
}




///////////////////////////////////////
// Handle Data Stream WITHOUT TIMESTAMP...
// This code does NOT handle arrays
//
///////////////////////////////////////
void HandleData(
	 STGMEDIUM *pSTM
)
{
	DWORD	count;
	OPCGROUPHEADER *grp;
	char *iptr;
	char *dptr;
	char *sptr;
	BOOL masterquality;
	unsigned int i;

	// Open the STGMEDIUM
	// and read the GROUP header
	//
	sptr = (char*)GlobalLock(pSTM->hGlobal);
	if(!sptr) return;

	grp = (OPCGROUPHEADER*)sptr;
	if(grp->hrStatus)
	{
		printf("\thrStatusr=%lx\n", grp->hrStatus);
		return;
	}

	// Allocate some memory into which to put the returned data
	//
	OPCHANDLE * handles;
	WORD      * quality;
	VARIANT   * values; 

	count = grp->dwItemCount;

	masterquality = 0;
	handles = new OPCHANDLE[count];
	values = new VARIANT[count];
	quality = new WORD[count];
	//zzz check for errors...

	for(i=0; i<count; i++)
	{
		VariantInit(&values[i]);
	}

	// Figure out where the ITEM headers start
	// And also where the DATA starts
	// We will read the two areas in parallel
	//
	iptr = sptr + sizeof(OPCGROUPHEADER);
	dptr = iptr + (count * sizeof(OPCITEMHEADER2)); 

	// For each ITEM in the stream...
	//
	for(i=0; i<count; i++)
	{
		OPCITEMHEADER2 * item;

		// find the next item header
		//
		item = (OPCITEMHEADER2*)iptr;
		iptr += sizeof( OPCITEMHEADER2 );

		// Find the data for the item
		//
		dptr = sptr + item->dwValueOffset;

		// Copy the client handle and quality
		//
		handles[i] = item->hClient;
		quality[i] = item->wQuality;
		if(quality[i] != 0xc0) masterquality = 1;

		// And then the data (which is a bit trickey)
		//
		OPCVariantUnpack(&values[i], dptr);
	}

	// now do something with the data...
	// which we also has to free...
	//
	//zzz also need to sort out read complete vs data change
	// also what to do with hrStatus ?
	//
	OnDataChange(grp->dwTransactionID,
		grp->hClientGroup,
		count,
		handles,
		values,
		masterquality,
		quality,
		0);

	// Free all of the scratch variables
	//
	for(i=0; i<count; i++) VariantClear(&values[i]);

	if(handles) delete [] handles;
	if(values) delete [] values;
	if(quality) delete [] quality;

	return;
}

	
///////////////////////////////////////
// Handle Data Stream WITH TIMESTAMP...
// This code does NOT handle arrays
//
///////////////////////////////////////
void HandleDataTime(
	 STGMEDIUM *pSTM
)
{
	DWORD	count;
	OPCGROUPHEADER *grp;
	char *iptr;
	char *dptr;
	char *sptr;
	BOOL masterquality;
	unsigned int i;

	// Open the STGMEDIUM
	// and read the GROUP header
	//
	sptr = (char*)GlobalLock(pSTM->hGlobal);
	if(!sptr) return;

	grp = (OPCGROUPHEADER*)sptr;
	if(grp->hrStatus)
	{
		printf("\thrStatusr=%lx\n", grp->hrStatus);
		return;
	}

	// Allocate some memory into which to put the returned data
	//
	OPCHANDLE * handles;
	WORD      * quality;
	VARIANT   * values; 
	FILETIME  * ft;

	count = grp->dwItemCount;

	masterquality = 0;
	handles = new OPCHANDLE[count];
	values = new VARIANT[count];
	quality = new WORD[count];
	ft = new FILETIME[count];
	//zzz check for errors...

	for(i=0; i<count; i++)
	{
		VariantInit(&values[i]);
	}

	// Figure out where the ITEM headers start
	// And also where the DATA starts
	// We will read the two areas in parallel
	//
	iptr = sptr + sizeof(OPCGROUPHEADER);
	dptr = iptr + (count * sizeof(OPCITEMHEADER1)); 

	// For each ITEM in the stream...
	//
	for(i=0; i<count; i++)
	{
		OPCITEMHEADER1 * item;

		// find the next item header
		//
		item = (OPCITEMHEADER1*)iptr;
		iptr += sizeof( OPCITEMHEADER1 );

		// Find the data for the item
		//
		dptr = sptr + item->dwValueOffset;

		// Copy the client handle and quality
		//
		handles[i] = item->hClient;
		quality[i] = item->wQuality;
		ft[i] = item->ftTimeStampItem;
		if(quality[i] != 0xc0) masterquality = 1;

		// And then the data (which is a bit trickey)
		//
		OPCVariantUnpack(&values[i], dptr);
	}

	// now do something with the data...
	// which we also has to free...
	//
	//zzz also need to sort out read complete vs data change
	// also what to do with hrStatus ?
	//
	OnDataChange(grp->dwTransactionID,
		grp->hClientGroup,
		count,
		handles,
		values,
		masterquality,
		quality,
		ft);

	// Free all of the scratch variables
	//
	for(i=0; i<count; i++) 
	{
		VariantClear(&values[i]);
	}

	if(handles) delete [] handles;
	if(values) delete [] values;
	if(quality) delete [] quality;
	if(ft) delete [] ft;

	return;
}


///////////////////////////////////////
// Handle Data Stream for Write...
//
///////////////////////////////////////
void HandleWrite(
	 STGMEDIUM *pSTM
)
{
	DWORD	count;
	OPCGROUPHEADERWRITE *grp;
	char *iptr;
	char *sptr;
	BOOL masterhr;
	unsigned int i;

	// Open the STGMEDIUM
	// and read the GROUP header
	//
	sptr = (char*)GlobalLock(pSTM->hGlobal);
	if(!sptr) return;

	grp = (OPCGROUPHEADERWRITE*)sptr;
	if(grp->hrStatus)
	{
		printf("\thrStatusr=%lx\n", grp->hrStatus);
		return;
	}

	// Allocate some memory into which to put the returned data
	//
	OPCHANDLE * handles;
	HRESULT   * hrs;

	count = grp->dwItemCount;

	masterhr = 0;
	handles = new OPCHANDLE[count];
	hrs = new HRESULT[count];
	//zzz check for errors...

	// Figure out where the ITEM headers start
	//
	iptr = sptr + sizeof(OPCGROUPHEADERWRITE);

	// For each ITEM in the stream...
	//
	for(i=0; i<count; i++)
	{
		OPCITEMHEADERWRITE * item;

		// find the next item header
		//
		item = (OPCITEMHEADERWRITE*)iptr;
		iptr += sizeof( OPCITEMHEADERWRITE );

		// Copy the client handle and hresult
		//
		handles[i] = item->hClient;
		hrs[i] = item->dwError;
		if(hrs[i] != S_OK) masterhr = 1;
	}

	// now do something with the results...
	//
	OnWriteComplete(grp->dwTransactionID,
		grp->hClientGroup,
		count,
		handles,
		hrs,
		masterhr);

	// Free all of the scratch variables
	//

	if(handles) delete [] handles;
	if(hrs) delete [] hrs;

	return;
}

	

	





//---------------------------------------------------------
//
//
void OnDataChange(
	 DWORD       Transid, 
	 OPCHANDLE   grphandle, 
	 DWORD       count, 
	 OPCHANDLE * clienthandles, 
	 VARIANT   * value, 
	 BOOL        masterquality,
	 WORD      * quality,
	 FILETIME  * time
)
{
	unsigned int i;

	printf("OnDataChange: Transid=%ld count=%ld GrpHandle=%ld\n", Transid, count, grphandle);
	for(i=0; i<count; i++)
	{
		DumpValue(i, &value[i], quality[i], clienthandles[i]);
	}
	// Note we do NOT free memory here!
}


//---------------------------------------------------------
//
//
void OnWriteComplete(
	 DWORD       Transid, 
	 OPCHANDLE   grphandle, 
	 DWORD       count, 
	 OPCHANDLE * clienthandles, 
	 HRESULT   * hrs, 
	 BOOL        masterhr
)
{
	unsigned int i;

	printf("OnWriteComplete: Transid=%ld count=%ld GrpHandle=%ld\n", Transid, count, grphandle);
	for(i=0; i<count; i++)
	{
		printf("  [%d]-handle=%d, HR=%x\n", i, clienthandles[i], hrs[i]);
	}
	// Note we do NOT free memory here!
}


//---------------------------------------------------------
// DumpValue
// This shows the value of a Variant
static void	DumpValue(int i, VARIANT *v, WORD q, OPCHANDLE h)
{
	printf("  [%d]-handle=%d, Qual=%2x, ", i, h, q);
	DumpVariant(v);
	printf("\n");
}
