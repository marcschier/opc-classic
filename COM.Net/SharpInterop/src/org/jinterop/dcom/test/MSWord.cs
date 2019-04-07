namespace org.jinterop.dcom.test {


    using JIException = common.JIException;
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIComServer = core.JIComServer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class MSWord
	{

		private JIComServer comStub;

		private IJIDispatch dispatch;

		private IJIComObject unknown;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSWord(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSWord(string address, string[] args)
		{
			var session = JISession.createSession(args[1], args[2], args[3]);
			session.useSessionSecurity(true);
			comStub = new JIComServer(JIProgId.valueOf("Word.Application"), address, session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startWord() throws org.jinterop.dcom.common.JIException
		public virtual void startWord()
		{
			unknown = comStub.CreateInstance();
			dispatch = (IJIDispatch) JIObjectFactory.narrowObject(unknown.QueryInterface(impls.automation.IJIDispatch_Fields.IID));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showWord() throws org.jinterop.dcom.common.JIException
		public virtual void showWord()
		{
			var dispId = dispatch.getIDsOfNames("Visible");
			var variant = new JIVariant(true);
			dispatch.put(dispId, variant);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void performOp()
		{

			/* JISystem config *
			 *
			 */
			JISystem.JavaCoClassAutoCollection = true;



			Console.WriteLine(((JIVariant) dispatch.get("Version")).ObjectAsString.String);
			Console.WriteLine(((JIVariant) dispatch.get("Path")).ObjectAsString.String);
			var variant = dispatch.get("Documents");

			Console.WriteLine("Open document...");
			var documents = (IJIDispatch) JIObjectFactory.narrowObject(variant.ObjectAsComObject);
			var filePath = new JIString("c:\\temp\\test.doc");
			var variant2 = documents.callMethodA("open", new object[] {filePath, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM()});

			Console.WriteLine("doc opened");
			//10
			sleep(10);

			Console.WriteLine("Get content...");
			var document = (IJIDispatch) JIObjectFactory.narrowObject(variant2[0].ObjectAsComObject);
			variant = document.get("Content");
			var range = (IJIDispatch) JIObjectFactory.narrowObject(variant.ObjectAsComObject);

			//10
			sleep(10);
			Console.WriteLine("Running find...");
			variant = range.get("Find");
			var find = (IJIDispatch) JIObjectFactory.narrowObject(variant.ObjectAsComObject);

			//2
			sleep(5);

			Console.WriteLine("Running execute...");
			var findString = new JIString("ow");
			var replaceString = new JIString("igh");
			find.callMethodA("Execute", new object[] {findString.VariantByRef, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), replaceString.VariantByRef, JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM()});

			//1
			sleep(2);

			Console.WriteLine("Closing document...");
			document.callMethod("Close");

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sleep(int minutes) throws InterruptedException
		private void sleep(int minutes)
		{
			Console.WriteLine("Sleeping " + minutes + " minute(s)...");
			Thread.Sleep((int)(minutes * 60 * 1000));
		}

		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void quitAndDestroy() throws org.jinterop.dcom.common.JIException
		private void quitAndDestroy()
		{
			Console.WriteLine("Quit...");
			dispatch.callMethod("Quit", new object[] {new JIVariant(-1, true), JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM()});
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

				Log.Logger.Level = Level.INFO;
				JISystem.InBuiltLogHandler = false;
				var test = new MSWord(args[0], args);
				test.startWord();
				test.showWord();

	//			for (int i = 0; i < 10; i++) {
					test.performOp();
	//			}

				test.quitAndDestroy();

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