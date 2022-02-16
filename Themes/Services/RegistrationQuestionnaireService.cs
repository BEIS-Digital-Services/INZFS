using System;
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
            await SaveQuestionnaire(questionnaire);
            return true;
        }

        private async Task SaveQuestionnaire(RegistrationQuestionnaire questionnaire)
        {
            _session.Save(questionnaire);
            await _session.SaveChangesAsync();

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
                await _session.SaveChangesAsync();
                query = _session.Query<RegistrationQuestionnaire, RegistrationQuestionnaireIndex>();
                query = query.With<RegistrationQuestionnaireIndex>(index => index.UserId == userId);
                questionnaire = await query.FirstOrDefaultAsync();
            }

            return questionnaire;
        }
    }
}
