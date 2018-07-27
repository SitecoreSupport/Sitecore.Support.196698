using Sitecore.Diagnostics;
using Sitecore.Forms.Mvc.Interfaces;
using System.Linq;
using System.Web.Mvc;

namespace Sitecore.Support.Forms.Mvc.Controllers.Filters
{
  public class SubmittedFormHandler : ActionFilterAttribute
  {
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
      Assert.ArgumentNotNull(filterContext, "filterContext");
      Sitecore.Support.Forms.Mvc.Controllers.FormController formController = filterContext.Controller as Sitecore.Support.Forms.Mvc.Controllers.FormController;
      if (formController != null)
      {
        IViewModel viewModel = filterContext.ActionParameters.Values.First() as IViewModel;
        if (viewModel == null)
        {
          filterContext.Result = formController.Form();
        }
      }
    }
  }
}