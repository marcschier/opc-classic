namespace org.jinterop.dcom.test {

    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JICallBuilder = core.JICallBuilder;
    using JIComServer = core.JIComServer;
    using JIFlags = core.JIFlags;
    using JILocalCoClass = core.JILocalCoClass;
    using JILocalInterfaceDefinition = core.JILocalInterfaceDefinition;
    using JILocalMethodDescriptor = core.JILocalMethodDescriptor;
    using JILocalParamsDescriptor = core.JILocalParamsDescriptor;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class Test_ITestServer2_Impl
	{

		public virtual void Execute(JIString str)
		{
			Console.WriteLine(str.String);
		}
		/// <param name="args"> </param>
		public static void Main(string[] args)
		{

			if (args.Length < 4)
			{
				Console.WriteLine("Please provide address domain username password");
				return;
			}



			try
			{

				JISystem.AutoRegisteration = true;
				JISystem.InBuiltLogHandler = false;
				Log.Logger.Level = Level.ALL;
				var session1 = JISession.createSession(args[1],args[2],args[3]);
				var session2 = JISession.createSession(args[1],args[2],args[3]);
				var testServer1 = new JIComServer(JIProgId.valueOf("TestJavaServer.TestServer1"),args[0],session1);
				var unkTestServer1 = testServer1.createInstance();
				var testServer1Intf = JIObjectFactory.narrowObject(unkTestServer1.queryInterface("2A93A24D-59FE-4DE0-B67E-B8D41C9F57F8"));
				var dispatch1 = (IJIDispatch)JIObjectFactory.narrowObject(unkTestServer1.queryInterface(impls.automation.IJIDispatch_Fields.IID));

				//First lets call the ITestServer1.Call_TestServer2_Java using the Dispatch interface
				//Acquire a reference to ITestServer2
				var testServer2 = new JIComServer(JIProgId.valueOf("TestJavaServer.TestServer2"),args[0],session2);
				var unkTestServer2 = testServer2.createInstance();
				//Get the interface pointer to ITestServer2
				var iTestServer2 = (IJIComObject)JIObjectFactory.narrowObject(unkTestServer2.queryInterface("9CCC5120-457D-49F3-8113-90F7E97B54A7"));

				var dispatch2 = (IJIDispatch)JIObjectFactory.narrowObject(unkTestServer2.queryInterface(impls.automation.IJIDispatch_Fields.IID));

				//send it directly without IDispatch interface, please note that the "dispatchNotSupported" flag of JICallBuilder is "false".
				var callObject = new JICallBuilder(false);
				callObject.addInParamAsComObject(iTestServer2, JIFlags.FLAG_NULL);
				callObject.Opnum = 0;
				testServer1Intf.call(callObject);

				//Send it to ITestServer.Call_TestServer2_Java2 via IDispatch of ITestServer1. Notice that pointer here id IDispatch.
				dispatch1.callMethod("Call_TestServer2_Java2", new object[]{new JIVariant(dispatch2)});

				//Send it to ITestServer.Call_TestServer2_Java via IDispatch of ITestServer1.
				dispatch1.callMethod("Call_TestServer2_Java", new object[]{new JIVariant(iTestServer2)});


				//Now for the Java Implementation of ITestServer2 interface (from the type library or IDL)  
				//IID of ITestServer2 interface
				var interfaceDefinition = new JILocalInterfaceDefinition("9CCC5120-457D-49F3-8113-90F7E97B54A7");
				//lets define the method "Execute" now. Please note that either this should be in the same order as defined in IDL
				//or use the addInParamAsObject with opnum parameter function.
				var parameterObject = new JILocalParamsDescriptor();
				parameterObject.addInParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
				var methodDescriptor = new JILocalMethodDescriptor("Execute",1,parameterObject);
				interfaceDefinition.addMethodDescriptor(methodDescriptor);
				//Create the Java Server class. This contains the instance to be called by the COM Server ITestServer1.
				var _testServer2 = new JILocalCoClass(interfaceDefinition,new Test_ITestServer2_Impl());
				//Get a interface pointer to the Java CO Class. The template could be any IJIComObject since only the session is reused.
				var __testServer2 = JIObjectFactory.buildObject(session1,_testServer2);
				//Call our Java server. The same message should be printed on the Java console.
				dispatch1.callMethod("Call_TestServer2_Java", new object[]{new JIVariant(__testServer2)});

			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}


		}

	}

}