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
/// Internal framework com object interface
/// </summary>
public interface IComObjectInternal {

    /// <summary>
    /// Returns self Interface pointer.
    /// </summary>
    InterfacePointer GetInterfacePointer();

    /// <summary>
    /// Adds a connection point information and it's cookie to the  
    /// connectionPointMap internally.
    /// To be called only by the framework.
    /// </summary>
    /// <param name="connectionPoint"> </param>
    /// <param name="cookie"> </param>
    /// <returns> unique identifier for the combination. </returns>
    string SetConnectionInfo(IComObject connectionPoint, int? cookie);

    /// <summary>
    /// Framework Internal.
    /// Returns the ConnectionPoint (<see cref="IComObject"/>)
    /// and it's Cookie.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    object[] GetConnectionInfo(string identifier);

    /// <summary>
    /// Framework Internal.
    /// Returns and Removes the connection info from the internal map.
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    object[] RemoveConnectionInfo(string identifier);

    /// <summary>
    /// <i><u>Framework Internal</u></i>
    /// <param name="deffered"> </param>
    /// </summary>
    void SetDeffered(bool deffered);
}
