// BatchItemSupport.h
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
//  Will Irwin, Fisher-Rosemount Systems Inc., and
//  Dave Emerson, Yokogawa Corporation of America.
//  Gregory Khrapunovich, ABB Inc., implemented the GetAddressList
//  and related methods to add support for the OPC_FLAT option.
//  James Wang - Yokogawa Corporation of America. Updated to V2Draft3.
//
// CONTENTS:
//
//  This file defines classes and global functions to 
//  support the simulated data for the OPC Batch Sample server.

#ifndef __BATCHITEMSUPPORT_H
#define __BATCHITEMSUPPORT_H

#include "opcbc.h"  // need definition of OPCBATCHSUMMARYFILTER

// borrow technique from struct pidlist used in I_PRM.cpp
typedef struct property {
	DWORD	pid;
	VARTYPE	vt;
	LPWSTR	idsuffix;
	LPWSTR	desc;
} PROPERTY;

// Class OPCBObj
// This is an object that represents something 
// 'real' in a batch system, to provide some data 
// for the OPC Batch sample server.  In a real 
// server this might be replaced with some kind 
// of link to a real batch execution system.

// Members of this class and its derivatives
// are identified by an item name which is 
// a unique string in some server defined format.

// In this sample server, these objects are 
// arranged in a hierarchy as defined
// by S88.01.  For example, a OPCBPhysObj 
// representing a Process Cell might have an
// OPCBPhysObj representing an Area as its 
// parent, and several OPCBPhysObjs representing
// Units as its children.  The hierarchy thus 
// created may be browsed using the 
// IOPCBrowseServerAddressSpace interface.

#define MAX_PROPERTIES 65   // we won't actually define
                            // this many....

//V2Draft3: Changed max objects to 500 from 200
#define MAX_BATCH_OBJ 500

class OPCBObj
{
  public:
    OPCBObj ();   // constructor
    virtual ~OPCBObj();   // destructor

    virtual PROPERTY * getPropertyList () { return NULL; }
    virtual PROPERTY * findProperty ( DWORD id );

    // Allow for a number of these objects to be wired
    // together into a hierarchy.  A real server would 
    // get the corresponding information from a real
    // batch execution system. 
    void SetParent(OPCBObj* p) { m_parent = p; }
    OPCBObj* getParent() { return m_parent; }
    void SetChildList(OPCBObj** list) { m_childList = list; }
    OPCBObj** getChildList() { return m_childList; }

    // search my child list for a name
    OPCBObj* findChildNamed ( LPCWSTR pName );
    OPCBObj* findChildNumbered ( int index );
    void SetName(LPWSTR name)  { m_sName = name; }
    LPWSTR GetName() { return m_sName; }

    void SetItemID(LPWSTR itemID)  { m_sItem = itemID; }
    LPWSTR GetItemID() { return m_sItem; }

    // Copied from GetAddressList() function in I_BSAS.cpp
    // Returns a list of itemIDs of my child objects, 
    // filtered in various ways.
    void GetAddressList(
      OPCBROWSETYPE dwBrowseFilterType,
      LPCWSTR szFilterCriteria,
      VARTYPE vtDataTypeFilter,
      DWORD dwAccessRightsFilter,
      LPOLESTR **AddressList,
      int *AddressCount);

    void GetItemCount(
      OPCBROWSETYPE dwBrowseFilterType,
      int *iCount);

  	void GetIDList(
      OPCBROWSETYPE dwBrowseFilterType,
      LPOLESTR* ItemIDList,
	    int* piCurPos, 
      int iCount,
      LPOLESTR szParentName);

    WCHAR* BuildChildID(
	    const WCHAR* szParentName,
	    const WCHAR *szDelimiter,
	    const WCHAR *szChildName);


    // Accessors for the property values
    HRESULT GetPropertyAtID (DWORD id, VARIANT& v);

    HRESULT SetPropertyAtID (DWORD id, VARIANT& v);
    HRESULT SetPropertyAtID (DWORD id, short i2);
    HRESULT SetPropertyAtID (DWORD id, DWORD i4);
    HRESULT SetPropertyAtID (DWORD id, float r4);
    HRESULT SetPropertyAtID (DWORD id, double r8);
    HRESULT SetPropertyAtID (DWORD id, LPWSTR str);
    HRESULT SetPropertyAtID (DWORD id, bool b);
    HRESULT SetPropertyAtID (DWORD id, FILETIME ft);
    HRESULT SetPropertyAtID (DWORD id, SAFEARRAY* arr);

  private:
    OPCBObj* m_parent;

    // Would be cleaner to employ some kind of collection class
    // to store the child list, but in the interests of keeping
    // the server as generic as possible (i.e. free of dependency 
    // on MFC, STL etc.) we'll resort to some old-style C stuff.
	  OPCBObj** m_childList;

    LPWSTR m_sName;   // short name (e.g. "UNIT12"), may not be unique across the server
    LPWSTR m_sItem;   // full unique OPC item ID (e.g. OPCBPhysicalModel.AREA1.PCELL1.UNIT12)

    // The following two arrays are used to implement a simple
    // map of ID to value.  Again, this would be cleaner if we
    // used something like MFC's map classes.  Also it would be 
    // more space-efficient to dynamically size these arrays 
    // according to the number of properties each object had, 
    // but for the sample server we'll keep it simple.
    DWORD   m_dwPropIDs[MAX_PROPERTIES];
    VARIANT m_vPropValues[MAX_PROPERTIES];
};


// Various derived classes are used to represent the different
// kinds of object in the sample server address space.  The
// only differences between these classes is the list of 
// standard properties each type supports.....

/////////////////////////////////////////////////////////
// Class to represent a Batch
class OPCBBatchObj : public OPCBObj
{
  public:
    OPCBBatchObj();
    ~OPCBBatchObj() {};

    // All objects of a class have the same set of properties,
    // held in a static member variable.
    PROPERTY * getPropertyList () { return &M_PropsList[0]; }

    // Return whether this batch meets the criteria 
    // specified in a filter.
    bool PassesFilter(OPCBATCHSUMMARYFILTER* pFilter);

  private:
    // The list of properties is shared by all instances
    static PROPERTY M_PropsList[MAX_PROPERTIES];
};

////////////////////////////////////////////////////////////////
// Class to represent something in the S88 Physical hierarchy
// such as a Unit, Equipment Module etc.
class OPCBPhysObj : public OPCBObj
{
  public:
    OPCBPhysObj();
    ~OPCBPhysObj() {};

    // All objects of a class have the same set of properties,
    // held in a static member variable.
    PROPERTY * getPropertyList () { return &M_PropsList[0]; }

  private:
    // The list of properties is shared by all instances
    static PROPERTY M_PropsList[MAX_PROPERTIES];
};

////////////////////////////////////////////////////
//V2Draft3: Class to represent something in the S88 Master Recipe Model
// 
class OPCBMasterRecipeObj : public OPCBObj
{
  public:
    OPCBMasterRecipeObj();
    ~OPCBMasterRecipeObj() {};

    // All objects of a class have the same set of properties,
    // held in a static member variable.
    PROPERTY * getPropertyList () { return &M_PropsList[0]; }

  private:
    // The list of properties is shared by all instances
    static PROPERTY M_PropsList[MAX_PROPERTIES];
};

/////////////////////////////////////////////////////////////
// Class to represent something in the S88 Recipe hierarchy
// such as a Unit Procedure, Operation etc.
class OPCBRPEObj : public OPCBObj
{
  public:
    OPCBRPEObj();
    ~OPCBRPEObj() {};

    // All objects of a class have the same set of properties,
    // held in a static member variable.
    PROPERTY * getPropertyList () { return &M_PropsList[0]; }

  private:
    // The list of properties is shared by all instances
    static PROPERTY M_PropsList[MAX_PROPERTIES];
};

/////////////////////////////////////////////////////////////
// Class to represent a Parameter
class OPCBParamObj : public OPCBObj
{
  public:
    OPCBParamObj();
    ~OPCBParamObj() {};

    // All objects of a class have the same set of properties,
    // held in a static member variable.
    PROPERTY * getPropertyList () { return &M_PropsList[0]; }

  private:
    // The list of properties is shared by all instances
    static PROPERTY M_PropsList[MAX_PROPERTIES];
};

/////////////////////////////////////////////////////////////
// Class to represent a Result
class OPCBResultObj : public OPCBObj
{
  public:
    OPCBResultObj();
    ~OPCBResultObj() {};

    // All objects of a class have the same set of properties,
    // held in a static member variable.
    PROPERTY * getPropertyList () { return &M_PropsList[0]; }

  private:
    // The list of properties is shared by all instances
    static PROPERTY M_PropsList[MAX_PROPERTIES];
};


/////////////////////////////////////////////////////////////
// Class to represent the special object for the item
// OPCBBatchIDList.  There will only be one instance
// of this per server.  It has its own class because 
// it has a different set of properties from the other
// objects.
class OPCBBatchIDListObj : public OPCBObj
{
  public:
    OPCBBatchIDListObj();
    ~OPCBBatchIDListObj() {};

    // All objects of a class have the same set of properties,
    // held in a static member variable.
    PROPERTY * getPropertyList () { return &M_PropsList[0]; }

  private:
    // The list of properties is shared by all instances
    static PROPERTY M_PropsList[MAX_PROPERTIES];
};


// Method to create a number of sample objects for the server
// to work with.
void CreateSampleObjects();
void LoadSampleObjects();
void RemoveSampleObjects();

// Method to add a single batch object
void AddSampleBatchObject ( OPCBObj* pOb );

// Method to look up an object in the sample server's
// set of batch objects.
OPCBObj* FindSampleBatchObjectByItemID ( LPCWSTR pszItem );

// Method to convert an item ID using the special 
// shortcut syntax for parameters/results into the
// full format.
void ConvertShortcutItemID ( LPWSTR pszDest, LPCWSTR pszSrc );

// Check if a string looks like an OPC batch itemID
bool isBatchItemID ( LPWSTR itemID );

// Break an item ID up into the branch and the suffix
// and find the object represented by branchID
OPCBObj* ParseItemID ( LPWSTR itemID, LPWSTR branchID, LPWSTR propertySuffix, DWORD& dwPropID );

// Create a SAFEARRY from an array of strings
HRESULT CreateSafeArrayFromStrings(LPCWSTR* pStrings, long count, SAFEARRAY*& arr);


#endif // __BATCHITEMSUPPORT_H