

/* this ALWAYS GENERATED file contains the proxy stub code */


 /* File created by MIDL compiler version 8.00.0603 */
/* at Tue Jun 30 10:35:10 2015
 */
/* Compiler settings for opcda.idl:
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


#include "opcda.h"

#define TYPE_FORMAT_STRING_SIZE   2157                              
#define PROC_FORMAT_STRING_SIZE   2957                              
#define EXPR_FORMAT_STRING_SIZE   1                                 
#define TRANSMIT_AS_TABLE_SIZE    0            
#define WIRE_MARSHAL_TABLE_SIZE   1            

typedef struct _opcda_MIDL_TYPE_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ TYPE_FORMAT_STRING_SIZE ];
    } opcda_MIDL_TYPE_FORMAT_STRING;

typedef struct _opcda_MIDL_PROC_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ PROC_FORMAT_STRING_SIZE ];
    } opcda_MIDL_PROC_FORMAT_STRING;

typedef struct _opcda_MIDL_EXPR_FORMAT_STRING
    {
    long          Pad;
    unsigned char  Format[ EXPR_FORMAT_STRING_SIZE ];
    } opcda_MIDL_EXPR_FORMAT_STRING;


static const RPC_SYNTAX_IDENTIFIER  _RpcTransferSyntax = 
{{0x8A885D04,0x1CEB,0x11C9,{0x9F,0xE8,0x08,0x00,0x2B,0x10,0x48,0x60}},{2,0}};


extern const opcda_MIDL_TYPE_FORMAT_STRING opcda__MIDL_TypeFormatString;
extern const opcda_MIDL_PROC_FORMAT_STRING opcda__MIDL_ProcFormatString;
extern const opcda_MIDL_EXPR_FORMAT_STRING opcda__MIDL_ExprFormatString;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO CATID_OPCDAServer10_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO CATID_OPCDAServer10_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO CATID_OPCDAServer20_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO CATID_OPCDAServer20_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO CATID_OPCDAServer30_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO CATID_OPCDAServer30_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO CATID_XMLDAServer10_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO CATID_XMLDAServer10_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCServer_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCServer_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCServerPublicGroups_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCServerPublicGroups_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCBrowseServerAddressSpace_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCBrowseServerAddressSpace_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCGroupStateMgt_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCGroupStateMgt_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCPublicGroupStateMgt_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCPublicGroupStateMgt_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCSyncIO_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCSyncIO_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCAsyncIO_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCAsyncIO_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCItemMgt_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCItemMgt_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IEnumOPCItemAttributes_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IEnumOPCItemAttributes_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCDataCallback_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCDataCallback_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCAsyncIO2_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCAsyncIO2_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCItemProperties_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCItemProperties_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCItemDeadbandMgt_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCItemDeadbandMgt_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCItemSamplingMgt_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCItemSamplingMgt_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCBrowse_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCBrowse_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCItemIO_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCItemIO_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCSyncIO2_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCSyncIO2_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCAsyncIO3_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCAsyncIO3_ProxyInfo;


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCGroupStateMgt2_ServerInfo;
extern const MIDL_STUBLESS_PROXY_INFO IOPCGroupStateMgt2_ProxyInfo;


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


static const opcda_MIDL_PROC_FORMAT_STRING opcda__MIDL_ProcFormatString =
    {
        0,
        {

	/* Procedure AddGroup */

			0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/*  2 */	NdrFcLong( 0x0 ),	/* 0 */
/*  6 */	NdrFcShort( 0x3 ),	/* 3 */
/*  8 */	NdrFcShort( 0x34 ),	/* x86 Stack size/offset = 52 */
/* 10 */	NdrFcShort( 0x9c ),	/* 156 */
/* 12 */	NdrFcShort( 0x40 ),	/* 64 */
/* 14 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0xc,		/* 12 */

	/* Parameter szName */

/* 16 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 18 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 20 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter bActive */

/* 22 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 24 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 26 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwRequestedUpdateRate */

/* 28 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 30 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 32 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hClientGroup */

/* 34 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 36 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 38 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pTimeBias */

/* 40 */	NdrFcShort( 0xa ),	/* Flags:  must free, in, */
/* 42 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 44 */	NdrFcShort( 0x6 ),	/* Type Offset=6 */

	/* Parameter pPercentDeadband */

/* 46 */	NdrFcShort( 0xa ),	/* Flags:  must free, in, */
/* 48 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 50 */	NdrFcShort( 0xa ),	/* Type Offset=10 */

	/* Parameter dwLCID */

/* 52 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 54 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 56 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServerGroup */

/* 58 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 60 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 62 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pRevisedUpdateRate */

/* 64 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 66 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 68 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter riid */

/* 70 */	NdrFcShort( 0x10a ),	/* Flags:  must free, in, simple ref, */
/* 72 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 74 */	NdrFcShort( 0x1c ),	/* Type Offset=28 */

	/* Parameter ppUnk */

/* 76 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 78 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 80 */	NdrFcShort( 0x28 ),	/* Type Offset=40 */

	/* Return value */

/* 82 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 84 */	NdrFcShort( 0x30 ),	/* x86 Stack size/offset = 48 */
/* 86 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetErrorString */

/* 88 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 90 */	NdrFcLong( 0x0 ),	/* 0 */
/* 94 */	NdrFcShort( 0x4 ),	/* 4 */
/* 96 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 98 */	NdrFcShort( 0x10 ),	/* 16 */
/* 100 */	NdrFcShort( 0x8 ),	/* 8 */
/* 102 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwError */

/* 104 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 106 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 108 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwLocale */

/* 110 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 112 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 114 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppString */

/* 116 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 118 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 120 */	NdrFcShort( 0x32 ),	/* Type Offset=50 */

	/* Return value */

/* 122 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 124 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 126 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetGroupByName */

/* 128 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 130 */	NdrFcLong( 0x0 ),	/* 0 */
/* 134 */	NdrFcShort( 0x5 ),	/* 5 */
/* 136 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 138 */	NdrFcShort( 0x44 ),	/* 68 */
/* 140 */	NdrFcShort( 0x8 ),	/* 8 */
/* 142 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter szName */

/* 144 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 146 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 148 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter riid */

/* 150 */	NdrFcShort( 0x10a ),	/* Flags:  must free, in, simple ref, */
/* 152 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 154 */	NdrFcShort( 0x1c ),	/* Type Offset=28 */

	/* Parameter ppUnk */

/* 156 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 158 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 160 */	NdrFcShort( 0x3a ),	/* Type Offset=58 */

	/* Return value */

/* 162 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 164 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 166 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetStatus */

/* 168 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 170 */	NdrFcLong( 0x0 ),	/* 0 */
/* 174 */	NdrFcShort( 0x6 ),	/* 6 */
/* 176 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 178 */	NdrFcShort( 0x0 ),	/* 0 */
/* 180 */	NdrFcShort( 0x8 ),	/* 8 */
/* 182 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x2,		/* 2 */

	/* Parameter ppServerStatus */

/* 184 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 186 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 188 */	NdrFcShort( 0x44 ),	/* Type Offset=68 */

	/* Return value */

/* 190 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 192 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 194 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure RemoveGroup */

/* 196 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 198 */	NdrFcLong( 0x0 ),	/* 0 */
/* 202 */	NdrFcShort( 0x7 ),	/* 7 */
/* 204 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 206 */	NdrFcShort( 0x10 ),	/* 16 */
/* 208 */	NdrFcShort( 0x8 ),	/* 8 */
/* 210 */	0x4,		/* Oi2 Flags:  has return, */
			0x3,		/* 3 */

	/* Parameter hServerGroup */

/* 212 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 214 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 216 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter bForce */

/* 218 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 220 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 222 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 224 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 226 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 228 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CreateGroupEnumerator */

/* 230 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 232 */	NdrFcLong( 0x0 ),	/* 0 */
/* 236 */	NdrFcShort( 0x8 ),	/* 8 */
/* 238 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 240 */	NdrFcShort( 0x4a ),	/* 74 */
/* 242 */	NdrFcShort( 0x8 ),	/* 8 */
/* 244 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwScope */

/* 246 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 248 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 250 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter riid */

/* 252 */	NdrFcShort( 0x10a ),	/* Flags:  must free, in, simple ref, */
/* 254 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 256 */	NdrFcShort( 0x1c ),	/* Type Offset=28 */

	/* Parameter ppUnk */

/* 258 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 260 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 262 */	NdrFcShort( 0x76 ),	/* Type Offset=118 */

	/* Return value */

/* 264 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 266 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 268 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetPublicGroupByName */

/* 270 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 272 */	NdrFcLong( 0x0 ),	/* 0 */
/* 276 */	NdrFcShort( 0x3 ),	/* 3 */
/* 278 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 280 */	NdrFcShort( 0x44 ),	/* 68 */
/* 282 */	NdrFcShort( 0x8 ),	/* 8 */
/* 284 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter szName */

/* 286 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 288 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 290 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter riid */

/* 292 */	NdrFcShort( 0x10a ),	/* Flags:  must free, in, simple ref, */
/* 294 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 296 */	NdrFcShort( 0x1c ),	/* Type Offset=28 */

	/* Parameter ppUnk */

/* 298 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 300 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 302 */	NdrFcShort( 0x80 ),	/* Type Offset=128 */

	/* Return value */

/* 304 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 306 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 308 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure RemovePublicGroup */

/* 310 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 312 */	NdrFcLong( 0x0 ),	/* 0 */
/* 316 */	NdrFcShort( 0x4 ),	/* 4 */
/* 318 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 320 */	NdrFcShort( 0x10 ),	/* 16 */
/* 322 */	NdrFcShort( 0x8 ),	/* 8 */
/* 324 */	0x4,		/* Oi2 Flags:  has return, */
			0x3,		/* 3 */

	/* Parameter hServerGroup */

/* 326 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 328 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 330 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter bForce */

/* 332 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 334 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 336 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 338 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 340 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 342 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryOrganization */

/* 344 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 346 */	NdrFcLong( 0x0 ),	/* 0 */
/* 350 */	NdrFcShort( 0x3 ),	/* 3 */
/* 352 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 354 */	NdrFcShort( 0x0 ),	/* 0 */
/* 356 */	NdrFcShort( 0x22 ),	/* 34 */
/* 358 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter pNameSpaceType */

/* 360 */	NdrFcShort( 0x2010 ),	/* Flags:  out, srv alloc size=8 */
/* 362 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 364 */	NdrFcShort( 0x8a ),	/* Type Offset=138 */

	/* Return value */

/* 366 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 368 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 370 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ChangeBrowsePosition */

/* 372 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 374 */	NdrFcLong( 0x0 ),	/* 0 */
/* 378 */	NdrFcShort( 0x4 ),	/* 4 */
/* 380 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 382 */	NdrFcShort( 0x6 ),	/* 6 */
/* 384 */	NdrFcShort( 0x8 ),	/* 8 */
/* 386 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter dwBrowseDirection */

/* 388 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 390 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 392 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter szString */

/* 394 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 396 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 398 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Return value */

/* 400 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 402 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 404 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure BrowseOPCItemIDs */

/* 406 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 408 */	NdrFcLong( 0x0 ),	/* 0 */
/* 412 */	NdrFcShort( 0x5 ),	/* 5 */
/* 414 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 416 */	NdrFcShort( 0x14 ),	/* 20 */
/* 418 */	NdrFcShort( 0x8 ),	/* 8 */
/* 420 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwBrowseFilterType */

/* 422 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 424 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 426 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter szFilterCriteria */

/* 428 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 430 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 432 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter vtDataTypeFilter */

/* 434 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 436 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 438 */	0x6,		/* FC_SHORT */
			0x0,		/* 0 */

	/* Parameter dwAccessRightsFilter */

/* 440 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 442 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 444 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppIEnumString */

/* 446 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 448 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 450 */	NdrFcShort( 0x8e ),	/* Type Offset=142 */

	/* Return value */

/* 452 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 454 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 456 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetItemID */

/* 458 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 460 */	NdrFcLong( 0x0 ),	/* 0 */
/* 464 */	NdrFcShort( 0x6 ),	/* 6 */
/* 466 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 468 */	NdrFcShort( 0x0 ),	/* 0 */
/* 470 */	NdrFcShort( 0x8 ),	/* 8 */
/* 472 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter szItemDataID */

/* 474 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 476 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 478 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter szItemID */

/* 480 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 482 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 484 */	NdrFcShort( 0x32 ),	/* Type Offset=50 */

	/* Return value */

/* 486 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 488 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 490 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure BrowseAccessPaths */

/* 492 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 494 */	NdrFcLong( 0x0 ),	/* 0 */
/* 498 */	NdrFcShort( 0x7 ),	/* 7 */
/* 500 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 502 */	NdrFcShort( 0x0 ),	/* 0 */
/* 504 */	NdrFcShort( 0x8 ),	/* 8 */
/* 506 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x3,		/* 3 */

	/* Parameter szItemID */

/* 508 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 510 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 512 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter ppIEnumString */

/* 514 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 516 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 518 */	NdrFcShort( 0x8e ),	/* Type Offset=142 */

	/* Return value */

/* 520 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 522 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 524 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetState */

/* 526 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 528 */	NdrFcLong( 0x0 ),	/* 0 */
/* 532 */	NdrFcShort( 0x3 ),	/* 3 */
/* 534 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 536 */	NdrFcShort( 0x0 ),	/* 0 */
/* 538 */	NdrFcShort( 0xcc ),	/* 204 */
/* 540 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x9,		/* 9 */

	/* Parameter pUpdateRate */

/* 542 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 544 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 546 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pActive */

/* 548 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 550 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 552 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppName */

/* 554 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 556 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 558 */	NdrFcShort( 0x32 ),	/* Type Offset=50 */

	/* Parameter pTimeBias */

/* 560 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 562 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 564 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pPercentDeadband */

/* 566 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 568 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 570 */	0xa,		/* FC_FLOAT */
			0x0,		/* 0 */

	/* Parameter pLCID */

/* 572 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 574 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 576 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phClientGroup */

/* 578 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 580 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 582 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServerGroup */

/* 584 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 586 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 588 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 590 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 592 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 594 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetState */

/* 596 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 598 */	NdrFcLong( 0x0 ),	/* 0 */
/* 602 */	NdrFcShort( 0x4 ),	/* 4 */
/* 604 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 606 */	NdrFcShort( 0xa8 ),	/* 168 */
/* 608 */	NdrFcShort( 0x24 ),	/* 36 */
/* 610 */	0x4,		/* Oi2 Flags:  has return, */
			0x8,		/* 8 */

	/* Parameter pRequestedUpdateRate */

/* 612 */	NdrFcShort( 0xa ),	/* Flags:  must free, in, */
/* 614 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 616 */	NdrFcShort( 0x6 ),	/* Type Offset=6 */

	/* Parameter pRevisedUpdateRate */

/* 618 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 620 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 622 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pActive */

/* 624 */	NdrFcShort( 0xa ),	/* Flags:  must free, in, */
/* 626 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 628 */	NdrFcShort( 0x6 ),	/* Type Offset=6 */

	/* Parameter pTimeBias */

/* 630 */	NdrFcShort( 0xa ),	/* Flags:  must free, in, */
/* 632 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 634 */	NdrFcShort( 0x6 ),	/* Type Offset=6 */

	/* Parameter pPercentDeadband */

/* 636 */	NdrFcShort( 0xa ),	/* Flags:  must free, in, */
/* 638 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 640 */	NdrFcShort( 0xa ),	/* Type Offset=10 */

	/* Parameter pLCID */

/* 642 */	NdrFcShort( 0xa ),	/* Flags:  must free, in, */
/* 644 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 646 */	NdrFcShort( 0x6 ),	/* Type Offset=6 */

	/* Parameter phClientGroup */

/* 648 */	NdrFcShort( 0xa ),	/* Flags:  must free, in, */
/* 650 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 652 */	NdrFcShort( 0x6 ),	/* Type Offset=6 */

	/* Return value */

/* 654 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 656 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 658 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetName */

/* 660 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 662 */	NdrFcLong( 0x0 ),	/* 0 */
/* 666 */	NdrFcShort( 0x5 ),	/* 5 */
/* 668 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 670 */	NdrFcShort( 0x0 ),	/* 0 */
/* 672 */	NdrFcShort( 0x8 ),	/* 8 */
/* 674 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x2,		/* 2 */

	/* Parameter szName */

/* 676 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 678 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 680 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Return value */

/* 682 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 684 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 686 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CloneGroup */

/* 688 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 690 */	NdrFcLong( 0x0 ),	/* 0 */
/* 694 */	NdrFcShort( 0x6 ),	/* 6 */
/* 696 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 698 */	NdrFcShort( 0x44 ),	/* 68 */
/* 700 */	NdrFcShort( 0x8 ),	/* 8 */
/* 702 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter szName */

/* 704 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 706 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 708 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter riid */

/* 710 */	NdrFcShort( 0x10a ),	/* Flags:  must free, in, simple ref, */
/* 712 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 714 */	NdrFcShort( 0x1c ),	/* Type Offset=28 */

	/* Parameter ppUnk */

/* 716 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 718 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 720 */	NdrFcShort( 0xa8 ),	/* Type Offset=168 */

	/* Return value */

/* 722 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 724 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 726 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetState */

/* 728 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 730 */	NdrFcLong( 0x0 ),	/* 0 */
/* 734 */	NdrFcShort( 0x3 ),	/* 3 */
/* 736 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 738 */	NdrFcShort( 0x0 ),	/* 0 */
/* 740 */	NdrFcShort( 0x24 ),	/* 36 */
/* 742 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter pPublic */

/* 744 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 746 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 748 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 750 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 752 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 754 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure MoveToPublic */

/* 756 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 758 */	NdrFcLong( 0x0 ),	/* 0 */
/* 762 */	NdrFcShort( 0x4 ),	/* 4 */
/* 764 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 766 */	NdrFcShort( 0x0 ),	/* 0 */
/* 768 */	NdrFcShort( 0x8 ),	/* 8 */
/* 770 */	0x4,		/* Oi2 Flags:  has return, */
			0x1,		/* 1 */

	/* Return value */

/* 772 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 774 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 776 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Read */

/* 778 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 780 */	NdrFcLong( 0x0 ),	/* 0 */
/* 784 */	NdrFcShort( 0x3 ),	/* 3 */
/* 786 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 788 */	NdrFcShort( 0xe ),	/* 14 */
/* 790 */	NdrFcShort( 0x8 ),	/* 8 */
/* 792 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwSource */

/* 794 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 796 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 798 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 800 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 802 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 804 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 806 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 808 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 810 */	NdrFcShort( 0xb6 ),	/* Type Offset=182 */

	/* Parameter ppItemValues */

/* 812 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 814 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 816 */	NdrFcShort( 0xc0 ),	/* Type Offset=192 */

	/* Parameter ppErrors */

/* 818 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 820 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 822 */	NdrFcShort( 0x4be ),	/* Type Offset=1214 */

	/* Return value */

/* 824 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 826 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 828 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Write */

/* 830 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 832 */	NdrFcLong( 0x0 ),	/* 0 */
/* 836 */	NdrFcShort( 0x4 ),	/* 4 */
/* 838 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 840 */	NdrFcShort( 0x8 ),	/* 8 */
/* 842 */	NdrFcShort( 0x8 ),	/* 8 */
/* 844 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 846 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 848 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 850 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 852 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 854 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 856 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter pItemValues */

/* 858 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 860 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 862 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter ppErrors */

/* 864 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 866 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 868 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 870 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 872 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 874 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Read */

/* 876 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 878 */	NdrFcLong( 0x0 ),	/* 0 */
/* 882 */	NdrFcShort( 0x3 ),	/* 3 */
/* 884 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 886 */	NdrFcShort( 0x16 ),	/* 22 */
/* 888 */	NdrFcShort( 0x24 ),	/* 36 */
/* 890 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwConnection */

/* 892 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 894 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 896 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwSource */

/* 898 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 900 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 902 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 904 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 906 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 908 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 910 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 912 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 914 */	NdrFcShort( 0x504 ),	/* Type Offset=1284 */

	/* Parameter pTransactionID */

/* 916 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 918 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 920 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 922 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 924 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 926 */	NdrFcShort( 0x50e ),	/* Type Offset=1294 */

	/* Return value */

/* 928 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 930 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 932 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Write */

/* 934 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 936 */	NdrFcLong( 0x0 ),	/* 0 */
/* 940 */	NdrFcShort( 0x4 ),	/* 4 */
/* 942 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 944 */	NdrFcShort( 0x10 ),	/* 16 */
/* 946 */	NdrFcShort( 0x24 ),	/* 36 */
/* 948 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwConnection */

/* 950 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 952 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 954 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 956 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 958 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 960 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 962 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 964 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 966 */	NdrFcShort( 0xb6 ),	/* Type Offset=182 */

	/* Parameter pItemValues */

/* 968 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 970 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 972 */	NdrFcShort( 0x51a ),	/* Type Offset=1306 */

	/* Parameter pTransactionID */

/* 974 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 976 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 978 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 980 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 982 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 984 */	NdrFcShort( 0x4be ),	/* Type Offset=1214 */

	/* Return value */

/* 986 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 988 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 990 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Refresh */

/* 992 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 994 */	NdrFcLong( 0x0 ),	/* 0 */
/* 998 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1000 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1002 */	NdrFcShort( 0xe ),	/* 14 */
/* 1004 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1006 */	0x4,		/* Oi2 Flags:  has return, */
			0x4,		/* 4 */

	/* Parameter dwConnection */

/* 1008 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1010 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1012 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwSource */

/* 1014 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1016 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1018 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter pTransactionID */

/* 1020 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1022 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1024 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1026 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1028 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1030 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Cancel2 */


	/* Procedure Cancel */

/* 1032 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1034 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1038 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1040 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1042 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1044 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1046 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter dwCancelID */


	/* Parameter dwTransactionID */

/* 1048 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1050 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1052 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */


	/* Return value */

/* 1054 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1056 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1058 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure AddItems */

/* 1060 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1062 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1066 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1068 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1070 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1072 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1074 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 1076 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1078 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1080 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pItemArray */

/* 1082 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1084 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1086 */	NdrFcShort( 0x568 ),	/* Type Offset=1384 */

	/* Parameter ppAddResults */

/* 1088 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1090 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1092 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Parameter ppErrors */

/* 1094 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1096 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1098 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 1100 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1102 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1104 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ValidateItems */

/* 1106 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1108 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1112 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1114 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1116 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1118 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1120 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwCount */

/* 1122 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1124 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1126 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pItemArray */

/* 1128 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1130 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1132 */	NdrFcShort( 0x568 ),	/* Type Offset=1384 */

	/* Parameter bBlobUpdate */

/* 1134 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1136 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1138 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppValidationResults */

/* 1140 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1142 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1144 */	NdrFcShort( 0x598 ),	/* Type Offset=1432 */

	/* Parameter ppErrors */

/* 1146 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1148 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1150 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 1152 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1154 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1156 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ClearItemSamplingRate */


	/* Procedure ClearItemDeadband */


	/* Procedure RemoveItems */

/* 1158 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1160 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1164 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1166 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1168 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1170 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1172 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x4,		/* 4 */

	/* Parameter dwCount */


	/* Parameter dwCount */


	/* Parameter dwCount */

/* 1174 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1176 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1178 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */


	/* Parameter phServer */


	/* Parameter phServer */

/* 1180 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1182 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1184 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter ppErrors */


	/* Parameter ppErrors */


	/* Parameter ppErrors */

/* 1186 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1188 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1190 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */


	/* Return value */


	/* Return value */

/* 1192 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1194 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1196 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetActiveState */

/* 1198 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1200 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1204 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1206 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1208 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1210 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1212 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 1214 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1216 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1218 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1220 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1222 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1224 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter bActive */

/* 1226 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1228 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1230 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1232 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1234 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1236 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 1238 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1240 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1242 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetClientHandles */

/* 1244 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1246 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1250 */	NdrFcShort( 0x7 ),	/* 7 */
/* 1252 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1254 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1256 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1258 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 1260 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1262 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1264 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1266 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1268 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1270 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter phClient */

/* 1272 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1274 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1276 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter ppErrors */

/* 1278 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1280 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1282 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 1284 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1286 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1288 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetDatatypes */

/* 1290 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1292 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1296 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1298 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1300 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1302 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1304 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 1306 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1308 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1310 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1312 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1314 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1316 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter pRequestedDatatypes */

/* 1318 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1320 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1322 */	NdrFcShort( 0x5e6 ),	/* Type Offset=1510 */

	/* Parameter ppErrors */

/* 1324 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1326 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1328 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 1330 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1332 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1334 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CreateEnumerator */

/* 1336 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1338 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1342 */	NdrFcShort( 0x9 ),	/* 9 */
/* 1344 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1346 */	NdrFcShort( 0x44 ),	/* 68 */
/* 1348 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1350 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x3,		/* 3 */

	/* Parameter riid */

/* 1352 */	NdrFcShort( 0x10a ),	/* Flags:  must free, in, simple ref, */
/* 1354 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1356 */	NdrFcShort( 0x1c ),	/* Type Offset=28 */

	/* Parameter ppUnk */

/* 1358 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 1360 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1362 */	NdrFcShort( 0x5f0 ),	/* Type Offset=1520 */

	/* Return value */

/* 1364 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1366 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1368 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Next */

/* 1370 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1372 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1376 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1378 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1380 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1382 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1384 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x4,		/* 4 */

	/* Parameter celt */

/* 1386 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1388 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1390 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppItemArray */

/* 1392 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1394 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1396 */	NdrFcShort( 0x5fa ),	/* Type Offset=1530 */

	/* Parameter pceltFetched */

/* 1398 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1400 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1402 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1404 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1406 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1408 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Skip */

/* 1410 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1412 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1416 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1418 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1420 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1422 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1424 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter celt */

/* 1426 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1428 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1430 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1432 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1434 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1436 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Reset */

/* 1438 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1440 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1444 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1446 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1448 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1450 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1452 */	0x4,		/* Oi2 Flags:  has return, */
			0x1,		/* 1 */

	/* Return value */

/* 1454 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1456 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1458 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Clone */

/* 1460 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1462 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1466 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1468 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1470 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1472 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1474 */	0x5,		/* Oi2 Flags:  srv must size, has return, */
			0x2,		/* 2 */

	/* Parameter ppEnumItemAttributes */

/* 1476 */	NdrFcShort( 0x13 ),	/* Flags:  must size, must free, out, */
/* 1478 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1480 */	NdrFcShort( 0x642 ),	/* Type Offset=1602 */

	/* Return value */

/* 1482 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1484 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1486 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnDataChange */

/* 1488 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1490 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1494 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1496 */	NdrFcShort( 0x30 ),	/* x86 Stack size/offset = 48 */
/* 1498 */	NdrFcShort( 0x28 ),	/* 40 */
/* 1500 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1502 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0xb,		/* 11 */

	/* Parameter dwTransid */

/* 1504 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1506 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1508 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hGroup */

/* 1510 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1512 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1514 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrMasterquality */

/* 1516 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1518 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1520 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrMastererror */

/* 1522 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1524 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1526 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 1528 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1530 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1532 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phClientItems */

/* 1534 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1536 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1538 */	NdrFcShort( 0x65c ),	/* Type Offset=1628 */

	/* Parameter pvValues */

/* 1540 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1542 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1544 */	NdrFcShort( 0x66a ),	/* Type Offset=1642 */

	/* Parameter pwQualities */

/* 1546 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1548 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1550 */	NdrFcShort( 0x680 ),	/* Type Offset=1664 */

	/* Parameter pftTimeStamps */

/* 1552 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1554 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 1556 */	NdrFcShort( 0x68e ),	/* Type Offset=1678 */

	/* Parameter pErrors */

/* 1558 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1560 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 1562 */	NdrFcShort( 0x65c ),	/* Type Offset=1628 */

	/* Return value */

/* 1564 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1566 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 1568 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnReadComplete */

/* 1570 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1572 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1576 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1578 */	NdrFcShort( 0x30 ),	/* x86 Stack size/offset = 48 */
/* 1580 */	NdrFcShort( 0x28 ),	/* 40 */
/* 1582 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1584 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0xb,		/* 11 */

	/* Parameter dwTransid */

/* 1586 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1588 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1590 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hGroup */

/* 1592 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1594 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1596 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrMasterquality */

/* 1598 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1600 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1602 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrMastererror */

/* 1604 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1606 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1608 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 1610 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1612 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1614 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phClientItems */

/* 1616 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1618 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1620 */	NdrFcShort( 0x65c ),	/* Type Offset=1628 */

	/* Parameter pvValues */

/* 1622 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1624 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1626 */	NdrFcShort( 0x66a ),	/* Type Offset=1642 */

	/* Parameter pwQualities */

/* 1628 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1630 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1632 */	NdrFcShort( 0x680 ),	/* Type Offset=1664 */

	/* Parameter pftTimeStamps */

/* 1634 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1636 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 1638 */	NdrFcShort( 0x68e ),	/* Type Offset=1678 */

	/* Parameter pErrors */

/* 1640 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1642 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 1644 */	NdrFcShort( 0x65c ),	/* Type Offset=1628 */

	/* Return value */

/* 1646 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1648 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 1650 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnWriteComplete */

/* 1652 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1654 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1658 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1660 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1662 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1664 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1666 */	0x6,		/* Oi2 Flags:  clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwTransid */

/* 1668 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1670 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1672 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hGroup */

/* 1674 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1676 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1678 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrMastererr */

/* 1680 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1682 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1684 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 1686 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1688 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1690 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pClienthandles */

/* 1692 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1694 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1696 */	NdrFcShort( 0x6a0 ),	/* Type Offset=1696 */

	/* Parameter pErrors */

/* 1698 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1700 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1702 */	NdrFcShort( 0x6a0 ),	/* Type Offset=1696 */

	/* Return value */

/* 1704 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1706 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1708 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnCancelComplete */

/* 1710 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1712 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1716 */	NdrFcShort( 0x6 ),	/* 6 */
/* 1718 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1720 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1722 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1724 */	0x4,		/* Oi2 Flags:  has return, */
			0x3,		/* 3 */

	/* Parameter dwTransid */

/* 1726 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1728 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1730 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hGroup */

/* 1732 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1734 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1736 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1738 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1740 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1742 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Read */

/* 1744 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1746 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1750 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1752 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1754 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1756 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1758 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwCount */

/* 1760 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1762 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1764 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1766 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1768 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1770 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter dwTransactionID */

/* 1772 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1774 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1776 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCancelID */

/* 1778 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1780 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1782 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1784 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1786 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1788 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 1790 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1792 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1794 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Write */

/* 1796 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1798 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1802 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1804 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 1806 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1808 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1810 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwCount */

/* 1812 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1814 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1816 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1818 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1820 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1822 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter pItemValues */

/* 1824 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1826 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1828 */	NdrFcShort( 0x4e6 ),	/* Type Offset=1254 */

	/* Parameter dwTransactionID */

/* 1830 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1832 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1834 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCancelID */

/* 1836 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1838 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1840 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1842 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1844 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 1846 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 1848 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1850 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1852 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Refresh2 */

/* 1854 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1856 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1860 */	NdrFcShort( 0x5 ),	/* 5 */
/* 1862 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1864 */	NdrFcShort( 0xe ),	/* 14 */
/* 1866 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1868 */	0x4,		/* Oi2 Flags:  has return, */
			0x4,		/* 4 */

	/* Parameter dwSource */

/* 1870 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1872 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1874 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter dwTransactionID */

/* 1876 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1878 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1880 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCancelID */

/* 1882 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1884 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1886 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1888 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1890 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1892 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetEnable */

/* 1894 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1896 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1900 */	NdrFcShort( 0x7 ),	/* 7 */
/* 1902 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1904 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1906 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1908 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter bEnable */

/* 1910 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 1912 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1914 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1916 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1918 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1920 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetKeepAlive */


	/* Procedure GetEnable */

/* 1922 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1924 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1928 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1930 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1932 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1934 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1936 */	0x4,		/* Oi2 Flags:  has return, */
			0x2,		/* 2 */

	/* Parameter pdwKeepAliveTime */


	/* Parameter pbEnable */

/* 1938 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1940 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1942 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */


	/* Return value */

/* 1944 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1946 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1948 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryAvailableProperties */

/* 1950 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 1952 */	NdrFcLong( 0x0 ),	/* 0 */
/* 1956 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1958 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 1960 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1962 */	NdrFcShort( 0x24 ),	/* 36 */
/* 1964 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter szItemID */

/* 1966 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 1968 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1970 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter pdwCount */

/* 1972 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 1974 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1976 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppPropertyIDs */

/* 1978 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1980 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1982 */	NdrFcShort( 0x6aa ),	/* Type Offset=1706 */

	/* Parameter ppDescriptions */

/* 1984 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1986 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1988 */	NdrFcShort( 0x6bc ),	/* Type Offset=1724 */

	/* Parameter ppvtDataTypes */

/* 1990 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 1992 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1994 */	NdrFcShort( 0x6e2 ),	/* Type Offset=1762 */

	/* Return value */

/* 1996 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 1998 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2000 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetItemProperties */

/* 2002 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2004 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2008 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2010 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2012 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2014 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2016 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter szItemID */

/* 2018 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2020 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2022 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter dwCount */

/* 2024 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2026 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2028 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwPropertyIDs */

/* 2030 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2032 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2034 */	NdrFcShort( 0xb6 ),	/* Type Offset=182 */

	/* Parameter ppvData */

/* 2036 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2038 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2040 */	NdrFcShort( 0x6f4 ),	/* Type Offset=1780 */

	/* Parameter ppErrors */

/* 2042 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2044 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2046 */	NdrFcShort( 0x4be ),	/* Type Offset=1214 */

	/* Return value */

/* 2048 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2050 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2052 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure LookupItemIDs */

/* 2054 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2056 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2060 */	NdrFcShort( 0x5 ),	/* 5 */
/* 2062 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2064 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2066 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2068 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter szItemID */

/* 2070 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2072 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2074 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter dwCount */

/* 2076 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2078 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2080 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwPropertyIDs */

/* 2082 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2084 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2086 */	NdrFcShort( 0xb6 ),	/* Type Offset=182 */

	/* Parameter ppszNewItemIDs */

/* 2088 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2090 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2092 */	NdrFcShort( 0x70e ),	/* Type Offset=1806 */

	/* Parameter ppErrors */

/* 2094 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2096 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2098 */	NdrFcShort( 0x4be ),	/* Type Offset=1214 */

	/* Return value */

/* 2100 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2102 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2104 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetItemDeadband */

/* 2106 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2108 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2112 */	NdrFcShort( 0x3 ),	/* 3 */
/* 2114 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2116 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2118 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2120 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 2122 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2124 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2126 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2128 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2130 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2132 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter pPercentDeadband */

/* 2134 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2136 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2138 */	NdrFcShort( 0x738 ),	/* Type Offset=1848 */

	/* Parameter ppErrors */

/* 2140 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2142 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2144 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 2146 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2148 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2150 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetItemDeadband */

/* 2152 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2154 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2158 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2160 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2162 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2164 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2166 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 2168 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2170 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2172 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2174 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2176 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2178 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter ppPercentDeadband */

/* 2180 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2182 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2184 */	NdrFcShort( 0x742 ),	/* Type Offset=1858 */

	/* Parameter ppErrors */

/* 2186 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2188 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2190 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 2192 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2194 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2196 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetItemSamplingRate */

/* 2198 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2200 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2204 */	NdrFcShort( 0x3 ),	/* 3 */
/* 2206 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2208 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2210 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2212 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x6,		/* 6 */

	/* Parameter dwCount */

/* 2214 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2216 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2218 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2220 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2222 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2224 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter pdwRequestedSamplingRate */

/* 2226 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2228 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2230 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter ppdwRevisedSamplingRate */

/* 2232 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2234 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2236 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Parameter ppErrors */

/* 2238 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2240 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2242 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 2244 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2246 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2248 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetItemSamplingRate */

/* 2250 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2252 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2256 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2258 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2260 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2262 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2264 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 2266 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2268 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2270 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2272 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2274 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2276 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter ppdwSamplingRate */

/* 2278 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2280 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2282 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Parameter ppErrors */

/* 2284 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2286 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2288 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 2290 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2292 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2294 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetItemBufferEnable */

/* 2296 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2298 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2302 */	NdrFcShort( 0x6 ),	/* 6 */
/* 2304 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2306 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2308 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2310 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 2312 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2314 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2316 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2318 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2320 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2322 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter pbEnable */

/* 2324 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2326 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2328 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter ppErrors */

/* 2330 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2332 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2334 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 2336 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2338 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2340 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetItemBufferEnable */

/* 2342 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2344 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2348 */	NdrFcShort( 0x7 ),	/* 7 */
/* 2350 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2352 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2354 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2356 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 2358 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2360 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2362 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2364 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2366 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2368 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter ppbEnable */

/* 2370 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2372 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2374 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Parameter ppErrors */

/* 2376 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2378 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2380 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 2382 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2384 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2386 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetProperties */

/* 2388 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2390 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2394 */	NdrFcShort( 0x3 ),	/* 3 */
/* 2396 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 2398 */	NdrFcShort( 0x18 ),	/* 24 */
/* 2400 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2402 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwItemCount */

/* 2404 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2406 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2408 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszItemIDs */

/* 2410 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2412 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2414 */	NdrFcShort( 0x74e ),	/* Type Offset=1870 */

	/* Parameter bReturnPropertyValues */

/* 2416 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2418 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2420 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwPropertyCount */

/* 2422 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2424 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2426 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwPropertyIDs */

/* 2428 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2430 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2432 */	NdrFcShort( 0x6a0 ),	/* Type Offset=1696 */

	/* Parameter ppItemProperties */

/* 2434 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2436 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2438 */	NdrFcShort( 0x76c ),	/* Type Offset=1900 */

	/* Return value */

/* 2440 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2442 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2444 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Browse */

/* 2446 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2448 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2452 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2454 */	NdrFcShort( 0x3c ),	/* x86 Stack size/offset = 60 */
/* 2456 */	NdrFcShort( 0x26 ),	/* 38 */
/* 2458 */	NdrFcShort( 0x40 ),	/* 64 */
/* 2460 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0xe,		/* 14 */

	/* Parameter szItemID */

/* 2462 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2464 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2466 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter pszContinuationPoint */

/* 2468 */	NdrFcShort( 0x201b ),	/* Flags:  must size, must free, in, out, srv alloc size=8 */
/* 2470 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2472 */	NdrFcShort( 0x32 ),	/* Type Offset=50 */

	/* Parameter dwMaxElementsReturned */

/* 2474 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2476 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2478 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwBrowseFilter */

/* 2480 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2482 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2484 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter szElementNameFilter */

/* 2486 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2488 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2490 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter szVendorFilter */

/* 2492 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2494 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2496 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter bReturnAllProperties */

/* 2498 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2500 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2502 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter bReturnPropertyValues */

/* 2504 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2506 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 2508 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwPropertyCount */

/* 2510 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2512 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 2514 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwPropertyIDs */

/* 2516 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2518 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 2520 */	NdrFcShort( 0x7ca ),	/* Type Offset=1994 */

	/* Parameter pbMoreElements */

/* 2522 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2524 */	NdrFcShort( 0x2c ),	/* x86 Stack size/offset = 44 */
/* 2526 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCount */

/* 2528 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2530 */	NdrFcShort( 0x30 ),	/* x86 Stack size/offset = 48 */
/* 2532 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppBrowseElements */

/* 2534 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2536 */	NdrFcShort( 0x34 ),	/* x86 Stack size/offset = 52 */
/* 2538 */	NdrFcShort( 0x7d4 ),	/* Type Offset=2004 */

	/* Return value */

/* 2540 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2542 */	NdrFcShort( 0x38 ),	/* x86 Stack size/offset = 56 */
/* 2544 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Read */

/* 2546 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2548 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2552 */	NdrFcShort( 0x3 ),	/* 3 */
/* 2554 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 2556 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2558 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2560 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x8,		/* 8 */

	/* Parameter dwCount */

/* 2562 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2564 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2566 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszItemIDs */

/* 2568 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2570 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2572 */	NdrFcShort( 0x74e ),	/* Type Offset=1870 */

	/* Parameter pdwMaxAge */

/* 2574 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2576 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2578 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter ppvValues */

/* 2580 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2582 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2584 */	NdrFcShort( 0x808 ),	/* Type Offset=2056 */

	/* Parameter ppwQualities */

/* 2586 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2588 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2590 */	NdrFcShort( 0x822 ),	/* Type Offset=2082 */

	/* Parameter ppftTimeStamps */

/* 2592 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2594 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2596 */	NdrFcShort( 0x82a ),	/* Type Offset=2090 */

	/* Parameter ppErrors */

/* 2598 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2600 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2602 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 2604 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2606 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 2608 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure WriteVQT */

/* 2610 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2612 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2616 */	NdrFcShort( 0x4 ),	/* 4 */
/* 2618 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2620 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2622 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2624 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 2626 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2628 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2630 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pszItemIDs */

/* 2632 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2634 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2636 */	NdrFcShort( 0x74e ),	/* Type Offset=1870 */

	/* Parameter pItemVQT */

/* 2638 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2640 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2642 */	NdrFcShort( 0x85a ),	/* Type Offset=2138 */

	/* Parameter ppErrors */

/* 2644 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2646 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2648 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 2650 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2652 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2654 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadMaxAge */

/* 2656 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2658 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2662 */	NdrFcShort( 0x5 ),	/* 5 */
/* 2664 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 2666 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2668 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2670 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x8,		/* 8 */

	/* Parameter dwCount */

/* 2672 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2674 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2676 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2678 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2680 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2682 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter pdwMaxAge */

/* 2684 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2686 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2688 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter ppvValues */

/* 2690 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2692 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2694 */	NdrFcShort( 0x808 ),	/* Type Offset=2056 */

	/* Parameter ppwQualities */

/* 2696 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2698 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2700 */	NdrFcShort( 0x822 ),	/* Type Offset=2082 */

	/* Parameter ppftTimeStamps */

/* 2702 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2704 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2706 */	NdrFcShort( 0x82a ),	/* Type Offset=2090 */

	/* Parameter ppErrors */

/* 2708 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2710 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2712 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 2714 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2716 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 2718 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure WriteVQT */

/* 2720 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2722 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2726 */	NdrFcShort( 0x6 ),	/* 6 */
/* 2728 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2730 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2732 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2734 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 2736 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2738 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2740 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2742 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2744 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2746 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter pItemVQT */

/* 2748 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2750 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2752 */	NdrFcShort( 0x85a ),	/* Type Offset=2138 */

	/* Parameter ppErrors */

/* 2754 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2756 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2758 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 2760 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2762 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2764 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ReadMaxAge */

/* 2766 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2768 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2772 */	NdrFcShort( 0x9 ),	/* 9 */
/* 2774 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 2776 */	NdrFcShort( 0x10 ),	/* 16 */
/* 2778 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2780 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwCount */

/* 2782 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2784 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2786 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2788 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2790 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2792 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter pdwMaxAge */

/* 2794 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2796 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2798 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter dwTransactionID */

/* 2800 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2802 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2804 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCancelID */

/* 2806 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2808 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2810 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 2812 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2814 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2816 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 2818 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2820 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2822 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure WriteVQT */

/* 2824 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2826 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2830 */	NdrFcShort( 0xa ),	/* 10 */
/* 2832 */	NdrFcShort( 0x20 ),	/* x86 Stack size/offset = 32 */
/* 2834 */	NdrFcShort( 0x10 ),	/* 16 */
/* 2836 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2838 */	0x7,		/* Oi2 Flags:  srv must size, clt must size, has return, */
			0x7,		/* 7 */

	/* Parameter dwCount */

/* 2840 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2842 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2844 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 2846 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2848 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2850 */	NdrFcShort( 0x4ca ),	/* Type Offset=1226 */

	/* Parameter pItemVQT */

/* 2852 */	NdrFcShort( 0x10b ),	/* Flags:  must size, must free, in, simple ref, */
/* 2854 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2856 */	NdrFcShort( 0x85a ),	/* Type Offset=2138 */

	/* Parameter dwTransactionID */

/* 2858 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2860 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2862 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCancelID */

/* 2864 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2866 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2868 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 2870 */	NdrFcShort( 0x2013 ),	/* Flags:  must size, must free, out, srv alloc size=8 */
/* 2872 */	NdrFcShort( 0x18 ),	/* x86 Stack size/offset = 24 */
/* 2874 */	NdrFcShort( 0x4f8 ),	/* Type Offset=1272 */

	/* Return value */

/* 2876 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2878 */	NdrFcShort( 0x1c ),	/* x86 Stack size/offset = 28 */
/* 2880 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure RefreshMaxAge */

/* 2882 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2884 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2888 */	NdrFcShort( 0xb ),	/* 11 */
/* 2890 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 2892 */	NdrFcShort( 0x10 ),	/* 16 */
/* 2894 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2896 */	0x4,		/* Oi2 Flags:  has return, */
			0x4,		/* 4 */

	/* Parameter dwMaxAge */

/* 2898 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2900 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2902 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwTransactionID */

/* 2904 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2906 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2908 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCancelID */

/* 2910 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2912 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2914 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 2916 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2918 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2920 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetKeepAlive */

/* 2922 */	0x33,		/* FC_AUTO_HANDLE */
			0x6c,		/* Old Flags:  object, Oi2 */
/* 2924 */	NdrFcLong( 0x0 ),	/* 0 */
/* 2928 */	NdrFcShort( 0x7 ),	/* 7 */
/* 2930 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 2932 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2934 */	NdrFcShort( 0x24 ),	/* 36 */
/* 2936 */	0x4,		/* Oi2 Flags:  has return, */
			0x3,		/* 3 */

	/* Parameter dwKeepAliveTime */

/* 2938 */	NdrFcShort( 0x48 ),	/* Flags:  in, base type, */
/* 2940 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2942 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwRevisedKeepAliveTime */

/* 2944 */	NdrFcShort( 0x2150 ),	/* Flags:  out, base type, simple ref, srv alloc size=8 */
/* 2946 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 2948 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 2950 */	NdrFcShort( 0x70 ),	/* Flags:  out, return, base type, */
/* 2952 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 2954 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

			0x0
        }
    };

static const opcda_MIDL_TYPE_FORMAT_STRING opcda__MIDL_TypeFormatString =
    {
        0,
        {
			NdrFcShort( 0x0 ),	/* 0 */
/*  2 */	
			0x11, 0x8,	/* FC_RP [simple_pointer] */
/*  4 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/*  6 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/*  8 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 10 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 12 */	0xa,		/* FC_FLOAT */
			0x5c,		/* FC_PAD */
/* 14 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 16 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 18 */	
			0x11, 0x0,	/* FC_RP */
/* 20 */	NdrFcShort( 0x8 ),	/* Offset= 8 (28) */
/* 22 */	
			0x1d,		/* FC_SMFARRAY */
			0x0,		/* 0 */
/* 24 */	NdrFcShort( 0x8 ),	/* 8 */
/* 26 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 28 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 30 */	NdrFcShort( 0x10 ),	/* 16 */
/* 32 */	0x8,		/* FC_LONG */
			0x6,		/* FC_SHORT */
/* 34 */	0x6,		/* FC_SHORT */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 36 */	0x0,		/* 0 */
			NdrFcShort( 0xfff1 ),	/* Offset= -15 (22) */
			0x5b,		/* FC_END */
/* 40 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 42 */	NdrFcShort( 0x2 ),	/* Offset= 2 (44) */
/* 44 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 46 */	0x28,		/* Corr desc:  parameter, FC_LONG */
			0x0,		/*  */
/* 48 */	NdrFcShort( 0x28 ),	/* x86 Stack size/offset = 40 */
/* 50 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 52 */	NdrFcShort( 0x2 ),	/* Offset= 2 (54) */
/* 54 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 56 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 58 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 60 */	NdrFcShort( 0x2 ),	/* Offset= 2 (62) */
/* 62 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 64 */	0x28,		/* Corr desc:  parameter, FC_LONG */
			0x0,		/*  */
/* 66 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 68 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 70 */	NdrFcShort( 0x2 ),	/* Offset= 2 (72) */
/* 72 */	
			0x13, 0x0,	/* FC_OP */
/* 74 */	NdrFcShort( 0xa ),	/* Offset= 10 (84) */
/* 76 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 78 */	NdrFcShort( 0x8 ),	/* 8 */
/* 80 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 82 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 84 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 86 */	NdrFcShort( 0x30 ),	/* 48 */
/* 88 */	NdrFcShort( 0x0 ),	/* 0 */
/* 90 */	NdrFcShort( 0x18 ),	/* Offset= 24 (114) */
/* 92 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 94 */	NdrFcShort( 0xffee ),	/* Offset= -18 (76) */
/* 96 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 98 */	NdrFcShort( 0xffea ),	/* Offset= -22 (76) */
/* 100 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 102 */	NdrFcShort( 0xffe6 ),	/* Offset= -26 (76) */
/* 104 */	0xd,		/* FC_ENUM16 */
			0x8,		/* FC_LONG */
/* 106 */	0x8,		/* FC_LONG */
			0x6,		/* FC_SHORT */
/* 108 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 110 */	0x6,		/* FC_SHORT */
			0x36,		/* FC_POINTER */
/* 112 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 114 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 116 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 118 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 120 */	NdrFcShort( 0x2 ),	/* Offset= 2 (122) */
/* 122 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 124 */	0x28,		/* Corr desc:  parameter, FC_LONG */
			0x0,		/*  */
/* 126 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 128 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 130 */	NdrFcShort( 0x2 ),	/* Offset= 2 (132) */
/* 132 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 134 */	0x28,		/* Corr desc:  parameter, FC_LONG */
			0x0,		/*  */
/* 136 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 138 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 140 */	0xd,		/* FC_ENUM16 */
			0x5c,		/* FC_PAD */
/* 142 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 144 */	NdrFcShort( 0x2 ),	/* Offset= 2 (146) */
/* 146 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 148 */	NdrFcLong( 0x101 ),	/* 257 */
/* 152 */	NdrFcShort( 0x0 ),	/* 0 */
/* 154 */	NdrFcShort( 0x0 ),	/* 0 */
/* 156 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 158 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 160 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 162 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 164 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 166 */	0xa,		/* FC_FLOAT */
			0x5c,		/* FC_PAD */
/* 168 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 170 */	NdrFcShort( 0x2 ),	/* Offset= 2 (172) */
/* 172 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 174 */	0x28,		/* Corr desc:  parameter, FC_LONG */
			0x0,		/*  */
/* 176 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 178 */	
			0x11, 0x0,	/* FC_RP */
/* 180 */	NdrFcShort( 0x2 ),	/* Offset= 2 (182) */
/* 182 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 184 */	NdrFcShort( 0x4 ),	/* 4 */
/* 186 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 188 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 190 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 192 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 194 */	NdrFcShort( 0x2 ),	/* Offset= 2 (196) */
/* 196 */	
			0x13, 0x0,	/* FC_OP */
/* 198 */	NdrFcShort( 0x3e6 ),	/* Offset= 998 (1196) */
/* 200 */	
			0x13, 0x0,	/* FC_OP */
/* 202 */	NdrFcShort( 0x3b0 ),	/* Offset= 944 (1146) */
/* 204 */	
			0x2b,		/* FC_NON_ENCAPSULATED_UNION */
			0x9,		/* FC_ULONG */
/* 206 */	0x7,		/* Corr desc: FC_USHORT */
			0x0,		/*  */
/* 208 */	NdrFcShort( 0xfff8 ),	/* -8 */
/* 210 */	NdrFcShort( 0x2 ),	/* Offset= 2 (212) */
/* 212 */	NdrFcShort( 0x10 ),	/* 16 */
/* 214 */	NdrFcShort( 0x2f ),	/* 47 */
/* 216 */	NdrFcLong( 0x14 ),	/* 20 */
/* 220 */	NdrFcShort( 0x800b ),	/* Simple arm type: FC_HYPER */
/* 222 */	NdrFcLong( 0x3 ),	/* 3 */
/* 226 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 228 */	NdrFcLong( 0x11 ),	/* 17 */
/* 232 */	NdrFcShort( 0x8001 ),	/* Simple arm type: FC_BYTE */
/* 234 */	NdrFcLong( 0x2 ),	/* 2 */
/* 238 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 240 */	NdrFcLong( 0x4 ),	/* 4 */
/* 244 */	NdrFcShort( 0x800a ),	/* Simple arm type: FC_FLOAT */
/* 246 */	NdrFcLong( 0x5 ),	/* 5 */
/* 250 */	NdrFcShort( 0x800c ),	/* Simple arm type: FC_DOUBLE */
/* 252 */	NdrFcLong( 0xb ),	/* 11 */
/* 256 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 258 */	NdrFcLong( 0xa ),	/* 10 */
/* 262 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 264 */	NdrFcLong( 0x6 ),	/* 6 */
/* 268 */	NdrFcShort( 0xe8 ),	/* Offset= 232 (500) */
/* 270 */	NdrFcLong( 0x7 ),	/* 7 */
/* 274 */	NdrFcShort( 0x800c ),	/* Simple arm type: FC_DOUBLE */
/* 276 */	NdrFcLong( 0x8 ),	/* 8 */
/* 280 */	NdrFcShort( 0xe2 ),	/* Offset= 226 (506) */
/* 282 */	NdrFcLong( 0xd ),	/* 13 */
/* 286 */	NdrFcShort( 0xf4 ),	/* Offset= 244 (530) */
/* 288 */	NdrFcLong( 0x9 ),	/* 9 */
/* 292 */	NdrFcShort( 0x100 ),	/* Offset= 256 (548) */
/* 294 */	NdrFcLong( 0x2000 ),	/* 8192 */
/* 298 */	NdrFcShort( 0x10c ),	/* Offset= 268 (566) */
/* 300 */	NdrFcLong( 0x24 ),	/* 36 */
/* 304 */	NdrFcShort( 0x300 ),	/* Offset= 768 (1072) */
/* 306 */	NdrFcLong( 0x4024 ),	/* 16420 */
/* 310 */	NdrFcShort( 0x2fa ),	/* Offset= 762 (1072) */
/* 312 */	NdrFcLong( 0x4011 ),	/* 16401 */
/* 316 */	NdrFcShort( 0x2f8 ),	/* Offset= 760 (1076) */
/* 318 */	NdrFcLong( 0x4002 ),	/* 16386 */
/* 322 */	NdrFcShort( 0x2f6 ),	/* Offset= 758 (1080) */
/* 324 */	NdrFcLong( 0x4003 ),	/* 16387 */
/* 328 */	NdrFcShort( 0x2f4 ),	/* Offset= 756 (1084) */
/* 330 */	NdrFcLong( 0x4014 ),	/* 16404 */
/* 334 */	NdrFcShort( 0x2f2 ),	/* Offset= 754 (1088) */
/* 336 */	NdrFcLong( 0x4004 ),	/* 16388 */
/* 340 */	NdrFcShort( 0x2f0 ),	/* Offset= 752 (1092) */
/* 342 */	NdrFcLong( 0x4005 ),	/* 16389 */
/* 346 */	NdrFcShort( 0x2ee ),	/* Offset= 750 (1096) */
/* 348 */	NdrFcLong( 0x400b ),	/* 16395 */
/* 352 */	NdrFcShort( 0x2d8 ),	/* Offset= 728 (1080) */
/* 354 */	NdrFcLong( 0x400a ),	/* 16394 */
/* 358 */	NdrFcShort( 0x2d6 ),	/* Offset= 726 (1084) */
/* 360 */	NdrFcLong( 0x4006 ),	/* 16390 */
/* 364 */	NdrFcShort( 0x2e0 ),	/* Offset= 736 (1100) */
/* 366 */	NdrFcLong( 0x4007 ),	/* 16391 */
/* 370 */	NdrFcShort( 0x2d6 ),	/* Offset= 726 (1096) */
/* 372 */	NdrFcLong( 0x4008 ),	/* 16392 */
/* 376 */	NdrFcShort( 0x2d8 ),	/* Offset= 728 (1104) */
/* 378 */	NdrFcLong( 0x400d ),	/* 16397 */
/* 382 */	NdrFcShort( 0x2d6 ),	/* Offset= 726 (1108) */
/* 384 */	NdrFcLong( 0x4009 ),	/* 16393 */
/* 388 */	NdrFcShort( 0x2d4 ),	/* Offset= 724 (1112) */
/* 390 */	NdrFcLong( 0x6000 ),	/* 24576 */
/* 394 */	NdrFcShort( 0x2d2 ),	/* Offset= 722 (1116) */
/* 396 */	NdrFcLong( 0x400c ),	/* 16396 */
/* 400 */	NdrFcShort( 0x2d0 ),	/* Offset= 720 (1120) */
/* 402 */	NdrFcLong( 0x10 ),	/* 16 */
/* 406 */	NdrFcShort( 0x8002 ),	/* Simple arm type: FC_CHAR */
/* 408 */	NdrFcLong( 0x12 ),	/* 18 */
/* 412 */	NdrFcShort( 0x8006 ),	/* Simple arm type: FC_SHORT */
/* 414 */	NdrFcLong( 0x13 ),	/* 19 */
/* 418 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 420 */	NdrFcLong( 0x15 ),	/* 21 */
/* 424 */	NdrFcShort( 0x800b ),	/* Simple arm type: FC_HYPER */
/* 426 */	NdrFcLong( 0x16 ),	/* 22 */
/* 430 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 432 */	NdrFcLong( 0x17 ),	/* 23 */
/* 436 */	NdrFcShort( 0x8008 ),	/* Simple arm type: FC_LONG */
/* 438 */	NdrFcLong( 0xe ),	/* 14 */
/* 442 */	NdrFcShort( 0x2ae ),	/* Offset= 686 (1128) */
/* 444 */	NdrFcLong( 0x400e ),	/* 16398 */
/* 448 */	NdrFcShort( 0x2b2 ),	/* Offset= 690 (1138) */
/* 450 */	NdrFcLong( 0x4010 ),	/* 16400 */
/* 454 */	NdrFcShort( 0x2b0 ),	/* Offset= 688 (1142) */
/* 456 */	NdrFcLong( 0x4012 ),	/* 16402 */
/* 460 */	NdrFcShort( 0x26c ),	/* Offset= 620 (1080) */
/* 462 */	NdrFcLong( 0x4013 ),	/* 16403 */
/* 466 */	NdrFcShort( 0x26a ),	/* Offset= 618 (1084) */
/* 468 */	NdrFcLong( 0x4015 ),	/* 16405 */
/* 472 */	NdrFcShort( 0x268 ),	/* Offset= 616 (1088) */
/* 474 */	NdrFcLong( 0x4016 ),	/* 16406 */
/* 478 */	NdrFcShort( 0x25e ),	/* Offset= 606 (1084) */
/* 480 */	NdrFcLong( 0x4017 ),	/* 16407 */
/* 484 */	NdrFcShort( 0x258 ),	/* Offset= 600 (1084) */
/* 486 */	NdrFcLong( 0x0 ),	/* 0 */
/* 490 */	NdrFcShort( 0x0 ),	/* Offset= 0 (490) */
/* 492 */	NdrFcLong( 0x1 ),	/* 1 */
/* 496 */	NdrFcShort( 0x0 ),	/* Offset= 0 (496) */
/* 498 */	NdrFcShort( 0xffff ),	/* Offset= -1 (497) */
/* 500 */	
			0x15,		/* FC_STRUCT */
			0x7,		/* 7 */
/* 502 */	NdrFcShort( 0x8 ),	/* 8 */
/* 504 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 506 */	
			0x13, 0x0,	/* FC_OP */
/* 508 */	NdrFcShort( 0xc ),	/* Offset= 12 (520) */
/* 510 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 512 */	NdrFcShort( 0x2 ),	/* 2 */
/* 514 */	0x9,		/* Corr desc: FC_ULONG */
			0x0,		/*  */
/* 516 */	NdrFcShort( 0xfffc ),	/* -4 */
/* 518 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 520 */	
			0x17,		/* FC_CSTRUCT */
			0x3,		/* 3 */
/* 522 */	NdrFcShort( 0x8 ),	/* 8 */
/* 524 */	NdrFcShort( 0xfff2 ),	/* Offset= -14 (510) */
/* 526 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 528 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 530 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 532 */	NdrFcLong( 0x0 ),	/* 0 */
/* 536 */	NdrFcShort( 0x0 ),	/* 0 */
/* 538 */	NdrFcShort( 0x0 ),	/* 0 */
/* 540 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 542 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 544 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 546 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 548 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 550 */	NdrFcLong( 0x20400 ),	/* 132096 */
/* 554 */	NdrFcShort( 0x0 ),	/* 0 */
/* 556 */	NdrFcShort( 0x0 ),	/* 0 */
/* 558 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 560 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 562 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 564 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 566 */	
			0x13, 0x10,	/* FC_OP [pointer_deref] */
/* 568 */	NdrFcShort( 0x2 ),	/* Offset= 2 (570) */
/* 570 */	
			0x13, 0x0,	/* FC_OP */
/* 572 */	NdrFcShort( 0x1e2 ),	/* Offset= 482 (1054) */
/* 574 */	
			0x2a,		/* FC_ENCAPSULATED_UNION */
			0x49,		/* 73 */
/* 576 */	NdrFcShort( 0x18 ),	/* 24 */
/* 578 */	NdrFcShort( 0xa ),	/* 10 */
/* 580 */	NdrFcLong( 0x8 ),	/* 8 */
/* 584 */	NdrFcShort( 0x58 ),	/* Offset= 88 (672) */
/* 586 */	NdrFcLong( 0xd ),	/* 13 */
/* 590 */	NdrFcShort( 0x78 ),	/* Offset= 120 (710) */
/* 592 */	NdrFcLong( 0x9 ),	/* 9 */
/* 596 */	NdrFcShort( 0x94 ),	/* Offset= 148 (744) */
/* 598 */	NdrFcLong( 0xc ),	/* 12 */
/* 602 */	NdrFcShort( 0xbc ),	/* Offset= 188 (790) */
/* 604 */	NdrFcLong( 0x24 ),	/* 36 */
/* 608 */	NdrFcShort( 0x114 ),	/* Offset= 276 (884) */
/* 610 */	NdrFcLong( 0x800d ),	/* 32781 */
/* 614 */	NdrFcShort( 0x11e ),	/* Offset= 286 (900) */
/* 616 */	NdrFcLong( 0x10 ),	/* 16 */
/* 620 */	NdrFcShort( 0x136 ),	/* Offset= 310 (930) */
/* 622 */	NdrFcLong( 0x2 ),	/* 2 */
/* 626 */	NdrFcShort( 0x14e ),	/* Offset= 334 (960) */
/* 628 */	NdrFcLong( 0x3 ),	/* 3 */
/* 632 */	NdrFcShort( 0x166 ),	/* Offset= 358 (990) */
/* 634 */	NdrFcLong( 0x14 ),	/* 20 */
/* 638 */	NdrFcShort( 0x17e ),	/* Offset= 382 (1020) */
/* 640 */	NdrFcShort( 0xffff ),	/* Offset= -1 (639) */
/* 642 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 644 */	NdrFcShort( 0x4 ),	/* 4 */
/* 646 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 648 */	NdrFcShort( 0x0 ),	/* 0 */
/* 650 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 652 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 654 */	NdrFcShort( 0x4 ),	/* 4 */
/* 656 */	NdrFcShort( 0x0 ),	/* 0 */
/* 658 */	NdrFcShort( 0x1 ),	/* 1 */
/* 660 */	NdrFcShort( 0x0 ),	/* 0 */
/* 662 */	NdrFcShort( 0x0 ),	/* 0 */
/* 664 */	0x13, 0x0,	/* FC_OP */
/* 666 */	NdrFcShort( 0xff6e ),	/* Offset= -146 (520) */
/* 668 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 670 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 672 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 674 */	NdrFcShort( 0x8 ),	/* 8 */
/* 676 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 678 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 680 */	NdrFcShort( 0x4 ),	/* 4 */
/* 682 */	NdrFcShort( 0x4 ),	/* 4 */
/* 684 */	0x11, 0x0,	/* FC_RP */
/* 686 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (642) */
/* 688 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 690 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 692 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 694 */	NdrFcShort( 0x0 ),	/* 0 */
/* 696 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 698 */	NdrFcShort( 0x0 ),	/* 0 */
/* 700 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 704 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 706 */	NdrFcShort( 0xff50 ),	/* Offset= -176 (530) */
/* 708 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 710 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 712 */	NdrFcShort( 0x8 ),	/* 8 */
/* 714 */	NdrFcShort( 0x0 ),	/* 0 */
/* 716 */	NdrFcShort( 0x6 ),	/* Offset= 6 (722) */
/* 718 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 720 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 722 */	
			0x11, 0x0,	/* FC_RP */
/* 724 */	NdrFcShort( 0xffe0 ),	/* Offset= -32 (692) */
/* 726 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 728 */	NdrFcShort( 0x0 ),	/* 0 */
/* 730 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 732 */	NdrFcShort( 0x0 ),	/* 0 */
/* 734 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 738 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 740 */	NdrFcShort( 0xff40 ),	/* Offset= -192 (548) */
/* 742 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 744 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 746 */	NdrFcShort( 0x8 ),	/* 8 */
/* 748 */	NdrFcShort( 0x0 ),	/* 0 */
/* 750 */	NdrFcShort( 0x6 ),	/* Offset= 6 (756) */
/* 752 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 754 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 756 */	
			0x11, 0x0,	/* FC_RP */
/* 758 */	NdrFcShort( 0xffe0 ),	/* Offset= -32 (726) */
/* 760 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 762 */	NdrFcShort( 0x4 ),	/* 4 */
/* 764 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 766 */	NdrFcShort( 0x0 ),	/* 0 */
/* 768 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 770 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 772 */	NdrFcShort( 0x4 ),	/* 4 */
/* 774 */	NdrFcShort( 0x0 ),	/* 0 */
/* 776 */	NdrFcShort( 0x1 ),	/* 1 */
/* 778 */	NdrFcShort( 0x0 ),	/* 0 */
/* 780 */	NdrFcShort( 0x0 ),	/* 0 */
/* 782 */	0x13, 0x0,	/* FC_OP */
/* 784 */	NdrFcShort( 0x16a ),	/* Offset= 362 (1146) */
/* 786 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 788 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 790 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 792 */	NdrFcShort( 0x8 ),	/* 8 */
/* 794 */	NdrFcShort( 0x0 ),	/* 0 */
/* 796 */	NdrFcShort( 0x6 ),	/* Offset= 6 (802) */
/* 798 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 800 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 802 */	
			0x11, 0x0,	/* FC_RP */
/* 804 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (760) */
/* 806 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 808 */	NdrFcLong( 0x2f ),	/* 47 */
/* 812 */	NdrFcShort( 0x0 ),	/* 0 */
/* 814 */	NdrFcShort( 0x0 ),	/* 0 */
/* 816 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 818 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 820 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 822 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 824 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 826 */	NdrFcShort( 0x1 ),	/* 1 */
/* 828 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 830 */	NdrFcShort( 0x4 ),	/* 4 */
/* 832 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 834 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 836 */	NdrFcShort( 0x10 ),	/* 16 */
/* 838 */	NdrFcShort( 0x0 ),	/* 0 */
/* 840 */	NdrFcShort( 0xa ),	/* Offset= 10 (850) */
/* 842 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 844 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 846 */	NdrFcShort( 0xffd8 ),	/* Offset= -40 (806) */
/* 848 */	0x36,		/* FC_POINTER */
			0x5b,		/* FC_END */
/* 850 */	
			0x13, 0x0,	/* FC_OP */
/* 852 */	NdrFcShort( 0xffe4 ),	/* Offset= -28 (824) */
/* 854 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 856 */	NdrFcShort( 0x4 ),	/* 4 */
/* 858 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 860 */	NdrFcShort( 0x0 ),	/* 0 */
/* 862 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 864 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 866 */	NdrFcShort( 0x4 ),	/* 4 */
/* 868 */	NdrFcShort( 0x0 ),	/* 0 */
/* 870 */	NdrFcShort( 0x1 ),	/* 1 */
/* 872 */	NdrFcShort( 0x0 ),	/* 0 */
/* 874 */	NdrFcShort( 0x0 ),	/* 0 */
/* 876 */	0x13, 0x0,	/* FC_OP */
/* 878 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (834) */
/* 880 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 882 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 884 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 886 */	NdrFcShort( 0x8 ),	/* 8 */
/* 888 */	NdrFcShort( 0x0 ),	/* 0 */
/* 890 */	NdrFcShort( 0x6 ),	/* Offset= 6 (896) */
/* 892 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 894 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 896 */	
			0x11, 0x0,	/* FC_RP */
/* 898 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (854) */
/* 900 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 902 */	NdrFcShort( 0x18 ),	/* 24 */
/* 904 */	NdrFcShort( 0x0 ),	/* 0 */
/* 906 */	NdrFcShort( 0xa ),	/* Offset= 10 (916) */
/* 908 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 910 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 912 */	NdrFcShort( 0xfc8c ),	/* Offset= -884 (28) */
/* 914 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 916 */	
			0x11, 0x0,	/* FC_RP */
/* 918 */	NdrFcShort( 0xff1e ),	/* Offset= -226 (692) */
/* 920 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 922 */	NdrFcShort( 0x1 ),	/* 1 */
/* 924 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 926 */	NdrFcShort( 0x0 ),	/* 0 */
/* 928 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 930 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 932 */	NdrFcShort( 0x8 ),	/* 8 */
/* 934 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 936 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 938 */	NdrFcShort( 0x4 ),	/* 4 */
/* 940 */	NdrFcShort( 0x4 ),	/* 4 */
/* 942 */	0x13, 0x0,	/* FC_OP */
/* 944 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (920) */
/* 946 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 948 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 950 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 952 */	NdrFcShort( 0x2 ),	/* 2 */
/* 954 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 956 */	NdrFcShort( 0x0 ),	/* 0 */
/* 958 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 960 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 962 */	NdrFcShort( 0x8 ),	/* 8 */
/* 964 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 966 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 968 */	NdrFcShort( 0x4 ),	/* 4 */
/* 970 */	NdrFcShort( 0x4 ),	/* 4 */
/* 972 */	0x13, 0x0,	/* FC_OP */
/* 974 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (950) */
/* 976 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 978 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 980 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 982 */	NdrFcShort( 0x4 ),	/* 4 */
/* 984 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 986 */	NdrFcShort( 0x0 ),	/* 0 */
/* 988 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 990 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 992 */	NdrFcShort( 0x8 ),	/* 8 */
/* 994 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 996 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 998 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1000 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1002 */	0x13, 0x0,	/* FC_OP */
/* 1004 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (980) */
/* 1006 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1008 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1010 */	
			0x1b,		/* FC_CARRAY */
			0x7,		/* 7 */
/* 1012 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1014 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1016 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1018 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 1020 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1022 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1024 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1026 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1028 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1030 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1032 */	0x13, 0x0,	/* FC_OP */
/* 1034 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1010) */
/* 1036 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1038 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1040 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1042 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1044 */	0x7,		/* Corr desc: FC_USHORT */
			0x0,		/*  */
/* 1046 */	NdrFcShort( 0xffd8 ),	/* -40 */
/* 1048 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1050 */	NdrFcShort( 0xfc32 ),	/* Offset= -974 (76) */
/* 1052 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1054 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1056 */	NdrFcShort( 0x28 ),	/* 40 */
/* 1058 */	NdrFcShort( 0xffee ),	/* Offset= -18 (1040) */
/* 1060 */	NdrFcShort( 0x0 ),	/* Offset= 0 (1060) */
/* 1062 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1064 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1066 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1068 */	NdrFcShort( 0xfe12 ),	/* Offset= -494 (574) */
/* 1070 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1072 */	
			0x13, 0x0,	/* FC_OP */
/* 1074 */	NdrFcShort( 0xff10 ),	/* Offset= -240 (834) */
/* 1076 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1078 */	0x1,		/* FC_BYTE */
			0x5c,		/* FC_PAD */
/* 1080 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1082 */	0x6,		/* FC_SHORT */
			0x5c,		/* FC_PAD */
/* 1084 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1086 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 1088 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1090 */	0xb,		/* FC_HYPER */
			0x5c,		/* FC_PAD */
/* 1092 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1094 */	0xa,		/* FC_FLOAT */
			0x5c,		/* FC_PAD */
/* 1096 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1098 */	0xc,		/* FC_DOUBLE */
			0x5c,		/* FC_PAD */
/* 1100 */	
			0x13, 0x0,	/* FC_OP */
/* 1102 */	NdrFcShort( 0xfda6 ),	/* Offset= -602 (500) */
/* 1104 */	
			0x13, 0x10,	/* FC_OP [pointer_deref] */
/* 1106 */	NdrFcShort( 0xfda8 ),	/* Offset= -600 (506) */
/* 1108 */	
			0x13, 0x10,	/* FC_OP [pointer_deref] */
/* 1110 */	NdrFcShort( 0xfdbc ),	/* Offset= -580 (530) */
/* 1112 */	
			0x13, 0x10,	/* FC_OP [pointer_deref] */
/* 1114 */	NdrFcShort( 0xfdca ),	/* Offset= -566 (548) */
/* 1116 */	
			0x13, 0x10,	/* FC_OP [pointer_deref] */
/* 1118 */	NdrFcShort( 0xfdd8 ),	/* Offset= -552 (566) */
/* 1120 */	
			0x13, 0x10,	/* FC_OP [pointer_deref] */
/* 1122 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1124) */
/* 1124 */	
			0x13, 0x0,	/* FC_OP */
/* 1126 */	NdrFcShort( 0x14 ),	/* Offset= 20 (1146) */
/* 1128 */	
			0x15,		/* FC_STRUCT */
			0x7,		/* 7 */
/* 1130 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1132 */	0x6,		/* FC_SHORT */
			0x1,		/* FC_BYTE */
/* 1134 */	0x1,		/* FC_BYTE */
			0x8,		/* FC_LONG */
/* 1136 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 1138 */	
			0x13, 0x0,	/* FC_OP */
/* 1140 */	NdrFcShort( 0xfff4 ),	/* Offset= -12 (1128) */
/* 1142 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1144 */	0x2,		/* FC_CHAR */
			0x5c,		/* FC_PAD */
/* 1146 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x7,		/* 7 */
/* 1148 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1150 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1152 */	NdrFcShort( 0x0 ),	/* Offset= 0 (1152) */
/* 1154 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1156 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1158 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1160 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1162 */	NdrFcShort( 0xfc42 ),	/* Offset= -958 (204) */
/* 1164 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1166 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1168 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1170 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1172 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1174 */	NdrFcShort( 0xfc32 ),	/* Offset= -974 (200) */
/* 1176 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1178 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1180 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1182 */	NdrFcShort( 0x0 ),	/* Offset= 0 (1182) */
/* 1184 */	0x8,		/* FC_LONG */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1186 */	0x0,		/* 0 */
			NdrFcShort( 0xfba9 ),	/* Offset= -1111 (76) */
			0x6,		/* FC_SHORT */
/* 1190 */	0x6,		/* FC_SHORT */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1192 */	0x0,		/* 0 */
			NdrFcShort( 0xffe5 ),	/* Offset= -27 (1166) */
			0x5b,		/* FC_END */
/* 1196 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1198 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1200 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1202 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1204 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1208 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1210 */	NdrFcShort( 0xffde ),	/* Offset= -34 (1176) */
/* 1212 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1214 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1216 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1218) */
/* 1218 */	
			0x13, 0x0,	/* FC_OP */
/* 1220 */	NdrFcShort( 0xfbf2 ),	/* Offset= -1038 (182) */
/* 1222 */	
			0x11, 0x0,	/* FC_RP */
/* 1224 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1226) */
/* 1226 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1228 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1230 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1232 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1234 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1236 */	
			0x11, 0x0,	/* FC_RP */
/* 1238 */	NdrFcShort( 0x10 ),	/* Offset= 16 (1254) */
/* 1240 */	
			0x12, 0x0,	/* FC_UP */
/* 1242 */	NdrFcShort( 0xffa0 ),	/* Offset= -96 (1146) */
/* 1244 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1246 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1248 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1250 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1252 */	NdrFcShort( 0xfff4 ),	/* Offset= -12 (1240) */
/* 1254 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1256 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1258 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1260 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1262 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1266 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1268 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1244) */
/* 1270 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1272 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1274 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1276) */
/* 1276 */	
			0x13, 0x0,	/* FC_OP */
/* 1278 */	NdrFcShort( 0xffcc ),	/* Offset= -52 (1226) */
/* 1280 */	
			0x11, 0x0,	/* FC_RP */
/* 1282 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1284) */
/* 1284 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1286 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1288 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1290 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1292 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1294 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1296 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1298) */
/* 1298 */	
			0x13, 0x0,	/* FC_OP */
/* 1300 */	NdrFcShort( 0xfff0 ),	/* Offset= -16 (1284) */
/* 1302 */	
			0x11, 0x0,	/* FC_RP */
/* 1304 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1306) */
/* 1306 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1308 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1310 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1312 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1314 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1318 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1320 */	NdrFcShort( 0xffb4 ),	/* Offset= -76 (1244) */
/* 1322 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1324 */	
			0x11, 0x0,	/* FC_RP */
/* 1326 */	NdrFcShort( 0x3a ),	/* Offset= 58 (1384) */
/* 1328 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 1330 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1332 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1334 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1336 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 1338 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1340 */	NdrFcShort( 0x1c ),	/* 28 */
/* 1342 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1344 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1346 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1348 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1350 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1352 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1354 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1356 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1358 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1360 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1362 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1364 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1366 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1368 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1370 */	0x12, 0x0,	/* FC_UP */
/* 1372 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (1328) */
/* 1374 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1376 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1378 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1380 */	0x8,		/* FC_LONG */
			0x6,		/* FC_SHORT */
/* 1382 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 1384 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1386 */	NdrFcShort( 0x1c ),	/* 28 */
/* 1388 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1390 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1392 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1394 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1396 */	NdrFcShort( 0x1c ),	/* 28 */
/* 1398 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1400 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1402 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1404 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1406 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1408 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1410 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1412 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1414 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1416 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1418 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1420 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1422 */	0x12, 0x0,	/* FC_UP */
/* 1424 */	NdrFcShort( 0xffa0 ),	/* Offset= -96 (1328) */
/* 1426 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1428 */	0x0,		/* 0 */
			NdrFcShort( 0xffa5 ),	/* Offset= -91 (1338) */
			0x5b,		/* FC_END */
/* 1432 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1434 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1436) */
/* 1436 */	
			0x13, 0x0,	/* FC_OP */
/* 1438 */	NdrFcShort( 0x24 ),	/* Offset= 36 (1474) */
/* 1440 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 1442 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1444 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1446 */	NdrFcShort( 0xc ),	/* 12 */
/* 1448 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 1450 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1452 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1454 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1456 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1458 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1460 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1462 */	0x13, 0x0,	/* FC_OP */
/* 1464 */	NdrFcShort( 0xffe8 ),	/* Offset= -24 (1440) */
/* 1466 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1468 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1470 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1472 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1474 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1476 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1478 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1480 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1482 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1484 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1486 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1488 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1490 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1492 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1494 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1496 */	0x13, 0x0,	/* FC_OP */
/* 1498 */	NdrFcShort( 0xffc6 ),	/* Offset= -58 (1440) */
/* 1500 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1502 */	0x0,		/* 0 */
			NdrFcShort( 0xffcb ),	/* Offset= -53 (1450) */
			0x5b,		/* FC_END */
/* 1506 */	
			0x11, 0x0,	/* FC_RP */
/* 1508 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1510) */
/* 1510 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 1512 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1514 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1516 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1518 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 1520 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 1522 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1524) */
/* 1524 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 1526 */	0x28,		/* Corr desc:  parameter, FC_LONG */
			0x0,		/*  */
/* 1528 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1530 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1532 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1534) */
/* 1534 */	
			0x13, 0x0,	/* FC_OP */
/* 1536 */	NdrFcShort( 0x30 ),	/* Offset= 48 (1584) */
/* 1538 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 1540 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1542 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1544 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1546 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 1548 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1550 */	NdrFcShort( 0x38 ),	/* 56 */
/* 1552 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1554 */	NdrFcShort( 0x12 ),	/* Offset= 18 (1572) */
/* 1556 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1558 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1560 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1562 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1564 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1566 */	0xd,		/* FC_ENUM16 */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1568 */	0x0,		/* 0 */
			NdrFcShort( 0xfe6d ),	/* Offset= -403 (1166) */
			0x5b,		/* FC_END */
/* 1572 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1574 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1576 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1578 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1580 */	
			0x13, 0x0,	/* FC_OP */
/* 1582 */	NdrFcShort( 0xffd4 ),	/* Offset= -44 (1538) */
/* 1584 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1586 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1588 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 1590 */	NdrFcShort( 0xc ),	/* x86 Stack size/offset = 12 */
/* 1592 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1596 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1598 */	NdrFcShort( 0xffce ),	/* Offset= -50 (1548) */
/* 1600 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1602 */	
			0x11, 0x10,	/* FC_RP [pointer_deref] */
/* 1604 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1606) */
/* 1606 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 1608 */	NdrFcLong( 0x39c13a55 ),	/* 968964693 */
/* 1612 */	NdrFcShort( 0x11e ),	/* 286 */
/* 1614 */	NdrFcShort( 0x11d0 ),	/* 4560 */
/* 1616 */	0x96,		/* 150 */
			0x75,		/* 117 */
/* 1618 */	0x0,		/* 0 */
			0x20,		/* 32 */
/* 1620 */	0xaf,		/* 175 */
			0xd8,		/* 216 */
/* 1622 */	0xad,		/* 173 */
			0xb3,		/* 179 */
/* 1624 */	
			0x11, 0x0,	/* FC_RP */
/* 1626 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1628) */
/* 1628 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1630 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1632 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1634 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1636 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1638 */	
			0x11, 0x0,	/* FC_RP */
/* 1640 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1642) */
/* 1642 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1644 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1646 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1648 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1650 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1654 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1656 */	NdrFcShort( 0xfe64 ),	/* Offset= -412 (1244) */
/* 1658 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1660 */	
			0x11, 0x0,	/* FC_RP */
/* 1662 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1664) */
/* 1664 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 1666 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1668 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1670 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1672 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 1674 */	
			0x11, 0x0,	/* FC_RP */
/* 1676 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1678) */
/* 1678 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1680 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1682 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1684 */	NdrFcShort( 0x14 ),	/* x86 Stack size/offset = 20 */
/* 1686 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1688 */	NdrFcShort( 0xf9b4 ),	/* Offset= -1612 (76) */
/* 1690 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1692 */	
			0x11, 0x0,	/* FC_RP */
/* 1694 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1696) */
/* 1696 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1698 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1700 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1702 */	NdrFcShort( 0x10 ),	/* x86 Stack size/offset = 16 */
/* 1704 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1706 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1708 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1710) */
/* 1710 */	
			0x13, 0x0,	/* FC_OP */
/* 1712 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1714) */
/* 1714 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1716 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1718 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 1720 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1722 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1724 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1726 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1728) */
/* 1728 */	
			0x13, 0x0,	/* FC_OP */
/* 1730 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1732) */
/* 1732 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1734 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1736 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 1738 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1740 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1742 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1744 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1746 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1748 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1750 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1752 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1754 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1756 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1758 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1760 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1762 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1764 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1766) */
/* 1766 */	
			0x13, 0x0,	/* FC_OP */
/* 1768 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1770) */
/* 1770 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 1772 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1774 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 1776 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1778 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 1780 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1782 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1784) */
/* 1784 */	
			0x13, 0x0,	/* FC_OP */
/* 1786 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1788) */
/* 1788 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1790 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1792 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1794 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1796 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1800 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1802 */	NdrFcShort( 0xfd84 ),	/* Offset= -636 (1166) */
/* 1804 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1806 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1808 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1810) */
/* 1810 */	
			0x13, 0x0,	/* FC_OP */
/* 1812 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1814) */
/* 1814 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1816 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1818 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1820 */	NdrFcShort( 0x8 ),	/* x86 Stack size/offset = 8 */
/* 1822 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1824 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1826 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1828 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1830 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1832 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1834 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1836 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1838 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1840 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1842 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1844 */	
			0x11, 0x0,	/* FC_RP */
/* 1846 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1848) */
/* 1848 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1850 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1852 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1854 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1856 */	0xa,		/* FC_FLOAT */
			0x5b,		/* FC_END */
/* 1858 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1860 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1862) */
/* 1862 */	
			0x13, 0x0,	/* FC_OP */
/* 1864 */	NdrFcShort( 0xfff0 ),	/* Offset= -16 (1848) */
/* 1866 */	
			0x11, 0x0,	/* FC_RP */
/* 1868 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1870) */
/* 1870 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1872 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1874 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1876 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1878 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1880 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1882 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1884 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1886 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1888 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1890 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1892 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1894 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1896 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1898 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1900 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 1902 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1904) */
/* 1904 */	
			0x13, 0x0,	/* FC_OP */
/* 1906 */	NdrFcShort( 0x42 ),	/* Offset= 66 (1972) */
/* 1908 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1910 */	NdrFcShort( 0x28 ),	/* 40 */
/* 1912 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1914 */	NdrFcShort( 0xe ),	/* Offset= 14 (1928) */
/* 1916 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1918 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1920 */	0x36,		/* FC_POINTER */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1922 */	0x0,		/* 0 */
			NdrFcShort( 0xfd0b ),	/* Offset= -757 (1166) */
			0x8,		/* FC_LONG */
/* 1926 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1928 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1930 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1932 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1934 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1936 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1938 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1940 */	0x19,		/* Corr desc:  field pointer, FC_ULONG */
			0x0,		/*  */
/* 1942 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1944 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1948 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1950 */	NdrFcShort( 0xffd6 ),	/* Offset= -42 (1908) */
/* 1952 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1954 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1956 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1958 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1960 */	NdrFcShort( 0x8 ),	/* Offset= 8 (1968) */
/* 1962 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1964 */	0x36,		/* FC_POINTER */
			0x8,		/* FC_LONG */
/* 1966 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1968 */	
			0x13, 0x0,	/* FC_OP */
/* 1970 */	NdrFcShort( 0xffde ),	/* Offset= -34 (1936) */
/* 1972 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1974 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1976 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 1978 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 1980 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1984 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1986 */	NdrFcShort( 0xffe0 ),	/* Offset= -32 (1954) */
/* 1988 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1990 */	
			0x11, 0x0,	/* FC_RP */
/* 1992 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1994) */
/* 1994 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1996 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1998 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 2000 */	NdrFcShort( 0x24 ),	/* x86 Stack size/offset = 36 */
/* 2002 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 2004 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 2006 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2008) */
/* 2008 */	
			0x13, 0x0,	/* FC_OP */
/* 2010 */	NdrFcShort( 0x1c ),	/* Offset= 28 (2038) */
/* 2012 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 2014 */	NdrFcShort( 0x20 ),	/* 32 */
/* 2016 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2018 */	NdrFcShort( 0xc ),	/* Offset= 12 (2030) */
/* 2020 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 2022 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 2024 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 2026 */	NdrFcShort( 0xffb8 ),	/* Offset= -72 (1954) */
/* 2028 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 2030 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 2032 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 2034 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 2036 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 2038 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 2040 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2042 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x54,		/* FC_DEREFERENCE */
/* 2044 */	NdrFcShort( 0x30 ),	/* x86 Stack size/offset = 48 */
/* 2046 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 2050 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 2052 */	NdrFcShort( 0xffd8 ),	/* Offset= -40 (2012) */
/* 2054 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 2056 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 2058 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2060) */
/* 2060 */	
			0x13, 0x0,	/* FC_OP */
/* 2062 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2064) */
/* 2064 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 2066 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2068 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 2070 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2072 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 2076 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 2078 */	NdrFcShort( 0xfc70 ),	/* Offset= -912 (1166) */
/* 2080 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 2082 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 2084 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2086) */
/* 2086 */	
			0x13, 0x0,	/* FC_OP */
/* 2088 */	NdrFcShort( 0xfdbe ),	/* Offset= -578 (1510) */
/* 2090 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] [pointer_deref] */
/* 2092 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2094) */
/* 2094 */	
			0x13, 0x0,	/* FC_OP */
/* 2096 */	NdrFcShort( 0x2 ),	/* Offset= 2 (2098) */
/* 2098 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 2100 */	NdrFcShort( 0x8 ),	/* 8 */
/* 2102 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 2104 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2106 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 2108 */	NdrFcShort( 0xf810 ),	/* Offset= -2032 (76) */
/* 2110 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 2112 */	
			0x11, 0x0,	/* FC_RP */
/* 2114 */	NdrFcShort( 0x18 ),	/* Offset= 24 (2138) */
/* 2116 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 2118 */	NdrFcShort( 0x28 ),	/* 40 */
/* 2120 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2122 */	NdrFcShort( 0x0 ),	/* Offset= 0 (2122) */
/* 2124 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 2126 */	NdrFcShort( 0xfc8e ),	/* Offset= -882 (1244) */
/* 2128 */	0x8,		/* FC_LONG */
			0x6,		/* FC_SHORT */
/* 2130 */	0x6,		/* FC_SHORT */
			0x8,		/* FC_LONG */
/* 2132 */	0x8,		/* FC_LONG */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 2134 */	0x0,		/* 0 */
			NdrFcShort( 0xf7f5 ),	/* Offset= -2059 (76) */
			0x5b,		/* FC_END */
/* 2138 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 2140 */	NdrFcShort( 0x0 ),	/* 0 */
/* 2142 */	0x29,		/* Corr desc:  parameter, FC_ULONG */
			0x0,		/*  */
/* 2144 */	NdrFcShort( 0x4 ),	/* x86 Stack size/offset = 4 */
/* 2146 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 2150 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 2152 */	NdrFcShort( 0xffdc ),	/* Offset= -36 (2116) */
/* 2154 */	0x5c,		/* FC_PAD */
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


/* Object interface: CATID_OPCDAServer10, ver. 0.0,
   GUID={0x63D5F430,0xCFE4,0x11d1,{0xB2,0xC8,0x00,0x60,0x08,0x3B,0xA1,0xFB}} */

#pragma code_seg(".orpc")
static const unsigned short CATID_OPCDAServer10_FormatStringOffsetTable[] =
    {
    0
    };

static const MIDL_STUBLESS_PROXY_INFO CATID_OPCDAServer10_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &CATID_OPCDAServer10_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO CATID_OPCDAServer10_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &CATID_OPCDAServer10_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(3) _CATID_OPCDAServer10ProxyVtbl = 
{
    0,
    &IID_CATID_OPCDAServer10,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy
};

const CInterfaceStubVtbl _CATID_OPCDAServer10StubVtbl =
{
    &IID_CATID_OPCDAServer10,
    &CATID_OPCDAServer10_ServerInfo,
    3,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: CATID_OPCDAServer20, ver. 0.0,
   GUID={0x63D5F432,0xCFE4,0x11d1,{0xB2,0xC8,0x00,0x60,0x08,0x3B,0xA1,0xFB}} */

#pragma code_seg(".orpc")
static const unsigned short CATID_OPCDAServer20_FormatStringOffsetTable[] =
    {
    0
    };

static const MIDL_STUBLESS_PROXY_INFO CATID_OPCDAServer20_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &CATID_OPCDAServer20_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO CATID_OPCDAServer20_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &CATID_OPCDAServer20_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(3) _CATID_OPCDAServer20ProxyVtbl = 
{
    0,
    &IID_CATID_OPCDAServer20,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy
};

const CInterfaceStubVtbl _CATID_OPCDAServer20StubVtbl =
{
    &IID_CATID_OPCDAServer20,
    &CATID_OPCDAServer20_ServerInfo,
    3,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: CATID_OPCDAServer30, ver. 0.0,
   GUID={0xCC603642,0x66D7,0x48f1,{0xB6,0x9A,0xB6,0x25,0xE7,0x36,0x52,0xD7}} */

#pragma code_seg(".orpc")
static const unsigned short CATID_OPCDAServer30_FormatStringOffsetTable[] =
    {
    0
    };

static const MIDL_STUBLESS_PROXY_INFO CATID_OPCDAServer30_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &CATID_OPCDAServer30_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO CATID_OPCDAServer30_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &CATID_OPCDAServer30_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(3) _CATID_OPCDAServer30ProxyVtbl = 
{
    0,
    &IID_CATID_OPCDAServer30,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy
};

const CInterfaceStubVtbl _CATID_OPCDAServer30StubVtbl =
{
    &IID_CATID_OPCDAServer30,
    &CATID_OPCDAServer30_ServerInfo,
    3,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: CATID_XMLDAServer10, ver. 0.0,
   GUID={0x3098EDA4,0xA006,0x48b2,{0xA2,0x7F,0x24,0x74,0x53,0x95,0x94,0x08}} */

#pragma code_seg(".orpc")
static const unsigned short CATID_XMLDAServer10_FormatStringOffsetTable[] =
    {
    0
    };

static const MIDL_STUBLESS_PROXY_INFO CATID_XMLDAServer10_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &CATID_XMLDAServer10_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO CATID_XMLDAServer10_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &CATID_XMLDAServer10_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(3) _CATID_XMLDAServer10ProxyVtbl = 
{
    0,
    &IID_CATID_XMLDAServer10,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy
};

const CInterfaceStubVtbl _CATID_XMLDAServer10StubVtbl =
{
    &IID_CATID_XMLDAServer10,
    &CATID_XMLDAServer10_ServerInfo,
    3,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Standard interface: __MIDL_itf_opcda_0000_0004, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: IOPCServer, ver. 0.0,
   GUID={0x39c13a4d,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCServer_FormatStringOffsetTable[] =
    {
    0,
    88,
    128,
    168,
    196,
    230
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCServer_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCServer_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCServer_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCServer_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(9) _IOPCServerProxyVtbl = 
{
    &IOPCServer_ProxyInfo,
    &IID_IOPCServer,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCServer::AddGroup */ ,
    (void *) (INT_PTR) -1 /* IOPCServer::GetErrorString */ ,
    (void *) (INT_PTR) -1 /* IOPCServer::GetGroupByName */ ,
    (void *) (INT_PTR) -1 /* IOPCServer::GetStatus */ ,
    (void *) (INT_PTR) -1 /* IOPCServer::RemoveGroup */ ,
    (void *) (INT_PTR) -1 /* IOPCServer::CreateGroupEnumerator */
};

const CInterfaceStubVtbl _IOPCServerStubVtbl =
{
    &IID_IOPCServer,
    &IOPCServer_ServerInfo,
    9,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCServerPublicGroups, ver. 0.0,
   GUID={0x39c13a4e,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCServerPublicGroups_FormatStringOffsetTable[] =
    {
    270,
    310
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCServerPublicGroups_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCServerPublicGroups_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCServerPublicGroups_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCServerPublicGroups_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(5) _IOPCServerPublicGroupsProxyVtbl = 
{
    &IOPCServerPublicGroups_ProxyInfo,
    &IID_IOPCServerPublicGroups,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCServerPublicGroups::GetPublicGroupByName */ ,
    (void *) (INT_PTR) -1 /* IOPCServerPublicGroups::RemovePublicGroup */
};

const CInterfaceStubVtbl _IOPCServerPublicGroupsStubVtbl =
{
    &IID_IOPCServerPublicGroups,
    &IOPCServerPublicGroups_ServerInfo,
    5,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCBrowseServerAddressSpace, ver. 0.0,
   GUID={0x39c13a4f,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCBrowseServerAddressSpace_FormatStringOffsetTable[] =
    {
    344,
    372,
    406,
    458,
    492
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCBrowseServerAddressSpace_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCBrowseServerAddressSpace_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCBrowseServerAddressSpace_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCBrowseServerAddressSpace_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(8) _IOPCBrowseServerAddressSpaceProxyVtbl = 
{
    &IOPCBrowseServerAddressSpace_ProxyInfo,
    &IID_IOPCBrowseServerAddressSpace,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCBrowseServerAddressSpace::QueryOrganization */ ,
    (void *) (INT_PTR) -1 /* IOPCBrowseServerAddressSpace::ChangeBrowsePosition */ ,
    (void *) (INT_PTR) -1 /* IOPCBrowseServerAddressSpace::BrowseOPCItemIDs */ ,
    (void *) (INT_PTR) -1 /* IOPCBrowseServerAddressSpace::GetItemID */ ,
    (void *) (INT_PTR) -1 /* IOPCBrowseServerAddressSpace::BrowseAccessPaths */
};

const CInterfaceStubVtbl _IOPCBrowseServerAddressSpaceStubVtbl =
{
    &IID_IOPCBrowseServerAddressSpace,
    &IOPCBrowseServerAddressSpace_ServerInfo,
    8,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCGroupStateMgt, ver. 0.0,
   GUID={0x39c13a50,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCGroupStateMgt_FormatStringOffsetTable[] =
    {
    526,
    596,
    660,
    688
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCGroupStateMgt_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCGroupStateMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCGroupStateMgt_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCGroupStateMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(7) _IOPCGroupStateMgtProxyVtbl = 
{
    &IOPCGroupStateMgt_ProxyInfo,
    &IID_IOPCGroupStateMgt,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCGroupStateMgt::GetState */ ,
    (void *) (INT_PTR) -1 /* IOPCGroupStateMgt::SetState */ ,
    (void *) (INT_PTR) -1 /* IOPCGroupStateMgt::SetName */ ,
    (void *) (INT_PTR) -1 /* IOPCGroupStateMgt::CloneGroup */
};

const CInterfaceStubVtbl _IOPCGroupStateMgtStubVtbl =
{
    &IID_IOPCGroupStateMgt,
    &IOPCGroupStateMgt_ServerInfo,
    7,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCPublicGroupStateMgt, ver. 0.0,
   GUID={0x39c13a51,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCPublicGroupStateMgt_FormatStringOffsetTable[] =
    {
    728,
    756
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCPublicGroupStateMgt_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCPublicGroupStateMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCPublicGroupStateMgt_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCPublicGroupStateMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(5) _IOPCPublicGroupStateMgtProxyVtbl = 
{
    &IOPCPublicGroupStateMgt_ProxyInfo,
    &IID_IOPCPublicGroupStateMgt,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCPublicGroupStateMgt::GetState */ ,
    (void *) (INT_PTR) -1 /* IOPCPublicGroupStateMgt::MoveToPublic */
};

const CInterfaceStubVtbl _IOPCPublicGroupStateMgtStubVtbl =
{
    &IID_IOPCPublicGroupStateMgt,
    &IOPCPublicGroupStateMgt_ServerInfo,
    5,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCSyncIO, ver. 0.0,
   GUID={0x39c13a52,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCSyncIO_FormatStringOffsetTable[] =
    {
    778,
    830
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCSyncIO_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCSyncIO_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCSyncIO_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCSyncIO_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(5) _IOPCSyncIOProxyVtbl = 
{
    &IOPCSyncIO_ProxyInfo,
    &IID_IOPCSyncIO,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCSyncIO::Read */ ,
    (void *) (INT_PTR) -1 /* IOPCSyncIO::Write */
};

const CInterfaceStubVtbl _IOPCSyncIOStubVtbl =
{
    &IID_IOPCSyncIO,
    &IOPCSyncIO_ServerInfo,
    5,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCAsyncIO, ver. 0.0,
   GUID={0x39c13a53,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCAsyncIO_FormatStringOffsetTable[] =
    {
    876,
    934,
    992,
    1032
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCAsyncIO_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCAsyncIO_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCAsyncIO_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCAsyncIO_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(7) _IOPCAsyncIOProxyVtbl = 
{
    &IOPCAsyncIO_ProxyInfo,
    &IID_IOPCAsyncIO,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO::Read */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO::Write */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO::Refresh */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO::Cancel */
};

const CInterfaceStubVtbl _IOPCAsyncIOStubVtbl =
{
    &IID_IOPCAsyncIO,
    &IOPCAsyncIO_ServerInfo,
    7,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCItemMgt, ver. 0.0,
   GUID={0x39c13a54,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCItemMgt_FormatStringOffsetTable[] =
    {
    1060,
    1106,
    1158,
    1198,
    1244,
    1290,
    1336
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCItemMgt_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCItemMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCItemMgt_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCItemMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(10) _IOPCItemMgtProxyVtbl = 
{
    &IOPCItemMgt_ProxyInfo,
    &IID_IOPCItemMgt,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCItemMgt::AddItems */ ,
    (void *) (INT_PTR) -1 /* IOPCItemMgt::ValidateItems */ ,
    (void *) (INT_PTR) -1 /* IOPCItemMgt::RemoveItems */ ,
    (void *) (INT_PTR) -1 /* IOPCItemMgt::SetActiveState */ ,
    (void *) (INT_PTR) -1 /* IOPCItemMgt::SetClientHandles */ ,
    (void *) (INT_PTR) -1 /* IOPCItemMgt::SetDatatypes */ ,
    (void *) (INT_PTR) -1 /* IOPCItemMgt::CreateEnumerator */
};

const CInterfaceStubVtbl _IOPCItemMgtStubVtbl =
{
    &IID_IOPCItemMgt,
    &IOPCItemMgt_ServerInfo,
    10,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IEnumOPCItemAttributes, ver. 0.0,
   GUID={0x39c13a55,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}} */

#pragma code_seg(".orpc")
static const unsigned short IEnumOPCItemAttributes_FormatStringOffsetTable[] =
    {
    1370,
    1410,
    1438,
    1460
    };

static const MIDL_STUBLESS_PROXY_INFO IEnumOPCItemAttributes_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IEnumOPCItemAttributes_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IEnumOPCItemAttributes_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IEnumOPCItemAttributes_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(7) _IEnumOPCItemAttributesProxyVtbl = 
{
    &IEnumOPCItemAttributes_ProxyInfo,
    &IID_IEnumOPCItemAttributes,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IEnumOPCItemAttributes::Next */ ,
    (void *) (INT_PTR) -1 /* IEnumOPCItemAttributes::Skip */ ,
    (void *) (INT_PTR) -1 /* IEnumOPCItemAttributes::Reset */ ,
    (void *) (INT_PTR) -1 /* IEnumOPCItemAttributes::Clone */
};

const CInterfaceStubVtbl _IEnumOPCItemAttributesStubVtbl =
{
    &IID_IEnumOPCItemAttributes,
    &IEnumOPCItemAttributes_ServerInfo,
    7,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCDataCallback, ver. 0.0,
   GUID={0x39c13a70,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCDataCallback_FormatStringOffsetTable[] =
    {
    1488,
    1570,
    1652,
    1710
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCDataCallback_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCDataCallback_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCDataCallback_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCDataCallback_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(7) _IOPCDataCallbackProxyVtbl = 
{
    &IOPCDataCallback_ProxyInfo,
    &IID_IOPCDataCallback,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCDataCallback::OnDataChange */ ,
    (void *) (INT_PTR) -1 /* IOPCDataCallback::OnReadComplete */ ,
    (void *) (INT_PTR) -1 /* IOPCDataCallback::OnWriteComplete */ ,
    (void *) (INT_PTR) -1 /* IOPCDataCallback::OnCancelComplete */
};

const CInterfaceStubVtbl _IOPCDataCallbackStubVtbl =
{
    &IID_IOPCDataCallback,
    &IOPCDataCallback_ServerInfo,
    7,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCAsyncIO2, ver. 0.0,
   GUID={0x39c13a71,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCAsyncIO2_FormatStringOffsetTable[] =
    {
    1744,
    1796,
    1854,
    1032,
    1894,
    1922
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCAsyncIO2_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCAsyncIO2_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCAsyncIO2_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCAsyncIO2_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(9) _IOPCAsyncIO2ProxyVtbl = 
{
    &IOPCAsyncIO2_ProxyInfo,
    &IID_IOPCAsyncIO2,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO2::Read */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO2::Write */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO2::Refresh2 */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO2::Cancel2 */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO2::SetEnable */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO2::GetEnable */
};

const CInterfaceStubVtbl _IOPCAsyncIO2StubVtbl =
{
    &IID_IOPCAsyncIO2,
    &IOPCAsyncIO2_ServerInfo,
    9,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCItemProperties, ver. 0.0,
   GUID={0x39c13a72,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCItemProperties_FormatStringOffsetTable[] =
    {
    1950,
    2002,
    2054
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCItemProperties_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCItemProperties_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCItemProperties_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCItemProperties_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(6) _IOPCItemPropertiesProxyVtbl = 
{
    &IOPCItemProperties_ProxyInfo,
    &IID_IOPCItemProperties,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCItemProperties::QueryAvailableProperties */ ,
    (void *) (INT_PTR) -1 /* IOPCItemProperties::GetItemProperties */ ,
    (void *) (INT_PTR) -1 /* IOPCItemProperties::LookupItemIDs */
};

const CInterfaceStubVtbl _IOPCItemPropertiesStubVtbl =
{
    &IID_IOPCItemProperties,
    &IOPCItemProperties_ServerInfo,
    6,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCItemDeadbandMgt, ver. 0.0,
   GUID={0x5946DA93,0x8B39,0x4ec8,{0xAB,0x3D,0xAA,0x73,0xDF,0x5B,0xC8,0x6F}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCItemDeadbandMgt_FormatStringOffsetTable[] =
    {
    2106,
    2152,
    1158
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCItemDeadbandMgt_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCItemDeadbandMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCItemDeadbandMgt_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCItemDeadbandMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(6) _IOPCItemDeadbandMgtProxyVtbl = 
{
    &IOPCItemDeadbandMgt_ProxyInfo,
    &IID_IOPCItemDeadbandMgt,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCItemDeadbandMgt::SetItemDeadband */ ,
    (void *) (INT_PTR) -1 /* IOPCItemDeadbandMgt::GetItemDeadband */ ,
    (void *) (INT_PTR) -1 /* IOPCItemDeadbandMgt::ClearItemDeadband */
};

const CInterfaceStubVtbl _IOPCItemDeadbandMgtStubVtbl =
{
    &IID_IOPCItemDeadbandMgt,
    &IOPCItemDeadbandMgt_ServerInfo,
    6,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCItemSamplingMgt, ver. 0.0,
   GUID={0x3E22D313,0xF08B,0x41a5,{0x86,0xC8,0x95,0xE9,0x5C,0xB4,0x9F,0xFC}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCItemSamplingMgt_FormatStringOffsetTable[] =
    {
    2198,
    2250,
    1158,
    2296,
    2342
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCItemSamplingMgt_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCItemSamplingMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCItemSamplingMgt_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCItemSamplingMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(8) _IOPCItemSamplingMgtProxyVtbl = 
{
    &IOPCItemSamplingMgt_ProxyInfo,
    &IID_IOPCItemSamplingMgt,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCItemSamplingMgt::SetItemSamplingRate */ ,
    (void *) (INT_PTR) -1 /* IOPCItemSamplingMgt::GetItemSamplingRate */ ,
    (void *) (INT_PTR) -1 /* IOPCItemSamplingMgt::ClearItemSamplingRate */ ,
    (void *) (INT_PTR) -1 /* IOPCItemSamplingMgt::SetItemBufferEnable */ ,
    (void *) (INT_PTR) -1 /* IOPCItemSamplingMgt::GetItemBufferEnable */
};

const CInterfaceStubVtbl _IOPCItemSamplingMgtStubVtbl =
{
    &IID_IOPCItemSamplingMgt,
    &IOPCItemSamplingMgt_ServerInfo,
    8,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCBrowse, ver. 0.0,
   GUID={0x39227004,0xA18F,0x4b57,{0x8B,0x0A,0x52,0x35,0x67,0x0F,0x44,0x68}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCBrowse_FormatStringOffsetTable[] =
    {
    2388,
    2446
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCBrowse_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCBrowse_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCBrowse_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCBrowse_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(5) _IOPCBrowseProxyVtbl = 
{
    &IOPCBrowse_ProxyInfo,
    &IID_IOPCBrowse,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCBrowse::GetProperties */ ,
    (void *) (INT_PTR) -1 /* IOPCBrowse::Browse */
};

const CInterfaceStubVtbl _IOPCBrowseStubVtbl =
{
    &IID_IOPCBrowse,
    &IOPCBrowse_ServerInfo,
    5,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCItemIO, ver. 0.0,
   GUID={0x85C0B427,0x2893,0x4cbc,{0xBD,0x78,0xE5,0xFC,0x51,0x46,0xF0,0x8F}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCItemIO_FormatStringOffsetTable[] =
    {
    2546,
    2610
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCItemIO_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCItemIO_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCItemIO_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCItemIO_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(5) _IOPCItemIOProxyVtbl = 
{
    &IOPCItemIO_ProxyInfo,
    &IID_IOPCItemIO,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCItemIO::Read */ ,
    (void *) (INT_PTR) -1 /* IOPCItemIO::WriteVQT */
};

const CInterfaceStubVtbl _IOPCItemIOStubVtbl =
{
    &IID_IOPCItemIO,
    &IOPCItemIO_ServerInfo,
    5,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCSyncIO2, ver. 0.0,
   GUID={0x730F5F0F,0x55B1,0x4c81,{0x9E,0x18,0xFF,0x8A,0x09,0x04,0xE1,0xFA}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCSyncIO2_FormatStringOffsetTable[] =
    {
    778,
    830,
    2656,
    2720
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCSyncIO2_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCSyncIO2_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCSyncIO2_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCSyncIO2_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(7) _IOPCSyncIO2ProxyVtbl = 
{
    &IOPCSyncIO2_ProxyInfo,
    &IID_IOPCSyncIO2,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCSyncIO::Read */ ,
    (void *) (INT_PTR) -1 /* IOPCSyncIO::Write */ ,
    (void *) (INT_PTR) -1 /* IOPCSyncIO2::ReadMaxAge */ ,
    (void *) (INT_PTR) -1 /* IOPCSyncIO2::WriteVQT */
};

const CInterfaceStubVtbl _IOPCSyncIO2StubVtbl =
{
    &IID_IOPCSyncIO2,
    &IOPCSyncIO2_ServerInfo,
    7,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCAsyncIO3, ver. 0.0,
   GUID={0x0967B97B,0x36EF,0x423e,{0xB6,0xF8,0x6B,0xFF,0x1E,0x40,0xD3,0x9D}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCAsyncIO3_FormatStringOffsetTable[] =
    {
    1744,
    1796,
    1854,
    1032,
    1894,
    1922,
    2766,
    2824,
    2882
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCAsyncIO3_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCAsyncIO3_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCAsyncIO3_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCAsyncIO3_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(12) _IOPCAsyncIO3ProxyVtbl = 
{
    &IOPCAsyncIO3_ProxyInfo,
    &IID_IOPCAsyncIO3,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO2::Read */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO2::Write */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO2::Refresh2 */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO2::Cancel2 */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO2::SetEnable */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO2::GetEnable */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO3::ReadMaxAge */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO3::WriteVQT */ ,
    (void *) (INT_PTR) -1 /* IOPCAsyncIO3::RefreshMaxAge */
};

const CInterfaceStubVtbl _IOPCAsyncIO3StubVtbl =
{
    &IID_IOPCAsyncIO3,
    &IOPCAsyncIO3_ServerInfo,
    12,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};


/* Object interface: IOPCGroupStateMgt2, ver. 0.0,
   GUID={0x8E368666,0xD72E,0x4f78,{0x87,0xED,0x64,0x76,0x11,0xC6,0x1C,0x9F}} */

#pragma code_seg(".orpc")
static const unsigned short IOPCGroupStateMgt2_FormatStringOffsetTable[] =
    {
    526,
    596,
    660,
    688,
    2922,
    1922
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCGroupStateMgt2_ProxyInfo =
    {
    &Object_StubDesc,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCGroupStateMgt2_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };


static const MIDL_SERVER_INFO IOPCGroupStateMgt2_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    opcda__MIDL_ProcFormatString.Format,
    &IOPCGroupStateMgt2_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0};
CINTERFACE_PROXY_VTABLE(9) _IOPCGroupStateMgt2ProxyVtbl = 
{
    &IOPCGroupStateMgt2_ProxyInfo,
    &IID_IOPCGroupStateMgt2,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *) (INT_PTR) -1 /* IOPCGroupStateMgt::GetState */ ,
    (void *) (INT_PTR) -1 /* IOPCGroupStateMgt::SetState */ ,
    (void *) (INT_PTR) -1 /* IOPCGroupStateMgt::SetName */ ,
    (void *) (INT_PTR) -1 /* IOPCGroupStateMgt::CloneGroup */ ,
    (void *) (INT_PTR) -1 /* IOPCGroupStateMgt2::SetKeepAlive */ ,
    (void *) (INT_PTR) -1 /* IOPCGroupStateMgt2::GetKeepAlive */
};

const CInterfaceStubVtbl _IOPCGroupStateMgt2StubVtbl =
{
    &IID_IOPCGroupStateMgt2,
    &IOPCGroupStateMgt2_ServerInfo,
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
    opcda__MIDL_TypeFormatString.Format,
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

const CInterfaceProxyVtbl * const _opcda_ProxyVtblList[] = 
{
    ( CInterfaceProxyVtbl *) &_IOPCBrowseProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCSyncIO2ProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCItemSamplingMgtProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCItemIOProxyVtbl,
    ( CInterfaceProxyVtbl *) &_CATID_OPCDAServer10ProxyVtbl,
    ( CInterfaceProxyVtbl *) &_CATID_OPCDAServer20ProxyVtbl,
    ( CInterfaceProxyVtbl *) &_CATID_OPCDAServer30ProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCServerProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCServerPublicGroupsProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCBrowseServerAddressSpaceProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCGroupStateMgtProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCPublicGroupStateMgtProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCSyncIOProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCAsyncIOProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCItemMgtProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IEnumOPCItemAttributesProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCGroupStateMgt2ProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCDataCallbackProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCAsyncIO2ProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCItemPropertiesProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCAsyncIO3ProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCItemDeadbandMgtProxyVtbl,
    ( CInterfaceProxyVtbl *) &_CATID_XMLDAServer10ProxyVtbl,
    0
};

const CInterfaceStubVtbl * const _opcda_StubVtblList[] = 
{
    ( CInterfaceStubVtbl *) &_IOPCBrowseStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCSyncIO2StubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCItemSamplingMgtStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCItemIOStubVtbl,
    ( CInterfaceStubVtbl *) &_CATID_OPCDAServer10StubVtbl,
    ( CInterfaceStubVtbl *) &_CATID_OPCDAServer20StubVtbl,
    ( CInterfaceStubVtbl *) &_CATID_OPCDAServer30StubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCServerStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCServerPublicGroupsStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCBrowseServerAddressSpaceStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCGroupStateMgtStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCPublicGroupStateMgtStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCSyncIOStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCAsyncIOStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCItemMgtStubVtbl,
    ( CInterfaceStubVtbl *) &_IEnumOPCItemAttributesStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCGroupStateMgt2StubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCDataCallbackStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCAsyncIO2StubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCItemPropertiesStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCAsyncIO3StubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCItemDeadbandMgtStubVtbl,
    ( CInterfaceStubVtbl *) &_CATID_XMLDAServer10StubVtbl,
    0
};

PCInterfaceName const _opcda_InterfaceNamesList[] = 
{
    "IOPCBrowse",
    "IOPCSyncIO2",
    "IOPCItemSamplingMgt",
    "IOPCItemIO",
    "CATID_OPCDAServer10",
    "CATID_OPCDAServer20",
    "CATID_OPCDAServer30",
    "IOPCServer",
    "IOPCServerPublicGroups",
    "IOPCBrowseServerAddressSpace",
    "IOPCGroupStateMgt",
    "IOPCPublicGroupStateMgt",
    "IOPCSyncIO",
    "IOPCAsyncIO",
    "IOPCItemMgt",
    "IEnumOPCItemAttributes",
    "IOPCGroupStateMgt2",
    "IOPCDataCallback",
    "IOPCAsyncIO2",
    "IOPCItemProperties",
    "IOPCAsyncIO3",
    "IOPCItemDeadbandMgt",
    "CATID_XMLDAServer10",
    0
};


#define _opcda_CHECK_IID(n)	IID_GENERIC_CHECK_IID( _opcda, pIID, n)

int __stdcall _opcda_IID_Lookup( const IID * pIID, int * pIndex )
{
    IID_BS_LOOKUP_SETUP

    IID_BS_LOOKUP_INITIAL_TEST( _opcda, 23, 16 )
    IID_BS_LOOKUP_NEXT_TEST( _opcda, 8 )
    IID_BS_LOOKUP_NEXT_TEST( _opcda, 4 )
    IID_BS_LOOKUP_NEXT_TEST( _opcda, 2 )
    IID_BS_LOOKUP_NEXT_TEST( _opcda, 1 )
    IID_BS_LOOKUP_RETURN_RESULT( _opcda, 23, *pIndex )
    
}

const ExtendedProxyFileInfo opcda_ProxyFileInfo = 
{
    (PCInterfaceProxyVtblList *) & _opcda_ProxyVtblList,
    (PCInterfaceStubVtblList *) & _opcda_StubVtblList,
    (const PCInterfaceName * ) & _opcda_InterfaceNamesList,
    0, /* no delegation */
    & _opcda_IID_Lookup, 
    23,
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

