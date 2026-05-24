//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Common; 

/// <summary>
/// Default implementation of <code>IAuthInfo</code>.
/// </summary>
public sealed class DefaultAuthInfoImpl : IAuthInfo {

    /// <summary>
    ///Creates the AuthInfo Object.
    /// </summary>
    /// <param name="domain"> </param>
    /// <param name="username"> </param>
    /// <param name="password"> </param>
    public DefaultAuthInfoImpl(string domain, string username,
        string password) {
        UserName = username;
        Password = password;
        Domain = domain;
    }

    /// <summary>
    /// User name
    /// </summary>
    public string UserName { get; }

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// Domain
    /// </summary>
    public string Domain { get; }
}
