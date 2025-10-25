using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Reflection.Metadata;

namespace LambdaFunctionFast
{
    public interface IHandler<TRequest>
        where TRequest : class, new()
    {
        Task<ResponseResult<Response>> Handler(TRequest request, APIGatewayProxyRequest apiGateway, ILambdaContext context);
    }

    public interface IHandler<TRequest, TResponse>
        where TResponse : class, new()
    {
        Task<ResponseResult<TResponse>> Handler(TRequest request, APIGatewayProxyRequest apiGateway, ILambdaContext context);
    }

    public interface IHandlerWithoutRequest
    {
        Task<ResponseResult<Response>> Handler(APIGatewayProxyRequest apiGateway, ILambdaContext context);
    }

    public interface IHandlerWithoutRequest<TResponse>
        where TResponse : class, new()
    {
        Task<ResponseResult<TResponse>> Handler(APIGatewayProxyRequest apiGateway, ILambdaContext context);
    }

    public abstract class FunctionImpl<THandler, TRequest> : FunctionBase
        where THandler : IHandler<TRequest>
        where TRequest : class, new()
    {
        protected FunctionImpl(IServiceCollection serviceCollection)
            : base(serviceCollection)
        {
            BuildServiceProvider(typeof(THandler));
        }

        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest apiGateway, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                var middlewareRespononse = await RunMiddleware(apiGateway, context);
                if (middlewareRespononse != null)
                    return middlewareRespononse;

                var request = DeserializeObject<TRequest>(apiGateway.Body);

                var errors = TryValidateObject(request);
                if (errors.Any())
                    return BadRequest(errors.Select(e => e.MemberNames.FirstOrDefault() + ": " + e.ErrorMessage).ToList());

                var func = scope.ServiceProvider.GetRequiredService<THandler>();
                var response = await func.Handler(request, apiGateway, context);

                return ActionResult(response);
            }
        }
    }

    public abstract class FunctionImpl<THandler, TRequest, TResponse> : FunctionBase
        where THandler : IHandler<TRequest, TResponse>
        where TRequest : class, new()
        where TResponse : class, new()
    {
        protected FunctionImpl(IServiceCollection serviceCollection)
            : base(serviceCollection)
        {
            BuildServiceProvider(typeof(THandler));
        }

        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest apiGateway, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                var middlewareRespononse = await RunMiddleware(apiGateway, context);
                if (middlewareRespononse != null)
                    return middlewareRespononse;

                var request = DeserializeObject<TRequest>(apiGateway.Body);

                var errors = TryValidateObject(request);
                if (errors.Any())
                    return BadRequest(errors.Select(e => e.MemberNames.FirstOrDefault() + ": " + e.ErrorMessage).ToList());

                var func = scope.ServiceProvider.GetRequiredService<THandler>();
                var response = await func.Handler(request, apiGateway, context);

                return ActionResult(response);
            }
        }
    }

    public abstract class FunctionWithoutRequestImpl<THandler> : FunctionBase
        where THandler : IHandlerWithoutRequest
    {
        protected FunctionWithoutRequestImpl(IServiceCollection serviceCollection)
            : base(serviceCollection)
        {
            BuildServiceProvider(typeof(THandler));
        }

        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest apiGateway, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                var middlewareRespononse = await RunMiddleware(apiGateway, context);
                if (middlewareRespononse != null)
                    return middlewareRespononse;

                var func = scope.ServiceProvider.GetRequiredService<THandler>();
                var response = await func.Handler(apiGateway, context);

                return ActionResult(response);
            }
        }
    }

    public abstract class FunctionWithoutRequestImpl<THandler, TResponse> : FunctionBase
        where THandler : IHandlerWithoutRequest<TResponse>
        where TResponse : class, new()
    {
        protected FunctionWithoutRequestImpl(IServiceCollection serviceCollection)
            : base(serviceCollection)
        {
            BuildServiceProvider(typeof(THandler));
        }

        public async Task<APIGatewayProxyResponse> Run(APIGatewayProxyRequest apiGateway, ILambdaContext context)
        {
            using (var scope = CreateScope())
            {
                var middlewareRespononse = await RunMiddleware(apiGateway, context);
                if (middlewareRespononse != null)
                    return middlewareRespononse;

                var func = scope.ServiceProvider.GetRequiredService<THandler>();
                var response = await func.Handler(apiGateway, context);

                return ActionResult(response);
            }
        }
    }

    public abstract class FunctionBase
    {
        protected IServiceProvider _serviceProvider;
        private readonly IServiceCollection _serviceCollection;

        public FunctionBase(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public void BuildServiceProvider(Type functionImpl)
        {
            _serviceCollection.AddScoped(functionImpl);
            _serviceProvider = _serviceCollection.BuildServiceProvider();
        }

        public APIGatewayProxyResponse ActionResult<T>(ResponseResult<T> result)
        {
            if (result.HttpStatus is Success) return Ok();
            if (result.HttpStatus is Created) return Created();
            if (result.HttpStatus is Deleted) return NoContent();
            if (result.HttpStatus is Updated) return NoContent();

            if (result.HttpStatus is NotFound) return Response(result.Errors, httpStatusCode: HttpStatusCode.NotFound);
            if (result.HttpStatus is BadRequest) return Response(result.Errors, httpStatusCode: HttpStatusCode.BadRequest);

            if (result.Errors.Count > 0)
                return BadRequest(result.Errors);

            return Ok(result.Data);
        }

        protected IServiceScope CreateScope()
        {
            var scope = _serviceProvider.CreateScope();
            return scope;
        }

        protected async Task<APIGatewayProxyResponse> RunMiddleware(APIGatewayProxyRequest apiGateway, ILambdaContext context)
        {
            var pipeline = _serviceProvider.GetRequiredService<MiddlewarePipeline>();
            return await pipeline.ExecuteAsync(apiGateway, context);
        }

        protected APIGatewayProxyResponse Ok(object value = null)
        {
            return new()
            {
                Body = value != null ? SerializeObject(value) : null,
                StatusCode = (int)HttpStatusCode.OK,
                Headers = new Dictionary<string, string>() { ["Content-Type"] = "application/json" }
            };
        }

        protected APIGatewayProxyResponse BadRequest(string message)
        {
            return BadRequest(new List<string> { message });
        }

        protected APIGatewayProxyResponse BadRequest(List<string> messagesErrors, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new()
            {
                Body = SerializeObject(new { messages = messagesErrors }),
                StatusCode = (int)statusCode,
                Headers = new Dictionary<string, string>() { ["Content-Type"] = "application/json" }
            };
        }

        protected APIGatewayProxyResponse NoContent()
        {
            return Response(messagesErrors: null, HttpStatusCode.NoContent);
        }

        protected APIGatewayProxyResponse Created()
        {
            return Response(messagesErrors: null, HttpStatusCode.Created);
        }

        protected APIGatewayProxyResponse Response(List<string> messagesErrors = null, HttpStatusCode httpStatusCode = HttpStatusCode.NoContent)
        {
            return new()
            {
                Body = messagesErrors != null && messagesErrors.Count > 0 ? SerializeObject(new { messages = messagesErrors }) : null,
                StatusCode = (int)httpStatusCode,
                Headers = new Dictionary<string, string>() { ["Content-Type"] = "application/json" }
            };
        }

        protected List<ValidationResult> TryValidateObject<T>(T obj)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(obj, null, null);
            Validator.TryValidateObject(obj, context, results, true);
            return results;
        }

        protected string SerializeObject(object value)
        {
            var camelSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return JsonConvert.SerializeObject(value, settings: camelSettings);
        }

        protected T DeserializeObject<T>(string value)
        {
            var camelSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return JsonConvert.DeserializeObject<T>(value, settings: camelSettings);
        }
    }
}