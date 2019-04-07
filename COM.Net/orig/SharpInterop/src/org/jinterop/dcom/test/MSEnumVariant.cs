using System;

namespace org.jinterop.dcom.test {

	using JIException = org.jinterop.dcom.common.JIException;
	using JISystem = org.jinterop.dcom.common.JISystem;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIArray = org.jinterop.dcom.core.JIArray;
	using JIComServer = org.jinterop.dcom.core.JIComServer;
	using JIProgId = org.jinterop.dcom.core.JIProgId;
	using JISession = org.jinterop.dcom.core.JISession;
	using JIString = org.jinterop.dcom.core.JIString;
	using JIVariant = org.jinterop.dcom.core.JIVariant;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
	using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;
	using IJIEnumVariant = org.jinterop.dcom.impls.automation.IJIEnumVariant;

	//StdCollection.VBCollection
	public class MSEnumVariant {

		private JIComServer ComServer = null;
		private JISession Session = null;
		private IJIDispatch Dispatch = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSEnumVariant(String address,String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSEnumVariant(string address, string[] args) {
			Session = JISession.CreateSession(args[1],args[2],args[3]);
			ComServer = new JIComServer(JIProgId.ValueOf("StdCollection.VBCollection"),address,Session);
			IJIComObject @object = ComServer.CreateInstance();
			Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject(@object.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException
		public virtual void PerformOp() {
			int i = 0;
			for (; i < 5; i++) {
				Dispatch.CallMethod("Add", new object[]{ new int?(i),new JIString("Key-" + i) });
			}

			for (; i < 10; i++) {
				Dispatch.CallMethod("Add", new object[]{ new int?(i),JIVariant.OPTIONAL_PARAM() });
			}

			JIVariant variant = Dispatch.Get("_NewEnum");

			IJIComObject object2 = variant.ObjectAsComObject;
			//IJIComObject enumObject = (IJIComObject)object2.queryInterface(IJIEnumVARIANT.IID);

			IJIEnumVariant enumVARIANT = (IJIEnumVariant)JIObjectFactory.NarrowObject(object2.QueryInterface(org.jinterop.dcom.impls.automation.IJIEnumVariant_Fields.IID));

			for (i = 0; i < 10; i++) {
				object[] values = enumVARIANT.Next(1);
				JIArray array = (JIArray)values[0];
				object[] arrayObj = (object[])array.ArrayInstance;
				for (int j = 0; j < arrayObj.Length; j++) {
					Console.WriteLine(((JIVariant)arrayObj[j]).ObjectAsInt + "," + (int)((int?)values[1]));
				}

				int j = 0;
			}

			enumVARIANT.Reset();
			object[] values = enumVARIANT.Next(5);
			enumVARIANT.Next(1);
			enumVARIANT.Skip(2);
			values = enumVARIANT.Next(1);
			IJIEnumVariant newenum = enumVARIANT.Clone();
			newenum.Reset();
			values = newenum.Next(10);
			i = 0;

			JISession.DestroySession(Session);
		}


		public static void Main(string[] args) {

			try {
				if (args.Length < 4) {
					Console.WriteLine("Please provide address domain username password");
					return;
				}
				JISystem.AutoRegisteration = true;
				MSEnumVariant enumVariant = new MSEnumVariant(args[0],args);
				enumVariant.PerformOp();
			}
			catch (Exception e) {
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

	}

}