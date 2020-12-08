using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CurrencyConverter.Contracts.Outgoing;
using CurrencyConverter.Services.Mapper;
using CurrencyConverter.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.WebApi.Controllers
{
    public class CurrencyController : ControllerBase
    {

        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet]
        [Route("currency")]
        [ProducesResponseType(typeof(CurrencyDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCurrencyByCode([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Currency code must be provided");
            }

            var currency = _currencyService.GetCurrencyByCode(code);

            if (currency is null)
            {
                return new BadRequestResult();
            }

            return Ok(currency.ToDto());
        }

        [HttpGet]
        [Route("list")]
        [ProducesResponseType(typeof(IList<CurrencyDto>), 200)]
        public async Task<IActionResult> GetCurrencyList()
        {
            var temp = _currencyService.GetCurrencyList();
            IList<CurrencyDto> list = temp.Select(currency => currency.ToDto()).ToList();
            return Ok(list);
        }

        [HttpGet]
        [Route("rate")]
        [ProducesResponseType(typeof(decimal), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCustomRate([FromQuery] string currencyCode1, string currencyCode2)
        {
            if (string.IsNullOrEmpty(currencyCode1) || string.IsNullOrEmpty(currencyCode2))
            {
                return BadRequest("Currency code must be provided");
            }

            var rate = _currencyService.GetCustomRate(currencyCode1, currencyCode2);

            if (rate is null)
            {
                return new BadRequestResult();
            }

            return Ok(rate);
        }
    }
}
