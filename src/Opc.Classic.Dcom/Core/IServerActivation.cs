//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Core; 

/// <summary>
/// Server activation interface
/// </summary>
internal interface IServerActivation {

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
