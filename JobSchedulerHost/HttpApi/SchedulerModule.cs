using System.Collections.Generic;
using System.Security.Principal;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses;

namespace JobSchedulerHost.HttpApi
{
    public class SchedulerModule : NancyModule
    {
        public SchedulerModule(ISchedulerInteractor interactor) : base("/scheduler")
        {
            After.AddItemToEndOfPipeline((ctx) => ctx.Response
                .WithHeader("Access-Control-Allow-Origin", "*")
                .WithHeader("Access-Control-Allow-Methods", "PUT,POST,GET")
                .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type"));

            Get["/status"] = _ => interactor.GetStatus();

            Get["/jobs"] = _ =>
            {
                var requestModel = this.Bind<SearchCriteriaModel>();

                if (requestModel.Criteria == null)
                {
                    return Response.AsJson(interactor.GetJobs());
                }
                if (requestModel.Criteria.ToLower() == "executing")
                {
                    return Response.AsJson(interactor.GetCurrentlyExecutingJobs());
                }
                return HttpStatusCode.NotFound;
            };

            Get["/jobs/{name}"] = parameters =>
            {
                if (!interactor.JobExists(parameters.name))
                    return HttpStatusCode.NotFound;

                JobDetailsModel result = interactor.GetJobDetails(parameters.name);
                return Response.AsJson(result);
            };

            Put["/jobs", ctx => ctx.Request.Form.ActionToTake.ToString().ToLower() == HttpApiConstants.JobAction.Pause] = _ =>
            {
                interactor.PauseAllJobs();
                return HttpStatusCode.NoContent;
            };

            Put["/jobs", ctx => ctx.Request.Form.ActionToTake.ToString().ToLower() == HttpApiConstants.JobAction.Resume] = _ =>
            {
                interactor.ResumeAllJobs();
                return HttpStatusCode.NoContent;
            };

            Put["/jobs/{name}", ctx => ctx.Request.Form.ActionToTake.ToString().ToLower() == HttpApiConstants.JobAction.Pause] = parameters =>
            {
                if (!interactor.JobExists(parameters.name))
                    return HttpStatusCode.NotFound;

                interactor.PauseJob(parameters.name);
                return HttpStatusCode.NoContent;
            };

            Put["/jobs/{name}", ctx => ctx.Request.Form.ActionToTake.ToString().ToLower() == HttpApiConstants.JobAction.Resume] = parameters =>
            {
                if (!interactor.JobExists(parameters.name))
                    return HttpStatusCode.NotFound;

                interactor.ResumeJob(parameters.name);
                return HttpStatusCode.NoContent;
            };

            Put["/jobs/{name}", ctx => ctx.Request.Form.ActionToTake.ToString().ToLower() == HttpApiConstants.JobAction.Start] = parameters =>
            {
                if (!interactor.JobExists(parameters.name))
                    return HttpStatusCode.NotFound;

                interactor.StartJob(parameters.name);
                return HttpStatusCode.NoContent;
            };

            Put["/jobs/{name}", ctx => ctx.Request.Form.ActionToTake.ToString().ToLower() == HttpApiConstants.JobAction.Kill] = parameters =>
                {
                    if (!interactor.JobExists(parameters.name))
                        return HttpStatusCode.NotFound;

                    if (interactor.KillJob(parameters.name))
                    {
                        return HttpStatusCode.NoContent;
                    }
                    else
                    {
                        return new TextResponse(HttpStatusCode.BadRequest, $"Job '{parameters.name}' is not currently executing.");
                    }
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
