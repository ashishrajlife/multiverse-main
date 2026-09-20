using Microsoft.AspNetCore.Mvc.Filters;

namespace ERPDemo.Filters
{
    /// <summary>
    /// Prevents the browser from caching the response so that hitting the
    /// Back button after logout never shows a stale authenticated page.
    /// </summary>
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var headers = context.HttpContext.Response.Headers;

            headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
            headers["Pragma"] = "no-cache";
            headers["Expires"] = "-1";

            base.OnResultExecuting(context);
        }
    }
}