using Microsoft.AspNetCore.Builder;

namespace OksidiCom.AspNetCore.UserServices
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
