using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rachel.Models;
using Rachel.Services.interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rachel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {

        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public IActionResult GetTheEmailAndReceivedTime([FromBody] EmailRequest request)
        {
            try
            {
                return Ok(_emailService.GetTheEmailAndReceivedTime(request));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}