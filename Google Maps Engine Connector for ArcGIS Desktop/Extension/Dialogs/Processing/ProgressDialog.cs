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
using log4net;
using log4net.Config;

namespace com.google.mapsengine.connectors.arcgis.Extension.Dialogs.Processing
{
    public partial class ProgressDialog : Form
    {
        // setup and configure log4net
        protected static ILog log = LogManager.GetLogger(typeof(ProgressDialog));

        // establish a link back to the Extension object
        GoogleMapsEngineToolsExtensionForArcGIS ext = null;

        public ProgressDialog()
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("ProgressDialog initializing.");

            // retrieve a reference to the extension
            log.Debug("Retrieiving a reference to the extension object.");
            ext = GoogleMapsEngineToolsExtensionForArcGIS.GetExtension();

            InitializeComponent();

            // subscribe to Download Progress state change events through the extension
            ext.RaiseDownloadProgressChangeEvent += HandleDownloadProgressStateChangeEvent;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        void HandleDownloadProgressStateChangeEvent(object sender, Extension.DownloadProgressChangeEventArgs e)
        {
            // determine if the download is complete
            if (e.isComplete)
            {
                // close the dialog
                this.Close();
            }
            else if (e.index == -1 || e.total == -1)
            {
                // update the message to the event message
                this.tbProgress.Text = e.message;

                // force an update of the dialog
                this.Update();
            }
            else
            {
                // update the message to the event message
                this.tbProgress.Text = e.message;

                // update the progress bar
                this.progressBar.Minimum = 0;
                this.progressBar.Maximum = e.total;
                this.progressBar.Value = e.index;

                // force an update of the dialog
                this.Update();
            }
        }
    }
}
