/*
 * Copyright (c) 2013 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 * in compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the
 * License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Web.Mvc;
using System.Web.Routing;

namespace MirrorQuickstart
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "auth", // Route name
                "auth", // URL with parameters
                new { controller = "Auth", action = "StartAuth" } // Parameter defaults
            );

            routes.MapRoute(
                "oauth2callback", // Route name
                "oauth2callback", // URL with parameters
                new { controller = "Auth", action = "OAuth2Callback" } // Parameter defaults
            );

            routes.MapRoute(
                "signout", // Route name
                "signout", // URL with parameters
                new { controller = "Auth", action = "Signout" } // Parameter defaults
            );

            routes.MapRoute(
                "attachmentproxy", // Route name
                "attachmentproxy", // URL with parameters
                new { controller = "AttachmentProxy", action = "GetAttachment" } // Parameter defaults
            );

            routes.MapRoute(
                "notify", // Route name
                "notify", // URL with parameters
                new { controller = "Notify", action = "Notify" } // Parameter defaults
            );

            routes.MapRoute(
                "post", // Route name
                "post", // URL with parameters
                new { controller = "Main", action = "Post" } // Parameter defaults
            );

            routes.MapRoute(
                "default", // Route name
                "", // URL with parameters
                new { controller = "Main", action = "Index" } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            // Use LocalDB for Entity Framework by default
            Database.DefaultConnectionFactory = new SqlConnectionFactory(@"Data Source=(localdb)\v11.0; Integrated Security=True; MultipleActiveResultSets=True");

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}