using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SportWeb.Filters
{
    public class MessageFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Действие перед выполнением контроллера
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Controller is Controller controller && controller.TempData.TryGetValue("Message", out object? value))
            {
                controller.ViewBag.Message = value;
            }
        }
    }
}