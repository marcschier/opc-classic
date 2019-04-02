namespace org.jinterop.dcom.test {
    using JISystem = common.JISystem;
    using IJIComObject = core.IJIComObject;
    using JIArray = core.JIArray;
    using JICallBuilder = core.JICallBuilder;
    using JIComServer = core.JIComServer;
    using JIFlags = core.JIFlags;
    using JIPointer = core.JIPointer;
    using JIProgId = core.JIProgId;
    using JISession = core.JISession;
    using JIStruct = core.JIStruct;
    using JIUnsignedFactory = core.JIUnsignedFactory;
    using JIUnsignedInteger = core.JIUnsignedInteger;
    using JIUnsignedShort = core.JIUnsignedShort;
    using IJIDispatch = impls.automation.IJIDispatch;

    /// <summary>
    /// Contributed Code sample. Works in conjunction with SampleTestServers.zip
    /// 
    /// 
    /// 
    /// </summary>
    public class SampleTestServer
	{

	  private JIComServer comStub;
	  private IJIComObject comObject;
	  private readonly IJIDispatch dispatch;
	  private readonly string address;
	  private readonly JISession session;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SampleTestServer(String address, String[] args) throws org.jinterop.dcom.common.JIException, java.net.UnknownHostException
	  public SampleTestServer(string address, string[] args)
	  {
		this.address = address;
		session = JISession.createSession(args[1], args[2], args[3]);
		comStub = new JIComServer(JIProgId.valueOf("SampleTestServer.TestServer"), address, session);
		var unknown = comStub.createInstance();
		comObject = (IJIComObject) unknown.queryInterface("1F438B1C-02BA-462E-A971-8E0640C141E5"); //ITestServer
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performSquare(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
	  public virtual void performSquare(string[] args)
	  {

            var callObject = new JICallBuilder(true) {
                Opnum = 1 //obtained from the IDL or TypeLib. //    AskTestServerToSquare
            };
            object[] results;
		short i = 3;
		callObject.addInParamAsShort(i, JIFlags.FLAG_NULL);
		callObject.addOutParamAsType(typeof(short?), JIFlags.FLAG_NULL); //Short
		results = comObject.call(callObject);
		Console.WriteLine("ITestServer.AskTestServerToSquare succeeded, input=" + i + " output=" + results[0]);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void performCallback(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
	  public virtual void performCallback(string[] args)
	  {


	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getTCharArray() throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
	  public virtual void getTCharArray()
	  {
		  System.gc();
            var callObject = new JICallBuilder(true) {
                Opnum = 6
            };
            object[] results;

		  callObject.addOutParamAsObject(new JIArray(typeof(sbyte?), new int[]{50},1,false), JIFlags.FLAG_NULL);
		  results = comObject.call(callObject);

		  var arrayOfResults = (JIArray)results[0];
		  var arrayOfBytes = (sbyte?[]) arrayOfResults.ArrayInstance;
		  var length = 50;
		  for (var i = 0; i < length; i++)
		  {
			Console.WriteLine((sbyte)arrayOfBytes[i]);
		  }
	  }


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setTCharArray() throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
	  public virtual void setTCharArray()
	  {
		  System.gc();
            var callObject = new JICallBuilder(true) {
                Opnum = 7
            };
            object[] results;
		  callObject.addInParamAsString("AHHHHHHH!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", JIFlags.FLAG_REPRESENTATION_STRING_LPWSTR);

		  results = comObject.call(callObject);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setConformantIntArray() throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
	  public virtual void setConformantIntArray()
	  {
		  System.gc();
            var callObject = new JICallBuilder(true) {
                Opnum = 9
            };
            object[] results;
		  var i = 4;
		  var intAry = new int?[i];
		  for (var j = 0; j < i; j++)
		  {
			  intAry[j] = j;
		  }
		  var ary = new JIArray(intAry, true);
		  callObject.addInParamAsInt(i, JIFlags.FLAG_NULL);
		  callObject.addInParamAsArray(ary, JIFlags.FLAG_NULL);
		  results = comObject.call(callObject);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getConformantIntArray() throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
	  public virtual void getConformantIntArray()
	  {

            var callObject = new JICallBuilder(true) {
                Opnum = 8
            };
            object[] results;

		  callObject.addOutParamAsType(typeof(int?), JIFlags.FLAG_NULL);
		  callObject.addOutParamAsObject(new JIPointer(new JIArray(typeof(int?), null, 1, true)), JIFlags.FLAG_NULL);
		  results = comObject.call(callObject);

		  var arrayOfResults = (JIArray)((JIPointer)results[1]).Referent;
		  var arrayOfIntegers = (int?[]) arrayOfResults.ArrayInstance;
		  var length = (int)(int?)results[0];
		  for (var i = 0; i < length; i++)
		  {
			Console.WriteLine((int)arrayOfIntegers[i]);
		  }
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void GetStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
		public virtual void GetStruct(string[] args)
		{

            var callObject = new JICallBuilder(true) {
                Opnum = 10 //obtained from the IDL or TypeLib. //
            };
            object[] results;

			// change the struct to have the array as the last item
			var @struct = new JIStruct();
			var longArray = new JIArray(typeof(int?), new int[]{50},1,false);
			@struct.addMember(typeof(int?));
			@struct.addMember(typeof(float?));
			@struct.addMember(longArray);
			callObject.addOutParamAsObject(new JIPointer(@struct), JIFlags.FLAG_NULL);

			results = comObject.call(callObject);
			Console.WriteLine(results[0]);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getSimpleStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
		public virtual void getSimpleStruct(string[] args)
		{

            var callObject = new JICallBuilder(true) {
                Opnum = 12 //obtained from the IDL or TypeLib. //
            };
            object[] results;

			var @struct = new JIStruct();
			@struct.addMember(typeof(int?));
			@struct.addMember(typeof(double?));
			@struct.addMember(typeof(float?));
			callObject.addOutParamAsObject(new JIPointer(@struct), JIFlags.FLAG_NULL);

			results = comObject.call(callObject);
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
		public virtual void getSimpleStructArray(string[] args)
		{

            var callObject = new JICallBuilder(true) {
                Opnum = 13 //obtained from the IDL or TypeLib. //
            };
            object[] results;

			callObject.addOutParamAsType(typeof(JIUnsignedShort), JIFlags.FLAG_NULL);

			var @struct = new JIStruct();
			@struct.addMember(typeof(int?));
			@struct.addMember(typeof(double?));
			@struct.addMember(typeof(float?));
			var DataArray = new JIArray(@struct, null, 1, true);
			callObject.addOutParamAsObject(new JIPointer(DataArray), JIFlags.FLAG_NULL);
			results = comObject.call(callObject);
			Console.WriteLine(((JIUnsignedShort)results[0]).Value);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void getConformantStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
		public virtual void getConformantStruct(string[] args)
		{

            var callObject = new JICallBuilder(true) {
                Opnum = 14 //obtained from the IDL or TypeLib. //
            };
            object[] results;

			var @struct = new JIStruct();
			@struct.addMember(typeof(int?));
			@struct.addMember(typeof(double?));
			@struct.addMember(typeof(JIUnsignedShort));
			var longArray = new JIArray(typeof(int?), null, 1, true);
			@struct.addMember(new JIPointer(longArray));
			callObject.addOutParamAsObject(new JIPointer(@struct), JIFlags.FLAG_NULL);

			results = comObject.call(callObject);
			Console.WriteLine(results[0]);
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void GetStructStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
		public virtual void GetStructStruct(string[] args)
		{

            var callObject = new JICallBuilder(true) {
                Opnum = 17 //obtained from the IDL or TypeLib. //
            };
            object[] results;

		  var @struct = new JIStruct();
		  @struct.addMember(typeof(int?));
		  @struct.addMember(typeof(double?));
		  @struct.addMember(typeof(JIUnsignedShort));
		  var longArray = new JIArray(typeof(int?), null, 1, true);
		  @struct.addMember(new JIPointer(longArray));

		  var StructStruct = new JIStruct();
		  StructStruct.addMember(typeof(int?));
		  StructStruct.addMember(typeof(double?));
		  StructStruct.addMember(@struct);

		 callObject.addOutParamAsObject(new JIPointer(StructStruct), JIFlags.FLAG_NULL);

		  results = comObject.call(callObject);
		  Console.WriteLine(results[0]);

		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void GetStructStructArray(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
		public virtual void GetStructStructArray(string[] args)
		{

            var callObject = new JICallBuilder(true) {
                Opnum = 18 //obtained from the IDL or TypeLib. //
            };
            object[] results;

		  var @struct = new JIStruct();
		  @struct.addMember(typeof(int?));
		  @struct.addMember(typeof(double?));
		  @struct.addMember(typeof(JIUnsignedShort));
		  var longArray = new JIArray(typeof(int?), null, 1, true);
		  @struct.addMember(new JIPointer(longArray));

		  var StructStruct = new JIStruct();
		  StructStruct.addMember(typeof(int?));
		  StructStruct.addMember(typeof(double?));
		  StructStruct.addMember(@struct);

		  var DataArray = new JIArray(StructStruct, null, 1, true);
		  callObject.addOutParamAsType(typeof(JIUnsignedShort), JIFlags.FLAG_NULL);
		  callObject.addOutParamAsObject(new JIPointer(DataArray), JIFlags.FLAG_NULL);

		  results = comObject.call(callObject);
		  Console.WriteLine(((JIUnsignedShort)results[0]).Value);

		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void GetSimpleArrayStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
		public virtual void GetSimpleArrayStruct(string[] args)
		{

            var callObject = new JICallBuilder(true) {
                Opnum = 19 //obtained from the IDL or TypeLib. //
            };
            object[] results;

		  var simpleStruct = new JIStruct();
		  simpleStruct.addMember(typeof(int?));
		  simpleStruct.addMember(typeof(double?));
		  simpleStruct.addMember(typeof(float?));

		  var simpleArrayStruct = new JIStruct();
		  simpleArrayStruct.addMember(typeof(int?));
		  simpleArrayStruct.addMember(typeof(double?));
		  simpleArrayStruct.addMember(typeof(JIUnsignedShort));
		  var structArray = new JIArray(simpleStruct, null, 1, true);
		  simpleArrayStruct.addMember(new JIPointer(structArray));

		  callObject.addOutParamAsObject(new JIPointer(simpleArrayStruct), JIFlags.FLAG_NULL);

		  results = comObject.call(callObject);
		  Console.WriteLine(results[0]);

		}

	//[helpstring("20 method GetSimpleArrayStructArray")] HRESULT GetSimpleArrayStructArray([out] unsigned short* unDataSize,
	//    [out, size_is(,*unDataSize)] SimpleArrayStruct** pp);
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void GetSimpleArrayStructArray(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
		public virtual void GetSimpleArrayStructArray(string[] args)
		{

            var callObject = new JICallBuilder(true) {
                Opnum = 20 //obtained from the IDL or TypeLib. //
            };
            object[] results;

		  var simpleStruct = new JIStruct();
		  simpleStruct.addMember(typeof(int?));
		  simpleStruct.addMember(typeof(double?));
		  simpleStruct.addMember(typeof(float?));

		  var simpleArrayStruct = new JIStruct();
		  simpleArrayStruct.addMember(typeof(int?));
		  simpleArrayStruct.addMember(typeof(double?));
		  simpleArrayStruct.addMember(typeof(JIUnsignedShort));
		  var structArray = new JIArray(simpleStruct, null, 1, true);
		  simpleArrayStruct.addMember(new JIPointer(structArray)); //try no pointer next

		  var DataArray = new JIArray(simpleArrayStruct, null, 1, true);
		  callObject.addOutParamAsType(typeof(JIUnsignedShort), JIFlags.FLAG_NULL);
		  callObject.addOutParamAsObject(new JIPointer(DataArray), JIFlags.FLAG_NULL);

		  results = comObject.call(callObject);
		  Console.WriteLine(((JIUnsignedShort)results[0]).Value);

		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void SetSimpleArrayStructArray(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
		public virtual void SetSimpleArrayStructArray(string[] args)
		{

            var callObject = new JICallBuilder(true) {
                Opnum = 21 //obtained from the IDL or TypeLib. ModifyStaticData
            };
            object[] results;

			var simpleStruct = new JIStruct();
			simpleStruct.addMember(5);
			simpleStruct.addMember(25);
			simpleStruct.addMember(2.5);

			var shortValue = new int?(1);
			var simpleArrayStruct = new JIStruct();
			simpleArrayStruct.addMember(54);
			simpleArrayStruct.addMember(5);
			simpleArrayStruct.addMember(JIUnsignedFactory.getUnsigned(shortValue, JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT));
			var structArray = new JIStruct[1];
			structArray[0] = simpleStruct;
			simpleArrayStruct.addMember(new JIPointer(new JIArray(structArray, true)));
			var DataArray = new JIStruct[1];
			DataArray[0] = simpleArrayStruct;
			short size = 1;
			callObject.addInParamAsShort(size, JIFlags.FLAG_NULL);
			callObject.addInParamAsArray(new JIArray(DataArray, true), JIFlags.FLAG_NULL);

			results = comObject.call(callObject);
			Console.WriteLine("SetSimpleArrayStructArray worked!");
		}



	  // Index out of bound exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void GetStaticStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
	  public virtual void GetStaticStruct(string[] args)
	  {

            var callObject = new JICallBuilder(true) {
                Opnum = 15 //obtained from the IDL or TypeLib. //
            };
            object[] results;

		  var varStruct = new JIStruct();
		  varStruct.addMember(typeof(JIUnsignedInteger));
		  varStruct.addMember(typeof(float?));
		  varStruct.addMember(typeof(float?));
		  varStruct.addMember(typeof(JIUnsignedShort));
		  varStruct.addMember(typeof(float?));
		  varStruct.addMember(typeof(DateTime));
		  varStruct.addMember(typeof(JIUnsignedInteger));

		  var pointStruct = new JIStruct();
		  pointStruct.addMember(typeof(JIUnsignedInteger));
		  pointStruct.addMember(typeof(JIUnsignedInteger));
		  pointStruct.addMember(typeof(sbyte?));
		  var structArray = new JIArray(varStruct, null, 1, true);
		  pointStruct.addMember(new JIPointer(structArray));


		  var DataArray = new JIArray(pointStruct, null, 1, true);
		  callObject.addOutParamAsType(typeof(JIUnsignedShort), JIFlags.FLAG_NULL);
		  callObject.addOutParamAsObject(new JIPointer(DataArray, false), JIFlags.FLAG_NULL);


		  results = comObject.call(callObject);
		  Console.WriteLine(((JIUnsignedShort)results[0]).Value);

	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void SetStaticStruct(String[] args) throws org.jinterop.dcom.common.JIException, InterruptedException, java.net.UnknownHostException
		public virtual void SetStaticStruct(string[] args)
		{

            var callObject = new JICallBuilder(true) {
                Opnum = 16 //obtained from the IDL or TypeLib.
            };
            object[] results;

			JIUnsignedShort j;
			var value = new long?(10);
			var shortValue = new int?(5);
			var varStruct = new JIStruct();
			varStruct.addMember(JIUnsignedFactory.getUnsigned(value, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT));
			varStruct.addMember(1.1);
			varStruct.addMember(1.2);
			varStruct.addMember(JIUnsignedFactory.getUnsigned(shortValue, JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT));
			varStruct.addMember(1.0);
			varStruct.addMember(DateTime.Now);
			varStruct.addMember(JIUnsignedFactory.getUnsigned(value, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT));

			var pointStruct = new JIStruct();
			pointStruct.addMember(JIUnsignedFactory.getUnsigned(15, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT));
			pointStruct.addMember(JIUnsignedFactory.getUnsigned(10, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT));
			pointStruct.addMember((sbyte)1);
			var varStructArray = new JIStruct[1];
			varStructArray[0] = varStruct;
			pointStruct.addMember(new JIPointer(new JIArray(varStructArray, true))); //since this is an embedded pointer

			var pointAry = new JIStruct[1];
			pointAry[0] = pointStruct;

			var ary = new JIArray(pointAry,true);
			callObject.addInParamAsShort((short)1, JIFlags.FLAG_REPRESENTATION_UNSIGNED_SHORT);
			callObject.addInParamAsArray(ary, JIFlags.FLAG_NULL);

			results = comObject.call(callObject);
			Console.WriteLine("SetStaticStruct worked!");
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
		  JISystem.InBuiltLogHandler = false;
		  JISystem.AutoRegisteration = true;
		  var test = new SampleTestServer(args[0], args);

		  test.performCallback(args);
		  test.performSquare(args);
		  test.setTCharArray();
		  test.TCharArray;
		  test.setConformantIntArray();
		  test.ConformantIntArray;
		  test.GetStruct(args);
		  test.getSimpleStruct(args);
		  test.getConformantStruct(args);
		  test.getSimpleStructArray(args);
		  test.GetStructStruct(args);
		  test.GetStructStructArray(args);
		  test.GetSimpleArrayStruct(args);
	//
		  test.GetSimpleArrayStructArray(args);
		  test.SetSimpleArrayStructArray(args);
		  test.GetStaticStruct(args);
		  test.SetStaticStruct(args);
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