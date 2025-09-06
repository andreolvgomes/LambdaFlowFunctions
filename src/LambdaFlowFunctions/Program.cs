using Amazon.Lambda.APIGatewayEvents;
using LambdaFlowFunctions;

public class Program
{
    public static void Main()
    {
        var apiGatewayProxyRequest = new APIGatewayProxyRequest();

        var func = new ProdutosFunctionWithoutRequest2();
        func.Run(apiGatewayProxyRequest, null);

        var func2 = new ProdutosFunctionWithoutRequest();
        func2.Run(apiGatewayProxyRequest, null);

        var func3 = new ProdutosFunction();
        func3.Run(apiGatewayProxyRequest, null);

        var func4 = new ProdutosFunction2();
        func4.Run(apiGatewayProxyRequest, null);
    }
}

public class ProdutosFunctionWithoutRequest2 : FunctionWithoutRequestImpl<ProdutosHandlerWithoutRequest2>;
public class ProdutosHandlerWithoutRequest2 : IHandlerWithoutRequest
{
    public void Handler()
    {
    }
}

public class ProdutosFunctionWithoutRequest : FunctionWithoutRequestImpl<ProdutosHandlerWithoutRequest, Produtos>;
public class ProdutosHandlerWithoutRequest : IHandlerWithoutRequest<Produtos>
{
    public Produtos Handler()
    {
        return new Produtos()
        {
            Id = Guid.NewGuid()
        };
    }
}

public class ProdutosFunction : FunctionImpl<ProdutosHandler, Produtos>;
public class ProdutosHandler : IHandler<Produtos>
{
    public void Handler(Produtos request)
    {
    }
}

public class ProdutosFunction2 : FunctionImpl<ProdutosHandler2, Produtos, List<Produtos>>;
public class ProdutosHandler2 : IHandler<Produtos, List<Produtos>>
{
    public List<Produtos> Handler(Produtos request)
    {
        return new List<Produtos>();
    }
}

public class Produtos
{
    public Guid Id { get; internal set; }
}