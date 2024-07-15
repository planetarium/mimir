using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Mimir.Options;
using MongoDB.Driver;

namespace Mimir.Services;

public class MongoDBCollectionService(IOptions<DatabaseOption> databaseOption)
{
    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        var database = GetDatabase();
        return database.GetCollection<T>(collectionName);
    }

    public IMongoDatabase GetDatabase()
    {
        var settings = MongoClientSettings.FromUrl(new MongoUrl(databaseOption.Value.ConnectionString));

        if (databaseOption.Value.CAFile is not null)
        {
            var caCertificate = new X509Certificate2(databaseOption.Value.CAFile);

            settings.SslSettings = new SslSettings
            {
                ServerCertificateValidationCallback = (sender, certificate, chain, errors) =>
                {
                    if (errors == SslPolicyErrors.None)
                        return true;

                    if ((errors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
                    {
                        foreach (var status in chain.ChainStatus)
                        {
                            if (
                                status.Status != X509ChainStatusFlags.UntrustedRoot
                                && status.Status != X509ChainStatusFlags.PartialChain
                            )
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                },
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };
        }
        var client = new MongoClient(settings);
        return client.GetDatabase(databaseOption.Value.Database);
    }
}
