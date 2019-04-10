namespace org.jinterop.dcom.test {



    using JIException = common.JIException;
    using IJIComObject = core.IJIComObject;
    using JIArray = core.JIArray;
    using JIComServer = core.JIComServer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;
    using JIExcepInfo = impls.automation.JIExcepInfo;


    public class MSExcel3
	{


		private JIComServer comServer;
		private IJIDispatch dispatch;
		private IJIComObject unknown;
		private IJIDispatch dispatchOfWorkSheets;
		private IJIDispatch dispatchOfWorkBook;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSExcel3(String address, String args[]) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSExcel3(string address, string[] args)
		{
			var session = JISession.createSession(args[1],args[2],args[3]);
			comServer = new JIComServer(JIProgId.ValueOf("Excel.Application"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startExcel() throws org.jinterop.dcom.common.JIException
		public virtual void startExcel()
		{
			unknown = comServer.CreateInstance();
			dispatch = (IJIDispatch)JIObjectFactory.NarrowObject((IJIComObject)unknown.QueryInterface(impls.automation.DispatchFlags.IID));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showExcel() throws org.jinterop.dcom.common.JIException
		public virtual void showExcel()
		{
			var dispId = dispatch.GetIDsOfNames("Visible");
			var variant = new JIVariant(true);
			dispatch.Put(dispId,variant);

			dispatch.Put("DisplayAlerts",new JIVariant(true));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void createWorkSheet() throws org.jinterop.dcom.common.JIException
		public virtual void createWorkSheet()
		{
			var dispId = dispatch.GetIDsOfNames("Workbooks");

			var outVal = dispatch.Get(dispId);
			var dispatchOfWorkBooks = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);


			var outVal2 = dispatchOfWorkBooks.CallMethodA("Open",new object[]{new JIString("C:\\temp\\chart.xls"),true,true,JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
			dispatchOfWorkBook = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

			outVal = dispatchOfWorkBook.Get("Worksheets");
			dispatchOfWorkSheets = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);

			outVal2 = dispatchOfWorkSheets.Get("Item", new object[]{new JIVariant(1)});
			var sheet = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
			outVal2 = sheet.Get("Range",new object[]{new JIString("A1:B19"),JIVariant.CreateOPTIONAL_PARAM()});
			var range = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

			 int?[][] newValue = {
				 new Nullable[] { 121, 117},
				 new Nullable[] { 111, 156},
				 new Nullable[] { 132, 138},
				 new Nullable[] { 116, 119},
				 new Nullable[] { 148, 126},
				 new Nullable[] { 163, 143},
				 new Nullable[] { 174, 135},
				 new Nullable[] { 136, 142},
				 new Nullable[] { 142, 163},
				 new Nullable[] { 121, 117},
				 new Nullable[] { 111, 156},
				 new Nullable[] { 132, 138},
				 new Nullable[] { 116, 119},
				 new Nullable[] { 148, 126},
				 new Nullable[] { 163, 143},
				 new Nullable[] { 174, 135},
				 new Nullable[] { 136, 142},
				 new Nullable[] { 142, 163},
				 new Nullable[] { 121, 117 }
			 };

			range.Put("Value", new JIVariant(new JIArray(newValue)));

			try
			{
				Thread.Sleep(5000);
			}
			catch (InterruptedException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			for (var j = 0; j < 60; j++)
			{
				try
				{
					Thread.Sleep(300);
				}
				catch (InterruptedException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				var temp1 = newValue[0][0];
				var temp2 = newValue[0][1];
				var i = 0;
				for (i = 1; i < newValue.Length; i++)
				{
				  for (var k = 0; k < newValue[i - 1].Length; k++)
				  {
					newValue[i - 1][k] = newValue[i][k];
				  }
				}

				newValue[i - 1][0] = temp1;
				newValue[i - 1][1] = temp2;
			   // For Excel XP, use: range.setValue2(newValue);
				range.Put("Value", new JIVariant(new JIArray(newValue)));
			}

			outVal2 = sheet.Get("ChartObjects",new object[]{JIVariant.CreateOPTIONAL_PARAM()});
			var chartObjects = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
			outVal2 = chartObjects.CallMethodA("Add",new object[]{ 100, new double?(30), 400, new double?(250)});
			//outVal2 = chartObjects.get("Item", new Object[]{new Integer(1)});
			var chartObject = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
			outVal = chartObject.Get("Chart");
			var chart = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);
			chart.CallMethod("SetSourceData",new object[]{range,JIVariant.CreateOPTIONAL_PARAM()});
			try
			{
				Thread.Sleep(5000);
			}
			catch (InterruptedException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			outVal = sheet.Get("PageSetup");
			var pageSetup = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);
			pageSetup.Put("Orientation",new JIVariant(2));
			pageSetup.Put("Zoom",new JIVariant(100));
			try
			{
				sheet.CallMethod("PrintOut",new object[]{JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
			}
			catch (JIException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				var excepInfo = sheet.LastExcepInfo;
				Console.WriteLine("Error Code in EXCEPINFO: " + excepInfo.ErrorCode);
			}
			dispatchOfWorkBook.CallMethod("close",new object[]{false,JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
			dispatch.CallMethod("Quit");
			JISession.destroySession(dispatch.AssociatedSession);
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
					var test = new MSExcel3(args[0],args);
					test.startExcel();
					test.showExcel();
					test.createWorkSheet();
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