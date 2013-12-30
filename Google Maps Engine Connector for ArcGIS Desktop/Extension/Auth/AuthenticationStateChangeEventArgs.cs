/*
Copyright 2013 Google Inc

Licensed under the Apache License, Version 2.0(the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.google.mapsengine.connectors.arcgis.Extension.Auth
{
    /*
     * A customized event argument class used to maintain the state of user's 
     * authentication
     */
    public class AuthenticationStateChangeEventArgs : EventArgs
    {
        #region Variables
        // an OAuth 2.0 object
        private OAuth2Token _token;

        // a boolean to track the state of a user's authentication
        private bool _isAuthorized;

        #endregion

        // constructor containing both the OAuth token object and a initial state
        public AuthenticationStateChangeEventArgs(bool isAuthorized, OAuth2Token token)
        {
            this. _isAuthorized = isAuthorized;
            this._token = token;
        }

        // constructor containing only the initial state
        public AuthenticationStateChangeEventArgs(bool isAuthorized)
        {
            this._isAuthorized = isAuthorized;
            this._token = null;
        }

        #region Getter/Setter Functions

        // a getter/setter function for the OAuth 2.0 token
        public OAuth2Token token
        {
            get { return _token; }
            set { _token = value; }
        }

        // a getter/setter function for the boolean state
        public bool isAuthorized
        {
            get { return _isAuthorized; }
            set { _isAuthorized = value; }
        }

        #endregion
    }
}
