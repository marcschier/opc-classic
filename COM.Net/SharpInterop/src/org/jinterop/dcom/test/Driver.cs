namespace org.jinterop.dcom.test {


    using Encdec = SharpCifs.util.Encdec;

    using JISystem = common.JISystem;
    using IJIUnsigned = core.IJIUnsigned;
    using JIArray = core.JIArray;
    using JIFlags = core.JIFlags;
    using JIUnsignedFactory = core.JIUnsignedFactory;
    using JIVariant = core.JIVariant;
    using IJIDispatch = impls.automation.IJIDispatch;

    using UUID = rpc.core.UUID;

    //import com.iwombat.foundation.ObjectId;

    internal class Driver : iota
	{

		/// <param name="args"> </param>
		public static void Main(string[] args)
		{
			  try
			  {

				  string n = "variant[index]sss".replaceFirst("index", Convert.ToString(100));
				  var xxxs = new short?((short)1);
				  Type ccccccc = typeof(JIVariant[][]);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  Console.WriteLine(ccccccc.FullName);
				  ccccccc = typeof(int?[]);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  Console.WriteLine(ccccccc.FullName);
				  ccccccc = typeof(short?[][]);
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  Console.WriteLine(ccccccc.FullName);
					 if (ccccccc.IsArray)
					 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 string name2 = ccccccc.FullName;
						 var i = name2.LastIndexOf("L", StringComparison.Ordinal);
						 Console.WriteLine(name2.Substring(i + 1, name2.Length - 1 - (i + 1)));
						// System.out.println(ccccccc.getSimpleName());
					 }

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: object oi = new float[10][10];
				  object oi = RectangularArrays.ReturnRectangularFloatArray(10, 10);
				  Console.WriteLine(oi.GetType().GetElementType().Name);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: oi = new int[10][10];
				  oi = RectangularArrays.ReturnRectangularIntArray(10, 10);
				  Console.WriteLine(oi.GetType().GetElementType().Name);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: oi = new double[10][10];
				  oi = RectangularArrays.ReturnRectangularDoubleArray(10, 10);
				  Console.WriteLine(oi.GetType().GetElementType().Name);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: oi = new char[10][10];
				  oi = RectangularArrays.ReturnRectangularCharArray(10, 10);
				  Console.WriteLine(oi.GetType().GetElementType().Name);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: oi = new bool[10][10];
				  oi = RectangularArrays.ReturnRectangularBoolArray(10, 10);
				  Console.WriteLine(oi.GetType().GetElementType().Name);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: oi = new sbyte[10][10];
				  oi = RectangularArrays.ReturnRectangularSbyteArray(10, 10);
				  Console.WriteLine(oi.GetType().GetElementType().Name);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: oi = new short[10][10];
				  oi = RectangularArrays.ReturnRectangularShortArray(10, 10);
				  Console.WriteLine(oi.GetType().GetElementType().Name);
				  oi = new long[][]
				  {
					  new long[] {1,2},
					  new long[] {3,4,5,6,7},
					  new long[] {8,9,10}
				  };
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				  Console.WriteLine(oi.GetType().FullName + " , " + oi.GetType().GetElementType().Name);

				  var isPrimitive = false;
				  Type d = oi.GetType().GetElementType();
				  while (d != null)
				  {
					  Type dd = d.GetElementType();
					  if (dd == null)
					  {
						  isPrimitive = d.IsPrimitive;
					  }
					  d = dd;
				  }
				  //extract the class name
				  string clazzName = null;
                if (isPrimitive) {
                    //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                    clazzName = oi.GetType().FullName;
                    if (clazzName.EndsWith("F", StringComparison.Ordinal)) {
                        //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                        clazzName = typeof(float?).FullName;
                    }
                    else {
                        if (clazzName.EndsWith("I", StringComparison.Ordinal)) {
                            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                            clazzName = typeof(int?).FullName;
                        }
                        else {
                            if (clazzName.EndsWith("D", StringComparison.Ordinal)) {
                                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                                clazzName = typeof(double?).FullName;
                            }
                            else {
                                if (clazzName.EndsWith("C", StringComparison.Ordinal)) {
                                    //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                                    clazzName = typeof(char?).FullName;
                                }
                                else {
                                    if (clazzName.EndsWith("Z", StringComparison.Ordinal)) {
                                        //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                                        clazzName = typeof(bool?).FullName;
                                    }
                                    else {
                                        if (clazzName.EndsWith("B", StringComparison.Ordinal)) {
                                            //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                                            clazzName = typeof(sbyte?).FullName;
                                        }
                                        else {
                                            if (clazzName.EndsWith("S", StringComparison.Ordinal)) {
                                                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                                                clazzName = typeof(short?).FullName;
                                            }
                                            else {
                                                if (clazzName.EndsWith("J", StringComparison.Ordinal)) {
                                                    //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
                                                    clazzName = typeof(long?).FullName;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var arrrry = new JIArray(new long?[][]
				  {
					  new long?[] { 1, new long?(2)},
					  new long?[] { 3, 4, 5},
					  new long?[] { 3, 4, 5 }
				  });
				  var upperBounds2 = new ArrayList();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					var name = oi.GetType().FullName;
					var subArray = oi;
					var dimension = 0;
					while (name.StartsWith("[", StringComparison.Ordinal))
					{
						name = name.Substring(1);
						int x = Array.getLength(subArray);
						upperBounds2.Add(x);
						if (x == 0) //In which ever index the length is 0 , the array stops there, example Byte[0],Byte[0][10],Byte[10][0]
						{
							break;
						}
						subArray = Array.get(subArray,0);
						dimension++;
					}

					var upperBounds = new int[upperBounds2.Count];
					for (var i = 0;i < upperBounds2.Count; i++)
					{
						upperBounds[i] = (int)(int?)upperBounds2[i];
					}

	//			 Object newArray = createArray(oi, Class.forName(clazzName), dimension);
					var newArray = createArray(new long?[][]
					{
						new long?[] { 1, new long?(2)},
						new long?[] { 3, 4, 5},
						new long?[] { 3, 4, 5 }
					},
					typeof(long), dimension);

//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: int[][] g1 = new int[10][10];
				  var g1 = RectangularArrays.ReturnRectangularIntArray(10, 10);
				  var g = new int?[10][10];

				 ((System.Array)g).SetValue(Array.get(g1, 0), 0);


				 Type cx = ccccccc.GetElementType();
				  object rrr = 0;
				 // System.out.println(rrr.getClass().getSimpleName());
				  Console.WriteLine(rrr.GetType());
				  //Object hhhhh = Integer.class.cast(rrr);
				  var un = JIUnsignedFactory.getUnsigned(100, JIFlags.FLAG_REPRESENTATION_UNSIGNED_INT);
				  Console.WriteLine(un.GetType());
				  Type xx = typeof(void);
				  var y = new int?(123);
				  Type xxxx = y.GetType();
				  Console.WriteLine(xxxx.IsPrimitive);
				  Constructor cc_Renamed = typeof(JIVariant).GetConstructor(new Type[]{typeof(int), typeof(bool)});
				  var vv = (JIVariant)cc_Renamed.newInstance(new object[]{y, Convert.ToBoolean(true)});
				  object o007 = 1;
                cc(o007);
				  if (typeof(int) == typeof(int?))
				  {
					  Console.WriteLine("same");
				  }
				  object o123 = new int[][]
				  {
					  new int[] {1,2},
					  new int[] {2,3}
				  };
				  var o1234 = o123;
				  Type c = o1234.GetType();
				  object o234 = Array.get(o123,0);
				   c = o234.GetType();
				  Console.WriteLine(o123.GetType());

				  var str = "123.9".Split("\\.", true);
				  var a = ~int.MinValue;
				  Console.WriteLine(a);
				  a = a + 1;
				  Console.WriteLine(a);
				  if (a == int.MaxValue)
				  {

				  }
				  object o = Array.CreateInstance(typeof(IJIDispatch),10);

				  iota[] s = new Driver[100];
				  Console.WriteLine(s.GetType().GetElementType());
				  JISystem.ErrorMessages;
				  var bgh = new sbyte?[10][0];
				  var jj = 2147483670L;
				  var b = unchecked((short)40000);
				  var b1123 = (sbyte)b;
				  var msd = null + "";
				  var uid = new UID();
				  Console.WriteLine(uid);
	//			  System.out.println(new ObjectId().toHexString());
	//			  String str = new ObjectId().toHexString();
	//			  System.out.println(str);
	//			  str = str.substring(0,8) + "-" + str.substring(8,12) + "-" + str.substring(12,16) + "-" + str.substring(16,20) + "-" + str.substring(20)   ;
	//			  System.out.println(str);
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: object[][] obj = new object[6][2];
				  var obj = RectangularArrays.ReturnRectangularObjectArray(6, 2);
				  Console.WriteLine(obj.Length);
				  var lowbyte = -12345678;
				  //lowbyte = (int)(lowbyte - lowbyte%10000);
				  var lowbyte1 = lowbyte % 10000.0;
				  var uuid = new UUID();
				  Console.WriteLine(uuid.ToString());
				  var toSend = unchecked((int)0xFFFFFFFF);
				  Console.WriteLine(toSend.ToString("x"));
				  double gss = 100020 % 10000;
				  var toSend2 = toSend.ToString("x");
				  sbyte[] hibuffer = {0,0,0,0,0,0,0,0};
				  sbyte[] lowbuffer = {0,0,0,0,0,0,0,0};
				  var lo = "";
				  if (toSend2.Length > 8)
				  {
					  Array.Copy(toSend2.Substring(8).GetBytes(),0,lowbuffer,0,8);
					  Array.Copy(toSend2.Substring(0,8).GetBytes(),0,hibuffer,0,8);
				  }
				  else
				  {
					  Array.Copy(toSend2.GetBytes(),0,lowbuffer,0,8);
				  }

	//			  double d = -125;
	//			  Double.
	//			  d = 0xf;
	//			  long va = d & 0xF;

				  var buffer = new sbyte[1148]; //1144
					System.IO.FileStream inputStream;
					try
					{
						inputStream = new System.IO.FileStream("c:/temp/webbrowserevent3", System.IO.FileMode.Open, System.IO.FileAccess.Read); //change the 32nd member to 106 byte value , in inspect and change
						inputStream.Read(buffer,0,1148); //1144

	//					FileOutputStream outputStream = new FileOutputStream("c:/temp/webbrowserevent3");
	//					outputStream.write(buffer,0,544);
	//					outputStream.write(buffer,548,1148 - 548);
	//					outputStream.flush();
	//					outputStream.close();
					}
					catch (Exception e)
					{
						// TODO Auto-generated catch block
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}



	//			  	Integer iarray[][] = new Integer[5][6];
	//			  	System.out.println(((Object[])iarray).length);
	//			  	Object r = Array.get(iarray,0);
	//			  	Short s = new Short((short)Float.NaN);
	//			  	Float[] flt = new Float[]{new Float(10)};
	//			  	Class c = flt.getClass();
	//			  	if (c.equals(Float[].class))
	//			  		System.out.println(true);
	//
	//
	//			  	Float f[][][] = new Float[][][]{new Float[][]{{new Float(1),new Float(2)},{new Float(3),new Float(4)},{new Float(13),new Float(14)},{new Float(113),new Float(114)}},new Float[][]{{new Float(1),new Float(2)},{new Float(1234),new Float(123)},{new Float(999),new Float(555)},{new Float(345),new Float(123)}}};
	//				computeLengthArray(f);
					  int?[] yx = { 19, new int?(20), 22, new int?(23)};
					computeLengthArray(yx);
					double?[][] yx2 = {new double?[]{}, new double?[]{ 123.3, new double?(123.4), 123.5 }, new double?[]{}};
					computeLengthArray(yx2);
	//				serializeArray(yx);
	//
					  var array = deSerializeArray(typeof(float?),new int[]{10,20},2);
	//			  	//Object o3 = Array.get(f,0);
	//
	//			  	//Object[] o = f[0];
	//			  	//System.out.println(o[0]);
	//			  	c = new Float[10][10].getClass();
	//			  	System.out.println(c.getCanonicalName());
	//			  	System.out.println(c);
	//			  	System.out.println("Starting...");
	//			  	byte b1[] = new byte[]{(byte)0,(byte)0,(byte)0,(byte)0,(byte)0,(byte)0,(byte)-1,(byte)128};
	//			  	double val = Encdec.dec_doublele(b1, 0);
	//			    byte[] b = new byte[100];
	//			    int i = 0;
	//			    while (i < 100)b[i++] = -1;
	//			  	Encdec.enc_doublele(Float.NaN,b,0);
					//Encdec.enc_uint32le(268435456,b,0);
	//			  InetAddress address = InetAddress.getLocalHost();
	//			  byte[] array = address.getAddress();
					  var b1 = new sbyte[10]; //{(byte)0x9c,(byte)0x3f,(byte)0x16,(byte)0};
					  int val = Encdec.dec_uint32le(b1,0);
					  Encdec.enc_doublele(10.0,b1,0);
	//				FirstContact_Stub test = (FirstContact_Stub)
	//				  StubFactory.newInstance().createStub(
	//						  FirstContact.class);
	//				test.setAddress("ncacn_ip_tcp:10.74.2.90[135]");
					  string strKey = Convert.ToString(12345678);
					  char[] buffer1 = {};
					  Array.Copy(strKey.ToCharArray(),0,buffer1,buffer1.Length - strKey.Length,strKey.Length);
					  strKey = Convert.ToString(buffer1);
					  //FirstContact_Stub test = new FirstContact_Stub("ncacn_ip_tcp:10.74.85.56[135]");
					  //FirstContact_Stub test = new FirstContact_Stub("ncacn_ip_tcp:10.24.10.14[135]");
					  //FirstContact_Stub test = new FirstContact_Stub("ncacn_ip_tcp:127.0.0.1[135]");
					  //FirstContact_Stub test = new FirstContact_Stub("itl-hw-38602a");
					  //FirstContact_Stub test = new FirstContact_Stub("20.0.0.1");
					  JISystem.InBuiltLogHandler = false;
					  var test = new FirstContact_Stub("estroopchandxp");
					  //FirstContact_Stub test = new FirstContact_Stub("ncacn_ip_tcp:10.74.2.87[135]");
					//test.setAddress("ncacn_ip_tcp:127.0.0.1[135]");
					//test.setObject("4d9f4ab8-7d1c-11cf-861e-0020af6e7c57");
					//test.setObject(UUID.NIL_UUID);
					test.obtainReference();
			  }
				catch (Exception e)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
		}

		internal static object createArray(object srcArray, Type targetArrayType, int dimension)
		{
			object array = null;
			var c = targetArrayType;
			int len = Array.getLength(srcArray);
			for (var j = 0; j < dimension; j++)
			{
				array = Array.CreateInstance(c, len);
				c = array.GetType();
			}

			for (var i = 0; i < len ; i++)
			{
				if (dimension == 1)
				{
					//fill value here
					if (i == Array.getLength(srcArray))
					{
						//this means this array has less data than its upper bounds which is the max value.
						//resize it.
						object array2 = Array.CreateInstance(targetArrayType, i);
						Array.Copy(array, 0, array2, 0, i);
						array = array2;
						break;
					}
					((System.Array)array).SetValue(Array.get(srcArray, i), i);
				}
				else
				{
					((System.Array)array).SetValue(createArray(Array.get(srcArray, i),targetArrayType,dimension - 1), i);
				}

			}

			return array;
		}

		internal static void serializeArray(object array)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			var name = array.GetType().FullName;
			var o = (object[])array;
			for (var i = 0;i < o.Length; i++)
			{
				if (name[1] != '[')
				{
					var o1 = (object[])array;
					for (var j = 0;j < o1.Length; j++)
					{
						Console.WriteLine(o1[j]);
					}
					return;
				}
				serializeArray(Array.get(array,i));
			}

		}

		internal static int computeLengthArray(object array)
		{
			var length = 0;
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			var name = array.GetType().FullName;
			var o = (object[])array;
			for (var i = 0;i < o.Length; i++)
			{
				if (name[1] != '[')
				{
					var o1 = (object[])array;
					Console.WriteLine(o1.GetType().GetElementType());
					return length;
				}
				length = length + computeLengthArray(Array.get(array,i));
			}

			return length;
		}

		internal static object deSerializeArray(Type arrayType, int[] upperBounds, int dimension)
		{
			object array = null;
			var c = arrayType;
			for (var j = 0; j < dimension; j++)
			{
				array = Array.CreateInstance(c, upperBounds[upperBounds.Length - j - 1]);
				c = array.GetType();
			}

			for (var i = 0; i < upperBounds[upperBounds.Length - dimension] ; i++)
			{
				if (dimension == 1)
				{
					//fill value here
					((System.Array)array).SetValue(i, i);
				}
				else
				{
					((System.Array)array).SetValue(deSerializeArray(arrayType,upperBounds,dimension - 1), i);
				}

			}

			return array;
		}

		public static void cc(int? i)
		{
			Console.WriteLine(i + " , " + i.GetType());
		}

		public static void cc(object i)
		{
			Console.WriteLine(i + " () , " + i.GetType());
		}

	public virtual void v()
	{
	}



	}

	internal interface iota
	{
		void v();
	}

}