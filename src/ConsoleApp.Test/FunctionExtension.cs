using LambdaFunctionFast;

namespace ConsoleApp.Test
{
    public abstract class Function<THandler, TRequest> : FunctionImpl<THandler, TRequest>
        where THandler : IHandler<TRequest>
        where TRequest : class, new()
    {
        protected Function() 
            : base(IoC.Services())
        {
        }
    }

    public abstract class Function<THandler, TRequest, TResponse> : FunctionImpl<THandler, TRequest, TResponse>
        where THandler : IHandler<TRequest, TResponse>
        where TRequest : class, new()
        where TResponse : class, new()
    {
        protected Function()
            : base(IoC.Services())
        {
        }
    }

    public abstract class FunctionWithoutRequest<THandler> : FunctionWithoutRequestImpl<THandler>
        where THandler : IHandlerWithoutRequest
    {
        protected FunctionWithoutRequest()
            : base(IoC.Services())
        {
        }
    }

    public abstract class FunctionWithoutRequest<THandler, TResponse> : FunctionWithoutRequestImpl<THandler, TResponse>
        where THandler : IHandlerWithoutRequest<TResponse>
        where TResponse : class, new()
    {
        protected FunctionWithoutRequest()
            : base(IoC.Services())
        {
        }
    }
}