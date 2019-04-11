using System.Text;

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

    using PresentationResult = rpc.core.PresentationResult;

    public class PresentationException : BindException {

        /// 
        private const long SerialVersionUID = 5421952951585948361L;

        public PresentationException() : base() {
        }

        public PresentationException(string message) : base(message) {
        }

        public PresentationException(string message, PresentationResult result) : base(Message(message, result)) {
        }

        private static string Message(string message, PresentationResult result) {
            if (result == null) {
                return message;
            }
            return (message != null) ? message + " (" + Message(result) + ")" : Message(result);
        }

        private static string Message(PresentationResult result) {
            StringBuilder message = new StringBuilder();
            switch (result.Result) {
            case PresentationResult.ACCEPTANCE:
                message.Append("ACCEPTANCE");
                break;
            case PresentationResult.USER_REJECTION:
                message.Append("USER_REJECTION");
                break;
            case PresentationResult.PROVIDER_REJECTION:
                message.Append("PROVIDER_REJECTION");
                break;
            default:
                message.Append("unknown");
            break;
            }
            message.Append("; ");
            switch (result.Reason) {
            case PresentationResult.REASON_NOT_SPECIFIED:
                message.Append("REASON_NOT_SPECIFIED");
                break;
            case PresentationResult.ABSTRACT_SYNTAX_NOT_SUPPORTED:
                message.Append("ABSTRACT_SYNTAX_NOT_SUPPORTED");
                break;
            case PresentationResult.PROPOSED_TRANSFER_SYNTAXES_NOT_SUPPORTED:
                message.Append("PROPOSED_TRANSFER_SYNTAXES_NOT_SUPPORTED");
                break;
            case PresentationResult.LOCAL_LIMIT_EXCEEDED:
                message.Append("LOCAL_LIMIT_EXCEEDED");
                break;
            default:
                message.Append("unknown");
            break;
            }
            return message.ToString();
        }

    }

}