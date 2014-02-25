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

using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Mirror.v1;
using Google.Apis.Mirror.v1.Data;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using MirrorQuickstart.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MirrorQuickstart.Controllers
{
    public class AuthController : AuthRequiredController
    {

        private static readonly String OAUTH2_REVOKE_ENDPOINT =
            "https://accounts.google.com/o/oauth2/revoke";

        //
        // GET: /auth

        public ActionResult StartAuth()
        {
            UriBuilder builder =
                new UriBuilder(GetProvider().RequestUserAuthorization(GetAuthorizationState()));
            NameValueCollection queryParameters = HttpUtility.ParseQueryString(builder.Query);

            queryParameters.Set("access_type", "offline");
            queryParameters.Set("approval_prompt", "force");

            builder.Query = queryParameters.ToString();

            return Redirect(builder.Uri.ToString());
        }

        //
        // GET: /oauth2callback

        public ActionResult OAuth2Callback(String code)
        {
            var provider = GetProvider();
            try
            {
                var state = provider.ProcessUserAuthorization(code, GetAuthorizationState());
                var initializer = new BaseClientService.Initializer()
                {
                    Authenticator = Utils.GetAuthenticatorFromState(state)
                };

                Oauth2Service userService = new Oauth2Service(initializer);
                String userId = userService.Userinfo.Get().Fetch().Id;

                Utils.StoreCredentials(userId, state);
                Session["userId"] = userId;

                BootstrapUser(new MirrorService(initializer), userId);
            }
            catch (ProtocolException)
            {
                // TODO(alainv): handle error.
            }
            return Redirect(Url.Action("Index", "Main"));
        }

        //
        // POST: /signout

        [HttpPost]
        public ActionResult Signout()
        {
            String userId = Session["userId"] as String;

            if (!String.IsNullOrEmpty(userId))
            {
                StoredCredentialsDBContext db = new StoredCredentialsDBContext();
                StoredCredentials sc =
                    db.StoredCredentialSet.FirstOrDefault(x => x.UserId == userId);

                if (sc != null)
                {
                    // Revoke the token.
                    UriBuilder builder = new UriBuilder(OAUTH2_REVOKE_ENDPOINT);
                    builder.Query = "token=" + sc.RefreshToken;

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(builder.Uri);
                    request.GetResponse();

                    db.StoredCredentialSet.Remove(sc);
                }
                Session.Remove("userId");
            }
            return Redirect(Url.Action("Index", "Main"));
        }

        /// <summary>
        /// Bootstrap the user with a welcome message; if not running locally, this method also
        /// adds a sharing contact and subscribes to timeline notifications.
        /// </summary>
        /// <param name="mirrorService">Authorized Mirror service object</param>
        /// <param name="userId">ID of the user being bootstrapped.</param>
        private void BootstrapUser(MirrorService mirrorService, String userId)
        {
            if (Request.IsSecureConnection && !Request.IsLocal)
            {
                // Insert a subscription.
                Subscription subscription = new Subscription()
                {
                    Collection = "timeline",
                    UserToken = userId,
                    CallbackUrl = Url.Action("notify", "notify", null, Request.Url.Scheme)
                };
                mirrorService.Subscriptions.Insert(subscription).Fetch();

                // Insert a sharing contact.
                UriBuilder builder = new UriBuilder(Request.Url);
                builder.Path = Url.Content("~/Content/img/dotnet.png");
                Contact contact = new Contact()
                {
                    Id = ".NET Quick Start",
                    DisplayName = ".NET Quick Start",
                    ImageUrls = new List<String>() { builder.ToString() }
                };
                mirrorService.Contacts.Insert(contact).Fetch();
            }
            else
            {
                Console.WriteLine("Post auth tasks are not supported on staging.");
            }

            TimelineItem item = new TimelineItem()
            {
                Text = "Welcome to the .NET Quick Start",
                Notification = new NotificationConfig()
                {
                    Level = "DEFAULT"
                }
            };
            mirrorService.Timeline.Insert(item).Fetch();
        }

        /// <summary>
        /// Returns an OAuth 2.0 provider for the authorization flow.
        /// </summary>
        /// <returns>OAuth 2.0 provider.</returns>
        private NativeApplicationClient GetProvider()
        {
            var provider =
                new NativeApplicationClient(
                    GoogleAuthenticationServer.Description, Config.CLIENT_ID,
                    Config.CLIENT_SECRET);
            return provider;
        }

        /// <summary>
        /// Returns a pre-filled OAuth 2.0 state for the authorization flow.
        /// </summary>
        /// <returns>OAuth 2.0 state.</returns>
        private IAuthorizationState GetAuthorizationState()
        {
            var authorizationState = new AuthorizationState(Config.SCOPES);
            authorizationState.Callback = new Uri(Config.REDIRECT_URI);
            return authorizationState;
        }
    }
}
