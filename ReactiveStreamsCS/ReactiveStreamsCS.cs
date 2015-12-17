using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveStreamsCS
{
    public interface IPublisher<out T>
    {
        void Subscribe(ISubscriber<T> s);
    }

    public interface ISubscriber<in T>
    {
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
}
