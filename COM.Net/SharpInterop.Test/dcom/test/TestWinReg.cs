namespace org.jinterop.dcom.test {

    using IJIAuthInfo = common.IJIAuthInfo;
    using JIDefaultAuthInfoImpl = common.JIDefaultAuthInfoImpl;
    using JIException = common.JIException;
    using IJIWinReg = winreg.IJIWinReg;
    using JIPolicyHandle = winreg.JIPolicyHandle;
    using JIWinRegFactory = winreg.JIWinRegFactory;


    /// 
    /// <summary>
    /// Make sure you have Administrator level access on the target machine and if your password/username has special
    /// characters , please use the URLEncoder before passing them to WinReg example. 
    /// 
    /// </summary>
    public class TestWinReg
	{

		public static void Main(string[] args)
		{

			if (args.Length < 5)
			{
				Console.WriteLine("Please provide address domain username password keyname");
				return;
			}
			IJIAuthInfo authInfo = new JIDefaultAuthInfoImpl(args[1],args[2],args[3]);



			try
			{
				var registry = JIWinRegFactory.SingleTon.getWinreg(authInfo,args[0],true);
				//Open HKLM
				var policyHandle = registry.winreg_OpenHKLM();
				//Open a key here
				var policyHandle2 = registry.winreg_OpenKey(policyHandle,"Software\\Classes", winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);

				Console.WriteLine("Printing first 1000 entries under \"Software\\Classes\"...");
				for (var i = 0;i < 1000;i++)
				{
					var values = registry.winreg_EnumKey(policyHandle2,i);
					Console.WriteLine(values[0] + " , " + values[1]);
				}

				Console.WriteLine("****************************************************");
				Console.WriteLine("\nCreating Key " + args[4] + " under \"Software\\Classes\"...");
				var key = args[4].Trim();
				var policyHandle3 = registry.winreg_CreateKey(policyHandle2,key, winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE, winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);

				Console.WriteLine("Setting values to " + key);
				registry.winreg_SetValue(policyHandle3,"j-Interop_None");
				registry.winreg_SetValue(policyHandle3,"j-Interop_String",".".GetBytes(),false,false);
				var values1 = registry.winreg_QueryValue(policyHandle3,"j-Interop_String",1024);
				registry.winreg_SetValue(policyHandle3,"j-Interop_String_Ex","%PATH%\\Test12345".GetBytes(),false,true);
				registry.winreg_SetValue(policyHandle3,"j-Interop_Bin","123456789".GetBytes(),true,false);
				registry.winreg_SetValue(policyHandle3,"j-Interop_Dword",100);

				string[] strings = {"123", "456", "6789", "10","11"};
				var data = new sbyte[strings.Length][];
				for (var i = 0; i < strings.Length;i++)
				{
					data[i] = strings[i].GetBytes();
				}

				registry.winreg_SetValue(policyHandle3,"j-Interop_Multi_sz",data);

				for (var i = 0; i < 6;i++)
				{
					var values = registry.winreg_EnumValue(policyHandle3,i);
					Console.WriteLine(values[0] + " , " + values[1]);
				}

				Console.WriteLine("Retrieving j-Interop_String_Ex value " + key);
				var values = registry.winreg_QueryValue(policyHandle3,"j-Interop_String_Ex",1024);
				Console.WriteLine(StringHelperClass.NewString((sbyte[])values[1]));

				Console.WriteLine("Deleting j-Interop_Bin value");
				registry.winreg_DeleteKeyOrValue(policyHandle3,"j-Interop_Bin",false);

				Console.WriteLine("Saving the " + key + " in a file to local server location as c:\\temp\\j-Interop");
				registry.winreg_SaveFile(policyHandle3,"c:\\temp\\j-Interop");

				registry.winreg_CloseKey(policyHandle3);
				registry.winreg_CloseKey(policyHandle2);
				registry.winreg_CloseKey(policyHandle);
				registry.closeConnection();

	//			
	//			//Open HKCR
	//			policyHandle = registry.winreg_OpenHKCR();
	//			
	//			policyHandle2 = registry.winreg_OpenKey(policyHandle,"ClSID",IJIWinReg.KEY_ALL_ACCESS);
	//			policyHandle3 = registry.winreg_CreateKey(policyHandle2,"j-Interop007",IJIWinReg.REG_OPTION_NON_VOLATILE,IJIWinReg.KEY_ALL_ACCESS);
	//			registry.winreg_CloseKey(policyHandle3);
	//			registry.winreg_CloseKey(policyHandle2);
	//			registry.winreg_CloseKey(policyHandle);
	//			
	//			//Open HKCU
	//			policyHandle = registry.winreg_OpenHKCU();
	//			
	//			policyHandle2 = registry.winreg_OpenKey(policyHandle,"Software\\Classes",IJIWinReg.KEY_ALL_ACCESS);
	//			registry.winreg_CloseKey(policyHandle2);
	//			registry.winreg_CloseKey(policyHandle);
	//			
	//			//Open HKU
	//			policyHandle = registry.winreg_OpenHKU();
	//			
	//			policyHandle2 = registry.winreg_OpenKey(policyHandle,".DEFAULT",IJIWinReg.KEY_ALL_ACCESS);
	//			registry.winreg_CloseKey(policyHandle2);
	//			registry.winreg_CloseKey(policyHandle);


			}
			catch (JIException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			catch (UnknownHostException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}



		}

	}

}