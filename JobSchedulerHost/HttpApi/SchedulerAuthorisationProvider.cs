using System.Collections.Generic;
using System.Security.Principal;
using Nancy;

namespace JobSchedulerHost.HttpApi
{
    public interface ISchedulerAuthorisationProvider
    {
        bool IsAuthorisedForOperation(NancyContext context);
    }

    public class SchedulerAuthorisationProvider : ISchedulerAuthorisationProvider
    {
        public bool IsAuthorisedForOperation(NancyContext context)
        {
            var user = GetUserPrincipal(context);
            return ExecuteRequestIfAuthorised(user.Identity, context.Request);
        }

        private IPrincipal GetUserPrincipal(NancyContext context)
        {
            var env = ((IDictionary<string, object>)context.Items["OWIN_REQUEST_ENVIRONMENT"]);
            var user = (IPrincipal)env["server.User"];
            return user;
        }

        private bool ExecuteRequestIfAuthorised(IIdentity user, Request request)
        {
            switch (request.Method)
            {
                case "GET":
                    //check they have read-only access
                    return IsAuthorisedForReadAccess(user);
                case "POST":
                case "PUT":
                    //check they have read-write access
                    return IsAuthorisedForReadWriteAccess(user);
                default:
                    return false;
            }
        }

        private bool IsAuthorisedForReadAccess(IIdentity user)
        {
            return (true);
        }

        private bool IsAuthorisedForReadWriteAccess(IIdentity user)
        {
            return (true);
        }
    }
}