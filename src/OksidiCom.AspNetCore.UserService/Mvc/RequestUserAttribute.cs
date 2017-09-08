using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OksidiCom.AspNetCore.Common.ApiErrors;
using OksidiCom.AspNetCore.UserService.Models;

namespace OksidiCom.AspNetCore.UserService.Mvc
{
    /// <summary>
    /// Request user modelbinder from UserManager automatically
    /// </summary>
    public class RequestUserModelBinder : IModelBinder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public RequestUserModelBinder(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var user = await _userManager.GetUserAsync(bindingContext.ActionContext.HttpContext.User);
            if (user == null)
            {
                throw new NotAuthorized();
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Success(user);
            }
        }
    }

    /// <summary>
    /// Get the request user from UserManager automatically
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class RequestUserAttribute : Attribute, IBinderTypeProviderMetadata
    {
        public BindingSource BindingSource
        {
            get
            {
                return new BindingSource(
                    id: "RequestUser",
                    displayName: "RequestUser",
                    isGreedy: false,
                    isFromRequest: false);
            }
        }

        Type IBinderTypeProviderMetadata.BinderType
        {
            get
            {
                return typeof(RequestUserModelBinder);
            }
        }

    }
}