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

    using BindNoAcknowledgePdu = rpc.pdu.BindNoAcknowledgePdu;

    public class BindException : RpcException {

        public BindException() : base() {
        }

        public BindException(string message) : base(message) {
        }

        public BindException(string message, int rejectReason) : base(Message(message, rejectReason)) {
        }

        private static string Message(string message, int reason) {
            return (message != null) ? message + " (" + Message(reason) + ")" : Message(reason);
        }

        private static string Message(int reason) {
            switch (reason) {
            case BindNoAcknowledgePdu.REASON_NOT_SPECIFIED:
                return "REASON_NOT_SPECIFIED";
            case BindNoAcknowledgePdu.TEMPORARY_CONGESTION:
                return "TEMPORARY_CONGESTION";
            case BindNoAcknowledgePdu.LOCAL_LIMIT_EXCEEDED:
                return "LOCAL_LIMIT_EXCEEDED";
            case BindNoAcknowledgePdu.CALLED_PADDR_UNKNOWN:
                return "CALLED_PADDR_UNKNOWN";
            case BindNoAcknowledgePdu.PROTOCOL_VERSION_NOT_SUPPORTED:
                return "PROTOCOL_VERSION_NOT_SUPPORTED";
            case BindNoAcknowledgePdu.DEFAULT_CONTEXT_NOT_SUPPORTED:
                return "DEFAULT_CONTEXT_NOT_SUPPORTED";
            case BindNoAcknowledgePdu.USER_DATA_NOT_READABLE:
                return "USER_DATA_NOT_READABLE";
            case BindNoAcknowledgePdu.NO_PSAP_AVAILABLE:
                return "NO_PSAP_AVAILABLE";
            default:
                return "unknown";
            }
        }

    }

}