// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Common;
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
    using System;
    using System.Threading;

    public class MSExcel3 {
        private readonly ComServer _comServer;
        private IDispatch _dispatch;
        private IComObject _unknown;
        private IDispatch _dispatchOfWorkSheets;
        private IDispatch _dispatchOfWorkBook;


        public MSExcel3(string address, string[] args) {
            var session = Session.CreateSession(args[1], args[2], args[3]);
            _comServer = new ComServer(ProgId.ValueOf("Excel.Application"), address, session);
        }


        public void StartExcel() {
            _unknown = _comServer.CreateInstance();
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(
                _unknown.QueryInterface(Interfaces.IID_IDispatch));
        }


        public void ShowExcel() {
            var dispId = _dispatch.GetIDsOfNames("Visible");
            var variant = new Variant(true);
            _dispatch.Put(dispId, variant);

            _dispatch.Put("DisplayAlerts", new Variant(true));
        }


        public void CreateWorkSheet() {
            var dispId = _dispatch.GetIDsOfNames("Workbooks");

            var outVal = _dispatch.Get(dispId);
            var dispatchOfWorkBooks = (IDispatch)ObjectFactory.NarrowObject(outVal.ObjectAsComObject);

            var outVal2 = dispatchOfWorkBooks.CallMethodA("Open", new object[] {
                new ComString("C:\\temp\\chart.xls"), true, true,
                Variant.CreateOPTIONAL_PARAM(),
                Variant.CreateOPTIONAL_PARAM(),
                Variant.CreateOPTIONAL_PARAM(),
                Variant.CreateOPTIONAL_PARAM(),
                Variant.CreateOPTIONAL_PARAM(),
                Variant.CreateOPTIONAL_PARAM(),
                Variant.CreateOPTIONAL_PARAM(),
                Variant.CreateOPTIONAL_PARAM(),
                Variant.CreateOPTIONAL_PARAM(),
                Variant.CreateOPTIONAL_PARAM(),
                Variant.CreateOPTIONAL_PARAM(),
                Variant.CreateOPTIONAL_PARAM() });
            _dispatchOfWorkBook = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

            outVal = _dispatchOfWorkBook.Get("Worksheets");
            _dispatchOfWorkSheets = (IDispatch)ObjectFactory.NarrowObject(outVal.ObjectAsComObject);

            outVal2 = _dispatchOfWorkSheets.Get("Item", new object[] { new Variant(1) });
            var sheet = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
            outVal2 = sheet.Get("Range", new object[] { new ComString("A1:B19"), Variant.CreateOPTIONAL_PARAM() });
            var range = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

            int[][] newValue = {
                 new int[] { 121, 117},
                 new int[] { 111, 156},
                 new int[] { 132, 138},
                 new int[] { 116, 119},
                 new int[] { 148, 126},
                 new int[] { 163, 143},
                 new int[] { 174, 135},
                 new int[] { 136, 142},
                 new int[] { 142, 163},
                 new int[] { 121, 117},
                 new int[] { 111, 156},
                 new int[] { 132, 138},
                 new int[] { 116, 119},
                 new int[] { 148, 126},
                 new int[] { 163, 143},
                 new int[] { 174, 135},
                 new int[] { 136, 142},
                 new int[] { 142, 163},
                 new int[] { 121, 117 }
             };

            range.Put("Value", new Variant(new ComArray(newValue)));

            Thread.Sleep(5000);

            for (var j = 0; j < 60; j++) {
                Thread.Sleep(300);
                var temp1 = newValue[0][0];
                var temp2 = newValue[0][1];

                int i;
                for (i = 1; i < newValue.Length; i++) {
                    for (var k = 0; k < newValue[i - 1].Length; k++) {
                        newValue[i - 1][k] = newValue[i][k];
                    }
                }

                newValue[i - 1][0] = temp1;
                newValue[i - 1][1] = temp2;
                // For Excel XP, use: range.setValue2(newValue);
                range.Put("Value", new Variant(new ComArray(newValue)));
            }

            outVal2 = sheet.Get("ChartObjects", new object[] { Variant.CreateOPTIONAL_PARAM() });
            var chartObjects = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
            outVal2 = chartObjects.CallMethodA("Add", new object[] { 100, 30.0, 400, 250.0 });
            // outVal2 = chartObjects.get("Item", new Object[]{new Integer(1)});
            var chartObject = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
            outVal = chartObject.Get("Chart");
            var chart = (IDispatch)ObjectFactory.NarrowObject(outVal.ObjectAsComObject);
            chart.CallMethod("SetSourceData", new object[] { range, Variant.CreateOPTIONAL_PARAM() });
            Thread.Sleep(5000);

            outVal = sheet.Get("PageSetup");
            var pageSetup = (IDispatch)ObjectFactory.NarrowObject(outVal.ObjectAsComObject);
            pageSetup.Put("Orientation", new Variant(2));
            pageSetup.Put("Zoom", new Variant(100));
            try {
                sheet.CallMethod("PrintOut", new object[] { Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });
            }
            catch (InteropException e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
                var excepInfo = sheet.LastExcepInfo;
                Console.WriteLine("Error Code in EXCEPINFO: " + excepInfo.ErrorCode);
            }
            _dispatchOfWorkBook.CallMethod("close", new object[] { false, Variant.CreateOPTIONAL_PARAM(), Variant.CreateOPTIONAL_PARAM() });
            _dispatch.CallMethod("Quit");
            Session.DestroySession(_dispatch.AssociatedSession);
        }


        public static void RunTest(string[] args) {
            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                var test = new MSExcel3(args[0], args);
                test.StartExcel();
                test.ShowExcel();
                test.CreateWorkSheet();
            }
            catch (Exception e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }





    }

}
