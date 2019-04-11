/// <summary>
/// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
/// 
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v1.
/// 
/// </summary>

namespace rpc {

    using NdrObject = ndr.NdrObject;
    using PresentationSyntax = rpc.core.PresentationSyntax;
    using UUID = rpc.core.UUID;

    public interface Endpoint {

        Transport Transport { get; }

        PresentationSyntax Syntax { get; }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void call(int semantics, rpc.core.UUID object, int opnum, ndr.NdrObject ndrobj) throws java.io.IOException;
        void Call(int semantics, UUID @object, int opnum, NdrObject ndrobj);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void detach() throws java.io.IOException;
        void Detach();

    }

    public static class Endpoint_Fields {
        public const int MAYBE = 0x01;
        public const int IDEMPOTENT = 0x02;
        public const int BROADCAST = 0x04;
    }

}