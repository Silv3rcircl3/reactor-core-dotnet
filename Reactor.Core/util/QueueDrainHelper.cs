﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reactive.Streams;
using Reactor.Core;
using System.Threading;
using Reactor.Core.flow;
using Reactor.Core.subscription;
using Reactor.Core.util;

namespace Reactor.Core.util
{
    /// <summary>
    /// Helper methods to work with the regular queue-drain serialization approach
    /// </summary>
    public static class QueueDrainHelper
    {
        /// <summary>
        /// Atomically increment the work-in-progress counter and return true if
        /// it transitioned from 0 to 1.
        /// </summary>
        /// <param name="wip">The work-in-progress field</param>
        /// <returns>True if the counter transitioned from 0 to 1</returns>
        public static bool Enter(ref int wip)
        {
            return Interlocked.Increment(ref wip) == 1;
        }

        /// <summary>
        /// Atomically try to decrement the work-in-progress counter and return
        /// its new value.
        /// </summary>
        /// <param name="wip">The target work-in-progress counter field</param>
        /// <param name="missed">The number to decrement the counter, positive (not verified)</param>
        /// <returns>The new work-in-progress value</returns>
        public static int Leave(ref int wip, int missed)
        {
            int w = Volatile.Read(ref wip);
            if (w == missed)
            {
                return Interlocked.Add(ref wip, -missed);
            }
            else
            {
                return w;
            }
        }

        public static bool CheckTerminated<T, U>(ref bool cancelled, ref bool done, ref Exception error, 
            ISubscriber<T> actual, IQueue<U> queue, ISubscription s)
        {
            if (Volatile.Read(ref cancelled))
            {
                queue.Clear();
                return true;
            }

            if (Volatile.Read(ref done))
            {
                Exception ex = Volatile.Read(ref error);
                if (ex != null)
                {
                    ex = ExceptionHelper.Terminate(ref error);
                    queue.Clear();
                    actual.OnError(ex);
                    return true;
                }
                else
                {
                    bool empty;

                    try
                    {
                        empty = queue.IsEmpty();
                    }
                    catch (Exception exc)
                    {
                        ExceptionHelper.ThrowIfFatal(exc);

                        queue.Clear();
                        s.Cancel();

                        ExceptionHelper.AddError(ref error, exc);
                        exc = ExceptionHelper.Terminate(ref error);

                        actual.OnError(exc);
                        return true;
                    }

                    if (empty)
                    {
                        actual.OnComplete();
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool CheckTerminatedDelayed<T, U>(ref bool cancelled, ref bool done, ref Exception error, 
            ISubscriber<T> actual, IQueue<U> queue, ISubscription s)
        {
            if (Volatile.Read(ref cancelled))
            {
                queue.Clear();
                return true;
            }

            if (Volatile.Read(ref done))
            {
                bool empty;

                try
                {
                    empty = queue.IsEmpty();
                }
                catch (Exception exc)
                {
                    ExceptionHelper.ThrowIfFatal(exc);

                    queue.Clear();
                    s.Cancel();

                    ExceptionHelper.AddError(ref error, exc);
                    exc = ExceptionHelper.Terminate(ref error);

                    actual.OnError(exc);
                    return true;
                }

                if (empty)
                {
                    Exception ex = Volatile.Read(ref error);
                    if (ex != null)
                    {
                        ex = ExceptionHelper.Terminate(ref error);
                        actual.OnError(ex);
                    }
                    else
                    {
                        actual.OnComplete();
                    }
                    return true;
                }
            }

            return false;
        }

    }
}
