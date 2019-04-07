using System;
using System.Threading;

namespace org.jinterop.dcom.test {



	using JIException = org.jinterop.dcom.common.JIException;
	using IJIComObject = org.jinterop.dcom.core.IJIComObject;
	using JIClsid = org.jinterop.dcom.core.JIClsid;
	using JIComServer = org.jinterop.dcom.core.JIComServer;
	using JISession = org.jinterop.dcom.core.JISession;
	using JIString = org.jinterop.dcom.core.JIString;
	using JIVariant = org.jinterop.dcom.core.JIVariant;
	using JIObjectFactory = org.jinterop.dcom.impls.JIObjectFactory;
	using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

	public class MSPowerPoint {

		private JIComServer ComStub = null;
		private IJIDispatch Dispatch = null;
		private IJIComObject Unknown = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSPowerPoint(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSPowerPoint(string address, string[] args) {
			JISession session = JISession.CreateSession(args[1],args[2],args[3]);
			ComStub = new JIComServer(JIClsid.ValueOf("91493441-5A91-11CF-8700-00AA0060263B"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startPowerPoint() throws org.jinterop.dcom.common.JIException
		public virtual void StartPowerPoint() {
			Unknown = ComStub.CreateInstance();
			Dispatch = (IJIDispatch)JIObjectFactory.NarrowObject((IJIComObject)Unknown.QueryInterface(org.jinterop.dcom.impls.automation.IJIDispatch_Fields.IID));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showPowerPoint() throws org.jinterop.dcom.common.JIException
		public virtual void ShowPowerPoint() {
			int dispId = Dispatch.GetIDsOfNames("Visible");
			JIVariant variant = new JIVariant(-1);
			Dispatch.Put(dispId,variant);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void PerformOp() {
			//JIVariant variant = dispatch.get("Presentations");
			//JIInterfacePointer ptr = variant.getObjectAsInterfacePointer();
			//IJIDispatch presentations = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,ptr);
			IJIDispatch presentations = (IJIDispatch)JIObjectFactory.NarrowObject(Dispatch.Get("Presentations").ObjectAsComObject);

			for (int i = 0; i < 2; i++) {
				JIVariant[] results = presentations.CallMethodA("Add",new object[]{ JIVariant.OPTIONAL_PARAM() });
				//variant = results[0];
				//ptr = variant.getObjectAsInterfacePointer();
				//IJIDispatch presentation = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,ptr);
				IJIDispatch presentation = (IJIDispatch)JIObjectFactory.NarrowObject(results[0].ObjectAsComObject);
				//variant = presentation.get("Slides");
				//ptr = variant.getObjectAsInterfacePointer();
				//IJIDispatch slides = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,ptr);
				IJIDispatch slides = (IJIDispatch)JIObjectFactory.NarrowObject(presentation.Get("Slides").ObjectAsComObject);

				results = slides.CallMethodA("Add", new object[]{ new int?(1),new int?(1) });
				//variant = results[0];
				//ptr = variant.getObjectAsInterfacePointer();
				IJIDispatch slide = (IJIDispatch)JIObjectFactory.NarrowObject(results[0].ObjectAsComObject);

				//variant = slide.get("Shapes");
				//ptr = variant.getObjectAsInterfacePointer();
				IJIDispatch shapes = (IJIDispatch)JIObjectFactory.NarrowObject(slide.Get("Shapes").ObjectAsComObject);

				//variant = shapes.get("Title");
				//ptr = variant.getObjectAsInterfacePointer();
				IJIDispatch shape = (IJIDispatch)JIObjectFactory.NarrowObject(shapes.Get("Title").ObjectAsComObject);

				//variant = shape.get("TextFrame");
				//ptr = variant.getObjectAsInterfacePointer();
				IJIDispatch textframe = (IJIDispatch)JIObjectFactory.NarrowObject(shape.Get("TextFrame").ObjectAsComObject);

				//variant = textframe.get("TextRange");
				//ptr = variant.getObjectAsInterfacePointer();
				IJIDispatch textrange = (IJIDispatch)JIObjectFactory.NarrowObject(textframe.Get("TextRange").ObjectAsComObject);

				if (i == 0) {
					textrange.Put("Text",(new JIString("Presentation1")).Variant);
					presentation.CallMethod("SaveAs", new object[]{ (new JIString("C:\\temp\\presentation1.ppt")).Variant,JIVariant.OPTIONAL_PARAM(),new int?(-1) });
					Thread.Sleep(3000);
					presentation.CallMethod("Close");
				}
				else {
					textrange.Put("Text",(new JIString("Presentation2")).Variant);
					slides.CallMethod("InsertFromFile", new object[]{ new JIString("C:\\temp\\presentation1.ppt"),new int?(1), new int?(1), new int?(1) });
					presentation.CallMethod("SaveAs", new object[]{ new JIString("C:\\temp\\presentation2.ppt"),JIVariant.OPTIONAL_PARAM(),new int?(-1) });
					Thread.Sleep(3000);
					presentation.CallMethod("Close");

					Dispatch.CallMethod("Quit");
				}


			}

			JISession.DestroySession(Dispatch.AssociatedSession);
		}

		public static void Main(string[] args) {

			try {
					if (args.Length < 4) {
						Console.WriteLine("Please provide address domain username password");
						return;
					}
					MSPowerPoint test = new MSPowerPoint(args[0],args);
					test.StartPowerPoint();
					test.ShowPowerPoint();
					test.PerformOp();
			}
				catch (Exception e) {
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
		}





	}

}