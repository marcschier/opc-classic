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

    public class MSExcel2
	{

		private JIComServer comServer;
		private IJIDispatch dispatch;
		private IJIComObject unknown;
		private IJIDispatch dispatchOfWorkSheets;
		private IJIDispatch dispatchOfWorkBook;
		private IJIDispatch dispatchOfWorkSheet;
		private readonly JISession session;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSExcel2(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSExcel2(string address, string[] args)
		{
			session = JISession.createSession(args[1],args[2],args[3]);
	//		session.useSessionSecurity(true);
			comServer = new JIComServer(JIProgId.ValueOf("Excel.Application"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startExcel() throws org.jinterop.dcom.common.JIException
		public virtual void startExcel()
		{
			unknown = comServer.CreateInstance();
			dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(unknown.QueryInterface(impls.automation.DispatchFlags.IID));
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

			var dispatchOfWorkBooks = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);


			var outVal2 = dispatchOfWorkBooks.CallMethodA("Add",new object[]{JIVariant.CreateOPTIONAL_PARAM()});
			dispatchOfWorkBook = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

			outVal = dispatchOfWorkBook.Get("Worksheets");

			dispatchOfWorkSheets = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);

			outVal2 = dispatchOfWorkSheets.CallMethodA("Add",new object[]{JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
			dispatchOfWorkSheet = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pasteArrayToWorkSheet() throws org.jinterop.dcom.common.JIException
		public virtual void pasteArrayToWorkSheet()
		{
			var dispId = dispatchOfWorkSheet.GetIDsOfNames("Range");
			var variant = new JIVariant(new JIString("A1:C3"));
			object[] @out = {typeof(JIVariant)};
			var outVal2 = dispatchOfWorkSheet.Get(dispId, new object[]{variant});
			var dispRange = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);


			  JIVariant[][] newValue = {
				  new JIVariant[] {new JIVariant(new JIString("defe")), new JIVariant(false), new JIVariant((double)(98765.0 / 12345.0))},
				  new JIVariant[] {new JIVariant(DateTime.Now), new JIVariant((int)5454),new JIVariant((float)(22.0 / 7.0))},
				  new JIVariant[] {new JIVariant(true), new JIVariant(new JIString("dffe")),new JIVariant(DateTime.Now)}
			  };

			 // implement safe array XxX dimension

			dispRange.Put("Value2", new JIVariant(new JIArray(newValue)));

			try
			{
				Thread.Sleep(10000);
			}
			catch (InterruptedException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			var variant2 = dispRange.Get("Value2");
			var newValue2 = variant2.ObjectAsArray;
			newValue = (JIVariant[][])newValue2.ArrayInstance;
			for (var i = 0; i < newValue.Length; i++)
			{
				for (var j = 0; j < newValue[i].Length; j++)
				{
				  Console.Write(newValue[i][j] + "\t");
				}
				Console.WriteLine();
			}

			dispatchOfWorkBook.CallMethod("close",new object[]{false,JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
			dispatch.CallMethod("Quit");
			JISession.destroySession(session);
		}


		public static void Main(string[] args)
		{

			try
			{
					if (args.Length < 4)
					{
						Console.WriteLine("Please provide address domain username password");
						return;
					}
					JISystem.InBuiltLogHandler = false;
					Logger l = Logger.getLogger("org.jinterop");
					l.Level = Level.FINEST;
					var test = new MSExcel2(args[0],args);
					test.startExcel();
					test.showExcel();
					test.createWorkSheet();
					test.pasteArrayToWorkSheet();
			}
				catch (Exception e)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
		}





	}

}