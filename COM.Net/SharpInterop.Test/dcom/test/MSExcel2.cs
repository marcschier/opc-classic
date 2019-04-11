namespace org.jinterop.dcom.test {
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.impls.automation;
    using System;
    using System.Threading;
    public class MSExcel2 {

        private readonly JIComServer _comServer;
        private IJIDispatch _dispatch;
        private IComObject _unknown;
        private IJIDispatch _dispatchOfWorkSheets;
        private IJIDispatch _dispatchOfWorkBook;
        private IJIDispatch _dispatchOfWorkSheet;
        private readonly JISession _session;

        public MSExcel2(string address, string[] args) {
            _session = JISession.CreateSession(args[1], args[2], args[3]);
            //        session.useSessionSecurity(true);
            _comServer = new JIComServer(JIProgId.ValueOf("Excel.Application"), address, _session);
        }


        public void StartExcel() {
            _unknown = _comServer.CreateInstance();
            _dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(_unknown.QueryInterface(Interfaces.IID_IDispatch));
        }


        public void ShowExcel() {
            var dispId = _dispatch.GetIDsOfNames("Visible");
            var variant = new JIVariant(true);
            _dispatch.Put(dispId, variant);
        }


        public void CreateWorkSheet() {
            var dispId = _dispatch.GetIDsOfNames("Workbooks");

            var outVal = _dispatch.Get(dispId);

            var dispatchOfWorkBooks = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);


            var outVal2 = dispatchOfWorkBooks.CallMethodA("Add", new object[] { JIVariant.CreateOPTIONAL_PARAM() });
            _dispatchOfWorkBook = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

            outVal = _dispatchOfWorkBook.Get("Worksheets");

            _dispatchOfWorkSheets = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);

            outVal2 = _dispatchOfWorkSheets.CallMethodA("Add", new object[] { JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });
            _dispatchOfWorkSheet = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
        }


        public void PasteArrayToWorkSheet() {
            var dispId = _dispatchOfWorkSheet.GetIDsOfNames("Range");
            var variant = new JIVariant(new JIString("A1:C3"));
            object[] @out = { typeof(JIVariant) };
            var outVal2 = _dispatchOfWorkSheet.Get(dispId, new object[] { variant });
            var dispRange = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);


            JIVariant[][] newValue = {
                  new JIVariant[] {new JIVariant(new JIString("defe")), new JIVariant(false), new JIVariant(98765.0 / 12345.0) },
                  new JIVariant[] {new JIVariant(DateTime.Now), new JIVariant(5454),new JIVariant((float)(22.0 / 7.0))},
                  new JIVariant[] {new JIVariant(true), new JIVariant(new JIString("dffe")),new JIVariant(DateTime.Now)}
              };

            // implement safe array XxX dimension

            dispRange.Put("Value2", new JIVariant(new JIArray(newValue)));

            Thread.Sleep(10000);

            var variant2 = dispRange.Get("Value2");
            var newValue2 = variant2.ObjectAsArray;
            newValue = (JIVariant[][])newValue2.ArrayInstance;
            for (var i = 0; i < newValue.Length; i++) {
                for (var j = 0; j < newValue[i].Length; j++) {
                    Console.Write(newValue[i][j] + "\t");
                }
                Console.WriteLine();
            }

            _dispatchOfWorkBook.CallMethod("close", new object[] { false, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM() });
            _dispatch.CallMethod("Quit");
            JISession.DestroySession(_session);
        }


        public static void Main(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }


                var test = new MSExcel2(args[0], args);
                test.StartExcel();
                test.ShowExcel();
                test.CreateWorkSheet();
                test.PasteArrayToWorkSheet();
            }
            catch (Exception e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }





    }

}