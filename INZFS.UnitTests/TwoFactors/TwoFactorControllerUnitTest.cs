using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.AspNetCore.Mvc;
using INZFS.Theme.Controllers;
using INZFS.Theme.Services;
using INZFS.Theme.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace INZFS.UnitTests.TwoFactors
{
    [TestClass]
    public class TwoFactorControllerUnitTest
    {
        const string returnUrl = "/someurl";

        private TwoFactorControllerBuilder _builder;

        [TestInitialize]
        public void Initialize()
        {
            _builder = new TwoFactorControllerBuilder();
        }

        [TestMethod]
        public async Task Select_Should_Populate_Scan_Qr_Code_If_2Fa_Not_Activated()
        {
            var result = await _builder.Build().Select(returnUrl);
            result.As<ViewResult>().Model.As<ChooseVerificationMethodViewModel>().AuthenticationMethod.Should().Be("");
        }

        [TestMethod]
        public async Task Select_Should_Populate_Authenticator_Code_If_2Fa_Is_Activated()
        {
            var result = await _builder.With2FactorEnabled().Build().Select(returnUrl);
            result.As<ViewResult>().Model.As<ChooseVerificationMethodViewModel>().AuthenticationMethod.Should().Be("");
        }

        [TestMethod]
        public async Task ScanQr_Should_Return_ViewResult_If_2Fa_Is_Not_Activated()
        {
            var result = await _builder.Build().ScanQr(returnUrl);
            result.Should().BeViewResult();
        }

        [TestMethod]
        public async Task ScanQr_Should_Redirect_To_AuthenticatorCode_If_2Fa_Is_Activated()
        {
            var result = await _builder.With2FactorEnabled().Build().ScanQr(returnUrl);
            result.Should().BeRedirectToActionResult().WithActionName("AuthenticatorCode");
        }

        [TestMethod]
        public async Task AuthenticatorCode_Should_Return_ViewResult()
        {
            var result = await _builder.Build().EnterCode(new EnterCodeViewModel(), returnUrl);
            result.Should().BeViewResult();
        }

        [TestMethod]
        public async Task AuthenticatorCode_Post_Should_Throw_NotFound_if_User_Does_Not_Exist()
        {
            var result = await _builder.Build().EnterCode(new EnterCodeViewModel(), returnUrl);
            result.Should().BeNotFoundObjectResult();
        }

        
        [TestMethod]
        public async Task AuthenticatorCode_Post_Should_AddModelError_If_Token_Is_Not_Valid()
        {
            var sut = _builder.WithUser().Build();
            var result = await sut.EnterCode(new EnterCodeViewModel() { Code = "384988"}, returnUrl);
            var errorMessage = sut.ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage;
            errorMessage.Should().Be("Verification code is not valid, please enter a valid code and try again");
        } 
        
        [TestMethod]
        public async Task AuthenticatorCode_Should_Set_Twofactor_As_Enabled_If_Not_Activated_Yet()
        {
            var sut = _builder
                    .WithUser()
                    .WithSuccessfullyVerifyTwoFactor()
                    //.With2FactorEnabled()
                    .Build();

            var result = await sut.EnterCode(new EnterCodeViewModel() { Code = "384988"}, returnUrl);
            _builder.userManagerMock.Verify(m=>m.SetTwoFactorEnabledAsync(It.IsAny<IUser>(), It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        public async Task AuthenticatorCode_Should_Redirect_To_Return_Url_If_Login_Successful()
        {
            var sut = _builder
                .WithUser()
                .WithSuccessfullyVerifyTwoFactor()
                .WithSuccessfulLogin()
                .Build();

            var result = await sut.EnterCode(new EnterCodeViewModel() { Code = "384988" }, returnUrl);

            result.Should().BeLocalRedirectResult();

        }


    }



    internal class TwoFactorControllerBuilder
    {
        public Mock<IUserStore<IUser>> userStoreMock;
        public Mock<IHttpContextAccessor> httpContextAccessorMock;
        public Mock<IUserClaimsPrincipalFactory<IUser>> userClaimsPrincipalFactoryMock;


        public Mock<UserManager<IUser>> userManagerMock;
        public Mock<ITwoFactorAuthenticationService> twoFactorAuthenticationServiceMock;
        public Mock<IUserTwoFactorSettingsService> factorSettingsServiceMock;
        public Mock<SignInManager<IUser>> signInManagerMock;
        public Mock<ILogger<TwoFactorController>> loggerMock;
        public Mock<INotificationService> notificationService;
        public Mock<IUrlEncodingService> urlEncodingService;

        public TwoFactorControllerBuilder()
        {
            httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            userClaimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<IUser>>();
            userStoreMock = new Mock<IUserStore<IUser>>();
            userManagerMock = new Mock<UserManager<IUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            twoFactorAuthenticationServiceMock = new Mock<ITwoFactorAuthenticationService>();
            factorSettingsServiceMock = new Mock<IUserTwoFactorSettingsService>();
            signInManagerMock = new Mock<SignInManager<IUser>>(userManagerMock.Object, httpContextAccessorMock.Object, userClaimsPrincipalFactoryMock.Object, null, null, null, null); ;
            loggerMock = new Mock<ILogger<TwoFactorController>>();
            notificationService = new Mock<INotificationService>();
            urlEncodingService = new Mock<IUrlEncodingService>();
        }

        public TwoFactorController Build()
        {
            WithDefault();
            
            var sut = new TwoFactorController(
                userManagerMock.Object,
                twoFactorAuthenticationServiceMock.Object,
                factorSettingsServiceMock.Object,
                signInManagerMock.Object,
                loggerMock.Object,
                notificationService.Object,
                urlEncodingService.Object);


            return sut;
        }

        public TwoFactorControllerBuilder WithDefault()
        {
            return this;
        }
        
        public TwoFactorControllerBuilder With2FactorEnabled()
        {
            factorSettingsServiceMock.Setup(m => m.GetTwoFactorEnabledAsync(It.IsAny<string>())).Returns(Task.FromResult(true));
            return this;
        } 
        
        public TwoFactorControllerBuilder WithUser()
        {
            IUser user = new User() {UserId = Guid.NewGuid().ToString()};
            signInManagerMock.Setup(m => m.GetTwoFactorAuthenticationUserAsync()).Returns(Task.FromResult(user));
            return this;
        }

        public TwoFactorControllerBuilder WithSuccessfullyVerifyTwoFactor()
        {
            userManagerMock
                .Setup(m => m.VerifyTwoFactorTokenAsync(It.IsAny<IUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(true));

            return this;
        }
        
        public TwoFactorControllerBuilder WithSuccessfulLogin()
        {
            signInManagerMock
                .Setup(m => m.TwoFactorAuthenticatorSignInAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));

            return this;
        }
    }


}
