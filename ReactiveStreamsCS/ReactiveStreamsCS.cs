using System;

// <summary>
// Reactive Streams Specification for C#, http://www.reactive-streams.org/
// </summary>
namespace ReactiveStreamsCS
{
    /// <summary>
    /// An IPublisher is a provider of a potentially unbounded number of sequenced elements, 
    /// publishing them according to the demand received from its ISubscribers.
    /// </summary>
    /// <remarks>
    /// An IPublisher can serve multiple ISubscribers subscribed via Subscribe(ISubscriber) dynamically
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

        /// <summary>
        /// Data notification sent by the IPublisher in response to requests to ISubscription.Request(long).
        /// </summary>
        /// <param name="t">The element signaled.</param>
        void OnNext(T t);

        /// <summary>
        /// Failed terminal state.
        /// </summary>
        /// <remarks>
        /// No further events will be sent even if ISubscription.Request(long) is invoked again.
        /// </remarks>
        /// <param name="e">The Exception signaled.</param>
        void OnError(Exception e);

        /// <summary>
        /// Successful terminal state.
        /// </summary>
        /// <remarks>
        /// No further events will be sent even if ISubscription.Request(long) is invoked again.
        /// </remarks>
        void OnComplete();
    }

    /// <summary>
    /// An ISubscription represents a one-to-one lifecycle of an ISubscriber subscribing to an IPublisher.
    /// </summary>
    /// <remarks>
    /// It can only be used once by a single ISubscriber.
    /// 
    /// It is used to both signal desire for data and cancel demand (and allow resource cleanup).
    /// </remarks>
    public interface ISubscription
    {
        /// <summary>
        /// No events will be sent by a Publisher until demand is signaled via this method.
        /// </summary>
        /// <remarks>
        /// It can be called however often and whenever needed—but the outstanding cumulative demand must never exceed long.MaxValue.
        /// An outstanding cumulative demand of long.MaxValue may be treated by the IPublisher as "effectively unbounded".
        /// 
        /// Whatever has been requested can be sent by the IPublisher so only signal demand for what can be safely handled.
        /// 
        /// A IPublisher can send less than is requested if the stream ends but
        /// then must emit either ISubscriber.OnError(Exception) or ISubscriber.OnComplete().
        /// </remarks>
        /// <param name="n">The strictly positive number of elements to requests to the upstream IPublisher.</param>
        void Request(long n);

        /// <summary>
        /// Request the IPublisher to stop sending data and clean up resources.
        /// </summary>
        /// <remarks>
        /// Data may still be sent to meet previously signalled demand after calling cancel as this request is asynchronous.
        /// </remarks>
        void Cancel();
    }

    /// <summary>
    /// A Processor represents a processing stage—which is both an ISubscriber
    /// and an IPublisher and obeys the contracts of both.
    /// </summary>
    /// <typeparam name="T">The type of element signaled to the ISubscriber side.</typeparam>
    /// <typeparam name="R">The type of element signaled by the IPublisher side.</typeparam>
    public interface IProcessor<in T, out R> : IPublisher<R>, ISubscriber<T>
    {

    }

    /// <summary>
    /// An ISingle represents a provider of exactly one element or exactly one Exception to its
    /// ISingleSubscribers.
    /// </summary>
    /// <remarks>
    /// An ISingle can serve multiple ISingleSubscribers subscribed via Subscribe(ISingleSubscriber) dynamically
    /// at various points in time.
    /// 
    /// An ISingle signalling events to its ISingleSubscribers has to and will follow the following protocol:
    /// OnSubscribe (OnSuccess | OnError)?
    /// </remarks>
    /// <typeparam name="T">The type of the element signalled to the ISingleSubscriber</typeparam>
    public interface ISingle<out T>
    {
        /// <summary>
        /// Request the ISingle to signal an element or Exception.
        /// </summary>
        /// <param name="s">The ISingleSubscriber instance that will receive the element or Exception.</param>
        void Subscribe(ISingleSubscriber<T> s);
    }

    /// <summary>
    /// An ISingleSubscriber represents the consumer of exactly one element or exactly one Exception signalled
    /// from an ISingle provider.
    /// </summary>
    /// <remarks>
    /// Subscribing an ISingleSubscriber to multiple ISingle providers are generally discouraged and
    /// requires the implementor of ISingleSubscriber to handle merging signals from multiple sources
    /// in a thread-safe manner.
    /// </remarks>
    /// <typeparam name="T">The type of the element received from an ISingle.</typeparam>
    public interface ISingleSubscriber<in T>
    {
        /// <summary>
        /// Invoked by the ISingle after the ISingleSubscriber has been subscribed 
        /// to it via ISingle.Subscribe() method and receives an IDisposable instance 
        /// that can be used for cancelling the subscription.
        /// </summary>
        /// <param name="d">The IDisposable instance used for cancelling the subscription to the ISingle.</param>
        void OnSubscribe(IDisposable d);

        /// <summary>
        /// Invoked by the ISingle when it produced the exactly one element.
        /// </summary>
        /// <remarks>
        /// The call to this method is mutually exclusive with OnError.
        /// </remarks>
        /// <param name="t">The element produced by the source ISingle.</param>
        void OnSuccess(T t);

        /// <summary>
        /// Invoked by the ISingle when it produced the exaclty one Exception.
        /// </summary>
        /// <remarks>
        /// The call to this method is mutually exclusive with OnSucccess.
        /// </remarks>
        /// <param name="e">The Exception produced by the source ISingle</param>
        void OnError(Exception e);
    }

    /// <summary>
    /// An ISingleProcessor is a combination of an ISingleSubscriber and an ISingle, adhering
    /// to both contracts at the same time and may represent a single-use processing stage.
    /// </summary>
    /// <typeparam name="T">The type of element signaled to the ISingleSubscriber side.</typeparam>
    /// <typeparam name="R">The type of element signaled by the ISingle side.</typeparam>
    public interface ISingleProcessor<in T, out R> : ISingle<R>, ISingleSubscriber<T>
    {

    }

    /// <summary>
    /// An ICompletable represents the provider of exactly one completion or exactly one error signal
    /// and is mainly useful for composing side-effecting computation that don't generate values.
    /// </summary>
    /// <remarks>
    /// An ICompletable can serve multiple ICompletableSubscriber subscribed via Subscribe(ICompletableSubscriber) dynamically
    /// at various points in time.
    /// 
    /// An ICompletable signalling events to its ICompletableSubscribers has to and will follow the following protocol:
    /// OnSubscribe (OnError | OnComplete)?
    /// </remarks>
    public interface ICompletable
    {
        /// <summary>
        /// Request the ICompletable to signal a completion or an error signal.
        /// </summary>
        /// <param name="s">The ICompletableSubscriber instance that will receive either the completion
        /// signal or the error signal.</param>
        void Subscribe(ICompletableSubscriber s);
    }

    /// <summary>
    /// An ICompletableSubscriber represents the receiver of exaclty one completion or exactly one error signal
    /// from an ICompletable provider.
    /// </summary>
    /// <remarks>
    /// Subscribing an ICompletableSubscriber to multiple ICompletable providers are generally discouraged and
    /// requires the implementor of ICompletableSubscriber to handle merging signals from multiple sources
    /// in a thread-safe manner.
    /// </remarks>
    public interface ICompletableSubscriber
    {
        /// <summary>
        /// Invoked by the ICompletable after the ICompletableSubscriber has been subscribed 
        /// to it via ICompletable.Subscribe() method and receives an IDisposable instance 
        /// that can be used for cancelling the subscription.
        /// </summary>
        /// <param name="d">The IDisposable instance used for cancelling the subscription to the ICompletable.</param>
        void OnSubscribe(IDisposable d);


        /// <summary>
        /// Invoked by the ICompletable when it produced the exaclty one Exception.
        /// </summary>
        /// <remarks>
        /// The call to this method is mutually exclusive with OnComplete.
        /// </remarks>
        /// <param name="e">The Exception produced by the source ICompletable</param>
        void OnError(Exception e);

        /// <summary>
        /// Invoked by the ICompletable when it produced the exactly one completion signal.
        /// </summary>
        /// <remarks>
        /// The call to this method is mutually exclusive with OnError.
        /// </remarks>
        void OnComplete();
    }

    /// <summary>
    /// An ICompletableProcessor is a combination of an ICompletableSubscriber and an ICompletable, adhering
    /// to both contracts at the same time and may represent a single-use processing stage.
    /// </summary>
    public interface ICompletableProcessor : ICompletable, ICompletableSubscriber
    {

    }
}
