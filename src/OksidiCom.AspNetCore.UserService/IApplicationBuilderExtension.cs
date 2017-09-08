using Microsoft.AspNetCore.Builder;

namespace OksidiCom.AspNetCore.UserService
{
    public static class IApplicationBuilderExtension
    {
        public static void UseUserService(this IApplicationBuilder app)
        {
            // TODO: Verify that UseMvc has NOT been called yet

            app.UseAuthentication();
        }
    }
}
