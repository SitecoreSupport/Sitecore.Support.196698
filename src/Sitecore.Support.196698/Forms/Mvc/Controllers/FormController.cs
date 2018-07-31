namespace Sitecore.Support.Forms.Mvc.Controllers
{
  using Sitecore.Configuration;
  using Sitecore.Diagnostics;
  using Sitecore.Forms.Mvc.Controllers;
  using Sitecore.Forms.Mvc.Controllers.Filters;
  using Sitecore.Forms.Mvc.Controllers.ModelBinders;
  using Sitecore.Forms.Mvc.Interfaces;
  using Sitecore.Forms.Mvc.Models;
  using Sitecore.Forms.Mvc.ViewModels;
  using Sitecore.Mvc.Controllers;
  using Sitecore.Support.Forms.Mvc.Controllers.Filters;
  using Sitecore.WFFM.Abstractions.Dependencies;
  using Sitecore.WFFM.Abstractions.Shared;
  using System.IO;
  using System.Web.Mvc;
  using Constants = Sitecore.Forms.Mvc.Constants;
  using FormErrorHandlerAttribute = Sitecore.Support.Forms.Mvc.Attributes.FormErrorHandlerAttribute;

  [ModelBinder(typeof(FormModelBinder))]
  public class FormController : SitecoreController
  {
    private readonly IAnalyticsTracker analyticsTracker;

    public FormController() : this((IRepository<FormModel>)Factory.CreateObject(Constants.FormRepository, true), (IAutoMapper<IFormModel, FormViewModel>)Factory.CreateObject(Constants.FormAutoMapper, true), (IFormProcessor<FormModel>)Factory.CreateObject(Constants.FormProcessor, true), DependenciesManager.AnalyticsTracker)
    {
    }

    public FormController(IRepository<FormModel> repository, IAutoMapper<IFormModel, FormViewModel> mapper, IFormProcessor<FormModel> processor, IAnalyticsTracker analyticsTracker)
    {
      Assert.ArgumentNotNull(repository, "repository");
      Assert.ArgumentNotNull(mapper, "mapper");
      Assert.ArgumentNotNull(processor, "processor");
      Assert.ArgumentNotNull(analyticsTracker, "analyticsTracker");
      this.FormRepository = repository;
      this.Mapper = mapper;
      this.FormProcessor = processor;
      this.analyticsTracker = analyticsTracker;
    }

    public virtual FormResult<FormModel, FormViewModel> Form() =>
        new FormResult<FormModel, FormViewModel>(this.FormRepository, (IAutoMapper<FormModel, FormViewModel>)this.Mapper)
        {
          ViewData = base.ViewData,
          TempData = base.TempData,
          ViewEngineCollection = base.ViewEngineCollection
        };

    [FormErrorHandler, AcceptVerbs(HttpVerbs.Head | HttpVerbs.Get)]
    public override ActionResult Index() =>
        this.Form();

    [SubmittedFormHandler, FormErrorHandler, HttpPost, WffmLimitMultipleSubmits, WffmValidateAntiForgeryToken]
    public virtual ActionResult Index([ModelBinder(typeof(FormModelBinder))] FormViewModel formViewModel)
    {
      this.analyticsTracker.InitializeTracker();
      return this.ProcessedForm(formViewModel, "");
    }

    [FormErrorHandler, AllowCrossSiteJson]
    public virtual JsonResult Process([ModelBinder(typeof(FormModelBinder))] FormViewModel formViewModel)
    {
      string str;
      this.analyticsTracker.InitializeTracker();
      ProcessedFormResult<FormModel, FormViewModel> result = this.ProcessedForm(formViewModel, "~/Views/Form/Index.cshtml");
      result.ExecuteResult(base.ControllerContext);
      using (StringWriter writer = new StringWriter())
      {
        ViewContext viewContext = new ViewContext(base.ControllerContext, result.View, base.ViewData, base.TempData, writer);
        result.View.Render(viewContext, writer);
        str = writer.GetStringBuilder().ToString();
      }
      base.ControllerContext.HttpContext.Response.Clear();
      return new JsonResult { Data = str };
    }

    public virtual ProcessedFormResult<FormModel, FormViewModel> ProcessedForm(FormViewModel viewModel, string viewName = "")
    {
      ProcessedFormResult<FormModel, FormViewModel> result = new ProcessedFormResult<FormModel, FormViewModel>(this.FormRepository, (IAutoMapper<FormModel, FormViewModel>)this.Mapper, this.FormProcessor, viewModel)
      {
        ViewData = base.ViewData,
        TempData = base.TempData,
        ViewEngineCollection = base.ViewEngineCollection
      };
      if (!string.IsNullOrEmpty(viewName))
      {
        result.ViewName = viewName;
      }
      return result;
    }

    public IFormProcessor<FormModel> FormProcessor { get; private set; }

    public IRepository<FormModel> FormRepository { get; private set; }

    public IAutoMapper<IFormModel, FormViewModel> Mapper { get; private set; }
  }
}