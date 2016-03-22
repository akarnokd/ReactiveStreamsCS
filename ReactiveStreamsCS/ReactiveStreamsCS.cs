using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Reactive Streams Specification for C#, http://www.reactive-streams.org/
/// </summary>
namespace ReactiveStreamsCS
{
    /// <summary>
    /// An IPublisher is a provider of a potentially unbounded number of sequenced elements, 
    /// publishing them according to the demand received from its ISubscribers.
    /// </summary>
    /// <remarks>
    /// A Publisher can serve multiple ISubscribers subscribed via Subscribe(ISubscriber) dynamically
    /// at various points in time.
    /// </remarks>
    /// <typeparam name="T">The type of element signaled.</typeparam>
    public interface IPublisher<out T>
    {
        /// <summary>
        /// Request the IPublisher to start streaming data.
        /// </summary>
        /// <remarks>
        /// This is a "factory method" and can be called multiple times, each time starting a new Subscription.
        /// 
        /// Each ISubscription will work for only a single ISubscriber.
        /// 
        /// A ISubscriber should only subscribe once to a single IPublisher.
        /// 
        /// If the IPublisher rejects the subscription attempt or otherwise fails it will
        /// signal the error via ISubscriber.OnError.
        /// </remarks>
        /// <param name="s">The ISubscriber that will consume signals from this IPublisher.</param>
        void Subscribe(ISubscriber<T> s);
    }

    /// <summary>
    /// Will receive call to OnSubscribe(ISubscription) once after passing an 
    /// instance of ISubscriber to IPublisher.Subscribe(ISubscriber). 
    /// </summary>
    /// <remarks>
    /// No further notifications will be received until ISubscription.Request(long) is called. 
    ///
    /// After signaling demand: 
    /// • One or more invocations of OnNext(Object) up to the maximum number defined by ISubscription.request(long)
    /// • Single invocation of OnError(Throwable) or ISubscriber.OnComplete() which signals a terminal state after which no further events will be sent.
    ///
    /// Demand can be signaled via ISubscription.request(long) whenever the ISubscriber 
    /// instance is capable of handling more.
    /// </remarks>
    /// <typeparam name="T">The type of element signaled.</typeparam>
    public interface ISubscriber<in T>
    {
        /// <summary>
        /// Invoked after calling IPublisher.Subscribe(ISubscriber). 
        /// </summary>
        /// <remarks>
        /// No data will start flowing until ISubscription.Request(long) is invoked.
        /// 
        /// It is the responsibility of this ISubscriber instance to call ISubscription.Request(long) whenever more data is wanted.
        /// 
        /// The IPublisher will send notifications only in response to ISubscription.Request(long).
        /// </remarks>
        /// <param name="s">The ISubscription that allows requesting data via ISubscription.Request(long).</param>
        void OnSubscribe(ISubscription s);

        void OnNext(T t);

        void OnError(Exception e);

        void OnComplete();
    }

    public interface ISubscription
    {
        void Request(long n);

        void Cancel();
    }

    public interface IProcessor<in T, out R> : IPublisher<R>, ISubscriber<T>
    {

    }

    public interface ISingle<out T>
    {
        void Subscribe(ISingleSubscriber<T> s);
    }

    public interface ISingleSubscriber<in T>
    {
        void OnSubscribe(IDisposable d);

        void OnSuccess(T t);

        void OnError(Exception e);
    }

    public interface ISingleProcessor<in T, out R> : ISingle<R>, ISingleSubscriber<T>
    {

    }

    public interface ICompletable
    {
        void Subscribe(ICompletableSubscriber s);
    }

    public interface ICompletableSubscriber
    {
        void OnSubscribe(IDisposable d);

        void OnError(Exception e);

        void OnComplete();
    }

    public interface ICompletableProcessor : ICompletable, ICompletableSubscriber
    {

    }
}
