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
using INZFS.MVC.Attributes;

namespace INZFS.MVC.Controllers
{
    [ApiKey]
    //[Authorize(Roles="Admin")]
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
        public async Task<ActionResult<List<int>>> GetListOfApplications()
        {
            List<int> allresults = new List<int>();

            var users = _getUserList.GetAllUsers();
            foreach (string user in users)
            {
                _applicationContent = await _contentRepository.GetApplicationContent(user);
                int applicationID = _applicationContent.Id;
                allresults.Add(applicationID);
            }
            return allresults;
        }

        [HttpGet("{id}")]
        public ActionResult<ApplicationContent> GetApplicationByApplicationId(int id)
        {
            var _applicationContent = _contentRepository.GetApplicationContentById(id).Result;
            
            return _applicationContent;
        }
    }
}
