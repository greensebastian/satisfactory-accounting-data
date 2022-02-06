using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatisfactoryAccountingData.Client.Model
{
    public class FactoryPlan
    {
        public Guid Id = Guid.NewGuid();
        public List<FactoryComponent> Components { get; set; } = new();
        public List<Guid> Sources { get; set; } = new();
    }

    public class FactoryComponent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ComponentType Type { get; set; }
        public List<Guid> Sources { get; set; } = new();
        public string RecipeName { get; set; } = string.Empty;
    }

    public enum ComponentType
    {
        Source,
        Builder
    }
}
