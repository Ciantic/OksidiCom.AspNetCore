using System.Threading.Tasks;

namespace OksidiCom.AspNetCore.UserService.Db
{
    internal interface IInitDb
    {
        Task InitAsync();
    }
}
