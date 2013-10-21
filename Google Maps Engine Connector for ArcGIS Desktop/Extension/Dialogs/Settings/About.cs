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
using System.Reflection;

namespace com.google.mapsengine.connectors.arcgis.Extension.Dialogs.Settings
{
    public partial class About : Form
    {
        // setup and configure log4net
        protected static readonly ILog log = LogManager.GetLogger(typeof(About));

        public About()
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("About initializing.");

            InitializeComponent();
        }

        private void About_Load(object sender, EventArgs e)
        {
            try
            {
                // to check the state of the extension, get the extension
                log.Debug("Retrieving a reference to the extension object to check state.");
                GoogleMapsEngineToolsExtensionForArcGIS ext = GoogleMapsEngineToolsExtensionForArcGIS.GetExtension();

                // fetch the about_dialog_html resource
                log.Debug("fetch the about_dialog_html resource");
                string buildinfotxt = Properties.Resources.about_dialog_html;

                // replace, if exists, the {addinname}
                log.Debug("replace, if exists, the {addinname}");
                buildinfotxt = buildinfotxt.Replace("{addinname}", ext.getAddinName());

                // replace, if exists, the {addindescription}
                log.Debug("replace, if exists, the {addindescription}");
                buildinfotxt = buildinfotxt.Replace("{addindescription}", ext.getAddinDescription());

                // replace, if exists, the {addindate}
                log.Debug("replace, if exists, the {addindate}");
                buildinfotxt = buildinfotxt.Replace("{addindate}", ext.getAddinDate());

                // replace, if exists, the {addinbuildversion}
                log.Debug("replace, if exists, the {addinbuildversion}");
                buildinfotxt = buildinfotxt.Replace("{addinbuildversion}", ext.getAddinVersion());

                // replace, if exists, the {assemblyversion}
                log.Debug("replace, if exists, the {assemblyversion}");
                buildinfotxt = buildinfotxt.Replace("{assemblyversion}", Assembly.GetExecutingAssembly().GetName().Version.ToString());

                // set the build information
                log.Debug("Setting the HTML to the web browser on the dialog.");
                this.wblegal.DocumentText = buildinfotxt;

                // add a script manager to the browser
                // in order to capture select events
                this.wblegal.ObjectForScripting = new BrowerScriptManager(this);
            }
            catch (System.Exception ex)
            {
                // an error occured, log and write standard text
                log.Error(ex);
                this.wblegal.DocumentText = "Google Maps Engine Add-in for ArcGIS Desktop.";
            }
        }

        protected void cancelButtonClicked()
        {
            // close the dialog
            this.Close();
        }

        // A nested "com visible" class to handle javascript events in the web browser
        [ComVisible(true)]
        public class BrowerScriptManager
        {
            // Variable to store the form of type Form1.
            private About mForm;

            // Constructor.
            public BrowerScriptManager(About form)
            {
                // Save the form so it can be referenced later.
                mForm = form;
            }

            // This method can be called from JavaScript.
            public void handleCancelClickEvent()
            {
                // Call a method on the form.
                mForm.cancelButtonClicked();
            }
        }
    }
}
