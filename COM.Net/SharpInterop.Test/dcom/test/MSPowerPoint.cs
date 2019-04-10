namespace org.jinterop.dcom.test {
    using IJIComObject = core.IJIComObject;
    using JIClsid = core.JIClsid;
    using JIComServer = core.JIComServer;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class MSPowerPoint
	{

		private JIComServer comStub;
		private IJIDispatch dispatch;
		private IJIComObject unknown;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public MSPowerPoint(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public MSPowerPoint(string address, string[] args)
		{
			var session = JISession.createSession(args[1],args[2],args[3]);
			comStub = new JIComServer(JIClsid.ValueOf("91493441-5A91-11CF-8700-00AA0060263B"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startPowerPoint() throws org.jinterop.dcom.common.JIException
		public virtual void startPowerPoint()
		{
			unknown = comStub.CreateInstance();
			dispatch = (IJIDispatch)JIObjectFactory.NarrowObject((IJIComObject)unknown.QueryInterface(impls.automation.DispatchFlags.IID));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showPowerPoint() throws org.jinterop.dcom.common.JIException
		public virtual void showPowerPoint()
		{
			var dispId = dispatch.GetIDsOfNames("Visible");
			var variant = new JIVariant(-1);
			dispatch.Put(dispId,variant);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void performOp()
		{
			//JIVariant variant = dispatch.get("Presentations");
			//JIInterfacePointer ptr = variant.getObjectAsInterfacePointer();
			//IJIDispatch presentations = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,ptr);
			var presentations = (IJIDispatch)JIObjectFactory.NarrowObject(dispatch.Get("Presentations").ObjectAsComObject);

			for (var i = 0; i < 2; i++)
			{
				var results = presentations.CallMethodA("Add",new object[]{JIVariant.CreateOPTIONAL_PARAM()});
				//variant = results[0];
				//ptr = variant.getObjectAsInterfacePointer();
				//IJIDispatch presentation = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,ptr);
				var presentation = (IJIDispatch)JIObjectFactory.NarrowObject(results[0].ObjectAsComObject);
				//variant = presentation.get("Slides");
				//ptr = variant.getObjectAsInterfacePointer();
				//IJIDispatch slides = (IJIDispatch)JIObjectFactory.createCOMInstance(unknown,ptr);
				var slides = (IJIDispatch)JIObjectFactory.NarrowObject(presentation.Get("Slides").ObjectAsComObject);

				results = slides.CallMethodA("Add", new object[]{ 1, new int?(1)});
				//variant = results[0];
				//ptr = variant.getObjectAsInterfacePointer();
				var slide = (IJIDispatch)JIObjectFactory.NarrowObject(results[0].ObjectAsComObject);

				//variant = slide.get("Shapes");
				//ptr = variant.getObjectAsInterfacePointer();
				var shapes = (IJIDispatch)JIObjectFactory.NarrowObject(slide.Get("Shapes").ObjectAsComObject);

				//variant = shapes.get("Title");
				//ptr = variant.getObjectAsInterfacePointer();
				var shape = (IJIDispatch)JIObjectFactory.NarrowObject(shapes.Get("Title").ObjectAsComObject);

				//variant = shape.get("TextFrame");
				//ptr = variant.getObjectAsInterfacePointer();
				var textframe = (IJIDispatch)JIObjectFactory.NarrowObject(shape.Get("TextFrame").ObjectAsComObject);

				//variant = textframe.get("TextRange");
				//ptr = variant.getObjectAsInterfacePointer();
				var textrange = (IJIDispatch)JIObjectFactory.NarrowObject(textframe.Get("TextRange").ObjectAsComObject);

				if (i == 0)
				{
					textrange.Put("Text",new JIString("Presentation1").Variant);
					presentation.CallMethod("SaveAs", new object[]{new JIString("C:\\temp\\presentation1.ppt").Variant,JIVariant.CreateOPTIONAL_PARAM(), -1 });
					Thread.Sleep(3000);
					presentation.CallMethod("Close");
				}
				else
				{
					textrange.Put("Text",new JIString("Presentation2").Variant);
					slides.CallMethod("InsertFromFile", new object[]{new JIString("C:\\temp\\presentation1.ppt"), 1, 1, 1 });
					presentation.CallMethod("SaveAs", new object[]{new JIString("C:\\temp\\presentation2.ppt"),JIVariant.CreateOPTIONAL_PARAM(), -1 });
					Thread.Sleep(3000);
					presentation.CallMethod("Close");

					dispatch.CallMethod("Quit");
				}


			}

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
					var test = new MSPowerPoint(args[0],args);
					test.startPowerPoint();
					test.showPowerPoint();
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