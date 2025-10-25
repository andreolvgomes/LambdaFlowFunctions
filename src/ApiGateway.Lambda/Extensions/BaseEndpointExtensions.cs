using Amazon.Lambda.APIGatewayEvents;
using ApiGateway.Lambda.Utils;
using FastEndpoints;
using System.Text;

namespace ApiGateway.Lambda.Extensions;

public static class BaseEndpointExtensions
{
    private static async Task<string> ConvertStreamToString(Stream stream)
    {
        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        {
            return await reader.ReadToEndAsync();
        }
    }

    private static async Task<APIGatewayProxyRequest> ConvertHttpRequestToApiGatewayProxyRequest(HttpRequest httpRequest, object body = null)
    {
        // Extract necessary information from the HttpRequest
        // and create an APIGatewayProxyRequest
        // Adjust this based on your specific requirements
        var requestBody = await ConvertStreamToString(httpRequest.Body);
        if (body != null)
            requestBody = Json.Deserialize(body);

        var apiGatewayProxyRequest = new APIGatewayProxyRequest
        {
            Body = requestBody,
            Headers = httpRequest.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            QueryStringParameters = httpRequest.Query.ToDictionary(q => q.Key, q => q.Value.ToString()),
            PathParameters = httpRequest.RouteValues.ToDictionary(q => q.Key, q => q.Value.ToString()),
            HttpMethod = httpRequest.Method,
            Path = httpRequest.Path,
            // Add other necessary properties as needed
        };

        return apiGatewayProxyRequest;
    }

    public static Task<APIGatewayProxyRequest> ToProxyRequest(this BaseEndpoint endpoint, object body = null)
    {
        return ConvertHttpRequestToApiGatewayProxyRequest(endpoint.HttpContext.Request, body);
    }
}