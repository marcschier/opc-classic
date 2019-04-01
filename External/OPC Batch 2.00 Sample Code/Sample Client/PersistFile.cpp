// PersistFile.cpp: implementation of the CPersistFile class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "OPCBClient.h"
#include "PersistFile.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CPersistFile::CPersistFile()
{
   m_pIPersistFile   = NULL;
   m_pIMalloc        = NULL;

}

CPersistFile::~CPersistFile()
{
   CleanUp();
}

void CPersistFile::CleanUp()
{
   if (m_pIPersistFile) m_pIPersistFile->Release();
   if (m_pIMalloc)      m_pIMalloc->Release();
};

