using Sitecore.Pipelines;
using System.Web.Mvc;
using System.Web.Routing;

namespace Sitecore.Support.Forms.Mvc.Pipelines.Initialize
{
  public class InitializeRoutes : Sitecore.Mvc.Pipelines.Loader.InitializeRoutes
  {
    protected override void RegisterRoutes(RouteCollection routes, PipelineArgs args)
    {
      RegisterClientEventController(routes);
    }

    private static void RegisterClientEventController(RouteCollection routes)
    {
      routes.MapRoute(Sitecore.Forms.Mvc.Constants.Routes.ClientEvent, "clientevent/{action}", new
      {
        controller = "ClientEvent",
        action = "Process"
      });
      routes.MapRoute(Sitecore.Forms.Mvc.Constants.Routes.Form, "Form/{action}", new
      {
        controller = "Form",
        action = "Process"
      }, new[] { "Sitecore.Support.Forms.Mvc.Controllers" });
      routes.MapRoute("FormIndex", "Form/{action}", new
      {
        controller = "Form",
        action = "Index"
      }, new[] { "Sitecore.Support.Forms.Mvc.Controllers" });
    }
  }
}