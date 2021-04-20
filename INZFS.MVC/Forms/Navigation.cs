using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Forms
{
    public interface INavigation
    {
        Page GetNextPage(string currentPage);
        Page GetNextPageByContentType(string contentType);
        Page GetPreviousPage(string currentPage);
        Page GetPreviousPageByContentType(string contentType);
        Page GetPage(string currentPage);
        IList<Page> PageList();
    }
    public class Navigation : INavigation
    {
        private readonly IList<Page> _pages;
        public Navigation()
        {
            _pages = new List<Page>();
            _pages.Add(new ContentPage { Name = "project-summary", ContentType = "ProjectSummaryPart" });
            _pages.Add(new ContentPage { Name = "project-details", ContentType = "ProjectDetailsPart" });
            _pages.Add(new ContentPage { Name = "org-funding", ContentType = "OrgFundingPart" });
            _pages.Add(new Page { Name = "summary" });
            _pages.Add(new ContentPage { Name = "proposal-written-details", ContentType = "ProjectProposalDetails" });
            _pages.Add(new ViewPage { Name = "upload-project-plan", ViewName = "ProjectPlan", ContentType = "ApplicationDocument" });
            _pages.Add(new ContentPage { Name = "project-experience", ContentType = "ProjectExperience" });
            _pages.Add(new ViewPage { Name = "experience-and-skills", ViewName = "ExperienceSkills", ContentType = "ApplicationDocument" });
            _pages.Add(new Page { Name = "proposal-written-summary" });
            _pages.Add(new ContentPage { Name = "finance-turnover", ContentType = "FinanceTurnover" });
            _pages.Add(new ContentPage { Name = "finance-balance-sheet", ContentType = "FinanceBalanceSheet" });
            _pages.Add(new ContentPage { Name = "finance-recover-vat", ContentType = "FinanceRecoverVat" });
            _pages.Add(new ContentPage { Name = "finance-barriers", ContentType = "FinanceBarriers" });
            _pages.Add(new Page { Name = "proposal-finance-summary" });

        }

        public IList<Page> PageList()
        {
            return _pages;
        }

        public Page GetPage(string currentPage)
        {
           return _pages.FirstOrDefault(predicate => predicate.Name.Equals(currentPage));
        }

        public Page GetNextPage(string currentPage)
        {
            var page = _pages.FirstOrDefault(predicate => predicate.Name.Equals(currentPage));
            return GetNextPage(page);
        }

        public Page GetNextPageByContentType(string contentType)
        {
            var contentPages = _pages.OfType<ContentPage>();
            var page = contentPages.FirstOrDefault(predicate => predicate.ContentType.Equals(contentType));
            return GetNextPage(page);
        }

        private Page GetNextPage(Page page)
        {
            if (page == null)
            {
                return page;
            }
            var index = _pages.IndexOf(page);
            if (index + 1 == _pages.Count)
            {
                // Last page
                return null;
            }
            return _pages[index + 1];
        }

        public Page GetPreviousPage(string currentPage)
        {
            var page = _pages.FirstOrDefault(predicate => predicate.Name.Equals(currentPage));
            return GetPreviousPage(page);
        }

        public Page GetPreviousPageByContentType(string contentType)
        {
            var contentPages = _pages.OfType<ContentPage>();
            var page = contentPages.FirstOrDefault(predicate => predicate.ContentType.Equals(contentType));
            return GetPreviousPage(page);
        }
        private Page GetPreviousPage(Page page)
        {
            if (page == null)
            {
                throw new System.ArgumentException("Invalid page name");
            }
            var index = _pages.IndexOf(page);
            if (index - 1 == -1)
            {
                // First Page page
                return null;
            }
            return _pages[index - 1];
        }
    }
}
