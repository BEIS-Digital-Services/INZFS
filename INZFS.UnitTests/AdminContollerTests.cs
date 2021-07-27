using NUnit.Framework;
using INZFS;
using INZFS.MVC.Controllers;
using INZFS.MVC;
using Moq;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using OrchardCore.ContentManagement;
using System.Collections.Generic;
using OrchardCore.Flows.Models;

namespace INZFS.UnitTests
{
    public class AdminControllerTests
    {
        private  AdminController _controller;
        private Mock<IContentRepository> _contentRepository;

        [SetUp]
        public void Setup()
        {
            _contentRepository = new Mock<IContentRepository>();
            _controller = new AdminController(_contentRepository.Object);
        }

        [Test]
        public void Should_return_null_content_types_for_invalid_content_type_id()
        {
            var result = _controller.Application(It.IsAny<string>()).Result as ViewResult;

            result.Should().NotBeNull();
            
            var model = result.Model as List<ContentItem>;
            model.Should().BeNull();
        }

        [Test]
        public void Should_return_content_types_for_valid_content_type_id()
        {
            var contentItem = new ContentItem();
            contentItem.Weld<BagPart>();
            

            _contentRepository.Setup(cr => cr.GetContentItemById(It.IsAny<string>())).ReturnsAsync(contentItem);
            var result = _controller.Application(It.IsAny<string>()).Result as ViewResult;

            result.Should().NotBeNull();

            var model = result.Model as List<ContentItem>;
            model.Should().NotBeNull();
        }
    }
}
