using System.Collections.Generic;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Security;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests
{
    [Trait("Category", "Unit")]
    public class MemberReduceDetailsTests
    {
        [Theory]
        [InlineData(nameof(MemberResponseGroup.Full))]
        [InlineData(nameof(MemberResponseGroup.WithSecurityAccounts))]
        [InlineData(null)]
        public void Contact_ReduceDetails_RemovesPasswordHashAndSecurityStamp(string responseGroup)
        {
            var user = CreateUser();
            var contact = new Contact { Id = "contact", SecurityAccounts = new List<ApplicationUser> { user } };

            contact.ReduceDetails(responseGroup);

            AssertReduced(Assert.Single(contact.SecurityAccounts));
            Assert.Equal("password-hash", user.PasswordHash);
            Assert.Equal("security-stamp", user.SecurityStamp);
        }

        [Fact]
        public void Contact_ReduceDetails_WithoutSecurityAccountsGroup_RemovesAccounts()
        {
            var contact = new Contact { Id = "contact", SecurityAccounts = new List<ApplicationUser> { CreateUser() } };

            contact.ReduceDetails(nameof(MemberResponseGroup.Default));

            Assert.Null(contact.SecurityAccounts);
        }

        [Fact]
        public void Employee_ReduceDetails_RemovesPasswordHashAndSecurityStamp()
        {
            var employee = new Employee { Id = "employee", SecurityAccounts = new List<ApplicationUser> { CreateUser() } };

            employee.ReduceDetails(nameof(MemberResponseGroup.Full));

            AssertReduced(Assert.Single(employee.SecurityAccounts));
        }

        [Fact]
        public void Vendor_ReduceDetails_RemovesPasswordHashAndSecurityStamp()
        {
            var vendor = new Vendor { Id = "vendor", SecurityAccounts = new List<ApplicationUser> { CreateUser() } };

            vendor.ReduceDetails(nameof(MemberResponseGroup.Full));

            AssertReduced(Assert.Single(vendor.SecurityAccounts));
        }

        private static ApplicationUser CreateUser()
        {
            return new ApplicationUser { Id = "user", UserName = "user", PasswordHash = "password-hash", SecurityStamp = "security-stamp" };
        }

        private static void AssertReduced(ApplicationUser account)
        {
            Assert.Equal("user", account.Id);
            Assert.Equal("user", account.UserName);
            Assert.Null(account.PasswordHash);
            Assert.Null(account.SecurityStamp);
        }
    }
}
