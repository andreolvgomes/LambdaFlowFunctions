using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Infra.Lambda;
using Infra.Repositories;
using LambdaFunctionFast;

namespace AWSLambda1;

public class GetAllProdutosFunction : FunctionWithoutRequest<GetAllProdutosHandler> { }
public class GetAllProdutosHandler : IHandlerWithoutRequest
{
    private readonly IRepository _repository;

    public GetAllProdutosHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResponseResult<Response>> Handler(APIGatewayProxyRequest apiGateway, ILambdaContext context)
    {
        return Result.Success;
    }
}
