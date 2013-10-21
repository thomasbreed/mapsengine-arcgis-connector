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
using System.Text;
using System.IO;
using log4net;
using log4net.Config;

namespace com.google.mapsengine.connectors.arcgis.Extension.Tools
{
    public class ToolsCommand_Signin : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        // setup and configure log4net
        protected static readonly ILog log = LogManager.GetLogger(typeof(ToolsCommand_Signin));

        // keep a reference to the extension for state management
        GoogleMapsEngineToolsExtensionForArcGIS ext;

        public ToolsCommand_Signin()
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("ToolsCommand_Signin initializing.");

            // to check the state of the extension, get the extension
            log.Info("Retrieving a reference to the extension object to check state.");
            ext = GoogleMapsEngineToolsExtensionForArcGIS.GetExtension();

            // check to see if the extension is enabled
            log.Debug("Verifying extension is enabled.");
            if (ext.isExtensionEnabled())
            {
                // the extension is enabled, verify the button is also enabled
                log.Debug("Extension is enabled, enable button and check to verify the user has an auth token.");
                this.Enabled = true;

                // check to see if the user has an auth code
                log.Debug("Checking to see if the user has an OAuth code");
                if (ext.isAuthorizationAvailable())
                {
                    // the user has an OAuth code available for use
                    log.Debug("User has an OAuth code available. Check the button.");
                    this.Checked = true;
                }
                else
                {
                    // the user does not have an OAuth code available for use
                    log.Debug("User does not have an OAuth code available. Uncheck the button.");
                    this.Checked = false;
                }

            }
            else
            {
                // disable this button
                log.Debug("The extension is disabled, disable this button.");
                this.Enabled = false;
            }

            // subscribe to extension state change events through the extension
            ext.RaiseExtensionStateChangeEvent += HandleExtensionStateChange;

            // subscribe to Authentication state change events through the extension
            ext.RaiseAuthenticationStateChangeEvent += HandleAuthenticationStateChangeEvent;
        }

        void HandleExtensionStateChange(object sender, Extension.StateChangeEventArgs e)
        {
            // check to see if the extension is enabled
            log.Debug("Verifying extension is enabled.");
            if (e.State)
            {
                // the extension is enabled, verify the button is also enabled
                log.Debug("Extension is enabled, enable button and check to verify the user has an auth token.");
                this.Enabled = true;
            }
            else
            {
                // disable this button
                log.Debug("The extension is disabled, disable this button.");
                this.Enabled = false;
            }
        }

        protected override void OnClick()
        {
            log.Debug("User has clicked the authentication button.");

            try
            {
                // check to make sure the button is not checked (i.e. user has an auth code)
                log.Debug("Verifying the button is checked or not.");
                log.Debug("this.Checked: " + this.Checked);
                if (!this.Checked)
                {
                    // establish a new OAuth2 form
                    log.Debug("Creating a new OAuth2 authorization form.");
                    Extension.Dialogs.Auth.OAuth2AuthForm oauthForm = new Extension.Dialogs.Auth.OAuth2AuthForm(ext);
                    
                    // show the dialog
                    log.Debug("Showing new dialog window.");
                    oauthForm.Show();
                }
                else
                {
                    // sign the user out of the toolbar
                    log.Debug("Sign the user out of the Google Earth Builder tools by removing all token references.");
                    ext.clearToken();
                }
            }
            catch (System.Exception ex)
            {
                log.Debug(ex);
                ext.displayErrorDialog("Unable to change user authentication state.");
            }
        }

        void HandleAuthenticationStateChangeEvent(object sender, Extension.Auth.AuthenticationStateChangeEventArgs e)
        {
            // an authentication event occured, determine which one and act accordingly
            log.Debug("An auth event occured, check which one occured");
            if (e.isAuthorized)
            {
                log.Debug("Authorized event occured.");

                // suppress/check the button
                log.Debug("Setting the button checked state to true.");
                this.Checked = true;

                // update the token object
                log.Debug("Updating the auth token object.");
                this.ext.setToken(e.token);
            }
            else
            {
                log.Info("Non-authorized event occured (may have signed out).");

                // unsupress/uncheck the button
                log.Debug("Setting the button checked state to false.");
                this.Checked = false;
            }
        }

        protected override void OnUpdate()
        {
          
        }
    }
}
