using SatisfactoryAccountingData.Client.Model.Observables;

namespace SatisfactoryAccountingData.Client.Model.Observable;

public abstract class BaseProducer : IProducer
{
    private readonly ReplaySubject<IItemRateList> _productManager = new (new ItemRateList());
    private readonly ReplaySubject<IItemRateList> _efficiencyManager = new(new ItemRateList());
    private readonly ReplaySubject<IItemRateList> _productEfficienciesManager = new(new ItemRateList());
    private IItemRateList _desiredProducts = new ItemRateList();
    private IReadOnlySet<IProducer> _sources = new HashSet<IProducer>();
    private readonly List<IDisposable> _sourceSubscriptions = new();

    public IObservable<IItemRateList> Products => _productManager;
    public IItemRateList Ingredients { get; protected set; } = new ItemRateList();
    public IObservable<IItemRateList> ProductEfficiencies => _productEfficienciesManager;
    public IObservable<IItemRateList> Efficiency => _efficiencyManager;
    public IItemRateList CurrentProducts => _productManager.CurrentValue;
    public IItemRateList CurrentProductEfficiencies => _productEfficienciesManager.CurrentValue;

    public IItemRateList DesiredProducts
    {
        get => _desiredProducts;
        set
        {
            _desiredProducts = value;
            OnInputChanged();
        }
    }

    public IReadOnlySet<IProducer> Sources
    {
        get => _sources;
        set
        {
            foreach (var subscription in _sourceSubscriptions)
            {
                subscription.Dispose();
            }
            _sourceSubscriptions.Clear();
            _sources = value;
            _sourceSubscriptions.AddRange(_sources.Select(source => source.Products.Subscribe(_ => OnInputChanged())));
            OnInputChanged();
        }
    }

    private bool _onInputChangedEnabled = true;

    protected virtual void OnInputChanged()
    {
        UpdateSourceConsumption();

        var products = ComputeProducts();

        if (!_onInputChangedEnabled) return;
        _onInputChangedEnabled = false;

        _productManager.CurrentValue = products;

        _onInputChangedEnabled = true;

        _productEfficienciesManager.CurrentValue = ComputeProductEfficiencies();
        // TODO figure out efficiency?
    }

    protected abstract void UpdateSourceConsumption();

    protected abstract IItemRateList ComputeProducts();

    protected abstract IItemRateList ComputeProductEfficiencies();
}