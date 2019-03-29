// Define the CATID for OPC Batch Servers
// This matches the ones defined in the OPC Batch Specification 1.0
// and 2.0

// History:
//  Jan 2000 - original for version 1.0
//  Nov 1, 2000 - WGI - added new CATID for version 2.0


#ifdef __cplusplus
extern "C"{
#endif 


#ifndef __GUID_DEFINED__
#define __GUID_DEFINED__

typedef struct _GUID
{
    unsigned long x;
    unsigned short s1;
    unsigned short s2;
    unsigned char  c[8];
} GUID;

#endif // __GUID_DEFINED__

const GUID CATID_OPCBatchServer10 = 
{ 0xa8080da0, 0xe23e, 0x11d2, { 0xaf, 0xa7, 0x0, 0xc0, 0x4f, 0x53, 0x94, 0x21 } };
// {a8080da0-e23e-11d2-afa7-00c04f539421}

const GUID CATID_OPCBatchServer20 = 
{ 0x843de67b, 0xb0c9, 0x11d4, { 0xa0, 0xb7, 0x0, 0x1, 0x2, 0xa9, 0x80, 0xb1 } };
// {843DE67B-B0C9-11d4-A0B7-000102A980B1}


#ifdef __cplusplus
}
#endif