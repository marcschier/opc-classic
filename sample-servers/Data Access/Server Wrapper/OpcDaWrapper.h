

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Sat Mar 30 12:04:41 2019
 */
/* Compiler settings for OpcDaWrapper.idl:
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

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __OpcDaWrapper_h__
#define __OpcDaWrapper_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IOPCWrappedServer_FWD_DEFINED__
#define __IOPCWrappedServer_FWD_DEFINED__
typedef interface IOPCWrappedServer IOPCWrappedServer;

#endif 	/* __IOPCWrappedServer_FWD_DEFINED__ */


#ifndef __OpcDaWrapper_FWD_DEFINED__
#define __OpcDaWrapper_FWD_DEFINED__

#ifdef __cplusplus
typedef class OpcDaWrapper OpcDaWrapper;
#else
typedef struct OpcDaWrapper OpcDaWrapper;
#endif /* __cplusplus */

#endif 	/* __OpcDaWrapper_FWD_DEFINED__ */


/* header files for imported files */
#include "opccomn.h"
#include "opcda.h"

#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __IOPCWrappedServer_INTERFACE_DEFINED__
#define __IOPCWrappedServer_INTERFACE_DEFINED__

/* interface IOPCWrappedServer */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_IOPCWrappedServer;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("50E8496C-FA60-46a4-AF72-512494C664C6")
    IOPCWrappedServer : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE Load( 
            /* [in] */ REFCLSID tClsid) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE Unload( void) = 0;
        
    };
    
    
#else 	/* C style interface */

    typedef struct IOPCWrappedServerVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IOPCWrappedServer * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            _COM_Outptr_  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IOPCWrappedServer * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IOPCWrappedServer * This);
        
        HRESULT ( STDMETHODCALLTYPE *Load )( 
            IOPCWrappedServer * This,
            /* [in] */ REFCLSID tClsid);
        
        HRESULT ( STDMETHODCALLTYPE *Unload )( 
            IOPCWrappedServer * This);
        
        END_INTERFACE
    } IOPCWrappedServerVtbl;

    interface IOPCWrappedServer
    {
        CONST_VTBL struct IOPCWrappedServerVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IOPCWrappedServer_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IOPCWrappedServer_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IOPCWrappedServer_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IOPCWrappedServer_Load(This,tClsid)	\
    ( (This)->lpVtbl -> Load(This,tClsid) ) 

#define IOPCWrappedServer_Unload(This)	\
    ( (This)->lpVtbl -> Unload(This) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IOPCWrappedServer_INTERFACE_DEFINED__ */



#ifndef __OpcDaServerLib_LIBRARY_DEFINED__
#define __OpcDaServerLib_LIBRARY_DEFINED__

/* library OpcDaServerLib */
/* [version][uuid] */ 


EXTERN_C const IID LIBID_OpcDaServerLib;

EXTERN_C const CLSID CLSID_OpcDaWrapper;

#ifdef __cplusplus

class DECLSPEC_UUID("1437DC7F-D66E-4aa3-BA79-2CD0A4A6F3D8")
OpcDaWrapper;
#endif
#endif /* __OpcDaServerLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


