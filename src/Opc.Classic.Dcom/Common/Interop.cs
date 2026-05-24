// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal;
using SharpCifs.Util.Sharpen;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Sockets;

namespace Opc.Classic.Dcom.Common; 
/// <summary>
/// Class implemented for defining system wide changes.
/// <para><b>Note</b>: Methods starting with <i>internal_</i>
/// keyword are internal to the framework
/// and must not be called by the developer.
/// </para>
/// </summary>
public static class Interop {

    /// <summary>
    /// Indicates to the framework, if Windows Registry settings for
    /// DLL\OCX component identified by this object should be modified
    /// to add a <code>Surrogate</code> automatically.
    /// A <code>Surrogate</code> is a process which provides resources
    /// such as memory and cpu for a DLL\OCX to execute.
    /// This API overrides the instance specific flags set on 
    /// <see cref="Clsid"/> or <see cref="ProgId"/>.
    /// </summary>
    public static bool UseAutoRegistration { get; set; }

    /// <summary>
    /// Sometimes the DCOM runtime of Windows will not send a ping on
    /// time to the Framework.
    /// It is not very abnormal, since Windows can sometimes resort
    /// to mechanisms other than DCOM to keep a reference count for
    /// the instances they imported. In case of this framework, if a
    /// ping is not received within <see cref="DcomTimings.ObjectExpiryPeriod"/>, the Local Class is
    /// collected for GC. And if the COM server requires a
    /// reference to it or acts on a previously obtained reference,
    /// it is sent back an <i>Exception</i>.
    /// Please use this flag to set the Auto Collection status
    /// to ON or OFF. By Default, it is ON.
    /// </summary>
    public static bool IsCoClassAutoCollection { get; set; } = true;

    /// <summary>
    /// Sets the COM version this library uses to communicate with COM servers.
    /// </summary>
    /// <remarks>
    /// Default is 5.4 — which selects the modern <c>IRemoteSCMActivator</c>
    /// (a.k.a. DCOM v5.6) activation path in <see cref="Opc.Classic.Dcom.Core.ComServer"/>
    /// rather than the legacy <c>IRemoteActivation</c> (v5.4) path.
    /// <see cref="Opc.Classic.Dcom.Core.ComServer"/> selects the SCM activator
    /// whenever <c>MinorVersion &gt; 1</c>, which is true for the default.
    /// Set this to <c>new ComVersion(5, 1)</c> to opt back into the legacy
    /// v5.4 activation surface for Windows 2000 / XP pre-SP2 targets.
    /// </remarks>
    public static ComVersion COMVersion { set; get; } = new ComVersion();

    /// <summary>
    /// Returns the localized error messages for the error code.
    /// </summary>
    /// <param name="code"> error code
    /// </param>
    public static string GetLocalizedMessage(ErrorCode code) {
        var key = ((int)code).ToString("X8");

        string message;
        try {
            message = Resource.ResourceManager.GetString("0x" + key, CultureInfo.InvariantCulture);
            message = message + " [" + key + "]";
        }
        catch (MissingResourceException) {
            message = "Message not found for errorCode: " + key;
        }
        return message;
    }

    /// <summary>
    /// Queries the property file maintaining the <code>PROGID</code>
    /// Vs <code>CLSID</code> mappings and returns the <code>CLSID</code>
    /// if found or null otherwise.
    /// </summary>
    /// <param name="progId"> user friendly string such as "Excel.Application".
    /// </param>
    public static string GetClsidFromProgId(string progId) {
        if (progId == null) {
            return null;
        }
        if (_pathToDB == null) {
            lock (_syncRoot) {
                if (_pathToDB == null) {
                    Internal_readProgIdsFromFile();
                }
            }
        }
        return (string)kMapOfProgIdsVsClsids.GetProperty(progId);
    }

    /// <summary>
    /// Helper to load
    /// </summary>
    private static void Internal_readProgIdsFromFile() {
        _pathToDB = AppContext.BaseDirectory;
        try {
            var inputStream = new FileStream(_pathToDB, FileMode.Create, FileAccess.Write);
            kMapOfProgIdsVsClsids.Load(inputStream);
            inputStream.Close();
        }
        catch (Exception e) {
            Log.Logger.Error(e, "writeProgIdsToFile");
        }
        Log.Logger.Information("Read {@progIdVsClsidDB}", kMapOfProgIdsVsClsids);
    }

    /// <summary>
    /// Should be called from system shut down only
    /// </summary>
    internal static void Internal_writeProgIdsToFile() {
        if (_pathToDB != null) {
            try {
                var outputStream = new FileStream(_pathToDB, FileMode.Create, FileAccess.Write);
                kMapOfProgIdsVsClsids.Store(outputStream);
                outputStream.Close();
                Log.Logger.Information("Wrote {@progIdVsClsidDB}", kMapOfProgIdsVsClsids);
            }
            catch (IOException e) {
                Log.Logger.Error(e, "writeProgIdsToFile");
            }
        }
    }

    /// <summary>
    /// Stores it in a temporary hash map here, and this is later persisted
    /// when the library is shutdown
    /// </summary>
    internal static void Internal_setClsidtoProgId(string progId,
        string clsid) => kMapOfProgIdsVsClsids.SetProperty(progId, clsid);

    /// <summary>
    /// Adds a mapping between the <code>hostname</code> and its
    /// <code>IP</code>. This method should be used when there is
    /// a possibility of multiple adapters (for example from a Virtual
    /// Machine) on the COM server. The Framework only uses
    /// the host name and ignores the I.P addresses supplied in the
    /// interface reference of a COM object. If this hostname
    /// is not reachable from the machine where library is currently
    /// running (such as a Linux machine with no name mappings)
    /// then the call to this COM server would fail with an
    /// <code>UnknownHostException</code>. To avoid that either add the
    /// binding in the host machine or add the binding here.
    /// This method stores the name vs I.P binding in a <code>Map</code>.
    /// Providing the same <code>hostname</code> will overwrite
    /// the binding specified before.
    /// </summary>
    /// <param name="hostname"> name of target machine. </param>
    /// <param name="IP"> address of target machine in I.P format.
    /// </param>
    /// <exception cref="UnknownHostException"> if the <code>IP</code>
    /// is invalid or cannot be reached. </exception>
    /// <exception cref="ArgumentException"> if any parameter is
    /// <code>null</code> or of 0 length. </exception>
    public static void MapHostNametoIP(string hostname, string IP) {
        lock (_syncRoot) {
            if (string.IsNullOrWhiteSpace(hostname)) {
                throw new ArgumentException("Hostname must not be null, empty, or whitespace.", nameof(hostname));
            }
            if (string.IsNullOrWhiteSpace(IP)) {
                throw new ArgumentException("IP address must not be null, empty, or whitespace.", nameof(IP));
            }
            //just check the validity of IP
            // InetAddress.getByName(IP.Trim());
            kMapOfHostnamesVsIPs[hostname.Trim().ToUpperInvariant()] = IP.Trim();
        }
    }

    /// <summary>
    /// Returns I.P address for the given <code>hostname</code>.
    /// </summary>
    /// <param name="hostname"> </param>
    /// <returns> <code>null</code> if a mapping could not be found. </returns>
    internal static string GetIPForHostName(string hostname) {
        lock (_syncRoot) {
            return kMapOfHostnamesVsIPs[hostname.Trim().ToUpperInvariant()];
        }
    }

    /// <summary>
    /// Internal dump
    /// </summary>
    internal static void Internal_dumpMap() {
        lock (_syncRoot) {
            Log.Logger.Information("{@mapOfHostnamesVsIPs}", kMapOfHostnamesVsIPs);
        }
    }
    /// <summary>
    /// synchronisation will be performed by the oxid master
    /// </summary>
    public static Socket Internal_getSocket() => kSocketQueue.Remove(0);

    /// <summary>
    /// synchronisation will be performed by the oxid master
    /// @exclude
    /// </summary>
    public static void Internal_setSocket(Socket socket) => kSocketQueue.Add(socket);

    private static readonly System.Threading.Lock _syncRoot = new();
    private static string _pathToDB;
    private static readonly PropertyBag kMapOfProgIdsVsClsids = new PropertyBag();
    private static readonly List<Socket> kSocketQueue = new List<Socket>();
    private static readonly Dictionary<string, string> kMapOfHostnamesVsIPs =
        new Dictionary<string, string>(StringComparer.Ordinal);
}
