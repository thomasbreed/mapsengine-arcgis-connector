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
     * A class representing an OAuth 2.0 access token provided by
     * accounts.google.com.  Build and stored in memory after a user
     * has successfully authenticated and authorized the Extension for
     * access to their Google Maps Engine account.
     */
    public class OAuth2Token
    {
        #region Variables
        // A unique identifier used to track the transaction state
        // in calls to accounts.google.com
        private System.Guid _state;

        // a string value representing an OAuth 2.0 access token
        private string _access_token;

        // a integer value representing the number of seconds until
        // the access token expires
        private int _expires_in;

        // a date/time object representing when the access token expires
        private DateTime _expires_on;

        // the type of OAuth 2.0 access token returned by
        // accounts.google.com
        private string _token_type; // Always Bearer at this time

        // the piece of code returned by accounts.google.com
        // that can be used to refresh the short lived access token
        private string _refresh_token;

        #endregion

        // default constructor used to build a new OAuth 2.0 token
        public OAuth2Token()
        {
            // build a new date/time object representing now
            System.DateTime now = System.DateTime.UtcNow;

            // set the expires on to now + expires_in
            this._expires_on = now.AddSeconds(this._expires_in);
        }

        #region Getter/Setter Functions

        // a getter/setter function for the unique state identifier
        public System.Guid state
        {
            get { return _state; }
            set { _state = value; }
        }

        // a getter/setter function to access the OAuth 2.0 token
        public string access_token
        {
            get { return _access_token; }
            set { _access_token = value; }
        }

        // a getter/setter function to access the expires in value
        public int expires_in
        {
            get { return _expires_in; }
            set { _expires_in = value; }
        }

        // a getter/setter function to access the expires on date/time object
        public DateTime expires_on
        {
            get { return _expires_on; }
            set { _expires_on = value; }
        }

        // a getter/setter function to access the type of OAuth 2.0 token stored
        public string token_type
        {
            get { return _token_type; }
            set { _token_type = value; }
        }

        // a getter/setter function to access the refresh code stored
        public string refresh_token
        {
            get { return this._refresh_token; }
            set { this._refresh_token = value; }
        }

        #endregion
    }
}
