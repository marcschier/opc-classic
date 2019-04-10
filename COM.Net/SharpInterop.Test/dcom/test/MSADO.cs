namespace org.jinterop.dcom.test {
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIComServer = core.JIComServer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;
    using IJITypeInfo = impls.automation.IJITypeInfo;

    public class MSADO
	{

		private JIComServer comServer;
		private IJIDispatch dispatch;
		private IJIComObject unknown;
		private readonly JISession session;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSADO(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSADO(string address, string[] args)
		{
			session = JISession.createSession(args[1],args[2],args[3]);
			comServer = new JIComServer(JIProgId.ValueOf("ADODB.Connection"),address,session);
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void performOp()
		{
			unknown = comServer.CreateInstance();
			dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(unknown.QueryInterface(impls.automation.DispatchFlags.IID));
			var typeInfo = dispatch.GetTypeInfo(0);
			typeInfo.GetFuncDesc(0);

			dispatch.CallMethod("Open",new object[]{new JIString("driver=Microsoft Access Driver (*.mdb);dbq=C:\\temp\\products.mdb"),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), -1 });

			var variant = dispatch.CallMethodA("Execute",new object[]{new JIString("SELECT * FROM Products"), -1 });
			if (variant[0].Null)
			{
				Console.WriteLine("Recordset is empty.");
			}
			else
			{
				var resultSet = (IJIDispatch)JIObjectFactory.NarrowObject(variant[0].ObjectAsComObject);
				//variant = resultSet.get("EOF");
				while (!resultSet.Get("EOF").ObjectAsBoolean)
				{
					var variant2 = resultSet.Get("Fields");
					var fields = (IJIDispatch)JIObjectFactory.NarrowObject(variant2.ObjectAsComObject);
					var count = fields.Get("Count").ObjectAsInt;
					for (var i = 0;i < count;i++)
					{
						variant = fields.Get("Item",new object[]{ i });
						var field = (IJIDispatch)JIObjectFactory.NarrowObject(variant[0].ObjectAsComObject);
						variant2 = field.Get("Value");
						object val = null;
						if (variant2.Type == JIVariant.VT_BSTR)
						{
							val = variant2.ObjectAsString.String;
						}
						if (variant2.Type == JIVariant.VT_I4)
						{
							val = variant2.ObjectAsInt;
						}
						Console.WriteLine(field.Get("Name").ObjectAsString.String + " = " + val + "[" + variant2.Type + "]");
					}
					resultSet.CallMethod("MoveNext");
				}


			}

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
					var test = new MSADO(args[0],args);
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