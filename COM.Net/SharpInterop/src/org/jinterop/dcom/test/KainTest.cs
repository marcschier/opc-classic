namespace org.jinterop.dcom.test {
    using IJIComObject = core.IJIComObject;
    using JIComServer = core.JIComServer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIString = core.JIString;
    using JIVariant = core.JIVariant;
    using JIObjectFactory = impls.JIObjectFactory;
    using IJIDispatch = impls.automation.IJIDispatch;

    public class KainTest
	{

		private JIComServer comServer;
		private IJIDispatch dispatch;
		private IJIComObject unknown;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public KainTest(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
		public KainTest(string address, string[] args)
		{
			var session = JISession.createSession(args[1],args[2],args[3]);
			comServer = new JIComServer(JIProgId.valueOf("Word.Application"),address,session);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startWord() throws org.jinterop.dcom.common.JIException
		public virtual void startWord()
		{
			unknown = comServer.CreateInstance();
			var dispatch = (IJIDispatch)JIObjectFactory.narrowObject(unknown.QueryInterface(impls.automation.IJIDispatch_Fields.IID));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void showWord() throws org.jinterop.dcom.common.JIException
		public virtual void showWord()
		{
			var dispId = dispatch.getIDsOfNames("Visible");
			var variant = new JIVariant(true);
			dispatch.put(dispId,variant);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performOp() throws org.jinterop.dcom.common.JIException, InterruptedException
		public virtual void performOp()
		{
			 var sDir = "c:\\tmp\\";
			 var sInputDoc = sDir + "file_in.doc";
			 var sOutputDoc = sDir + "file_out.doc";

			 var sOldText = "[label:import:1]";
			 var sNewText = "I am some horribly long sentence, so long that [insert something long here]";
			 var tVisible = true;
			 var tSaveOnExit = false;

			Console.WriteLine(((JIVariant)dispatch.get("Version")).ObjectAsString.String);
			Console.WriteLine(((JIVariant)dispatch.get("Path")).ObjectAsString.String);

			var variant = dispatch.get("Documents");
			var documents = (IJIDispatch)JIObjectFactory.narrowObject(variant.ObjectAsComObject);
			//String has to be a JIString.
			var filePath = new JIString(sInputDoc);
			//this "open" is of Word 2003
			var variant2 = documents.callMethodA("open",new object[]{new JIVariant(filePath,true),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});

			var document = (IJIDispatch)JIObjectFactory.narrowObject(variant2[0].ObjectAsComObject);
			variant = dispatch.get("Selection");
			var selection = (IJIDispatch)JIObjectFactory.narrowObject(variant.ObjectAsComObject);

			variant = selection.get("Find");
			var find = (IJIDispatch)JIObjectFactory.narrowObject(variant.ObjectAsComObject);

			Thread.Sleep(2000);

			find.put("Text",new JIVariant(new JIString(sOldText)));
			find.callMethodA("Execute",new object[]{JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(), JIVariant.CreateOPTIONAL_PARAM()});

			Thread.Sleep(2000);

			selection.put("Text",new JIVariant(new JIString(sNewText)));
			selection.callMethodA("MoveDown",new object[]{JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
			selection.put("Text",new JIVariant(new JIString("\nSo we got the next line including BR.\n")));

			variant = selection.get("Font");
			var font = (IJIDispatch)JIObjectFactory.narrowObject(variant.ObjectAsComObject);
			font.put("Bold",new JIVariant(1));
			font.put("Italic",new JIVariant(1));
			font.put("Underline",new JIVariant(0));

			variant = selection.get("ParagraphFormat");
			var align = (IJIDispatch)JIObjectFactory.narrowObject(variant.ObjectAsComObject);
			align.put("Alignment",new JIVariant(3));

			Thread.Sleep(5000);

			var sImgFile = new JIString(sDir + "image.png");
			selection.callMethodA("MoveDown",new object[]{JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
			variant = selection.get("InLineShapes");
			var image = (IJIDispatch)JIObjectFactory.narrowObject(variant.ObjectAsComObject);
			image.callMethodA("AddPicture",new object[]{new JIVariant(sImgFile),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});

			var sHyperlink = new JIString("http://www.google.com");
			selection.put("Text",new JIVariant(new JIString("Text for the link to Google")));
			variant = selection.get("Range");
			var range = (IJIDispatch)JIObjectFactory.narrowObject(variant.ObjectAsComObject);
			variant = document.get("Hyperlinks");
			var link = (IJIDispatch)JIObjectFactory.narrowObject(variant.ObjectAsComObject);
			link.callMethod("Add",new object[]{range,sHyperlink,JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});

			variant = dispatch.get("WordBasic");
			var wordBasic = (IJIDispatch)JIObjectFactory.narrowObject(variant.ObjectAsComObject);
			wordBasic.callMethod("FileSaveAs",new object[]{new JIString(sOutputDoc)});

			dispatch.callMethod("Quit", new object[]{new JIVariant(-1,true),JIVariant.CreateOPTIONAL_PARAM(),JIVariant.CreateOPTIONAL_PARAM()});
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
					var test = new KainTest(args[0],args);
					test.startWord();
					test.showWord();
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