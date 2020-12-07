﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.WebApi.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class CurrencyController : ControllerBase
    {

        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("schedule")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetScheduleByName([FromQuery] string lecturerFirstName, string lecturerLastName)
        {
            if (string.IsNullOrEmpty(lecturerFirstName) || string.IsNullOrEmpty(lecturerLastName))
            {
                return BadRequest("Lecturer first and last name must be provided");
            }

            return Ok(new { lectureName = "TOP", lectureTime = DateTime.Now });
        }
    }
}
