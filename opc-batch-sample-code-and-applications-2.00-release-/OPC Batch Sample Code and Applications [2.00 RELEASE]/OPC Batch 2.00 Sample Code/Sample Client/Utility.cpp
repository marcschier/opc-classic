// Utility.cpp : implementation of various utility functions
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

#include "stdafx.h"

#include "utility.h"

#include "OPCBClientDoc.h"
#include "OPCBClientView.h"
#include "MainFrm.h"
#include "afxpriv.h"
#include "OPCBatchError.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif





/////////////////////////////////////////////////////////////////////////////
// Utility Functions


//---------------------------------------------------------
// display a line of output in the main list window
//
void DisplayOutput(DWORD dwFlags, LPCTSTR pFormat, ...)
{
   CFrameWnd* pFrame = (CFrameWnd*)AfxGetMainWnd();
   COPCBClientView* pView = (COPCBClientView*)pFrame->GetActiveView();

   // Check flags for verbose setting.
   // if this is a verbose message, and the user doesn't want verbose, then return
   if (dwFlags == VERBOSE_OUT && !pView->m_bVerboseOutput)
      return;

   // Format message.
   va_list pArgs;
   CString strOutput;

   va_start(pArgs, pFormat);
   strOutput.FormatV(pFormat, pArgs);


   if (pView->m_bScrollDown)
   {
      CListCtrl& list = pView->GetListCtrl();
      list.EnsureVisible(list.InsertItem(list.GetItemCount(), strOutput), FALSE);
   }
   else
   {
      pView->GetListCtrl().InsertItem(0, strOutput);
   }

   
   if (pView->m_bLogToFile)
   {
      // since this is a prototype and prone to problems, 
      // let's open and close file each time.
      CFile fLog;

      // make a file name
      static CString strName;

      if (strName.GetLength() == 0)
      {
         CTime t = CTime::GetCurrentTime();
         strName.Format("Log %s.txt", t.FormatGmt("%m%d%y %H%M%S"));
      }

      if (fLog.Open(strName, CFile::modeCreate | CFile::modeNoTruncate | CFile::modeWrite))
      {
         fLog.SeekToEnd();
         strOutput += _T("\n");
         fLog.Write(strOutput, strOutput.GetLength());
         fLog.Close();
      }
      
   }
   
   
   
   // Done with parameters.
   va_end(pArgs);
}



//---------------------------------------------------------
// update the current browse path on the status bar
//
void UpdatePath(LPCTSTR szPath)
{

   CMainFrame* pFrame = (CMainFrame*)AfxGetMainWnd();

   pFrame->SetPath(szPath);


}



//---------------------------------------------------------
// used to report QueryInterface results - Thanks Al
//
void	Report(DWORD dwFlags, char* outstring, HRESULT hRes)
{
	
	if(FAILED(hRes)) 
      DisplayOutput(dwFlags, _T("%s NOT SUPPORTED(%lx)"), outstring, hRes);
	else 
      DisplayOutput(dwFlags, _T("%s Supported"), outstring);

}



//---------------------------------------------------------
// Put the variant data into CString format so we can display - ala Al
//
void	FormatVariant(CString& strOut, VARIANT *v)
{
   //needed for OLE2T macro below, include afxpriv.h
   USES_CONVERSION;
   
   switch(v->vt)
	{
	case VT_I2:
		strOut.Format(" value(VT_I2)   = %d ", v->iVal );
		break;
	case VT_I4:
		strOut.Format(" value(VT_I4)   = %ld ", v->lVal );
		break;
	case VT_R4:
		strOut.Format(" value(VT_R4)   = %5.2f ", v->fltVal );
		break;
	case VT_R8:
		strOut.Format(" value(VT_R8)   = %9.4f ", v->dblVal );
		break;
	case VT_BSTR:
		strOut.Format(" value(VT_BSTR) = %ls ", v->bstrVal );
		break;
	case VT_DATE:
      {
         COleDateTime t(v->date);
		   strOut.Format(" value(VT_DATE) = %s ", t.Format("%X %x"));
      }
      break;
   case VT_ARRAY | VT_BSTR:
      {
         COleSafeArray sa(v);

         BSTR *pbstr;
         LONG cElements, lLBound, lUBound;
  
         try 
         {
            // Get array bounds.
            sa.GetLBound(1, &lLBound);
            sa.GetUBound(1, &lUBound);

            // Get a pointer to the elements of the array
            // and increments the lock count on the array
            sa.AccessData((LPVOID*)&pbstr);

            strOut.Format(" value(VT_ARRAY | VT_BSTR) ");

            //get no. of elements in array
            cElements = lUBound-lLBound+1;
         
            if (cElements == 0)
              strOut += "(empty)";

            for (int i = 0; i < cElements; i++)
            {
               //output the elements of the array
               strOut += ", ";
               strOut += OLE2T(pbstr[i]);
            }
      
            //decrement lock count
            sa.UnaccessData();
         }
         catch (COleException *pEx)
         {
            strOut.Format(" value(VT_ARRAY | VT_BSTR) Unexpected Error while processing" );
            pEx->Delete();
         }

      }
      break;
   case VT_ARRAY | VT_VARIANT:
      {
         COleSafeArray sa(v);
         CString strTemp;
         VARIANT * pvElement;
         LONG cElements, lLBound, lUBound;
  
         try 
         {
            // Get array bounds.
            sa.GetLBound(1, &lLBound);
            sa.GetUBound(1, &lUBound);

            // Get a pointer to the elements of the array
            // and increments the lock count on the array
            sa.AccessData((LPVOID*)&pvElement);

            strOut.Format(" value(VT_ARRAY | VT_VARIANT) ");

            //get no. of elements in array
            cElements = lUBound-lLBound+1;

            if (cElements == 0)
              strOut += "(empty)";

            for (int i = 0; i < cElements-1; i++)
            {
               //output the elements of the array
               FormatVariant(strTemp, &pvElement[i]);     // get a little recursive...
               strOut += strTemp;
            }
      
            //decrement lock count
            sa.UnaccessData();
         }
         catch (COleException *pEx)
         {
            strOut.Format(" value(VT_ARRAY | VT_VARIANT) Unexpected Error while processing" );
            pEx->Delete();
         }

      }
      break;
      
	default:
		strOut.Format(" value(unknown type:%d) ", v->vt );
		break;
	}
}

// Credit goes to Gregory Khrapunovich for this function
// This function interprests the return code and returns a string
// contaning the value of HRESULT and its description

CString FormatHresult(HRESULT hr)
{
	CString szHr, szDesc;
	switch (hr) {
	case E_UNEXPECTED:
		szDesc = _T("Unexpected failure");
		break;
	case E_NOTIMPL:
		szDesc = _T("Not implemented");
		break;
	case E_OUTOFMEMORY:
		szDesc = _T("Out of memory");
		break;
	case E_INVALIDARG:
		szDesc = _T("Invalid argument(s)");
		break;
	case E_NOINTERFACE:
		szDesc = _T("No such interface supported");
		break;
	case E_POINTER:
		szDesc = _T("Invalid pointer");
		break;
	case E_HANDLE:
		szDesc = _T("Invalid handle");
		break;
	case E_ABORT:
		szDesc = _T("Operation aborted");
		break;
	case E_FAIL:
		szDesc = _T("Unspecified failure");
		break;
	case E_ACCESSDENIED:
		szDesc = _T("General access denied error");
		break;
	case S_OK:
		szDesc = _T("Success");
		break;
	case S_FALSE:
		szDesc = _T("S_FALSE");
		break;
	case REGDB_E_CLASSNOTREG:
		szDesc = _T("Class not registered or missing");
		break;
	case OPCB_E_NOT_MEANINGFUL:
   	szDesc = _T("The data is not meaningful at the present time");
	break;

	default:
		szDesc = _T("");
	}
	szHr.Format(_T("0x%X  %s"), hr, szDesc);
	return szHr;
}

