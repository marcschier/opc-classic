//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using SharpInterop.Common;
using SharpInterop.Registry;
using SharpCifs.Util.Sharpen;
using System;

namespace SharpInterop.Core; 
/// <summary>
/// Wrapper class used to define user friendly <code>ProgID</code>.
/// Definition from MSDN:
/// <i>
/// A ProgID, or programmatic identifier, is a registry entry that
/// can be associated with a CLSID. The format of a ProgID is
/// &lt;Vendor&gt;.&lt;Component&gt;.&lt;Version&gt;, separated by
/// periods and with no spaces, as in Word.Document.6. Like the CLSID,
/// the ProgID identifies a class, but with less precision.
/// </i>
/// This class uses the <code>WINREG</code> service to get the
/// mapping between the <code>ProgId</code> and the <code>CLSID</code>.
/// The internal database is looked up first before making calls
/// to <code>WINREG</code> service.
/// </summary>
public class ProgId {

    /// <summary>
    /// Indicates to the framework, if Windows Registry settings for
    /// DLL\OCX component identified by this object should be
    /// modified to add a <code>Surrogate</code> automatically.
    /// A <code>Surrogate</code> is a process which provides resources
    /// such as memory and cpu for a DLL\OCX to execute.
    /// </summary>
    /// <remarks> <code>true</code> if auto registration should be
    /// done by the framework. </remarks>
    public bool AutoRegistration { set; get; }

    /// <summary>
    /// Factory method returning an instance of this class.
    /// </summary>
    /// <param name="progId"> user-friendly string representation
    /// such as "Excel.Application"
    /// </param>
    public static ProgId ValueOf(string progId) => new ProgId(progId);

    /// <summary>
    /// Create prog id
    /// </summary>
    /// <param name="progId"></param>
    private ProgId(string progId) {
        _progId = progId;
        _clsid = Clsid.ValueOf(Interop.GetClsidFromProgId(progId));
    }

    /// <summary>
    /// Returns the <code>CLSID</code> for this <code>ProgId</code>.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="session"></param>
    /// <exception cref="InteropException"> </exception>
    public Clsid GetCorrespondingClsid(
        string server, Session session) {
        if (_clsid == null) {
            _clsid = GetIdFromWinReg(server, session);
        }
        return _clsid;
    }

    /// <summary>
    /// Returns the <code>CLSID</code> for this <code>ProgId</code>.
    /// </summary>
    /// <exception cref="InteropException"> </exception>
    public Clsid GetCorrespondingClsid() => _clsid;

    /// <summary>
    /// Get id from remote registry
    /// </summary>
    /// <param name="server"></param>
    /// <param name="session"></param>
    /// <exception cref="InteropException"></exception>
    private Clsid GetIdFromWinReg(string server, Session session) {
        IRegistry winreg;
        if (server == null) {
            server = session.TargetServer;
        }
        try {
            if (session.SSOEnabled) {
                winreg = RegistryFactory.Instance.GetRegistryClient(
                    server, true);
            }
            else {
                winreg = RegistryFactory.Instance.GetRegistryClient(
                    new DefaultAuthInfoImpl(session.Domain,
                    session.UserName, session.Password), server, true);
            }
        }
        catch (UnknownHostException) {
            throw new InteropException(ErrorCode.INTEROP_WINREG_EXCEPTION3);
        }
        var handle = winreg.OpenHKLM();
        var handle2 = winreg.OpenKey(handle, "SOFTWARE\\Classes\\" +
            _progId + "\\CLSID", RegKeyAccess.KEY_READ);
        var key = StringHelperClass.NewString(winreg.QueryValue(handle2, 255));
        winreg.CloseKey(handle2);
        winreg.CloseKey(handle);
        winreg.CloseConnection();
        // seperate the {}
        var clsid = Clsid.ValueOf(key.SubstringSpecial(key.IndexOf('{') + 1,
            key.IndexOf('}')));
        clsid.UseAutoRegistration = AutoRegistration;
        Interop.Internal_setClsidtoProgId(_progId, clsid.CLSID);
        return clsid;
    }

    private readonly string _progId;
    private Clsid _clsid;
}
