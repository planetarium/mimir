using System.Net;
using Libplanet.Crypto;
using Libplanet.Types.Tx;
using Mimir.Worker.Client;
using Moq;
using Moq.Protected;
using Xunit;
using System.Text;
using Newtonsoft.Json;
using Lib9c.Models.Block;

namespace Mimir.Worker.Tests.Client;

public class HeadlessGQLClientTests
{
    [Fact]
    public async Task GetTransactionStatusesAsync_ReturnsCorrectResponse()
    {
        var txIds = new List<TxId> { new TxId(new byte[32]), new TxId(new byte[32]) };
        var expectedResponse = new GetTransactionStatusesResponse
        {
            Transaction = new TransactionStatusResponse
            {
                TransactionResults = new List<TransactionResult>
                {
                    new TransactionResult
                    {
                        TxStatus = TxStatus.SUCCESS,
                        ExceptionNames = new List<string?> { null, null }
                    },
                    new TransactionResult
                    {
                        TxStatus = TxStatus.FAILURE,
                        ExceptionNames = new List<string?> { "Exception1" }
                    }
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
                Content = new StringContent(JsonConvert.SerializeObject(new GraphQLResponse<GetTransactionStatusesResponse>
                {
                    Data = expectedResponse
                }))
            });

        var httpClient = new HttpClient(mockHttpHandler.Object);
        var urls = new[] { new Uri("http://localhost:8080") };
        var client = new HeadlessGQLClient(httpClient, urls, null, null);

        var (response, jsonResponse) = await client.GetTransactionStatusesAsync(txIds, CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotNull(response.Transaction);
        Assert.NotNull(response.Transaction.TransactionResults);
        Assert.Equal(2, response.Transaction.TransactionResults.Count);
        Assert.Equal(TxStatus.SUCCESS, response.Transaction.TransactionResults[0].TxStatus);
        Assert.Equal(TxStatus.FAILURE, response.Transaction.TransactionResults[1].TxStatus);
    }

    [Fact]
    public async Task GetTransactionStatusesAsync_WithEmptyList_ReturnsCorrectResponse()
    {
        var txIds = new List<TxId>();
        var expectedResponse = new GetTransactionStatusesResponse
        {
            Transaction = new TransactionStatusResponse
            {
                TransactionResults = new List<TransactionResult>()
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
                Content = new StringContent(JsonConvert.SerializeObject(new GraphQLResponse<GetTransactionStatusesResponse>
                {
                    Data = expectedResponse
                }))
            });

        var httpClient = new HttpClient(mockHttpHandler.Object);
        var urls = new[] { new Uri("http://localhost:8080") };
        var client = new HeadlessGQLClient(httpClient, urls, null, null);

        var (response, jsonResponse) = await client.GetTransactionStatusesAsync(txIds, CancellationToken.None);

        Assert.NotNull(response);
        Assert.NotNull(response.Transaction);
        Assert.NotNull(response.Transaction.TransactionResults);
        Assert.Empty(response.Transaction.TransactionResults);
    }

    [Fact]
    public async Task GetTransactionStatusesAsync_WithNullResponse_ThrowsException()
    {
        var txIds = new List<TxId> { new TxId(new byte[32]) };

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
                Content = new StringContent(JsonConvert.SerializeObject(new GraphQLResponse<GetTransactionStatusesResponse>
                {
                    Data = null
                }))
            });

        var httpClient = new HttpClient(mockHttpHandler.Object);
        var urls = new[] { new Uri("http://localhost:8080") };
        var client = new HeadlessGQLClient(httpClient, urls, null, null);

        await Assert.ThrowsAsync<HttpRequestException>(() => 
            client.GetTransactionStatusesAsync(txIds, CancellationToken.None));
    }

    [Fact]
    public void TxStatus_Enum_Serialization_Deserialization_Works_Correctly()
    {
        var transactionResult = new TransactionResult
        {
            TxStatus = TxStatus.SUCCESS,
            ExceptionNames = new List<string?> { "TestException" }
        };

        var response = new GetTransactionStatusesResponse
        {
            Transaction = new TransactionStatusResponse
            {
                TransactionResults = new List<TransactionResult> { transactionResult }
            }
        };

        var json = JsonConvert.SerializeObject(response);
        var deserializedResponse = JsonConvert.DeserializeObject<GetTransactionStatusesResponse>(json);

        Assert.NotNull(deserializedResponse);
        Assert.NotNull(deserializedResponse.Transaction);
        Assert.NotNull(deserializedResponse.Transaction.TransactionResults);
        Assert.Single(deserializedResponse.Transaction.TransactionResults);
        Assert.Equal(TxStatus.SUCCESS, deserializedResponse.Transaction.TransactionResults[0].TxStatus);
    }

    [Theory]
    [InlineData("INVALID", TxStatus.INVALID)]
    [InlineData("STAGING", TxStatus.STAGING)]
    [InlineData("SUCCESS", TxStatus.SUCCESS)]
    [InlineData("FAILURE", TxStatus.FAILURE)]
    [InlineData("INCLUDED", TxStatus.INCLUDED)]
    public void TxStatus_Enum_Deserialization_From_String_Works_Correctly(string statusString, TxStatus expectedStatus)
    {
        var json = $"{{\"txStatus\":\"{statusString}\",\"exceptionNames\":[]}}";
        var transactionResult = JsonConvert.DeserializeObject<TransactionResult>(json);

        Assert.NotNull(transactionResult);
        Assert.Equal(expectedStatus, transactionResult.TxStatus);
    }

    [Fact]
    public void TxStatus_Enum_Serialization_To_String_Works_Correctly()
    {
        var transactionResult = new TransactionResult
        {
            TxStatus = TxStatus.FAILURE,
            ExceptionNames = new List<string?>()
        };

        var json = JsonConvert.SerializeObject(transactionResult);
        var expectedJson = "{\"txStatus\":\"FAILURE\",\"exceptionNames\":[]}";

        Assert.Equal(expectedJson, json);
    }
} 