using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Forms.Mvc;
using Sitecore.Forms.Mvc.Controllers;
using Sitecore.Forms.Mvc.Data.Wrappers;
using Sitecore.Forms.Mvc.Models;
using Sitecore.Forms.Mvc.ViewModels;
using Sitecore.WFFM.Abstractions.Actions;
using System;
using System.Web.Mvc;

namespace Sitecore.Forms.Mvc.Attributes
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
  public class FormErrorHandlerAttribute : HandleErrorAttribute
  {
    private readonly IRenderingContext renderingContext;

    public FormErrorHandlerAttribute()
        : this((IRenderingContext)Factory.CreateObject(Constants.FormRenderingContext, true))
    {
    }

    public FormErrorHandlerAttribute(IRenderingContext renderingContext)
    {
      Assert.ArgumentNotNull(renderingContext, "renderingContext");
      this.renderingContext = renderingContext;
    }

    public override void OnException(ExceptionContext filterContext)
    {
      if (!filterContext.ExceptionHandled)
      {
        Guid uniqueId = renderingContext.Rendering.UniqueId;
        string value = filterContext.RequestContext.HttpContext.Request.Form[FormViewModel.GetClientId(uniqueId) + ".Id"];
        if (!string.IsNullOrEmpty(value))
        {
          Log.Error(filterContext.Exception.Message, filterContext.Exception, this);
          FormController formController = filterContext.Controller as FormController;
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
  }
}