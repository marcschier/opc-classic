// Grp_do.cpp
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
//  This file contains the implementation of 
//  the XXX Group Object DataObject Async Logic
//  for the OPC Sample server.
//
//
// Note - this logic supports independent subscriptions for
// data with and without timestamp (although it is not likely
// that this would ever be used)
//
// Note - this logic CANNOT be used at the same time as ConnectionPoints.
// The client must use one or the other but not both.
//
// Modification Log:
// Vers    Date   By    Notes
// ----  -------- ---   -----
// 0.90  04/08/97 ACC   add async logic
//       05/20/97 ACC   SendWriteStream should use GROUPHEADERWRITE, not GROUPHEADER
//       09/11/97 ACC   checkOnDataChange should call SendStream, not SendStreamSithTime
//

#define WIN32_LEAN_AND_MEAN
#define NOCOMM

#include "OPCXXX.h"

#include "OPC_PACK.h"



///////////////////////////////////////
// XXXGroup::CheckDOOnDataTimeChange
//
// Handle IDataObject Callbacks for OnDataChange (WITH TimeStamp)
//
void		XXXGroup::CheckDOOnDataTimeChange( void )
{

	// If group is active
	//
	if(!m_bActive) return;

	// And if there is a Data object
	//
	if(!m_pIXXXDO) return;

	// And if the DataObject has a callback
	//
	if(m_pIXXXDO->m_datatimeCallback)
	{
		int i, Count, DataSize;

		// Build a list of the things that changed
		//
		OPCHANDLE *ItemHandleList = new OPCHANDLE[ItemHandles()];

		Count = 0;
		for(i=0, Count=0; i<ItemHandles(); i++)
		{
			if(ItemValid(i))
			{
				if(ItemPtr(i)->Changed(OPC_ODC_DOT))	// with time
				{
					ItemHandleList[Count] = i;
					Count++;
				}
			}
		}

		// And if anything changed
		//
		if(Count)
		{
			// compute the size needed to send them
			//
			DataSize = ComputeSize(Count, ItemHandleList);

			// Given the item list, build and send the callback
			//
			SendStreamWithTime(Count, ItemHandleList, DataSize, OPC_ODC_DOT, 0);	// with time
		}

		// Free the itemhandlelist
		//
		delete [] ItemHandleList;
	}
}

///////////////////////////////////////
// XXXGroup::CheckDOOnDataChange
//
// Handle IDataObject Callbacks for OnDataChange (WITHOUT TimeStamp)
//
void		XXXGroup::CheckDOOnDataChange( void )
{

	// If group is active
	//
	if(!m_bActive) return;

	// And if there is a Data object
	//
	if(!m_pIXXXDO) return;

	// And if the DataObject point has a callback
	//
	if(m_pIXXXDO->m_dataCallback)
	{
		int i, Count, DataSize;

		// Build a list of the things that changed
		//
		OPCHANDLE *ItemHandleList = new OPCHANDLE[ItemHandles()];

		Count = 0;
		for(i=0, Count=0; i<ItemHandles(); i++)
		{
			if(ItemValid(i))
			{
				if(ItemPtr(i)->Changed(OPC_ODC_DO))
				{
					ItemHandleList[Count] = i;
					Count++;
				}
			}
		}

		// And if anything changed
		//
		if(Count)
		{
			// compute the size needed to send them
			//
			DataSize = ComputeSize(Count, ItemHandleList);

			// Given the item list, build and send the callback (without time)
			//
			SendStream(Count, ItemHandleList, DataSize, OPC_ODC_DO, 0);	//without time
		}

		// Free the itemhandlelist
		//
		delete [] ItemHandleList;
	}
}


///////////////////////////////////////
// XXXGroup::CheckDOAsyncRead
//
// If asyncread active
//  If cancel requested then
//   send cancel complete
//   free handle list
//   clear asyncread active
//  else check for complete
//  If all item reads complete then
//   assemble read complete callback
//   if callback ok then free the handle list
//   clear asyncread active
//  
void		XXXGroup::CheckDOAsyncRead( void )
{
	BOOL complete;
	int i, DataSize;

	// If a read is in progress
	//
	if(m_AsyncReadActive)
	{
		// then check to see if a cancel of this action was requested 
		// (TID will have been matched already)
		//
		if(m_AsyncReadCancel)
		{
			// And if so then cancel the request 
			// (note there is no callback in this case)
			//
			pIMalloc->Free(m_AsyncReadList);
			m_AsyncReadList = 0;
			m_AsyncReadActive = 0;
			m_AsyncReadCancel = 0;
			return;
		}

		// Else (if no cancel) then check progress of read 
		// If read was from DEVICE then see if it's done
		//
		complete = 1;
		if(m_AsyncReadSource == OPC_DS_DEVICE)
		{
			for(i=0; i<m_AsyncReadActive; i++)
			{
				OPCHANDLE h = m_AsyncReadList[i];
				XXXItem *pItem = ItemPtr(h);
	
				// See if any of the device reads are still pending
				//
				if (!pItem->CheckDeviceRead(0))
					complete = 0;
			}
		}

		// If Read is complete/data is ready
		//
		if(complete)
		{
			if(m_pIXXXDO)
			{
				// Compute data size for stream
				//
				DataSize = ComputeSize(m_AsyncReadActive, m_AsyncReadList);

				// And if the DataObject has a callback
				//
				if(m_pIXXXDO->m_dataCallback)
				{
					SendStream(m_AsyncReadActive, m_AsyncReadList, DataSize, 0, m_AsyncReadTID);
				}
				if(m_pIXXXDO->m_datatimeCallback)
				{
					SendStreamWithTime(m_AsyncReadActive, m_AsyncReadList, DataSize, 0, m_AsyncReadTID);
				}
			}

			// In any case the read is complete...
			// Note that if readlist was not sent to the client
			// then we must free it here.
			//
			if(m_AsyncReadList) pIMalloc->Free(m_AsyncReadList);
			m_AsyncReadList = 0;
			m_AsyncReadActive = 0;
			m_AsyncReadCancel = 0;

		}	// (else read not complete)
	}		// (else read not active)
}


///////////////////////////////////////
// XXXGroup::CheckDOAsyncWrite
//
// If asyncwrite active
//  If cancel requested
//   send cancel complete
//   free handle list ??
//   clear asyncwrite active
//  else
//   If all item writes complete
//   then assemble write complete callback
//   free handle list ??
//   clear asyncwrite active
//  
void		XXXGroup::CheckDOAsyncWrite( void )
{
	BOOL complete;
	int i;

	// If a write is in progress
	//
	if(m_AsyncWriteActive)
	{
		// then check to see if a cancel of this action was requested 
		// (TID will have been matched already)
		//
		if(m_AsyncWriteCancel)
		{
			// And if so then cancel the request 
			// (note there is no callback in this case)
			//
			pIMalloc->Free(m_AsyncWriteList);
			m_AsyncWriteList = 0;
			m_AsyncWriteActive = 0;
			m_AsyncWriteCancel = 0;
			return;
		}

		// Else (if no cancel) then check progress of Write 
		//
		complete = 1;
		for(i=0; i<m_AsyncWriteActive; i++)
		{
			OPCHANDLE h = m_AsyncWriteList[i];
			XXXItem *pItem = ItemPtr(h);

			// See if any of the device Writes are still pending
			//
			if (!pItem->CheckDeviceWrite(0))
				complete = 0;
		}

		// If Write is complete
		//
		if(complete)
		{
			if(m_pIXXXDO)
			{
				// If the DataObject has a callback
				//
				if(m_pIXXXDO->m_writeCallback)
				{
					SendWriteStream(m_AsyncWriteActive, m_AsyncWriteList, m_AsyncWriteTID);
				}
			}

			// In any case the Write is complete...
			// Note that if Writelist was not sent to the client
			// then we must free it here.
			//
			if(m_AsyncWriteList) pIMalloc->Free(m_AsyncWriteList);
			m_AsyncWriteList = 0;
			m_AsyncWriteActive = 0;
			m_AsyncWriteCancel = 0;

		}	// (else Write not complete)
	}		// (else Write not active)
}


///////////////////////////////////////
// XXXGroup::CheckDOFefresh
//
// This code is nearly identical to CheckDOAsyncRead except
//  it uses a different set of state variables.
//  there is a slight difference in 'active' logic
//  and it invokes OnDataChange rather than OnReadComplete
//
// If refresh active
//  If cancel requested then
//   send cancel complete
//   free handle list
//   clear refresh active
//  else check for complete
//  If all item reads complete then
//   assemble refresh complete callback
//   if callback ok then free the handle list
//   clear refresh active
//  
void		XXXGroup::CheckDORefresh( void )
{
	BOOL complete;
	int i, DataSize;

	// If a read is in progress
	//
	if(m_RefreshActive)
	{
		// then check to see if a cancel was requested
		//
		if(m_RefreshCancel)
		{
			// Cancel the refresh
			// (note there is no callback in this case)
			//
			pIMalloc->Free(m_RefreshList);
			m_RefreshList = 0;
			m_RefreshActive = 0;
			m_RefreshCancel = 0;
			return;
		}

		// Else (if no cancel) then check progress of read 
		// If read was from DEVICE then see if it's done
		//
		complete = 1;
		if(m_RefreshSource == OPC_DS_DEVICE)
		{
			for(i=0; i<m_RefreshActive; i++)
			{
				OPCHANDLE h = m_RefreshList[i];
				XXXItem *pItem = ItemPtr(h);
	
				// See if any of the DEVICE reads are still pending
				//
				if (!pItem->CheckDeviceRead(0))
					complete = 0;
			}
		}

		// If Read is complete/data is ready
		//
		if(complete)
		{
			if(m_pIXXXDO)
			{
				// Compute data size for stream
				//
				DataSize = ComputeSize(m_RefreshActive, m_RefreshList);

				// generally there are only active items in list
				// but if not then logic in the subroutine will handle it
				//
				if(m_pIXXXDO->m_dataCallback)
				{
					SendStream(m_RefreshActive, m_RefreshList, DataSize, 0, m_RefreshTID);
				}
				if(m_pIXXXDO->m_datatimeCallback)
				{
					SendStreamWithTime(m_RefreshActive, m_RefreshList, DataSize, 0, m_RefreshTID);
				}
			} // (else no callback)

			// In any case the refresh is complete...
			// Note that if readlist was not sent to the client
			// then we must free it here.
			//
			if(m_RefreshList) pIMalloc->Free(m_RefreshList);
			m_RefreshList = 0;
			m_RefreshActive = 0;
			m_RefreshCancel = 0;

		}	// (else read not complete)
	}		// (else read not active)
}


///////////////////////////////////////
// XXXGroup::SendStreamWithTime 
//
// This function is used to build the stream for OnDataChange and also
// for async operations such as Read and Refresh
// The two cases are 'almost' the same except for 
// a small bit of 'mask' and quality logic as you can see below
//
// Create the Stream which consists of:
//  GroupHeader
//  ItemHeaders
//  Variants and Related Data.
//
//
void		XXXGroup::SendStreamWithTime(int Count, OPCHANDLE *ItemHandleList, DWORD DataSize, WORD mask, DWORD tid)
{
		int i, j, HdrSize, TotalSize;

		// Allocate the Data Stream
		//
		HdrSize = sizeof(OPCGROUPHEADER) + Count * sizeof(OPCITEMHEADER1);	// with time
		TotalSize = HdrSize + DataSize;

		char * gp;
		HGLOBAL gh;

		gh = GlobalAlloc( GMEM_FIXED, TotalSize );
		if(gh == NULL)
		{
			// If no memory, For now we just give up and return;
			// But a better error handler would be nice
			//
			return;
		}
		gp = (char*)GlobalLock(gh);

		OPCGROUPHEADER * GrpPtr;
		OPCITEMHEADER1 * ItemHdr1;	// With Time
		char * DataPtr;

		GrpPtr = (OPCGROUPHEADER *) gp;
		ItemHdr1 = (OPCITEMHEADER1 *) (gp + sizeof(OPCGROUPHEADER));	//With Time
		DataPtr = gp + HdrSize;

		// Fill in the Group header
		//
		GrpPtr->dwSize = TotalSize;
		GrpPtr->dwItemCount = Count;
		GrpPtr->hClientGroup =	m_ClientGroupHandle;
		GrpPtr->hrStatus = S_OK;
		GrpPtr->dwTransactionID = tid;

		// Now fill in the Item Information
		//

		for(j=0; j<Count; j++)
		{
			// Get serverhandle of items to send
			// (they were in use a millisecond ago and are assumed to be still)
			// We should be in a critical seciton here
			//
			i = ItemHandleList[j];

			int vsize;
			VARIANT vtemp;

			VariantInit(&vtemp);

			// Get the basic ItemHeader info
			//
			ItemPtr(i)->GetValue( OPC_DS_CACHE,
				&vtemp,
				&ItemHdr1->wQuality,
				&ItemHdr1->ftTimeStampItem);		// with time
			ItemHdr1->hClient = ItemPtr(i)->GetHandle();
			ItemHdr1->dwValueOffset = DataPtr - gp;
			ItemHdr1->wReserved = 0;
					
			// Plus the variant
			// Note that any 'pointer' in the variant will be meaningless
			//
			vsize = OPCVariantPack(DataPtr, &vtemp);
			VariantClear(&vtemp);
			
			// If this is an OnDataChange then clear the item changed flag
			// else if this is an Async Read then do some special quality handling
			//
			if(mask)
			{
				ItemPtr(i)->ClearChanged(mask);	//with time
			} else
			{
				// if async read then group must be active
				//
				if((m_AsyncReadSource == OPC_DS_CACHE) && !m_bActive) 
					ItemHdr1->wQuality = QUAL_NOTACTIVE;
			}

			// The update the pointers
			//
			ItemHdr1++;
			DataPtr += vsize;
		}

		// And invoke the callback
		//
		STGMEDIUM stm;
		FORMATETC fe;

		fe.cfFormat = m_pIXXXDO->m_datatime;		// with time
		fe.ptd = NULL;
		fe.dwAspect = DVASPECT_CONTENT;
		fe.lindex = -1;
		fe.tymed = TYMED_HGLOBAL;

		stm.tymed = TYMED_HGLOBAL;
		stm.hGlobal = gh;
		stm.pUnkForRelease = NULL;

		m_pIXXXDO->m_datatimeCallback->OnDataChange(&fe, &stm);

		// And free the storage
		//
		GlobalFree(gh);

}

///////////////////////////////////////
// XXXGroup::SendStream  (Without Time)
//
// This function is used to build the stream for OnDataChange and also
// for async operations such as Read and Refresh
// The two cases are 'almost' the same except for 
// a small bit of 'mask' and quality logic as you can see below
//
// Create the Stream which consists of:
//  GroupHeader
//  ItemHeaders
//  Variants and Related Data.
//
void		XXXGroup::SendStream(int Count, OPCHANDLE *ItemHandleList, DWORD DataSize, WORD mask, DWORD tid)
{
		int i, j, HdrSize, TotalSize;

		// Allocate the Data Stream
		//
		HdrSize = sizeof(OPCGROUPHEADER) + Count * sizeof(OPCITEMHEADER2);	// without time
		TotalSize = HdrSize + DataSize;

		char * gp;
		HGLOBAL gh;

		gh = GlobalAlloc( GMEM_FIXED, TotalSize );
		if(gh == NULL)
		{
			// If no memory, For now we just give up and return;
			// But a better error handler would be nice
			//
			return;
		}
		gp = (char*)GlobalLock(gh);

		OPCGROUPHEADER * GrpPtr;
		OPCITEMHEADER2 * ItemHdr1;	// Without Time
		char * DataPtr;

		GrpPtr = (OPCGROUPHEADER *) gp;
		ItemHdr1 = (OPCITEMHEADER2 *) (gp + sizeof(OPCGROUPHEADER));	//Without Time
		DataPtr = gp + HdrSize;

		// Fill in the Group header
		//
		GrpPtr->dwSize = TotalSize;
		GrpPtr->dwItemCount = Count;
		GrpPtr->hClientGroup =	m_ClientGroupHandle;
		GrpPtr->hrStatus = S_OK;
		GrpPtr->dwTransactionID = tid;

		// Now fill in the Item Information
		//

		for(j=0; j<Count; j++)
		{
			// Get serverhandle of items to send
			// (they were in use a millisecond ago and are assumed to be still)
			// We should be in a critical seciton here
			//
			i = ItemHandleList[j];

			int vsize;
			VARIANT vtemp;

			VariantInit(&vtemp);

			// Get the basic ItemHeader info
			//
			ItemPtr(i)->GetValue( OPC_DS_CACHE,
				&vtemp,
				&ItemHdr1->wQuality,
				0);		// without time
			ItemHdr1->hClient = ItemPtr(i)->GetHandle();
			ItemHdr1->dwValueOffset = DataPtr - gp;
			ItemHdr1->wReserved = 0;
					
			// Plus the variant
			// Note that any 'pointer' in the variant will be meaningless
			//
			vsize = OPCVariantPack(DataPtr, &vtemp);
			VariantClear(&vtemp);
			
			// If this is an OnDataChange then clear the item changed flag
			// else if this is an Async Read then do some special quality handling
			//
			if(mask)
			{
				ItemPtr(i)->ClearChanged(mask);	//without time
			} else
			{
				// if async read then group must be active
				//
				if((m_AsyncReadSource == OPC_DS_CACHE) && !m_bActive) 
					ItemHdr1->wQuality = QUAL_NOTACTIVE;
			}

			// The update the pointers
			//
			ItemHdr1++;
			DataPtr += vsize;
		}

		// And invoke the callback
		//
		STGMEDIUM stm;
		FORMATETC fe;

		fe.cfFormat = m_pIXXXDO->m_data;		// without time
		fe.ptd = NULL;
		fe.dwAspect = DVASPECT_CONTENT;
		fe.lindex = -1;
		fe.tymed = TYMED_HGLOBAL;

		stm.tymed = TYMED_HGLOBAL;
		stm.hGlobal = gh;
		stm.pUnkForRelease = NULL;

		m_pIXXXDO->m_dataCallback->OnDataChange(&fe, &stm);	// without time

		// And free the storage
		//
		GlobalFree(gh);

}
///////////////////////////////////////
// XXXGroup::SendWriteStream
//
// This function is used to build the stream for OnWriteComplete
//
// Create the Stream which consists of:
//  GroupHeader
//  ItemHeaders
//
void		XXXGroup::SendWriteStream(int Count, OPCHANDLE *ItemHandleList, DWORD tid)
{
		int i, j, HdrSize, TotalSize;

		// Allocate the Data Stream
		//
		HdrSize = sizeof(OPCGROUPHEADERWRITE) + Count * sizeof(OPCITEMHEADERWRITE);
		TotalSize = HdrSize ;

		char * gp;
		HGLOBAL gh;

		gh = GlobalAlloc( GMEM_FIXED, TotalSize );
		if(gh == NULL)
		{
			// If no memory, For now we just give up and return;
			// But a better error handler would be nice
			//
			return;
		}
		gp = (char*)GlobalLock(gh);

		OPCGROUPHEADERWRITE * GrpPtr;
		OPCITEMHEADERWRITE * ItemHdr1;

		GrpPtr = (OPCGROUPHEADERWRITE *) gp;
		ItemHdr1 = (OPCITEMHEADERWRITE *) (gp + sizeof(OPCGROUPHEADERWRITE));

		// Fill in the Group header
		//
		GrpPtr->dwItemCount = Count;
		GrpPtr->hClientGroup =	m_ClientGroupHandle;
		GrpPtr->hrStatus = S_OK;
		GrpPtr->dwTransactionID = tid;

		// Now fill in the Item Information
		//

		for(j=0; j<Count; j++)
		{
			// Get serverhandle of items to send
			// We are be in a critical seciton here
			//
			i = ItemHandleList[j];

			// Get the ItemHeader info
			//
			ItemPtr(i)->CheckDeviceWrite( &ItemHdr1->dwError);
			ItemHdr1->hClient = ItemPtr(i)->GetHandle();
					
			// The update the pointers
			//
			ItemHdr1++;
		}

		// And invoke the callback
		//
		STGMEDIUM stm;
		FORMATETC fe;

		fe.cfFormat = m_pIXXXDO->m_write;
		fe.ptd = NULL;
		fe.dwAspect = DVASPECT_CONTENT;
		fe.lindex = -1;
		fe.tymed = TYMED_HGLOBAL;

		stm.tymed = TYMED_HGLOBAL;
		stm.hGlobal = gh;
		stm.pUnkForRelease = NULL;

		m_pIXXXDO->m_writeCallback->OnDataChange(&fe, &stm);

		// And free the storage
		//
		GlobalFree(gh);
}

///////////////////////////////////////
// XXXGroup::ComputeSize  
//
// This function is used to compute the size of the HGLOBAL
// (a.k.a. 'the stream')
// needed to contain the OnDataChange info for IAdviseSink
//
long XXXGroup::ComputeSize(int Count, OPCHANDLE *List)
{
	int i, DataSize = 0L;

	for(i=0; i<Count; i++)
	{
		VARIANT vtemp;
		OPCHANDLE h = List[i];

		// This method of getting size is not very efficient...
		//
		VariantInit(&vtemp);
		ItemPtr(h)->GetValue(OPC_DS_CACHE, &vtemp, 0,0);
		DataSize += OPCVariantSize(&vtemp);
		VariantClear(&vtemp);
	}
	return DataSize;
}
