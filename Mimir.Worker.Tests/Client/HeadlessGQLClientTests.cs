using System.Net;
using System.Text.Json;
using Libplanet.Crypto;
using Libplanet.Types.Tx;
using Mimir.Worker.Client;
using Moq;
using Moq.Protected;
using Xunit;

namespace Mimir.Worker.Tests.Client;

public class HeadlessGQLClientTests
{
    [Fact]
    public async Task GetTransactionStatusAsync_ReturnsCorrectResponse()
    {
        var txId = new TxId(new byte[32]);
        var expectedResponse = new GetTransactionStatusResponse
        {
            Transaction = new TransactionStatusResponse
            {
                TransactionResult = new TransactionResult
                {
                    TxStatus = TxStatus.SUCCESS,
                    ExceptionNames = new List<string?> { null, null, null, null }
                }
            }
        };

        var mockHttpHandler = new Mock<HttpMessageHandler>();
        mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new GraphQLResponse<GetTransactionStatusResponse>
                {
                    Data = expectedResponse
                }))
            });

        var httpClient = new HttpClient(mockHttpHandler.Object);
        var urls = new[] { new Uri("http://localhost:8080") };
        var client = new HeadlessGQLClient(httpClient, urls, null, null);

        var (response, jsonResponse) = await client.GetTransactionStatusAsync(txId, CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotNull(response.Transaction);
        Assert.NotNull(response.Transaction.TransactionResult);
        Assert.Equal(TxStatus.SUCCESS, response.Transaction.TransactionResult.TxStatus);
        Assert.Equal(4, response.Transaction.TransactionResult.ExceptionNames.Count);
        Assert.All(response.Transaction.TransactionResult.ExceptionNames, exception => Assert.Null(exception));
    }

    [Fact]
    public async Task GetTransactionStatusAsync_WithFailureStatus_ReturnsCorrectResponse()
    {
        var txId = new TxId(new byte[32]);
        var expectedResponse = new GetTransactionStatusResponse
        {
            Transaction = new TransactionStatusResponse
            {
                TransactionResult = new TransactionResult
                {
                    TxStatus = TxStatus.FAILURE,
                    ExceptionNames = new List<string?> { "Exception1", "Exception2" }
                }
            }
        };

        var mockHttpHandler = new Mock<HttpMessageHandler>();
        mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new GraphQLResponse<GetTransactionStatusResponse>
                {
                    Data = expectedResponse
                }))
            });

        var httpClient = new HttpClient(mockHttpHandler.Object);
        var urls = new[] { new Uri("http://localhost:8080") };
        var client = new HeadlessGQLClient(httpClient, urls, null, null);

        var (response, jsonResponse) = await client.GetTransactionStatusAsync(txId, CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotNull(response.Transaction);
        Assert.NotNull(response.Transaction.TransactionResult);
        Assert.Equal(TxStatus.FAILURE, response.Transaction.TransactionResult.TxStatus);
        Assert.Equal(2, response.Transaction.TransactionResult.ExceptionNames.Count);
        Assert.Equal("Exception1", response.Transaction.TransactionResult.ExceptionNames[0]);
        Assert.Equal("Exception2", response.Transaction.TransactionResult.ExceptionNames[1]);
    }

    [Fact]
    public async Task GetTransactionStatusAsync_WithNullResponse_ThrowsException()
    {
        var txId = new TxId(new byte[32]);

        var mockHttpHandler = new Mock<HttpMessageHandler>();
        mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new GraphQLResponse<GetTransactionStatusResponse>
                {
                    Data = null
                }))
            });

        var httpClient = new HttpClient(mockHttpHandler.Object);
        var urls = new[] { new Uri("http://localhost:8080") };
        var client = new HeadlessGQLClient(httpClient, urls, null, null);

        await Assert.ThrowsAsync<HttpRequestException>(() => 
            client.GetTransactionStatusAsync(txId, CancellationToken.None));
    }
} 