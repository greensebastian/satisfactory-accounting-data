namespace SatisfactoryAccountingData.Client.Model.Observables;

public class ActionObserver<T> : IObserver<T>
{
    private readonly Action<T> _onChange;

    public ActionObserver(Action<T> onChange)
    {
        _onChange = onChange;
    }

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnNext(T value)
    {
        _onChange(value);
    }
}