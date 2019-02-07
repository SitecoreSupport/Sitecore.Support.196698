using System.Web.Mvc;

namespace Sitecore.Support.Forms.Mvc.Controllers.Filters
{
  public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
  {
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
      if (filterContext.HttpContext.Request.UrlReferrer != null)
      {
        filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", filterContext.HttpContext.Request.UrlReferrer.Scheme + "://" + filterContext.HttpContext.Request.UrlReferrer.Host);
        filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Credentials", "true");
      }

      base.OnActionExecuting(filterContext);
    }
  }
}
