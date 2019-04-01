// OPCTHRD.h
//
// (c) Copyright 1998 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Specifications and may be used
//  as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by
//  Al Chisholm, Intellution Inc.
//
// CONTENTS:
//
// This file contains function prototyles
// for the OPC Pack/Unpack functions for use with IDataObject
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//  2.00  05/01/97 acc   
//

int		OPCVariantSize( VARIANT * v);
int		OPCVariantPack( char * t, VARIANT *s);
void	OPCVariantUnpack( VARIANT *vp, char * src);


