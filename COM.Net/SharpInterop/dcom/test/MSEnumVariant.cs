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
    using IJIEnumVariant = impls.automation.IJIEnumVariant;

    //StdCollection.VBCollection
    public class MSEnumVariant
	{

		private JIComServer comServer;
		private readonly JISession session;
		private IJIDispatch dispatch;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSEnumVariant(String address,String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSEnumVariant(string address, string[] args)
		{
			session = JISession.createSession(args[1],args[2],args[3]);
			comServer = new JIComServer(JIProgId.valueOf("StdCollection.VBCollection"),address,session);
			var @object = comServer.CreateInstance();
			dispatch = (IJIDispatch)JIObjectFactory.narrowObject(@object.QueryInterface(impls.automation.IJIDispatch_Fields.IID));

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException
		public virtual void performOp()
		{
			var i = 0;
			for (; i < 5; i++)
			{
				dispatch.callMethod("Add", new object[]{ i, new JIString("Key-" + i)});
			}

			for (; i < 10; i++)
			{
				dispatch.callMethod("Add", new object[]{ i, JIVariant.CreateOPTIONAL_PARAM()});
			}

			var variant = dispatch.get("_NewEnum");

			var object2 = variant.ObjectAsComObject;
			//IJIComObject enumObject = (IJIComObject)object2.queryInterface(IJIEnumVARIANT.IID);

			var enumVARIANT = (IJIEnumVariant)JIObjectFactory.narrowObject(object2.QueryInterface(impls.automation.IJIEnumVariant_Fields.IID));

			for (i = 0; i < 10; i++)
			{
				var values = enumVARIANT.next(1);
				var array = (JIArray)values[0];
				var arrayObj = (object[])array.ArrayInstance;
				for (var j = 0; j < arrayObj.Length; j++)
				{
					Console.WriteLine(((JIVariant)arrayObj[j]).ObjectAsInt + "," + (int)(int?)values[1]);
				}

				var j = 0;
			}

			enumVARIANT.reset();
			var values = enumVARIANT.next(5);
			enumVARIANT.next(1);
			enumVARIANT.skip(2);
			values = enumVARIANT.next(1);
			var newenum = enumVARIANT.Clone();
			newenum.reset();
			values = newenum.next(10);
			i = 0;

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
				JISystem.AutoRegisteration = true;
				var enumVariant = new MSEnumVariant(args[0],args);
				enumVariant.performOp();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

	}

}