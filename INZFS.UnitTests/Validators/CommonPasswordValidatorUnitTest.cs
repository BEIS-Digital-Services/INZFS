using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using INZFS.Theme;
using INZFS.Theme.Validators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OrchardCore.Users.Models;

namespace INZFS.UnitTests.Validators
{
    [TestClass]
    public class CommonPasswordValidatorUnitTest
    {
        private CommonPasswordValidatorBuilder builder;

        [TestInitialize]
        public void Initialize()
        {
            builder = new CommonPasswordValidatorBuilder();
        }


        [TestMethod]
        public async Task ValidateAsync_Should_Sucess_If_Password_Not_In_List()
        {
            var result = await builder.Build().ValidateAsync(null, new User(), "MyNotComPas!!");
            result.Succeeded.Should().BeTrue();
        }

        [TestMethod]
        public async Task ValidateAsync_Should_Fail_If_Password_Is_In_List()
        {
            var result = await builder.Build().ValidateAsync(null, new User(), "hello123");
            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        public async Task ValidateAsync_Should_Fail_If_Password_Is_Contain_In_The_List()
        {
            var result = await builder.Build().ValidateAsync(null, new User(), "helloA!");
            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        public async Task ValidateAsync_Should_Sucess_If_Password_Is_Contain_In_The_List_but_three_char_more()
        {
            var result = await builder.Build().ValidateAsync(null, new User(), "helloA123");
            result.Succeeded.Should().BeTrue();
        }

    }



    internal class CommonPasswordValidatorBuilder
    {
        public Mock<ICommonPasswordLists> commonPasswordListsMock;

        public CommonPasswordValidatorBuilder()
        {
            commonPasswordListsMock = new Mock<ICommonPasswordLists>();
        }

        public CommonPasswordValidator Build()
        {
            WithDefault();

            var sut = new CommonPasswordValidator(
                commonPasswordListsMock.Object);


            return sut;
        }

        public CommonPasswordValidatorBuilder WithDefault()
        {
            HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            hashSet.Add("hello123"); 
            hashSet.Add("hello");
            commonPasswordListsMock.Setup(m => m.GetPasswords()).Returns(hashSet);
            return this;
        }
    }


}
