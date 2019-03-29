

/* this ALWAYS GENERATED file contains the proxy stub code */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Thu Jun 25 06:09:02 2015
 */
/* Compiler settings for opchda.idl:
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


#include "opchda.h"

#define TYPE_FORMAT_STRING_SIZE   2169                              
#define PROC_FORMAT_STRING_SIZE   2919                              
#define EXPR_FORMAT_STRING_SIZE   1                                 
#define TRANSMIT_AS_TABLE_SIZE    0            
#define WIRE_MARSHAL_TABLE_SIZE   1            

typedef struct _opchda_MIDL_TYPE_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ TYPE_FORMAT_STRING_SIZE ];
    } opchda_MIDL_TYPE_FORMAT_STRING;

typedef struct _opchda_MIDL_PROC_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ PROC_FORMAT_STRING_SIZE ];
    } opchda_MIDL_PROC_FORMAT_STRING;

typedef struct _opchda_MIDL_EXPR_FORMAT_STRING
    {
    long          Pad;
    unsigned char  Format[ EXPR_FORMAT_STRING_SIZE ];
    } opchda_MIDL_EXPR_FORMAT_STRING;


static const RPC_SYNTAX_IDENTIFIER  _RpcTransferSyntax = 
{{0x8A885D04,0x1CEB,0x11C9,{0x9F,0xE8,0x08,0x00,0x2B,0x10,0x48,0x60}},{2,0}};


extern const opchda_MIDL_TYPE_FORMAT_STRING opchda__MIDL_TypeFormatString;
extern const opchda_MIDL_PROC_FORMAT_STRING opchda__MIDL_ProcFormatString;
extern const opchda_MIDL_EXPR_FORMAT_STRING opchda__MIDL_ExprFormatString;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO CATID_OPCHDAServer10_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO CATID_OPCHDAServer10_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCHDA_Browser_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCHDA_Browser_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCHDA_Server_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCHDA_Server_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCHDA_SyncRead_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCHDA_SyncRead_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCHDA_SyncUpdate_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCHDA_SyncUpdate_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCHDA_SyncAnnotations_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCHDA_SyncAnnotations_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCHDA_AsyncRead_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCHDA_AsyncRead_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCHDA_AsyncUpdate_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCHDA_AsyncUpdate_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCHDA_AsyncAnnotations_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCHDA_AsyncAnnotations_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCHDA_Playback_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCHDA_Playback_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCHDA_DataCallback_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCHDA_DataCallback_ProxyInfo;


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


static const opchda_MIDL_PROC_FORMAT_STRING opchda__MIDL_ProcFormatString =
    {
        0,
        {

	/* Procedure GetEnum */

			0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/*  2 */	NdrFcLong( 0x0 ),	/* 0 */
/*  6 */	NdrFcShort( 0x3 ),	/* 3 */
/*  8 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 10 */	NdrFcShort( 0x6 ),	/* 6 */
/* 12 */	NdrFcShort( 0x8 ),	/* 8 */
/* 14 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x3,		/* 3 */

	/* Parameter dwBrowseType */

/* 16 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 18 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 20 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter ppIEnumString */

/* 22 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 24 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 26 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Return value */

/* 28 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 30 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 32 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ChangeBrowsePosition */

/* 34 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 36 */	NdrFcLong( 0x0 ),	/* 0 */
/* 40 */	NdrFcShort( 0x4 ),	/* 4 */
/* 42 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 44 */	NdrFcShort( 0x6 ),	/* 6 */
/* 46 */	NdrFcShort( 0x8 ),	/* 8 */
/* 48 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter dwBrowseDirection */

/* 50 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 52 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 54 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter szString */

/* 56 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 58 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 60 */	NdrFcShort( 0x1a ),	/* Type Offset=26 */

	/* Return value */

/* 62 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 64 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 66 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetItemID */

/* 68 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 70 */	NdrFcLong( 0x0 ),	/* 0 */
/* 74 */	NdrFcShort( 0x5 ),	/* 5 */
/* 76 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 78 */	NdrFcShort( 0x0 ),	/* 0 */
/* 80 */	NdrFcShort( 0x8 ),	/* 8 */
/* 82 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter szNode */

/* 84 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 86 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 88 */	NdrFcShort( 0x1a ),	/* Type Offset=26 */

	/* Parameter pszItemID */

/* 90 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 92 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 94 */	NdrFcShort( 0x1c ),	/* Type Offset=28 */

	/* Return value */

/* 96 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 98 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 100 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetBranchPosition */

/* 102 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 104 */	NdrFcLong( 0x0 ),	/* 0 */
/* 108 */	NdrFcShort( 0x6 ),	/* 6 */
/* 110 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 112 */	NdrFcShort( 0x0 ),	/* 0 */
/* 114 */	NdrFcShort( 0x8 ),	/* 8 */
/* 116 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x2,		/* 2 */

	/* Parameter pszBranchPos */

/* 118 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 120 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 122 */	NdrFcShort( 0x1c ),	/* Type Offset=28 */

	/* Return value */

/* 124 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 126 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 128 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetItemAttributes */

/* 130 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 132 */	NdrFcLong( 0x0 ),	/* 0 */
/* 136 */	NdrFcShort( 0x3 ),	/* 3 */
/* 138 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 140 */	NdrFcShort( 0x0 ),	/* 0 */
/* 142 */	NdrFcShort( 0x24 ),	/* 36 */
/* 144 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x6,		/* 6 */

	/* Parameter pdwCount */

/* 146 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 148 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 150 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppdwAttrID */

/* 152 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 154 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 156 */	NdrFcShort( 0x28 ),	/* Type Offset=40 */

	/* Parameter ppszAttrName */

/* 158 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 160 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 162 */	NdrFcShort( 0x3a ),	/* Type Offset=58 */

	/* Parameter ppszAttrDesc */

/* 164 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 166 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 168 */	NdrFcShort( 0x3a ),	/* Type Offset=58 */

	/* Parameter ppvtAttrDataType */

/* 170 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 172 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 174 */	NdrFcShort( 0x60 ),	/* Type Offset=96 */

	/* Return value */

/* 176 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 178 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 180 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetAggregates */

/* 182 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 184 */	NdrFcLong( 0x0 ),	/* 0 */
/* 188 */	NdrFcShort( 0x4 ),	/* 4 */
/* 190 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 192 */	NdrFcShort( 0x0 ),	/* 0 */
/* 194 */	NdrFcShort( 0x24 ),	/* 36 */
/* 196 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x5,		/* 5 */

	/* Parameter pdwCount */

/* 198 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 200 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 202 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppdwAggrID */

/* 204 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 206 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 208 */	NdrFcShort( 0x28 ),	/* Type Offset=40 */

	/* Parameter ppszAggrName */

/* 210 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 212 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 214 */	NdrFcShort( 0x3a ),	/* Type Offset=58 */

	/* Parameter ppszAggrDesc */

/* 216 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 218 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 220 */	NdrFcShort( 0x3a ),	/* Type Offset=58 */

	/* Return value */

/* 222 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 224 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 226 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetHistorianStatus */

/* 228 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 230 */	NdrFcLong( 0x0 ),	/* 0 */
/* 234 */	NdrFcShort( 0x5 ),	/* 5 */
/* 236 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 238 */	NdrFcShort( 0x0 ),	/* 0 */
/* 240 */	NdrFcShort( 0x10c ),	/* 268 */
/* 242 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0xa,		/* 10 */

	/* Parameter pwStatus */

/* 244 */	NdrFcShort( 0x2010 ),	/* Flags:  out, srv alloc size=8 */
/* 246 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 248 */	NdrFcShort( 0x72 ),	/* Type Offset=114 */

	/* Parameter pftCurrentTime */

/* 250 */	NdrFcShort( 0x2012 ),	/* Flags:  must free, out, srv alloc size=8 */
/* 252 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 254 */	NdrFcShort( 0x76 ),	/* Type Offset=118 */

	/* Parameter pftStartTime */

/* 256 */	NdrFcShort( 0x2012 ),	/* Flags:  must free, out, srv alloc size=8 */
/* 258 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 260 */	NdrFcShort( 0x76 ),	/* Type Offset=118 */

	/* Parameter pwMajorVersion */

/* 262 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 264 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 266 */	0x6,		/* FC_SHORT */
			0x0,		/* 0 */

	/* Parameter pwMinorVersion */

/* 268 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 270 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 272 */	0x6,		/* FC_SHORT */
			0x0,		/* 0 */

	/* Parameter pwBuildNumber */

/* 274 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 276 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 278 */	0x6,		/* FC_SHORT */
			0x0,		/* 0 */

	/* Parameter pdwMaxReturnValues */

/* 280 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 282 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 284 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppszStatusString */

/* 286 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 288 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 290 */	NdrFcShort( 0x1c ),	/* Type Offset=28 */

	/* Parameter ppszVendorInfo */

/* 292 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 294 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 296 */	NdrFcShort( 0x1c ),	/* Type Offset=28 */

	/* Return value */

/* 298 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 300 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 302 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetItemHandles */

/* 304 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 306 */	NdrFcLong( 0x0 ),	/* 0 */
/* 310 */	NdrFcShort( 0x6 ),	/* 6 */
/* 312 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 314 */	NdrFcShort( 0x8 ),	/* 8 */
/* 316 */	NdrFcShort( 0x8 ),	/* 8 */
/* 318 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwCount */

/* 320 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 322 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 324 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszItemID */

/* 326 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 328 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 330 */	NdrFcShort( 0x8e ),	/* Type Offset=142 */

	/* Parameter phClient */

/* 332 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 334 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 336 */	NdrFcShort( 0xb0 ),	/* Type Offset=176 */

	/* Parameter pphServer */

/* 338 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 340 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 342 */	NdrFcShort( 0xba ),	/* Type Offset=186 */

	/* Parameter ppErrors */

/* 344 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 346 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 348 */	NdrFcShort( 0xba ),	/* Type Offset=186 */

	/* Return value */

/* 350 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 352 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 354 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReleaseItemHandles */

/* 356 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 358 */	NdrFcLong( 0x0 ),	/* 0 */
/* 362 */	NdrFcShort( 0x7 ),	/* 7 */
/* 364 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 366 */	NdrFcShort( 0x8 ),	/* 8 */
/* 368 */	NdrFcShort( 0x8 ),	/* 8 */
/* 370 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwCount */

/* 372 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 374 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 376 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 378 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 380 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 382 */	NdrFcShort( 0xb0 ),	/* Type Offset=176 */

	/* Parameter ppErrors */

/* 384 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 386 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 388 */	NdrFcShort( 0xba ),	/* Type Offset=186 */

	/* Return value */

/* 390 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 392 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 394 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ValidateItemIDs */

/* 396 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 398 */	NdrFcLong( 0x0 ),	/* 0 */
/* 402 */	NdrFcShort( 0x8 ),	/* 8 */
/* 404 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 406 */	NdrFcShort( 0x8 ),	/* 8 */
/* 408 */	NdrFcShort( 0x8 ),	/* 8 */
/* 410 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwCount */

/* 412 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 414 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 416 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszItemID */

/* 418 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 420 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 422 */	NdrFcShort( 0x8e ),	/* Type Offset=142 */

	/* Parameter ppErrors */

/* 424 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 426 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 428 */	NdrFcShort( 0xba ),	/* Type Offset=186 */

	/* Return value */

/* 430 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 432 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 434 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CreateBrowse */

/* 436 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 438 */	NdrFcLong( 0x0 ),	/* 0 */
/* 442 */	NdrFcShort( 0x9 ),	/* 9 */
/* 444 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 446 */	NdrFcShort( 0x8 ),	/* 8 */
/* 448 */	NdrFcShort( 0x8 ),	/* 8 */
/* 450 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwCount */

/* 452 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 454 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 456 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwAttrID */

/* 458 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 460 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 462 */	NdrFcShort( 0xb0 ),	/* Type Offset=176 */

	/* Parameter pOperator */

/* 464 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 466 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 468 */	NdrFcShort( 0xc6 ),	/* Type Offset=198 */

	/* Parameter vFilter */

/* 470 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 472 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 474 */	NdrFcShort( 0x4ba ),	/* Type Offset=1210 */

	/* Parameter pphBrowser */

/* 476 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 478 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 480 */	NdrFcShort( 0x4cc ),	/* Type Offset=1228 */

	/* Parameter ppErrors */

/* 482 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 484 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 486 */	NdrFcShort( 0xba ),	/* Type Offset=186 */

	/* Return value */

/* 488 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 490 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 492 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadRaw */

/* 494 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 496 */	NdrFcLong( 0x0 ),	/* 0 */
/* 500 */	NdrFcShort( 0x3 ),	/* 3 */
/* 502 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 504 */	NdrFcShort( 0x18 ),	/* 24 */
/* 506 */	NdrFcShort( 0x8 ),	/* 8 */
/* 508 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x9,		/* 9 */

	/* Parameter htStartTime */

/* 510 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 512 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 514 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 516 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 518 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 520 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter dwNumValues */

/* 522 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 524 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 526 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter bBounds */

/* 528 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 530 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 532 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 534 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 536 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 538 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 540 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 542 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 544 */	NdrFcShort( 0x502 ),	/* Type Offset=1282 */

	/* Parameter ppItemValues */

/* 546 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 548 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 550 */	NdrFcShort( 0x50c ),	/* Type Offset=1292 */

	/* Parameter ppErrors */

/* 552 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 554 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 556 */	NdrFcShort( 0x57a ),	/* Type Offset=1402 */

	/* Return value */

/* 558 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 560 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 562 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadProcessed */

/* 564 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 566 */	NdrFcLong( 0x0 ),	/* 0 */
/* 570 */	NdrFcShort( 0x4 ),	/* 4 */
/* 572 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 574 */	NdrFcShort( 0x20 ),	/* 32 */
/* 576 */	NdrFcShort( 0x8 ),	/* 8 */
/* 578 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x9,		/* 9 */

	/* Parameter htStartTime */

/* 580 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 582 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 584 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 586 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 588 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 590 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter ftResampleInterval */

/* 592 */	NdrFcShort( 0x8a ),	/* Flags:  must free, in, by val, */
/* 594 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 596 */	NdrFcShort( 0x7e ),	/* Type Offset=126 */

	/* Parameter dwNumItems */

/* 598 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 600 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 602 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 604 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 606 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 608 */	NdrFcShort( 0x502 ),	/* Type Offset=1282 */

	/* Parameter haAggregate */

/* 610 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 612 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 614 */	NdrFcShort( 0x502 ),	/* Type Offset=1282 */

	/* Parameter ppItemValues */

/* 616 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 618 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 620 */	NdrFcShort( 0x50c ),	/* Type Offset=1292 */

	/* Parameter ppErrors */

/* 622 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 624 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 626 */	NdrFcShort( 0x57a ),	/* Type Offset=1402 */

	/* Return value */

/* 628 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 630 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 632 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadAtTime */

/* 634 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 636 */	NdrFcLong( 0x0 ),	/* 0 */
/* 640 */	NdrFcShort( 0x5 ),	/* 5 */
/* 642 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 644 */	NdrFcShort( 0x10 ),	/* 16 */
/* 646 */	NdrFcShort( 0x8 ),	/* 8 */
/* 648 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwNumTimeStamps */

/* 650 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 652 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 654 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ftTimeStamps */

/* 656 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 658 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 660 */	NdrFcShort( 0x586 ),	/* Type Offset=1414 */

	/* Parameter dwNumItems */

/* 662 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 664 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 666 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 668 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 670 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 672 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Parameter ppItemValues */

/* 674 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 676 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 678 */	NdrFcShort( 0x5a2 ),	/* Type Offset=1442 */

	/* Parameter ppErrors */

/* 680 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 682 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 684 */	NdrFcShort( 0x5bc ),	/* Type Offset=1468 */

	/* Return value */

/* 686 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 688 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 690 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadModified */

/* 692 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 694 */	NdrFcLong( 0x0 ),	/* 0 */
/* 698 */	NdrFcShort( 0x6 ),	/* 6 */
/* 700 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 702 */	NdrFcShort( 0x10 ),	/* 16 */
/* 704 */	NdrFcShort( 0x8 ),	/* 8 */
/* 706 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x8,		/* 8 */

	/* Parameter htStartTime */

/* 708 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 710 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 712 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 714 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 716 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 718 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter dwNumValues */

/* 720 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 722 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 724 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 726 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 728 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 730 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 732 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 734 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 736 */	NdrFcShort( 0x5c8 ),	/* Type Offset=1480 */

	/* Parameter ppItemValues */

/* 738 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 740 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 742 */	NdrFcShort( 0x5d2 ),	/* Type Offset=1490 */

	/* Parameter ppErrors */

/* 744 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 746 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 748 */	NdrFcShort( 0x66c ),	/* Type Offset=1644 */

	/* Return value */

/* 750 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 752 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 754 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadAttribute */

/* 756 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 758 */	NdrFcLong( 0x0 ),	/* 0 */
/* 762 */	NdrFcShort( 0x7 ),	/* 7 */
/* 764 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 766 */	NdrFcShort( 0x10 ),	/* 16 */
/* 768 */	NdrFcShort( 0x8 ),	/* 8 */
/* 770 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x8,		/* 8 */

	/* Parameter htStartTime */

/* 772 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 774 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 776 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 778 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 780 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 782 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter hServer */

/* 784 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 786 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 788 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumAttributes */

/* 790 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 792 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 794 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwAttributeIDs */

/* 796 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 798 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 800 */	NdrFcShort( 0x5c8 ),	/* Type Offset=1480 */

	/* Parameter ppAttributeValues */

/* 802 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 804 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 806 */	NdrFcShort( 0x674 ),	/* Type Offset=1652 */

	/* Parameter ppErrors */

/* 808 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 810 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 812 */	NdrFcShort( 0x66c ),	/* Type Offset=1644 */

	/* Return value */

/* 814 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 816 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 818 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryCapabilities */


	/* Procedure QueryCapabilities */


	/* Procedure QueryCapabilities */


	/* Procedure QueryCapabilities */

/* 820 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 822 */	NdrFcLong( 0x0 ),	/* 0 */
/* 826 */	NdrFcShort( 0x3 ),	/* 3 */
/* 828 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 830 */	NdrFcShort( 0x0 ),	/* 0 */
/* 832 */	NdrFcShort( 0x22 ),	/* 34 */
/* 834 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter pCapabilities */


	/* Parameter pCapabilities */


	/* Parameter pCapabilities */


	/* Parameter pCapabilities */

/* 836 */	NdrFcShort( 0x2010 ),	/* Flags:  out, srv alloc size=8 */
/* 838 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 840 */	NdrFcShort( 0x72 ),	/* Type Offset=114 */

	/* Return value */


	/* Return value */


	/* Return value */


	/* Return value */

/* 842 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 844 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 846 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Insert */

/* 848 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 850 */	NdrFcLong( 0x0 ),	/* 0 */
/* 854 */	NdrFcShort( 0x4 ),	/* 4 */
/* 856 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 858 */	NdrFcShort( 0x8 ),	/* 8 */
/* 860 */	NdrFcShort( 0x8 ),	/* 8 */
/* 862 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwNumItems */

/* 864 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 866 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 868 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 870 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 872 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 874 */	NdrFcShort( 0xb0 ),	/* Type Offset=176 */

	/* Parameter ftTimeStamps */

/* 876 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 878 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 880 */	NdrFcShort( 0x586 ),	/* Type Offset=1414 */

	/* Parameter vDataValues */

/* 882 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 884 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 886 */	NdrFcShort( 0x4ba ),	/* Type Offset=1210 */

	/* Parameter pdwQualities */

/* 888 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 890 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 892 */	NdrFcShort( 0xb0 ),	/* Type Offset=176 */

	/* Parameter ppErrors */

/* 894 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 896 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 898 */	NdrFcShort( 0xba ),	/* Type Offset=186 */

	/* Return value */

/* 900 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 902 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 904 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Replace */

/* 906 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 908 */	NdrFcLong( 0x0 ),	/* 0 */
/* 912 */	NdrFcShort( 0x5 ),	/* 5 */
/* 914 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 916 */	NdrFcShort( 0x8 ),	/* 8 */
/* 918 */	NdrFcShort( 0x8 ),	/* 8 */
/* 920 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwNumItems */

/* 922 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 924 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 926 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 928 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 930 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 932 */	NdrFcShort( 0xb0 ),	/* Type Offset=176 */

	/* Parameter ftTimeStamps */

/* 934 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 936 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 938 */	NdrFcShort( 0x586 ),	/* Type Offset=1414 */

	/* Parameter vDataValues */

/* 940 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 942 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 944 */	NdrFcShort( 0x4ba ),	/* Type Offset=1210 */

	/* Parameter pdwQualities */

/* 946 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 948 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 950 */	NdrFcShort( 0xb0 ),	/* Type Offset=176 */

	/* Parameter ppErrors */

/* 952 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 954 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 956 */	NdrFcShort( 0xba ),	/* Type Offset=186 */

	/* Return value */

/* 958 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 960 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 962 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure InsertReplace */

/* 964 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 966 */	NdrFcLong( 0x0 ),	/* 0 */
/* 970 */	NdrFcShort( 0x6 ),	/* 6 */
/* 972 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 974 */	NdrFcShort( 0x8 ),	/* 8 */
/* 976 */	NdrFcShort( 0x8 ),	/* 8 */
/* 978 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwNumItems */

/* 980 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 982 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 984 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 986 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 988 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 990 */	NdrFcShort( 0xb0 ),	/* Type Offset=176 */

	/* Parameter ftTimeStamps */

/* 992 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 994 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 996 */	NdrFcShort( 0x586 ),	/* Type Offset=1414 */

	/* Parameter vDataValues */

/* 998 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1000 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1002 */	NdrFcShort( 0x4ba ),	/* Type Offset=1210 */

	/* Parameter pdwQualities */

/* 1004 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1006 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1008 */	NdrFcShort( 0xb0 ),	/* Type Offset=176 */

	/* Parameter ppErrors */

/* 1010 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1012 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1014 */	NdrFcShort( 0xba ),	/* Type Offset=186 */

	/* Return value */

/* 1016 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1018 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1020 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure DeleteRaw */

/* 1022 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1024 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1028 */	NdrFcShort( 0x7 ),	/* 7 */
/* 1030 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1032 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1034 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1036 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter htStartTime */

/* 1038 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1040 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1042 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 1044 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1046 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1048 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter dwNumItems */

/* 1050 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1052 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1054 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1056 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1058 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1060 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Parameter ppErrors */

/* 1062 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1064 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1066 */	NdrFcShort( 0x5bc ),	/* Type Offset=1468 */

	/* Return value */

/* 1068 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1070 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1072 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure DeleteAtTime */

/* 1074 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1076 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1080 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1082 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1084 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1086 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1088 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwNumItems */

/* 1090 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1092 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1094 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1096 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1098 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1100 */	NdrFcShort( 0xb0 ),	/* Type Offset=176 */

	/* Parameter ftTimeStamps */

/* 1102 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1104 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1106 */	NdrFcShort( 0x586 ),	/* Type Offset=1414 */

	/* Parameter ppErrors */

/* 1108 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1110 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1112 */	NdrFcShort( 0xba ),	/* Type Offset=186 */

	/* Return value */

/* 1114 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1116 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1118 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Read */

/* 1120 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1122 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1126 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1128 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1130 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1132 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1134 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter htStartTime */

/* 1136 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1138 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1140 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 1142 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1144 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1146 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter dwNumItems */

/* 1148 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1150 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1152 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1154 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1156 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1158 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Parameter ppAnnotationValues */

/* 1160 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1162 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1164 */	NdrFcShort( 0x6a4 ),	/* Type Offset=1700 */

	/* Parameter ppErrors */

/* 1166 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1168 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1170 */	NdrFcShort( 0x5bc ),	/* Type Offset=1468 */

	/* Return value */

/* 1172 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1174 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1176 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Insert */

/* 1178 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1180 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1184 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1186 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1188 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1190 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1192 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwNumItems */

/* 1194 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1196 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1198 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1200 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1202 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1204 */	NdrFcShort( 0xb0 ),	/* Type Offset=176 */

	/* Parameter ftTimeStamps */

/* 1206 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1208 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1210 */	NdrFcShort( 0x586 ),	/* Type Offset=1414 */

	/* Parameter pAnnotationValues */

/* 1212 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1214 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1216 */	NdrFcShort( 0x71e ),	/* Type Offset=1822 */

	/* Parameter ppErrors */

/* 1218 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1220 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1222 */	NdrFcShort( 0xba ),	/* Type Offset=186 */

	/* Return value */

/* 1224 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1226 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1228 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadRaw */

/* 1230 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1232 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1236 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1238 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 1240 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1242 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1244 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0xa,		/* 10 */

	/* Parameter dwTransactionID */

/* 1246 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1248 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1250 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter htStartTime */

/* 1252 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1254 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1256 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 1258 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1260 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1262 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter dwNumValues */

/* 1264 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1266 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1268 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter bBounds */

/* 1270 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1272 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1274 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 1276 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1278 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1280 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1282 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1284 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1286 */	NdrFcShort( 0x75a ),	/* Type Offset=1882 */

	/* Parameter pdwCancelID */

/* 1288 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1290 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1292 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1294 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1296 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 1298 */	NdrFcShort( 0x764 ),	/* Type Offset=1892 */

	/* Return value */

/* 1300 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1302 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 1304 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure AdviseRaw */

/* 1306 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1308 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1312 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1314 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 1316 */	NdrFcShort( 0x28 ),	/* 40 */
/* 1318 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1320 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x8,		/* 8 */

	/* Parameter dwTransactionID */

/* 1322 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1324 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1326 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter htStartTime */

/* 1328 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1330 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1332 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter ftUpdateInterval */

/* 1334 */	NdrFcShort( 0x8a ),	/* Flags:  must free, in, by val, */
/* 1336 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1338 */	NdrFcShort( 0x7e ),	/* Type Offset=126 */

	/* Parameter dwNumItems */

/* 1340 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1342 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1344 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1346 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1348 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1350 */	NdrFcShort( 0x502 ),	/* Type Offset=1282 */

	/* Parameter pdwCancelID */

/* 1352 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1354 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1356 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1358 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1360 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1362 */	NdrFcShort( 0x57a ),	/* Type Offset=1402 */

	/* Return value */

/* 1364 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1366 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 1368 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadProcessed */

/* 1370 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1372 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1376 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1378 */	NdrFcShort( 0x30 ),	/* x86 Stack size/offset = 48 */
/* 1380 */	NdrFcShort( 0x28 ),	/* 40 */
/* 1382 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1384 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0xa,		/* 10 */

	/* Parameter dwTransactionID */

/* 1386 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1388 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1390 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter htStartTime */

/* 1392 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1394 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1396 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 1398 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1400 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1402 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter ftResampleInterval */

/* 1404 */	NdrFcShort( 0x8a ),	/* Flags:  must free, in, by val, */
/* 1406 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1408 */	NdrFcShort( 0x7e ),	/* Type Offset=126 */

	/* Parameter dwNumItems */

/* 1410 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1412 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1414 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1416 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1418 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1420 */	NdrFcShort( 0x75a ),	/* Type Offset=1882 */

	/* Parameter haAggregate */

/* 1422 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1424 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1426 */	NdrFcShort( 0x75a ),	/* Type Offset=1882 */

	/* Parameter pdwCancelID */

/* 1428 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1430 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 1432 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1434 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1436 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 1438 */	NdrFcShort( 0x764 ),	/* Type Offset=1892 */

	/* Return value */

/* 1440 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1442 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 1444 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure AdviseProcessed */

/* 1446 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1448 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1452 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1454 */	NdrFcShort( 0x30 ),	/* x86 Stack size/offset = 48 */
/* 1456 */	NdrFcShort( 0x30 ),	/* 48 */
/* 1458 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1460 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0xa,		/* 10 */

	/* Parameter dwTransactionID */

/* 1462 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1464 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1466 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter htStartTime */

/* 1468 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1470 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1472 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter ftResampleInterval */

/* 1474 */	NdrFcShort( 0x8a ),	/* Flags:  must free, in, by val, */
/* 1476 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1478 */	NdrFcShort( 0x7e ),	/* Type Offset=126 */

	/* Parameter dwNumItems */

/* 1480 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1482 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1484 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1486 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1488 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1490 */	NdrFcShort( 0x502 ),	/* Type Offset=1282 */

	/* Parameter haAggregate */

/* 1492 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1494 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1496 */	NdrFcShort( 0x502 ),	/* Type Offset=1282 */

	/* Parameter dwNumIntervals */

/* 1498 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1500 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1502 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCancelID */

/* 1504 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1506 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 1508 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1510 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1512 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 1514 */	NdrFcShort( 0x57a ),	/* Type Offset=1402 */

	/* Return value */

/* 1516 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1518 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 1520 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadAtTime */

/* 1522 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1524 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1528 */	NdrFcShort( 0x7 ),	/* 7 */
/* 1530 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 1532 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1534 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1536 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x8,		/* 8 */

	/* Parameter dwTransactionID */

/* 1538 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1540 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1542 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumTimeStamps */

/* 1544 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1546 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1548 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ftTimeStamps */

/* 1550 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1552 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1554 */	NdrFcShort( 0x770 ),	/* Type Offset=1904 */

	/* Parameter dwNumItems */

/* 1556 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1558 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1560 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1562 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1564 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1566 */	NdrFcShort( 0x5c8 ),	/* Type Offset=1480 */

	/* Parameter pdwCancelID */

/* 1568 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1570 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1572 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1574 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1576 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1578 */	NdrFcShort( 0x66c ),	/* Type Offset=1644 */

	/* Return value */

/* 1580 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1582 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1584 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadModified */

/* 1586 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1588 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1592 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1594 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 1596 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1598 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1600 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x9,		/* 9 */

	/* Parameter dwTransactionID */

/* 1602 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1604 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1606 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter htStartTime */

/* 1608 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1610 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1612 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 1614 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1616 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1618 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter dwNumValues */

/* 1620 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1622 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1624 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 1626 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1628 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1630 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1632 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1634 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1636 */	NdrFcShort( 0x502 ),	/* Type Offset=1282 */

	/* Parameter pdwCancelID */

/* 1638 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1640 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1642 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1644 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1646 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1648 */	NdrFcShort( 0x57a ),	/* Type Offset=1402 */

	/* Return value */

/* 1650 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1652 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 1654 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadAttribute */

/* 1656 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1658 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1662 */	NdrFcShort( 0x9 ),	/* 9 */
/* 1664 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 1666 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1668 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1670 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x9,		/* 9 */

	/* Parameter dwTransactionID */

/* 1672 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1674 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1676 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter htStartTime */

/* 1678 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1680 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1682 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 1684 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1686 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1688 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter hServer */

/* 1690 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1692 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1694 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumAttributes */

/* 1696 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1698 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1700 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwAttributeIDs */

/* 1702 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1704 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1706 */	NdrFcShort( 0x502 ),	/* Type Offset=1282 */

	/* Parameter pdwCancelID */

/* 1708 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1710 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1712 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1714 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1716 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1718 */	NdrFcShort( 0x57a ),	/* Type Offset=1402 */

	/* Return value */

/* 1720 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1722 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 1724 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Cancel */

/* 1726 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1728 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1732 */	NdrFcShort( 0xa ),	/* 10 */
/* 1734 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1736 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1738 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1740 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter dwCancelID */

/* 1742 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1744 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1746 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1748 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1750 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1752 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Insert */

/* 1754 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1756 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1760 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1762 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 1764 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1766 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1768 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x9,		/* 9 */

	/* Parameter dwTransactionID */

/* 1770 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1772 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1774 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 1776 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1778 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1780 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1782 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1784 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1786 */	NdrFcShort( 0x782 ),	/* Type Offset=1922 */

	/* Parameter ftTimeStamps */

/* 1788 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1790 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1792 */	NdrFcShort( 0x770 ),	/* Type Offset=1904 */

	/* Parameter vDataValues */

/* 1794 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1796 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1798 */	NdrFcShort( 0x790 ),	/* Type Offset=1936 */

	/* Parameter pdwQualities */

/* 1800 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1802 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1804 */	NdrFcShort( 0x782 ),	/* Type Offset=1922 */

	/* Parameter pdwCancelID */

/* 1806 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1808 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1810 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1812 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1814 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1816 */	NdrFcShort( 0x7a2 ),	/* Type Offset=1954 */

	/* Return value */

/* 1818 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1820 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 1822 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Replace */

/* 1824 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1826 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1830 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1832 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 1834 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1836 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1838 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x9,		/* 9 */

	/* Parameter dwTransactionID */

/* 1840 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1842 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1844 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 1846 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1848 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1850 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1852 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1854 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1856 */	NdrFcShort( 0x782 ),	/* Type Offset=1922 */

	/* Parameter ftTimeStamps */

/* 1858 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1860 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1862 */	NdrFcShort( 0x770 ),	/* Type Offset=1904 */

	/* Parameter vDataValues */

/* 1864 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1866 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1868 */	NdrFcShort( 0x790 ),	/* Type Offset=1936 */

	/* Parameter pdwQualities */

/* 1870 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1872 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1874 */	NdrFcShort( 0x782 ),	/* Type Offset=1922 */

	/* Parameter pdwCancelID */

/* 1876 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1878 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1880 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1882 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1884 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1886 */	NdrFcShort( 0x7a2 ),	/* Type Offset=1954 */

	/* Return value */

/* 1888 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1890 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 1892 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure InsertReplace */

/* 1894 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1896 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1900 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1902 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 1904 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1906 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1908 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x9,		/* 9 */

	/* Parameter dwTransactionID */

/* 1910 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1912 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1914 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 1916 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1918 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1920 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1922 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1924 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1926 */	NdrFcShort( 0x782 ),	/* Type Offset=1922 */

	/* Parameter ftTimeStamps */

/* 1928 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1930 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1932 */	NdrFcShort( 0x770 ),	/* Type Offset=1904 */

	/* Parameter vDataValues */

/* 1934 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1936 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1938 */	NdrFcShort( 0x790 ),	/* Type Offset=1936 */

	/* Parameter pdwQualities */

/* 1940 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1942 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1944 */	NdrFcShort( 0x782 ),	/* Type Offset=1922 */

	/* Parameter pdwCancelID */

/* 1946 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1948 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1950 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1952 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1954 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1956 */	NdrFcShort( 0x7a2 ),	/* Type Offset=1954 */

	/* Return value */

/* 1958 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1960 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 1962 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure DeleteRaw */

/* 1964 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1966 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1970 */	NdrFcShort( 0x7 ),	/* 7 */
/* 1972 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 1974 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1976 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1978 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x8,		/* 8 */

	/* Parameter dwTransactionID */

/* 1980 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1982 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1984 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter htStartTime */

/* 1986 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1988 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1990 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 1992 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 1994 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1996 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter dwNumItems */

/* 1998 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2000 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2002 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2004 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2006 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2008 */	NdrFcShort( 0x5c8 ),	/* Type Offset=1480 */

	/* Parameter pdwCancelID */

/* 2010 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2012 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2014 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 2016 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2018 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2020 */	NdrFcShort( 0x66c ),	/* Type Offset=1644 */

	/* Return value */

/* 2022 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2024 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 2026 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure DeleteAtTime */

/* 2028 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2030 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2034 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2036 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 2038 */	NdrFcShort( 0x10 ),	/* 16 */
/* 2040 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2042 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwTransactionID */

/* 2044 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2046 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2048 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 2050 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2052 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2054 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2056 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2058 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2060 */	NdrFcShort( 0x782 ),	/* Type Offset=1922 */

	/* Parameter ftTimeStamps */

/* 2062 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2064 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2066 */	NdrFcShort( 0x770 ),	/* Type Offset=1904 */

	/* Parameter pdwCancelID */

/* 2068 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2070 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2072 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 2074 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2076 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2078 */	NdrFcShort( 0x7a2 ),	/* Type Offset=1954 */

	/* Return value */

/* 2080 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2082 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2084 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Cancel */

/* 2086 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2088 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2092 */	NdrFcShort( 0x9 ),	/* 9 */
/* 2094 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2096 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2098 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2100 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter dwCancelID */

/* 2102 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2104 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2106 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 2108 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2110 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2112 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Read */

/* 2114 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2116 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2120 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2122 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 2124 */	NdrFcShort( 0x10 ),	/* 16 */
/* 2126 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2128 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x8,		/* 8 */

	/* Parameter dwTransactionID */

/* 2130 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2132 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2134 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter htStartTime */

/* 2136 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 2138 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2140 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 2142 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 2144 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2146 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter dwNumItems */

/* 2148 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2150 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2152 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2154 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2156 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2158 */	NdrFcShort( 0x5c8 ),	/* Type Offset=1480 */

	/* Parameter pdwCancelID */

/* 2160 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2162 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2164 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 2166 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2168 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2170 */	NdrFcShort( 0x66c ),	/* Type Offset=1644 */

	/* Return value */

/* 2172 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2174 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 2176 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Insert */

/* 2178 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2180 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2184 */	NdrFcShort( 0x5 ),	/* 5 */
/* 2186 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 2188 */	NdrFcShort( 0x10 ),	/* 16 */
/* 2190 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2192 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x8,		/* 8 */

	/* Parameter dwTransactionID */

/* 2194 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2196 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2198 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 2200 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2202 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2204 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2206 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2208 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2210 */	NdrFcShort( 0x782 ),	/* Type Offset=1922 */

	/* Parameter ftTimeStamps */

/* 2212 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2214 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2216 */	NdrFcShort( 0x770 ),	/* Type Offset=1904 */

	/* Parameter pAnnotationValues */

/* 2218 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2220 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2222 */	NdrFcShort( 0x7ae ),	/* Type Offset=1966 */

	/* Parameter pdwCancelID */

/* 2224 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2226 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2228 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 2230 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2232 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2234 */	NdrFcShort( 0x7a2 ),	/* Type Offset=1954 */

	/* Return value */

/* 2236 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2238 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 2240 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Cancel */

/* 2242 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2244 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2248 */	NdrFcShort( 0x6 ),	/* 6 */
/* 2250 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2252 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2254 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2256 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter dwCancelID */

/* 2258 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2260 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2262 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 2264 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2266 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2268 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadRawWithUpdate */

/* 2270 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2272 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2276 */	NdrFcShort( 0x3 ),	/* 3 */
/* 2278 */	NdrFcShort( 0x38 ),	/* x86 Stack size/offset = 56 */
/* 2280 */	NdrFcShort( 0x48 ),	/* 72 */
/* 2282 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2284 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0xb,		/* 11 */

	/* Parameter dwTransactionID */

/* 2286 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2288 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2290 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter htStartTime */

/* 2292 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 2294 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2296 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 2298 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 2300 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2302 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter dwNumValues */

/* 2304 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2306 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2308 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ftUpdateDuration */

/* 2310 */	NdrFcShort( 0x8a ),	/* Flags:  must free, in, by val, */
/* 2312 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2314 */	NdrFcShort( 0x7e ),	/* Type Offset=126 */

	/* Parameter ftUpdateInterval */

/* 2316 */	NdrFcShort( 0x8a ),	/* Flags:  must free, in, by val, */
/* 2318 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2320 */	NdrFcShort( 0x7e ),	/* Type Offset=126 */

	/* Parameter dwNumItems */

/* 2322 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2324 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 2326 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2328 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2330 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 2332 */	NdrFcShort( 0x7ea ),	/* Type Offset=2026 */

	/* Parameter pdwCancelID */

/* 2334 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2336 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 2338 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 2340 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2342 */	NdrFcShort( 0x30 ),	/* x86 Stack size/offset = 48 */
/* 2344 */	NdrFcShort( 0x7f4 ),	/* Type Offset=2036 */

	/* Return value */

/* 2346 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2348 */	NdrFcShort( 0x34 ),	/* x86 Stack size/offset = 52 */
/* 2350 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadProcessedWithUpdate */

/* 2352 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2354 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2358 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2360 */	NdrFcShort( 0x3c ),	/* x86 Stack size/offset = 60 */
/* 2362 */	NdrFcShort( 0x48 ),	/* 72 */
/* 2364 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2366 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0xc,		/* 12 */

	/* Parameter dwTransactionID */

/* 2368 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2370 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2372 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter htStartTime */

/* 2374 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 2376 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2378 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter htEndTime */

/* 2380 */	NdrFcShort( 0x11b ),	/* Flags:  must size, must free, in, out, simple ref, */
/* 2382 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2384 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter ftResampleInterval */

/* 2386 */	NdrFcShort( 0x8a ),	/* Flags:  must free, in, by val, */
/* 2388 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2390 */	NdrFcShort( 0x7e ),	/* Type Offset=126 */

	/* Parameter dwNumIntervals */

/* 2392 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2394 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2396 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ftUpdateInterval */

/* 2398 */	NdrFcShort( 0x8a ),	/* Flags:  must free, in, by val, */
/* 2400 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2402 */	NdrFcShort( 0x7e ),	/* Type Offset=126 */

	/* Parameter dwNumItems */

/* 2404 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2406 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 2408 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2410 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2412 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 2414 */	NdrFcShort( 0x7ea ),	/* Type Offset=2026 */

	/* Parameter haAggregate */

/* 2416 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2418 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 2420 */	NdrFcShort( 0x7ea ),	/* Type Offset=2026 */

	/* Parameter pdwCancelID */

/* 2422 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2424 */	NdrFcShort( 0x30 ),	/* x86 Stack size/offset = 48 */
/* 2426 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 2428 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2430 */	NdrFcShort( 0x34 ),	/* x86 Stack size/offset = 52 */
/* 2432 */	NdrFcShort( 0x7f4 ),	/* Type Offset=2036 */

	/* Return value */

/* 2434 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2436 */	NdrFcShort( 0x38 ),	/* x86 Stack size/offset = 56 */
/* 2438 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Cancel */

/* 2440 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2442 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2446 */	NdrFcShort( 0x5 ),	/* 5 */
/* 2448 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2450 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2452 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2454 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter dwCancelID */

/* 2456 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2458 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2460 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 2462 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2464 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2466 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnDataChange */

/* 2468 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2470 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2474 */	NdrFcShort( 0x3 ),	/* 3 */
/* 2476 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2478 */	NdrFcShort( 0x18 ),	/* 24 */
/* 2480 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2482 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwTransactionID */

/* 2484 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2486 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2488 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrStatus */

/* 2490 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2492 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2494 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 2496 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2498 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2500 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pItemValues */

/* 2502 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2504 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2506 */	NdrFcShort( 0x5aa ),	/* Type Offset=1450 */

	/* Parameter phrErrors */

/* 2508 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2510 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2512 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Return value */

/* 2514 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2516 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2518 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnReadComplete */

/* 2520 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2522 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2526 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2528 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2530 */	NdrFcShort( 0x18 ),	/* 24 */
/* 2532 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2534 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwTransactionID */

/* 2536 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2538 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2540 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrStatus */

/* 2542 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2544 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2546 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 2548 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2550 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2552 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pItemValues */

/* 2554 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2556 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2558 */	NdrFcShort( 0x5aa ),	/* Type Offset=1450 */

	/* Parameter phrErrors */

/* 2560 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2562 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2564 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Return value */

/* 2566 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2568 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2570 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnReadModifiedComplete */

/* 2572 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2574 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2578 */	NdrFcShort( 0x5 ),	/* 5 */
/* 2580 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2582 */	NdrFcShort( 0x18 ),	/* 24 */
/* 2584 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2586 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwTransactionID */

/* 2588 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2590 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2592 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrStatus */

/* 2594 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2596 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2598 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 2600 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2602 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2604 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pItemValues */

/* 2606 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2608 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2610 */	NdrFcShort( 0x804 ),	/* Type Offset=2052 */

	/* Parameter phrErrors */

/* 2612 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2614 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2616 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Return value */

/* 2618 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2620 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2622 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnReadAttributeComplete */

/* 2624 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2626 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2630 */	NdrFcShort( 0x6 ),	/* 6 */
/* 2632 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 2634 */	NdrFcShort( 0x20 ),	/* 32 */
/* 2636 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2638 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwTransactionID */

/* 2640 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2642 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2644 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrStatus */

/* 2646 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2648 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2650 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hClient */

/* 2652 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2654 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2656 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 2658 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2660 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2662 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pAttributeValues */

/* 2664 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2666 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2668 */	NdrFcShort( 0x692 ),	/* Type Offset=1682 */

	/* Parameter phrErrors */

/* 2670 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2672 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2674 */	NdrFcShort( 0x5c8 ),	/* Type Offset=1480 */

	/* Return value */

/* 2676 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2678 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2680 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnReadAnnotations */

/* 2682 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2684 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2688 */	NdrFcShort( 0x7 ),	/* 7 */
/* 2690 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2692 */	NdrFcShort( 0x18 ),	/* 24 */
/* 2694 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2696 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwTransactionID */

/* 2698 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2700 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2702 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrStatus */

/* 2704 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2706 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2708 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 2710 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2712 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2714 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pAnnotationValues */

/* 2716 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2718 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2720 */	NdrFcShort( 0x81e ),	/* Type Offset=2078 */

	/* Parameter phrErrors */

/* 2722 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2724 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2726 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Return value */

/* 2728 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2730 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2732 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnInsertAnnotations */

/* 2734 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2736 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2740 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2742 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2744 */	NdrFcShort( 0x18 ),	/* 24 */
/* 2746 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2748 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwTransactionID */

/* 2750 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2752 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2754 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrStatus */

/* 2756 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2758 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2760 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 2762 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2764 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2766 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phClients */

/* 2768 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2770 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2772 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Parameter phrErrors */

/* 2774 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2776 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2778 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Return value */

/* 2780 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2782 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2784 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnPlayback */

/* 2786 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2788 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2792 */	NdrFcShort( 0x9 ),	/* 9 */
/* 2794 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2796 */	NdrFcShort( 0x18 ),	/* 24 */
/* 2798 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2800 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwTransactionID */

/* 2802 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2804 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2806 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrStatus */

/* 2808 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2810 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2812 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwNumItems */

/* 2814 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2816 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2818 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppItemValues */

/* 2820 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2822 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2824 */	NdrFcShort( 0x85a ),	/* Type Offset=2138 */

	/* Parameter phrErrors */

/* 2826 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2828 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2830 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Return value */

/* 2832 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2834 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2836 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnUpdateComplete */

/* 2838 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2840 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2844 */	NdrFcShort( 0xa ),	/* 10 */
/* 2846 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2848 */	NdrFcShort( 0x18 ),	/* 24 */
/* 2850 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2852 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwTransactionID */

/* 2854 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2856 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2858 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrStatus */

/* 2860 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2862 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2864 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 2866 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2868 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2870 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phClients */

/* 2872 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2874 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2876 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Parameter phrErrors */

/* 2878 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2880 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2882 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Return value */

/* 2884 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2886 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2888 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnCancelComplete */

/* 2890 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2892 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2896 */	NdrFcShort( 0xb ),	/* 11 */
/* 2898 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2900 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2902 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2904 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter dwCancelID */

/* 2906 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2908 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2910 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 2912 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2914 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2916 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

			0x0
        }
    };

static const opchda_MIDL_TYPE_FORMAT_STRING opchda__MIDL_TypeFormatString =
    {
        0,
        {
			NdrFcShort( 0x0 ),	/* 0 */
/*  2 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/*  4 */	NdrFcShort( 0x2 ),	/* Offset= 2 (6) */
/*  6 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/*  8 */	NdrFcLong( 0x101 ),	/* 257 */
/* 12 */	NdrFcShort( 0x0 ),	/* 0 */
/* 14 */	NdrFcShort( 0x0 ),	/* 0 */
/* 16 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 18 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 20 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 22 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 24 */	
			0x11, 0x8,	/* FC_RP [simple_pointer] */
/* 26 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 28 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 30 */	NdrFcShort( 0x2 ),	/* Offset= 2 (32) */
/* 32 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 34 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 36 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 38 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 40 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 42 */	NdrFcShort( 0x2 ),	/* Offset= 2 (44) */
/* 44 */	
			0x13, 0x0,	/* FC_OP */
/* 46 */	NdrFcShort( 0x2 ),	/* Offset= 2 (48) */
/* 48 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 50 */	NdrFcShort( 0x4 ),	/* 4 */
/* 52 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 54 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 56 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 58 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 60 */	NdrFcShort( 0x2 ),	/* Offset= 2 (62) */
/* 62 */	
			0x13, 0x0,	/* FC_OP */
/* 64 */	NdrFcShort( 0x2 ),	/* Offset= 2 (66) */
/* 66 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 68 */	NdrFcShort( 0x4 ),	/* 4 */
/* 70 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 72 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 74 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 76 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 78 */	NdrFcShort( 0x4 ),	/* 4 */
/* 80 */	NdrFcShort( 0x0 ),	/* 0 */
/* 82 */	NdrFcShort( 0x1 ),	/* 1 */
/* 84 */	NdrFcShort( 0x0 ),	/* 0 */
/* 86 */	NdrFcShort( 0x0 ),	/* 0 */
/* 88 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 90 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 92 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 94 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 96 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 98 */	NdrFcShort( 0x2 ),	/* Offset= 2 (100) */
/* 100 */	
			0x13, 0x0,	/* FC_OP */
/* 102 */	NdrFcShort( 0x2 ),	/* Offset= 2 (104) */
/* 104 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 106 */	NdrFcShort( 0x2 ),	/* 2 */
/* 108 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 110 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 112 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 114 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 116 */	0xd,		/* FC_ENUM16 */
			0x5c,		/* FC_PAD */
/* 118 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 120 */	NdrFcShort( 0x2 ),	/* Offset= 2 (122) */
/* 122 */	
			0x13, 0x0,	/* FC_OP */
/* 124 */	NdrFcShort( 0x2 ),	/* Offset= 2 (126) */
/* 126 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 128 */	NdrFcShort( 0x8 ),	/* 8 */
/* 130 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 132 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 134 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 136 */	0x6,		/* FC_SHORT */
			0x5c,		/* FC_PAD */
/* 138 */	
			0x11, 0x0,	/* FC_RP */
/* 140 */	NdrFcShort( 0x2 ),	/* Offset= 2 (142) */
/* 142 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 144 */	NdrFcShort( 0x4 ),	/* 4 */
/* 146 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 148 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 150 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 152 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 154 */	NdrFcShort( 0x4 ),	/* 4 */
/* 156 */	NdrFcShort( 0x0 ),	/* 0 */
/* 158 */	NdrFcShort( 0x1 ),	/* 1 */
/* 160 */	NdrFcShort( 0x0 ),	/* 0 */
/* 162 */	NdrFcShort( 0x0 ),	/* 0 */
/* 164 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 166 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 168 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 170 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 172 */	
			0x11, 0x0,	/* FC_RP */
/* 174 */	NdrFcShort( 0x2 ),	/* Offset= 2 (176) */
/* 176 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 178 */	NdrFcShort( 0x4 ),	/* 4 */
/* 180 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 182 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 184 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 186 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 188 */	NdrFcShort( 0x2 ),	/* Offset= 2 (190) */
/* 190 */	
			0x13, 0x0,	/* FC_OP */
/* 192 */	NdrFcShort( 0xfff0 ),	/* Offset= -16 (176) */
/* 194 */	
			0x11, 0x0,	/* FC_RP */
/* 196 */	NdrFcShort( 0x2 ),	/* Offset= 2 (198) */
/* 198 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x1,		/* 1 */
/* 200 */	NdrFcShort( 0x0 ),	/* 0 */
/* 202 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 204 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 206 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 210 */	0xd,		/* FC_ENUM16 */
			0x5b,		/* FC_END */
/* 212 */	
			0x11, 0x0,	/* FC_RP */
/* 214 */	NdrFcShort( 0x3e4 ),	/* Offset= 996 (1210) */
/* 216 */	
			0x12, 0x0,	/* FC_UP */
/* 218 */	NdrFcShort( 0x3c2 ),	/* Offset= 962 (1180) */
/* 220 */	
			0x2b,		/* FC_NON_ENCAPSULATED_UNION */
			0x9,		/* FC_ULONG */
/* 222 */	0x7,		/* Corr desc: FC_USHORT */
			0x0,		/*  */
/* 224 */	NdrFcShort( 0xfff8 ),	/* -8 */
/* 226 */	NdrFcShort( 0x2 ),	/* Offset= 2 (228) */
/* 228 */	NdrFcShort( 0x10 ),	/* 16 */
/* 230 */	NdrFcShort( 0x2f ),	/* 47 */
/* 232 */	NdrFcLong( 0x14 ),	/* 20 */
/* 236 */	NdrFcShort( 0x800b ),	/* Simple arm type: FC_HYPER */
/* 238 */	NdrFcLong( 0x3 ),	/* 3 */
/* 242 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 244 */	NdrFcLong( 0x11 ),	/* 17 */
/* 248 */	NdrFcShort( 0x8001 ),	/* Simple arm type: FC_BYTE */
/* 250 */	NdrFcLong( 0x2 ),	/* 2 */
/* 254 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 256 */	NdrFcLong( 0x4 ),	/* 4 */
/* 260 */	NdrFcShort( 0x800a ),	/* Simple arm type: FC_FLOAT */
/* 262 */	NdrFcLong( 0x5 ),	/* 5 */
/* 266 */	NdrFcShort( 0x800c ),	/* Simple arm type: FC_DOUBLE */
/* 268 */	NdrFcLong( 0xb ),	/* 11 */
/* 272 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 274 */	NdrFcLong( 0xa ),	/* 10 */
/* 278 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 280 */	NdrFcLong( 0x6 ),	/* 6 */
/* 284 */	NdrFcShort( 0xe8 ),	/* Offset= 232 (516) */
/* 286 */	NdrFcLong( 0x7 ),	/* 7 */
/* 290 */	NdrFcShort( 0x800c ),	/* Simple arm type: FC_DOUBLE */
/* 292 */	NdrFcLong( 0x8 ),	/* 8 */
/* 296 */	NdrFcShort( 0xe2 ),	/* Offset= 226 (522) */
/* 298 */	NdrFcLong( 0xd ),	/* 13 */
/* 302 */	NdrFcShort( 0xf4 ),	/* Offset= 244 (546) */
/* 304 */	NdrFcLong( 0x9 ),	/* 9 */
/* 308 */	NdrFcShort( 0x100 ),	/* Offset= 256 (564) */
/* 310 */	NdrFcLong( 0x2000 ),	/* 8192 */
/* 314 */	NdrFcShort( 0x10c ),	/* Offset= 268 (582) */
/* 316 */	NdrFcLong( 0x24 ),	/* 36 */
/* 320 */	NdrFcShort( 0x312 ),	/* Offset= 786 (1106) */
/* 322 */	NdrFcLong( 0x4024 ),	/* 16420 */
/* 326 */	NdrFcShort( 0x30c ),	/* Offset= 780 (1106) */
/* 328 */	NdrFcLong( 0x4011 ),	/* 16401 */
/* 332 */	NdrFcShort( 0x30a ),	/* Offset= 778 (1110) */
/* 334 */	NdrFcLong( 0x4002 ),	/* 16386 */
/* 338 */	NdrFcShort( 0x308 ),	/* Offset= 776 (1114) */
/* 340 */	NdrFcLong( 0x4003 ),	/* 16387 */
/* 344 */	NdrFcShort( 0x306 ),	/* Offset= 774 (1118) */
/* 346 */	NdrFcLong( 0x4014 ),	/* 16404 */
/* 350 */	NdrFcShort( 0x304 ),	/* Offset= 772 (1122) */
/* 352 */	NdrFcLong( 0x4004 ),	/* 16388 */
/* 356 */	NdrFcShort( 0x302 ),	/* Offset= 770 (1126) */
/* 358 */	NdrFcLong( 0x4005 ),	/* 16389 */
/* 362 */	NdrFcShort( 0x300 ),	/* Offset= 768 (1130) */
/* 364 */	NdrFcLong( 0x400b ),	/* 16395 */
/* 368 */	NdrFcShort( 0x2ea ),	/* Offset= 746 (1114) */
/* 370 */	NdrFcLong( 0x400a ),	/* 16394 */
/* 374 */	NdrFcShort( 0x2e8 ),	/* Offset= 744 (1118) */
/* 376 */	NdrFcLong( 0x4006 ),	/* 16390 */
/* 380 */	NdrFcShort( 0x2f2 ),	/* Offset= 754 (1134) */
/* 382 */	NdrFcLong( 0x4007 ),	/* 16391 */
/* 386 */	NdrFcShort( 0x2e8 ),	/* Offset= 744 (1130) */
/* 388 */	NdrFcLong( 0x4008 ),	/* 16392 */
/* 392 */	NdrFcShort( 0x2ea ),	/* Offset= 746 (1138) */
/* 394 */	NdrFcLong( 0x400d ),	/* 16397 */
/* 398 */	NdrFcShort( 0x2e8 ),	/* Offset= 744 (1142) */
/* 400 */	NdrFcLong( 0x4009 ),	/* 16393 */
/* 404 */	NdrFcShort( 0x2e6 ),	/* Offset= 742 (1146) */
/* 406 */	NdrFcLong( 0x6000 ),	/* 24576 */
/* 410 */	NdrFcShort( 0x2e4 ),	/* Offset= 740 (1150) */
/* 412 */	NdrFcLong( 0x400c ),	/* 16396 */
/* 416 */	NdrFcShort( 0x2e2 ),	/* Offset= 738 (1154) */
/* 418 */	NdrFcLong( 0x10 ),	/* 16 */
/* 422 */	NdrFcShort( 0x8002 ),	/* Simple arm type: FC_CHAR */
/* 424 */	NdrFcLong( 0x12 ),	/* 18 */
/* 428 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 430 */	NdrFcLong( 0x13 ),	/* 19 */
/* 434 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 436 */	NdrFcLong( 0x15 ),	/* 21 */
/* 440 */	NdrFcShort( 0x800b ),	/* Simple arm type: FC_HYPER */
/* 442 */	NdrFcLong( 0x16 ),	/* 22 */
/* 446 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 448 */	NdrFcLong( 0x17 ),	/* 23 */
/* 452 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 454 */	NdrFcLong( 0xe ),	/* 14 */
/* 458 */	NdrFcShort( 0x2c0 ),	/* Offset= 704 (1162) */
/* 460 */	NdrFcLong( 0x400e ),	/* 16398 */
/* 464 */	NdrFcShort( 0x2c4 ),	/* Offset= 708 (1172) */
/* 466 */	NdrFcLong( 0x4010 ),	/* 16400 */
/* 470 */	NdrFcShort( 0x2c2 ),	/* Offset= 706 (1176) */
/* 472 */	NdrFcLong( 0x4012 ),	/* 16402 */
/* 476 */	NdrFcShort( 0x27e ),	/* Offset= 638 (1114) */
/* 478 */	NdrFcLong( 0x4013 ),	/* 16403 */
/* 482 */	NdrFcShort( 0x27c ),	/* Offset= 636 (1118) */
/* 484 */	NdrFcLong( 0x4015 ),	/* 16405 */
/* 488 */	NdrFcShort( 0x27a ),	/* Offset= 634 (1122) */
/* 490 */	NdrFcLong( 0x4016 ),	/* 16406 */
/* 494 */	NdrFcShort( 0x270 ),	/* Offset= 624 (1118) */
/* 496 */	NdrFcLong( 0x4017 ),	/* 16407 */
/* 500 */	NdrFcShort( 0x26a ),	/* Offset= 618 (1118) */
/* 502 */	NdrFcLong( 0x0 ),	/* 0 */
/* 506 */	NdrFcShort( 0x0 ),	/* Offset= 0 (506) */
/* 508 */	NdrFcLong( 0x1 ),	/* 1 */
/* 512 */	NdrFcShort( 0x0 ),	/* Offset= 0 (512) */
/* 514 */	NdrFcShort( 0xffff ),	/* Offset= -1 (513) */
/* 516 */	
			0x15,		/* FC_STRUCT */
			0x7,		/* 7 */
/* 518 */	NdrFcShort( 0x8 ),	/* 8 */
/* 520 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 522 */	
			0x12, 0x0,	/* FC_UP */
/* 524 */	NdrFcShort( 0xc ),	/* Offset= 12 (536) */
/* 526 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 528 */	NdrFcShort( 0x2 ),	/* 2 */
/* 530 */	0x9,		/* Corr desc: FC_ULONG */
			0x0,		/*  */
/* 532 */	NdrFcShort( 0xfffc ),	/* -4 */
/* 534 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 536 */	
			0x17,		/* FC_CSTRUCT */
			0x3,		/* 3 */
/* 538 */	NdrFcShort( 0x8 ),	/* 8 */
/* 540 */	NdrFcShort( 0xfff2 ),	/* Offset= -14 (526) */
/* 542 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 544 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 546 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 548 */	NdrFcLong( 0x0 ),	/* 0 */
/* 552 */	NdrFcShort( 0x0 ),	/* 0 */
/* 554 */	NdrFcShort( 0x0 ),	/* 0 */
/* 556 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 558 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 560 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 562 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 564 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 566 */	NdrFcLong( 0x20400 ),	/* 132096 */
/* 570 */	NdrFcShort( 0x0 ),	/* 0 */
/* 572 */	NdrFcShort( 0x0 ),	/* 0 */
/* 574 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 576 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 578 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 580 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 582 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 584 */	NdrFcShort( 0x2 ),	/* Offset= 2 (586) */
/* 586 */	
			0x12, 0x0,	/* FC_UP */
/* 588 */	NdrFcShort( 0x1f4 ),	/* Offset= 500 (1088) */
/* 590 */	
			0x2a,		/* FC_ENCAPSULATED_UNION */
			0x49,		/* 73 */
/* 592 */	NdrFcShort( 0x18 ),	/* 24 */
/* 594 */	NdrFcShort( 0xa ),	/* 10 */
/* 596 */	NdrFcLong( 0x8 ),	/* 8 */
/* 600 */	NdrFcShort( 0x58 ),	/* Offset= 88 (688) */
/* 602 */	NdrFcLong( 0xd ),	/* 13 */
/* 606 */	NdrFcShort( 0x78 ),	/* Offset= 120 (726) */
/* 608 */	NdrFcLong( 0x9 ),	/* 9 */
/* 612 */	NdrFcShort( 0x94 ),	/* Offset= 148 (760) */
/* 614 */	NdrFcLong( 0xc ),	/* 12 */
/* 618 */	NdrFcShort( 0xbc ),	/* Offset= 188 (806) */
/* 620 */	NdrFcLong( 0x24 ),	/* 36 */
/* 624 */	NdrFcShort( 0x114 ),	/* Offset= 276 (900) */
/* 626 */	NdrFcLong( 0x800d ),	/* 32781 */
/* 630 */	NdrFcShort( 0x130 ),	/* Offset= 304 (934) */
/* 632 */	NdrFcLong( 0x10 ),	/* 16 */
/* 636 */	NdrFcShort( 0x148 ),	/* Offset= 328 (964) */
/* 638 */	NdrFcLong( 0x2 ),	/* 2 */
/* 642 */	NdrFcShort( 0x160 ),	/* Offset= 352 (994) */
/* 644 */	NdrFcLong( 0x3 ),	/* 3 */
/* 648 */	NdrFcShort( 0x178 ),	/* Offset= 376 (1024) */
/* 650 */	NdrFcLong( 0x14 ),	/* 20 */
/* 654 */	NdrFcShort( 0x190 ),	/* Offset= 400 (1054) */
/* 656 */	NdrFcShort( 0xffff ),	/* Offset= -1 (655) */
/* 658 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 660 */	NdrFcShort( 0x4 ),	/* 4 */
/* 662 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 664 */	NdrFcShort( 0x0 ),	/* 0 */
/* 666 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 668 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 670 */	NdrFcShort( 0x4 ),	/* 4 */
/* 672 */	NdrFcShort( 0x0 ),	/* 0 */
/* 674 */	NdrFcShort( 0x1 ),	/* 1 */
/* 676 */	NdrFcShort( 0x0 ),	/* 0 */
/* 678 */	NdrFcShort( 0x0 ),	/* 0 */
/* 680 */	0x12, 0x0,	/* FC_UP */
/* 682 */	NdrFcShort( 0xff6e ),	/* Offset= -146 (536) */
/* 684 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 686 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 688 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 690 */	NdrFcShort( 0x8 ),	/* 8 */
/* 692 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 694 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 696 */	NdrFcShort( 0x4 ),	/* 4 */
/* 698 */	NdrFcShort( 0x4 ),	/* 4 */
/* 700 */	0x11, 0x0,	/* FC_RP */
/* 702 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (658) */
/* 704 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 706 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 708 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 710 */	NdrFcShort( 0x0 ),	/* 0 */
/* 712 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 714 */	NdrFcShort( 0x0 ),	/* 0 */
/* 716 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 720 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 722 */	NdrFcShort( 0xff50 ),	/* Offset= -176 (546) */
/* 724 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 726 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 728 */	NdrFcShort( 0x8 ),	/* 8 */
/* 730 */	NdrFcShort( 0x0 ),	/* 0 */
/* 732 */	NdrFcShort( 0x6 ),	/* Offset= 6 (738) */
/* 734 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 736 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 738 */	
			0x11, 0x0,	/* FC_RP */
/* 740 */	NdrFcShort( 0xffe0 ),	/* Offset= -32 (708) */
/* 742 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 744 */	NdrFcShort( 0x0 ),	/* 0 */
/* 746 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 748 */	NdrFcShort( 0x0 ),	/* 0 */
/* 750 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 754 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 756 */	NdrFcShort( 0xff40 ),	/* Offset= -192 (564) */
/* 758 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 760 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 762 */	NdrFcShort( 0x8 ),	/* 8 */
/* 764 */	NdrFcShort( 0x0 ),	/* 0 */
/* 766 */	NdrFcShort( 0x6 ),	/* Offset= 6 (772) */
/* 768 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 770 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 772 */	
			0x11, 0x0,	/* FC_RP */
/* 774 */	NdrFcShort( 0xffe0 ),	/* Offset= -32 (742) */
/* 776 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 778 */	NdrFcShort( 0x4 ),	/* 4 */
/* 780 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 782 */	NdrFcShort( 0x0 ),	/* 0 */
/* 784 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 786 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 788 */	NdrFcShort( 0x4 ),	/* 4 */
/* 790 */	NdrFcShort( 0x0 ),	/* 0 */
/* 792 */	NdrFcShort( 0x1 ),	/* 1 */
/* 794 */	NdrFcShort( 0x0 ),	/* 0 */
/* 796 */	NdrFcShort( 0x0 ),	/* 0 */
/* 798 */	0x12, 0x0,	/* FC_UP */
/* 800 */	NdrFcShort( 0x17c ),	/* Offset= 380 (1180) */
/* 802 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 804 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 806 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 808 */	NdrFcShort( 0x8 ),	/* 8 */
/* 810 */	NdrFcShort( 0x0 ),	/* 0 */
/* 812 */	NdrFcShort( 0x6 ),	/* Offset= 6 (818) */
/* 814 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 816 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 818 */	
			0x11, 0x0,	/* FC_RP */
/* 820 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (776) */
/* 822 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 824 */	NdrFcLong( 0x2f ),	/* 47 */
/* 828 */	NdrFcShort( 0x0 ),	/* 0 */
/* 830 */	NdrFcShort( 0x0 ),	/* 0 */
/* 832 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 834 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 836 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 838 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 840 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 842 */	NdrFcShort( 0x1 ),	/* 1 */
/* 844 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 846 */	NdrFcShort( 0x4 ),	/* 4 */
/* 848 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 850 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 852 */	NdrFcShort( 0x10 ),	/* 16 */
/* 854 */	NdrFcShort( 0x0 ),	/* 0 */
/* 856 */	NdrFcShort( 0xa ),	/* Offset= 10 (866) */
/* 858 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 860 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 862 */	NdrFcShort( 0xffd8 ),	/* Offset= -40 (822) */
/* 864 */	0x36,		/* FC_POINTER */
			0x5b,		/* FC_END */
/* 866 */	
			0x12, 0x0,	/* FC_UP */
/* 868 */	NdrFcShort( 0xffe4 ),	/* Offset= -28 (840) */
/* 870 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 872 */	NdrFcShort( 0x4 ),	/* 4 */
/* 874 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 876 */	NdrFcShort( 0x0 ),	/* 0 */
/* 878 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 880 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 882 */	NdrFcShort( 0x4 ),	/* 4 */
/* 884 */	NdrFcShort( 0x0 ),	/* 0 */
/* 886 */	NdrFcShort( 0x1 ),	/* 1 */
/* 888 */	NdrFcShort( 0x0 ),	/* 0 */
/* 890 */	NdrFcShort( 0x0 ),	/* 0 */
/* 892 */	0x12, 0x0,	/* FC_UP */
/* 894 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (850) */
/* 896 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 898 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 900 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 902 */	NdrFcShort( 0x8 ),	/* 8 */
/* 904 */	NdrFcShort( 0x0 ),	/* 0 */
/* 906 */	NdrFcShort( 0x6 ),	/* Offset= 6 (912) */
/* 908 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 910 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 912 */	
			0x11, 0x0,	/* FC_RP */
/* 914 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (870) */
/* 916 */	
			0x1d,		/* FC_SMFARRAY */
			0x0,		/* 0 */
/* 918 */	NdrFcShort( 0x8 ),	/* 8 */
/* 920 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 922 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 924 */	NdrFcShort( 0x10 ),	/* 16 */
/* 926 */	0x8,		/* FC_LONG */
			0x6,		/* FC_SHORT */
/* 928 */	0x6,		/* FC_SHORT */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 930 */	0x0,		/* 0 */
			NdrFcShort( 0xfff1 ),	/* Offset= -15 (916) */
			0x5b,		/* FC_END */
/* 934 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 936 */	NdrFcShort( 0x18 ),	/* 24 */
/* 938 */	NdrFcShort( 0x0 ),	/* 0 */
/* 940 */	NdrFcShort( 0xa ),	/* Offset= 10 (950) */
/* 942 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 944 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 946 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (922) */
/* 948 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 950 */	
			0x11, 0x0,	/* FC_RP */
/* 952 */	NdrFcShort( 0xff0c ),	/* Offset= -244 (708) */
/* 954 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 956 */	NdrFcShort( 0x1 ),	/* 1 */
/* 958 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 960 */	NdrFcShort( 0x0 ),	/* 0 */
/* 962 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 964 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 966 */	NdrFcShort( 0x8 ),	/* 8 */
/* 968 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 970 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 972 */	NdrFcShort( 0x4 ),	/* 4 */
/* 974 */	NdrFcShort( 0x4 ),	/* 4 */
/* 976 */	0x12, 0x0,	/* FC_UP */
/* 978 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (954) */
/* 980 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 982 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 984 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 986 */	NdrFcShort( 0x2 ),	/* 2 */
/* 988 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 990 */	NdrFcShort( 0x0 ),	/* 0 */
/* 992 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 994 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 996 */	NdrFcShort( 0x8 ),	/* 8 */
/* 998 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1000 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1002 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1004 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1006 */	0x12, 0x0,	/* FC_UP */
/* 1008 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (984) */
/* 1010 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1012 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1014 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1016 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1018 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1020 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1022 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1024 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1026 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1028 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1030 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1032 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1034 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1036 */	0x12, 0x0,	/* FC_UP */
/* 1038 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1014) */
/* 1040 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1042 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1044 */	
			0x1b,		/* FC_CARRAY */
			0x7,		/* 7 */
/* 1046 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1048 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1050 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1052 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 1054 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1056 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1058 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1060 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1062 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1064 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1066 */	0x12, 0x0,	/* FC_UP */
/* 1068 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1044) */
/* 1070 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1072 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1074 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1076 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1078 */	0x7,		/* Corr desc: FC_USHORT */
			0x0,		/*  */
/* 1080 */	NdrFcShort( 0xffd8 ),	/* -40 */
/* 1082 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1084 */	NdrFcShort( 0xfc42 ),	/* Offset= -958 (126) */
/* 1086 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1088 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1090 */	NdrFcShort( 0x28 ),	/* 40 */
/* 1092 */	NdrFcShort( 0xffee ),	/* Offset= -18 (1074) */
/* 1094 */	NdrFcShort( 0x0 ),	/* Offset= 0 (1094) */
/* 1096 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1098 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1100 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1102 */	NdrFcShort( 0xfe00 ),	/* Offset= -512 (590) */
/* 1104 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1106 */	
			0x12, 0x0,	/* FC_UP */
/* 1108 */	NdrFcShort( 0xfefe ),	/* Offset= -258 (850) */
/* 1110 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1112 */	0x1,		/* FC_BYTE */
			0x5c,		/* FC_PAD */
/* 1114 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1116 */	0x6,		/* FC_SHORT */
			0x5c,		/* FC_PAD */
/* 1118 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1120 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 1122 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1124 */	0xb,		/* FC_HYPER */
			0x5c,		/* FC_PAD */
/* 1126 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1128 */	0xa,		/* FC_FLOAT */
			0x5c,		/* FC_PAD */
/* 1130 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1132 */	0xc,		/* FC_DOUBLE */
			0x5c,		/* FC_PAD */
/* 1134 */	
			0x12, 0x0,	/* FC_UP */
/* 1136 */	NdrFcShort( 0xfd94 ),	/* Offset= -620 (516) */
/* 1138 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 1140 */	NdrFcShort( 0xfd96 ),	/* Offset= -618 (522) */
/* 1142 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 1144 */	NdrFcShort( 0xfdaa ),	/* Offset= -598 (546) */
/* 1146 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 1148 */	NdrFcShort( 0xfdb8 ),	/* Offset= -584 (564) */
/* 1150 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 1152 */	NdrFcShort( 0xfdc6 ),	/* Offset= -570 (582) */
/* 1154 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 1156 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1158) */
/* 1158 */	
			0x12, 0x0,	/* FC_UP */
/* 1160 */	NdrFcShort( 0x14 ),	/* Offset= 20 (1180) */
/* 1162 */	
			0x15,		/* FC_STRUCT */
			0x7,		/* 7 */
/* 1164 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1166 */	0x6,		/* FC_SHORT */
			0x1,		/* FC_BYTE */
/* 1168 */	0x1,		/* FC_BYTE */
			0x8,		/* FC_LONG */
/* 1170 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 1172 */	
			0x12, 0x0,	/* FC_UP */
/* 1174 */	NdrFcShort( 0xfff4 ),	/* Offset= -12 (1162) */
/* 1176 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1178 */	0x2,		/* FC_CHAR */
			0x5c,		/* FC_PAD */
/* 1180 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x7,		/* 7 */
/* 1182 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1184 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1186 */	NdrFcShort( 0x0 ),	/* Offset= 0 (1186) */
/* 1188 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1190 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1192 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1194 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1196 */	NdrFcShort( 0xfc30 ),	/* Offset= -976 (220) */
/* 1198 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1200 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1202 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1204 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1206 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1208 */	NdrFcShort( 0xfc20 ),	/* Offset= -992 (216) */
/* 1210 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1212 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1214 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1216 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1218 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1222 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1224 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1200) */
/* 1226 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1228 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 1230 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1232) */
/* 1232 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 1234 */	NdrFcLong( 0x1f1217b1 ),	/* 521279409 */
/* 1238 */	NdrFcShort( 0xdee0 ),	/* -8480 */
/* 1240 */	NdrFcShort( 0x11d2 ),	/* 4562 */
/* 1242 */	0xa5,		/* 165 */
			0xe5,		/* 229 */
/* 1244 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 1246 */	0x86,		/* 134 */
			0x33,		/* 51 */
/* 1248 */	0x93,		/* 147 */
			0x99,		/* 153 */
/* 1250 */	
			0x11, 0x0,	/* FC_RP */
/* 1252 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1254) */
/* 1254 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1256 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1258 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1260 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1262 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1264 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1266 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1268 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1270 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1272 */	0x8,		/* FC_LONG */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1274 */	0x0,		/* 0 */
			NdrFcShort( 0xfb83 ),	/* Offset= -1149 (126) */
			0x5b,		/* FC_END */
/* 1278 */	
			0x11, 0x0,	/* FC_RP */
/* 1280 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1282) */
/* 1282 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1284 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1286 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1288 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1290 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1292 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1294 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1296) */
/* 1296 */	
			0x13, 0x0,	/* FC_OP */
/* 1298 */	NdrFcShort( 0x56 ),	/* Offset= 86 (1384) */
/* 1300 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1302 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1304 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1306 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1308 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1310 */	NdrFcShort( 0xfb60 ),	/* Offset= -1184 (126) */
/* 1312 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1314 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1316 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1318 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1320 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1322 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1324 */	
			0x13, 0x0,	/* FC_OP */
/* 1326 */	NdrFcShort( 0xff6e ),	/* Offset= -146 (1180) */
/* 1328 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1330 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1332 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1334 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1336 */	NdrFcShort( 0xfff4 ),	/* Offset= -12 (1324) */
/* 1338 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1340 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1342 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1344 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1346 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1350 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1352 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1328) */
/* 1354 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1356 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1358 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1360 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1362 */	NdrFcShort( 0xa ),	/* Offset= 10 (1372) */
/* 1364 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1366 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1368 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1370 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1372 */	
			0x13, 0x0,	/* FC_OP */
/* 1374 */	NdrFcShort( 0xffb6 ),	/* Offset= -74 (1300) */
/* 1376 */	
			0x13, 0x0,	/* FC_OP */
/* 1378 */	NdrFcShort( 0xffc0 ),	/* Offset= -64 (1314) */
/* 1380 */	
			0x13, 0x0,	/* FC_OP */
/* 1382 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (1338) */
/* 1384 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1386 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1388 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1390 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1392 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1396 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1398 */	NdrFcShort( 0xffd6 ),	/* Offset= -42 (1356) */
/* 1400 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1402 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1404 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1406) */
/* 1406 */	
			0x13, 0x0,	/* FC_OP */
/* 1408 */	NdrFcShort( 0xff82 ),	/* Offset= -126 (1282) */
/* 1410 */	
			0x11, 0x0,	/* FC_RP */
/* 1412 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1414) */
/* 1414 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1416 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1418 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1420 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1422 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1424 */	NdrFcShort( 0xfaee ),	/* Offset= -1298 (126) */
/* 1426 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1428 */	
			0x11, 0x0,	/* FC_RP */
/* 1430 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1432) */
/* 1432 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1434 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1436 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1438 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1440 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1442 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1444 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1446) */
/* 1446 */	
			0x13, 0x0,	/* FC_OP */
/* 1448 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1450) */
/* 1450 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1452 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1454 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1456 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1458 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1462 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1464 */	NdrFcShort( 0xff94 ),	/* Offset= -108 (1356) */
/* 1466 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1468 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1470 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1472) */
/* 1472 */	
			0x13, 0x0,	/* FC_OP */
/* 1474 */	NdrFcShort( 0xffd6 ),	/* Offset= -42 (1432) */
/* 1476 */	
			0x11, 0x0,	/* FC_RP */
/* 1478 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1480) */
/* 1480 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1482 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1484 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1486 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1488 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1490 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1492 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1494) */
/* 1494 */	
			0x13, 0x0,	/* FC_OP */
/* 1496 */	NdrFcShort( 0x82 ),	/* Offset= 130 (1626) */
/* 1498 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1500 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1502 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1504 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1506 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1508 */	NdrFcShort( 0xfa9a ),	/* Offset= -1382 (126) */
/* 1510 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1512 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1514 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1516 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1518 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1520 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1522 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1524 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1526 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1528 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1530 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1534 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1536 */	NdrFcShort( 0xff30 ),	/* Offset= -208 (1328) */
/* 1538 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1540 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x1,		/* 1 */
/* 1542 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1544 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1546 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1548 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1552 */	0xd,		/* FC_ENUM16 */
			0x5b,		/* FC_END */
/* 1554 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1556 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1558 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1560 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1562 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1564 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1566 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1568 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1570 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1572 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1574 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1576 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1578 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1580 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1582 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1584 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1586 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1588 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1590 */	NdrFcShort( 0xc ),	/* Offset= 12 (1602) */
/* 1592 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1594 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1596 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1598 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1600 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1602 */	
			0x13, 0x0,	/* FC_OP */
/* 1604 */	NdrFcShort( 0xff96 ),	/* Offset= -106 (1498) */
/* 1606 */	
			0x13, 0x0,	/* FC_OP */
/* 1608 */	NdrFcShort( 0xffa0 ),	/* Offset= -96 (1512) */
/* 1610 */	
			0x13, 0x0,	/* FC_OP */
/* 1612 */	NdrFcShort( 0xffa6 ),	/* Offset= -90 (1522) */
/* 1614 */	
			0x13, 0x0,	/* FC_OP */
/* 1616 */	NdrFcShort( 0xff8a ),	/* Offset= -118 (1498) */
/* 1618 */	
			0x13, 0x0,	/* FC_OP */
/* 1620 */	NdrFcShort( 0xffb0 ),	/* Offset= -80 (1540) */
/* 1622 */	
			0x13, 0x0,	/* FC_OP */
/* 1624 */	NdrFcShort( 0xffba ),	/* Offset= -70 (1554) */
/* 1626 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1628 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1630 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1632 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1634 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1638 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1640 */	NdrFcShort( 0xffc8 ),	/* Offset= -56 (1584) */
/* 1642 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1644 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1646 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1648) */
/* 1648 */	
			0x13, 0x0,	/* FC_OP */
/* 1650 */	NdrFcShort( 0xff56 ),	/* Offset= -170 (1480) */
/* 1652 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1654 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1656) */
/* 1656 */	
			0x13, 0x0,	/* FC_OP */
/* 1658 */	NdrFcShort( 0x18 ),	/* Offset= 24 (1682) */
/* 1660 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1662 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1664 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1666 */	NdrFcShort( 0x8 ),	/* Offset= 8 (1674) */
/* 1668 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1670 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1672 */	0x36,		/* FC_POINTER */
			0x5b,		/* FC_END */
/* 1674 */	
			0x13, 0x0,	/* FC_OP */
/* 1676 */	NdrFcShort( 0xff4e ),	/* Offset= -178 (1498) */
/* 1678 */	
			0x13, 0x0,	/* FC_OP */
/* 1680 */	NdrFcShort( 0xff62 ),	/* Offset= -158 (1522) */
/* 1682 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1684 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1686 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1688 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1690 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1694 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1696 */	NdrFcShort( 0xffdc ),	/* Offset= -36 (1660) */
/* 1698 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1700 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1702 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1704) */
/* 1704 */	
			0x13, 0x0,	/* FC_OP */
/* 1706 */	NdrFcShort( 0x38 ),	/* Offset= 56 (1762) */
/* 1708 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1710 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1712 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1714 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1716 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1718 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1720 */	0x13, 0x0,	/* FC_OP */
/* 1722 */	NdrFcShort( 0xff20 ),	/* Offset= -224 (1498) */
/* 1724 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1726 */	NdrFcShort( 0xc ),	/* 12 */
/* 1728 */	NdrFcShort( 0xc ),	/* 12 */
/* 1730 */	0x13, 0x0,	/* FC_OP */
/* 1732 */	NdrFcShort( 0xff4e ),	/* Offset= -178 (1554) */
/* 1734 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1736 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1738 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1740 */	0x13, 0x0,	/* FC_OP */
/* 1742 */	NdrFcShort( 0xff0c ),	/* Offset= -244 (1498) */
/* 1744 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1746 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1748 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1750 */	0x13, 0x0,	/* FC_OP */
/* 1752 */	NdrFcShort( 0xff3a ),	/* Offset= -198 (1554) */
/* 1754 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1756 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1758 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1760 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1762 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1764 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1766 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1768 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1770 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1772 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1774 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1776 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1778 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1780 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1782 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1784 */	0x13, 0x0,	/* FC_OP */
/* 1786 */	NdrFcShort( 0xfee0 ),	/* Offset= -288 (1498) */
/* 1788 */	NdrFcShort( 0xc ),	/* 12 */
/* 1790 */	NdrFcShort( 0xc ),	/* 12 */
/* 1792 */	0x13, 0x0,	/* FC_OP */
/* 1794 */	NdrFcShort( 0xff10 ),	/* Offset= -240 (1554) */
/* 1796 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1798 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1800 */	0x13, 0x0,	/* FC_OP */
/* 1802 */	NdrFcShort( 0xfed0 ),	/* Offset= -304 (1498) */
/* 1804 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1806 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1808 */	0x13, 0x0,	/* FC_OP */
/* 1810 */	NdrFcShort( 0xff00 ),	/* Offset= -256 (1554) */
/* 1812 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1814 */	0x0,		/* 0 */
			NdrFcShort( 0xff95 ),	/* Offset= -107 (1708) */
			0x5b,		/* FC_END */
/* 1818 */	
			0x11, 0x0,	/* FC_RP */
/* 1820 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1822) */
/* 1822 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1824 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1826 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1828 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1830 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1832 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1834 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1836 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1838 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1840 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1842 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1844 */	0x12, 0x0,	/* FC_UP */
/* 1846 */	NdrFcShort( 0xfea4 ),	/* Offset= -348 (1498) */
/* 1848 */	NdrFcShort( 0xc ),	/* 12 */
/* 1850 */	NdrFcShort( 0xc ),	/* 12 */
/* 1852 */	0x12, 0x0,	/* FC_UP */
/* 1854 */	NdrFcShort( 0xfed4 ),	/* Offset= -300 (1554) */
/* 1856 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1858 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1860 */	0x12, 0x0,	/* FC_UP */
/* 1862 */	NdrFcShort( 0xfe94 ),	/* Offset= -364 (1498) */
/* 1864 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1866 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1868 */	0x12, 0x0,	/* FC_UP */
/* 1870 */	NdrFcShort( 0xfec4 ),	/* Offset= -316 (1554) */
/* 1872 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1874 */	0x0,		/* 0 */
			NdrFcShort( 0xff59 ),	/* Offset= -167 (1708) */
			0x5b,		/* FC_END */
/* 1878 */	
			0x11, 0x0,	/* FC_RP */
/* 1880 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1882) */
/* 1882 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1884 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1886 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1888 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1890 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1892 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1894 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1896) */
/* 1896 */	
			0x13, 0x0,	/* FC_OP */
/* 1898 */	NdrFcShort( 0xfff0 ),	/* Offset= -16 (1882) */
/* 1900 */	
			0x11, 0x0,	/* FC_RP */
/* 1902 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1904) */
/* 1904 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1906 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1908 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1910 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1912 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1914 */	NdrFcShort( 0xf904 ),	/* Offset= -1788 (126) */
/* 1916 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1918 */	
			0x11, 0x0,	/* FC_RP */
/* 1920 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1922) */
/* 1922 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1924 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1926 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1928 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1930 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1932 */	
			0x11, 0x0,	/* FC_RP */
/* 1934 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1936) */
/* 1936 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1938 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1940 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1942 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1944 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1948 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1950 */	NdrFcShort( 0xfd12 ),	/* Offset= -750 (1200) */
/* 1952 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1954 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1956 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1958) */
/* 1958 */	
			0x13, 0x0,	/* FC_OP */
/* 1960 */	NdrFcShort( 0xffda ),	/* Offset= -38 (1922) */
/* 1962 */	
			0x11, 0x0,	/* FC_RP */
/* 1964 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1966) */
/* 1966 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1968 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1970 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1972 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1974 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1976 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1978 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1980 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1982 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1984 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1986 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1988 */	0x12, 0x0,	/* FC_UP */
/* 1990 */	NdrFcShort( 0xfe14 ),	/* Offset= -492 (1498) */
/* 1992 */	NdrFcShort( 0xc ),	/* 12 */
/* 1994 */	NdrFcShort( 0xc ),	/* 12 */
/* 1996 */	0x12, 0x0,	/* FC_UP */
/* 1998 */	NdrFcShort( 0xfe44 ),	/* Offset= -444 (1554) */
/* 2000 */	NdrFcShort( 0x10 ),	/* 16 */
/* 2002 */	NdrFcShort( 0x10 ),	/* 16 */
/* 2004 */	0x12, 0x0,	/* FC_UP */
/* 2006 */	NdrFcShort( 0xfe04 ),	/* Offset= -508 (1498) */
/* 2008 */	NdrFcShort( 0x14 ),	/* 20 */
/* 2010 */	NdrFcShort( 0x14 ),	/* 20 */
/* 2012 */	0x12, 0x0,	/* FC_UP */
/* 2014 */	NdrFcShort( 0xfe34 ),	/* Offset= -460 (1554) */
/* 2016 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 2018 */	0x0,		/* 0 */
			NdrFcShort( 0xfec9 ),	/* Offset= -311 (1708) */
			0x5b,		/* FC_END */
/* 2022 */	
			0x11, 0x0,	/* FC_RP */
/* 2024 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2026) */
/* 2026 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 2028 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2030 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 2032 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 2034 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 2036 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 2038 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2040) */
/* 2040 */	
			0x13, 0x0,	/* FC_OP */
/* 2042 */	NdrFcShort( 0xfff0 ),	/* Offset= -16 (2026) */
/* 2044 */	
			0x11, 0x0,	/* FC_RP */
/* 2046 */	NdrFcShort( 0xfdac ),	/* Offset= -596 (1450) */
/* 2048 */	
			0x11, 0x0,	/* FC_RP */
/* 2050 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2052) */
/* 2052 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 2054 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2056 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 2058 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2060 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 2064 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 2066 */	NdrFcShort( 0xfe1e ),	/* Offset= -482 (1584) */
/* 2068 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 2070 */	
			0x11, 0x0,	/* FC_RP */
/* 2072 */	NdrFcShort( 0xfe7a ),	/* Offset= -390 (1682) */
/* 2074 */	
			0x11, 0x0,	/* FC_RP */
/* 2076 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2078) */
/* 2078 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 2080 */	NdrFcShort( 0x18 ),	/* 24 */
/* 2082 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 2084 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2086 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 2088 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 2090 */	NdrFcShort( 0x18 ),	/* 24 */
/* 2092 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2094 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2096 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2098 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2100 */	0x12, 0x0,	/* FC_UP */
/* 2102 */	NdrFcShort( 0xfda4 ),	/* Offset= -604 (1498) */
/* 2104 */	NdrFcShort( 0xc ),	/* 12 */
/* 2106 */	NdrFcShort( 0xc ),	/* 12 */
/* 2108 */	0x12, 0x0,	/* FC_UP */
/* 2110 */	NdrFcShort( 0xfdd4 ),	/* Offset= -556 (1554) */
/* 2112 */	NdrFcShort( 0x10 ),	/* 16 */
/* 2114 */	NdrFcShort( 0x10 ),	/* 16 */
/* 2116 */	0x12, 0x0,	/* FC_UP */
/* 2118 */	NdrFcShort( 0xfd94 ),	/* Offset= -620 (1498) */
/* 2120 */	NdrFcShort( 0x14 ),	/* 20 */
/* 2122 */	NdrFcShort( 0x14 ),	/* 20 */
/* 2124 */	0x12, 0x0,	/* FC_UP */
/* 2126 */	NdrFcShort( 0xfdc4 ),	/* Offset= -572 (1554) */
/* 2128 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 2130 */	0x0,		/* 0 */
			NdrFcShort( 0xfe59 ),	/* Offset= -423 (1708) */
			0x5b,		/* FC_END */
/* 2134 */	
			0x11, 0x0,	/* FC_RP */
/* 2136 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2138) */
/* 2138 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 2140 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2142 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 2144 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2146 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 2148 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 2150 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2152 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2154 */	NdrFcShort( 0x1 ),	/* 1 */
/* 2156 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2158 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2160 */	0x12, 0x0,	/* FC_UP */
/* 2162 */	NdrFcShort( 0xfcda ),	/* Offset= -806 (1356) */
/* 2164 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 2166 */	0x5c,		/* FC_PAD */
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


/* Object interface: CATID_OPCHDAServer10, ver. 0.0,
   GUID={0x7DE5B060,0xE089,0x11d2,{0xA5,0xE6,0x00,0x00,0x86,0x33,0x93,0x99}} */

#pragma code_seg(".orpc")
static const unsigned short CATID_OPCHDAServer10_FormatStringOffsetTable[] =
    {
    0
    };

static const MIDL_STUBLESS_PROXY_INFO CATID_OPCHDAServer10_ProxyInfo =
    {
    &Object_StubDesc,
    opchda__MIDL_ProcFormatString.Format,
    &CATID_OPCHDAServer10_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO CATID_OPCHDAServer10_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opchda__MIDL_ProcFormatString.Format,
    &CATID_OPCHDAServer10_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(3) _CATID_OPCHDAServer10ProxyVtbl = 
{
    0,
    &IID_CATID_OPCHDAServer10,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy
};

const CInterfaceStubVtbl _CATID_OPCHDAServer10StubVtbl =
{
    &IID_CATID_OPCHDAServer10,
    &CATID_OPCHDAServer10_ServerInfo,
    3,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Standard interface: __MIDL_itf_opchda_0000_0001, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: IOPCHDA_Browser, ver. 0.0,
   GUID={0x1F1217B1,0xDEE0,0x11d2,{0xA5,0xE5,0x00,0x00,0x86,0x33,0x93,0x99}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCHDA_Browser_FormatStringOffsetTable[] =
    {
    0,
    34,
    68,
    102
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCHDA_Browser_ProxyInfo =
    {
    &Object_StubDesc,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_Browser_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCHDA_Browser_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_Browser_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(7) _IOPCHDA_BrowserProxyVtbl = 
{
    &IOPCHDA_Browser_ProxyInfo,
    &IID_IOPCHDA_Browser,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Browser::GetEnum */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Browser::ChangeBrowsePosition */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Browser::GetItemID */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Browser::GetBranchPosition */
};

const CInterfaceStubVtbl _IOPCHDA_BrowserStubVtbl =
{
    &IID_IOPCHDA_Browser,
    &IOPCHDA_Browser_ServerInfo,
    7,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCHDA_Server, ver. 0.0,
   GUID={0x1F1217B0,0xDEE0,0x11d2,{0xA5,0xE5,0x00,0x00,0x86,0x33,0x93,0x99}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCHDA_Server_FormatStringOffsetTable[] =
    {
    130,
    182,
    228,
    304,
    356,
    396,
    436
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCHDA_Server_ProxyInfo =
    {
    &Object_StubDesc,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_Server_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCHDA_Server_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_Server_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(10) _IOPCHDA_ServerProxyVtbl = 
{
    &IOPCHDA_Server_ProxyInfo,
    &IID_IOPCHDA_Server,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Server::GetItemAttributes */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Server::GetAggregates */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Server::GetHistorianStatus */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Server::GetItemHandles */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Server::ReleaseItemHandles */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Server::ValidateItemIDs */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Server::CreateBrowse */
};

const CInterfaceStubVtbl _IOPCHDA_ServerStubVtbl =
{
    &IID_IOPCHDA_Server,
    &IOPCHDA_Server_ServerInfo,
    10,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCHDA_SyncRead, ver. 0.0,
   GUID={0x1F1217B2,0xDEE0,0x11d2,{0xA5,0xE5,0x00,0x00,0x86,0x33,0x93,0x99}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCHDA_SyncRead_FormatStringOffsetTable[] =
    {
    494,
    564,
    634,
    692,
    756
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCHDA_SyncRead_ProxyInfo =
    {
    &Object_StubDesc,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_SyncRead_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCHDA_SyncRead_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_SyncRead_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(8) _IOPCHDA_SyncReadProxyVtbl = 
{
    &IOPCHDA_SyncRead_ProxyInfo,
    &IID_IOPCHDA_SyncRead,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncRead::ReadRaw */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncRead::ReadProcessed */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncRead::ReadAtTime */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncRead::ReadModified */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncRead::ReadAttribute */
};

const CInterfaceStubVtbl _IOPCHDA_SyncReadStubVtbl =
{
    &IID_IOPCHDA_SyncRead,
    &IOPCHDA_SyncRead_ServerInfo,
    8,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCHDA_SyncUpdate, ver. 0.0,
   GUID={0x1F1217B3,0xDEE0,0x11d2,{0xA5,0xE5,0x00,0x00,0x86,0x33,0x93,0x99}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCHDA_SyncUpdate_FormatStringOffsetTable[] =
    {
    820,
    848,
    906,
    964,
    1022,
    1074
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCHDA_SyncUpdate_ProxyInfo =
    {
    &Object_StubDesc,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_SyncUpdate_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCHDA_SyncUpdate_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_SyncUpdate_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(9) _IOPCHDA_SyncUpdateProxyVtbl = 
{
    &IOPCHDA_SyncUpdate_ProxyInfo,
    &IID_IOPCHDA_SyncUpdate,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncUpdate::QueryCapabilities */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncUpdate::Insert */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncUpdate::Replace */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncUpdate::InsertReplace */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncUpdate::DeleteRaw */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncUpdate::DeleteAtTime */
};

const CInterfaceStubVtbl _IOPCHDA_SyncUpdateStubVtbl =
{
    &IID_IOPCHDA_SyncUpdate,
    &IOPCHDA_SyncUpdate_ServerInfo,
    9,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCHDA_SyncAnnotations, ver. 0.0,
   GUID={0x1F1217B4,0xDEE0,0x11d2,{0xA5,0xE5,0x00,0x00,0x86,0x33,0x93,0x99}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCHDA_SyncAnnotations_FormatStringOffsetTable[] =
    {
    820,
    1120,
    1178
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCHDA_SyncAnnotations_ProxyInfo =
    {
    &Object_StubDesc,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_SyncAnnotations_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCHDA_SyncAnnotations_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_SyncAnnotations_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(6) _IOPCHDA_SyncAnnotationsProxyVtbl = 
{
    &IOPCHDA_SyncAnnotations_ProxyInfo,
    &IID_IOPCHDA_SyncAnnotations,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncAnnotations::QueryCapabilities */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncAnnotations::Read */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_SyncAnnotations::Insert */
};

const CInterfaceStubVtbl _IOPCHDA_SyncAnnotationsStubVtbl =
{
    &IID_IOPCHDA_SyncAnnotations,
    &IOPCHDA_SyncAnnotations_ServerInfo,
    6,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCHDA_AsyncRead, ver. 0.0,
   GUID={0x1F1217B5,0xDEE0,0x11d2,{0xA5,0xE5,0x00,0x00,0x86,0x33,0x93,0x99}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCHDA_AsyncRead_FormatStringOffsetTable[] =
    {
    1230,
    1306,
    1370,
    1446,
    1522,
    1586,
    1656,
    1726
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCHDA_AsyncRead_ProxyInfo =
    {
    &Object_StubDesc,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_AsyncRead_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCHDA_AsyncRead_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_AsyncRead_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(11) _IOPCHDA_AsyncReadProxyVtbl = 
{
    &IOPCHDA_AsyncRead_ProxyInfo,
    &IID_IOPCHDA_AsyncRead,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncRead::ReadRaw */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncRead::AdviseRaw */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncRead::ReadProcessed */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncRead::AdviseProcessed */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncRead::ReadAtTime */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncRead::ReadModified */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncRead::ReadAttribute */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncRead::Cancel */
};

const CInterfaceStubVtbl _IOPCHDA_AsyncReadStubVtbl =
{
    &IID_IOPCHDA_AsyncRead,
    &IOPCHDA_AsyncRead_ServerInfo,
    11,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCHDA_AsyncUpdate, ver. 0.0,
   GUID={0x1F1217B6,0xDEE0,0x11d2,{0xA5,0xE5,0x00,0x00,0x86,0x33,0x93,0x99}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCHDA_AsyncUpdate_FormatStringOffsetTable[] =
    {
    820,
    1754,
    1824,
    1894,
    1964,
    2028,
    2086
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCHDA_AsyncUpdate_ProxyInfo =
    {
    &Object_StubDesc,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_AsyncUpdate_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCHDA_AsyncUpdate_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_AsyncUpdate_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(10) _IOPCHDA_AsyncUpdateProxyVtbl = 
{
    &IOPCHDA_AsyncUpdate_ProxyInfo,
    &IID_IOPCHDA_AsyncUpdate,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncUpdate::QueryCapabilities */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncUpdate::Insert */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncUpdate::Replace */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncUpdate::InsertReplace */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncUpdate::DeleteRaw */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncUpdate::DeleteAtTime */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncUpdate::Cancel */
};

const CInterfaceStubVtbl _IOPCHDA_AsyncUpdateStubVtbl =
{
    &IID_IOPCHDA_AsyncUpdate,
    &IOPCHDA_AsyncUpdate_ServerInfo,
    10,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCHDA_AsyncAnnotations, ver. 0.0,
   GUID={0x1F1217B7,0xDEE0,0x11d2,{0xA5,0xE5,0x00,0x00,0x86,0x33,0x93,0x99}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCHDA_AsyncAnnotations_FormatStringOffsetTable[] =
    {
    820,
    2114,
    2178,
    2242
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCHDA_AsyncAnnotations_ProxyInfo =
    {
    &Object_StubDesc,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_AsyncAnnotations_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCHDA_AsyncAnnotations_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_AsyncAnnotations_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(7) _IOPCHDA_AsyncAnnotationsProxyVtbl = 
{
    &IOPCHDA_AsyncAnnotations_ProxyInfo,
    &IID_IOPCHDA_AsyncAnnotations,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncAnnotations::QueryCapabilities */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncAnnotations::Read */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncAnnotations::Insert */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_AsyncAnnotations::Cancel */
};

const CInterfaceStubVtbl _IOPCHDA_AsyncAnnotationsStubVtbl =
{
    &IID_IOPCHDA_AsyncAnnotations,
    &IOPCHDA_AsyncAnnotations_ServerInfo,
    7,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCHDA_Playback, ver. 0.0,
   GUID={0x1F1217B8,0xDEE0,0x11d2,{0xA5,0xE5,0x00,0x00,0x86,0x33,0x93,0x99}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCHDA_Playback_FormatStringOffsetTable[] =
    {
    2270,
    2352,
    2440
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCHDA_Playback_ProxyInfo =
    {
    &Object_StubDesc,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_Playback_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCHDA_Playback_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_Playback_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(6) _IOPCHDA_PlaybackProxyVtbl = 
{
    &IOPCHDA_Playback_ProxyInfo,
    &IID_IOPCHDA_Playback,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Playback::ReadRawWithUpdate */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Playback::ReadProcessedWithUpdate */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_Playback::Cancel */
};

const CInterfaceStubVtbl _IOPCHDA_PlaybackStubVtbl =
{
    &IID_IOPCHDA_Playback,
    &IOPCHDA_Playback_ServerInfo,
    6,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCHDA_DataCallback, ver. 0.0,
   GUID={0x1F1217B9,0xDEE0,0x11d2,{0xA5,0xE5,0x00,0x00,0x86,0x33,0x93,0x99}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCHDA_DataCallback_FormatStringOffsetTable[] =
    {
    2468,
    2520,
    2572,
    2624,
    2682,
    2734,
    2786,
    2838,
    2890
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCHDA_DataCallback_ProxyInfo =
    {
    &Object_StubDesc,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_DataCallback_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCHDA_DataCallback_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opchda__MIDL_ProcFormatString.Format,
    &IOPCHDA_DataCallback_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(12) _IOPCHDA_DataCallbackProxyVtbl = 
{
    &IOPCHDA_DataCallback_ProxyInfo,
    &IID_IOPCHDA_DataCallback,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCHDA_DataCallback::OnDataChange */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_DataCallback::OnReadComplete */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_DataCallback::OnReadModifiedComplete */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_DataCallback::OnReadAttributeComplete */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_DataCallback::OnReadAnnotations */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_DataCallback::OnInsertAnnotations */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_DataCallback::OnPlayback */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_DataCallback::OnUpdateComplete */ ,
    (void *) (INT_PTR) -1 /* IOPCHDA_DataCallback::OnCancelComplete */
};

const CInterfaceStubVtbl _IOPCHDA_DataCallbackStubVtbl =
{
    &IID_IOPCHDA_DataCallback,
    &IOPCHDA_DataCallback_ServerInfo,
    12,
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
    opchda__MIDL_TypeFormatString.Format,
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

const CInterfaceProxyVtbl * const _opchda_ProxyVtblList[] = 
{
    ( CInterfaceProxyVtbl *) &_CATID_OPCHDAServer10ProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCHDA_ServerProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCHDA_BrowserProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCHDA_SyncReadProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCHDA_SyncUpdateProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCHDA_SyncAnnotationsProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCHDA_AsyncReadProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCHDA_AsyncUpdateProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCHDA_AsyncAnnotationsProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCHDA_PlaybackProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCHDA_DataCallbackProxyVtbl,
    0
};

const CInterfaceStubVtbl * const _opchda_StubVtblList[] = 
{
    ( CInterfaceStubVtbl *) &_CATID_OPCHDAServer10StubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCHDA_ServerStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCHDA_BrowserStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCHDA_SyncReadStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCHDA_SyncUpdateStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCHDA_SyncAnnotationsStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCHDA_AsyncReadStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCHDA_AsyncUpdateStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCHDA_AsyncAnnotationsStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCHDA_PlaybackStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCHDA_DataCallbackStubVtbl,
    0
};

PCInterfaceName const _opchda_InterfaceNamesList[] = 
{
    "CATID_OPCHDAServer10",
    "IOPCHDA_Server",
    "IOPCHDA_Browser",
    "IOPCHDA_SyncRead",
    "IOPCHDA_SyncUpdate",
    "IOPCHDA_SyncAnnotations",
    "IOPCHDA_AsyncRead",
    "IOPCHDA_AsyncUpdate",
    "IOPCHDA_AsyncAnnotations",
    "IOPCHDA_Playback",
    "IOPCHDA_DataCallback",
    0
};


#define _opchda_CHECK_IID(n)	IID_GENERIC_CHECK_IID( _opchda, pIID, n)

int __stdcall _opchda_IID_Lookup( const IID * pIID, int * pIndex )
{
    IID_BS_LOOKUP_SETUP

    IID_BS_LOOKUP_INITIAL_TEST( _opchda, 11, 8 )
    IID_BS_LOOKUP_NEXT_TEST( _opchda, 4 )
    IID_BS_LOOKUP_NEXT_TEST( _opchda, 2 )
    IID_BS_LOOKUP_NEXT_TEST( _opchda, 1 )
    IID_BS_LOOKUP_RETURN_RESULT( _opchda, 11, *pIndex )
    
}

const ExtendedProxyFileInfo opchda_ProxyFileInfo = 
{
    (PCInterfaceProxyVtblList *) & _opchda_ProxyVtblList,
    (PCInterfaceStubVtblList *) & _opchda_StubVtblList,
    (const PCInterfaceName * ) & _opchda_InterfaceNamesList,
    0, /* no delegation */
    & _opchda_IID_Lookup, 
    11,
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

