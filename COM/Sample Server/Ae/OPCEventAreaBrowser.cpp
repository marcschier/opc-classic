// OPCEventAreaBrowser.cpp
//
// © Copyright 1997,1998 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Alarms and Events Specification and may be used 
//  as set forth in the License Grant section of the OPC Specification.  
//  This code is provided as-is and without warranty or support of any sort 
//  and is subject to the Warranty and Liability Disclaimers which appear 
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by 
//  ICONICS, Inc.  http://www.iconics.com
//
// CONTENTS:
//
//  
//
//
//      System: OPC Alarm & Events
//   Subsystem: Sample Server
//
//
// Description: 
//
// Functions:   
//
//
//
//
//
/*   $History: OPCEventAreaBrowser.cpp $
 * 
 * *****************  Version 18  *****************
 * User: Jiml         Date: 8/02/02    Time: 4:47p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 17  *****************
 * User: Jiml         Date: 1/18/02    Time: 3:35p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 16  *****************
 * User: Jiml         Date: 8/14/01    Time: 12:50p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 11/18/98   Time: 3:08p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:56a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 3/31/98    Time: 5:58p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 3/31/98    Time: 2:56p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 3/24/98    Time: 11:54a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 3/13/98    Time: 6:46p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 3/11/98    Time: 4:34p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 3/04/98    Time: 4:14p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 2/26/98    Time: 11:04a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 1/02/98    Time: 6:29p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 12/31/97   Time: 11:47a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 12/30/97   Time: 6:46p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:08a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 1  *****************
 * User: Jiml         Date: 12/15/97   Time: 11:11a
 * Created in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 11/24/97   Time: 10:01a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 11/14/97   Time: 6:39p
 * Updated in $/OPC/AlarmEvents/SampleServer
*/
//
//
//*************************************************************************          
// OPCEventAreaBrowser.cpp : Implementation of COPCEventAreaBrowser
#include "stdafx.h"
#include "OPC_AE_SampleServer.h"
#include "OPCEventAreaBrowser.h"
#include "QueryContainers.h"
#include "MatchPattern.h"
#include "ThreadSafe.h"



const WCHAR	COPCEventAreaBrowser::seps[] = L"\\";


/////////////////////////////////////////////////////////////////////////////
// COPCEventAreaBrowser


COPCEventAreaBrowser::COPCEventAreaBrowser()
{
	m_pAreaNode = &theAreaRootNode;
	m_lcid = LOCALE_ENGLISH_US;
}


void COPCEventAreaBrowser::FillStringArray( const AreaNode& rAreaNode )
{
	AreaNodeSet::const_iterator it;
	for( it = rAreaNode.ConstChildren().begin();
		 it !=  rAreaNode.ConstChildren().end();
		 it++ )
	{
		switch( m_dwBrowseFilterType )
		{
		case OPC_AREA:
			if( !(*it).IsSource() )
			{
				if( MatchPattern( (*it).Name(), m_szFilterCriteria ) )
				{
					if( m_pLPOLESTR )
						m_pLPOLESTR[m_dwCount] = (*it).Name();
					++m_dwCount;
				}
			}
			break;
		case OPC_SOURCE:
			if( (*it).IsSource() )
			{
				if( MatchPattern( (*it).Name(), m_szFilterCriteria ) )
				{
					if( m_pLPOLESTR )
						m_pLPOLESTR[m_dwCount] = (*it).Name();
					++m_dwCount;
				}
			}
			break;
		}
	}
}


HRESULT COPCEventAreaBrowser::ChangeBrowsePosition( 
            /* [in] */ OPCAEBROWSEDIRECTION dwBrowseDirection,
            /* [string][in] */ LPCWSTR szString)
{


	ThreadSafe<COPCEventAreaBrowser> AutoLock(*this);  // calls Lock() and Unlock() in constructor/destructor

	switch( dwBrowseDirection )
	{
	case OPCAE_BROWSE_UP:
		{
		AreaNode *pParent = m_pAreaNode->Parent();
		if( !pParent )
			return E_FAIL;   // I must be at root
		m_pAreaNode = pParent;
		break;
		}

	case OPCAE_BROWSE_DOWN:
		{
			LPWSTR szText = OpcDeLocalizeText(m_lcid, szString);
			AreaNode test( szText, m_pAreaNode );
			AreaNodeSet::iterator it = m_pAreaNode->Children().find( test );
			
			if( it == m_pAreaNode->Children().end() )
			{
				CoTaskMemFree(szText);
				return OPC_E_INVALIDBRANCHNAME; // can't find szString
			}

			if( (*it).IsSource() )
			{
				CoTaskMemFree(szText);
				return OPC_E_INVALIDBRANCHNAME; // node is a Source, not an area
			}

			m_pAreaNode = const_cast<AreaNode*>(&(*it));
			CoTaskMemFree(szText);
			break;
		}

	case OPCAE_BROWSE_TO:
		{
		AreaNode *pNode = theAreaRootNode.find( szString, seps );
		if( pNode )
			m_pAreaNode = pNode;
		else
			return OPC_E_INVALIDBRANCHNAME;

		break;
		}
	default:  // unknown dwBrowseDirection
		return E_INVALIDARG;

	}

	return S_OK;
}

        
HRESULT COPCEventAreaBrowser::BrowseOPCAreas( 
	/* [in] */ OPCAEBROWSETYPE dwBrowseFilterType,
	/* [string][in] */ LPCWSTR szFilterCriteria,
	/* [out] */ LPENUMSTRING __RPC_FAR *ppIEnumString)
{
	if (ppIEnumString == NULL)
	{
		return E_INVALIDARG;
	}

	*ppIEnumString = NULL;
	CComObject<CComEnumString>* pEnum = NULL;
	ATLTRY(pEnum = new CComObject<CComEnumString>)

	if (pEnum == NULL)
	{
		return E_OUTOFMEMORY;
	}

	pEnum->_AtlInitialConstruct();

	m_dwCount = 0;
	{	
		ThreadSafe<COPCEventAreaBrowser> AutoLock(*this);  // calls Lock() and Unlock() in constructor/destructor

		m_pLPOLESTR = NULL;
		m_dwBrowseFilterType = dwBrowseFilterType;
		m_szFilterCriteria = szFilterCriteria;
		if( !m_szFilterCriteria || !wcslen(m_szFilterCriteria ) )
			m_szFilterCriteria = L"*";

		// call first just to set m_dwCount
		FillStringArray( *m_pAreaNode );

		if( m_dwCount != 0 )
		{
			ATLTRY(m_pLPOLESTR = new LPCOLESTR[m_dwCount])
			if (m_pLPOLESTR == NULL)
			{
				delete pEnum;
				return E_OUTOFMEMORY;
			}

			LPOLESTR* pend = (LPOLESTR *)&m_pLPOLESTR[m_dwCount];
			m_dwCount = 0;
			// call again to actually fill the array
			FillStringArray( *m_pAreaNode );

			// translate array.
			for (DWORD ii = 0; ii < m_dwCount; ii++)
			{
				LPWSTR szText = OpcLocalizeText(m_lcid, m_pLPOLESTR[ii]);
				m_pLPOLESTR[ii] = szText;
			}

			// copy the data
			pEnum->Init((LPOLESTR *)m_pLPOLESTR, pend, NULL,  AtlFlagCopy );

			// free array.
			for (DWORD ii = 0; ii < m_dwCount; ii++)
			{
				CoTaskMemFree((void*)m_pLPOLESTR[ii]);
			}		
			
			delete [] m_pLPOLESTR;
			m_pLPOLESTR = NULL;
		}
	}	// this brace does the unlock
	HRESULT hRes = pEnum->_InternalQueryInterface(IID_IEnumString, (void**)ppIEnumString);
	if (FAILED(hRes))
		delete pEnum;
	return (hRes == S_OK && !m_dwCount) ? S_FALSE : hRes;

}


        
HRESULT COPCEventAreaBrowser::GetQualifiedAreaName( 
            /* [in] */ LPCWSTR szAreaName,
            /* [string][out] */ LPWSTR __RPC_FAR *pszQualifiedAreaName)
{
	ThreadSafe<COPCEventAreaBrowser> AutoLock(*this);  // calls Lock() and Unlock() in constructor/destructor

	LPWSTR szText = OpcDeLocalizeText(m_lcid, szAreaName);

	AreaNode test( szText, m_pAreaNode );
	AreaNodeSet::const_iterator it = m_pAreaNode->ConstChildren().find( test );

	if( it == m_pAreaNode->ConstChildren().end() )
	{
		CoTaskMemFree(szText);
		return E_INVALIDARG;
	}

	// name exists but it's a source, not an area
	if( (*it).IsSource() )
	{
		CoTaskMemFree(szText);
		return E_INVALIDARG;
	}

	wstring sep( seps );
	wstring rtn( (*it).Name() );

	for( const AreaNode *pNode = (*it).Parent();
					pNode->Parent() != NULL;
					pNode = pNode->Parent() )
	{
		rtn = pNode->wsName() + sep + rtn;
	}

	*pszQualifiedAreaName = CoTaskAlloc( rtn.data() );

	CoTaskMemFree(szText);
	return S_OK;
}
      

HRESULT COPCEventAreaBrowser::GetQualifiedSourceName( 
            /* [in] */ LPCWSTR szSourceName,
            /* [string][out] */ LPWSTR __RPC_FAR *pszQualifiedSourceName)
{
	ThreadSafe<COPCEventAreaBrowser> AutoLock(*this);  // calls Lock() and Unlock() in constructor/destructor

	LPWSTR szText = OpcDeLocalizeText(m_lcid, szSourceName);

	AreaNode test( szText, m_pAreaNode );
	AreaNodeSet::const_iterator it = m_pAreaNode->ConstChildren().find( test );

	// szSourceName not found
	if (it == m_pAreaNode->ConstChildren().end())
	{
		CoTaskMemFree(szText);
		return E_INVALIDARG;	
	}

	// name exists but it's an area, not a source
	if (!(*it).IsSource())  
	{
		CoTaskMemFree(szText);
		return E_INVALIDARG;
	}

	// for this server the source name is already fully qualified so 
	// just make a copy and return it.

	*pszQualifiedSourceName = CoTaskAlloc((*it).Name());
	CoTaskMemFree(szText);
	return S_OK;
}

