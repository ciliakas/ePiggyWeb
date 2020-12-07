using Newtonsoft.Json;

namespace CurrencyConverter.Contracts
{
    public class ErrorDetails
    {
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
