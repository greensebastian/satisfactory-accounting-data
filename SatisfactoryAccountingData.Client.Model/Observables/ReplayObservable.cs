namespace SatisfactoryAccountingData.Client.Model.Observables
{
    public class ReplayObservable<T> : IObservable<T> where T : class
    {
        private readonly HashSet<IObserver<T>> _observers = new();
        private T _value;

        public ReplayObservable(T initialValue)
        {
            _value = initialValue;
        }

        public T Value
        {
            get => _value;
            set
            {
                _value = value;
                foreach (var observer in _observers)
                {
                    observer.OnNext(Value);
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
