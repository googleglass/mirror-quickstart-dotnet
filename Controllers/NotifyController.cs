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

using Google.Apis.Json;
using Google.Apis.Mirror.v1;
using Google.Apis.Mirror.v1.Data;
using Google.Apis.Services;
using MirrorQuickstart.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;

namespace MirrorQuickstart.Controllers
{
    public class NotifyController : Controller
    {
        //
        // POST: /notify

        [HttpPost]
        public ActionResult Notify()
        {
            Notification notification =
                new NewtonsoftJsonSerializer().Deserialize<Notification>(Request.InputStream);
            String userId = notification.UserToken;
            MirrorService service = new MirrorService(new BaseClientService.Initializer()
            {
                Authenticator = Utils.GetAuthenticatorFromState(Utils.GetStoredCredentials(userId))
            });

            if (notification.Collection == "locations")
            {
                HandleLocationsNotification(notification, service);
            }
            else if (notification.Collection == "timeline")
            {
                HandleTimelineNotification(notification, service);
            }

            return new HttpStatusCodeResult((int)HttpStatusCode.OK);
        }

        /// <summary>
        /// Inserts a timeline item with the latest location information.
        /// </summary>
        /// <param name="notification">Notification payload.</param>
        /// <param name="service">Authorized Mirror service.</param>
        private void HandleLocationsNotification(Notification notification, MirrorService service)
        {
            Location location = service.Locations.Get(notification.ItemId).Fetch();
            TimelineItem item = new TimelineItem()
            {
                Text = "New location is " + location.Latitude + ", " + location.Longitude,
                Location = location,
                MenuItems = new List<MenuItem>() { new MenuItem() { Action = "NAVIGATE" } },
                Notification = new NotificationConfig() { Level = "DEFAULT" }
            };

            service.Timeline.Insert(item).Fetch();
        }

        /// <summary>
        /// Process the timeline collection notification.
        /// </summary>
        /// <param name="notification">Notification payload.</param>
        /// <param name="service">Authorized Mirror service.</param>
        private void HandleTimelineNotification(Notification notification, MirrorService service)
        {
            foreach (UserAction action in notification.UserActions)
            {
                if (action.Type == "SHARE")
                {
                    TimelineItem item = service.Timeline.Get(notification.ItemId).Fetch();

                    item.Text = "Thanks for sharing: " + item.Text;
                    item.Notification = new NotificationConfig() { Level = "DEFAULT" };

                    service.Timeline.Update(item, item.Id).Fetch();
                    // Only handle the first successful action.
                    break;
                }
                else
                {
                    Console.WriteLine(
                        "I don't know what to do with this notification: " + action.ToString());
                }
            }
        }

    }
}
