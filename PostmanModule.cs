using System;
using System.Web;
using System.Web.UI;

namespace MisterPostman
{
    /// <summary>
    /// Postman Module.
    /// </summary>
    public class PostmanModule : IHttpModule
    {
        public void Dispose() { }

        public void Init(HttpApplication application)
        {
            application.PreRequestHandlerExecute += new EventHandler(application_PreRequestHandlerExecute);
        }
        void application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            // If request is to a page, creates a Postman activator.
            var page = HttpContext.Current.Handler as Page;
            if(page != null) new PostmanActivator(page);
        }
    }
}