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
using log4net;
using log4net.Config;
using System.Net;
using Newtonsoft.Json;

namespace com.google.mapsengine.connectors.arcgis.Extension.Auth
{
    /*
     * This utility class helps work through the OAuth 2.0 installed application workflow
     * https://developers.google.com/accounts/docs/OAuth2InstalledApp
     * 1) Display web browser
     * 2) Direct user to https://accounts.google.com/o/oauth2/auth
     * 3) User sign-in and application authorization
     * 4) Google changes title to "Success code=" when auth'd
     * 5) Utility parses title for code, then stores, closes browser
     * 6) Code used to retrieve temporary access_token(s) when necessary
     * 7) User sign-out destroys code and temporary access_token(s)
     */
    public static class OAuth2Utilities
    {
        /*
         * A utility class used to initialize the token object stored
         * in a user's profile (occures when a user is signed-in but opens a new
         * window)
         */
        public static OAuth2Token getToken(ref ILog log)
        {
            log.Debug("getToken starting.");

            // surround by try/catch just in case there is an issue fetching a token
            try
            {
                // establish a new OAuth2Token object
                log.Debug("Establishing a new OAuth2 Token Object.");
                OAuth2Token token = new OAuth2Token();

                // fill the OAuth2 Token Object with properties from the user's profile
                log.Debug("Retrieving the user's settings profile and updating the OAuth2 Token object.");
                token.access_token = Properties.Settings.Default.oauth2_user_access_token;
                log.Debug("access_token= " + token.access_token);
                token.expires_in = Properties.Settings.Default.oauth2_user_expires_in;
                log.Debug("expires_in= " + token.expires_in);
                token.expires_on = Properties.Settings.Default.oauth2_user_expires_on;
                log.Debug("expires_on= " + token.expires_on);
                token.token_type = Properties.Settings.Default.oauth2_user_token_type;
                log.Debug("token_type= " + token.token_type);
                token.refresh_token = Properties.Settings.Default.oauth2_user_refresh_token;
                log.Debug("refresh_token= " + token.refresh_token);

                // return the token
                log.Debug("Returning the token object.");
                return token;
            }
            catch (System.Exception ex)
            {
                // an error occured, log the error
                log.Error(ex);

                // throw the error to the requesting function
                throw new System.Exception(Properties.Resources.auth_getToken_error_unknown);
            }
        }

        /*
         * Utility function used to set/save the OAuth 2.0 in the user's profile for
         * longer term storage
         */
        public static void setToken(ref ILog log, OAuth2Token token)
        {
            log.Debug("setToken start.");

            // surround with try/catch in case there is an issue setting the values in the user's profile
            try
            {
                // update the user's profile with the Token Object
                log.Debug("Updating the user's profile based on the OAuth2Token object.");
                Properties.Settings.Default.oauth2_user_access_token = token.access_token;
                log.Debug("oauth2_user_access_token= " + token.access_token);
                Properties.Settings.Default.oauth2_user_expires_in = token.expires_in;
                log.Debug("oauth2_user_expires_in= " + token.expires_in);
                Properties.Settings.Default.oauth2_user_expires_on = token.expires_on;
                log.Debug("oauth2_user_expires_on= " + token.expires_on);
                Properties.Settings.Default.oauth2_user_token_type = token.token_type;
                log.Debug("oauth2_user_token_type= " + token.token_type);
                Properties.Settings.Default.oauth2_user_refresh_token = token.refresh_token;
                log.Debug("oauth2_user_refresh_token= " + token.refresh_token);

                // save the user's profile
                log.Debug("Saving the user's settings profile.");
                Properties.Settings.Default.Save();
            }
            catch (System.Exception ex)
            {
                // there was an error saving the token to the user's profile
                log.Error(ex);

                // throw the error to the requestor
                throw new System.Exception(Properties.Resources.auth_setToken_error_unknown);
            }
        }

        /*
         * Utility function used to clear the token from memory and
         * from the user's profile (longer term)
         */
        public static void clearToken(ref ILog log)
        {
            // attempt to clear the token
            try
            {
                // TODO: Sign the user out of Google.com services (de-authorize)

                // clear the OAuth2 properties stored in the users profile
                Properties.Settings.Default.oauth2_user_code = "";
                Properties.Settings.Default.oauth2_user_access_token = "";
                Properties.Settings.Default.oauth2_user_expires_in = 0;
                Properties.Settings.Default.oauth2_user_expires_on = System.DateTime.UtcNow.AddDays(-1);
                Properties.Settings.Default.oauth2_user_token_type = "";
                Properties.Settings.Default.oauth2_user_refresh_token = "";

                // save the updated properties to clear the settings
                Properties.Settings.Default.Save();
            }
            catch (System.Exception ex)
            {
                // there was an error saving to the to the user's profile
                log.Error(ex);

                // throw the error to the requestor
                throw new System.Exception(Properties.Resources.auth_clearToken_error_unknown);
            }
        }


        /*
         * Utility to check if a token exists or not
         */
        public static bool doesTokenExist(ref ILog log)
        {
            // verify the code parameter, stored in the users profile, is valid
            if (Properties.Settings.Default.oauth2_user_code != null
                && Properties.Settings.Default.oauth2_user_code.Length > 0)
                return true;
            else
                return false;
        }

        /*
         * Utility function to check if the user's profile token
         * is expired or not
         */
        public static bool isTokenExpired(ref ILog log)
        {
            // verify the following parameters
            /// Valid OAuth2 Code Exists
            /// Valid OAuth2 Temporary Token Exists
            /// Valid OAuth2 Temporary Token has not expired
            if (Properties.Settings.Default.oauth2_user_code != null
                && Properties.Settings.Default.oauth2_user_access_token != null
                && Properties.Settings.Default.oauth2_user_expires_on != null
                && Properties.Settings.Default.oauth2_user_code.Length > 0
                && Properties.Settings.Default.oauth2_user_access_token.Length > 0
                && Properties.Settings.Default.oauth2_user_expires_on > System.DateTime.UtcNow)
                return false;
            else
                return true;
        }

        /*
         * Utility funciton used to check if the in-memory token object
         * is expired or not
         */
        public static bool isTokenExpired(ref ILog log, OAuth2Token token)
        {
            // verify the following parameters
            /// Valid OAuth2 Code Exists
            /// Valid OAuth2 Temporary Token Exists
            /// Valid OAuth2 Temporary Token has not expired
            if (token != null
                && token.access_token != null
                && token.expires_in > 0
                && token.access_token.Length > 0
                && token.expires_on > System.DateTime.UtcNow)
                return false;
            else
                return true;
        }

        /*
         * Utility function to build a URL to request
         * access and scope for Google Maps Engine.
         */
        public static Uri buildAuthenticationUri(ref ILog log, string scopes)
        {
            // setup and build the Google OAuth2 Authentication URL
            log.Debug("Retreiving auth URL stub from application settings.");
            string oAuthAuthenticationUrl = Properties.Settings.Default.oauth2_settings_auth_url;
            log.Debug("oAuthAuthenticationUrl = " + oAuthAuthenticationUrl);

            // build the query string portion of the URL
            log.Debug("Starting to build the query portion of the auth request URL");
            string query = "";

            // add the response_type to the query string
            log.Debug("response_type=" + "code");
            query += "response_type=" + "code";

            // add the client_id to the query string
            log.Debug("client_id=" + Properties.Settings.Default.oauth2_settings_clientId);
            query += "&client_id=" + Properties.Settings.Default.oauth2_settings_clientId;

            // add the redirect_uri to the query string
            log.Debug("redirect_uri=urn:ietf:wg:oauth:2.0:oob");
            query += "&redirect_uri=" + "urn:ietf:wg:oauth:2.0:oob";

            // add the scope to the query string
            log.Debug("scope=" + scopes);
            query += "&scope=" + scopes;

            // add the approval_prompt to the query string
            log.Debug("approval_prompt=" + "auto");
            query += "&approval_prompt=" + "auto";

            // set the URi of the web browser
            log.Debug("Returning the auth url.");
            log.Debug(oAuthAuthenticationUrl + "?" + query);
            return new Uri(oAuthAuthenticationUrl + "?" + query);
        }

        /*
         * Utility function used to decode the browser title upon
         * successful authentication by the user
         */
        public static OAuth2Token decodeTitleResponse(ref ILog log, string title)
        {
            log.Debug("Browser title = \"" + title + "\"");

            if (!String.IsNullOrEmpty(title))
            {
                // determine if the request was successful
                if (title.StartsWith("Success"))
                {
                    // the browser title starts with success, so the user must be authenticated
                    // deconstruct and extract the code snippet from the browser title
                    log.Debug("Browser title starts with success, deconstructing title...");

                    // grab the code from the query string
                    log.Debug("Finding the start and end location for the code= portion of the title");
                    int startindex = title.IndexOf("code=") + 5;
                    int endindex = title.Length - startindex;

                    // extract the code from the title
                    string code = title.Substring(startindex, endindex);
                    log.Debug("code=" + code);

                    // store the code in this users properties
                    // TODO: Encrypt storage of auth token while in user's profile
                    log.Debug("Setting and storing the new code in the user's profile");
                    Properties.Settings.Default.oauth2_user_code = code;

                    // save the user's profile settings
                    log.Debug("Saving the user's profile");
                    Properties.Settings.Default.Save();

                    // immediately trade in code for token, as it expires
                    log.Debug("Trading in the short-term code for a longer-term responce and access token.  Then return it to the requestor.");
                    return tradeCodeForToken(ref log, code);

                    // raise authorized event
                    //OnRaiseCustomEvent(new CustomEventArgs("Authorized"));
                }
                else if (title.StartsWith("Denied"))
                {
                    // Response from accounts.google.com was Denied, do nothing by responding with null
                    log.Debug("Response from accounts.google.com was \"Denied\"");
                    return null;
                }
                else
                {
                    // all other scenarios (connection issues, proxy issues) where title is not shown, return null
                    log.Debug("Response from accounts.google.com was not Success or Denied.  Throwing error.");

                    // throw an error
                    throw new System.Exception(Properties.Resources.auth_decodeTitleResponse_error_unknown);
                }
            }
            else
            {
                // the title was either null or did not contain any characters
                log.Error("Title response from server was either null or contained no characters.");

                // throw an error
                throw new System.Exception(Properties.Resources.auth_decodeTitleResponse_error_unknown);
            }
        }

        /*
         * Utility function to trade in a code snippet for a valid OAuth 2.0 token
         */
        public static OAuth2Token tradeCodeForToken(ref ILog log, string code)
        {
            log.Debug("Attempting to trade in a code snippet for an OAuth 2.0 access token.");
            log.Debug("code=" + code);

            // attempt to trade in the code snippet for token
            try
            {
                // build a request Url to trade the code for a shortlived token
                log.Debug("Buliding the base URL to trade in the code for a token.");
                string oAuthTokenUrl = Properties.Settings.Default.oauth2_settings_token_url;
                log.Debug(oAuthTokenUrl);

                // build the query string for the trade
                log.Debug("Building the query portion of the URL");
                string query = "";

                // set the code parameter of the query
                log.Debug("code=" + code);
                query += "code=" + code;

                // set the client_id parameter of the query
                log.Debug("client_id=" + Properties.Settings.Default.oauth2_settings_clientId);
                query += "&client_id=" + Properties.Settings.Default.oauth2_settings_clientId;

                // set the client_secret parameter of the query 
                log.Debug("client_secret=" + Properties.Settings.Default.oauth2_settings_client_secret);
                query += "&client_secret=" + Properties.Settings.Default.oauth2_settings_client_secret;
                // TODO: Encode and decode client secret

                // set the redirect_uri parameter of the query
                log.Debug("redirect_uri=" + "urn:ietf:wg:oauth:2.0:oob");
                query += "&redirect_uri=" + "urn:ietf:wg:oauth:2.0:oob";

                // set the grant_type paramter of the query
                log.Debug("grant_type=" + "authorization_code");
                query += "&grant_type=" + "authorization_code";

                // create a URI of the url and query string
                log.Debug("Creating a URI object using the base URL");
                Uri oAuthTokenExchangeUri = new Uri(oAuthTokenUrl);
                log.Debug(oAuthTokenExchangeUri);

                // create a byte array to hold the contents of the post
                log.Debug("Encoding the query portion of the POST.");
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                byte[] payload = encoding.GetBytes(query);

                // setup an HTTP Web Request method
                log.Debug("Creating a new HttpWebRequest object to make the POST request.");
                WebRequest tokenWebRequest = HttpWebRequest.Create(oAuthTokenExchangeUri);

                // set the request method to POST
                log.Debug("Setting the HTTP method to POST");
                tokenWebRequest.Method = "POST";

                // set the content type
                log.Debug("Setting the content type to \"application/x-www-form-urlencoded\"");
                tokenWebRequest.ContentType = "application/x-www-form-urlencoded";

                // set the user agent, for trouble shooting
                log.Debug("Setting the user agent for this request for identification.");
                ((HttpWebRequest)tokenWebRequest).UserAgent = "Google Maps Engine Connector for ArcGIS";

                // set the content length
                log.Debug("Setting the content length of the POST payload/body.");
                tokenWebRequest.ContentLength = payload.Length;

                // TODO: Handle local proxy servers

                // get the request stream to write the query bytes to
                log.Debug("Getting the data stream for this POST request.");
                System.IO.Stream dataStream = tokenWebRequest.GetRequestStream();

                // write the query payload to the data stream
                log.Debug("Attempting to write the payload to the POST stream.");
                dataStream.Write(payload, 0, payload.Length);

                // close the data stream
                log.Debug("Closing the POST data stream.");
                dataStream.Close();

                // get the HTTP response
                log.Debug("Fetching the HTTP response from this request.");
                using (WebResponse tokenWebResponse = tokenWebRequest.GetResponse())
                {
                    // verify the response status was OK
                    log.Debug("Checking the HTTP status of the response to make sure it is valid.");
                    log.Debug("StatusCode=" + ((HttpWebResponse)tokenWebResponse).StatusCode);
                    if (((HttpWebResponse)tokenWebResponse).StatusCode == HttpStatusCode.OK)
                    {
                        // setup a stream reader to read the response from the server
                        log.Debug("Establishing a data reader to retreive the server response.");
                        System.IO.StreamReader reader = new System.IO.StreamReader(tokenWebResponse.GetResponseStream());

                        // read the response into a local variable
                        log.Debug("Reading to the end of the server's response.");
                        string response = reader.ReadToEnd();

                        // close the response stream from the server
                        log.Debug("Closing the server response object.");
                        tokenWebResponse.Close();

                        // deserialize response into a token object
                        log.Debug("Deserializing (JSON to C# object) the server's response");
                        OAuth2Token token = DeserializeResponseToken(ref log, response);
                        log.Debug("Deserialization complete and token object created.");

                        // store the token in the users profile
                        log.Debug("Save and set the token object in the user's profile.");
                        setToken(ref log, token);

                        // setup a token object (decode from JSON to object)
                        log.Debug("Returning the valid token object decoded.");
                        return token;
                    }
                    else
                    {
                        // log an error
                        log.Debug("The response from the server was not OK.");

                        // throw a new exception
                        throw new System.Exception(Properties.Resources.auth_tokenexchange_error_404);
                    }
                }
            }
            catch (System.Exception ex)
            {
                // log an error
                log.Error(ex);

                // throw an unknown error
                throw new System.Exception(Properties.Resources.auth_tokenexchange_error_unknown);
            }
        }

        public static OAuth2Token tradeRefreshForToken(ref ILog log, string code)
        {
            // attempting to trade in a refresh code for an auth token
            log.Debug("Attempting to trade in a refresh code for another auth code.");
            log.Debug("code=" + code);

            // attempt to trade in
            try
            {
                // build a request Url to trade the code for a shortlived token
                log.Debug("Building a base URL to exchange token for code.");
                string oAuthTokenUrl = Properties.Settings.Default.oauth2_settings_token_url;

                // build the query string for the trade
                log.Debug("Establishing the new query parameters for request.");
                string query = "";

                // set the client_id parameter of the query
                log.Debug("client_id=" + Properties.Settings.Default.oauth2_settings_clientId);
                query += "client_id=" + Properties.Settings.Default.oauth2_settings_clientId;

                // set the client_secret parameter of the query 
                log.Debug("client_secret=" + Properties.Settings.Default.oauth2_settings_client_secret);
                query += "&client_secret=" + Properties.Settings.Default.oauth2_settings_client_secret;
                // TODO: Encode and decode client secret

                // set the refresh_token = to the code
                log.Debug("refresh_token=" + code);
                query += "&refresh_token=" + code;

                // set the grant_type paramter of the query
                log.Debug("grant_type=" + "refresh_token");
                query += "&grant_type=" + "refresh_token";

                // create a URI of the url and query string
                log.Debug("Building a new URI object for this request.");
                Uri oAuthTokenExchangeUri = new Uri(oAuthTokenUrl);

                // create a byte array to hold the contents of the post
                log.Debug("Encoding the payload (HTTP Post) into bytes.");
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                byte[] payload = encoding.GetBytes(query);

                // setup an HTTP Web Request method
                log.Debug("Creating a new HTTP WebRequest object to make the request.");
                WebRequest tokenWebRequest = HttpWebRequest.Create(oAuthTokenExchangeUri);

                // set the request method to POST
                log.Debug("Setting the HTTP method to POST.");
                tokenWebRequest.Method = "POST";

                // set the content type
                log.Debug("Setting the contentType for this request to \"application/x-www-form-urlencoded\"");
                tokenWebRequest.ContentType = "application/x-www-form-urlencoded";

                // set the user agent, for trouble shooting
                log.Debug("Setting the user agent for this request for identification.");
                ((HttpWebRequest)tokenWebRequest).UserAgent = "Google Maps Engine Connector for ArcGIS";

                // set the content length
                log.Debug("Setting the HTTP Post content length to " + payload.Length);
                tokenWebRequest.ContentLength = payload.Length;

                // TODO: Handle local proxy servers

                // get the request stream to write the query bytes to
                log.Debug("Creating a new stream object to write the HTTP PoST body to the request.");
                System.IO.Stream dataStream = tokenWebRequest.GetRequestStream();

                // write the query payload to the data stream
                log.Debug("Writing the HTTP POST payload the request.");
                dataStream.Write(payload, 0, payload.Length);

                // close the data stream
                log.Debug("Closing the HTTP POST data stream.");
                dataStream.Close();

                // get the HTTP response
                log.Debug("Creating a new response object from the server.");
                using (WebResponse tokenWebResponse = tokenWebRequest.GetResponse())
                {

                    // verify the response status was OK
                    log.Debug("Attempting to determine if the response from the server was OK.");
                    log.Debug("StatusCode=" + ((HttpWebResponse)tokenWebResponse).StatusCode);
                    if (((HttpWebResponse)tokenWebResponse).StatusCode == HttpStatusCode.OK)
                    {
                        log.Debug("Response from the server was OK.");

                        // setup a stream reader to read the response from the server
                        log.Debug("Creating a stream reader object to read the server response payload.");
                        System.IO.StreamReader reader = new System.IO.StreamReader(tokenWebResponse.GetResponseStream());

                        // read the response into a local variable
                        log.Debug("Reading to the end of the content.");
                        string response = reader.ReadToEnd();

                        // close the response stream from the server
                        log.Debug("Closing the response object.");
                        tokenWebResponse.Close();

                        // deserialize response into a token object
                        log.Debug("Deserializing the server response into a local Token object.");
                        OAuth2Token token = DeserializeResponseToken(ref log, response);

                        // update refresh_token to the current code, as it does not come back in the
                        // refresh token response, only as apart of the code exchange
                        log.Debug("Updating the refresh token to code used in this refresh request.");
                        token.refresh_token = code;

                        // store the token in the users profile
                        log.Debug("Storing/saving the token to the user's profile");
                        setToken(ref log, token);

                        // setup a token object (decode from JSON to object)
                        log.Debug("Returning the token.");
                        return token;
                    }
                    else
                    {
                        // log an error
                        log.Debug("The response from the server was not OK.");

                        // throw a new exception
                        throw new System.Exception(Properties.Resources.auth_refreshexchange_error_404);
                    }
                }
            }
            catch (System.Exception ex)
            {
                // log an error
                log.Error(ex);

                // throw an unknown error
                throw new System.Exception(Properties.Resources.auth_refreshexchange_error_unknown);
            }
        }

        private static OAuth2Token DeserializeResponseToken(ref ILog log, string json)
        {
            // deserialize the JSON Token response from Google Accounts into a token object
            log.Debug("Attempting to deserialize the JSON token response from Google Accounts to a local OAuth2Token object.");
            OAuth2Token deserializedToken = JsonConvert.DeserializeObject<OAuth2Token>(json);

            // update the Expires on date time to now + expires_in - some buffer (2-minutes)
            log.Debug("Updating the Expires on to now + expires_in - a 120 threshold (expires 120 seconds early)");
            deserializedToken.expires_on = System.DateTime.UtcNow.AddSeconds(deserializedToken.expires_in - 120);

            // return the token object
            log.Debug("Returning deserialized object.");
            return deserializedToken;
        }

        public static OAuth2Token refreshToken(ref ILog log, OAuth2Token token)
        {
            // if the user has a valid code
            log.Debug("Attempting to determine if the token already exists.");
            if (doesTokenExist(ref log))
            {
                log.Debug("Token exist.");

                // trade in the code for an updated temporary token
                //return tradeCodeForToken(ref log, Properties.Settings.Default.oauth2_user_code);
                log.Debug("Returning the result from a refresh trade in.");
                return tradeRefreshForToken(ref log, Properties.Settings.Default.oauth2_user_refresh_token);
            }
            else
            {
                log.Debug("Does not exist.");

                // clear the token from the user's profile
                log.Debug("Clearing the token from the users profile.");
                clearToken(ref log);

                // throw an error message
                throw new System.Exception(Properties.Resources.auth_refreshToken_error_tokendoesntexist);
            }
        }
    }
}
