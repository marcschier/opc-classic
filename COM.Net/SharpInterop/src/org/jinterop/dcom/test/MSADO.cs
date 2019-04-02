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
			comServer = new JIComServer(JIProgId.valueOf("ADODB.Connection"),address,session);
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void performOp()
		{
			unknown = comServer.createInstance();
			dispatch = (IJIDispatch)JIObjectFactory.narrowObject(unknown.queryInterface(impls.automation.IJIDispatch_Fields.IID));
			var typeInfo = dispatch.getTypeInfo(0);
			typeInfo.getFuncDesc(0);

			dispatch.callMethod("Open",new object[]{new JIString("driver=Microsoft Access Driver (*.mdb);dbq=C:\\temp\\products.mdb"),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), -1 });

			var variant = dispatch.callMethodA("Execute",new object[]{new JIString("SELECT * FROM Products"), -1 });
			if (variant[0].Null)
			{
				Console.WriteLine("Recordset is empty.");
			}
			else
			{
				var resultSet = (IJIDispatch)JIObjectFactory.narrowObject(variant[0].ObjectAsComObject);
				//variant = resultSet.get("EOF");
				while (!resultSet.get("EOF").ObjectAsBoolean)
				{
					var variant2 = resultSet.get("Fields");
					var fields = (IJIDispatch)JIObjectFactory.narrowObject(variant2.ObjectAsComObject);
					var count = fields.get("Count").ObjectAsInt;
					for (var i = 0;i < count;i++)
					{
						variant = fields.get("Item",new object[]{ i });
						var field = (IJIDispatch)JIObjectFactory.narrowObject(variant[0].ObjectAsComObject);
						variant2 = field.get("Value");
						object val = null;
						if (variant2.Type == JIVariant.VT_BSTR)
						{
							val = variant2.ObjectAsString.String;
						}
						if (variant2.Type == JIVariant.VT_I4)
						{
							val = variant2.ObjectAsInt;
						}
						Console.WriteLine(field.get("Name").ObjectAsString.String + " = " + val + "[" + variant2.Type + "]");
					}
					resultSet.callMethod("MoveNext");
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