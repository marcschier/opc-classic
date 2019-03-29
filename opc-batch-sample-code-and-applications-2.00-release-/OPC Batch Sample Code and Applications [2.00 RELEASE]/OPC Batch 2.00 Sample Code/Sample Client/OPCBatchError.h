/*++
Module Name:
 OpcBatchError.h
Author:
OPC Batch Committee

Revision History:
Release 1.0
    initial version for 1.0 spec

--*/

/*
Code Assignements:
  0000 to 0200 are reserved for Microsoft use 
  (although some were inadverdantly used for OPC 1.0 errors). 

  0200 to 8000 are reserved for future OPC use. 
    of these, 0300 to 03ff are reserved for future OPC Batch use

  8000 to FFFF can be vendor specific.

*/


//
// MessageId: OPCB_E_NOT_MEANINGFUL
//
// MessageText:
//
//  The data is not meaningful at the present time
//
#define OPCB_E_NOT_MEANINGFUL               ((HRESULT)0xC0040300L)
