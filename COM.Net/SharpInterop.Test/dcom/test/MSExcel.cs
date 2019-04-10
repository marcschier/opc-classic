namespace org.jinterop.dcom.test {

    using JIException = common.JIException;
    using IJIComObject = core.IJIComObject;
    using JIComServer = core.JIComServer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;
    using IJITypeInfo = impls.automation.IJITypeInfo;

    public class MSExcel
	{

		private readonly int xlWorksheet = -4167;
		private readonly int xlXYScatterLinesNoMarkers = 75;
		private readonly int xlColumns = 2;

		private JIComServer comServer;
		private IJIDispatch dispatch;
		private IJIComObject unknown;
		private IJIDispatch dispatchOfWorkSheet;
		private IJIDispatch dispatchOfWorkBook;
		private readonly JISession session;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSExcel(String address,String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSExcel(string address, string[] args)
		{
			session = JISession.createSession(args[1],args[2],args[3]);
			comServer = new JIComServer(JIProgId.ValueOf("Excel.Application"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startExcel() throws org.jinterop.dcom.common.JIException
		public virtual void startExcel()
		{
			unknown = comServer.CreateInstance();
			dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(unknown.QueryInterface(impls.automation.DispatchFlags.IID));
			var typeInfo = dispatch.GetTypeInfo(0);
			typeInfo.GetFuncDesc(0);
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
			object[] @out = {typeof(JIVariant)};
			JIVariant[] outVal2 = null;
			var outVal = dispatch.Get(dispId);
			dispatchOfWorkBook = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);


			var dispIds = dispatchOfWorkBook.GetIDsOfNames(new string[]{"Add","Template"});

			@out = new object[]{typeof(JIVariant)};
			dispId = dispatchOfWorkBook.GetIDsOfNames("Add");

			outVal2 = dispatchOfWorkBook.CallMethodA(dispId,new object[]{ xlWorksheet });
			dispatchOfWorkBook = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

			dispId = dispatchOfWorkBook.GetIDsOfNames("Worksheets");
			var variant = new JIVariant((short)1);
			@out = new object[]{typeof(JIVariant)};
			outVal2 = dispatchOfWorkBook.Get(dispId,new object[]{variant});

			dispatchOfWorkSheet = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pasteStringToWorkSheet() throws org.jinterop.dcom.common.JIException
		public virtual void pasteStringToWorkSheet()
		{
			var dispId = dispatchOfWorkSheet.GetIDsOfNames("Range");

			var variant = new JIVariant(new JIString("A1"));
			object[] @out = {typeof(JIVariant)};
			JIVariant outVal ; JIVariant[] outVal2 = null;
			outVal2 = dispatchOfWorkSheet.Get(dispId, new object[]{variant});

			var dispRange = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

			dispId = dispRange.GetIDsOfNames("Select");
			@out = new object[]{typeof(JIVariant)};
			outVal = dispRange.Get(dispId);

			dispId = dispatchOfWorkBook.GetIDsOfNames("ActiveSheet");
			@out = new object[]{typeof(JIVariant)};
			outVal = dispatchOfWorkBook.Get(dispId);

			var dispatchActiveSheet = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);
			dispId = dispatchActiveSheet.GetIDsOfNames("Paste");
			@out = new object[]{typeof(JIVariant)};
			try
			{
				outVal = dispatchActiveSheet.CallMethodA(dispId);
			}
			catch (JIException e)
			{
				throw e;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void createXYChart() throws org.jinterop.dcom.common.JIException
		public virtual void createXYChart()
		{
			//column 2.
			var dispId = dispatchOfWorkSheet.GetIDsOfNames("Columns");

			var cols = new double?(2);
			object[] @out = {typeof(JIVariant)};
			JIVariant outVal ; JIVariant[] outVal2 = null;
			outVal2 = dispatchOfWorkSheet.Get(dispId,new object[]{cols});


			var dispatchRange = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

			dispId = dispatchOfWorkBook.GetIDsOfNames("Charts");
			@out = new object[]{typeof(JIVariant)};
			outVal = dispatchOfWorkBook.Get(dispId);

			var dispatchChart = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);



			dispId = dispatchChart.GetIDsOfNames("Add");
			@out = new object[]{typeof(JIVariant)};
			outVal = dispatchChart.CallMethodA(dispId);

			dispatchChart = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);

			dispId = dispatchOfWorkBook.GetIDsOfNames("ActiveChart");
			@out = new object[]{typeof(JIVariant)};

			outVal = dispatchOfWorkBook.Get(dispId);

			var dispatchActiveChart = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);

			dispId = dispatchActiveChart.GetIDsOfNames("ChartType");
			@out = new object[]{typeof(JIVariant)};

			dispatchActiveChart.Put(dispId,new JIVariant((short)xlXYScatterLinesNoMarkers));

			var dispIds = dispatchActiveChart.GetIDsOfNames(new string[]{"SetSourceData","Source","PlotBy"});

			dispId = dispatchActiveChart.GetIDsOfNames("SetSourceData");
			@out = new object[]{typeof(JIVariant)};
			outVal2 = dispatchActiveChart.CallMethodA(dispId,new object[]{dispatchRange, (short)xlColumns },new int[]{dispIds[1],dispIds[2]}); //invoke(dispIds[0],IJIDispatch.DISPATCH_METHOD,new Object[]{variant,new JIArray(new Integer[]{new Integer(dispIds[1]),new Integer(dispIds[2])},true),null,null,null},null);

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
					var test = new MSExcel(args[0],args);
					test.startExcel();
					test.showExcel();
					test.createWorkSheet();
					test.pasteStringToWorkSheet();
					test.createXYChart();
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