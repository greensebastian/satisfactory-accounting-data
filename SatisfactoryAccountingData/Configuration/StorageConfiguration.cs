namespace SatisfactoryAccountingData.Configuration
{
    public class StorageConfiguration : IAppConfigurationSection
    {
        public string SectionName { get; } = "Storage";
        public string EndPointUrl { get; set; }
        public string AuthorizationKey { get; set; }
    }
}
