namespace SatisfactoryAccountingData.Client.Model.Observables
{
    public static class ObservableExtensions
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onChange)
        {
            return source.Subscribe(new ActionObserver<T>(onChange, _ => {}, () => {}));
        }
    }
}
