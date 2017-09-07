using System;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;
using System.Threading.Tasks;
using OksidiCom.AspNetCore.Common.Db;

namespace OksidiCom.AspNetCore.Common.ApiErrors
{
    public class ApiErrorFilter : Attribute, IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ApiError)
            {
                context.Result = (context.Exception as ApiError).GetResult();
                context.Exception = null;
                context.ExceptionHandled = true;
            }
            else if (context.Exception is EntityNotFoundException)
            {
                context.Result = new NotFound().GetResult();
                context.Exception = null;
                context.ExceptionHandled = true;
            }
            else if (context.Exception is EntityListEmptyException)
            {
                context.Result = new NotFound().GetResult();
                context.Exception = null;
                context.ExceptionHandled = true;
            }
        }
    }
}