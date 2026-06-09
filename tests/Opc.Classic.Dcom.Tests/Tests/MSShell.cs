// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Common;
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using SharpCifs.Util.Sharpen;
    using System;

    public class MSShell {

        internal Session _session;
        internal ComServer _comServer;

        internal MSShell(string[] args) {
            _session = Session.CreateSession(args[1], args[2], args[3]);
            _comServer = new ComServer(ProgId.ValueOf("Shell.Application"), args[0], _session);
        }

        internal void DoStuff() {
            // this will return a reference to the IUnknown of the Shell coclass.
            var comUnknown = _comServer.CreateInstance();

            // now we query for the IShellDispatch interface
            var shellDispatch = comUnknown.QueryInterface("D8F015C0-C278-11CE-A49E-444553540000");

            var callObject = new CallBuilder();
            //        callObject.Opnum = 5;
            //        callObject.AddInParamAsVariant(new Variant(new ComString("c:")));
            //        var result[] = shellDispatch.Call(callObject);

            //        callObject.ReInit();
            //        callObject.Opnum = 7;
            //        result = shellDispatch.Call(callObject);

            callObject.ReInit();
            callObject.Opnum = 2;
            callObject.AddInParamAsVariant(new Variant(2));
            callObject.AddOutParamAsType(typeof(IComObject));
            var result = shellDispatch.Call(callObject);
            var folder = ObjectFactory.NarrowObject((IComObject)result[0]);

            callObject = new CallBuilder {
                Opnum = 0
            };
            callObject.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR),
                InteropFlags.FLAG_NULL);
            result = folder.Call(callObject);
            Console.WriteLine("Current Folder: " + result[0]);

            callObject.ReInit();
            callObject.Opnum = 1;
            callObject.AddOutParamAsType(typeof(IComObject));
            result = folder.Call(callObject);
            var test = ObjectFactory.NarrowObject((IComObject)result[0]);

            //        Not implemented by shell
            //        callObject.ReInit();
            //        callObject.Opnum = 2;
            //        callObject.AddOutParamAsType(typeof(InterfacePointer));
            //        result = folder.call(callObject);
            //        test = ObjectFactory.CreateCOMInstance(shellDispatch,(InterfacePointer)result[0]);

            callObject.ReInit();
            callObject.Opnum = 3;
            callObject.AddOutParamAsType(typeof(IComObject));
            result = folder.Call(callObject);
            test = ObjectFactory.NarrowObject((IComObject)result[0]);

            callObject.ReInit();
            callObject.Opnum = 4;
            callObject.AddOutParamAsType(typeof(IComObject));
            result = folder.Call(callObject);
            var folderItems = ObjectFactory.NarrowObject((IComObject)result[0]);

            callObject = new CallBuilder {
                Opnum = 0
            };
            callObject.AddOutParamAsType(typeof(int));
            result = folderItems.Call(callObject);

            var count = (int)result[0];

            for (var i = 0; i < count; i++) {
                callObject.ReInit();
                callObject.Opnum = 3;
                callObject.AddInParamAsVariant(new Variant(i));
                callObject.AddOutParamAsType(typeof(IComObject));
                result = folderItems.Call(callObject);
                var folderItem = ObjectFactory.NarrowObject((IComObject)result[0]);


                var callObject2 = new CallBuilder {
                    Opnum = 2
                };
                callObject2.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
                result = folderItem.Call(callObject2);
                Console.WriteLine("Name of Object: " + result[0]);

                callObject2.ReInit();
                callObject2.Opnum = 4;
                callObject2.AddOutParamAsObject(new ComString(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR));
                result = folderItem.Call(callObject2);
                Console.WriteLine("Path of the Object: " + result[0]);


                callObject2.ReInit();
                callObject2 = new CallBuilder {
                    Opnum = 9
                };
                // VARIANT_BOOL is Boolean
                callObject2.AddOutParamAsType(typeof(bool));
                result = folderItem.Call(callObject2);

                var isFileSystemObject = (bool)result[0];

                if (isFileSystemObject) {
                    Console.Write(" and is part of file system\n");
                }
                else {
                    Console.Write(" and is not part of file system\n");
                }

                callObject2.ReInit();
                callObject2 = new CallBuilder {
                    Opnum = 13
                };
                callObject2.AddOutParamAsObject(typeof(int));
                result = folderItem.Call(callObject2);
                Console.Write(" and size(in bytes) is: " + (int)result[0] + "\n");
            }
        }

        public static void RunTest(string[] args) {

            if (args.Length < 4) {
                Console.WriteLine("Please provide address domain username password");
                return;
            }
            Interop.UseAutoRegistration = true;
            try {
                var shell = new MSShell(args);
                shell.DoStuff();
                Session.DestroySession(shell._session);
            }
            catch (UnknownHostException e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            catch (InteropException e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }

    }

}
