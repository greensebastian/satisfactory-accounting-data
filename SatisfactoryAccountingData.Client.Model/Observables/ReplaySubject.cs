namespace SatisfactoryAccountingData.Client.Model.Observables
{
    public interface IReplayObservable<out T> : IObservable<T> where T : class
    {
        T CurrentValue { get; }
    }

    public class ReplaySubject<T> : IReplayObservable<T> where T : class
    {
        private readonly HashSet<IObserver<T>> _observers = new();
        private T _currentValue;

        public ReplaySubject(T initialCurrentValue)
        {
            _currentValue = initialCurrentValue;
        }

        public T CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                foreach (var observer in _observers)
                {
                    observer.OnNext(CurrentValue);
                }
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
            return new Unsubscriber(_observers, observer);
        }

        public void Complete()
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
                _observers.Remove(observer);
            }
        }

        private class Unsubscriber : IDisposable
        {
            private readonly ISet<IObserver<T>> _observers;
            private readonly IObserver<T> _observer;

            public Unsubscriber(ISet<IObserver<T>> observers, IObserver<T> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observers.Contains(_observer)) _observers.Remove(_observer);
            }
        }
    }
}
