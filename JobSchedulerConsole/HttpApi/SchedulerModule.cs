using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using Nancy;
using Nancy.ModelBinding;

namespace JobSchedulerHost.HttpApi
{
    public class SchedulerModule : NancyModule
    {
        public SchedulerModule(ISchedulerInteractor interactor) : base("/scheduler")
        {
            Get["/status"] = _ => interactor.GetStatus();

            Get["/jobs"] = _ => Response.AsJson(interactor.GetJobNames());

            Get["/job/{name}"] = parameters =>
            {
                JobDetailsModel result = interactor.GetJobDetails(parameters.name);
                return Response.AsJson(result);
            };
        }

        private IPrincipal GetUserPrincipal()
        {
            var env = ((IDictionary<string, object>) Context.Items["OWIN_REQUEST_ENVIRONMENT"]);
            var user = (IPrincipal) env["server.User"];
            return user;
        }
    }
}
