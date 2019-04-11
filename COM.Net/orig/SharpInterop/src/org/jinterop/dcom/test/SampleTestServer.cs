using System;

namespace org.jinterop.dcom.test {


    using JIException = org.jinterop.dcom.common.JIException;
    using JISystem = org.jinterop.dcom.common.JISystem;
    using IJIComObject = org.jinterop.dcom.core.IJIComObject;
    using JIArray = org.jinterop.dcom.core.JIArray;
    using JICallBuilder = org.jinterop.dcom.core.JICallBuilder;
    using JIComServer = org.jinterop.dcom.core.JIComServer;
    using JIFlags = org.jinterop.dcom.core.JIFlags;
    using JIPointer = org.jinterop.dcom.core.JIPointer;
    using JIProgId = org.jinterop.dcom.core.JIProgId;
    using JISession = org.jinterop.dcom.core.JISession;
    using JIStruct = org.jinterop.dcom.core.JIStruct;
    using JIUnsignedFactory = org.jinterop.dcom.core.JIUnsignedFactory;
    using JIUnsignedInteger = org.jinterop.dcom.core.JIUnsignedInteger;
    using JIUnsignedShort = org.jinterop.dcom.core.JIUnsignedShort;
    using IJIDispatch = org.jinterop.dcom.impls.automation.IJIDispatch;

    /// <summary>
    /// Contributed Code sample. Works in conjunction with SampleTestServers.zip
    /// 
    /// 
    /// 
    /// </summary>
    public class SampleTestServer {

      private JIComServer ComStub = null;
      private IJIComObject ComObject = null;
      private IJIDispatch Dispatch = null;
      private string Address = null;
      private JISession Session = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SampleTestServer(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
      public SampleTestServer(string address, string[] args) {
        this.Address = address;
        Session = JISession.CreateSession(args[1], args[2], args[3]);
        ComStub = new JIComServer(JIProgId.ValueOf("SampleTestServer.TestServer"), address, Session);
        IJIComObject unknown = ComStub.CreateInstance();
        ComObject = (IJIComObject) unknown.QueryInterface("1F438B1C-02BA-462E-A971-8E0640C141E5"); //ITestServer
      }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performSquare(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
      public virtual void PerformSquare(string[] args) {

        JICallBuilder callObject = new JICallBuilder(true);
        callObject.Opnum = 1; //obtained from the IDL or TypeLib. //    AskTestServerToSquare
        object[] results;
        short i = 3;
        callObject.AddInParamAsShort(i, JIFlags.FLAG_NULL);
        callObject.AddOutParamAsType(typeof(short?), JIFlags.FLAG_NULL); //Short
        results = ComObject.Call(callObject);
        Console.WriteLine("ITestServer.AskTestServerToSquare succeeded, input=" + i + " output=" + results[0]);
      }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performCallback(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
      public virtual void PerformCallback(string[] args) {


      }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getTCharArray() throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
      public virtual void GetTCharArray() {
          System.gc();
          JICallBuilder callObject = new JICallBuilder(true);
          callObject.Opnum = 6;
          object[] results;

          callObject.AddOutParamAsObject(new JIArray(typeof(sbyte?), new int[]{ 50 },1,false), JIFlags.FLAG_NULL);
          results = ComObject.Call(callObject);

          JIArray arrayOfResults = (JIArray)results[0];
          sbyte?[] arrayOfBytes = (sbyte?[]) arrayOfResults.ArrayInstance;
          int length = 50;
          for (int i = 0; i < length; i++) {
            Console.WriteLine((sbyte)arrayOfBytes[i]);
          }
      }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setTCharArray() throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
      public virtual void SetTCharArray() {
          System.gc();
          JICallBuilder callObject = new JICallBuilder(true);
          callObject.Opnum = 7;
          object[] results;
          callObject.AddInParamAsString("AHHHHHHH!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR);

          results = ComObject.Call(callObject);
      }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setConformantIntArray() throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
      public virtual void SetConformantIntArray() {
          System.gc();
          JICallBuilder callObject = new JICallBuilder(true);
          callObject.Opnum = 9;
          object[] results;
          int i = 4;
          int?[] intAry = new int?[i];
          for (int j = 0; j < i; j++) {
              intAry[j] = new int?(j);
          }
          JIArray ary = new JIArray(intAry, true);
          callObject.AddInParamAsInt(i, JIFlags.FLAG_NULL);
          callObject.AddInParamAsArray(ary, JIFlags.FLAG_NULL);
          results = ComObject.Call(callObject);
      }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getConformantIntArray() throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
      public virtual void GetConformantIntArray() {

          JICallBuilder callObject = new JICallBuilder(true);
          callObject.Opnum = 8;
          object[] results;

          callObject.AddOutParamAsType(typeof(int?), JIFlags.FLAG_NULL);
          callObject.AddOutParamAsObject(new JIPointer(new JIArray(typeof(int?), null, 1, true)), JIFlags.FLAG_NULL);
          results = ComObject.Call(callObject);

          JIArray arrayOfResults = (JIArray)((JIPointer)results[1]).GetReferent();
          int?[] arrayOfIntegers = (int?[]) arrayOfResults.ArrayInstance;
          int length = (int)((int?)results[0]);
          for (int i = 0; i < length; i++) {
            Console.WriteLine((int)arrayOfIntegers[i]);
          }
      }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void GetStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetStruct(string[] args) {

            JICallBuilder callObject = new JICallBuilder(true);
            callObject.Opnum = 10; //obtained from the IDL or TypeLib. //
            object[] results;

            // change the struct to have the array as the last item
            JIStruct @struct = new JIStruct();
            JIArray longArray = new JIArray(typeof(int?), new int[]{ 50 },1,false);
            @struct.AddMember(typeof(int?));
            @struct.AddMember(typeof(float?));
            @struct.AddMember(longArray);
            callObject.AddOutParamAsObject(new JIPointer(@struct), JIFlags.FLAG_NULL);

            results = ComObject.Call(callObject);
            Console.WriteLine(results[0]);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getSimpleStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetSimpleStruct(string[] args) {

            JICallBuilder callObject = new JICallBuilder(true);
            callObject.Opnum = 12; //obtained from the IDL or TypeLib. //
            object[] results;

            JIStruct @struct = new JIStruct();
            @struct.AddMember(typeof(int?));
            @struct.AddMember(typeof(double?));
            @struct.AddMember(typeof(float?));
            callObject.AddOutParamAsObject(new JIPointer(@struct), JIFlags.FLAG_NULL);

            results = ComObject.Call(callObject);
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

            JICallBuilder callObject = new JICallBuilder(true);
            callObject.Opnum = 13; //obtained from the IDL or TypeLib. //
            object[] results;

            callObject.AddOutParamAsType(typeof(JIUnsignedShort), JIFlags.FLAG_NULL);

            JIStruct @struct = new JIStruct();
            @struct.AddMember(typeof(int?));
            @struct.AddMember(typeof(double?));
            @struct.AddMember(typeof(float?));
            JIArray DataArray = new JIArray(@struct, null, 1, true);
            callObject.AddOutParamAsObject(new JIPointer(DataArray), JIFlags.FLAG_NULL);
            results = ComObject.Call(callObject);
            Console.WriteLine(((JIUnsignedShort)results[0]).Value);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getConformantStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetConformantStruct(string[] args) {

            JICallBuilder callObject = new JICallBuilder(true);
            callObject.Opnum = 14; //obtained from the IDL or TypeLib. //
            object[] results;

            JIStruct @struct = new JIStruct();
            @struct.AddMember(typeof(int?));
            @struct.AddMember(typeof(double?));
            @struct.AddMember(typeof(JIUnsignedShort));
            JIArray longArray = new JIArray(typeof(int?), null, 1, true);
            @struct.AddMember(new JIPointer(longArray));
            callObject.AddOutParamAsObject(new JIPointer(@struct), JIFlags.FLAG_NULL);

            results = ComObject.Call(callObject);
            Console.WriteLine(results[0]);
        }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void GetStructStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetStructStruct(string[] args) {

          JICallBuilder callObject = new JICallBuilder(true);
          callObject.Opnum = 17; //obtained from the IDL or TypeLib. //
          object[] results;

          JIStruct @struct = new JIStruct();
          @struct.AddMember(typeof(int?));
          @struct.AddMember(typeof(double?));
          @struct.AddMember(typeof(JIUnsignedShort));
          JIArray longArray = new JIArray(typeof(int?), null, 1, true);
          @struct.AddMember(new JIPointer(longArray));

          JIStruct StructStruct = new JIStruct();
          StructStruct.AddMember(typeof(int?));
          StructStruct.AddMember(typeof(double?));
          StructStruct.AddMember(@struct);

         callObject.AddOutParamAsObject(new JIPointer(StructStruct), JIFlags.FLAG_NULL);

          results = ComObject.Call(callObject);
          Console.WriteLine(results[0]);

        }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void GetStructStructArray(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetStructStructArray(string[] args) {

          JICallBuilder callObject = new JICallBuilder(true);
          callObject.Opnum = 18; //obtained from the IDL or TypeLib. //
          object[] results;

          JIStruct @struct = new JIStruct();
          @struct.AddMember(typeof(int?));
          @struct.AddMember(typeof(double?));
          @struct.AddMember(typeof(JIUnsignedShort));
          JIArray longArray = new JIArray(typeof(int?), null, 1, true);
          @struct.AddMember(new JIPointer(longArray));

          JIStruct StructStruct = new JIStruct();
          StructStruct.AddMember(typeof(int?));
          StructStruct.AddMember(typeof(double?));
          StructStruct.AddMember(@struct);

          JIArray DataArray = new JIArray(StructStruct, null, 1, true);
          callObject.AddOutParamAsType(typeof(JIUnsignedShort), JIFlags.FLAG_NULL);
          callObject.AddOutParamAsObject(new JIPointer(DataArray), JIFlags.FLAG_NULL);

          results = ComObject.Call(callObject);
          Console.WriteLine(((JIUnsignedShort)results[0]).Value);

        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void GetSimpleArrayStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetSimpleArrayStruct(string[] args) {

          JICallBuilder callObject = new JICallBuilder(true);
          callObject.Opnum = 19; //obtained from the IDL or TypeLib. //
          object[] results;

          JIStruct simpleStruct = new JIStruct();
          simpleStruct.AddMember(typeof(int?));
          simpleStruct.AddMember(typeof(double?));
          simpleStruct.AddMember(typeof(float?));

          JIStruct simpleArrayStruct = new JIStruct();
          simpleArrayStruct.AddMember(typeof(int?));
          simpleArrayStruct.AddMember(typeof(double?));
          simpleArrayStruct.AddMember(typeof(JIUnsignedShort));
          JIArray structArray = new JIArray(simpleStruct, null, 1, true);
          simpleArrayStruct.AddMember(new JIPointer(structArray));

          callObject.AddOutParamAsObject(new JIPointer(simpleArrayStruct), JIFlags.FLAG_NULL);

          results = ComObject.Call(callObject);
          Console.WriteLine(results[0]);

        }

    //[helpstring("20 method GetSimpleArrayStructArray")] HRESULT GetSimpleArrayStructArray([out] unsigned short* unDataSize,
    //    [out, size_is(,*unDataSize)] SimpleArrayStruct** pp);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void GetSimpleArrayStructArray(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void GetSimpleArrayStructArray(string[] args) {

          JICallBuilder callObject = new JICallBuilder(true);
          callObject.Opnum = 20; //obtained from the IDL or TypeLib. //
          object[] results;

          JIStruct simpleStruct = new JIStruct();
          simpleStruct.AddMember(typeof(int?));
          simpleStruct.AddMember(typeof(double?));
          simpleStruct.AddMember(typeof(float?));

          JIStruct simpleArrayStruct = new JIStruct();
          simpleArrayStruct.AddMember(typeof(int?));
          simpleArrayStruct.AddMember(typeof(double?));
          simpleArrayStruct.AddMember(typeof(JIUnsignedShort));
          JIArray structArray = new JIArray(simpleStruct, null, 1, true);
          simpleArrayStruct.AddMember(new JIPointer(structArray)); //try no pointer next

          JIArray DataArray = new JIArray(simpleArrayStruct, null, 1, true);
          callObject.AddOutParamAsType(typeof(JIUnsignedShort), JIFlags.FLAG_NULL);
          callObject.AddOutParamAsObject(new JIPointer(DataArray), JIFlags.FLAG_NULL);

          results = ComObject.Call(callObject);
          Console.WriteLine(((JIUnsignedShort)results[0]).Value);

        }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void SetSimpleArrayStructArray(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void SetSimpleArrayStructArray(string[] args) {

            JICallBuilder callObject = new JICallBuilder(true);
            callObject.Opnum = 21; //obtained from the IDL or TypeLib. ModifyStaticData
            object[] results;

            JIStruct simpleStruct = new JIStruct();
            simpleStruct.AddMember(new int?(5));
            simpleStruct.AddMember(new double?(25));
            simpleStruct.AddMember(new float?(2.5));

            int? shortValue = new int?(1);
            JIStruct simpleArrayStruct = new JIStruct();
            simpleArrayStruct.AddMember(new int?(54));
            simpleArrayStruct.AddMember(new double?(5));
            simpleArrayStruct.AddMember(JIUnsignedFactory.GetUnsigned(shortValue, JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT));
            JIStruct[] structArray = new JIStruct[1];
            structArray[0] = simpleStruct;
            simpleArrayStruct.AddMember(new JIPointer(new JIArray(structArray, true)));
            JIStruct[] DataArray = new JIStruct[1];
            DataArray[0] = simpleArrayStruct;
            short size = 1;
            callObject.AddInParamAsShort(size, JIFlags.FLAG_NULL);
            callObject.AddInParamAsArray(new JIArray(DataArray, true), JIFlags.FLAG_NULL);

            results = ComObject.Call(callObject);
            Console.WriteLine("SetSimpleArrayStructArray worked!");
        }



      // Index out of bound exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void GetStaticStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
      public virtual void GetStaticStruct(string[] args) {

           JICallBuilder callObject = new JICallBuilder(true);
          callObject.Opnum = 15; //obtained from the IDL or TypeLib. //
          object[] results;

          JIStruct varStruct = new JIStruct();
          varStruct.AddMember(typeof(JIUnsignedInteger));
          varStruct.AddMember(typeof(float?));
          varStruct.AddMember(typeof(float?));
          varStruct.AddMember(typeof(JIUnsignedShort));
          varStruct.AddMember(typeof(float?));
          varStruct.AddMember(typeof(DateTime?));
          varStruct.AddMember(typeof(JIUnsignedInteger));

          JIStruct pointStruct = new JIStruct();
          pointStruct.AddMember(typeof(JIUnsignedInteger));
          pointStruct.AddMember(typeof(JIUnsignedInteger));
          pointStruct.AddMember(typeof(sbyte?));
          JIArray structArray = new JIArray(varStruct, null, 1, true);
          pointStruct.AddMember(new JIPointer(structArray));


          JIArray DataArray = new JIArray(pointStruct, null, 1, true);
          callObject.AddOutParamAsType(typeof(JIUnsignedShort), JIFlags.FLAG_NULL);
          callObject.AddOutParamAsObject(new JIPointer(DataArray, false), JIFlags.FLAG_NULL);


          results = ComObject.Call(callObject);
          Console.WriteLine(((JIUnsignedShort)results[0]).Value);

      }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void SetStaticStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
        public virtual void SetStaticStruct(string[] args) {

            JICallBuilder callObject = new JICallBuilder(true);
            callObject.Opnum = 16; //obtained from the IDL or TypeLib.
            object[] results;

            JIUnsignedShort j;
            long? value = new long?(10);
            int? shortValue = new int?(5);
            JIStruct varStruct = new JIStruct();
            varStruct.AddMember(JIUnsignedFactory.GetUnsigned(value, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT));
            varStruct.AddMember(new float?(1.1));
            varStruct.AddMember(new float?(1.2));
            varStruct.AddMember(JIUnsignedFactory.GetUnsigned(shortValue, JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT));
            varStruct.AddMember(new float?(1.0));
            varStruct.AddMember(DateTime.Now);
            varStruct.AddMember(JIUnsignedFactory.GetUnsigned(value, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT));

            JIStruct pointStruct = new JIStruct();
            pointStruct.AddMember(JIUnsignedFactory.GetUnsigned(new long?(15), JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT));
            pointStruct.AddMember(JIUnsignedFactory.GetUnsigned(new long?(10), JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT));
            pointStruct.AddMember(new sbyte?((sbyte)1));
            JIStruct[] varStructArray = new JIStruct[1];
            varStructArray[0] = varStruct;
            pointStruct.AddMember(new JIPointer(new JIArray(varStructArray, true))); //since this is an embedded pointer

            JIStruct[] pointAry = new JIStruct[1];
            pointAry[0] = pointStruct;

            JIArray ary = new JIArray(pointAry,true);
            callObject.AddInParamAsShort((short)1, JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT);
            callObject.AddInParamAsArray(ary, JIFlags.FLAG_NULL);

            results = ComObject.Call(callObject);
            Console.WriteLine("SetStaticStruct worked!");
        }


      public static void Main(string[] args) {

        try {
          if (args.Length < 4) {
            Console.WriteLine("Please provide address domain username password");
            return;
          }
          JISystem.InBuiltLogHandler = false;
          JISystem.AutoRegisteration = true;
          SampleTestServer test = new SampleTestServer(args[0], args);

          test.PerformCallback(args);
          test.PerformSquare(args);
          test.SetTCharArray();
          test.TCharArray;
          test.SetConformantIntArray();
          test.ConformantIntArray;
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