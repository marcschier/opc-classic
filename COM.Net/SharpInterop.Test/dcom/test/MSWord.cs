namespace org.jinterop.dcom.test {
    using System;
    using Serilog;
    using System.Threading;
    using org.jinterop.dcom.core;
    using org.jinterop.dcom.impls.automation;
    using org.jinterop.dcom.impls;
    using org.jinterop.dcom.common;

    public class MSWord
	{
		private readonly JIComServer _comStub;
		private IJIDispatch _dispatch;
		private IJIComObject _unknown;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSWord(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSWord(string address, string[] args)
		{
			var session = JISession.CreateSession(args[1], args[2], args[3]);
			session.UseSessionSecurity(true);
			_comStub = new JIComServer(JIProgId.ValueOf("Word.Application"), address, session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startWord() throws org.jinterop.dcom.common.JIException
		public virtual void StartWord()
		{
			_unknown = _comStub.CreateInstance();
			_dispatch = (IJIDispatch) JIObjectFactory.NarrowObject(_unknown.QueryInterface(impls.automation.Interfaces.IID_IDispatch));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showWord() throws org.jinterop.dcom.common.JIException
		public virtual void ShowWord()
		{
			var dispId = _dispatch.GetIDsOfNames("Visible");
			var variant = new JIVariant(true);
			_dispatch.Put(dispId, variant);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void PerformOp()
		{
			JISystem.IsCoClassAutoCollection = true;

			Console.WriteLine(_dispatch.Get("Version").ObjectAsString.String);
			Console.WriteLine(_dispatch.Get("Path").ObjectAsString.String);
			var variant = _dispatch.Get("Documents");

			Console.WriteLine("Open document...");
			var documents = (IJIDispatch) JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
			var filePath = new JIString("c:\\temp\\test.doc");
			var variant2 = documents.CallMethodA("open", new object[] {filePath, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM()});

			Console.WriteLine("doc opened");
			//10
			Sleep(10);

			Console.WriteLine("Get content...");
			var document = (IJIDispatch) JIObjectFactory.NarrowObject(variant2[0].ObjectAsComObject);
			variant = document.Get("Content");
			var range = (IJIDispatch) JIObjectFactory.NarrowObject(variant.ObjectAsComObject);

			//10
			Sleep(10);
			Console.WriteLine("Running find...");
			variant = range.Get("Find");
			var find = (IJIDispatch) JIObjectFactory.NarrowObject(variant.ObjectAsComObject);

			//2
			Sleep(5);

			Console.WriteLine("Running execute...");
			var findString = new JIString("ow");
			var replaceString = new JIString("igh");
			find.CallMethodA("Execute", new object[] {findString.VariantByRef, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), replaceString.VariantByRef, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM()});

			//1
			Sleep(2);

			Console.WriteLine("Closing document...");
			document.CallMethod("Close");

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sleep(int minutes) throws InterruptedException
		private void Sleep(int minutes)
		{
			Console.WriteLine("Sleeping " + minutes + " minute(s)...");
			Thread.Sleep(minutes * 60 * 1000);
		}

		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void quitAndDestroy() throws org.jinterop.dcom.common.JIException
		private void QuitAndDestroy()
		{
			Console.WriteLine("Quit...");
			_dispatch.CallMethod("Quit", new object[] {new JIVariant(-1, true), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM()});
			JISession.DestroySession(_dispatch.AssociatedSession);
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

				
				var test = new MSWord(args[0], args);
				test.StartWord();
				test.ShowWord();

	//			for (int i = 0; i < 10; i++) {
					test.PerformOp();
	//			}

				test.QuitAndDestroy();

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