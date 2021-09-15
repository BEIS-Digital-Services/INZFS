using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notify.Interfaces;

namespace INZFS.Theme.Services
{
    public interface INotificationService
    {
        Task<bool> SendEmailAsync(string email, string templateId, Dictionary<string, object> personalisation = null);
        Task<bool> SendSmsAsync(string telephone, string templateId, Dictionary<string, object> personalisation = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly INotificationClient _notificationClient;

        public NotificationService(INotificationClient notificationClient)
        {
            _notificationClient = notificationClient;
        }


        public async Task<bool> SendEmailAsync(string email, string templateId, Dictionary<string, object> personalisation = null)
        {
            _notificationClient.SendEmail(email, templateId, personalisation);
            return await Task.FromResult(true);
        }

        public async Task<bool> SendSmsAsync(string telephone, string templateId, Dictionary<string, object> personalisation = null)
        {
            var response = _notificationClient.SendSms(
                telephone,
                templateId,
                personalisation
            );
            return await Task.FromResult(true);
        }
    }
}
