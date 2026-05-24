// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Test {
    using Opc.Classic.Dcom.Common;
    using Opc.Classic.Dcom.Core;
    using Opc.Classic.Dcom;
    using Opc.Classic.Dcom.Automation;
    using System;

    public class MSExcel {

        private readonly int _xlWorksheet = -4167;
        private readonly int _xlXYScatterLinesNoMarkers = 75;
        private readonly int _xlColumns = 2;

        private readonly ComServer _comServer;
        private IDispatch _dispatch;
        private IComObject _unknown;
        private IDispatch _dispatchOfWorkSheet;
        private IDispatch _dispatchOfWorkBook;
        private readonly Session _session;

        public MSExcel(string address, string[] args) {
            _session = Session.CreateSession(args[1], args[2], args[3]);
            _comServer = new ComServer(ProgId.ValueOf("Excel.Application"), address, _session);
        }


        public void StartExcel() {
            _unknown = _comServer.CreateInstance();
            _dispatch = (IDispatch)ObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
            var typeInfo = _dispatch.GetTypeInfo(0);
            typeInfo.GetFuncDesc(0);
        }


        public void ShowExcel() {
            var dispId = _dispatch.GetIDsOfNames("Visible");
            var variant = new Variant(true);
            _dispatch.Put(dispId, variant);
        }


        public void CreateWorkSheet() {
            var dispId = _dispatch.GetIDsOfNames("Workbooks");
            object[] @out = { typeof(Variant) };
            var outVal = _dispatch.Get(dispId);
            _dispatchOfWorkBook = (IDispatch)ObjectFactory.NarrowObject(outVal.ObjectAsComObject);


            var dispIds = _dispatchOfWorkBook.GetIDsOfNames(new string[] { "Add", "Template" });

            @out = new object[] { typeof(Variant) };
            dispId = _dispatchOfWorkBook.GetIDsOfNames("Add");

            var outVal2 = _dispatchOfWorkBook.CallMethodA(dispId, new object[] { _xlWorksheet });
            _dispatchOfWorkBook = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

            dispId = _dispatchOfWorkBook.GetIDsOfNames("Worksheets");
            var variant = new Variant((short)1);
            @out = new object[] { typeof(Variant) };
            outVal2 = _dispatchOfWorkBook.Get(dispId, new object[] { variant });

            _dispatchOfWorkSheet = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

        }


        public void PasteStringToWorkSheet() {
            var dispId = _dispatchOfWorkSheet.GetIDsOfNames("Range");

            var variant = new Variant(new ComString("A1"));
            object[] @out = { typeof(Variant) };
            Variant outVal;
            var outVal2 = _dispatchOfWorkSheet.Get(dispId, new object[] { variant });

            var dispRange = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

            dispId = dispRange.GetIDsOfNames("Select");
            @out = new object[] { typeof(Variant) };
            outVal = dispRange.Get(dispId);

            dispId = _dispatchOfWorkBook.GetIDsOfNames("ActiveSheet");
            @out = new object[] { typeof(Variant) };
            outVal = _dispatchOfWorkBook.Get(dispId);

            var dispatchActiveSheet = (IDispatch)ObjectFactory.NarrowObject(outVal.ObjectAsComObject);
            dispId = dispatchActiveSheet.GetIDsOfNames("Paste");
            @out = new object[] { typeof(Variant) };
            try {
                outVal = dispatchActiveSheet.CallMethodA(dispId);
            }
            catch (InteropException e) {
                throw e;
            }
        }


        public void CreateXYChart() {
            // column 2.
            var dispId = _dispatchOfWorkSheet.GetIDsOfNames("Columns");

            var cols = 2.0;
            object[] @out = { typeof(Variant) };
            Variant outVal;
            var outVal2 = _dispatchOfWorkSheet.Get(dispId, new object[] { cols });


            var dispatchRange = (IDispatch)ObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

            dispId = _dispatchOfWorkBook.GetIDsOfNames("Charts");
            @out = new object[] { typeof(Variant) };
            outVal = _dispatchOfWorkBook.Get(dispId);

            var dispatchChart = (IDispatch)ObjectFactory.NarrowObject(outVal.ObjectAsComObject);



            dispId = dispatchChart.GetIDsOfNames("Add");
            @out = new object[] { typeof(Variant) };
            outVal = dispatchChart.CallMethodA(dispId);

            dispatchChart = (IDispatch)ObjectFactory.NarrowObject(outVal.ObjectAsComObject);

            dispId = _dispatchOfWorkBook.GetIDsOfNames("ActiveChart");
            @out = new object[] { typeof(Variant) };

            outVal = _dispatchOfWorkBook.Get(dispId);

            var dispatchActiveChart = (IDispatch)ObjectFactory.NarrowObject(outVal.ObjectAsComObject);

            dispId = dispatchActiveChart.GetIDsOfNames("ChartType");
            @out = new object[] { typeof(Variant) };

            dispatchActiveChart.Put(dispId, new Variant((short)_xlXYScatterLinesNoMarkers));

            var dispIds = dispatchActiveChart.GetIDsOfNames(new string[] { "SetSourceData", "Source", "PlotBy" });

            dispId = dispatchActiveChart.GetIDsOfNames("SetSourceData");
            @out = new object[] { typeof(Variant) };
            outVal2 = dispatchActiveChart.CallMethodA(dispId, new object[] { dispatchRange, (short)_xlColumns }, new int[] { dispIds[1], dispIds[2] }); // invoke(dispIds[0],<see cref="IDispatch"/>.DISPATCH_METHOD,new Object[]{variant,new ComArray(new Integer[]{new Integer(dispIds[1]),new Integer(dispIds[2])},true),null,null,null},null);

            Session.DestroySession(_session);
        }

        public static void RunTest(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }
                var test = new MSExcel(args[0], args);
                test.StartExcel();
                test.ShowExcel();
                test.CreateWorkSheet();
                test.PasteStringToWorkSheet();
                test.CreateXYChart();
            }
            catch (Exception e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }





    }

}