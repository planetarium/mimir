using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using Lib9c.GraphQL.Types;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Mimir.GraphQL;
using Mimir.GraphQL.Queries;
using Mimir.GraphQL.Types;
using Mimir.MongoDB.Bson;
using Mimir.Services;
using Mimir.Options;
using Mimir.Repositories;
using Mimir.Repositories.AdventureBoss;

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
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IStateService, HeadlessStateService>();
builder.Services
    .AddSingleton<MongoDbService>()
    .AddSingleton(sp => sp.GetService<MongoDbService>()!.GetCollection<AvatarDocument>("avatar"))
    .AddGraphQLServer()
    .AddMimirGraphQLTypes()
    .AddType<AddressType>()
    .AddType<BencodexIValueType>()
    // .AddLib9cGraphQLTypes()
    // .AddMimirGraphQLTypes()
    // .AddMongoDbFiltering()
    // .AddMongoDbProjections()
    // .AddMongoDbSorting()
    .AddMongoDbPagingProviders()
    .AddErrorFilter<ErrorFilter>()
    .ModifyRequestOptions(requestExecutorOptions =>
    {
        requestExecutorOptions.IncludeExceptionDetails = true;
    });
// builder.Services.AddSingleton<ArenaRepository>();
// builder.Services.AddSingleton<TableSheetsRepository>();
// builder.Services.AddSingleton<MetadataRepository>();
// builder.Services.AddSingleton<AgentRepository>();
// builder.Services.AddSingleton<AvatarRepository>();
// builder.Services.AddSingleton<ActionPointRepository>();
// builder.Services.AddSingleton<DailyRewardRepository>();
// builder.Services.AddSingleton<InventoryRepository>();
// builder.Services.AddSingleton<AllRuneRepository>();
// builder.Services.AddSingleton<CollectionRepository>();
// builder.Services.AddSingleton<ItemSlotRepository>();
// builder.Services.AddSingleton<RuneSlotRepository>();
// builder.Services.AddSingleton<StakeRepository>();
// builder.Services.AddSingleton<ProductRepository>();
// builder.Services.AddSingleton<SeasonInfoRepository>();
builder.Services.AddControllers();
builder.Services.AddHeadlessGQLClient()
    .ConfigureHttpClient((provider, client) =>
    {
        var headlessStateServiceOption = provider.GetRequiredService<IOptions<HeadlessStateServiceOption>>();
        var option = headlessStateServiceOption.Value;
        client.BaseAddress = new Uri(option.Endpoint);

        if (option.JwtSecretKey is not null && option.JwtIssuer is not null)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(option.JwtSecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: option.JwtIssuer,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
        }
    });
builder.Services.AddCors();
builder.Services.AddHttpClient();

var app = builder.Build();
app.UseRouting();
app.MapGet("/", () => "Health Check");
app.MapControllers();
app.MapGraphQL();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseCors(policy =>
{
    policy.AllowAnyMethod();
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
});

app.Run();
