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

using DotNetOpenAuth.OAuth2;
using Google;
using Google.Apis.Mirror.v1;
using Google.Apis.Mirror.v1.Data;
using Google.Apis.Services;
using MirrorQuickstart.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MirrorQuickstart.Controllers
{
    public class MainController : AuthRequiredController
    {
        private delegate string Operation(MainController controller);

        /// <summary>
        /// Map of supported operations.
        /// </summary>
        static private Dictionary<String, Operation> Operations =
            new Dictionary<string, Operation>()
        {
            { "insertSubscription", new Operation(InsertSubscription) },
            { "deleteSubscription", new Operation(DeleteSubscription) },
            { "insertItem", new Operation(InsertItem) },
            { "insertItemWithAction", new Operation(InsertItemWithAction) },
            { "insertItemAllUsers", new Operation(InsertItemAllUsers) },
            { "insertContact", new Operation(InsertContact) },
            { "deleteContact", new Operation(DeleteContact) },
            { "deleteTimelineItem", new Operation(DeleteTimelineItem) }
        };

        #region HTTP Request Handlers

        //
        // GET: /Root/

        public ActionResult Index()
        {
            if (!Initialize())
            {
                return Redirect(Url.Action("StartAuth", "Auth"));
            }

            IndexModel rootData = new IndexModel()
            {
                Message = Session["message"] as String
            };
            Session.Remove("message");

            var listRequest = Service.Timeline.List();
            listRequest.MaxResults = 3;
            TimelineListResponse items = listRequest.Fetch();

            rootData.TimelineItems = items.Items;

            try
            {
                Contact dotnetContact = Service.Contacts.Get(".NET Quick Start").Fetch();
                rootData.HasContact = dotnetContact != null;
            }
            catch (GoogleApiRequestException e)
            {
                // 404 on this request is an acceptable error, all the rest should be handled.
                if (e.RequestError.Code != (int)HttpStatusCode.NotFound)
                {
                    throw e;
                }
            }

            SubscriptionsListResponse subscriptions = Service.Subscriptions.List().Fetch();

            rootData.HasTimelineSubscription =
                subscriptions.Items.Any(x => x.Collection == "timeline");
            rootData.HasLocationSubscription =
                subscriptions.Items.Any(x => x.Collection == "locations");

            return View(rootData);
        }

        [HttpPost, ActionName("Post")]
        public ActionResult Post()
        {
            if (!Initialize())
            {
                return Redirect(Url.Action("StartAuth", "Auth"));
            }

            String operation = Request.Form.Get("operation");
            String message = null;

            if (Operations.ContainsKey(operation))
            {
                message = Operations[operation](this);
            }
            else
            {
                message = "I don't know how to " + operation;
            }

            Session["message"] = message;

            return Redirect(Url.Action("Index", "Main"));
        }

        #endregion

        #region Static Mirror API Operations

        /// <summary>
        /// Inserts a new subscription.
        /// </summary>
        /// <param name="controller">Controller calling this method.</param>
        /// <returns>Status message.</returns>
        private static String InsertSubscription(MainController controller)
        {
            String collection = controller.Request.Form.Get("collection");
            if (String.IsNullOrEmpty(collection))
            {
                collection = "timeline";
            }

            Subscription subscription = new Subscription()
            {
                Collection = collection,
                UserToken = controller.UserId,
                CallbackUrl = controller.Url.Action(
                    "Notify", "Notify", null, controller.Request.Url.Scheme)
            };
            controller.Service.Subscriptions.Insert(subscription).Fetch();

            return "Application is now subscribed to updates.";
        }

        /// <summary>
        /// Deletes an existing subscription.
        /// </summary>
        /// <param name="controller">Controller calling this method.</param>
        /// <returns>Status message.</returns>
        private static String DeleteSubscription(MainController controller)
        {
            String collection = controller.Request.Form.Get("subscriptionId");

            controller.Service.Subscriptions.Delete(collection).Fetch();
            return "Application has been unsubscribed.";
        }

        /// <summary>
        /// Inserts a timeline item.
        /// </summary>
        /// <param name="controller">Controller calling this method.</param>
        /// <returns>Status message.</returns>
        private static String InsertItem(MainController controller)
        {
            TimelineItem item = new TimelineItem()
            {
                Text = controller.Request.Form.Get("message"),
                Notification = new NotificationConfig() { Level = "DEFAULT" }
            };
            String mediaLink = controller.Request.Form.Get("imageUrl");

            if (!String.IsNullOrEmpty(mediaLink))
            {
                Stream stream = null;
                if (mediaLink.StartsWith("/"))
                {
                    stream = new StreamReader(controller.Server.MapPath(mediaLink)).BaseStream;
                }
                else
                {
                    HttpWebRequest request = WebRequest.Create(mediaLink) as HttpWebRequest;
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    stream = response.GetResponseStream();
                }

                controller.Service.Timeline.Insert(item, stream, "image/jpeg").Upload();
            }
            else
            {
                controller.Service.Timeline.Insert(item).Fetch();
            }

            return "A timeline item has been inserted.";
        }

        /// <summary>
        /// Inserts a timeline item with action.
        /// </summary>
        /// <param name="controller">Controller calling this method.</param>
        /// <returns>Status message.</returns>
        private static String InsertItemWithAction(MainController controller)
        {
            TimelineItem item = new TimelineItem()
            {
                Creator = new Contact()
                {
                    DisplayName = ".NET Quick Start",
                    Id = "DOTNET_QUICK_START",
                },
                Text = "Tell me what you had for lunch :)",
                Notification = new NotificationConfig() { Level = "DEFAULT" },
                MenuItems = new List<MenuItem>() { { new MenuItem() { Action = "REPLY" } } },
            };
            controller.Service.Timeline.Insert(item).Fetch();
            return "A timeline item with action has been inserted.";
        }

        /// <summary>
        /// Inserts a timeline item to all users (up to 10).
        /// </summary>
        /// <param name="controller">Controller calling this method.</param>
        /// <returns>Status message.</returns>
        private static String InsertItemAllUsers(MainController controller)
        {
            StoredCredentialsDBContext db = new StoredCredentialsDBContext();
            int userCount = db.StoredCredentialSet.Count();

            if (userCount > 10)
            {
                return "Total user count is " + userCount +
                    ". Aborting broadcast to save your quota";
            }
            else
            {
                TimelineItem item = new TimelineItem()
                {
                    Text = "Hello Everyone!",
                    Notification = new NotificationConfig() { Level = "DEFAULT" }
                };

                foreach (StoredCredentials creds in db.StoredCredentialSet)
                {
                    AuthorizationState state = new AuthorizationState()
                    {
                        AccessToken = creds.AccessToken,
                        RefreshToken = creds.RefreshToken
                    };
                    MirrorService service = new MirrorService(new BaseClientService.Initializer()
                    {
                        Authenticator = Utils.GetAuthenticatorFromState(state)
                    });
                    service.Timeline.Insert(item).Fetch();
                }
                return "Successfully sent cards to " + userCount + " users.";
            }
        }

        /// <summary>
        /// Inserts a sharing contact.
        /// </summary>
        /// <param name="controller">Controller calling this method.</param>
        /// <returns>Status message.</returns>
        private static String InsertContact(MainController controller)
        {
            String name = controller.Request.Form.Get("name");
            String imageUrl = controller.Request.Form.Get("imageUrl");

            if (imageUrl.StartsWith("/"))
            {
                UriBuilder builder = new UriBuilder(controller.Request.Url);
                builder.Path = imageUrl;
                imageUrl = builder.ToString();
            }

            Contact contact = new Contact()
            {
                DisplayName = name,
                Id = name,
                ImageUrls = new List<String>() { imageUrl }
            };

            controller.Service.Contacts.Insert(contact).Fetch();

            return "Inserted contact: " + name + ".";
        }

        /// <summary>
        /// Deletes a sharing contact.
        /// </summary>
        /// <param name="controller">Controller calling this method.</param>
        /// <returns>Status message.</returns>
        private static String DeleteContact(MainController controller)
        {
            controller.Service.Contacts.Delete(controller.Request.Form.Get("id")).Fetch();
            return "Contact has been deleted.";
        }

	/// <summary>
        /// Deletes a timeline item.
        /// </summary>
        /// <param name="controller">Controller calling this method.</param>
        /// <returns>Status message.</returns>
        private static String DeleteTimelineItem(MainController controller)
        {
            controller.Service.Timeline.Delete(controller.Request.Form.Get("itemId")).Fetch();
            return "A timeline item has been deleted.";
        }

        #endregion
    }
}
