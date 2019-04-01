/* this ALWAYS GENERATED file contains the definitions for the interfaces */


/* File created by MIDL compiler version 3.01.75 */
/* at Mon Nov 10 10:44:26 1997
 */
/* Compiler settings for OPC_AE_SampleServer.idl:
    Oicf (OptLev=i2), W1, Zp8, env=Win32, ms_ext, c_ext
    error checks: none
*/
//@@MIDL_FILE_HEADING(  )
#include "rpc.h"
#include "rpcndr.h"
#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __OPC_AE_SampleServer_h__
#define __OPC_AE_SampleServer_h__

#ifdef __cplusplus
extern "C"{
#endif 

/* Forward Declarations */ 

#ifndef __IOPCEventServer_FWD_DEFINED__
#define __IOPCEventServer_FWD_DEFINED__
typedef interface IOPCEventServer IOPCEventServer;
#endif 	/* __IOPCEventServer_FWD_DEFINED__ */


#ifndef __IOPCEventSubscriptionMgt_FWD_DEFINED__
#define __IOPCEventSubscriptionMgt_FWD_DEFINED__
typedef interface IOPCEventSubscriptionMgt IOPCEventSubscriptionMgt;
#endif 	/* __IOPCEventSubscriptionMgt_FWD_DEFINED__ */


#ifndef __IOPCBrowseServerAreaSpace_FWD_DEFINED__
#define __IOPCBrowseServerAreaSpace_FWD_DEFINED__
typedef interface IOPCBrowseServerAreaSpace IOPCBrowseServerAreaSpace;
#endif 	/* __IOPCBrowseServerAreaSpace_FWD_DEFINED__ */


#ifndef __IOPCBrowseConditions_FWD_DEFINED__
#define __IOPCBrowseConditions_FWD_DEFINED__
typedef interface IOPCBrowseConditions IOPCBrowseConditions;
#endif 	/* __IOPCBrowseConditions_FWD_DEFINED__ */


#ifndef __OPCEventServer_FWD_DEFINED__
#define __OPCEventServer_FWD_DEFINED__

#ifdef __cplusplus
typedef class OPCEventServer OPCEventServer;
#else
typedef struct OPCEventServer OPCEventServer;
#endif /* __cplusplus */

#endif 	/* __OPCEventServer_FWD_DEFINED__ */


#ifndef __OPCEventSubscriptionMgt_FWD_DEFINED__
#define __OPCEventSubscriptionMgt_FWD_DEFINED__

#ifdef __cplusplus
typedef class OPCEventSubscriptionMgt OPCEventSubscriptionMgt;
#else
typedef struct OPCEventSubscriptionMgt OPCEventSubscriptionMgt;
#endif /* __cplusplus */

#endif 	/* __OPCEventSubscriptionMgt_FWD_DEFINED__ */


#ifndef __OPCBrowseServerAreaSpace_FWD_DEFINED__
#define __OPCBrowseServerAreaSpace_FWD_DEFINED__

#ifdef __cplusplus
typedef class OPCBrowseServerAreaSpace OPCBrowseServerAreaSpace;
#else
typedef struct OPCBrowseServerAreaSpace OPCBrowseServerAreaSpace;
#endif /* __cplusplus */

#endif 	/* __OPCBrowseServerAreaSpace_FWD_DEFINED__ */


#ifndef __OPCBrowseConditions_FWD_DEFINED__
#define __OPCBrowseConditions_FWD_DEFINED__

#ifdef __cplusplus
typedef class OPCBrowseConditions OPCBrowseConditions;
#else
typedef struct OPCBrowseConditions OPCBrowseConditions;
#endif /* __cplusplus */

#endif 	/* __OPCBrowseConditions_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

void __RPC_FAR * __RPC_USER MIDL_user_allocate(size_t);
void __RPC_USER MIDL_user_free( void __RPC_FAR * ); 

#ifndef __IOPCEventServer_INTERFACE_DEFINED__
#define __IOPCEventServer_INTERFACE_DEFINED__

/****************************************
 * Generated header for interface: IOPCEventServer
 * at Mon Nov 10 10:44:26 1997
 * using MIDL 3.01.75
 ****************************************/
/* [object][unique][helpstring][uuid] */ 



EXTERN_C const IID IID_IOPCEventServer;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    interface DECLSPEC_UUID("EA35CE65-59D6-11D1-84A0-00608CB8A7E9")
    IOPCEventServer : public IUnknown
    {
    public:
    };
    
#else 	/* C style interface */

    typedef struct IOPCEventServerVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *QueryInterface )( 
            IOPCEventServer __RPC_FAR * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void __RPC_FAR *__RPC_FAR *ppvObject);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *AddRef )( 
            IOPCEventServer __RPC_FAR * This);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *Release )( 
            IOPCEventServer __RPC_FAR * This);
        
        END_INTERFACE
    } IOPCEventServerVtbl;

    interface IOPCEventServer
    {
        CONST_VTBL struct IOPCEventServerVtbl __RPC_FAR *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IOPCEventServer_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IOPCEventServer_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IOPCEventServer_Release(This)	\
    (This)->lpVtbl -> Release(This)


#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IOPCEventServer_INTERFACE_DEFINED__ */


#ifndef __IOPCEventSubscriptionMgt_INTERFACE_DEFINED__
#define __IOPCEventSubscriptionMgt_INTERFACE_DEFINED__

/****************************************
 * Generated header for interface: IOPCEventSubscriptionMgt
 * at Mon Nov 10 10:44:26 1997
 * using MIDL 3.01.75
 ****************************************/
/* [object][unique][helpstring][uuid] */ 



EXTERN_C const IID IID_IOPCEventSubscriptionMgt;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    interface DECLSPEC_UUID("EA35CE66-59D6-11D1-84A0-00608CB8A7E9")
    IOPCEventSubscriptionMgt : public IUnknown
    {
    public:
    };
    
#else 	/* C style interface */

    typedef struct IOPCEventSubscriptionMgtVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *QueryInterface )( 
            IOPCEventSubscriptionMgt __RPC_FAR * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void __RPC_FAR *__RPC_FAR *ppvObject);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *AddRef )( 
            IOPCEventSubscriptionMgt __RPC_FAR * This);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *Release )( 
            IOPCEventSubscriptionMgt __RPC_FAR * This);
        
        END_INTERFACE
    } IOPCEventSubscriptionMgtVtbl;

    interface IOPCEventSubscriptionMgt
    {
        CONST_VTBL struct IOPCEventSubscriptionMgtVtbl __RPC_FAR *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IOPCEventSubscriptionMgt_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IOPCEventSubscriptionMgt_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IOPCEventSubscriptionMgt_Release(This)	\
    (This)->lpVtbl -> Release(This)


#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IOPCEventSubscriptionMgt_INTERFACE_DEFINED__ */


#ifndef __IOPCBrowseServerAreaSpace_INTERFACE_DEFINED__
#define __IOPCBrowseServerAreaSpace_INTERFACE_DEFINED__

/****************************************
 * Generated header for interface: IOPCBrowseServerAreaSpace
 * at Mon Nov 10 10:44:26 1997
 * using MIDL 3.01.75
 ****************************************/
/* [object][unique][helpstring][uuid] */ 



EXTERN_C const IID IID_IOPCBrowseServerAreaSpace;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    interface DECLSPEC_UUID("EA35CE67-59D6-11D1-84A0-00608CB8A7E9")
    IOPCBrowseServerAreaSpace : public IUnknown
    {
    public:
    };
    
#else 	/* C style interface */

    typedef struct IOPCBrowseServerAreaSpaceVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *QueryInterface )( 
            IOPCBrowseServerAreaSpace __RPC_FAR * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void __RPC_FAR *__RPC_FAR *ppvObject);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *AddRef )( 
            IOPCBrowseServerAreaSpace __RPC_FAR * This);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *Release )( 
            IOPCBrowseServerAreaSpace __RPC_FAR * This);
        
        END_INTERFACE
    } IOPCBrowseServerAreaSpaceVtbl;

    interface IOPCBrowseServerAreaSpace
    {
        CONST_VTBL struct IOPCBrowseServerAreaSpaceVtbl __RPC_FAR *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IOPCBrowseServerAreaSpace_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IOPCBrowseServerAreaSpace_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IOPCBrowseServerAreaSpace_Release(This)	\
    (This)->lpVtbl -> Release(This)


#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IOPCBrowseServerAreaSpace_INTERFACE_DEFINED__ */


#ifndef __IOPCBrowseConditions_INTERFACE_DEFINED__
#define __IOPCBrowseConditions_INTERFACE_DEFINED__

/****************************************
 * Generated header for interface: IOPCBrowseConditions
 * at Mon Nov 10 10:44:26 1997
 * using MIDL 3.01.75
 ****************************************/
/* [object][unique][helpstring][uuid] */ 



EXTERN_C const IID IID_IOPCBrowseConditions;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    interface DECLSPEC_UUID("EA35CE68-59D6-11D1-84A0-00608CB8A7E9")
    IOPCBrowseConditions : public IUnknown
    {
    public:
    };
    
#else 	/* C style interface */

    typedef struct IOPCBrowseConditionsVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *QueryInterface )( 
            IOPCBrowseConditions __RPC_FAR * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void __RPC_FAR *__RPC_FAR *ppvObject);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *AddRef )( 
            IOPCBrowseConditions __RPC_FAR * This);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *Release )( 
            IOPCBrowseConditions __RPC_FAR * This);
        
        END_INTERFACE
    } IOPCBrowseConditionsVtbl;

    interface IOPCBrowseConditions
    {
        CONST_VTBL struct IOPCBrowseConditionsVtbl __RPC_FAR *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IOPCBrowseConditions_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IOPCBrowseConditions_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IOPCBrowseConditions_Release(This)	\
    (This)->lpVtbl -> Release(This)


#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IOPCBrowseConditions_INTERFACE_DEFINED__ */



#ifndef __OPC_AE_SAMPLESERVERLib_LIBRARY_DEFINED__
#define __OPC_AE_SAMPLESERVERLib_LIBRARY_DEFINED__

/****************************************
 * Generated header for library: OPC_AE_SAMPLESERVERLib
 * at Mon Nov 10 10:44:26 1997
 * using MIDL 3.01.75
 ****************************************/
/* [helpstring][version][uuid] */ 



EXTERN_C const IID LIBID_OPC_AE_SAMPLESERVERLib;

#ifdef __cplusplus
EXTERN_C const CLSID CLSID_OPCEventServer;

class DECLSPEC_UUID("65168852-5783-11D1-84A0-00608CB8A7E9")
OPCEventServer;
#endif

#ifdef __cplusplus
EXTERN_C const CLSID CLSID_OPCEventSubscriptionMgt;

class DECLSPEC_UUID("65168856-5783-11D1-84A0-00608CB8A7E9")
OPCEventSubscriptionMgt;
#endif

#ifdef __cplusplus
EXTERN_C const CLSID CLSID_OPCBrowseServerAreaSpace;

class DECLSPEC_UUID("65168858-5783-11D1-84A0-00608CB8A7E9")
OPCBrowseServerAreaSpace;
#endif

#ifdef __cplusplus
EXTERN_C const CLSID CLSID_OPCBrowseConditions;

class DECLSPEC_UUID("6516885A-5783-11D1-84A0-00608CB8A7E9")
OPCBrowseConditions;
#endif
#endif /* __OPC_AE_SAMPLESERVERLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif
