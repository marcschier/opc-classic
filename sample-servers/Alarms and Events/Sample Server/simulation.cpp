// simulation.cpp
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
//   Subsystem: 
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
/*   $History: simulation.cpp $
 * 
 * *****************  Version 20  *****************
 * User: Jiml         Date: 1/07/03    Time: 5:41p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * minor source changes to build in VC7
 * 
 * *****************  Version 19  *****************
 * User: Jiml         Date: 8/02/02    Time: 4:47p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 18  *****************
 * User: Jiml         Date: 8/14/01    Time: 12:50p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 17  *****************
 * User: Jiml         Date: 3/29/99    Time: 2:49p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 16  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:58a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 15  *****************
 * User: Jiml         Date: 7/24/98    Time: 11:32a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 14  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 13  *****************
 * User: Jiml         Date: 3/19/98    Time: 12:41p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 12  *****************
 * User: Jiml         Date: 3/18/98    Time: 7:10p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 11  *****************
 * User: Jiml         Date: 3/13/98    Time: 6:46p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 3/12/98    Time: 4:55p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 3/04/98    Time: 4:14p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 2/26/98    Time: 11:04a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 12/30/97   Time: 6:46p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 12/29/97   Time: 7:00p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 12/26/97   Time: 6:55p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:09a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 12/23/97   Time: 12:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 12/19/97   Time: 6:40p
 * Updated in $/OPC/AlarmEvents/SampleServer
*/
//
//
//*************************************************************************          
#include "stdafx.h"
#include "simulation.h"
#include "pseudoMFC.h"
#include <process.h>
#include "ConditionMap.h"
#include <strstream>
#include "QueryContainers.h"
#include "global.h"
#include "TraceStream.h"
#include "FileTime.h"

Simulation Simulation::theSimulation;  // the global simulation instance
HANDLE Simulation::m_hSimulationThread = NULL;


Simulation::Simulation()
{
	USES_CONVERSION;
	m_dwSimulationMsecs = Registry::SimulationPeriod();  // default is 5 seconds
	m_hSimulationThread = NULL;

#ifdef _DEBUG	
	// the global TraceStream may not be created yet so use my own
	TraceStream MyTrace;
	MyTrace << "Simulation Period = " << m_dwSimulationMsecs << endl;
#endif


	TCHAR path[ _MAX_PATH ];
	GetModuleFileName( NULL, path, _MAX_PATH ); 
	// change last three chars from exe to csv
	_tcscpy( &path[_tcslen( path ) - 3 ], _T("csv") );

	m_File.open( T2A(path) );
}


// read the names of the attributes from the first (title) record of the file
void Simulation::InitAttributes()
{
	USES_CONVERSION;
	// The attributes follow field "Areas", so reaq up to there
	if( !m_File.is_open() )
		return;

	while( !m_File.eof() )
	{
		if( strcmp( GetTextField(), "Areas" ) == 0 )
			break;
	}
	string sAttrs;
	getline( m_File, sAttrs );

	istrstream Attrs( sAttrs.data() );

	string sAttr;
	while( !Attrs.eof() )
	{
		getline( Attrs, sAttr, ',' );
		wstring wsAttr( A2W(sAttr.data()) );
		theAttributesVector.push_back( wsAttr );
	}
	m_File.clear();
	m_File.seekg( 0 );  // rewind
}


// read a text field from the fstream
const char *Simulation::GetTextField( BOOL bLastField )
{
	static char buf[ _MAX_PATH ];
	*buf = '\0';
	m_File.getline( buf, _MAX_PATH, bLastField ? '\n' : ',' );
	return buf;		
}


DWORD Simulation::LookupEventCategory( LPCSTR szCatName )
{
	USES_CONVERSION;	
		
	wstring wsCat( A2W( szCatName ) );
	CATEGORIES_MAP::iterator it;
	it = theCategoriesMap.find( wsCat );
	_ASSERT( it != theCategoriesMap.end() );
	if( it != theCategoriesMap.end() )
		return (*it).second.dwID;

	return 0;
}


void Simulation::InitEventCategories()
{
	USES_CONVERSION;

	if( !m_File.is_open() )
		return;

	m_File.clear();
	m_File.seekg( 0 );  // rewind
	m_File.ignore( 5000, _T('\n') );  // skip header record

	while( !m_File.eof() )
	{
		CategoryData data;
		char EatComma;
						
		// EventType
		m_File >> data.dwEventType >> EatComma;  // first field is event type
		
		int i;
		
		for( i = 1; i < 2; i++ ) // ConditionName is column 3
			GetTextField();

		const char *szConditionName = GetTextField();
		wstring wsConditionName( A2W(szConditionName) );

		const char *szSubConditionName = GetTextField(); // SubconditionName is column 4
		wstring wsSubconditionName( A2W(szSubConditionName )); 

		GetTextField(); // skip column 5
		GetTextField(); // skip column 6
		GetTextField(); // skip column 7
	

		if( m_File.eof() )
			break;
		
		const char *szCat = GetTextField();
		wstring wsCat( A2W( szCat ) );
		pair<CATEGORIES_MAP::iterator, bool> catPair;
		// the DWORD id for the map is the 1-based index 
		// i.e. the first one inserted is one, the second is 2 ...
		data.dwID = theCategoriesMap.size() + 1;
		catPair = theCategoriesMap.insert( CATEGORIES_MAP::value_type( wsCat, 
															data ) );

		// if the insert failed because the category already exists in the map,
		// or in this dwEventType
		(*catPair.first).second.dwEventType |= data.dwEventType;

		// add the ConditionName to theConditionNameSet
		pair<ConditionNameSet::iterator, bool> cidsPair;
		cidsPair = theConditionNameSet.insert( wsConditionName );
		// now add the SubconditionName to the ConnditionName
		SUBCONDITIONNAME_SET& SubCondSet = (SUBCONDITIONNAME_SET&)((*(cidsPair.first)).m_SubconditionNameSet);   // cast away const
		SubCondSet.insert( wsSubconditionName );

		
		// add the event category and ConditionName to theConditionNameMap
		pair<CONDITIONNAME_MAP::iterator, bool> cidmPair;
		CONDTIONNAMELPCWSTR_SET NewSet;
		cidmPair = theConditionNameMap.insert( CONDITIONNAME_MAP::value_type
											( (*catPair.first).second.dwID, NewSet ) );

		pair<CONDTIONNAMELPCWSTR_SET::iterator, bool> cptrPair;
		cptrPair = (*(cidmPair.first)).second.insert( (*(cidsPair.first)).data() );



		m_File.ignore( 5000, _T('\n') );  // skip the rest of the record
	}
	m_File.clear();
	m_File.seekg( 0 );  // rewind
}


void Simulation::AddArea( LPCSTR szPath, LPCSTR szSource )
{
	istrstream path( szPath );

	AreaNode *pParent = &theAreaRootNode;  

	string sNodeName;
	while( !path.eof() )
	{
		getline( path, sNodeName, '\\' );
		AreaNode NewNode( sNodeName.data(), pParent );
		pair<AreaNodeSet::iterator, bool> InsertPair;

		InsertPair = pParent->Children().insert( NewNode );
		pParent = (AreaNode *)&(*(InsertPair.first));
	}

	// now add the source
	AreaNode NewNode( szSource, pParent, true );
	pair<AreaNodeSet::iterator, bool> InsertPair;

	InsertPair = pParent->Children().insert( NewNode );
}



void Simulation::InitAreaTree()
{
	USES_CONVERSION;

	if( !m_File.is_open() )
		return;

	m_File.clear();
	m_File.seekg( 0 );  // rewind
	m_File.ignore( 5000, _T('\n') );  // skip header record

	while( !m_File.eof() )
	{
		GetTextField();
		string sSource = GetTextField();   // Source is column 2

		for( int i = 2; i < 12; i++ )  // EventCategory is column 13
			GetTextField();

		if( m_File.eof() )
			break;
	
		istrstream areas( GetTextField() );

		string area;
		while( !areas.eof() )
		{
			getline( areas, area, ';' );
			AddArea( area.data(), sSource.data() );
		}

		m_File.ignore( 5000, _T('\n') );  // skip the rest of the record
	}
	m_File.clear();
	m_File.seekg( 0 );  // rewind
}



// process each record in the simulation file once to init the condition map
void Simulation::InitConditionMap()
{
	if( m_File.is_open() )
	{
		m_File.clear();
		m_File.seekg( 0 );  // rewind
		while( m_File.peek() != EOF && !m_File.eof() )
			ProcessRecord();
	}
}




void Simulation::ProcessCondition( LPCWSTR wszSource, LPCWSTR wszCondition, OPCCondition& cond )
{
	// read the rest of the record into cond
	USES_CONVERSION;
	char EatComma;

	cond.ClearChangeMask();

	cond.SubconditionName( GetTextField() );

	// read SubconditionDefinition
	LPCSTR szSubconditionDef = GetTextField();
	LPCWSTR wszSubconditionDef = A2W( szSubconditionDef );



	DWORD dw;
	m_File >> dw >> EatComma;
	cond.Active( dw );
	cond.Message( GetTextField() );
	cond.EventCategory( LookupEventCategory( GetTextField() ) );
	m_File >> dw >> EatComma;
	cond.Severity( dw );
	m_File >> dw >> EatComma;
	cond.Quality( (WORD)dw );
	m_File >> dw >> EatComma;
	cond.AckRequired( dw );
	cond.ActorID( GetTextField() );

	OPCSubcondition SubCond( cond.SubconditionName(), cond.Message(), 
					wszSubconditionDef, cond.Severity() );
	cond.push_back_subcondition( SubCond );

	istrstream areas( GetTextField() );

	string area;
	while( !areas.eof() )
	{
		getline( areas, area, ';' );
		wstring wsArea( A2W(area.data()) );
		cond.push_back_area( wsArea );
	}
	
	// deal with optional Attributes
	cond.clear();  // clear the existing attributes
	for( int i = 0; i < (int)theAttributesVector.size(); i++ )
	{
		CComVariant  attr( GetTextField( i == (theAttributesVector.size() - 1) ) );
		cond.push_back( attr );
	}

	// set the time of the condition to now
	cond.Time();

	// send to all clients if enabled

	if( cond.IsEffectivelyEnabled() )
	{
		ThreadSafeGlobal tsg;
		tsg->NotifyClients( wszSource, wszCondition, cond );
	}
}



void Simulation::ProcessRecord()
{
	USES_CONVERSION;

	if( !m_File.is_open() )
		return;

	OPCCondition NewCond;
	SourceMapPtr pSourceMap;
	pair<SourceMap::iterator, bool> SourceMapPair;

	
	if( m_File.peek() == EOF || m_File.eof() )
	{
		m_File.clear();
		m_File.seekg( 0 );  // rewind
	}

	if( m_File.tellg() == (streampos)0 )  // at bof so skip first record
		m_File.ignore( 5000, _T('\n') );


	DWORD dwEventType;
	char EatComma;
	
	// EventType
	m_File >> dwEventType >> EatComma;
	NewCond.EventType( dwEventType );

	// Source
	const char *szSource = GetTextField();
	wstring wsSource( A2W(szSource) );

	// ConditionName
	const char *szConditionName = GetTextField();
	wstring wsConditionName( A2W(szConditionName) );

	if( dwEventType == OPC_CONDITION_EVENT )
	{
		ConditionMap NewConditionMap;
		SourceMapPair = pSourceMap->insert( SourceMap::value_type( wsSource, NewConditionMap ) );
		ConditionMap& CondMap = (*(SourceMapPair.first)).second;
		pair<ConditionMap::iterator, bool> ConditionMapPair;
		ConditionMapPair = CondMap.insert( ConditionMap::value_type( wsConditionName, NewCond ) );
		OPCCondition& condition = (*(ConditionMapPair.first)).second;

		ProcessCondition( wsSource.data(), wsConditionName.data(), condition );
	}
	else  // a tracking or simple event
	{
		ProcessCondition( wsSource.data(), wsConditionName.data(), NewCond );
	}
}





void Simulation::Thread()
{
	do 
	{
		ProcessRecord();
	} while( WaitForSingleObject( AxeSimulationThread, m_dwSimulationMsecs) == WAIT_TIMEOUT );
}



void Simulation::Init()
{
	theSimulation.InitAttributes();
	theSimulation.InitEventCategories();
	theSimulation.InitAreaTree();
	theSimulation.InitConditionMap();

	// start the simulation thread to read alarms from the simulation file
	m_hSimulationThread = (HANDLE)_beginthread( ThreadStub, 0, &theSimulation );
}


void Simulation::Shutdown()
{
	// stop the simulation thread
	theSimulation.AxeSimulationThread.SetEvent();
	WaitForSingleObject( m_hSimulationThread, INFINITE );
}


