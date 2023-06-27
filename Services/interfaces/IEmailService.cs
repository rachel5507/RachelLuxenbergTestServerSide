using Rachel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rachel.Services.interfaces
{
    public interface IEmailService
    {
        EmailResponse GetTheEmailAndReceivedTime(EmailRequest request);
    }
}
