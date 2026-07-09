// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Internal framework com object interface
/// </summary>
public interface IComObjectInternal
{
    /// <summary>
    /// Returns self Interface pointer.
    /// </summary>
    InterfacePointer GetInterfacePointer();

    /// <summary>
    /// Adds a connection point information and it's cookie to the  
    /// connectionPointMap internally.
    /// To be called only by the framework.
    /// </summary>
    /// <param name="connectionPoint">Connection point COM object associated with the generated cookie.</param>
    /// <param name="cookie">Connection-point cookie returned by the remote advise call.</param>
    /// <returns> unique identifier for the combination. </returns>
    string SetConnectionInfo(IComObject connectionPoint, int? cookie);

    /// <summary>
    /// Framework Internal.
    /// Returns the ConnectionPoint (<see cref="IComObject"/>)
    /// and it's Cookie.
    /// </summary>
    /// <param name="identifier">Connection-point identifier returned when the mapping was registered.</param>
    /// <returns>The sequence of connection info values produced by the operation.</returns>
    object[] GetConnectionInfo(string identifier);

    /// <summary>
    /// Framework Internal.
    /// Returns and Removes the connection info from the internal map.
    /// </summary>
    /// <param name="identifier">Connection-point identifier returned when the mapping was registered.</param>
    /// <returns>The sequence of remove connection info values produced by the operation.</returns>
    object[] RemoveConnectionInfo(string identifier);

    /// <summary>
    /// <i><u>Framework Internal</u></i>
    /// <param name="deffered">Value indicating whether marshaling for the COM object should be deferred.</param>
    /// </summary>
    void SetDeffered(bool deffered);
}
