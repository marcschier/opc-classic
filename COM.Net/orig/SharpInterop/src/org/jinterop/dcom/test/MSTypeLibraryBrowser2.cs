using System;

namespace org.jinterop.dcom.test {


	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIArray = org.jinterop.dcom.core.JIArray;
	using JIComServer = org.jinterop.dcom.core.JIComServer;
	using JIProgId = org.jinterop.dcom.core.JIProgId;
	using JISession = org.jinterop.dcom.core.JISession;
	using JIString = org.jinterop.dcom.core.JIString;
	using JIStruct = org.jinterop.dcom.core.JIStruct;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
	using ElemDesc = org.jinterop.dcom.impls.automation.ElemDesc;
	using FuncDesc = org.jinterop.dcom.impls.automation.FuncDesc;
	using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;
	using IJITypeInfo = org.jinterop.dcom.impls.automation.IJITypeInfo;
	using IJITypeLib = org.jinterop.dcom.impls.automation.IJITypeLib;
	using ImplTypeFlags = org.jinterop.dcom.impls.automation.ImplTypeFlags;
	using TypeAttr = org.jinterop.dcom.impls.automation.TypeAttr;
	using TypeDesc = org.jinterop.dcom.impls.automation.TypeDesc;
	using TypeKind = org.jinterop.dcom.impls.automation.TypeKind;
	using VarDesc = org.jinterop.dcom.impls.automation.VarDesc;

	public class MSTypeLibraryBrowser2 {

		private JIComServer ComServer = null;
		private IJIDispatch Dispatch = null;
		private IJIComObject Unknown = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSTypeLibraryBrowser2(String address, String args[]) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSTypeLibraryBrowser2(string address, string[] args) {
			JISession session = JISession.CreateSession(args[1],args[2],args[3]);
			session.UseSessionSecurity(true);
			ComServer = new JIComServer(JIProgId.ValueOf(args[4]),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void start() throws org.jinterop.dcom.common.JIException
		public virtual void Start() {
			Unknown = ComServer.CreateInstance();
			Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(Unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
			IJITypeLib typeLib = (IJITypeLib)((object[])Dispatch.GetTypeInfo(0).ContainingTypeLib)[0];
			object[] result = typeLib.GetDocumentation(-1);
			Console.WriteLine("Name: " + ((JIString)result[0]).String);
			Console.WriteLine("Library Name: " + ((JIString)result[1]).String);
			Console.WriteLine("Full path to help file: " + ((JIString)result[3]).String);
			Console.WriteLine("\n------------------------Library Members---------------------");
			int typeInfoCount = typeLib.TypeInfoCount;
			string[] g_arrClassification = {};
			for (int l = 0; l < typeInfoCount;l++) {
				Console.WriteLine("\n\n-----------------------Member Description--------------------------");
				result = typeLib.GetDocumentation(l);
				int k = typeLib.GetTypeInfoType(l);


				Console.WriteLine("Name: " + ((JIString)result[0]).String);
				Console.WriteLine("Type: " + g_arrClassification[k]);


				IJITypeInfo typeInfo = typeLib.GetTypeInfo(l);
				TypeAttr typeAttr = typeInfo.TypeAttr;
				IJITypeInfo ptempInfo = null;
				TypeAttr pTempAttr = null;
				if (typeAttr.Typekind != (int)org.jinterop.dcom.impls.automation.TypeKind_Fields.TKIND_DISPATCH && typeAttr.Typekind != (int)org.jinterop.dcom.impls.automation.TypeKind_Fields.TKIND_COCLASS) {
					int p = 0;
					p++;
				}

				if (typeAttr.Typekind == (int)org.jinterop.dcom.impls.automation.TypeKind_Fields.TKIND_COCLASS) {

					for (int i = 0;i < typeAttr.CImplTypes;i++) {
						int nFlags = -1;
						try {
							nFlags = typeInfo.GetImplTypeFlags(i);
						}
						catch (JIException) {
							continue;
						}

						if ((nFlags & org.jinterop.dcom.impls.automation.ImplTypeFlags_Fields.IMPLTYPEFLAG_FDEFAULT) == org.jinterop.dcom.impls.automation.ImplTypeFlags_Fields.IMPLTYPEFLAG_FDEFAULT) {
							int hRefType = -1;
							try {
								hRefType = typeInfo.GetRefTypeOfImplType(i);
							}
							catch (JIException) {
								break;
							}


							try {
								ptempInfo = typeInfo.GetRefTypeInfo(hRefType);
							}
							catch (JIException) {
								break;
							}

							try {
								pTempAttr = ptempInfo.TypeAttr;
							}
							catch (JIException) {
								Console.WriteLine("Failed to get reference type info.");
								return;
							}
						}
					}

				}

				if (pTempAttr != null) {
					typeInfo = ptempInfo;
					typeAttr = pTempAttr;
				}

				int m_nMethodCount = typeAttr.CFuncs;
				int m_nVarCount = typeAttr.CVars;
				int m_nDispInfoCount = m_nMethodCount + 2 * m_nVarCount;
				Console.WriteLine("Method and variable count = " + m_nMethodCount + m_nVarCount + "\n\n");


				for (int i = 0;i < m_nMethodCount; i++) {
					Console.WriteLine("************Method Seperator*****************");
					FuncDesc pFuncDesc;

					try {
						pFuncDesc = typeInfo.GetFuncDesc(i);
					}
					catch (JIException e) {
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						return;
					}

					Console.WriteLine(i + ": DispID = " + pFuncDesc.MemberId);

					int nCount;
					try {
						object[] ret = typeInfo.GetNames(pFuncDesc.MemberId,1);
						Console.WriteLine("MethodName = " + ((JIString)((object[])((JIArray)ret[0]).ArrayInstance)[0]).String);
						nCount = (int)((int?)ret[1]);
					}
					catch (JIException) {
						Console.WriteLine("GetNames failed.");
						return;
					}

					switch (pFuncDesc.InvokeKind) {

					case 2: //InvokeKind.INVOKE_PROPERTYGET.intValue():
						Console.WriteLine("PropertyGet");
						break;
					case 4: //InvokeKind.INVOKE_PROPERTYPUT.intValue():
						Console.WriteLine("PropertyPut");
						break;
					case 8: //InvokeKind.INVOKE_PROPERTYPUTREF.intValue():
						Console.WriteLine("PropertyPutRef");
						break;
					case 1: //InvokeKind.INVOKE_FUNC.intValue():
						Console.WriteLine("DispatchMethod");
						break;
					default:
						break;
					}

					Console.WriteLine("VTable offset: " + pFuncDesc.OVft);
					Console.WriteLine("Calling convention: " + pFuncDesc.CallConv);
					//TODO need to return a string representation of this.
					Console.WriteLine("Return type = " + pFuncDesc.ElemdescFunc.TypeDesc.Vt);
					Console.WriteLine("ParamCount = " + pFuncDesc.CParams);
					JIArray array = (JIArray)pFuncDesc.LprgelemdescParam.GetReferent();
					ElemDesc[] types = null;
					if (array != null) {
						object[] temp = (object[])array.ArrayInstance;
						types = new ElemDesc[temp.Length];
						for (int k1 = 0;k1 < temp.Length;k1++) {
							types[k1] = new ElemDesc((JIStruct)temp[k1]);
						}
					}

					for (int j = 0;j < pFuncDesc.CParams; j++) {

						if (((ElemDesc)types[j]).TypeDesc.Vt == (short)TypeDesc.VT_SAFEARRAY) {
							Console.WriteLine("Param(" + j + ") type = SafeArray");
						}
						else if (((ElemDesc)types[j]).TypeDesc.Vt == (short)TypeDesc.VT_PTR) {
							Console.WriteLine("Param(" + j + ") type = Pointer");
						}
						else {
							Console.WriteLine("Param(" + j + ") type = UserDefined");
						}
					}
				}


				for (int i = m_nMethodCount; i < m_nMethodCount + m_nVarCount; i++) {
					Console.WriteLine("************Variable Seperator*****************");
					VarDesc pVarDesc;
					try {
						pVarDesc = typeInfo.GetVarDesc(i - m_nMethodCount);
					}
					catch (JIException) {
						Console.WriteLine("GetVarDesc failed.");
						return;
					}

					Console.WriteLine(i + ": DispID = " + pVarDesc.MemberId);

					int nCount;
					try {
						object[] ret = typeInfo.GetNames(pVarDesc.MemberId,1);
						Console.WriteLine("VarName = " + ((JIString)((object[])((JIArray)ret[0]).ArrayInstance)[0]).String);
						nCount = (int)((int?)ret[1]);
					}
					catch (JIException) {
						Console.WriteLine("GetNames failed.");
						return;
					}

					switch (pVarDesc.Varkind) {
					case VarDesc.VAR_DISPATCH:
						Console.WriteLine("VarKind = VAR_DISPATCH");
						Console.WriteLine("VarType = " + pVarDesc.ElemdescVar.TypeDesc.Vt);
						break;
					default:
						//TODO resolve to it's string representation
						Console.WriteLine("VarKind = " + pVarDesc.Varkind);
						break;
					}
				}
			}

			Console.WriteLine("########################Execution complete#########################");
			JISession.DestroySession(Dispatch.AssociatedSession);
		}

		public static void Main(string[] args) {
			try {
				if (args.Length < 5) {
					Console.WriteLine("Please provide address domain username password progIdOfApplication");
					return;
				}
				JISystem.Logger.Level = Level.OFF;
				JISystem.InBuiltLogHandler = false;
				MSTypeLibraryBrowser2 typeLibraryBrowser = new MSTypeLibraryBrowser2(args[0],args);
				typeLibraryBrowser.Start();
			}
			catch (Exception e) {
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

		}

	}

}