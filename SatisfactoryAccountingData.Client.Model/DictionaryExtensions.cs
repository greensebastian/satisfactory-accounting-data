namespace SatisfactoryAccountingData.Client.Model
{
    public static class DictionaryExtensions
    {
        public static void CreateOrAdd(this IDictionary<string, double> dict, string key, double value)
        {
            if (!dict.ContainsKey(key)) dict[key] = 0d;
            dict[key] += value;
        }

        public static double Subtract(this IDictionary<string, double> dict, string key, double value)
        {
            if (!dict.ContainsKey(key)) return 0;

            var toSubtract = Math.Min(value, dict[key]);
            dict[key] -= toSubtract;
            return toSubtract;
        }
    }
}
