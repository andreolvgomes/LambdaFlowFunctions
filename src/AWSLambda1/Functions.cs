using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Infra.Lambda;
using Infra.Repositories;
using LambdaFunctionFast;

namespace AWSLambda1;

public class ProdutosRequest { }
public class ProdutosResponse { }

public class ProdutosCreateFunction : Function<ProdutosCreateFunctionHandler, ProdutosRequest, ProdutosResponse> { }
public class ProdutosCreateFunctionHandler : IHandler<ProdutosRequest, ProdutosResponse>
{
    private readonly IRepository _repository;

    public ProdutosCreateFunctionHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResponseResult<ProdutosResponse>> Handler(ProdutosRequest request, APIGatewayProxyRequest apiGateway, ILambdaContext context)
    {
        return new ProdutosResponse();
    }
}

public class ProdutosGetFunction : FunctionWithoutRequest<ProdutosGetFunctionHandler, ProdutosResponse> { }
public class ProdutosGetFunctionHandler : IHandlerWithoutRequest<ProdutosResponse>
{
    private readonly IRepository _repository;

    public ProdutosGetFunctionHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResponseResult<ProdutosResponse>> Handler(APIGatewayProxyRequest apiGateway, ILambdaContext context)
    {
        return new ProdutosResponse();
    }
}

public class GetAllProdutosFunction : FunctionWithoutRequest<GetAllProdutosHandler, List<ProdutosResponse>> { }
public class GetAllProdutosHandler : IHandlerWithoutRequest<List<ProdutosResponse>>
{
    private readonly IRepository _repository;

    public GetAllProdutosHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<ResponseResult<List<ProdutosResponse>>> Handler(APIGatewayProxyRequest apiGateway, ILambdaContext context)
    {
        return new List<ProdutosResponse>();
    }
}