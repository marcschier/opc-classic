// OPCXXX.h
//
// (c) Copyright 1997 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Data Access Specification and may be used
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
//
// This file contains OBJECT definitions which are SPECFIC
// to the OPC server for XXX provided by vendor ZZZ.
// (The OPC.H file contains common interface base classes that
//  each vendor derives from)
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.00  11/18/96 ACC
//        12/10/96 acc   add Enumerators, XXXItem
//  0.02  03/01/97 acc   add GetValue(), SetValue() to XXXItem
//  0.90  04/01/97 acc   add Async Support
//  1.0a  08/01/97 acc   1.0a updates; remove IEnumUnkown,
//                       add BrowseAddressSpace
//        08/25/97 acc   add ConnectionPoint Support to group.
//  2.00  01/12/98 acc   add ConnectionPoint Support to server.
//                       made connection point stuff more generic
//        04/08/98 acc   make BSAS a tearoff i/f rather than serparate obj
//        06/17/98 acc   rename some AsyncIO2 methods; cancel2,setenable,getenable
//        10/14/98 acc   rename AsyncIO2::Refresh to Refresh2
//        08/19/99 acc   add stuff for hierarchical address browser example
//                        NOTE: at this time the AddItem, ValidateItem and Read functions
//                        do not actually use the sample tags that the browser sees (itemids.cpp).
//                        Making them do so would be a good future improvement to this sample code.
//        07/14/00 acc   add SetUpdateRate to XXXGroup
//


#ifndef OPCXXX_H
#define OPCXXX_H


#include "OPCCOMN.h"	//The 'standard' OPC Common custom interface defintions
#include "OPCDA.h"	//The 'standard' OPC Data Access custom interface defintions
#include "OLECTL.h"	// For ConnectionPoint Stuff
#include "ENUMHELP.h"	//Some enumerator helper classeses
#include "CPCHELP.h"	//Some connection point helper classeses

#include "BatchItemSupport.h"

#include "opcbc.h" // The standard OPC Batch custom interface definitions
#include "OPCBatchProps.h"
#include "OPCBatchError.h"

// Define the unique classID that represents the OPC Batch Sample Server

//
// *** EVERY IMPLEMENTATION MUST USE A DIFFERENT GUID ***.

// CLSID_OPCBatchSampleServer == {E53B21C7-990E-11d3-B3E4-00C04F8ECEAA}
DEFINE_GUID(CLSID_OPCBatchSampleServer, 0xe53b21c7, 0x990e, 0x11d3, 0xb3, 0xe4, 0x0, 0xc0, 0x4f, 0x8e, 0xce, 0xaa);


// Implementation specific definitions.
#define N_GRPS	10		// max number of groups supported per server
#define N_ITEMS	100		// max number of items supported per group

// Special Quality flag for Inactive groups and items
//
#define QUAL_NOTACTIVE OPC_QUALITY_OUT_OF_SERVICE	// Bad/Out of Service

/////////////////////////////////////////////////////////////////////////////
// Forward type definitions of the vendor specific classes

class	XXXServer;	// The server object itself
class	IXXXServer;	// The IOPCServer Interface
class	IXXXSPG;	// The optional IOPCServerPublicGroups interface
class	IXXXBSAS;	// The optional IOPCBrowseServerAddressSpace interface
class	IXXXPF;		// The optional IPersistFile interface
class	IXXXCOMN;	// The IOPCCommon interface

class	XXXGroup;	// The Group Object itself
class	IXXXGSM;	// The IOPCGroupStateMgt interface
class	IXXXPGSM;	// The optional IOPCPublicGroupStateMgt interface
class	IXXXSIO;	// The IOPCSyncIO interface
class	IXXXASIO;	// The IOPCAsynIO interface
class	IXXXASIO2;	// The IOPCAsynIO2 interface 2.0
class	IXXXIM;		// The IOPCItemMgt interface
class	IXXXDO;		// The IDataObject interface
class	IXXXPRM;	// The IOPCProperties interface 2.0

class	IXXXCPC;	// The IConnectionPointContainer interface	V2.0
class	IXXXCP;		// The IOPCDataCallback ConnectionPoint interface 2.0

class	IXXXEnumIA;	// enum item attrs is returned from an itemmgt method

class IXXXBES;  // The IOPCEnumerations interface (from the OPC Batch spec)
class IXXXBSVR; // The IOPCBatchServer interface (from the OPC Batch spec)
class IXXXBSVR2;// The IOPCBatchServer2 interface (from the OPC Batch 2.0 spec)
class IXXXEnumBatchSummary;  // The IEnumOPCBatchSummary interface

class	XXXItem;	// A 'local' Item Class
class OPCBBatchObj;   // A 'local' batch class


/////////////////////////////////////////////////////////////////////////////
// Global server variables.
// These variables keep track of state information which is global to the server process.
// The common memory allocator is required so that the server
//   can allocate items to be returned to the client.
// The client must be able to access and to free these items 
//   so they need to be in common memory.
/////////////////////////////////////////////////////////////////////////////

extern FILETIME		serverStartTime;
extern IMalloc		*pIMalloc;		// Common memory allocator

/////////////////////////////////////////////////////////////////////////////
// Batch Server-specific delimiter
#define DELIMITER  L"."


/////////////////////////////////////////////////////////////////////////////
// From OPCUtil.cpp
// Utility and convenience functions
//
#define WCHAR	wchar_t

WCHAR * WSTRFromBSTR(const BSTR bstr, IMalloc *pmem);		// Aloc a WSTR and copy a BSTR into it
WCHAR * WSTRClone(const WCHAR *oldstr, IMalloc *pmem);
void WSTRFree(WCHAR * c, IMalloc *pmem);

/////////////////////////////////////////////////////////////////////////////
// From ItemUtil.cpp
// Utility and convenience functions
//
void IAClone(OPCITEMATTRIBUTES * pNewIA, const OPCITEMATTRIBUTES*pOldIA, IMalloc *pmem);
void IAFree(OPCITEMATTRIBUTES *pIA, IMalloc *pmem);

/////////////////////////////////////////////////////////////////////////////
// From BATCHSUMMUtil.cpp
// Utility and convenience functions
//
void BSClone(OPCBATCHSUMMARY * pNewBS, const OPCBATCHSUMMARY* pOldBS, IMalloc *pmem);
void BSFree(OPCBATCHSUMMARY *pBS, IMalloc *pmem);

/////////////////////////////////////////////////////////////////////////////
// Define a simple OPCITEM including support for Browse Address Space
// 
//
typedef struct tagitem
{
	LPOLESTR tag;
	struct tagitem *parent;
	struct tagitem *((*child)[]);	// ptr to an array of tagitem pointers
	// VARIANT  value;
	// WORD     quality;
	// FILETIME ftime;
	// additional vendor specific information...
} TAGITEM;


/////////////////////////////////////////////////////////////////////////////
// XXXServer
//   This class is an implementation of a XXX specific version of the
//   OPCServer class.
// There is no standard OPC Server 'object', only vendor specific objects
//   so this derives from IUnKnown. 
// The other XXX sepcific implementations of the OPC interfaces for this 
//   will derive from the OPC interface classes in OPC.H (see below)
//
/////////////////////////////////////////////////////////////////////////////
class XXXServer : public IUnknown
{
  public:
    XXXServer( LPUNKNOWN, void (*pfn)(void));
    ~XXXServer( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// Public (non-OPC) Functions
	void			UpdateData(DWORD tics);
	void			GenerateTransaction(DWORD *pTransactionID);
	void			ShutdownClients(void);	

  private:
	// Some private (non-opc) utility functions to help with Enumeration
	void 			GetUnkList( OPCENUMSCOPE scope, 
						LPUNKNOWN ** GroupList, int *GroupCount);
	void			FreeUnkList( LPUNKNOWN*GroupList, int GroupCount);
	void 			GetNameList( OPCENUMSCOPE scope, 
						LPOLESTR ** GroupList, int *GroupCount);
	void			FreeNameList( LPOLESTR*GroupList, int GroupCount);

  friend class IXXXServer;
  friend class IXXXPF;
  friend class IXXXGSM;
  friend class IXXXBSAS;
  friend class IXXXPRM;
  friend class IXXXCOMN;
 private:
	// Member Variables
	IXXXServer		*m_pIServer;	// ptr to 'tear-off' IOPCServer i/f
	IXXXSPG 		*m_pISPG;		// public groups (optional)
	IXXXPF			*m_pIPF;		// persistfile (optional)
	IXXXPRM			*m_pIPRM;		// IOPCProperties 
	IXXXCPC			*m_pCPC;		// ConnectionPointContainer for OPCShutdown
	IXXXCOMN		*m_pICOMN;		// IOPCCommmon
	IXXXBSAS		*m_pIBSAS;		// IOPCBrowseServerAddressSpace

  // Implementors for OPC-Batch interfaces
  IXXXBES     *m_pIBES;   // IOPCEnumerations implementor
  IXXXBSVR    *m_pIBSVR;  // IOPCBatchServer implementor
  IXXXBSVR2   *m_pIBSVR2; // IOPCBatchServer2 implementor

    ULONG			mRefCount;
    FILETIME		mLastUpdate;
    void			(*m_pfnDestroy)(void);

	LCID			m_LCID;			// server default LCID
	DWORD			m_tid;			// transaction ID
	int				m_Slot;			// Asnyc helper thread slot

	OPCBObj*	m_curptr;		// current address pointer for Browse
  void  setCurrentBrowsePosition(OPCBObj* obj) { m_curptr = obj; }
  OPCBObj* getCurrentBrowsePosition() { return m_curptr; }

	// A Real server might use an OBJLIST here
    struct {
	 int		inuse;
	 XXXGroup	* pGroup;
    } m_groups[N_GRPS];

};


/////////////////////////////////////////////////////////////////////////////
// XXX specific implementation of an IOPCServer Interface
//
class IXXXServer : public IOPCServer
{
  public:
    IXXXServer( LPUNKNOWN );
    ~IXXXServer( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IOPCServer Functions
	STDMETHODIMP         GetStatus( 
		OPCSERVERSTATUS** ppServerStatus);
	STDMETHODIMP         GetErrorString( 
		HRESULT hr, 
		LCID locale,
		LPWSTR *ppstring);
	STDMETHODIMP         AddGroup( 
		LPCWSTR szName,
		BOOL bActive,
		DWORD dwRequestedUpdateRate,
		OPCHANDLE hClientGroup,
		LONG *pTimeBias,
		FLOAT *pPercentDeadband,
		DWORD dwLCID,
		OPCHANDLE *phServerGroup,
		DWORD *pRevisedUpdateRate,
		REFIID riid,
		LPUNKNOWN *ppUnk
		);
	STDMETHODIMP         GetGroupByName( 
		LPCWSTR szGroupName, 
		REFIID riid, LPUNKNOWN *ppUnk);
	STDMETHODIMP         RemoveGroup( 
		OPCHANDLE groupHandleID,
		BOOL bForce);
			
	STDMETHODIMP CreateGroupEnumerator( 
		OPCENUMSCOPE dwScope,
		REFIID riid,
		LPUNKNOWN *ppUnk
		);

  private:
	// Member Variables
	XXXServer	*m_Parent;	// ptr to 'owning' XXXServer
};

/////////////////////////////////////////////////////////////////////////////
// XXX specific implementation of an IOPCCommon Interface for the Server
//
class IXXXCOMN: public IOPCCommon
{
  public:
    IXXXCOMN( LPUNKNOWN );
    ~IXXXCOMN( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IOPCCommon Functions
	STDMETHODIMP SetLocaleID( 
            LCID dwLcid
			);
        
	STDMETHODIMP GetLocaleID( 
            LCID *pdwLcid
			);
        
	STDMETHODIMP QueryAvailableLocaleIDs( 
            DWORD  *pdwCount,
            LCID  **pdwLcid
			);
        
	STDMETHODIMP GetErrorString( 
            HRESULT dwError,
            LPWSTR  *ppString
			);
        
	STDMETHODIMP SetClientName( 
            LPCWSTR szName
			);

  private:
	// Member Variables
	XXXServer	*m_Parent;	// ptr to 'owning' XXXServer
};

/////////////////////////////////////////////////////////////////////////////
// XXX specific implementation of an IPersistFile Interface for the Server
//
class IXXXPF: public IPersistFile
{
  public:
    IXXXPF( LPUNKNOWN );
    ~IXXXPF( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IPersist Functions
	STDMETHODIMP         GetClassID( 
		CLSID *pClassID
		);

	// the IPersistFile Functions
	STDMETHODIMP         IsDirty( void);
        
	STDMETHODIMP         Load(
		LPCOLESTR pszFileName,
		DWORD dwMode
		);
        
	STDMETHODIMP         Save( 
		LPCOLESTR pszFileName,
		BOOL fRemember
		);
        
	STDMETHODIMP         SaveCompleted( 
		LPCOLESTR pszFileName
		);
        
	STDMETHODIMP         GetCurFile( 
		LPOLESTR *ppszFileName
		);
        
  private:
	// Member Variables
	XXXServer	*m_Parent;	// ptr to 'owning' XXXServer
};

/////////////////////////////////////////////////////////////////////////////
// XXX specific implementation of an IOPCBrowserServerAddressSpace Interface
//
class IXXXBSAS : public IOPCBrowseServerAddressSpace
{
  public:
    IXXXBSAS( LPUNKNOWN parent,IMalloc * pmem );
    ~IXXXBSAS( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IOPCBrowse... Functions
    STDMETHODIMP QueryOrganization( 
            /* [out] */ OPCNAMESPACETYPE  *pNameSpaceType) ;
        
    STDMETHODIMP ChangeBrowsePosition( 
            /* [in] */ OPCBROWSEDIRECTION dwBrowseDirection,
            /* [string][in] */ LPCWSTR szString) ;
        
    STDMETHODIMP BrowseOPCItemIDs( 
            /* [in] */ OPCBROWSETYPE dwBrowseFilterType,
            /* [string][in] */ LPCWSTR szFilterCriteria,
            /* [in] */ VARTYPE vtDataTypeFilter,
            /* [in] */ DWORD dwAccessRightsFilter,
            /* [out] */ LPENUMSTRING  *ppIEnumString) ;
        
    STDMETHODIMP GetItemID( 
            /* [in] */ LPWSTR szItemDataID,
            /* [string][out] */ LPWSTR  *szItemID) ;
        
    STDMETHODIMP BrowseAccessPaths( 
            /* [string][in] */ LPCWSTR szItemID,
            /* [out] */ LPENUMSTRING  *ppIEnumString) ;
        


  private:
	  // Member Variables
	  XXXServer       *m_Parent;  //'parent' object for ref counting
	  IMalloc			*m_pmem;	// memory allocator to use

    // convenience methods for tracking the current
    // browse position; these just call accessors on the
    // parent XXXServer.
    OPCBObj* getCurrentBrowsePosition()
          { return m_Parent->getCurrentBrowsePosition(); }
    void  setCurrentBrowsePosition( OPCBObj* obj )
          { m_Parent->setCurrentBrowsePosition( obj ); }

};


/////////////////////////////////////////////////////////////////////////////
// XXX specific implementation of an IOPCProperties Interface for the Server
//
class IXXXPRM: public IOPCItemProperties
{
  public:
    IXXXPRM( LPUNKNOWN parent, IMalloc * pmem);
    ~IXXXPRM( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IOPCProperties Functions
	STDMETHODIMP IXXXPRM::QueryAvailableProperties( 
		LPWSTR     ItemID,			//  [in]                        
		DWORD    * pdwNumItems,		//  [out]                       
		DWORD   ** ppParameterIDs,	//  [out, size_is(,*dwNumItems)] 
		LPWSTR  ** ppDescriptions,	//  [out, size_is(,*dwNumItems)] 
		VARTYPE ** ppDataTypes		//  [out, size_is(,*dwNumItems)] 
		);
	STDMETHODIMP IXXXPRM::LookupItemIDs( 
		LPWSTR     ItemID,		    //	[in]                        
		DWORD      dwNumItems,		//	[in]                        
		DWORD    * ParameterIDs,	//	[in, size_is(dwNumItems)]                        
		LPWSTR  ** ppNewItemIDs,	//	[out, string, size_is(,dwNumItems)] 
		HRESULT ** ppErrors		    //	[out, size_is(,dwNumItems)] 
		);
	STDMETHODIMP IXXXPRM::GetItemProperties( 
		LPWSTR     ItemID,		    //	[in]
		DWORD      dwNumItems,		//	[in]
		DWORD    * pParameterIDs,	//	[in, size_is(dwNumItems)]
		VARIANT ** ppData,		    //	[out, size_is(,dwNumItems)]
		HRESULT ** ppErrors		    //	[out, size_is(,dwNumItems)]
		);

 private:
	// Member Variables
	XXXServer	*m_Parent;	// ptr to 'owning' XXXServer
	IMalloc		*m_pmem;	// memory allocator to use

  // Some private methods used by the above public methods
  // to encapsulate batch-specific code.
  STDMETHODIMP BatchQueryAvailableProperties(
 	  LPWSTR     ItemID,				    //   [in]                        
	  DWORD     *pdwNumItems,			  //   [out]                       
	  DWORD   ** ppParameterIDs,		//   [out, size_is(,*dwNumItems)] 
	  LPWSTR  ** ppDescriptions,		//   [out, size_is(,*dwNumItems)] 
	  VARTYPE ** ppDataTypes			  //   [out, size_is(,*dwNumItems)] 
    );

  STDMETHODIMP BatchLookupItemIDs(  
    LPWSTR     ItemID,        //  [in]                        
    DWORD      dwNumItems,    //  [in]                        
    DWORD    * pParameterIDs, //  [in]                        
    LPWSTR  ** ppNewItemIDs,  //  [out, string, size_is(,dwNumItems)] 
    HRESULT ** ppErrors       //  [out, size_is(,dwNumItems)] 
    );

  STDMETHODIMP BatchGetItemProperties( 
	  LPWSTR     ItemID,			  //	[in]
	  DWORD      dwNumItems,		//	[in]
	  DWORD    * pParameterIDs,	//	[in]
	  VARIANT ** ppData,			  //	[out, size_is(,dwNumItems)]
	  HRESULT ** ppErrors 		  //	[out, size_is(,dwNumItems)]
    );

};



/////////////////////////////////////////////////////////////////////////////
// XXXGroup
//   This class is an implementation of a XXX specific version of the
//   OPC Group class.
//
/////////////////////////////////////////////////////////////////////////////
class XXXGroup : public IUnknown
{
  public:
    XXXGroup( LPUNKNOWN );
    ~XXXGroup( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

  private:
	// Some private (non-opc) utility functions to help with Enumeration
	void		GetItemList( OPCITEMATTRIBUTES **AttrList, int *ItemCount);
	void		FreeItemList( OPCITEMATTRIBUTES *AttrList, int ItemCount);

	// Item List Management Functions
	BOOL		ItemValid(OPCHANDLE h);	// Validate Item Handle (see if in use)
	XXXItem		*ItemPtr(OPCHANDLE h);	// Given handle, return pointer
	int			ItemHandles(void);		// Total # item handles (some may be empty)
	HRESULT		ItemAlloc(OPCHANDLE *h);		// Alloc next avaialable Handle
	HRESULT		ItemReAlloc(OPCHANDLE h);		// Alloc a specific Handle (used by Clone)
	void		ItemSet(OPCHANDLE h, XXXItem * p);		// Assign an item to a slot
	void		ItemFree(OPCHANDLE h);

	void		AsyncHelper( DWORD tics );	//2.0
	void		Scan( void );
	BOOL		IsAdvised( void );

	void		CheckCPOnDataChange( IXXXCP * pCP );
	void		CheckCPRefresh( IXXXCP * pCP );
	void		CheckCPAsyncRead( IXXXCP * pCP );
	void		CheckCPAsyncWrite( IXXXCP * pCP );

	void		CheckDOOnDataTimeChange( void );
	void		CheckDOOnDataChange( void );
	void		CheckDORefresh( void );
	void		CheckDOAsyncRead( void );
	void		CheckDOAsyncWrite( void );
	void		SendStreamWithTime(int Count, OPCHANDLE *ItemHandleList, DWORD size, WORD mask, DWORD tid);
	void		SendStream(int Count, OPCHANDLE *ItemHandleList, DWORD size, WORD mask, DWORD tid);
	void		SendWriteStream(int Count, OPCHANDLE *ItemHandleList, DWORD tid);
	long		ComputeSize(int Count, OPCHANDLE *ItemHandleList);
	HRESULT		SetUpdateRate(DWORD dwUpDate);

  friend class IXXXServer;
  friend class XXXServer;
  friend class IXXXIM;
  friend class IXXXSIO;
  friend class IXXXASIO;
  friend class IXXXGSM;
  friend class IXXXPGSM;
  friend class IXXXDO;
  friend class IXXXEnumIA;
  friend class IXXXASIO2;
  private:
	// Member Variables
    ULONG		mRefCount;
	XXXServer	*m_ParentServer;
	DWORD		m_dwRevisedRate;
	FLOAT		m_Deadband;
	DWORD		m_LCID;
	LONG		m_TimeBias;
	BOOL		m_bActive;
	WCHAR		*m_szName;
	OPCHANDLE	m_ServerGroupHandle;
	OPCHANDLE	m_ClientGroupHandle;

	IXXXGSM		*m_pIXXXGSM;	//Group State Mgt
	IXXXPGSM	*m_pIXXXPGSM;	// Public Group State Mgt (optional)
	IXXXSIO		*m_pIXXXSIO;	// SyncIO
	IXXXIM		*m_pIXXXIM;		// Item Mgt

	// IDataObject Stuff
	IXXXASIO	*m_pIXXXASIO;	// ASynioIO
	IXXXDO		*m_pIXXXDO;		// Data Object

	// ConnectionPoint Stuff
	IXXXASIO2	*m_pIXXXASIO2;	// ASynioIO2
	IXXXCPC		*m_pCPC;		// ConnectionPointContainer V2.0

	// For managing update rate
	//
	long		m_scan;

	// AsyncIO/DataObject Flags	
	//
	BOOL		m_OnDataEnable;	// Enable/Disable OnDataChange for ASIO2
	BOOL		m_AsyncReadActive;
	BOOL		m_AsyncReadCancel;
	DWORD		m_AsyncReadTID;
	OPCDATASOURCE	m_AsyncReadSource;
	OPCHANDLE	*m_AsyncReadList;

	BOOL		m_AsyncWriteActive;
	BOOL		m_AsyncWriteCancel;
	DWORD		m_AsyncWriteTID;
	OPCHANDLE	*m_AsyncWriteList;

	BOOL		m_RefreshActive;
	BOOL		m_RefreshCancel;
	DWORD		m_RefreshTID;
	OPCDATASOURCE	m_RefreshSource;
	OPCHANDLE	*m_RefreshList;

	// AsyncIO2/ConnectionPoint Flags
	//
	BOOL		m_Async2ReadActive;
	BOOL		m_Async2ReadCancel;
	DWORD		m_Async2ReadTID;
	DWORD		m_Async2ReadCanID;
	OPCHANDLE	*m_Async2ReadList;

	BOOL		m_Async2WriteActive;
	BOOL		m_Async2WriteCancel;
	DWORD		m_Async2WriteTID;
	DWORD		m_Async2WriteCanID;
	OPCHANDLE	*m_Async2WriteList;

	OPCDATASOURCE	m_Refresh2Source;
	BOOL		m_Refresh2Active;
	BOOL		m_Refresh2Cancel;
	DWORD		m_Refresh2TID;
	DWORD		m_Refresh2CanID;
	OPCHANDLE	*m_Refresh2List;


	// A Real server might use an OBJLIST here
    struct {
	 int		inuse;
	 XXXItem	*pItem;
    } m_items[N_ITEMS];
};



/////////////////////////////////////////////////////////////////////////////
// IXXXIM
//   This class is an implementation of a XXX specific version of the
//   OPC ItemMgt Interface
//
class IXXXIM : public IOPCItemMgt
{
  public:
    IXXXIM( LPUNKNOWN );
    ~IXXXIM( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IOPCItemMgt Functions
	STDMETHODIMP AddItems( 
		DWORD            dwNumItems,
		OPCITEMDEF     * pItemArray,
		OPCITEMRESULT ** ppAddResults,
		HRESULT       ** ppErrors
	    );

	STDMETHODIMP ValidateItems( 
		DWORD             dwNumItems,
		OPCITEMDEF      * pItemArray,
		BOOL              bBlobUpdate,
		OPCITEMRESULT  ** ppValidationResults,
		HRESULT        ** ppErrors
	    );

	STDMETHODIMP RemoveItems( 
		DWORD        dwNumItems,
		OPCHANDLE  * phServer,
		HRESULT   ** ppErrors
	    );

	STDMETHODIMP SetActiveState(
		DWORD        dwNumItems,
		OPCHANDLE  * phServer,
		BOOL         bActive, 
		HRESULT   ** ppErrors
		);

	STDMETHODIMP SetClientHandles(
		DWORD        dwNumItems,
		OPCHANDLE  * phServer,
		OPCHANDLE  * phClient,
		HRESULT   ** ppErrors
		);

	STDMETHODIMP SetDatatypes(
		DWORD        dwNumItems,
		OPCHANDLE  * phServer,
		VARTYPE    * pRequestedDatatypes,
		HRESULT   ** ppErrors
	    );
 
	STDMETHODIMP CreateEnumerator(
		REFIID      riid,
		LPUNKNOWN * ppUnk
	    );

  private:
	// Member Variables
	XXXGroup	*m_Parent;	// ptr to owning 'XXXGroup'

};


/////////////////////////////////////////////////////////////////////////////
// IXXXSIO
//   This class is an implementation of a XXX specific version of the
//   OPC AsyncIO Interface
//
class IXXXSIO : public IOPCSyncIO
{
  public:
    IXXXSIO( LPUNKNOWN );
    ~IXXXSIO( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IOPCSyncIO Functions
	STDMETHODIMP Read(
		OPCDATASOURCE   dwSource,
		DWORD           dwNumItems, 
		OPCHANDLE     * phServer, 
		OPCITEMSTATE ** ppItemValues,
		HRESULT      ** ppErrors
	    );

	STDMETHODIMP Write(
		DWORD        dwNumItems, 
		OPCHANDLE  * phServer, 
		VARIANT    * pItemValues, 
		HRESULT   ** ppErrors
	    );

  private:
	// Member Variables
	XXXGroup	*m_Parent;	// ptr to owning 'XXXGroup'
};


/////////////////////////////////////////////////////////////////////////////
// IXXXASIO
//   This class is an implementation of a XXX specific version of the
//   OPC AsyncIO Interface
//
class IXXXASIO : public IOPCAsyncIO
{
  public:
    IXXXASIO( LPUNKNOWN );
    ~IXXXASIO( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

  	// the IOPCAsyncIO Functions
	STDMETHODIMP Read(
		DWORD           dwConnection,
		OPCDATASOURCE   dwSource,
		DWORD           dwNumItems,
		OPCHANDLE     * phServer,
		DWORD         * pTransactionID,
		HRESULT      ** ppErrors
	    );

	STDMETHODIMP Write(
		DWORD       dwConnection,
		DWORD       dwNumItems, 
		OPCHANDLE * phServer,
		VARIANT   * pItemValues, 
		DWORD     * pTransactionID,
		HRESULT ** ppErrors
	    );

	STDMETHODIMP Refresh(
		DWORD           dwConnection,
		OPCDATASOURCE   dwSource,
		DWORD         * pTransactionID
	    );

	STDMETHODIMP Cancel(
		DWORD dwTransactionID
	    );

  private:
	// Member Variables
	XXXGroup	*m_Parent;	// ptr to owning 'XXXGroup'
};

/////////////////////////////////////////////////////////////////////////////
// IXXXASIO2
//   This class is an implementation of a XXX specific version of the
//   OPC AsyncIO Interface
//
class IXXXASIO2 : public IOPCAsyncIO2
{
  public:
    IXXXASIO2( LPUNKNOWN );
    ~IXXXASIO2( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

  	// the IOPCAsyncIO2 Functions
	STDMETHODIMP Read(
		DWORD           dwNumItems,
		OPCHANDLE     * phServer,
		DWORD           TransactionID,
		DWORD         * pCancelID,
		HRESULT      ** ppErrors
	    );

	STDMETHODIMP Write(
		DWORD           dwNumItems, 
		OPCHANDLE     * phServer,
		VARIANT       * pItemValues, 
		DWORD           TransactionID,
		DWORD         * pCancelID,
		HRESULT      ** ppErrors
	    );

	STDMETHODIMP Refresh2(
		OPCDATASOURCE   dwSource,
		DWORD           TransactionID,
		DWORD         * pCancelID
	    );

	STDMETHODIMP Cancel2(
		DWORD           dwCancelID
	    );

	STDMETHODIMP SetEnable(
		BOOL            bEnable
    );

	STDMETHODIMP GetEnable(
		BOOL          * pbEnable
    );


  private:
	// Member Variables
	XXXGroup	*m_Parent;	// ptr to owning 'XXXGroup'
};

/////////////////////////////////////////////////////////////////////////////
// IXXXPGSM
//   This class is an implementation of a XXX specific version of the
//   Optional OPC PublicGroupStateMgt Interface
//
class IXXXPGSM : public IOPCPublicGroupStateMgt
{
  public:
    IXXXPGSM( LPUNKNOWN );
    ~IXXXPGSM( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IOPCPublicGroupStateMgt Functions
	STDMETHODIMP GetState(
		BOOL * pPublic
		);	

	STDMETHODIMP MoveToPublic(
		void
		);

  private:
	// Member Variables
	XXXGroup	*m_Parent;	// ptr to owning 'XXXGroup'
};


/////////////////////////////////////////////////////////////////////////////
// IXXXGSM
//   This class is an implementation of a XXX specific version of the
//   OPC PublicGroupStateMgt Interface
//
class IXXXGSM : public IOPCGroupStateMgt
{
  public:
    IXXXGSM( LPUNKNOWN );
    ~IXXXGSM( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IOPCGroupStateMgt Functions
	STDMETHODIMP GetState(
		DWORD     * pUpdateRate, 
		BOOL      * pActive, 
		LPWSTR    * ppName,
		LONG      * pTimeBias,
		FLOAT     * pPercentDeadband,
		DWORD     * pLCID,
		OPCHANDLE * phClientGroup,
		OPCHANDLE * phServerGroup
		);

	STDMETHODIMP SetState( 
		DWORD     * pRequestedUpdateRate, 
		DWORD     * pRevisedUpdateRate, 
		BOOL      * pActive, 
		LONG      * pTimeBias,
		FLOAT     * pPercentDeadband,
		DWORD     * pLCID,
		OPCHANDLE * phClientGroup
	    );

	STDMETHODIMP SetName( 
		LPCWSTR szName
	    );

	STDMETHODIMP CloneGroup(
		LPCWSTR     szName,
		REFIID      riid,
		LPUNKNOWN * ppUnk
	    );

  private:
	// Member Variables
	XXXGroup	*m_Parent;	// ptr to owning 'XXXGroup'
};


/////////////////////////////////////////////////////////////////////////////
// IXXXDO
//   This class is an implementation of a XXX specific version of the
//   OPC PublicGroupStateMgt Interface
//
class IXXXDO : public IDataObject
{
  public:
    IXXXDO( LPUNKNOWN );
    ~IXXXDO( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IDataObject Functions
	STDMETHODIMP GetData( 
			FORMATETC *pformatetcIn,
			STGMEDIUM *pmedium) ;
        
	STDMETHODIMP GetDataHere( 
			FORMATETC *pformatetc,
			STGMEDIUM *pmedium) ;
        
	STDMETHODIMP QueryGetData( 
			FORMATETC *pformatetc) ;
        
	STDMETHODIMP GetCanonicalFormatEtc( 
			FORMATETC *pformatectIn,
			FORMATETC *pformatetcOut) ;
        
	STDMETHODIMP SetData( 
			FORMATETC *pformatetc,
			STGMEDIUM *pmedium,
			BOOL fRelease) ;
        
	STDMETHODIMP EnumFormatEtc( 
			DWORD dwDirection,
			IEnumFORMATETC **ppenumFormatEtc) ;
        
	STDMETHODIMP DAdvise( 
			FORMATETC *pformatetc,
			DWORD advf,
			IAdviseSink *pAdvSink,
			DWORD *pdwConnection) ;
        
	STDMETHODIMP DUnadvise( 
			DWORD dwConnection) ;
        
	STDMETHODIMP EnumDAdvise( 
			IEnumSTATDATA **ppenumAdvise) ;
	 
  private:
	// Member Variables
	XXXGroup	*m_Parent;	// ptr to owning 'XXXGroup'
	IAdviseSink	*m_dataCallback;
	IAdviseSink	*m_datatimeCallback;
	IAdviseSink	*m_writeCallback;
	UINT m_data;
	UINT m_datatime;
	UINT m_write;

  friend class XXXGroup;

};



/////////////////////////////////////////////////////////////////////////////
// IXXXEnumIA
//   This class is an implementation of a XXX specific version of the
//   OPC IEnumOPCItemAttributes Interface
//
class IXXXEnumIA : public IEnumOPCItemAttributes
{
  public:
	IXXXEnumIA(LPUNKNOWN parent, ULONG c, OPCITEMATTRIBUTES* ia, IMalloc* m);
    IXXXEnumIA( LPUNKNOWN );
    ~IXXXEnumIA( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IEnum Functions
	STDMETHODIMP         Next (
		ULONG celt,
		OPCITEMATTRIBUTES **rgelt,
		ULONG *pceltFetched
		);
        
	STDMETHODIMP         Skip (
		ULONG celt
		);
        
	STDMETHODIMP         Reset(
		void
		);
        
	STDMETHODIMP         Clone( 
		IEnumOPCItemAttributes **ppenum
		);

  private:
	// Member Variables
	ULONG           m_cRef;     //Enum Object reference count
	LPUNKNOWN       m_pUnkRef;  //'parent' IUnknown for ref counting
	ULONG           m_iCur;     //Current element
	ULONG           m_cIA;     //Number of item attribues in us
	OPCITEMATTRIBUTES       *m_pIA;  //List of item attributes
	IMalloc			*m_pmem;	// memory allocator to use
};

/////////////////////////////////////////////////////////////////////////////
// IXXXBES
//   This class is an implementation of a XXX specific version of the
//   OPC-Batch IOPCEnumerationSets Interface
//
class IXXXBES : public IOPCEnumerationSets
{
  public:
    IXXXBES( LPUNKNOWN parent, IMalloc* pMem );
    ~IXXXBES( void );

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IOPCEnumserationSets Functions
	STDMETHODIMP QueryEnumerationSets(
		DWORD* pdwCount,          // [out]
    DWORD** ppdwEnumSetId,    // [out, size_is(,*pdwCount)]
    LPWSTR** ppszEnumSetName  // [out, string, size_is(,*pdwCount)]
		);	

	STDMETHODIMP QueryEnumeration(
		DWORD dwEnumSetId,        // [in]
    DWORD dwEnumValue,        // [in]
    LPWSTR* pszEnumName       // [out, string]
		);

  STDMETHODIMP QueryEnumerationList(
    DWORD dwEnumSetId,        // [in]
    DWORD* pdwCount,          // [out]
    DWORD** ppdwEnumValue,    // [out, size_is(,*pdwCount)]
    LPWSTR** ppszEnumString   // [out, string, size_is(,*pdwCount)]
    );

  private:
	// Member Variables
	XXXServer	*m_Parent;	// ptr to 'owning' XXXServer
	IMalloc		*m_pMem;	  // memory allocator to use
};

/////////////////////////////////////////////////////////////////////////////
// IXXXBSVR
//   This class is an implementation of a XXX specific version of the
//   OPC-Batch IOPCBatchServer Interface
//
class IXXXBSVR : public IOPCBatchServer
{
  public:
    IXXXBSVR( LPUNKNOWN parent, IMalloc* pMem );
    ~IXXXBSVR( void );

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

  STDMETHODIMP GetDelimiter(
    LPWSTR* pszDelimiter   // [out]
    );

  STDMETHODIMP CreateEnumerator(
    REFIID riid,      // [in]
    LPUNKNOWN* ppunk  // [out], iid_id(riid)]
    );


  private:
	// Member Variables
	XXXServer	*m_Parent;	// ptr to 'owning' XXXServer
	IMalloc		*m_pMem;	  // memory allocator to use
};

/////////////////////////////////////////////////////////////////////////////
// IXXXBSVR2
//   This class is an implementation of a XXX specific version of the
//   OPC-Batch IOPCBatchServer2 Interface
//
class IXXXBSVR2 : public IOPCBatchServer2
{
  public:
    IXXXBSVR2( LPUNKNOWN parent, IMalloc* pMem );
    ~IXXXBSVR2( void );

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

  STDMETHODIMP CreateFilteredEnumerator(
    REFIID riid,                        // [in]
    OPCBATCHSUMMARYFILTER *pFilter,    // [in]
    LPCWSTR szModel,                    // [in, string] 

    LPUNKNOWN* ppunk                    // [out], iid_id(riid)]
    );


  private:
	// Member Variables
	XXXServer	*m_Parent;	// ptr to 'owning' XXXServer
	IMalloc		*m_pMem;	  // memory allocator to use
};


/////////////////////////////////////////////////////////////////////////////
// IXXXEnumBatchSummary
//   This class is an implementation of a XXX specific version of the
//   OPC IEnumOPCBatchSummary Interface
//
class IXXXEnumBatchSummary : public IEnumOPCBatchSummary
{
  public:
	IXXXEnumBatchSummary(LPUNKNOWN parent, IMalloc* m);
  
  
  IXXXEnumBatchSummary( LPUNKNOWN );
  IXXXEnumBatchSummary(LPUNKNOWN pUnkRef, 
                       OPCBATCHSUMMARYFILTER* pFilter,
                       int iModel,
                       IMalloc * pmem);

  ~IXXXEnumBatchSummary( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// the IEnum Functions
	STDMETHODIMP         Next (
		ULONG celt,
		OPCBATCHSUMMARY **rgelt,
		ULONG *pceltFetched
		);
        
	STDMETHODIMP         Skip (
		ULONG celt
		);
        
	STDMETHODIMP         Reset(
		void
		);
        
	STDMETHODIMP         Clone( 
		IEnumOPCBatchSummary **ppenum
		);

  STDMETHODIMP         Count(
    ULONG *pcelt    // [out]
    );  

  private:
	// Member Variables
	ULONG           m_cRef;     //Enum Object reference count
	LPUNKNOWN       m_pUnkRef;  //'parent' IUnknown for ref counting
	ULONG           m_iCur;     //Current element
	ULONG           m_cBS;     //Number of batch summaries in us
	OPCBATCHSUMMARY       *m_pBS;  //List of batch summaries
	IMalloc			*m_pmem;	// memory allocator to use

  // Private Methods
  void          CreateBatchSummary(OPCBATCHSUMMARY* pBS, OPCBBatchObj* pBatch);
};


/////////////////////////////////////////////////////////////////////////////
// XXXItem
//   This class is an implementation of an XXX specific Item Class.
//   This implementation is PURELY for the sake of this sample code
//   and does NOT represent a required OPC object or interface.
//   It is intended to show a very simple example of a working item implementation.
//   You can implement your Items any way you chose - you can even
//   embed them as structures in the XXXGroup.
//
/////////////////////////////////////////////////////////////////////////////
// Cached Data Change Flags
#define OPC_ODC_CP	1		// ConnectionPoint Object
#define OPC_ODC_DO	2		// Data Object without time
#define OPC_ODC_DOT	4		// Data Object with time
#define OPC_ODC_ANY	(4+2+1)

// for use by QueDeviceRead()
//
#define DEVICE_READ	1
#define DEVICE_REFRESH	2

class XXXItem : public IUnknown
{
  public:
    XXXItem( XXXGroup *ParentGroup);
    virtual ~XXXItem( void);

	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface);
	STDMETHODIMP_(ULONG) AddRef( void);
	STDMETHODIMP_(ULONG) Release( void);

	// Member Utility Functions (Note these are NOT an OPC required interface!)
  // Made these all virtual so I could derive XXXBatchItem from this class - WGI
	virtual HRESULT 	Init(int j, OPCITEMDEF *def, OPCITEMRESULT *ir);
	virtual void		SetActive(BOOL a);
	virtual BOOL		GetActive(void);
	virtual void		SetHandle(OPCHANDLE h);
	virtual OPCHANDLE	GetHandle(void);
	virtual HRESULT		SetDatatype(VARTYPE v);
	virtual VARTYPE		GetDatatype(void);
	virtual XXXItem 	* Clone(XXXGroup * newparent, OPCHANDLE newserveritemhandle);
	virtual void		IAGet(OPCITEMATTRIBUTES * pIA);
	virtual HRESULT		GetValue(OPCDATASOURCE ds, VARIANT * v, WORD *q, FILETIME*t);
	virtual HRESULT		SetValue(VARIANT * v);
	virtual void		Simulate( void );
	virtual void		QueDeviceRead( short mask );	// 2.0
	virtual BOOL		CheckDeviceRead( HRESULT *r );
	virtual void		QueDeviceWrite( VARIANT * v);
	virtual BOOL		CheckDeviceWrite( HRESULT *r );
	virtual void		MarkAsChanged( WORD flg );
	virtual void		ClearChanged( WORD flg );
	virtual BOOL		Changed(WORD flg);
	
  protected:  // So the derived class can access them...

	OPCHANDLE	m_hServerItem;
	VARTYPE		m_vtCanonical;

	OPCHANDLE	m_hClientItem;
	WCHAR		* m_szItemID;
	WCHAR		* m_szAccessPath;
	VARTYPE		m_vtRequested;
	BOOL		m_bActive;

	// Simulated Server supplied Dynamic Data for testing...
	FLOAT		m_SimValue;		// used by the sample code's data simulator logic
	FLOAT		m_Value;
	WORD		m_Quality;
	FILETIME	m_TimeStamp;

	HRESULT		m_LastWriteError;	// Place to save result of Async Write
	BOOL		m_GenData;		// set to generate sine wave simulation
								// this flag is cleared by any WRITE
	WORD		m_AsyncMask;	// bit mask used by Async operations 	V2.0

  private:
	// Member Variables
  ULONG		mRefCount;
	XXXGroup	*m_Parent;		// The group that owns this item


};

class XXXBatchItem : public XXXItem
{
  public:
    XXXBatchItem( XXXGroup *ParentGroup);
    virtual ~XXXBatchItem( void);
   	virtual HRESULT 	Init(int j, OPCITEMDEF *def, OPCITEMRESULT *ir);
  	virtual HRESULT		GetValue(OPCDATASOURCE ds, VARIANT * v, WORD *q, FILETIME*t);
	  virtual HRESULT		SetValue(VARIANT * v);

  
  private: 
    OPCBObj*  m_pBatchObj;
    VARIANT   m_vValue;

};

#endif
