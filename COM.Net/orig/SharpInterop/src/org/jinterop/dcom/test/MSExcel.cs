using System;

namespace org.jinterop.dcom.test {

	using JIException = org.jinterop.dcom.common.JIException;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIComServer = org.jinterop.dcom.core.JIComServer;
	using JIProgId = org.jinterop.dcom.core.JIProgId;
	using JISession = org.jinterop.dcom.core.JISession;
	using JIString = org.jinterop.dcom.core.JIString;
	using JIVariant = org.jinterop.dcom.core.JIVariant;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
	using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;
	using IJITypeInfo = org.jinterop.dcom.impls.automation.IJITypeInfo;

	public class MSExcel {

		private readonly int XlWorksheet = -4167;
		private readonly int XlXYScatterLinesNoMarkers = 75;
		private readonly int XlColumns = 2;

		private JIComServer ComServer = null;
		private IJIDispatch Dispatch = null;
		private IJIComObject Unknown = null;
		private IJIDispatch DispatchOfWorkSheet = null;
		private IJIDispatch DispatchOfWorkBook = null;
		private JISession Session = null;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSExcel(String address,String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSExcel(string address, string[] args) {
			Session = JISession.CreateSession(args[1],args[2],args[3]);
			ComServer = new JIComServer(JIProgId.ValueOf("Excel.Application"),address,Session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startExcel() throws org.jinterop.dcom.common.JIException
		public virtual void StartExcel() {
			Unknown = ComServer.CreateInstance();
			Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(Unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
			IJITypeInfo typeInfo = Dispatch.GetTypeInfo(0);
			typeInfo.GetFuncDesc(0);
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
			object[] @out = new object[]{ typeof(JIVariant) };
			JIVariant[] outVal2 = null;
			JIVariant outVal = Dispatch.Get(dispId);
			DispatchOfWorkBook = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);


			int[] dispIds = DispatchOfWorkBook.GetIDsOfNames(new string[]{ "Add","Template" });

			@out = new object[]{ typeof(JIVariant) };
			dispId = DispatchOfWorkBook.GetIDsOfNames("Add");

			outVal2 = DispatchOfWorkBook.CallMethodA(dispId,new object[]{ new int?(XlWorksheet) });
			DispatchOfWorkBook = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

			dispId = DispatchOfWorkBook.GetIDsOfNames("Worksheets");
			JIVariant variant = new JIVariant((short)1);
			@out = new object[]{ typeof(JIVariant) };
			outVal2 = DispatchOfWorkBook.Get(dispId,new object[]{ variant });

			DispatchOfWorkSheet = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void pasteStringToWorkSheet() throws org.jinterop.dcom.common.JIException
		public virtual void PasteStringToWorkSheet() {
			int dispId = DispatchOfWorkSheet.GetIDsOfNames("Range");

			JIVariant variant = new JIVariant(new JIString("A1"));
			object[] @out = new object[]{ typeof(JIVariant) };
			JIVariant outVal ; JIVariant[] outVal2 = null;
			outVal2 = DispatchOfWorkSheet.Get(dispId, new object[]{ variant });

			IJIDispatch dispRange = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

			dispId = dispRange.GetIDsOfNames("Select");
			@out = new object[]{ typeof(JIVariant) };
			outVal = dispRange.Get(dispId);

			dispId = DispatchOfWorkBook.GetIDsOfNames("ActiveSheet");
			@out = new object[]{ typeof(JIVariant) };
			outVal = DispatchOfWorkBook.Get(dispId);

			IJIDispatch dispatchActiveSheet = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);
			dispId = dispatchActiveSheet.GetIDsOfNames("Paste");
			@out = new object[]{ typeof(JIVariant) };
			try {
				outVal = dispatchActiveSheet.CallMethodA(dispId);
			}
			catch (JIException e) {
				throw e;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void createXYChart() throws org.jinterop.dcom.common.JIException
		public virtual void CreateXYChart() {
			//column 2.
			int dispId = DispatchOfWorkSheet.GetIDsOfNames("Columns");

			double? cols = new double?(2);
			object[] @out = new object[]{ typeof(JIVariant) };
			JIVariant outVal ; JIVariant[] outVal2 = null;
			outVal2 = DispatchOfWorkSheet.Get(dispId,new object[]{ cols });


			IJIDispatch dispatchRange = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

			dispId = DispatchOfWorkBook.GetIDsOfNames("Charts");
			@out = new object[]{ typeof(JIVariant) };
			outVal = DispatchOfWorkBook.Get(dispId);

			IJIDispatch dispatchChart = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);



			dispId = dispatchChart.GetIDsOfNames("Add");
			@out = new object[]{ typeof(JIVariant) };
			outVal = dispatchChart.CallMethodA(dispId);

			dispatchChart = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);

			dispId = DispatchOfWorkBook.GetIDsOfNames("ActiveChart");
			@out = new object[]{ typeof(JIVariant) };

			outVal = DispatchOfWorkBook.Get(dispId);

			IJIDispatch dispatchActiveChart = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);

			dispId = dispatchActiveChart.GetIDsOfNames("ChartType");
			@out = new object[]{ typeof(JIVariant) };

			dispatchActiveChart.Put(dispId,new JIVariant((short)XlXYScatterLinesNoMarkers));

			int[] dispIds = dispatchActiveChart.GetIDsOfNames(new string[]{ "SetSourceData","Source","PlotBy" });

			dispId = dispatchActiveChart.GetIDsOfNames("SetSourceData");
			@out = new object[]{ typeof(JIVariant) };
			outVal2 = dispatchActiveChart.CallMethodA(dispId,new object[]{ dispatchRange,new short?((short)XlColumns) },new int[]{ dispIds[1],dispIds[2] }); //invoke(dispIds[0],IJIDispatch.DISPATCH_METHOD,new Object[]{variant,new JIArray(new Integer[]{new Integer(dispIds[1]),new Integer(dispIds[2])},true),null,null,null},null);

			JISession.DestroySession(Session);
		}

		public static void Main(string[] args) {

			try {
					if (args.Length < 4) {
						Console.WriteLine("Please provide address domain username password");
						return;
					}
					MSExcel test = new MSExcel(args[0],args);
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