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
			dispatch = (IJIDispatch)JIObjectFactory.narrowObject(unknown.QueryInterface(impls.automation.IJIDispatch_Fields.IID));
			var typeInfo = dispatch.getTypeInfo(0);
			typeInfo.getFuncDesc(0);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showExcel() throws org.jinterop.dcom.common.JIException
		public virtual void showExcel()
		{
			var dispId = dispatch.getIDsOfNames("Visible");
			var variant = new JIVariant(true);
			dispatch.put(dispId,variant);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void createWorkSheet() throws org.jinterop.dcom.common.JIException
		public virtual void createWorkSheet()
		{
			var dispId = dispatch.getIDsOfNames("Workbooks");
			object[] @out = {typeof(JIVariant)};
			JIVariant[] outVal2 = null;
			var outVal = dispatch.get(dispId);
			dispatchOfWorkBook = (IJIDispatch)JIObjectFactory.narrowObject(outVal.ObjectAsComObject);


			var dispIds = dispatchOfWorkBook.getIDsOfNames(new string[]{"Add","Template"});

			@out = new object[]{typeof(JIVariant)};
			dispId = dispatchOfWorkBook.getIDsOfNames("Add");

			outVal2 = dispatchOfWorkBook.callMethodA(dispId,new object[]{ xlWorksheet });
			dispatchOfWorkBook = (IJIDispatch)JIObjectFactory.narrowObject(outVal2[0].ObjectAsComObject);

			dispId = dispatchOfWorkBook.getIDsOfNames("Worksheets");
			var variant = new JIVariant((short)1);
			@out = new object[]{typeof(JIVariant)};
			outVal2 = dispatchOfWorkBook.get(dispId,new object[]{variant});

			dispatchOfWorkSheet = (IJIDispatch)JIObjectFactory.narrowObject(outVal2[0].ObjectAsComObject);

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pasteStringToWorkSheet() throws org.jinterop.dcom.common.JIException
		public virtual void pasteStringToWorkSheet()
		{
			var dispId = dispatchOfWorkSheet.getIDsOfNames("Range");

			var variant = new JIVariant(new JIString("A1"));
			object[] @out = {typeof(JIVariant)};
			JIVariant outVal ; JIVariant[] outVal2 = null;
			outVal2 = dispatchOfWorkSheet.get(dispId, new object[]{variant});

			var dispRange = (IJIDispatch)JIObjectFactory.narrowObject(outVal2[0].ObjectAsComObject);

			dispId = dispRange.getIDsOfNames("Select");
			@out = new object[]{typeof(JIVariant)};
			outVal = dispRange.get(dispId);

			dispId = dispatchOfWorkBook.getIDsOfNames("ActiveSheet");
			@out = new object[]{typeof(JIVariant)};
			outVal = dispatchOfWorkBook.get(dispId);

			var dispatchActiveSheet = (IJIDispatch)JIObjectFactory.narrowObject(outVal.ObjectAsComObject);
			dispId = dispatchActiveSheet.getIDsOfNames("Paste");
			@out = new object[]{typeof(JIVariant)};
			try
			{
				outVal = dispatchActiveSheet.callMethodA(dispId);
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
			var dispId = dispatchOfWorkSheet.getIDsOfNames("Columns");

			var cols = new double?(2);
			object[] @out = {typeof(JIVariant)};
			JIVariant outVal ; JIVariant[] outVal2 = null;
			outVal2 = dispatchOfWorkSheet.get(dispId,new object[]{cols});


			var dispatchRange = (IJIDispatch)JIObjectFactory.narrowObject(outVal2[0].ObjectAsComObject);

			dispId = dispatchOfWorkBook.getIDsOfNames("Charts");
			@out = new object[]{typeof(JIVariant)};
			outVal = dispatchOfWorkBook.get(dispId);

			var dispatchChart = (IJIDispatch)JIObjectFactory.narrowObject(outVal.ObjectAsComObject);



			dispId = dispatchChart.getIDsOfNames("Add");
			@out = new object[]{typeof(JIVariant)};
			outVal = dispatchChart.callMethodA(dispId);

			dispatchChart = (IJIDispatch)JIObjectFactory.narrowObject(outVal.ObjectAsComObject);

			dispId = dispatchOfWorkBook.getIDsOfNames("ActiveChart");
			@out = new object[]{typeof(JIVariant)};

			outVal = dispatchOfWorkBook.get(dispId);

			var dispatchActiveChart = (IJIDispatch)JIObjectFactory.narrowObject(outVal.ObjectAsComObject);

			dispId = dispatchActiveChart.getIDsOfNames("ChartType");
			@out = new object[]{typeof(JIVariant)};

			dispatchActiveChart.put(dispId,new JIVariant((short)xlXYScatterLinesNoMarkers));

			var dispIds = dispatchActiveChart.getIDsOfNames(new string[]{"SetSourceData","Source","PlotBy"});

			dispId = dispatchActiveChart.getIDsOfNames("SetSourceData");
			@out = new object[]{typeof(JIVariant)};
			outVal2 = dispatchActiveChart.callMethodA(dispId,new object[]{dispatchRange, (short)xlColumns },new int[]{dispIds[1],dispIds[2]}); //invoke(dispIds[0],IJIDispatch.DISPATCH_METHOD,new Object[]{variant,new JIArray(new Integer[]{new Integer(dispIds[1]),new Integer(dispIds[2])},true),null,null,null},null);

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