namespace org.jinterop.dcom.test {


    using JIException = common.JIException;
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIArray = core.JIArray;
    using JIComServer = core.JIComServer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIStruct = core.JIStruct;
    using JIObjectFactory = impls.JIObjectFactory;
    using ElemDesc = impls.automation.ElemDesc;
    using FuncDesc = impls.automation.FuncDesc;
    using IJIDispatch = impls.automation.IJIDispatch;
    using IJITypeInfo = impls.automation.IJITypeInfo;
    using IJITypeLib = impls.automation.IJITypeLib;
    using TypeAttr = impls.automation.TypeAttr;
    using TypeDesc = impls.automation.TypeDesc;
    using VarDesc = impls.automation.VarDesc;

    public class MSTypeLibraryBrowser2
	{

		private JIComServer comServer;
		private IJIDispatch dispatch;
		private IJIComObject unknown;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSTypeLibraryBrowser2(String address, String args[]) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSTypeLibraryBrowser2(string address, string[] args)
		{
			var session = JISession.createSession(args[1],args[2],args[3]);
			session.useSessionSecurity(true);
			comServer = new JIComServer(JIProgId.ValueOf(args[4]),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void start() throws org.jinterop.dcom.common.JIException
		public virtual void start()
		{
			unknown = comServer.CreateInstance();
			dispatch = (IJIDispatch)JIObjectFactory.narrowObject(unknown.QueryInterface(impls.automation.IJIDispatch_Fields.IID));
			var typeLib = (IJITypeLib)((object[])dispatch.getTypeInfo(0).ContainingTypeLib)[0];
			var result = typeLib.getDocumentation(-1);
			Console.WriteLine("Name: " + ((JIString)result[0]).String);
			Console.WriteLine("Library Name: " + ((JIString)result[1]).String);
			Console.WriteLine("Full path to help file: " + ((JIString)result[3]).String);
			Console.WriteLine("\n------------------------Library Members---------------------");
			var typeInfoCount = typeLib.TypeInfoCount;
			string[] g_arrClassification = {};
			for (var l = 0; l < typeInfoCount;l++)
			{
				Console.WriteLine("\n\n-----------------------Member Description--------------------------");
				result = typeLib.getDocumentation(l);
				var k = typeLib.getTypeInfoType(l);


				Console.WriteLine("Name: " + ((JIString)result[0]).String);
				Console.WriteLine("Type: " + g_arrClassification[k]);


				var typeInfo = typeLib.getTypeInfo(l);
				var typeAttr = typeInfo.TypeAttr;
				IJITypeInfo ptempInfo = null;
				TypeAttr pTempAttr = null;
				if (typeAttr.typekind != (int)impls.automation.TypeKind_Fields.TKIND_DISPATCH && typeAttr.typekind != (int)impls.automation.TypeKind_Fields.TKIND_COCLASS)
				{
					var p = 0;
					p++;
				}

				if (typeAttr.typekind == (int)impls.automation.TypeKind_Fields.TKIND_COCLASS)
				{

					for (var i = 0;i < typeAttr.cImplTypes;i++)
					{
						var nFlags = -1;
						try
						{
							nFlags = typeInfo.getImplTypeFlags(i);
						}
						catch (JIException)
						{
							continue;
						}

						if ((nFlags & impls.automation.ImplTypeFlags_Fields.IMPLTYPEFLAG_FDEFAULT) == impls.automation.ImplTypeFlags_Fields.IMPLTYPEFLAG_FDEFAULT)
						{
							var hRefType = -1;
							try
							{
								hRefType = typeInfo.getRefTypeOfImplType(i);
							}
							catch (JIException)
							{
								break;
							}


							try
							{
								ptempInfo = typeInfo.getRefTypeInfo(hRefType);
							}
							catch (JIException)
							{
								break;
							}

							try
							{
								pTempAttr = ptempInfo.TypeAttr;
							}
							catch (JIException)
							{
								Console.WriteLine("Failed to get reference type info.");
								return;
							}
						}
					}

				}

				if (pTempAttr != null)
				{
					typeInfo = ptempInfo;
					typeAttr = pTempAttr;
				}

				int m_nMethodCount = typeAttr.cFuncs;
				int m_nVarCount = typeAttr.cVars;
				var m_nDispInfoCount = m_nMethodCount + 2 * m_nVarCount;
				Console.WriteLine("Method and variable count = " + m_nMethodCount + m_nVarCount + "\n\n");


				for (var i = 0;i < m_nMethodCount; i++)
				{
					Console.WriteLine("************Method Seperator*****************");
					FuncDesc pFuncDesc;

					try
					{
						pFuncDesc = typeInfo.getFuncDesc(i);
					}
					catch (JIException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						return;
					}

					Console.WriteLine(i + ": DispID = " + pFuncDesc.memberId);

					int nCount;
					try
					{
						var ret = typeInfo.getNames(pFuncDesc.memberId,1);
						Console.WriteLine("MethodName = " + ((JIString)((object[])((JIArray)ret[0]).ArrayInstance)[0]).String);
						nCount = (int)(int?)ret[1];
					}
					catch (JIException)
					{
						Console.WriteLine("GetNames failed.");
						return;
					}

					switch (pFuncDesc.invokeKind)
					{

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

					Console.WriteLine("VTable offset: " + pFuncDesc.oVft);
					Console.WriteLine("Calling convention: " + pFuncDesc.callConv);
					//TODO need to return a string representation of this.
					Console.WriteLine("Return type = " + pFuncDesc.elemdescFunc.typeDesc.vt);
					Console.WriteLine("ParamCount = " + pFuncDesc.cParams);
					var array = (JIArray)pFuncDesc.lprgelemdescParam.Referent;
					ElemDesc[] types = null;
					if (array != null)
					{
						var temp = (object[])array.ArrayInstance;
						types = new ElemDesc[temp.Length];
						for (var k1 = 0;k1 < temp.Length;k1++)
						{
							types[k1] = new ElemDesc((JIStruct)temp[k1]);
						}
					}

					for (var j = 0;j < pFuncDesc.cParams; j++)
					{

						if (((ElemDesc)types[j]).typeDesc.vt == (short)TypeDesc.VT_SAFEARRAY)
						{
							Console.WriteLine("Param(" + j + ") type = SafeArray");
						}
						else if (((ElemDesc)types[j]).typeDesc.vt == (short)TypeDesc.VT_PTR)
						{
							Console.WriteLine("Param(" + j + ") type = Pointer");
						}
						else
						{
							Console.WriteLine("Param(" + j + ") type = UserDefined");
						}
					}
				}


				for (var i = m_nMethodCount; i < m_nMethodCount + m_nVarCount; i++)
				{
					Console.WriteLine("************Variable Seperator*****************");
					VarDesc pVarDesc;
					try
					{
						pVarDesc = typeInfo.getVarDesc(i - m_nMethodCount);
					}
					catch (JIException)
					{
						Console.WriteLine("GetVarDesc failed.");
						return;
					}

					Console.WriteLine(i + ": DispID = " + pVarDesc.memberId);

					int nCount;
					try
					{
						var ret = typeInfo.getNames(pVarDesc.memberId,1);
						Console.WriteLine("VarName = " + ((JIString)((object[])((JIArray)ret[0]).ArrayInstance)[0]).String);
						nCount = (int)(int?)ret[1];
					}
					catch (JIException)
					{
						Console.WriteLine("GetNames failed.");
						return;
					}

					switch (pVarDesc.varkind)
					{
					case VarDesc.VAR_DISPATCH:
						Console.WriteLine("VarKind = VAR_DISPATCH");
						Console.WriteLine("VarType = " + pVarDesc.elemdescVar.typeDesc.vt);
						break;
					default:
						//TODO resolve to it's string representation
						Console.WriteLine("VarKind = " + pVarDesc.varkind);
						break;
					}
				}
			}

			Console.WriteLine("########################Execution complete#########################");
			JISession.destroySession(dispatch.AssociatedSession);
		}

		public static void Main(string[] args)
		{
			try
			{
				if (args.Length < 5)
				{
					Console.WriteLine("Please provide address domain username password progIdOfApplication");
					return;
				}
				Log.Logger.Level = Level.OFF;
				JISystem.InBuiltLogHandler = false;
				var typeLibraryBrowser = new MSTypeLibraryBrowser2(args[0],args);
				typeLibraryBrowser.start();
			}
			catch (Exception e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

		}

	}

}