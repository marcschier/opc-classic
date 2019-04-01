//============================================================================
// TITLE: StdAfx.h
//
// CONTENTS:
// 
// This file standard precompiled include files.
//
// (c) Copyright 2003-2004 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This code is provided by the OPC Foundation solely to assist in 
//  understanding and use of the appropriate OPC Specification(s) and may be 
//  used as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// MODIFICATION LOG:
//
// Date       By    Notes
// ---------- ---   -----
// 2003/12/17 RSA   Initial implementation.

#ifndef _OpcDa_StdAfx_H
#define _OpcDa_StdAfx_H

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

// Insert your headers here
#define WIN32_LEAN_AND_MEAN // Exclude rarely-used stuff from Windows headers

#include <windows.h>
#include <stdio.h>
#include <tchar.h>
#include <objbase.h>
#include <olectl.h>
#include <comcat.h>

#define _USE_MATH_DEFINES 1

#include <float.h>
#include <limits.h>
#include <math.h>

#include "OpcUtils.h"

#include "opcda.h"
#include "opchda.h"
#include "OpcHda_Error.h"
#include "opcerror.h"

#endif // _OpcDa_StdAfx_H
