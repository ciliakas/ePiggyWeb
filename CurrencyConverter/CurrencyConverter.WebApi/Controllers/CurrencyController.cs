using System.Threading.Tasks;
using CurrencyConverter.Contracts.Outgoing;
using CurrencyConverter.Services.Mapper;
using CurrencyConverter.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.WebApi.Controllers
{
    public class CurrencyController : ControllerBase
    {

        private readonly ICurrencyService _scheduleService;

        public CurrencyController(ICurrencyService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet]
        [Route("schedule")]
        [ProducesResponseType(typeof(CurrencyDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetScheduleByName([FromQuery] string lecturerFirstName, string lecturerLastName)
        {
            if (string.IsNullOrEmpty(lecturerFirstName) || string.IsNullOrEmpty(lecturerLastName))
            {
                return BadRequest("Lecturer first and last name must be provided");
            }

            var schedule = _scheduleService.GetCurrencyByName(lecturerFirstName, lecturerLastName).ToDto();

            return Ok(schedule);
        }

        //[Microsoft.AspNetCore.Mvc.HttpGet]
        //[Microsoft.AspNetCore.Mvc.Route("schedule")]
        //[ProducesResponseType(200)]
        //[ProducesResponseType(400)]
        //[ProducesResponseType(404)]
        //public async Task<IActionResult> GetScheduleByName([FromQuery] string lecturerFirstName, string lecturerLastName)
        //{
        //    if (string.IsNullOrEmpty(lecturerFirstName) || string.IsNullOrEmpty(lecturerLastName))
        //    {
        //        return BadRequest("Lecturer first and last name must be provided");
        //    }

        //    if (lecturerFirstName.Equals("Erroras"))
        //    {
        //        throw new Exception();
        //    }

        //    return Ok(new { lectureName = "TOP", lectureTime = DateTime.Now });
        //}
    }
}
