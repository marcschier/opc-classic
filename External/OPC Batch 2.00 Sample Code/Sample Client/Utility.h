// Utility.h : header for implementation of various utility functions
//
/////////////////////////////////////////////////////////////////////////////
// (c) Copyright 2000 The OPC Foundation
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
//  Dave Emerson and Jack Beaudry, Yokogawa Electric Company.
//  This application is based on the code for the sample DA client
//  generously provided by Al Chisholm, Intellution Inc.
//
// CONTENTS:
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
// .4    01/12/00 jb    preliminary release for testing.
//

#pragma once

// display flags
const int VERBOSE_OUT   = 0;
const int NORMAL_OUT    = 1;
const int ERROR_OUT     = 2;

// display a line of output in the main list window
void  DisplayOutput(
         DWORD dwFlags,       // one of the display flags
         LPCTSTR pFormat,     // printf type format string
         ...);                // variable parameter list for printf type formatting

// used to report QueryInterface results - Thanks Al
void	Report(
         DWORD dwFlags, 
         char*n, 
         HRESULT hRes);

// update the current browse path on the status bar
void  UpdatePath(LPCTSTR szPath);

// put the variant data into CString format so we can display - ala Al
void	FormatVariant(
         CString& strOut,     // CString to recieve variant data
         VARIANT *v);         // variant data holder


// Credit goes to Gregory Khrapunovich for this function
CString FormatHresult(HRESULT hr);

