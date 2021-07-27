using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notify.Interfaces;

namespace INZFS.Theme.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmail(string email, string templateId, Dictionary<string, object> personalisation = null);
    }

    public class EmailService : IEmailService
    {
        private readonly INotificationClient _notificationClient;

        public EmailService(INotificationClient notificationClient)
        {
            _notificationClient = notificationClient;
        }


        public async Task<bool> SendEmail(string email, string templateId, Dictionary<string, object> personalisation = null)
        {
            _notificationClient.SendEmail(email, templateId, personalisation);
            return await Task.FromResult(true);
        }
    }

}
