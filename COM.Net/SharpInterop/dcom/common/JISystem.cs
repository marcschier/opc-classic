//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//


namespace org.jinterop.dcom.common {

    /// <summary>
    /// Class implemented for defining system wide changes.
    /// <para><b>Note</b>: Methods starting with <i>internal_</i> keyword are internal to the framework
    /// and must not be called by the developer.
    /// </para>
    /// </summary>
    public sealed class JISystem
	{


		private JISystem()
		{
		}

		private static string pathToDB;
		private static Locale locale = Locale.Default;
		private static ResourceBundle resourceBundle;
		private static SharpCifs.Util.Sharpen.Properties mapOfProgIdsVsClsids = new SharpCifs.Util.Sharpen.Properties();
		private static ArrayList socketQueue = new ArrayList();
		private static JIComVersion comVersion = new JIComVersion();
        private static readonly IDictionary mapOfHostnamesVsIPs = new Hashtable();

        /// <summary>
        /// Returns the framework logger identified by the name "org.jinterop".
        ///
        /// @return
        /// </summary>
        public static Logger Logger { get; } = Logger.getLogger("org.jinterop");

        /// <summary>
        /// Sets the COM version which the library would use for communicating with COM servers.
        /// Default is 5.2.
        /// </summary>
        public static JIComVersion COMVersion {
            set => comVersion = value;
            get => comVersion;
        }


        /// <summary>
        /// Sets the locale, this locale will be used to retrieve the resource bundle for Error Messages.
        /// </summary>
        /// <param name="locale"> default is <code>Locale.getDefault()</code>. </param>
        public static Locale Locale {
            set => locale = value;
            get => locale;
        }


        /// <summary>
        /// Returns the ResourceBundle associated with current locale.
        ///
        /// @return
        /// </summary>
        public static ResourceBundle ErrorMessages
		{
			get
			{
				if (resourceBundle == null)
				{
					lock (typeof(JISystem))
					{
						try
						{
							if (resourceBundle == null)
							{
								resourceBundle = ResourceBundle.getBundle("org.jinterop.dcom.jierrormessages", locale);
							}
						}
						catch (MissingResourceException)
						{
							//now use the parent US english bundle , which you already have
							resourceBundle = ResourceBundle.getBundle("org.jinterop.dcom.jierrormessages");
						}
					}
				}

				return resourceBundle;
			}
		}

		/// <summary>
		/// Returns the localized error messages for the error code.
		/// </summary>
		/// <param name="code"> error code
		/// </param>
		public static string getLocalizedMessage(int code)
		{
			var strKey = code.ToString("x").ToUpper();
			  char[] buffer = {};
			  Array.Copy(strKey.ToCharArray(),0,buffer,buffer.Length - strKey.Length,strKey.Length);
			return getLocalizedMessage(Convert.ToString(buffer));
		}

		private static string getLocalizedMessage(string key)
		{
			string message = null;
			try
			{
				message = ErrorMessages.getString(key);
				message = message + " [" + key + "]";
			}
			catch (MissingResourceException)
			{
				message = "Message not found for errorCode: " + key;
			}

			return message;
		}

		/// <summary>
		/// Queries the property file maintaining the <code>PROGID</code> Vs <code>CLSID</code> mappings
		/// and returns the <code>CLSID</code> if found or null otherwise.
		/// </summary>
		/// <param name="progId"> user friendly string such as "Excel.Application".
		/// </param>
		public static string getClsidFromProgId(string progId)
		{
			if (progId == null)
			{
				return null;
			}

			if (pathToDB == null)
			{
				lock (typeof(JISystem))
				{
					if (pathToDB == null)
					{
						saveDBPathAndLoadFile();
					}
				}
			}

			return (string)mapOfProgIdsVsClsids.get(progId);
		}

		private static void saveDBPathAndLoadFile()
		{
			ClassLoader loader = Thread.CurrentThread.ContextClassLoader;
			if (loader == null)
			{
				loader = typeof(JISystem).ClassLoader; // fallback
			}

			var locations = new HashSet();
			   if (loader != null)
			   {
					try
					{
						System.Collections.IEnumerator resources = loader.getResources("progIdVsClsidDB.properties");
						while (resources.hasMoreElements())
						{
							locations.Add(resources.nextElement());
							break;
						}
					}
					catch (IOException)
					{
					}
			   }
				try
				{
					if (locations.Count == 0)
					{
						System.Collections.IEnumerator resources = ClassLoader.getSystemResources("progIdVsClsidDB.properties");
						while (resources.hasMoreElements())
						{
							locations.Add(resources.nextElement());
							break;
						}
					}
				}
				catch (IOException)
				{
				}

				IEnumerator iterator = locations.GetEnumerator();
				while (iterator.hasNext())
				{
					try
					{
							var url = (URL) iterator.next();
							pathToDB = url.Path;

							try
							{

								if (!pathToDB.StartsWith("file:", StringComparison.Ordinal))
								{
								  url = new URL("file:" + pathToDB);
								}

								if (Logger.isLoggable(Level.INFO))
								{
									Logger.info("progIdVsClsidDB file located at: " + url);
								}

								URLConnection con = url.openConnection();
								System.IO.Stream inputStream = con.InputStream;
								mapOfProgIdsVsClsids.load(inputStream);
								inputStream.Close();
								//outputStream = con.getOutputStream();
							}
							catch (Exception)
							{
							}

							//mapOfProgIdsVsClsids.load(new FileInputStream(pathToDB));
					}
					catch (Exception)
					{
						//ex.printStackTrace();
					}
				}

				if (Logger.isLoggable(Level.INFO))
				{
					Logger.info("progIdVsClsidDB: " + mapOfProgIdsVsClsids);
				}
		}

		//should be called from system shut down only
		/// <summary>
		/// Should be called from system shut down only
		///
		/// @exclude
		/// </summary>
		public static void internal_writeProgIdsToFile()
		{
			if (pathToDB != null)
			{
				try
				{
					var outputStream = new System.IO.FileStream(pathToDB, System.IO.FileMode.Create, System.IO.FileAccess.Write);
					mapOfProgIdsVsClsids.store(outputStream,"progId Vs ClsidDB");
					outputStream.Close();
				}
				catch (FileNotFoundException e)
				{

					Logger.throwing("JISystem", "writeProgIdsToFile", e);
				}
				catch (IOException e)
				{

					Logger.throwing("JISystem", "writeProgIdsToFile", e);
				}
			}
		}



		//stores it in a temporary hash map here, and this is later persisted when the library is shutdown
		/// <summary>
		///Stores it in a temporary hash map here, and this is later persisted when the library is shutdown
		/// @exclude
		/// </summary>
		public static void internal_setClsidtoProgId(string progId, string clsid)
		{
			mapOfProgIdsVsClsids.put(progId,clsid);
		}

		/// <summary>
		/// synchronisation will be performed by the oxid master
		/// @exclude
		/// @return
		/// </summary>
		public static object internal_getSocket()
		{
			{
			//synchronized (socketQueue)
				return socketQueue.Remove(0);
			}
		}

		/// <summary>
		///synchronisation will be performed by the oxid master
		/// @exclude
		/// </summary>
		public static void internal_setSocket(object socket)
		{
			{
			//synchronized (socketQueue)
				socketQueue.Add(socket);
			}
		}

		/// <summary>
		/// @exclude
		/// @return
		/// </summary>
		public static void internal_initLogger()
		{
			lock (typeof(JISystem))
			{
				logSystemPropertiesAndVersion();
			}
		}

		private static void logSystemPropertiesAndVersion()
		{
			var pr = System.SharpCifs.Util.Sharpen.Properties;
			IEnumerator itr = pr.Keys.GetEnumerator();
			var str = "";
			string jinteropVersion = typeof(JISystem).Assembly.ImplementationVersion;
			Logger logger = Logger.getLogger("org.jinterop");
			if (logger.isLoggable(Level.INFO))
			{
				logger.info("j-Interop Version = " + jinteropVersion + "\n");
				while (itr.hasNext())
				{
					var key = (string)itr.next();
					str = str + key + " = " + pr.getProperty(key) + "\n";
				}
				logger.info(str);
			}
		}

		/// <summary>
		///Indicates to the framework, if Windows Registry settings for DLL\OCX
		/// component identified by this object should be modified to add a <code>Surrogate</code>
		/// automatically. A <code>Surrogate</code> is a process which provides resources
		/// such as memory and cpu for a DLL\OCX to execute.
		/// <para> This API overrides the instance specific flags set on JIClsid or JIProgID.
		///
		/// </para>
		/// </summary>
		/// <param name="autoRegisteration"> <code>true</code> if auto registration should be done by the framework. </param>
		public static bool AutoRegisteration {
            set => AutoRegistrationSet = value;
        }

        /// <summary>
        ///Returns true is auto registration is enabled.
        ///
        /// @return
        /// </summary>
        public static bool AutoRegistrationSet { get; private set; } = false;

        /// <summary>
        ///<para>Sometimes the DCOM runtime of Windows will not send a ping on time to the Framework.
        /// It is not very abnormal, since Windows can sometimes resort to mechanisms other than
        /// DCOM to keep a reference count for the instances they imported. In case of j-Interop
        /// framework, if a ping is not received in 8 minutes , the Java Local Class is collected for
        /// GC. And if the COM server requires a reference to it or acts on a previously obtained reference
        /// , it is sent back an <i>Exception</i>. Please use this flag to set the Auto Collection status
        /// to ON or OFF. By Default, it is ON. </para>
        /// </summary>
        /// <param name="autoCollection"> <code>false</code> if auto collection should be turned off. </param>
        public static bool JavaCoClassAutoCollection {
            set => JavaCoClassAutoCollectionSet = value;
        }

        /// <summary>
        /// Status of autoCollection flag.
        /// </summary>
        /// <returns> <code>true</code> if autoCollection is enabled, <code>false</code> otherwise. </returns>
        public static bool JavaCoClassAutoCollectionSet { get; private set; } = true;

        /// <summary>
        /// Used to set the in built log handler.
        /// </summary>
        /// <param name="useParentHandlers"> true if parent handlers should be used. </param>
        /// <exception cref="IOException"> </exception>
        /// <exception cref="SecurityException">  </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static void setInBuiltLogHandler(bool useParentHandlers) throws SecurityException, java.io.IOException
        public static bool InBuiltLogHandler
		{
			set
			{
				Logger.UseParentHandlers = value;
                var fileHandler = new FileHandler("%t/j-Interop%g.log", 0, 1, true) {
                    Formatter = new SimpleFormatter()
                };
                Logger.addHandler(fileHandler);
			}
		}

		/// <summary>
		/// Adds a mapping between the <code>hostname</code> and its <code>IP</code>. This method should be used when there is a possibility
		/// of multiple adapters (for example from a Virtual Machine) on the COM server. j-Interop Framework only uses
		/// the host name and ignores the I.P addresses supplied in the interface reference of a COM object. If this hostname
		/// is not reachable from the machine where library is currently running (such as a Linux machine with no name mappings)
		/// then the call to this COM server would fail with an <code>UnknownHostException</code>. To avoid that either add the
		/// binding in the host machine or add the binding here.
		/// <para>
		/// This method stores the name vs I.P binding in a <code>Map</code>. Providing the same <code>hostname</code> will overwrite
		/// the binding specified before.
		///
		/// </para>
		/// </summary>
		/// <param name="hostname"> name of target machine. </param>
		/// <param name="IP"> address of target machine in I.P format. </param>
		/// <exception cref="UnknownHostException"> if the <code>IP</code> is invalid or cannot be reached. </exception>
		/// <exception cref="System.ArgumentException"> if any parameter is <code>null</code> or of 0 length. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static synchronized void mapHostNametoIP(String hostname, String IP) throws java.net.UnknownHostException
		public static void mapHostNametoIP(string hostname, string IP)
		{
			lock (typeof(JISystem))
			{
				if (hostname == null || IP == null || hostname.Trim().Length == 0 || IP.Trim().Length == 0)
				{
					throw new System.ArgumentException();
				}

				//just check the validity of IP
				InetAddress.getByName(IP.Trim());

				mapOfHostnamesVsIPs[hostname.Trim().ToUpper()] = IP.Trim();
			}
		}

		/// <summary>
		/// Returns I.P address for the given <code>hostname</code>.
		/// </summary>
		/// <param name="hostname"> </param>
		/// <returns> <code>null</code> if a mapping could not be found. </returns>
		public static string getIPForHostName(string hostname)
		{
			lock (typeof(JISystem))
			{
				return (string)mapOfHostnamesVsIPs[hostname.Trim().ToUpper()];
			}
		}

		public static void internal_dumpMap()
		{
			lock (typeof(JISystem))
			{
				if (Logger.isLoggable(Level.INFO))
				{
					Logger.info("mapOfHostnamesVsIPs: " + mapOfHostnamesVsIPs);
				}
			}
		}
	}

}