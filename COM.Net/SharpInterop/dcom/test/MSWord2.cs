namespace org.jinterop.dcom.test {
    using IJIComObject = core.IJIComObject;
    using JIComServer = core.JIComServer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class MSWord2
	{

		private JIComServer comStub;
		private IJIDispatch dispatch;
		private IJIComObject unknown;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSWord2(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSWord2(string address, string[] args)
		{
			var session = JISession.createSession(args[1],args[2],args[3]);
			comStub = new JIComServer(JIProgId.valueOf("Word.Application"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startWord() throws org.jinterop.dcom.common.JIException
		public virtual void startWord()
		{
			unknown = comStub.CreateInstance();
			dispatch = (IJIDispatch)JIObjectFactory.narrowObject(unknown.QueryInterface(impls.automation.IJIDispatch_Fields.IID));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showWord() throws org.jinterop.dcom.common.JIException
		public virtual void showWord()
		{
			var dispId = dispatch.getIDsOfNames("Visible");
			var variant = new JIVariant(true);
			dispatch.put(dispId,variant);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void performOp()
		{
			Console.WriteLine(((JIVariant)dispatch.get("Version")).ObjectAsString.String);
			Console.WriteLine(((JIVariant)dispatch.get("Path")).ObjectAsString.String);
			var variant = dispatch.get("Documents");
			//JIInterfacePointer ptr = variant.getObjectAsInterfacePointer();
			//IJIDispatch documents = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,ptr);
			var documents = (IJIDispatch)JIObjectFactory.narrowObject(variant.ObjectAsComObject);
			var filePath = new JIString("c:/temp/test.doc");
			var variant2 = documents.callMethodA("open",new object[]{filePath.VariantByRef,JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
			//IJIDispatch document = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,variant2[0].getObjectAsInterfacePointer());
			var document = (IJIDispatch)JIObjectFactory.narrowObject(variant2[0].ObjectAsComObject);
			variant = document.get("Content");
			//IJIDispatch range = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,variant.getObjectAsInterfacePointer());
			var range = (IJIDispatch)JIObjectFactory.narrowObject(variant.ObjectAsComObject);

			variant = range.get("Find");
			var find = (IJIDispatch)JIObjectFactory.narrowObject(variant.ObjectAsComObject);

			Thread.Sleep(2000);
			var findString = new JIString("ow");
			var replaceString = new JIString("igh");
			find.callMethodA("Execute",new object[]{findString.VariantByRef,JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),replaceString.VariantByRef,JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM()});
			Thread.Sleep(5000);

			dispatch.callMethod("Quit", new object[]{new JIVariant(-1,true),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
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
					var test = new MSWord2(args[0],args);
					test.startWord();
					test.showWord();
					test.performOp();
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