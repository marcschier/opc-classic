using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.common {


    /// <summary>
    ///<para>Class implemented for defining system wide changes. 
    /// 
    /// </para>
    /// <para>A note on logging: The framework exposes JRE based logger "org.jinterop". Applications need to 
    /// attach their own handler to this logger. If you would like to set the in-built handler, which 
    /// writes to a file <code>j-Interop.log</code> in the <code>java.io.tmpdir</code> directory, please use 
    /// the <seealso cref="#setInBuiltLogHandler(boolean)"/>. Please note that the <code>level</code> for the logger 
    /// and all other configuration parameters should be set directly on the logger instance, 
    /// using <code>LogManager.getLogger("org.jinterop")</code></para>
    /// 
    /// <para><b>Note</b>: Methods starting with <i>internal_</i> keyword are internal to the framework 
    /// and must not be called by the developer.
    /// 
    /// @since 1.0
    /// 
    /// </para>
    /// </summary>
    public sealed class JISystem {


        private JISystem() {
        }

        private static string PathToDB = null;
        private static Locale Locale_Renamed = Locale.Default;
        private static ResourceBundle ResourceBundle = null;
        private static Properties MapOfProgIdsVsClsids = new Properties();
        private static List<object> SocketQueue = new List<object>();
        private static JIComVersion ComVersion = new JIComVersion();
        private static bool AutoRegister = false;
        private static bool AutoCollection = true;
        private static readonly Logger Logger_Renamed = Logger.getLogger("org.jinterop");
        private static readonly IDictionary MapOfHostnamesVsIPs = new Hashtable();

        /// <summary>
        /// Returns the framework logger identified by the name "org.jinterop".
        /// 
        /// @return
        /// </summary>
        public static Logger Logger {
            get {
                return Logger_Renamed;
            }
        }

        /// <summary>
        /// Sets the COM version which the library would use for communicating with COM servers. 
        /// Default is 5.2. 
        /// </summary>
        /// <param name="comVersion"> new COM version </param>
        public static JIComVersion COMVersion {
            set {
                JISystem.ComVersion = value;
            }
            get {
                return JISystem.ComVersion;
            }
        }


        /// <summary>
        /// Sets the locale, this locale will be used to retrieve the resource bundle for Error Messages. 
        /// </summary>
        /// <param name="locale"> default is <code>Locale.getDefault()</code>. </param>
        public static Locale Locale {
            set {
                JISystem.Locale_Renamed = value;
            }
            get {
                return JISystem.Locale_Renamed;
            }
        }


        /// <summary>
        /// Returns the ResourceBundle associated with current locale.
        /// 
        /// @return
        /// </summary>
        public static ResourceBundle ErrorMessages {
            get {
                if (ResourceBundle == null) {
                    lock (typeof(JISystem)) {
                        try {
                            if (ResourceBundle == null) {
                                ResourceBundle = ResourceBundle.getBundle("org.jinterop.dcom.jierrormessages", Locale_Renamed);
                            }
                        }
                        catch (MissingResourceException) {
                            //now use the parent US english bundle , which you already have
                            ResourceBundle = ResourceBundle.getBundle("org.jinterop.dcom.jierrormessages");
                        }
                    }
                }
    
                return ResourceBundle;
            }
        }

        /// <summary>
        /// Returns the localized error messages for the error code.
        /// </summary>
        /// <param name="code"> error code 
        /// @return </param>
        public static string GetLocalizedMessage(int code) {
            string strKey = code.ToString("x").ToUpper();
              char[] buffer = {};
              Array.Copy(strKey.ToCharArray(),0,buffer,buffer.Length - strKey.Length,strKey.Length);
            return GetLocalizedMessage(Convert.ToString(buffer));
        }

        private static string GetLocalizedMessage(string key) {
            string message = null;
            try {
                message = JISystem.ErrorMessages.getString(key);
                message = message + " [" + key + "]";
            }
            catch (MissingResourceException) {
                message = "Message not found for errorCode: " + key;
            }

            return message;
        }

        /// <summary>
        /// Queries the property file maintaining the <code>PROGID</code> Vs <code>CLSID</code> mappings 
        /// and returns the <code>CLSID</code> if found or null otherwise.
        /// </summary>
        /// <param name="progId"> user friendly string such as "Excel.Application".
        /// @return </param>
        public static string GetClsidFromProgId(string progId) {
            if (progId == null) {
                return null;
            }

            if (PathToDB == null) {
                lock (typeof(JISystem)) {
                    if (PathToDB == null) {
                        SaveDBPathAndLoadFile();
                    }
                }
            }

            return ((string)MapOfProgIdsVsClsids.get(progId));
        }

        private static void SaveDBPathAndLoadFile() {
            ClassLoader loader = Thread.CurrentThread.ContextClassLoader;
            if (loader == null) {
                loader = typeof(JISystem).ClassLoader; // fallback
            }

            HashSet locations = new HashSet();
               if (loader != null) {
                    try {
                        System.Collections.IEnumerator resources = loader.getResources("progIdVsClsidDB.properties");
                        while (resources.hasMoreElements()) {
                            locations.Add(resources.nextElement());
                            break;
                        }
                    }
                    catch (IOException) {
                    }
               }
                try {
                    if (locations.Count == 0) {
                        System.Collections.IEnumerator resources = ClassLoader.getSystemResources("progIdVsClsidDB.properties");
                        while (resources.hasMoreElements()) {
                            locations.Add(resources.nextElement());
                            break;
                        }
                    }
                }
                catch (IOException) {
                }

                IEnumerator iterator = locations.GetEnumerator();
                while (iterator.hasNext()) {
                    try {
                            URL url = (URL) iterator.next();
                            PathToDB = url.Path;

                            try {

                                if (!PathToDB.StartsWith("file:", StringComparison.Ordinal)) {
                                  url = new URL("file:" + PathToDB);
                                }

                                if (Logger_Renamed.isLoggable(Level.INFO)) {
                                    Logger_Renamed.info("progIdVsClsidDB file located at: " + url);
                                }

                                URLConnection con = url.openConnection();
                                System.IO.Stream inputStream = con.InputStream;
                                MapOfProgIdsVsClsids.load(inputStream);
                                inputStream.Close();
                                //outputStream = con.getOutputStream();
                            }
                            catch (Exception) {
                            }

                            //mapOfProgIdsVsClsids.load(new FileInputStream(pathToDB));
                    }
                    catch (Exception) {
                        //ex.printStackTrace();
                    }
                }

                if (Logger_Renamed.isLoggable(Level.INFO)) {
                    Logger_Renamed.info("progIdVsClsidDB: " + MapOfProgIdsVsClsids);
                }
        }

        //should be called from system shut down only
        /// <summary>
        /// Should be called from system shut down only
        /// 
        /// @exclude
        /// </summary>
        public static void Internal_writeProgIdsToFile() {
            if (PathToDB != null) {
                try {
                    System.IO.FileStream outputStream = new System.IO.FileStream(PathToDB, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    MapOfProgIdsVsClsids.store(outputStream,"progId Vs ClsidDB");
                    outputStream.Close();
                }
                catch (FileNotFoundException e) {

                    Logger_Renamed.throwing("JISystem", "writeProgIdsToFile", e);
                }
                catch (IOException e) {

                    Logger_Renamed.throwing("JISystem", "writeProgIdsToFile", e);
                }
            }
        }



        //stores it in a temporary hash map here, and this is later persisted when the library is shutdown
        /// <summary>
        ///Stores it in a temporary hash map here, and this is later persisted when the library is shutdown
        /// @exclude
        /// </summary>
        public static void Internal_setClsidtoProgId(string progId, string clsid) {
            MapOfProgIdsVsClsids.put(progId,clsid);
        }

        /// <summary>
        /// synchronisation will be performed by the oxid master
        /// @exclude
        /// @return
        /// </summary>
        public static object Internal_getSocket() {
        {
            //synchronized (socketQueue) 
                return SocketQueue.Remove(0);
        }
        }

        /// <summary>
        ///synchronisation will be performed by the oxid master
        /// @exclude
        /// </summary>
        public static void Internal_setSocket(object socket) {
        {
            //synchronized (socketQueue) 
                SocketQueue.Add(socket);
        }
        }

        /// <summary>
        /// @exclude
        /// @return
        /// </summary>
        public static void Internal_initLogger() {
            lock (typeof(JISystem)) {
                LogSystemPropertiesAndVersion();
            }
        }

        private static void LogSystemPropertiesAndVersion() {
            Properties pr = System.Properties;
            IEnumerator itr = pr.Keys.GetEnumerator();
            string str = "";
            string jinteropVersion = typeof(JISystem).Assembly.ImplementationVersion;
            Logger logger = Logger.getLogger("org.jinterop");
            if (logger.isLoggable(Level.INFO)) {
                logger.info("j-Interop Version = " + jinteropVersion + "\n");
                while (itr.hasNext()) {
                    string key = (string)itr.next();
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
            set {
                AutoRegister = value;
            }
        }

        /// <summary>
        ///Returns true is auto registration is enabled.
        /// 
        /// @return
        /// </summary>
        public static bool AutoRegistrationSet {
            get {
                return AutoRegister;
            }
        }

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
            set {
                JISystem.AutoCollection = value;
            }
        }

        /// <summary>
        /// Status of autoCollection flag.   
        /// </summary>
        /// <returns> <code>true</code> if autoCollection is enabled, <code>false</code> otherwise. </returns>
        public static bool JavaCoClassAutoCollectionSet {
            get {
                return AutoCollection;
            }
        }

        /// <summary>
        /// Used to set the in built log handler. 
        /// </summary>
        /// <param name="useParentHandlers"> true if parent handlers should be used. </param>
        /// <exception cref="IOException"> </exception>
        /// <exception cref="SecurityException">  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void setInBuiltLogHandler(boolean useParentHandlers) throws SecurityException, java.io.IOException
        public static bool InBuiltLogHandler {
            set {
                Logger_Renamed.UseParentHandlers = value;
                FileHandler fileHandler = new FileHandler("%t/j-Interop%g.log",0,1,true);
                fileHandler.Formatter = new SimpleFormatter();
                Logger_Renamed.addHandler(fileHandler);
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
        /// <exception cref="IllegalArgumentException"> if any parameter is <code>null</code> or of 0 length. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static synchronized void mapHostNametoIP(String hostname, String IP) throws java.net.UnknownHostException
        public static void MapHostNametoIP(string hostname, string IP) {
            lock (typeof(JISystem)) {
                if (hostname == null || IP == null || hostname.Trim().Length == 0 || IP.Trim().Length == 0) {
                    throw new System.ArgumentException();
                }
        
                //just check the validity of IP
                InetAddress.getByName(IP.Trim());
        
                MapOfHostnamesVsIPs[hostname.Trim().ToUpper()] = IP.Trim();
            }
        }

        /// <summary>
        /// Returns I.P address for the given <code>hostname</code>.
        /// </summary>
        /// <param name="hostname"> </param>
        /// <returns> <code>null</code> if a mapping could not be found. </returns>
        public static string GetIPForHostName(string hostname) {
            lock (typeof(JISystem)) {
                return (string)MapOfHostnamesVsIPs.GetValueOrNull(hostname.Trim().ToUpper());
            }
        }

        public static void Internal_dumpMap() {
            lock (typeof(JISystem)) {
                if (JISystem.Logger.isLoggable(Level.INFO)) {
                    Logger.info("mapOfHostnamesVsIPs: " + MapOfHostnamesVsIPs);
                }
            }
        }
    }

}