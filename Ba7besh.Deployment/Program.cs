using System.Threading.Tasks;

namespace Ba7besh.Deployment;

public static class Program
{
    public static Task<int> Main()
    {
        return Pulumi.Deployment.RunAsync<MainStack>();
    }
}