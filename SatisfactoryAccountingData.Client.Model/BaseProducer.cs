using SatisfactoryAccountingData.Client.Model.Observables;
using SatisfactoryAccountingData.Shared.Model;

namespace SatisfactoryAccountingData.Client.Model;

public abstract class BaseProducer : IProducer
{
    private readonly ReplayObservable<IItemRateSet> _productManager = new (new ItemRateSet());
    private readonly ReplayObservable<IItemRateSet> _efficiencyManager = new(new ItemRateSet());
    private readonly ReplayObservable<IItemRateSet> _productionRatiosManager = new(new ItemRateSet());
    private IItemRateSet _desiredProducts = new ItemRateSet();
    private IReadOnlySet<IProducer> _sources = new HashSet<IProducer>();

    public IObservable<IItemRateSet> Products => _productManager;
    public IObservable<IItemRateSet> ProductionRatios => _productionRatiosManager;
    public IObservable<IItemRateSet> Efficiency => _efficiencyManager;
    public IItemRateSet CurrentProducts => _productManager.Value;

    public IItemRateSet DesiredProducts
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
            _sources = value;
            OnInputChanged();
        }
    }

    protected void OnInputChanged()
    {
        _productManager.Value = ComputeProducts();
        _productionRatiosManager.Value = ComputeProductionRatios();
        // TODO figure out efficiency?
    }

    protected abstract IItemRateSet ComputeProducts();

    private IItemRateSet ComputeProductionRatios()
    {
        var ratios = new ItemRateSet();
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