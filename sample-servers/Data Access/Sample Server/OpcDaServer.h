

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Mon Apr 01 10:19:44 2019
 */
/* Compiler settings for OpcDaServer.idl:
    Oicf, W1, Zp8, env=Win32 (32b run), target_arch=X86 8.00.0603 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 500
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__


#ifndef __OpcDaServer_h__
#define __OpcDaServer_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __OpcDaServer_FWD_DEFINED__
#define __OpcDaServer_FWD_DEFINED__

#ifdef __cplusplus
typedef class OpcDaServer OpcDaServer;
#else
typedef struct OpcDaServer OpcDaServer;
#endif /* __cplusplus */

#endif 	/* __OpcDaServer_FWD_DEFINED__ */


/* header files for imported files */
#include "opccomn.h"
#include "opcda.h"

#ifdef __cplusplus
extern "C"{
#endif 



#ifndef __OpcDaServerLib_LIBRARY_DEFINED__
#define __OpcDaServerLib_LIBRARY_DEFINED__

/* library OpcDaServerLib */
/* [version][uuid] */ 


EXTERN_C const IID LIBID_OpcDaServerLib;

EXTERN_C const CLSID CLSID_OpcDaServer;

#ifdef __cplusplus

class DECLSPEC_UUID("625C49A1-BE1C-45d7-9A8A-14BEDCF5CE6C")
OpcDaServer;
#endif
#endif /* __OpcDaServerLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


