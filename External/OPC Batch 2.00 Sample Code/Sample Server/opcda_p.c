/* this ALWAYS GENERATED file contains the proxy stub code */


/* File created by MIDL compiler version 3.01.75 */
/* at Wed Oct 14 12:14:58 1998
 */
/* Compiler settings for opcda.idl:
    Oicf (OptLev=i2), W1, Zp8, env=Win32, ms_ext, c_ext
    error checks: none
*/
//@@MIDL_FILE_HEADING(  )

#define USE_STUBLESS_PROXY

#include "rpcproxy.h"
#include "opcda.h"

#define TYPE_FORMAT_STRING_SIZE   1779                              
#define PROC_FORMAT_STRING_SIZE   1915                              

typedef struct _MIDL_TYPE_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ TYPE_FORMAT_STRING_SIZE ];
    } MIDL_TYPE_FORMAT_STRING;

typedef struct _MIDL_PROC_FORMAT_STRING
    {
    short          Pad;
    unsigned char  Format[ PROC_FORMAT_STRING_SIZE ];
    } MIDL_PROC_FORMAT_STRING;


extern const MIDL_TYPE_FORMAT_STRING __MIDL_TypeFormatString;
extern const MIDL_PROC_FORMAT_STRING __MIDL_ProcFormatString;


/* Standard interface: __MIDL_itf_opcda_0000, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}} */


/* Object interface: IUnknown, ver. 0.0,
   GUID={0x00000000,0x0000,0x0000,{0xC0,0x00,0x00,0x00,0x00,0x00,0x00,0x46}} */


/* Object interface: IOPCServer, ver. 0.0,
   GUID={0x39c13a4d,0x011e,0x11d0,{0x96,0x75,0x00,0x20,0xaf,0xd8,0xad,0xb3}} */


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCServer_ServerInfo;

#pragma code_seg(".orpc")
static const unsigned short IOPCServer_FormatStringOffsetTable[] = 
    {
    0,
    84,
    120,
    156,
    180,
    210
    };

static const MIDL_SERVER_INFO IOPCServer_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    __MIDL_ProcFormatString.Format,
    &IOPCServer_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCServer_ProxyInfo =
    {
    &Object_StubDesc,
    __MIDL_ProcFormatString.Format,
    &IOPCServer_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };

CINTERFACE_PROXY_VTABLE(9) _IOPCServerProxyVtbl = 
{
    &IOPCServer_ProxyInfo,
    &IID_IOPCServer,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *)-1 /* IOPCServer::AddGroup */ ,
    (void *)-1 /* IOPCServer::GetErrorString */ ,
    (void *)-1 /* IOPCServer::GetGroupByName */ ,
    (void *)-1 /* IOPCServer::GetStatus */ ,
    (void *)-1 /* IOPCServer::RemoveGroup */ ,
    (void *)-1 /* IOPCServer::CreateGroupEnumerator */
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


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCServerPublicGroups_ServerInfo;

#pragma code_seg(".orpc")
static const unsigned short IOPCServerPublicGroups_FormatStringOffsetTable[] = 
    {
    246,
    282
    };

static const MIDL_SERVER_INFO IOPCServerPublicGroups_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    __MIDL_ProcFormatString.Format,
    &IOPCServerPublicGroups_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCServerPublicGroups_ProxyInfo =
    {
    &Object_StubDesc,
    __MIDL_ProcFormatString.Format,
    &IOPCServerPublicGroups_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };

CINTERFACE_PROXY_VTABLE(5) _IOPCServerPublicGroupsProxyVtbl = 
{
    &IOPCServerPublicGroups_ProxyInfo,
    &IID_IOPCServerPublicGroups,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *)-1 /* IOPCServerPublicGroups::GetPublicGroupByName */ ,
    (void *)-1 /* IOPCServerPublicGroups::RemovePublicGroup */
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


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCBrowseServerAddressSpace_ServerInfo;

#pragma code_seg(".orpc")
static const unsigned short IOPCBrowseServerAddressSpace_FormatStringOffsetTable[] = 
    {
    312,
    336,
    366,
    414,
    444
    };

static const MIDL_SERVER_INFO IOPCBrowseServerAddressSpace_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    __MIDL_ProcFormatString.Format,
    &IOPCBrowseServerAddressSpace_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCBrowseServerAddressSpace_ProxyInfo =
    {
    &Object_StubDesc,
    __MIDL_ProcFormatString.Format,
    &IOPCBrowseServerAddressSpace_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };

CINTERFACE_PROXY_VTABLE(8) _IOPCBrowseServerAddressSpaceProxyVtbl = 
{
    &IOPCBrowseServerAddressSpace_ProxyInfo,
    &IID_IOPCBrowseServerAddressSpace,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *)-1 /* IOPCBrowseServerAddressSpace::QueryOrganization */ ,
    (void *)-1 /* IOPCBrowseServerAddressSpace::ChangeBrowsePosition */ ,
    (void *)-1 /* IOPCBrowseServerAddressSpace::BrowseOPCItemIDs */ ,
    (void *)-1 /* IOPCBrowseServerAddressSpace::GetItemID */ ,
    (void *)-1 /* IOPCBrowseServerAddressSpace::BrowseAccessPaths */
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


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCGroupStateMgt_ServerInfo;

#pragma code_seg(".orpc")
static const unsigned short IOPCGroupStateMgt_FormatStringOffsetTable[] = 
    {
    474,
    540,
    600,
    624
    };

static const MIDL_SERVER_INFO IOPCGroupStateMgt_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    __MIDL_ProcFormatString.Format,
    &IOPCGroupStateMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCGroupStateMgt_ProxyInfo =
    {
    &Object_StubDesc,
    __MIDL_ProcFormatString.Format,
    &IOPCGroupStateMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };

CINTERFACE_PROXY_VTABLE(7) _IOPCGroupStateMgtProxyVtbl = 
{
    &IOPCGroupStateMgt_ProxyInfo,
    &IID_IOPCGroupStateMgt,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *)-1 /* IOPCGroupStateMgt::GetState */ ,
    (void *)-1 /* IOPCGroupStateMgt::SetState */ ,
    (void *)-1 /* IOPCGroupStateMgt::SetName */ ,
    (void *)-1 /* IOPCGroupStateMgt::CloneGroup */
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


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCPublicGroupStateMgt_ServerInfo;

#pragma code_seg(".orpc")
static const unsigned short IOPCPublicGroupStateMgt_FormatStringOffsetTable[] = 
    {
    660,
    684
    };

static const MIDL_SERVER_INFO IOPCPublicGroupStateMgt_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    __MIDL_ProcFormatString.Format,
    &IOPCPublicGroupStateMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCPublicGroupStateMgt_ProxyInfo =
    {
    &Object_StubDesc,
    __MIDL_ProcFormatString.Format,
    &IOPCPublicGroupStateMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };

CINTERFACE_PROXY_VTABLE(5) _IOPCPublicGroupStateMgtProxyVtbl = 
{
    &IOPCPublicGroupStateMgt_ProxyInfo,
    &IID_IOPCPublicGroupStateMgt,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *)-1 /* IOPCPublicGroupStateMgt::GetState */ ,
    (void *)-1 /* IOPCPublicGroupStateMgt::MoveToPublic */
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


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCSyncIO_ServerInfo;

#pragma code_seg(".orpc")
static const unsigned short IOPCSyncIO_FormatStringOffsetTable[] = 
    {
    702,
    750
    };

static const MIDL_SERVER_INFO IOPCSyncIO_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    __MIDL_ProcFormatString.Format,
    &IOPCSyncIO_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCSyncIO_ProxyInfo =
    {
    &Object_StubDesc,
    __MIDL_ProcFormatString.Format,
    &IOPCSyncIO_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };

CINTERFACE_PROXY_VTABLE(5) _IOPCSyncIOProxyVtbl = 
{
    &IOPCSyncIO_ProxyInfo,
    &IID_IOPCSyncIO,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *)-1 /* IOPCSyncIO::Read */ ,
    (void *)-1 /* IOPCSyncIO::Write */
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


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCAsyncIO_ServerInfo;

#pragma code_seg(".orpc")
static const unsigned short IOPCAsyncIO_FormatStringOffsetTable[] = 
    {
    792,
    846,
    900,
    936
    };

static const MIDL_SERVER_INFO IOPCAsyncIO_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    __MIDL_ProcFormatString.Format,
    &IOPCAsyncIO_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCAsyncIO_ProxyInfo =
    {
    &Object_StubDesc,
    __MIDL_ProcFormatString.Format,
    &IOPCAsyncIO_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };

CINTERFACE_PROXY_VTABLE(7) _IOPCAsyncIOProxyVtbl = 
{
    &IOPCAsyncIO_ProxyInfo,
    &IID_IOPCAsyncIO,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *)-1 /* IOPCAsyncIO::Read */ ,
    (void *)-1 /* IOPCAsyncIO::Write */ ,
    (void *)-1 /* IOPCAsyncIO::Refresh */ ,
    (void *)-1 /* IOPCAsyncIO::Cancel */
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


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCItemMgt_ServerInfo;

#pragma code_seg(".orpc")
static const unsigned short IOPCItemMgt_FormatStringOffsetTable[] = 
    {
    960,
    1002,
    1050,
    1086,
    1128,
    1170,
    1212
    };

static const MIDL_SERVER_INFO IOPCItemMgt_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    __MIDL_ProcFormatString.Format,
    &IOPCItemMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCItemMgt_ProxyInfo =
    {
    &Object_StubDesc,
    __MIDL_ProcFormatString.Format,
    &IOPCItemMgt_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };

CINTERFACE_PROXY_VTABLE(10) _IOPCItemMgtProxyVtbl = 
{
    &IOPCItemMgt_ProxyInfo,
    &IID_IOPCItemMgt,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *)-1 /* IOPCItemMgt::AddItems */ ,
    (void *)-1 /* IOPCItemMgt::ValidateItems */ ,
    (void *)-1 /* IOPCItemMgt::RemoveItems */ ,
    (void *)-1 /* IOPCItemMgt::SetActiveState */ ,
    (void *)-1 /* IOPCItemMgt::SetClientHandles */ ,
    (void *)-1 /* IOPCItemMgt::SetDatatypes */ ,
    (void *)-1 /* IOPCItemMgt::CreateEnumerator */
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


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IEnumOPCItemAttributes_ServerInfo;

#pragma code_seg(".orpc")
static const unsigned short IEnumOPCItemAttributes_FormatStringOffsetTable[] = 
    {
    1242,
    1278,
    1302,
    1320
    };

static const MIDL_SERVER_INFO IEnumOPCItemAttributes_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    __MIDL_ProcFormatString.Format,
    &IEnumOPCItemAttributes_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IEnumOPCItemAttributes_ProxyInfo =
    {
    &Object_StubDesc,
    __MIDL_ProcFormatString.Format,
    &IEnumOPCItemAttributes_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };

CINTERFACE_PROXY_VTABLE(7) _IEnumOPCItemAttributesProxyVtbl = 
{
    &IEnumOPCItemAttributes_ProxyInfo,
    &IID_IEnumOPCItemAttributes,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *)-1 /* IEnumOPCItemAttributes::Next */ ,
    (void *)-1 /* IEnumOPCItemAttributes::Skip */ ,
    (void *)-1 /* IEnumOPCItemAttributes::Reset */ ,
    (void *)-1 /* IEnumOPCItemAttributes::Clone */
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


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCDataCallback_ServerInfo;

#pragma code_seg(".orpc")
static const unsigned short IOPCDataCallback_FormatStringOffsetTable[] = 
    {
    1344,
    1422,
    1500,
    1554
    };

static const MIDL_SERVER_INFO IOPCDataCallback_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    __MIDL_ProcFormatString.Format,
    &IOPCDataCallback_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCDataCallback_ProxyInfo =
    {
    &Object_StubDesc,
    __MIDL_ProcFormatString.Format,
    &IOPCDataCallback_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };

CINTERFACE_PROXY_VTABLE(7) _IOPCDataCallbackProxyVtbl = 
{
    &IOPCDataCallback_ProxyInfo,
    &IID_IOPCDataCallback,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *)-1 /* IOPCDataCallback::OnDataChange */ ,
    (void *)-1 /* IOPCDataCallback::OnReadComplete */ ,
    (void *)-1 /* IOPCDataCallback::OnWriteComplete */ ,
    (void *)-1 /* IOPCDataCallback::OnCancelComplete */
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


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCAsyncIO2_ServerInfo;

#pragma code_seg(".orpc")
static const unsigned short IOPCAsyncIO2_FormatStringOffsetTable[] = 
    {
    1584,
    1632,
    1686,
    936,
    1722,
    1746
    };

static const MIDL_SERVER_INFO IOPCAsyncIO2_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    __MIDL_ProcFormatString.Format,
    &IOPCAsyncIO2_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCAsyncIO2_ProxyInfo =
    {
    &Object_StubDesc,
    __MIDL_ProcFormatString.Format,
    &IOPCAsyncIO2_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };

CINTERFACE_PROXY_VTABLE(9) _IOPCAsyncIO2ProxyVtbl = 
{
    &IOPCAsyncIO2_ProxyInfo,
    &IID_IOPCAsyncIO2,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *)-1 /* IOPCAsyncIO2::Read */ ,
    (void *)-1 /* IOPCAsyncIO2::Write */ ,
    (void *)-1 /* IOPCAsyncIO2::Refresh2 */ ,
    (void *)-1 /* IOPCAsyncIO2::Cancel2 */ ,
    (void *)-1 /* IOPCAsyncIO2::SetEnable */ ,
    (void *)-1 /* IOPCAsyncIO2::GetEnable */
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


extern const MIDL_STUB_DESC Object_StubDesc;


extern const MIDL_SERVER_INFO IOPCItemProperties_ServerInfo;

#pragma code_seg(".orpc")
extern const USER_MARSHAL_ROUTINE_QUADRUPLE UserMarshalRoutines[1];

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
    __MIDL_TypeFormatString.Format,
    0, /* -error bounds_check flag */
    0x20000, /* Ndr library version */
    0,
    0x301004b, /* MIDL Version 3.1.75 */
    0,
    UserMarshalRoutines,
    0,  /* Reserved1 */
    0,  /* Reserved2 */
    0,  /* Reserved3 */
    0,  /* Reserved4 */
    0   /* Reserved5 */
    };

static const unsigned short IOPCItemProperties_FormatStringOffsetTable[] = 
    {
    1770,
    1818,
    1866
    };

static const MIDL_SERVER_INFO IOPCItemProperties_ServerInfo = 
    {
    &Object_StubDesc,
    0,
    __MIDL_ProcFormatString.Format,
    &IOPCItemProperties_FormatStringOffsetTable[-3],
    0,
    0,
    0,
    0
    };

static const MIDL_STUBLESS_PROXY_INFO IOPCItemProperties_ProxyInfo =
    {
    &Object_StubDesc,
    __MIDL_ProcFormatString.Format,
    &IOPCItemProperties_FormatStringOffsetTable[-3],
    0,
    0,
    0
    };

CINTERFACE_PROXY_VTABLE(6) _IOPCItemPropertiesProxyVtbl = 
{
    &IOPCItemProperties_ProxyInfo,
    &IID_IOPCItemProperties,
    IUnknown_QueryInterface_Proxy,
    IUnknown_AddRef_Proxy,
    IUnknown_Release_Proxy ,
    (void *)-1 /* IOPCItemProperties::QueryAvailableProperties */ ,
    (void *)-1 /* IOPCItemProperties::GetItemProperties */ ,
    (void *)-1 /* IOPCItemProperties::LookupItemIDs */
};

const CInterfaceStubVtbl _IOPCItemPropertiesStubVtbl =
{
    &IID_IOPCItemProperties,
    &IOPCItemProperties_ServerInfo,
    6,
    0, /* pure interpreted */
    CStdStubBuffer_METHODS
};

#pragma data_seg(".rdata")

static const USER_MARSHAL_ROUTINE_QUADRUPLE UserMarshalRoutines[1] = 
        {
            
            {
            VARIANT_UserSize
            ,VARIANT_UserMarshal
            ,VARIANT_UserUnmarshal
            ,VARIANT_UserFree
            }

        };


#if !defined(__RPC_WIN32__)
#error  Invalid build platform for this stub.
#endif

#if !(TARGET_IS_NT40_OR_LATER)
#error You need a Windows NT 4.0 or later to run this stub because it uses these features:
#error   -Oif or -Oicf, [wire_marshal] or [user_marshal] attribute.
#error However, your C/C++ compilation flags indicate you intend to run this app on earlier systems.
#error This app will die there with the RPC_X_WRONG_STUB_VERSION error.
#endif


static const MIDL_PROC_FORMAT_STRING __MIDL_ProcFormatString =
    {
        0,
        {

	/* Procedure AddGroup */

			0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/*  2 */	NdrFcShort( 0x3 ),	/* 3 */
#ifndef _ALPHA_
/*  4 */	NdrFcShort( 0x34 ),	/* x86, MIPS, PPC Stack size/offset = 52 */
#else
			NdrFcShort( 0x68 ),	/* Alpha Stack size/offset = 104 */
#endif
/*  6 */	NdrFcShort( 0x60 ),	/* 96 */
/*  8 */	NdrFcShort( 0x18 ),	/* 24 */
/* 10 */	0x7,		/* 7 */
			0xc,		/* 12 */

	/* Parameter szName */

/* 12 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 14 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 16 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Parameter bActive */

/* 18 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 20 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 22 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwRequestedUpdateRate */

/* 24 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 26 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 28 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hClientGroup */

/* 30 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 32 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 34 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pTimeBias */

/* 36 */	NdrFcShort( 0xa ),	/* 10 */
#ifndef _ALPHA_
/* 38 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 40 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter pPercentDeadband */

/* 42 */	NdrFcShort( 0xa ),	/* 10 */
#ifndef _ALPHA_
/* 44 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 46 */	NdrFcShort( 0x8 ),	/* Type Offset=8 */

	/* Parameter dwLCID */

/* 48 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 50 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 52 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServerGroup */

/* 54 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 56 */	NdrFcShort( 0x20 ),	/* x86, MIPS, PPC Stack size/offset = 32 */
#else
			NdrFcShort( 0x40 ),	/* Alpha Stack size/offset = 64 */
#endif
/* 58 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pRevisedUpdateRate */

/* 60 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 62 */	NdrFcShort( 0x24 ),	/* x86, MIPS, PPC Stack size/offset = 36 */
#else
			NdrFcShort( 0x48 ),	/* Alpha Stack size/offset = 72 */
#endif
/* 64 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter riid */

/* 66 */	NdrFcShort( 0x10a ),	/* 266 */
#ifndef _ALPHA_
/* 68 */	NdrFcShort( 0x28 ),	/* x86, MIPS, PPC Stack size/offset = 40 */
#else
			NdrFcShort( 0x50 ),	/* Alpha Stack size/offset = 80 */
#endif
/* 70 */	NdrFcShort( 0x1a ),	/* Type Offset=26 */

	/* Parameter ppUnk */

/* 72 */	NdrFcShort( 0x13 ),	/* 19 */
#ifndef _ALPHA_
/* 74 */	NdrFcShort( 0x2c ),	/* x86, MIPS, PPC Stack size/offset = 44 */
#else
			NdrFcShort( 0x58 ),	/* Alpha Stack size/offset = 88 */
#endif
/* 76 */	NdrFcShort( 0x26 ),	/* Type Offset=38 */

	/* Return value */

/* 78 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 80 */	NdrFcShort( 0x30 ),	/* x86, MIPS, PPC Stack size/offset = 48 */
#else
			NdrFcShort( 0x60 ),	/* Alpha Stack size/offset = 96 */
#endif
/* 82 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetErrorString */

/* 84 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 86 */	NdrFcShort( 0x4 ),	/* 4 */
#ifndef _ALPHA_
/* 88 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 90 */	NdrFcShort( 0x10 ),	/* 16 */
/* 92 */	NdrFcShort( 0x8 ),	/* 8 */
/* 94 */	0x5,		/* 5 */
			0x4,		/* 4 */

	/* Parameter dwError */

/* 96 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 98 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 100 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwLocale */

/* 102 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 104 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 106 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppString */

/* 108 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 110 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 112 */	NdrFcShort( 0x30 ),	/* Type Offset=48 */

	/* Return value */

/* 114 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 116 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 118 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetGroupByName */

/* 120 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 122 */	NdrFcShort( 0x5 ),	/* 5 */
#ifndef _ALPHA_
/* 124 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 126 */	NdrFcShort( 0x20 ),	/* 32 */
/* 128 */	NdrFcShort( 0x8 ),	/* 8 */
/* 130 */	0x7,		/* 7 */
			0x4,		/* 4 */

	/* Parameter szName */

/* 132 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 134 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 136 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Parameter riid */

/* 138 */	NdrFcShort( 0x10a ),	/* 266 */
#ifndef _ALPHA_
/* 140 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 142 */	NdrFcShort( 0x1a ),	/* Type Offset=26 */

	/* Parameter ppUnk */

/* 144 */	NdrFcShort( 0x13 ),	/* 19 */
#ifndef _ALPHA_
/* 146 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 148 */	NdrFcShort( 0x38 ),	/* Type Offset=56 */

	/* Return value */

/* 150 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 152 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 154 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetStatus */

/* 156 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 158 */	NdrFcShort( 0x6 ),	/* 6 */
#ifndef _ALPHA_
/* 160 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 162 */	NdrFcShort( 0x0 ),	/* 0 */
/* 164 */	NdrFcShort( 0x8 ),	/* 8 */
/* 166 */	0x5,		/* 5 */
			0x2,		/* 2 */

	/* Parameter ppServerStatus */

/* 168 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 170 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 172 */	NdrFcShort( 0x42 ),	/* Type Offset=66 */

	/* Return value */

/* 174 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 176 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 178 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure RemoveGroup */

/* 180 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 182 */	NdrFcShort( 0x7 ),	/* 7 */
#ifndef _ALPHA_
/* 184 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 186 */	NdrFcShort( 0x10 ),	/* 16 */
/* 188 */	NdrFcShort( 0x8 ),	/* 8 */
/* 190 */	0x4,		/* 4 */
			0x3,		/* 3 */

	/* Parameter hServerGroup */

/* 192 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 194 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 196 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter bForce */

/* 198 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 200 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 202 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 204 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 206 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 208 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CreateGroupEnumerator */

/* 210 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 212 */	NdrFcShort( 0x8 ),	/* 8 */
#ifndef _ALPHA_
/* 214 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 216 */	NdrFcShort( 0x26 ),	/* 38 */
/* 218 */	NdrFcShort( 0x8 ),	/* 8 */
/* 220 */	0x5,		/* 5 */
			0x4,		/* 4 */

	/* Parameter dwScope */

/* 222 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 224 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 226 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter riid */

/* 228 */	NdrFcShort( 0x10a ),	/* 266 */
#ifndef _ALPHA_
/* 230 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 232 */	NdrFcShort( 0x1a ),	/* Type Offset=26 */

	/* Parameter ppUnk */

/* 234 */	NdrFcShort( 0x13 ),	/* 19 */
#ifndef _ALPHA_
/* 236 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 238 */	NdrFcShort( 0x74 ),	/* Type Offset=116 */

	/* Return value */

/* 240 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 242 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 244 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetPublicGroupByName */

/* 246 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 248 */	NdrFcShort( 0x3 ),	/* 3 */
#ifndef _ALPHA_
/* 250 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 252 */	NdrFcShort( 0x20 ),	/* 32 */
/* 254 */	NdrFcShort( 0x8 ),	/* 8 */
/* 256 */	0x7,		/* 7 */
			0x4,		/* 4 */

	/* Parameter szName */

/* 258 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 260 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 262 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Parameter riid */

/* 264 */	NdrFcShort( 0x10a ),	/* 266 */
#ifndef _ALPHA_
/* 266 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 268 */	NdrFcShort( 0x1a ),	/* Type Offset=26 */

	/* Parameter ppUnk */

/* 270 */	NdrFcShort( 0x13 ),	/* 19 */
#ifndef _ALPHA_
/* 272 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 274 */	NdrFcShort( 0x7e ),	/* Type Offset=126 */

	/* Return value */

/* 276 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 278 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 280 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure RemovePublicGroup */

/* 282 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 284 */	NdrFcShort( 0x4 ),	/* 4 */
#ifndef _ALPHA_
/* 286 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 288 */	NdrFcShort( 0x10 ),	/* 16 */
/* 290 */	NdrFcShort( 0x8 ),	/* 8 */
/* 292 */	0x4,		/* 4 */
			0x3,		/* 3 */

	/* Parameter hServerGroup */

/* 294 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 296 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 298 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter bForce */

/* 300 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 302 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 304 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 306 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 308 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 310 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryOrganization */

/* 312 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 314 */	NdrFcShort( 0x3 ),	/* 3 */
#ifndef _ALPHA_
/* 316 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 318 */	NdrFcShort( 0x0 ),	/* 0 */
/* 320 */	NdrFcShort( 0xe ),	/* 14 */
/* 322 */	0x4,		/* 4 */
			0x2,		/* 2 */

	/* Parameter pNameSpaceType */

/* 324 */	NdrFcShort( 0x2010 ),	/* 8208 */
#ifndef _ALPHA_
/* 326 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 328 */	NdrFcShort( 0x88 ),	/* Type Offset=136 */

	/* Return value */

/* 330 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 332 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 334 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ChangeBrowsePosition */

/* 336 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 338 */	NdrFcShort( 0x4 ),	/* 4 */
#ifndef _ALPHA_
/* 340 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 342 */	NdrFcShort( 0x6 ),	/* 6 */
/* 344 */	NdrFcShort( 0x8 ),	/* 8 */
/* 346 */	0x6,		/* 6 */
			0x3,		/* 3 */

	/* Parameter dwBrowseDirection */

/* 348 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 350 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 352 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter szString */

/* 354 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 356 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 358 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Return value */

/* 360 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 362 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 364 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure BrowseOPCItemIDs */

/* 366 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 368 */	NdrFcShort( 0x5 ),	/* 5 */
#ifndef _ALPHA_
/* 370 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 372 */	NdrFcShort( 0x14 ),	/* 20 */
/* 374 */	NdrFcShort( 0x8 ),	/* 8 */
/* 376 */	0x7,		/* 7 */
			0x6,		/* 6 */

	/* Parameter dwBrowseFilterType */

/* 378 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 380 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 382 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter szFilterCriteria */

/* 384 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 386 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 388 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Parameter vtDataTypeFilter */

/* 390 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 392 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 394 */	0x6,		/* FC_SHORT */
			0x0,		/* 0 */

	/* Parameter dwAccessRightsFilter */

/* 396 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 398 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 400 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppIEnumString */

/* 402 */	NdrFcShort( 0x13 ),	/* 19 */
#ifndef _ALPHA_
/* 404 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 406 */	NdrFcShort( 0x8c ),	/* Type Offset=140 */

	/* Return value */

/* 408 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 410 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 412 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetItemID */

/* 414 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 416 */	NdrFcShort( 0x6 ),	/* 6 */
#ifndef _ALPHA_
/* 418 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 420 */	NdrFcShort( 0x0 ),	/* 0 */
/* 422 */	NdrFcShort( 0x8 ),	/* 8 */
/* 424 */	0x7,		/* 7 */
			0x3,		/* 3 */

	/* Parameter szItemDataID */

/* 426 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 428 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 430 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Parameter szItemID */

/* 432 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 434 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 436 */	NdrFcShort( 0x30 ),	/* Type Offset=48 */

	/* Return value */

/* 438 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 440 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 442 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure BrowseAccessPaths */

/* 444 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 446 */	NdrFcShort( 0x7 ),	/* 7 */
#ifndef _ALPHA_
/* 448 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 450 */	NdrFcShort( 0x0 ),	/* 0 */
/* 452 */	NdrFcShort( 0x8 ),	/* 8 */
/* 454 */	0x7,		/* 7 */
			0x3,		/* 3 */

	/* Parameter szItemID */

/* 456 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 458 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 460 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Parameter ppIEnumString */

/* 462 */	NdrFcShort( 0x13 ),	/* 19 */
#ifndef _ALPHA_
/* 464 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 466 */	NdrFcShort( 0x8c ),	/* Type Offset=140 */

	/* Return value */

/* 468 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 470 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 472 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetState */

/* 474 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 476 */	NdrFcShort( 0x3 ),	/* 3 */
#ifndef _ALPHA_
/* 478 */	NdrFcShort( 0x28 ),	/* x86, MIPS, PPC Stack size/offset = 40 */
#else
			NdrFcShort( 0x50 ),	/* Alpha Stack size/offset = 80 */
#endif
/* 480 */	NdrFcShort( 0x0 ),	/* 0 */
/* 482 */	NdrFcShort( 0x40 ),	/* 64 */
/* 484 */	0x5,		/* 5 */
			0x9,		/* 9 */

	/* Parameter pUpdateRate */

/* 486 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 488 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 490 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pActive */

/* 492 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 494 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 496 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppName */

/* 498 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 500 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 502 */	NdrFcShort( 0x30 ),	/* Type Offset=48 */

	/* Parameter pTimeBias */

/* 504 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 506 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 508 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pPercentDeadband */

/* 510 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 512 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 514 */	0xa,		/* FC_FLOAT */
			0x0,		/* 0 */

	/* Parameter pLCID */

/* 516 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 518 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 520 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phClientGroup */

/* 522 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 524 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 526 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServerGroup */

/* 528 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 530 */	NdrFcShort( 0x20 ),	/* x86, MIPS, PPC Stack size/offset = 32 */
#else
			NdrFcShort( 0x40 ),	/* Alpha Stack size/offset = 64 */
#endif
/* 532 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 534 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 536 */	NdrFcShort( 0x24 ),	/* x86, MIPS, PPC Stack size/offset = 36 */
#else
			NdrFcShort( 0x48 ),	/* Alpha Stack size/offset = 72 */
#endif
/* 538 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetState */

/* 540 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 542 */	NdrFcShort( 0x4 ),	/* 4 */
#ifndef _ALPHA_
/* 544 */	NdrFcShort( 0x24 ),	/* x86, MIPS, PPC Stack size/offset = 36 */
#else
			NdrFcShort( 0x48 ),	/* Alpha Stack size/offset = 72 */
#endif
/* 546 */	NdrFcShort( 0x60 ),	/* 96 */
/* 548 */	NdrFcShort( 0x10 ),	/* 16 */
/* 550 */	0x4,		/* 4 */
			0x8,		/* 8 */

	/* Parameter pRequestedUpdateRate */

/* 552 */	NdrFcShort( 0xa ),	/* 10 */
#ifndef _ALPHA_
/* 554 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 556 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter pRevisedUpdateRate */

/* 558 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 560 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 562 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pActive */

/* 564 */	NdrFcShort( 0xa ),	/* 10 */
#ifndef _ALPHA_
/* 566 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 568 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter pTimeBias */

/* 570 */	NdrFcShort( 0xa ),	/* 10 */
#ifndef _ALPHA_
/* 572 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 574 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter pPercentDeadband */

/* 576 */	NdrFcShort( 0xa ),	/* 10 */
#ifndef _ALPHA_
/* 578 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 580 */	NdrFcShort( 0x8 ),	/* Type Offset=8 */

	/* Parameter pLCID */

/* 582 */	NdrFcShort( 0xa ),	/* 10 */
#ifndef _ALPHA_
/* 584 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 586 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Parameter phClientGroup */

/* 588 */	NdrFcShort( 0xa ),	/* 10 */
#ifndef _ALPHA_
/* 590 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 592 */	NdrFcShort( 0x4 ),	/* Type Offset=4 */

	/* Return value */

/* 594 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 596 */	NdrFcShort( 0x20 ),	/* x86, MIPS, PPC Stack size/offset = 32 */
#else
			NdrFcShort( 0x40 ),	/* Alpha Stack size/offset = 64 */
#endif
/* 598 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetName */

/* 600 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 602 */	NdrFcShort( 0x5 ),	/* 5 */
#ifndef _ALPHA_
/* 604 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 606 */	NdrFcShort( 0x0 ),	/* 0 */
/* 608 */	NdrFcShort( 0x8 ),	/* 8 */
/* 610 */	0x6,		/* 6 */
			0x2,		/* 2 */

	/* Parameter szName */

/* 612 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 614 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 616 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Return value */

/* 618 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 620 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 622 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CloneGroup */

/* 624 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 626 */	NdrFcShort( 0x6 ),	/* 6 */
#ifndef _ALPHA_
/* 628 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 630 */	NdrFcShort( 0x20 ),	/* 32 */
/* 632 */	NdrFcShort( 0x8 ),	/* 8 */
/* 634 */	0x7,		/* 7 */
			0x4,		/* 4 */

	/* Parameter szName */

/* 636 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 638 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 640 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Parameter riid */

/* 642 */	NdrFcShort( 0x10a ),	/* 266 */
#ifndef _ALPHA_
/* 644 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 646 */	NdrFcShort( 0x1a ),	/* Type Offset=26 */

	/* Parameter ppUnk */

/* 648 */	NdrFcShort( 0x13 ),	/* 19 */
#ifndef _ALPHA_
/* 650 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 652 */	NdrFcShort( 0xa6 ),	/* Type Offset=166 */

	/* Return value */

/* 654 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 656 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 658 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetState */

/* 660 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 662 */	NdrFcShort( 0x3 ),	/* 3 */
#ifndef _ALPHA_
/* 664 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 666 */	NdrFcShort( 0x0 ),	/* 0 */
/* 668 */	NdrFcShort( 0x10 ),	/* 16 */
/* 670 */	0x4,		/* 4 */
			0x2,		/* 2 */

	/* Parameter pPublic */

/* 672 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 674 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 676 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 678 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 680 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 682 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure MoveToPublic */

/* 684 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 686 */	NdrFcShort( 0x4 ),	/* 4 */
#ifndef _ALPHA_
/* 688 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 690 */	NdrFcShort( 0x0 ),	/* 0 */
/* 692 */	NdrFcShort( 0x8 ),	/* 8 */
/* 694 */	0x4,		/* 4 */
			0x1,		/* 1 */

	/* Return value */

/* 696 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 698 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 700 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Read */

/* 702 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 704 */	NdrFcShort( 0x3 ),	/* 3 */
#ifndef _ALPHA_
/* 706 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 708 */	NdrFcShort( 0xe ),	/* 14 */
/* 710 */	NdrFcShort( 0x8 ),	/* 8 */
/* 712 */	0x7,		/* 7 */
			0x6,		/* 6 */

	/* Parameter dwSource */

/* 714 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 716 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 718 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 720 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 722 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 724 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 726 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 728 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 730 */	NdrFcShort( 0xb4 ),	/* Type Offset=180 */

	/* Parameter ppItemValues */

/* 732 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 734 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 736 */	NdrFcShort( 0xbe ),	/* Type Offset=190 */

	/* Parameter ppErrors */

/* 738 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 740 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 742 */	NdrFcShort( 0x410 ),	/* Type Offset=1040 */

	/* Return value */

/* 744 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 746 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 748 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Write */

/* 750 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 752 */	NdrFcShort( 0x4 ),	/* 4 */
#ifndef _ALPHA_
/* 754 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 756 */	NdrFcShort( 0x8 ),	/* 8 */
/* 758 */	NdrFcShort( 0x8 ),	/* 8 */
/* 760 */	0x7,		/* 7 */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 762 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 764 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 766 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 768 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 770 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 772 */	NdrFcShort( 0x41c ),	/* Type Offset=1052 */

	/* Parameter pItemValues */

/* 774 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 776 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 778 */	NdrFcShort( 0x438 ),	/* Type Offset=1080 */

	/* Parameter ppErrors */

/* 780 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 782 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 784 */	NdrFcShort( 0x44a ),	/* Type Offset=1098 */

	/* Return value */

/* 786 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 788 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 790 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Read */

/* 792 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 794 */	NdrFcShort( 0x3 ),	/* 3 */
#ifndef _ALPHA_
/* 796 */	NdrFcShort( 0x20 ),	/* x86, MIPS, PPC Stack size/offset = 32 */
#else
			NdrFcShort( 0x40 ),	/* Alpha Stack size/offset = 64 */
#endif
/* 798 */	NdrFcShort( 0x16 ),	/* 22 */
/* 800 */	NdrFcShort( 0x10 ),	/* 16 */
/* 802 */	0x7,		/* 7 */
			0x7,		/* 7 */

	/* Parameter dwConnection */

/* 804 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 806 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 808 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwSource */

/* 810 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 812 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 814 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 816 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 818 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 820 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 822 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 824 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 826 */	NdrFcShort( 0x456 ),	/* Type Offset=1110 */

	/* Parameter pTransactionID */

/* 828 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 830 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 832 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 834 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 836 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 838 */	NdrFcShort( 0x460 ),	/* Type Offset=1120 */

	/* Return value */

/* 840 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 842 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 844 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Write */

/* 846 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 848 */	NdrFcShort( 0x4 ),	/* 4 */
#ifndef _ALPHA_
/* 850 */	NdrFcShort( 0x20 ),	/* x86, MIPS, PPC Stack size/offset = 32 */
#else
			NdrFcShort( 0x40 ),	/* Alpha Stack size/offset = 64 */
#endif
/* 852 */	NdrFcShort( 0x10 ),	/* 16 */
/* 854 */	NdrFcShort( 0x10 ),	/* 16 */
/* 856 */	0x7,		/* 7 */
			0x7,		/* 7 */

	/* Parameter dwConnection */

/* 858 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 860 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 862 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 864 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 866 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 868 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 870 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 872 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 874 */	NdrFcShort( 0xb4 ),	/* Type Offset=180 */

	/* Parameter pItemValues */

/* 876 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 878 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 880 */	NdrFcShort( 0x476 ),	/* Type Offset=1142 */

	/* Parameter pTransactionID */

/* 882 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 884 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 886 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 888 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 890 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 892 */	NdrFcShort( 0x410 ),	/* Type Offset=1040 */

	/* Return value */

/* 894 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 896 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 898 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Refresh */

/* 900 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 902 */	NdrFcShort( 0x5 ),	/* 5 */
#ifndef _ALPHA_
/* 904 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 906 */	NdrFcShort( 0xe ),	/* 14 */
/* 908 */	NdrFcShort( 0x10 ),	/* 16 */
/* 910 */	0x4,		/* 4 */
			0x4,		/* 4 */

	/* Parameter dwConnection */

/* 912 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 914 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 916 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwSource */

/* 918 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 920 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 922 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter pTransactionID */

/* 924 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 926 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 928 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 930 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 932 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 934 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Cancel2 */


	/* Procedure Cancel */

/* 936 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 938 */	NdrFcShort( 0x6 ),	/* 6 */
#ifndef _ALPHA_
/* 940 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 942 */	NdrFcShort( 0x8 ),	/* 8 */
/* 944 */	NdrFcShort( 0x8 ),	/* 8 */
/* 946 */	0x4,		/* 4 */
			0x2,		/* 2 */

	/* Parameter dwCancelID */


	/* Parameter dwTransactionID */

/* 948 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 950 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 952 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */


	/* Return value */

/* 954 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 956 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 958 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure AddItems */

/* 960 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 962 */	NdrFcShort( 0x3 ),	/* 3 */
#ifndef _ALPHA_
/* 964 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 966 */	NdrFcShort( 0x8 ),	/* 8 */
/* 968 */	NdrFcShort( 0x8 ),	/* 8 */
/* 970 */	0x7,		/* 7 */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 972 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 974 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 976 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pItemArray */

/* 978 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 980 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 982 */	NdrFcShort( 0x4c4 ),	/* Type Offset=1220 */

	/* Parameter ppAddResults */

/* 984 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 986 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 988 */	NdrFcShort( 0x4f4 ),	/* Type Offset=1268 */

	/* Parameter ppErrors */

/* 990 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 992 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 994 */	NdrFcShort( 0x44a ),	/* Type Offset=1098 */

	/* Return value */

/* 996 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 998 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1000 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure ValidateItems */

/* 1002 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1004 */	NdrFcShort( 0x4 ),	/* 4 */
#ifndef _ALPHA_
/* 1006 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 1008 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1010 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1012 */	0x7,		/* 7 */
			0x6,		/* 6 */

	/* Parameter dwCount */

/* 1014 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1016 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1018 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pItemArray */

/* 1020 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1022 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1024 */	NdrFcShort( 0x4c4 ),	/* Type Offset=1220 */

	/* Parameter bBlobUpdate */

/* 1026 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1028 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1030 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppValidationResults */

/* 1032 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1034 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1036 */	NdrFcShort( 0x4f4 ),	/* Type Offset=1268 */

	/* Parameter ppErrors */

/* 1038 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1040 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1042 */	NdrFcShort( 0x44a ),	/* Type Offset=1098 */

	/* Return value */

/* 1044 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1046 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 1048 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure RemoveItems */

/* 1050 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1052 */	NdrFcShort( 0x5 ),	/* 5 */
#ifndef _ALPHA_
/* 1054 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1056 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1058 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1060 */	0x7,		/* 7 */
			0x4,		/* 4 */

	/* Parameter dwCount */

/* 1062 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1064 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1066 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1068 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1070 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1072 */	NdrFcShort( 0x41c ),	/* Type Offset=1052 */

	/* Parameter ppErrors */

/* 1074 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1076 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1078 */	NdrFcShort( 0x44a ),	/* Type Offset=1098 */

	/* Return value */

/* 1080 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1082 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1084 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetActiveState */

/* 1086 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1088 */	NdrFcShort( 0x6 ),	/* 6 */
#ifndef _ALPHA_
/* 1090 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 1092 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1094 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1096 */	0x7,		/* 7 */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 1098 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1100 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1102 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1104 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1106 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1108 */	NdrFcShort( 0x41c ),	/* Type Offset=1052 */

	/* Parameter bActive */

/* 1110 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1112 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1114 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1116 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1118 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1120 */	NdrFcShort( 0x44a ),	/* Type Offset=1098 */

	/* Return value */

/* 1122 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1124 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1126 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetClientHandles */

/* 1128 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1130 */	NdrFcShort( 0x7 ),	/* 7 */
#ifndef _ALPHA_
/* 1132 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 1134 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1136 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1138 */	0x7,		/* 7 */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 1140 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1142 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1144 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1146 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1148 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1150 */	NdrFcShort( 0x41c ),	/* Type Offset=1052 */

	/* Parameter phClient */

/* 1152 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1154 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1156 */	NdrFcShort( 0x41c ),	/* Type Offset=1052 */

	/* Parameter ppErrors */

/* 1158 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1160 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1162 */	NdrFcShort( 0x44a ),	/* Type Offset=1098 */

	/* Return value */

/* 1164 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1166 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1168 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetDatatypes */

/* 1170 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1172 */	NdrFcShort( 0x8 ),	/* 8 */
#ifndef _ALPHA_
/* 1174 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 1176 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1178 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1180 */	0x7,		/* 7 */
			0x5,		/* 5 */

	/* Parameter dwCount */

/* 1182 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1184 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1186 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1188 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1190 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1192 */	NdrFcShort( 0x41c ),	/* Type Offset=1052 */

	/* Parameter pRequestedDatatypes */

/* 1194 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1196 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1198 */	NdrFcShort( 0x544 ),	/* Type Offset=1348 */

	/* Parameter ppErrors */

/* 1200 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1202 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1204 */	NdrFcShort( 0x44a ),	/* Type Offset=1098 */

	/* Return value */

/* 1206 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1208 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1210 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure CreateEnumerator */

/* 1212 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1214 */	NdrFcShort( 0x9 ),	/* 9 */
#ifndef _ALPHA_
/* 1216 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1218 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1220 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1222 */	0x5,		/* 5 */
			0x3,		/* 3 */

	/* Parameter riid */

/* 1224 */	NdrFcShort( 0x10a ),	/* 266 */
#ifndef _ALPHA_
/* 1226 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1228 */	NdrFcShort( 0x1a ),	/* Type Offset=26 */

	/* Parameter ppUnk */

/* 1230 */	NdrFcShort( 0x13 ),	/* 19 */
#ifndef _ALPHA_
/* 1232 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1234 */	NdrFcShort( 0x54e ),	/* Type Offset=1358 */

	/* Return value */

/* 1236 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1238 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1240 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Next */

/* 1242 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1244 */	NdrFcShort( 0x3 ),	/* 3 */
#ifndef _ALPHA_
/* 1246 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1248 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1250 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1252 */	0x5,		/* 5 */
			0x4,		/* 4 */

	/* Parameter celt */

/* 1254 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1256 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1258 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppItemArray */

/* 1260 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1262 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1264 */	NdrFcShort( 0x558 ),	/* Type Offset=1368 */

	/* Parameter pceltFetched */

/* 1266 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 1268 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1270 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1272 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1274 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1276 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Skip */

/* 1278 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1280 */	NdrFcShort( 0x4 ),	/* 4 */
#ifndef _ALPHA_
/* 1282 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1284 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1286 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1288 */	0x4,		/* 4 */
			0x2,		/* 2 */

	/* Parameter celt */

/* 1290 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1292 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1294 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1296 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1298 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1300 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Reset */

/* 1302 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1304 */	NdrFcShort( 0x5 ),	/* 5 */
#ifndef _ALPHA_
/* 1306 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1308 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1310 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1312 */	0x4,		/* 4 */
			0x1,		/* 1 */

	/* Return value */

/* 1314 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1316 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1318 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Clone */

/* 1320 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1322 */	NdrFcShort( 0x6 ),	/* 6 */
#ifndef _ALPHA_
/* 1324 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1326 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1328 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1330 */	0x5,		/* 5 */
			0x2,		/* 2 */

	/* Parameter ppEnumItemAttributes */

/* 1332 */	NdrFcShort( 0x13 ),	/* 19 */
#ifndef _ALPHA_
/* 1334 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1336 */	NdrFcShort( 0x5ac ),	/* Type Offset=1452 */

	/* Return value */

/* 1338 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1340 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1342 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnDataChange */

/* 1344 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1346 */	NdrFcShort( 0x3 ),	/* 3 */
#ifndef _ALPHA_
/* 1348 */	NdrFcShort( 0x30 ),	/* x86, MIPS, PPC Stack size/offset = 48 */
#else
			NdrFcShort( 0x60 ),	/* Alpha Stack size/offset = 96 */
#endif
/* 1350 */	NdrFcShort( 0x28 ),	/* 40 */
/* 1352 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1354 */	0x6,		/* 6 */
			0xb,		/* 11 */

	/* Parameter dwTransid */

/* 1356 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1358 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1360 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hGroup */

/* 1362 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1364 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1366 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrMasterquality */

/* 1368 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1370 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1372 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrMastererror */

/* 1374 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1376 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1378 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 1380 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1382 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1384 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phClientItems */

/* 1386 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1388 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 1390 */	NdrFcShort( 0x5c6 ),	/* Type Offset=1478 */

	/* Parameter pvValues */

/* 1392 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1394 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 1396 */	NdrFcShort( 0x5de ),	/* Type Offset=1502 */

	/* Parameter pwQualities */

/* 1398 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1400 */	NdrFcShort( 0x20 ),	/* x86, MIPS, PPC Stack size/offset = 32 */
#else
			NdrFcShort( 0x40 ),	/* Alpha Stack size/offset = 64 */
#endif
/* 1402 */	NdrFcShort( 0x5f4 ),	/* Type Offset=1524 */

	/* Parameter pftTimeStamps */

/* 1404 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1406 */	NdrFcShort( 0x24 ),	/* x86, MIPS, PPC Stack size/offset = 36 */
#else
			NdrFcShort( 0x48 ),	/* Alpha Stack size/offset = 72 */
#endif
/* 1408 */	NdrFcShort( 0x602 ),	/* Type Offset=1538 */

	/* Parameter pErrors */

/* 1410 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1412 */	NdrFcShort( 0x28 ),	/* x86, MIPS, PPC Stack size/offset = 40 */
#else
			NdrFcShort( 0x50 ),	/* Alpha Stack size/offset = 80 */
#endif
/* 1414 */	NdrFcShort( 0x5c6 ),	/* Type Offset=1478 */

	/* Return value */

/* 1416 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1418 */	NdrFcShort( 0x2c ),	/* x86, MIPS, PPC Stack size/offset = 44 */
#else
			NdrFcShort( 0x58 ),	/* Alpha Stack size/offset = 88 */
#endif
/* 1420 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnReadComplete */

/* 1422 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1424 */	NdrFcShort( 0x4 ),	/* 4 */
#ifndef _ALPHA_
/* 1426 */	NdrFcShort( 0x30 ),	/* x86, MIPS, PPC Stack size/offset = 48 */
#else
			NdrFcShort( 0x60 ),	/* Alpha Stack size/offset = 96 */
#endif
/* 1428 */	NdrFcShort( 0x28 ),	/* 40 */
/* 1430 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1432 */	0x6,		/* 6 */
			0xb,		/* 11 */

	/* Parameter dwTransid */

/* 1434 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1436 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1438 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hGroup */

/* 1440 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1442 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1444 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrMasterquality */

/* 1446 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1448 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1450 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrMastererror */

/* 1452 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1454 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1456 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 1458 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1460 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1462 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phClientItems */

/* 1464 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1466 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 1468 */	NdrFcShort( 0x5c6 ),	/* Type Offset=1478 */

	/* Parameter pvValues */

/* 1470 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1472 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 1474 */	NdrFcShort( 0x61e ),	/* Type Offset=1566 */

	/* Parameter pwQualities */

/* 1476 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1478 */	NdrFcShort( 0x20 ),	/* x86, MIPS, PPC Stack size/offset = 32 */
#else
			NdrFcShort( 0x40 ),	/* Alpha Stack size/offset = 64 */
#endif
/* 1480 */	NdrFcShort( 0x5f4 ),	/* Type Offset=1524 */

	/* Parameter pftTimeStamps */

/* 1482 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1484 */	NdrFcShort( 0x24 ),	/* x86, MIPS, PPC Stack size/offset = 36 */
#else
			NdrFcShort( 0x48 ),	/* Alpha Stack size/offset = 72 */
#endif
/* 1486 */	NdrFcShort( 0x602 ),	/* Type Offset=1538 */

	/* Parameter pErrors */

/* 1488 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1490 */	NdrFcShort( 0x28 ),	/* x86, MIPS, PPC Stack size/offset = 40 */
#else
			NdrFcShort( 0x50 ),	/* Alpha Stack size/offset = 80 */
#endif
/* 1492 */	NdrFcShort( 0x5c6 ),	/* Type Offset=1478 */

	/* Return value */

/* 1494 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1496 */	NdrFcShort( 0x2c ),	/* x86, MIPS, PPC Stack size/offset = 44 */
#else
			NdrFcShort( 0x58 ),	/* Alpha Stack size/offset = 88 */
#endif
/* 1498 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnWriteComplete */

/* 1500 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1502 */	NdrFcShort( 0x5 ),	/* 5 */
#ifndef _ALPHA_
/* 1504 */	NdrFcShort( 0x20 ),	/* x86, MIPS, PPC Stack size/offset = 32 */
#else
			NdrFcShort( 0x40 ),	/* Alpha Stack size/offset = 64 */
#endif
/* 1506 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1508 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1510 */	0x6,		/* 6 */
			0x7,		/* 7 */

	/* Parameter dwTransid */

/* 1512 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1514 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1516 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hGroup */

/* 1518 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1520 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1522 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hrMastererr */

/* 1524 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1526 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1528 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter dwCount */

/* 1530 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1532 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1534 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pClienthandles */

/* 1536 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1538 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1540 */	NdrFcShort( 0x634 ),	/* Type Offset=1588 */

	/* Parameter pErrors */

/* 1542 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1544 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 1546 */	NdrFcShort( 0x634 ),	/* Type Offset=1588 */

	/* Return value */

/* 1548 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1550 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 1552 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure OnCancelComplete */

/* 1554 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1556 */	NdrFcShort( 0x6 ),	/* 6 */
#ifndef _ALPHA_
/* 1558 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1560 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1562 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1564 */	0x4,		/* 4 */
			0x3,		/* 3 */

	/* Parameter dwTransid */

/* 1566 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1568 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1570 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter hGroup */

/* 1572 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1574 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1576 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1578 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1580 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1582 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Read */

/* 1584 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1586 */	NdrFcShort( 0x3 ),	/* 3 */
#ifndef _ALPHA_
/* 1588 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 1590 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1592 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1594 */	0x7,		/* 7 */
			0x6,		/* 6 */

	/* Parameter dwCount */

/* 1596 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1598 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1600 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1602 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1604 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1606 */	NdrFcShort( 0x41c ),	/* Type Offset=1052 */

	/* Parameter dwTransactionID */

/* 1608 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1610 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1612 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCancelID */

/* 1614 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 1616 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1618 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1620 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1622 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1624 */	NdrFcShort( 0x44a ),	/* Type Offset=1098 */

	/* Return value */

/* 1626 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1628 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 1630 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Write */

/* 1632 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1634 */	NdrFcShort( 0x4 ),	/* 4 */
#ifndef _ALPHA_
/* 1636 */	NdrFcShort( 0x20 ),	/* x86, MIPS, PPC Stack size/offset = 32 */
#else
			NdrFcShort( 0x40 ),	/* Alpha Stack size/offset = 64 */
#endif
/* 1638 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1640 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1642 */	0x7,		/* 7 */
			0x7,		/* 7 */

	/* Parameter dwCount */

/* 1644 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1646 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1648 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter phServer */

/* 1650 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1652 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1654 */	NdrFcShort( 0x41c ),	/* Type Offset=1052 */

	/* Parameter pItemValues */

/* 1656 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1658 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1660 */	NdrFcShort( 0x64c ),	/* Type Offset=1612 */

	/* Parameter dwTransactionID */

/* 1662 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1664 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1666 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCancelID */

/* 1668 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 1670 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1672 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppErrors */

/* 1674 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1676 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 1678 */	NdrFcShort( 0x44a ),	/* Type Offset=1098 */

	/* Return value */

/* 1680 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1682 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 1684 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure Refresh2 */

/* 1686 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1688 */	NdrFcShort( 0x5 ),	/* 5 */
#ifndef _ALPHA_
/* 1690 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1692 */	NdrFcShort( 0xe ),	/* 14 */
/* 1694 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1696 */	0x4,		/* 4 */
			0x4,		/* 4 */

	/* Parameter dwSource */

/* 1698 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1700 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1702 */	0xd,		/* FC_ENUM16 */
			0x0,		/* 0 */

	/* Parameter dwTransactionID */

/* 1704 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1706 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1708 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwCancelID */

/* 1710 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 1712 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1714 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1716 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1718 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1720 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure SetEnable */

/* 1722 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1724 */	NdrFcShort( 0x7 ),	/* 7 */
#ifndef _ALPHA_
/* 1726 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1728 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1730 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1732 */	0x4,		/* 4 */
			0x2,		/* 2 */

	/* Parameter bEnable */

/* 1734 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1736 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1738 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1740 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1742 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1744 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetEnable */

/* 1746 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1748 */	NdrFcShort( 0x8 ),	/* 8 */
#ifndef _ALPHA_
/* 1750 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1752 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1754 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1756 */	0x4,		/* 4 */
			0x2,		/* 2 */

	/* Parameter pbEnable */

/* 1758 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 1760 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1762 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Return value */

/* 1764 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1766 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1768 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure QueryAvailableProperties */

/* 1770 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1772 */	NdrFcShort( 0x3 ),	/* 3 */
#ifndef _ALPHA_
/* 1774 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 1776 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1778 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1780 */	0x7,		/* 7 */
			0x6,		/* 6 */

	/* Parameter szItemID */

/* 1782 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1784 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1786 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Parameter pdwCount */

/* 1788 */	NdrFcShort( 0x2150 ),	/* 8528 */
#ifndef _ALPHA_
/* 1790 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1792 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter ppPropertyIDs */

/* 1794 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1796 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1798 */	NdrFcShort( 0x65e ),	/* Type Offset=1630 */

	/* Parameter ppDescriptions */

/* 1800 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1802 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1804 */	NdrFcShort( 0x670 ),	/* Type Offset=1648 */

	/* Parameter ppvtDataTypes */

/* 1806 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1808 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1810 */	NdrFcShort( 0x696 ),	/* Type Offset=1686 */

	/* Return value */

/* 1812 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1814 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 1816 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure GetItemProperties */

/* 1818 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1820 */	NdrFcShort( 0x4 ),	/* 4 */
#ifndef _ALPHA_
/* 1822 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 1824 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1826 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1828 */	0x7,		/* 7 */
			0x6,		/* 6 */

	/* Parameter szItemID */

/* 1830 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1832 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1834 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Parameter dwCount */

/* 1836 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1838 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1840 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwPropertyIDs */

/* 1842 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1844 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1846 */	NdrFcShort( 0xb4 ),	/* Type Offset=180 */

	/* Parameter ppvData */

/* 1848 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1850 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1852 */	NdrFcShort( 0x6a8 ),	/* Type Offset=1704 */

	/* Parameter ppErrors */

/* 1854 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1856 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1858 */	NdrFcShort( 0x410 ),	/* Type Offset=1040 */

	/* Return value */

/* 1860 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1862 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 1864 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Procedure LookupItemIDs */

/* 1866 */	0x33,		/* FC_AUTO_HANDLE */
			0x64,		/* 100 */
/* 1868 */	NdrFcShort( 0x5 ),	/* 5 */
#ifndef _ALPHA_
/* 1870 */	NdrFcShort( 0x1c ),	/* x86, MIPS, PPC Stack size/offset = 28 */
#else
			NdrFcShort( 0x38 ),	/* Alpha Stack size/offset = 56 */
#endif
/* 1872 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1874 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1876 */	0x7,		/* 7 */
			0x6,		/* 6 */

	/* Parameter szItemID */

/* 1878 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1880 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1882 */	NdrFcShort( 0x2 ),	/* Type Offset=2 */

	/* Parameter dwCount */

/* 1884 */	NdrFcShort( 0x48 ),	/* 72 */
#ifndef _ALPHA_
/* 1886 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1888 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

	/* Parameter pdwPropertyIDs */

/* 1890 */	NdrFcShort( 0x10b ),	/* 267 */
#ifndef _ALPHA_
/* 1892 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1894 */	NdrFcShort( 0xb4 ),	/* Type Offset=180 */

	/* Parameter ppszNewItemIDs */

/* 1896 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1898 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1900 */	NdrFcShort( 0x6cc ),	/* Type Offset=1740 */

	/* Parameter ppErrors */

/* 1902 */	NdrFcShort( 0x2013 ),	/* 8211 */
#ifndef _ALPHA_
/* 1904 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1906 */	NdrFcShort( 0x410 ),	/* Type Offset=1040 */

	/* Return value */

/* 1908 */	NdrFcShort( 0x70 ),	/* 112 */
#ifndef _ALPHA_
/* 1910 */	NdrFcShort( 0x18 ),	/* x86, MIPS, PPC Stack size/offset = 24 */
#else
			NdrFcShort( 0x30 ),	/* Alpha Stack size/offset = 48 */
#endif
/* 1912 */	0x8,		/* FC_LONG */
			0x0,		/* 0 */

			0x0
        }
    };

static const MIDL_TYPE_FORMAT_STRING __MIDL_TypeFormatString =
    {
        0,
        {
			0x11, 0x8,	/* FC_RP [simple_pointer] */
/*  2 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/*  4 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/*  6 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/*  8 */	
			0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 10 */	0xa,		/* FC_FLOAT */
			0x5c,		/* FC_PAD */
/* 12 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 14 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 16 */	
			0x11, 0x0,	/* FC_RP */
/* 18 */	NdrFcShort( 0x8 ),	/* Offset= 8 (26) */
/* 20 */	
			0x1d,		/* FC_SMFARRAY */
			0x0,		/* 0 */
/* 22 */	NdrFcShort( 0x8 ),	/* 8 */
/* 24 */	0x2,		/* FC_CHAR */
			0x5b,		/* FC_END */
/* 26 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 28 */	NdrFcShort( 0x10 ),	/* 16 */
/* 30 */	0x8,		/* FC_LONG */
			0x6,		/* FC_SHORT */
/* 32 */	0x6,		/* FC_SHORT */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 34 */	0x0,		/* 0 */
			NdrFcShort( 0xfffffff1 ),	/* Offset= -15 (20) */
			0x5b,		/* FC_END */
/* 38 */	
			0x11, 0x10,	/* FC_RP */
/* 40 */	NdrFcShort( 0x2 ),	/* Offset= 2 (42) */
/* 42 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 44 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 46 */	NdrFcShort( 0x28 ),	/* x86, MIPS, PPC Stack size/offset = 40 */
#else
			NdrFcShort( 0x50 ),	/* Alpha Stack size/offset = 80 */
#endif
/* 48 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] */
/* 50 */	NdrFcShort( 0x2 ),	/* Offset= 2 (52) */
/* 52 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 54 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 56 */	
			0x11, 0x10,	/* FC_RP */
/* 58 */	NdrFcShort( 0x2 ),	/* Offset= 2 (60) */
/* 60 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 62 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 64 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 66 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] */
/* 68 */	NdrFcShort( 0x2 ),	/* Offset= 2 (70) */
/* 70 */	
			0x13, 0x0,	/* FC_OP */
/* 72 */	NdrFcShort( 0xa ),	/* Offset= 10 (82) */
/* 74 */	
			0x15,		/* FC_STRUCT */
			0x3,		/* 3 */
/* 76 */	NdrFcShort( 0x8 ),	/* 8 */
/* 78 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 80 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 82 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 84 */	NdrFcShort( 0x30 ),	/* 48 */
/* 86 */	NdrFcShort( 0x0 ),	/* 0 */
/* 88 */	NdrFcShort( 0x18 ),	/* Offset= 24 (112) */
/* 90 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 92 */	NdrFcShort( 0xffffffee ),	/* Offset= -18 (74) */
/* 94 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 96 */	NdrFcShort( 0xffffffea ),	/* Offset= -22 (74) */
/* 98 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 100 */	NdrFcShort( 0xffffffe6 ),	/* Offset= -26 (74) */
/* 102 */	0xd,		/* FC_ENUM16 */
			0x8,		/* FC_LONG */
/* 104 */	0x8,		/* FC_LONG */
			0x6,		/* FC_SHORT */
/* 106 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 108 */	0x6,		/* FC_SHORT */
			0x38,		/* FC_ALIGNM4 */
/* 110 */	0x36,		/* FC_POINTER */
			0x5b,		/* FC_END */
/* 112 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 114 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 116 */	
			0x11, 0x10,	/* FC_RP */
/* 118 */	NdrFcShort( 0x2 ),	/* Offset= 2 (120) */
/* 120 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 122 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 124 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 126 */	
			0x11, 0x10,	/* FC_RP */
/* 128 */	NdrFcShort( 0x2 ),	/* Offset= 2 (130) */
/* 130 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 132 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 134 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 136 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 138 */	0xd,		/* FC_ENUM16 */
			0x5c,		/* FC_PAD */
/* 140 */	
			0x11, 0x10,	/* FC_RP */
/* 142 */	NdrFcShort( 0x2 ),	/* Offset= 2 (144) */
/* 144 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 146 */	NdrFcLong( 0x101 ),	/* 257 */
/* 150 */	NdrFcShort( 0x0 ),	/* 0 */
/* 152 */	NdrFcShort( 0x0 ),	/* 0 */
/* 154 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 156 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 158 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 160 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 162 */	
			0x11, 0xc,	/* FC_RP [alloced_on_stack] [simple_pointer] */
/* 164 */	0xa,		/* FC_FLOAT */
			0x5c,		/* FC_PAD */
/* 166 */	
			0x11, 0x10,	/* FC_RP */
/* 168 */	NdrFcShort( 0x2 ),	/* Offset= 2 (170) */
/* 170 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 172 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 174 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 176 */	
			0x11, 0x0,	/* FC_RP */
/* 178 */	NdrFcShort( 0x2 ),	/* Offset= 2 (180) */
/* 180 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 182 */	NdrFcShort( 0x4 ),	/* 4 */
/* 184 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 186 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 188 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 190 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] */
/* 192 */	NdrFcShort( 0x2 ),	/* Offset= 2 (194) */
/* 194 */	
			0x13, 0x0,	/* FC_OP */
/* 196 */	NdrFcShort( 0x33a ),	/* Offset= 826 (1022) */
/* 198 */	
			0x13, 0x0,	/* FC_OP */
/* 200 */	NdrFcShort( 0x304 ),	/* Offset= 772 (972) */
/* 202 */	
			0x2b,		/* FC_NON_ENCAPSULATED_UNION */
			0x6,		/* FC_SHORT */
/* 204 */	0x6,		/* 6 */
			0x0,		/*  */
/* 206 */	NdrFcShort( 0xfffffff8 ),	/* -8 */
/* 208 */	NdrFcShort( 0x2 ),	/* Offset= 2 (210) */
/* 210 */	NdrFcShort( 0x10 ),	/* 16 */
/* 212 */	NdrFcShort( 0x29 ),	/* 41 */
/* 214 */	NdrFcLong( 0x3 ),	/* 3 */
/* 218 */	NdrFcShort( 0xffff8008 ),	/* Offset= -32760 (-32542) */
/* 220 */	NdrFcLong( 0x11 ),	/* 17 */
/* 224 */	NdrFcShort( 0xffff8002 ),	/* Offset= -32766 (-32542) */
/* 226 */	NdrFcLong( 0x2 ),	/* 2 */
/* 230 */	NdrFcShort( 0xffff8006 ),	/* Offset= -32762 (-32532) */
/* 232 */	NdrFcLong( 0x4 ),	/* 4 */
/* 236 */	NdrFcShort( 0xffff800a ),	/* Offset= -32758 (-32522) */
/* 238 */	NdrFcLong( 0x5 ),	/* 5 */
/* 242 */	NdrFcShort( 0xffff800c ),	/* Offset= -32756 (-32514) */
/* 244 */	NdrFcLong( 0xb ),	/* 11 */
/* 248 */	NdrFcShort( 0xffff8006 ),	/* Offset= -32762 (-32514) */
/* 250 */	NdrFcLong( 0xa ),	/* 10 */
/* 254 */	NdrFcShort( 0xffff8008 ),	/* Offset= -32760 (-32506) */
/* 256 */	NdrFcLong( 0x6 ),	/* 6 */
/* 260 */	NdrFcShort( 0xca ),	/* Offset= 202 (462) */
/* 262 */	NdrFcLong( 0x7 ),	/* 7 */
/* 266 */	NdrFcShort( 0xffff800c ),	/* Offset= -32756 (-32490) */
/* 268 */	NdrFcLong( 0x8 ),	/* 8 */
/* 272 */	NdrFcShort( 0xc4 ),	/* Offset= 196 (468) */
/* 274 */	NdrFcLong( 0xd ),	/* 13 */
/* 278 */	NdrFcShort( 0xd6 ),	/* Offset= 214 (492) */
/* 280 */	NdrFcLong( 0x9 ),	/* 9 */
/* 284 */	NdrFcShort( 0xe2 ),	/* Offset= 226 (510) */
/* 286 */	NdrFcLong( 0x2000 ),	/* 8192 */
/* 290 */	NdrFcShort( 0xee ),	/* Offset= 238 (528) */
/* 292 */	NdrFcLong( 0x4011 ),	/* 16401 */
/* 296 */	NdrFcShort( 0x264 ),	/* Offset= 612 (908) */
/* 298 */	NdrFcLong( 0x4002 ),	/* 16386 */
/* 302 */	NdrFcShort( 0x262 ),	/* Offset= 610 (912) */
/* 304 */	NdrFcLong( 0x4003 ),	/* 16387 */
/* 308 */	NdrFcShort( 0x260 ),	/* Offset= 608 (916) */
/* 310 */	NdrFcLong( 0x4004 ),	/* 16388 */
/* 314 */	NdrFcShort( 0x25e ),	/* Offset= 606 (920) */
/* 316 */	NdrFcLong( 0x4005 ),	/* 16389 */
/* 320 */	NdrFcShort( 0x25c ),	/* Offset= 604 (924) */
/* 322 */	NdrFcLong( 0x400b ),	/* 16395 */
/* 326 */	NdrFcShort( 0x24a ),	/* Offset= 586 (912) */
/* 328 */	NdrFcLong( 0x400a ),	/* 16394 */
/* 332 */	NdrFcShort( 0x248 ),	/* Offset= 584 (916) */
/* 334 */	NdrFcLong( 0x4006 ),	/* 16390 */
/* 338 */	NdrFcShort( 0x24e ),	/* Offset= 590 (928) */
/* 340 */	NdrFcLong( 0x4007 ),	/* 16391 */
/* 344 */	NdrFcShort( 0x244 ),	/* Offset= 580 (924) */
/* 346 */	NdrFcLong( 0x4008 ),	/* 16392 */
/* 350 */	NdrFcShort( 0x246 ),	/* Offset= 582 (932) */
/* 352 */	NdrFcLong( 0x400d ),	/* 16397 */
/* 356 */	NdrFcShort( 0x244 ),	/* Offset= 580 (936) */
/* 358 */	NdrFcLong( 0x4009 ),	/* 16393 */
/* 362 */	NdrFcShort( 0x242 ),	/* Offset= 578 (940) */
/* 364 */	NdrFcLong( 0x6000 ),	/* 24576 */
/* 368 */	NdrFcShort( 0x240 ),	/* Offset= 576 (944) */
/* 370 */	NdrFcLong( 0x400c ),	/* 16396 */
/* 374 */	NdrFcShort( 0x23e ),	/* Offset= 574 (948) */
/* 376 */	NdrFcLong( 0x10 ),	/* 16 */
/* 380 */	NdrFcShort( 0xffff8002 ),	/* Offset= -32766 (-32386) */
/* 382 */	NdrFcLong( 0x12 ),	/* 18 */
/* 386 */	NdrFcShort( 0xffff8006 ),	/* Offset= -32762 (-32376) */
/* 388 */	NdrFcLong( 0x13 ),	/* 19 */
/* 392 */	NdrFcShort( 0xffff8008 ),	/* Offset= -32760 (-32368) */
/* 394 */	NdrFcLong( 0x16 ),	/* 22 */
/* 398 */	NdrFcShort( 0xffff8008 ),	/* Offset= -32760 (-32362) */
/* 400 */	NdrFcLong( 0x17 ),	/* 23 */
/* 404 */	NdrFcShort( 0xffff8008 ),	/* Offset= -32760 (-32356) */
/* 406 */	NdrFcLong( 0xe ),	/* 14 */
/* 410 */	NdrFcShort( 0x222 ),	/* Offset= 546 (956) */
/* 412 */	NdrFcLong( 0x400e ),	/* 16398 */
/* 416 */	NdrFcShort( 0x228 ),	/* Offset= 552 (968) */
/* 418 */	NdrFcLong( 0x4010 ),	/* 16400 */
/* 422 */	NdrFcShort( 0x1e6 ),	/* Offset= 486 (908) */
/* 424 */	NdrFcLong( 0x4012 ),	/* 16402 */
/* 428 */	NdrFcShort( 0x1e4 ),	/* Offset= 484 (912) */
/* 430 */	NdrFcLong( 0x4013 ),	/* 16403 */
/* 434 */	NdrFcShort( 0x1e2 ),	/* Offset= 482 (916) */
/* 436 */	NdrFcLong( 0x4016 ),	/* 16406 */
/* 440 */	NdrFcShort( 0x1dc ),	/* Offset= 476 (916) */
/* 442 */	NdrFcLong( 0x4017 ),	/* 16407 */
/* 446 */	NdrFcShort( 0x1d6 ),	/* Offset= 470 (916) */
/* 448 */	NdrFcLong( 0x0 ),	/* 0 */
/* 452 */	NdrFcShort( 0x0 ),	/* Offset= 0 (452) */
/* 454 */	NdrFcLong( 0x1 ),	/* 1 */
/* 458 */	NdrFcShort( 0x0 ),	/* Offset= 0 (458) */
/* 460 */	NdrFcShort( 0xffffffff ),	/* Offset= -1 (459) */
/* 462 */	
			0x15,		/* FC_STRUCT */
			0x7,		/* 7 */
/* 464 */	NdrFcShort( 0x8 ),	/* 8 */
/* 466 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 468 */	
			0x13, 0x0,	/* FC_OP */
/* 470 */	NdrFcShort( 0xc ),	/* Offset= 12 (482) */
/* 472 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 474 */	NdrFcShort( 0x2 ),	/* 2 */
/* 476 */	0x9,		/* 9 */
			0x0,		/*  */
/* 478 */	NdrFcShort( 0xfffffffc ),	/* -4 */
/* 480 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 482 */	
			0x17,		/* FC_CSTRUCT */
			0x3,		/* 3 */
/* 484 */	NdrFcShort( 0x8 ),	/* 8 */
/* 486 */	NdrFcShort( 0xfffffff2 ),	/* Offset= -14 (472) */
/* 488 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 490 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 492 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 494 */	NdrFcLong( 0x0 ),	/* 0 */
/* 498 */	NdrFcShort( 0x0 ),	/* 0 */
/* 500 */	NdrFcShort( 0x0 ),	/* 0 */
/* 502 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 504 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 506 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 508 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 510 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 512 */	NdrFcLong( 0x20400 ),	/* 132096 */
/* 516 */	NdrFcShort( 0x0 ),	/* 0 */
/* 518 */	NdrFcShort( 0x0 ),	/* 0 */
/* 520 */	0xc0,		/* 192 */
			0x0,		/* 0 */
/* 522 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 524 */	0x0,		/* 0 */
			0x0,		/* 0 */
/* 526 */	0x0,		/* 0 */
			0x46,		/* 70 */
/* 528 */	
			0x13, 0x0,	/* FC_OP */
/* 530 */	NdrFcShort( 0x168 ),	/* Offset= 360 (890) */
/* 532 */	
			0x2a,		/* FC_ENCAPSULATED_UNION */
			0x48,		/* 72 */
/* 534 */	NdrFcShort( 0x8 ),	/* 8 */
/* 536 */	NdrFcShort( 0x8 ),	/* 8 */
/* 538 */	NdrFcLong( 0x8 ),	/* 8 */
/* 542 */	NdrFcShort( 0x4c ),	/* Offset= 76 (618) */
/* 544 */	NdrFcLong( 0xd ),	/* 13 */
/* 548 */	NdrFcShort( 0x6c ),	/* Offset= 108 (656) */
/* 550 */	NdrFcLong( 0x9 ),	/* 9 */
/* 554 */	NdrFcShort( 0x88 ),	/* Offset= 136 (690) */
/* 556 */	NdrFcLong( 0xc ),	/* 12 */
/* 560 */	NdrFcShort( 0xb0 ),	/* Offset= 176 (736) */
/* 562 */	NdrFcLong( 0x10 ),	/* 16 */
/* 566 */	NdrFcShort( 0xc8 ),	/* Offset= 200 (766) */
/* 568 */	NdrFcLong( 0x2 ),	/* 2 */
/* 572 */	NdrFcShort( 0xe0 ),	/* Offset= 224 (796) */
/* 574 */	NdrFcLong( 0x3 ),	/* 3 */
/* 578 */	NdrFcShort( 0xf8 ),	/* Offset= 248 (826) */
/* 580 */	NdrFcLong( 0x14 ),	/* 20 */
/* 584 */	NdrFcShort( 0x110 ),	/* Offset= 272 (856) */
/* 586 */	NdrFcShort( 0x0 ),	/* Offset= 0 (586) */
/* 588 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 590 */	NdrFcShort( 0x4 ),	/* 4 */
/* 592 */	0x18,		/* 24 */
			0x0,		/*  */
/* 594 */	NdrFcShort( 0x0 ),	/* 0 */
/* 596 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 598 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 600 */	NdrFcShort( 0x4 ),	/* 4 */
/* 602 */	NdrFcShort( 0x0 ),	/* 0 */
/* 604 */	NdrFcShort( 0x1 ),	/* 1 */
/* 606 */	NdrFcShort( 0x0 ),	/* 0 */
/* 608 */	NdrFcShort( 0x0 ),	/* 0 */
/* 610 */	0x13, 0x0,	/* FC_OP */
/* 612 */	NdrFcShort( 0xffffff7e ),	/* Offset= -130 (482) */
/* 614 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 616 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 618 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 620 */	NdrFcShort( 0x8 ),	/* 8 */
/* 622 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 624 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 626 */	NdrFcShort( 0x4 ),	/* 4 */
/* 628 */	NdrFcShort( 0x4 ),	/* 4 */
/* 630 */	0x11, 0x0,	/* FC_RP */
/* 632 */	NdrFcShort( 0xffffffd4 ),	/* Offset= -44 (588) */
/* 634 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 636 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 638 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 640 */	NdrFcShort( 0x0 ),	/* 0 */
/* 642 */	0x18,		/* 24 */
			0x0,		/*  */
/* 644 */	NdrFcShort( 0x0 ),	/* 0 */
/* 646 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 650 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 652 */	NdrFcShort( 0xffffff60 ),	/* Offset= -160 (492) */
/* 654 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 656 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 658 */	NdrFcShort( 0x8 ),	/* 8 */
/* 660 */	NdrFcShort( 0x0 ),	/* 0 */
/* 662 */	NdrFcShort( 0x6 ),	/* Offset= 6 (668) */
/* 664 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 666 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 668 */	
			0x11, 0x0,	/* FC_RP */
/* 670 */	NdrFcShort( 0xffffffe0 ),	/* Offset= -32 (638) */
/* 672 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 674 */	NdrFcShort( 0x0 ),	/* 0 */
/* 676 */	0x18,		/* 24 */
			0x0,		/*  */
/* 678 */	NdrFcShort( 0x0 ),	/* 0 */
/* 680 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 684 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 686 */	NdrFcShort( 0xffffff50 ),	/* Offset= -176 (510) */
/* 688 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 690 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 692 */	NdrFcShort( 0x8 ),	/* 8 */
/* 694 */	NdrFcShort( 0x0 ),	/* 0 */
/* 696 */	NdrFcShort( 0x6 ),	/* Offset= 6 (702) */
/* 698 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 700 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 702 */	
			0x11, 0x0,	/* FC_RP */
/* 704 */	NdrFcShort( 0xffffffe0 ),	/* Offset= -32 (672) */
/* 706 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 708 */	NdrFcShort( 0x4 ),	/* 4 */
/* 710 */	0x18,		/* 24 */
			0x0,		/*  */
/* 712 */	NdrFcShort( 0x0 ),	/* 0 */
/* 714 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 716 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 718 */	NdrFcShort( 0x4 ),	/* 4 */
/* 720 */	NdrFcShort( 0x0 ),	/* 0 */
/* 722 */	NdrFcShort( 0x1 ),	/* 1 */
/* 724 */	NdrFcShort( 0x0 ),	/* 0 */
/* 726 */	NdrFcShort( 0x0 ),	/* 0 */
/* 728 */	0x13, 0x0,	/* FC_OP */
/* 730 */	NdrFcShort( 0xf2 ),	/* Offset= 242 (972) */
/* 732 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 734 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 736 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 738 */	NdrFcShort( 0x8 ),	/* 8 */
/* 740 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 742 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 744 */	NdrFcShort( 0x4 ),	/* 4 */
/* 746 */	NdrFcShort( 0x4 ),	/* 4 */
/* 748 */	0x11, 0x0,	/* FC_RP */
/* 750 */	NdrFcShort( 0xffffffd4 ),	/* Offset= -44 (706) */
/* 752 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 754 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 756 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 758 */	NdrFcShort( 0x1 ),	/* 1 */
/* 760 */	0x19,		/* 25 */
			0x0,		/*  */
/* 762 */	NdrFcShort( 0x0 ),	/* 0 */
/* 764 */	0x1,		/* FC_BYTE */
			0x5b,		/* FC_END */
/* 766 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 768 */	NdrFcShort( 0x8 ),	/* 8 */
/* 770 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 772 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 774 */	NdrFcShort( 0x4 ),	/* 4 */
/* 776 */	NdrFcShort( 0x4 ),	/* 4 */
/* 778 */	0x13, 0x0,	/* FC_OP */
/* 780 */	NdrFcShort( 0xffffffe8 ),	/* Offset= -24 (756) */
/* 782 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 784 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 786 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 788 */	NdrFcShort( 0x2 ),	/* 2 */
/* 790 */	0x19,		/* 25 */
			0x0,		/*  */
/* 792 */	NdrFcShort( 0x0 ),	/* 0 */
/* 794 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 796 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 798 */	NdrFcShort( 0x8 ),	/* 8 */
/* 800 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 802 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 804 */	NdrFcShort( 0x4 ),	/* 4 */
/* 806 */	NdrFcShort( 0x4 ),	/* 4 */
/* 808 */	0x13, 0x0,	/* FC_OP */
/* 810 */	NdrFcShort( 0xffffffe8 ),	/* Offset= -24 (786) */
/* 812 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 814 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 816 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 818 */	NdrFcShort( 0x4 ),	/* 4 */
/* 820 */	0x19,		/* 25 */
			0x0,		/*  */
/* 822 */	NdrFcShort( 0x0 ),	/* 0 */
/* 824 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 826 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 828 */	NdrFcShort( 0x8 ),	/* 8 */
/* 830 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 832 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 834 */	NdrFcShort( 0x4 ),	/* 4 */
/* 836 */	NdrFcShort( 0x4 ),	/* 4 */
/* 838 */	0x13, 0x0,	/* FC_OP */
/* 840 */	NdrFcShort( 0xffffffe8 ),	/* Offset= -24 (816) */
/* 842 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 844 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 846 */	
			0x1b,		/* FC_CARRAY */
			0x7,		/* 7 */
/* 848 */	NdrFcShort( 0x8 ),	/* 8 */
/* 850 */	0x19,		/* 25 */
			0x0,		/*  */
/* 852 */	NdrFcShort( 0x0 ),	/* 0 */
/* 854 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 856 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 858 */	NdrFcShort( 0x8 ),	/* 8 */
/* 860 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 862 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 864 */	NdrFcShort( 0x4 ),	/* 4 */
/* 866 */	NdrFcShort( 0x4 ),	/* 4 */
/* 868 */	0x13, 0x0,	/* FC_OP */
/* 870 */	NdrFcShort( 0xffffffe8 ),	/* Offset= -24 (846) */
/* 872 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 874 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 876 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 878 */	NdrFcShort( 0x8 ),	/* 8 */
/* 880 */	0x6,		/* 6 */
			0x0,		/*  */
/* 882 */	NdrFcShort( 0xffffffe8 ),	/* -24 */
/* 884 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 886 */	NdrFcShort( 0xfffffcd4 ),	/* Offset= -812 (74) */
/* 888 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 890 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 892 */	NdrFcShort( 0x18 ),	/* 24 */
/* 894 */	NdrFcShort( 0xffffffee ),	/* Offset= -18 (876) */
/* 896 */	NdrFcShort( 0x0 ),	/* Offset= 0 (896) */
/* 898 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 900 */	0x38,		/* FC_ALIGNM4 */
			0x8,		/* FC_LONG */
/* 902 */	0x8,		/* FC_LONG */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 904 */	0x0,		/* 0 */
			NdrFcShort( 0xfffffe8b ),	/* Offset= -373 (532) */
			0x5b,		/* FC_END */
/* 908 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 910 */	0x2,		/* FC_CHAR */
			0x5c,		/* FC_PAD */
/* 912 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 914 */	0x6,		/* FC_SHORT */
			0x5c,		/* FC_PAD */
/* 916 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 918 */	0x8,		/* FC_LONG */
			0x5c,		/* FC_PAD */
/* 920 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 922 */	0xa,		/* FC_FLOAT */
			0x5c,		/* FC_PAD */
/* 924 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 926 */	0xc,		/* FC_DOUBLE */
			0x5c,		/* FC_PAD */
/* 928 */	
			0x13, 0x0,	/* FC_OP */
/* 930 */	NdrFcShort( 0xfffffe2c ),	/* Offset= -468 (462) */
/* 932 */	
			0x13, 0x10,	/* FC_OP */
/* 934 */	NdrFcShort( 0xfffffe2e ),	/* Offset= -466 (468) */
/* 936 */	
			0x13, 0x10,	/* FC_OP */
/* 938 */	NdrFcShort( 0xfffffe42 ),	/* Offset= -446 (492) */
/* 940 */	
			0x13, 0x10,	/* FC_OP */
/* 942 */	NdrFcShort( 0xfffffe50 ),	/* Offset= -432 (510) */
/* 944 */	
			0x13, 0x10,	/* FC_OP */
/* 946 */	NdrFcShort( 0xfffffe5e ),	/* Offset= -418 (528) */
/* 948 */	
			0x13, 0x10,	/* FC_OP */
/* 950 */	NdrFcShort( 0x2 ),	/* Offset= 2 (952) */
/* 952 */	
			0x13, 0x0,	/* FC_OP */
/* 954 */	NdrFcShort( 0xfffffc46 ),	/* Offset= -954 (0) */
/* 956 */	
			0x15,		/* FC_STRUCT */
			0x7,		/* 7 */
/* 958 */	NdrFcShort( 0x10 ),	/* 16 */
/* 960 */	0x6,		/* FC_SHORT */
			0x2,		/* FC_CHAR */
/* 962 */	0x2,		/* FC_CHAR */
			0x38,		/* FC_ALIGNM4 */
/* 964 */	0x8,		/* FC_LONG */
			0x39,		/* FC_ALIGNM8 */
/* 966 */	0xb,		/* FC_HYPER */
			0x5b,		/* FC_END */
/* 968 */	
			0x13, 0x0,	/* FC_OP */
/* 970 */	NdrFcShort( 0xfffffff2 ),	/* Offset= -14 (956) */
/* 972 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x7,		/* 7 */
/* 974 */	NdrFcShort( 0x20 ),	/* 32 */
/* 976 */	NdrFcShort( 0x0 ),	/* 0 */
/* 978 */	NdrFcShort( 0x0 ),	/* Offset= 0 (978) */
/* 980 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 982 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 984 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 986 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 988 */	NdrFcShort( 0xfffffcee ),	/* Offset= -786 (202) */
/* 990 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 992 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 994 */	NdrFcShort( 0x0 ),	/* 0 */
/* 996 */	NdrFcShort( 0x10 ),	/* 16 */
/* 998 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1000 */	NdrFcShort( 0xfffffcde ),	/* Offset= -802 (198) */
/* 1002 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1004 */	NdrFcShort( 0x20 ),	/* 32 */
/* 1006 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1008 */	NdrFcShort( 0x0 ),	/* Offset= 0 (1008) */
/* 1010 */	0x8,		/* FC_LONG */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1012 */	0x0,		/* 0 */
			NdrFcShort( 0xfffffc55 ),	/* Offset= -939 (74) */
			0x6,		/* FC_SHORT */
/* 1016 */	0x6,		/* FC_SHORT */
			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1018 */	0x0,		/* 0 */
			NdrFcShort( 0xffffffe5 ),	/* Offset= -27 (992) */
			0x5b,		/* FC_END */
/* 1022 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1024 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1026 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1028 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1030 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1034 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1036 */	NdrFcShort( 0xffffffde ),	/* Offset= -34 (1002) */
/* 1038 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1040 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] */
/* 1042 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1044) */
/* 1044 */	
			0x13, 0x0,	/* FC_OP */
/* 1046 */	NdrFcShort( 0xfffffc9e ),	/* Offset= -866 (180) */
/* 1048 */	
			0x11, 0x0,	/* FC_RP */
/* 1050 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1052) */
/* 1052 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1054 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1056 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1058 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1060 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1062 */	
			0x11, 0x0,	/* FC_RP */
/* 1064 */	NdrFcShort( 0x10 ),	/* Offset= 16 (1080) */
/* 1066 */	
			0x12, 0x0,	/* FC_UP */
/* 1068 */	NdrFcShort( 0xffffffa0 ),	/* Offset= -96 (972) */
/* 1070 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1072 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1074 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1076 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1078 */	NdrFcShort( 0xfffffff4 ),	/* Offset= -12 (1066) */
/* 1080 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1082 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1084 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1086 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1088 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1092 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1094 */	NdrFcShort( 0xffffffe8 ),	/* Offset= -24 (1070) */
/* 1096 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1098 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] */
/* 1100 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1102) */
/* 1102 */	
			0x13, 0x0,	/* FC_OP */
/* 1104 */	NdrFcShort( 0xffffffcc ),	/* Offset= -52 (1052) */
/* 1106 */	
			0x11, 0x0,	/* FC_RP */
/* 1108 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1110) */
/* 1110 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1112 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1114 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1116 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1118 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1120 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] */
/* 1122 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1124) */
/* 1124 */	
			0x13, 0x0,	/* FC_OP */
/* 1126 */	NdrFcShort( 0xfffffff0 ),	/* Offset= -16 (1110) */
/* 1128 */	
			0x11, 0x0,	/* FC_RP */
/* 1130 */	NdrFcShort( 0xc ),	/* Offset= 12 (1142) */
/* 1132 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1134 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1136 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1138 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1140 */	NdrFcShort( 0xffffffb6 ),	/* Offset= -74 (1066) */
/* 1142 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1144 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1146 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1148 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1150 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1154 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1156 */	NdrFcShort( 0xffffffe8 ),	/* Offset= -24 (1132) */
/* 1158 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1160 */	
			0x11, 0x0,	/* FC_RP */
/* 1162 */	NdrFcShort( 0x3a ),	/* Offset= 58 (1220) */
/* 1164 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 1166 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1168 */	0x18,		/* 24 */
			0x0,		/*  */
/* 1170 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1172 */	0x2,		/* FC_CHAR */
			0x5b,		/* FC_END */
/* 1174 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1176 */	NdrFcShort( 0x1c ),	/* 28 */
/* 1178 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1180 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1182 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1184 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1186 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1188 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1190 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1192 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1194 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1196 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1198 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1200 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1202 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1204 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1206 */	0x12, 0x0,	/* FC_UP */
/* 1208 */	NdrFcShort( 0xffffffd4 ),	/* Offset= -44 (1164) */
/* 1210 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1212 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1214 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1216 */	0x8,		/* FC_LONG */
			0x6,		/* FC_SHORT */
/* 1218 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 1220 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1222 */	NdrFcShort( 0x1c ),	/* 28 */
/* 1224 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1226 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1228 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1230 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1232 */	NdrFcShort( 0x1c ),	/* 28 */
/* 1234 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1236 */	NdrFcShort( 0x3 ),	/* 3 */
/* 1238 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1240 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1242 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1244 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1246 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1248 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1250 */	0x12, 0x8,	/* FC_UP [simple_pointer] */
/* 1252 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1254 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1256 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1258 */	0x12, 0x0,	/* FC_UP */
/* 1260 */	NdrFcShort( 0xffffffa0 ),	/* Offset= -96 (1164) */
/* 1262 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1264 */	0x0,		/* 0 */
			NdrFcShort( 0xffffffa5 ),	/* Offset= -91 (1174) */
			0x5b,		/* FC_END */
/* 1268 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] */
/* 1270 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1272) */
/* 1272 */	
			0x13, 0x0,	/* FC_OP */
/* 1274 */	NdrFcShort( 0x26 ),	/* Offset= 38 (1312) */
/* 1276 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 1278 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1280 */	0x18,		/* 24 */
			0x0,		/*  */
/* 1282 */	NdrFcShort( 0xc ),	/* 12 */
/* 1284 */	0x2,		/* FC_CHAR */
			0x5b,		/* FC_END */
/* 1286 */	
			0x16,		/* FC_PSTRUCT */
			0x3,		/* 3 */
/* 1288 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1290 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1292 */	
			0x46,		/* FC_NO_REPEAT */
			0x5c,		/* FC_PAD */
/* 1294 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1296 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1298 */	0x13, 0x0,	/* FC_OP */
/* 1300 */	NdrFcShort( 0xffffffe8 ),	/* Offset= -24 (1276) */
/* 1302 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1304 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1306 */	0x38,		/* FC_ALIGNM4 */
			0x8,		/* FC_LONG */
/* 1308 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1310 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1312 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1314 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1316 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1318 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1320 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1322 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1324 */	NdrFcShort( 0x14 ),	/* 20 */
/* 1326 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1328 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1330 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1332 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1334 */	0x13, 0x0,	/* FC_OP */
/* 1336 */	NdrFcShort( 0xffffffc4 ),	/* Offset= -60 (1276) */
/* 1338 */	
			0x5b,		/* FC_END */

			0x4c,		/* FC_EMBEDDED_COMPLEX */
/* 1340 */	0x0,		/* 0 */
			NdrFcShort( 0xffffffc9 ),	/* Offset= -55 (1286) */
			0x5b,		/* FC_END */
/* 1344 */	
			0x11, 0x0,	/* FC_RP */
/* 1346 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1348) */
/* 1348 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 1350 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1352 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1354 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1356 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 1358 */	
			0x11, 0x10,	/* FC_RP */
/* 1360 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1362) */
/* 1362 */	
			0x2f,		/* FC_IP */
			0x5c,		/* FC_PAD */
/* 1364 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1366 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1368 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] */
/* 1370 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1372) */
/* 1372 */	
			0x13, 0x0,	/* FC_OP */
/* 1374 */	NdrFcShort( 0x3c ),	/* Offset= 60 (1434) */
/* 1376 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1378 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1380 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1382 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1384 */	NdrFcShort( 0xfffffb5e ),	/* Offset= -1186 (198) */
/* 1386 */	
			0x1b,		/* FC_CARRAY */
			0x0,		/* 0 */
/* 1388 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1390 */	0x18,		/* 24 */
			0x0,		/*  */
/* 1392 */	NdrFcShort( 0x18 ),	/* 24 */
/* 1394 */	0x2,		/* FC_CHAR */
			0x5b,		/* FC_END */
/* 1396 */	
			0x1a,		/* FC_BOGUS_STRUCT */
			0x3,		/* 3 */
/* 1398 */	NdrFcShort( 0x38 ),	/* 56 */
/* 1400 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1402 */	NdrFcShort( 0x14 ),	/* Offset= 20 (1422) */
/* 1404 */	0x36,		/* FC_POINTER */
			0x36,		/* FC_POINTER */
/* 1406 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1408 */	0x8,		/* FC_LONG */
			0x8,		/* FC_LONG */
/* 1410 */	0x8,		/* FC_LONG */
			0x36,		/* FC_POINTER */
/* 1412 */	0x6,		/* FC_SHORT */
			0x6,		/* FC_SHORT */
/* 1414 */	0x38,		/* FC_ALIGNM4 */
			0xd,		/* FC_ENUM16 */
/* 1416 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1418 */	NdrFcShort( 0xffffffd6 ),	/* Offset= -42 (1376) */
/* 1420 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1422 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1424 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1426 */	
			0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1428 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1430 */	
			0x13, 0x0,	/* FC_OP */
/* 1432 */	NdrFcShort( 0xffffffd2 ),	/* Offset= -46 (1386) */
/* 1434 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1436 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1438 */	0x28,		/* 40 */
			0x54,		/* FC_DEREFERENCE */
#ifndef _ALPHA_
/* 1440 */	NdrFcShort( 0xc ),	/* x86, MIPS, PPC Stack size/offset = 12 */
#else
			NdrFcShort( 0x18 ),	/* Alpha Stack size/offset = 24 */
#endif
/* 1442 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1446 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1448 */	NdrFcShort( 0xffffffcc ),	/* Offset= -52 (1396) */
/* 1450 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1452 */	
			0x11, 0x10,	/* FC_RP */
/* 1454 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1456) */
/* 1456 */	
			0x2f,		/* FC_IP */
			0x5a,		/* FC_CONSTANT_IID */
/* 1458 */	NdrFcLong( 0x39c13a55 ),	/* 968964693 */
/* 1462 */	NdrFcShort( 0x11e ),	/* 286 */
/* 1464 */	NdrFcShort( 0x11d0 ),	/* 4560 */
/* 1466 */	0x96,		/* 150 */
			0x75,		/* 117 */
/* 1468 */	0x0,		/* 0 */
			0x20,		/* 32 */
/* 1470 */	0xaf,		/* 175 */
			0xd8,		/* 216 */
/* 1472 */	0xad,		/* 173 */
			0xb3,		/* 179 */
/* 1474 */	
			0x11, 0x0,	/* FC_RP */
/* 1476 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1478) */
/* 1478 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1480 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1482 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1484 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1486 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1488 */	
			0x11, 0x0,	/* FC_RP */
/* 1490 */	NdrFcShort( 0xc ),	/* Offset= 12 (1502) */
/* 1492 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1494 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1496 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1498 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1500 */	NdrFcShort( 0xfffffe4e ),	/* Offset= -434 (1066) */
/* 1502 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1504 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1506 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1508 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1510 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1514 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1516 */	NdrFcShort( 0xffffffe8 ),	/* Offset= -24 (1492) */
/* 1518 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1520 */	
			0x11, 0x0,	/* FC_RP */
/* 1522 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1524) */
/* 1524 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 1526 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1528 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1530 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1532 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 1534 */	
			0x11, 0x0,	/* FC_RP */
/* 1536 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1538) */
/* 1538 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1540 */	NdrFcShort( 0x8 ),	/* 8 */
/* 1542 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1544 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1546 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1548 */	NdrFcShort( 0xfffffa3e ),	/* Offset= -1474 (74) */
/* 1550 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1552 */	
			0x11, 0x0,	/* FC_RP */
/* 1554 */	NdrFcShort( 0xc ),	/* Offset= 12 (1566) */
/* 1556 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1558 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1560 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1562 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1564 */	NdrFcShort( 0xfffffe0e ),	/* Offset= -498 (1066) */
/* 1566 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1568 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1570 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1572 */	NdrFcShort( 0x14 ),	/* x86, MIPS, PPC Stack size/offset = 20 */
#else
			NdrFcShort( 0x28 ),	/* Alpha Stack size/offset = 40 */
#endif
/* 1574 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1578 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1580 */	NdrFcShort( 0xffffffe8 ),	/* Offset= -24 (1556) */
/* 1582 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1584 */	
			0x11, 0x0,	/* FC_RP */
/* 1586 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1588) */
/* 1588 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1590 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1592 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1594 */	NdrFcShort( 0x10 ),	/* x86, MIPS, PPC Stack size/offset = 16 */
#else
			NdrFcShort( 0x20 ),	/* Alpha Stack size/offset = 32 */
#endif
/* 1596 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1598 */	
			0x11, 0x0,	/* FC_RP */
/* 1600 */	NdrFcShort( 0xc ),	/* Offset= 12 (1612) */
/* 1602 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1604 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1606 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1608 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1610 */	NdrFcShort( 0xfffffde0 ),	/* Offset= -544 (1066) */
/* 1612 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1614 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1616 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1618 */	NdrFcShort( 0x4 ),	/* x86, MIPS, PPC Stack size/offset = 4 */
#else
			NdrFcShort( 0x8 ),	/* Alpha Stack size/offset = 8 */
#endif
/* 1620 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1624 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1626 */	NdrFcShort( 0xffffffe8 ),	/* Offset= -24 (1602) */
/* 1628 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1630 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] */
/* 1632 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1634) */
/* 1634 */	
			0x13, 0x0,	/* FC_OP */
/* 1636 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1638) */
/* 1638 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1640 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1642 */	0x28,		/* 40 */
			0x54,		/* FC_DEREFERENCE */
#ifndef _ALPHA_
/* 1644 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1646 */	0x8,		/* FC_LONG */
			0x5b,		/* FC_END */
/* 1648 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] */
/* 1650 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1652) */
/* 1652 */	
			0x13, 0x0,	/* FC_OP */
/* 1654 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1656) */
/* 1656 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1658 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1660 */	0x28,		/* 40 */
			0x54,		/* FC_DEREFERENCE */
#ifndef _ALPHA_
/* 1662 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1664 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1666 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1668 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1670 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1672 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1674 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1676 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1678 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1680 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1682 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1684 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1686 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] */
/* 1688 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1690) */
/* 1690 */	
			0x13, 0x0,	/* FC_OP */
/* 1692 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1694) */
/* 1694 */	
			0x1b,		/* FC_CARRAY */
			0x1,		/* 1 */
/* 1696 */	NdrFcShort( 0x2 ),	/* 2 */
/* 1698 */	0x28,		/* 40 */
			0x54,		/* FC_DEREFERENCE */
#ifndef _ALPHA_
/* 1700 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1702 */	0x6,		/* FC_SHORT */
			0x5b,		/* FC_END */
/* 1704 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] */
/* 1706 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1708) */
/* 1708 */	
			0x13, 0x0,	/* FC_OP */
/* 1710 */	NdrFcShort( 0xc ),	/* Offset= 12 (1722) */
/* 1712 */	0xb4,		/* FC_USER_MARSHAL */
			0x83,		/* 131 */
/* 1714 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1716 */	NdrFcShort( 0x10 ),	/* 16 */
/* 1718 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1720 */	NdrFcShort( 0xfffffa0e ),	/* Offset= -1522 (198) */
/* 1722 */	
			0x21,		/* FC_BOGUS_ARRAY */
			0x3,		/* 3 */
/* 1724 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1726 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1728 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1730 */	NdrFcLong( 0xffffffff ),	/* -1 */
/* 1734 */	0x4c,		/* FC_EMBEDDED_COMPLEX */
			0x0,		/* 0 */
/* 1736 */	NdrFcShort( 0xffffffe8 ),	/* Offset= -24 (1712) */
/* 1738 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */
/* 1740 */	
			0x11, 0x14,	/* FC_RP [alloced_on_stack] */
/* 1742 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1744) */
/* 1744 */	
			0x13, 0x0,	/* FC_OP */
/* 1746 */	NdrFcShort( 0x2 ),	/* Offset= 2 (1748) */
/* 1748 */	
			0x1b,		/* FC_CARRAY */
			0x3,		/* 3 */
/* 1750 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1752 */	0x28,		/* 40 */
			0x0,		/*  */
#ifndef _ALPHA_
/* 1754 */	NdrFcShort( 0x8 ),	/* x86, MIPS, PPC Stack size/offset = 8 */
#else
			NdrFcShort( 0x10 ),	/* Alpha Stack size/offset = 16 */
#endif
/* 1756 */	
			0x4b,		/* FC_PP */
			0x5c,		/* FC_PAD */
/* 1758 */	
			0x48,		/* FC_VARIABLE_REPEAT */
			0x49,		/* FC_FIXED_OFFSET */
/* 1760 */	NdrFcShort( 0x4 ),	/* 4 */
/* 1762 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1764 */	NdrFcShort( 0x1 ),	/* 1 */
/* 1766 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1768 */	NdrFcShort( 0x0 ),	/* 0 */
/* 1770 */	0x13, 0x8,	/* FC_OP [simple_pointer] */
/* 1772 */	
			0x25,		/* FC_C_WSTRING */
			0x5c,		/* FC_PAD */
/* 1774 */	
			0x5b,		/* FC_END */

			0x8,		/* FC_LONG */
/* 1776 */	0x5c,		/* FC_PAD */
			0x5b,		/* FC_END */

			0x0
        }
    };

const CInterfaceProxyVtbl * _opcda_ProxyVtblList[] = 
{
    ( CInterfaceProxyVtbl *) &_IOPCServerProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCServerPublicGroupsProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCBrowseServerAddressSpaceProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCGroupStateMgtProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCPublicGroupStateMgtProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCSyncIOProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCAsyncIOProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCItemMgtProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IEnumOPCItemAttributesProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCDataCallbackProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCAsyncIO2ProxyVtbl,
    ( CInterfaceProxyVtbl *) &_IOPCItemPropertiesProxyVtbl,
    0
};

const CInterfaceStubVtbl * _opcda_StubVtblList[] = 
{
    ( CInterfaceStubVtbl *) &_IOPCServerStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCServerPublicGroupsStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCBrowseServerAddressSpaceStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCGroupStateMgtStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCPublicGroupStateMgtStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCSyncIOStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCAsyncIOStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCItemMgtStubVtbl,
    ( CInterfaceStubVtbl *) &_IEnumOPCItemAttributesStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCDataCallbackStubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCAsyncIO2StubVtbl,
    ( CInterfaceStubVtbl *) &_IOPCItemPropertiesStubVtbl,
    0
};

PCInterfaceName const _opcda_InterfaceNamesList[] = 
{
    "IOPCServer",
    "IOPCServerPublicGroups",
    "IOPCBrowseServerAddressSpace",
    "IOPCGroupStateMgt",
    "IOPCPublicGroupStateMgt",
    "IOPCSyncIO",
    "IOPCAsyncIO",
    "IOPCItemMgt",
    "IEnumOPCItemAttributes",
    "IOPCDataCallback",
    "IOPCAsyncIO2",
    "IOPCItemProperties",
    0
};


#define _opcda_CHECK_IID(n)	IID_GENERIC_CHECK_IID( _opcda, pIID, n)

int __stdcall _opcda_IID_Lookup( const IID * pIID, int * pIndex )
{
    IID_BS_LOOKUP_SETUP

    IID_BS_LOOKUP_INITIAL_TEST( _opcda, 12, 8 )
    IID_BS_LOOKUP_NEXT_TEST( _opcda, 4 )
    IID_BS_LOOKUP_NEXT_TEST( _opcda, 2 )
    IID_BS_LOOKUP_NEXT_TEST( _opcda, 1 )
    IID_BS_LOOKUP_RETURN_RESULT( _opcda, 12, *pIndex )
    
}

const ExtendedProxyFileInfo opcda_ProxyFileInfo = 
{
    (PCInterfaceProxyVtblList *) & _opcda_ProxyVtblList,
    (PCInterfaceStubVtblList *) & _opcda_StubVtblList,
    (const PCInterfaceName * ) & _opcda_InterfaceNamesList,
    0, // no delegation
    & _opcda_IID_Lookup, 
    12,
    2
};
