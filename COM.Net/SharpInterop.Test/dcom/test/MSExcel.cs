namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;

    public class MSExcel {

        private readonly int _xlWorksheet = -4167;
        private readonly int _xlXYScatterLinesNoMarkers = 75;
        private readonly int _xlColumns = 2;

        private readonly JIComServer _comServer;
        private IJIDispatch _dispatch;
        private IComObject _unknown;
        private IJIDispatch _dispatchOfWorkSheet;
        private IJIDispatch _dispatchOfWorkBook;
        private readonly JISession _session;

        public MSExcel(string address, string[] args) {
            _session = JISession.CreateSession(args[1], args[2], args[3]);
            _comServer = new JIComServer(JIProgId.ValueOf("Excel.Application"), address, _session);
        }


        public void StartExcel() {
            _unknown = _comServer.CreateInstance();
            _dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
            var typeInfo = _dispatch.GetTypeInfo(0);
            typeInfo.GetFuncDesc(0);
        }


        public void ShowExcel() {
            var dispId = _dispatch.GetIDsOfNames("Visible");
            var variant = new JIVariant(true);
            _dispatch.Put(dispId, variant);
        }


        public void CreateWorkSheet() {
            var dispId = _dispatch.GetIDsOfNames("Workbooks");
            object[] @out = { typeof(JIVariant) };
            var outVal = _dispatch.Get(dispId);
            _dispatchOfWorkBook = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);


            var dispIds = _dispatchOfWorkBook.GetIDsOfNames(new string[] { "Add", "Template" });

            @out = new object[] { typeof(JIVariant) };
            dispId = _dispatchOfWorkBook.GetIDsOfNames("Add");

            var outVal2 = _dispatchOfWorkBook.CallMethodA(dispId, new object[] { _xlWorksheet });
            _dispatchOfWorkBook = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

            dispId = _dispatchOfWorkBook.GetIDsOfNames("Worksheets");
            var variant = new JIVariant((short)1);
            @out = new object[] { typeof(JIVariant) };
            outVal2 = _dispatchOfWorkBook.Get(dispId, new object[] { variant });

            _dispatchOfWorkSheet = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

        }


        public void PasteStringToWorkSheet() {
            var dispId = _dispatchOfWorkSheet.GetIDsOfNames("Range");

            var variant = new JIVariant(new JIString("A1"));
            object[] @out = { typeof(JIVariant) };
            JIVariant outVal;
            var outVal2 = _dispatchOfWorkSheet.Get(dispId, new object[] { variant });

            var dispRange = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

            dispId = dispRange.GetIDsOfNames("Select");
            @out = new object[] { typeof(JIVariant) };
            outVal = dispRange.Get(dispId);

            dispId = _dispatchOfWorkBook.GetIDsOfNames("ActiveSheet");
            @out = new object[] { typeof(JIVariant) };
            outVal = _dispatchOfWorkBook.Get(dispId);

            var dispatchActiveSheet = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);
            dispId = dispatchActiveSheet.GetIDsOfNames("Paste");
            @out = new object[] { typeof(JIVariant) };
            try {
                outVal = dispatchActiveSheet.CallMethodA(dispId);
            }
            catch (JIException e) {
                throw e;
            }
        }


        public void CreateXYChart() {
            // column 2.
            var dispId = _dispatchOfWorkSheet.GetIDsOfNames("Columns");

            var cols = 2.0;
            object[] @out = { typeof(JIVariant) };
            JIVariant outVal;
            var outVal2 = _dispatchOfWorkSheet.Get(dispId, new object[] { cols });


            var dispatchRange = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

            dispId = _dispatchOfWorkBook.GetIDsOfNames("Charts");
            @out = new object[] { typeof(JIVariant) };
            outVal = _dispatchOfWorkBook.Get(dispId);

            var dispatchChart = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);



            dispId = dispatchChart.GetIDsOfNames("Add");
            @out = new object[] { typeof(JIVariant) };
            outVal = dispatchChart.CallMethodA(dispId);

            dispatchChart = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);

            dispId = _dispatchOfWorkBook.GetIDsOfNames("ActiveChart");
            @out = new object[] { typeof(JIVariant) };

            outVal = _dispatchOfWorkBook.Get(dispId);

            var dispatchActiveChart = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);

            dispId = dispatchActiveChart.GetIDsOfNames("ChartType");
            @out = new object[] { typeof(JIVariant) };

            dispatchActiveChart.Put(dispId, new JIVariant((short)_xlXYScatterLinesNoMarkers));

            var dispIds = dispatchActiveChart.GetIDsOfNames(new string[] { "SetSourceData", "Source", "PlotBy" });

            dispId = dispatchActiveChart.GetIDsOfNames("SetSourceData");
            @out = new object[] { typeof(JIVariant) };
            outVal2 = dispatchActiveChart.CallMethodA(dispId, new object[] { dispatchRange, (short)_xlColumns }, new int[] { dispIds[1], dispIds[2] }); // invoke(dispIds[0],IJIDispatch.DISPATCH_METHOD,new Object[]{variant,new JIArray(new Integer[]{new Integer(dispIds[1]),new Integer(dispIds[2])},true),null,null,null},null);

            JISession.DestroySession(_session);
        }

        public static void Main(string[] args) {

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