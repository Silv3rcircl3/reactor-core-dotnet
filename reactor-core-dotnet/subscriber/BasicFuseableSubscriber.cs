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

namespace Reactor.Core.subscriber
{
    internal abstract class BasicFuseableSubscriber<T, U> : ISubscriber<T>, IQueueSubscription<U>
    {
        /// <summary>
        /// The actual child ISubscriber.
        /// </summary>
        protected readonly ISubscriber<U> actual;

        protected bool done;

        protected IQueueSubscription<T> s;

        protected int fusionMode;

        internal BasicFuseableSubscriber(ISubscriber<U> actual)
        {
            this.actual = actual;
        }

        public void Cancel()
        {
            s.Cancel();
        }

        public abstract void OnComplete();

        public abstract void OnError(Exception e);

        public abstract void OnNext(T t);

        public void OnSubscribe(ISubscription s)
        {
            if (SubscriptionHelper.Validate(ref this.s, s as IQueueSubscription<T>))
            {

                actual.OnSubscribe(this);

                OnStart();
            }
        }

        public void Request(long n)
        {
            s.Request(n);
        }

        /// <summary>
        /// Called once the OnSubscribe has been called the first time
        /// and this has been set on the child ISubscriber.
        /// </summary>
        protected abstract void OnStart();

        /// <summary>
        /// Complete the actual ISubscriber if the sequence is not already done.
        /// </summary>
        protected void Complete()
        {
            if (done)
            {
                return;
            }
            done = true;
            actual.OnComplete();
        }

        /// <summary>
        /// Signal an error to the actual ISubscriber if the sequence is not already done.
        /// </summary>
        protected void Error(Exception ex)
        {
            if (done)
            {
                ExceptionHelper.OnErrorDropped(ex);
                return;
            }
            done = true;
            actual.OnComplete();
        }

        /// <summary>
        /// Rethrows a fatal exception, cancels the ISubscription and
        /// calls <see cref="Error(Exception)"/>
        /// </summary>
        /// <param name="ex">The exception to rethrow or signal</param>
        protected void Fail(Exception ex)
        {
            ExceptionHelper.ThrowIfFatal(ex);
            s.Cancel();
            Error(ex);
        }

        public abstract int RequestFusion(int mode);

        public bool Offer(U value)
        {
            return FuseableHelper.DontCallOffer();
        }

        public abstract bool Poll(out U value);

        public abstract bool IsEmpty();

        public abstract void Clear();

        /// <summary>
        /// Forward the mode request to the upstream IQueueSubscription and
        /// set the mode it returns.
        /// </summary>
        /// <param name="mode">The incoming fusion mode.</param>
        /// <returns>The established fusion mode</returns>
        protected int TransitiveAnyFusion(int mode)
        {
            int m = s.RequestFusion(mode);
            fusionMode = m;
            return m;
        }

        /// <summary>
        /// Unless the mode contains the <see cref="FuseableHelper.BOUNDARY"/> flag,
        /// forward the mode request to the upstream IQueueSubscription and
        /// set the mode it returns.
        /// </summary>
        /// <param name="mode">The incoming fusion mode.</param>
        /// <returns>The established fusion mode</returns>
        protected int TransitiveBoundaryFusion(int mode)
        {
            if ((mode & FuseableHelper.BOUNDARY) != 0)
            {
                return FuseableHelper.NONE;
            }
            int m = s.RequestFusion(mode);
            fusionMode = m;
            return m;
        }
    }
}
