using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mesawer.ApplicationLayer.Interfaces;
using Mesawer.ApplicationLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Mesawer.InfrastructureLayer.Services
{
    public class RazorRendererService : IRazorRendererService
    {
        private readonly IServiceProvider  _services;
        private readonly ITempDataProvider _tempData;
        private readonly IRazorViewEngine  _viewEngine;

        public RazorRendererService(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempData,
            IServiceProvider services)
        {
            _viewEngine = viewEngine;
            _tempData   = tempData;
            _services   = services;
        }

        public async Task<string> RenderAsync<TModel>(string viewName, TModel model) where TModel : EmailTemplate
        {
            var actionContext = GetActionContext();
            var view          = FindView(actionContext, viewName);

            await using var output = new StringWriter();

            var viewData = new ViewDataDictionary<TModel>(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model,
            };

            if (model.ViewData is not null && model.ViewData.Any())
                foreach (var (key, value) in model.ViewData)
                    viewData[key] = value;

            var viewContext = new ViewContext(
                actionContext,
                view,
                viewData,
                new TempDataDictionary(actionContext.HttpContext, _tempData),
                output,
                new HtmlHelperOptions());

            await view.RenderAsync(viewContext);

            return output.ToString();
        }

        private IView FindView(ActionContext actionContext, string viewName)
        {
            var getViewResult = viewName.StartsWith("/")
                ? _viewEngine.GetView(null, viewName, true)
                : _viewEngine.GetView(null, Path.Combine("/Views/Emails", viewName), true);

            if (getViewResult.Success) return getViewResult.View;

            var findViewResult = _viewEngine.FindView(actionContext, viewName, true);

            if (findViewResult.Success) return findViewResult.View;

            var searchedLocations = getViewResult.SearchedLocations
                .Concat(findViewResult.SearchedLocations);

            var errorMessage = string.Join(
                Environment.NewLine,
                new[]
                    {
                        $"Unable to find view \"{viewName}\". The following locations were searched:",
                    }
                    .Concat(searchedLocations));

            throw new InvalidOperationException(errorMessage);
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _services,
            };

            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}
