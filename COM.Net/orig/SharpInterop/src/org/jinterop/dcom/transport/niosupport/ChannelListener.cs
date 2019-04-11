/*
 * Copyright 2004 WIT-Software, Lda. 
 * - web: http://www.wit-software.com 
 * - email: info@wit-software.com
 *
 * All rights reserved. Relased under terms of the 
 * Creative Commons' Attribution-NonCommercial-ShareAlike license.
 */

namespace org.jinterop.dcom.transport.niosupport {

    /// <summary>
    /// Listener to be notified when read operations can be performed.
    /// <para>
    /// Implementations should perform the actual IO on a different thread so as not
    /// to hold up the single selector thread on which callbacks are made. They can
    /// do this either by the use of a thread pool or by using another
    /// synchronization mechanism to indicate to the IO thread that the operation is
    /// now ready to be performed.
    /// </para>
    /// <para>
    /// Currently supports read operations only.
    /// </para>
    /// </summary>
    public interface ChannelListener {
        /// <summary>
        /// Called when the channel is ready for reading
        /// </summary>
        void ReadReady();
    }

}