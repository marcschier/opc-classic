// ParamsResults.h: 
//////////////////////////////////////////////////////////////////////


#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "OPCDA.h"

HRESULT  Try_ParametersResults(IUnknown* pOPCBatchServer);

HRESULT  UseAnonymous(IUnknown* pOPCBatchServer, const char* szType, BOOL bParams);
