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

            Get["/jobs/{id}"] = parameters =>
            {
                var name = interactor.GetJobNameById(parameters.id);

                if (string.IsNullOrEmpty(name))
                    return HttpStatusCode.NotFound;

                JobDetailsModel result = interactor.GetJobDetails(name);
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

            Put["/jobs/{id}", ctx => ctx.Request.Form.ActionToTake.ToString().ToLower() == HttpApiConstants.JobAction.Pause] = parameters =>
            {
                var name = interactor.GetJobNameById(parameters.id);

                if (string.IsNullOrEmpty(name))
                    return HttpStatusCode.NotFound;

                interactor.PauseJob(name);
                return HttpStatusCode.NoContent;
            };

            Put["/jobs/{id}", ctx => ctx.Request.Form.ActionToTake.ToString().ToLower() == HttpApiConstants.JobAction.Resume] = parameters =>
            {
                var name = interactor.GetJobNameById(parameters.id);

                if (string.IsNullOrEmpty(name))
                    return HttpStatusCode.NotFound;

                interactor.ResumeJob(name);
                return HttpStatusCode.NoContent;
            };

            Put["/jobs/{id}", ctx => ctx.Request.Form.ActionToTake.ToString().ToLower() == HttpApiConstants.JobAction.Start] = parameters =>
            {
                var name = interactor.GetJobNameById(parameters.id);

                if (string.IsNullOrEmpty(name))
                    return HttpStatusCode.NotFound;

                interactor.StartJob(name);
                return HttpStatusCode.NoContent;
            };

            Put["/jobs/{id}", ctx => ctx.Request.Form.ActionToTake.ToString().ToLower() == HttpApiConstants.JobAction.Kill] = parameters =>
                {
                    var name = interactor.GetJobNameById(parameters.id);

                    if (string.IsNullOrEmpty(name))
                        return HttpStatusCode.NotFound;

                    if (interactor.KillJob(name))
                    {
                        return HttpStatusCode.NoContent;
                    }
                    else
                    {
                        return new TextResponse(HttpStatusCode.BadRequest, $"Job '{name}' is not currently executing.");
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
