//////////////////////////////////////////////////////////////////////
// OPCCommon.cpp: implementation of the COPCCommon class, a wrapper for IOPCCommon.
//

#include "stdafx.h"
#include "OPCCommon.h"

#include "utility.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

COPCCommon::COPCCommon()
{
   m_pIOPCServer = NULL;
}

COPCCommon::~COPCCommon()
{
   if (m_pIOPCCommon)
      m_pIOPCCommon->Release();
}


HRESULT COPCCommon::Initialize(IUnknown* pOPCBatchServer)
{
   // initialize member pointer to IOPCServer interface.
   HRESULT hRes = pOPCBatchServer->QueryInterface(IID_IOPCCommon, (void**)&m_pIOPCCommon);
   if (FAILED(hRes))
      m_pIOPCCommon = NULL;   //$TODO is this necessary

   return hRes;
}




//////////////////////////////////////////////////////////////////////
// Method Test Functions
//////////////////////////////////////////////////////////////////////



HRESULT COPCCommon::Try_GetErrorString()
{

   DisplayOutput(NORMAL_OUT, _T("IOPCCommon::GetErrorString Not Fully Implemented.  Need server connection."));

   return E_NOTIMPL;
}

