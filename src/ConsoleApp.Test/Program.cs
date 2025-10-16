using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ConsoleApp.Test;
using LambdaFunctionFast;

public class Program
{
    public static async Task Main()
    {
        var apiGatewayProxyRequest = new APIGatewayProxyRequest();

        var func = new ProdutosFunctionWithoutRequest2();
        var respose = await func.Run(apiGatewayProxyRequest, null);

        var func2 = new ProdutosFunctionWithoutRequest();
        var respose2 = await func2.Run(apiGatewayProxyRequest, null);

        var func3 = new ProdutosFunction();
        var respose3 = await func3.Run(apiGatewayProxyRequest, null);

        var func4 = new ProdutosFunction2();
        var respose4 = await func4.Run(apiGatewayProxyRequest, null);
    }
}

public class ProdutosFunction : Function<ProdutosHandler, Produtos>;
public class ProdutosHandler : IHandler<Produtos>
{
    public void Handler(Produtos request, APIGatewayProxyRequest apiGateway, ILambdaContext context)
    {
    }
}

public class ProdutosFunction2 : Function<ProdutosHandler2, Produtos, List<Produtos>>;
public class ProdutosHandler2 : IHandler<Produtos, List<Produtos>>
{
    public List<Produtos> Handler(Produtos request, APIGatewayProxyRequest apiGateway, ILambdaContext context)
    {
        return new List<Produtos>();
    }
}

public class ProdutosFunctionWithoutRequest2 : FunctionWithoutRequest<ProdutosHandlerWithoutRequest2>;
public class ProdutosHandlerWithoutRequest2 : IHandlerWithoutRequest
{
    public void Handler(APIGatewayProxyRequest apiGateway, ILambdaContext context)
    {
    }
}

public class ProdutosFunctionWithoutRequest : FunctionWithoutRequest<ProdutosHandlerWithoutRequest, Produtos>;
public class ProdutosHandlerWithoutRequest : IHandlerWithoutRequest<Produtos>
{
    public Produtos Handler(APIGatewayProxyRequest apiGateway, ILambdaContext context)
    {
        return new Produtos()
        {
            Id = Guid.NewGuid()
        };
    }
}

public class Produtos
{
    public Guid Id { get; internal set; }
}