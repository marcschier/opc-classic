

/* this ALWAYS GENERATED file contains the proxy stub code */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Thu Jun 25 06:09:03 2015
 */
/* Compiler settings for OpcDx.idl:
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


#include "OpcDx.h"

#define TYPE_FORMAT_STRING_SIZE   1807                              
#define PROC_FORMAT_STRING_SIZE   565                               
#define EXPR_FORMAT_STRING_SIZE   1                                 
#define TRANSMIT_AS_TABLE_SIZE    0            
#define WIRE_MARSHAL_TABLE_SIZE   1            

typedef struct _OpcDx_MIDL_TYPE_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ TYPE_FORMAT_STRING_SIZE ];
    } OpcDx_MIDL_TYPE_FORMAT_STRING;

typedef struct _OpcDx_MIDL_PROC_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ PROC_FORMAT_STRING_SIZE ];
    } OpcDx_MIDL_PROC_FORMAT_STRING;

typedef struct _OpcDx_MIDL_EXPR_FORMAT_STRING
    {
    long          Pad;
    unsigned char  Format[ EXPR_FORMAT_STRING_SIZE ];
    } OpcDx_MIDL_EXPR_FORMAT_STRING;


static const RPC_SYNTAX_IDENTIFIER  _RpcTransferSyntax = 
{{0x8A885D04,0x1CEB,0x11C9,{0x9F,0xE8,0x08,0x00,0x2B,0x10,0x48,0x60}},{2,0}};


extern const OpcDx_MIDL_TYPE_FORMAT_STRING OpcDx__MIDL_TypeFormatString;
extern const OpcDx_MIDL_PROC_FORMAT_STRING OpcDx__MIDL_ProcFormatString;
extern const OpcDx_MIDL_EXPR_FORMAT_STRING OpcDx__MIDL_ExprFormatString;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO CATID_OPCDXServer10_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO CATID_OPCDXServer10_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCConfiguration_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCConfiguration_ProxyInfo;


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


static const OpcDx_MIDL_PROC_FORMAT_STRING OpcDx__MIDL_ProcFormatString =
    {
        0,
        {

	/* Procedure GetServers */

			0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/*  2 */	NdrFcLong( 0x0 ),	/* 0 */
/*  6 */	NdrFcShort( 0x3 ),	/* 3 */
/*  8 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 10 */	NdrFcShort( 0x0 ),	/* 0 */
/* 12 */	NdrFcShort( 0x24 ),	/* 36 */
/* 14 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x3,		/* 3 */

	/* Parameter pdwCount */

/* 16 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 18 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 20 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppServers */

/* 22 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 24 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 26 */	NdrFcShort( 0x6 ),	/* Type Offset=6 */

	/* Return value */

/* 28 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 30 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 32 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure AddServers */

/* 34 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 36 */	NdrFcLong( 0x0 ),	/* 0 */
/* 40 */	NdrFcShort( 0x4 ),	/* 4 */
/* 42 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 44 */	NdrFcShort( 0x8 ),	/* 8 */
/* 46 */	NdrFcShort( 0x8 ),	/* 8 */
/* 48 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwCount */

/* 50 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 52 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 54 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pServers */

/* 56 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 58 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 60 */	NdrFcShort( 0xba ),	/* Type Offset=186 */

	/* Parameter pResponse */

/* 62 */	NdrFcShort( 0x4113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=16 */
/* 64 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 66 */	NdrFcShort( 0x168 ),	/* Type Offset=360 */

	/* Return value */

/* 68 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 70 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 72 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ModifyServers */

/* 74 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 76 */	NdrFcLong( 0x0 ),	/* 0 */
/* 80 */	NdrFcShort( 0x5 ),	/* 5 */
/* 82 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 84 */	NdrFcShort( 0x8 ),	/* 8 */
/* 86 */	NdrFcShort( 0x8 ),	/* 8 */
/* 88 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwCount */

/* 90 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 92 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 94 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pServers */

/* 96 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 98 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 100 */	NdrFcShort( 0xba ),	/* Type Offset=186 */

	/* Parameter pResponse */

/* 102 */	NdrFcShort( 0x4113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=16 */
/* 104 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 106 */	NdrFcShort( 0x168 ),	/* Type Offset=360 */

	/* Return value */

/* 108 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 110 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 112 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure DeleteServers */

/* 114 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 116 */	NdrFcLong( 0x0 ),	/* 0 */
/* 120 */	NdrFcShort( 0x6 ),	/* 6 */
/* 122 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 124 */	NdrFcShort( 0x8 ),	/* 8 */
/* 126 */	NdrFcShort( 0x8 ),	/* 8 */
/* 128 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwCount */

/* 130 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 132 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 134 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pServers */

/* 136 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 138 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 140 */	NdrFcShort( 0x1b6 ),	/* Type Offset=438 */

	/* Parameter pResponse */

/* 142 */	NdrFcShort( 0x4113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=16 */
/* 144 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 146 */	NdrFcShort( 0x168 ),	/* Type Offset=360 */

	/* Return value */

/* 148 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 150 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 152 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CopyDefaultServerAttributes */

/* 154 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 156 */	NdrFcLong( 0x0 ),	/* 0 */
/* 160 */	NdrFcShort( 0x7 ),	/* 7 */
/* 162 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 164 */	NdrFcShort( 0x10 ),	/* 16 */
/* 166 */	NdrFcShort( 0x8 ),	/* 8 */
/* 168 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter bConfigToStatus */

/* 170 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 172 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 174 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 176 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 178 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 180 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pServers */

/* 182 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 184 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 186 */	NdrFcShort( 0x1ea ),	/* Type Offset=490 */

	/* Parameter pResponse */

/* 188 */	NdrFcShort( 0x4113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=16 */
/* 190 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 192 */	NdrFcShort( 0x168 ),	/* Type Offset=360 */

	/* Return value */

/* 194 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 196 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 198 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryDXConnections */

/* 200 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 202 */	NdrFcLong( 0x0 ),	/* 0 */
/* 206 */	NdrFcShort( 0x8 ),	/* 8 */
/* 208 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 210 */	NdrFcShort( 0x10 ),	/* 16 */
/* 212 */	NdrFcShort( 0x24 ),	/* 36 */
/* 214 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x8,		/* 8 */

	/* Parameter szBrowsePath */

/* 216 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 218 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 220 */	NdrFcShort( 0x21c ),	/* Type Offset=540 */

	/* Parameter dwNoOfMasks */

/* 222 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 224 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 226 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pDXConnectionMasks */

/* 228 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 230 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 232 */	NdrFcShort( 0x686 ),	/* Type Offset=1670 */

	/* Parameter bRecursive */

/* 234 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 236 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 238 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 240 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 242 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 244 */	NdrFcShort( 0x698 ),	/* Type Offset=1688 */

	/* Parameter pdwCount */

/* 246 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 248 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 250 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppConnections */

/* 252 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 254 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 256 */	NdrFcShort( 0x6aa ),	/* Type Offset=1706 */

	/* Return value */

/* 258 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 260 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 262 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure AddDXConnections */

/* 264 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 266 */	NdrFcLong( 0x0 ),	/* 0 */
/* 270 */	NdrFcShort( 0x9 ),	/* 9 */
/* 272 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 274 */	NdrFcShort( 0x8 ),	/* 8 */
/* 276 */	NdrFcShort( 0x8 ),	/* 8 */
/* 278 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwCount */

/* 280 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 282 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 284 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pConnections */

/* 286 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 288 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 290 */	NdrFcShort( 0x6c8 ),	/* Type Offset=1736 */

	/* Parameter pResponse */

/* 292 */	NdrFcShort( 0x4113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=16 */
/* 294 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 296 */	NdrFcShort( 0x168 ),	/* Type Offset=360 */

	/* Return value */

/* 298 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 300 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 302 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure UpdateDXConnections */

/* 304 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 306 */	NdrFcLong( 0x0 ),	/* 0 */
/* 310 */	NdrFcShort( 0xa ),	/* 10 */
/* 312 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 314 */	NdrFcShort( 0x10 ),	/* 16 */
/* 316 */	NdrFcShort( 0x8 ),	/* 8 */
/* 318 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x8,		/* 8 */

	/* Parameter szBrowsePath */

/* 320 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 322 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 324 */	NdrFcShort( 0x21c ),	/* Type Offset=540 */

	/* Parameter dwNoOfMasks */

/* 326 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 328 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 330 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pDXConnectionMasks */

/* 332 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 334 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 336 */	NdrFcShort( 0x686 ),	/* Type Offset=1670 */

	/* Parameter bRecursive */

/* 338 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 340 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 342 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pDXConnectionDefinition */

/* 344 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 346 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 348 */	NdrFcShort( 0x62a ),	/* Type Offset=1578 */

	/* Parameter ppErrors */

/* 350 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 352 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 354 */	NdrFcShort( 0x698 ),	/* Type Offset=1688 */

	/* Parameter pResponse */

/* 356 */	NdrFcShort( 0x4113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=16 */
/* 358 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 360 */	NdrFcShort( 0x168 ),	/* Type Offset=360 */

	/* Return value */

/* 362 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 364 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 366 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ModifyDXConnections */

/* 368 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 370 */	NdrFcLong( 0x0 ),	/* 0 */
/* 374 */	NdrFcShort( 0xb ),	/* 11 */
/* 376 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 378 */	NdrFcShort( 0x8 ),	/* 8 */
/* 380 */	NdrFcShort( 0x8 ),	/* 8 */
/* 382 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwCount */

/* 384 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 386 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 388 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pDXConnectionDefinitions */

/* 390 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 392 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 394 */	NdrFcShort( 0x6c8 ),	/* Type Offset=1736 */

	/* Parameter pResponse */

/* 396 */	NdrFcShort( 0x4113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=16 */
/* 398 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 400 */	NdrFcShort( 0x168 ),	/* Type Offset=360 */

	/* Return value */

/* 402 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 404 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 406 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure DeleteDXConnections */

/* 408 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 410 */	NdrFcLong( 0x0 ),	/* 0 */
/* 414 */	NdrFcShort( 0xc ),	/* 12 */
/* 416 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 418 */	NdrFcShort( 0x10 ),	/* 16 */
/* 420 */	NdrFcShort( 0x8 ),	/* 8 */
/* 422 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter szBrowsePath */

/* 424 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 426 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 428 */	NdrFcShort( 0x21c ),	/* Type Offset=540 */

	/* Parameter dwNoOfMasks */

/* 430 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 432 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 434 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pDXConnectionMasks */

/* 436 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 438 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 440 */	NdrFcShort( 0x686 ),	/* Type Offset=1670 */

	/* Parameter bRecursive */

/* 442 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 444 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 446 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 448 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 450 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 452 */	NdrFcShort( 0x698 ),	/* Type Offset=1688 */

	/* Parameter pResponse */

/* 454 */	NdrFcShort( 0x4113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=16 */
/* 456 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 458 */	NdrFcShort( 0x168 ),	/* Type Offset=360 */

	/* Return value */

/* 460 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 462 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 464 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CopyDXConnectionDefaultAttributes */

/* 466 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 468 */	NdrFcLong( 0x0 ),	/* 0 */
/* 472 */	NdrFcShort( 0xd ),	/* 13 */
/* 474 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 476 */	NdrFcShort( 0x18 ),	/* 24 */
/* 478 */	NdrFcShort( 0x8 ),	/* 8 */
/* 480 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x8,		/* 8 */

	/* Parameter bConfigToStatus */

/* 482 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 484 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 486 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter szBrowsePath */

/* 488 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 490 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 492 */	NdrFcShort( 0x21c ),	/* Type Offset=540 */

	/* Parameter dwNoOfMasks */

/* 494 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 496 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 498 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pDXConnectionMasks */

/* 500 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 502 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 504 */	NdrFcShort( 0x6e2 ),	/* Type Offset=1762 */

	/* Parameter bRecursive */

/* 506 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 508 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 510 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 512 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 514 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 516 */	NdrFcShort( 0x6f4 ),	/* Type Offset=1780 */

	/* Parameter pResponse */

/* 518 */	NdrFcShort( 0x4113 ),	/* Flags:  must size, must free, out, simple ref, srv alloc size=16 */
/* 520 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 522 */	NdrFcShort( 0x168 ),	/* Type Offset=360 */

	/* Return value */

/* 524 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 526 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 528 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ResetConfiguration */

/* 530 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 532 */	NdrFcLong( 0x0 ),	/* 0 */
/* 536 */	NdrFcShort( 0xe ),	/* 14 */
/* 538 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 540 */	NdrFcShort( 0x0 ),	/* 0 */
/* 542 */	NdrFcShort( 0x8 ),	/* 8 */
/* 544 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter szConfigurationVersion */

/* 546 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 548 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 550 */	NdrFcShort( 0x21c ),	/* Type Offset=540 */

	/* Parameter pszConfigurationVersion */

/* 552 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 554 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 556 */	NdrFcShort( 0x706 ),	/* Type Offset=1798 */

	/* Return value */

/* 558 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 560 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 562 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

			0x0
        }
    };

static const OpcDx_MIDL_TYPE_FORMAT_STRING OpcDx__MIDL_TypeFormatString =
    {
        0,
        {
			NdrFcShort( 0x0 ),	/* 0 */
/*  2 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/*  4 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/*  6 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/*  8 */	NdrFcShort( 0x2 ),	/* Offset= 2 (10) */
/* 10 */	
			0x13, 0x0,	/* FC_OP */
/* 12 */	NdrFcShort( 0x5a ),	/* Offset= 90 (102) */
/* 14 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 16 */	NdrFcShort( 0x28 ),	/* 40 */
/* 18 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 20 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 22 */	NdrFcShort( 0x4 ),	/* 4 */
/* 24 */	NdrFcShort( 0x4 ),	/* 4 */
/* 26 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 28 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 30 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 32 */	NdrFcShort( 0x8 ),	/* 8 */
/* 34 */	NdrFcShort( 0x8 ),	/* 8 */
/* 36 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 38 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 40 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 42 */	NdrFcShort( 0xc ),	/* 12 */
/* 44 */	NdrFcShort( 0xc ),	/* 12 */
/* 46 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 48 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 50 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 52 */	NdrFcShort( 0x10 ),	/* 16 */
/* 54 */	NdrFcShort( 0x10 ),	/* 16 */
/* 56 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 58 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 60 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 62 */	NdrFcShort( 0x14 ),	/* 20 */
/* 64 */	NdrFcShort( 0x14 ),	/* 20 */
/* 66 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 68 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 70 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 72 */	NdrFcShort( 0x18 ),	/* 24 */
/* 74 */	NdrFcShort( 0x18 ),	/* 24 */
/* 76 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 78 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 80 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 82 */	NdrFcShort( 0x1c ),	/* 28 */
/* 84 */	NdrFcShort( 0x1c ),	/* 28 */
/* 86 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 88 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 90 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 92 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 94 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 96 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 98 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 100 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 102 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 104 */	NdrFcShort( 0x28 ),	/* 40 */
/* 106 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 108 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 110 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 112 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 114 */	NdrFcShort( 0x28 ),	/* 40 */
/* 116 */	NdrFcShort( 0x0 ),	/* 0 */
/* 118 */	NdrFcShort( 0x7 ),	/* 7 */
/* 120 */	NdrFcShort( 0x4 ),	/* 4 */
/* 122 */	NdrFcShort( 0x4 ),	/* 4 */
/* 124 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 126 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 128 */	NdrFcShort( 0x8 ),	/* 8 */
/* 130 */	NdrFcShort( 0x8 ),	/* 8 */
/* 132 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 134 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 136 */	NdrFcShort( 0xc ),	/* 12 */
/* 138 */	NdrFcShort( 0xc ),	/* 12 */
/* 140 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 142 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 144 */	NdrFcShort( 0x10 ),	/* 16 */
/* 146 */	NdrFcShort( 0x10 ),	/* 16 */
/* 148 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 150 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 152 */	NdrFcShort( 0x14 ),	/* 20 */
/* 154 */	NdrFcShort( 0x14 ),	/* 20 */
/* 156 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 158 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 160 */	NdrFcShort( 0x18 ),	/* 24 */
/* 162 */	NdrFcShort( 0x18 ),	/* 24 */
/* 164 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 166 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 168 */	NdrFcShort( 0x1c ),	/* 28 */
/* 170 */	NdrFcShort( 0x1c ),	/* 28 */
/* 172 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 174 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 176 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 178 */	0x0,		/* 0 */
			NdrFcShort( 0xff5b ),	/* Offset= -165 (14) */
			0x5b,		/* FC_END */
/* 182 */	
			0x11, 0x0,	/* FC_RP */
/* 184 */	NdrFcShort( 0x2 ),	/* Offset= 2 (186) */
/* 186 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 188 */	NdrFcShort( 0x28 ),	/* 40 */
/* 190 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 192 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 194 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 196 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 198 */	NdrFcShort( 0x28 ),	/* 40 */
/* 200 */	NdrFcShort( 0x0 ),	/* 0 */
/* 202 */	NdrFcShort( 0x7 ),	/* 7 */
/* 204 */	NdrFcShort( 0x4 ),	/* 4 */
/* 206 */	NdrFcShort( 0x4 ),	/* 4 */
/* 208 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 210 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 212 */	NdrFcShort( 0x8 ),	/* 8 */
/* 214 */	NdrFcShort( 0x8 ),	/* 8 */
/* 216 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 218 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 220 */	NdrFcShort( 0xc ),	/* 12 */
/* 222 */	NdrFcShort( 0xc ),	/* 12 */
/* 224 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 226 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 228 */	NdrFcShort( 0x10 ),	/* 16 */
/* 230 */	NdrFcShort( 0x10 ),	/* 16 */
/* 232 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 234 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 236 */	NdrFcShort( 0x14 ),	/* 20 */
/* 238 */	NdrFcShort( 0x14 ),	/* 20 */
/* 240 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 242 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 244 */	NdrFcShort( 0x18 ),	/* 24 */
/* 246 */	NdrFcShort( 0x18 ),	/* 24 */
/* 248 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 250 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 252 */	NdrFcShort( 0x1c ),	/* 28 */
/* 254 */	NdrFcShort( 0x1c ),	/* 28 */
/* 256 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 258 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 260 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 262 */	0x0,		/* 0 */
			NdrFcShort( 0xff07 ),	/* Offset= -249 (14) */
			0x5b,		/* FC_END */
/* 266 */	
			0x11, 0x4,	/* FC_RP [alloced_on_stack] */
/* 268 */	NdrFcShort( 0x5c ),	/* Offset= 92 (360) */
/* 270 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 272 */	NdrFcShort( 0x10 ),	/* 16 */
/* 274 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 276 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 278 */	NdrFcShort( 0x0 ),	/* 0 */
/* 280 */	NdrFcShort( 0x0 ),	/* 0 */
/* 282 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 284 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 286 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 288 */	NdrFcShort( 0x4 ),	/* 4 */
/* 290 */	NdrFcShort( 0x4 ),	/* 4 */
/* 292 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 294 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 296 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 298 */	NdrFcShort( 0x8 ),	/* 8 */
/* 300 */	NdrFcShort( 0x8 ),	/* 8 */
/* 302 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 304 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 306 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 308 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 310 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 312 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 314 */	NdrFcShort( 0x10 ),	/* 16 */
/* 316 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 318 */	NdrFcShort( 0x4 ),	/* 4 */
/* 320 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 322 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 324 */	NdrFcShort( 0x10 ),	/* 16 */
/* 326 */	NdrFcShort( 0x0 ),	/* 0 */
/* 328 */	NdrFcShort( 0x3 ),	/* 3 */
/* 330 */	NdrFcShort( 0x0 ),	/* 0 */
/* 332 */	NdrFcShort( 0x0 ),	/* 0 */
/* 334 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 336 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 338 */	NdrFcShort( 0x4 ),	/* 4 */
/* 340 */	NdrFcShort( 0x4 ),	/* 4 */
/* 342 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 344 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 346 */	NdrFcShort( 0x8 ),	/* 8 */
/* 348 */	NdrFcShort( 0x8 ),	/* 8 */
/* 350 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 352 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 354 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 356 */	0x0,		/* 0 */
			NdrFcShort( 0xffa9 ),	/* Offset= -87 (270) */
			0x5b,		/* FC_END */
/* 360 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 362 */	NdrFcShort( 0x10 ),	/* 16 */
/* 364 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 366 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 368 */	NdrFcShort( 0x0 ),	/* 0 */
/* 370 */	NdrFcShort( 0x0 ),	/* 0 */
/* 372 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 374 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 376 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 378 */	NdrFcShort( 0x8 ),	/* 8 */
/* 380 */	NdrFcShort( 0x8 ),	/* 8 */
/* 382 */	0x13, 0x0,	/* FC_OP */
/* 384 */	NdrFcShort( 0xffb8 ),	/* Offset= -72 (312) */
/* 386 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 388 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 390 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 392 */	
			0x11, 0x0,	/* FC_RP */
/* 394 */	NdrFcShort( 0x2c ),	/* Offset= 44 (438) */
/* 396 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 398 */	NdrFcShort( 0x10 ),	/* 16 */
/* 400 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 402 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 404 */	NdrFcShort( 0x0 ),	/* 0 */
/* 406 */	NdrFcShort( 0x0 ),	/* 0 */
/* 408 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 410 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 412 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 414 */	NdrFcShort( 0x4 ),	/* 4 */
/* 416 */	NdrFcShort( 0x4 ),	/* 4 */
/* 418 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 420 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 422 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 424 */	NdrFcShort( 0x8 ),	/* 8 */
/* 426 */	NdrFcShort( 0x8 ),	/* 8 */
/* 428 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 430 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 432 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 434 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 436 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 438 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 440 */	NdrFcShort( 0x10 ),	/* 16 */
/* 442 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 444 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 446 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 448 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 450 */	NdrFcShort( 0x10 ),	/* 16 */
/* 452 */	NdrFcShort( 0x0 ),	/* 0 */
/* 454 */	NdrFcShort( 0x3 ),	/* 3 */
/* 456 */	NdrFcShort( 0x0 ),	/* 0 */
/* 458 */	NdrFcShort( 0x0 ),	/* 0 */
/* 460 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 462 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 464 */	NdrFcShort( 0x4 ),	/* 4 */
/* 466 */	NdrFcShort( 0x4 ),	/* 4 */
/* 468 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 470 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 472 */	NdrFcShort( 0x8 ),	/* 8 */
/* 474 */	NdrFcShort( 0x8 ),	/* 8 */
/* 476 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 478 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 480 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 482 */	0x0,		/* 0 */
			NdrFcShort( 0xffa9 ),	/* Offset= -87 (396) */
			0x5b,		/* FC_END */
/* 486 */	
			0x11, 0x0,	/* FC_RP */
/* 488 */	NdrFcShort( 0x2 ),	/* Offset= 2 (490) */
/* 490 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 492 */	NdrFcShort( 0x10 ),	/* 16 */
/* 494 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 496 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 498 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 500 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 502 */	NdrFcShort( 0x10 ),	/* 16 */
/* 504 */	NdrFcShort( 0x0 ),	/* 0 */
/* 506 */	NdrFcShort( 0x3 ),	/* 3 */
/* 508 */	NdrFcShort( 0x0 ),	/* 0 */
/* 510 */	NdrFcShort( 0x0 ),	/* 0 */
/* 512 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 514 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 516 */	NdrFcShort( 0x4 ),	/* 4 */
/* 518 */	NdrFcShort( 0x4 ),	/* 4 */
/* 520 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 522 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 524 */	NdrFcShort( 0x8 ),	/* 8 */
/* 526 */	NdrFcShort( 0x8 ),	/* 8 */
/* 528 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 530 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 532 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 534 */	0x0,		/* 0 */
			NdrFcShort( 0xff75 ),	/* Offset= -139 (396) */
			0x5b,		/* FC_END */
/* 538 */	
			0x11, 0x8,	/* FC_RP [simple_pointer] */
/* 540 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 542 */	
			0x11, 0x0,	/* FC_RP */
/* 544 */	NdrFcShort( 0x466 ),	/* Offset= 1126 (1670) */
/* 546 */	
			0x12, 0x0,	/* FC_UP */
/* 548 */	NdrFcShort( 0x3ca ),	/* Offset= 970 (1518) */
/* 550 */	
			0x2b,		/* FC_NON_ENCAPSULATED_UNION */
			0x9,		/* FC_ULONG */
/* 552 */	0x7,		/* Corr desc: FC_USHORT */
			0x0,		/*  */
/* 554 */	NdrFcShort( 0xfff8 ),	/* -8 */
/* 556 */	NdrFcShort( 0x2 ),	/* Offset= 2 (558) */
/* 558 */	NdrFcShort( 0x10 ),	/* 16 */
/* 560 */	NdrFcShort( 0x2f ),	/* 47 */
/* 562 */	NdrFcLong( 0x14 ),	/* 20 */
/* 566 */	NdrFcShort( 0x800b ),	/* Simple arm type: FC_HYPER */
/* 568 */	NdrFcLong( 0x3 ),	/* 3 */
/* 572 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 574 */	NdrFcLong( 0x11 ),	/* 17 */
/* 578 */	NdrFcShort( 0x8001 ),	/* Simple arm type: FC_BYTE */
/* 580 */	NdrFcLong( 0x2 ),	/* 2 */
/* 584 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 586 */	NdrFcLong( 0x4 ),	/* 4 */
/* 590 */	NdrFcShort( 0x800a ),	/* Simple arm type: FC_FLOAT */
/* 592 */	NdrFcLong( 0x5 ),	/* 5 */
/* 596 */	NdrFcShort( 0x800c ),	/* Simple arm type: FC_DOUBLE */
/* 598 */	NdrFcLong( 0xb ),	/* 11 */
/* 602 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 604 */	NdrFcLong( 0xa ),	/* 10 */
/* 608 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 610 */	NdrFcLong( 0x6 ),	/* 6 */
/* 614 */	NdrFcShort( 0xe8 ),	/* Offset= 232 (846) */
/* 616 */	NdrFcLong( 0x7 ),	/* 7 */
/* 620 */	NdrFcShort( 0x800c ),	/* Simple arm type: FC_DOUBLE */
/* 622 */	NdrFcLong( 0x8 ),	/* 8 */
/* 626 */	NdrFcShort( 0xe2 ),	/* Offset= 226 (852) */
/* 628 */	NdrFcLong( 0xd ),	/* 13 */
/* 632 */	NdrFcShort( 0xf4 ),	/* Offset= 244 (876) */
/* 634 */	NdrFcLong( 0x9 ),	/* 9 */
/* 638 */	NdrFcShort( 0x100 ),	/* Offset= 256 (894) */
/* 640 */	NdrFcLong( 0x2000 ),	/* 8192 */
/* 644 */	NdrFcShort( 0x10c ),	/* Offset= 268 (912) */
/* 646 */	NdrFcLong( 0x24 ),	/* 36 */
/* 650 */	NdrFcShort( 0x31a ),	/* Offset= 794 (1444) */
/* 652 */	NdrFcLong( 0x4024 ),	/* 16420 */
/* 656 */	NdrFcShort( 0x314 ),	/* Offset= 788 (1444) */
/* 658 */	NdrFcLong( 0x4011 ),	/* 16401 */
/* 662 */	NdrFcShort( 0x312 ),	/* Offset= 786 (1448) */
/* 664 */	NdrFcLong( 0x4002 ),	/* 16386 */
/* 668 */	NdrFcShort( 0x310 ),	/* Offset= 784 (1452) */
/* 670 */	NdrFcLong( 0x4003 ),	/* 16387 */
/* 674 */	NdrFcShort( 0x30e ),	/* Offset= 782 (1456) */
/* 676 */	NdrFcLong( 0x4014 ),	/* 16404 */
/* 680 */	NdrFcShort( 0x30c ),	/* Offset= 780 (1460) */
/* 682 */	NdrFcLong( 0x4004 ),	/* 16388 */
/* 686 */	NdrFcShort( 0x30a ),	/* Offset= 778 (1464) */
/* 688 */	NdrFcLong( 0x4005 ),	/* 16389 */
/* 692 */	NdrFcShort( 0x308 ),	/* Offset= 776 (1468) */
/* 694 */	NdrFcLong( 0x400b ),	/* 16395 */
/* 698 */	NdrFcShort( 0x2f2 ),	/* Offset= 754 (1452) */
/* 700 */	NdrFcLong( 0x400a ),	/* 16394 */
/* 704 */	NdrFcShort( 0x2f0 ),	/* Offset= 752 (1456) */
/* 706 */	NdrFcLong( 0x4006 ),	/* 16390 */
/* 710 */	NdrFcShort( 0x2fa ),	/* Offset= 762 (1472) */
/* 712 */	NdrFcLong( 0x4007 ),	/* 16391 */
/* 716 */	NdrFcShort( 0x2f0 ),	/* Offset= 752 (1468) */
/* 718 */	NdrFcLong( 0x4008 ),	/* 16392 */
/* 722 */	NdrFcShort( 0x2f2 ),	/* Offset= 754 (1476) */
/* 724 */	NdrFcLong( 0x400d ),	/* 16397 */
/* 728 */	NdrFcShort( 0x2f0 ),	/* Offset= 752 (1480) */
/* 730 */	NdrFcLong( 0x4009 ),	/* 16393 */
/* 734 */	NdrFcShort( 0x2ee ),	/* Offset= 750 (1484) */
/* 736 */	NdrFcLong( 0x6000 ),	/* 24576 */
/* 740 */	NdrFcShort( 0x2ec ),	/* Offset= 748 (1488) */
/* 742 */	NdrFcLong( 0x400c ),	/* 16396 */
/* 746 */	NdrFcShort( 0x2ea ),	/* Offset= 746 (1492) */
/* 748 */	NdrFcLong( 0x10 ),	/* 16 */
/* 752 */	NdrFcShort( 0x8002 ),	/* Simple arm type: FC_CHAR */
/* 754 */	NdrFcLong( 0x12 ),	/* 18 */
/* 758 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 760 */	NdrFcLong( 0x13 ),	/* 19 */
/* 764 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 766 */	NdrFcLong( 0x15 ),	/* 21 */
/* 770 */	NdrFcShort( 0x800b ),	/* Simple arm type: FC_HYPER */
/* 772 */	NdrFcLong( 0x16 ),	/* 22 */
/* 776 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 778 */	NdrFcLong( 0x17 ),	/* 23 */
/* 782 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 784 */	NdrFcLong( 0xe ),	/* 14 */
/* 788 */	NdrFcShort( 0x2c8 ),	/* Offset= 712 (1500) */
/* 790 */	NdrFcLong( 0x400e ),	/* 16398 */
/* 794 */	NdrFcShort( 0x2cc ),	/* Offset= 716 (1510) */
/* 796 */	NdrFcLong( 0x4010 ),	/* 16400 */
/* 800 */	NdrFcShort( 0x2ca ),	/* Offset= 714 (1514) */
/* 802 */	NdrFcLong( 0x4012 ),	/* 16402 */
/* 806 */	NdrFcShort( 0x286 ),	/* Offset= 646 (1452) */
/* 808 */	NdrFcLong( 0x4013 ),	/* 16403 */
/* 812 */	NdrFcShort( 0x284 ),	/* Offset= 644 (1456) */
/* 814 */	NdrFcLong( 0x4015 ),	/* 16405 */
/* 818 */	NdrFcShort( 0x282 ),	/* Offset= 642 (1460) */
/* 820 */	NdrFcLong( 0x4016 ),	/* 16406 */
/* 824 */	NdrFcShort( 0x278 ),	/* Offset= 632 (1456) */
/* 826 */	NdrFcLong( 0x4017 ),	/* 16407 */
/* 830 */	NdrFcShort( 0x272 ),	/* Offset= 626 (1456) */
/* 832 */	NdrFcLong( 0x0 ),	/* 0 */
/* 836 */	NdrFcShort( 0x0 ),	/* Offset= 0 (836) */
/* 838 */	NdrFcLong( 0x1 ),	/* 1 */
/* 842 */	NdrFcShort( 0x0 ),	/* Offset= 0 (842) */
/* 844 */	NdrFcShort( 0xffff ),	/* Offset= -1 (843) */
/* 846 */	
			0x15,		/* FC_STRUCT */
			0x7,		/* 7 */
/* 848 */	NdrFcShort( 0x8 ),	/* 8 */
/* 850 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 852 */	
			0x12, 0x0,	/* FC_UP */
/* 854 */	NdrFcShort( 0xc ),	/* Offset= 12 (866) */
/* 856 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 858 */	NdrFcShort( 0x2 ),	/* 2 */
/* 860 */	0x9,		/* Corr desc: FC_ULONG */
			0x0,		/*  */
/* 862 */	NdrFcShort( 0xfffc ),	/* -4 */
/* 864 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 866 */	
			0x17,		/* FC_CSTRUCT */
			0x3,		/* 3 */
/* 868 */	NdrFcShort( 0x8 ),	/* 8 */
/* 870 */	NdrFcShort( 0xfff2 ),	/* Offset= -14 (856) */
/* 872 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 874 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 876 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 878 */	NdrFcLong( 0x0 ),	/* 0 */
/* 882 */	NdrFcShort( 0x0 ),	/* 0 */
/* 884 */	NdrFcShort( 0x0 ),	/* 0 */
/* 886 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 888 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 890 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 892 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 894 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 896 */	NdrFcLong( 0x20400 ),	/* 132096 */
/* 900 */	NdrFcShort( 0x0 ),	/* 0 */
/* 902 */	NdrFcShort( 0x0 ),	/* 0 */
/* 904 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 906 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 908 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 910 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 912 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 914 */	NdrFcShort( 0x2 ),	/* Offset= 2 (916) */
/* 916 */	
			0x12, 0x0,	/* FC_UP */
/* 918 */	NdrFcShort( 0x1fc ),	/* Offset= 508 (1426) */
/* 920 */	
			0x2a,		/* FC_ENCAPSULATED_UNION */
			0x49,		/* 73 */
/* 922 */	NdrFcShort( 0x18 ),	/* 24 */
/* 924 */	NdrFcShort( 0xa ),	/* 10 */
/* 926 */	NdrFcLong( 0x8 ),	/* 8 */
/* 930 */	NdrFcShort( 0x58 ),	/* Offset= 88 (1018) */
/* 932 */	NdrFcLong( 0xd ),	/* 13 */
/* 936 */	NdrFcShort( 0x78 ),	/* Offset= 120 (1056) */
/* 938 */	NdrFcLong( 0x9 ),	/* 9 */
/* 942 */	NdrFcShort( 0x94 ),	/* Offset= 148 (1090) */
/* 944 */	NdrFcLong( 0xc ),	/* 12 */
/* 948 */	NdrFcShort( 0xbc ),	/* Offset= 188 (1136) */
/* 950 */	NdrFcLong( 0x24 ),	/* 36 */
/* 954 */	NdrFcShort( 0x114 ),	/* Offset= 276 (1230) */
/* 956 */	NdrFcLong( 0x800d ),	/* 32781 */
/* 960 */	NdrFcShort( 0x130 ),	/* Offset= 304 (1264) */
/* 962 */	NdrFcLong( 0x10 ),	/* 16 */
/* 966 */	NdrFcShort( 0x148 ),	/* Offset= 328 (1294) */
/* 968 */	NdrFcLong( 0x2 ),	/* 2 */
/* 972 */	NdrFcShort( 0x160 ),	/* Offset= 352 (1324) */
/* 974 */	NdrFcLong( 0x3 ),	/* 3 */
/* 978 */	NdrFcShort( 0x178 ),	/* Offset= 376 (1354) */
/* 980 */	NdrFcLong( 0x14 ),	/* 20 */
/* 984 */	NdrFcShort( 0x190 ),	/* Offset= 400 (1384) */
/* 986 */	NdrFcShort( 0xffff ),	/* Offset= -1 (985) */
/* 988 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 990 */	NdrFcShort( 0x4 ),	/* 4 */
/* 992 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 994 */	NdrFcShort( 0x0 ),	/* 0 */
/* 996 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 998 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1000 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1002 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1004 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1006 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1008 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1010 */	0x12, 0x0,	/* FC_UP */
/* 1012 */	NdrFcShort( 0xff6e ),	/* Offset= -146 (866) */
/* 1014 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1016 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1018 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1020 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1022 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1024 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1026 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1028 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1030 */	0x11, 0x0,	/* FC_RP */
/* 1032 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (988) */
/* 1034 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1036 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1038 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1040 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1042 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1044 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1046 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1050 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1052 */	NdrFcShort( 0xff50 ),	/* Offset= -176 (876) */
/* 1054 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1056 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1058 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1060 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1062 */	NdrFcShort( 0x6 ),	/* Offset= 6 (1068) */
/* 1064 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1066 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1068 */	
			0x11, 0x0,	/* FC_RP */
/* 1070 */	NdrFcShort( 0xffe0 ),	/* Offset= -32 (1038) */
/* 1072 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1074 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1076 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1078 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1080 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1084 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1086 */	NdrFcShort( 0xff40 ),	/* Offset= -192 (894) */
/* 1088 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1090 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1092 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1094 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1096 */	NdrFcShort( 0x6 ),	/* Offset= 6 (1102) */
/* 1098 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1100 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1102 */	
			0x11, 0x0,	/* FC_RP */
/* 1104 */	NdrFcShort( 0xffe0 ),	/* Offset= -32 (1072) */
/* 1106 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1108 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1110 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1112 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1114 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1116 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1118 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1120 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1122 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1124 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1126 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1128 */	0x12, 0x0,	/* FC_UP */
/* 1130 */	NdrFcShort( 0x184 ),	/* Offset= 388 (1518) */
/* 1132 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1134 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1136 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1138 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1140 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1142 */	NdrFcShort( 0x6 ),	/* Offset= 6 (1148) */
/* 1144 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1146 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1148 */	
			0x11, 0x0,	/* FC_RP */
/* 1150 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (1106) */
/* 1152 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 1154 */	NdrFcLong( 0x2f ),	/* 47 */
/* 1158 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1160 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1162 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 1164 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 1166 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 1168 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 1170 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 1172 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1174 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1176 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1178 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 1180 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1182 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1184 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1186 */	NdrFcShort( 0xa ),	/* Offset= 10 (1196) */
/* 1188 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1190 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1192 */	NdrFcShort( 0xffd8 ),	/* Offset= -40 (1152) */
/* 1194 */	0x36,		/* FC_POINTER */
			0x5b,		/* FC_END */
/* 1196 */	
			0x12, 0x0,	/* FC_UP */
/* 1198 */	NdrFcShort( 0xffe4 ),	/* Offset= -28 (1170) */
/* 1200 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1202 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1204 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1206 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1208 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1210 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1212 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1214 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1216 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1218 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1220 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1222 */	0x12, 0x0,	/* FC_UP */
/* 1224 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (1180) */
/* 1226 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1228 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1230 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1232 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1234 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1236 */	NdrFcShort( 0x6 ),	/* Offset= 6 (1242) */
/* 1238 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1240 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1242 */	
			0x11, 0x0,	/* FC_RP */
/* 1244 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (1200) */
/* 1246 */	
			0x1d,		/* FC_SMFARRAY */
			0x0,		/* 0 */
/* 1248 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1250 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 1252 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 1254 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1256 */	0x8,		/* FC_LONG */
			0x6,		/* FC_SHORT */
/* 1258 */	0x6,		/* FC_SHORT */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1260 */	0x0,		/* 0 */
			NdrFcShort( 0xfff1 ),	/* Offset= -15 (1246) */
			0x5b,		/* FC_END */
/* 1264 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1266 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1268 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1270 */	NdrFcShort( 0xa ),	/* Offset= 10 (1280) */
/* 1272 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1274 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1276 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1252) */
/* 1278 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1280 */	
			0x11, 0x0,	/* FC_RP */
/* 1282 */	NdrFcShort( 0xff0c ),	/* Offset= -244 (1038) */
/* 1284 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 1286 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1288 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1290 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1292 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 1294 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1296 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1298 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1300 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1302 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1304 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1306 */	0x12, 0x0,	/* FC_UP */
/* 1308 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1284) */
/* 1310 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1312 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1314 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 1316 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1318 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1320 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1322 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 1324 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1326 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1328 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1330 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1332 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1334 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1336 */	0x12, 0x0,	/* FC_UP */
/* 1338 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1314) */
/* 1340 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1342 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1344 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1346 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1348 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1350 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1352 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1354 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1356 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1358 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1360 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1362 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1364 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1366 */	0x12, 0x0,	/* FC_UP */
/* 1368 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1344) */
/* 1370 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1372 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1374 */	
			0x1b,		/* FC_CARRAY */
			0x7,		/* 7 */
/* 1376 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1378 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1380 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1382 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 1384 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1386 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1388 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1390 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1392 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1394 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1396 */	0x12, 0x0,	/* FC_UP */
/* 1398 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1374) */
/* 1400 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1402 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1404 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 1406 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1408 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1410 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1412 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1414 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1416 */	0x7,		/* Corr desc: FC_USHORT */
			0x0,		/*  */
/* 1418 */	NdrFcShort( 0xffd8 ),	/* -40 */
/* 1420 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1422 */	NdrFcShort( 0xffee ),	/* Offset= -18 (1404) */
/* 1424 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1426 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1428 */	NdrFcShort( 0x28 ),	/* 40 */
/* 1430 */	NdrFcShort( 0xffee ),	/* Offset= -18 (1412) */
/* 1432 */	NdrFcShort( 0x0 ),	/* Offset= 0 (1432) */
/* 1434 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1436 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1438 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1440 */	NdrFcShort( 0xfdf8 ),	/* Offset= -520 (920) */
/* 1442 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1444 */	
			0x12, 0x0,	/* FC_UP */
/* 1446 */	NdrFcShort( 0xfef6 ),	/* Offset= -266 (1180) */
/* 1448 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1450 */	0x1,		/* FC_BYTE */
			0x5c,		/* FC_PAD */
/* 1452 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1454 */	0x6,		/* FC_SHORT */
			0x5c,		/* FC_PAD */
/* 1456 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1458 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 1460 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1462 */	0xb,		/* FC_HYPER */
			0x5c,		/* FC_PAD */
/* 1464 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1466 */	0xa,		/* FC_FLOAT */
			0x5c,		/* FC_PAD */
/* 1468 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1470 */	0xc,		/* FC_DOUBLE */
			0x5c,		/* FC_PAD */
/* 1472 */	
			0x12, 0x0,	/* FC_UP */
/* 1474 */	NdrFcShort( 0xfd8c ),	/* Offset= -628 (846) */
/* 1476 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 1478 */	NdrFcShort( 0xfd8e ),	/* Offset= -626 (852) */
/* 1480 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 1482 */	NdrFcShort( 0xfda2 ),	/* Offset= -606 (876) */
/* 1484 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 1486 */	NdrFcShort( 0xfdb0 ),	/* Offset= -592 (894) */
/* 1488 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 1490 */	NdrFcShort( 0xfdbe ),	/* Offset= -578 (912) */
/* 1492 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 1494 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1496) */
/* 1496 */	
			0x12, 0x0,	/* FC_UP */
/* 1498 */	NdrFcShort( 0x14 ),	/* Offset= 20 (1518) */
/* 1500 */	
			0x15,		/* FC_STRUCT */
			0x7,		/* 7 */
/* 1502 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1504 */	0x6,		/* FC_SHORT */
			0x1,		/* FC_BYTE */
/* 1506 */	0x1,		/* FC_BYTE */
			0x8,		/* FC_LONG */
/* 1508 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 1510 */	
			0x12, 0x0,	/* FC_UP */
/* 1512 */	NdrFcShort( 0xfff4 ),	/* Offset= -12 (1500) */
/* 1514 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1516 */	0x2,		/* FC_CHAR */
			0x5c,		/* FC_PAD */
/* 1518 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x7,		/* 7 */
/* 1520 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1522 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1524 */	NdrFcShort( 0x0 ),	/* Offset= 0 (1524) */
/* 1526 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1528 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1530 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1532 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1534 */	NdrFcShort( 0xfc28 ),	/* Offset= -984 (550) */
/* 1536 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1538 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1540 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1542 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1544 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1546 */	NdrFcShort( 0xfc18 ),	/* Offset= -1000 (546) */
/* 1548 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1550 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1552 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1554 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1556 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1558 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1560 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1562 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1564 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1566 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1568 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1570 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1572 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1574 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1576 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1578 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1580 */	NdrFcShort( 0x78 ),	/* 120 */
/* 1582 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1584 */	NdrFcShort( 0x22 ),	/* Offset= 34 (1618) */
/* 1586 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1588 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1590 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1592 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1594 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 1596 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1598 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1600 */	NdrFcShort( 0xffc2 ),	/* Offset= -62 (1538) */
/* 1602 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1604 */	NdrFcShort( 0xffbe ),	/* Offset= -66 (1538) */
/* 1606 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1608 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1610 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1612 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1614 */	0xa,		/* FC_FLOAT */
			0x36,		/* FC_POINTER */
/* 1616 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1618 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1620 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1622 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1624 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1626 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1628 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1630 */	
			0x12, 0x0,	/* FC_UP */
/* 1632 */	NdrFcShort( 0xffac ),	/* Offset= -84 (1548) */
/* 1634 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1636 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1638 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1640 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1642 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1644 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1646 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1648 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1650 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1652 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1654 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1656 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1658 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1660 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1662 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1664 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1666 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1668 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1670 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1672 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1674 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1676 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1678 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1682 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1684 */	NdrFcShort( 0xff96 ),	/* Offset= -106 (1578) */
/* 1686 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1688 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1690 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1692) */
/* 1692 */	
			0x13, 0x0,	/* FC_OP */
/* 1694 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1696) */
/* 1696 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1698 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1700 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1702 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1704 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1706 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1708 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1710) */
/* 1710 */	
			0x13, 0x0,	/* FC_OP */
/* 1712 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1714) */
/* 1714 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1716 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1718 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 1720 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1722 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1726 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1728 */	NdrFcShort( 0xff6a ),	/* Offset= -150 (1578) */
/* 1730 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1732 */	
			0x11, 0x0,	/* FC_RP */
/* 1734 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1736) */
/* 1736 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1738 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1740 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1742 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1744 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1748 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1750 */	NdrFcShort( 0xff54 ),	/* Offset= -172 (1578) */
/* 1752 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1754 */	
			0x11, 0x0,	/* FC_RP */
/* 1756 */	NdrFcShort( 0xff4e ),	/* Offset= -178 (1578) */
/* 1758 */	
			0x11, 0x0,	/* FC_RP */
/* 1760 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1762) */
/* 1762 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1764 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1766 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1768 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1770 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1774 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1776 */	NdrFcShort( 0xff3a ),	/* Offset= -198 (1578) */
/* 1778 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1780 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1782 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1784) */
/* 1784 */	
			0x13, 0x0,	/* FC_OP */
/* 1786 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1788) */
/* 1788 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1790 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1792 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1794 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1796 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1798 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1800 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1802) */
/* 1802 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1804 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */

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


/* Object interface: CATID_OPCDXServer10, ver. 0.0,
   GUID={0xA0C85BB8,0x4161,0x4fd6,{0x86,0x55,0xBB,0x58,0x46,0x01,0xC9,0xE0}} */

#pragma code_seg(".orpc")
static const unsigned short CATID_OPCDXServer10_FormatStringOffsetTable[] =
    {
    0
    };

static const MIDL_STUBLESS_PROXY_INFO CATID_OPCDXServer10_ProxyInfo =
    {
    &Object_StubDesc,
    OpcDx__MIDL_ProcFormatString.Format,
    &CATID_OPCDXServer10_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO CATID_OPCDXServer10_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    OpcDx__MIDL_ProcFormatString.Format,
    &CATID_OPCDXServer10_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(3) _CATID_OPCDXServer10ProxyVtbl = 
{
    0,
    &IID_CATID_OPCDXServer10,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy
};

const CInterfaceStubVtbl _CATID_OPCDXServer10StubVtbl =
{
    &IID_CATID_OPCDXServer10,
    &CATID_OPCDXServer10_ServerInfo,
    3,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Standard interface: __MIDL_itf_OpcDx_0000_0001, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: IOPCConfiguration, ver. 0.0,
   GUID={0xC130D281,0xF4AA,0x4779,{0x88,0x46,0xC2,0xC4,0xCB,0x44,0x4F,0x2A}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCConfiguration_FormatStringOffsetTable[] =
    {
    0,
    34,
    74,
    114,
    154,
    200,
    264,
    304,
    368,
    408,
    466,
    530
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCConfiguration_ProxyInfo =
    {
    &Object_StubDesc,
    OpcDx__MIDL_ProcFormatString.Format,
    &IOPCConfiguration_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCConfiguration_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    OpcDx__MIDL_ProcFormatString.Format,
    &IOPCConfiguration_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(15) _IOPCConfigurationProxyVtbl = 
{
    &IOPCConfiguration_ProxyInfo,
    &IID_IOPCConfiguration,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCConfiguration::GetServers */ ,
    (void *) (INT_PTR) -1 /* IOPCConfiguration::AddServers */ ,
    (void *) (INT_PTR) -1 /* IOPCConfiguration::ModifyServers */ ,
    (void *) (INT_PTR) -1 /* IOPCConfiguration::DeleteServers */ ,
    (void *) (INT_PTR) -1 /* IOPCConfiguration::CopyDefaultServerAttributes */ ,
    (void *) (INT_PTR) -1 /* IOPCConfiguration::QueryDXConnections */ ,
    (void *) (INT_PTR) -1 /* IOPCConfiguration::AddDXConnections */ ,
    (void *) (INT_PTR) -1 /* IOPCConfiguration::UpdateDXConnections */ ,
    (void *) (INT_PTR) -1 /* IOPCConfiguration::ModifyDXConnections */ ,
    (void *) (INT_PTR) -1 /* IOPCConfiguration::DeleteDXConnections */ ,
    (void *) (INT_PTR) -1 /* IOPCConfiguration::CopyDXConnectionDefaultAttributes */ ,
    (void *) (INT_PTR) -1 /* IOPCConfiguration::ResetConfiguration */
};

const CInterfaceStubVtbl _IOPCConfigurationStubVtbl =
{
    &IID_IOPCConfiguration,
    &IOPCConfiguration_ServerInfo,
    15,
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
    OpcDx__MIDL_TypeFormatString.Format,
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

const CInterfaceProxyVtbl * const _OpcDx_ProxyVtblList[] = 
{
    ( CInterfaceProxyVtbl *) &_IOPCConfigurationProxyVtbl,
    ( CInterfaceProxyVtbl *) &_CATID_OPCDXServer10ProxyVtbl,
    0
};

const CInterfaceStubVtbl * const _OpcDx_StubVtblList[] = 
{
    ( CInterfaceStubVtbl *) &_IOPCConfigurationStubVtbl,
    ( CInterfaceStubVtbl *) &_CATID_OPCDXServer10StubVtbl,
    0
};

PCInterfaceName const _OpcDx_InterfaceNamesList[] = 
{
    "IOPCConfiguration",
    "CATID_OPCDXServer10",
    0
};


#define _OpcDx_CHECK_IID(n)	IID_GENERIC_CHECK_IID( _OpcDx, pIID, n)

int __stdcall _OpcDx_IID_Lookup( const IID * pIID, int * pIndex )
{
    IID_BS_LOOKUP_SETUP

    IID_BS_LOOKUP_INITIAL_TEST( _OpcDx, 2, 1 )
    IID_BS_LOOKUP_RETURN_RESULT( _OpcDx, 2, *pIndex )
    
}

const ExtendedProxyFileInfo OpcDx_ProxyFileInfo = 
{
    (PCInterfaceProxyVtblList *) & _OpcDx_ProxyVtblList,
    (PCInterfaceStubVtblList *) & _OpcDx_StubVtblList,
    (const PCInterfaceName * ) & _OpcDx_InterfaceNamesList,
    0, /* no delegation */
    & _OpcDx_IID_Lookup, 
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

