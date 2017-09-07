using System.Threading.Tasks;

namespace OksidiCom.AspNetCore.UserServices.Db
{
    internal interface IInitDb
    {
        Task InitAsync();
    }
}
