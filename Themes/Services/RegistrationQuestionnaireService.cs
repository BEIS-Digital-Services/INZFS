using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INZFS.Theme.Models;
using INZFS.Theme.Records;
using YesSql;

namespace INZFS.Theme.Services
{
    public interface IRegistrationQuestionnaireService
    {
        Task<bool> SaveOrganisationAsync(string userId, string organisationName);
    }

    public class RegistrationQuestionnaireService : IRegistrationQuestionnaireService
    {
        private readonly ISession _session;

        public RegistrationQuestionnaireService(ISession session)
        {
            _session = session;
        }

        public async Task<bool> SaveOrganisationAsync(string userId, string organisationName)
        {
            var questionnaire = await GetRegistrationQuestionnaire(userId);
            questionnaire.OrganisationName = organisationName;
            SaveQuestionnaire(questionnaire);
            return true;
        }

        private void SaveQuestionnaire(RegistrationQuestionnaire questionnaire)
        {
            _session.Save(questionnaire);
            //TODO: FM need to check how save works for YesSql with multi-threaded env
            //await _session.CommitAsync();

        }

        private async Task<RegistrationQuestionnaire> GetRegistrationQuestionnaire(string userId)
        {
            var query = _session.Query<RegistrationQuestionnaire, RegistrationQuestionnaireIndex>();
            query = query.With<RegistrationQuestionnaireIndex>(index => index.UserId == userId);
            var questionnaire = await query.FirstOrDefaultAsync();

            if (questionnaire == null)
            {
                var registrationQuestionnaire = new RegistrationQuestionnaire
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };

                _session.Save(registrationQuestionnaire);
                questionnaire = await query.FirstOrDefaultAsync();
            }

            return questionnaire;
        }
    }
}
