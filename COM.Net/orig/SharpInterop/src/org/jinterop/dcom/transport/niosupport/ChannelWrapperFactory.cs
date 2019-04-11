/// <summary>
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
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.transport.niosupport {


    /// <summary>
    /// Factory for ChannelWrappers
    /// </summary>
    public sealed class ChannelWrapperFactory {
        private ChannelWrapperFactory() {
            // Nothing to do
        }

        /// <summary>
        /// Static method to create a Channel Wrapper.
        /// </summary>
        /// <param name="selectorManager"> </param>
        /// <param name="selectableChannel"> </param>
        /// <param name="channelListener"> </param>
        /// <returns> the new read/write channel wrapper </returns>
        /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static ChannelWrapper createChannelWrapper(final SelectorManager selectorManager, final java.nio.channels.SelectableChannel selectableChannel, final ChannelListener channelListener) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public static ChannelWrapper CreateChannelWrapper(SelectorManager selectorManager, SelectableChannel selectableChannel, ChannelListener channelListener) {
            return new ChannelWrapperImpl(selectorManager, selectableChannel, channelListener);
        }
    }

}