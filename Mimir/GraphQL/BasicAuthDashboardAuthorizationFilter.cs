using System;
using System.Text;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Mimir.Options;

namespace Mimir.GraphQL;

public class BasicAuthDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly string _username;
    private readonly string _password;

    public BasicAuthDashboardAuthorizationFilter(IOptions<HangfireOption> config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));
            
        _username = config.Value.Username;
        _password = config.Value.Password;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var authHeader = httpContext.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
        {
            return false;
        }

        var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
        var credentials = Encoding
            .UTF8.GetString(Convert.FromBase64String(encodedCredentials))
            .Split(':');

        return credentials.Length == 2
            && credentials[0] == _username
            && credentials[1] == _password;
    }
} 