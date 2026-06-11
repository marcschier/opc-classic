// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Server activation interface
/// </summary>
internal interface IServerActivation
{

    /// <summary>
    /// Activation successful
    /// </summary>
    bool ActivationSuccessful { get; }

    /// <summary>
    /// Dual string array
    /// </summary>
    DualStringArray DualStringArrayForOxid { get; }

    /// <summary>
    /// Interface pointer
    /// </summary>
    InterfacePointer MInterfacePointer { get; }

    /// <summary>
    /// Pid
    /// </summary>
    string IPID { get; }

    /// <summary>
    /// Dual interface
    /// </summary>
    bool Dual { get; }

    /// <summary>
    /// Dispatch id
    /// </summary>
    string DispIpid { get; set; }

    /// <summary>
    /// Dispatch references
    /// </summary>
    int DispRefs { get; }
}
