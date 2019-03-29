// BatchItems.cpp
//
// (c) Copyright 2000 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Batch Specification and may be used
//  as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by
//  Will Irwin, Fisher-Rosemount Systems Inc. and 
//  Dave Emerson, Yokogawa Corporation of America.
//  James Wang, Yokogawa Corporation of America: Added XML file interface.
//
// CONTENTS:
//
//  This file contains the code to create simulated data
//  for the OPC Batch Sample server.

#include "OPCXXX.h"
#include "OPCProps.h"
#include "OPCBatchProps.h"
#include "OPCBatchDef.h"

#define MAX_BATCH_SIZE		50

void CreateSamplePhysObjects();
void CreateSampleBatchObjects();
void CreateSampleRPEObjects();
void CreateSampleRootObjects();


// Create some data to represent batch objects of
// various kinds.  In this sample server these are
// stored as arrays of VARIANTs, which is convenient
// when they are accessed as properties.
// A real server would get similar information
// from a real batch execution system.

// TODO
//   Add more batches, equipment etc to make 
//   the sample more 'interesting'
//
//   Make it read the data from a text file to
//   allow the data to change without modifying code
//   ... this would require making the code more
//   robust in case the person creating the text
//   file left something out or made some other
//   error....
//
//   Make the data dynamic (batches added and removed
//   from the list over time) so that clients can work
//   out how they should handle this.



// An object to act as the root of the namespace.
// This has no properties.
OPCBObj root_tag;

// Create two objects to represent the well-known 
// itemIDs that live just below the root of the batch 
// hierarchy. These are here to act as the parents 
// of the various other batch objects.  The don't 
// really have any properties but we assign them
// the same set of properties (with default values)
// as the other items in their part of the namespace.
OPCBPhysObj OPCBPhysicalModel;
OPCBBatchObj OPCBBatchModel;

//V2Draft3: Added OPCBMasterRecipeModel and OPCBBatchArchiveModel
OPCBMasterRecipeObj OPCBMasterRecipeModel;
OPCBBatchObj OPCBBatchArchiveModel;

// Create an object to represent the well-known itemID
// OPCBBatchIDList.  This is held in the OPC_B_VALUE
// property and is of type VT_ARRAY | VT_BSTR
OPCBBatchIDListObj  OPCBBatchIDList;

//V2Draft3
OPCBObj* root_list[] = { &OPCBPhysicalModel, &OPCBMasterRecipeModel, &OPCBBatchModel, 
	&OPCBBatchArchiveModel, &OPCBBatchIDList, NULL };

/* Sample equipment hierarchy is:
 OPCBPhysicalModel
   Area1
     PCell1
       Unit11
         EqMod111
       Unit21
         EqMod211
     PCell2
       Unit21
         EqMod211
       Unit22
         EqMod221
*/

// Be really careful when modifying the hierarchy. For example, 
// omitting the NULL that terminates each of these arrays of
// child pointers will cause no compiler error, and it may even
// appear to run correctly, but the hierarchy will be messed up.
// It's important to view the hierarchy, preferably with a client
// that lets you see the whole thing at once, to make sure it is
// properly linked together.

// Note that there is no consistency enforced here between the 
// various parts of the hierarchy.  It is up to whomever modifies
// the code below to make sure that (for example) if a Batch 
// has a Unit in its AllocationEquipmentList, then that Unit also
// has the Batch in its RelatedBatchIDs property.  This gets hard
// to maintain as this hierachy gets larger.   Of course in a real
// server all of this would be real data reflecting what was going
// on in a batch execution system.

/*  
  Fragment of code showing how to read a property....
  VARIANT v;
  VariantInit(&v);
  HRESULT hr;
  hr = Area1.GetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_LEVEL, v);
*/

OPCBPhysObj Area1, PCell1, PCell2, Unit11, Unit12, Unit21, Unit22, EqMod111, EqMod121, EqMod211, EqMod221;

OPCBObj *PhysModelRoot_list[] = { &Area1, NULL };
OPCBObj *Area1_list[] = { &PCell1, &PCell2, NULL };
OPCBObj *PCell1_list[] = { &Unit11, &Unit12, NULL };
OPCBObj *PCell2_list[] = { &Unit21, &Unit22, NULL };
OPCBObj *Unit11_list[] = { &EqMod111, NULL };
OPCBObj *Unit12_list[] = { &EqMod121, NULL };
OPCBObj *Unit21_list[] = { &EqMod211, NULL };
OPCBObj *Unit22_list[] = { &EqMod221, NULL };

void CreateSamplePhysObjects()
{
  Area1.SetName(L"AREA1");
  Area1.SetItemID(L"OPCBPhysicalModel.AREA1");
  Area1.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);  // what's the canonical data type of an Area?!
  Area1.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBPhysicalModel.AREA1");
  Area1.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Area1.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Area1.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Area1.SetPropertyAtID(OPC_PROP_DESC, L"Soup Plant - 3rd floor");
  Area1.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBPhysicalModel.AREA1");
  Area1.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBPhysicalModel.AREA1");
  Area1.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Area1.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_LEVEL, (DWORD) OPCB_PHYS_AREA);
  Area1.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_STATE, L"Area in production");
  Area1.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_MODE, L"no mode");
  
  // Create an array with the related batch IDs of Area1
  LPCWSTR arrString[]  = { L"BATCH1", L"BATCH2", L"BATCH3" };
  SAFEARRAY* pArr;
  CreateSafeArrayFromStrings(arrString, 3, pArr);
  Area1.SetPropertyAtID(OPC_PROP_B_RELATED_BATCH_IDS, pArr);
  
// Some test code
  VARIANT vTest;  
  VariantInit(&vTest);
  Area1.GetPropertyAtID(OPC_PROP_B_RELATED_BATCH_IDS, vTest);
  SAFEARRAY* saTest = vTest.parray;
  BSTR bstrTemp = NULL;

  for (long i = 0; i < 3; i++)
  {
    HRESULT hrtest = SafeArrayGetElement(saTest, &i, (void *)&bstrTemp);
  }
// End of test code


  Area1.SetChildList((OPCBObj**)&Area1_list);
  Area1.SetParent(&OPCBPhysicalModel);
  AddSampleBatchObject(&Area1);


  PCell1.SetName(L"PCELL1");
  PCell1.SetItemID(L"OPCBPhysicalModel.AREA1.PCELL1");
  PCell1.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  PCell1.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBPhysicalModel.AREA1.PCELL1");
  PCell1.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  PCell1.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  PCell1.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  PCell1.SetPropertyAtID(OPC_PROP_DESC, L"Canned soup line");
  PCell1.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBPhysicalModel.AREA1.PCELL1");
  PCell1.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBPhysicalModel.AREA1.PCELL1");
  PCell1.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  PCell1.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_LEVEL, (DWORD) OPCB_PHYS_PROCESSCELL);
  PCell1.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_STATE, L"Cell in production");
  PCell1.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_MODE, L"no mode");

  // Create an array with the related batch IDs of PCell1
  LPCWSTR arrString2[]  = { L"BATCH1" };
  SAFEARRAY* pArr2;
  CreateSafeArrayFromStrings(arrString, 1, pArr2);
  PCell1.SetPropertyAtID(OPC_PROP_B_RELATED_BATCH_IDS, pArr2);

  PCell1.SetChildList((OPCBObj**)&PCell1_list);
  PCell1.SetParent(&Area1);
  AddSampleBatchObject(&PCell1);

  PCell2.SetName(L"PCELL2");
  PCell2.SetItemID(L"OPCBPhysicalModel.AREA1.PCELL2");
  PCell2.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR); 
  PCell2.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBPhysicalModel.AREA1.PCELL2");
  PCell2.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  PCell2.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  PCell2.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  PCell2.SetPropertyAtID(OPC_PROP_DESC, L"Bulk soup line");
  PCell2.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBPhysicalModel.AREA1.PCELL2");
  PCell2.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBPhysicalModel.AREA1.PCELL2");
  PCell2.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  PCell2.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_LEVEL, (DWORD) OPCB_PHYS_PROCESSCELL);
  PCell2.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_STATE, L"Cell in startup");
  PCell2.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_MODE, L"no mode");

  // Create an array with the related batch IDs of PCell2
  LPCWSTR arrString3[]  = { L"BATCH3" };
  SAFEARRAY* pArr3;
  CreateSafeArrayFromStrings(arrString, 1, pArr3);
  PCell2.SetPropertyAtID(OPC_PROP_B_RELATED_BATCH_IDS, pArr3);

  PCell2.SetChildList((OPCBObj**)&PCell2_list);
  PCell2.SetParent(&Area1);
  AddSampleBatchObject(&PCell2);

  Unit11.SetName(L"UNIT11");
  Unit11.SetItemID(L"OPCBPhysicalModel.AREA1.PCELL1.UNIT11");
  Unit11.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Unit11.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBPhysicalModel.AREA1.PCELL1.UNIT11");
  Unit11.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Unit11.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Unit11.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Unit11.SetPropertyAtID(OPC_PROP_DESC, L"Large steam-heated cooker");
  Unit11.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBPhysicalModel.AREA1.PCELL1.UNIT11");
  Unit11.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBPhysicalModel.AREA1.PCELL1.UNIT11");
  Unit11.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Unit11.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_LEVEL, (DWORD) OPCB_PHYS_UNIT);
  Unit11.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_STATE, L"RUNNING");
  Unit11.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_MODE, L"AUTO");

    // Create an array with the related batch IDs of Unit11
  LPCWSTR arrString4[]  = { L"BATCH1" };
  SAFEARRAY* pArr4;
  CreateSafeArrayFromStrings(arrString4, 1, pArr4);
  Unit11.SetPropertyAtID(OPC_PROP_B_RELATED_BATCH_IDS, pArr4);

  Unit11.SetChildList((OPCBObj**)&Unit11_list);
  Unit11.SetParent(&PCell1);
  AddSampleBatchObject(&Unit11);

  Unit12.SetName(L"UNIT12");
  Unit12.SetItemID(L"OPCBPhysicalModel.AREA1.PCELL1.UNIT12");
  Unit12.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Unit12.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBPhysicalModel.AREA1.PCELL1.UNIT12");
  Unit12.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Unit12.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Unit12.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Unit12.SetPropertyAtID(OPC_PROP_DESC, L"Sterilizer");
  Unit12.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBPhysicalModel.AREA1.PCELL1.UNIT12");
  Unit12.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBPhysicalModel.AREA1.PCELL1.UNIT12");
  Unit12.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Unit12.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_LEVEL, (DWORD) OPCB_PHYS_UNIT);
  Unit12.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_STATE, L"COMPLETE");
  Unit12.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_MODE, L"AUTO");

  // Unit12 has no current batches
  
  Unit12.SetChildList((OPCBObj**)&Unit12_list);
  Unit12.SetParent(&PCell1);
  AddSampleBatchObject(&Unit12);

  Unit21.SetName(L"UNIT21");
  Unit21.SetItemID(L"OPCBPhysicalModel.AREA1.PCELL2.UNIT21");
  Unit21.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Unit21.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBPhysicalModel.AREA1.PCELL2.UNIT21");
  Unit21.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Unit21.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Unit21.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Unit21.SetPropertyAtID(OPC_PROP_DESC, L"Small electrically-heated cooker");
  Unit21.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBPhysicalModel.AREA1.PCELL2.UNIT21");
  Unit21.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBPhysicalModel.AREA1.PCELL2.UNIT21");
  Unit21.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Unit21.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_LEVEL, (DWORD) OPCB_PHYS_UNIT);
  Unit21.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_STATE, L"IDLE");
  Unit21.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_MODE, L"MANUAL");

  // Create an array with the related batch IDs of Unit21
  LPCWSTR arrString5[]  = { L"BATCH3" };
  SAFEARRAY* pArr5;
  CreateSafeArrayFromStrings(arrString5, 1, pArr5);
  Unit21.SetPropertyAtID(OPC_PROP_B_RELATED_BATCH_IDS, pArr5);

  Unit21.SetChildList((OPCBObj**)&Unit21_list);
  Unit21.SetParent(&PCell2);  
  AddSampleBatchObject(&Unit21);

  Unit22.SetName(L"UNIT22");
  Unit22.SetItemID(L"OPCBPhysicalModel.AREA1.PCELL2.UNIT22");
  Unit22.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Unit22.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBPhysicalModel.AREA1.PCELL2.UNIT22");
  Unit22.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Unit22.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Unit22.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Unit22.SetPropertyAtID(OPC_PROP_DESC, L"Small sterlizer");
  Unit22.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBPhysicalModel.AREA1.PCELL2.UNIT22");
  Unit22.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBPhysicalModel.AREA1.PCELL2.UNIT22");
  Unit22.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Unit22.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_LEVEL, (DWORD) OPCB_PHYS_UNIT);
  Unit22.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_STATE, L"RUNNING");
  Unit22.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_MODE, L"AUTO");

  // Create an array with the related batch IDs of Unit22
  LPCWSTR arrString6[]  = { L"BATCH2" };
  SAFEARRAY* pArr6;
  CreateSafeArrayFromStrings(arrString, 1, pArr6);
  Unit22.SetPropertyAtID(OPC_PROP_B_RELATED_BATCH_IDS, pArr6);

  Unit22.SetChildList((OPCBObj**)&Unit22_list);
  Unit22.SetParent(&PCell2);
  AddSampleBatchObject(&Unit22);

  EqMod111.SetName(L"EQMOD111");
  EqMod111.SetItemID(L"OPCBPhysicalModel.AREA1.PCELL1.UNIT11.EQMOD111");
  EqMod111.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  EqMod111.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBPhysicalModel.AREA1.PCELL1.UNIT11.EQMOD111");
  EqMod111.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  EqMod111.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  EqMod111.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  EqMod111.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBPhysicalModel.AREA1.PCELL1.UNIT11.EQMOD111");
  EqMod111.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBPhysicalModel.AREA1.PCELL1.UNIT11.EQMOD111");
  EqMod111.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  EqMod111.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_LEVEL, (DWORD) OPCB_PHYS_EQUIPMENTMODULE);
  EqMod111.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_STATE, L"RUNNING");
  EqMod111.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_MODE, L"AUTO");

  EqMod111.SetChildList((OPCBObj**)NULL);
  EqMod111.SetParent(&Unit11);
  AddSampleBatchObject(&EqMod111);

  EqMod121.SetName(L"EQMOD121");
  EqMod121.SetItemID(L"OPCBPhysicalModel.AREA1.PCELL1.UNIT12.EQMOD121");
  EqMod121.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  EqMod121.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBPhysicalModel.AREA1.PCELL1.UNIT12.EQMOD121");
  EqMod121.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  EqMod121.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  EqMod121.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  EqMod121.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBPhysicalModel.AREA1.PCELL1.UNIT12.EQMOD121");
  EqMod121.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBPhysicalModel.AREA1.PCELL1.UNIT12.EQMOD121");
  EqMod121.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  EqMod121.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_LEVEL, (DWORD) OPCB_PHYS_EQUIPMENTMODULE);
  EqMod121.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_STATE, L"IDLE");
  EqMod121.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_MODE, L"MANUAL");

  EqMod121.SetChildList((OPCBObj**)NULL);
  EqMod121.SetParent(&Unit12);
  AddSampleBatchObject(&EqMod121);

  EqMod211.SetName(L"EQMOD211");
  EqMod211.SetItemID(L"OPCBPhysicalModel.AREA1.PCELL2.UNIT21.EQMOD211");
  EqMod211.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  EqMod211.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBPhysicalModel.AREA1.PCELL2.UNIT21.EQMOD211");
  EqMod211.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  EqMod211.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  EqMod211.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  EqMod211.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBPhysicalModel.AREA1.PCELL2.UNIT21.EQMOD211");
  EqMod211.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBPhysicalModel.AREA1.PCELL2.UNIT21.EQMOD211");
  EqMod211.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  EqMod211.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_LEVEL, (DWORD) OPCB_PHYS_EQUIPMENTMODULE);
  EqMod211.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_STATE, L"IDLE");
  EqMod211.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_MODE, L"AUTO");

  EqMod211.SetChildList((OPCBObj**)NULL);
  EqMod211.SetParent(&Unit21);
  AddSampleBatchObject(&EqMod211);

  EqMod221.SetName(L"EQMOD221");
  EqMod221.SetItemID(L"OPCBPhysicalModel.AREA1.PCELL2.UNIT22.EQMOD221");
  EqMod221.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  EqMod221.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBPhysicalModel.AREA1.PCELL2.UNIT22.EQMOD221");
  EqMod221.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  EqMod221.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  EqMod221.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  EqMod221.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBPhysicalModel.AREA1.PCELL2.UNIT22.EQMOD221");
  EqMod221.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBPhysicalModel.AREA1.PCELL2.UNIT22.EQMOD221");
  EqMod221.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  EqMod221.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_LEVEL, (DWORD) OPCB_PHYS_EQUIPMENTMODULE);
  EqMod221.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_STATE, L"RUNNING");
  EqMod221.SetPropertyAtID(OPC_PROP_B_EQUIPMENT_MODE, L"AUTO");

  EqMod221.SetChildList((OPCBObj**)NULL);
  EqMod221.SetParent(&Unit22);
  AddSampleBatchObject(&EqMod221);
}




/* Sample RPE Hierarchy is:

  Proc1
    UProc1
      Oper1
        Phase1
      Oper2
        Phase2

  UProc2
    Oper3
      Phase3
*/

// Declare the batches here to allow parent/child 
// pointers to be set up.
OPCBBatchObj Batch1, Batch2, Batch3;


// Declare some RPEs 
OPCBRPEObj Proc1, UProc1, UProc2, Oper1, Oper2, Oper3, Phase1, Phase2, Phase3, Phase4;

// Declare some "OPCBParameters" and "OPCBResults" objects.
// These objects are the 'collections' that act as the
// parents of the parameters and reports in the hierarchy.
// Any RPE having parameters will have a "OPCBParameters"
// item under it.  Any RPE having results will have a 
// "OPCBResults" item under it.
//
// In the example below, PROC1 has two parameters and one
// report.  The hierarchy looks like this:
//
//   OPCBBatchModel
//     OPCBBatchModel.BATCH1
//       OPCBBatchModel.BATCH1.PROC1
//         OPCBBatchModel.BATCH1.PROC1.OPCBParameters
//           OPCBBatchModel.BATCH1.PROC1.OPCBParameters.CatalystAmount
//           OPCBBatchModel.BATCH1.PROC1.OPCBParameters.TargetSoakTime
//         OPCBBatchModel.BATCH1.PROC1.OPCBResults
//           OPCBBatchModel.BATCH1.PROC1.OPCBResults.ReactionDuration

// The OPCBParameters collection should support the same properties
// as the individual parameters.  This allows a client to find out
// what properties it can expect by doing a QueryItemProperties
// once on the OPCBParameters item. 
OPCBParamObj Proc1Params;

// The OPCBResults collection should support the same properties
// as the individual results.  This allows a client to find out
// what properties it can expect by doing a QueryItemProperties
// once on the OPCBResults item. 
OPCBResultObj Proc1Results;

// Declare some objects to represent the parameters and 
// reports themselves
OPCBParamObj  Proc1Param1, Proc1Param2;
OPCBResultObj Proc1Result1;

OPCBObj *Proc1_list[] = { &UProc1, &Proc1Params, &Proc1Results, NULL };
OPCBObj *UProc1_list[] = { &Oper1, &Oper2, NULL };
OPCBObj *UProc2_list[] = { &Oper3, NULL };
OPCBObj *Oper1_list[] = { &Phase1, NULL };
OPCBObj *Oper2_list[] = { &Phase2, NULL };
OPCBObj *Oper3_list[] = { &Phase3, NULL };

OPCBObj *Proc1Param_list[] = { &Proc1Param1, &Proc1Param2, NULL };
OPCBObj *Proc1Result_list[] = { &Proc1Result1, NULL };



void CreateSampleRPEObjects()
{
  Proc1.SetName(L"PROC1");
  Proc1.SetItemID(L"OPCBBatchModel.BATCH1.PROC1");
  Proc1.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Proc1.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBBatchModel.BATCH1.PROC1");
  Proc1.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Proc1.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Proc1.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Proc1.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH1.PROC1");
  Proc1.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBBatchModel.BATCH1.PROC1");
  Proc1.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Proc1.SetPropertyAtID(OPC_PROP_B_DESC, L"Procedure to make soup");
  Proc1.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)OPCB_PROC_PROCEDURE);
  Proc1.SetPropertyAtID(OPC_PROP_B_VERSION, L"1.23 dated 28-OCT-1999");
  Proc1.SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, L"");
  Proc1.SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"RUNNING");
  Proc1.SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD)OPCB_STATE_RUNNING);
  Proc1.SetPropertyAtID(OPC_PROP_B_PARAMETER_COUNT, (DWORD)2);
  Proc1.SetPropertyAtID(OPC_PROP_B_RESULT_COUNT, (DWORD)1);

  Proc1.SetChildList((OPCBObj**)&Proc1_list);
  Proc1.SetParent(&Batch1);
  AddSampleBatchObject(&Proc1);

  Proc1Params.SetName(L"OPCBParameters");
  Proc1Params.SetItemID(L"OPCBBatchModel.BATCH1.PROC1.OPCBParameters");
  Proc1.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Proc1Params.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Proc1Params.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Proc1Params.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH1.PROC1.OPCBParameters");
  Proc1Params.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBBatchModel.BATCH1.PROC1.OPCBParameters");
  Proc1Params.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Proc1Params.SetPropertyAtID(OPC_PROP_B_DESC, L"Parameters for soup-making");
  Proc1Params.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)OPCB_PROC_PARAMETER_COLLECTION);

  Proc1Params.SetChildList((OPCBObj**)&Proc1Param_list);
  Proc1Params.SetParent(&Proc1);
  AddSampleBatchObject(&Proc1Params);

  Proc1Param1.SetName(L"CatalystAmount"); 
  Proc1Param1.SetItemID(L"OPCBBatchModel.BATCH1.PROC1.OPCBParameters.CatalystAmount");
  Proc1Param1.SetPropertyAtID(OPC_PROP_CDT, (short)VT_R4);
  Proc1Param1.SetPropertyAtID(OPC_PROP_VALUE, (float)4.66);
  Proc1Param1.SetPropertyAtID(OPC_PROP_UNIT, L"kg");
  Proc1Param1.SetPropertyAtID(OPC_PROP_DESC, L"Amount of catalyst");
  Proc1Param1.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH1.PROC1.OPCBParameters.CatalystAmount");
  Proc1Param1.SetPropertyAtID(OPC_PROP_B_VALUE, (float)4.66);
  Proc1Param1.SetPropertyAtID(OPC_PROP_B_DESC, L"Amount of catalyst");
  Proc1Param1.SetPropertyAtID(OPC_PROP_B_HIGH_VALUE_LIMIT, (float)7.0);
  Proc1Param1.SetPropertyAtID(OPC_PROP_B_LOW_VALUE_LIMIT, (float)2.0);

  Proc1Param1.SetChildList((OPCBObj**)NULL);
  Proc1Param1.SetParent(&Proc1Params);
  AddSampleBatchObject(&Proc1Param1);

  Proc1Param2.SetName(L"TargetSoakTime"); 
  Proc1Param2.SetItemID(L"OPCBBatchModel.BATCH1.PROC1.OPCBParameters.TargetSoakTime");
  Proc1Param2.SetPropertyAtID(OPC_PROP_CDT, (short)VT_I2);
  Proc1Param2.SetPropertyAtID(OPC_PROP_VALUE, (short)125);
  Proc1Param2.SetPropertyAtID(OPC_PROP_UNIT, L"sec");
  Proc1Param2.SetPropertyAtID(OPC_PROP_DESC, L"seconds to soak");
  Proc1Param2.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH1.PROC1.OPCBParameters.TargetSoakTime");
  Proc1Param2.SetPropertyAtID(OPC_PROP_B_VALUE, (short)125);
  Proc1Param2.SetPropertyAtID(OPC_PROP_B_DESC, L"seconds to soak");
  Proc1Param2.SetPropertyAtID(OPC_PROP_B_HIGH_VALUE_LIMIT, (short)200);
  Proc1Param2.SetPropertyAtID(OPC_PROP_B_LOW_VALUE_LIMIT, (short)0);

  Proc1Param2.SetChildList((OPCBObj**)NULL);
  Proc1Param2.SetParent(&Proc1Params);
  AddSampleBatchObject(&Proc1Param2);


  Proc1Results.SetName(L"OPCBResults");
  Proc1Results.SetItemID(L"OPCBBatchModel.BATCH1.PROC1.OPCBResults");
  Proc1.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Proc1Results.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Proc1Results.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Proc1Results.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH1.PROC1.OPCBResults");
  Proc1Results.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBBatchModel.BATCH1.PROC1.OPCBResults");
  Proc1Results.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Proc1Results.SetPropertyAtID(OPC_PROP_B_DESC, L"Results for soup-making");
  Proc1Results.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)OPCB_PROC_RESULT_COLLECTION);

  Proc1Results.SetChildList((OPCBObj**)&Proc1Result_list);
  Proc1Results.SetParent(&Proc1);
  AddSampleBatchObject(&Proc1Results);
  Proc1Result1.SetName(L"ReactionDuration"); 
  Proc1Result1.SetItemID(L"OPCBBatchModel.BATCH1.PROC1.OPCBResults.ReactionDuration");
  Proc1Result1.SetPropertyAtID(OPC_PROP_CDT, (short)VT_R4);
  Proc1Result1.SetPropertyAtID(OPC_PROP_VALUE, (float)42.0);
  Proc1Result1.SetPropertyAtID(OPC_PROP_UNIT, L"min");
  Proc1Result1.SetPropertyAtID(OPC_PROP_DESC, L"actual reaction time");
  Proc1Result1.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH1.PROC1.OPCBResults.ReactionDuration");
  Proc1Result1.SetPropertyAtID(OPC_PROP_B_VALUE, (float)42.0);
  Proc1Result1.SetPropertyAtID(OPC_PROP_B_DESC, L"actual reaction time");

  Proc1Result1.SetChildList((OPCBObj**)NULL);
  Proc1Result1.SetParent(&Proc1Results);
  AddSampleBatchObject(&Proc1Result1);


  UProc1.SetName(L"UPROC1");
  UProc1.SetItemID(L"OPCBBatchModel.BATCH1.PROC1.UPROC1");
  UProc1.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  UProc1.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBBatchModel.BATCH1.PROC1.UPROC1");
  UProc1.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  UProc1.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  UProc1.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  UProc1.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH1.PROC1.UPROC1");
  UProc1.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBBatchModel.BATCH1.PROC1.UPROC1");
  UProc1.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  UProc1.SetPropertyAtID(OPC_PROP_B_DESC, L"Unit Procedure to heat cooker");
  UProc1.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)OPCB_PROC_UNITPROCEDURE);
  UProc1.SetPropertyAtID(OPC_PROP_B_VERSION, L"2.25 dated 28-OCT-1999");
  UProc1.SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, L"UNIT11");
  UProc1.SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"RUNNING");
  UProc1.SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD)OPCB_STATE_RUNNING);
  UProc1.SetPropertyAtID(OPC_PROP_B_PARAMETER_COUNT, (DWORD)0);
  UProc1.SetPropertyAtID(OPC_PROP_B_RESULT_COUNT, (DWORD)0);

  UProc1.SetChildList((OPCBObj**)&UProc1_list);
  UProc1.SetParent(&Proc1);
  AddSampleBatchObject(&UProc1);

  UProc2.SetName(L"UPROC2");
  UProc2.SetItemID(L"OPCBBatchModel.BATCH3.UPROC2");
  UProc2.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  UProc2.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBBatchModel.BATCH3.UPROC2");
  UProc2.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  UProc2.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  UProc2.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  UProc2.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH3.UPROC2");
  UProc2.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBBatchModel.BATCH3.UPROC2");
  UProc2.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  UProc2.SetPropertyAtID(OPC_PROP_B_DESC, L"Unit Procedure to blend ingredients");
  UProc2.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)OPCB_PROC_UNITPROCEDURE);
  UProc2.SetPropertyAtID(OPC_PROP_B_VERSION, L"2.25 dated 28-OCT-1999");
  UProc2.SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, L"UNIT21");
  UProc2.SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"HOLDING");
  UProc2.SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD)OPCB_STATE_HOLDING);
  UProc2.SetPropertyAtID(OPC_PROP_B_PARAMETER_COUNT, (DWORD)0);
  UProc2.SetPropertyAtID(OPC_PROP_B_RESULT_COUNT, (DWORD)0);

  UProc2.SetChildList((OPCBObj**)&UProc2_list);
  UProc2.SetParent(&Batch3);
  AddSampleBatchObject(&UProc2);

  Oper1.SetName(L"OPER1");
  Oper1.SetItemID(L"OPCBBatchModel.BATCH1.UPROC1.OPER1");
  Oper1.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Oper1.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBBatchModel.BATCH1.UPROC1.OPER1");
  Oper1.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Oper1.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Oper1.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Oper1.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH1.UPROC1.OPER1");
  Oper1.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBBatchModel.BATCH1.UPROC1.OPER1");
  Oper1.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Oper1.SetPropertyAtID(OPC_PROP_B_DESC, L"Operation to heat up a vessel");
  Oper1.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)OPCB_PROC_OPERATION);
  Oper1.SetPropertyAtID(OPC_PROP_B_VERSION, L"2.25 dated 28-OCT-1999");
  Oper1.SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, L"UNIT11");
  Oper1.SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"RUNNING");
  Oper1.SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD)OPCB_STATE_RUNNING);
  Oper1.SetPropertyAtID(OPC_PROP_B_PARAMETER_COUNT, (DWORD)0);
  Oper1.SetPropertyAtID(OPC_PROP_B_RESULT_COUNT, (DWORD)0);

  Oper1.SetChildList((OPCBObj**)&Oper1_list);
  Oper1.SetParent(&UProc1);
  AddSampleBatchObject(&Oper1);

  Oper2.SetName(L"OPER2");
  Oper2.SetItemID(L"OPCBBatchModel.BATCH1.PROC1.UPROC1.OPER2");
  Oper2.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Oper2.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBBatchModel.BATCH1.PROC1.UPROC1.OPER2");
  Oper2.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Oper2.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Oper2.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Oper2.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH1.PROC1.UPROC1.OPER2");
  Oper2.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBBatchModel.BATCH1.PROC1.UPROC1.OPER2");
  Oper2.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Oper2.SetPropertyAtID(OPC_PROP_B_DESC, L"Operation to control boiling");
  Oper2.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)OPCB_PROC_OPERATION);
  Oper2.SetPropertyAtID(OPC_PROP_B_VERSION, L"2.25 dated 28-OCT-1999");
  Oper2.SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, L"UNIT11");
  Oper2.SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"IDLE");
  Oper2.SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD)OPCB_STATE_IDLE);
  Oper2.SetPropertyAtID(OPC_PROP_B_PARAMETER_COUNT, (DWORD)0);
  Oper2.SetPropertyAtID(OPC_PROP_B_RESULT_COUNT, (DWORD)0);

  Oper2.SetChildList((OPCBObj**)&Oper2_list);
  Oper2.SetParent(&UProc1);
  AddSampleBatchObject(&Oper2);

  Oper3.SetName(L"OPER3");
  Oper3.SetItemID(L"OPCBBatchModel.BATCH3.UPROC2.OPER3");
  Oper3.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Oper3.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBBatchModel.BATCH3.UPROC2.OPER3");
  Oper3.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Oper3.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Oper3.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Oper3.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH3.UPROC2.OPER3");
  Oper3.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBBatchModel.BATCH3.UPROC2.OPER3");
  Oper3.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Oper3.SetPropertyAtID(OPC_PROP_B_DESC, L"Operation to control blending");
  Oper3.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)OPCB_PROC_OPERATION);
  Oper3.SetPropertyAtID(OPC_PROP_B_VERSION, L"1.10 dated 28-OCT-1999");
  Oper3.SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, L"UNIT21");
  Oper3.SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"HOLDING");
  Oper3.SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD)OPCB_STATE_HOLDING);
  Oper3.SetPropertyAtID(OPC_PROP_B_PARAMETER_COUNT, (DWORD)0);
  Oper3.SetPropertyAtID(OPC_PROP_B_RESULT_COUNT, (DWORD)0);

  Oper3.SetChildList((OPCBObj**)&Oper3_list);
  Oper3.SetParent(&UProc2);
  AddSampleBatchObject(&Oper3);

  Phase1.SetName(L"PHASE1");
  Phase1.SetItemID(L"OPCBBatchModel.BATCH1.UPROC1.OPER1.PHASE1");
  Phase1.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Phase1.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBBatchModel.BATCH1.UPROC1.OPER1.PHASE1");
  Phase1.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Phase1.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Phase1.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Phase1.SetPropertyAtID(OPC_PROP_B_ID, L"PHASE1");
  Phase1.SetPropertyAtID(OPC_PROP_B_VALUE, L"");
  Phase1.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Phase1.SetPropertyAtID(OPC_PROP_B_DESC, L"Steam heating");
  Phase1.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)OPCB_PROC_PHASE);
  Phase1.SetPropertyAtID(OPC_PROP_B_VERSION, L"1.10 dated 28-OCT-1999");
  Phase1.SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, L"");
  Phase1.SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"RUNNING");
  Phase1.SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD)OPCB_STATE_RUNNING);
  Phase1.SetPropertyAtID(OPC_PROP_B_PARAMETER_COUNT, (DWORD)1);
  Phase1.SetPropertyAtID(OPC_PROP_B_RESULT_COUNT, (DWORD)0);

  Phase1.SetChildList((OPCBObj**)NULL);
  Phase1.SetParent(&Oper1);
  AddSampleBatchObject(&Phase1);

  Phase2.SetName(L"PHASE2");
  Phase2.SetItemID(L"OPCBBatchModel.BATCH1.UPROC1.OPER2.PHASE2");
  Phase2.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Phase2.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBBatchModel.BATCH1.UPROC1.OPER2.PHASE2");
  Phase2.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Phase2.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Phase2.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Phase2.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH1.UPROC1.OPER2.PHASE2");
  Phase2.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBBatchModel.BATCH1.UPROC1.OPER2.PHASE2");
  Phase2.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Phase2.SetPropertyAtID(OPC_PROP_B_DESC, L"Temperature control");
  Phase2.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)OPCB_PROC_PHASE);
  Phase2.SetPropertyAtID(OPC_PROP_B_VERSION, L"1.22 dated 28-OCT-1999");
  Phase2.SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, L"");
  Phase2.SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"IDLE");
  Phase2.SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD)OPCB_STATE_IDLE);
  Phase2.SetPropertyAtID(OPC_PROP_B_PARAMETER_COUNT, (DWORD)0);
  Phase2.SetPropertyAtID(OPC_PROP_B_RESULT_COUNT, (DWORD)0);

  Phase2.SetChildList((OPCBObj**)NULL);
  Phase2.SetParent(&Oper2);
  AddSampleBatchObject(&Phase2);

  Phase3.SetName(L"PHASE3");
  Phase3.SetItemID(L"OPCBBatchModel.BATCH3.UPROC2.OPER3.PHASE3");
  Phase3.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Phase3.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBBatchModel.BATCH3.UPROC2.OPER3.PHASE3");
  Phase3.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Phase3.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Phase3.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Phase3.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH3.UPROC2.OPER3.PHASE3");
  Phase3.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBBatchModel.BATCH3.UPROC2.OPER3.PHASE3");
  Phase3.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Phase3.SetPropertyAtID(OPC_PROP_B_DESC, L"Blend");
  Phase3.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)OPCB_PROC_PHASE);
  Phase3.SetPropertyAtID(OPC_PROP_B_VERSION, L"1.22 dated 28-OCT-1999");
  Phase3.SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, L"");
  Phase3.SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"HOLDING");
  Phase3.SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD)OPCB_STATE_HOLDING);
  Phase3.SetPropertyAtID(OPC_PROP_B_PARAMETER_COUNT, (DWORD)0);
  Phase3.SetPropertyAtID(OPC_PROP_B_RESULT_COUNT, (DWORD)0);

  Phase3.SetChildList((OPCBObj**)NULL);
  Phase3.SetParent(&Oper3);
  AddSampleBatchObject(&Phase3);

  Phase4.SetName(L"PHASE4");
  Phase4.SetItemID(L"OPCBBatchModel.BATCH2.PHASE4");
  Phase4.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Phase4.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBBatchModel.BATCH2.PHASE4");
  Phase4.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Phase4.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Phase4.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Phase4.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH2.PHASE4");
  Phase4.SetPropertyAtID(OPC_PROP_B_VALUE, L"OPCBBatchModel.BATCH2.PHASE4");
  Phase4.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Phase4.SetPropertyAtID(OPC_PROP_B_DESC, L"Clean");
  Phase4.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)OPCB_PROC_PHASE);
  Phase4.SetPropertyAtID(OPC_PROP_B_VERSION, L"2.24 dated 2-NOV-1999");
  Phase4.SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, L"");
  Phase4.SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"RETRYING");  // vendor-specific statename
  Phase4.SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD)OPCB_STATE_RESTARTING); // S88 statename
  Phase4.SetPropertyAtID(OPC_PROP_B_PARAMETER_COUNT, (DWORD)0);
  Phase4.SetPropertyAtID(OPC_PROP_B_RESULT_COUNT, (DWORD)0);

  Phase4.SetChildList((OPCBObj**)NULL);
  Phase4.SetParent(&Batch2);
  AddSampleBatchObject(&Phase4);
}

//XML: Set Batch List to a fixed size to allow more entries.
//OPCBBatchObj* BatchList[] = { &Batch1, &Batch2, &Batch3, NULL };
OPCBBatchObj* BatchList[MAX_BATCH_SIZE] = { &Batch1, NULL, NULL, NULL, NULL, NULL };

// Each batch has a single child that is the top-level
// recipe in the batch.
OPCBObj *Batch1_child_list[] = { &Proc1, NULL };
OPCBObj *Batch2_child_list[] = { &Phase4, NULL };
OPCBObj *Batch3_child_list[] = { &UProc2, NULL };

void CreateSampleBatchObjects()
{
  FILETIME ftNow;
  FILETIME ft1, ft2, ft3, ft4;

  Batch1.SetName(L"BATCH1");
  Batch1.SetItemID(L"OPCBBatchModel.BATCH1");
  Batch1.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Batch1.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBBatchModel.BATCH1");
  Batch1.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"

  // Set the timestamp property.  In a real server this would 
  // be refreshed whenever something changed in the batch, or
  // whenever the OPC Batch Server received an update.
  CoFileTimeNow ( &ftNow );      // this means 'server startup time')
  Batch1.SetPropertyAtID(OPC_PROP_time, ftNow);

  Batch1.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Batch1.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Batch1.SetPropertyAtID(OPC_PROP_DESC, L"Batch of vegetable soup");
  Batch1.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH1");
  Batch1.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Batch1.SetPropertyAtID(OPC_PROP_B_EU, L"gallons");
  Batch1.SetPropertyAtID(OPC_PROP_B_DESC, L"Batch of vegetable soup");

  LPCWSTR arrString[] = { L"UNIT11" }; 
  SAFEARRAY* pArr;
  CreateSafeArrayFromStrings(arrString, 1, pArr);
  Batch1.SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, pArr);

  
  Batch1.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)0);  //zero as spec requires for a batch
  Batch1.SetPropertyAtID(OPC_PROP_B_MASTER_RECIPE_ID, L"PROC1");
  Batch1.SetPropertyAtID(OPC_PROP_B_BATCH_SIZE, (float)200.0);
  Batch1.SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"RUNNING");
  Batch1.SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD)OPCB_STATE_RUNNING);
  Batch1.SetPropertyAtID(OPC_PROP_B_EXECUTION_MODE, L"AUTO");
  Batch1.SetPropertyAtID(OPC_PROP_B_IEC61512_1_MODE, (DWORD)OPCB_MODE_AUTOMATIC);
  Batch1.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_REFERENCE, L"PCELL1");
  
  CoFileTimeNow(&ftNow);
  ft1 = ftNow;
  ft1.dwHighDateTime -= 6;  // about 42 minutes ago
  Batch1.SetPropertyAtID(OPC_PROP_B_SCHEDULED_START_TIME, ft1);
  ft2 = ftNow;
  ft2.dwHighDateTime -= 4;  // about 28 minutes ago
  Batch1.SetPropertyAtID(OPC_PROP_B_ACTUAL_START_TIME, ft2);
  ft3 = ftNow;
  ft3.dwHighDateTime += 1;  // about 7 minutes in the future
  Batch1.SetPropertyAtID(OPC_PROP_B_ESTIMATED_END_TIME, ft3);
  
  // This sample server uses a special FILETIME value to
  // represent times in the future (that are not known yet).
  ft4.dwLowDateTime = 0;
  ft4.dwHighDateTime = 0;
  Batch1.SetPropertyAtID(OPC_PROP_B_ACTUAL_END_TIME, ft4);

  Batch1.SetChildList((OPCBObj**) Batch1_child_list);
  Batch1.SetParent(&OPCBBatchModel);
  AddSampleBatchObject(&Batch1);

  Batch2.SetName(L"BATCH2");
  Batch2.SetItemID(L"OPCBBatchModel.BATCH2");
  Batch2.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Batch2.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBBatchModel.BATCH2");
  Batch2.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Batch2.SetPropertyAtID(OPC_PROP_time, ftNow);
  Batch2.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Batch2.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Batch2.SetPropertyAtID(OPC_PROP_DESC, L"manual batch to clean up");
  Batch2.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH2");
  Batch2.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Batch2.SetPropertyAtID(OPC_PROP_B_EU, L"litres");
  Batch2.SetPropertyAtID(OPC_PROP_B_DESC, L"manual batch to clean up");

  LPCWSTR arrString2[] = { L"UNIT21" }; 
  SAFEARRAY* pArr2;
  CreateSafeArrayFromStrings(arrString2, 1, pArr2);
  Batch2.SetPropertyAtID(OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST, pArr2);


  Batch2.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)0);  //zero as spec requires for a batch
  Batch2.SetPropertyAtID(OPC_PROP_B_MASTER_RECIPE_ID, L"PHASE4");
  Batch2.SetPropertyAtID(OPC_PROP_B_BATCH_SIZE, (float)122.0);
  Batch2.SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"RESTARTING");
  Batch2.SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD)OPCB_STATE_RESTARTING);
  Batch2.SetPropertyAtID(OPC_PROP_B_EXECUTION_MODE, L"MANUAL");
  UProc1.SetPropertyAtID(OPC_PROP_B_IEC61512_1_MODE, (DWORD)OPCB_MODE_MANUAL);
  Batch2.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_REFERENCE, L"UNIT22");

  CoFileTimeNow(&ftNow);
  ft1 = ftNow;
  ft1.dwHighDateTime -= 2;  // about 14 minutes ago
  Batch2.SetPropertyAtID(OPC_PROP_B_SCHEDULED_START_TIME, ft1);
  ft2 = ftNow;
  ft2.dwHighDateTime += 1;  // about 7 minutes in the future
  Batch2.SetPropertyAtID(OPC_PROP_B_ACTUAL_START_TIME, ft2);
  ft3 = ftNow;
  ft3.dwHighDateTime += 5;  // about 35 minutes in the future
  Batch2.SetPropertyAtID(OPC_PROP_B_ESTIMATED_END_TIME, ft3);
  // This sample server uses a special FILETIME value to
  // represent times in the future (that are not known yet).
  ft4.dwLowDateTime = 0;
  ft4.dwHighDateTime = 0;
  Batch2.SetPropertyAtID(OPC_PROP_B_ACTUAL_END_TIME, ft4);

  Batch2.SetChildList((OPCBObj**) Batch2_child_list);
  Batch2.SetParent(&OPCBBatchModel);
  AddSampleBatchObject(&Batch2);

  Batch3.SetName(L"BATCH3");
  Batch3.SetItemID(L"OPCBBatchModel.BATCH3");
  Batch3.SetPropertyAtID(OPC_PROP_CDT, (short) VT_BSTR);
  Batch3.SetPropertyAtID(OPC_PROP_VALUE, L"OPCBBatchModel.BATCH3");
  Batch3.SetPropertyAtID(OPC_PROP_QUALITY, (short) 0xC0);  // "good"
  Batch3.SetPropertyAtID(OPC_PROP_time, ftNow);
  Batch3.SetPropertyAtID(OPC_PROP_RIGHTS, (DWORD) 0);
  Batch3.SetPropertyAtID(OPC_PROP_SCANRATE, 1.0);
  Batch3.SetPropertyAtID(OPC_PROP_DESC, L"Batch of tomato soup");
  Batch3.SetPropertyAtID(OPC_PROP_B_ID, L"OPCBBatchModel.BATCH3");
  Batch3.SetPropertyAtID(OPC_PROP_B_RIGHTS, (DWORD) 0);
  Batch3.SetPropertyAtID(OPC_PROP_B_EU, L"kg");
  Batch3.SetPropertyAtID(OPC_PROP_B_DESC, L"Batch of tomato soup");
  Batch3.SetPropertyAtID(OPC_PROP_B_BATCH_MODEL_LEVEL, (DWORD)0);  //zero as spec requires for a batch
  Batch3.SetPropertyAtID(OPC_PROP_B_MASTER_RECIPE_ID, L"UPROC2");
  Batch3.SetPropertyAtID(OPC_PROP_B_BATCH_SIZE, (float)150.0);
  Batch3.SetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, L"RUNNING");
  Batch3.SetPropertyAtID(OPC_PROP_B_IEC61512_1_STATE, (DWORD)OPCB_STATE_RUNNING);
  Batch3.SetPropertyAtID(OPC_PROP_B_EXECUTION_MODE, L"AUTO");
  Batch3.SetPropertyAtID(OPC_PROP_B_IEC61512_1_MODE, (DWORD)OPCB_MODE_AUTOMATIC);
  Batch3.SetPropertyAtID(OPC_PROP_B_PHYSICAL_MODEL_REFERENCE, L"PCELL2");

  CoFileTimeNow(&ftNow);
  ft1 = ftNow;
  ft1.dwHighDateTime -= 13;  // about 90 minutes ago
  Batch3.SetPropertyAtID(OPC_PROP_B_SCHEDULED_START_TIME, ft1);
  ft2 = ftNow;
  ft2.dwHighDateTime -= 12;  // about 84 minutes ago
  Batch3.SetPropertyAtID(OPC_PROP_B_ACTUAL_START_TIME, ft2);
  ft3 = ftNow;
  ft3.dwHighDateTime += 2;  // about 14 minutes in the future
  Batch3.SetPropertyAtID(OPC_PROP_B_ESTIMATED_END_TIME, ft3);
  // This sample server uses a special FILETIME value to
  // represent times in the future (that are not known yet).
  ft4.dwLowDateTime = 0;
  ft4.dwHighDateTime = 0;
  Batch3.SetPropertyAtID(OPC_PROP_B_ACTUAL_END_TIME, ft4);

  Batch3.SetChildList((OPCBObj**) Batch3_child_list);
  Batch3.SetParent(&OPCBBatchModel);
  AddSampleBatchObject(&Batch3);


}

// Create an array of archived batches.  These will be populated by
// data in the XML file.  If the server is not using XML, this
// will remain empty, unless someone adds some sample data to
// represent archived batches here.
OPCBBatchObj* BatchArchiveList[MAX_BATCH_SIZE] = { NULL, NULL, NULL, NULL, NULL, NULL };


void CreateSampleRootObjects()
{
  // Init the root object itself
  root_tag.SetParent(NULL);
  root_tag.SetChildList((OPCBObj**)&root_list);
  root_tag.SetName(L"<root>");
  root_tag.SetItemID(L"<root>");
  // no need to put this one in the list of all objects
  
  // Init the three batch-related objects that inhabit the root
  // of the name space.  
  OPCBPhysicalModel.SetName(L"OPCBPhysicalModel");
  OPCBPhysicalModel.SetItemID(L"OPCBPhysicalModel");
  OPCBPhysicalModel.SetChildList((OPCBObj**)&PhysModelRoot_list);
  OPCBPhysicalModel.SetParent(&root_tag);    // parent is the root
  AddSampleBatchObject(&OPCBPhysicalModel);

  OPCBBatchModel.SetName(L"OPCBBatchModel");
  OPCBBatchModel.SetItemID(L"OPCBBatchModel");
  OPCBBatchModel.SetChildList((OPCBObj**)&BatchList);
  OPCBBatchModel.SetParent(&root_tag);
  AddSampleBatchObject(&OPCBBatchModel);

  OPCBBatchIDList.SetName(L"OPCBBatchIDList");
  OPCBBatchIDList.SetItemID(L"OPCBBatchIDList");
  OPCBBatchIDList.SetPropertyAtID(OPC_PROP_CDT, (short)(VT_ARRAY | VT_BSTR));

  LPCWSTR arrString[]  = { L"BATCH1", L"BATCH2", L"BATCH3" };
  SAFEARRAY* pArr;
  CreateSafeArrayFromStrings(arrString, 3, pArr);
  OPCBBatchIDList.SetPropertyAtID(OPC_PROP_VALUE, pArr);

  OPCBBatchIDList.SetChildList(NULL);   // this never has children
  OPCBBatchIDList.SetParent(&root_tag);
  AddSampleBatchObject(&OPCBBatchIDList);
}
  