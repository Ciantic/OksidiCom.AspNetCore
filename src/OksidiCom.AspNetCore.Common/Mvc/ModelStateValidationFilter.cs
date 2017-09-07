using System;
using Microsoft.AspNetCore.Mvc.Filters;
using OksidiCom.AspNetCore.Common.ApiErrors;

namespace OksidiCom.AspNetCore.Common.Mvc
{
    public class ModelStateValidationFilter : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new ValidationError(context.ModelState).GetResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}