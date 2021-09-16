using INZFS.MVC.Forms;
using INZFS.MVC.Records;
using INZFS.MVC.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Flows.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using YesSql;

namespace INZFS.MVC
{
    public interface IContentRepository
    {
        public Task<ApplicationContent> GetApplicationContent(string userName);
        public Task<ApplicationContent> CreateApplicationContent(string userName);
        public Task UpdateStatus(string userName, ApplicationStatus newStatus);

    }
    public class ContentRepository : IContentRepository
    {
        private readonly ISession _session;
        private readonly IApplicationNumberGenerator _applicationNumberGenerator;
        public ContentRepository(ISession session, IApplicationNumberGenerator applicationNumberGenerator)
        {
            _session = session;
            _applicationNumberGenerator = applicationNumberGenerator;
        }

        public async Task UpdateStatus(string userName, ApplicationStatus newStatus)
        {
            var query = _session.Query<ApplicationContent, ApplicationContentIndex>();
            query = query.With<ApplicationContentIndex>(x => x.Author == userName);
            var content = await query.FirstOrDefaultAsync();
            if(content != null)
            {
                content.ApplicationStatus = newStatus;
                content.ModifiedUtc = DateTime.UtcNow;
                _session.Save(content);
            }
        }

        public async Task<ApplicationContent> GetApplicationContent(string userName)
        {
            var query = _session.Query<ApplicationContent, ApplicationContentIndex>();
            query = query.With<ApplicationContentIndex>(x => x.Author == userName);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<ApplicationContent> CreateApplicationContent(string userName)
        {
            var contentToSave = new ApplicationContent();
            contentToSave.Application = new Application();
            contentToSave.Author = userName;
            contentToSave.CreatedUtc = DateTime.UtcNow;
            contentToSave.ModifiedUtc = DateTime.UtcNow;
            contentToSave.ApplicationNumber = await GetNewApplicationNumber();
            contentToSave.ApplicationStatus = ApplicationStatus.InProgress;
            _session.Save(contentToSave);

            return contentToSave;

        }
        private async Task<string> GetNewApplicationNumber()
        {
            string applicationNumber;
            bool duplicateFound;
            IEnumerable<ApplicationContent> cachedRecords = null;
            do
            {
                applicationNumber = _applicationNumberGenerator.Generate(4);

                if(cachedRecords == null)
                {
                    var applicationNumberQuery = _session.Query<ApplicationContent, ApplicationContentIndex>();
                    cachedRecords = await applicationNumberQuery.ListAsync();

                }

                duplicateFound = cachedRecords.Any(record => record.ApplicationNumber == applicationNumber);

            } while (duplicateFound);

            return applicationNumber;
        }

    }
}
