using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using LambdaFunctionFast;

namespace CrossCutting;

public class ApiKeyMiddleware : ILambdaMiddleware
{
    public async Task<APIGatewayProxyResponse> InvokeAsync(APIGatewayProxyRequest request, ILambdaContext context, Func<Task<APIGatewayProxyResponse>> next)
    {
        //if (request.Headers is null)
        //{
        //    return new APIGatewayProxyResponse
        //    {
        //        StatusCode = 401,
        //        Body = "API Key inválida",
        //        Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        //    };
        //}

        //if (!request.Headers.TryGetValue("x-api-key", out var apiKey) || apiKey != "minha-chave")
        //{
        //    return new APIGatewayProxyResponse
        //    {
        //        StatusCode = 401,
        //        Body = "API Key inválida",
        //        Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        //    };
        //}

        return await next();
    }
}

public class LoggingMiddleware : ILambdaMiddleware
{
    public async Task<APIGatewayProxyResponse> InvokeAsync(APIGatewayProxyRequest request, ILambdaContext context, Func<Task<APIGatewayProxyResponse>> next)
    {
        Console.WriteLine($"[Request] {request.HttpMethod} {request.Path}");
        Console.WriteLine("[Response] concluído");

        return await next();
    }
}