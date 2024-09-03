using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SportWeb.Filters
{
    public class ValidateModelStateFilter : ActionFilterAttribute
    {
        public string? ViewName { get; set; }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if the ModelState is valid
            if (!context.ModelState.IsValid && context.Controller is Controller controller)
            {
                controller.ViewBag.Message = "Model state is invalid.";

                // If not, set the result to the current view with the model
                context.Result = new ViewResult
                {
                    ViewData = controller.ViewData,
                    TempData = controller.TempData,
                    ViewName = ViewName ?? (context.ActionDescriptor is ControllerActionDescriptor descriptor
                        ? descriptor.ActionName
                        : null)
                };

                if (context.ActionArguments.TryGetValue("form", out var model))
                {
                    controller.ViewData.Model = model;
                }
            }
        }
    }

}
