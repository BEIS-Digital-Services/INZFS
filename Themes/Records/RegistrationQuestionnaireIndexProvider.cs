using INZFS.Theme.Models;
using YesSql.Indexes;

namespace INZFS.Theme.Records
{
    public class RegistrationQuestionnaireIndexProvider : IndexProvider<RegistrationQuestionnaire>
    {
        public override void Describe(DescribeContext<RegistrationQuestionnaire> context)
        {
            context.For<RegistrationQuestionnaireIndex>()
                .Map(contentItem =>
                {
                    var contentItemIndex = new RegistrationQuestionnaireIndex
                    {
                        UserId = contentItem.UserId
                    };
                    return contentItemIndex;
                });
        }
    }
}