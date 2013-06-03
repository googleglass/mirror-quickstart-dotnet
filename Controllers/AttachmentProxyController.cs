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

using Google.Apis.Mirror.v1.Data;
using System;
using System.Net;
using System.Web.Mvc;

namespace MirrorQuickstart.Controllers
{
    public class AttachmentProxyController : AuthRequiredController
    {
        //
        // GET: /attachmentproxy

        public ActionResult GetAttachment(String timelineItem, String attachment)
        {
            if (!Initialize())
            {
                return Redirect(Url.Action("StartAuth", "Auth"));
            }

            Attachment attachmentMetadata =
                Service.Timeline.Attachments.Get(timelineItem, attachment).Fetch();

            HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create(attachmentMetadata.ContentUrl);
            Service.Authenticator.ApplyAuthenticationToRequest(request);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return new FileStreamResult(
                    response.GetResponseStream(), attachmentMetadata.ContentType);
            }
            else
            {
                return HttpNotFound();
            }
        }

    }
}
