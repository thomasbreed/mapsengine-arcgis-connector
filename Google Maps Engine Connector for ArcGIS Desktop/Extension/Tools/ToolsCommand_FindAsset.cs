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
    public class ToolsCommand_FindAsset : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        // setup and configure log4net
        protected static readonly ILog log = LogManager.GetLogger(typeof(ToolsCommand_FindAsset));

        GoogleMapsEngineToolsExtensionForArcGIS ext;

        private String selectedProjectId;

        public ToolsCommand_FindAsset()
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("ToolsCommand_FindAsset initializing.");

            // to check the state of the extension, get the extension
            log.Info("Retrieving a reference to the extension object to check state.");
            ext = GoogleMapsEngineToolsExtensionForArcGIS.GetExtension();

            // check to see if the extension is enabled
            log.Debug("Verifying extension is enabled.");
            if (!ext.isExtensionEnabled())
            {
                this.Enabled = false;
            }
            else
            {
                if (!ext.isAuthorizationAvailable())
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
            if (e.State && ext.isAuthorizationAvailable())
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

        void HandleAuthenticationStateChangeEvent(object sender, Extension.Auth.AuthenticationStateChangeEventArgs e)
        {
            // an authentication event occured, determine which one and act accordingly
            if (e.isAuthorized)
            {
                // suppress/check the button
                this.Enabled = true;
            }
            else
            {
                // unsupress/uncheck the button
                this.Enabled = false;
            }
        }

        protected override void OnClick()
        {
            try
            {
                // verify there is a valid
                if (ext.isAuthorizationAvailable())
                {
                    Extension.Dialogs.Directory.GoogleMapsEngineDirectoryListing directory = new Extension.Dialogs.Directory.GoogleMapsEngineDirectoryListing();
                    //Extension.Dialogs.Directory.MapDirectory directory = new Extension.Dialogs.Directory.MapDirectory(ext);
                    if (!String.IsNullOrEmpty(selectedProjectId))
                    {
                        directory.SelectedProjectId = selectedProjectId;
                    }
                    directory.ShowDialog();
                    selectedProjectId = directory.SelectedProjectId;
                }
                else
                {
                    // no token exists
                }
            }
            catch (System.Exception ex)
            {
                ext.displayErrorDialog("Unable to display Google Earth Builder directory.  " + ex.Message);
            }
        }

        protected override void OnUpdate()
        {
        }
    }
}
