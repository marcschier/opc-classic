namespace org.jinterop.dcom.test {

    using JIException = common.JIException;
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JICallBuilder = core.JICallBuilder;
    using JIComServer = core.JIComServer;
    using JIFlags = core.JIFlags;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;

    public class MSShell
	{

		internal JISession session;
		internal JIComServer comServer;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: MSShell(String args[]) throws java.net.UnknownHostException, org.jinterop.dcom.common.JIException
		internal MSShell(string[] args)
		{
			session = JISession.createSession(args[1],args[2],args[3]);
			comServer = new JIComServer(JIProgId.valueOf("Shell.Application"),args[0],session);
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void doStuff() throws org.jinterop.dcom.common.JIException
		internal virtual void doStuff()
		{
			//this will return a reference to the IUnknown of the Shell coclass.
			var comUnknown = (IJIComObject)comServer.CreateInstance();

			//now we query for the IShellDispatch interface
			var shellDispatch = (IJIComObject)comUnknown.QueryInterface("D8F015C0-C278-11CE-A49E-444553540000");

			var callObject = new JICallBuilder();
	//		callObject.setOpnum(5);
	//		callObject.addInParamAsVariant(new JIVariant(new JIString("c:")),JIFlags.FLAG_NULL);
	//		Object result[] = shellDispatch.call(callObject);

	//		callObject.reInit();
	//		callObject.setOpnum(7);
	//		result = shellDispatch.call(callObject);

			callObject.ReInit();
			callObject.Opnum = 2;
			callObject.AddInParamAsVariant(new JIVariant(2),JIFlags.FLAG_NULL);
			callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			var result = shellDispatch.Call(callObject);
			var folder = JIObjectFactory.narrowObject((IJIComObject)result[0]);

            callObject = new JICallBuilder {
                Opnum = 0
            };
            callObject.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
			result = folder.Call(callObject);
			Console.WriteLine("Current Folder: " + result[0]);

			callObject.ReInit();
			callObject.Opnum = 1;
			callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			result = folder.Call(callObject);
			var test = JIObjectFactory.narrowObject((IJIComObject)result[0]);

	//		Not implemented by shell
	//		callObject.reInit();
	//		callObject.setOpnum(2);
	//		callObject.addOutParamAsType(JIInterfacePointer.class,JIFlags.FLAG_NULL);
	//		result = folder.call(callObject);
	//		test = JIObjectFactory.createCOMInstance(shellDispatch,(JIInterfacePointer)result[0]);

			callObject.ReInit();
			callObject.Opnum = 3;
			callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			result = folder.Call(callObject);
			test = JIObjectFactory.narrowObject((IJIComObject)result[0]);

			callObject.ReInit();
			callObject.Opnum = 4;
			callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			result = folder.Call(callObject);
			var folderItems = JIObjectFactory.narrowObject((IJIComObject)result[0]);

            callObject = new JICallBuilder {
                Opnum = 0
            };
            callObject.AddOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			result = folderItems.Call(callObject);

			var count = (int)(int?)result[0];

			for (var i = 0;i < count;i++)
			{
				callObject.ReInit();
				callObject.Opnum = 3;
				callObject.AddInParamAsVariant(new JIVariant(i),JIFlags.FLAG_NULL);
				callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
				result = folderItems.Call(callObject);
				var folderItem = JIObjectFactory.narrowObject((IJIComObject)result[0]);


                var callObject2 = new JICallBuilder {
                    Opnum = 2
                };
                callObject2.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
				result = folderItem.Call(callObject2);
				Console.WriteLine("Name of Object: " + result[0]);

				callObject2.ReInit();
				callObject2.Opnum = 4;
				callObject2.AddOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
				result = folderItem.Call(callObject2);
				Console.WriteLine("Path of the Object: " + result[0]);


				callObject2.ReInit();
                callObject2 = new JICallBuilder {
                    Opnum = 9
                };
                //VARIANT_BOOL is Boolean
                callObject2.AddOutParamAsType(typeof(bool?),JIFlags.FLAG_NULL);
				result = folderItem.Call(callObject2);

				var isFileSystemObject = (bool)(bool?)result[0];

				if (isFileSystemObject)
				{
					Console.Write(" and is part of file system\n");
				}
				else
				{
					Console.Write(" and is not part of file system\n");
				}

				callObject2.ReInit();
                callObject2 = new JICallBuilder {
                    Opnum = 13
                };
                callObject2.AddOutParamAsObject(typeof(int?),JIFlags.FLAG_NULL);
				result = folderItem.Call(callObject2);
				Console.Write(" and size(in bytes) is: " + (int)(int?)result[0] + "\n");

			}

		}



		public static void Main(string[] args)
		{

			if (args.Length < 4)
			{
				Console.WriteLine("Please provide address domain username password");
				return;
			}
			JISystem.AutoRegisteration = true;
			try
			{
				var shell = new MSShell(args);
				shell.doStuff();
				JISession.destroySession(shell.session);
			}
			catch (UnknownHostException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			catch (JIException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

	}

}