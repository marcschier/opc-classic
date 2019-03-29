

/* this ALWAYS GENERATED file contains the proxy stub code */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Tue Jun 30 10:35:10 2015
 */
/* Compiler settings for OpcSec.idl:
    Oicf, W1, Zp8, env=Win32 (32b run), target_arch=X86 8.00.0603 
    protocol : dce , ms_ext, c_ext
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#if !defined(_M_IA64) && !defined(_M_AMD64) && !defined(_ARM_)


#pragma warning( disable: 4049 )  /* more than 64k source lines */
#if _MSC_VER >= 1200
#pragma warning(push)
#endif

#pragma warning( disable: 4211 )  /* redefine extern to static */
#pragma warning( disable: 4232 )  /* dllimport identity*/
#pragma warning( disable: 4024 )  /* array to pointer mapping*/
#pragma warning( disable: 4152 )  /* function/data pointer conversion in expression */
#pragma warning( disable: 4100 ) /* unreferenced arguments in x86 call */

#pragma optimize("", off ) 

#define USE_STUBLESS_PROXY


/* verify that the <rpcproxy.h> version is high enough to compile this file*/
#ifndef __REDQ_RPCPROXY_H_VERSION__
#define __REQUIRED_RPCPROXY_H_VERSION__ 440
#endif


#include "rpcproxy.h"
#ifndef __RPCPROXY_H_VERSION__
#error this stub requires an updated version of <rpcproxy.h>
#endif /* __RPCPROXY_H_VERSION__ */


#include "OpcSec.h"

#define TYPE_FORMAT_STRING_SIZE   11                                
#define PROC_FORMAT_STRING_SIZE   113                               
#define EXPR_FORMAT_STRING_SIZE   1                                 
#define TRANSMIT_AS_TABLE_SIZE    0            
#define WIRE_MARSHAL_TABLE_SIZE   0            

typedef struct _OpcSec_MIDL_TYPE_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ TYPE_FORMAT_STRING_SIZE ];
    } OpcSec_MIDL_TYPE_FORMAT_STRING;

typedef struct _OpcSec_MIDL_PROC_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ PROC_FORMAT_STRING_SIZE ];
    } OpcSec_MIDL_PROC_FORMAT_STRING;

typedef struct _OpcSec_MIDL_EXPR_FORMAT_STRING
    {
    long          Pad;
    unsigned char  Format[ EXPR_FORMAT_STRING_SIZE ];
    } OpcSec_MIDL_EXPR_FORMAT_STRING;


static const RPC_SYNTAX_IDENTIFIER  _RpcTransferSyntax = 
{{0x8A885D04,0x1CEB,0x11C9,{0x9F,0xE8,0x08,0x00,0x2B,0x10,0x48,0x60}},{2,0}};


extern const OpcSec_MIDL_TYPE_FORMAT_STRING OpcSec__MIDL_TypeFormatString;
extern const OpcSec_MIDL_PROC_FORMAT_STRING OpcSec__MIDL_ProcFormatString;
extern const OpcSec_MIDL_EXPR_FORMAT_STRING OpcSec__MIDL_ExprFormatString;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCSecurityNT_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCSecurityNT_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCSecurityPrivate_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCSecurityPrivate_ProxyInfo;



#if !defined(__RPC_WIN32__)
#error  Invalid build platform for this stub.
#endif

#if !(TARGET_IS_NT40_OR_LATER)
#error You need Windows NT 4.0 or later to run this stub because it uses these features:
#error   -Oif or -Oicf.
#error However, your C/C++ compilation flags indicate you intend to run this app on earlier systems.
#error This app will fail with the RPC_X_WRONG_STUB_VERSION error.
#endif


static const OpcSec_MIDL_PROC_FORMAT_STRING OpcSec__MIDL_ProcFormatString =
    {
        0,
        {

	/* Procedure IsAvailablePriv */


	/* Procedure IsAvailableNT */

			0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/*  2 */	NdrFcLong( 0x0 ),	/* 0 */
/*  6 */	NdrFcShort( 0x3 ),	/* 3 */
/*  8 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 10 */	NdrFcShort( 0x0 ),	/* 0 */
/* 12 */	NdrFcShort( 0x24 ),	/* 36 */
/* 14 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter pbAvailable */


	/* Parameter pbAvailable */

/* 16 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 18 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 20 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */


	/* Return value */

/* 22 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 24 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 26 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryMinImpersonationLevel */

/* 28 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 30 */	NdrFcLong( 0x0 ),	/* 0 */
/* 34 */	NdrFcShort( 0x4 ),	/* 4 */
/* 36 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 38 */	NdrFcShort( 0x0 ),	/* 0 */
/* 40 */	NdrFcShort( 0x24 ),	/* 36 */
/* 42 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter pdwMinImpLevel */

/* 44 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 46 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 48 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 50 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 52 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 54 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Logoff */


	/* Procedure ChangeUser */

/* 56 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 58 */	NdrFcLong( 0x0 ),	/* 0 */
/* 62 */	NdrFcShort( 0x5 ),	/* 5 */
/* 64 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 66 */	NdrFcShort( 0x0 ),	/* 0 */
/* 68 */	NdrFcShort( 0x8 ),	/* 8 */
/* 70 */	0x4,		/* Oi2 Flags:  has return, */
			0x1,		/* 1 */

	/* Return value */


	/* Return value */

/* 72 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 74 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 76 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Logon */

/* 78 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 80 */	NdrFcLong( 0x0 ),	/* 0 */
/* 84 */	NdrFcShort( 0x4 ),	/* 4 */
/* 86 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 88 */	NdrFcShort( 0x0 ),	/* 0 */
/* 90 */	NdrFcShort( 0x8 ),	/* 8 */
/* 92 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter szUserID */

/* 94 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 96 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 98 */	NdrFcShort( 0x8 ),	/* Type Offset=8 */

	/* Parameter szPassword */

/* 100 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 102 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 104 */	NdrFcShort( 0x8 ),	/* Type Offset=8 */

	/* Return value */

/* 106 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 108 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 110 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

			0x0
        }
    };

static const OpcSec_MIDL_TYPE_FORMAT_STRING OpcSec__MIDL_TypeFormatString =
    {
        0,
        {
			NdrFcShort( 0x0 ),	/* 0 */
/*  2 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/*  4 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/*  6 */	
			0x11, 0x8,	/* FC_RP [simple_pointer] */
/*  8 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */

			0x0
        }
    };


/* Object interface: IUnknown, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0xC0,0x00,0x00,0x00,0x00,0x00,0x00,0x46}} */


/* Object interface: IOPCSecurityNT, ver. 0.0,
   GUID={0x7AA83A01,0x6C77,0x11d3,{0x84,0xF9,0x00,0x00,0x86,0x30,0xA3,0x8B}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCSecurityNT_FormatStringOffsetTable[] =
    {
    0,
    28,
    56
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCSecurityNT_ProxyInfo =
    {
    &Object_StubDesc,
    OpcSec__MIDL_ProcFormatString.Format,
    &IOPCSecurityNT_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCSecurityNT_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    OpcSec__MIDL_ProcFormatString.Format,
    &IOPCSecurityNT_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(6) _IOPCSecurityNTProxyVtbl = 
{
    &IOPCSecurityNT_ProxyInfo,
    &IID_IOPCSecurityNT,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCSecurityNT::IsAvailableNT */ ,
    (void *) (INT_PTR) -1 /* IOPCSecurityNT::QueryMinImpersonationLevel */ ,
    (void *) (INT_PTR) -1 /* IOPCSecurityNT::ChangeUser */
};

const CInterfaceStubVtbl _IOPCSecurityNTStubVtbl =
{
    &IID_IOPCSecurityNT,
    &IOPCSecurityNT_ServerInfo,
    6,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCSecurityPrivate, ver. 0.0,
   GUID={0x7AA83A02,0x6C77,0x11d3,{0x84,0xF9,0x00,0x00,0x86,0x30,0xA3,0x8B}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCSecurityPrivate_FormatStringOffsetTable[] =
    {
    0,
    78,
    56
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCSecurityPrivate_ProxyInfo =
    {
    &Object_StubDesc,
    OpcSec__MIDL_ProcFormatString.Format,
    &IOPCSecurityPrivate_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCSecurityPrivate_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    OpcSec__MIDL_ProcFormatString.Format,
    &IOPCSecurityPrivate_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(6) _IOPCSecurityPrivateProxyVtbl = 
{
    &IOPCSecurityPrivate_ProxyInfo,
    &IID_IOPCSecurityPrivate,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCSecurityPrivate::IsAvailablePriv */ ,
    (void *) (INT_PTR) -1 /* IOPCSecurityPrivate::Logon */ ,
    (void *) (INT_PTR) -1 /* IOPCSecurityPrivate::Logoff */
};

const CInterfaceStubVtbl _IOPCSecurityPrivateStubVtbl =
{
    &IID_IOPCSecurityPrivate,
    &IOPCSecurityPrivate_ServerInfo,
    6,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};

static const MIDL_STUB_DESC Object_StubDesc = 
    {
    0,
    NdrOleAllocate,
    NdrOleFree,
    0,
    0,
    0,
    0,
    0,
    OpcSec__MIDL_TypeFormatString.Format,
    1, /* -error bounds_check flag */
    0x20000, /* Ndr library version */
    0,
    0x800025b, /* MIDL Version 8.0.603 */
    0,
    0,
    0,  /* notify & notify_flag routine table */
    0x1, /* MIDL flag */
    0, /* cs routines */
    0,   /* proxy/server info */
    0
    };

const CInterfaceProxyVtbl * const _OpcSec_ProxyVtblList[] = 
{
    ( CInterfaceProxyVtbl *) &_IOPCSecurityNTProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCSecurityPrivateProxyVtbl,
    0
};

const CInterfaceStubVtbl * const _OpcSec_StubVtblList[] = 
{
    ( CInterfaceStubVtbl *) &_IOPCSecurityNTStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCSecurityPrivateStubVtbl,
    0
};

PCInterfaceName const _OpcSec_InterfaceNamesList[] = 
{
    "IOPCSecurityNT",
    "IOPCSecurityPrivate",
    0
};


#define _OpcSec_CHECK_IID(n)	IID_GENERIC_CHECK_IID( _OpcSec, pIID, n)

int __stdcall _OpcSec_IID_Lookup( const IID * pIID, int * pIndex )
{
    IID_BS_LOOKUP_SETUP

    IID_BS_LOOKUP_INITIAL_TEST( _OpcSec, 2, 1 )
    IID_BS_LOOKUP_RETURN_RESULT( _OpcSec, 2, *pIndex )
    
}

const ExtendedProxyFileInfo OpcSec_ProxyFileInfo = 
{
    (PCInterfaceProxyVtblList *) & _OpcSec_ProxyVtblList,
    (PCInterfaceStubVtblList *) & _OpcSec_StubVtblList,
    (const PCInterfaceName * ) & _OpcSec_InterfaceNamesList,
    0, /* no delegation */
    & _OpcSec_IID_Lookup, 
    2,
    2,
    0, /* table of [async_uuid] interfaces */
    0, /* Filler1 */
    0, /* Filler2 */
    0  /* Filler3 */
};
#pragma optimize("", on )
#if _MSC_VER >= 1200
#pragma warning(pop)
#endif


#endif /* !defined(_M_IA64) && !defined(_M_AMD64) && !defined(_ARM_) */

