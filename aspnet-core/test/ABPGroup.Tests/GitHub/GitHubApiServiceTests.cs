using ABPGroup.Authentication.External.GitHub;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ABPGroup.Tests.GitHub
{
    public class GitHubApiServiceTests
    {
        // ── helpers ──────────────────────────────────────────────────────────

        private static IHttpClientFactory BuildFactory(HttpMessageHandler handler)
        {
            var client = new HttpClient(handler);
            var factory = new Mock<IHttpClientFactory>();
            factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
            return factory.Object;
        }

        private static HttpMessageHandler BuildHandler(
            string url, HttpStatusCode status, string json)
        {
            var mock = new Mock<HttpMessageHandler>();
            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains(url)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = status,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
            return mock.Object;
        }

        private static HttpMessageHandler BuildMultiHandler(
            string tokenJson, string userJson, string emailJson = null)
        {
            var mock = new Mock<HttpMessageHandler>();

            // Token exchange endpoint
            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri.ToString().Contains("login/oauth/access_token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(tokenJson, Encoding.UTF8, "application/json")
                });

            // User info endpoint (match before /emails so order matters; use Contains check)
            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri.ToString().EndsWith("/user") ||
                        r.RequestUri.ToString().Contains("api.github.com/user") &&
                        !r.RequestUri.ToString().Contains("/emails")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(userJson, Encoding.UTF8, "application/json")
                });

            // Emails endpoint
            if (emailJson != null)
            {
                mock.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(r =>
                            r.RequestUri.ToString().Contains("/user/emails")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(emailJson, Encoding.UTF8, "application/json")
                    });
            }

            return mock.Object;
        }

        // ── ExchangeCodeForAccessTokenAsync ──────────────────────────────────

        [Fact]
        public async Task ExchangeCodeForAccessToken_SuccessResponse_ReturnsToken()
        {
            var tokenJson = JsonSerializer.Serialize(new { access_token = "gho_testtoken123" });
            var factory = BuildFactory(BuildHandler("access_token", HttpStatusCode.OK, tokenJson));
            var sut = new GitHubApiService(factory);

            var result = await sut.ExchangeCodeForAccessTokenAsync(
                "auth-code", "client-id", "client-secret", "https://example.com/callback");

            Assert.Equal("gho_testtoken123", result);
        }

        [Fact]
        public async Task ExchangeCodeForAccessToken_HttpFailure_ReturnsNull()
        {
            var factory = BuildFactory(BuildHandler(
                "access_token", HttpStatusCode.BadRequest, "{}"));
            var sut = new GitHubApiService(factory);

            var result = await sut.ExchangeCodeForAccessTokenAsync(
                "bad-code", "client-id", "client-secret", "https://example.com/callback");

            Assert.Null(result);
        }

        [Fact]
        public async Task ExchangeCodeForAccessToken_EmptyAccessTokenInBody_ReturnsNull()
        {
            var tokenJson = JsonSerializer.Serialize(new { access_token = (string)null });
            var factory = BuildFactory(BuildHandler("access_token", HttpStatusCode.OK, tokenJson));
            var sut = new GitHubApiService(factory);

            var result = await sut.ExchangeCodeForAccessTokenAsync(
                "code", "id", "secret", "uri");

            Assert.Null(result);
        }

        // ── GetUserInfoAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task GetUserInfo_UserHasEmail_ReturnsUserInfoWithEmail()
        {
            var userJson = JsonSerializer.Serialize(new
            {
                id = 42L,
                login = "octocat",
                name = "Mona Lisa",
                email = "mona@github.com",
                avatar_url = "https://avatars.example.com/u/42"
            });

            // GitHubApiService creates ONE client and reuses it for both /user and /user/emails.
            // Since this user has an email, /user/emails is never called.
            var handler = BuildHandler("api.github.com/user", HttpStatusCode.OK, userJson);
            var factory = BuildFactory(handler);
            var sut = new GitHubApiService(factory);

            var result = await sut.GetUserInfoAsync("gho_token");

            Assert.NotNull(result);
            Assert.Equal(42L, result.Id);
            Assert.Equal("octocat", result.Login);
            Assert.Equal("Mona Lisa", result.Name);
            Assert.Equal("mona@github.com", result.Email);
        }

        [Fact]
        public async Task GetUserInfo_UserHasNoEmail_FetchesPrimaryEmailFromEmailsEndpoint()
        {
            var userJson = JsonSerializer.Serialize(new
            {
                id = 7L,
                login = "silent-dev",
                name = "Dev User",
                email = (string)null,
                avatar_url = "https://avatars.example.com/u/7"
            });

            var emailsJson = JsonSerializer.Serialize(new[]
            {
                new { email = "secondary@example.com", primary = false, verified = true },
                new { email = "primary@example.com",   primary = true,  verified = true },
                new { email = "unverified@example.com",primary = true,  verified = false }
            });

            var mock = new Mock<HttpMessageHandler>();

            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        r.RequestUri.ToString().Contains("/user/emails")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(emailsJson, Encoding.UTF8, "application/json")
                })
                .Verifiable();

            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r =>
                        !r.RequestUri.ToString().Contains("/emails")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(userJson, Encoding.UTF8, "application/json")
                });

            var factory = BuildFactory(mock.Object);
            var sut = new GitHubApiService(factory);

            var result = await sut.GetUserInfoAsync("gho_token");

            Assert.NotNull(result);
            Assert.Equal("primary@example.com", result.Email);
            mock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("/user/emails")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetUserInfo_NoPrimaryVerifiedEmail_ReturnsNullEmail()
        {
            var userJson = JsonSerializer.Serialize(new
            {
                id = 9L,
                login = "ghost",
                name = (string)null,
                email = (string)null,
                avatar_url = (string)null
            });

            var emailsJson = JsonSerializer.Serialize(new[]
            {
                new { email = "unverified@example.com", primary = true, verified = false }
            });

            var mock = new Mock<HttpMessageHandler>();

            mock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString().Contains("/user/emails")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(emailsJson, Encoding.UTF8, "application/json")
                });

            mock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => !r.RequestUri.ToString().Contains("/emails")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(userJson, Encoding.UTF8, "application/json")
                });

            var factory = BuildFactory(mock.Object);
            var sut = new GitHubApiService(factory);

            var result = await sut.GetUserInfoAsync("gho_token");

            Assert.NotNull(result);
            Assert.Null(result.Email);
        }
    }
}
