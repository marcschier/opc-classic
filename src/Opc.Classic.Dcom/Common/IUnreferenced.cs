// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Common;

/// <summary>
/// Implement this interface receive notifications for <code><see cref="IComObject"/></code>s when
/// they get garbage collected. This also means that the actual interface reference on
/// the COM server have a reference count of 0 and will get garbage collected itself by
/// COM runtime.
/// </summary>
/// <remarks>
/// One note of caution, the <code><see cref="IComObject"/></code> is uniquely identified across the
/// client-server relationship by it's <code>IPID</code>. The <code>IPID</code> should be used
/// as a key to store a relevant "action" object when <code>unReferenced</code> method of this
/// interface is invoked. If the <code><see cref="IComObject"/></code> is stored at a place solely for the
/// purpose of this housekeeping than it will <b>NEVER</b> get garbage collected by the framework as
/// the logic of collection is based on weak references.
/// <para>
/// <code>
///   comObject.RegisterUnreferencedHandler(session, new IUnreferenced(){
///           public void UnReferenced()
///           {
///               // do something here
///           }
///       });
/// </code>
/// </para>
/// Please refer to MSWMI example for more details on how to use this class.
/// </remarks>
public interface IUnreferenced {

    /// <summary>
    /// Called when the <code><see cref="IComObject"/></code> associated with this
    /// interface is garbage collected by the framework.
    /// </summary>
    void UnReferenced();
}
