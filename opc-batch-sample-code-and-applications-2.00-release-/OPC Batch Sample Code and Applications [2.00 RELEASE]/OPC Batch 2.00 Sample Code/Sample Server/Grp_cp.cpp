// Grp_co.cpp
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
//  the XXX Group Object Connection Point Logic
//  for the OPC Sample server.
//
//
// Modification Log:
// Vers    Date   By    Notes
// ----  -------- ---   -----
//       04/08/97 ACC   early draft
//       08/25/97 acc   add ConnectionPoint Support.
// 2.00  01/15/98 acc   add 'final' ConnectionPoint Support.
//       02/06/98 acc   This code is supposed to free the buffers, not the client
//                      Note that IMalloc is NOT actually needed for the buffers 
//                      since they are all 'in' parameters. This could be fixed later.
//                      This is problably also true of IDataObject.
//       06/12/98 acc   cosmetic cleanup
//       06/18/98 acc   fix read,write and refresh to return 
//                      client handle list rather than server list
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"



///////////////////////////////////////
// XXXGroup::CheckCPOnDataChange
//
// Handle IConnectionPoint Callbacks for OnDataChange
//
void		XXXGroup::CheckCPOnDataChange( IXXXCP * pCP )
{

	// If group is active
	//
	if(m_bActive) 
	{
		int i, Count;

		// Create data buffers
		// In this simple example we just alloc space for all possible Item Handles
		// and only use as many as we need. 
		// (This is reasonable efficient if we assume most of the handles are in use)
		//
		// Also note that this logic allocates them 
		// before we know if anything changed - simple but not efficient
		//
		OPCHANDLE *ItemHandleList = (OPCHANDLE*)pIMalloc->Alloc(ItemHandles() * sizeof( OPCHANDLE ));
		VARIANT   *ValueList      = (VARIANT*) pIMalloc->Alloc(ItemHandles() * sizeof(VARIANT));
		WORD      *QualList       = (WORD*) pIMalloc->Alloc(ItemHandles() * sizeof(WORD));
		FILETIME  *TimeList       = (FILETIME*)pIMalloc->Alloc(ItemHandles() * sizeof(FILETIME));
		HRESULT   *ErrorList      = (HRESULT*) pIMalloc->Alloc(ItemHandles() * sizeof(HRESULT));
		HRESULT    MasterQual     = S_OK;
		HRESULT    MasterError    = S_OK;

		// Should never happen but check errors for mallocs...
		//
		if(!ItemHandleList || !ValueList || !QualList || !TimeList || !ErrorList)
		{
			if(ItemHandleList) pIMalloc->Free( ItemHandleList);
			if(ValueList) pIMalloc->Free( ValueList);
			if(QualList) pIMalloc->Free( QualList);
			if(TimeList) pIMalloc->Free( TimeList);
			if(ErrorList) pIMalloc->Free( ErrorList);
			return;
		}

		for(i=0; i<ItemHandles(); i++)
		{
			VariantInit(&ValueList[i]);
		}

		// And fill in as many as have changed (if any)
		//
		for(i=0, Count=0; i<ItemHandles(); i++)
		{
			if(ItemValid(i))
			{
				BOOL changed;

				// Fetch values and clear 'ODC dirty' bits	
				//
				ItemPtr(i)->GetValue(OPC_DS_CACHE, &ValueList[Count], &QualList[Count], &TimeList[Count]);
				ItemHandleList[Count] = ItemPtr(i)->GetHandle();
				changed = ItemPtr(i)->Changed(OPC_ODC_CP);

				// If Item has changed then check MasterQuality
				// and also clear Changed Flag
				//
				if(changed) 	// (always 0 if item not active)
				{
					if((QualList[Count] & OPC_QUALITY_MASK) != OPC_QUALITY_GOOD) MasterQual = S_FALSE; 

					// If an error were detected for this item
					// (which this sample code does not do)
					// Then we would also set MasterError to S_FALSE
					//
					ErrorList[Count] = S_OK;
					Count++;

					ItemPtr(i)->ClearChanged(OPC_ODC_CP);
				}

			}
		}
	
		// And invoke the callback
		//
		if(Count) 
		{
			// Note note TID = 0 for normal data change.
			//
			IOPCDataCallback * pDC;
			pDC = (IOPCDataCallback *) pCP->m_Callback;
			pDC->OnDataChange(
					0,
					m_ClientGroupHandle,
					MasterQual, 
					MasterError, 
					Count, 
					ItemHandleList,
					ValueList, 
					QualList, 
					TimeList,
					ErrorList 
					);
		}

		// In any case free the buffers
		// (Again, this is not very efficient)
		//
		for(i=0; i<ItemHandles(); i++)
		{
			VariantClear(&ValueList[i]);
		}

		pIMalloc->Free( ItemHandleList);
		pIMalloc->Free( ValueList);
		pIMalloc->Free( QualList);
		pIMalloc->Free( TimeList);
		pIMalloc->Free( ErrorList);
	}
}



///////////////////////////////////////
// XXXGroup::CheckCPAsyncRead
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
void		XXXGroup::CheckCPAsyncRead( IXXXCP * pCP )
{
	BOOL complete;
	int i;

	// If a read is in progress
	//
	if(m_Async2ReadActive)
	{
		// Then find any connection point and callback stuff
		//
		IOPCDataCallback * pDC;
		pDC = (IOPCDataCallback *) pCP->m_Callback;		// note group.cpp already validated this

		// then check to see if a cancel was requested
		//
		if(m_Async2ReadCancel)
		{
			// And if so then Construct and send cancel complete
			//
			pDC->OnCancelComplete(m_Async2ReadTID, m_ClientGroupHandle);

			// and, cancel the read
			//
			pIMalloc->Free(m_Async2ReadList);
			m_Async2ReadList = 0;
			m_Async2ReadActive = 0;
			m_Async2ReadCancel = 0;
			return;
		}

		// Else (if no cancel) then check progress of read 
		// If read was from DEVICE then see if it's done
		//
		complete = 1;
		for(i=0; i<m_Async2ReadActive; i++)
		{
			OPCHANDLE h = m_Async2ReadList[i];
			XXXItem *pItem = ItemPtr(h);
	
			// See if any of the DEVICE reads are still pending
			// Note device reads don't distinguish CPoint and DataObject
			//
			if (!pItem->CheckDeviceRead(0)) 
			complete = 0;

		}

		// If Read is complete/data is ready
		//
		if(complete)
		{
			// Construct and send read complete
			//
			int i;
			VARIANT   *ValueList      = (VARIANT*) pIMalloc->Alloc(m_Async2ReadActive * sizeof(VARIANT));
			WORD      *QualList       = (WORD*) pIMalloc->Alloc(m_Async2ReadActive * sizeof(WORD));
			FILETIME  *TimeList       = (FILETIME*)pIMalloc->Alloc(m_Async2ReadActive * sizeof(FILETIME));
			HRESULT   *ErrorList      = (HRESULT*) pIMalloc->Alloc(m_Async2ReadActive * sizeof(HRESULT));
			HRESULT    MasterQual     = S_OK;
			HRESULT    MasterError    = S_OK;
			OPCHANDLE *ItemHandleList = (OPCHANDLE*)pIMalloc->Alloc(m_Async2ReadActive * sizeof( OPCHANDLE ));

			// A real server should do error checks for mallocs...

			for(i=0; i<m_Async2ReadActive; i++)
			{
				VariantInit(&ValueList[i]);
			}

			// And fill them in
			//
			for(i=0; i<m_Async2ReadActive; i++)
			{
				OPCHANDLE h = m_Async2ReadList[i];

				// Get item value, time, quality.
				// Note that quality will reflect item active status 
				//
				XXXItem *pItem = ItemPtr(h);
				pItem->GetValue(OPC_DS_DEVICE, &ValueList[i], &QualList[i], &TimeList[i]);

				// Note that Async Reads are independent of OnDataChange
				// So we do NOT clear the changed flag
				//
				if((QualList[i] & OPC_QUALITY_MASK) != OPC_QUALITY_GOOD)	MasterQual = S_FALSE; 

				// Also generate the Client Handle List to be returned
				//
				ItemHandleList[i] = pItem->GetHandle();

				// If an error were detected for this item
				// (which this sample code does not do)
				// Then we would also set MasterError to S_FALSE
				//
				ErrorList[i] = S_OK;

			}

			// Invoke the client callback
			//
			pDC->OnReadComplete(
					m_Async2ReadTID, 
					m_ClientGroupHandle,
					MasterQual, 
					MasterError, 
					m_Async2ReadActive, 
					ItemHandleList,
					ValueList, 
					QualList, 
					TimeList,
					ErrorList
			);

			// In any case the read is complete...
			// Free all the memory
			//
			for(i=0; i<m_Async2ReadActive; i++)
			{
				VariantClear(&ValueList[i]);
			}

			pIMalloc->Free( ItemHandleList);
			pIMalloc->Free( ValueList);
			pIMalloc->Free( QualList);
			pIMalloc->Free( TimeList);
			pIMalloc->Free( ErrorList);
			pIMalloc->Free( m_Async2ReadList);

			m_Async2ReadList = 0;
			m_Async2ReadActive = 0;
			m_Async2ReadCancel = 0;
			//

		}	// (else read not complete)
	}		// (else read not active)
}


///////////////////////////////////////
// XXXGroup::CheckCPAsyncWrite
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
void		XXXGroup::CheckCPAsyncWrite( IXXXCP * pCP )
{
	BOOL complete;
	int i;

	// If a Write is in progress
	//
	if(m_Async2WriteActive)
	{
		// Then find any connection point and callback stuff
		//
		IOPCDataCallback * pDC;
		pDC = (IOPCDataCallback *) pCP->m_Callback;		// note group.cpp already validated this

		// then check to see if a cancel was requested
		//
		if(m_Async2WriteCancel)
		{
			// And if so then Construct and send cancel complete
			//
			pDC->OnCancelComplete(m_Async2WriteTID, m_ClientGroupHandle);

			// cancel the Write
			//
			pIMalloc->Free(m_Async2WriteList);
			m_Async2WriteList = 0;
			m_Async2WriteActive = 0;
			m_Async2WriteCancel = 0;
			return;
		}

		// Else (if no cancel) then check progress of Write 
		//
		complete = 1;
		for(i=0; i<m_Async2WriteActive; i++)
		{
			OPCHANDLE h = m_Async2WriteList[i];
			XXXItem *pItem = ItemPtr(h);

			// See if any of the Writes are still pending
			//
			if (!pItem->CheckDeviceWrite(0))
				complete = 0;
		}

		// If Write is complete
		//
		if(complete)
		{
			// Construct and send Write complete
			//
			int i;
			HRESULT   *ResultList     = (HRESULT*) pIMalloc->Alloc(m_Async2WriteActive * sizeof(HRESULT));
			WORD      *QualList       = (WORD*) pIMalloc->Alloc(m_Async2WriteActive * sizeof(WORD));
			HRESULT    MasterHR       = S_OK;
			OPCHANDLE *ItemHandleList = (OPCHANDLE*)pIMalloc->Alloc(m_Async2WriteActive * sizeof( OPCHANDLE ));

			// A real server should do error checks for mallocs...

			// And fill them in
			//
			for(i=0; i<m_Async2WriteActive; i++)
			{
				OPCHANDLE h = m_Async2WriteList[i];

				// Get item write results
				//
				XXXItem *pItem = ItemPtr(h);
				pItem->CheckDeviceWrite(&ResultList[i]);
				if(ResultList[i] != S_OK) MasterHR = TRUE;

				// Also generate the Client Handle List to be returned
				//
				ItemHandleList[i] = pItem->GetHandle();
			}

			// Invoke the client callback
			//
			pDC->OnWriteComplete(
				m_Async2WriteTID, 
				m_ClientGroupHandle,
				MasterHR, 
				m_Async2WriteActive, 
				ItemHandleList,
				ResultList
			);

			// In any case the Write is complete...
			// Free all the buffers...
			//
			pIMalloc->Free( ItemHandleList);
			pIMalloc->Free(ResultList);
			pIMalloc->Free(QualList);
			pIMalloc->Free(m_Async2WriteList);

			m_Async2WriteList = 0;
			m_Async2WriteActive = 0;
			m_Async2WriteCancel = 0;

		}	// (else Write not complete)
	}		// (else Write not active)
}


///////////////////////////////////////
// XXXGroup::CheckCPFefresh
//
// This code is nearly identical to CheckCPAsyncRead except
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
void		XXXGroup::CheckCPRefresh( IXXXCP * pCP )
{
	BOOL complete;
	int i;

	// If a refresh is in progress
	//
	if(m_Refresh2Active)
	{
		// Then find any connection point and callback stuff
		//
		IOPCDataCallback * pDC;
		pDC = (IOPCDataCallback *) pCP->m_Callback;		// note group.cpp already validated this

		// then check to see if a cancel was requested
		//
		if(m_Refresh2Cancel)
		{
			// And if so then Construct and send cancel complete
			//
			pDC->OnCancelComplete(m_Refresh2TID, m_ClientGroupHandle);

			// in any case, cancel the refresh
			//
			pIMalloc->Free(m_Refresh2List);
			m_Refresh2List = 0;
			m_Refresh2Active = 0;
			m_Refresh2Cancel = 0;
			return;
		}

		// Else (if no cancel) then check progress of refresh 
		// If refresh was from DEVICE then see if it's done
		//
		complete = 1;
		if(m_Refresh2Source == OPC_DS_DEVICE)
		{
			for(i=0; i<m_Refresh2Active; i++)
			{
				OPCHANDLE h = m_Refresh2List[i];
				XXXItem *pItem = ItemPtr(h);
	
				// See if any of the DEVICE reads are still pending
				//
				if (!pItem->CheckDeviceRead(0))
					complete = 0;
			}
		}

		// If Device read is complete and data is ready
		//
		if(complete)
		{
			// Construct and send refresh complete
			//
			int i;
			VARIANT   *ValueList      = (VARIANT*) pIMalloc->Alloc(m_Refresh2Active * sizeof(VARIANT));
			WORD      *QualList       = (WORD*) pIMalloc->Alloc(m_Refresh2Active * sizeof(WORD));
			FILETIME  *TimeList       = (FILETIME*)pIMalloc->Alloc(m_Refresh2Active * sizeof(FILETIME));
			HRESULT   *ErrorList      = (HRESULT*) pIMalloc->Alloc(m_Refresh2Active * sizeof(HRESULT));
			HRESULT    MasterQual     = S_OK;
			HRESULT    MasterError    = S_OK;
			OPCHANDLE *ItemHandleList = (OPCHANDLE*)pIMalloc->Alloc(m_Refresh2Active * sizeof( OPCHANDLE ));

			// A real server should do error checks for mallocs...

			// Init the variants
			//
			for(i=0; i<m_Refresh2Active; i++)
			{
				VariantInit(&ValueList[i]);
			}

			// And fill them in
			//
			for(i=0; i<m_Refresh2Active; i++)
			{
				OPCHANDLE h = m_Refresh2List[i];

				// Get item value, time, quality.
				// Note that quality will reflect item active status 
				//
				XXXItem *pItem = ItemPtr(h);
				pItem->GetValue(m_Refresh2Source, &ValueList[i], &QualList[i], &TimeList[i]);

				// Note that a Refresh is a Special case of OnDataChange
				// So we DO clear the Changed flag
				//
				pItem->ClearChanged(OPC_ODC_CP);

				// We checked group active when request was queued
				// Although it could have changed since then
				// 
				if((QualList[i] & OPC_QUALITY_MASK) != OPC_QUALITY_GOOD)	MasterQual = 1; 

				// Also generate the Client Handle List to be returned
				//
				ItemHandleList[i] = pItem->GetHandle();

				// If an error were detected for this item
				// (which this sample code does not do)
				// Then we would also set MasterError to S_FALSE
				//
				ErrorList[i] = S_OK;
			}

			// Invoke the client callback
			//
			pDC->OnDataChange(
				m_Refresh2TID, 
				m_ClientGroupHandle,
				MasterQual, 
				MasterError, 
				m_Refresh2Active, 
				ItemHandleList, 
				ValueList, 
				QualList, 
				TimeList,
				ErrorList
				);

			// In any case the refresh is complete...
			// Free all the buffers
			//
			for(i=0; i<m_Refresh2Active; i++)
			{
				VariantClear(&ValueList[i]);
			}

			pIMalloc->Free( ItemHandleList);
			pIMalloc->Free( ValueList);
			pIMalloc->Free( QualList);
			pIMalloc->Free( TimeList);
			pIMalloc->Free( ErrorList);
			pIMalloc->Free( m_Refresh2List);

			m_Refresh2List = 0;
			m_Refresh2Active = 0;
			m_Refresh2Cancel = 0;

		}	// (else refresh not complete)
	}		// (else refresh not active)
}

