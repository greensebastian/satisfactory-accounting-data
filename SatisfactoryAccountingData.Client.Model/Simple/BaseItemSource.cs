namespace SatisfactoryAccountingData.Client.Model.Simple
{
    public abstract class BaseItemSource : ISimpleItemSource
    {
        public IDictionary<IItemRateList, IItemRateList> Products { get; } =
            new Dictionary<IItemRateList, IItemRateList>();

        public IItemRateList LeftoverProducts => LeftoverItems;

        public IItemRateList AddRequest(IItemRateList product)
        {
            Requests.Add(product);
            RecomputeProducts();
            return Products.ContainsKey(product) ? Products[product] : new ItemRateList();
        }

        public void RemoveRequest(IItemRateList product)
        {
            if (!Requests.Contains(product)) return;
            Requests.Remove(product);
            RecomputeProducts();
        }

        public void AddSource(ISimpleItemSource source)
        {
            if (Sources.Contains(source)) return;
            Sources.Add(source);
            RecomputeProducts();
        }

        public void RemoveSource(ISimpleItemSource source)
        {
            if (!Sources.Contains(source)) return;
            Sources.Remove(source);
            RecomputeProducts();
        }

        protected void RecomputeProducts()
        {
            Products.Clear();
            LeftoverItems.Clear();
            ComputeResult();
        }

        protected ItemRateList LeftoverItems { get; } = new();

        protected List<IItemRateList> Requests { get; } = new();

        protected List<ISimpleItemSource> Sources { get; } = new();

        protected void AddToProduct(IItemRateList request, IItemRateList result)
        {
            if (Products.ContainsKey(request))
            {
                var newTotal = Products[request].AsDictionary();
                foreach (var item in result)
                {
                    newTotal.CreateOrAdd(item.ClassName, item.Amount);
                }

                Products[request] = new ItemRateList(newTotal);
            }
            else
            {
                Products[request] = result;
            }
        }
        
        protected abstract void ComputeResult();

        protected IItemRateList MissingFromRequest(IItemRateList request)
        {
            if (!Products.ContainsKey(request)) return request.DeepCopy();

            var produced = Products[request];
            var missing = new ItemRateList().AsDictionary();
            foreach (var requestedItem in request)
            {
                var producedAmount = produced.FirstOrDefault(item => item.ClassName == requestedItem.ClassName)?.Amount ?? 0;
                if (producedAmount >= requestedItem.Amount) continue;

                if (!missing.ContainsKey(requestedItem.ClassName)) missing[requestedItem.ClassName] = 0d;
                missing[requestedItem.ClassName] += requestedItem.Amount - producedAmount;
            }

            return new ItemRateList(missing);
        }
    }
}
