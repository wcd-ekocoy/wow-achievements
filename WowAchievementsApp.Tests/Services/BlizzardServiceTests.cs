using Xunit;
using Moq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using WowAchievementsApp.Services;
using System.Threading.Tasks;
using WowAchievementsApp.Models;
using System.Net;
using Newtonsoft.Json;
using Moq.Protected;
using System.Threading;

namespace WowAchievementsApp.Tests.Services
{
    public class BlizzardServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly BlizzardService _blizzardService;

        public BlizzardServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["BlizzardApi:BaseUrl"]).Returns("https://{0}.api.blizzard.com");

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _blizzardService = new BlizzardService(_httpClient, _mockConfiguration.Object);
        }

        [Fact]
        public async Task GetProfileSummary_ReturnsProfile_OnSuccess()
        {
            // Arrange
            var accessToken = "test_token";
            var region = "us";
            var expectedProfile = new SelfProfile { Id = 1, AccountId = 123 };
            var responseContent = JsonConvert.SerializeObject(expectedProfile);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            // Act
            var result = await _blizzardService.GetProfileSummary(accessToken, region);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProfile.Id, result.Id);
            Assert.Equal(expectedProfile.AccountId, result.AccountId);
        }

        [Fact]
        public async Task GetProfileSummary_ReturnsNull_OnUnsuccessfulResponse()
        {
            // Arrange
            var accessToken = "test_token";
            var region = "us";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Error")
                });

            // Act
            var result = await _blizzardService.GetProfileSummary(accessToken, region);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCharacterAchievements_ReturnsAchievements_OnSuccess()
        {
            // Arrange
            var accessToken = "test_token";
            var region = "us";
            var realmSlug = "sargeras";
            var characterName = "charactername";
            var expectedAchievements = new CharacterAchievements { TotalPoints = 100 };
            var responseContent = JsonConvert.SerializeObject(expectedAchievements);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            // Act
            var result = await _blizzardService.GetCharacterAchievements(accessToken, region, realmSlug, characterName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedAchievements.TotalPoints, result.TotalPoints);
        }

        [Fact]
        public async Task GetCharacterAchievements_ReturnsNull_OnUnsuccessfulResponse()
        {
            // Arrange
            var accessToken = "test_token";
            var region = "us";
            var realmSlug = "sargeras";
            var characterName = "charactername";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Error")
                });

            // Act
            var result = await _blizzardService.GetCharacterAchievements(accessToken, region, realmSlug, characterName);

            // Assert
            Assert.Null(result);
        }
    }
}
