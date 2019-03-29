

/* this ALWAYS GENERATED file contains the proxy stub code */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Tue Jun 30 10:35:10 2015
 */
/* Compiler settings for OpcCmd.idl:
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


#include "OpcCmd.h"

#define TYPE_FORMAT_STRING_SIZE   2235                              
#define PROC_FORMAT_STRING_SIZE   561                               
#define EXPR_FORMAT_STRING_SIZE   1                                 
#define TRANSMIT_AS_TABLE_SIZE    0            
#define WIRE_MARSHAL_TABLE_SIZE   1            

typedef struct _OpcCmd_MIDL_TYPE_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ TYPE_FORMAT_STRING_SIZE ];
    } OpcCmd_MIDL_TYPE_FORMAT_STRING;

typedef struct _OpcCmd_MIDL_PROC_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ PROC_FORMAT_STRING_SIZE ];
    } OpcCmd_MIDL_PROC_FORMAT_STRING;

typedef struct _OpcCmd_MIDL_EXPR_FORMAT_STRING
    {
    long          Pad;
    unsigned char  Format[ EXPR_FORMAT_STRING_SIZE ];
    } OpcCmd_MIDL_EXPR_FORMAT_STRING;


static const RPC_SYNTAX_IDENTIFIER  _RpcTransferSyntax = 
{{0x8A885D04,0x1CEB,0x11C9,{0x9F,0xE8,0x08,0x00,0x2B,0x10,0x48,0x60}},{2,0}};


extern const OpcCmd_MIDL_TYPE_FORMAT_STRING OpcCmd__MIDL_TypeFormatString;
extern const OpcCmd_MIDL_PROC_FORMAT_STRING OpcCmd__MIDL_ProcFormatString;
extern const OpcCmd_MIDL_EXPR_FORMAT_STRING OpcCmd__MIDL_ExprFormatString;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO CATID_OPCCMDServer10_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO CATID_OPCCMDServer10_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCComandCallback_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCComandCallback_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCCommandInformation_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCCommandInformation_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCCommandExecution_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCCommandExecution_ProxyInfo;


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


static const OpcCmd_MIDL_PROC_FORMAT_STRING OpcCmd__MIDL_ProcFormatString =
    {
        0,
        {

	/* Procedure OnStateChange */

			0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/*  2 */	NdrFcLong( 0x0 ),	/* 0 */
/*  6 */	NdrFcShort( 0x3 ),	/* 3 */
/*  8 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 10 */	NdrFcShort( 0x18 ),	/* 24 */
/* 12 */	NdrFcShort( 0x8 ),	/* 8 */
/* 14 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwNoOfEvents */

/* 16 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 18 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 20 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pEvents */

/* 22 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 24 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 26 */	NdrFcShort( 0x45c ),	/* Type Offset=1116 */

	/* Parameter dwNoOfPermittedControls */

/* 28 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 30 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 32 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszPermittedControls */

/* 34 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 36 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 38 */	NdrFcShort( 0x472 ),	/* Type Offset=1138 */

	/* Parameter bNoStateChange */

/* 40 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 42 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 44 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 46 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 48 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 50 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryCapabilities */

/* 52 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 54 */	NdrFcLong( 0x0 ),	/* 0 */
/* 58 */	NdrFcShort( 0x3 ),	/* 3 */
/* 60 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 62 */	NdrFcShort( 0x0 ),	/* 0 */
/* 64 */	NdrFcShort( 0x48 ),	/* 72 */
/* 66 */	0x4,		/* Oi2 Flags:  has return, */
			0x3,		/* 3 */

	/* Parameter pdblMaxStorageTime */

/* 68 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 70 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 72 */	0xc,		/* FC_DOUBLE */
			0x0,		/* 0 */

	/* Parameter pbSupportsEventFilter */

/* 74 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 76 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 78 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 80 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 82 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 84 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryComands */

/* 86 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 88 */	NdrFcLong( 0x0 ),	/* 0 */
/* 92 */	NdrFcShort( 0x4 ),	/* 4 */
/* 94 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 96 */	NdrFcShort( 0x0 ),	/* 0 */
/* 98 */	NdrFcShort( 0x24 ),	/* 36 */
/* 100 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x3,		/* 3 */

	/* Parameter pdwCount */

/* 102 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 104 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 106 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppNamespaces */

/* 108 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 110 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 112 */	NdrFcShort( 0x498 ),	/* Type Offset=1176 */

	/* Return value */

/* 114 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 116 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 118 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure BrowseCommandTargets */

/* 120 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 122 */	NdrFcLong( 0x0 ),	/* 0 */
/* 126 */	NdrFcShort( 0x5 ),	/* 5 */
/* 128 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 130 */	NdrFcShort( 0x6 ),	/* 6 */
/* 132 */	NdrFcShort( 0x24 ),	/* 36 */
/* 134 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter szTargetID */

/* 136 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 138 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 140 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Parameter szNamespaceUri */

/* 142 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 144 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 146 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Parameter eBrowseFilter */

/* 148 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 150 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 152 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter pdwCount */

/* 154 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 156 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 158 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppTargets */

/* 160 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 162 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 164 */	NdrFcShort( 0x51c ),	/* Type Offset=1308 */

	/* Return value */

/* 166 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 168 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 170 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetCommandDescription */

/* 172 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 174 */	NdrFcLong( 0x0 ),	/* 0 */
/* 178 */	NdrFcShort( 0x6 ),	/* 6 */
/* 180 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 182 */	NdrFcShort( 0x0 ),	/* 0 */
/* 184 */	NdrFcShort( 0x8 ),	/* 8 */
/* 186 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter szCommandName */

/* 188 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 190 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 192 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Parameter szNamespaceUri */

/* 194 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 196 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 198 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Parameter pDescription */

/* 200 */	NdrFcShort( 0x113 ),	/* Flags:  must size, must free, out, simple ref, */
/* 202 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 204 */	NdrFcShort( 0x7be ),	/* Type Offset=1982 */

	/* Return value */

/* 206 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 208 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 210 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SyncInvoke */

/* 212 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 214 */	NdrFcLong( 0x0 ),	/* 0 */
/* 218 */	NdrFcShort( 0x3 ),	/* 3 */
/* 220 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 222 */	NdrFcShort( 0x10 ),	/* 16 */
/* 224 */	NdrFcShort( 0x24 ),	/* 36 */
/* 226 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0xa,		/* 10 */

	/* Parameter szCommandName */

/* 228 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 230 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 232 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Parameter szNamespaceUri */

/* 234 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 236 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 238 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Parameter szTargetID */

/* 240 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 242 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 244 */	NdrFcShort( 0x80a ),	/* Type Offset=2058 */

	/* Parameter dwNoOfArguments */

/* 246 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 248 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 250 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pArguments */

/* 252 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 254 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 256 */	NdrFcShort( 0x812 ),	/* Type Offset=2066 */

	/* Parameter dwNoOfFilters */

/* 258 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 260 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 262 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszFilters */

/* 264 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 266 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 268 */	NdrFcShort( 0x828 ),	/* Type Offset=2088 */

	/* Parameter pdwNoOfEvents */

/* 270 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 272 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 274 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppEvents */

/* 276 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 278 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 280 */	NdrFcShort( 0x846 ),	/* Type Offset=2118 */

	/* Return value */

/* 282 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 284 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 286 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure AsyncInvoke */

/* 288 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 290 */	NdrFcLong( 0x0 ),	/* 0 */
/* 294 */	NdrFcShort( 0x4 ),	/* 4 */
/* 296 */	NdrFcShort( 0x38 ),	/* x86 Stack size/offset = 56 */
/* 298 */	NdrFcShort( 0x20 ),	/* 32 */
/* 300 */	NdrFcShort( 0x24 ),	/* 36 */
/* 302 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0xd,		/* 13 */

	/* Parameter szCommandName */

/* 304 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 306 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 308 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Parameter szNamespaceUri */

/* 310 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 312 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 314 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Parameter szTargetID */

/* 316 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 318 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 320 */	NdrFcShort( 0x80a ),	/* Type Offset=2058 */

	/* Parameter dwNoOfArguments */

/* 322 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 324 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 326 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pArguments */

/* 328 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 330 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 332 */	NdrFcShort( 0x812 ),	/* Type Offset=2066 */

	/* Parameter dwNoOfFilters */

/* 334 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 336 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 338 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszFilters */

/* 340 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 342 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 344 */	NdrFcShort( 0x828 ),	/* Type Offset=2088 */

	/* Parameter ipCallback */

/* 346 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 348 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 350 */	NdrFcShort( 0x860 ),	/* Type Offset=2144 */

	/* Parameter dwUpdateFrequency */

/* 352 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 354 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 356 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwKeepAliveTime */

/* 358 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 360 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 362 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszInvokeUUID */

/* 364 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 366 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 368 */	NdrFcShort( 0x872 ),	/* Type Offset=2162 */

	/* Parameter pdwRevisedUpdateFrequency */

/* 370 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 372 */	NdrFcShort( 0x30 ),	/* x86 Stack size/offset = 48 */
/* 374 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 376 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 378 */	NdrFcShort( 0x34 ),	/* x86 Stack size/offset = 52 */
/* 380 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Connect */

/* 382 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 384 */	NdrFcLong( 0x0 ),	/* 0 */
/* 388 */	NdrFcShort( 0x5 ),	/* 5 */
/* 390 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 392 */	NdrFcShort( 0x10 ),	/* 16 */
/* 394 */	NdrFcShort( 0x24 ),	/* 36 */
/* 396 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter szInvokeUUID */

/* 398 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 400 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 402 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Parameter ipCallback */

/* 404 */	NdrFcShort( 0xb ),	/* Flags:  must size, must free, in, */
/* 406 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 408 */	NdrFcShort( 0x860 ),	/* Type Offset=2144 */

	/* Parameter dwUpdateFrequency */

/* 410 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 412 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 414 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwKeepAliveTime */

/* 416 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 418 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 420 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwRevisedUpdateFrequency */

/* 422 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 424 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 426 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 428 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 430 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 432 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Disconnect */

/* 434 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 436 */	NdrFcLong( 0x0 ),	/* 0 */
/* 440 */	NdrFcShort( 0x6 ),	/* 6 */
/* 442 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 444 */	NdrFcShort( 0x0 ),	/* 0 */
/* 446 */	NdrFcShort( 0x8 ),	/* 8 */
/* 448 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x2,		/* 2 */

	/* Parameter szInvokeUUID */

/* 450 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 452 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 454 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Return value */

/* 456 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 458 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 460 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryState */

/* 462 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 464 */	NdrFcLong( 0x0 ),	/* 0 */
/* 468 */	NdrFcShort( 0x7 ),	/* 7 */
/* 470 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 472 */	NdrFcShort( 0x8 ),	/* 8 */
/* 474 */	NdrFcShort( 0x5c ),	/* 92 */
/* 476 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x8,		/* 8 */

	/* Parameter szInvokeUUID */

/* 478 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 480 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 482 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Parameter dwWaitTime */

/* 484 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 486 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 488 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwNoOfEvents */

/* 490 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 492 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 494 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppEvents */

/* 496 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 498 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 500 */	NdrFcShort( 0x87a ),	/* Type Offset=2170 */

	/* Parameter pdwNoOfPermittedControls */

/* 502 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 504 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 506 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppszPermittedControls */

/* 508 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 510 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 512 */	NdrFcShort( 0x894 ),	/* Type Offset=2196 */

	/* Parameter pbNoStateChange */

/* 514 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 516 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 518 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 520 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 522 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 524 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Control */

/* 526 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 528 */	NdrFcLong( 0x0 ),	/* 0 */
/* 532 */	NdrFcShort( 0x8 ),	/* 8 */
/* 534 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 536 */	NdrFcShort( 0x0 ),	/* 0 */
/* 538 */	NdrFcShort( 0x8 ),	/* 8 */
/* 540 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter szInvokeUUID */

/* 542 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 544 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 546 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Parameter szControl */

/* 548 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 550 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 552 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Return value */

/* 554 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 556 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 558 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

			0x0
        }
    };

static const OpcCmd_MIDL_TYPE_FORMAT_STRING OpcCmd__MIDL_TypeFormatString =
    {
        0,
        {
			NdrFcShort( 0x0 ),	/* 0 */
/*  2 */	
			0x11, 0x0,	/* FC_RP */
/*  4 */	NdrFcShort( 0x458 ),	/* Offset= 1112 (1116) */
/*  6 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/*  8 */	NdrFcShort( 0x8 ),	/* 8 */
/* 10 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 12 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 14 */	
			0x12, 0x0,	/* FC_UP */
/* 16 */	NdrFcShort( 0x3c2 ),	/* Offset= 962 (978) */
/* 18 */	
			0x2b,		/* FC_NON_ENCAPSULATED_UNION */
			0x9,		/* FC_ULONG */
/* 20 */	0x7,		/* Corr desc: FC_USHORT */
			0x0,		/*  */
/* 22 */	NdrFcShort( 0xfff8 ),	/* -8 */
/* 24 */	NdrFcShort( 0x2 ),	/* Offset= 2 (26) */
/* 26 */	NdrFcShort( 0x10 ),	/* 16 */
/* 28 */	NdrFcShort( 0x2f ),	/* 47 */
/* 30 */	NdrFcLong( 0x14 ),	/* 20 */
/* 34 */	NdrFcShort( 0x800b ),	/* Simple arm type: FC_HYPER */
/* 36 */	NdrFcLong( 0x3 ),	/* 3 */
/* 40 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 42 */	NdrFcLong( 0x11 ),	/* 17 */
/* 46 */	NdrFcShort( 0x8001 ),	/* Simple arm type: FC_BYTE */
/* 48 */	NdrFcLong( 0x2 ),	/* 2 */
/* 52 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 54 */	NdrFcLong( 0x4 ),	/* 4 */
/* 58 */	NdrFcShort( 0x800a ),	/* Simple arm type: FC_FLOAT */
/* 60 */	NdrFcLong( 0x5 ),	/* 5 */
/* 64 */	NdrFcShort( 0x800c ),	/* Simple arm type: FC_DOUBLE */
/* 66 */	NdrFcLong( 0xb ),	/* 11 */
/* 70 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 72 */	NdrFcLong( 0xa ),	/* 10 */
/* 76 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 78 */	NdrFcLong( 0x6 ),	/* 6 */
/* 82 */	NdrFcShort( 0xe8 ),	/* Offset= 232 (314) */
/* 84 */	NdrFcLong( 0x7 ),	/* 7 */
/* 88 */	NdrFcShort( 0x800c ),	/* Simple arm type: FC_DOUBLE */
/* 90 */	NdrFcLong( 0x8 ),	/* 8 */
/* 94 */	NdrFcShort( 0xe2 ),	/* Offset= 226 (320) */
/* 96 */	NdrFcLong( 0xd ),	/* 13 */
/* 100 */	NdrFcShort( 0xf4 ),	/* Offset= 244 (344) */
/* 102 */	NdrFcLong( 0x9 ),	/* 9 */
/* 106 */	NdrFcShort( 0x100 ),	/* Offset= 256 (362) */
/* 108 */	NdrFcLong( 0x2000 ),	/* 8192 */
/* 112 */	NdrFcShort( 0x10c ),	/* Offset= 268 (380) */
/* 114 */	NdrFcLong( 0x24 ),	/* 36 */
/* 118 */	NdrFcShort( 0x312 ),	/* Offset= 786 (904) */
/* 120 */	NdrFcLong( 0x4024 ),	/* 16420 */
/* 124 */	NdrFcShort( 0x30c ),	/* Offset= 780 (904) */
/* 126 */	NdrFcLong( 0x4011 ),	/* 16401 */
/* 130 */	NdrFcShort( 0x30a ),	/* Offset= 778 (908) */
/* 132 */	NdrFcLong( 0x4002 ),	/* 16386 */
/* 136 */	NdrFcShort( 0x308 ),	/* Offset= 776 (912) */
/* 138 */	NdrFcLong( 0x4003 ),	/* 16387 */
/* 142 */	NdrFcShort( 0x306 ),	/* Offset= 774 (916) */
/* 144 */	NdrFcLong( 0x4014 ),	/* 16404 */
/* 148 */	NdrFcShort( 0x304 ),	/* Offset= 772 (920) */
/* 150 */	NdrFcLong( 0x4004 ),	/* 16388 */
/* 154 */	NdrFcShort( 0x302 ),	/* Offset= 770 (924) */
/* 156 */	NdrFcLong( 0x4005 ),	/* 16389 */
/* 160 */	NdrFcShort( 0x300 ),	/* Offset= 768 (928) */
/* 162 */	NdrFcLong( 0x400b ),	/* 16395 */
/* 166 */	NdrFcShort( 0x2ea ),	/* Offset= 746 (912) */
/* 168 */	NdrFcLong( 0x400a ),	/* 16394 */
/* 172 */	NdrFcShort( 0x2e8 ),	/* Offset= 744 (916) */
/* 174 */	NdrFcLong( 0x4006 ),	/* 16390 */
/* 178 */	NdrFcShort( 0x2f2 ),	/* Offset= 754 (932) */
/* 180 */	NdrFcLong( 0x4007 ),	/* 16391 */
/* 184 */	NdrFcShort( 0x2e8 ),	/* Offset= 744 (928) */
/* 186 */	NdrFcLong( 0x4008 ),	/* 16392 */
/* 190 */	NdrFcShort( 0x2ea ),	/* Offset= 746 (936) */
/* 192 */	NdrFcLong( 0x400d ),	/* 16397 */
/* 196 */	NdrFcShort( 0x2e8 ),	/* Offset= 744 (940) */
/* 198 */	NdrFcLong( 0x4009 ),	/* 16393 */
/* 202 */	NdrFcShort( 0x2e6 ),	/* Offset= 742 (944) */
/* 204 */	NdrFcLong( 0x6000 ),	/* 24576 */
/* 208 */	NdrFcShort( 0x2e4 ),	/* Offset= 740 (948) */
/* 210 */	NdrFcLong( 0x400c ),	/* 16396 */
/* 214 */	NdrFcShort( 0x2e2 ),	/* Offset= 738 (952) */
/* 216 */	NdrFcLong( 0x10 ),	/* 16 */
/* 220 */	NdrFcShort( 0x8002 ),	/* Simple arm type: FC_CHAR */
/* 222 */	NdrFcLong( 0x12 ),	/* 18 */
/* 226 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 228 */	NdrFcLong( 0x13 ),	/* 19 */
/* 232 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 234 */	NdrFcLong( 0x15 ),	/* 21 */
/* 238 */	NdrFcShort( 0x800b ),	/* Simple arm type: FC_HYPER */
/* 240 */	NdrFcLong( 0x16 ),	/* 22 */
/* 244 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 246 */	NdrFcLong( 0x17 ),	/* 23 */
/* 250 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 252 */	NdrFcLong( 0xe ),	/* 14 */
/* 256 */	NdrFcShort( 0x2c0 ),	/* Offset= 704 (960) */
/* 258 */	NdrFcLong( 0x400e ),	/* 16398 */
/* 262 */	NdrFcShort( 0x2c4 ),	/* Offset= 708 (970) */
/* 264 */	NdrFcLong( 0x4010 ),	/* 16400 */
/* 268 */	NdrFcShort( 0x2c2 ),	/* Offset= 706 (974) */
/* 270 */	NdrFcLong( 0x4012 ),	/* 16402 */
/* 274 */	NdrFcShort( 0x27e ),	/* Offset= 638 (912) */
/* 276 */	NdrFcLong( 0x4013 ),	/* 16403 */
/* 280 */	NdrFcShort( 0x27c ),	/* Offset= 636 (916) */
/* 282 */	NdrFcLong( 0x4015 ),	/* 16405 */
/* 286 */	NdrFcShort( 0x27a ),	/* Offset= 634 (920) */
/* 288 */	NdrFcLong( 0x4016 ),	/* 16406 */
/* 292 */	NdrFcShort( 0x270 ),	/* Offset= 624 (916) */
/* 294 */	NdrFcLong( 0x4017 ),	/* 16407 */
/* 298 */	NdrFcShort( 0x26a ),	/* Offset= 618 (916) */
/* 300 */	NdrFcLong( 0x0 ),	/* 0 */
/* 304 */	NdrFcShort( 0x0 ),	/* Offset= 0 (304) */
/* 306 */	NdrFcLong( 0x1 ),	/* 1 */
/* 310 */	NdrFcShort( 0x0 ),	/* Offset= 0 (310) */
/* 312 */	NdrFcShort( 0xffff ),	/* Offset= -1 (311) */
/* 314 */	
			0x15,		/* FC_STRUCT */
			0x7,		/* 7 */
/* 316 */	NdrFcShort( 0x8 ),	/* 8 */
/* 318 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 320 */	
			0x12, 0x0,	/* FC_UP */
/* 322 */	NdrFcShort( 0xc ),	/* Offset= 12 (334) */
/* 324 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 326 */	NdrFcShort( 0x2 ),	/* 2 */
/* 328 */	0x9,		/* Corr desc: FC_ULONG */
			0x0,		/*  */
/* 330 */	NdrFcShort( 0xfffc ),	/* -4 */
/* 332 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 334 */	
			0x17,		/* FC_CSTRUCT */
			0x3,		/* 3 */
/* 336 */	NdrFcShort( 0x8 ),	/* 8 */
/* 338 */	NdrFcShort( 0xfff2 ),	/* Offset= -14 (324) */
/* 340 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 342 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 344 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 346 */	NdrFcLong( 0x0 ),	/* 0 */
/* 350 */	NdrFcShort( 0x0 ),	/* 0 */
/* 352 */	NdrFcShort( 0x0 ),	/* 0 */
/* 354 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 356 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 358 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 360 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 362 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 364 */	NdrFcLong( 0x20400 ),	/* 132096 */
/* 368 */	NdrFcShort( 0x0 ),	/* 0 */
/* 370 */	NdrFcShort( 0x0 ),	/* 0 */
/* 372 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 374 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 376 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 378 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 380 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 382 */	NdrFcShort( 0x2 ),	/* Offset= 2 (384) */
/* 384 */	
			0x12, 0x0,	/* FC_UP */
/* 386 */	NdrFcShort( 0x1f4 ),	/* Offset= 500 (886) */
/* 388 */	
			0x2a,		/* FC_ENCAPSULATED_UNION */
			0x49,		/* 73 */
/* 390 */	NdrFcShort( 0x18 ),	/* 24 */
/* 392 */	NdrFcShort( 0xa ),	/* 10 */
/* 394 */	NdrFcLong( 0x8 ),	/* 8 */
/* 398 */	NdrFcShort( 0x58 ),	/* Offset= 88 (486) */
/* 400 */	NdrFcLong( 0xd ),	/* 13 */
/* 404 */	NdrFcShort( 0x78 ),	/* Offset= 120 (524) */
/* 406 */	NdrFcLong( 0x9 ),	/* 9 */
/* 410 */	NdrFcShort( 0x94 ),	/* Offset= 148 (558) */
/* 412 */	NdrFcLong( 0xc ),	/* 12 */
/* 416 */	NdrFcShort( 0xbc ),	/* Offset= 188 (604) */
/* 418 */	NdrFcLong( 0x24 ),	/* 36 */
/* 422 */	NdrFcShort( 0x114 ),	/* Offset= 276 (698) */
/* 424 */	NdrFcLong( 0x800d ),	/* 32781 */
/* 428 */	NdrFcShort( 0x130 ),	/* Offset= 304 (732) */
/* 430 */	NdrFcLong( 0x10 ),	/* 16 */
/* 434 */	NdrFcShort( 0x148 ),	/* Offset= 328 (762) */
/* 436 */	NdrFcLong( 0x2 ),	/* 2 */
/* 440 */	NdrFcShort( 0x160 ),	/* Offset= 352 (792) */
/* 442 */	NdrFcLong( 0x3 ),	/* 3 */
/* 446 */	NdrFcShort( 0x178 ),	/* Offset= 376 (822) */
/* 448 */	NdrFcLong( 0x14 ),	/* 20 */
/* 452 */	NdrFcShort( 0x190 ),	/* Offset= 400 (852) */
/* 454 */	NdrFcShort( 0xffff ),	/* Offset= -1 (453) */
/* 456 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 458 */	NdrFcShort( 0x4 ),	/* 4 */
/* 460 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 462 */	NdrFcShort( 0x0 ),	/* 0 */
/* 464 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 466 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 468 */	NdrFcShort( 0x4 ),	/* 4 */
/* 470 */	NdrFcShort( 0x0 ),	/* 0 */
/* 472 */	NdrFcShort( 0x1 ),	/* 1 */
/* 474 */	NdrFcShort( 0x0 ),	/* 0 */
/* 476 */	NdrFcShort( 0x0 ),	/* 0 */
/* 478 */	0x12, 0x0,	/* FC_UP */
/* 480 */	NdrFcShort( 0xff6e ),	/* Offset= -146 (334) */
/* 482 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 484 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 486 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 488 */	NdrFcShort( 0x8 ),	/* 8 */
/* 490 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 492 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 494 */	NdrFcShort( 0x4 ),	/* 4 */
/* 496 */	NdrFcShort( 0x4 ),	/* 4 */
/* 498 */	0x11, 0x0,	/* FC_RP */
/* 500 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (456) */
/* 502 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 504 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 506 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 508 */	NdrFcShort( 0x0 ),	/* 0 */
/* 510 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 512 */	NdrFcShort( 0x0 ),	/* 0 */
/* 514 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 518 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 520 */	NdrFcShort( 0xff50 ),	/* Offset= -176 (344) */
/* 522 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 524 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 526 */	NdrFcShort( 0x8 ),	/* 8 */
/* 528 */	NdrFcShort( 0x0 ),	/* 0 */
/* 530 */	NdrFcShort( 0x6 ),	/* Offset= 6 (536) */
/* 532 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 534 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 536 */	
			0x11, 0x0,	/* FC_RP */
/* 538 */	NdrFcShort( 0xffe0 ),	/* Offset= -32 (506) */
/* 540 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 542 */	NdrFcShort( 0x0 ),	/* 0 */
/* 544 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 546 */	NdrFcShort( 0x0 ),	/* 0 */
/* 548 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 552 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 554 */	NdrFcShort( 0xff40 ),	/* Offset= -192 (362) */
/* 556 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 558 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 560 */	NdrFcShort( 0x8 ),	/* 8 */
/* 562 */	NdrFcShort( 0x0 ),	/* 0 */
/* 564 */	NdrFcShort( 0x6 ),	/* Offset= 6 (570) */
/* 566 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 568 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 570 */	
			0x11, 0x0,	/* FC_RP */
/* 572 */	NdrFcShort( 0xffe0 ),	/* Offset= -32 (540) */
/* 574 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 576 */	NdrFcShort( 0x4 ),	/* 4 */
/* 578 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 580 */	NdrFcShort( 0x0 ),	/* 0 */
/* 582 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 584 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 586 */	NdrFcShort( 0x4 ),	/* 4 */
/* 588 */	NdrFcShort( 0x0 ),	/* 0 */
/* 590 */	NdrFcShort( 0x1 ),	/* 1 */
/* 592 */	NdrFcShort( 0x0 ),	/* 0 */
/* 594 */	NdrFcShort( 0x0 ),	/* 0 */
/* 596 */	0x12, 0x0,	/* FC_UP */
/* 598 */	NdrFcShort( 0x17c ),	/* Offset= 380 (978) */
/* 600 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 602 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 604 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 606 */	NdrFcShort( 0x8 ),	/* 8 */
/* 608 */	NdrFcShort( 0x0 ),	/* 0 */
/* 610 */	NdrFcShort( 0x6 ),	/* Offset= 6 (616) */
/* 612 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 614 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 616 */	
			0x11, 0x0,	/* FC_RP */
/* 618 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (574) */
/* 620 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 622 */	NdrFcLong( 0x2f ),	/* 47 */
/* 626 */	NdrFcShort( 0x0 ),	/* 0 */
/* 628 */	NdrFcShort( 0x0 ),	/* 0 */
/* 630 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 632 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 634 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 636 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 638 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 640 */	NdrFcShort( 0x1 ),	/* 1 */
/* 642 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 644 */	NdrFcShort( 0x4 ),	/* 4 */
/* 646 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 648 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 650 */	NdrFcShort( 0x10 ),	/* 16 */
/* 652 */	NdrFcShort( 0x0 ),	/* 0 */
/* 654 */	NdrFcShort( 0xa ),	/* Offset= 10 (664) */
/* 656 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 658 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 660 */	NdrFcShort( 0xffd8 ),	/* Offset= -40 (620) */
/* 662 */	0x36,		/* FC_POINTER */
			0x5b,		/* FC_END */
/* 664 */	
			0x12, 0x0,	/* FC_UP */
/* 666 */	NdrFcShort( 0xffe4 ),	/* Offset= -28 (638) */
/* 668 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 670 */	NdrFcShort( 0x4 ),	/* 4 */
/* 672 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 674 */	NdrFcShort( 0x0 ),	/* 0 */
/* 676 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 678 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 680 */	NdrFcShort( 0x4 ),	/* 4 */
/* 682 */	NdrFcShort( 0x0 ),	/* 0 */
/* 684 */	NdrFcShort( 0x1 ),	/* 1 */
/* 686 */	NdrFcShort( 0x0 ),	/* 0 */
/* 688 */	NdrFcShort( 0x0 ),	/* 0 */
/* 690 */	0x12, 0x0,	/* FC_UP */
/* 692 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (648) */
/* 694 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 696 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 698 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 700 */	NdrFcShort( 0x8 ),	/* 8 */
/* 702 */	NdrFcShort( 0x0 ),	/* 0 */
/* 704 */	NdrFcShort( 0x6 ),	/* Offset= 6 (710) */
/* 706 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 708 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 710 */	
			0x11, 0x0,	/* FC_RP */
/* 712 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (668) */
/* 714 */	
			0x1d,		/* FC_SMFARRAY */
			0x0,		/* 0 */
/* 716 */	NdrFcShort( 0x8 ),	/* 8 */
/* 718 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 720 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 722 */	NdrFcShort( 0x10 ),	/* 16 */
/* 724 */	0x8,		/* FC_LONG */
			0x6,		/* FC_SHORT */
/* 726 */	0x6,		/* FC_SHORT */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 728 */	0x0,		/* 0 */
			NdrFcShort( 0xfff1 ),	/* Offset= -15 (714) */
			0x5b,		/* FC_END */
/* 732 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 734 */	NdrFcShort( 0x18 ),	/* 24 */
/* 736 */	NdrFcShort( 0x0 ),	/* 0 */
/* 738 */	NdrFcShort( 0xa ),	/* Offset= 10 (748) */
/* 740 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 742 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 744 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (720) */
/* 746 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 748 */	
			0x11, 0x0,	/* FC_RP */
/* 750 */	NdrFcShort( 0xff0c ),	/* Offset= -244 (506) */
/* 752 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 754 */	NdrFcShort( 0x1 ),	/* 1 */
/* 756 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 758 */	NdrFcShort( 0x0 ),	/* 0 */
/* 760 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 762 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 764 */	NdrFcShort( 0x8 ),	/* 8 */
/* 766 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 768 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 770 */	NdrFcShort( 0x4 ),	/* 4 */
/* 772 */	NdrFcShort( 0x4 ),	/* 4 */
/* 774 */	0x12, 0x0,	/* FC_UP */
/* 776 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (752) */
/* 778 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 780 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 782 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 784 */	NdrFcShort( 0x2 ),	/* 2 */
/* 786 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 788 */	NdrFcShort( 0x0 ),	/* 0 */
/* 790 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 792 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 794 */	NdrFcShort( 0x8 ),	/* 8 */
/* 796 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 798 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 800 */	NdrFcShort( 0x4 ),	/* 4 */
/* 802 */	NdrFcShort( 0x4 ),	/* 4 */
/* 804 */	0x12, 0x0,	/* FC_UP */
/* 806 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (782) */
/* 808 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 810 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 812 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 814 */	NdrFcShort( 0x4 ),	/* 4 */
/* 816 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 818 */	NdrFcShort( 0x0 ),	/* 0 */
/* 820 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 822 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 824 */	NdrFcShort( 0x8 ),	/* 8 */
/* 826 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 828 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 830 */	NdrFcShort( 0x4 ),	/* 4 */
/* 832 */	NdrFcShort( 0x4 ),	/* 4 */
/* 834 */	0x12, 0x0,	/* FC_UP */
/* 836 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (812) */
/* 838 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 840 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 842 */	
			0x1b,		/* FC_CARRAY */
			0x7,		/* 7 */
/* 844 */	NdrFcShort( 0x8 ),	/* 8 */
/* 846 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 848 */	NdrFcShort( 0x0 ),	/* 0 */
/* 850 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 852 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 854 */	NdrFcShort( 0x8 ),	/* 8 */
/* 856 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 858 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 860 */	NdrFcShort( 0x4 ),	/* 4 */
/* 862 */	NdrFcShort( 0x4 ),	/* 4 */
/* 864 */	0x12, 0x0,	/* FC_UP */
/* 866 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (842) */
/* 868 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 870 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 872 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 874 */	NdrFcShort( 0x8 ),	/* 8 */
/* 876 */	0x7,		/* Corr desc: FC_USHORT */
			0x0,		/*  */
/* 878 */	NdrFcShort( 0xffd8 ),	/* -40 */
/* 880 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 882 */	NdrFcShort( 0xfc94 ),	/* Offset= -876 (6) */
/* 884 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 886 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 888 */	NdrFcShort( 0x28 ),	/* 40 */
/* 890 */	NdrFcShort( 0xffee ),	/* Offset= -18 (872) */
/* 892 */	NdrFcShort( 0x0 ),	/* Offset= 0 (892) */
/* 894 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 896 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 898 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 900 */	NdrFcShort( 0xfe00 ),	/* Offset= -512 (388) */
/* 902 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 904 */	
			0x12, 0x0,	/* FC_UP */
/* 906 */	NdrFcShort( 0xfefe ),	/* Offset= -258 (648) */
/* 908 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 910 */	0x1,		/* FC_BYTE */
			0x5c,		/* FC_PAD */
/* 912 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 914 */	0x6,		/* FC_SHORT */
			0x5c,		/* FC_PAD */
/* 916 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 918 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 920 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 922 */	0xb,		/* FC_HYPER */
			0x5c,		/* FC_PAD */
/* 924 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 926 */	0xa,		/* FC_FLOAT */
			0x5c,		/* FC_PAD */
/* 928 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 930 */	0xc,		/* FC_DOUBLE */
			0x5c,		/* FC_PAD */
/* 932 */	
			0x12, 0x0,	/* FC_UP */
/* 934 */	NdrFcShort( 0xfd94 ),	/* Offset= -620 (314) */
/* 936 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 938 */	NdrFcShort( 0xfd96 ),	/* Offset= -618 (320) */
/* 940 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 942 */	NdrFcShort( 0xfdaa ),	/* Offset= -598 (344) */
/* 944 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 946 */	NdrFcShort( 0xfdb8 ),	/* Offset= -584 (362) */
/* 948 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 950 */	NdrFcShort( 0xfdc6 ),	/* Offset= -570 (380) */
/* 952 */	
			0x12, 0x10,	/* FC_UP [pointer_deref] */
/* 954 */	NdrFcShort( 0x2 ),	/* Offset= 2 (956) */
/* 956 */	
			0x12, 0x0,	/* FC_UP */
/* 958 */	NdrFcShort( 0x14 ),	/* Offset= 20 (978) */
/* 960 */	
			0x15,		/* FC_STRUCT */
			0x7,		/* 7 */
/* 962 */	NdrFcShort( 0x10 ),	/* 16 */
/* 964 */	0x6,		/* FC_SHORT */
			0x1,		/* FC_BYTE */
/* 966 */	0x1,		/* FC_BYTE */
			0x8,		/* FC_LONG */
/* 968 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 970 */	
			0x12, 0x0,	/* FC_UP */
/* 972 */	NdrFcShort( 0xfff4 ),	/* Offset= -12 (960) */
/* 974 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 976 */	0x2,		/* FC_CHAR */
			0x5c,		/* FC_PAD */
/* 978 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x7,		/* 7 */
/* 980 */	NdrFcShort( 0x20 ),	/* 32 */
/* 982 */	NdrFcShort( 0x0 ),	/* 0 */
/* 984 */	NdrFcShort( 0x0 ),	/* Offset= 0 (984) */
/* 986 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 988 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 990 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 992 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 994 */	NdrFcShort( 0xfc30 ),	/* Offset= -976 (18) */
/* 996 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 998 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1000 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1002 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1004 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1006 */	NdrFcShort( 0xfc20 ),	/* Offset= -992 (14) */
/* 1008 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1010 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1012 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1014 */	NdrFcShort( 0xa ),	/* Offset= 10 (1024) */
/* 1016 */	0x36,		/* FC_POINTER */
			0x40,		/* FC_STRUCTPAD4 */
/* 1018 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1020 */	NdrFcShort( 0xffea ),	/* Offset= -22 (998) */
/* 1022 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1024 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1026 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1028 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1030 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1032 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1034 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1036 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1040 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1042 */	NdrFcShort( 0xffde ),	/* Offset= -34 (1008) */
/* 1044 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1046 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1048 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1050 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1052 */	NdrFcShort( 0x28 ),	/* 40 */
/* 1054 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1058 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1060 */	NdrFcShort( 0xffcc ),	/* Offset= -52 (1008) */
/* 1062 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1064 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1066 */	NdrFcShort( 0x30 ),	/* 48 */
/* 1068 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1070 */	NdrFcShort( 0x12 ),	/* Offset= 18 (1088) */
/* 1072 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 1074 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1076 */	NdrFcShort( 0xfbd2 ),	/* Offset= -1070 (6) */
/* 1078 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1080 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1082 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1084 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1086 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1088 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1090 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1092 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1094 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1096 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1098 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1100 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1102 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1104 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1106 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1108 */	
			0x12, 0x0,	/* FC_UP */
/* 1110 */	NdrFcShort( 0xffae ),	/* Offset= -82 (1028) */
/* 1112 */	
			0x12, 0x0,	/* FC_UP */
/* 1114 */	NdrFcShort( 0xffbc ),	/* Offset= -68 (1046) */
/* 1116 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1118 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1120 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1122 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1124 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1128 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1130 */	NdrFcShort( 0xffbe ),	/* Offset= -66 (1064) */
/* 1132 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1134 */	
			0x11, 0x0,	/* FC_RP */
/* 1136 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1138) */
/* 1138 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1140 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1142 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1144 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1146 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1148 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1150 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1152 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1154 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1156 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1158 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1160 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1162 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1164 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1166 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1168 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 1170 */	0xc,		/* FC_DOUBLE */
			0x5c,		/* FC_PAD */
/* 1172 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 1174 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 1176 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1178 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1180) */
/* 1180 */	
			0x13, 0x0,	/* FC_OP */
/* 1182 */	NdrFcShort( 0x4a ),	/* Offset= 74 (1256) */
/* 1184 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1186 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1188 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1190 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1192 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1194 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1196 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1198 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1200 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1202 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1204 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1206 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1208 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1210 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1212 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1214 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1216 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1218 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1220 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1222 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1224 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1226 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1228 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1230 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1232 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1234 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1236 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1238 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1240 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1242 */	NdrFcShort( 0xc ),	/* 12 */
/* 1244 */	NdrFcShort( 0xc ),	/* 12 */
/* 1246 */	0x13, 0x0,	/* FC_OP */
/* 1248 */	NdrFcShort( 0xffc0 ),	/* Offset= -64 (1184) */
/* 1250 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1252 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1254 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1256 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1258 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1260 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 1262 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1264 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1266 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1268 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1270 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1272 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1274 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1276 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1278 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1280 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1282 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1284 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1286 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1288 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1290 */	NdrFcShort( 0xc ),	/* 12 */
/* 1292 */	NdrFcShort( 0xc ),	/* 12 */
/* 1294 */	0x13, 0x0,	/* FC_OP */
/* 1296 */	NdrFcShort( 0xff90 ),	/* Offset= -112 (1184) */
/* 1298 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1300 */	0x0,		/* 0 */
			NdrFcShort( 0xffa9 ),	/* Offset= -87 (1214) */
			0x5b,		/* FC_END */
/* 1304 */	
			0x11, 0x8,	/* FC_RP [simple_pointer] */
/* 1306 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1308 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1310 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1312) */
/* 1312 */	
			0x13, 0x0,	/* FC_OP */
/* 1314 */	NdrFcShort( 0x4c ),	/* Offset= 76 (1390) */
/* 1316 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1318 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1320 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1322 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1324 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1326 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1328 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1330 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1332 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1334 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1336 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1338 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1340 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1342 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1344 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1346 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1348 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1350 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1352 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1354 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1356 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1358 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1360 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1362 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1364 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1366 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1368 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1370 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1372 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1374 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1376 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1378 */	0x13, 0x0,	/* FC_OP */
/* 1380 */	NdrFcShort( 0xffc0 ),	/* Offset= -64 (1316) */
/* 1382 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1384 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1386 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1388 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1390 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1392 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1394 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 1396 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1398 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1400 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1402 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1404 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1406 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1408 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1410 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1412 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1414 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1416 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1418 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1420 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1422 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1424 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1426 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1428 */	0x13, 0x0,	/* FC_OP */
/* 1430 */	NdrFcShort( 0xff8e ),	/* Offset= -114 (1316) */
/* 1432 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1434 */	0x0,		/* 0 */
			NdrFcShort( 0xffa7 ),	/* Offset= -89 (1346) */
			0x5b,		/* FC_END */
/* 1438 */	
			0x11, 0x0,	/* FC_RP */
/* 1440 */	NdrFcShort( 0x21e ),	/* Offset= 542 (1982) */
/* 1442 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1444 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1446 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1448 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1450 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1452 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1454 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1456 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1458 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1460 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1462 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1464 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1466 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1468 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1470 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1472 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1474 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1476 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1478 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1480 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1482 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1484 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1486 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1488 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1490 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1492 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1494 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1496 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1498 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1500 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1502 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1504 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1506 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1508 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1510 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1512 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1514 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1516 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1518 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1520 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1522 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1524 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1526 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1528 */	0x0,		/* 0 */
			NdrFcShort( 0xffa9 ),	/* Offset= -87 (1442) */
			0x5b,		/* FC_END */
/* 1532 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1534 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1536 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1538 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1540 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1542 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1544 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1546 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1548 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1550 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1552 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1554 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1556 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1558 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1560 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1562 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1564 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1566 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1568 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1570 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1572 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1574 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1576 */	0x0,		/* 0 */
			NdrFcShort( 0xff79 ),	/* Offset= -135 (1442) */
			0x5b,		/* FC_END */
/* 1580 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1582 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1584 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1586 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1588 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1590 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1592 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1594 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1596 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1598 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1600 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1602 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1604 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1606 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1608 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1610 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1612 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1614 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1616 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1618 */	NdrFcShort( 0xc ),	/* 12 */
/* 1620 */	NdrFcShort( 0xc ),	/* 12 */
/* 1622 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1624 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1626 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1628 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1630 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1632 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1634 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1636 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1638 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1640 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1642 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1644 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1646 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1648 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1650 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1652 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1654 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1656 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1658 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1660 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1662 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1664 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1666 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1668 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1670 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1672 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1674 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1676 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1678 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1680 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1682 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1684 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1686 */	NdrFcShort( 0xc ),	/* 12 */
/* 1688 */	NdrFcShort( 0xc ),	/* 12 */
/* 1690 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1692 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1694 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1696 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1698 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1700 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1702 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1704 */	0x0,		/* 0 */
			NdrFcShort( 0xff83 ),	/* Offset= -125 (1580) */
			0x5b,		/* FC_END */
/* 1708 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1710 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1712 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1714 */	NdrFcShort( 0x28 ),	/* 40 */
/* 1716 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1718 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1720 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1722 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1724 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1726 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1728 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1730 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1732 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1734 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1736 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1738 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1740 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1742 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1744 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1746 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1748 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1750 */	NdrFcShort( 0xc ),	/* 12 */
/* 1752 */	NdrFcShort( 0xc ),	/* 12 */
/* 1754 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1756 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1758 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1760 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1762 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1764 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1766 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1768 */	0x0,		/* 0 */
			NdrFcShort( 0xff43 ),	/* Offset= -189 (1580) */
			0x5b,		/* FC_END */
/* 1772 */	
			0x13, 0x0,	/* FC_OP */
/* 1774 */	NdrFcShort( 0xfce4 ),	/* Offset= -796 (978) */
/* 1776 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1778 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1780 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1782 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1784 */	NdrFcShort( 0xfff4 ),	/* Offset= -12 (1772) */
/* 1786 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1788 */	NdrFcShort( 0x48 ),	/* 72 */
/* 1790 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1792 */	NdrFcShort( 0x16 ),	/* Offset= 22 (1814) */
/* 1794 */	0x36,		/* FC_POINTER */
			0x6,		/* FC_SHORT */
/* 1796 */	0x6,		/* FC_SHORT */
			0x8,		/* FC_LONG */
/* 1798 */	0x36,		/* FC_POINTER */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1800 */	0x0,		/* 0 */
			NdrFcShort( 0xffe7 ),	/* Offset= -25 (1776) */
			0x36,		/* FC_POINTER */
/* 1804 */	0x8,		/* FC_LONG */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1806 */	0x0,		/* 0 */
			NdrFcShort( 0xffe1 ),	/* Offset= -31 (1776) */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1810 */	0x0,		/* 0 */
			NdrFcShort( 0xffdd ),	/* Offset= -35 (1776) */
			0x5b,		/* FC_END */
/* 1814 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1816 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1818 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1820 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1822 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1824 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1826 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1828 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1830 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1832 */	NdrFcShort( 0x30 ),	/* 48 */
/* 1834 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1838 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1840 */	NdrFcShort( 0xffca ),	/* Offset= -54 (1786) */
/* 1842 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1844 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1846 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1848 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1850 */	NdrFcShort( 0x38 ),	/* 56 */
/* 1852 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1856 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1858 */	NdrFcShort( 0xffb8 ),	/* Offset= -72 (1786) */
/* 1860 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1862 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1864 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1866 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1868 */	NdrFcShort( 0x40 ),	/* 64 */
/* 1870 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1872 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1874 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1876 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1878 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1880 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1882 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1884 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1886 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1888 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1890 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1892 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1894 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1896 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1898 */	NdrFcShort( 0x48 ),	/* 72 */
/* 1900 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1902 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1904 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1906 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1908 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1910 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1912 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1914 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1916 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1918 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1920 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1922 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1924 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1926 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1928 */	NdrFcShort( 0x50 ),	/* 80 */
/* 1930 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1932 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1934 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1936 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1938 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1940 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1942 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1944 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1946 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1948 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1950 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1952 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1954 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1956 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1958 */	NdrFcShort( 0x58 ),	/* 88 */
/* 1960 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1962 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1964 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1966 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1968 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1970 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1972 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1974 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1976 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1978 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1980 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1982 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x7,		/* 7 */
/* 1984 */	NdrFcShort( 0x60 ),	/* 96 */
/* 1986 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1988 */	NdrFcShort( 0x1a ),	/* Offset= 26 (2014) */
/* 1990 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 1992 */	0xc,		/* FC_DOUBLE */
			0x8,		/* FC_LONG */
/* 1994 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 1996 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 1998 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 2000 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 2002 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 2004 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 2006 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 2008 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 2010 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 2012 */	0x36,		/* FC_POINTER */
			0x5b,		/* FC_END */
/* 2014 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 2016 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 2018 */	
			0x13, 0x0,	/* FC_OP */
/* 2020 */	NdrFcShort( 0xfde8 ),	/* Offset= -536 (1484) */
/* 2022 */	
			0x13, 0x0,	/* FC_OP */
/* 2024 */	NdrFcShort( 0xfe14 ),	/* Offset= -492 (1532) */
/* 2026 */	
			0x13, 0x0,	/* FC_OP */
/* 2028 */	NdrFcShort( 0xfe80 ),	/* Offset= -384 (1644) */
/* 2030 */	
			0x13, 0x0,	/* FC_OP */
/* 2032 */	NdrFcShort( 0xfebc ),	/* Offset= -324 (1708) */
/* 2034 */	
			0x13, 0x0,	/* FC_OP */
/* 2036 */	NdrFcShort( 0xff2e ),	/* Offset= -210 (1826) */
/* 2038 */	
			0x13, 0x0,	/* FC_OP */
/* 2040 */	NdrFcShort( 0xff3c ),	/* Offset= -196 (1844) */
/* 2042 */	
			0x13, 0x0,	/* FC_OP */
/* 2044 */	NdrFcShort( 0xff4a ),	/* Offset= -182 (1862) */
/* 2046 */	
			0x13, 0x0,	/* FC_OP */
/* 2048 */	NdrFcShort( 0xff64 ),	/* Offset= -156 (1892) */
/* 2050 */	
			0x13, 0x0,	/* FC_OP */
/* 2052 */	NdrFcShort( 0xff7e ),	/* Offset= -130 (1922) */
/* 2054 */	
			0x13, 0x0,	/* FC_OP */
/* 2056 */	NdrFcShort( 0xff98 ),	/* Offset= -104 (1952) */
/* 2058 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 2060 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 2062 */	
			0x11, 0x0,	/* FC_RP */
/* 2064 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2066) */
/* 2066 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 2068 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2070 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 2072 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2074 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 2078 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 2080 */	NdrFcShort( 0xfbd0 ),	/* Offset= -1072 (1008) */
/* 2082 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 2084 */	
			0x11, 0x0,	/* FC_RP */
/* 2086 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2088) */
/* 2088 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 2090 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2092 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 2094 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2096 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 2098 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 2100 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2102 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2104 */	NdrFcShort( 0x1 ),	/* 1 */
/* 2106 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2108 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2110 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 2112 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 2114 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 2116 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 2118 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 2120 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2122) */
/* 2122 */	
			0x13, 0x0,	/* FC_OP */
/* 2124 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2126) */
/* 2126 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 2128 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2130 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 2132 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 2134 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 2138 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 2140 */	NdrFcShort( 0xfbcc ),	/* Offset= -1076 (1064) */
/* 2142 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 2144 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 2146 */	NdrFcLong( 0x3104b527 ),	/* 822392103 */
/* 2150 */	NdrFcShort( 0x2016 ),	/* 8214 */
/* 2152 */	NdrFcShort( 0x442d ),	/* 17453 */
/* 2154 */	0x96,		/* 150 */
			0x96,		/* 150 */
/* 2156 */	0x12,		/* 18 */
			0x75,		/* 117 */
/* 2158 */	0xde,		/* 222 */
			0x97,		/* 151 */
/* 2160 */	0x87,		/* 135 */
			0x78,		/* 120 */
/* 2162 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 2164 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2166) */
/* 2166 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 2168 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 2170 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 2172 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2174) */
/* 2174 */	
			0x13, 0x0,	/* FC_OP */
/* 2176 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2178) */
/* 2178 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 2180 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2182 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 2184 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2186 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 2190 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 2192 */	NdrFcShort( 0xfb98 ),	/* Offset= -1128 (1064) */
/* 2194 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 2196 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 2198 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2200) */
/* 2200 */	
			0x13, 0x0,	/* FC_OP */
/* 2202 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2204) */
/* 2204 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 2206 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2208 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 2210 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2212 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 2214 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 2216 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2218 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2220 */	NdrFcShort( 0x1 ),	/* 1 */
/* 2222 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2224 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2226 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 2228 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 2230 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 2232 */	0x5c,		/* FC_PAD */
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


/* Object interface: CATID_OPCCMDServer10, ver. 0.0,
   GUID={0x2D869D5C,0x3B05,0x41fb,{0x85,0x1A,0x64,0x2F,0xB2,0xB8,0x01,0xA0}} */

#pragma code_seg(".orpc")
static const unsigned short CATID_OPCCMDServer10_FormatStringOffsetTable[] =
    {
    0
    };

static const MIDL_STUBLESS_PROXY_INFO CATID_OPCCMDServer10_ProxyInfo =
    {
    &Object_StubDesc,
    OpcCmd__MIDL_ProcFormatString.Format,
    &CATID_OPCCMDServer10_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO CATID_OPCCMDServer10_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    OpcCmd__MIDL_ProcFormatString.Format,
    &CATID_OPCCMDServer10_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(3) _CATID_OPCCMDServer10ProxyVtbl = 
{
    0,
    &IID_CATID_OPCCMDServer10,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy
};

const CInterfaceStubVtbl _CATID_OPCCMDServer10StubVtbl =
{
    &IID_CATID_OPCCMDServer10,
    &CATID_OPCCMDServer10_ServerInfo,
    3,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Standard interface: __MIDL_itf_OpcCmd_0000_0001, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: IOPCComandCallback, ver. 0.0,
   GUID={0x3104B527,0x2016,0x442d,{0x96,0x96,0x12,0x75,0xDE,0x97,0x87,0x78}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCComandCallback_FormatStringOffsetTable[] =
    {
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCComandCallback_ProxyInfo =
    {
    &Object_StubDesc,
    OpcCmd__MIDL_ProcFormatString.Format,
    &IOPCComandCallback_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCComandCallback_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    OpcCmd__MIDL_ProcFormatString.Format,
    &IOPCComandCallback_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(4) _IOPCComandCallbackProxyVtbl = 
{
    &IOPCComandCallback_ProxyInfo,
    &IID_IOPCComandCallback,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCComandCallback::OnStateChange */
};

const CInterfaceStubVtbl _IOPCComandCallbackStubVtbl =
{
    &IID_IOPCComandCallback,
    &IOPCComandCallback_ServerInfo,
    4,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCCommandInformation, ver. 0.0,
   GUID={0x3104B525,0x2016,0x442d,{0x96,0x96,0x12,0x75,0xDE,0x97,0x87,0x78}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCCommandInformation_FormatStringOffsetTable[] =
    {
    52,
    86,
    120,
    172
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCCommandInformation_ProxyInfo =
    {
    &Object_StubDesc,
    OpcCmd__MIDL_ProcFormatString.Format,
    &IOPCCommandInformation_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCCommandInformation_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    OpcCmd__MIDL_ProcFormatString.Format,
    &IOPCCommandInformation_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(7) _IOPCCommandInformationProxyVtbl = 
{
    &IOPCCommandInformation_ProxyInfo,
    &IID_IOPCCommandInformation,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCCommandInformation::QueryCapabilities */ ,
    (void *) (INT_PTR) -1 /* IOPCCommandInformation::QueryComands */ ,
    (void *) (INT_PTR) -1 /* IOPCCommandInformation::BrowseCommandTargets */ ,
    (void *) (INT_PTR) -1 /* IOPCCommandInformation::GetCommandDescription */
};

const CInterfaceStubVtbl _IOPCCommandInformationStubVtbl =
{
    &IID_IOPCCommandInformation,
    &IOPCCommandInformation_ServerInfo,
    7,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCCommandExecution, ver. 0.0,
   GUID={0x3104B526,0x2016,0x442d,{0x96,0x96,0x12,0x75,0xDE,0x97,0x87,0x78}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCCommandExecution_FormatStringOffsetTable[] =
    {
    212,
    288,
    382,
    434,
    462,
    526
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCCommandExecution_ProxyInfo =
    {
    &Object_StubDesc,
    OpcCmd__MIDL_ProcFormatString.Format,
    &IOPCCommandExecution_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCCommandExecution_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    OpcCmd__MIDL_ProcFormatString.Format,
    &IOPCCommandExecution_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(9) _IOPCCommandExecutionProxyVtbl = 
{
    &IOPCCommandExecution_ProxyInfo,
    &IID_IOPCCommandExecution,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCCommandExecution::SyncInvoke */ ,
    (void *) (INT_PTR) -1 /* IOPCCommandExecution::AsyncInvoke */ ,
    (void *) (INT_PTR) -1 /* IOPCCommandExecution::Connect */ ,
    (void *) (INT_PTR) -1 /* IOPCCommandExecution::Disconnect */ ,
    (void *) (INT_PTR) -1 /* IOPCCommandExecution::QueryState */ ,
    (void *) (INT_PTR) -1 /* IOPCCommandExecution::Control */
};

const CInterfaceStubVtbl _IOPCCommandExecutionStubVtbl =
{
    &IID_IOPCCommandExecution,
    &IOPCCommandExecution_ServerInfo,
    9,
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
    OpcCmd__MIDL_TypeFormatString.Format,
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

const CInterfaceProxyVtbl * const _OpcCmd_ProxyVtblList[] = 
{
    ( CInterfaceProxyVtbl *) &_IOPCCommandInformationProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCCommandExecutionProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCComandCallbackProxyVtbl,
    ( CInterfaceProxyVtbl *) &_CATID_OPCCMDServer10ProxyVtbl,
    0
};

const CInterfaceStubVtbl * const _OpcCmd_StubVtblList[] = 
{
    ( CInterfaceStubVtbl *) &_IOPCCommandInformationStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCCommandExecutionStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCComandCallbackStubVtbl,
    ( CInterfaceStubVtbl *) &_CATID_OPCCMDServer10StubVtbl,
    0
};

PCInterfaceName const _OpcCmd_InterfaceNamesList[] = 
{
    "IOPCCommandInformation",
    "IOPCCommandExecution",
    "IOPCComandCallback",
    "CATID_OPCCMDServer10",
    0
};


#define _OpcCmd_CHECK_IID(n)	IID_GENERIC_CHECK_IID( _OpcCmd, pIID, n)

int __stdcall _OpcCmd_IID_Lookup( const IID * pIID, int * pIndex )
{
    IID_BS_LOOKUP_SETUP

    IID_BS_LOOKUP_INITIAL_TEST( _OpcCmd, 4, 2 )
    IID_BS_LOOKUP_NEXT_TEST( _OpcCmd, 1 )
    IID_BS_LOOKUP_RETURN_RESULT( _OpcCmd, 4, *pIndex )
    
}

const ExtendedProxyFileInfo OpcCmd_ProxyFileInfo = 
{
    (PCInterfaceProxyVtblList *) & _OpcCmd_ProxyVtblList,
    (PCInterfaceStubVtblList *) & _OpcCmd_StubVtblList,
    (const PCInterfaceName * ) & _OpcCmd_InterfaceNamesList,
    0, /* no delegation */
    & _OpcCmd_IID_Lookup, 
    4,
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

