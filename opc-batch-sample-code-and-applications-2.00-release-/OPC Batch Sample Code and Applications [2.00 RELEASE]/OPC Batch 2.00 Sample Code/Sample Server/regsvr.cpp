// regsvr.cpp
//
// (c) Copyright 1998,1999 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Batch Specification and may be used
//  as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by
//  Al Chisholm, Intellution Inc.  Modifications for OPC Batch 
//  by Will Irwin, Fisher-Rosemount Systems Inc.
//
// CONTENTS:
//
//  This source file contains fucntions which Register/Unregister 
//  this sample OPC server.
//
//  CAUTION: if you register both a DLL and EXE (or Handler) then
//  UnRegister will delete the common entries. You will need to re-register
//  the other servers.
//
//
// Modification Log:
// Vers    Date   By    Notes
// ----  -------- ---   -----
// 2.00  04/06/98 acc  
// 3.00  11/12/99 wgi   Change the ProgID and ClassID since this Batch
//                      sample server is different from acc's original
//                      data access server.


#define WIN32_LEAN_AND_MEAN
#include "windows.h"
#include "OLE2.h"

#define GUID_SIZE 128
#define MAX_STRING_LENGTH 256
HRESULT A_StringFromGUID2(REFGUID guid, LPSTR pszGUID, int cch);
HRESULT UcToAnsi(LPWSTR pszUc, LPSTR pszAnsi, int cch);

/*F+F++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ 
  Function: SetRegKeyValue 
 
  Summary:  Internal utility function to set a Key, Subkey, and value 
            in the system Registry under HKEY_CLASSES_ROOT. 
 
  Args:     char * pszKey, 
            char * pszSubkey, 
            char * pszValue) 
 
  Returns:  BOOL 
              TRUE if success; FALSE if not. 
------------------------------------------------------------------------F-F*/ 
BOOL SetRegKeyValue(LPTSTR pszKey, LPTSTR pszSubkey, LPTSTR pszValue)
{
	BOOL bOk = FALSE;
	LONG ec;
	HKEY hKey;
	TCHAR szKey[MAX_STRING_LENGTH];


	lstrcpy(szKey, pszKey);

	if (NULL != pszSubkey)
	{
		lstrcat(szKey, TEXT("\\"));
		lstrcat(szKey, pszSubkey);
	}

	ec = RegCreateKeyEx(HKEY_CLASSES_ROOT, szKey, 0, NULL,
		REG_OPTION_NON_VOLATILE, KEY_ALL_ACCESS, NULL, &hKey, NULL);
	if (NULL != pszValue && ERROR_SUCCESS == ec)
	{
		ec = RegSetValueEx(hKey, NULL, 0, REG_SZ, (BYTE *)pszValue,
			(lstrlen(pszValue)+1)*sizeof(TCHAR));
		
		if (ERROR_SUCCESS == ec)
			bOk = TRUE;

		RegCloseKey(hKey);
	}

	return bOk;
}
 
/*M+M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M 
  Method:   RegisterServer 
 
  Summary:  Member function used by this server to register itself in the 
            system registry. 
 
  Args:     void. 

Example:

NOTE: keys must be added 'top down'

1. HKEY_CLASSES_ROOT\CLSID\{E53B21C7-990E-11d3-B3E4-00C04F8ECEAA} = OPC Batch Sample
2. HKEY_CLASSES_ROOT\CLSID\{E53B21C7-990E-11d3-B3E4-00C04F8ECEAA}\ProgID = OPC.BatchSample.1
3. HKEY_CLASSES_ROOT\CLSID\{E53B21C7-990E-11d3-B3E4-00C04F8ECEAA}\LocalServer32 = d:\opcsvr20\release\opc_exe.exe

4. HKEY_CLASSES_ROOT\OPC.BatchSample.1 = OPC Batch Sample
5. HKEY_CLASSES_ROOT\OPC.BatchSample.1\CLSID = {E53B21C7-990E-11d3-B3E4-00C04F8ECEAA}
6. HKEY_CLASSES_ROOT\OPC.BatchSample.1\OPC

M---M---M---M---M---M---M---M---M---M---M---M---M---M---M---M---M---M---M-M*/ 
void	  OPCQuickReg(GUID clsid, TCHAR * path, TCHAR *progid,TCHAR *Type, TCHAR *desc);
void	  OPCQuickReg(GUID clsid, TCHAR * path, TCHAR *progid,TCHAR *Type, TCHAR *desc)
{ 
  TCHAR szID[GUID_SIZE+1]; 
  TCHAR szCLSID[GUID_SIZE+127]; 
  HRESULT hr;


  /*------------------------------------------------------------------------- 
    Create registry entries for the Component. 
  -------------------------------------------------------------------------*/ 
  // Create some base key strings. 
  A_StringFromGUID2(clsid, szID, GUID_SIZE);
  lstrcpy(szCLSID, TEXT("CLSID\\")); 
  lstrcat(szCLSID, szID); 
 
 
  // Create entries under CLSID. 
  // line 1
  hr = SetRegKeyValue( 
    szCLSID, 
    NULL, 
    desc); 

  // line 2
  hr = SetRegKeyValue( 
    szCLSID, 
    TEXT("ProgID"), 
    progid); 

  // line 3
  hr = SetRegKeyValue( 
    szCLSID, 
    Type, 
    path); 

  // Create ProgID keys. 
  // line 4
  hr = SetRegKeyValue( 
    progid, 
    NULL, 
    desc); 

  // line 5
  hr = SetRegKeyValue( 
    progid, 
    TEXT("CLSID"), 
    szID); 

  // line 6
  hr = SetRegKeyValue( 
    progid, 
    TEXT("OPC"),	// for V1 compatability
    szID); 
 
} 
 
 
/*M+M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M+++M 
  Method:   UnregisterServer 
 
  Summary:  Member function used by this server to unregister itself from 
            the system Registry. 
 
Example:

  NOTE subkeys must be deleted BEFORE keys !!

1. HKEY_CLASSES_ROOT\CLSID\{E53B21C7-990E-11d3-B3E4-00C04F8ECEAA} = OPC Batch Sample
2. HKEY_CLASSES_ROOT\CLSID\{E53B21C7-990E-11d3-B3E4-00C04F8ECEAA}\ProgID = OPC.BatchSample.1
3. HKEY_CLASSES_ROOT\CLSID\{E53B21C7-990E-11d3-B3E4-00C04F8ECEAA}\LocalServer32 = d:\opcsvr20\release\opc_exe.exe
x. HKEY_CLASSES_ROOT\CLSID\{E53B21C7-990E-11d3-B3E4-00C04F8ECEAA}\Implemented Categories...

4. HKEY_CLASSES_ROOT\OPC.BatchSample.1 = OPC Sample
5. HKEY_CLASSES_ROOT\OPC.BatchSample.1\CLSID = {E53B21C7-990E-11d3-B3E4-00C04F8ECEAA}
6. HKEY_CLASSES_ROOT\OPC.BatchSample.1\OPC

M---M---M---M---M---M---M---M---M---M---M---M---M---M---M---M---M---M---M-M*/ 
void	  OPCQuickUnReg(GUID clsid, TCHAR *progid, TCHAR *Type)
{ 
  BOOL  bOk = TRUE; 
  TCHAR szID[GUID_SIZE+1]; 
  TCHAR szCLSID[GUID_SIZE+1]; 
  TCHAR szTemp[GUID_SIZE+127]; 
  HRESULT hr;
 
  /*------------------------------------------------------------------------- 
    Delete registry entries for the Component (in reverse order). 
  -------------------------------------------------------------------------*/ 
  //Create some base key strings. 
  A_StringFromGUID2(clsid, szID, GUID_SIZE);
  lstrcpy(szCLSID, TEXT("CLSID\\")); 
  lstrcat(szCLSID, szID); 
 
  // line 6
  wsprintf(szTemp, TEXT("%s\\%s"), progid, TEXT("OPC")); // V1 key
  hr = RegDeleteKey(HKEY_CLASSES_ROOT, szTemp);

  // line 5
  wsprintf(szTemp, TEXT("%s\\%s"), progid, TEXT("CLSID")); 
  hr = RegDeleteKey(HKEY_CLASSES_ROOT, szTemp);

  // line 4
  hr = RegDeleteKey(HKEY_CLASSES_ROOT, progid); 

  // line x (Side effect of Category Use - must be removec manually)
  // This will only succeed if we have called UnRegisterClassImplCategories
  wsprintf(szTemp, TEXT("%s\\%s"), szCLSID, TEXT("Implemented Categories"));
  hr = RegDeleteKey(HKEY_CLASSES_ROOT, szTemp); 
  
  // line 3
  wsprintf(szTemp, TEXT("%s\\%s"), szCLSID, Type); 
  hr = RegDeleteKey(HKEY_CLASSES_ROOT, szTemp); 
  
  // line 2
  wsprintf(szTemp, TEXT("%s\\%s"), szCLSID, TEXT("ProgID")); 
  hr = RegDeleteKey(HKEY_CLASSES_ROOT, szTemp); 
 
  // line 1
  hr = RegDeleteKey(HKEY_CLASSES_ROOT, szCLSID); 
 
} 



//---(FUNCTION)----------------------------------------------------
//
// @func	HRESULT | A_StringFromGUID2 | ANSI Surrogate for the OLE
//			Unicode API call.
//
// @parm	REFGUID | guid | Interface identifier to be converted
//
// @parm	LPSTR | pszGUID | Pointer to the resulting string on return
//
// @parm	int | cch | Maximum size the returned string is expected to be
//
// @rdesc	This function returns an HRESULT as follows
//
// @flag	E_INVALIDARG | A parameter was invalid
// @flag	E_OUTOFMEMORY | Memory could not be allocated for Unicode string
// @flag	| Return value of StringFromGUID2() call
//
HRESULT A_StringFromGUID2(REFGUID guid, LPSTR pszGUID, int cch)
{
	HRESULT hr = E_INVALIDARG;
	LPWSTR  pszUc;
	IMalloc* pIMalloc;
	
	
	if (NULL != pszGUID && 0 < cch)
	{
		hr = CoGetMalloc(MEMCTX_TASK, &pIMalloc);
		if (SUCCEEDED(hr))
		{
			pszUc = (LPWSTR)pIMalloc->Alloc((cch+1)*sizeof(TCHAR));
			pIMalloc->Release();
			if (NULL != pszUc)
			{
				hr = StringFromGUID2(guid, pszUc, cch);
				if (SUCCEEDED(hr))
					hr = UcToAnsi(pszUc, pszGUID, cch);
				
				CoTaskMemFree((void *)pszUc);
			}
			else
				hr = E_OUTOFMEMORY;
		}
	}
	
	return hr;
}


//---(FUNCTION)----------------------------------------------------
//
// @func	HRESULT | UcToAnsi | Convert a UNICODE input string to an
//			output ANSI string
//
// @parm	LPWSTR | pszUc | Pointer to an input wide UNICODE string
//
// @parm	LPSTR | pszAnsi | Pointer to output ANSI string
//
// @parm	int | cch | Character count
//
// @rdesc	This function returns an HRESULT as follows
//
// @flag	S_OK | Success
// @flag	E_FAIL | Not successful
//
HRESULT UcToAnsi(LPWSTR pszUc, LPSTR pszAnsi, int cch)
{
	HRESULT hr = E_FAIL;
	int cSize;
	int cOut;
	
	
	if (0 == cch)
		cSize = WideCharToMultiByte(CP_ACP, 0, pszUc, -1, NULL, 0, NULL,
		NULL);
	else
		cSize = cch;
	
	if (0 != cSize)
	{
		cOut = WideCharToMultiByte(CP_ACP, 0, pszUc, -1, pszAnsi, cSize,
			NULL, NULL);
		if (0 != cOut)
			hr = S_OK;
	}
	
	return hr;
}