using System.Threading.Tasks;

namespace SampleApi.Db
{
    internal interface IInitDb
    {
        Task CreateAsync();
        Task PopulateAsync();
    }
}
