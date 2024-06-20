using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
namespace Workflows.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static async Task<IHtmlContent> RenderPartialViewAsync(this IHtmlHelper htmlHelper, string partialViewName, object model)
        {
            var viewEngine = htmlHelper.ViewContext.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            var viewResult = viewEngine.FindView(htmlHelper.ViewContext, partialViewName, false);

            if (!viewResult.Success)
            {
                throw new InvalidOperationException($"Partial view '{partialViewName}' not found.");
            }

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            using (var writer = new StringWriter())
            {
                var viewContext = new ViewContext(htmlHelper.ViewContext, viewResult.View, viewData, htmlHelper.ViewContext.TempData, writer, new HtmlHelperOptions());
                await viewResult.View.RenderAsync(viewContext);
                return new HtmlString(writer.GetStringBuilder().ToString());
            }
        }
    }
}
