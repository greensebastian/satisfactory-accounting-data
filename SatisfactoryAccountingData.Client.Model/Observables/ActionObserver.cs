namespace SatisfactoryAccountingData.Client.Model.Observables;

public class ActionObserver<T> : IObserver<T>
{
    private readonly Action<T> _onNext;
    private readonly Action<Exception> _onError;
    private readonly Action _onComplete;

    public ActionObserver(Action<T> onNext, Action<Exception> onError, Action onComplete)
    {
        _onNext = onNext;
        _onError = onError;
        _onComplete = onComplete;
    }

    public virtual void OnCompleted()
    {
        _onComplete();
    }

    public virtual void OnError(Exception error)
    {
        _onError(error);
    }

    public virtual void OnNext(T value)
    {
        _onNext(value);
    }
}