// Define a class to represent Enumeration Sets for the sample
// server.  In a real server this would probably be replaced by
// some vendor-specific technique to access the enumeration sets
// that the batch server used.

#include <assert.h>

const int maxEnumsPerSet = 12;
const int maxEnumSets = 10;

class EnumerationSet
{
public:
  static DWORD M_cEnumSets;
  
  // constructor specifying name, id and count of enums
  EnumerationSet( LPOLESTR name, DWORD id, DWORD maxSize ) 
    : m_pszName(name), m_dwSetId(id), m_dwSize(maxSize)
  {
    // This simple implementation just uses an array to 
    // store the names.  This wastes space if there are 
    // user-defined extensions to the enum set because
    // there will be a gap between the last standard enum
    // and the first user-defined one (user defined start
    // at index 100).
    m_ppszEnumName = new LPOLESTR[maxSize + 1];
    for (DWORD i = 0; i < maxSize + 1; i++)
    {
      m_ppszEnumName[i] = NULL;
    }
    m_dwCount = 0;    // init the count of enums in this set
    M_cEnumSets++;    // incr the count of all enum sets
  }

  virtual ~EnumerationSet ()
  {
    for (DWORD i = 0; i < m_dwSize + 1; i++)
    {
      if (m_ppszEnumName[i] != NULL)
        delete[] m_ppszEnumName[i];
    }
	delete[] m_ppszEnumName;
    M_cEnumSets--;  // decr the count of all enum sets
  }

  void AddEnum ( DWORD index, LPOLESTR pEnumName )
  {
    assert (index >= 0 && index <= m_dwSize);

    if (m_ppszEnumName[index] != NULL)    // delete the old one
      delete [] m_ppszEnumName[index];
    m_ppszEnumName[index] = new OLECHAR[wcslen(pEnumName) + 1];
    wcscpy(m_ppszEnumName[index], pEnumName);  // insert the new one
    m_dwCount++;
  }

  const LPOLESTR GetEnumSetName() { return m_pszName; }
  const DWORD    GetEnumSetId()   { return m_dwSetId; }

  const LPOLESTR GetEnumNameAt( int i )   { return m_ppszEnumName[i]; }
  const DWORD    GetEnumCount ( )         { return m_dwCount; }

  const DWORD    GetEnumMaxSize ( )       { return m_dwSize; }

private:
  LPOLESTR m_pszName;         // Implementation-defined name of set
  DWORD    m_dwSetId;         // OPCB_ENUM_PHYS, etc
  LPOLESTR* m_ppszEnumName;   // Implementation-defined enum names
  DWORD    m_dwSize;          // Max enums allowed in this set
  DWORD    m_dwCount;         // Number of enums in the set

};

EnumerationSet* getEnumSetWithId ( DWORD id );

