// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
    using System;
    using System.Threading;

    public class MSExcel2_Test {

        private readonly ComServer _comServer;
        private IDispatch _dispatch;
        private IComObject _unknown;
        private IDispatch _dispatchOfWorkSheets;
        private IDispatch _dispatchOfWorkBook;
        private IDispatch _dispatchOfWorkSheet;
        private readonly Session _session;

        public MSExcel2_Test(string address, string[] args) {
            _session = Session.CreateSession(args[1], args[2], args[3]);
            _session.UseSessionSecurity(true);
            _comServer = new ComServer(ProgId.ValueOf("Excel.Application"), address, _session);
        }


        public void StartExcel() {
            _unknown = _comServer.CreateInstance();
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
        }


        public void ShowExcel() {
            var dispId = _dispatch.GetIDsOfNames("Visible");
            var variant = new Variant(true);
            _dispatch.Put(dispId, variant);
        }


        public void CreateWorkSheet() {
            var dispId = _dispatch.GetIDsOfNames("Workbooks");

            var outVal = _dispatch.Get(dispId);

            var dispatchOfWorkBooks = (IDispatch)ObjectFactory.NarrowObject(outVal.ObjectAsComObject);


            var outVal2 = dispatchOfWorkBooks.CallMethodA("Add", new object[] { Variant.CreateOPTIONAL_PARAM() });
            _dispatchOfWorkBook = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

            outVal = _dispatchOfWorkBook.Get("Worksheets");

            _dispatchOfWorkSheets = (IDispatch)ObjectFactory.NarrowObject(outVal.ObjectAsComObject);

            outVal2 = _dispatchOfWorkSheets.CallMethodA("Add", new object[] { Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });
            _dispatchOfWorkSheet = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
        }


        public void PasteArrayToWorkSheet(int nRow) {
            var dispId = _dispatchOfWorkSheet.GetIDsOfNames("Range");
            var variant = new Variant(new ComString("A1:C" + nRow));
            object[] @out = { typeof(Variant) };
            var outVal2 = _dispatchOfWorkSheet.Get(dispId, new object[] { variant });
            var dispRange = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

            // <see cref="Variant"/>[][] newValue = new <see cref="Variant"/>[nRow][3];
            var newValue = RectangularArrays.ReturnRectangularVariantArray(nRow, 3);

            for (var i = 0; i < newValue.Length; i++) {
                for (var j = 0; j < newValue[i].Length; j++) {
                    newValue[i][j] = new Variant(10.0 * new Random(1).NextDouble());
                }
            }

            dispRange.Put("Value2", new Variant(new ComArray(newValue)));

            Thread.Sleep(20000);

            var variant2 = dispRange.Get("Value2");
            var newValue2 = variant2.ObjectAsArray;
            newValue = (Variant[][])newValue2.ArrayInstance;
            for (var i = 0; i < newValue.Length; i++) {
                for (var j = 0; j < newValue[i].Length; j++) {
                    Console.Write(newValue[i][j] + "\t");
                }
                Console.WriteLine();
            }

            // Now write the value down
            dispRange.Put("Value2", new Variant(newValue2));

            Thread.Sleep(20000);

            _dispatchOfWorkBook.CallMethod("close", new object[] { false, Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });
            _dispatch.CallMethod("Quit");
            Session.DestroySession(_session);

        }

        public static void RunTest(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }

                var nRow = 600;

                if (args.Length > 4) {
                    try {
                        nRow = int.Parse(args[4]);
                    }
                    catch (System.FormatException) {

                    }
                }

                var test = new MSExcel2_Test(args[0], args);

                test.StartExcel();
                test.ShowExcel();
                test.CreateWorkSheet();

                test.PasteArrayToWorkSheet(nRow);

            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

        }

    }

}