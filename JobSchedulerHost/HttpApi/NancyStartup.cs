using System.Net;
using Owin;

namespace JobSchedulerHost.HttpApi
{
    internal class NancyStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var listener = (HttpListener)app.Properties["System.Net.HttpListener"];
            //listener.AuthenticationSchemes = AuthenticationSchemes.IntegratedWindowsAuthentication;

            app.UseNancy();
        }
    }
}
