using System.Text.Json;

namespace CurrencyConverter.Contracts
{
    public class ErrorDetails
    {
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
            //return JsonConverter.Se(this, Formatting.Indented);
        }
    }
}
