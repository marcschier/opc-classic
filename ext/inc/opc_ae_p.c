

/* this ALWAYS GENERATED file contains the proxy stub code */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Tue Jun 30 10:35:10 2015
 */
/* Compiler settings for opc_ae.idl:
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


#include "opc_ae.h"

#define TYPE_FORMAT_STRING_SIZE   1781                              
#define PROC_FORMAT_STRING_SIZE   1625                              
#define EXPR_FORMAT_STRING_SIZE   1                                 
#define TRANSMIT_AS_TABLE_SIZE    0            
#define WIRE_MARSHAL_TABLE_SIZE   1            

typedef struct _opc_ae_MIDL_TYPE_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ TYPE_FORMAT_STRING_SIZE ];
    } opc_ae_MIDL_TYPE_FORMAT_STRING;

typedef struct _opc_ae_MIDL_PROC_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ PROC_FORMAT_STRING_SIZE ];
    } opc_ae_MIDL_PROC_FORMAT_STRING;

typedef struct _opc_ae_MIDL_EXPR_FORMAT_STRING
    {
    long          Pad;
    unsigned char  Format[ EXPR_FORMAT_STRING_SIZE ];
    } opc_ae_MIDL_EXPR_FORMAT_STRING;


static const RPC_SYNTAX_IDENTIFIER  _RpcTransferSyntax = 
{{0x8A885D04,0x1CEB,0x11C9,{0x9F,0xE8,0x08,0x00,0x2B,0x10,0x48,0x60}},{2,0}};


extern const opc_ae_MIDL_TYPE_FORMAT_STRING opc_ae__MIDL_TypeFormatString;
extern const opc_ae_MIDL_PROC_FORMAT_STRING opc_ae__MIDL_ProcFormatString;
extern const opc_ae_MIDL_EXPR_FORMAT_STRING opc_ae__MIDL_ExprFormatString;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO OPCEventServerCATID_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO OPCEventServerCATID_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCEventServer_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCEventServer_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCEventSubscriptionMgt_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCEventSubscriptionMgt_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCEventAreaBrowser_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCEventAreaBrowser_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCEventSink_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCEventSink_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCEventServer2_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCEventServer2_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCEventSubscriptionMgt2_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCEventSubscriptionMgt2_ProxyInfo;


extern const USER_MARSHAL_ROUTINE_QUADRUPLE UserMarshalRoutines[ WIRE_MARSHAL_TABLE_SIZE ];

#if !defined(__RPC_WIN32__)
#error  Invalid build platform for this stub.
#endif

#if !(TARGET_IS_NT40_OR_LATER)
#error You need Windows NT 4.0 or later to run this stub because it uses these features:
#error   -Oif or -Oicf, [wire_marshal] or [user_marshal] attribute.
#error However, your C/C++ compilation flags indicate you intend to run this app on earlier systems.
#error This app will fail with the RPC_X_WRONG_STUB_VERSION error.
#endif


static const opc_ae_MIDL_PROC_FORMAT_STRING opc_ae__MIDL_ProcFormatString =
    {
        0,
        {

	/* Procedure GetStatus */

			0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/*  2 */	NdrFcLong( 0x0 ),	/* 0 */
/*  6 */	NdrFcShort( 0x3 ),	/* 3 */
/*  8 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 10 */	NdrFcShort( 0x0 ),	/* 0 */
/* 12 */	NdrFcShort( 0x8 ),	/* 8 */
/* 14 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x2,		/* 2 */

	/* Parameter ppEventServerStatus */

/* 16 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 18 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 20 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Return value */

/* 22 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 24 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 26 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CreateEventSubscription */

/* 28 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 30 */	NdrFcLong( 0x0 ),	/* 0 */
/* 34 */	NdrFcShort( 0x4 ),	/* 4 */
/* 36 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 38 */	NdrFcShort( 0x64 ),	/* 100 */
/* 40 */	NdrFcShort( 0x40 ),	/* 64 */
/* 42 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x9,		/* 9 */

	/* Parameter bActive */

/* 44 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 46 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 48 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwBufferTime */

/* 50 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 52 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 54 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwMaxSize */

/* 56 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 58 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 60 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hClientSubscription */

/* 62 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 64 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 66 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter riid */

/* 68 */	NdrFcShort( 0x10a ),	/* Flags:  must free, in, simple ref, */
/* 70 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 72 */	NdrFcShort( 0x3c ),	/* Type Offset=60 */

	/* Parameter ppUnk */

/* 74 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 76 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 78 */	NdrFcShort( 0x48 ),	/* Type Offset=72 */

	/* Parameter pdwRevisedBufferTime */

/* 80 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 82 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 84 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwRevisedMaxSize */

/* 86 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 88 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 90 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 92 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 94 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 96 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryAvailableFilters */

/* 98 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 100 */	NdrFcLong( 0x0 ),	/* 0 */
/* 104 */	NdrFcShort( 0x5 ),	/* 5 */
/* 106 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 108 */	NdrFcShort( 0x0 ),	/* 0 */
/* 110 */	NdrFcShort( 0x24 ),	/* 36 */
/* 112 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter pdwFilterMask */

/* 114 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 116 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 118 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 120 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 122 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 124 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryEventCategories */

/* 126 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 128 */	NdrFcLong( 0x0 ),	/* 0 */
/* 132 */	NdrFcShort( 0x6 ),	/* 6 */
/* 134 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 136 */	NdrFcShort( 0x8 ),	/* 8 */
/* 138 */	NdrFcShort( 0x24 ),	/* 36 */
/* 140 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwEventType */

/* 142 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 144 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 146 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCount */

/* 148 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 150 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 152 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppdwEventCategories */

/* 154 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 156 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 158 */	NdrFcShort( 0x56 ),	/* Type Offset=86 */

	/* Parameter ppszEventCategoryDescs */

/* 160 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 162 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 164 */	NdrFcShort( 0x68 ),	/* Type Offset=104 */

	/* Return value */

/* 166 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 168 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 170 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryConditionNames */

/* 172 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 174 */	NdrFcLong( 0x0 ),	/* 0 */
/* 178 */	NdrFcShort( 0x7 ),	/* 7 */
/* 180 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 182 */	NdrFcShort( 0x8 ),	/* 8 */
/* 184 */	NdrFcShort( 0x24 ),	/* 36 */
/* 186 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwEventCategory */

/* 188 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 190 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 192 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCount */

/* 194 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 196 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 198 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppszConditionNames */

/* 200 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 202 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 204 */	NdrFcShort( 0x68 ),	/* Type Offset=104 */

	/* Return value */

/* 206 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 208 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 210 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QuerySubConditionNames */

/* 212 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 214 */	NdrFcLong( 0x0 ),	/* 0 */
/* 218 */	NdrFcShort( 0x8 ),	/* 8 */
/* 220 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 222 */	NdrFcShort( 0x0 ),	/* 0 */
/* 224 */	NdrFcShort( 0x24 ),	/* 36 */
/* 226 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter szConditionName */

/* 228 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 230 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 232 */	NdrFcShort( 0x90 ),	/* Type Offset=144 */

	/* Parameter pdwCount */

/* 234 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 236 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 238 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppszSubConditionNames */

/* 240 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 242 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 244 */	NdrFcShort( 0x68 ),	/* Type Offset=104 */

	/* Return value */

/* 246 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 248 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 250 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QuerySourceConditions */

/* 252 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 254 */	NdrFcLong( 0x0 ),	/* 0 */
/* 258 */	NdrFcShort( 0x9 ),	/* 9 */
/* 260 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 262 */	NdrFcShort( 0x0 ),	/* 0 */
/* 264 */	NdrFcShort( 0x24 ),	/* 36 */
/* 266 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter szSource */

/* 268 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 270 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 272 */	NdrFcShort( 0x90 ),	/* Type Offset=144 */

	/* Parameter pdwCount */

/* 274 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 276 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 278 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppszConditionNames */

/* 280 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 282 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 284 */	NdrFcShort( 0x68 ),	/* Type Offset=104 */

	/* Return value */

/* 286 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 288 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 290 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryEventAttributes */

/* 292 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 294 */	NdrFcLong( 0x0 ),	/* 0 */
/* 298 */	NdrFcShort( 0xa ),	/* 10 */
/* 300 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 302 */	NdrFcShort( 0x8 ),	/* 8 */
/* 304 */	NdrFcShort( 0x24 ),	/* 36 */
/* 306 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwEventCategory */

/* 308 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 310 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 312 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCount */

/* 314 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 316 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 318 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppdwAttrIDs */

/* 320 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 322 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 324 */	NdrFcShort( 0x56 ),	/* Type Offset=86 */

	/* Parameter ppszAttrDescs */

/* 326 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 328 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 330 */	NdrFcShort( 0x68 ),	/* Type Offset=104 */

	/* Parameter ppvtAttrTypes */

/* 332 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 334 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 336 */	NdrFcShort( 0x92 ),	/* Type Offset=146 */

	/* Return value */

/* 338 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 340 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 342 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure TranslateToItemIDs */

/* 344 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 346 */	NdrFcLong( 0x0 ),	/* 0 */
/* 350 */	NdrFcShort( 0xb ),	/* 11 */
/* 352 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 354 */	NdrFcShort( 0x10 ),	/* 16 */
/* 356 */	NdrFcShort( 0x8 ),	/* 8 */
/* 358 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0xa,		/* 10 */

	/* Parameter szSource */

/* 360 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 362 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 364 */	NdrFcShort( 0x90 ),	/* Type Offset=144 */

	/* Parameter dwEventCategory */

/* 366 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 368 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 370 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter szConditionName */

/* 372 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 374 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 376 */	NdrFcShort( 0x90 ),	/* Type Offset=144 */

	/* Parameter szSubconditionName */

/* 378 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 380 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 382 */	NdrFcShort( 0x90 ),	/* Type Offset=144 */

	/* Parameter dwCount */

/* 384 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 386 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 388 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwAssocAttrIDs */

/* 390 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 392 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 394 */	NdrFcShort( 0xa8 ),	/* Type Offset=168 */

	/* Parameter ppszAttrItemIDs */

/* 396 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 398 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 400 */	NdrFcShort( 0xb2 ),	/* Type Offset=178 */

	/* Parameter ppszNodeNames */

/* 402 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 404 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 406 */	NdrFcShort( 0xb2 ),	/* Type Offset=178 */

	/* Parameter ppCLSIDs */

/* 408 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 410 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 412 */	NdrFcShort( 0xd8 ),	/* Type Offset=216 */

	/* Return value */

/* 414 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 416 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 418 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetConditionState */

/* 420 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 422 */	NdrFcLong( 0x0 ),	/* 0 */
/* 426 */	NdrFcShort( 0xc ),	/* 12 */
/* 428 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 430 */	NdrFcShort( 0x8 ),	/* 8 */
/* 432 */	NdrFcShort( 0x8 ),	/* 8 */
/* 434 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter szSource */

/* 436 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 438 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 440 */	NdrFcShort( 0x90 ),	/* Type Offset=144 */

	/* Parameter szConditionName */

/* 442 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 444 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 446 */	NdrFcShort( 0x90 ),	/* Type Offset=144 */

	/* Parameter dwNumEventAttrs */

/* 448 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 450 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 452 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwAttributeIDs */

/* 454 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 456 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 458 */	NdrFcShort( 0xf2 ),	/* Type Offset=242 */

	/* Parameter ppConditionState */

/* 460 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 462 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 464 */	NdrFcShort( 0xfc ),	/* Type Offset=252 */

	/* Return value */

/* 466 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 468 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 470 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure EnableConditionByArea */

/* 472 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 474 */	NdrFcLong( 0x0 ),	/* 0 */
/* 478 */	NdrFcShort( 0xd ),	/* 13 */
/* 480 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 482 */	NdrFcShort( 0x8 ),	/* 8 */
/* 484 */	NdrFcShort( 0x8 ),	/* 8 */
/* 486 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter dwNumAreas */

/* 488 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 490 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 492 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszAreas */

/* 494 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 496 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 498 */	NdrFcShort( 0x574 ),	/* Type Offset=1396 */

	/* Return value */

/* 500 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 502 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 504 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure EnableConditionBySource */

/* 506 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 508 */	NdrFcLong( 0x0 ),	/* 0 */
/* 512 */	NdrFcShort( 0xe ),	/* 14 */
/* 514 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 516 */	NdrFcShort( 0x8 ),	/* 8 */
/* 518 */	NdrFcShort( 0x8 ),	/* 8 */
/* 520 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter dwNumSources */

/* 522 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 524 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 526 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszSources */

/* 528 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 530 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 532 */	NdrFcShort( 0x574 ),	/* Type Offset=1396 */

	/* Return value */

/* 534 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 536 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 538 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure DisableConditionByArea */

/* 540 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 542 */	NdrFcLong( 0x0 ),	/* 0 */
/* 546 */	NdrFcShort( 0xf ),	/* 15 */
/* 548 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 550 */	NdrFcShort( 0x8 ),	/* 8 */
/* 552 */	NdrFcShort( 0x8 ),	/* 8 */
/* 554 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter dwNumAreas */

/* 556 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 558 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 560 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszAreas */

/* 562 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 564 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 566 */	NdrFcShort( 0x574 ),	/* Type Offset=1396 */

	/* Return value */

/* 568 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 570 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 572 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure DisableConditionBySource */

/* 574 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 576 */	NdrFcLong( 0x0 ),	/* 0 */
/* 580 */	NdrFcShort( 0x10 ),	/* 16 */
/* 582 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 584 */	NdrFcShort( 0x8 ),	/* 8 */
/* 586 */	NdrFcShort( 0x8 ),	/* 8 */
/* 588 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter dwNumSources */

/* 590 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 592 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 594 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszSources */

/* 596 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 598 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 600 */	NdrFcShort( 0x574 ),	/* Type Offset=1396 */

	/* Return value */

/* 602 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 604 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 606 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure AckCondition */

/* 608 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 610 */	NdrFcLong( 0x0 ),	/* 0 */
/* 614 */	NdrFcShort( 0x11 ),	/* 17 */
/* 616 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 618 */	NdrFcShort( 0x8 ),	/* 8 */
/* 620 */	NdrFcShort( 0x8 ),	/* 8 */
/* 622 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x9,		/* 9 */

	/* Parameter dwCount */

/* 624 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 626 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 628 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter szAcknowledgerID */

/* 630 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 632 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 634 */	NdrFcShort( 0x90 ),	/* Type Offset=144 */

	/* Parameter szComment */

/* 636 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 638 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 640 */	NdrFcShort( 0x90 ),	/* Type Offset=144 */

	/* Parameter pszSource */

/* 642 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 644 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 646 */	NdrFcShort( 0x574 ),	/* Type Offset=1396 */

	/* Parameter pszConditionName */

/* 648 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 650 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 652 */	NdrFcShort( 0x574 ),	/* Type Offset=1396 */

	/* Parameter pftActiveTime */

/* 654 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 656 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 658 */	NdrFcShort( 0x596 ),	/* Type Offset=1430 */

	/* Parameter pdwCookie */

/* 660 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 662 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 664 */	NdrFcShort( 0x5a8 ),	/* Type Offset=1448 */

	/* Parameter ppErrors */

/* 666 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 668 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 670 */	NdrFcShort( 0x5b2 ),	/* Type Offset=1458 */

	/* Return value */

/* 672 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 674 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 676 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CreateAreaBrowser */

/* 678 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 680 */	NdrFcLong( 0x0 ),	/* 0 */
/* 684 */	NdrFcShort( 0x12 ),	/* 18 */
/* 686 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 688 */	NdrFcShort( 0x44 ),	/* 68 */
/* 690 */	NdrFcShort( 0x8 ),	/* 8 */
/* 692 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x3,		/* 3 */

	/* Parameter riid */

/* 694 */	NdrFcShort( 0x10a ),	/* Flags:  must free, in, simple ref, */
/* 696 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 698 */	NdrFcShort( 0x3c ),	/* Type Offset=60 */

	/* Parameter ppUnk */

/* 700 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 702 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 704 */	NdrFcShort( 0x5ba ),	/* Type Offset=1466 */

	/* Return value */

/* 706 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 708 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 710 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetFilter */

/* 712 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 714 */	NdrFcLong( 0x0 ),	/* 0 */
/* 718 */	NdrFcShort( 0x3 ),	/* 3 */
/* 720 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 722 */	NdrFcShort( 0x30 ),	/* 48 */
/* 724 */	NdrFcShort( 0x8 ),	/* 8 */
/* 726 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0xa,		/* 10 */

	/* Parameter dwEventType */

/* 728 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 730 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 732 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumCategories */

/* 734 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 736 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 738 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwEventCategories */

/* 740 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 742 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 744 */	NdrFcShort( 0x5c8 ),	/* Type Offset=1480 */

	/* Parameter dwLowSeverity */

/* 746 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 748 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 750 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwHighSeverity */

/* 752 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 754 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 756 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumAreas */

/* 758 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 760 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 762 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszAreaList */

/* 764 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 766 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 768 */	NdrFcShort( 0x5d6 ),	/* Type Offset=1494 */

	/* Parameter dwNumSources */

/* 770 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 772 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 774 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszSourceList */

/* 776 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 778 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 780 */	NdrFcShort( 0x5f8 ),	/* Type Offset=1528 */

	/* Return value */

/* 782 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 784 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 786 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetFilter */

/* 788 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 790 */	NdrFcLong( 0x0 ),	/* 0 */
/* 794 */	NdrFcShort( 0x4 ),	/* 4 */
/* 796 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 798 */	NdrFcShort( 0x0 ),	/* 0 */
/* 800 */	NdrFcShort( 0xb0 ),	/* 176 */
/* 802 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0xa,		/* 10 */

	/* Parameter pdwEventType */

/* 804 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 806 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 808 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwNumCategories */

/* 810 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 812 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 814 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppdwEventCategories */

/* 816 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 818 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 820 */	NdrFcShort( 0x56 ),	/* Type Offset=86 */

	/* Parameter pdwLowSeverity */

/* 822 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 824 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 826 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwHighSeverity */

/* 828 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 830 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 832 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwNumAreas */

/* 834 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 836 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 838 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppszAreaList */

/* 840 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 842 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 844 */	NdrFcShort( 0x616 ),	/* Type Offset=1558 */

	/* Parameter pdwNumSources */

/* 846 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 848 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 850 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppszSourceList */

/* 852 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 854 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 856 */	NdrFcShort( 0x63c ),	/* Type Offset=1596 */

	/* Return value */

/* 858 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 860 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 862 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SelectReturnedAttributes */

/* 864 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 866 */	NdrFcLong( 0x0 ),	/* 0 */
/* 870 */	NdrFcShort( 0x5 ),	/* 5 */
/* 872 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 874 */	NdrFcShort( 0x10 ),	/* 16 */
/* 876 */	NdrFcShort( 0x8 ),	/* 8 */
/* 878 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwEventCategory */

/* 880 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 882 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 884 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 886 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 888 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 890 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwAttributeIDs */

/* 892 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 894 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 896 */	NdrFcShort( 0x5c8 ),	/* Type Offset=1480 */

	/* Return value */

/* 898 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 900 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 902 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetReturnedAttributes */

/* 904 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 906 */	NdrFcLong( 0x0 ),	/* 0 */
/* 910 */	NdrFcShort( 0x6 ),	/* 6 */
/* 912 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 914 */	NdrFcShort( 0x8 ),	/* 8 */
/* 916 */	NdrFcShort( 0x24 ),	/* 36 */
/* 918 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwEventCategory */

/* 920 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 922 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 924 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCount */

/* 926 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 928 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 930 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppdwAttributeIDs */

/* 932 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 934 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 936 */	NdrFcShort( 0x56 ),	/* Type Offset=86 */

	/* Return value */

/* 938 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 940 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 942 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Refresh */

/* 944 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 946 */	NdrFcLong( 0x0 ),	/* 0 */
/* 950 */	NdrFcShort( 0x7 ),	/* 7 */
/* 952 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 954 */	NdrFcShort( 0x8 ),	/* 8 */
/* 956 */	NdrFcShort( 0x8 ),	/* 8 */
/* 958 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter dwConnection */

/* 960 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 962 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 964 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 966 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 968 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 970 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CancelRefresh */

/* 972 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 974 */	NdrFcLong( 0x0 ),	/* 0 */
/* 978 */	NdrFcShort( 0x8 ),	/* 8 */
/* 980 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 982 */	NdrFcShort( 0x8 ),	/* 8 */
/* 984 */	NdrFcShort( 0x8 ),	/* 8 */
/* 986 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter dwConnection */

/* 988 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 990 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 992 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 994 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 996 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 998 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetState */

/* 1000 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1002 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1006 */	NdrFcShort( 0x9 ),	/* 9 */
/* 1008 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1010 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1012 */	NdrFcShort( 0x78 ),	/* 120 */
/* 1014 */	0x4,		/* Oi2 Flags:  has return, */
			0x5,		/* 5 */

	/* Parameter pbActive */

/* 1016 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1018 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1020 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwBufferTime */

/* 1022 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1024 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1026 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwMaxSize */

/* 1028 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1030 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1032 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phClientSubscription */

/* 1034 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1036 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1038 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1040 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1042 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1044 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetState */

/* 1046 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1048 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1052 */	NdrFcShort( 0xa ),	/* 10 */
/* 1054 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1056 */	NdrFcShort( 0x5c ),	/* 92 */
/* 1058 */	NdrFcShort( 0x40 ),	/* 64 */
/* 1060 */	0x4,		/* Oi2 Flags:  has return, */
			0x7,		/* 7 */

	/* Parameter pbActive */

/* 1062 */	NdrFcShort( 0xa ),	/* Flags:  must free, in, */
/* 1064 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1066 */	NdrFcShort( 0x662 ),	/* Type Offset=1634 */

	/* Parameter pdwBufferTime */

/* 1068 */	NdrFcShort( 0xa ),	/* Flags:  must free, in, */
/* 1070 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1072 */	NdrFcShort( 0x662 ),	/* Type Offset=1634 */

	/* Parameter pdwMaxSize */

/* 1074 */	NdrFcShort( 0xa ),	/* Flags:  must free, in, */
/* 1076 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1078 */	NdrFcShort( 0x662 ),	/* Type Offset=1634 */

	/* Parameter hClientSubscription */

/* 1080 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1082 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1084 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwRevisedBufferTime */

/* 1086 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1088 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1090 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwRevisedMaxSize */

/* 1092 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1094 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1096 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1098 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1100 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1102 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ChangeBrowsePosition */

/* 1104 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1106 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1110 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1112 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1114 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1116 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1118 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter dwBrowseDirection */

/* 1120 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1122 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1124 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter szString */

/* 1126 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1128 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1130 */	NdrFcShort( 0x90 ),	/* Type Offset=144 */

	/* Return value */

/* 1132 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1134 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1136 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure BrowseOPCAreas */

/* 1138 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1140 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1144 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1146 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1148 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1150 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1152 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwBrowseFilterType */

/* 1154 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1156 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1158 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter szFilterCriteria */

/* 1160 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1162 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1164 */	NdrFcShort( 0x90 ),	/* Type Offset=144 */

	/* Parameter ppIEnumString */

/* 1166 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 1168 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1170 */	NdrFcShort( 0x666 ),	/* Type Offset=1638 */

	/* Return value */

/* 1172 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1174 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1176 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetQualifiedAreaName */

/* 1178 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1180 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1184 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1186 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1188 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1190 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1192 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter szAreaName */

/* 1194 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1196 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1198 */	NdrFcShort( 0x90 ),	/* Type Offset=144 */

	/* Parameter pszQualifiedAreaName */

/* 1200 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1202 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1204 */	NdrFcShort( 0x67c ),	/* Type Offset=1660 */

	/* Return value */

/* 1206 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1208 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1210 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetQualifiedSourceName */

/* 1212 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1214 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1218 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1220 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1222 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1224 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1226 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter szSourceName */

/* 1228 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1230 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1232 */	NdrFcShort( 0x90 ),	/* Type Offset=144 */

	/* Parameter pszQualifiedSourceName */

/* 1234 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1236 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1238 */	NdrFcShort( 0x67c ),	/* Type Offset=1660 */

	/* Return value */

/* 1240 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1242 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1244 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnEvent */

/* 1246 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1248 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1252 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1254 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1256 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1258 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1260 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter hClientSubscription */

/* 1262 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1264 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1266 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter bRefresh */

/* 1268 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1270 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1272 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter bLastRefresh */

/* 1274 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1276 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1278 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 1280 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1282 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1284 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pEvents */

/* 1286 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1288 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1290 */	NdrFcShort( 0x6e2 ),	/* Type Offset=1762 */

	/* Return value */

/* 1292 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1294 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1296 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure EnableConditionByArea2 */

/* 1298 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1300 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1304 */	NdrFcShort( 0x13 ),	/* 19 */
/* 1306 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1308 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1310 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1312 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwNumAreas */

/* 1314 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1316 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1318 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszAreas */

/* 1320 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1322 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1324 */	NdrFcShort( 0x574 ),	/* Type Offset=1396 */

	/* Parameter ppErrors */

/* 1326 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1328 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1330 */	NdrFcShort( 0x5b2 ),	/* Type Offset=1458 */

	/* Return value */

/* 1332 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1334 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1336 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure EnableConditionBySource2 */

/* 1338 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1340 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1344 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1346 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1348 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1350 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1352 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwNumSources */

/* 1354 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1356 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1358 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszSources */

/* 1360 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1362 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1364 */	NdrFcShort( 0x574 ),	/* Type Offset=1396 */

	/* Parameter ppErrors */

/* 1366 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1368 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1370 */	NdrFcShort( 0x5b2 ),	/* Type Offset=1458 */

	/* Return value */

/* 1372 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1374 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1376 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure DisableConditionByArea2 */

/* 1378 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1380 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1384 */	NdrFcShort( 0x15 ),	/* 21 */
/* 1386 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1388 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1390 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1392 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwNumAreas */

/* 1394 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1396 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1398 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszAreas */

/* 1400 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1402 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1404 */	NdrFcShort( 0x574 ),	/* Type Offset=1396 */

	/* Parameter ppErrors */

/* 1406 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1408 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1410 */	NdrFcShort( 0x5b2 ),	/* Type Offset=1458 */

	/* Return value */

/* 1412 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1414 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1416 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure DisableConditionBySource2 */

/* 1418 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1420 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1424 */	NdrFcShort( 0x16 ),	/* 22 */
/* 1426 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1428 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1430 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1432 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwNumSources */

/* 1434 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1436 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1438 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszSources */

/* 1440 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1442 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1444 */	NdrFcShort( 0x574 ),	/* Type Offset=1396 */

	/* Parameter ppErrors */

/* 1446 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1448 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1450 */	NdrFcShort( 0x5b2 ),	/* Type Offset=1458 */

	/* Return value */

/* 1452 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1454 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1456 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetEnableStateByArea */

/* 1458 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1460 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1464 */	NdrFcShort( 0x17 ),	/* 23 */
/* 1466 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1468 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1470 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1472 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwNumAreas */

/* 1474 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1476 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1478 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszAreas */

/* 1480 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1482 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1484 */	NdrFcShort( 0x574 ),	/* Type Offset=1396 */

	/* Parameter pbEnabled */

/* 1486 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1488 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1490 */	NdrFcShort( 0x5b2 ),	/* Type Offset=1458 */

	/* Parameter pbEffectivelyEnabled */

/* 1492 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1494 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1496 */	NdrFcShort( 0x5b2 ),	/* Type Offset=1458 */

	/* Parameter ppErrors */

/* 1498 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1500 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1502 */	NdrFcShort( 0x5b2 ),	/* Type Offset=1458 */

	/* Return value */

/* 1504 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1506 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1508 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetEnableStateBySource */

/* 1510 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1512 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1516 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1518 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1520 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1522 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1524 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwNumSources */

/* 1526 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1528 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1530 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszSources */

/* 1532 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1534 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1536 */	NdrFcShort( 0x574 ),	/* Type Offset=1396 */

	/* Parameter pbEnabled */

/* 1538 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1540 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1542 */	NdrFcShort( 0x5b2 ),	/* Type Offset=1458 */

	/* Parameter pbEffectivelyEnabled */

/* 1544 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1546 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1548 */	NdrFcShort( 0x5b2 ),	/* Type Offset=1458 */

	/* Parameter ppErrors */

/* 1550 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1552 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1554 */	NdrFcShort( 0x5b2 ),	/* Type Offset=1458 */

	/* Return value */

/* 1556 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1558 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1560 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetKeepAlive */

/* 1562 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1564 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1568 */	NdrFcShort( 0xb ),	/* 11 */
/* 1570 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1572 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1574 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1576 */	0x4,		/* Oi2 Flags:  has return, */
			0x3,		/* 3 */

	/* Parameter dwKeepAliveTime */

/* 1578 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1580 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1582 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwRevisedKeepAliveTime */

/* 1584 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1586 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1588 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1590 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1592 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1594 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetKeepAlive */

/* 1596 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1598 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1602 */	NdrFcShort( 0xc ),	/* 12 */
/* 1604 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1606 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1608 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1610 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter pdwKeepAliveTime */

/* 1612 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1614 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1616 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1618 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1620 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1622 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

			0x0
        }
    };

static const opc_ae_MIDL_TYPE_FORMAT_STRING opc_ae__MIDL_TypeFormatString =
    {
        0,
        {
			NdrFcShort( 0x0 ),	/* 0 */
/*  2 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/*  4 */	NdrFcShort( 0x2 ),	/* Offset= 2 (6) */
/*  6 */	
			0x13, 0x0,	/* FC_OP */
/*  8 */	NdrFcShort( 0xa ),	/* Offset= 10 (18) */
/* 10 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 12 */	NdrFcShort( 0x8 ),	/* 8 */
/* 14 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 16 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 18 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 20 */	NdrFcShort( 0x28 ),	/* 40 */
/* 22 */	NdrFcShort( 0x0 ),	/* 0 */
/* 24 */	NdrFcShort( 0x16 ),	/* Offset= 22 (46) */
/* 26 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 28 */	NdrFcShort( 0xffee ),	/* Offset= -18 (10) */
/* 30 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 32 */	NdrFcShort( 0xffea ),	/* Offset= -22 (10) */
/* 34 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 36 */	NdrFcShort( 0xffe6 ),	/* Offset= -26 (10) */
/* 38 */	0xd,		/* FC_ENUM16 */
			0x6,		/* FC_SHORT */
/* 40 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 42 */	0x6,		/* FC_SHORT */
			0x36,		/* FC_POINTER */
/* 44 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 46 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 48 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 50 */	
			0x11, 0x0,	/* FC_RP */
/* 52 */	NdrFcShort( 0x8 ),	/* Offset= 8 (60) */
/* 54 */	
			0x1d,		/* FC_SMFARRAY */
			0x0,		/* 0 */
/* 56 */	NdrFcShort( 0x8 ),	/* 8 */
/* 58 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 60 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 62 */	NdrFcShort( 0x10 ),	/* 16 */
/* 64 */	0x8,		/* FC_LONG */
			0x6,		/* FC_SHORT */
/* 66 */	0x6,		/* FC_SHORT */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 68 */	0x0,		/* 0 */
			NdrFcShort( 0xfff1 ),	/* Offset= -15 (54) */
			0x5b,		/* FC_END */
/* 72 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 74 */	NdrFcShort( 0x2 ),	/* Offset= 2 (76) */
/* 76 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 78 */	0x28,		/* Corr desc:  parameter, FC_LONG */
			0x0,		/*  */
/* 80 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 82 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 84 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 86 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 88 */	NdrFcShort( 0x2 ),	/* Offset= 2 (90) */
/* 90 */	
			0x13, 0x0,	/* FC_OP */
/* 92 */	NdrFcShort( 0x2 ),	/* Offset= 2 (94) */
/* 94 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 96 */	NdrFcShort( 0x4 ),	/* 4 */
/* 98 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 100 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 102 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 104 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 106 */	NdrFcShort( 0x2 ),	/* Offset= 2 (108) */
/* 108 */	
			0x13, 0x0,	/* FC_OP */
/* 110 */	NdrFcShort( 0x2 ),	/* Offset= 2 (112) */
/* 112 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 114 */	NdrFcShort( 0x4 ),	/* 4 */
/* 116 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 118 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 120 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 122 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 124 */	NdrFcShort( 0x4 ),	/* 4 */
/* 126 */	NdrFcShort( 0x0 ),	/* 0 */
/* 128 */	NdrFcShort( 0x1 ),	/* 1 */
/* 130 */	NdrFcShort( 0x0 ),	/* 0 */
/* 132 */	NdrFcShort( 0x0 ),	/* 0 */
/* 134 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 136 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 138 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 140 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 142 */	
			0x11, 0x8,	/* FC_RP [simple_pointer] */
/* 144 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 146 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 148 */	NdrFcShort( 0x2 ),	/* Offset= 2 (150) */
/* 150 */	
			0x13, 0x0,	/* FC_OP */
/* 152 */	NdrFcShort( 0x2 ),	/* Offset= 2 (154) */
/* 154 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 156 */	NdrFcShort( 0x2 ),	/* 2 */
/* 158 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 160 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 162 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 164 */	
			0x11, 0x0,	/* FC_RP */
/* 166 */	NdrFcShort( 0x2 ),	/* Offset= 2 (168) */
/* 168 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 170 */	NdrFcShort( 0x4 ),	/* 4 */
/* 172 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 174 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 176 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 178 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 180 */	NdrFcShort( 0x2 ),	/* Offset= 2 (182) */
/* 182 */	
			0x13, 0x0,	/* FC_OP */
/* 184 */	NdrFcShort( 0x2 ),	/* Offset= 2 (186) */
/* 186 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 188 */	NdrFcShort( 0x4 ),	/* 4 */
/* 190 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 192 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 194 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 196 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 198 */	NdrFcShort( 0x4 ),	/* 4 */
/* 200 */	NdrFcShort( 0x0 ),	/* 0 */
/* 202 */	NdrFcShort( 0x1 ),	/* 1 */
/* 204 */	NdrFcShort( 0x0 ),	/* 0 */
/* 206 */	NdrFcShort( 0x0 ),	/* 0 */
/* 208 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 210 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 212 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 214 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 216 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 218 */	NdrFcShort( 0x2 ),	/* Offset= 2 (220) */
/* 220 */	
			0x13, 0x0,	/* FC_OP */
/* 222 */	NdrFcShort( 0x2 ),	/* Offset= 2 (224) */
/* 224 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 226 */	NdrFcShort( 0x10 ),	/* 16 */
/* 228 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 230 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 232 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 234 */	NdrFcShort( 0xff52 ),	/* Offset= -174 (60) */
/* 236 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 238 */	
			0x11, 0x0,	/* FC_RP */
/* 240 */	NdrFcShort( 0x2 ),	/* Offset= 2 (242) */
/* 242 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 244 */	NdrFcShort( 0x4 ),	/* 4 */
/* 246 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 248 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 250 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 252 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 254 */	NdrFcShort( 0x2 ),	/* Offset= 2 (256) */
/* 256 */	
			0x13, 0x0,	/* FC_OP */
/* 258 */	NdrFcShort( 0x416 ),	/* Offset= 1046 (1304) */
/* 260 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 262 */	NdrFcShort( 0x4 ),	/* 4 */
/* 264 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 266 */	NdrFcShort( 0x40 ),	/* 64 */
/* 268 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 270 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 272 */	NdrFcShort( 0x4 ),	/* 4 */
/* 274 */	NdrFcShort( 0x0 ),	/* 0 */
/* 276 */	NdrFcShort( 0x1 ),	/* 1 */
/* 278 */	NdrFcShort( 0x0 ),	/* 0 */
/* 280 */	NdrFcShort( 0x0 ),	/* 0 */
/* 282 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 284 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 286 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 288 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 290 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 292 */	NdrFcShort( 0x4 ),	/* 4 */
/* 294 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 296 */	NdrFcShort( 0x40 ),	/* 64 */
/* 298 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 300 */	
			0x13, 0x0,	/* FC_OP */
/* 302 */	NdrFcShort( 0x3b0 ),	/* Offset= 944 (1246) */
/* 304 */	
			0x2b,		/* FC_NON_ENCAPSULATED_UNION */
			0x9,		/* FC_ULONG */
/* 306 */	0x7,		/* Corr desc: FC_USHORT */
			0x0,		/*  */
/* 308 */	NdrFcShort( 0xfff8 ),	/* -8 */
/* 310 */	NdrFcShort( 0x2 ),	/* Offset= 2 (312) */
/* 312 */	NdrFcShort( 0x10 ),	/* 16 */
/* 314 */	NdrFcShort( 0x2f ),	/* 47 */
/* 316 */	NdrFcLong( 0x14 ),	/* 20 */
/* 320 */	NdrFcShort( 0x800b ),	/* Simple arm type: FC_HYPER */
/* 322 */	NdrFcLong( 0x3 ),	/* 3 */
/* 326 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 328 */	NdrFcLong( 0x11 ),	/* 17 */
/* 332 */	NdrFcShort( 0x8001 ),	/* Simple arm type: FC_BYTE */
/* 334 */	NdrFcLong( 0x2 ),	/* 2 */
/* 338 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 340 */	NdrFcLong( 0x4 ),	/* 4 */
/* 344 */	NdrFcShort( 0x800a ),	/* Simple arm type: FC_FLOAT */
/* 346 */	NdrFcLong( 0x5 ),	/* 5 */
/* 350 */	NdrFcShort( 0x800c ),	/* Simple arm type: FC_DOUBLE */
/* 352 */	NdrFcLong( 0xb ),	/* 11 */
/* 356 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 358 */	NdrFcLong( 0xa ),	/* 10 */
/* 362 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 364 */	NdrFcLong( 0x6 ),	/* 6 */
/* 368 */	NdrFcShort( 0xe8 ),	/* Offset= 232 (600) */
/* 370 */	NdrFcLong( 0x7 ),	/* 7 */
/* 374 */	NdrFcShort( 0x800c ),	/* Simple arm type: FC_DOUBLE */
/* 376 */	NdrFcLong( 0x8 ),	/* 8 */
/* 380 */	NdrFcShort( 0xe2 ),	/* Offset= 226 (606) */
/* 382 */	NdrFcLong( 0xd ),	/* 13 */
/* 386 */	NdrFcShort( 0xf4 ),	/* Offset= 244 (630) */
/* 388 */	NdrFcLong( 0x9 ),	/* 9 */
/* 392 */	NdrFcShort( 0x100 ),	/* Offset= 256 (648) */
/* 394 */	NdrFcLong( 0x2000 ),	/* 8192 */
/* 398 */	NdrFcShort( 0x10c ),	/* Offset= 268 (666) */
/* 400 */	NdrFcLong( 0x24 ),	/* 36 */
/* 404 */	NdrFcShort( 0x300 ),	/* Offset= 768 (1172) */
/* 406 */	NdrFcLong( 0x4024 ),	/* 16420 */
/* 410 */	NdrFcShort( 0x2fa ),	/* Offset= 762 (1172) */
/* 412 */	NdrFcLong( 0x4011 ),	/* 16401 */
/* 416 */	NdrFcShort( 0x2f8 ),	/* Offset= 760 (1176) */
/* 418 */	NdrFcLong( 0x4002 ),	/* 16386 */
/* 422 */	NdrFcShort( 0x2f6 ),	/* Offset= 758 (1180) */
/* 424 */	NdrFcLong( 0x4003 ),	/* 16387 */
/* 428 */	NdrFcShort( 0x2f4 ),	/* Offset= 756 (1184) */
/* 430 */	NdrFcLong( 0x4014 ),	/* 16404 */
/* 434 */	NdrFcShort( 0x2f2 ),	/* Offset= 754 (1188) */
/* 436 */	NdrFcLong( 0x4004 ),	/* 16388 */
/* 440 */	NdrFcShort( 0x2f0 ),	/* Offset= 752 (1192) */
/* 442 */	NdrFcLong( 0x4005 ),	/* 16389 */
/* 446 */	NdrFcShort( 0x2ee ),	/* Offset= 750 (1196) */
/* 448 */	NdrFcLong( 0x400b ),	/* 16395 */
/* 452 */	NdrFcShort( 0x2d8 ),	/* Offset= 728 (1180) */
/* 454 */	NdrFcLong( 0x400a ),	/* 16394 */
/* 458 */	NdrFcShort( 0x2d6 ),	/* Offset= 726 (1184) */
/* 460 */	NdrFcLong( 0x4006 ),	/* 16390 */
/* 464 */	NdrFcShort( 0x2e0 ),	/* Offset= 736 (1200) */
/* 466 */	NdrFcLong( 0x4007 ),	/* 16391 */
/* 470 */	NdrFcShort( 0x2d6 ),	/* Offset= 726 (1196) */
/* 472 */	NdrFcLong( 0x4008 ),	/* 16392 */
/* 476 */	NdrFcShort( 0x2d8 ),	/* Offset= 728 (1204) */
/* 478 */	NdrFcLong( 0x400d ),	/* 16397 */
/* 482 */	NdrFcShort( 0x2d6 ),	/* Offset= 726 (1208) */
/* 484 */	NdrFcLong( 0x4009 ),	/* 16393 */
/* 488 */	NdrFcShort( 0x2d4 ),	/* Offset= 724 (1212) */
/* 490 */	NdrFcLong( 0x6000 ),	/* 24576 */
/* 494 */	NdrFcShort( 0x2d2 ),	/* Offset= 722 (1216) */
/* 496 */	NdrFcLong( 0x400c ),	/* 16396 */
/* 500 */	NdrFcShort( 0x2d0 ),	/* Offset= 720 (1220) */
/* 502 */	NdrFcLong( 0x10 ),	/* 16 */
/* 506 */	NdrFcShort( 0x8002 ),	/* Simple arm type: FC_CHAR */
/* 508 */	NdrFcLong( 0x12 ),	/* 18 */
/* 512 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 514 */	NdrFcLong( 0x13 ),	/* 19 */
/* 518 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 520 */	NdrFcLong( 0x15 ),	/* 21 */
/* 524 */	NdrFcShort( 0x800b ),	/* Simple arm type: FC_HYPER */
/* 526 */	NdrFcLong( 0x16 ),	/* 22 */
/* 530 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 532 */	NdrFcLong( 0x17 ),	/* 23 */
/* 536 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 538 */	NdrFcLong( 0xe ),	/* 14 */
/* 542 */	NdrFcShort( 0x2ae ),	/* Offset= 686 (1228) */
/* 544 */	NdrFcLong( 0x400e ),	/* 16398 */
/* 548 */	NdrFcShort( 0x2b2 ),	/* Offset= 690 (1238) */
/* 550 */	NdrFcLong( 0x4010 ),	/* 16400 */
/* 554 */	NdrFcShort( 0x2b0 ),	/* Offset= 688 (1242) */
/* 556 */	NdrFcLong( 0x4012 ),	/* 16402 */
/* 560 */	NdrFcShort( 0x26c ),	/* Offset= 620 (1180) */
/* 562 */	NdrFcLong( 0x4013 ),	/* 16403 */
/* 566 */	NdrFcShort( 0x26a ),	/* Offset= 618 (1184) */
/* 568 */	NdrFcLong( 0x4015 ),	/* 16405 */
/* 572 */	NdrFcShort( 0x268 ),	/* Offset= 616 (1188) */
/* 574 */	NdrFcLong( 0x4016 ),	/* 16406 */
/* 578 */	NdrFcShort( 0x25e ),	/* Offset= 606 (1184) */
/* 580 */	NdrFcLong( 0x4017 ),	/* 16407 */
/* 584 */	NdrFcShort( 0x258 ),	/* Offset= 600 (1184) */
/* 586 */	NdrFcLong( 0x0 ),	/* 0 */
/* 590 */	NdrFcShort( 0x0 ),	/* Offset= 0 (590) */
/* 592 */	NdrFcLong( 0x1 ),	/* 1 */
/* 596 */	NdrFcShort( 0x0 ),	/* Offset= 0 (596) */
/* 598 */	NdrFcShort( 0xffff ),	/* Offset= -1 (597) */
/* 600 */	
			0x15,		/* FC_STRUCT */
			0x7,		/* 7 */
/* 602 */	NdrFcShort( 0x8 ),	/* 8 */
/* 604 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 606 */	
			0x13, 0x0,	/* FC_OP */
/* 608 */	NdrFcShort( 0xc ),	/* Offset= 12 (620) */
/* 610 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 612 */	NdrFcShort( 0x2 ),	/* 2 */
/* 614 */	0x9,		/* Corr desc: FC_ULONG */
			0x0,		/*  */
/* 616 */	NdrFcShort( 0xfffc ),	/* -4 */
/* 618 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 620 */	
			0x17,		/* FC_CSTRUCT */
			0x3,		/* 3 */
/* 622 */	NdrFcShort( 0x8 ),	/* 8 */
/* 624 */	NdrFcShort( 0xfff2 ),	/* Offset= -14 (610) */
/* 626 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 628 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 630 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 632 */	NdrFcLong( 0x0 ),	/* 0 */
/* 636 */	NdrFcShort( 0x0 ),	/* 0 */
/* 638 */	NdrFcShort( 0x0 ),	/* 0 */
/* 640 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 642 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 644 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 646 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 648 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 650 */	NdrFcLong( 0x20400 ),	/* 132096 */
/* 654 */	NdrFcShort( 0x0 ),	/* 0 */
/* 656 */	NdrFcShort( 0x0 ),	/* 0 */
/* 658 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 660 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 662 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 664 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 666 */	
			0x13, 0x10,	/* FC_OP [pointer_deref] */
/* 668 */	NdrFcShort( 0x2 ),	/* Offset= 2 (670) */
/* 670 */	
			0x13, 0x0,	/* FC_OP */
/* 672 */	NdrFcShort( 0x1e2 ),	/* Offset= 482 (1154) */
/* 674 */	
			0x2a,		/* FC_ENCAPSULATED_UNION */
			0x49,		/* 73 */
/* 676 */	NdrFcShort( 0x18 ),	/* 24 */
/* 678 */	NdrFcShort( 0xa ),	/* 10 */
/* 680 */	NdrFcLong( 0x8 ),	/* 8 */
/* 684 */	NdrFcShort( 0x58 ),	/* Offset= 88 (772) */
/* 686 */	NdrFcLong( 0xd ),	/* 13 */
/* 690 */	NdrFcShort( 0x78 ),	/* Offset= 120 (810) */
/* 692 */	NdrFcLong( 0x9 ),	/* 9 */
/* 696 */	NdrFcShort( 0x94 ),	/* Offset= 148 (844) */
/* 698 */	NdrFcLong( 0xc ),	/* 12 */
/* 702 */	NdrFcShort( 0xbc ),	/* Offset= 188 (890) */
/* 704 */	NdrFcLong( 0x24 ),	/* 36 */
/* 708 */	NdrFcShort( 0x114 ),	/* Offset= 276 (984) */
/* 710 */	NdrFcLong( 0x800d ),	/* 32781 */
/* 714 */	NdrFcShort( 0x11e ),	/* Offset= 286 (1000) */
/* 716 */	NdrFcLong( 0x10 ),	/* 16 */
/* 720 */	NdrFcShort( 0x136 ),	/* Offset= 310 (1030) */
/* 722 */	NdrFcLong( 0x2 ),	/* 2 */
/* 726 */	NdrFcShort( 0x14e ),	/* Offset= 334 (1060) */
/* 728 */	NdrFcLong( 0x3 ),	/* 3 */
/* 732 */	NdrFcShort( 0x166 ),	/* Offset= 358 (1090) */
/* 734 */	NdrFcLong( 0x14 ),	/* 20 */
/* 738 */	NdrFcShort( 0x17e ),	/* Offset= 382 (1120) */
/* 740 */	NdrFcShort( 0xffff ),	/* Offset= -1 (739) */
/* 742 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 744 */	NdrFcShort( 0x4 ),	/* 4 */
/* 746 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 748 */	NdrFcShort( 0x0 ),	/* 0 */
/* 750 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 752 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 754 */	NdrFcShort( 0x4 ),	/* 4 */
/* 756 */	NdrFcShort( 0x0 ),	/* 0 */
/* 758 */	NdrFcShort( 0x1 ),	/* 1 */
/* 760 */	NdrFcShort( 0x0 ),	/* 0 */
/* 762 */	NdrFcShort( 0x0 ),	/* 0 */
/* 764 */	0x13, 0x0,	/* FC_OP */
/* 766 */	NdrFcShort( 0xff6e ),	/* Offset= -146 (620) */
/* 768 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 770 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 772 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 774 */	NdrFcShort( 0x8 ),	/* 8 */
/* 776 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 778 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 780 */	NdrFcShort( 0x4 ),	/* 4 */
/* 782 */	NdrFcShort( 0x4 ),	/* 4 */
/* 784 */	0x11, 0x0,	/* FC_RP */
/* 786 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (742) */
/* 788 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 790 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 792 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 794 */	NdrFcShort( 0x0 ),	/* 0 */
/* 796 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 798 */	NdrFcShort( 0x0 ),	/* 0 */
/* 800 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 804 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 806 */	NdrFcShort( 0xff50 ),	/* Offset= -176 (630) */
/* 808 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 810 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 812 */	NdrFcShort( 0x8 ),	/* 8 */
/* 814 */	NdrFcShort( 0x0 ),	/* 0 */
/* 816 */	NdrFcShort( 0x6 ),	/* Offset= 6 (822) */
/* 818 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 820 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 822 */	
			0x11, 0x0,	/* FC_RP */
/* 824 */	NdrFcShort( 0xffe0 ),	/* Offset= -32 (792) */
/* 826 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 828 */	NdrFcShort( 0x0 ),	/* 0 */
/* 830 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 832 */	NdrFcShort( 0x0 ),	/* 0 */
/* 834 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 838 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 840 */	NdrFcShort( 0xff40 ),	/* Offset= -192 (648) */
/* 842 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 844 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 846 */	NdrFcShort( 0x8 ),	/* 8 */
/* 848 */	NdrFcShort( 0x0 ),	/* 0 */
/* 850 */	NdrFcShort( 0x6 ),	/* Offset= 6 (856) */
/* 852 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 854 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 856 */	
			0x11, 0x0,	/* FC_RP */
/* 858 */	NdrFcShort( 0xffe0 ),	/* Offset= -32 (826) */
/* 860 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 862 */	NdrFcShort( 0x4 ),	/* 4 */
/* 864 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 866 */	NdrFcShort( 0x0 ),	/* 0 */
/* 868 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 870 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 872 */	NdrFcShort( 0x4 ),	/* 4 */
/* 874 */	NdrFcShort( 0x0 ),	/* 0 */
/* 876 */	NdrFcShort( 0x1 ),	/* 1 */
/* 878 */	NdrFcShort( 0x0 ),	/* 0 */
/* 880 */	NdrFcShort( 0x0 ),	/* 0 */
/* 882 */	0x13, 0x0,	/* FC_OP */
/* 884 */	NdrFcShort( 0x16a ),	/* Offset= 362 (1246) */
/* 886 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 888 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 890 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 892 */	NdrFcShort( 0x8 ),	/* 8 */
/* 894 */	NdrFcShort( 0x0 ),	/* 0 */
/* 896 */	NdrFcShort( 0x6 ),	/* Offset= 6 (902) */
/* 898 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 900 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 902 */	
			0x11, 0x0,	/* FC_RP */
/* 904 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (860) */
/* 906 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 908 */	NdrFcLong( 0x2f ),	/* 47 */
/* 912 */	NdrFcShort( 0x0 ),	/* 0 */
/* 914 */	NdrFcShort( 0x0 ),	/* 0 */
/* 916 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 918 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 920 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 922 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 924 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 926 */	NdrFcShort( 0x1 ),	/* 1 */
/* 928 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 930 */	NdrFcShort( 0x4 ),	/* 4 */
/* 932 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 934 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 936 */	NdrFcShort( 0x10 ),	/* 16 */
/* 938 */	NdrFcShort( 0x0 ),	/* 0 */
/* 940 */	NdrFcShort( 0xa ),	/* Offset= 10 (950) */
/* 942 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 944 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 946 */	NdrFcShort( 0xffd8 ),	/* Offset= -40 (906) */
/* 948 */	0x36,		/* FC_POINTER */
			0x5b,		/* FC_END */
/* 950 */	
			0x13, 0x0,	/* FC_OP */
/* 952 */	NdrFcShort( 0xffe4 ),	/* Offset= -28 (924) */
/* 954 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 956 */	NdrFcShort( 0x4 ),	/* 4 */
/* 958 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 960 */	NdrFcShort( 0x0 ),	/* 0 */
/* 962 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 964 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 966 */	NdrFcShort( 0x4 ),	/* 4 */
/* 968 */	NdrFcShort( 0x0 ),	/* 0 */
/* 970 */	NdrFcShort( 0x1 ),	/* 1 */
/* 972 */	NdrFcShort( 0x0 ),	/* 0 */
/* 974 */	NdrFcShort( 0x0 ),	/* 0 */
/* 976 */	0x13, 0x0,	/* FC_OP */
/* 978 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (934) */
/* 980 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 982 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 984 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 986 */	NdrFcShort( 0x8 ),	/* 8 */
/* 988 */	NdrFcShort( 0x0 ),	/* 0 */
/* 990 */	NdrFcShort( 0x6 ),	/* Offset= 6 (996) */
/* 992 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 994 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 996 */	
			0x11, 0x0,	/* FC_RP */
/* 998 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (954) */
/* 1000 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1002 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1004 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1006 */	NdrFcShort( 0xa ),	/* Offset= 10 (1016) */
/* 1008 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1010 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1012 */	NdrFcShort( 0xfc48 ),	/* Offset= -952 (60) */
/* 1014 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1016 */	
			0x11, 0x0,	/* FC_RP */
/* 1018 */	NdrFcShort( 0xff1e ),	/* Offset= -226 (792) */
/* 1020 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 1022 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1024 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1026 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1028 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 1030 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1032 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1034 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1036 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1038 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1040 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1042 */	0x13, 0x0,	/* FC_OP */
/* 1044 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1020) */
/* 1046 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1048 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1050 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 1052 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1054 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1056 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1058 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 1060 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1062 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1064 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1066 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1068 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1070 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1072 */	0x13, 0x0,	/* FC_OP */
/* 1074 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1050) */
/* 1076 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1078 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1080 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1082 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1084 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1086 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1088 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1090 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1092 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1094 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1096 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1098 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1100 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1102 */	0x13, 0x0,	/* FC_OP */
/* 1104 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1080) */
/* 1106 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1108 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1110 */	
			0x1b,		/* FC_CARRAY */
			0x7,		/* 7 */
/* 1112 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1114 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1116 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1118 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 1120 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1122 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1124 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1126 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1128 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1130 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1132 */	0x13, 0x0,	/* FC_OP */
/* 1134 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1110) */
/* 1136 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1138 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1140 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1142 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1144 */	0x7,		/* Corr desc: FC_USHORT */
			0x0,		/*  */
/* 1146 */	NdrFcShort( 0xffd8 ),	/* -40 */
/* 1148 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1150 */	NdrFcShort( 0xfb8c ),	/* Offset= -1140 (10) */
/* 1152 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1154 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1156 */	NdrFcShort( 0x28 ),	/* 40 */
/* 1158 */	NdrFcShort( 0xffee ),	/* Offset= -18 (1140) */
/* 1160 */	NdrFcShort( 0x0 ),	/* Offset= 0 (1160) */
/* 1162 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1164 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1166 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1168 */	NdrFcShort( 0xfe12 ),	/* Offset= -494 (674) */
/* 1170 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1172 */	
			0x13, 0x0,	/* FC_OP */
/* 1174 */	NdrFcShort( 0xff10 ),	/* Offset= -240 (934) */
/* 1176 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1178 */	0x1,		/* FC_BYTE */
			0x5c,		/* FC_PAD */
/* 1180 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1182 */	0x6,		/* FC_SHORT */
			0x5c,		/* FC_PAD */
/* 1184 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1186 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 1188 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1190 */	0xb,		/* FC_HYPER */
			0x5c,		/* FC_PAD */
/* 1192 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1194 */	0xa,		/* FC_FLOAT */
			0x5c,		/* FC_PAD */
/* 1196 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1198 */	0xc,		/* FC_DOUBLE */
			0x5c,		/* FC_PAD */
/* 1200 */	
			0x13, 0x0,	/* FC_OP */
/* 1202 */	NdrFcShort( 0xfda6 ),	/* Offset= -602 (600) */
/* 1204 */	
			0x13, 0x10,	/* FC_OP [pointer_deref] */
/* 1206 */	NdrFcShort( 0xfda8 ),	/* Offset= -600 (606) */
/* 1208 */	
			0x13, 0x10,	/* FC_OP [pointer_deref] */
/* 1210 */	NdrFcShort( 0xfdbc ),	/* Offset= -580 (630) */
/* 1212 */	
			0x13, 0x10,	/* FC_OP [pointer_deref] */
/* 1214 */	NdrFcShort( 0xfdca ),	/* Offset= -566 (648) */
/* 1216 */	
			0x13, 0x10,	/* FC_OP [pointer_deref] */
/* 1218 */	NdrFcShort( 0xfdd8 ),	/* Offset= -552 (666) */
/* 1220 */	
			0x13, 0x10,	/* FC_OP [pointer_deref] */
/* 1222 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1224) */
/* 1224 */	
			0x13, 0x0,	/* FC_OP */
/* 1226 */	NdrFcShort( 0x14 ),	/* Offset= 20 (1246) */
/* 1228 */	
			0x15,		/* FC_STRUCT */
			0x7,		/* 7 */
/* 1230 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1232 */	0x6,		/* FC_SHORT */
			0x1,		/* FC_BYTE */
/* 1234 */	0x1,		/* FC_BYTE */
			0x8,		/* FC_LONG */
/* 1236 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 1238 */	
			0x13, 0x0,	/* FC_OP */
/* 1240 */	NdrFcShort( 0xfff4 ),	/* Offset= -12 (1228) */
/* 1242 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1244 */	0x2,		/* FC_CHAR */
			0x5c,		/* FC_PAD */
/* 1246 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x7,		/* 7 */
/* 1248 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1250 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1252 */	NdrFcShort( 0x0 ),	/* Offset= 0 (1252) */
/* 1254 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1256 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1258 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1260 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1262 */	NdrFcShort( 0xfc42 ),	/* Offset= -958 (304) */
/* 1264 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1266 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1268 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1270 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1272 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1274 */	NdrFcShort( 0xfc32 ),	/* Offset= -974 (300) */
/* 1276 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1278 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1280 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1282 */	NdrFcShort( 0x54 ),	/* 84 */
/* 1284 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1288 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1290 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1266) */
/* 1292 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1294 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1296 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1298 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1300 */	NdrFcShort( 0x54 ),	/* 84 */
/* 1302 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1304 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1306 */	NdrFcShort( 0x60 ),	/* 96 */
/* 1308 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1310 */	NdrFcShort( 0x26 ),	/* Offset= 38 (1348) */
/* 1312 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1314 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1316 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1318 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1320 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1322 */	NdrFcShort( 0xfae0 ),	/* Offset= -1312 (10) */
/* 1324 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1326 */	NdrFcShort( 0xfadc ),	/* Offset= -1316 (10) */
/* 1328 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1330 */	NdrFcShort( 0xfad8 ),	/* Offset= -1320 (10) */
/* 1332 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1334 */	NdrFcShort( 0xfad4 ),	/* Offset= -1324 (10) */
/* 1336 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1338 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1340 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1342 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 1344 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1346 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1348 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1350 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1352 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1354 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1356 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1358 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1360 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1362 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1364 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1366 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1368 */	
			0x13, 0x0,	/* FC_OP */
/* 1370 */	NdrFcShort( 0xfbaa ),	/* Offset= -1110 (260) */
/* 1372 */	
			0x13, 0x0,	/* FC_OP */
/* 1374 */	NdrFcShort( 0xfba6 ),	/* Offset= -1114 (260) */
/* 1376 */	
			0x13, 0x0,	/* FC_OP */
/* 1378 */	NdrFcShort( 0xfbc0 ),	/* Offset= -1088 (290) */
/* 1380 */	
			0x13, 0x0,	/* FC_OP */
/* 1382 */	NdrFcShort( 0xfb9e ),	/* Offset= -1122 (260) */
/* 1384 */	
			0x13, 0x0,	/* FC_OP */
/* 1386 */	NdrFcShort( 0xff92 ),	/* Offset= -110 (1276) */
/* 1388 */	
			0x13, 0x0,	/* FC_OP */
/* 1390 */	NdrFcShort( 0xffa0 ),	/* Offset= -96 (1294) */
/* 1392 */	
			0x11, 0x0,	/* FC_RP */
/* 1394 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1396) */
/* 1396 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1398 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1400 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1402 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1404 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1406 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1408 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1410 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1412 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1414 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1416 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1418 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1420 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1422 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1424 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1426 */	
			0x11, 0x0,	/* FC_RP */
/* 1428 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1430) */
/* 1430 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1432 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1434 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1436 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1438 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1440 */	NdrFcShort( 0xfa6a ),	/* Offset= -1430 (10) */
/* 1442 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1444 */	
			0x11, 0x0,	/* FC_RP */
/* 1446 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1448) */
/* 1448 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1450 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1452 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1454 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1456 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1458 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1460 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1462) */
/* 1462 */	
			0x13, 0x0,	/* FC_OP */
/* 1464 */	NdrFcShort( 0xfff0 ),	/* Offset= -16 (1448) */
/* 1466 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 1468 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1470) */
/* 1470 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 1472 */	0x28,		/* Corr desc:  parameter, FC_LONG */
			0x0,		/*  */
/* 1474 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1476 */	
			0x11, 0x0,	/* FC_RP */
/* 1478 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1480) */
/* 1480 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1482 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1484 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1486 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1488 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1490 */	
			0x11, 0x0,	/* FC_RP */
/* 1492 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1494) */
/* 1494 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1496 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1498 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1500 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1502 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1504 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1506 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1508 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1510 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1512 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1514 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1516 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1518 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1520 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1522 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1524 */	
			0x11, 0x0,	/* FC_RP */
/* 1526 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1528) */
/* 1528 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1530 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1532 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1534 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1536 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1538 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1540 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1542 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1544 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1546 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1548 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1550 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1552 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1554 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1556 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1558 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1560 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1562) */
/* 1562 */	
			0x13, 0x0,	/* FC_OP */
/* 1564 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1566) */
/* 1566 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1568 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1570 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 1572 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1574 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1576 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1578 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1580 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1582 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1584 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1586 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1588 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1590 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1592 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1594 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1596 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1598 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1600) */
/* 1600 */	
			0x13, 0x0,	/* FC_OP */
/* 1602 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1604) */
/* 1604 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1606 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1608 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 1610 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1612 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1614 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1616 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1618 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1620 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1622 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1624 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1626 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1628 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1630 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1632 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1634 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1636 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 1638 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 1640 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1642) */
/* 1642 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 1644 */	NdrFcLong( 0x101 ),	/* 257 */
/* 1648 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1650 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1652 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 1654 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 1656 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 1658 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 1660 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1662 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1664) */
/* 1664 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1666 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1668 */	
			0x11, 0x0,	/* FC_RP */
/* 1670 */	NdrFcShort( 0x5c ),	/* Offset= 92 (1762) */
/* 1672 */	
			0x12, 0x0,	/* FC_UP */
/* 1674 */	NdrFcShort( 0xfe54 ),	/* Offset= -428 (1246) */
/* 1676 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1678 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1680 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1682 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1684 */	NdrFcShort( 0xfff4 ),	/* Offset= -12 (1672) */
/* 1686 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1688 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1690 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1692 */	NdrFcShort( 0x3c ),	/* 60 */
/* 1694 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1698 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1700 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1676) */
/* 1702 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1704 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1706 */	NdrFcShort( 0x48 ),	/* 72 */
/* 1708 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1710 */	NdrFcShort( 0x1c ),	/* Offset= 28 (1738) */
/* 1712 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1714 */	0x36,		/* FC_POINTER */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1716 */	0x0,		/* 0 */
			NdrFcShort( 0xf955 ),	/* Offset= -1707 (10) */
			0x36,		/* FC_POINTER */
/* 1720 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1722 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1724 */	0x36,		/* FC_POINTER */
			0x6,		/* FC_SHORT */
/* 1726 */	0x6,		/* FC_SHORT */
			0x8,		/* FC_LONG */
/* 1728 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1730 */	NdrFcShort( 0xf948 ),	/* Offset= -1720 (10) */
/* 1732 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1734 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1736 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1738 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1740 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1742 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1744 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1746 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1748 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1750 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1752 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1754 */	
			0x12, 0x0,	/* FC_UP */
/* 1756 */	NdrFcShort( 0xffba ),	/* Offset= -70 (1686) */
/* 1758 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1760 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1762 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1764 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1766 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1768 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1770 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1774 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1776 */	NdrFcShort( 0xffb8 ),	/* Offset= -72 (1704) */
/* 1778 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */

			0x0
        }
    };

static const USER_MARSHAL_ROUTINE_QUADRUPLE UserMarshalRoutines[ WIRE_MARSHAL_TABLE_SIZE ] = 
        {
            
            {
            VARIANT_UserSize
            ,VARIANT_UserMarshal
            ,VARIANT_UserUnmarshal
            ,VARIANT_UserFree
            }

        };



/* Object interface: IUnknown, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0xC0,0x00,0x00,0x00,0x00,0x00,0x00,0x46}} */


/* Object interface: OPCEventServerCATID, ver. 0.0,
   GUID={0x58E13251,0xAC87,0x11d1,{0x84,0xD5,0x00,0x60,0x8C,0xB8,0xA7,0xE9}} */

#pragma code_seg(".orpc")
static const unsigned short OPCEventServerCATID_FormatStringOffsetTable[] =
    {
    0
    };

static const MIDL_STUBLESS_PROXY_INFO OPCEventServerCATID_ProxyInfo =
    {
    &Object_StubDesc,
    opc_ae__MIDL_ProcFormatString.Format,
    &OPCEventServerCATID_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO OPCEventServerCATID_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opc_ae__MIDL_ProcFormatString.Format,
    &OPCEventServerCATID_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(3) _OPCEventServerCATIDProxyVtbl = 
{
    0,
    &IID_OPCEventServerCATID,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy
};

const CInterfaceStubVtbl _OPCEventServerCATIDStubVtbl =
{
    &IID_OPCEventServerCATID,
    &OPCEventServerCATID_ServerInfo,
    3,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Standard interface: __MIDL_itf_opc_ae_0000_0001, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: IOPCEventServer, ver. 0.0,
   GUID={0x65168851,0x5783,0x11D1,{0x84,0xA0,0x00,0x60,0x8C,0xB8,0xA7,0xE9}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCEventServer_FormatStringOffsetTable[] =
    {
    0,
    28,
    98,
    126,
    172,
    212,
    252,
    292,
    344,
    420,
    472,
    506,
    540,
    574,
    608,
    678
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCEventServer_ProxyInfo =
    {
    &Object_StubDesc,
    opc_ae__MIDL_ProcFormatString.Format,
    &IOPCEventServer_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCEventServer_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opc_ae__MIDL_ProcFormatString.Format,
    &IOPCEventServer_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(19) _IOPCEventServerProxyVtbl = 
{
    &IOPCEventServer_ProxyInfo,
    &IID_IOPCEventServer,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::GetStatus */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::CreateEventSubscription */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::QueryAvailableFilters */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::QueryEventCategories */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::QueryConditionNames */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::QuerySubConditionNames */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::QuerySourceConditions */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::QueryEventAttributes */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::TranslateToItemIDs */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::GetConditionState */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::EnableConditionByArea */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::EnableConditionBySource */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::DisableConditionByArea */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::DisableConditionBySource */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::AckCondition */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::CreateAreaBrowser */
};

const CInterfaceStubVtbl _IOPCEventServerStubVtbl =
{
    &IID_IOPCEventServer,
    &IOPCEventServer_ServerInfo,
    19,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCEventSubscriptionMgt, ver. 0.0,
   GUID={0x65168855,0x5783,0x11D1,{0x84,0xA0,0x00,0x60,0x8C,0xB8,0xA7,0xE9}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCEventSubscriptionMgt_FormatStringOffsetTable[] =
    {
    712,
    788,
    864,
    904,
    944,
    972,
    1000,
    1046
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCEventSubscriptionMgt_ProxyInfo =
    {
    &Object_StubDesc,
    opc_ae__MIDL_ProcFormatString.Format,
    &IOPCEventSubscriptionMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCEventSubscriptionMgt_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opc_ae__MIDL_ProcFormatString.Format,
    &IOPCEventSubscriptionMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(11) _IOPCEventSubscriptionMgtProxyVtbl = 
{
    &IOPCEventSubscriptionMgt_ProxyInfo,
    &IID_IOPCEventSubscriptionMgt,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::SetFilter */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::GetFilter */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::SelectReturnedAttributes */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::GetReturnedAttributes */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::Refresh */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::CancelRefresh */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::GetState */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::SetState */
};

const CInterfaceStubVtbl _IOPCEventSubscriptionMgtStubVtbl =
{
    &IID_IOPCEventSubscriptionMgt,
    &IOPCEventSubscriptionMgt_ServerInfo,
    11,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCEventAreaBrowser, ver. 0.0,
   GUID={0x65168857,0x5783,0x11D1,{0x84,0xA0,0x00,0x60,0x8C,0xB8,0xA7,0xE9}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCEventAreaBrowser_FormatStringOffsetTable[] =
    {
    1104,
    1138,
    1178,
    1212
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCEventAreaBrowser_ProxyInfo =
    {
    &Object_StubDesc,
    opc_ae__MIDL_ProcFormatString.Format,
    &IOPCEventAreaBrowser_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCEventAreaBrowser_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opc_ae__MIDL_ProcFormatString.Format,
    &IOPCEventAreaBrowser_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(7) _IOPCEventAreaBrowserProxyVtbl = 
{
    &IOPCEventAreaBrowser_ProxyInfo,
    &IID_IOPCEventAreaBrowser,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCEventAreaBrowser::ChangeBrowsePosition */ ,
    (void *) (INT_PTR) -1 /* IOPCEventAreaBrowser::BrowseOPCAreas */ ,
    (void *) (INT_PTR) -1 /* IOPCEventAreaBrowser::GetQualifiedAreaName */ ,
    (void *) (INT_PTR) -1 /* IOPCEventAreaBrowser::GetQualifiedSourceName */
};

const CInterfaceStubVtbl _IOPCEventAreaBrowserStubVtbl =
{
    &IID_IOPCEventAreaBrowser,
    &IOPCEventAreaBrowser_ServerInfo,
    7,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCEventSink, ver. 0.0,
   GUID={0x6516885F,0x5783,0x11D1,{0x84,0xA0,0x00,0x60,0x8C,0xB8,0xA7,0xE9}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCEventSink_FormatStringOffsetTable[] =
    {
    1246
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCEventSink_ProxyInfo =
    {
    &Object_StubDesc,
    opc_ae__MIDL_ProcFormatString.Format,
    &IOPCEventSink_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCEventSink_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opc_ae__MIDL_ProcFormatString.Format,
    &IOPCEventSink_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(4) _IOPCEventSinkProxyVtbl = 
{
    &IOPCEventSink_ProxyInfo,
    &IID_IOPCEventSink,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCEventSink::OnEvent */
};

const CInterfaceStubVtbl _IOPCEventSinkStubVtbl =
{
    &IID_IOPCEventSink,
    &IOPCEventSink_ServerInfo,
    4,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCEventServer2, ver. 0.0,
   GUID={0x71BBE88E,0x9564,0x4bcd,{0xBC,0xFC,0x71,0xC5,0x58,0xD9,0x4F,0x2D}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCEventServer2_FormatStringOffsetTable[] =
    {
    0,
    28,
    98,
    126,
    172,
    212,
    252,
    292,
    344,
    420,
    472,
    506,
    540,
    574,
    608,
    678,
    1298,
    1338,
    1378,
    1418,
    1458,
    1510
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCEventServer2_ProxyInfo =
    {
    &Object_StubDesc,
    opc_ae__MIDL_ProcFormatString.Format,
    &IOPCEventServer2_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCEventServer2_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opc_ae__MIDL_ProcFormatString.Format,
    &IOPCEventServer2_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(25) _IOPCEventServer2ProxyVtbl = 
{
    &IOPCEventServer2_ProxyInfo,
    &IID_IOPCEventServer2,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::GetStatus */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::CreateEventSubscription */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::QueryAvailableFilters */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::QueryEventCategories */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::QueryConditionNames */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::QuerySubConditionNames */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::QuerySourceConditions */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::QueryEventAttributes */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::TranslateToItemIDs */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::GetConditionState */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::EnableConditionByArea */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::EnableConditionBySource */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::DisableConditionByArea */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::DisableConditionBySource */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::AckCondition */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer::CreateAreaBrowser */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer2::EnableConditionByArea2 */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer2::EnableConditionBySource2 */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer2::DisableConditionByArea2 */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer2::DisableConditionBySource2 */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer2::GetEnableStateByArea */ ,
    (void *) (INT_PTR) -1 /* IOPCEventServer2::GetEnableStateBySource */
};

const CInterfaceStubVtbl _IOPCEventServer2StubVtbl =
{
    &IID_IOPCEventServer2,
    &IOPCEventServer2_ServerInfo,
    25,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCEventSubscriptionMgt2, ver. 0.0,
   GUID={0x94C955DC,0x3684,0x4ccb,{0xAF,0xAB,0xF8,0x98,0xCE,0x19,0xAA,0xC3}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCEventSubscriptionMgt2_FormatStringOffsetTable[] =
    {
    712,
    788,
    864,
    904,
    944,
    972,
    1000,
    1046,
    1562,
    1596
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCEventSubscriptionMgt2_ProxyInfo =
    {
    &Object_StubDesc,
    opc_ae__MIDL_ProcFormatString.Format,
    &IOPCEventSubscriptionMgt2_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCEventSubscriptionMgt2_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opc_ae__MIDL_ProcFormatString.Format,
    &IOPCEventSubscriptionMgt2_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(13) _IOPCEventSubscriptionMgt2ProxyVtbl = 
{
    &IOPCEventSubscriptionMgt2_ProxyInfo,
    &IID_IOPCEventSubscriptionMgt2,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::SetFilter */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::GetFilter */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::SelectReturnedAttributes */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::GetReturnedAttributes */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::Refresh */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::CancelRefresh */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::GetState */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt::SetState */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt2::SetKeepAlive */ ,
    (void *) (INT_PTR) -1 /* IOPCEventSubscriptionMgt2::GetKeepAlive */
};

const CInterfaceStubVtbl _IOPCEventSubscriptionMgt2StubVtbl =
{
    &IID_IOPCEventSubscriptionMgt2,
    &IOPCEventSubscriptionMgt2_ServerInfo,
    13,
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
    opc_ae__MIDL_TypeFormatString.Format,
    1, /* -error bounds_check flag */
    0x20000, /* Ndr library version */
    0,
    0x800025b, /* MIDL Version 8.0.603 */
    0,
    UserMarshalRoutines,
    0,  /* notify & notify_flag routine table */
    0x1, /* MIDL flag */
    0, /* cs routines */
    0,   /* proxy/server info */
    0
    };

const CInterfaceProxyVtbl * const _opc_ae_ProxyVtblList[] = 
{
    ( CInterfaceProxyVtbl *) &_OPCEventServerCATIDProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCEventServerProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCEventSubscriptionMgtProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCEventAreaBrowserProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCEventSinkProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCEventServer2ProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCEventSubscriptionMgt2ProxyVtbl,
    0
};

const CInterfaceStubVtbl * const _opc_ae_StubVtblList[] = 
{
    ( CInterfaceStubVtbl *) &_OPCEventServerCATIDStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCEventServerStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCEventSubscriptionMgtStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCEventAreaBrowserStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCEventSinkStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCEventServer2StubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCEventSubscriptionMgt2StubVtbl,
    0
};

PCInterfaceName const _opc_ae_InterfaceNamesList[] = 
{
    "OPCEventServerCATID",
    "IOPCEventServer",
    "IOPCEventSubscriptionMgt",
    "IOPCEventAreaBrowser",
    "IOPCEventSink",
    "IOPCEventServer2",
    "IOPCEventSubscriptionMgt2",
    0
};


#define _opc_ae_CHECK_IID(n)	IID_GENERIC_CHECK_IID( _opc_ae, pIID, n)

int __stdcall _opc_ae_IID_Lookup( const IID * pIID, int * pIndex )
{
    IID_BS_LOOKUP_SETUP

    IID_BS_LOOKUP_INITIAL_TEST( _opc_ae, 7, 4 )
    IID_BS_LOOKUP_NEXT_TEST( _opc_ae, 2 )
    IID_BS_LOOKUP_NEXT_TEST( _opc_ae, 1 )
    IID_BS_LOOKUP_RETURN_RESULT( _opc_ae, 7, *pIndex )
    
}

const ExtendedProxyFileInfo opc_ae_ProxyFileInfo = 
{
    (PCInterfaceProxyVtblList *) & _opc_ae_ProxyVtblList,
    (PCInterfaceStubVtblList *) & _opc_ae_StubVtblList,
    (const PCInterfaceName * ) & _opc_ae_InterfaceNamesList,
    0, /* no delegation */
    & _opc_ae_IID_Lookup, 
    7,
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

