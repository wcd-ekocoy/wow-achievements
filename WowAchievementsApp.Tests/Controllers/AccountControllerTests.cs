using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using WowAchievementsApp.Controllers;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Collections.Generic;

namespace WowAchievementsApp.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IAuthenticationService> _mockAuthenticationService;
        private readonly Mock<IUrlHelper> _mockUrlHelper;
        private readonly Mock<IResponseCookies> _mockResponseCookies;
        private readonly Mock<HttpResponse> _mockHttpResponse;
        private readonly Mock<HttpRequest> _mockHttpRequest;
        private readonly AccountController _accountController;

        public AccountControllerTests()
        {
            _mockAuthenticationService = new Mock<IAuthenticationService>();
            _mockUrlHelper = new Mock<IUrlHelper>();
            _mockResponseCookies = new Mock<IResponseCookies>();
            _mockHttpResponse = new Mock<HttpResponse>();
            _mockHttpRequest = new Mock<HttpRequest>();

            _mockHttpRequest.SetupGet(x => x.Scheme).Returns("https");
            _mockHttpRequest.SetupGet(x => x.Host).Returns(new HostString("localhost"));
            _mockHttpResponse.SetupGet(x => x.Cookies).Returns(_mockResponseCookies.Object);
            _mockResponseCookies.Setup(x => x.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()));

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(x => x.RequestServices).Returns(new ServiceCollection().AddSingleton(_mockAuthenticationService.Object).BuildServiceProvider());
            mockHttpContext.Setup(x => x.Response).Returns(_mockHttpResponse.Object);
            mockHttpContext.Setup(x => x.Request).Returns(_mockHttpRequest.Object);
            mockHttpContext.Setup(x => x.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));

            _accountController = new AccountController()
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = mockHttpContext.Object,
                },
                Url = _mockUrlHelper.Object
            };

            _mockAuthenticationService.Setup(
                x => x.SignInAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<ClaimsPrincipal>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);
            _mockAuthenticationService.Setup(
                x => x.SignOutAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public void Login_RedirectsToHome_WhenAlreadyAuthenticated()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity("TestAuth");
            var authenticatedUser = new ClaimsPrincipal(claimsIdentity);
            var mockHttpContext = Mock.Get(_accountController.ControllerContext.HttpContext);
            mockHttpContext.Setup(x => x.User).Returns(authenticatedUser);

            // Act
            var result = _accountController.Login("us");

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
        }

        [Fact]
        public void Login_RedirectsToHome_WhenRegionIsNullOrEmpty()
        {
            // Arrange
            var mockHttpContext = Mock.Get(_accountController.ControllerContext.HttpContext);
            mockHttpContext.Setup(x => x.User).Returns(new ClaimsPrincipal()); // Not authenticated

            // Act
            var result = _accountController.Login((string)null!); // Cast to string to satisfy non-nullable parameter, using null-forgiving operator.

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
        }

        [Fact]
        public void Login_RedirectsToHome_WhenRegionIsEmptyString()
        {
            // Arrange
            var mockHttpContext = Mock.Get(_accountController.ControllerContext.HttpContext);
            mockHttpContext.Setup(x => x.User).Returns(new ClaimsPrincipal()); // Not authenticated

            // Act
            var result = _accountController.Login(string.Empty);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
        }

        [Fact]
        public void Login_ReturnsChallengeResult_WithCorrectProperties_WhenNotAuthenticatedAndRegionProvided()
        {
            // Arrange
            var mockHttpContext = Mock.Get(_accountController.ControllerContext.HttpContext);
            mockHttpContext.Setup(x => x.User).Returns(new ClaimsPrincipal()); // Not authenticated
            _mockUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/Account/LoginCallback");

            // Act
            var result = _accountController.Login("eu");

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result);
            Assert.Equal("BattleNet", challengeResult.AuthenticationSchemes.Single());
            Assert.NotNull(challengeResult.Properties);
            Assert.Equal("/Account/LoginCallback", challengeResult.Properties.RedirectUri);
            Assert.Equal("eu", challengeResult.Properties.Items["region"]);

            // Verify that a cookie for region is appended
            _mockResponseCookies.Verify(x => x.Append("region", "eu", It.IsAny<CookieOptions>()), Times.Once);
        }

        [Fact]
        public async Task Logout_SignsOutAndRedirectsToHome()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity("TestAuth");
            var authenticatedUser = new ClaimsPrincipal(claimsIdentity);
            var mockHttpContext = Mock.Get(_accountController.ControllerContext.HttpContext);
            mockHttpContext.Setup(x => x.User).Returns(authenticatedUser);

            // Act
            var result = await _accountController.Logout();

            // Assert
            _mockAuthenticationService.Verify(x => x.SignOutAsync(
                _accountController.ControllerContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<AuthenticationProperties>()), Times.Once);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
        }

        [Fact]
        public void LoginCallback_RedirectsToHome_WhenAuthorized()
        {
            // Arrange
            var claimsIdentity = new ClaimsIdentity("TestAuth");
            var authenticatedUser = new ClaimsPrincipal(claimsIdentity);
            var mockHttpContext = Mock.Get(_accountController.ControllerContext.HttpContext);
            mockHttpContext.Setup(x => x.User).Returns(authenticatedUser);

            // Act
            var result = _accountController.LoginCallback();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
        }
    }
}
