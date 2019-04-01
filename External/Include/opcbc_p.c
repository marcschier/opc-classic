

/* this ALWAYS GENERATED file contains the proxy stub code */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Tue Jun 30 10:35:04 2015
 */
/* Compiler settings for opcbc.idl:
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


#include "opcbc.h"

#define TYPE_FORMAT_STRING_SIZE   493                               
#define PROC_FORMAT_STRING_SIZE   381                               
#define EXPR_FORMAT_STRING_SIZE   1                                 
#define TRANSMIT_AS_TABLE_SIZE    0            
#define WIRE_MARSHAL_TABLE_SIZE   0            

typedef struct _opcbc_MIDL_TYPE_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ TYPE_FORMAT_STRING_SIZE ];
    } opcbc_MIDL_TYPE_FORMAT_STRING;

typedef struct _opcbc_MIDL_PROC_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ PROC_FORMAT_STRING_SIZE ];
    } opcbc_MIDL_PROC_FORMAT_STRING;

typedef struct _opcbc_MIDL_EXPR_FORMAT_STRING
    {
    long          Pad;
    unsigned char  Format[ EXPR_FORMAT_STRING_SIZE ];
    } opcbc_MIDL_EXPR_FORMAT_STRING;


static const RPC_SYNTAX_IDENTIFIER  _RpcTransferSyntax = 
{{0x8A885D04,0x1CEB,0x11C9,{0x9F,0xE8,0x08,0x00,0x2B,0x10,0x48,0x60}},{2,0}};


extern const opcbc_MIDL_TYPE_FORMAT_STRING opcbc__MIDL_TypeFormatString;
extern const opcbc_MIDL_PROC_FORMAT_STRING opcbc__MIDL_ProcFormatString;
extern const opcbc_MIDL_EXPR_FORMAT_STRING opcbc__MIDL_ExprFormatString;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO CATID_OPCBatchServer10_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO CATID_OPCBatchServer10_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO CATID_OPCBatchServer20_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO CATID_OPCBatchServer20_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCBatchServer_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCBatchServer_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCBatchServer2_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCBatchServer2_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IEnumOPCBatchSummary_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IEnumOPCBatchSummary_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCEnumerationSets_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCEnumerationSets_ProxyInfo;



#if !defined(__RPC_WIN32__)
#error  Invalid build platform for this stub.
#endif

#if !(TARGET_IS_NT40_OR_LATER)
#error You need Windows NT 4.0 or later to run this stub because it uses these features:
#error   -Oif or -Oicf.
#error However, your C/C++ compilation flags indicate you intend to run this app on earlier systems.
#error This app will fail with the RPC_X_WRONG_STUB_VERSION error.
#endif


static const opcbc_MIDL_PROC_FORMAT_STRING opcbc__MIDL_ProcFormatString =
    {
        0,
        {

	/* Procedure GetDelimiter */

			0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/*  2 */	NdrFcLong( 0x0 ),	/* 0 */
/*  6 */	NdrFcShort( 0x3 ),	/* 3 */
/*  8 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 10 */	NdrFcShort( 0x0 ),	/* 0 */
/* 12 */	NdrFcShort( 0x8 ),	/* 8 */
/* 14 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x2,		/* 2 */

	/* Parameter pszDelimiter */

/* 16 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 18 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 20 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Return value */

/* 22 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 24 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 26 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CreateEnumerator */

/* 28 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 30 */	NdrFcLong( 0x0 ),	/* 0 */
/* 34 */	NdrFcShort( 0x4 ),	/* 4 */
/* 36 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 38 */	NdrFcShort( 0x44 ),	/* 68 */
/* 40 */	NdrFcShort( 0x8 ),	/* 8 */
/* 42 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x3,		/* 3 */

	/* Parameter riid */

/* 44 */	NdrFcShort( 0x10a ),	/* Flags:  must free, in, simple ref, */
/* 46 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 48 */	NdrFcShort( 0x14 ),	/* Type Offset=20 */

	/* Parameter ppUnk */

/* 50 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 52 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 54 */	NdrFcShort( 0x20 ),	/* Type Offset=32 */

	/* Return value */

/* 56 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 58 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 60 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CreateFilteredEnumerator */

/* 62 */	0x33,		/* FC_AUTO_HANDLE */
			0x6d,		/* Old Flags:  full ptr, object, Oi2 */
/* 64 */	NdrFcLong( 0x0 ),	/* 0 */
/* 68 */	NdrFcShort( 0x3 ),	/* 3 */
/* 70 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 72 */	NdrFcShort( 0x44 ),	/* 68 */
/* 74 */	NdrFcShort( 0x8 ),	/* 8 */
/* 76 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter riid */

/* 78 */	NdrFcShort( 0x10a ),	/* Flags:  must free, in, simple ref, */
/* 80 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 82 */	NdrFcShort( 0x14 ),	/* Type Offset=20 */

	/* Parameter pFilter */

/* 84 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 86 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 88 */	NdrFcShort( 0x2a ),	/* Type Offset=42 */

	/* Parameter szModel */

/* 90 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 92 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 94 */	NdrFcShort( 0xa0 ),	/* Type Offset=160 */

	/* Parameter ppUnk */

/* 96 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 98 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 100 */	NdrFcShort( 0xa2 ),	/* Type Offset=162 */

	/* Return value */

/* 102 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 104 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 106 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Next */

/* 108 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 110 */	NdrFcLong( 0x0 ),	/* 0 */
/* 114 */	NdrFcShort( 0x3 ),	/* 3 */
/* 116 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 118 */	NdrFcShort( 0x8 ),	/* 8 */
/* 120 */	NdrFcShort( 0x24 ),	/* 36 */
/* 122 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x4,		/* 4 */

	/* Parameter celt */

/* 124 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 126 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 128 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppSummaryArray */

/* 130 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 132 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 134 */	NdrFcShort( 0xac ),	/* Type Offset=172 */

	/* Parameter pceltFetched */

/* 136 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 138 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 140 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 142 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 144 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 146 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Skip */

/* 148 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 150 */	NdrFcLong( 0x0 ),	/* 0 */
/* 154 */	NdrFcShort( 0x4 ),	/* 4 */
/* 156 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 158 */	NdrFcShort( 0x8 ),	/* 8 */
/* 160 */	NdrFcShort( 0x8 ),	/* 8 */
/* 162 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter celt */

/* 164 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 166 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 168 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 170 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 172 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 174 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Reset */

/* 176 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 178 */	NdrFcLong( 0x0 ),	/* 0 */
/* 182 */	NdrFcShort( 0x5 ),	/* 5 */
/* 184 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 186 */	NdrFcShort( 0x0 ),	/* 0 */
/* 188 */	NdrFcShort( 0x8 ),	/* 8 */
/* 190 */	0x4,		/* Oi2 Flags:  has return, */
			0x1,		/* 1 */

	/* Return value */

/* 192 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 194 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 196 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Clone */

/* 198 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 200 */	NdrFcLong( 0x0 ),	/* 0 */
/* 204 */	NdrFcShort( 0x6 ),	/* 6 */
/* 206 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 208 */	NdrFcShort( 0x0 ),	/* 0 */
/* 210 */	NdrFcShort( 0x8 ),	/* 8 */
/* 212 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x2,		/* 2 */

	/* Parameter ppEnumBatchSummary */

/* 214 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 216 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 218 */	NdrFcShort( 0x166 ),	/* Type Offset=358 */

	/* Return value */

/* 220 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 222 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 224 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Count */

/* 226 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 228 */	NdrFcLong( 0x0 ),	/* 0 */
/* 232 */	NdrFcShort( 0x7 ),	/* 7 */
/* 234 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 236 */	NdrFcShort( 0x0 ),	/* 0 */
/* 238 */	NdrFcShort( 0x24 ),	/* 36 */
/* 240 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter pcelt */

/* 242 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 244 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 246 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 248 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 250 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 252 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryEnumerationSets */

/* 254 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 256 */	NdrFcLong( 0x0 ),	/* 0 */
/* 260 */	NdrFcShort( 0x3 ),	/* 3 */
/* 262 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 264 */	NdrFcShort( 0x0 ),	/* 0 */
/* 266 */	NdrFcShort( 0x24 ),	/* 36 */
/* 268 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x4,		/* 4 */

	/* Parameter pdwCount */

/* 270 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 272 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 274 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppdwEnumSetId */

/* 276 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 278 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 280 */	NdrFcShort( 0x17c ),	/* Type Offset=380 */

	/* Parameter ppszEnumSetName */

/* 282 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 284 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 286 */	NdrFcShort( 0x18e ),	/* Type Offset=398 */

	/* Return value */

/* 288 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 290 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 292 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryEnumeration */

/* 294 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 296 */	NdrFcLong( 0x0 ),	/* 0 */
/* 300 */	NdrFcShort( 0x4 ),	/* 4 */
/* 302 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 304 */	NdrFcShort( 0x10 ),	/* 16 */
/* 306 */	NdrFcShort( 0x8 ),	/* 8 */
/* 308 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwEnumSetId */

/* 310 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 312 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 314 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwEnumValue */

/* 316 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 318 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 320 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszEnumName */

/* 322 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 324 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 326 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Return value */

/* 328 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 330 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 332 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryEnumerationList */

/* 334 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 336 */	NdrFcLong( 0x0 ),	/* 0 */
/* 340 */	NdrFcShort( 0x5 ),	/* 5 */
/* 342 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 344 */	NdrFcShort( 0x8 ),	/* 8 */
/* 346 */	NdrFcShort( 0x24 ),	/* 36 */
/* 348 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwEnumSetId */

/* 350 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 352 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 354 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCount */

/* 356 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 358 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 360 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppdwEnumValue */

/* 362 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 364 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 366 */	NdrFcShort( 0x1b4 ),	/* Type Offset=436 */

	/* Parameter ppszEnumName */

/* 368 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 370 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 372 */	NdrFcShort( 0x1c6 ),	/* Type Offset=454 */

	/* Return value */

/* 374 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 376 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 378 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

			0x0
        }
    };

static const opcbc_MIDL_TYPE_FORMAT_STRING opcbc__MIDL_TypeFormatString =
    {
        0,
        {
			NdrFcShort( 0x0 ),	/* 0 */
/*  2 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/*  4 */	NdrFcShort( 0x2 ),	/* Offset= 2 (6) */
/*  6 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/*  8 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 10 */	
			0x11, 0x0,	/* FC_RP */
/* 12 */	NdrFcShort( 0x8 ),	/* Offset= 8 (20) */
/* 14 */	
			0x1d,		/* FC_SMFARRAY */
			0x0,		/* 0 */
/* 16 */	NdrFcShort( 0x8 ),	/* 8 */
/* 18 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 20 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 22 */	NdrFcShort( 0x10 ),	/* 16 */
/* 24 */	0x8,		/* FC_LONG */
			0x6,		/* FC_SHORT */
/* 26 */	0x6,		/* FC_SHORT */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 28 */	0x0,		/* 0 */
			NdrFcShort( 0xfff1 ),	/* Offset= -15 (14) */
			0x5b,		/* FC_END */
/* 32 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 34 */	NdrFcShort( 0x2 ),	/* Offset= 2 (36) */
/* 36 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 38 */	0x28,		/* Corr desc:  parameter, FC_LONG */
			0x0,		/*  */
/* 40 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 42 */	
			0x14, 0x0,	/* FC_FP */
/* 44 */	NdrFcShort( 0xa ),	/* Offset= 10 (54) */
/* 46 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 48 */	NdrFcShort( 0x8 ),	/* 8 */
/* 50 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 52 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 54 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 56 */	NdrFcShort( 0x44 ),	/* 68 */
/* 58 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 60 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 62 */	NdrFcShort( 0x0 ),	/* 0 */
/* 64 */	NdrFcShort( 0x0 ),	/* 0 */
/* 66 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 68 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 70 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 72 */	NdrFcShort( 0x4 ),	/* 4 */
/* 74 */	NdrFcShort( 0x4 ),	/* 4 */
/* 76 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 78 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 80 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 82 */	NdrFcShort( 0x8 ),	/* 8 */
/* 84 */	NdrFcShort( 0x8 ),	/* 8 */
/* 86 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 88 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 90 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 92 */	NdrFcShort( 0xc ),	/* 12 */
/* 94 */	NdrFcShort( 0xc ),	/* 12 */
/* 96 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 98 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 100 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 102 */	NdrFcShort( 0x18 ),	/* 24 */
/* 104 */	NdrFcShort( 0x18 ),	/* 24 */
/* 106 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 108 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 110 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 112 */	NdrFcShort( 0x1c ),	/* 28 */
/* 114 */	NdrFcShort( 0x1c ),	/* 28 */
/* 116 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 118 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 120 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 122 */	NdrFcShort( 0x20 ),	/* 32 */
/* 124 */	NdrFcShort( 0x20 ),	/* 32 */
/* 126 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 128 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 130 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 132 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 134 */	0x8,		/* FC_LONG */
			0xa,		/* FC_FLOAT */
/* 136 */	0xa,		/* FC_FLOAT */
			0x8,		/* FC_LONG */
/* 138 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 140 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 142 */	NdrFcShort( 0xffa0 ),	/* Offset= -96 (46) */
/* 144 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 146 */	NdrFcShort( 0xff9c ),	/* Offset= -100 (46) */
/* 148 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 150 */	NdrFcShort( 0xff98 ),	/* Offset= -104 (46) */
/* 152 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 154 */	NdrFcShort( 0xff94 ),	/* Offset= -108 (46) */
/* 156 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 158 */	
			0x11, 0x8,	/* FC_RP [simple_pointer] */
/* 160 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 162 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 164 */	NdrFcShort( 0x2 ),	/* Offset= 2 (166) */
/* 166 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 168 */	0x28,		/* Corr desc:  parameter, FC_LONG */
			0x0,		/*  */
/* 170 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 172 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 174 */	NdrFcShort( 0x2 ),	/* Offset= 2 (176) */
/* 176 */	
			0x13, 0x0,	/* FC_OP */
/* 178 */	NdrFcShort( 0x60 ),	/* Offset= 96 (274) */
/* 180 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 182 */	NdrFcShort( 0x30 ),	/* 48 */
/* 184 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 186 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 188 */	NdrFcShort( 0x0 ),	/* 0 */
/* 190 */	NdrFcShort( 0x0 ),	/* 0 */
/* 192 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 194 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 196 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 198 */	NdrFcShort( 0x4 ),	/* 4 */
/* 200 */	NdrFcShort( 0x4 ),	/* 4 */
/* 202 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 204 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 206 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 208 */	NdrFcShort( 0x8 ),	/* 8 */
/* 210 */	NdrFcShort( 0x8 ),	/* 8 */
/* 212 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 214 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 216 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 218 */	NdrFcShort( 0xc ),	/* 12 */
/* 220 */	NdrFcShort( 0xc ),	/* 12 */
/* 222 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 224 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 226 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 228 */	NdrFcShort( 0x14 ),	/* 20 */
/* 230 */	NdrFcShort( 0x14 ),	/* 20 */
/* 232 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 234 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 236 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 238 */	NdrFcShort( 0x18 ),	/* 24 */
/* 240 */	NdrFcShort( 0x18 ),	/* 24 */
/* 242 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 244 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 246 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 248 */	NdrFcShort( 0x1c ),	/* 28 */
/* 250 */	NdrFcShort( 0x1c ),	/* 28 */
/* 252 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 254 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 256 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 258 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 260 */	0x8,		/* FC_LONG */
			0xa,		/* FC_FLOAT */
/* 262 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 264 */	0x8,		/* FC_LONG */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 266 */	0x0,		/* 0 */
			NdrFcShort( 0xff23 ),	/* Offset= -221 (46) */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 270 */	0x0,		/* 0 */
			NdrFcShort( 0xff1f ),	/* Offset= -225 (46) */
			0x5b,		/* FC_END */
/* 274 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 276 */	NdrFcShort( 0x30 ),	/* 48 */
/* 278 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 280 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 282 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 284 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 286 */	NdrFcShort( 0x30 ),	/* 48 */
/* 288 */	NdrFcShort( 0x0 ),	/* 0 */
/* 290 */	NdrFcShort( 0x7 ),	/* 7 */
/* 292 */	NdrFcShort( 0x0 ),	/* 0 */
/* 294 */	NdrFcShort( 0x0 ),	/* 0 */
/* 296 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 298 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 300 */	NdrFcShort( 0x4 ),	/* 4 */
/* 302 */	NdrFcShort( 0x4 ),	/* 4 */
/* 304 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 306 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 308 */	NdrFcShort( 0x8 ),	/* 8 */
/* 310 */	NdrFcShort( 0x8 ),	/* 8 */
/* 312 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 314 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 316 */	NdrFcShort( 0xc ),	/* 12 */
/* 318 */	NdrFcShort( 0xc ),	/* 12 */
/* 320 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 322 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 324 */	NdrFcShort( 0x14 ),	/* 20 */
/* 326 */	NdrFcShort( 0x14 ),	/* 20 */
/* 328 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 330 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 332 */	NdrFcShort( 0x18 ),	/* 24 */
/* 334 */	NdrFcShort( 0x18 ),	/* 24 */
/* 336 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 338 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 340 */	NdrFcShort( 0x1c ),	/* 28 */
/* 342 */	NdrFcShort( 0x1c ),	/* 28 */
/* 344 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 346 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 348 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 350 */	0x0,		/* 0 */
			NdrFcShort( 0xff55 ),	/* Offset= -171 (180) */
			0x5b,		/* FC_END */
/* 354 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 356 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 358 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 360 */	NdrFcShort( 0x2 ),	/* Offset= 2 (362) */
/* 362 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 364 */	NdrFcLong( 0xa8080da2 ),	/* -1475867230 */
/* 368 */	NdrFcShort( 0xe23e ),	/* -7618 */
/* 370 */	NdrFcShort( 0x11d2 ),	/* 4562 */
/* 372 */	0xaf,		/* 175 */
			0xa7,		/* 167 */
/* 374 */	0x0,		/* 0 */
			0xc0,		/* 192 */
/* 376 */	0x4f,		/* 79 */
			0x53,		/* 83 */
/* 378 */	0x94,		/* 148 */
			0x21,		/* 33 */
/* 380 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 382 */	NdrFcShort( 0x2 ),	/* Offset= 2 (384) */
/* 384 */	
			0x13, 0x0,	/* FC_OP */
/* 386 */	NdrFcShort( 0x2 ),	/* Offset= 2 (388) */
/* 388 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 390 */	NdrFcShort( 0x4 ),	/* 4 */
/* 392 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 394 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 396 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 398 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 400 */	NdrFcShort( 0x2 ),	/* Offset= 2 (402) */
/* 402 */	
			0x13, 0x0,	/* FC_OP */
/* 404 */	NdrFcShort( 0x2 ),	/* Offset= 2 (406) */
/* 406 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 408 */	NdrFcShort( 0x4 ),	/* 4 */
/* 410 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 412 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 414 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 416 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 418 */	NdrFcShort( 0x4 ),	/* 4 */
/* 420 */	NdrFcShort( 0x0 ),	/* 0 */
/* 422 */	NdrFcShort( 0x1 ),	/* 1 */
/* 424 */	NdrFcShort( 0x0 ),	/* 0 */
/* 426 */	NdrFcShort( 0x0 ),	/* 0 */
/* 428 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 430 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 432 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 434 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 436 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 438 */	NdrFcShort( 0x2 ),	/* Offset= 2 (440) */
/* 440 */	
			0x13, 0x0,	/* FC_OP */
/* 442 */	NdrFcShort( 0x2 ),	/* Offset= 2 (444) */
/* 444 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 446 */	NdrFcShort( 0x4 ),	/* 4 */
/* 448 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 450 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 452 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 454 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 456 */	NdrFcShort( 0x2 ),	/* Offset= 2 (458) */
/* 458 */	
			0x13, 0x0,	/* FC_OP */
/* 460 */	NdrFcShort( 0x2 ),	/* Offset= 2 (462) */
/* 462 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 464 */	NdrFcShort( 0x4 ),	/* 4 */
/* 466 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 468 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 470 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 472 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 474 */	NdrFcShort( 0x4 ),	/* 4 */
/* 476 */	NdrFcShort( 0x0 ),	/* 0 */
/* 478 */	NdrFcShort( 0x1 ),	/* 1 */
/* 480 */	NdrFcShort( 0x0 ),	/* 0 */
/* 482 */	NdrFcShort( 0x0 ),	/* 0 */
/* 484 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 486 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 488 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 490 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */

			0x0
        }
    };


/* Object interface: IUnknown, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0xC0,0x00,0x00,0x00,0x00,0x00,0x00,0x46}} */


/* Object interface: CATID_OPCBatchServer10, ver. 0.0,
   GUID={0xA8080DA0,0xE23E,0x11D2,{0xAF,0xA7,0x00,0xC0,0x4F,0x53,0x94,0x21}} */

#pragma code_seg(".orpc")
static const unsigned short CATID_OPCBatchServer10_FormatStringOffsetTable[] =
    {
    0
    };

static const MIDL_STUBLESS_PROXY_INFO CATID_OPCBatchServer10_ProxyInfo =
    {
    &Object_StubDesc,
    opcbc__MIDL_ProcFormatString.Format,
    &CATID_OPCBatchServer10_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO CATID_OPCBatchServer10_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcbc__MIDL_ProcFormatString.Format,
    &CATID_OPCBatchServer10_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(3) _CATID_OPCBatchServer10ProxyVtbl = 
{
    0,
    &IID_CATID_OPCBatchServer10,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy
};

const CInterfaceStubVtbl _CATID_OPCBatchServer10StubVtbl =
{
    &IID_CATID_OPCBatchServer10,
    &CATID_OPCBatchServer10_ServerInfo,
    3,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: CATID_OPCBatchServer20, ver. 0.0,
   GUID={0x843DE67B,0xB0C9,0x11d4,{0xA0,0xB7,0x00,0x01,0x02,0xA9,0x80,0xB1}} */

#pragma code_seg(".orpc")
static const unsigned short CATID_OPCBatchServer20_FormatStringOffsetTable[] =
    {
    0
    };

static const MIDL_STUBLESS_PROXY_INFO CATID_OPCBatchServer20_ProxyInfo =
    {
    &Object_StubDesc,
    opcbc__MIDL_ProcFormatString.Format,
    &CATID_OPCBatchServer20_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO CATID_OPCBatchServer20_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcbc__MIDL_ProcFormatString.Format,
    &CATID_OPCBatchServer20_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(3) _CATID_OPCBatchServer20ProxyVtbl = 
{
    0,
    &IID_CATID_OPCBatchServer20,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy
};

const CInterfaceStubVtbl _CATID_OPCBatchServer20StubVtbl =
{
    &IID_CATID_OPCBatchServer20,
    &CATID_OPCBatchServer20_ServerInfo,
    3,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Standard interface: __MIDL_itf_opcbc_0000_0002, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: IOPCBatchServer, ver. 0.0,
   GUID={0x8BB4ED50,0xB314,0x11d3,{0xB3,0xEA,0x00,0xC0,0x4F,0x8E,0xCE,0xAA}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCBatchServer_FormatStringOffsetTable[] =
    {
    0,
    28
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCBatchServer_ProxyInfo =
    {
    &Object_StubDesc,
    opcbc__MIDL_ProcFormatString.Format,
    &IOPCBatchServer_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCBatchServer_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcbc__MIDL_ProcFormatString.Format,
    &IOPCBatchServer_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(5) _IOPCBatchServerProxyVtbl = 
{
    &IOPCBatchServer_ProxyInfo,
    &IID_IOPCBatchServer,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCBatchServer::GetDelimiter */ ,
    (void *) (INT_PTR) -1 /* IOPCBatchServer::CreateEnumerator */
};

const CInterfaceStubVtbl _IOPCBatchServerStubVtbl =
{
    &IID_IOPCBatchServer,
    &IOPCBatchServer_ServerInfo,
    5,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCBatchServer2, ver. 0.0,
   GUID={0x895A78CF,0xB0C5,0x11d4,{0xA0,0xB7,0x00,0x01,0x02,0xA9,0x80,0xB1}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCBatchServer2_FormatStringOffsetTable[] =
    {
    62
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCBatchServer2_ProxyInfo =
    {
    &Object_StubDesc,
    opcbc__MIDL_ProcFormatString.Format,
    &IOPCBatchServer2_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCBatchServer2_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcbc__MIDL_ProcFormatString.Format,
    &IOPCBatchServer2_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(4) _IOPCBatchServer2ProxyVtbl = 
{
    &IOPCBatchServer2_ProxyInfo,
    &IID_IOPCBatchServer2,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCBatchServer2::CreateFilteredEnumerator */
};

const CInterfaceStubVtbl _IOPCBatchServer2StubVtbl =
{
    &IID_IOPCBatchServer2,
    &IOPCBatchServer2_ServerInfo,
    4,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IEnumOPCBatchSummary, ver. 0.0,
   GUID={0xa8080da2,0xe23e,0x11d2,{0xaf,0xa7,0x00,0xc0,0x4f,0x53,0x94,0x21}} */

#pragma code_seg(".orpc")
static const unsigned short IEnumOPCBatchSummary_FormatStringOffsetTable[] =
    {
    108,
    148,
    176,
    198,
    226
    };

static const MIDL_STUBLESS_PROXY_INFO IEnumOPCBatchSummary_ProxyInfo =
    {
    &Object_StubDesc,
    opcbc__MIDL_ProcFormatString.Format,
    &IEnumOPCBatchSummary_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IEnumOPCBatchSummary_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcbc__MIDL_ProcFormatString.Format,
    &IEnumOPCBatchSummary_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(8) _IEnumOPCBatchSummaryProxyVtbl = 
{
    &IEnumOPCBatchSummary_ProxyInfo,
    &IID_IEnumOPCBatchSummary,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IEnumOPCBatchSummary::Next */ ,
    (void *) (INT_PTR) -1 /* IEnumOPCBatchSummary::Skip */ ,
    (void *) (INT_PTR) -1 /* IEnumOPCBatchSummary::Reset */ ,
    (void *) (INT_PTR) -1 /* IEnumOPCBatchSummary::Clone */ ,
    (void *) (INT_PTR) -1 /* IEnumOPCBatchSummary::Count */
};

const CInterfaceStubVtbl _IEnumOPCBatchSummaryStubVtbl =
{
    &IID_IEnumOPCBatchSummary,
    &IEnumOPCBatchSummary_ServerInfo,
    8,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCEnumerationSets, ver. 0.0,
   GUID={0xa8080da3,0xe23e,0x11d2,{0xaf,0xa7,0x00,0xc0,0x4f,0x53,0x94,0x21}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCEnumerationSets_FormatStringOffsetTable[] =
    {
    254,
    294,
    334
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCEnumerationSets_ProxyInfo =
    {
    &Object_StubDesc,
    opcbc__MIDL_ProcFormatString.Format,
    &IOPCEnumerationSets_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCEnumerationSets_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcbc__MIDL_ProcFormatString.Format,
    &IOPCEnumerationSets_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(6) _IOPCEnumerationSetsProxyVtbl = 
{
    &IOPCEnumerationSets_ProxyInfo,
    &IID_IOPCEnumerationSets,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCEnumerationSets::QueryEnumerationSets */ ,
    (void *) (INT_PTR) -1 /* IOPCEnumerationSets::QueryEnumeration */ ,
    (void *) (INT_PTR) -1 /* IOPCEnumerationSets::QueryEnumerationList */
};

const CInterfaceStubVtbl _IOPCEnumerationSetsStubVtbl =
{
    &IID_IOPCEnumerationSets,
    &IOPCEnumerationSets_ServerInfo,
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
    opcbc__MIDL_TypeFormatString.Format,
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

const CInterfaceProxyVtbl * const _opcbc_ProxyVtblList[] = 
{
    ( CInterfaceProxyVtbl *) &_IOPCBatchServerProxyVtbl,
    ( CInterfaceProxyVtbl *) &_CATID_OPCBatchServer20ProxyVtbl,
    ( CInterfaceProxyVtbl *) &_CATID_OPCBatchServer10ProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IEnumOPCBatchSummaryProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCEnumerationSetsProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCBatchServer2ProxyVtbl,
    0
};

const CInterfaceStubVtbl * const _opcbc_StubVtblList[] = 
{
    ( CInterfaceStubVtbl *) &_IOPCBatchServerStubVtbl,
    ( CInterfaceStubVtbl *) &_CATID_OPCBatchServer20StubVtbl,
    ( CInterfaceStubVtbl *) &_CATID_OPCBatchServer10StubVtbl,
    ( CInterfaceStubVtbl *) &_IEnumOPCBatchSummaryStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCEnumerationSetsStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCBatchServer2StubVtbl,
    0
};

PCInterfaceName const _opcbc_InterfaceNamesList[] = 
{
    "IOPCBatchServer",
    "CATID_OPCBatchServer20",
    "CATID_OPCBatchServer10",
    "IEnumOPCBatchSummary",
    "IOPCEnumerationSets",
    "IOPCBatchServer2",
    0
};


#define _opcbc_CHECK_IID(n)	IID_GENERIC_CHECK_IID( _opcbc, pIID, n)

int __stdcall _opcbc_IID_Lookup( const IID * pIID, int * pIndex )
{
    IID_BS_LOOKUP_SETUP

    IID_BS_LOOKUP_INITIAL_TEST( _opcbc, 6, 4 )
    IID_BS_LOOKUP_NEXT_TEST( _opcbc, 2 )
    IID_BS_LOOKUP_NEXT_TEST( _opcbc, 1 )
    IID_BS_LOOKUP_RETURN_RESULT( _opcbc, 6, *pIndex )
    
}

const ExtendedProxyFileInfo opcbc_ProxyFileInfo = 
{
    (PCInterfaceProxyVtblList *) & _opcbc_ProxyVtblList,
    (PCInterfaceStubVtblList *) & _opcbc_StubVtblList,
    (const PCInterfaceName * ) & _opcbc_InterfaceNamesList,
    0, /* no delegation */
    & _opcbc_IID_Lookup, 
    6,
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

