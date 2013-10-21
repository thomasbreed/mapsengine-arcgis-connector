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
    public class ToolsCommand_CheckForUpdate : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        // setup and configure log4net
        protected static readonly ILog log = LogManager.GetLogger(typeof(ToolsCommand_CheckForUpdate));

        public ToolsCommand_CheckForUpdate()
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("ToolsCommand_CheckForUpdate initializing.");
        }

        protected override void OnClick()
        {
            try
            {
                // to check the state of the extension, get the extension
                log.Info("Retrieving a reference to the extension object to check state.");
                GoogleMapsEngineToolsExtensionForArcGIS ext = GoogleMapsEngineToolsExtensionForArcGIS.GetExtension();

                // check to see if the extension is enabled
                log.Debug("Verifying extension is enabled.");
                if (ext.isExtensionEnabled())
                {
                    // create an update check object
                    log.Debug("Creating an update check object.");
                    Extension.Update.ExtensionUpdateCheck updateCheck = new Extension.Update.ExtensionUpdateCheck();

                    // check to see if there is an update available
                    log.Debug("Checking to see if there is an update available.");
                    if (updateCheck.isUpdateAvailable())
                    {
                        log.Debug("isUpdateAvailable = true");

                        // create a dialog to inform the user of an update
                        log.Debug("Showing the user an OK/Cancel dialog to notify them of an update.");
                        if (System.Windows.Forms.MessageBox.Show("An update to the Google Maps Engine Tools for ArcGIS is avaiable.  Would you like to update the Add-in now?"
                            , "Update Available (" + updateCheck.getLocalVersion().ToString() + " < " + updateCheck.getServerVersion().ToString() + ")"
                            , System.Windows.Forms.MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                        {
                            // launch browser at the URL provided in the update check
                            log.Debug("Launching the browser to the update URL, as the user selected OK.");
                            System.Diagnostics.Process.Start(updateCheck.getUpdateURI());
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                // an error occured
                log.Error(ex);
            }
        }

        protected override void OnUpdate()
        {
        }
    }
}
