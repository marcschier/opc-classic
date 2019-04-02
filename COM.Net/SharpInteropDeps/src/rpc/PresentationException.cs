using System.Text;

// 
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
// 
// j-Interop (Pure Java implementation of DCOM protocol)
// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 



namespace rpc
{

	using PresentationResult = core.PresentationResult;

	public class PresentationException : BindException
	{

		/// 
		private const long serialVersionUID = 5421952951585948361L;

		public PresentationException() : base()
		{
		}

		public PresentationException(string message) : base(message_Renamed)
		{
		}

		public PresentationException(string message, PresentationResult result) : base(message(message_Renamed, result))
		{
		}

		private static string message(string message, PresentationResult result)
		{
			if (result == null)
			{
				return message_Renamed;
			}
			return (message_Renamed != null) ? message_Renamed + " (" + message(result) + ")" : message(result);
		}

		private static string message(PresentationResult result)
		{
			var message_Renamed = new StringBuilder();
			switch (result.result)
			{
			case PresentationResult.ACCEPTANCE:
				message_Renamed.Append("ACCEPTANCE");
				break;
			case PresentationResult.USER_REJECTION:
				message_Renamed.Append("USER_REJECTION");
				break;
			case PresentationResult.PROVIDER_REJECTION:
				message_Renamed.Append("PROVIDER_REJECTION");
				break;
			default:
				message_Renamed.Append("unknown");
			break;
			}
			message_Renamed.Append("; ");
			switch (result.reason)
			{
			case PresentationResult.REASON_NOT_SPECIFIED:
				message_Renamed.Append("REASON_NOT_SPECIFIED");
				break;
			case PresentationResult.ABSTRACT_SYNTAX_NOT_SUPPORTED:
				message_Renamed.Append("ABSTRACT_SYNTAX_NOT_SUPPORTED");
				break;
			case PresentationResult.PROPOSED_TRANSFER_SYNTAXES_NOT_SUPPORTED:
				message_Renamed.Append("PROPOSED_TRANSFER_SYNTAXES_NOT_SUPPORTED");
				break;
			case PresentationResult.LOCAL_LIMIT_EXCEEDED:
				message_Renamed.Append("LOCAL_LIMIT_EXCEEDED");
				break;
			default:
				message_Renamed.Append("unknown");
			break;
			}
			return message_Renamed.ToString();
		}

	}

}