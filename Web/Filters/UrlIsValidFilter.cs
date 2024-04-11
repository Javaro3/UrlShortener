using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Filters {
    public class UrlIsValidFilter : ActionFilterAttribute {
        public override void OnActionExecuting(ActionExecutingContext context) {
            var url = context.HttpContext.Request.Form["Url"];

            if (!IsValidUrl(url)) {
                context.ModelState.AddModelError("Url", "URL has the wrong structure");
            }

            base.OnActionExecuting(context);
        }

        private static bool IsValidUrl(string url) {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}
