using System;
using System.Threading;

namespace org.jinterop.dcom.test {



	using JIException = org.jinterop.dcom.common.JIException;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIArray = org.jinterop.dcom.core.JIArray;
	using JIComServer = org.jinterop.dcom.core.JIComServer;
	using JIProgId = org.jinterop.dcom.core.JIProgId;
	using JISession = org.jinterop.dcom.core.JISession;
	using JIString = org.jinterop.dcom.core.JIString;
	using JIVariant = org.jinterop.dcom.core.JIVariant;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
	using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;
	using JIExcepInfo = org.jinterop.dcom.impls.automation.JIExcepInfo;


	public class MSExcel3 {


		private JIComServer ComServer = null;
		private IJIDispatch Dispatch = null;
		private IJIComObject Unknown = null;
		private IJIDispatch DispatchOfWorkSheets = null;
		private IJIDispatch DispatchOfWorkBook = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSExcel3(String address, String args[]) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSExcel3(string address, string[] args) {
			JISession session = JISession.CreateSession(args[1],args[2],args[3]);
			ComServer = new JIComServer(JIProgId.ValueOf("Excel.Application"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startExcel() throws org.jinterop.dcom.common.JIException
		public virtual void StartExcel() {
			Unknown = ComServer.CreateInstance();
			Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject((IJIComObject)Unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showExcel() throws org.jinterop.dcom.common.JIException
		public virtual void ShowExcel() {
			int dispId = Dispatch.GetIDsOfNames("Visible");
			JIVariant variant = new JIVariant(true);
			Dispatch.Put(dispId,variant);

			Dispatch.Put("DisplayAlerts",new JIVariant(true));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void createWorkSheet() throws org.jinterop.dcom.common.JIException
		public virtual void CreateWorkSheet() {
			int dispId = Dispatch.GetIDsOfNames("Workbooks");

			JIVariant outVal = Dispatch.Get(dispId);
			IJIDispatch dispatchOfWorkBooks = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);


			JIVariant[] outVal2 = dispatchOfWorkBooks.CallMethodA("Open",new object[]{ new JIString("C:\\temp\\chart.xls"),true,true,JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });
			DispatchOfWorkBook = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

			outVal = DispatchOfWorkBook.Get("Worksheets");
			DispatchOfWorkSheets = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);

			outVal2 = DispatchOfWorkSheets.Get("Item", new object[]{ new JIVariant(1) });
			IJIDispatch sheet = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
			outVal2 = sheet.Get("Range",new object[]{ new JIString("A1:B19"),JIVariant.OPTIONAL_PARAM() });
			IJIDispatch range = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);

			 int?[][] newValue = new int?[][] {
				 new Nullable[] { new int?(121), new int?(117) },
				 new Nullable[] { new int?(111), new int?(156) },
				 new Nullable[] { new int?(132), new int?(138) },
				 new Nullable[] { new int?(116), new int?(119) },
				 new Nullable[] { new int?(148), new int?(126) },
				 new Nullable[] { new int?(163), new int?(143) },
				 new Nullable[] { new int?(174), new int?(135) },
				 new Nullable[] { new int?(136), new int?(142) },
				 new Nullable[] { new int?(142), new int?(163) },
				 new Nullable[] { new int?(121), new int?(117) },
				 new Nullable[] { new int?(111), new int?(156) },
				 new Nullable[] { new int?(132), new int?(138) },
				 new Nullable[] { new int?(116), new int?(119) },
				 new Nullable[] { new int?(148), new int?(126) },
				 new Nullable[] { new int?(163), new int?(143) },
				 new Nullable[] { new int?(174), new int?(135) },
				 new Nullable[] { new int?(136), new int?(142) },
				 new Nullable[] { new int?(142), new int?(163) },
				 new Nullable[] { new int?(121), new int?(117) }
			 };

			range.Put("Value", new JIVariant(new JIArray(newValue)));

			try {
				Thread.Sleep(5000);
			}
			catch (InterruptedException e) {
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			for (int j = 0; j < 60; j++) {
				try {
					Thread.Sleep(300);
				}
				catch (InterruptedException e) {
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				int? temp1 = newValue[0][0];
				int? temp2 = newValue[0][1];
				int i = 0;
				for (i = 1; i < newValue.Length; i++) {
				  for (int k = 0; k < newValue[i - 1].Length; k++) {
					newValue[i - 1][k] = newValue[i][k];
				  }
				}

				newValue[i - 1][0] = temp1;
				newValue[i - 1][1] = temp2;
			   // For Excel XP, use: range.setValue2(newValue);
				range.Put("Value", new JIVariant(new JIArray(newValue)));
			}

			outVal2 = sheet.Get("ChartObjects",new object[]{ JIVariant.OPTIONAL_PARAM() });
			IJIDispatch chartObjects = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
			outVal2 = chartObjects.CallMethodA("Add",new object[]{ new double?(100),new double?(30),new double?(400),new double?(250) });
			//outVal2 = chartObjects.get("Item", new Object[]{new Integer(1)});
			IJIDispatch chartObject = (IJIDispatch)JIObjectFactory.NarrowObject(outVal2[0].ObjectAsComObject);
			outVal = chartObject.Get("Chart");
			IJIDispatch chart = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);
			chart.CallMethod("SetSourceData",new object[]{ range,JIVariant.OPTIONAL_PARAM() });
			try {
				Thread.Sleep(5000);
			}
			catch (InterruptedException e) {
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			outVal = sheet.Get("PageSetup");
			IJIDispatch pageSetup = (IJIDispatch)JIObjectFactory.NarrowObject(outVal.ObjectAsComObject);
			pageSetup.Put("Orientation",new JIVariant(2));
			pageSetup.Put("Zoom",new JIVariant(100));
			try {
				sheet.CallMethod("PrintOut",new object[]{ JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });
			}
			catch (JIException e) {
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				JIExcepInfo excepInfo = sheet.LastExcepInfo;
				Console.WriteLine("Error Code in EXCEPINFO: " + excepInfo.ErrorCode);
			}
			DispatchOfWorkBook.CallMethod("close",new object[]{ false,JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM() });
			Dispatch.CallMethod("Quit");
			JISession.DestroySession(Dispatch.AssociatedSession);
		}




		public static void Main(string[] args) {

			try {
					if (args.Length < 4) {
						Console.WriteLine("Please provide address domain username password");
						return;
					}
					MSExcel3 test = new MSExcel3(args[0],args);
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