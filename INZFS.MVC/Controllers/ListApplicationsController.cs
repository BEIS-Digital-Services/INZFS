using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using INZFS.MVC.Services.UserService;
using Microsoft.AspNetCore.Authorization;

namespace INZFS.MVC.Controllers
{
    [Authorize(Roles="Admin")]
    [Route("api/listapplications")]
    [ApiController]
    public class ListApplicationsController : Controller
    {
        private readonly IContentRepository _contentRepository;
        private readonly ApplicationDefinition _applicationDefinition;
        private ApplicationContent _applicationContent;
        private readonly UserManager<IUser> _userManager;
        private readonly IUserService _getUserList;
        public ListApplicationsController(IContentRepository contentRepository, ApplicationDefinition applicationDefinition, UserManager<IUser> userManager, IUserService getUserList)
        {

            _contentRepository = contentRepository;
            _applicationDefinition = applicationDefinition;
            _getUserList = getUserList;

            _userManager = userManager;
        }
        [HttpGet]
        public async Task<ActionResult<List<ApplicationContent>>> GetListOfApplications()
        {
            List<ApplicationContent> allresults = new List<ApplicationContent>();

            var users = _getUserList.GetAllUsers();
            foreach (string user in users)
            {
                _applicationContent = await _contentRepository.GetApplicationContent(user);
                allresults.Add(_applicationContent);
            }
            return allresults;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<List<ApplicationContent>>> GetApplicationByApplicationId(int ApplicationId)
        {
            List<ApplicationContent> allresults = new List<ApplicationContent>();

            var users = _getUserList.GetAllUsers();
            foreach (string user in users)
            {
                _applicationContent = await _contentRepository.GetApplicationContent(user);
                allresults.Add(_applicationContent);
            }
            return allresults;
        }
    }
}
