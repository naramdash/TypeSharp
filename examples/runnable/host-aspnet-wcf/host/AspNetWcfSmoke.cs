using System.ServiceModel;
using System.Web.UI;

namespace Samples.Runnable.HostAspNetWcf.Host
{
    public sealed class GreetingPage : Page
    {
        public string RenderGreeting()
        {
            var unit = TypeSharp.Core.Unit.Value;
            return Samples.Runnable.HostAspNetWcf.Module.greeting()
                + ":"
                + TypeSharp.Runtime.TypeSharpRuntimeInfo.TargetFramework
                + ":"
                + unit.ToString();
        }
    }

    [ServiceContract]
    public interface IGreetingService
    {
        [OperationContract]
        string GetGreeting();
    }

    public sealed class GreetingService : IGreetingService
    {
        public string GetGreeting()
        {
            var option = TypeSharp.Core.Option<string>.Some(Samples.Runnable.HostAspNetWcf.Module.greeting());
            return option.Value + ":" + TypeSharp.Runtime.TypeSharpRuntimeInfo.TargetFramework;
        }
    }

    public sealed class GreetingClient : ClientBase<IGreetingService>, IGreetingService
    {
        public string GetGreeting()
        {
            return Channel.GetGreeting();
        }

        public string GetLocalFallbackGreeting()
        {
            var result = TypeSharp.Core.Result<string, string>.Ok(Samples.Runnable.HostAspNetWcf.Module.greeting());
            return result.Value + ":" + TypeSharp.Runtime.TypeSharpRuntimeInfo.RuntimeAbiVersion.ToString();
        }
    }
}
