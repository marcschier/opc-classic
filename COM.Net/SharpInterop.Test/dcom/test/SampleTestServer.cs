namespace org.jinterop.dcom.test {
    using System;
    using IJIComObject = core.IJIComObject;
    using IJIDispatch = impls.automation.IJIDispatch;
    using JIArray = core.JIArray;
    using JICallBuilder = core.JICallBuilder;
    using JIComServer = core.JIComServer;
    using JIFlags = core.JIFlags;
    using JIPointer = core.JIPointer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIStruct = core.JIStruct;
    using JISystem = common.JISystem;
    using JIUnsignedFactory = core.JIUnsignedFactory;
    using JIUnsignedInteger = core.JIUnsignedInteger;
    using JIUnsignedShort = core.JIUnsignedShort;

    /// <summary>
    /// Contributed Code sample. Works in conjunction with SampleTestServers.zip
    /// 
    /// 
    /// 
    /// </summary>
    public class SampleTestServer {

        private readonly JIComServer _comStub;
        private readonly IJIComObject _comObject;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly string _address;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly JISession _session;

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public SampleTestServer(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
        public SampleTestServer(string address, string[] args) {
            _address = address;
            _session = JISession.CreateSession(args[1], args[2], args[3]);
            _comStub = new JIComServer(JIProgId.ValueOf("SampleTestServer.TestServer"), address, _session);
            var unknown = _comStub.CreateInstance();
            _comObject = unknown.QueryInterface("1F438B1C-02BA-462E-A971-8E0640C141E5"); //ITestServer
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void performSquare(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void PerformSquare(string[] args) {

            var callObject = new JICallBuilder(true) {
                Opnum = 1 //obtained from the IDL or TypeLib. //    AskTestServerToSquare
            };
            object[] results;
            short i = 3;
            callObject.AddInParamAsShort(i, JIFlags.FLAG_NULL);
            callObject.AddOutParamAsType(typeof(short), JIFlags.FLAG_NULL); //Short
            results = _comObject.Call(callObject);
            Console.WriteLine("ITestServer.AskTestServerToSquare succeeded, input=" + i + " output=" + results[0]);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void performCallback(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void PerformCallback(string[] args) {


        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void getTCharArray() throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetTCharArray() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            var callObject = new JICallBuilder(true) {
                Opnum = 6
            };
            object[] results;

            callObject.AddOutParamAsObject(new JIArray(typeof(byte), new int[] { 50 }, 1, false), JIFlags.FLAG_NULL);
            results = _comObject.Call(callObject);

            var arrayOfResults = (JIArray)results[0];
            var arrayOfBytes = (byte?[])arrayOfResults.ArrayInstance;
            var length = 50;
            for (var i = 0; i < length; i++) {
                Console.WriteLine((sbyte)arrayOfBytes[i]);
            }
        }


        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void setTCharArray() throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void SetTCharArray() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            var callObject = new JICallBuilder(true) {
                Opnum = 7
            };
            object[] results;
            callObject.AddInParamAsString("AHHHHHHH!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR);

            results = _comObject.Call(callObject);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void setConformantIntArray() throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void SetConformantIntArray() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            var callObject = new JICallBuilder(true) {
                Opnum = 9
            };
            object[] results;
            var i = 4;
            var intAry = new int?[i];
            for (var j = 0; j < i; j++) {
                intAry[j] = j;
            }
            var ary = new JIArray(intAry, true);
            callObject.AddInParamAsInt(i, JIFlags.FLAG_NULL);
            callObject.AddInParamAsArray(ary, JIFlags.FLAG_NULL);
            results = _comObject.Call(callObject);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void getConformantIntArray() throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetConformantIntArray() {

            var callObject = new JICallBuilder(true) {
                Opnum = 8
            };
            object[] results;

            callObject.AddOutParamAsType(typeof(int), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIPointer(new JIArray(typeof(int), null, 1, true)), JIFlags.FLAG_NULL);
            results = _comObject.Call(callObject);

            var arrayOfResults = (JIArray)((JIPointer)results[1]).GetReferent();
            var arrayOfIntegers = (int?[])arrayOfResults.ArrayInstance;
            var length = (int)results[0];
            for (var i = 0; i < length; i++) {
                Console.WriteLine((int)arrayOfIntegers[i]);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void GetStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetStruct(string[] args) {

            var callObject = new JICallBuilder(true) {
                Opnum = 10 //obtained from the IDL or TypeLib. //
            };
            object[] results;

            // change the struct to have the array as the last item
            var @struct = new JIStruct();
            var longArray = new JIArray(typeof(int), new int[] { 50 }, 1, false);
            @struct.AddMember(typeof(int));
            @struct.AddMember(typeof(float));
            @struct.AddMember(longArray);
            callObject.AddOutParamAsObject(new JIPointer(@struct), JIFlags.FLAG_NULL);

            results = _comObject.Call(callObject);
            Console.WriteLine(results[0]);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void getSimpleStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetSimpleStruct(string[] args) {

            var callObject = new JICallBuilder(true) {
                Opnum = 12 //obtained from the IDL or TypeLib. //
            };
            object[] results;

            var @struct = new JIStruct();
            @struct.AddMember(typeof(int));
            @struct.AddMember(typeof(double));
            @struct.AddMember(typeof(float));
            callObject.AddOutParamAsObject(new JIPointer(@struct), JIFlags.FLAG_NULL);

            results = _comObject.Call(callObject);
            Console.WriteLine(results[0]);
        }

        /*
             typedef struct stSimpleStruct
            {
               long     l;
               double   d;
               float    f;
            } SimpleStruct;

           [helpstring("13 method GetConformantStructArray")] HRESULT GetConformantStructArray(unsigned short* unDataSize,
                                                             [out, size_is(,*unDataSize)] SimpleStruct** ppSimpleStruct);


        */
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void getSimpleStructArray(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetSimpleStructArray(string[] args) {

            var callObject = new JICallBuilder(true) {
                Opnum = 13 //obtained from the IDL or TypeLib. //
            };
            object[] results;

            callObject.AddOutParamAsType(typeof(JIUnsignedShort), JIFlags.FLAG_NULL);

            var @struct = new JIStruct();
            @struct.AddMember(typeof(int));
            @struct.AddMember(typeof(double));
            @struct.AddMember(typeof(float));
            var DataArray = new JIArray(@struct, null, 1, true);
            callObject.AddOutParamAsObject(new JIPointer(DataArray), JIFlags.FLAG_NULL);
            results = _comObject.Call(callObject);
            Console.WriteLine(((JIUnsignedShort)results[0]).Value);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void getConformantStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetConformantStruct(string[] args) {

            var callObject = new JICallBuilder(true) {
                Opnum = 14 //obtained from the IDL or TypeLib. //
            };
            object[] results;

            var @struct = new JIStruct();
            @struct.AddMember(typeof(int));
            @struct.AddMember(typeof(double));
            @struct.AddMember(typeof(JIUnsignedShort));
            var longArray = new JIArray(typeof(int), null, 1, true);
            @struct.AddMember(new JIPointer(longArray));
            callObject.AddOutParamAsObject(new JIPointer(@struct), JIFlags.FLAG_NULL);

            results = _comObject.Call(callObject);
            Console.WriteLine(results[0]);
        }


        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void GetStructStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetStructStruct(string[] args) {

            var callObject = new JICallBuilder(true) {
                Opnum = 17 //obtained from the IDL or TypeLib. //
            };
            object[] results;

            var @struct = new JIStruct();
            @struct.AddMember(typeof(int));
            @struct.AddMember(typeof(double));
            @struct.AddMember(typeof(JIUnsignedShort));
            var longArray = new JIArray(typeof(int), null, 1, true);
            @struct.AddMember(new JIPointer(longArray));

            var StructStruct = new JIStruct();
            StructStruct.AddMember(typeof(int));
            StructStruct.AddMember(typeof(double));
            StructStruct.AddMember(@struct);

            callObject.AddOutParamAsObject(new JIPointer(StructStruct), JIFlags.FLAG_NULL);

            results = _comObject.Call(callObject);
            Console.WriteLine(results[0]);

        }


        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void GetStructStructArray(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetStructStructArray(string[] args) {

            var callObject = new JICallBuilder(true) {
                Opnum = 18 //obtained from the IDL or TypeLib. //
            };
            object[] results;

            var @struct = new JIStruct();
            @struct.AddMember(typeof(int));
            @struct.AddMember(typeof(double));
            @struct.AddMember(typeof(JIUnsignedShort));
            var longArray = new JIArray(typeof(int), null, 1, true);
            @struct.AddMember(new JIPointer(longArray));

            var StructStruct = new JIStruct();
            StructStruct.AddMember(typeof(int));
            StructStruct.AddMember(typeof(double));
            StructStruct.AddMember(@struct);

            var DataArray = new JIArray(StructStruct, null, 1, true);
            callObject.AddOutParamAsType(typeof(JIUnsignedShort), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIPointer(DataArray), JIFlags.FLAG_NULL);

            results = _comObject.Call(callObject);
            Console.WriteLine(((JIUnsignedShort)results[0]).Value);

        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void GetSimpleArrayStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetSimpleArrayStruct(string[] args) {

            var callObject = new JICallBuilder(true) {
                Opnum = 19 //obtained from the IDL or TypeLib. //
            };
            object[] results;

            var simpleStruct = new JIStruct();
            simpleStruct.AddMember(typeof(int));
            simpleStruct.AddMember(typeof(double));
            simpleStruct.AddMember(typeof(float));

            var simpleArrayStruct = new JIStruct();
            simpleArrayStruct.AddMember(typeof(int));
            simpleArrayStruct.AddMember(typeof(double));
            simpleArrayStruct.AddMember(typeof(JIUnsignedShort));
            var structArray = new JIArray(simpleStruct, null, 1, true);
            simpleArrayStruct.AddMember(new JIPointer(structArray));

            callObject.AddOutParamAsObject(new JIPointer(simpleArrayStruct), JIFlags.FLAG_NULL);

            results = _comObject.Call(callObject);
            Console.WriteLine(results[0]);

        }

        //[helpstring("20 method GetSimpleArrayStructArray")] HRESULT GetSimpleArrayStructArray([out] unsigned short* unDataSize,
        //    [out, size_is(,*unDataSize)] SimpleArrayStruct** pp);
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void GetSimpleArrayStructArray(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetSimpleArrayStructArray(string[] args) {

            var callObject = new JICallBuilder(true) {
                Opnum = 20 //obtained from the IDL or TypeLib. //
            };
            object[] results;

            var simpleStruct = new JIStruct();
            simpleStruct.AddMember(typeof(int));
            simpleStruct.AddMember(typeof(double));
            simpleStruct.AddMember(typeof(float));

            var simpleArrayStruct = new JIStruct();
            simpleArrayStruct.AddMember(typeof(int));
            simpleArrayStruct.AddMember(typeof(double));
            simpleArrayStruct.AddMember(typeof(JIUnsignedShort));
            var structArray = new JIArray(simpleStruct, null, 1, true);
            simpleArrayStruct.AddMember(new JIPointer(structArray)); //try no pointer next

            var DataArray = new JIArray(simpleArrayStruct, null, 1, true);
            callObject.AddOutParamAsType(typeof(JIUnsignedShort), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIPointer(DataArray), JIFlags.FLAG_NULL);

            results = _comObject.Call(callObject);
            Console.WriteLine(((JIUnsignedShort)results[0]).Value);

        }


        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void SetSimpleArrayStructArray(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void SetSimpleArrayStructArray(string[] args) {

            var callObject = new JICallBuilder(true) {
                Opnum = 21 //obtained from the IDL or TypeLib. ModifyStaticData
            };
            object[] results;

            var simpleStruct = new JIStruct();
            simpleStruct.AddMember(5);
            simpleStruct.AddMember(25);
            simpleStruct.AddMember(2.5);

            var shortValue = new int?(1);
            var simpleArrayStruct = new JIStruct();
            simpleArrayStruct.AddMember(54);
            simpleArrayStruct.AddMember(5);
            simpleArrayStruct.AddMember(JIUnsignedFactory.GetUnsigned(shortValue, JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT));
            var structArray = new JIStruct[1];
            structArray[0] = simpleStruct;
            simpleArrayStruct.AddMember(new JIPointer(new JIArray(structArray, true)));
            var DataArray = new JIStruct[1];
            DataArray[0] = simpleArrayStruct;
            short size = 1;
            callObject.AddInParamAsShort(size, JIFlags.FLAG_NULL);
            callObject.AddInParamAsArray(new JIArray(DataArray, true), JIFlags.FLAG_NULL);

            results = _comObject.Call(callObject);
            Console.WriteLine("SetSimpleArrayStructArray worked!");
        }



        // Index out of bound exception
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void GetStaticStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetStaticStruct(string[] args) {

            var callObject = new JICallBuilder(true) {
                Opnum = 15 //obtained from the IDL or TypeLib. //
            };
            object[] results;

            var varStruct = new JIStruct();
            varStruct.AddMember(typeof(JIUnsignedInteger));
            varStruct.AddMember(typeof(float));
            varStruct.AddMember(typeof(float));
            varStruct.AddMember(typeof(JIUnsignedShort));
            varStruct.AddMember(typeof(float));
            varStruct.AddMember(typeof(DateTime));
            varStruct.AddMember(typeof(JIUnsignedInteger));

            var pointStruct = new JIStruct();
            pointStruct.AddMember(typeof(JIUnsignedInteger));
            pointStruct.AddMember(typeof(JIUnsignedInteger));
            pointStruct.AddMember(typeof(sbyte));
            var structArray = new JIArray(varStruct, null, 1, true);
            pointStruct.AddMember(new JIPointer(structArray));


            var DataArray = new JIArray(pointStruct, null, 1, true);
            callObject.AddOutParamAsType(typeof(JIUnsignedShort), JIFlags.FLAG_NULL);
            callObject.AddOutParamAsObject(new JIPointer(DataArray, false), JIFlags.FLAG_NULL);


            results = _comObject.Call(callObject);
            Console.WriteLine(((JIUnsignedShort)results[0]).Value);

        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void SetStaticStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void SetStaticStruct(string[] args) {

            var callObject = new JICallBuilder(true) {
                Opnum = 16 //obtained from the IDL or TypeLib.
            };
            object[] results;
            var value = new long?(10);
            var shortValue = new int?(5);
            var varStruct = new JIStruct();
            varStruct.AddMember(JIUnsignedFactory.GetUnsigned(value, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT));
            varStruct.AddMember(1.1);
            varStruct.AddMember(1.2);
            varStruct.AddMember(JIUnsignedFactory.GetUnsigned(shortValue, JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT));
            varStruct.AddMember(1.0);
            varStruct.AddMember(DateTime.Now);
            varStruct.AddMember(JIUnsignedFactory.GetUnsigned(value, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT));

            var pointStruct = new JIStruct();
            pointStruct.AddMember(JIUnsignedFactory.GetUnsigned(15, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT));
            pointStruct.AddMember(JIUnsignedFactory.GetUnsigned(10, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT));
            pointStruct.AddMember((sbyte)1);
            var varStructArray = new JIStruct[1];
            varStructArray[0] = varStruct;
            pointStruct.AddMember(new JIPointer(new JIArray(varStructArray, true))); //since this is an embedded pointer

            var pointAry = new JIStruct[1];
            pointAry[0] = pointStruct;

            var ary = new JIArray(pointAry, true);
            callObject.AddInParamAsShort(1, JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT);
            callObject.AddInParamAsArray(ary, JIFlags.FLAG_NULL);

            results = _comObject.Call(callObject);
            Console.WriteLine("SetStaticStruct worked!");
        }


        public static void Main(string[] args) {

            try {
                if (args.Length < 4) {
                    Console.WriteLine("Please provide address domain username password");
                    return;
                }

                JISystem.UseAutoRegistration = true;
                var test = new SampleTestServer(args[0], args);

                test.PerformCallback(args);
                test.PerformSquare(args);
                test.SetTCharArray();
                test.GetTCharArray();
                test.SetConformantIntArray();
                test.GetConformantIntArray();
                test.GetStruct(args);
                test.GetSimpleStruct(args);
                test.GetConformantStruct(args);
                test.GetSimpleStructArray(args);
                test.GetStructStruct(args);
                test.GetStructStructArray(args);
                test.GetSimpleArrayStruct(args);
                //
                test.GetSimpleArrayStructArray(args);
                test.SetSimpleArrayStructArray(args);
                test.GetStaticStruct(args);
                test.SetStaticStruct(args);
            }
            catch (Exception e) {
                // TODO Auto-generated catch block
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }


    }

}