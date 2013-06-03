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
using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using System;
using System.Linq;

namespace MirrorQuickstart.Models
{
    public class Utils
    {

        /// <summary>
        /// Retrieve an IAuthenticator instance using the provided state.
        /// </summary>
        /// <param name="credentials">OAuth 2.0 credentials to use.</param>
        /// <returns>Authenticator using the provided OAuth 2.0 credentials</returns>
        public static IAuthenticator GetAuthenticatorFromState(IAuthorizationState credentials)
        {
            var provider =
                new StoredStateClient(
                    GoogleAuthenticationServer.Description, Config.CLIENT_ID, Config.CLIENT_SECRET,
                    credentials);
            var auth =
                new OAuth2Authenticator<StoredStateClient>(provider, StoredStateClient.GetState);
            auth.LoadAccessToken();
            return auth;
        }

        /// <summary>
        /// Retrieved stored credentials for the provided user ID.
        /// </summary>
        /// <param name="userId">User's ID.</param>
        /// <returns>Stored GoogleAccessProtectedResource if found, null otherwise.</returns>
        public static IAuthorizationState GetStoredCredentials(String userId)
        {
            StoredCredentialsDBContext db = new StoredCredentialsDBContext();
            StoredCredentials sc = db.StoredCredentialSet.FirstOrDefault(x => x.UserId == userId);
            if (sc != null)
            {
                return new AuthorizationState()
                {
                    AccessToken = sc.AccessToken,
                    RefreshToken = sc.RefreshToken
                };
            }
            return null;
        }

        /// <summary>
        /// Store OAuth 2.0 credentials in the application's database.
        /// </summary>
        /// <param name="userId">User's ID.</param>
        /// <param name="credentials">The OAuth 2.0 credentials to store.</param>
        public static void StoreCredentials(String userId, IAuthorizationState credentials)
        {
            StoredCredentialsDBContext db = new StoredCredentialsDBContext();
            StoredCredentials sc = db.StoredCredentialSet.FirstOrDefault(x => x.UserId == userId);
            if (sc != null)
            {
                sc.AccessToken = credentials.AccessToken;
                sc.RefreshToken = credentials.RefreshToken;
            }
            else
            {
                db.StoredCredentialSet.Add(new StoredCredentials
                {
                    UserId = userId,
                    AccessToken = credentials.AccessToken,
                    RefreshToken = credentials.RefreshToken
                });
            }
            db.SaveChanges();
        }


        /// <summary>
        /// Extends the NativeApplicationClient class to allow setting of a custom
        /// IAuthorizationState.
        /// </summary>
        public class StoredStateClient : NativeApplicationClient
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StoredStateClient"/> class.
            /// </summary>
            /// <param name="authorizationServer">The token issuer.</param>
            /// <param name="clientIdentifier">The client identifier.</param>
            /// <param name="clientSecret">The client secret.</param>
            public StoredStateClient(AuthorizationServerDescription authorizationServer,
                String clientIdentifier,
                String clientSecret,
                IAuthorizationState state)
                : base(authorizationServer, clientIdentifier, clientSecret)
            {
                this.State = state;
            }

            public IAuthorizationState State { get; private set; }

            /// <summary>
            /// Returns the IAuthorizationState stored in the StoredStateClient instance.
            /// </summary>
            /// <param name="provider">OAuth2 client.</param>
            /// <returns>The stored authorization state.</returns>
            static public IAuthorizationState GetState(StoredStateClient provider)
            {
                return provider.State;
            }
        }
    }
}