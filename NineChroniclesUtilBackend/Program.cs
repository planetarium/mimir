using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using NineChroniclesUtilBackend.Services;
using NineChroniclesUtilBackend.Options;
using NineChroniclesUtilBackend.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<HeadlessStateServiceOption>(builder.Configuration.GetRequiredSection("StateService"));
builder.Services.Configure<DatabaseOption>(builder.Configuration.GetRequiredSection("Database"));
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SupportNonNullableReferenceTypes();
});
builder.Services.AddSingleton<IStateService, HeadlessStateService>();
builder.Services.AddSingleton<MongoDBCollectionService>();
builder.Services.AddSingleton<ArenaRankingRepository>();
builder.Services.AddSingleton<AvatarRepository>();
builder.Services.AddControllers();
builder.Services.AddHeadlessGQLClient()
    .ConfigureHttpClient((provider, client) =>
    {
        var headlessStateServiceOption = provider.GetRequiredService<IOptions<HeadlessStateServiceOption>>();
        client.BaseAddress = headlessStateServiceOption.Value.HeadlessEndpoint;

        if (headlessStateServiceOption.Value.JwtSecretKey is not null && headlessStateServiceOption.Value.JwtIssuer is not null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(headlessStateServiceOption.Value.JwtSecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: headlessStateServiceOption.Value.JwtIssuer,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        }
    });
builder.Services.AddCors();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();
app.UseCors(policy =>
{
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
});

app.MapGet("/", () => "Health Check");

app.Run();
