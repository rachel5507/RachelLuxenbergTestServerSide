using Microsoft.Extensions.Logging;
using Rachel.Models;
using Rachel.Services.interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rachel.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly ConcurrentDictionary<string, DateTime> _requestTimes = new ConcurrentDictionary<string, DateTime>();
        private readonly ILogger<EmailService> _logger;
        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }
        public EmailResponse GetTheEmailAndReceivedTime(EmailRequest request)
        {
            try
            {
                _requestTimes.AddOrUpdate(request.Email, DateTime.Now, (_, __) => DateTime.Now);

                return new EmailResponse { Email = request.Email, ReceivedTime = DateTime.Now };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetTheEmailAndReceivedTime failed");
                return new EmailResponse();
            }
        }
    }
}

