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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using log4net;
using log4net.Config;

namespace com.google.mapsengine.connectors.arcgis.Extension.Dialogs.Auth
{
    public partial class OAuth2AuthForm : Form
    {
        // setup and configure log4net
        protected static ILog log = LogManager.GetLogger(typeof(OAuth2AuthForm));

        // establish a link back to the Extension object
        GoogleMapsEngineToolsExtensionForArcGIS ext = null;

        // a boolean to track if the user is requesting view only access
        bool isViewOnly = false;

        /*
         * Constructor for the OAuth 2.0 windows form
         */
        public OAuth2AuthForm(GoogleMapsEngineToolsExtensionForArcGIS ext)
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("OAuth2AuthForm initializing.");

            // initialize the local extension object
            log.Debug("Initializing the reference to the Extension.");
            this.ext = ext;

            // initialize the components
            log.Debug("Initialized form components.");
            InitializeComponent();
        }

        /*
         * A class representing when the browser is navigated by the system or user
         */
        private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            // for debugging only, if logging set to DEBUG, display title on form
            log.Debug("Determining if the log mode is debug or not.");
            if (log.IsDebugEnabled)
            {
                // set the browser's title to the windows form title
                log.Debug("Setting the form's title to the actual title of the browser.");
                this.Text = "(title = " + this.webBrowser.DocumentTitle.ToString() + ", url = " + this.webBrowser.Url + ")";
            }

            // check to see if the URI is an approval from Google Accounts
            log.Debug("Checking to see if the URI, back from the sever, contains .../approval/");
            log.Debug("AbsoluteUri: " + e.Url.AbsoluteUri);
            log.Debug("Title: " + this.webBrowser.DocumentTitle.ToString());
            if (e.Url.AbsoluteUri.StartsWith("https://accounts.google.com/o/oauth2/approval"))
            {
                log.Debug("URL does contain .../approval/");

                // fetch the title from the web browser
                log.Debug("Fetching the title from the browser object.");
                string title = this.webBrowser.DocumentTitle.ToString();

                // immediately close the browser window
                log.Debug("Closing the browser window");
                this.Close();

                // decode the title of the browser and retrieve the token object
                log.Debug("Decoding the title string response from the server into a Token object.");
                log.Debug("Title: " + title);
                Extension.Auth.OAuth2Token token = Extension.Auth.OAuth2Utilities.decodeTitleResponse(ref log, title);

                if (token != null)
                {
                    // raise authentication event, success
                    log.Debug("Raising an authentication event, Authorized.");
                    ext.publishRaiseAuthenticationStateChangeEvent(true, token);
                }
                else
                {
                    // raise authentication event, failed
                    log.Debug("Raising an authentication event, Unauthorized.");
                    ext.publishRaiseAuthenticationStateChangeEvent(false, null);
                }
            }
            else
            {
                // the title did not show /approval/, raise an unauthorized event
                log.Debug("Raising an authentication event, Unauthorized.");
                ext.publishRaiseAuthenticationStateChangeEvent(false, null);
            }
        }

        /*
         * Loading of the OAuth 2.0 windows form
         */
        private void OAuth2AuthForm_Load(object sender, EventArgs e)
        {
            // initialize the web browser
            log.Debug("OAuth2AuthForm_Load");

            // set the initial state of the web browser to the OAuth 2.0 Sign-in page
            log.Debug("Setting the URI of the web browser.");
            this.webBrowser.Url = Extension.Auth.OAuth2Utilities.buildAuthenticationUri(ref log, Properties.Settings.Default.gme_auth_editscopes);

            log.Debug("URI: " + webBrowser.Url);
        }
    }
}
