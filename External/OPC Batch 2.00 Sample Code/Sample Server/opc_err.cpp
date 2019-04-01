// I_Server.cpp
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
//  This file contains a common subroutine used to obtain Error Strings.
//
//
// Modification Log:
//  Vers    Date   By    Notes
//  ----  -------- ---   -----
//  2.00  08/11/00 ACC/MD
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCERROR.h"

///////////////////////////////////////
// GetOPCError()
//
// For OPC error codes we need to return a user displayable string 
// Another way to do this in a real server would probably be
// to put the strings in the RC file and use LoadString
//
///////////////////////////////////////

HRESULT GetOPCError( HRESULT hr, LPWSTR *ppString)
{

	// A real server would obtain the string from a resource file
	switch (hr)
	{
		case OPC_E_INVALIDHANDLE:
			*ppString = WSTRClone( L"The value of the handle is invalid.", pIMalloc); 
			break;
		case OPC_E_BADTYPE:
			*ppString = WSTRClone( L"The server cannot convert the data between the requested data type and the canonical data type.", pIMalloc); 
			break; 
		case OPC_E_PUBLIC:
			*ppString = WSTRClone( L"The requested operation cannot be done on a public group.", pIMalloc); 
			break;
		case OPC_E_BADRIGHTS:
			*ppString = WSTRClone( L"The Items AccessRights do not allow the operation.", pIMalloc); 
			break; 
		case OPC_E_UNKNOWNITEMID:
			*ppString = WSTRClone( L"The item is no longer available in the server address space.", pIMalloc); 
			break; 
		case OPC_E_INVALIDITEMID:
			*ppString = WSTRClone( L"The item definition doesn't conform to the server's syntax.", pIMalloc); 
			break; 
		case OPC_E_INVALIDFILTER:
			*ppString = WSTRClone( L"The filter string was not valid.", pIMalloc); 
			break; 
		case OPC_E_UNKNOWNPATH:
			*ppString = WSTRClone( L"The item's access path is not known to the server.", pIMalloc); 
			break; 
		case OPC_E_RANGE:
			*ppString = WSTRClone( L"The value was out of range.", pIMalloc); 
			break; 
		case OPC_E_DUPLICATENAME:
			*ppString = WSTRClone( L"Duplicate name not allowed.", pIMalloc); 
			break; 
		case OPC_S_UNSUPPORTEDRATE:
			*ppString = WSTRClone( L"The server does not support the requested data rate but will use the closest available rate.", pIMalloc); 
			break; 
		case OPC_S_CLAMP:
			*ppString = WSTRClone( L"A value passed to WRITE was accepted but the output was clamped.", pIMalloc); 
			break; 
		case OPC_S_INUSE:
			*ppString = WSTRClone( L"The operation cannot be completed because the object still has references that exist.", pIMalloc); 
			break; 
		case OPC_E_INVALIDCONFIGFILE:
			*ppString = WSTRClone( L"The server's configuration file is an invalid format.", pIMalloc); 
			break; 
		case OPC_E_NOTFOUND:
			*ppString = WSTRClone( L"The server could not locate the requested object.", pIMalloc); 
			break; 
		case OPC_E_INVALID_PID:
			*ppString = WSTRClone( L"The server does not recognise the passed property ID.", pIMalloc); 
			break; 

		// Add additional vendor specific errors here....

		default:
			TCHAR   tmpResult[1000];
			if (FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, NULL,
								hr, 0,
								tmpResult, 1000, NULL) == 0)
			{
				*ppString = WSTRClone( L"Unknown error", pIMalloc); 
			}
			else
			{
				int	length, i;
				WCHAR	*temp;

				length = strlen(tmpResult) + 1;
				temp = (WCHAR*)pIMalloc->Alloc(sizeof(WCHAR) * length);
				if(temp)
					for(i=0; i<length; i++) temp[i] = (WCHAR) tmpResult[i];
				else
				{
					*ppString = NULL;
					return E_OUTOFMEMORY;
				}
				*ppString = temp;
			}
	}

	return S_OK;
}