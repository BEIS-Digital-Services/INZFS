using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using INZFS.MVC.Services.UserService;

namespace INZFS.MVC.Controllers
{
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
        public async Task<ActionResult<List<string>>> GetAllApplicationContent()
        {
            List<string> allresults = new List<string>();

            var users = _getUserList.GetAllUsers();
            foreach (string user in users)
            {

                _applicationContent = await _contentRepository.GetApplicationContent(user);
                var applicationContentList = _applicationContent.Fields;
                foreach (Field field in applicationContentList)
                {
                    var fielddata = field.Data;
                    var fieldName = field.Name;
                    allresults.Add(fieldName);
                    allresults.Add(fielddata);
                }
                
            }


            //List<string> resultsList = new List<string>();
            //foreach (var section in _applicationDefinition.Application.Sections)
            //{
            //    foreach (var page in section.Pages)
            //    {
            //        var question = page.Question;
            //        var answer = _applicationContent?.Fields.Find(question => question.Name == page.Name);
            //        if (answer == null)
            //        {
            //            var questionPair = question + ": " + "";
            //            resultsList.Add(questionPair);

            //        }
            //        else
            //        {
            //            var response = answer.Data;
            //            var questionPair = question + ": " + response;
            //            resultsList.Add(questionPair);
            //        }

            //    }
            //}


            //return resultsList;
            return allresults;

        }
    }
}
