using System.ServiceProcess;

namespace Samples.Runnable.HostWorker.Host
{
    public sealed class GreetingWorker : ServiceBase
    {
        public string RunOnce()
        {
            var result = TypeSharp.Core.Result<string, string>.Ok(Samples.Runnable.HostWorker.Module.nextRunLabel());
            return result.Value + ":" + TypeSharp.Runtime.TypeSharpRuntimeInfo.RuntimeAbiVersion.ToString();
        }
    }
}
