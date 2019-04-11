using System;

namespace org.jinterop.dcom.test {

    using IJIAuthInfo = org.jinterop.dcom.common.IJIAuthInfo;
    using JIDefaultAuthInfoImpl = org.jinterop.dcom.common.JIDefaultAuthInfoImpl;
    using JIException = org.jinterop.dcom.common.JIException;
    using IJIWinReg = org.jinterop.winreg.IJIWinReg;
    using JIPolicyHandle = org.jinterop.winreg.JIPolicyHandle;
    using JIWinRegFactory = org.jinterop.winreg.JIWinRegFactory;


    /// 
    /// <summary>
    /// Make sure you have Administrator level access on the target machine and if your password/username has special
    /// characters , please use the URLEncoder before passing them to WinReg example. 
    /// 
    /// </summary>
    public class TestWinReg {

        public static void Main(string[] args) {

            if (args.Length < 5) {
                Console.WriteLine("Please provide address domain username password keyname");
                return;
            }
            IJIAuthInfo authInfo = new JIDefaultAuthInfoImpl(args[1],args[2],args[3]);



            try {
                IJIWinReg registry = JIWinRegFactory.SingleTon.GetWinreg(authInfo,args[0],true);
                //Open HKLM
                JIPolicyHandle policyHandle = registry.Winreg_OpenHKLM();
                //Open a key here
                JIPolicyHandle policyHandle2 = registry.Winreg_OpenKey(policyHandle,"Software\\Classes",org.jinterop.winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);

                Console.WriteLine("Printing first 1000 entries under \"Software\\Classes\"...");
                for (int i = 0;i < 1000;i++) {
                    string[] values = registry.Winreg_EnumKey(policyHandle2,i);
                    Console.WriteLine(values[0] + " , " + values[1]);
                }

                Console.WriteLine("****************************************************");
                Console.WriteLine("\nCreating Key " + args[4] + " under \"Software\\Classes\"...");
                string key = args[4].Trim();
                JIPolicyHandle policyHandle3 = registry.Winreg_CreateKey(policyHandle2,key,org.jinterop.winreg.IJIWinReg_Fields.REG_OPTION_NON_VOLATILE,org.jinterop.winreg.IJIWinReg_Fields.KEY_ALL_ACCESS);

                Console.WriteLine("Setting values to " + key);
                registry.Winreg_SetValue(policyHandle3,"j-Interop_None");
                registry.Winreg_SetValue(policyHandle3,"j-Interop_String",".".GetBytes(),false,false);
                object[] values1 = registry.Winreg_QueryValue(policyHandle3,"j-Interop_String",1024);
                registry.Winreg_SetValue(policyHandle3,"j-Interop_String_Ex","%PATH%\\Test12345".GetBytes(),false,true);
                registry.Winreg_SetValue(policyHandle3,"j-Interop_Bin","123456789".GetBytes(),true,false);
                registry.Winreg_SetValue(policyHandle3,"j-Interop_Dword",100);

                string[] strings = new string[] { "123", "456", "6789", "10","11" };
                sbyte[][] data = new sbyte[strings.Length][];
                for (int i = 0; i < strings.Length;i++) {
                    data[i] = strings[i].GetBytes();
                }

                registry.Winreg_SetValue(policyHandle3,"j-Interop_Multi_sz",data);

                for (int i = 0; i < 6;i++) {
                    object[] values = registry.Winreg_EnumValue(policyHandle3,i);
                    Console.WriteLine(values[0] + " , " + values[1]);
                }

                Console.WriteLine("Retrieving j-Interop_String_Ex value " + key);
                object[] values = registry.Winreg_QueryValue(policyHandle3,"j-Interop_String_Ex",1024);
                Console.WriteLine(StringHelperClass.NewString((sbyte[])values[1]));

                Console.WriteLine("Deleting j-Interop_Bin value");
                registry.Winreg_DeleteKeyOrValue(policyHandle3,"j-Interop_Bin",false);

                Console.WriteLine("Saving the " + key + " in a file to local server location as c:\\temp\\j-Interop");
                registry.Winreg_SaveFile(policyHandle3,"c:\\temp\\j-Interop");

                registry.Winreg_CloseKey(policyHandle3);
                registry.Winreg_CloseKey(policyHandle2);
                registry.Winreg_CloseKey(policyHandle);
                registry.CloseConnection();

    //            
    //            //Open HKCR
    //            policyHandle = registry.winreg_OpenHKCR();
    //            
    //            policyHandle2 = registry.winreg_OpenKey(policyHandle,"ClSID",IJIWinReg.KEY_ALL_ACCESS);
    //            policyHandle3 = registry.winreg_CreateKey(policyHandle2,"j-Interop007",IJIWinReg.REG_OPTION_NON_VOLATILE,IJIWinReg.KEY_ALL_ACCESS);
    //            registry.winreg_CloseKey(policyHandle3);
    //            registry.winreg_CloseKey(policyHandle2);
    //            registry.winreg_CloseKey(policyHandle);
    //            
    //            //Open HKCU
    //            policyHandle = registry.winreg_OpenHKCU();
    //            
    //            policyHandle2 = registry.winreg_OpenKey(policyHandle,"Software\\Classes",IJIWinReg.KEY_ALL_ACCESS);
    //            registry.winreg_CloseKey(policyHandle2);
    //            registry.winreg_CloseKey(policyHandle);
    //            
    //            //Open HKU
    //            policyHandle = registry.winreg_OpenHKU();
    //            
    //            policyHandle2 = registry.winreg_OpenKey(policyHandle,".DEFAULT",IJIWinReg.KEY_ALL_ACCESS);
    //            registry.winreg_CloseKey(policyHandle2);
    //            registry.winreg_CloseKey(policyHandle);


            }
            catch (JIException e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            catch (UnknownHostException e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }



        }

    }

}