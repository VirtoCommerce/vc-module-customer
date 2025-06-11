using System;
using System.Threading.Tasks;
using FluentValidation;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.CustomerSampleModule.Web.Model;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security.Search;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CustomerSampleModule.Web.Services;

public class MemberService2(
    Func<IMemberRepository> repositoryFactory,
    IUserSearchService userSearchService,
    IEventPublisher eventPublisher,
    IPlatformMemoryCache platformMemoryCache,
    AbstractValidator<Member> memberValidator,
    ICountriesService countriesService,
    ISettingsManager settingsManager,
    IUniqueNumberGenerator uniqueNumberGenerator)
    : MemberService(
        repositoryFactory,
        userSearchService,
        eventPublisher,
        platformMemoryCache,
        memberValidator,
        countriesService)
{
    public override Task SaveChangesAsync(Member[] members)
    {
        foreach (var member in members)
        {
            if (member is Contact2 contact2 &&
                string.IsNullOrEmpty(contact2.WebContactId))
            {
                var webContactIdTemplate = settingsManager.GetValue<string>(ModuleConstants.Settings.General.WebContactIdTemplate);
                contact2.WebContactId = uniqueNumberGenerator.GenerateNumber(webContactIdTemplate);
            }
        }

        return base.SaveChangesAsync(members);
    }
}
