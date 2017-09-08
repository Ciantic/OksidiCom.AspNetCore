using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OksidiCom.AspNetCore.UserService.Models;
using OksidiCom.AspNetCore.UserService.Mvc;
using SampleApi.Db;
using SampleApi.Models;

[Authorize]
[Route("[controller]")]
class BusinessThingsController
{
    private readonly BusinessDbContext _businessDbContext;

    public BusinessThingsController(BusinessDbContext businessDbContext)
    {
        _businessDbContext = businessDbContext;
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> MyBusinessThing([RequestUser] ApplicationUser user)
    {
        return new ContentResult()
        {
            Content = user.Email
        };
    }
}