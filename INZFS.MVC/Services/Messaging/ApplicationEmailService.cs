using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notify.Interfaces;
using OrchardCore.Users;

namespace INZFS.MVC.Services
{
    public interface IApplicationEmailService
    {
        Task<bool> SendConfirmationEmailAsync(ClaimsPrincipal user, string applicationRef, string utl);
    }

    public class ApplicationEmailService : IApplicationEmailService
    {
        const string ApplicationSubmissionConfirmationTemplateKey = "Notification:ApplicationSubmissionConfirmationTemplate";

        private readonly INotificationClient _notificationClient;
        private readonly ILogger<ApplicationEmailService> _logger;
        private readonly UserManager<IUser> _userManager;
        private readonly IConfiguration _configuration;

        public ApplicationEmailService(
            INotificationClient notificationClient, 
            ILogger<ApplicationEmailService> logger, 
            UserManager<IUser> userManager,
            IConfiguration configuration)
        {
            _notificationClient = notificationClient;
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<bool> SendConfirmationEmailAsync(ClaimsPrincipal claimsPrincipal, string applicationRef, string url)
        {
            var user = await _userManager.GetUserAsync(claimsPrincipal);
            
            var templateId = _configuration.GetValue<string>(ApplicationSubmissionConfirmationTemplateKey);
            var email = await _userManager.GetEmailAsync(user);
            try
            {
                var parameters = new Dictionary<string, dynamic>();
                parameters.Add("reference", applicationRef);
                parameters.Add("link", url);

                //This should be going through a durable message queuing, which can be implemented in the next phase 
                _notificationClient.SendEmail(email, templateId, parameters);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unable to send email for template {templateId}", templateId);
                return await Task.FromResult(false);
            }
            
            return await Task.FromResult(true);
        }

        
    }
}
