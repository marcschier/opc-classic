using System;
using System.Threading;

namespace org.jinterop.dcom.test {


	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIComServer = org.jinterop.dcom.core.JIComServer;
	using JIProgId = org.jinterop.dcom.core.JIProgId;
	using JISession = org.jinterop.dcom.core.JISession;
	using JIString = org.jinterop.dcom.core.JIString;
	using JIVariant = org.jinterop.dcom.core.JIVariant;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
	using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

	public class MSWord {

		private JIComServer ComStub = null;

		private IJIDispatch Dispatch = null;

		private IJIComObject Unknown = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSWord(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSWord(string address, string[] args) {
			JISession session = JISession.CreateSession(args[1], args[2], args[3]);
			session.UseSessionSecurity(true);
			ComStub = new JIComServer(JIProgId.ValueOf("Word.Application"), address, session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startWord() throws org.jinterop.dcom.common.JIException
		public virtual void StartWord() {
			Unknown = ComStub.CreateInstance();
			Dispatch = (IJIDispatch) JIObjectFactory.NarrowObject(Unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showWord() throws org.jinterop.dcom.common.JIException
		public virtual void ShowWord() {
			int dispId = Dispatch.GetIDsOfNames("Visible");
			JIVariant variant = new JIVariant(true);
			Dispatch.Put(dispId, variant);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void PerformOp() {

			/* JISystem config *
			 *
			 */
			JISystem.JavaCoClassAutoCollection = true;



			Console.WriteLine(((JIVariant) Dispatch.Get("Version")).ObjectAsString.String);
			Console.WriteLine(((JIVariant) Dispatch.Get("Path")).ObjectAsString.String);
			JIVariant variant = Dispatch.Get("Documents");

			Console.WriteLine("Open document...");
			IJIDispatch documents = (IJIDispatch) JIObjectFactory.NarrowObject(variant.ObjectAsComObject);
			JIString filePath = new JIString("c:\\temp\\test.doc");
			JIVariant[] variant2 = documents.CallMethodA("open", new object[] { filePath, JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM() });

			Console.WriteLine("doc opened");
			//10
			Sleep(10);

			Console.WriteLine("Get content...");
			IJIDispatch document = (IJIDispatch) JIObjectFactory.NarrowObject(variant2[0].ObjectAsComObject);
			variant = document.Get("Content");
			IJIDispatch range = (IJIDispatch) JIObjectFactory.NarrowObject(variant.ObjectAsComObject);

			//10
			Sleep(10);
			Console.WriteLine("Running find...");
			variant = range.Get("Find");
			IJIDispatch find = (IJIDispatch) JIObjectFactory.NarrowObject(variant.ObjectAsComObject);

			//2
			Sleep(5);

			Console.WriteLine("Running execute...");
			JIString findString = new JIString("ow");
			JIString replaceString = new JIString("igh");
			find.CallMethodA("Execute", new object[] { findString.VariantByRef, JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), replaceString.VariantByRef, JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM() });

			//1
			Sleep(2);

			Console.WriteLine("Closing document...");
			document.CallMethod("Close");

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void sleep(int minutes) throws InterruptedException
		private void Sleep(int minutes) {
			Console.WriteLine("Sleeping " + minutes + " minute(s)...");
			Thread.Sleep((int)(minutes * 60 * 1000));
		}

		/// <exception cref="JIException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void quitAndDestroy() throws org.jinterop.dcom.common.JIException
		private void QuitAndDestroy() {
			Console.WriteLine("Quit...");
			Dispatch.CallMethod("Quit", new object[] { new JIVariant(-1, true), JIVariant.OPTIONAL_PARAM(), JIVariant.OPTIONAL_PARAM() });
			JISession.DestroySession(Dispatch.AssociatedSession);
		}

		public static void Main(string[] args) {

			try {
				if (args.Length < 4) {
					Console.WriteLine("Please provide address domain username password");
					return;
				}

				JISystem.Logger.Level = Level.INFO;
				JISystem.InBuiltLogHandler = false;
				MSWord test = new MSWord(args[0], args);
				test.StartWord();
				test.ShowWord();

	//			for (int i = 0; i < 10; i++) {
					test.PerformOp();
	//			}

				test.QuitAndDestroy();

			}
			catch (Exception e) {
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

	}

}