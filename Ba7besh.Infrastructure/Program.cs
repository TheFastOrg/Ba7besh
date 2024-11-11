using System.Threading.Tasks;
using Pulumi;

namespace Ba7besh.Infrastructure;

public static class Program
{
    public static Task<int> Main()
    {
        return Deployment.RunAsync<MainStack>();
    }
}