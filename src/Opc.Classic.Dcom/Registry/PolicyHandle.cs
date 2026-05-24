//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Registry; 

/// <summary>
/// Policy handle for each key.
/// </summary>
public class PolicyHandle {

    /// <summary>
    /// Handle to the Key
    /// </summary>
    public byte[] Handle { get; }

    /// <summary>
    /// True, if the key was newly created.
    /// </summary>
    public bool NewlyCreated { get; }

    /// <summary>
    /// Create handle
    /// </summary>
    /// <param name="newlyCreated"> </param>
    public PolicyHandle(bool newlyCreated) {
        NewlyCreated = newlyCreated;
        Handle = new byte[20];
    }
}
