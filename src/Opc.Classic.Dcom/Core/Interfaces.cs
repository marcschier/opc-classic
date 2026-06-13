// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom;

/// <summary>
/// Interface constants
/// </summary>
public static class Interfaces
{
    /// <summary>
    /// IID representing the COM <code>IDispatch</code>.
    /// </summary>
    public const string IID_IDispatch = "00020400-0000-0000-C000-000000000046";

    /// <summary>
    /// IID representing the COM <code>ITypeLib</code>.
    /// </summary>
    public const string IID_ITypeLib = "00020402-0000-0000-C000-000000000046";

    /// <summary>
    /// IID representing the COM <code>ITypeInfo</code>.
    /// </summary>
    public const string IID_ITypeInfo = "00020401-0000-0000-C000-000000000046";

    /// <summary>
    /// IID representing the COM <code>IEnumVARIANT</code>.
    /// </summary>
    public const string IID_IEnumVARIANT = "00020404-0000-0000-C000-000000000046";

    /// <summary>
    /// RPC interface UUID for <code>IActivation</code>.
    /// </summary>
    public const string IID_IActivation = "4d9f4ab8-7d1c-11cf-861e-0020af6e7c57";

    /// <summary>
    /// The value of the iid field of the <code>pActProperties OBJREF</code>. structure
    /// </summary>
    public const string IID_IActivationPropertiesIn = "000001A2-0000-0000-C000-000000000046";

    /// <summary>
    /// The value of the iid field of the <code>ppActProperties OBJREF</code>. structure
    /// </summary>
    public const string IID_IActivationPropertiesOut = "000001A3-0000-0000-C000-000000000046";

    /// <summary>
    /// The value of the iid field of the <code>Context</code>. structure.
    /// </summary>
    public const string IID_IContext = "000001C0-0000-0000-C000-000000000046";

    /// <summary>
    /// RPC interface UUID for <code>IObjectExporter</code>.
    /// </summary>
    public const string IID_IObjectExporter = "99fcfec4-5260-101b-bbcb-00aa0021347a";

    /// <summary>
    /// RPC interface UUID for <code>IRemoteSCMActivator</code>.
    /// </summary>
    public const string IID_IRemoteSCMActivator = "000001A0-0000-0000-C000-000000000046";

    /// <summary>
    /// RPC interface UUID for <code>IRemUnknown</code>.
    /// </summary>
    public const string IID_IRemUnknown = "00000131-0000-0000-C000-000000000046";

    /// <summary>
    /// RPC interface UUID for <code>IRemUnknown2</code>.
    /// </summary>
    public const string IID_IRemUnknown2 = "00000143-0000-0000-C000-000000000046";

    /// <summary>
    /// RPC interface UUID for <code>IUnknown</code>.
    /// </summary>
    public const string IID_IUnknown = "00000000-0000-0000-C000-000000000046";
}
