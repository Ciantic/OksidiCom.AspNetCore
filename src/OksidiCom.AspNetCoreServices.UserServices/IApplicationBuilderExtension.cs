using Microsoft.AspNetCore.Builder;

namespace OksidiCom.AspNetCoreServices.UserServices
{
    public static class IApplicationBuilderExtension
    {
        public static void UseUserServices(this IApplicationBuilder app)
        {
            // TODO: Verify that UseMvc has NOT been called yet

            app.UseAuthentication();
        }
    }
}
