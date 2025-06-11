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

public class MemberService2 : MemberService
{
    private readonly ISettingsManager _settingsManager;
    private readonly IUniqueNumberGenerator _uniqueNumberGenerator;

    public MemberService2(
            Func<IMemberRepository> repositoryFactory,
            IUserSearchService userSearchService,
            IEventPublisher eventPublisher,
            IPlatformMemoryCache platformMemoryCache,
            AbstractValidator<Member> memberValidator,
            ICountriesService countriesService,
            ISettingsManager settingsManager,
            IUniqueNumberGenerator uniqueNumberGenerator)
        : base(
            repositoryFactory,
            userSearchService,
            eventPublisher,
            platformMemoryCache,
            memberValidator,
            countriesService
            )
    {
        _settingsManager = settingsManager;
        _uniqueNumberGenerator = uniqueNumberGenerator;
    }

    public override Task SaveChangesAsync(Member[] members)
    {
        foreach (var member in members)
        {
            if (member is Contact2 contact2 &&
                string.IsNullOrEmpty(contact2.WebContactId))
            {
                var WebContactIdTemplate = _settingsManager.GetValue<string>(ModuleConstants.Settings.General.WebContactIdTemplate);

                contact2.WebContactId = _uniqueNumberGenerator.GenerateNumber(WebContactIdTemplate);
            }
        }

        return base.SaveChangesAsync(members);
    }
}
