using System;
using System.Threading;

namespace org.jinterop.dcom.test {


	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIArray = org.jinterop.dcom.core.JIArray;
	using JIComServer = org.jinterop.dcom.core.JIComServer;
	using JIProgId = org.jinterop.dcom.core.JIProgId;
	using JISession = org.jinterop.dcom.core.JISession;
	using JIString = org.jinterop.dcom.core.JIString;
	using JIVariant = org.jinterop.dcom.core.JIVariant;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
	using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

	public class MSExcel2_Test {

		private JIComServer ComServer = null;
		private IJIDispatch Dispatch = null;
		private IJIComObject Unknown = null;
		private IJIDispatch DispatchOfWorkSheets = null;
		private IJIDispatch DispatchOfWorkBook = null;
		private IJIDispatch DispatchOfWorkSheet = null;
		private JISession Session = null;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSExcel2_Test(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSExcel2_Test(string address, string[] args) {
			Session = JISession.CreateSession(args[1],args[2],args[3]);
			Session.UseSessionSecurity(true);
			ComServer = new JIComServer(JIProgId.ValueOf("Excel.Application"),address,Session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startExcel() throws org.jinterop.dcom.common.JIException
		public virtual void StartExcel() {
			Unknown = ComServer.CreateInstance();
			Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(Unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showExcel() throws org.jinterop.dcom.common.JIException
		public virtual void ShowExcel() {
			int dispId = Dispatch.GetIDsOfNames("Visible");
			JIVariant variant = new JIVariant(true);
			Dispatch.Put(dispId,variant);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void createWorkSheet() throws org.jinterop.dcom.common.JIException
		public virtual void CreateWorkSheet() {
			int dispId = Dispatch.GetIDsOfNames("Workbooks");

			JIVariant outVal = Dispatch.Get(dispId);

			IJIDispatch dispatchOfWorkBooks = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);


			JIVariant[] outVal2 = dispatchOfWorkBooks.CallMethodA("Add",new object[]{ JIVariant.OPTIONAL_PARAM() });
			DispatchOfWorkBook = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

			outVal = DispatchOfWorkBook.Get("Worksheets");

			DispatchOfWorkSheets = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);

			outVal2 = DispatchOfWorkSheets.CallMethodA("Add",new object[]{ JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });
			DispatchOfWorkSheet = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pasteArrayToWorkSheet(int nRow) throws org.jinterop.dcom.common.JIException
		public virtual void PasteArrayToWorkSheet(int nRow) {
			int dispId = DispatchOfWorkSheet.GetIDsOfNames("Range");
			JIVariant variant = new JIVariant(new JIString("A1:C" + nRow));
			object[] @out = new object[]{ typeof(JIVariant) };
			JIVariant[] outVal2 = DispatchOfWorkSheet.Get(dispId, new object[]{ variant });
			IJIDispatch dispRange = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: JIVariant[][] newValue = new JIVariant[nRow][3];
			JIVariant[][] newValue = RectangularArrays.ReturnRectangularJIVariantArray(nRow, 3);

			for (int i = 0; i < newValue.Length; i++) {
				for (int j = 0; j < newValue[i].Length; j++) {
					newValue[i][j] = new JIVariant((double)(10.0 * new Random(1).NextDouble()));
				}
			}

			dispRange.Put("Value2", new JIVariant(new JIArray(newValue)));

			try {
				Thread.Sleep(20000);
			}
			catch (InterruptedException e) {
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			JIVariant variant2 = dispRange.Get("Value2");
			JIArray newValue2 = variant2.ObjectAsArray;
			newValue = (JIVariant[][]) newValue2.ArrayInstance;
			for (int i = 0; i < newValue.Length; i++) {
				for (int j = 0; j < newValue[i].Length; j++) {
					Console.Write(newValue[i][j] + "\t");
				}
				Console.WriteLine();
			}

			//Now write the value down
			dispRange.Put("Value2", new JIVariant(newValue2));

			try {
				Thread.Sleep(20000);
			}
			catch (InterruptedException e) {
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			DispatchOfWorkBook.CallMethod("close", new object[] { false, JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM() });
			Dispatch.CallMethod("Quit");
			JISession.DestroySession(Session);

		}

		public static void Main(string[] args) {

			try {

				JISystem.Logger.Level = Level.FINEST;

				if (args.Length < 4) {
					Console.WriteLine("Please provide address domain username password");
					return;
				}

				//JISystem.setInBuiltLogHandler(false);
				//Logger l = Logger.getLogger("org.jinterop");
				//l.setLevel(Level.FINEST);

				int nRow = 600;

				if (args.Length > 4) {
					try {
						nRow = int.Parse(args[4]);
					}
					catch (System.FormatException) {

					}
				}

				MSExcel2_Test test = new MSExcel2_Test(args[0],args);

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