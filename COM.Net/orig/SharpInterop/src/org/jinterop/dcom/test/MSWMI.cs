using System;

namespace org.jinterop.dcom.test {




	using IJIUnreferenced = org.jinterop.dcom.common.IJIUnreferenced;
	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIArray = org.jinterop.dcom.core.JIArray;
	using JICallBuilder = org.jinterop.dcom.core.JICallBuilder;
	using JIComServer = org.jinterop.dcom.core.JIComServer;
	using JIFlags = org.jinterop.dcom.core.JIFlags;
	using JIProgId = org.jinterop.dcom.core.JIProgId;
	using JISession = org.jinterop.dcom.core.JISession;
	using JIString = org.jinterop.dcom.core.JIString;
	using JIVariant = org.jinterop.dcom.core.JIVariant;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
	using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;
	using IJIEnumVariant = org.jinterop.dcom.impls.automation.IJIEnumVariant;


	public class MSWMI {

		private JIComServer ComStub = null;
		private IJIComObject ComObject = null;
		private IJIDispatch Dispatch = null;
		private string Address = null;
		private JISession Session = null;
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSWMI(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSWMI(string address, string[] args) {
			this.Address = address;
			Session = JISession.CreateSession(args[1],args[2],args[3]);
			Session.UseSessionSecurity(true);
			Session.GlobalSocketTimeout = 5000;
			ComStub = new JIComServer(JIProgId.ValueOf("WbemScripting.SWbemLocator"),address,Session);
			IJIComObject unknown = ComStub.CreateInstance();
			ComObject = (IJIComObject)unknown.QueryInterface("76A6415B-CB41-11d1-8B02-00600806D9B6"); //ISWbemLocator
			//This will obtain the dispatch interface
			Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(ComObject.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void PerformOp() {
			System.gc();
			JIVariant[] results = Dispatch.CallMethodA("ConnectServer",new object[]{ new JIString(Address),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),JIVariant.OPTIONAL_PARAM(),new int?(0),JIVariant.OPTIONAL_PARAM() });

			//using the dispatch results above you can use the "ConnectServer" api to retrieve a pointer to IJIDispatch
			//of ISWbemServices

			//OR
			//Make a direct call like below , in this case you would get back an interface pointer to ISWbemServices , NOT to it's IDispatch
			JICallBuilder callObject = new JICallBuilder();
			callObject.AddInParamAsString(Address,JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.AddInParamAsString("",JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.AddInParamAsString("",JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.AddInParamAsString("",JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.AddInParamAsString("",JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.AddInParamAsString("",JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.AddInParamAsInt(0,JIFlags.FLAG_NULL);
			callObject.AddInParamAsPointer(null,JIFlags.FLAG_NULL);
			callObject.Opnum = 0;
			callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			IJIComObject wbemServices = JIObjectFactory.NarrowObject((IJIComObject)((object[])ComObject.Call(callObject))[0]);
			wbemServices.InstanceLevelSocketTimeout = 1000;
			wbemServices.RegisterUnreferencedHandler(new IJIUnreferencedAnonymousInnerClassHelper(this));

			//Lets have a look at both.
			IJIDispatch wbemServices_dispatch = (IJIDispatch)JIObjectFactory.NarrowObject((results[0]).ObjectAsComObject);
			results = wbemServices_dispatch.CallMethodA("InstancesOf", new object[]{ new JIString("Win32_Process"), new int?(0), JIVariant.OPTIONAL_PARAM() });
			IJIDispatch wbemObjectSet_dispatch = (IJIDispatch)JIObjectFactory.NarrowObject((results[0]).ObjectAsComObject);
			JIVariant variant = wbemObjectSet_dispatch.Get("_NewEnum");
			IJIComObject object2 = variant.ObjectAsComObject;

			Console.WriteLine(object2.DispatchSupported);
			Console.WriteLine(object2.DispatchSupported);

			object2.RegisterUnreferencedHandler(new IJIUnreferencedAnonymousInnerClassHelper2(this));

			IJIEnumVariant enumVARIANT = (IJIEnumVariant)JIObjectFactory.NarrowObject(object2.QueryInterface(org.jinterop.dcom.impls.automation.IJIEnumVariant_Fields.IID));

			//This will return back a dispatch of ISWbemObjectSet

			//OR
			//It returns back the pointer to ISWbemObjectSet
			callObject = new JICallBuilder();
			callObject.AddInParamAsString("Win32_Process",JIFlags.FLAG_REPRESENTATION_STRING_BSTR);
			callObject.AddInParamAsInt(0,JIFlags.FLAG_NULL);
			callObject.AddInParamAsPointer(null,JIFlags.FLAG_NULL);
			callObject.Opnum = 4;
			callObject.AddOutParamAsType(typeof(IJIComObject),JIFlags.FLAG_NULL);
			IJIComObject wbemObjectSet = JIObjectFactory.NarrowObject((IJIComObject)((object[])wbemServices.Call(callObject))[0]);

			//okay seen enough of the other usage, lets just stick to disptach, it's lot simpler
			JIVariant Count = wbemObjectSet_dispatch.Get("Count");
			int count = Count.ObjectAsInt;
			for (int i = 0; i < count; i++) {
				object[] values = enumVARIANT.Next(1);
				JIArray array = (JIArray)values[0];
				object[] arrayObj = (object[])array.ArrayInstance;
				for (int j = 0; j < arrayObj.Length; j++) {
					IJIDispatch wbemObject_dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(((JIVariant)arrayObj[j]).ObjectAsComObject);
					JIVariant variant2 = (JIVariant)(wbemObject_dispatch.CallMethodA("GetObjectText_",new object[]{ new int?(1) }))[0];
					Console.WriteLine(variant2.ObjectAsString.String);
					Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
				}
			}


		}

		private class IJIUnreferencedAnonymousInnerClassHelper : IJIUnreferenced {
			private readonly MSWMI OuterInstance;

			public IJIUnreferencedAnonymousInnerClassHelper(MSWMI outerInstance) {
				this.OuterInstance = outerInstance;
			}

			public virtual void UnReferenced() {
				Console.WriteLine("wbemServices unreferenced... ");
			}
		}

		private class IJIUnreferencedAnonymousInnerClassHelper2 : IJIUnreferenced {
			private readonly MSWMI OuterInstance;

			public IJIUnreferencedAnonymousInnerClassHelper2(MSWMI outerInstance) {
				this.OuterInstance = outerInstance;
			}

			public virtual void UnReferenced() {
				Console.WriteLine("object2 unreferenced...");
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void killme() throws org.jinterop.dcom.common.JIException
		private void Killme() {
			JISession.DestroySession(Session);
		}

		public static void Main(string[] args) {

			try {
					if (args.Length < 4) {
						Console.WriteLine("Please provide address domain username password");
						return;
					}

					JISystem.Logger.Level = Level.INFO;
					JISystem.InBuiltLogHandler = false;
					JISystem.AutoRegisteration = true;
					MSWMI test = new MSWMI(args[0],args);
					for (int i = 0 ; i < 100; i++) {
						Console.WriteLine("Index i: " + i);
						test.PerformOp();
					}
					test.Killme();
			}
				catch (Exception e) {
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
		}





	}

}