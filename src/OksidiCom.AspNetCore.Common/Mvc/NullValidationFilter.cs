using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OksidiCom.AspNetCore.Common.Mvc
{
    public class NullValidationFilter : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.Values.Contains(null))
            {
                context.Result = new BadRequestObjectResult(new { Error = "NULL_VALUE" });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}