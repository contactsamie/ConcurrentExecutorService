using System.Threading.Tasks;

namespace ConcurrentExecutorService.Tests.TestSystem
{
    public interface IPurchaseServiceFactory
    {
        Task<object> RunPurchaseServiceAsync(string o);
    }
}