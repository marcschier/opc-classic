/* this ALWAYS GENERATED file contains the definitions for the interfaces */


/* File created by MIDL compiler version 5.01.0164 */
/* at Mon Dec 18 10:08:49 2000
 */
/* Compiler settings for opcbc.idl:
    Os (OptLev=s), W1, Zp8, env=Win32, ms_ext, app_config, c_ext
    error checks: allocation ref bounds_check enum stub_data 
*/
//@@MIDL_FILE_HEADING(  )


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 440
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

#ifndef __opcbc_h__
#define __opcbc_h__

#ifdef __cplusplus
extern "C"{
#endif 

/* Forward Declarations */ 

#ifndef __IOPCBatchServer_FWD_DEFINED__
#define __IOPCBatchServer_FWD_DEFINED__
typedef interface IOPCBatchServer IOPCBatchServer;
#endif 	/* __IOPCBatchServer_FWD_DEFINED__ */


#ifndef __IOPCBatchServer2_FWD_DEFINED__
#define __IOPCBatchServer2_FWD_DEFINED__
typedef interface IOPCBatchServer2 IOPCBatchServer2;
#endif 	/* __IOPCBatchServer2_FWD_DEFINED__ */


#ifndef __IEnumOPCBatchSummary_FWD_DEFINED__
#define __IEnumOPCBatchSummary_FWD_DEFINED__
typedef interface IEnumOPCBatchSummary IEnumOPCBatchSummary;
#endif 	/* __IEnumOPCBatchSummary_FWD_DEFINED__ */


#ifndef __IOPCEnumerationSets_FWD_DEFINED__
#define __IOPCEnumerationSets_FWD_DEFINED__
typedef interface IOPCEnumerationSets IOPCEnumerationSets;
#endif 	/* __IOPCEnumerationSets_FWD_DEFINED__ */


#ifndef __IOPCBatchServer_FWD_DEFINED__
#define __IOPCBatchServer_FWD_DEFINED__
typedef interface IOPCBatchServer IOPCBatchServer;
#endif 	/* __IOPCBatchServer_FWD_DEFINED__ */


#ifndef __IEnumOPCBatchSummary_FWD_DEFINED__
#define __IEnumOPCBatchSummary_FWD_DEFINED__
typedef interface IEnumOPCBatchSummary IEnumOPCBatchSummary;
#endif 	/* __IEnumOPCBatchSummary_FWD_DEFINED__ */


#ifndef __IOPCEnumerationSets_FWD_DEFINED__
#define __IOPCEnumerationSets_FWD_DEFINED__
typedef interface IOPCEnumerationSets IOPCEnumerationSets;
#endif 	/* __IOPCEnumerationSets_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"

void __RPC_FAR * __RPC_USER MIDL_user_allocate(size_t);
void __RPC_USER MIDL_user_free( void __RPC_FAR * ); 

/* interface __MIDL_itf_opcbc_0000 */
/* [local] */ 

typedef struct  tagOPCBATCHSUMMARY
    {
    /* [string] */ LPWSTR szID;
    /* [string] */ LPWSTR szDescription;
    /* [string] */ LPWSTR szOPCItemID;
    /* [string] */ LPWSTR szMasterRecipeID;
    FLOAT fBatchSize;
    /* [string] */ LPWSTR szEU;
    /* [string] */ LPWSTR szExecutionState;
    /* [string] */ LPWSTR szExecutionMode;
    FILETIME ftActualStartTime;
    FILETIME ftActualEndTime;
    }	OPCBATCHSUMMARY;

typedef struct  tagOPCBATCHSUMMARYFILTER
    {
    /* [string] */ LPWSTR szID;
    /* [string] */ LPWSTR szDescription;
    /* [string] */ LPWSTR szOPCItemID;
    /* [string] */ LPWSTR szMasterRecipeID;
    FLOAT fMinBatchSize;
    FLOAT fMaxBatchSize;
    /* [string] */ LPWSTR szEU;
    /* [string] */ LPWSTR szExecutionState;
    /* [string] */ LPWSTR szExecutionMode;
    FILETIME ftMinStartTime;
    FILETIME ftMaxStartTime;
    FILETIME ftMinEndTime;
    FILETIME ftMaxEndTime;
    }	OPCBATCHSUMMARYFILTER;



extern RPC_IF_HANDLE __MIDL_itf_opcbc_0000_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_opcbc_0000_v0_0_s_ifspec;

#ifndef __IOPCBatchServer_INTERFACE_DEFINED__
#define __IOPCBatchServer_INTERFACE_DEFINED__

/* interface IOPCBatchServer */
/* [object][unique][helpstring][uuid] */ 


EXTERN_C const IID IID_IOPCBatchServer;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("8BB4ED50-B314-11d3-B3EA-00C04F8ECEAA")
    IOPCBatchServer : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetDelimiter( 
            /* [string][out] */ LPWSTR __RPC_FAR *pszDelimiter) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE CreateEnumerator( 
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ LPUNKNOWN __RPC_FAR *ppUnk) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IOPCBatchServerVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *QueryInterface )( 
            IOPCBatchServer __RPC_FAR * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void __RPC_FAR *__RPC_FAR *ppvObject);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *AddRef )( 
            IOPCBatchServer __RPC_FAR * This);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *Release )( 
            IOPCBatchServer __RPC_FAR * This);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *GetDelimiter )( 
            IOPCBatchServer __RPC_FAR * This,
            /* [string][out] */ LPWSTR __RPC_FAR *pszDelimiter);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *CreateEnumerator )( 
            IOPCBatchServer __RPC_FAR * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ LPUNKNOWN __RPC_FAR *ppUnk);
        
        END_INTERFACE
    } IOPCBatchServerVtbl;

    interface IOPCBatchServer
    {
        CONST_VTBL struct IOPCBatchServerVtbl __RPC_FAR *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IOPCBatchServer_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IOPCBatchServer_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IOPCBatchServer_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IOPCBatchServer_GetDelimiter(This,pszDelimiter)	\
    (This)->lpVtbl -> GetDelimiter(This,pszDelimiter)

#define IOPCBatchServer_CreateEnumerator(This,riid,ppUnk)	\
    (This)->lpVtbl -> CreateEnumerator(This,riid,ppUnk)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE IOPCBatchServer_GetDelimiter_Proxy( 
    IOPCBatchServer __RPC_FAR * This,
    /* [string][out] */ LPWSTR __RPC_FAR *pszDelimiter);


void __RPC_STUB IOPCBatchServer_GetDelimiter_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE IOPCBatchServer_CreateEnumerator_Proxy( 
    IOPCBatchServer __RPC_FAR * This,
    /* [in] */ REFIID riid,
    /* [iid_is][out] */ LPUNKNOWN __RPC_FAR *ppUnk);


void __RPC_STUB IOPCBatchServer_CreateEnumerator_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IOPCBatchServer_INTERFACE_DEFINED__ */


#ifndef __IOPCBatchServer2_INTERFACE_DEFINED__
#define __IOPCBatchServer2_INTERFACE_DEFINED__

/* interface IOPCBatchServer2 */
/* [object][unique][helpstring][uuid] */ 


EXTERN_C const IID IID_IOPCBatchServer2;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("895A78CF-B0C5-11d4-A0B7-000102A980B1")
    IOPCBatchServer2 : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE CreateFilteredEnumerator( 
            /* [in] */ REFIID riid,
            /* [full][in] */ OPCBATCHSUMMARYFILTER __RPC_FAR *pFilter,
            /* [string][in] */ LPCWSTR szModel,
            /* [iid_is][out] */ LPUNKNOWN __RPC_FAR *ppUnk) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IOPCBatchServer2Vtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *QueryInterface )( 
            IOPCBatchServer2 __RPC_FAR * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void __RPC_FAR *__RPC_FAR *ppvObject);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *AddRef )( 
            IOPCBatchServer2 __RPC_FAR * This);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *Release )( 
            IOPCBatchServer2 __RPC_FAR * This);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *CreateFilteredEnumerator )( 
            IOPCBatchServer2 __RPC_FAR * This,
            /* [in] */ REFIID riid,
            /* [full][in] */ OPCBATCHSUMMARYFILTER __RPC_FAR *pFilter,
            /* [string][in] */ LPCWSTR szModel,
            /* [iid_is][out] */ LPUNKNOWN __RPC_FAR *ppUnk);
        
        END_INTERFACE
    } IOPCBatchServer2Vtbl;

    interface IOPCBatchServer2
    {
        CONST_VTBL struct IOPCBatchServer2Vtbl __RPC_FAR *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IOPCBatchServer2_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IOPCBatchServer2_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IOPCBatchServer2_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IOPCBatchServer2_CreateFilteredEnumerator(This,riid,pFilter,szModel,ppUnk)	\
    (This)->lpVtbl -> CreateFilteredEnumerator(This,riid,pFilter,szModel,ppUnk)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE IOPCBatchServer2_CreateFilteredEnumerator_Proxy( 
    IOPCBatchServer2 __RPC_FAR * This,
    /* [in] */ REFIID riid,
    /* [full][in] */ OPCBATCHSUMMARYFILTER __RPC_FAR *pFilter,
    /* [string][in] */ LPCWSTR szModel,
    /* [iid_is][out] */ LPUNKNOWN __RPC_FAR *ppUnk);


void __RPC_STUB IOPCBatchServer2_CreateFilteredEnumerator_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IOPCBatchServer2_INTERFACE_DEFINED__ */


#ifndef __IEnumOPCBatchSummary_INTERFACE_DEFINED__
#define __IEnumOPCBatchSummary_INTERFACE_DEFINED__

/* interface IEnumOPCBatchSummary */
/* [object][unique][helpstring][uuid] */ 


EXTERN_C const IID IID_IEnumOPCBatchSummary;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("a8080da2-e23e-11d2-afa7-00c04f539421")
    IEnumOPCBatchSummary : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE Next( 
            /* [in] */ ULONG celt,
            /* [size_is][size_is][out] */ OPCBATCHSUMMARY __RPC_FAR *__RPC_FAR *ppSummaryArray,
            /* [out] */ ULONG __RPC_FAR *pceltFetched) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE Skip( 
            /* [in] */ ULONG celt) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE Reset( void) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE Clone( 
            /* [out] */ IEnumOPCBatchSummary __RPC_FAR *__RPC_FAR *ppEnumBatchSummary) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE Count( 
            /* [out] */ ULONG __RPC_FAR *pcelt) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IEnumOPCBatchSummaryVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *QueryInterface )( 
            IEnumOPCBatchSummary __RPC_FAR * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void __RPC_FAR *__RPC_FAR *ppvObject);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *AddRef )( 
            IEnumOPCBatchSummary __RPC_FAR * This);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *Release )( 
            IEnumOPCBatchSummary __RPC_FAR * This);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *Next )( 
            IEnumOPCBatchSummary __RPC_FAR * This,
            /* [in] */ ULONG celt,
            /* [size_is][size_is][out] */ OPCBATCHSUMMARY __RPC_FAR *__RPC_FAR *ppSummaryArray,
            /* [out] */ ULONG __RPC_FAR *pceltFetched);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *Skip )( 
            IEnumOPCBatchSummary __RPC_FAR * This,
            /* [in] */ ULONG celt);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *Reset )( 
            IEnumOPCBatchSummary __RPC_FAR * This);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *Clone )( 
            IEnumOPCBatchSummary __RPC_FAR * This,
            /* [out] */ IEnumOPCBatchSummary __RPC_FAR *__RPC_FAR *ppEnumBatchSummary);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *Count )( 
            IEnumOPCBatchSummary __RPC_FAR * This,
            /* [out] */ ULONG __RPC_FAR *pcelt);
        
        END_INTERFACE
    } IEnumOPCBatchSummaryVtbl;

    interface IEnumOPCBatchSummary
    {
        CONST_VTBL struct IEnumOPCBatchSummaryVtbl __RPC_FAR *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IEnumOPCBatchSummary_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IEnumOPCBatchSummary_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IEnumOPCBatchSummary_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IEnumOPCBatchSummary_Next(This,celt,ppSummaryArray,pceltFetched)	\
    (This)->lpVtbl -> Next(This,celt,ppSummaryArray,pceltFetched)

#define IEnumOPCBatchSummary_Skip(This,celt)	\
    (This)->lpVtbl -> Skip(This,celt)

#define IEnumOPCBatchSummary_Reset(This)	\
    (This)->lpVtbl -> Reset(This)

#define IEnumOPCBatchSummary_Clone(This,ppEnumBatchSummary)	\
    (This)->lpVtbl -> Clone(This,ppEnumBatchSummary)

#define IEnumOPCBatchSummary_Count(This,pcelt)	\
    (This)->lpVtbl -> Count(This,pcelt)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE IEnumOPCBatchSummary_Next_Proxy( 
    IEnumOPCBatchSummary __RPC_FAR * This,
    /* [in] */ ULONG celt,
    /* [size_is][size_is][out] */ OPCBATCHSUMMARY __RPC_FAR *__RPC_FAR *ppSummaryArray,
    /* [out] */ ULONG __RPC_FAR *pceltFetched);


void __RPC_STUB IEnumOPCBatchSummary_Next_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE IEnumOPCBatchSummary_Skip_Proxy( 
    IEnumOPCBatchSummary __RPC_FAR * This,
    /* [in] */ ULONG celt);


void __RPC_STUB IEnumOPCBatchSummary_Skip_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE IEnumOPCBatchSummary_Reset_Proxy( 
    IEnumOPCBatchSummary __RPC_FAR * This);


void __RPC_STUB IEnumOPCBatchSummary_Reset_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE IEnumOPCBatchSummary_Clone_Proxy( 
    IEnumOPCBatchSummary __RPC_FAR * This,
    /* [out] */ IEnumOPCBatchSummary __RPC_FAR *__RPC_FAR *ppEnumBatchSummary);


void __RPC_STUB IEnumOPCBatchSummary_Clone_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE IEnumOPCBatchSummary_Count_Proxy( 
    IEnumOPCBatchSummary __RPC_FAR * This,
    /* [out] */ ULONG __RPC_FAR *pcelt);


void __RPC_STUB IEnumOPCBatchSummary_Count_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IEnumOPCBatchSummary_INTERFACE_DEFINED__ */


#ifndef __IOPCEnumerationSets_INTERFACE_DEFINED__
#define __IOPCEnumerationSets_INTERFACE_DEFINED__

/* interface IOPCEnumerationSets */
/* [object][unique][helpstring][uuid] */ 


EXTERN_C const IID IID_IOPCEnumerationSets;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("a8080da3-e23e-11d2-afa7-00c04f539421")
    IOPCEnumerationSets : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE QueryEnumerationSets( 
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppdwEnumSetId,
            /* [size_is][size_is][string][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszEnumSetName) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE QueryEnumeration( 
            /* [in] */ DWORD dwEnumSetId,
            /* [in] */ DWORD dwEnumValue,
            /* [string][out] */ LPWSTR __RPC_FAR *pszEnumName) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE QueryEnumerationList( 
            /* [in] */ DWORD dwEnumSetId,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppdwEnumValue,
            /* [size_is][size_is][string][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszEnumName) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IOPCEnumerationSetsVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *QueryInterface )( 
            IOPCEnumerationSets __RPC_FAR * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void __RPC_FAR *__RPC_FAR *ppvObject);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *AddRef )( 
            IOPCEnumerationSets __RPC_FAR * This);
        
        ULONG ( STDMETHODCALLTYPE __RPC_FAR *Release )( 
            IOPCEnumerationSets __RPC_FAR * This);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *QueryEnumerationSets )( 
            IOPCEnumerationSets __RPC_FAR * This,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppdwEnumSetId,
            /* [size_is][size_is][string][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszEnumSetName);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *QueryEnumeration )( 
            IOPCEnumerationSets __RPC_FAR * This,
            /* [in] */ DWORD dwEnumSetId,
            /* [in] */ DWORD dwEnumValue,
            /* [string][out] */ LPWSTR __RPC_FAR *pszEnumName);
        
        HRESULT ( STDMETHODCALLTYPE __RPC_FAR *QueryEnumerationList )( 
            IOPCEnumerationSets __RPC_FAR * This,
            /* [in] */ DWORD dwEnumSetId,
            /* [out] */ DWORD __RPC_FAR *pdwCount,
            /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppdwEnumValue,
            /* [size_is][size_is][string][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszEnumName);
        
        END_INTERFACE
    } IOPCEnumerationSetsVtbl;

    interface IOPCEnumerationSets
    {
        CONST_VTBL struct IOPCEnumerationSetsVtbl __RPC_FAR *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IOPCEnumerationSets_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define IOPCEnumerationSets_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define IOPCEnumerationSets_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define IOPCEnumerationSets_QueryEnumerationSets(This,pdwCount,ppdwEnumSetId,ppszEnumSetName)	\
    (This)->lpVtbl -> QueryEnumerationSets(This,pdwCount,ppdwEnumSetId,ppszEnumSetName)

#define IOPCEnumerationSets_QueryEnumeration(This,dwEnumSetId,dwEnumValue,pszEnumName)	\
    (This)->lpVtbl -> QueryEnumeration(This,dwEnumSetId,dwEnumValue,pszEnumName)

#define IOPCEnumerationSets_QueryEnumerationList(This,dwEnumSetId,pdwCount,ppdwEnumValue,ppszEnumName)	\
    (This)->lpVtbl -> QueryEnumerationList(This,dwEnumSetId,pdwCount,ppdwEnumValue,ppszEnumName)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE IOPCEnumerationSets_QueryEnumerationSets_Proxy( 
    IOPCEnumerationSets __RPC_FAR * This,
    /* [out] */ DWORD __RPC_FAR *pdwCount,
    /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppdwEnumSetId,
    /* [size_is][size_is][string][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszEnumSetName);


void __RPC_STUB IOPCEnumerationSets_QueryEnumerationSets_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE IOPCEnumerationSets_QueryEnumeration_Proxy( 
    IOPCEnumerationSets __RPC_FAR * This,
    /* [in] */ DWORD dwEnumSetId,
    /* [in] */ DWORD dwEnumValue,
    /* [string][out] */ LPWSTR __RPC_FAR *pszEnumName);


void __RPC_STUB IOPCEnumerationSets_QueryEnumeration_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE IOPCEnumerationSets_QueryEnumerationList_Proxy( 
    IOPCEnumerationSets __RPC_FAR * This,
    /* [in] */ DWORD dwEnumSetId,
    /* [out] */ DWORD __RPC_FAR *pdwCount,
    /* [size_is][size_is][out] */ DWORD __RPC_FAR *__RPC_FAR *ppdwEnumValue,
    /* [size_is][size_is][string][out] */ LPWSTR __RPC_FAR *__RPC_FAR *ppszEnumName);


void __RPC_STUB IOPCEnumerationSets_QueryEnumerationList_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __IOPCEnumerationSets_INTERFACE_DEFINED__ */



#ifndef __OPC_BATCH_LIBRARY_DEFINED__
#define __OPC_BATCH_LIBRARY_DEFINED__

/* library OPC_BATCH */
/* [helpstring][version][uuid] */ 





EXTERN_C const IID LIBID_OPC_BATCH;
#endif /* __OPC_BATCH_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif
