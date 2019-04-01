// OPCBXMLData.cpp: implementation of the OPCBXMLData class.
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
//  James Wang - Yokogawa Corporation of America: Added XML file interface and 
//               updated to V2Draft3.
//
// CONTENTS:
//
//  This file contains the code to create sample data from XML format file
//  for the OPC Batch Sample server.
//
///////////////////////////////////////////////////////////////////////////
#pragma warning( disable : 4786 )

#include "windows.h"
#include <winuser.h>
#include <map>
#include <fstream>
#include <iostream>
#include <string>
#include <tchar.h>
#include "OPCBXMLData.h"

#include "OPCXXX.h"
#include "OPCProps.h"
#include "OPCBatchProps.h"
#include "OPCBatchDef.h"

#define MAX_INPUTTEXT 1024

//////////////////////////////////////////////////////////////////////
// Global definitions to server data access.
//////////////////////////////////////////////////////////////////////
extern OPCBObj root_tag;
extern OPCBObj* root_list[];
extern OPCBPhysObj OPCBPhysicalModel;
extern OPCBMasterRecipeObj OPCBMasterRecipeModel;
extern OPCBBatchObj OPCBBatchModel;
extern OPCBBatchObj OPCBBatchArchiveModel;
extern OPCBBatchIDListObj OPCBBatchIDList;
extern OPCBObj *PhysModelRoot_list[];
extern OPCBBatchObj* BatchList[];
extern OPCBBatchObj* BatchArchiveList[];
extern IMalloc		*pIMalloc;		// Common memory allocator
extern EnumerationSet* allEnumSets[];

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////
extern HWND serverWindow;

OPCBXMLData::OPCBXMLData()
{	
	m_nCurrentLevel = 0;
	m_nObjIndex = 0;
	m_nBatchObjIndex = 0;
  m_nBatchArchiveObjIndex = 0;
	m_nListItemIndex = 0;

	m_pIXMLDocument = NULL;
	
}

OPCBXMLData::~OPCBXMLData()
{
}

int OPCBXMLData::LoadPropIDTable(char *FileName)
{
	if( FileName[0] == '\0' )
		FileName = "c:\\PropertyIDTable.dat";

	ifstream dataFile( FileName, ios::in);

	char		name[64], dummy[64];
	int		value;

	if( !dataFile )
	{
		MessageBox(NULL, "Couldn't Load the Property ID cross reference table.", NULL, MB_OK);
		return -1;
	}

	while((dataFile >> name >> value >> dummy))
	{
		m_IDTable[name] = value;
	}

	for( m_iter = m_IDTable.begin(); m_iter != m_IDTable.end(); m_iter++ )
	{
		char msg[128];
		sprintf( msg, "%s: %d", ((*m_iter).first).c_str(), (*m_iter).second );
		//MessageBox( NULL, msg, "Test", MB_OK );
	}

	return 1;
}


HRESULT OPCBXMLData::LoadXMLFile(char *strFileName)
{
	BSTR	pBURL;
	IXMLDOMDocument*	pDoc = NULL;
	IXMLDOMNode*		pNode = NULL;
	IXMLDOMParseError*	pXMLError = NULL;
	
	VARIANT         vURL;
	VARIANT_BOOL    vb;
	HRESULT         hr;
		
	pBURL = AsciiToBSTR( strFileName );

	SAFERELEASE( m_pIXMLDocument );

	// Create an empty XML document	
	CoCreateInstance(CLSID_DOMDocument, NULL, CLSCTX_INPROC_SERVER,
		IID_IXMLDOMDocument, (void**)&m_pIXMLDocument);

	pDoc = m_pIXMLDocument;	
	CHECKHR(pDoc->put_async(VARIANT_FALSE));
	
	// Load xml document from the given URL or file path
	VariantInit(&vURL);
	vURL.vt = VT_BSTR;
	V_BSTR(&vURL) = pBURL;
	pDoc->load(vURL, &vb);
	
	CHECKHR(CheckLoad(pDoc));
	
	//Now walk the loaded XML document.
	CHECKHR(pDoc->QueryInterface(IID_IXMLDOMNode,(void**)&pNode));

	CHECKHR(DOMWalkTree( pNode, 0 ) );
	hr = CreateDataModel();


CleanUp:
	if( FAILED(hr) )
	{
		MessageBox( NULL, "Load XML Data file failed", "Test", MB_OK );
	}
	SysFreeString(pBURL);

	SAFERELEASE(pNode);
	  
	return hr;
}

HRESULT OPCBXMLData::CheckLoad(IXMLDOMDocument *pDoc)
{
	// Since we don't have the VARIANT_BOOL from load we
	// need to check the parse Error errorCode property to see
	// if everything went ok.
	IXMLDOMParseError  *pXMLError = NULL;
	LONG errorCode = E_FAIL;
	HRESULT hr;
	
	CHECKHR(pDoc->get_parseError(&pXMLError));
	CHECKHR(pXMLError->get_errorCode(&errorCode));
	if (errorCode != 0)
	{
		hr = ReportError(pXMLError);
		goto CleanUp;
	}
	else
	{
		//MessageBox( NULL, "XML document loaded successfully", "Status", MB_OK );
	}
CleanUp:
	SAFERELEASE(pXMLError);
	return errorCode;
}

HRESULT OPCBXMLData::ReportError(IXMLDOMParseError *pXMLError)
{
	long line, linePos;
	LONG errorCode;
	BSTR pBURL, pBReason;
	HRESULT hr;
	
	CHECKHR(pXMLError->get_line(&line));
	CHECKHR(pXMLError->get_linepos(&linePos));
	CHECKHR(pXMLError->get_errorCode(&errorCode));
	CHECKHR(pXMLError->get_url(&pBURL));
	CHECKHR(pXMLError->get_reason(&pBReason));
	
	if (line > 0)
	{
		char strMsg[128];
      sprintf( strMsg, "ope_exe: XML Load Error:%S\n on line %d, position %d in \"%S\".\n", 
			pBReason, line, linePos, pBURL);
		MessageBox( NULL, strMsg, "XML Error", MB_OK );
	}
	
CleanUp:
	
	SysFreeString(pBURL);
	SysFreeString(pBReason);
	
	return E_FAIL;
}

HRESULT OPCBXMLData::DOMWalkTree(IXMLDOMNode *node, int level)
{
	IXMLDOMNode* pChild, *pNext;
	BSTR nodeName;
	BSTR nodeValue;
	IXMLDOMNamedNodeMap* pattrs;
	DOMNodeType	nodeType;
	
	node->get_nodeName(&nodeName);

	if( SUCCEEDED( node->get_nodeType( &nodeType ) ) )
	{
		if( nodeType == NODE_TEXT )
		{
			VARIANT value;
			node->get_nodeValue(&value);
			if (value.vt == VT_BSTR)
			{
				nodeValue = V_BSTR(&value);
				if( FAILED( ProcessXMLNodeValue( level, nodeValue ) ) )
					return E_FAIL;		
			}
			VariantClear(&value);
		}
		else
		{
			//TRACE("Name[%d] %S", level, nodeName);
			//m_pCurrentNode = node;
			ProcessXMLNodeName( level, nodeName );
		}
	}
	SysFreeString(nodeName);
	
	if (SUCCEEDED(node->get_attributes(&pattrs)) && pattrs != NULL)
	{
		pattrs->nextNode(&pChild);
		while (pChild)
		{
			BSTR name;
			pChild->get_nodeName(&name);
			//TRACE(" Attribute[%d] %S='", level, name);
			SysFreeString(name);
			
			VARIANT value;
			pChild->get_nodeValue(&value);
			if (value.vt == VT_BSTR)
			{
				//TRACE("%S", V_BSTR(&value));
			}
			//TRACE("'\n");
			VariantClear(&value);
			pChild->Release();
			pattrs->nextNode(&pChild);
		}
		
		pattrs->Release();
	}
	
	node->get_firstChild(&pChild);
	while (pChild)
	{
		if( FAILED(DOMWalkTree(pChild, level+1) ) )
			return E_FAIL;
		pChild->get_nextSibling(&pNext);
		pChild->Release();
		pChild = pNext;
	}
	
	return S_OK;
}

HRESULT OPCBXMLData::ProcessXMLNodeValue(int level, BSTR nodeValue)
{
	TCHAR *pValue;
	BSTR bstrValue;

	pValue = BSTRtoAscii( nodeValue );

	if( m_nPropertyID <= 0 )
		return S_OK;
	
	bstrValue = SysAllocString(nodeValue);
	if( m_nWellKnownItem == nEnumSet )
	{
		switch( m_nPropertyID )
		{
		case 9006: //EnumerationSetNumber
			m_nEnum = atoi( pValue );
			break;
		case 9008: //EnumerationSetShortString
			m_bstrEnumString = bstrValue;
			break;
		case 9009: //EnumerationSetSize
			m_pCurrentEnumerationSet = 
				new EnumerationSet ( m_bstrEnumString, m_nEnum, atoi( pValue ) );
			allEnumSets[m_nEnumSet - 1] = m_pCurrentEnumerationSet;
			break;
		case 9003: //EnumerationValue
			m_nEnum = atoi( pValue );
			break;
		case 9005: //EnumerationShortString
			m_pCurrentEnumerationSet->AddEnum( m_nEnum, bstrValue );
			break;
		}
	}
	else
	{
		DWORD dwValue;

		switch( m_nPropertyID )
		{
		case 9000://Item Name
			m_pCurrentObject->SetName( bstrValue );
			break;
		case 9001://Item ID
			m_pCurrentObject->SetItemID( bstrValue );
			break;
		case 9002://Batch ID
			break;
		case 9010: //ListItem
			m_List[m_nListItemIndex++] = bstrValue;
			break;
		case OPC_PROP_CDT://DataType
			if( _tcscmp( pValue, _T("BSTR") ) == 0 )
				m_pCurrentObject->SetPropertyAtID( m_nPropertyID, (short) VT_BSTR );
			else
				m_pCurrentObject->SetPropertyAtID( m_nPropertyID, (short) VT_I4 );
			break;
		case OPC_PROP_QUALITY:
			short nQuality;
			nQuality = (short) m_IDTable[pValue].Value;
			m_pCurrentObject->SetPropertyAtID( m_nPropertyID, nQuality );
			break;
		//Property type as VT_I4
		case OPC_PROP_B_RIGHTS:
		case OPC_PROP_B_TIME_ZONE:
		case OPC_PROP_B_MAXIMUM_USER_COUNT:
		case OPC_PROP_B_CURRENT_USER_COUNT:
		case OPC_PROP_B_PRIORITY:
		case OPC_PROP_B_IEC61512_1_MODE:
		case OPC_PROP_B_IEC61512_1_STATE:
		case OPC_PROP_B_PARAMETER_TYPE:
		case OPC_PROP_B_PARAMETER_COUNT:
		case OPC_PROP_B_RESULT_COUNT:
			dwValue = (DWORD) atol(pValue);
			m_pCurrentObject->SetPropertyAtID( m_nPropertyID, dwValue );
			break;
		case OPC_PROP_B_ENUMERATION_SET_ID:
			dwValue = (DWORD)m_IDTable[pValue].Value;
			m_pCurrentObject->SetPropertyAtID( m_nPropertyID, dwValue );
			break;
		case OPC_PROP_B_PHYSICAL_MODEL_LEVEL:
		case OPC_PROP_B_BATCH_MODEL_LEVEL:
			dwValue = (DWORD)m_IDTable[pValue].Value;
			m_pCurrentObject->SetPropertyAtID( m_nPropertyID, dwValue );
			// Check "BATCH" (numeric value 8) instead of "0".
			//if( _tcscmp( pValue, _T("0") ) == 0 )
			if( dwValue == OPCB_PROC_BATCH )
      {
        // We come in here once for each batch defined in the XML.  
        // Determine whether it is a batch from the OPCBBatchModel or 
        // from OPCBArchivedBatchModel and store the current object 
        // pointer to the appropriate place.
        LPWSTR itemID = m_pCurrentObject->GetItemID();
        if (wcsncmp(itemID, L"OPCBBatchModel", wcslen(L"OPCBBatchModel")) == 0)
        {
  				BatchList[m_nBatchObjIndex++] = (OPCBBatchObj*) m_pCurrentObject;
        }
        else if (wcsncmp(itemID, L"OPCBBatchArchiveModel", wcslen(L"OPCBBatchArchiveModel")) == 0)
        {
          BatchArchiveList[m_nBatchArchiveObjIndex++] = (OPCBBatchObj*) m_pCurrentObject;
        }
      }
			break;
		case OPC_PROP_B_MASTER_RECIPE_MODEL_LEVEL:
			dwValue = (DWORD)m_IDTable[pValue].Value;
			m_pCurrentObject->SetPropertyAtID( m_nPropertyID, dwValue );
			break;
		//Property type as VT_DATE
		case OPC_PROP_B_SCHEDULED_START_TIME:
		case OPC_PROP_B_ACTUAL_START_TIME:
		case OPC_PROP_B_ESTIMATED_END_TIME:
		case OPC_PROP_B_ACTUAL_END_TIME:
			SetDateProperty( m_pCurrentObject, m_nPropertyID, pValue );
			break;
		//Property type as VT_R4
		case OPC_PROP_B_BATCH_SIZE:
		case OPC_PROP_B_HIGH_VALUE_LIMIT:
		case OPC_PROP_B_LOW_VALUE_LIMIT:
			m_pCurrentObject->SetPropertyAtID( m_nPropertyID, atof( pValue ) );
			break;
		//Property type as VT_BSTR
		default:
			m_pCurrentObject->SetPropertyAtID( m_nPropertyID, bstrValue );
			break;
		}
	}
	if( m_nPropertyID != 9000 && m_nPropertyID != 9001 && m_nPropertyID != 9010 &&
		m_nPropertyID != 9008 )
		SysFreeString( bstrValue );

	return S_OK;
}


HRESULT OPCBXMLData::ProcessXMLNodeName(int level, BSTR nodeName)
{
	TCHAR *pName;

	pName = BSTRtoAscii( nodeName );
	m_nPropertyID = m_IDTable[pName].Value;

	if( _tcscmp( pName, _T("OPCBPhysicalModel") ) == 0 )
	{
		InitObjectList();
		m_nWellKnownItem = nPhysicalModel;
		m_pCurrentObject = &OPCBPhysicalModel;
		AddObject( m_pCurrentObject, level );
	}
	else if( _tcscmp( pName, _T("OPCBBatchModel") ) == 0 )
	{
		//Load Physical Model to database.
		LoadObjectList();
		InitObjectList();
		m_nWellKnownItem = nBatchModel;
		m_pCurrentObject = &OPCBBatchModel;
		AddObject( m_pCurrentObject, level );

	}
	//V2Draft3: Adding MasterRecipe, BatchRecipe
	else if( _tcscmp( pName, _T("OPCBMasterRecipeModel") ) == 0 )
	{
		//Load MasterRecipe Model to database.
		LoadObjectList();
		InitObjectList();
		m_nWellKnownItem = nMasterRecipeModel;
		m_pCurrentObject = &OPCBMasterRecipeModel;
		AddObject( m_pCurrentObject, level );
	}	
	else if( _tcscmp( pName, _T("OPCBBatchArchiveModel") ) == 0 )
	{
		//Load Physical Model to database.
		LoadObjectList();
		InitObjectList();
		m_nWellKnownItem = nBatchArchiveModel;
		m_pCurrentObject = &OPCBBatchArchiveModel;
		AddObject( m_pCurrentObject, level );
	}
	else if( _tcscmp( pName, _T("OPCBBatchIDList") ) == 0 )
	{
		m_nWellKnownItem = nBatchIDList;
		m_pCurrentObject = &OPCBBatchIDList;
	}
	else if( _tcscmp( pName, _T("IOPCEnumerationSets") ) == 0 )
	{
		m_nWellKnownItem = nEnumSet;
		m_nEnumSet = 0;
	}
	else
	{
		switch( m_nWellKnownItem )
		{
		case nPhysicalModel:
			if( _tcscmp( pName, _T("PhysicalModelItem") ) == 0 )
			{
				//New object created.
				m_pCurrentObject = new OPCBPhysObj;
				if( m_pCurrentObject == NULL ) 
				{
					return E_OUTOFMEMORY;
				}
				AddObject( m_pCurrentObject, level );
			}
			else if( _tcscmp( pName, _T("Property") ) == 0 )
			{
				
			}
			break;
		case nBatchModel:
			if( _tcscmp( pName, _T("BatchModelItem") ) == 0 ||
				_tcscmp( pName, _T("OPCBParameters") ) == 0 ||
				_tcscmp( pName, _T("ParameterItem") ) == 0 ||
				_tcscmp( pName, _T("OPCBResults") ) == 0 ||
				_tcscmp( pName, _T("ResultItem") ) == 0 ) 
			{
				//New object created.
				m_pCurrentObject = new OPCBBatchObj;
				if( m_pCurrentObject == NULL ) 
				{
					return E_OUTOFMEMORY;
				}
				AddObject( m_pCurrentObject, level );
			}
			else if( _tcscmp( pName, _T("Property") ) == 0 )
			{
				
			}
			break;
		case nBatchArchiveModel:
			if( _tcscmp( pName, _T("BatchArchiveModelItem") ) == 0 ) 
			{
				//New object created.
				m_pCurrentObject = new OPCBBatchObj;
				if( m_pCurrentObject == NULL ) 
				{
					return E_OUTOFMEMORY;
				}
				AddObject( m_pCurrentObject, level );
			}
			else if( _tcscmp( pName, _T("Property") ) == 0 )
			{
				
			}
			break;
		case nMasterRecipeModel:
			if( _tcscmp( pName, _T("MasterRecipeModelItem") ) == 0 )
			{
				//New object created.
				m_pCurrentObject = new OPCBMasterRecipeObj;
				if( m_pCurrentObject == NULL ) 
				{
					return E_OUTOFMEMORY;
				}
				AddObject( m_pCurrentObject, level );
			}
			else if( _tcscmp( pName, _T("Property") ) == 0 )
			{
				
			}
			break;
		case nEnumSet:
			if( _tcscmp( pName, _T("EnumerationSet") ) == 0 )
			{
				m_nEnumSet++;
			}
			if( _tcscmp( pName, _T("Enumeration") ) == 0 )
			{
				m_nEnum++;
			}
			break;
		}

		if( m_nWellKnownItem == nPhysicalModel || m_nWellKnownItem == nBatchModel 
			|| m_nWellKnownItem == nMasterRecipeModel || m_nWellKnownItem == nBatchArchiveModel)
		{
			switch( m_nPropertyID )
			{
			case OPC_PROP_B_RELATED_BATCH_IDS:
			case OPC_PROP_B_CURRENT_USER_LIST:
			case OPC_PROP_B_ALLOCATED_EQUIPMENT_LIST:
			case OPC_PROP_B_REQUESTER_LIST: 
			case OPC_PROP_B_REQUESTED_LIST: 
			case OPC_PROP_B_SHARED_BY_LIST:
			case OPC_PROP_B_UPSTREAM_EQUIPMENT_LIST:
			case OPC_PROP_B_DOWNSTREAM_EQUIPMENT_LIST:
			case OPC_PROP_B_EQUIPMENT_PROCEDURAL_ELEMENT_LIST:
			case OPC_PROP_B_CURRENT_PROCEDURE_LIST:
			case OPC_PROP_B_TRAIN_LIST:
			case OPC_PROP_B_LOT_ID_LIST:
			case OPC_PROP_B_VALID_VALUES:
			case OPC_PROP_B_EQUIPMENT_CANDIDATE_LIST:
			case OPC_PROP_B_EQUIPMENT_CLASS_CANDIDATE_LIST:
				if( m_nListID != m_nPropertyID )
				{
					if( m_nListItemIndex > 0 )
						SetPropertyArray( m_pCurrentObject, m_nListID, (WCHAR**)m_List, 
							m_nListItemIndex );
					m_nListID = m_nPropertyID;
				}
				m_nListItemIndex = 0;
				break;
			case 9010: //ListItem - todo nothing.
				break;
			default:
				if( m_nListID )
				{
					if( m_nListItemIndex > 0 )
						SetPropertyArray( m_pCurrentObject, m_nListID, (WCHAR**)m_List, 
							m_nListItemIndex );
					m_nListID = 0;
					m_nListItemIndex = 0;
				}
				break;
			}
		}
	}

	return S_OK;
}

BSTR OPCBXMLData::AsciiToBSTR(const TCHAR *pszFName)
{
	WCHAR wszURL[MAX_PATH];
	MultiByteToWideChar(CP_ACP, 0, pszFName, -1, wszURL, MAX_PATH);
	return SysAllocString(wszURL);
}

static char input_text[MAX_INPUTTEXT];

char* OPCBXMLData::BSTRtoAscii(BSTR bstr)
{
   int default_used = FALSE;

   input_text[0] = NULL;

   WideCharToMultiByte(CP_ACP,0,bstr,-1,
                       input_text,MAX_INPUTTEXT,NULL,&default_used);

   return input_text;
}

//////////////////////////////////////////////////////////////
// Add an OPCB Object to elation list.
//////////////////////////////////////////////////////////////
HRESULT OPCBXMLData::AddObject(OPCBObj *p_obj, int level)
{
	int nChildCount;

	//Assign current object list.
	m_CurrentObj[level] = m_nObjIndex;
	m_Obj[m_nObjIndex].pObj = p_obj;

	if( level > m_nCurrentLevel )
	{
		if( m_nCurrentLevel > 0 )
		{
			//Parent
			m_Obj[m_nObjIndex].nParent = m_CurrentObj[m_nCurrentLevel];
			//Child
			nChildCount = m_Obj[m_CurrentObj[m_nCurrentLevel]].nChildCount;
			if( nChildCount >= 0 )
				m_Obj[m_CurrentObj[m_nCurrentLevel]].nChild[nChildCount] = m_nObjIndex;
			m_Obj[m_CurrentObj[m_nCurrentLevel]].nChildCount++;
		}
	}
	else
	{
		//Only parent.
		m_Obj[m_nObjIndex].nParent = m_CurrentObj[level-1];
		nChildCount = m_Obj[m_CurrentObj[level-1]].nChildCount;
		if( nChildCount >= 0 )
				m_Obj[m_CurrentObj[level-1]].nChild[nChildCount] = m_nObjIndex;
		m_Obj[m_CurrentObj[level-1]].nChildCount++;
	}
	m_nCurrentLevel = level;
	m_nObjIndex++;

	return S_OK;
}

HRESULT OPCBXMLData::CreateDataModel()
{
	LoadObjectList();

	CreateRootObjects();

	return S_OK;
}

void OPCBXMLData::CreateRootObjects()
{
  // Init the root object itself
  root_tag.SetParent(NULL);
  root_tag.SetChildList((OPCBObj**)&root_list);
  root_tag.SetName(L"<root>");
  root_tag.SetItemID(L"<root>");
  // no need to put this one in the list of all objects
  
  // Init the three + 2 new batch-related objects that inhabit the root
  // of the name space.  
  OPCBPhysicalModel.SetName(L"OPCBPhysicalModel");
  OPCBPhysicalModel.SetItemID(L"OPCBPhysicalModel");
  //OPCBPhysicalModel.SetChildList((OPCBObj**)&PhysModelRoot_list);
  OPCBPhysicalModel.SetParent(&root_tag);    // parent is the root
  AddSampleBatchObject(&OPCBPhysicalModel);

  //V2Draft3: OPCBMasterRecipeModel
  OPCBMasterRecipeModel.SetName(L"OPCBMasterRecipeModel");
  OPCBMasterRecipeModel.SetItemID(L"OPCBMasterRecipeModel");
  OPCBMasterRecipeModel.SetParent(&root_tag);
  AddSampleBatchObject(&OPCBMasterRecipeModel);

  OPCBBatchModel.SetName(L"OPCBBatchModel");
  OPCBBatchModel.SetItemID(L"OPCBBatchModel");
  //OPCBBatchModel.SetChildList((OPCBObj**)&BatchList);
  OPCBBatchModel.SetParent(&root_tag);
  AddSampleBatchObject(&OPCBBatchModel);

  //V2Draft3: OPCBBatchArchiveModel
  OPCBBatchArchiveModel.SetName(L"OPCBBatchArchiveModel");
  OPCBBatchArchiveModel.SetItemID(L"OPCBBatchArchiveModel");
  OPCBBatchArchiveModel.SetParent(&root_tag);
  AddSampleBatchObject(&OPCBBatchArchiveModel);

  OPCBBatchIDList.SetName(L"OPCBBatchIDList");
  OPCBBatchIDList.SetItemID(L"OPCBBatchIDList");
  OPCBBatchIDList.SetPropertyAtID(OPC_PROP_CDT, (short)(VT_ARRAY | VT_BSTR));

  //LPCWSTR arrString[]  = { L"BATCH1", L"BATCH2", L"BATCH3", L"BATCH4", L"BATCH5" };

  for( int i = 0; i < m_nBatchObjIndex; i++ )
  {
		m_BatchIDList[i] = BatchList[i]->GetItemID();
  }
  SAFEARRAY* pArr;
  
 //CreateSafeArrayFromStrings(arrString, 5, pArr);
  //TODO: Need make sure the code below are not needed.
  CreateSafeArrayFromStrings( (LPCWSTR*)m_BatchIDList, m_nBatchObjIndex, pArr );
  OPCBBatchIDList.SetPropertyAtID( OPC_PROP_VALUE, pArr );

  OPCBBatchIDList.SetChildList(NULL);   // this never has children
  OPCBBatchIDList.SetParent(&root_tag);
  AddSampleBatchObject(&OPCBBatchIDList);

}

void OPCBXMLData::InitObjectList()
{
	int i;

	for( i = 0; i < MAX_CURRENT_OBJECT; i++ )
	{
		m_CurrentObj[i] = -1;
	}

	for( i = 0; i < MAX_OBJECT; i++ )
	{
		m_Obj[i].pObj = NULL;
		m_Obj[i].nChildCount = 0;
		m_Obj[i].nParent = -1;
	}
	m_nObjIndex = 0;
	m_nCurrentLevel = 0;
}

void OPCBXMLData::LoadObjectList()
{
	int i, j;

	OPCBObj** ppChildList;
	for( i = 0; i < m_nObjIndex; i++ )
	{
		//Set parent.
		if( m_Obj[i].nParent >= 0 )
			m_Obj[i].pObj->SetParent( m_Obj[m_Obj[i].nParent].pObj );
		
		//Add children list.
		if( m_Obj[i].nChildCount > 0 )
		{
			ppChildList = (OPCBObj**) pIMalloc->Alloc( sizeof(OPCBObj*) * m_Obj[i].nChildCount + 1 );
		
			for( j = 0; j < m_Obj[i].nChildCount; j++ )
			{
				ppChildList[j] = m_Obj[m_Obj[i].nChild[j]].pObj;
			}

			ppChildList[j] = NULL;
			m_Obj[i].pObj->SetChildList( ppChildList );
		}

		if( m_Obj[i].nParent >= 0 )
			AddSampleBatchObject( m_Obj[i].pObj );
	}
}

long OPCBXMLData::Hex2Long(char *string)
{
	long nn, i; 
	TCHAR* str = string;
	
	nn = 0;
	
	for(i = 0; (str[i] >= '0' && str[i] <= '9') || (str[i] >= 'A' && str[i] <= 'F') ||
		(str[i] >= 'a' && str[i] <= 'f'); ++i)
	{
		
		if(str[i] >= '0' && str[i] <= '9')
		{
			nn = 16 * nn + str[i] - '0';
		}
		
		if(str[i] >= 'A' && str[i] <= 'F')
		{
			nn = 16 * nn + str[i] - 'A' + 10;
		}
		
		if(str[i] >= 'a' && str[i] <= 'f')
		{
			nn = 16 * nn + str[i] - 'a' + 10;
		}
	}
	
	return(nn);
}
//
// Helper function to set property for VT_DATE property ID..
//
HRESULT OPCBXMLData::SetDateProperty(OPCBObj *pObj, int Pid, char *strDate)
{
	HRESULT hr;
	VARIANT v;
	VariantInit(&v);
	SYSTEMTIME st;
	DATE date;
	int nYr, nMo, nDay, nHr, nMin, nSec;
	
	//Changed the DateTime to OPC Date format "yyyymmddThhmmss"
	//sscanf( strDate, "%2d/%2d/%2d %d:%d:%d", &nMo, &nDay, &nYr, &nHr, &nMin, &nSec );
	sscanf( strDate, "%4d%2d%2dT%2d%2d%2d", &nYr, &nMo, &nDay, &nHr, &nMin, &nSec );
	
	st.wMonth = nMo;
	st.wDay = nDay;
	st.wYear = nYr;
	st.wHour = nHr;
	st.wMinute = nMin;
	st.wSecond = nSec;
	
	if( st.wYear <= 100 )
	{
		if( st.wYear <= 30 )
			st.wYear += 2000;
		else
			st.wYear += 1900;
	}
	  
	
	SystemTimeToVariantTime (&st, &date);
	
	V_VT(&v) = VT_DATE;
	V_DATE(&v) = date;
	hr = pObj->SetPropertyAtID(Pid, v);
	VariantClear(&v);
	return hr;
}

HRESULT OPCBXMLData::SetPropertyArray(OPCBObj *pObj, int nPid, WCHAR** List, int size)
{

	// Create an array with the related ListItems
	SAFEARRAY* pArr;
	CreateSafeArrayFromStrings( (LPCWSTR*)List, size, pArr );
	pObj->SetPropertyAtID( nPid, pArr );
	return S_OK;
}
