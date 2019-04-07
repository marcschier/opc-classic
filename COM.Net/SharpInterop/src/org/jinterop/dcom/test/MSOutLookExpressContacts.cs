namespace org.jinterop.dcom.test {

    using JIException = common.JIException;
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JICallBuilder = core.JICallBuilder;
    using JIComServer = core.JIComServer;
    using JIFlags = core.JIFlags;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class MSOutLookExpressContacts
	{

		internal JISession session;
		internal JIComServer comServer;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: MSOutLookExpressContacts(String args[]) throws java.net.UnknownHostException, org.jinterop.dcom.common.JIException
		internal MSOutLookExpressContacts(string[] args)
		{
			session = JISession.createSession(args[1],args[2],args[3]);
			comServer = new JIComServer(JIProgId.valueOf("Outlook.Application"),args[0],session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void doStuff() throws org.jinterop.dcom.common.JIException
		internal virtual void doStuff()
		{
			var unknown = (IJIComObject)comServer.CreateInstance();
			var application = (IJIComObject)unknown.QueryInterface("00063001-0000-0000-C000-000000000046");

            var callObject = new JICallBuilder(!application.DispatchSupported) {
                Opnum = 12
            };
            callObject.addInParamAsString("MAPI", JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.addOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			var res = application.Call(callObject);

			var @namespace = JIObjectFactory.narrowObject((IJIComObject)res[0]);
            callObject = new JICallBuilder {
                Opnum = 16
            };
            callObject.addOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			res = @namespace.Call(callObject);

			if (res[0] == null)
			{
				Console.WriteLine("user cancelled request");
				return;
			}

			var folder = JIObjectFactory.narrowObject((IJIComObject)res[0]);
            callObject = new JICallBuilder {
                Opnum = 4
            };
            callObject.addOutParamAsType(typeof(int?),JIFlags.FLAG_NULL);
			res = folder.Call(callObject);

			if ((int)(int?)res[0] != 2)
			{
				Console.WriteLine("Invalid folder selected, this is not a \"contact\" folder , please reselect..");
				return;
			}

			callObject.reInit();
			callObject.Opnum = 10;
			callObject.addOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			res = folder.Call(callObject);
			if (res[0] == null)
			{
				Console.WriteLine("Unable to get Contact Items.");
				return;
			}

			var items = JIObjectFactory.narrowObject((IJIComObject)res[0]);
            callObject = new JICallBuilder {
                Opnum = 12
            };
            callObject.addOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			res = items.Call(callObject);

			while (true)
			{
				if (res[0] == null)
				{
					break;
				}

				string details = null;
				var contactItem = (IJIDispatch)JIObjectFactory.narrowObject((IJIComObject)res[0]);
				var res2 = contactItem.get("FullName");
	//			callObject = new JICallBuilder(contactItem.getIpid());
	//			callObject.setOpnum(124);
	//			callObject.addOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
	//			res = contactItem.call(callObject);
				details = res2.ObjectAsString.String;

	//			callObject.reInit();
	//			callObject.setOpnum(100);
	//			callObject.addOutParamAsObject(new JIString(JIFlags.FLAG_REPRESENTATION_STRING_BSTR),JIFlags.FLAG_NULL);
	//			res = contactItem.call(callObject);
				res2 = contactItem.get("Email1Address");
				details = details + "<" + res2.ObjectAsString.String + ">";

				Console.WriteLine(details);

                callObject = new JICallBuilder {
                    Opnum = 14
                };
                callObject.addOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
				res = items.Call(callObject);
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
				var outlookMessages = new MSOutLookExpressContacts(args);
				outlookMessages.doStuff();
				JISession.destroySession(outlookMessages.session);
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