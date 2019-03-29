// OPCBXMLData.h: interface for the OPCBXMLData class.
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
//  Dave Emerson  -  Yokogawa Corporation of America
//  James Wang  -  Yokogawa Corporation of America: Added XML file interface and 
//                 updated to V2Draft3
//
// CONTENTS:
//
//  This file contains the definition of data structure to create sample 
//  data from XML format file to the OPC Batch Sample server.
//
///////////////////////////////////////////////////////////////////////////

#if !defined(AFX_OPCBXMLDATA_H__D2906A11_5284_11D4_920A_00E029591900__INCLUDED_)
#define AFX_OPCBXMLDATA_H__D2906A11_5284_11D4_920A_00E029591900__INCLUDED_

#include "enumsamp.h"	// Added by ClassView
#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

// msxml reference
#import "msxml.dll" named_guids raw_interfaces_only no_namespace \
	rename( "XMLELEM_TYPE", "IE5XMLELEM_TYPE" ) \
	rename( "XMLDocument", "IE5XMLDocument" ) \
	rename( "IXMLElementCollection", "IE5IXMLElementCollection" ) \
	rename( "IXMLDocument", "IE5IXMLDocument" ) \
	rename( "IXMLElement", "IE5IXMLElement" ) \
	rename( "IXMLError", "IE5IXMLError" ) \
	rename( "_xml_error", "IE5_xml_error" ) \
	rename( "XMLELEMTYPE_ELEMENT", "IE5XMLELEMTYPE_ELEMENT" ) \
	rename( "XMLELEMTYPE_TEXT", "IE5XMLELEMTYPE_TEXT" ) \
	rename( "XMLELEMTYPE_COMMENT", "IE5XMLELEMTYPE_COMMENT" ) \
	rename( "XMLELEMTYPE_DTD", "IE5XMLELEMTYPE_DTD" ) \
	rename( "XMLELEMTYPE_PI", "IE5XMLELEMTYPE_PI" ) \
	rename( "XMLELEMTYPE_OTHER", "IE5XMLELEMTYPE_OTHER" ) \
	rename( "XMLELEMTYPE_DOCUMENT", "IE5XMLELEMTYPE_DOCUMENT" )

#define CHECKHR(x) {hr = x; if (FAILED(hr)) goto CleanUp;}
#define SAFERELEASE(p) {if (p) {(p)->Release(); p = NULL;}}
#define SAFEDELETE(p) {if (p) {delete p; p = NULL;}}
#define SAFEDELETEARRAY(p) {if (p) {delete [] p; p = NULL;}}
#define MAX_CURRENT_OBJECT 20
#define MAX_OBJECT 200

using namespace std;
class OPCBObj;
class EnumerationSet;

class OPCBXMLData  
{
	struct _ObjRelation
	{
		OPCBObj*	pObj;
		int		nParent;
		int		nChild[MAX_CURRENT_OBJECT];
		int		nChildCount;
	};
   typedef struct _ObjRelation ObjRelation;

	enum WellKnownItem{ nPhysicalModel, nBatchModel, nBatchIDList, nEnumSet, nServer,
		nMasterRecipeModel, nBatchArchiveModel };

public:
	HRESULT LoadXMLFile(char* strFileName);
	int LoadPropIDTable(char* FileName);
	OPCBXMLData();
	virtual ~OPCBXMLData();

	class IDValue
	{
	public:
		int Value;
		IDValue(): Value(0){}
		IDValue operator=(int a)
		{
			Value = a;
			return(*this);
		}
	};

	map<string, IDValue > m_IDTable;
	map<string, IDValue >::iterator m_iter;

private:
	int m_nListID;
	HRESULT SetPropertyArray( OPCBObj* pObj, int nPid, WCHAR** List, int size);
	HRESULT SetDateProperty( OPCBObj* pObj, int Pid, char* strDate);
	long Hex2Long( char* string );
	int m_nEnumSet;

	BSTR m_bstrEnumString;
	int m_nEnum;
	EnumerationSet* m_pCurrentEnumerationSet;
	int m_nBatchObjIndex;
  int m_nBatchArchiveObjIndex;
	void LoadObjectList();
	void InitObjectList();
	void CreateRootObjects();
	HRESULT CreateDataModel();
	HRESULT AddObject( OPCBObj* obj, int level );
	int m_nBatchIDCount;
	int m_nPropertyID;
	
	WellKnownItem	m_nWellKnownItem;
	int				m_nCurrentLevel;
	int				m_CurrentObj[MAX_CURRENT_OBJECT];
	int				m_nObjIndex;
	ObjRelation		m_Obj[MAX_OBJECT];
	LPWSTR			m_BatchIDList[MAX_OBJECT];
	LPWSTR			m_List[MAX_CURRENT_OBJECT];
	int				m_nListItemIndex;

	OPCBObj*			m_pCurrentObject;

	TCHAR* BSTRtoAscii(BSTR bstr);
	HRESULT ProcessXMLNodeName(int level, BSTR nodeName);
	HRESULT ProcessXMLNodeValue(int level, BSTR nodeValue);
	HRESULT DOMWalkTree(IXMLDOMNode *node, int level);
	HRESULT ReportError(IXMLDOMParseError *pXMLError);
	HRESULT CheckLoad(IXMLDOMDocument *pDoc);
	BSTR AsciiToBSTR(const TCHAR *pszFName);
	IXMLDOMDocument * m_pIXMLDocument;
};

#endif // !defined(AFX_OPCBXMLDATA_H__D2906A11_5284_11D4_920A_00E029591900__INCLUDED_)
