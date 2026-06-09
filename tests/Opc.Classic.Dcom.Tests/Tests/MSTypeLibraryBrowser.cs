// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
    using System;

    public class MSTypeLibraryBrowser {

        private readonly ComServer _comServer;
        private IDispatch _dispatch;
        private IComObject _unknown;


        public MSTypeLibraryBrowser(string address, string[] args) {
            var session = Session.CreateSession(args[1], args[2], args[3]);
            _comServer = new ComServer(ProgId.ValueOf("InternetExplorer.Application"), address, session);
        }


        public void Start() {
            _unknown = _comServer.CreateInstance();
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
            var typeInfo = _dispatch.GetTypeInfo(0);
            var typeLib = (ITypeLib)typeInfo.ContainingTypeLib[0];
            var result = typeLib.GetDocumentation(-1);
            Console.WriteLine(((ComString)result[0]).String);
            Console.WriteLine(((ComString)result[1]).String);
            Console.WriteLine(((ComString)result[3]).String);
            Console.WriteLine("-------------------------------");
            var typeInfoCount = typeLib.TypeInfoCount;
            var i = 0;
            string[] g_arrClassification = { };
            for (; i < typeInfoCount; i++) {
                result = typeLib.GetDocumentation(i);
                var j = typeLib.GetTypeInfoType(i);


                Console.WriteLine(((ComString)result[0]).String);
                Console.WriteLine(((ComString)result[1]).String);
                Console.WriteLine(((ComString)result[3]).String);
                Console.WriteLine(g_arrClassification[j]);

                var typeInfo2 = typeLib.GetTypeInfo(i);
                var typeAttr = typeInfo2.TypeAttr;
                for (j = 0; j < typeAttr.cFuncs; j++) {
                    var funcDesc = typeInfo2.GetFuncDesc(j);
                    result = typeInfo2.GetDocumentation(funcDesc.memberId);
                    Console.WriteLine(((ComString)result[0]).String);
                    Console.WriteLine(((ComString)result[1]).String);
                    Console.WriteLine(((ComString)result[3]).String);
                }

                for (j = 0; j < typeAttr.cVars; j++) {
                    var varDesc = typeInfo2.GetVarDesc(j);
                    result = typeInfo2.GetDocumentation(varDesc.memberId);
                    Console.WriteLine(((ComString)result[0]).String);
                    Console.WriteLine(((ComString)result[1]).String);
                    Console.WriteLine(((ComString)result[3]).String);
                    // System.out.println(j);
                }


                Console.WriteLine("***************************************");
            }
            Session.DestroySession(_dispatch.AssociatedSession);
        }

        public static void RunTest(string[] args) {
            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                var typeLibraryBrowser = new MSTypeLibraryBrowser(args[0], args);
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
