// pseudoMFC.h
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
//   $Workfile: pseudoMFC.h $
//
//
// Org. Author: Jim Luth
//     $Author: Jiml $
//   $Revision: 6 $
//       $Date: 7/07/99 5:06p $
//    $Archive: /OPC/AlarmEvents/SampleServer/CE/pseudoMFC.h $
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
/*   $History: pseudoMFC.h $
 * 
 * *****************  Version 6  *****************
 * User: Jiml         Date: 7/07/99    Time: 5:06p
 * Updated in $/OPC/AlarmEvents/SampleServer/CE
 * 
 * *****************  Version 5  *****************
 * User: Jiml         Date: 8/19/98    Time: 12:02p
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

#ifndef __PSEUDOMFC_H
#define __PSEUDOMFC_H

#ifndef __AFX_H__		// only define if not an mfc app !!!

// determine number of elements in an array (not bytes)
#ifndef _countof
#define _countof(array) (sizeof(array)/sizeof(array[0]))
#endif

#ifdef _DEBUG
#define UNUSED(x)
#else
#define UNUSED(x) x
#endif
#define UNUSED_ALWAYS(x) x


class CSyncObject;
class CSemaphore;
class CMutex;
class CEvent;
class CCriticalSection;

class CSingleLock;
class CMultiLock;


/////////////////////////////////////////////////////////////////////////////
// Basic synchronization object

class CSyncObject
{

// Constructor
public:
	CSyncObject(LPCTSTR pstrName);

// Attributes
public:
	HANDLE  m_hObject;
	operator HANDLE() const;

// Operations
	virtual BOOL Lock(DWORD dwTimeout = INFINITE);
	virtual BOOL Unlock() = 0;
	virtual BOOL Unlock(LONG /* lCount */, LPLONG /* lpPrevCount=NULL */)
		{ return TRUE; }

// Implementation
public:
	virtual ~CSyncObject();
	friend class CSingleLock;
	friend class CMultiLock;
};

#ifndef UNDER_CE
/////////////////////////////////////////////////////////////////////////////
// CSemaphore

class CSemaphore : public CSyncObject
{

// Constructor
public:
	CSemaphore(LONG lInitialCount = 1, LONG lMaxCount = 1,
		LPCTSTR pstrName=NULL, LPSECURITY_ATTRIBUTES lpsaAttributes = NULL);

// Implementation
public:
	virtual ~CSemaphore();
	virtual BOOL Unlock();
	virtual BOOL Unlock(LONG lCount, LPLONG lprevCount = NULL);
};

inline BOOL CSemaphore::Unlock()
	{ return Unlock(1, NULL); }


#endif

/////////////////////////////////////////////////////////////////////////////
// CMutex

class CMutex : public CSyncObject
{
// Constructor
public:
	CMutex(BOOL bInitiallyOwn = FALSE, LPCTSTR lpszName = NULL,
		LPSECURITY_ATTRIBUTES lpsaAttribute = NULL);

// Implementation
public:
	virtual ~CMutex();
	BOOL Unlock();
};

/////////////////////////////////////////////////////////////////////////////
// CEvent

class CEvent : public CSyncObject
{
// Constructor
public:
	CEvent(BOOL bInitiallyOwn = FALSE, BOOL bManualReset = FALSE,
		LPCTSTR lpszNAme = NULL, LPSECURITY_ATTRIBUTES lpsaAttribute = NULL);

// Operations
public:
	BOOL SetEvent();
	BOOL PulseEvent();
	BOOL ResetEvent();
	BOOL Unlock();

// Implementation
public:
	virtual ~CEvent();
};

/////////////////////////////////////////////////////////////////////////////
// CCriticalSection

class CCriticalSection : public CSyncObject
{
// Constructor
public:
	CCriticalSection();

// Attributes
public:
	operator CRITICAL_SECTION*();
	CRITICAL_SECTION m_sect;

// Operations
public:
	BOOL Unlock();
	BOOL Lock();
	BOOL Lock(DWORD dwTimeout);

// Implementation
public:
	virtual ~CCriticalSection();
};

/////////////////////////////////////////////////////////////////////////////
// CSingleLock

class CSingleLock
{
// Constructors
public:
	CSingleLock(CSyncObject* pObject, BOOL bInitialLock = FALSE);

// Operations
public:
	BOOL Lock(DWORD dwTimeOut = INFINITE);
	BOOL Unlock();
	BOOL Unlock(LONG lCount, LPLONG lPrevCount = NULL);
	BOOL IsLocked();

// Implementation
public:
	~CSingleLock();

protected:
	CSyncObject* m_pObject;
	HANDLE  m_hObject;
	BOOL    m_bAcquired;
};

/////////////////////////////////////////////////////////////////////////////
// CMultiLock

class CMultiLock
{
// Constructor
public:
	CMultiLock(CSyncObject* ppObjects[], DWORD dwCount, BOOL bInitialLock = FALSE);

// Operations
public:
	DWORD Lock(DWORD dwTimeOut = INFINITE, BOOL bWaitForAll = TRUE,
		DWORD dwWakeMask = 0);
	BOOL Unlock();
	BOOL Unlock(LONG lCount, LPLONG lPrevCount = NULL);
	BOOL IsLocked(DWORD dwItem);

// Implementation
public:
	~CMultiLock();

protected:
	HANDLE  m_hPreallocated[8];
	BOOL    m_bPreallocated[8];

	CSyncObject* const * m_ppObjectArray;
	HANDLE* m_pHandleArray;
	BOOL*   m_bLockedArray;
	DWORD   m_dwCount;
};


inline CSyncObject::operator HANDLE() const
	{ return m_hObject;}


inline BOOL CEvent::SetEvent()
	{ _ASSERT(m_hObject != NULL); return ::SetEvent(m_hObject); }
inline BOOL CEvent::PulseEvent()
	{ _ASSERT(m_hObject != NULL); return ::PulseEvent(m_hObject); }
inline BOOL CEvent::ResetEvent()
	{ _ASSERT(m_hObject != NULL); return ::ResetEvent(m_hObject); }

inline CSingleLock::~CSingleLock()
	{ Unlock(); }
inline BOOL CSingleLock::IsLocked()
	{ return m_bAcquired; }

inline BOOL CMultiLock::IsLocked(DWORD dwObject)
	{ _ASSERT(dwObject >= 0 && dwObject < m_dwCount);
		 return m_bLockedArray[dwObject]; }

inline CCriticalSection::CCriticalSection() : CSyncObject(NULL)
	{ ::InitializeCriticalSection(&m_sect); }
inline CCriticalSection::operator CRITICAL_SECTION*()
	{ return (CRITICAL_SECTION*) &m_sect; }
inline CCriticalSection::~CCriticalSection()
	{ ::DeleteCriticalSection(&m_sect); }
inline BOOL CCriticalSection::Lock()
	{ ::EnterCriticalSection(&m_sect); return TRUE; }
inline BOOL CCriticalSection::Lock(DWORD /* dwTimeout */)
	{ return Lock(); }
inline BOOL CCriticalSection::Unlock()
	{ ::LeaveCriticalSection(&m_sect); return TRUE; }



#endif
#endif
