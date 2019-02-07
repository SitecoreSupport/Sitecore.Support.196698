using Sitecore.Pipelines;
using System.Web;
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

      routes.MapRoute("FormProcessEx", "form/process", new
      {
        controller = "Form",
        action = "ProcessEx"
      }, new[] { "Sitecore.Support.Forms.Mvc.Controllers" });

      routes.MapRoute("FormIndexEx", "form/index", new
      {
        controller = "Form",
        action = "IndexEx"
      }, new { notPost=new NotPost() }, new string[] { "Sitecore.Support.Forms.Mvc.Controllers" });

      routes.MapRoute(Sitecore.Forms.Mvc.Constants.Routes.Form, "form/{action}", new
      {
        controller = "Form",
        action = "ProcessEx"
      }, new[] { "Sitecore.Support.Forms.Mvc.Controllers" });
    }
  }

  public class NotPost : IRouteConstraint
  {
    public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
    {
      return httpContext.Request.HttpMethod != "POST";
    }
  }
}