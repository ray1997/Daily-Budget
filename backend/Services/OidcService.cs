using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using DailyBudget.Api.Dtos;
using Microsoft.Extensions.Options;

namespace DailyBudget.Api.Services;

public sealed class OidcService(HttpClient httpClient, IOptions<OidcOptions> options)
{
    private readonly OidcOptions _options = options.Value;

    public async Task<OidcProfile> RedeemCodeAsync(OidcLoginRequest request, CancellationToken cancellationToken)
    {
        var discovery = await httpClient.GetFromJsonAsync<OidcDiscovery>(NormalizeAuthority(_options.Authority) + ".well-known/openid-configuration", cancellationToken)
            ?? throw new InvalidOperationException("OIDC discovery document was empty.");

        var tokenParameters = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _options.ClientId,
            ["code"] = request.Code,
            ["redirect_uri"] = request.RedirectUri,
            ["code_verifier"] = request.CodeVerifier
        };
        if (!string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            tokenParameters["client_secret"] = _options.ClientSecret;
        }
        using var form = new FormUrlEncodedContent(tokenParameters);

        var tokenResponse = await httpClient.PostAsync(discovery.TokenEndpoint, form, cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<OidcTokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("OIDC token response was empty.");

        using var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, discovery.UserInfoEndpoint);
        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var userInfoResponse = await httpClient.SendAsync(userInfoRequest, cancellationToken);
        userInfoResponse.EnsureSuccessStatusCode();
        var profile = await userInfoResponse.Content.ReadFromJsonAsync<OidcProfile>(cancellationToken)
            ?? throw new InvalidOperationException("OIDC user info response was empty.");

        if (string.IsNullOrWhiteSpace(profile.Subject))
        {
            throw new InvalidOperationException("OIDC provider did not return a subject.");
        }

        return profile;
    }

    private static string NormalizeAuthority(string authority) => authority.TrimEnd('/') + "/";

    private sealed record OidcDiscovery(
        [property: JsonPropertyName("token_endpoint")] string TokenEndpoint,
        [property: JsonPropertyName("userinfo_endpoint")] string UserInfoEndpoint);

    private sealed record OidcTokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
}

public sealed record OidcProfile(
    [property: JsonPropertyName("sub")] string Subject,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("preferred_username")] string? PreferredUsername);
