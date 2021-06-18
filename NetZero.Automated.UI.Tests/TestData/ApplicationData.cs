using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NetZero.Automated.UI.Tests.TestData
{
    public class ApplicationData
    {
        public class BasicInfo
        {
            private static string updatedText = "Updated Text";

            public static string CompanyName => "Auto Test Company";

            public static string CompanyNumber => "12324568";
            public static string ProjectName => "Auto Test Project";

            public static DateTime TurnoverDate => DateTime.ParseExact("10/12/2021", "dd/MM/yyyy", CultureInfo.InvariantCulture);
            public static string BriefProjectDescription => "Please give a brief summary description of the project. Please give a brief summary description of the project";

            public static bool ProjectCostEndBy2024 => true;
            public static List<string> CurrentFunding => new List<string>() { "Friends and Family", "Angel Investment", "Private Equity" };

            public static string UpdatedCompanyName => $"{updatedText} {CompanyName}";
            public static string UpdatedCompanyNumber => "90909009";
            public static string UpdatedProjectName => $"{updatedText} {ProjectName}";
            public static DateTime UpdatedTurnoverDate => DateTime.ParseExact("11/10/2022", "dd/MM/yyyy", CultureInfo.InvariantCulture);
            public static string UpdatedBriefProjectDescription => $"{updatedText} {BriefProjectDescription}";

            public static bool UpdatedProjectCostEndBy2024 => false;
            public static List<string> UpdatedCurrentFunding => new List<string>() { "Venture Capital", "Stock Market Flotation" };

        }
    }
}
