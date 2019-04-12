//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Test {
    using SharpInterop.Core;
    using System;

    public static class TestWinNativeSSO {

#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RECS0154 // Parameter is never used
        public static void RunTest(string[] args) {
#pragma warning restore RECS0154 // Parameter is never used
#pragma warning restore IDE0060 // Remove unused parameter

            try {

                var session = Session.CreateSession();
                var comServer = new ComServer(Clsid.ValueOf("00024500-0000-0000-C000-000000000046"), session);
                var comObject = comServer.CreateInstance();

                //            SSPIJNIClient jniClient = SSPIJNIClient.getInstance();
                //            byte[] type1Message = jniClient.invokePrepareSSORequest();
                //            Utils.HexString(type1Message, 0, type1Message.length);
                //            int h = 0;
                //
                //            jniClient.invokeUnInitialize();
                //
                //            type1Message = new Type1Message().toByteArray();
                //            Utils.HexString(type1Message, 0, type1Message.length);
            }
            catch (Exception e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

        }
    }

}