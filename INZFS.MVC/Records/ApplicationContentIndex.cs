using System;
using YesSql.Indexes;

namespace INZFS.MVC.Records
{
    public class ApplicationContentIndex : MapIndex
    {
        public const int MaxContentTypeSize = 255;
        public const int MaxContentPartSize = 255;
        public const int MaxContentFieldSize = 255;
        public const int MaxOwnerSize = 255;
        public const int MaxAuthorSize = 255;
        public const int MaxDisplayTextSize = 255;

        public int DocumentId { get; set; }
        //public ApplicationStatus ApplicationStatus { get; set; }
        //public string ApplicationNumber { get; set; }
        public bool Published { get; set; }
        public bool Latest { get; set; }
        public string ContentItemId { get; set; }
        public string ContentItemVersionId { get; set; }
        public string ContentType { get; set; }
        public DateTime? ModifiedUtc { get; set; }
        public DateTime? PublishedUtc { get; set; }
        public DateTime? CreatedUtc { get; set; }
        public string Owner { get; set; }
        public string Author { get; set; }
        public string DisplayText { get; set; }
    }


    public class ApplicationContentIndexProvider : IndexProvider<ApplicationContent>
    {
        public override void Describe(DescribeContext<ApplicationContent> context)
        {
            context.For<ApplicationContentIndex>()
                .Map(contentItem =>
                {
                    var contentItemIndex = new ApplicationContentIndex
                    {
                        Latest = contentItem.Latest,
                        Published = contentItem.Published,
                        ContentType = contentItem.ContentType,
                        ContentItemId = contentItem.ContentItemId,
                        ContentItemVersionId = contentItem.ContentItemVersionId,
                        ModifiedUtc = contentItem.ModifiedUtc,
                        PublishedUtc = contentItem.PublishedUtc,
                        CreatedUtc = contentItem.CreatedUtc,
                        Owner = contentItem.Owner,
                        Author = contentItem.Author,
                        DisplayText = contentItem.DisplayText
                        //ApplicationNumber = contentItem.ApplicationNumber,
                        //ApplicationStatus = contentItem.ApplicationStatus
                    };

                    if (contentItemIndex.ContentType?.Length > ApplicationContentIndex.MaxContentTypeSize)
                    {
                        contentItemIndex.ContentType = contentItem.ContentType.Substring(0, ApplicationContentIndex.MaxContentTypeSize);
                    }

                    if (contentItemIndex.Owner?.Length > ApplicationContentIndex.MaxOwnerSize)
                    {
                        contentItemIndex.Owner = contentItem.Owner.Substring(0, ApplicationContentIndex.MaxOwnerSize);
                    }

                    if (contentItemIndex.Author?.Length > ApplicationContentIndex.MaxAuthorSize)
                    {
                        contentItemIndex.Author = contentItem.Author.Substring(0, ApplicationContentIndex.MaxAuthorSize);
                    }

                    if (contentItemIndex.DisplayText?.Length > ApplicationContentIndex.MaxDisplayTextSize)
                    {
                        contentItemIndex.DisplayText = contentItem.DisplayText.Substring(0, ApplicationContentIndex.MaxDisplayTextSize);
                    }

                    return contentItemIndex;
                });
        }
    }
}