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

using Google.Apis.Mirror.v1;
using Google.Apis.Services;
using MirrorQuickstart.Models;
using System;
using System.Web.Mvc;

namespace MirrorQuickstart.Controllers
{
    /// <summary>
    /// Abstract class providing a utility method to load the current user credentials.
    /// </summary>
    public abstract class AuthRequiredController : Controller
    {
        protected MirrorService Service { get; set; }
        protected String UserId { get; set; }

        /// <summary>
        /// Initializes this Controller with the current user ID and an authorized Mirror service
        /// instance.
        /// </summary>
        /// <returns>Whether or not initialization succeeded.</returns>
        protected Boolean Initialize()
        {
            UserId = Session["userId"] as String;
            if (String.IsNullOrEmpty(UserId))
            {
                return false;
            }

            var state = Utils.GetStoredCredentials(UserId);
            if (state == null)
            {
                return false;
            }

            Service = new MirrorService(new BaseClientService.Initializer()
            {
                Authenticator = Utils.GetAuthenticatorFromState(state)
            });

            return true;
        }
    }
}
