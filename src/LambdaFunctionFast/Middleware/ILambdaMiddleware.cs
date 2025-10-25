﻿using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace LambdaFunctionFast.Middleware
{
    public interface ILambdaMiddleware
    {
        Task<ResponseResult<object>> InvokeAsync(APIGatewayProxyRequest request, ILambdaContext context, Func<Task<ResponseResult<object>>> next);
    }
}