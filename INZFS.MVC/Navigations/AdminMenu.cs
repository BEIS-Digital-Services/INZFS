using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Navigations
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            // We want to add our menus to the "admin" menu only.
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            // Adding our menu items to the builder.
            // The builder represents the full admin menu tree.
            builder
                .Add(S["Fund Manager"], S["Fund Manager"].PrefixPosition(), rootView => rootView
                   .Add(S["Applications"], S["Applications"].PrefixPosition(), GetApplicationsSearch => GetApplicationsSearch
                       .Action("GetApplicationsSearch", "Admin", new { area = "INZFS.MVC" })));

            return Task.CompletedTask;
        }
    }
}
