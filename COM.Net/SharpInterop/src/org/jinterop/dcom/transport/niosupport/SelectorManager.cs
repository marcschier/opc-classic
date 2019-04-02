// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.transport.niosupport {


    using JISystem = common.JISystem;

    /// <summary>
    /// Confines selector operations to a single thread. Calls back to registered
    /// <seealso cref="ChannelListener"/>s on this thread when read operations are ready.
    /// </summary>
    public sealed class SelectorManager : Runnable
	{
		private readonly Selector selector;

		private readonly Thread selectThread;

		private readonly IList<Runnable> taskList = new List<Runnable>();

		/// <summary>
		/// Constructor for SelectorManager.
		/// </summary>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SelectorManager() throws java.io.IOException
		public SelectorManager()
		{
            selectThread = new Thread(this, "jI_SelectorManager") {
                Daemon = true
            };

            selector = Selector.open();
			selectThread.Start();
		}

		/// <summary>
		/// Shuts down the selector manager
		/// </summary>
		public void destroy()
		{
			if (selectThread.IsAlive)
			{
				selectThread.Interrupt();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void registerChannel(final java.nio.channels.SelectableChannel selectableChannel, final ChannelListener listener) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		internal void registerChannel(SelectableChannel selectableChannel, ChannelListener listener)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Callable<Void> task = new java.util.concurrent.Callable<Void>()
			Callable<Void> task = new CallableAnonymousInnerClassHelper(this, selectableChannel, listener);

			invokeSync(task);
		}

		private class CallableAnonymousInnerClassHelper : Callable<Void>
		{
			private readonly SelectorManager outerInstance;

			private SelectableChannel selectableChannel;
			private readonly ChannelListener listener;

			public CallableAnonymousInnerClassHelper(SelectorManager outerInstance, SelectableChannel selectableChannel, ChannelListener listener)
			{
				this.outerInstance = outerInstance;
				this.selectableChannel = selectableChannel;
				this.listener = listener;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Void call() throws java.io.IOException
			public virtual Void call()
			{
				selectableChannel.configureBlocking(false);
				selectableChannel.register(outerInstance.selector, 0, listener);

				return null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void setReadInterest(final java.nio.channels.SelectableChannel selectableChannel) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		internal SelectableChannel ReadInterest
		{
			set
			{
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final java.util.concurrent.Callable<Void> task = new java.util.concurrent.Callable<Void>()
				Callable<Void> task = new CallableAnonymousInnerClassHelper2(this, value);
    
				invokeSync(task);
			}
		}

		private class CallableAnonymousInnerClassHelper2 : Callable<Void>
		{
			private readonly SelectorManager outerInstance;

			private readonly SelectableChannel selectableChannel;

			public CallableAnonymousInnerClassHelper2(SelectorManager outerInstance, SelectableChannel selectableChannel)
			{
				this.outerInstance = outerInstance;
				this.selectableChannel = selectableChannel;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Void call() throws java.io.IOException
			public virtual Void call()
			{
				outerInstance.setInterestOps(selectableChannel, SelectionKey.OP_READ);

				return null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void removeReadInterest(final java.nio.channels.SelectableChannel selectableChannel) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		internal void removeReadInterest(SelectableChannel selectableChannel)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.Callable<Void> task = new java.util.concurrent.Callable<Void>()
			Callable<Void> task = new CallableAnonymousInnerClassHelper3(this, selectableChannel);

			invokeSync(task);
		}

		private class CallableAnonymousInnerClassHelper3 : Callable<Void>
		{
			private readonly SelectorManager outerInstance;

			private readonly SelectableChannel selectableChannel;

			public CallableAnonymousInnerClassHelper3(SelectorManager outerInstance, SelectableChannel selectableChannel)
			{
				this.outerInstance = outerInstance;
				this.selectableChannel = selectableChannel;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Void call() throws Exception
			public virtual Void call()
			{
				outerInstance.setInterestOps(selectableChannel, 0);

				return null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void setInterestOps(java.nio.channels.SelectableChannel selectableChannel, int interestOps) throws java.io.IOException
		private void setInterestOps(SelectableChannel selectableChannel, int interestOps)
		{
			try
			{
				if (selectableChannel.Registered)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.channels.SelectionKey selectionKey = selectableChannel.keyFor(selector);
					SelectionKey selectionKey = selectableChannel.keyFor(selector);

					selectionKey.interestOps(interestOps);
				}
			}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final java.nio.channels.CancelledKeyException e)
			catch (CancelledKeyException e)
			{
				throw new IOException("Unable to set interest ops", e);
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void invokeAsync(final Runnable task)
		private void invokeAsync(Runnable task)
		{
			lock (taskList)
			{
				taskList.Add(task);
			}

			// To break out of the select and execute the tasks...
			selector.wakeup();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void invokeSync(final java.util.concurrent.Callable<Void> task) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		private void invokeSync(Callable<Void> task)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ExceptionHolder exceptionHolder = new ExceptionHolder();
			var exceptionHolder = new ExceptionHolder();

			if (Thread.CurrentThread == selectThread)
			{
				try
				{
					task.call();
				}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final Exception e)
				catch (Exception e)
				{
					// Store the exception so we can check it's one of the ones
					// declared as thrown
					exceptionHolder.Exception = e;
				}
			}
			else
			{
				// Used to deliver the notification that the task is executed
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(1);
				var latch = new CountDownLatch(1);

				invokeAsync(new RunnableAnonymousInnerClassHelper(this, task, exceptionHolder, latch));

				try
				{
					// Wait for completion
					latch.@await();
				}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final InterruptedException e)
				catch (terruptedException)
				{
					// Set the interrupted flag
					Thread.CurrentThread.Interrupt();
				}
			}

			// Throw any exception thrown by the task
			if (exceptionHolder.Exception != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Exception thrownException = exceptionHolder.getException();
				var thrownException = exceptionHolder.Exception;

				throw launderIOException(thrownException);
			}
		}

		private class RunnableAnonymousInnerClassHelper : Runnable
		{
			private readonly SelectorManager outerInstance;

			private Callable<Void> task;
			private ExceptionHolder exceptionHolder;
			private CountDownLatch latch;

			public RunnableAnonymousInnerClassHelper(SelectorManager outerInstance, Callable<Void> task, ExceptionHolder exceptionHolder, CountDownLatch latch)
			{
				this.outerInstance = outerInstance;
				this.task = task;
				this.exceptionHolder = exceptionHolder;
				this.latch = latch;
			}

			public virtual void run()
			{
				try
				{
					task.call();
				}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final Exception e)
				catch (Exception e)
				{
					exceptionHolder.Exception = e;
				}
				finally
				{
					latch.countDown();
				}
			}
		}

		private IOException launderIOException(Exception thrownException)
		{
			if (thrownException is Exception)
			{
				throw (Exception) thrownException;
			}

			if (thrownException is IOException)
			{
				return (IOException) thrownException;
			}

			throw new UndeclaredThrowableException(thrownException);
		}

		private void doInvocations()
		{
			var processedTask = false;

			lock (taskList)
			{
				foreach (Runnable task in taskList)
				{
					task.run();
					processedTask = true;
				}
				taskList.Clear();
			}

			// Just in case we are called with nothing to do so that we dont
			// get busy cpu.
			if (!processedTask)
			{
				try
				{
					Thread.Sleep(0, 1);
				}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final InterruptedException e)
				catch (terruptedException)
				{
					// Set the interrupted flag
					Thread.CurrentThread.Interrupt();
				}
			}
		}

		/// <seealso cref= java.lang.Runnable#run() </seealso>
		public void run()
		{
			try
			{
				while (true)
				{
					if (Thread.CurrentThread.Interrupted)
					{
						Log.Logger.log(Level.INFO, "Selector manager interrupted");
						return;
					}

					doInvocations();

					doSelect();
				}
			}
			catch (Exception t)
			{
				cleanup();
				Logger.log(Level.SEVERE, "Selector manager is unexpectedly exiting", t);
			}
		}

		private void doSelect()
		{
			try
			{
				if (selector.select() != 0)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Iterator<java.nio.channels.SelectionKey> it = selector.selectedKeys().iterator();
					IEnumerator<SelectionKey> it = selector.selectedKeys().GetEnumerator();

					while (it.MoveNext())
					{
						try
						{
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
							var listener = (ChannelListener) selectionKey.attachment();

							listener.readReady();
						}
						catch (CancelledKeyException e)
						{
							if (Logger.isLoggable(Level.FINE))
							{
								Logger.log(Level.FINE, "Ignoring cancelled key exception", e);
							}
						}
					}
				}
			}
			catch (IOException e)
			{
				Logger.log(Level.WARNING, "Exception during SelectionManager select", e);
			}
		}

		private void cleanup()
		{
			foreach (SelectionKey key in selector.keys())
			{
				try
				{
					key.channel().close();
				}
				catch (IOException e)
				{
					if (Logger.isLoggable(Level.FINE))
					{
						Logger.log(Level.FINE, "Ignoring channel close exception", e);
					}
				}
			}

			try
			{
				selector.close();
			}
			catch (IOException e)
			{
				if (Logger.isLoggable(Level.FINE))
				{
					Logger.log(Level.FINE, "Ignoring selector close exception", e);
				}
			}
		}

        private Logger Logger => Log.Logger;

        private class ExceptionHolder
		{
			internal Exception exception;

			internal virtual Exception Exception {
                get => exception;
                set => exception = value;
            }

        }
	}

}