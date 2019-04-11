using SharpCifs;
using SharpCifs.Smb;
using SharpCifs.Util.Sharpen;
using System;
using System.IO;

namespace org.jinterop.dcom.test {

    public class Test {

#pragma warning disable IDE0060 // Remove unused parameter
        public static void Main(string[] args) {
#pragma warning restore IDE0060 // Remove unused parameter
            try {

                // Socket socket = new Socket("10.24.10.65",139);
                // socket.close();
                //     UniAddress mydomaincontoller = UniAddress.getByName( "192.168.170.6" );
                //     NtlmPasswordAuthentication mycreds = new NtlmPasswordAuthentication( "itlinfosys", "vikram_roopchand", "Dilbert007" );
                // 
                //     SmbSession.logon( mydomaincontoller, mycreds );
                //     Config.setProperty("SharpCifs.smb.client.laddr","10.24.10.65");
                //     Config.setProperty("SharpCifs.smb.client.domain","itl-hw-lt15522.ad.infosys.com");
                //         System.setProperty("SharpCifs.smb.client.laddr","10.24.10.65");
                //         System.setProperty("SharpCifs.smb.client.domain","itl-hw-lt15522.ad.infosys.com");
                //         System.setProperty("SharpCifs.netbios.hostname","itl-hw-lt15522.ad.infosys.com");

                // NtlmChallenge challenge = SmbSession.getChallengeForDomain();

                var mydomaincontoller = UniAddress.GetByName("itl-hw-lt15522");
                var mycreds = new NtlmPasswordAuthentication("itl-hw-lt15522", "TestUser", "Enabler2000");
                SmbSession.Logon(mydomaincontoller, mycreds);
                // PLEASE NOTE THAT THE WINDOWS "SERVER" SERVICE SOULD BE RUNNING !!! OTHERWISE THE
                // GETCHALLENGE WILL FAIL.
                //         UniAddress mydomaincontoller = UniAddress.getByName("itl-hw-lt15522.ad.infosys.com");
                //         byte[] b =  SmbSession.getChallenge(mydomaincontoller,139);
                // NtlmChallenge challenge = SmbSession.getChallengeForDomain();

                // SUCCESS

            }
            catch (SmbAuthException sae) {
                // AUTHENTICATION FAILURE
                Console.WriteLine(sae.ToString());
                Console.Write(sae.StackTrace);
            }
            catch (SmbException se) {
                // NETWORK PROBLEMS?
                Console.WriteLine(se.ToString());
                Console.Write(se.StackTrace);
            }
            catch (UnknownHostException e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            catch (IOException e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }

        public void DoTest() => Console.WriteLine("Called back !!!");

    }

}