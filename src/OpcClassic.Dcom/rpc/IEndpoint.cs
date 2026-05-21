//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc {
    using SharpCifs.Dcerpc.Ndr;
    using SharpInterop.Rpc.Core;
    using System.IO;

    /// <summary>
    /// Endpoint interface
    /// </summary>
    public interface IEndpoint {

        /// <summary>
        /// Transport
        /// </summary>
        ITransport Transport { get; }

        /// <summary>
        /// Syntax
        /// </summary>
        PresentationSyntax Syntax { get; }

        /// <summary>
        /// Call
        /// </summary>
        /// <param name="semantics"></param>
        /// <param name="object"></param>
        /// <param name="opnum"></param>
        /// <param name="ndrobj"></param>
        /// <exception cref="IOException"></exception>
        void Call(Semantics semantics, UUID @object, int opnum, NdrOp ndrobj);

        /// <summary>
        /// Detach
        /// </summary>
        /// <exception cref="IOException"></exception>
        void Detach();
    }
}