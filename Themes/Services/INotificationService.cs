using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(INotificationClient notificationClient, ILogger<NotificationService> logger)
        {
            _notificationClient = notificationClient;
            _logger = logger;
        }


        public async Task<bool> SendEmailAsync(string email, string templateId, Dictionary<string, object> personalisation = null)
        {
            try
            {
                //This should be going through a durable message queuing, which can be implemented in the next phase 
                _notificationClient.SendEmail(email, templateId, personalisation);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to send email for {email} for template {templateId}", email, templateId);
                return await Task.FromResult(false);
            }
            
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
