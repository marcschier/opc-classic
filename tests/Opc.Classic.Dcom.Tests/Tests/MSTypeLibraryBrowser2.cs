// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Common;
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
    using System;

    public class MSTypeLibraryBrowser2 {

        private readonly ComServer _comServer;
        private IDispatch _dispatch;
        private IComObject _unknown;


        public MSTypeLibraryBrowser2(string address, string[] args) {
            var session = Session.CreateSession(args[1], args[2], args[3]);
            session.UseSessionSecurity(true);
            _comServer = new ComServer(ProgId.ValueOf(args[4]), address, session);
        }


        public void Start() {
            _unknown = _comServer.CreateInstance();
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
            var typeLib = (ITypeLib)_dispatch.GetTypeInfo(0).ContainingTypeLib[0];
            var result = typeLib.GetDocumentation(-1);
            Console.WriteLine("Name: " + ((ComString)result[0]).String);
            Console.WriteLine("Library Name: " + ((ComString)result[1]).String);
            Console.WriteLine("Full path to help file: " + ((ComString)result[3]).String);
            Console.WriteLine("\n------------------------Library Members---------------------");
            var typeInfoCount = typeLib.TypeInfoCount;
            string[] g_arrClassification = { };
            for (var l = 0; l < typeInfoCount; l++) {
                Console.WriteLine("\n\n-----------------------Member Description--------------------------");
                result = typeLib.GetDocumentation(l);
                var k = typeLib.GetTypeInfoType(l);


                Console.WriteLine("Name: " + ((ComString)result[0]).String);
                Console.WriteLine("Type: " + g_arrClassification[k]);


                var typeInfo = typeLib.GetTypeInfo(l);
                var typeAttr = typeInfo.TypeAttr;
                ITypeInfo ptempInfo = null;
                TypeAttr pTempAttr = null;
                if (typeAttr.typekind != TypeKind.TKIND_DISPATCH && typeAttr.typekind != TypeKind.TKIND_COCLASS) {
                   // var p = 0;
                }

                if (typeAttr.typekind == TypeKind.TKIND_COCLASS) {

                    for (var i = 0; i < typeAttr.cImplTypes; i++) {
                        int nFlags;
                        try {
                            nFlags = typeInfo.GetImplTypeFlags(i);
                        }
                        catch (InteropException) {
                            continue;
                        }

                        if ((nFlags & (int)ImplTypeFlags.IMPLTYPEFLAG_FDEFAULT) == (int)ImplTypeFlags.IMPLTYPEFLAG_FDEFAULT) {
                            int hRefType;
                            try {
                                hRefType = typeInfo.GetRefTypeOfImplType(i);
                            }
                            catch (InteropException) {
                                break;
                            }


                            try {
                                ptempInfo = typeInfo.GetRefTypeInfo(hRefType);
                            }
                            catch (InteropException) {
                                break;
                            }

                            try {
                                pTempAttr = ptempInfo.TypeAttr;
                            }
                            catch (InteropException) {
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

                int m_nMethodCount = typeAttr.cFuncs;
                int m_nVarCount = typeAttr.cVars;
                var m_nDispInfoCount = m_nMethodCount + (2 * m_nVarCount);
                Console.WriteLine("Method and variable count = " + m_nMethodCount + m_nVarCount + "\n\n");


                for (var i = 0; i < m_nMethodCount; i++) {
                    Console.WriteLine("************Method Seperator*****************");
                    FuncDesc pFuncDesc;

                    try {
                        pFuncDesc = typeInfo.GetFuncDesc(i);
                    }
                    catch (InteropException e) {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                        return;
                    }

                    Console.WriteLine(i + ": DispID = " + pFuncDesc.memberId);

                    int nCount;
                    try {
                        var ret = typeInfo.GetNames(pFuncDesc.memberId, 1);
                        Console.WriteLine("MethodName = " + ((ComString)((object[])((ComArray)ret[0]).ArrayInstance)[0]).String);
                        nCount = (int)ret[1];
                    }
                    catch (InteropException) {
                        Console.WriteLine("GetNames failed.");
                        return;
                    }

                    switch (pFuncDesc.invokeKind) {

                        case 2: // InvokeKind.INVOKE_PROPERTYGET.intValue():
                            Console.WriteLine("PropertyGet");
                            break;
                        case 4: // InvokeKind.INVOKE_PROPERTYPUT.intValue():
                            Console.WriteLine("PropertyPut");
                            break;
                        case 8: // InvokeKind.INVOKE_PROPERTYPUTREF.intValue():
                            Console.WriteLine("PropertyPutRef");
                            break;
                        case 1: // InvokeKind.INVOKE_FUNC.intValue():
                            Console.WriteLine("DispatchMethod");
                            break;
                        default:
                            break;
                    }

                    Console.WriteLine("VTable offset: " + pFuncDesc.oVft);
                    Console.WriteLine("Calling convention: " + pFuncDesc.callConv);
                    // TODO need to return a string representation of this.
                    Console.WriteLine("Return type = " + pFuncDesc.elemdescFunc.TypeDesc.vt);
                    Console.WriteLine("ParamCount = " + pFuncDesc.cParams);
                    var array = (ComArray)pFuncDesc.lprgelemdescParam.Referent;
                    ElemDesc[] types = null;
                    if (array != null) {
                        var temp = (object[])array.ArrayInstance;
                        types = new ElemDesc[temp.Length];
                        for (var k1 = 0; k1 < temp.Length; k1++) {
                            types[k1] = new ElemDesc((Struct)temp[k1]);
                        }
                    }

                    for (var j = 0; j < pFuncDesc.cParams; j++) {

                        if (types[j].TypeDesc.vt == TypeDesc.VT_SAFEARRAY) {
                            Console.WriteLine("Param(" + j + ") type = SafeArray");
                        }
                        else if (types[j].TypeDesc.vt == TypeDesc.VT_PTR) {
                            Console.WriteLine("Param(" + j + ") type = Pointer");
                        }
                        else {
                            Console.WriteLine("Param(" + j + ") type = UserDefined");
                        }
                    }
                }


                for (var i = m_nMethodCount; i < m_nMethodCount + m_nVarCount; i++) {
                    Console.WriteLine("************Variable Seperator*****************");
                    VarDesc pVarDesc;
                    try {
                        pVarDesc = typeInfo.GetVarDesc(i - m_nMethodCount);
                    }
                    catch (InteropException) {
                        Console.WriteLine("GetVarDesc failed.");
                        return;
                    }

                    Console.WriteLine(i + ": DispID = " + pVarDesc.memberId);

                    int nCount;
                    try {
                        var ret = typeInfo.GetNames(pVarDesc.memberId, 1);
                        Console.WriteLine("VarName = " + ((ComString)((object[])((ComArray)ret[0]).ArrayInstance)[0]).String);
                        nCount = (int)ret[1];
                    }
                    catch (InteropException) {
                        Console.WriteLine("GetNames failed.");
                        return;
                    }

                    switch (pVarDesc.varkind) {
                        case VarDesc.VAR_DISPATCH:
                            Console.WriteLine("VarKind = VAR_DISPATCH");
                            Console.WriteLine("VarType = " + pVarDesc.elemdescVar.TypeDesc.vt);
                            break;
                        default:
                            // TODO resolve to it's string representation
                            Console.WriteLine("VarKind = " + pVarDesc.varkind);
                            break;
                    }
                }
            }

            Console.WriteLine("########################Execution complete#########################");
            Session.DestroySession(_dispatch.AssociatedSession);
        }

        public static void RunTest(string[] args) {
            try {
                if (args.Length < 5) {
                    Console.WriteLine("Please provide address domain username password progIdOfApplication");
                    return;
                }

                var typeLibraryBrowser = new MSTypeLibraryBrowser2(args[0], args);
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