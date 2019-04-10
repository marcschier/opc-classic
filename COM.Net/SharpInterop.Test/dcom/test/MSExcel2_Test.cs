namespace org.jinterop.dcom.test {
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIArray = core.JIArray;
    using JIComServer = core.JIComServer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class MSExcel2_Test
	{

		private JIComServer comServer;
		private IJIDispatch dispatch;
		private IJIComObject unknown;
		private IJIDispatch dispatchOfWorkSheets;
		private IJIDispatch dispatchOfWorkBook;
		private IJIDispatch dispatchOfWorkSheet;
		private JISession session;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSExcel2_Test(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSExcel2_Test(string address, string[] args)
		{
			session = JISession.createSession(args[1],args[2],args[3]);
			session.useSessionSecurity(true);
			comServer = new JIComServer(JIProgId.ValueOf("Excel.Application"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startExcel() throws org.jinterop.dcom.common.JIException
		public virtual void startExcel()
		{
			unknown = comServer.CreateInstance();
			dispatch = (IJIDispatch)JIObjectFactory.narrowObject(unknown.QueryInterface(impls.automation.DispatchFlags.IID));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showExcel() throws org.jinterop.dcom.common.JIException
		public virtual void showExcel()
		{
			var dispId = dispatch.GetIDsOfNames("Visible");
			var variant = new JIVariant(true);
			dispatch.Put(dispId,variant);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void createWorkSheet() throws org.jinterop.dcom.common.JIException
		public virtual void createWorkSheet()
		{
			var dispId = dispatch.GetIDsOfNames("Workbooks");

			var outVal = dispatch.Get(dispId);

			var dispatchOfWorkBooks = (IJIDispatch)JIObjectFactory.narrowObject(outVal.ObjectAsComObject);


			var outVal2 = dispatchOfWorkBooks.CallMethodA("Add",new object[]{JIVariant.CreateOPTIONAL_PARAM()});
			dispatchOfWorkBook = (IJIDispatch)JIObjectFactory.narrowObject(outVal2[0].ObjectAsComObject);

			outVal = dispatchOfWorkBook.Get("Worksheets");

			dispatchOfWorkSheets = (IJIDispatch)JIObjectFactory.narrowObject(outVal.ObjectAsComObject);

			outVal2 = dispatchOfWorkSheets.CallMethodA("Add",new object[]{JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
			dispatchOfWorkSheet = (IJIDispatch)JIObjectFactory.narrowObject(outVal2[0].ObjectAsComObject);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pasteArrayToWorkSheet(int nRow) throws org.jinterop.dcom.common.JIException
		public virtual void pasteArrayToWorkSheet(int nRow)
		{
			var dispId = dispatchOfWorkSheet.GetIDsOfNames("Range");
			var variant = new JIVariant(new JIString("A1:C" + nRow));
			object[] @out = {typeof(JIVariant)};
			var outVal2 = dispatchOfWorkSheet.Get(dispId, new object[]{variant});
			var dispRange = (IJIDispatch)JIObjectFactory.narrowObject(outVal2[0].ObjectAsComObject);

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: JIVariant[][] newValue = new JIVariant[nRow][3];
			JIVariant[][] newValue = RectangularArrays.ReturnRectangularJIVariantArray(nRow, 3);

			for (var i = 0; i < newValue.Length; i++)
			{
				for (var j = 0; j < newValue[i].Length; j++)
				{
					newValue[i][j] = new JIVariant((double)(10.0 * new Random(1).NextDouble()));
				}
			}

			dispRange.Put("Value2", new JIVariant(new JIArray(newValue)));

			try
			{
				Thread.Sleep(20000);
			}
			catch (InterruptedException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			var variant2 = dispRange.Get("Value2");
			var newValue2 = variant2.ObjectAsArray;
			newValue = (JIVariant[][]) newValue2.ArrayInstance;
			for (var i = 0; i < newValue.Length; i++)
			{
				for (var j = 0; j < newValue[i].Length; j++)
				{
					Console.Write(newValue[i][j] + "\t");
				}
				Console.WriteLine();
			}

			//Now write the value down
			dispRange.Put("Value2", new JIVariant(newValue2));

			try
			{
				Thread.Sleep(20000);
			}
			catch (InterruptedException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			dispatchOfWorkBook.CallMethod("close", new object[] {false, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM()});
			dispatch.CallMethod("Quit");
			JISession.destroySession(session);

		}

		public static void Main(string[] args)
		{

			try
			{

				Log.Logger.Level = Level.FINEST;

				if (args.Length < 4)
				{
					Console.WriteLine("Please provide address domain username password");
					return;
				}

				//JISystem.setInBuiltLogHandler(false);
				//Logger l = Logger.getLogger("org.jinterop");
				//l.setLevel(Level.FINEST);

				var nRow = 600;

				if (args.Length > 4)
				{
					try
					{
						nRow = int.Parse(args[4]);
					}
					catch (System.FormatException)
					{

					}
				}

				var test = new MSExcel2_Test(args[0],args);

				test.startExcel();
				test.showExcel();
				test.createWorkSheet();

				test.pasteArrayToWorkSheet(nRow);

			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

		}

	}

}