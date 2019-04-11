using System;
using System.Collections.Generic;
using System.Threading;

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


    using JISystem = org.jinterop.dcom.common.JISystem;

    /// <summary>
    /// Confines selector operations to a single thread. Calls back to registered
    /// <seealso cref="ChannelListener"/>s on this thread when read operations are ready.
    /// </summary>
    public sealed class SelectorManager : Runnable {
        private readonly Selector Selector;

        private readonly Thread SelectThread;

        private readonly IList<Runnable> TaskList = new List<Runnable>();

        /// <summary>
        /// Constructor for SelectorManager.
        /// </summary>
        /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SelectorManager() throws java.io.IOException
        public SelectorManager() {
            SelectThread = new Thread(this, "jI_SelectorManager");
            SelectThread.Daemon = true;

            Selector = Selector.open();
            SelectThread.Start();
        }

        /// <summary>
        /// Shuts down the selector manager
        /// </summary>
        public void Destroy() {
            if (SelectThread.IsAlive) {
                SelectThread.Interrupt();
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void registerChannel(final java.nio.channels.SelectableChannel selectableChannel, final ChannelListener listener) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public void RegisterChannel(SelectableChannel selectableChannel, ChannelListener listener) {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Callable<Void> task = new java.util.concurrent.Callable<Void>()
            Callable<Void> task = new CallableAnonymousInnerClassHelper(this, selectableChannel, listener);

            InvokeSync(task);
        }

        private class CallableAnonymousInnerClassHelper : Callable<Void> {
            private readonly SelectorManager OuterInstance;

            private SelectableChannel SelectableChannel;
            private org.jinterop.dcom.transport.niosupport.ChannelListener Listener;

            public CallableAnonymousInnerClassHelper(SelectorManager outerInstance, SelectableChannel selectableChannel, org.jinterop.dcom.transport.niosupport.ChannelListener listener) {
                this.OuterInstance = outerInstance;
                this.SelectableChannel = selectableChannel;
                this.Listener = listener;
            }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Void call() throws java.io.IOException
            public virtual Void Call() {
                SelectableChannel.configureBlocking(false);
                SelectableChannel.register(OuterInstance.Selector, 0, Listener);

                return null;
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void setReadInterest(final java.nio.channels.SelectableChannel selectableChannel) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public SelectableChannel ReadInterest {
            set {
    //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
    //ORIGINAL LINE: final java.util.concurrent.Callable<Void> task = new java.util.concurrent.Callable<Void>()
                Callable<Void> task = new CallableAnonymousInnerClassHelper2(this, value);
    
                InvokeSync(task);
            }
        }

        private class CallableAnonymousInnerClassHelper2 : Callable<Void> {
            private readonly SelectorManager OuterInstance;

            private SelectableChannel SelectableChannel;

            public CallableAnonymousInnerClassHelper2(SelectorManager outerInstance, SelectableChannel selectableChannel) {
                this.OuterInstance = outerInstance;
                this.SelectableChannel = selectableChannel;
            }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Void call() throws java.io.IOException
            public virtual Void Call() {
                outerInstance.SetInterestOps(SelectableChannel, SelectionKey.OP_READ);

                return null;
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void removeReadInterest(final java.nio.channels.SelectableChannel selectableChannel) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public void RemoveReadInterest(SelectableChannel selectableChannel) {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Callable<Void> task = new java.util.concurrent.Callable<Void>()
            Callable<Void> task = new CallableAnonymousInnerClassHelper3(this, selectableChannel);

            InvokeSync(task);
        }

        private class CallableAnonymousInnerClassHelper3 : Callable<Void> {
            private readonly SelectorManager OuterInstance;

            private SelectableChannel SelectableChannel;

            public CallableAnonymousInnerClassHelper3(SelectorManager outerInstance, SelectableChannel selectableChannel) {
                this.OuterInstance = outerInstance;
                this.SelectableChannel = selectableChannel;
            }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Void call() throws Exception
            public virtual Void Call() {
                outerInstance.SetInterestOps(SelectableChannel, 0);

                return null;
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setInterestOps(java.nio.channels.SelectableChannel selectableChannel, int interestOps) throws java.io.IOException
        private void SetInterestOps(SelectableChannel selectableChannel, int interestOps) {
            try {
                if (selectableChannel.Registered) {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.channels.SelectionKey selectionKey = selectableChannel.keyFor(selector);
                    SelectionKey selectionKey = selectableChannel.keyFor(Selector);

                    selectionKey.interestOps(interestOps);
                }
            }
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final java.nio.channels.CancelledKeyException e)
            catch (CancelledKeyException e) {
                throw new IOException("Unable to set interest ops", e);
            }
        }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void invokeAsync(final Runnable task)
        private void InvokeAsync(Runnable task) {
            lock (TaskList) {
                TaskList.Add(task);
            }

            // To break out of the select and execute the tasks...
            Selector.wakeup();
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void invokeSync(final java.util.concurrent.Callable<Void> task) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        private void InvokeSync(Callable<Void> task) {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ExceptionHolder exceptionHolder = new ExceptionHolder();
            ExceptionHolder exceptionHolder = new ExceptionHolder();

            if (Thread.CurrentThread == SelectThread) {
                try {
                    task.call();
                }
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final Exception e)
                catch (Exception e) {
                    // Store the exception so we can check it's one of the ones
                    // declared as thrown
                    exceptionHolder.Exception = e;
                }
            }
            else {
                // Used to deliver the notification that the task is executed
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(1);
                CountDownLatch latch = new CountDownLatch(1);

                InvokeAsync(new RunnableAnonymousInnerClassHelper(this, task, exceptionHolder, latch));

                try {
                    // Wait for completion
                    latch.@await();
                }
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final InterruptedException e)
                catch (terruptedException) {
                    // Set the interrupted flag
                    Thread.CurrentThread.Interrupt();
                }
            }

            // Throw any exception thrown by the task
            if (exceptionHolder.Exception != null) {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Exception thrownException = exceptionHolder.getException();
                Exception thrownException = exceptionHolder.Exception;

                throw LaunderIOException(thrownException);
            }
        }

        private class RunnableAnonymousInnerClassHelper : Runnable {
            private readonly SelectorManager OuterInstance;

            private Callable<Void> Task;
            private org.jinterop.dcom.transport.niosupport.SelectorManager.ExceptionHolder ExceptionHolder;
            private CountDownLatch Latch;

            public RunnableAnonymousInnerClassHelper(SelectorManager outerInstance, Callable<Void> task, org.jinterop.dcom.transport.niosupport.SelectorManager.ExceptionHolder exceptionHolder, CountDownLatch latch) {
                this.OuterInstance = outerInstance;
                this.Task = task;
                this.ExceptionHolder = exceptionHolder;
                this.Latch = latch;
            }

            public virtual void Run() {
                try {
                    Task.call();
                }
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final Exception e)
                catch (Exception e) {
                    ExceptionHolder.Exception = e;
                }
                finally {
                    Latch.countDown();
                }
            }
        }

        private IOException LaunderIOException(Exception thrownException) {
            if (thrownException is Exception) {
                throw (Exception) thrownException;
            }

            if (thrownException is IOException) {
                return (IOException) thrownException;
            }

            throw new UndeclaredThrowableException(thrownException);
        }

        private void DoInvocations() {
            bool processedTask = false;

            lock (TaskList) {
                foreach (Runnable task in TaskList) {
                    task.run();
                    processedTask = true;
                }
                TaskList.Clear();
            }

            // Just in case we are called with nothing to do so that we dont
            // get busy cpu.
            if (!processedTask) {
                try {
                    Thread.Sleep(0, 1);
                }
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final InterruptedException e)
                catch (terruptedException) {
                    // Set the interrupted flag
                    Thread.CurrentThread.Interrupt();
                }
            }
        }

        /// <seealso cref= java.lang.Runnable#run() </seealso>
        public void Run() {
            try {
                while (true) {
                    if (Thread.CurrentThread.Interrupted) {
                        JISystem.Logger.log(Level.INFO, "Selector manager interrupted");
                        return;
                    }

                    DoInvocations();

                    DoSelect();
                }
            }
            catch (Exception t) {
                Cleanup();
                Logger.log(Level.SEVERE, "Selector manager is unexpectedly exiting", t);
            }
        }

        private void DoSelect() {
            try {
                if (Selector.select() != 0) {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Iterator<java.nio.channels.SelectionKey> it = selector.selectedKeys().iterator();
                    IEnumerator<SelectionKey> it = Selector.selectedKeys().GetEnumerator();

                    while (it.MoveNext()) {
                        try {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.channels.SelectionKey selectionKey = it.Current;
                            SelectionKey selectionKey = it.Current;
                            it.remove();

                            // Client must re-obtain read interest once it has
                            // handled the read and is ready for the next read
                            selectionKey.interestOps(0);

                            // Call back to the listener for it to do the read
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ChannelListener listener = (ChannelListener) selectionKey.attachment();
                            ChannelListener listener = (ChannelListener) selectionKey.attachment();

                            listener.ReadReady();
                        }
                        catch (CancelledKeyException e) {
                            if (Logger.isLoggable(Level.FINE)) {
                                Logger.log(Level.FINE, "Ignoring cancelled key exception", e);
                            }
                        }
                    }
                }
            }
            catch (IOException e) {
                Logger.log(Level.WARNING, "Exception during SelectionManager select", e);
            }
        }

        private void Cleanup() {
            foreach (SelectionKey key in Selector.keys()) {
                try {
                    key.channel().close();
                }
                catch (IOException e) {
                    if (Logger.isLoggable(Level.FINE)) {
                        Logger.log(Level.FINE, "Ignoring channel close exception", e);
                    }
                }
            }

            try {
                Selector.close();
            }
            catch (IOException e) {
                if (Logger.isLoggable(Level.FINE)) {
                    Logger.log(Level.FINE, "Ignoring selector close exception", e);
                }
            }
        }

        private Logger Logger {
            get {
                return JISystem.Logger;
            }
        }

        private class ExceptionHolder {
            internal Exception Exception_Renamed;

            public virtual Exception Exception {
                get {
                    return Exception_Renamed;
                }
                set {
                    this.Exception_Renamed = value;
                }
            }

        }
    }

}