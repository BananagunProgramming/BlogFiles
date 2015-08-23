using System;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace YourNamespace.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ValidateAntiForgeryTokenControllerLevelAttribute : FilterAttribute, IAuthorizationFilter
    {
        private const string AntiForgeryTokenFieldName = "__RequestVerificationToken";
        
        // collection of HTTP verbs that we will be doing validation against, initialized during consutrution, typically just a POST
        private readonly AcceptVerbsAttribute _verbs;

        // set of action names to be ignored for validation, e.g. a POST action which does not modify any data
        private readonly string[] _actionsToNotValidate = new [] {""};

        /// <summary>
        /// Basic version of the configuration. No ignored actions and no specific salt.
        /// </summary>
        /// <param name="verbs">HTTP request type for which anti forgery checks will be performed. Typically a POST.</param>
        public ValidateAntiForgeryTokenControllerLevelAttribute(HttpVerbs verbs)
            : this(verbs, (string[]) null)
        {
        }

        /// <summary>
        /// Allows to pass 1 action to be ignored.
        /// </summary>
        /// <param name="verbs">HTTP request type for which anti forgery checks will be performed. Typically a POST.</param>
        /// <param name="actionToNotValidate1"></param>
        public ValidateAntiForgeryTokenControllerLevelAttribute(HttpVerbs verbs, string actionToNotValidate1)
            : this(verbs, new[] {actionToNotValidate1})
        {
        }

        /// <summary>
        /// Allows to pass 2 actions to be ignored.
        /// </summary>
        /// <param name="verbs">HTTP request type for which anti forgery checks will be performed. Typically a POST.</param>
        /// <param name="actionToNotValidate1"></param>
        /// <param name="actionToNotValidate2"></param>
        public ValidateAntiForgeryTokenControllerLevelAttribute(HttpVerbs verbs, string actionToNotValidate1, string actionToNotValidate2)
            : this(verbs, new [] {actionToNotValidate1, actionToNotValidate2})
        {
        }

        /// <summary>
        /// More advanced configuration: allows to provide actions to be ignored from the check.
        /// </summary>
        /// <param name="verbs">HTTP request type for which anti forgery checks will be performed. Typically a POST.</param>
        /// <param name="actionsToNotValidate">A list of case insensitive strings carrying action names for which anti forgery checks should not be performed.
        /// Typically you might want to exclude POSTs which don't submit any [important] data and hence data tampering risk does not exist.</param>
        public ValidateAntiForgeryTokenControllerLevelAttribute(HttpVerbs verbs, params string[] actionsToNotValidate)
        {
            // use custom order to make sure this filter runs after any authorization filters
            // otherwise the principal on the thread/request will not be set and the anti forgery check will fail
            Order = 100;

            _verbs = new AcceptVerbsAttribute(verbs);

            if(actionsToNotValidate != null )
            {
                _actionsToNotValidate = actionsToNotValidate.Select(x => x.ToLower()).ToArray();
            }
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            
            var httpMethodOverride = request.GetHttpMethodOverride();
            if (_verbs.Verbs.Contains(httpMethodOverride, StringComparer.OrdinalIgnoreCase) &&
                !_actionsToNotValidate.Contains(filterContext.ActionDescriptor.ActionName.ToLower()))
            {
                try
                {
                    // This is required for scenarios where you post JSON because the Antiforgery helper 
                    // only looks in the FORM for it. We need to pull the token from the header instead.
                    var cookieToken = request.Cookies[AntiForgeryConfig.CookieName];
                    const string tokenId = "__RequestVerificationToken";
                    var headerToken = /*FORM*/ request.Form[tokenId] ?? /*JSON*/ request.Headers[tokenId];
                    AntiForgery.Validate(cookieToken != null ? cookieToken.Value : null, headerToken);
                }
                catch (HttpAntiForgeryException e)
                {
                    // intercept this exception here and throw a richer equivalent to provide more details on where the problem happened
                    throw new HttpAntiForgeryException(e.Message +
                                                       " Controller: " +
                                                       filterContext.ActionDescriptor.ControllerDescriptor
                                                           .ControllerName +
                                                       ". Action: " + filterContext.ActionDescriptor.ActionName +
                                                       ". Probable cause: " +
                                                       GetProbableCauseOfValidationFailure(filterContext.HttpContext));
                }
            }
        }

        /// <summary>
        /// This a mini-reimplementation of MVC's internal and sealed AntiForgeryWorker.Validate check of XSRF rules.
        /// It does not actually performs the full XSRF validation but attempts to analyze the XSRF cookie and the passed in
        /// form token values, and figure out why the out of the box Mvc implementation thinks we have a validation problem.
        /// </summary>
        private static string GetProbableCauseOfValidationFailure(HttpContextBase context)
        {
            // cookie might be missing if there are no cookies or no special XSRF cookie starting with "__RequestVerificationToken"
            // notice full cookie name will have a suffix we don't know at this point
            if( context.Request.Cookies.Count == 0  || !context.Request.Cookies.AllKeys.Any(x => x.StartsWith(AntiForgeryTokenFieldName)) )
            {
                return "XSRF cookie is missing. Make sure XSRF token is generated somewhere on the page at least once using Html.AntiForgeryToken() helper. " +
                    "This will also set the cookie. However we already have Html.AntiForgeryToken() called in a very promiment partial view so something " +
                    "went pretty unusual here...";
            }

            // no token was passed in the request
            var numberOfXsrfTokenValuesInFormRequest = context.Request.Form.AllKeys.Count(x => x == AntiForgeryTokenFieldName);
            if (numberOfXsrfTokenValuesInFormRequest == 0)
            {
                return "No XSRF token " + AntiForgeryTokenFieldName + " found in the form request. Add it to the form/POST using Html.AntiForgeryToken() helper " +
                "and/or adjust your AJAX submit so it POSTs " + AntiForgeryTokenFieldName + " with its value.";
            }

            // must have only 1 token passed in as a form field. if 2 or more are passed, their values will be concatenated
            // and Mvc implementation will fail to decode Base64 encoded value of the token.
            // actually 2 fields most likely won't come in separately in the form at this point, so it's a just in case check.
            // if 2 fields were submitted in the same form, most likely the next if will trigger
            if (numberOfXsrfTokenValuesInFormRequest > 1)
            {
                return "More than one " + AntiForgeryTokenFieldName + " parameter in the form request. You can have multiple invokations of Html.AntiForgeryToken() on a page, " +
                       "but only 1 in a form. Make sure your HTML form has only one such hidden field generated via Html.AntiForgeryToken() helper.";
            }

            // check if the token value is an invalid Base64 string
            var tokenValue = context.Request.Form[AntiForgeryTokenFieldName];
            try
            {
                var notUsed = Convert.FromBase64String(tokenValue);
            }
            catch (Exception)
            {
                return "XSRF token is passed with corrupt Base64 value. This could have happened for 2 reasons: " +
                    "1) More than one " + AntiForgeryTokenFieldName + " field was passed in the form. You can have multiple invokations " +
                    "of Html.AntiForgeryToken() on a page, but only 1 per form. 2) There is 1 value passed in but it got corrupted during AJAX post " +
                    "because it was not URL encoded prior to AJAX call. Use anti forgery JavaScript functions to get properly encoded " +
                    " XSRF token value for AJAX calls.";
            }
            string cookieName = context.Request.Cookies.AllKeys.First(x => x.StartsWith(AntiForgeryTokenFieldName));
            var cookie = context.Request.Cookies[cookieName];

            return string.Format("Not 100% sure: there is a cookie and 1 XSRF token passed in correctly. But Mvc's XSRF validation failed, potentially because: " +
                "1) token and cookie values mismatched because: 1a) XSRF cookie could become stale; or 1b) This is an actual XSRF attack; " + 
                "2) salt on cookie vs. token mismatched; 3) username on cookie vs. token mismatched. Token Value='{0}', Cookie Value='{1}'",
                tokenValue, cookie != null ? cookie.Value : "No Cookie Found");
        }    
    }
}
