using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<BusinessThing> MyBusinessThing()
    {
        return null;
    }
}