using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FluentAssertions;
using INZFS.Theme.Models;
using INZFS.Theme.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace INZFS.UnitTests.TwoFactors
{
    [TestClass]
    public class TwoFactorAuthenticationServiceUnitTest
    {
        private TwoFactorAuthenticationServiceBuilder _builder;

        [TestInitialize]
        public void Initialize()
        {
            _builder = new TwoFactorAuthenticationServiceBuilder();
        }


        [TestMethod]
        public async Task GetSharedKeyAndQrCodeUriAsync_Should_Return_A_Shared_Code()
        {
            var sharedCode = "B7FQ7OU5GGUIWCH3MQBI5ZRBV6XEAQ6M";
            var expectedSharedCode = "b7fq 7ou5 ggui wch3 mqbi 5zrb v6xe aq6m";

            var result = await _builder
                .WithAuthenticatorKey(sharedCode)
                .Build()
                .GetSharedKeyAndQrCodeUriAsync(new OrchardCore.Users.Models.User());

            result.SharedKey.Should().Be(expectedSharedCode);
        }

        [TestMethod]
        public async Task GetSharedKeyAndQrCodeUriAsync_Should_Generate_A_New_Shared_Code_When_There_Is_Not_One()
        {
            var sharedCode = "";

            var result = await _builder
                .WithAuthenticatorKey(sharedCode)
                .Build()
                .GetSharedKeyAndQrCodeUriAsync(new OrchardCore.Users.Models.User());

            _builder.userManagerMock.Verify(m=>m.ResetAuthenticatorKeyAsync(It.IsAny<IUser>()), Times.Once);

        }


    }



    internal class TwoFactorAuthenticationServiceBuilder
    {
        public Mock<IUserStore<IUser>> userStoreMock;
        public Mock<UserManager<IUser>> userManagerMock;
        public Mock<UrlEncoder> urlEncoderMock;
        public Mock<IOptions<TwoFactorOption>> optionsMock;

        public TwoFactorAuthenticationServiceBuilder()
        {
            userStoreMock = new Mock<IUserStore<IUser>>();
            userManagerMock = new Mock<UserManager<IUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            urlEncoderMock = new Mock<UrlEncoder>();
            optionsMock = new Mock<IOptions<TwoFactorOption>>();
        }

        public TwoFactorAuthenticationService Build()
        {
            WithDefault();

            var sut = new TwoFactorAuthenticationService(
                userManagerMock.Object,
                urlEncoderMock.Object,
                optionsMock.Object);


            return sut;
        }

        public TwoFactorAuthenticationServiceBuilder WithDefault()
        {
            optionsMock.SetupGet(p => p.Value).Returns(new TwoFactorOption() {Status = 0, AccountName = "APP"});
            return this;
        }

        public TwoFactorAuthenticationServiceBuilder WithAuthenticatorKey(string expectedSharedCode)
        {
            userManagerMock.Setup(m => m.GetAuthenticatorKeyAsync(It.IsAny<IUser>())).Returns(Task.FromResult(expectedSharedCode));
            return this;
        }
    }


}
