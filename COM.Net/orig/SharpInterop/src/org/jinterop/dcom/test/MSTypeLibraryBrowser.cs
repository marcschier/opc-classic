using System;

namespace org.jinterop.dcom.test {

    using JIException = org.jinterop.dcom.common.JIException;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JIComServer = org.jinterop.dcom.core.JIComServer;
    using JIProgId = org.jinterop.dcom.core.JIProgId;
    using JISession = org.jinterop.dcom.core.JISession;
    using JIString = org.jinterop.dcom.core.JIString;
    using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
    using FuncDesc = org.jinterop.dcom.impls.automation.FuncDesc;
    using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;
    using IJITypeInfo = org.jinterop.dcom.impls.automation.IJITypeInfo;
    using IJITypeLib = org.jinterop.dcom.impls.automation.IJITypeLib;
    using TypeAttr = org.jinterop.dcom.impls.automation.TypeAttr;
    using VarDesc = org.jinterop.dcom.impls.automation.VarDesc;

    public class MSTypeLibraryBrowser {

        private JIComServer ComServer = null;
        private IJIDispatch Dispatch = null;
        private IJIComObject Unknown = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSTypeLibraryBrowser(String address, String args[]) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public MSTypeLibraryBrowser(string address, string[] args) {
            JISession session = JISession.CreateSession(args[1],args[2],args[3]);
            ComServer = new JIComServer(JIProgId.ValueOf("InternetExplorer.Application"),address,session);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void start() throws org.jinterop.dcom.common.JIException
        public virtual void Start() {
            Unknown = ComServer.CreateInstance();
            Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(Unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
            IJITypeInfo typeInfo = Dispatch.GetTypeInfo(0);
            IJITypeLib typeLib = (IJITypeLib)((object[])typeInfo.ContainingTypeLib)[0];
            object[] result = typeLib.GetDocumentation(-1);
            Console.WriteLine(((JIString)result[0]).String);
            Console.WriteLine(((JIString)result[1]).String);
            Console.WriteLine(((JIString)result[3]).String);
            Console.WriteLine("-------------------------------");
            int typeInfoCount = typeLib.TypeInfoCount;
            int i = 0;
            string[] g_arrClassification = {};
            for (; i < typeInfoCount;i++) {
                result = typeLib.GetDocumentation(i);
                int j = typeLib.GetTypeInfoType(i);


                Console.WriteLine(((JIString)result[0]).String);
                Console.WriteLine(((JIString)result[1]).String);
                Console.WriteLine(((JIString)result[3]).String);
                Console.WriteLine(g_arrClassification[j]);

                IJITypeInfo typeInfo2 = typeLib.GetTypeInfo(i);
                TypeAttr typeAttr = typeInfo2.TypeAttr;
                for (j = 0;j < typeAttr.CFuncs;j++) {
                    FuncDesc funcDesc = typeInfo2.GetFuncDesc(j);
                    result = typeInfo2.GetDocumentation(funcDesc.MemberId);
                    Console.WriteLine(((JIString)result[0]).String);
                    Console.WriteLine(((JIString)result[1]).String);
                    Console.WriteLine(((JIString)result[3]).String);
                }

                for (j = 0;j < typeAttr.CVars;j++) {
                    if (j == 77) {
                        int kk = 0;
                    }
                    VarDesc varDesc = typeInfo2.GetVarDesc(j);
                    result = typeInfo2.GetDocumentation(varDesc.MemberId);
                    Console.WriteLine(((JIString)result[0]).String);
                    Console.WriteLine(((JIString)result[1]).String);
                    Console.WriteLine(((JIString)result[3]).String);
                    //System.out.println(j);
                }


                Console.WriteLine("***************************************");
            }
            JISession.DestroySession(Dispatch.AssociatedSession);
        }

        public static void Main(string[] args) {
            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                MSTypeLibraryBrowser typeLibraryBrowser = new MSTypeLibraryBrowser(args[0],args);
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