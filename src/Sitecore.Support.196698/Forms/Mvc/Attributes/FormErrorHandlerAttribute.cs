namespace Sitecore.Support.Forms.Mvc.Attributes
{
  using Sitecore.Configuration;
  using Sitecore.Diagnostics;
  using Sitecore.Forms.Mvc.Controllers;
  using Sitecore.Forms.Mvc.Data.Wrappers;
  using Sitecore.Forms.Mvc.Interfaces;
  using Sitecore.Forms.Mvc.Models;
  using Sitecore.Forms.Mvc.ViewModels;
  using Sitecore.Sites;
  using Sitecore.Web;
  using Sitecore.WFFM.Abstractions.Actions;
  using System;
  using System.Web;
  using System.Web.Mvc;
  using System.Web.SessionState;
  using Constants = Sitecore.Forms.Mvc.Constants;

  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
  public class FormErrorHandlerAttribute : HandleErrorAttribute
  {
    private readonly IRenderingContext renderingContext;

    public FormErrorHandlerAttribute() : this((IRenderingContext)Factory.CreateObject(Constants.FormRenderingContext, true))
    {
    }

    public FormErrorHandlerAttribute(IRenderingContext renderingContext)
    {
      Assert.ArgumentNotNull(renderingContext, "renderingContext");
      this.renderingContext = renderingContext;
    }

    public override void OnException(ExceptionContext filterContext)
    {
      try
      {
        if (!filterContext.ExceptionHandled)
        {
          Guid uniqueId = this.renderingContext.Rendering.UniqueId;
          if (!string.IsNullOrEmpty(filterContext.RequestContext.HttpContext.Request.Form[FormViewModel.GetClientId(uniqueId) + ".Id"]))
          {
            Log.Error(filterContext.Exception.Message, filterContext.Exception, this);
            FormController controller = filterContext.Controller as FormController;
            FormErrorResult<FormModel, FormViewModel> result = null;
            if (controller != null)
            {
              ExecuteResult.Failure failure = new ExecuteResult.Failure
              {
                ErrorMessage = filterContext.Exception.Message,
                StackTrace = filterContext.Exception.StackTrace,
                IsCustom = false
              };
              result = new FormErrorResult<FormModel, FormViewModel>(controller.FormRepository, (IAutoMapper<FormModel, FormViewModel>)controller.Mapper, controller.FormProcessor, failure)
              {
                ViewData = controller.ViewData,
                TempData = controller.TempData,
                ViewEngineCollection = controller.ViewEngineCollection
              };
            }
            filterContext.Result = (result != null) ? ((ActionResult)result) : ((ActionResult)new EmptyResult());
            filterContext.ExceptionHandled = true;
          }
        }
      }
      catch (NullReferenceException)
      {
        SiteContext site = Context.Site;
        HttpContext currentContext = HttpContext.Current;
        if (currentContext != null)
        {
          HttpSessionState aspSession = Context.Items["AspSession"] as HttpSessionState;
          string contextSite = (site != null) ? site.Name : string.Empty;
          aspSession.Add("WFFM196698SiTE", contextSite);
          aspSession.Add("WFFM196698USER", Context.User.Name);
          aspSession.Add("WFFM196698URL", Context.RawUrl);
          Log.Warn(string.Format("Request is redirected to document not found page. Requested url: {0}, User: {1}, Website: {2}", Context.RawUrl, Context.User.Name, contextSite), this);
          WebUtil.Redirect(Context.Item.Paths.Path);
        }
      }
    }
  }
}