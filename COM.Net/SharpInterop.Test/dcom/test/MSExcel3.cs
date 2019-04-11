namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;
    using System.Threading;

    public class MSExcel3 {


        private readonly JIComServer _comServer;
        private IJIDispatch _dispatch;
        private IJIComObject _unknown;
        private IJIDispatch _dispatchOfWorkSheets;
        private IJIDispatch _dispatchOfWorkBook;

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public MSExcel3(String address, String args[]) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public MSExcel3(string address, string[] args) {
            var session = JISession.CreateSession(args[1], args[2], args[3]);
            _comServer = new JIComServer(JIProgId.ValueOf("Excel.Application"), address, session);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void startExcel() throws org.jinterop.dcom.common.JIException
        public virtual void StartExcel() {
            _unknown = _comServer.CreateInstance();
            _dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void showExcel() throws org.jinterop.dcom.common.JIException
        public virtual void ShowExcel() {
            var dispId = _dispatch.GetIDsOfNames("Visible");
            var variant = new JIVariant(true);
            _dispatch.Put(dispId, variant);

            _dispatch.Put("DisplayAlerts", new JIVariant(true));
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void createWorkSheet() throws org.jinterop.dcom.common.JIException
        public virtual void CreateWorkSheet() {
            var dispId = _dispatch.GetIDsOfNames("Workbooks");

            var outVal = _dispatch.Get(dispId);
            var dispatchOfWorkBooks = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);


            var outVal2 = dispatchOfWorkBooks.CallMethodA("Open", new object[] { new JIString("C:\\temp\\chart.xls"), true, true, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });
            _dispatchOfWorkBook = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

            outVal = _dispatchOfWorkBook.Get("Worksheets");
            _dispatchOfWorkSheets = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);

            outVal2 = _dispatchOfWorkSheets.Get("Item", new object[] { new JIVariant(1) });
            var sheet = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
            outVal2 = sheet.Get("Range", new object[] { new JIString("A1:B19"), JIVariant.CreateOPTIONAL_PARAM() });
            var range = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

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

            range.Put("Value", new JIVariant(new JIArray(newValue)));

            Thread.Sleep(5000);

            for (var j = 0; j < 60; j++) {
                Thread.Sleep(300);
                var temp1 = newValue[0][0];
                var temp2 = newValue[0][1];
                var i = 0;
                for (i = 1; i < newValue.Length; i++) {
                    for (var k = 0; k < newValue[i - 1].Length; k++) {
                        newValue[i - 1][k] = newValue[i][k];
                    }
                }

                newValue[i - 1][0] = temp1;
                newValue[i - 1][1] = temp2;
                // For Excel XP, use: range.setValue2(newValue);
                range.Put("Value", new JIVariant(new JIArray(newValue)));
            }

            outVal2 = sheet.Get("ChartObjects", new object[] { JIVariant.CreateOPTIONAL_PARAM() });
            var chartObjects = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
            outVal2 = chartObjects.CallMethodA("Add", new object[] { 100, new double?(30), 400, new double?(250) });
            //outVal2 = chartObjects.get("Item", new Object[]{new Integer(1)});
            var chartObject = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
            outVal = chartObject.Get("Chart");
            var chart = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);
            chart.CallMethod("SetSourceData", new object[] { range, JIVariant.CreateOPTIONAL_PARAM() });
            Thread.Sleep(5000);

            outVal = sheet.Get("PageSetup");
            var pageSetup = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);
            pageSetup.Put("Orientation", new JIVariant(2));
            pageSetup.Put("Zoom", new JIVariant(100));
            try {
                sheet.CallMethod("PrintOut", new object[] { JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });
            }
            catch (JIException e) {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
                var excepInfo = sheet.LastExcepInfo;
                Console.WriteLine("Error Code in EXCEPINFO: " + excepInfo.ErrorCode);
            }
            _dispatchOfWorkBook.CallMethod("close", new object[] { false, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });
            _dispatch.CallMethod("Quit");
            JISession.DestroySession(_dispatch.AssociatedSession);
        }


        public static void Main(string[] args) {
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