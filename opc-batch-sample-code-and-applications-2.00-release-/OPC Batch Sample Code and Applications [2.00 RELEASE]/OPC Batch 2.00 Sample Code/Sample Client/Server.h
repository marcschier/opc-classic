// Server.h : header for functions to access server
/////////////////////////////////////////////////////////////////////////////




IUnknown*   CreateInprocOPCServer(LPCOLESTR pszProgID);

IUnknown*   CreateLocalOPCServer(LPCOLESTR pszProgID);

IUnknown*	CreateRemoteOPCServer(LPCOLESTR pszProgID, LPOLESTR pszNodeName);
