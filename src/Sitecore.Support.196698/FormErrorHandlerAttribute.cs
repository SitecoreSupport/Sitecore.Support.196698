using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Forms.Mvc.Controllers;
using Sitecore.Forms.Mvc.Data.Wrappers;
using Sitecore.Forms.Mvc.Models;
using Sitecore.Forms.Mvc.ViewModels;
using Sitecore.WFFM.Abstractions.Actions;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Sitecore.Sites;

namespace Sitecore.Support.Forms.Mvc.Attributes
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
  public class FormErrorHandlerAttribute : HandleErrorAttribute
  {
    private readonly IRenderingContext renderingContext;

    public FormErrorHandlerAttribute()
        : this((IRenderingContext)Factory.CreateObject(Sitecore.Forms.Mvc.Constants.FormRenderingContext, true))
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
        if(!filterContext.ExceptionHandled)
        {
          Guid uniqueId = renderingContext.Rendering.UniqueId;
          string value = filterContext.RequestContext.HttpContext.Request.Form[FormViewModel.GetClientId(uniqueId) + ".Id"];
          if (!string.IsNullOrEmpty(value))
          {
            Log.Error(filterContext.Exception.Message, filterContext.Exception, this);
            Sitecore.Support.Forms.Mvc.Controllers.FormController formController = filterContext.Controller as Sitecore.Support.Forms.Mvc.Controllers.FormController;
            FormErrorResult<FormModel, FormViewModel> formErrorResult = null;
            if (formController != null)
            {
              FormErrorResult<FormModel, FormViewModel> formErrorResult2 = new FormErrorResult<FormModel, FormViewModel>(formController.FormRepository, formController.Mapper, formController.FormProcessor, new ExecuteResult.Failure
              {
                ErrorMessage = filterContext.Exception.Message,
                StackTrace = filterContext.Exception.StackTrace,
                IsCustom = false
              });
              formErrorResult2.ViewData = formController.ViewData;
              formErrorResult2.TempData = formController.TempData;
              formErrorResult2.ViewEngineCollection = formController.ViewEngineCollection;
              formErrorResult = formErrorResult2;
            }
            filterContext.Result = ((formErrorResult != null) ? ((ActionResult)formErrorResult) : ((ActionResult)new EmptyResult()));
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
          aspSession.Add("WFFM196698SITE", contextSite);
          aspSession.Add("WFFM196698USER", Context.User.Name);
          aspSession.Add("WFFM196698URL", Context.RawUrl);
          Log.Warn(string.Format("Request is redirected to the document not found page. Requested URL: {0}, User: {1}, Website: {2}", Context.RawUrl, Context.User.Name, contextSite), this);
          Sitecore.Web.WebUtil.Redirect(Sitecore.Configuration.Settings.ItemNotFoundUrl);
        }
      }
    }
  }
}