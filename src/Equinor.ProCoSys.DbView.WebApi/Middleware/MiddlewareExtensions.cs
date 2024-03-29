﻿using Microsoft.AspNetCore.Builder;

namespace Equinor.ProCoSys.DbView.WebApi.Middleware
{
    public static class MiddlewareExtensions
    {
        public static void UseGlobalExceptionHandling(this IApplicationBuilder app) => app.UseMiddleware<GlobalExceptionHandler>();
    }
}
