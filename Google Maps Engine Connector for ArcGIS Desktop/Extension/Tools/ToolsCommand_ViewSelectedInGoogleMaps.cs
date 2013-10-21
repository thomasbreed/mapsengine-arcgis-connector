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
    public class ToolsCommand_ViewSelectedInGoogleMaps : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        // setup and configure log4net
        protected static readonly ILog log = LogManager.GetLogger(typeof(ToolsCommand_ViewSelectedInGoogleMaps));

        // keep a reference to the extension for state management
        GoogleMapsEngineToolsExtensionForArcGIS ext;

        public ToolsCommand_ViewSelectedInGoogleMaps()
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("ToolsCommand_ViewSelectedInGoogleMaps initializing.");

            // to check the state of the extension, get the extension
            log.Info("Retrieving a reference to the extension object to check state.");
            ext = GoogleMapsEngineToolsExtensionForArcGIS.GetExtension();

            // this button will always be disabled, until a selection is made
            log.Debug("Disable button until selection is made, at which time check extension and auth.");
            this.Enabled = false;

            // subscribe to selection change events events through the extension
            ext.RaiseSelectionChangeEvent += HandleSelectionChange;
        }

        void HandleSelectionChange(object sender, Extension.SelectionChangeEventArgs e)
        {
            // check to see if there is a map feature selected
            log.Debug("Verifying map feature selected.");
            if (e.isSelected)
            {
                // verify the extension is enabled and there is auth
                if (ext.isExtensionEnabled() && ext.isAuthorizationAvailable())
                {
                    // there is a map feature selected, enable this button for user to click
                    log.Debug("there is a map feature selected, enable this button for user to click");
                    this.Enabled = true;
                }
                else
                {
                    // the extension or authorization is not enabled, diable button
                    log.Debug("the extension or authorization is not enabled, diable button.");
                    this.Enabled = false;

                }
            }
            else
            {
                // disable this button as no feature selected
                log.Debug("There is no feature selected, disable button.");
                this.Enabled = false;
            }
        }

        protected override void OnClick()
        {
            try
            {
                // fetch the most up-to-date auth
                string auth_token = ext.getToken().access_token;

                // retrieve the url syntax for Google Maps widget
                string urlSyntax = Properties.Settings.Default.gme_url_mapwidget;

                // determine which Maps are selected
                foreach (string MapAssetId in ext.getSelectedMapAssetIds())
                {
                    // get the url
                    string url = urlSyntax;

                    // replace the map identifier
                    url = url.Replace("{mapId}", MapAssetId);

                    // launch the default broswer
                    System.Diagnostics.Process.Start(url);
                }
            }
            catch (System.Exception ex)
            {
                // unable to add GEB layer to map
                log.Error(ex);
                // TODO: Present user with graceful dialog
            }
        }

        protected override void OnUpdate()
        {
        }
    }
}
