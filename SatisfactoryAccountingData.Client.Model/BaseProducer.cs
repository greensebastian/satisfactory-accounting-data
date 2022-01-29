using SatisfactoryAccountingData.Client.Model.Observables;
using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client.Model;

public abstract class BaseProducer : IProducer
{
    private readonly ReplayObservable<IItemRateList> _productManager = new (new ItemRateList());
    private readonly ReplayObservable<IItemRateList> _efficiencyManager = new(new ItemRateList());
    private readonly ReplayObservable<IItemRateList> _productionRatiosManager = new(new ItemRateList());
    private IItemRateList _desiredProducts = new ItemRateList();
    private IReadOnlySet<IProducer> _sources = new HashSet<IProducer>();
    private readonly List<IDisposable> _sourceSubscriptions = new();

    public IObservable<IItemRateList> Products => _productManager;
    public IObservable<IItemRateList> ProductionRatios => _productionRatiosManager;
    public IObservable<IItemRateList> Efficiency => _efficiencyManager;
    public IItemRateList CurrentProducts => _productManager.Value;
    public IItemRateList CurrentProductionRatios => _productionRatiosManager.Value;

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
        var productionRatios = ComputeProductionRatios();

        if (!_onInputChangedEnabled) return;
        _onInputChangedEnabled = false;

        _productManager.Value = products;
        _productionRatiosManager.Value = productionRatios;

        _onInputChangedEnabled = true;
        // TODO figure out efficiency?
    }

    protected abstract void UpdateSourceConsumption();

    protected abstract IItemRateList ComputeProducts();

    private IItemRateList ComputeProductionRatios()
    {
        var ratios = new ItemRateList();
        foreach (var desiredProduct in _desiredProducts)
        {
            var product =
                _productManager.Value.FirstOrDefault(product => product.ClassName == desiredProduct.ClassName);

            if (product == null)
            {
                ratios.Add(new ItemRate { Amount = 0, ClassName = desiredProduct.ClassName });
            }
            else
            {
                ratios.Add(new ItemRate
                    { Amount = product.Amount / desiredProduct.Amount, ClassName = desiredProduct.ClassName });
            }
        }

        return ratios;
    }
}