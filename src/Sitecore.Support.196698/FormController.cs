using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Forms.Mvc.Controllers;
using Sitecore.Forms.Mvc.Controllers.Filters;
using Sitecore.Forms.Mvc.Controllers.ModelBinders;
using Sitecore.Forms.Mvc.Interfaces;
using Sitecore.Forms.Mvc.Models;
using Sitecore.Forms.Mvc.ViewModels;
using Sitecore.Mvc.Controllers;
using Sitecore.WFFM.Abstractions.Dependencies;
using Sitecore.WFFM.Abstractions.Shared;
using System.IO;
using System.Web.Mvc;

namespace Sitecore.Support.Forms.Mvc.Controllers
{
  [ModelBinder(typeof(FormModelBinder))]
  public class FormController : Sitecore.Forms.Mvc.Controllers.FormController
  {
    private readonly IAnalyticsTracker analyticsTracker;

    public IRepository<FormModel> FormRepository
    {
      get;
      private set;
    }

    public IAutoMapper<IFormModel, FormViewModel> Mapper
    {
      get;
      private set;
    }

    public IFormProcessor<FormModel> FormProcessor
    {
      get;
      private set;
    }

    public FormController()
        : this((IRepository<FormModel>)Factory.CreateObject(Sitecore.Forms.Mvc.Constants.FormRepository, true), (IAutoMapper<IFormModel, FormViewModel>)Factory.CreateObject(Sitecore.Forms.Mvc.Constants.FormAutoMapper, true), (IFormProcessor<FormModel>)Factory.CreateObject(Sitecore.Forms.Mvc.Constants.FormProcessor, true), DependenciesManager.AnalyticsTracker)
    {
    }

    public FormController(IRepository<FormModel> repository, IAutoMapper<IFormModel, FormViewModel> mapper, IFormProcessor<FormModel> processor, IAnalyticsTracker analyticsTracker)
    {
      Assert.ArgumentNotNull(repository, "repository");
      Assert.ArgumentNotNull(mapper, "mapper");
      Assert.ArgumentNotNull(processor, "processor");
      Assert.ArgumentNotNull(analyticsTracker, "analyticsTracker");
      FormRepository = repository;
      Mapper = mapper;
      FormProcessor = processor;
      this.analyticsTracker = analyticsTracker;
    }

    [Sitecore.Support.Forms.Mvc.Attributes.FormErrorHandler]
    [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Head)]
    public override ActionResult Index()
    {
      return Form();
    }

    [WffmValidateAntiForgeryToken]
    [HttpPost]
    [Sitecore.Support.Forms.Mvc.Controllers.Filters.SubmittedFormHandler]
    [Sitecore.Support.Forms.Mvc.Attributes.FormErrorHandler]
    public override ActionResult Index([ModelBinder(typeof(FormModelBinder))] FormViewModel formViewModel)
    {
      analyticsTracker.InitializeTracker();
      return ProcessedForm(formViewModel, "");
    }

    [AllowCrossSiteJson]
    [Sitecore.Support.Forms.Mvc.Attributes.FormErrorHandler]
    public override JsonResult Process([ModelBinder(typeof(FormModelBinder))] FormViewModel formViewModel)
    {
      analyticsTracker.InitializeTracker();
      ProcessedFormResult<FormModel, FormViewModel> processedFormResult = ProcessedForm(formViewModel, "~/Views/Form/Index.cshtml");
      processedFormResult.ExecuteResult(base.ControllerContext);
      string data = default(string);
      using (StringWriter stringWriter = new StringWriter())
      {
        ViewContext viewContext = new ViewContext(base.ControllerContext, processedFormResult.View, base.ViewData, base.TempData, stringWriter);
        processedFormResult.View.Render(viewContext, stringWriter);
        data = stringWriter.GetStringBuilder().ToString();
      }
      base.ControllerContext.HttpContext.Response.Clear();
      JsonResult jsonResult = new JsonResult();
      jsonResult.Data = data;
      return jsonResult;
    }

    public override FormResult<FormModel, FormViewModel> Form()
    {
      FormResult<FormModel, FormViewModel> formResult = new FormResult<FormModel, FormViewModel>(FormRepository, Mapper);
      formResult.ViewData = base.ViewData;
      formResult.TempData = base.TempData;
      formResult.ViewEngineCollection = base.ViewEngineCollection;
      return formResult;
    }

    public override ProcessedFormResult<FormModel, FormViewModel> ProcessedForm(FormViewModel viewModel, string viewName = "")
    {
      ProcessedFormResult<FormModel, FormViewModel> processedFormResult = new ProcessedFormResult<FormModel, FormViewModel>(FormRepository, Mapper, FormProcessor, viewModel);
      processedFormResult.ViewData = base.ViewData;
      processedFormResult.TempData = base.TempData;
      processedFormResult.ViewEngineCollection = base.ViewEngineCollection;
      ProcessedFormResult<FormModel, FormViewModel> processedFormResult2 = processedFormResult;
      if (!string.IsNullOrEmpty(viewName))
      {
        processedFormResult2.ViewName = viewName;
      }
      return processedFormResult2;
    }
  }
}