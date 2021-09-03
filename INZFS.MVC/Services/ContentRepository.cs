using INZFS.MVC.Forms;
using INZFS.MVC.Records;
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
        public Task<T> GetContentItemFromBagPart<T>(string contentToFilter, string userName) where T : ContentPart;
        public Task<List<ContentItem>> GetContentItemListFromBagPart(string userName);
        public Task<IEnumerable<ContentItem>> GetContentItems(Expression<Func<ContentItemIndex, bool>> predicate, string userName);

        public Task<ContentItem> GetContentItemById(string contentId);

        public Task<ApplicationContent> GetApplicationContent(string userName);
        public Task<ApplicationContent> GetApplicationContentById(int id);
    }
    public class ContentRepository : IContentRepository
    {
        private readonly ISession _session;
        public ContentRepository(ISession session)
        {
            _session = session;
        }
        public async Task<T> GetContentItemFromBagPart<T>(string contentToFilter, string userName) where T : ContentPart
        {
            var items = await GetContentItemListFromBagPart(userName);
            return GetContentPart<T>(items, contentToFilter);
        }

        public async Task<List<ContentItem>> GetContentItemListFromBagPart(string userName)
        {
            Expression<Func<ContentItemIndex, bool>> expression = index => index.ContentType == ContentTypes.INZFSApplicationContainer;
            var containerContentItems = await GetContentItems(expression, userName);
            var existingContainerContentItem = containerContentItems.FirstOrDefault();
            var applicationContainer = existingContainerContentItem?.ContentItem.As<BagPart>();
            return applicationContainer?.ContentItems;
        }

        public async Task<IEnumerable<ContentItem>> GetContentItems(Expression<Func<ContentItemIndex, bool>> predicate, string userName)
        {
            var query = _session.Query<ContentItem, ContentItemIndex>();
            query = query.With<ContentItemIndex>(predicate);
            query = query.With<ContentItemIndex>(x => x.Published);
            if (!string.IsNullOrEmpty(userName))
            {
                query = query.With<ContentItemIndex>(x => x.Author == userName);
            }

            return await query.ListAsync();
        }

        private T GetContentPart<T>(IEnumerable<ContentItem> contentItems, string contentToFilter) where T : ContentPart
        {
            var contentItem = contentItems?.FirstOrDefault(item => item.ContentType == contentToFilter);
            return contentItem?.ContentItem.As<T>();
        }

        public async Task<ContentItem> GetContentItemById(string contentId)
        {
            var query = _session.Query<ContentItem, ContentItemIndex>();
            query = query.With<ContentItemIndex>(index => index.ContentItemId == contentId.Trim());
            query = query.With<ContentItemIndex>(x => x.Published);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ApplicationContent> GetApplicationContent(string userName)
        {
            var query = _session.Query<ApplicationContent, ApplicationContentIndex>();
            query = query.With<ApplicationContentIndex>(x => x.Author == userName);
            return await query.FirstOrDefaultAsync();
        }
        public async Task<ApplicationContent> GetApplicationContentById(int id)
        {
            var query = _session.Query<ApplicationContent, ApplicationContentIndex>();
            query = query.With<ApplicationContentIndex>(x => x.DocumentId == id);
            return await query.FirstOrDefaultAsync();
        }
    }
}
