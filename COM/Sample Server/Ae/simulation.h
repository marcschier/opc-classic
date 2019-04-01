// simulation.h
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
//-------------------------------------------------------------------------
//
//   $Workfile: simulation.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 10 $
//       $Date: 12/08/02 3:40p $
//    $Archive: /OPC/AlarmEvents/SampleServer/simulation.h $
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
/*   $History: simulation.h $
 * 
 * *****************  Version 10  *****************
 * User: Jiml         Date: 12/08/02   Time: 3:40p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * Added Keep-alive
 * 
 * *****************  Version 9  *****************
 * User: Jiml         Date: 8/19/98    Time: 12:01p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 8  *****************
 * User: Jiml         Date: 7/24/98    Time: 11:32a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 7  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 3/04/98    Time: 4:14p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:10a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 12/23/97   Time: 12:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 12/19/97   Time: 6:40p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 12/19/97   Time: 10:52a
 * Updated in $/OPC/AlarmEvents/SampleServer
*/
//
//
//*************************************************************************          

#ifndef __SIMULATION_H
#define __SIMULATION_H

#include "pseudoMFC.h"
#include <fstream>
#include "condition.h"


class Simulation
{
private:
	DWORD	m_dwSimulationMsecs;		// the rate which simulation records are read 
	ifstream	m_File;
	CEvent		AxeSimulationThread;
	static HANDLE		m_hSimulationThread;


	
	Simulation();
	void AddArea( LPCSTR szPath, LPCSTR szSource );
	void InitAttributes();
	void InitEventCategories();
	void InitAreaTree();
	void InitConditionMap();
	DWORD LookupEventCategory( LPCSTR szCatName );
	const char* GetTextField( BOOL bLastField = FALSE );
	void ProcessRecord();
	void ProcessCondition( LPCWSTR wszSource, LPCWSTR wszCondition, OPCCondition& cond );
	void Thread();
	static void ThreadStub( void *pThis ){ ((Simulation *)pThis)->Thread(); }
public:
	static Simulation	theSimulation;		// the one and only simulation instance

	static void Init();
	static void Shutdown();

	DWORD SimulationMsecs() const { return m_dwSimulationMsecs; }
};

#endif

