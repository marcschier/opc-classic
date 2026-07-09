// pseudoMFC.cpp
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
/*   $History: pseudoMFC.cpp $
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 7/07/99    Time: 5:06p
 * Updated in $/OPC/AlarmEvents/SampleServer/CE
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 8/19/98    Time: 11:55a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 4  *****************
 * User: Jiml         Date: 4/23/98    Time: 2:28p
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 3  *****************
 * User: Jiml         Date: 12/24/97   Time: 10:09a
 * Updated in $/OPC/AlarmEvents/SampleServer
 * 
 * *****************  Version 2  *****************
 * User: Jiml         Date: 12/18/97   Time: 6:29p
 * Updated in $/OPC/AlarmEvents/SampleServer
*/
//
//
//*************************************************************************          
#include "stdafx.h"
#include "pseudoMFC.h"


#ifndef __AFX_H__		// only include code if not an mfc app !!!

CSyncObject::CSyncObject(LPCTSTR pstrName)
{
	UNUSED_ALWAYS(pstrName);   // unused in release builds

	m_hObject = NULL;

}

CSyncObject::~CSyncObject()
{
	if (m_hObject != NULL)
	{
		::CloseHandle(m_hObject);
		m_hObject = NULL;
	}
}

BOOL CSyncObject::Lock(DWORD dwTimeout)
{
	if (::WaitForSingleObject(m_hObject, dwTimeout) == WAIT_OBJECT_0)
		return TRUE;
	else
		return FALSE;
}


#ifndef UNDER_CE
/////////////////////////////////////////////////////////////////////////////
// CSemaphore

CSemaphore::CSemaphore(LONG lInitialCount, LONG lMaxCount,
	LPCTSTR pstrName, LPSECURITY_ATTRIBUTES lpsaAttributes)
	:  CSyncObject(pstrName)
{
	_ASSERT(lMaxCount > 0);
	_ASSERT(lInitialCount <= lMaxCount);

	m_hObject = ::CreateSemaphore(lpsaAttributes, lInitialCount, lMaxCount,
		pstrName);
}

CSemaphore::~CSemaphore()
{
}

BOOL CSemaphore::Unlock(LONG lCount, LPLONG lpPrevCount /* =NULL */)
{
	return ::ReleaseSemaphore(m_hObject, lCount, lpPrevCount);
}

#endif


/////////////////////////////////////////////////////////////////////////////
// CMutex

CMutex::CMutex(BOOL bInitiallyOwn, LPCTSTR pstrName,
	LPSECURITY_ATTRIBUTES lpsaAttribute /* = NULL */)
	: CSyncObject(pstrName)
{
	m_hObject = ::CreateMutex(lpsaAttribute, bInitiallyOwn, pstrName);
}

CMutex::~CMutex()
{
}

BOOL CMutex::Unlock()
{
	return ::ReleaseMutex(m_hObject);
}

/////////////////////////////////////////////////////////////////////////////
// CEvent

CEvent::CEvent(BOOL bInitiallyOwn, BOOL bManualReset, LPCTSTR pstrName,
	LPSECURITY_ATTRIBUTES lpsaAttribute)
	: CSyncObject(pstrName)
{
	m_hObject = ::CreateEvent(lpsaAttribute, bManualReset,
		bInitiallyOwn, pstrName);
}

CEvent::~CEvent()
{
}

BOOL CEvent::Unlock()
{
	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
// CSingleLock

CSingleLock::CSingleLock(CSyncObject* pObject, BOOL bInitialLock)
{
	_ASSERT(pObject != NULL);

	m_pObject = pObject;
	m_hObject = pObject->m_hObject;
	m_bAcquired = FALSE;

	if (bInitialLock)
		Lock();
}

BOOL CSingleLock::Lock(DWORD dwTimeOut /* = INFINITE */)
{
	_ASSERT(m_pObject != NULL || m_hObject != NULL);
	_ASSERT(!m_bAcquired);

	m_bAcquired = m_pObject->Lock(dwTimeOut);
	return m_bAcquired;
}

BOOL CSingleLock::Unlock()
{
	_ASSERT(m_pObject != NULL);
	if (m_bAcquired)
		m_bAcquired = !m_pObject->Unlock();

	// successfully unlocking means it isn't acquired
	return !m_bAcquired;
}

BOOL CSingleLock::Unlock(LONG lCount, LPLONG lpPrevCount /* = NULL */)
{
	_ASSERT(m_pObject != NULL);
	if (m_bAcquired)
		m_bAcquired = !m_pObject->Unlock(lCount, lpPrevCount);

	// successfully unlocking means it isn't acquired
	return !m_bAcquired;
}

/////////////////////////////////////////////////////////////////////////////
// CMultiLock

CMultiLock::CMultiLock(CSyncObject* pObjects[], DWORD dwCount,
	BOOL bInitialLock)
{
	_ASSERT(dwCount > 0 && dwCount <= MAXIMUM_WAIT_OBJECTS);
	_ASSERT(pObjects != NULL);

	m_ppObjectArray = pObjects;
	m_dwCount = dwCount;

	// as an optimization, skip alloacating array if
	// we can use a small, predeallocated bunch of handles

	if (m_dwCount > _countof(m_hPreallocated))
	{
		m_pHandleArray = new HANDLE[m_dwCount];
		m_bLockedArray = new BOOL[m_dwCount];
	}
	else
	{
		m_pHandleArray = m_hPreallocated;
		m_bLockedArray = m_bPreallocated;
	}

	// get list of handles from array of objects passed
	for (DWORD i = 0; i <m_dwCount; i++)
	{
		m_pHandleArray[i] = pObjects[i]->m_hObject;
		m_bLockedArray[i] = FALSE;
	}

	if (bInitialLock)
		Lock();
}

CMultiLock::~CMultiLock()
{
	Unlock();
	if (m_pHandleArray != m_hPreallocated)
	{
		delete[] m_bLockedArray;
		delete[] m_pHandleArray;
	}
}

DWORD CMultiLock::Lock(DWORD dwTimeOut /* = INFINITE */,
		BOOL bWaitForAll /* = TRUE */, DWORD dwWakeMask /* = 0 */)
{
	DWORD dwResult;
	if (dwWakeMask == 0)
		dwResult = ::WaitForMultipleObjects(m_dwCount,
			m_pHandleArray, bWaitForAll, dwTimeOut);
	else
		dwResult = ::MsgWaitForMultipleObjects(m_dwCount,
			m_pHandleArray, bWaitForAll, dwTimeOut, dwWakeMask);

	if (dwResult >= WAIT_OBJECT_0 && dwResult < (WAIT_OBJECT_0 + m_dwCount))
	{
		if (bWaitForAll)
		{
			for (DWORD i = 0; i < m_dwCount; i++)
				m_bLockedArray[i] = TRUE;
		}
		else
		{
			_ASSERT((dwResult - WAIT_OBJECT_0) >= 0);
			m_bLockedArray[dwResult - WAIT_OBJECT_0] = TRUE;
		}
	}
	return dwResult;
}

BOOL CMultiLock::Unlock()
{
	for (DWORD i=0; i < m_dwCount; i++)
	{
		if (m_bLockedArray[i])
			m_bLockedArray[i] = !m_ppObjectArray[i]->Unlock();
	}
	return TRUE;
}

BOOL CMultiLock::Unlock(LONG lCount, LPLONG lpPrevCount /* =NULL */)
{
	BOOL bGotOne = FALSE;
	for (DWORD i=0; i < m_dwCount; i++)
	{
		if (m_bLockedArray[i])
		{
			CSemaphore* pSemaphore = (CSemaphore*)(m_ppObjectArray[i]);
			if (pSemaphore != NULL)
			{
				bGotOne = TRUE;
				m_bLockedArray[i] = !m_ppObjectArray[i]->Unlock(lCount, lpPrevCount);
			}
		}
	}

	return bGotOne;
}

#endif


