using System.Threading;
using System.Threading.Tasks;
using Hangfire.Server;

namespace Hangfire_Mongo_Bug.Services
{
    public interface ITestService
    {
        Task ExecuteLongTaskAsync(int paramter, CancellationToken cancellationToken, PerformContext context);
        Task ExecuteShortTaskAsync(int paramter, CancellationToken cancellationToken, PerformContext context);
    }
}