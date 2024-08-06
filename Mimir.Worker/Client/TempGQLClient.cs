using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Mimir.Worker.Client
{
    public class TempGQLClient
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _url;
        private readonly string? _issuer;
        private readonly string? _secret;

        public TempGQLClient(Uri url, string? issuer, string? secret)
        {
            _httpClient = new HttpClient();
            _url = url;
            _issuer = issuer;
            _secret = secret;
        }

        private string GenerateJwtToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                expires: DateTime.UtcNow.AddMinutes(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<GetAccountDiffsResponse?> GetAccountDiffsAsync(
            long baseIndex,
            long changedIndex,
            string accountAddress
        )
        {
            if (_issuer is not null && _secret is not null)
            {
                var token = GenerateJwtToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );
            }

            var query =
                @"
                query GetAccountDiffs($baseIndex: Long!, $changedIndex: Long!, $accountAddress: Address!) {
                    accountDiffs(baseIndex: $baseIndex, changedIndex: $changedIndex, accountAddress: $accountAddress) {
                        path
                        baseState
                        changedState
                    }
                }";

            var request = new GraphQLRequest
            {
                query = query,
                variables = new Dictionary<string, object>
                {
                    { "baseIndex", baseIndex },
                    { "changedIndex", changedIndex },
                    { "accountAddress", accountAddress }
                }
            };

            var jsonRequest = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_url, content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var graphQLResponse = JsonSerializer.Deserialize<
                GraphQLResponse<GetAccountDiffsResponse>
            >(jsonResponse);

            return graphQLResponse?.data;
        }
    }
}
