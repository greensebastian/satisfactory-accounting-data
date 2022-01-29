namespace SatisfactoryAccountingData.Configuration
{
    public class AuthConfiguration : IAppConfigurationSection
    {
        public string SectionName { get; } = "Auth";
        public string Key { get; set; }
    }
}
