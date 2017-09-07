using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OksidiCom.AspNetCore.Common.ApiErrors;

namespace OksidiCom.AspNetCore.Common.Mvc
{
    /// <summary>
    /// Claim request binder
    /// </summary>
    internal class RequestClaimBinder : IModelBinder
    {

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var claimType = "";
            if (bindingContext.BindingSource is RequestClaimBindingSource)
            {
                claimType = (bindingContext.BindingSource as RequestClaimBindingSource).ClaimType;
            }
            var val = bindingContext.HttpContext.User.FindFirst(claimType);
            if (val != null)
            {
                bindingContext.Result = ModelBindingResult.Success(StringToType(val.Value, bindingContext.ModelType));
            }
            else
            {
                throw new NotAuthorized();
            }
            return Task.CompletedTask;
        }

        private object StringToType(string value, Type targetType)
        {
            if (typeof(string) == targetType)
            {
                return value;
            }
            else if (typeof(int) == targetType)
            {
                return int.Parse(value);
            }
            else
            {
                throw new FormatException("Claim binding type is unknown");
            }
        }
    }

    internal class RequestClaimBindingSource : BindingSource
    {
        public string ClaimType { get; set; }
        public RequestClaimBindingSource(string claimType, string id, string displayName, bool isGreedy, bool isFromRequest) : base(id, displayName, isGreedy, isFromRequest)
        {
            ClaimType = claimType;
        }
    }

    /// <summary>
    /// Get the request user claim value
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class RequestClaimAttribute : Attribute, IBinderTypeProviderMetadata
    {
        private string _claimType;
        public RequestClaimAttribute(string claimType)
        {
            _claimType = claimType;
        }

        public BindingSource BindingSource
        {
            get
            {
                return new RequestClaimBindingSource(
                    claimType: _claimType,
                    id: "RequestClaim",
                    displayName: "Request claim binder",
                    isGreedy: false,
                    isFromRequest: false)
                {

                };
            }
        }

        Type IBinderTypeProviderMetadata.BinderType
        {
            get
            {
                return typeof(RequestClaimBinder);
            }
        }

    }
}