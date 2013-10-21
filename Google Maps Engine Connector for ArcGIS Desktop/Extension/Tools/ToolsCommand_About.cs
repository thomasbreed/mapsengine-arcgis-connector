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
    public class ToolsCommand_About : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        // setup and configure log4net
        protected static readonly ILog log = LogManager.GetLogger(typeof(ToolsCommand_About));

        public ToolsCommand_About()
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("ToolsCommand_About initializing.");
        }

        protected override void OnClick()
        {
            // to check the state of the extension, get the extension
            log.Info("Retrieving a reference to the extension object to check state.");
            GoogleMapsEngineToolsExtensionForArcGIS ext = GoogleMapsEngineToolsExtensionForArcGIS.GetExtension();

             // check to see if the extension is enabled
            log.Debug("Verifying extension is enabled.");
            if (ext.isExtensionEnabled())
            {
                // initialize the About form
                log.Info("Initializing dialogs.settings.About and showing it.");
                Extension.Dialogs.Settings.About aboutForm = new Extension.Dialogs.Settings.About();

                // make the about form visible
                aboutForm.Show();
            }
        }

        protected override void OnUpdate()
        {
        }
    }
}
