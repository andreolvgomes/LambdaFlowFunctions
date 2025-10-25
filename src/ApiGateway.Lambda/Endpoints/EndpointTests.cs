using Amazon.Lambda.TestUtilities;
using ApiGateway.Lambda.Extensions;
using ApiGateway.Lambda.Utils;
using AWSLambda1;
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;

namespace ApiGateway.Lambda.Endpoints;

[HttpGet("function"), AllowAnonymous]
public class CGetAllProdutosFunctionEndpoint : EndpointWithoutRequest
{
    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = await new GetAllProdutosFunction().Run(await this.ToProxyRequest(), new TestLambdaContext());
        await Send.ResponseAsync(Json.Deserialize<object>(response.Body), statusCode: response.StatusCode);
    }
}