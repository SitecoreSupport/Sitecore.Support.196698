namespace Sitecore.Support.Forms.Mvc.Pipelines.Initialize
{
  using Sitecore.Pipelines;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using System.Web.Mvc;
  using System.Web.Routing;
  using Constants = Sitecore.Forms.Mvc.Constants;

  public class InitializeRoutes : Sitecore.Mvc.Pipelines.Loader.InitializeRoutes
  {
    private static void RegisterClientEventController(RouteCollection routes)
    {
      routes.MapRoute(Constants.Routes.ClientEvent, "ClientEvent/{action}", new
      {
        controller = "ClientEvent",
        action = "Process"
      });

      routes.MapRoute(Constants.Routes.Form, "Form/{action}", new
      {
        controller = "Form",
        action = "Process"
      }, new[] { "Sitecore.Support.Forms.Mvc.Controllers" });
    }

    protected override void RegisterRoutes(RouteCollection routes, PipelineArgs args)
    {
      RegisterClientEventController(routes);
    }
  }
}