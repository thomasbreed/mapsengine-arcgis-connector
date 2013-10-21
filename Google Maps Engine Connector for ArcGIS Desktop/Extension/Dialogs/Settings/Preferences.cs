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

namespace com.google.mapsengine.connectors.arcgis.Extension.Dialogs.Settings
{
    public partial class Preferences : Form
    {
        // setup and configure log4net
        private static log4net.ILog log = LogManager.GetLogger(typeof(Preferences));

        // an uninitailized refernece to the Extension object
        protected GoogleMapsEngineToolsExtensionForArcGIS ext;

        public Preferences()
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("Preferences initializing.");

            // retrieve a reference to the extension
            log.Debug("Retrieiving a reference to the extension object.");
            ext = GoogleMapsEngineToolsExtensionForArcGIS.GetExtension();

            // initialize the preference components
            InitializeComponent();

            // populate the UI fields
            populate();

            // subscribe to Authentication state change events through the extension
            ext.RaiseAuthenticationStateChangeEvent += HandleAuthenticationStateChangeEvent;
        }

        private void populate()
        {
            // populate the temporary storage location
            if (Properties.Settings.Default.temp_storage_location != null && Properties.Settings.Default.temp_storage_location.Length > 0)
            {
                // set the text box string as the properties value
                this.txtDefaultStorageLocation.Text = Properties.Settings.Default.temp_storage_location;
            }
            else
            {
                // the properties value does not exist, use default extension setting
                this.txtDefaultStorageLocation.Text = ext.getLocalWorkspaceDirectory().FullName;
            }
        }

        void HandleAuthenticationStateChangeEvent(object sender, Extension.Auth.AuthenticationStateChangeEventArgs e)
        {
            // an authentication event occured, determine which one and act accordingly
            if (e.isAuthorized)
            {
               // TODO: Handle authorization event.
            }
        }

        private void btnChangeDefaultStorageLocation_Click(object sender, EventArgs e)
        {
            // create a new folder dialog box
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            // set the default open location to the My Documents Folder
            //dialog.RootFolder = Environment.SpecialFolder.MyDocuments;

            // set the currently selected location as the default location
            dialog.SelectedPath = this.txtDefaultStorageLocation.Text;

            // disable creating a new folder location
            dialog.ShowNewFolderButton = false;

            // Show the FolderBrowserDialog.
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                // retrieve the folder name
                String folderName = dialog.SelectedPath;
                
                // set the new folder path to the default location
                this.txtDefaultStorageLocation.Text = folderName;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // close the window
            this.Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // verify the folder exists, then update the saved location
            if (System.IO.Directory.Exists(this.txtDefaultStorageLocation.Text))
            {
                Properties.Settings.Default.temp_storage_location = this.txtDefaultStorageLocation.Text;
            }

            // save the settings
            Properties.Settings.Default.Save();

            // close the dialog window
            this.Close();
        }
    }
}
