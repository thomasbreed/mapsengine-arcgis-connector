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
    public class ToolsCommand_UploadToGoogleMapsEngine : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        // setup and configure log4net
        protected static readonly ILog log = LogManager.GetLogger(typeof(ToolsCommand_UploadToGoogleMapsEngine));

        // keep a reference to the extension for state management
        GoogleMapsEngineToolsExtensionForArcGIS ext;

        public ToolsCommand_UploadToGoogleMapsEngine()
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("ToolsCommand_UploadToGoogleEarthBuilder initializing.");

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
                if (ext.isAuthorizationAvailable() && !ext.getToken().isViewOnly
                    && ext.hasAtLeastOneLayer())
                {
                    // the user has an OAuth code available for use
                    log.Debug("User has an OAuth code available. Check the button.");
                    this.Enabled = true;
                }
                else
                {
                    // the user does not have an OAuth code available for use
                    log.Debug("User does not have an OAuth code available. Uncheck the button.");
                    this.Enabled = false;
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

            // subscribe to map layer selection change events through the extension
            ext.RaiseMapLayerStateChangeEvent += HandleMapLayerSelectionStateChangeEvent;
        }

        void HandleExtensionStateChange(object sender, Extension.StateChangeEventArgs e)
        {
            // check to see if the extension is enabled
            log.Debug("Verifying extension is enabled.");
            if (e.State
                && ext.isAuthorizationAvailable() && !ext.getToken().isViewOnly
                && ext.hasAtLeastOneLayer())
            {
                // the extension is enabled, verify the button is also enabled
                log.Debug("Extension is enabled, enable button and check to verify the user has an auth token.");
                this.Enabled = false;
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
            if (e.isAuthorized && !e.isViewOnly && ext.hasAtLeastOneLayer())
            {
                // suppress/check the button
                this.Enabled = false;
            }
            else
            {
                // unsupress/uncheck the button
                this.Enabled = false;
            }
        }

        void HandleMapLayerSelectionStateChangeEvent(object sender, Extension.MapLayerStateChangeEventArgs e)
        {
            // check to see if the extension is enabled
            log.Debug("Verifying extension is enabled.");
            if (e.isLayerSelected && ext.isExtensionEnabled() 
                && ext.isAuthorizationAvailable() && !ext.getToken().isViewOnly)
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
            try
            {
                // establish a reference to the running ArcMap instance
                ESRI.ArcGIS.ArcMapUI.IMxDocument mxDoc 
                    = (ESRI.ArcGIS.ArcMapUI.IMxDocument)ArcMap.Application.Document;

                // retrieve the TOC selected layer (if there is one)
                ESRI.ArcGIS.Carto.ILayer selectedLayer = mxDoc.SelectedLayer;

                // validate layer meets minmum requirements
                if (isLayerValid(selectedLayer))
                {
                    // open the upload dialog
                    Dialogs.Interact.UploadToGoogleMapsEngine dialog 
                        = new Dialogs.Interact.UploadToGoogleMapsEngine(ref ext);
                    dialog.Show();
                }
                else
                {
                    // display an error to the user
                    System.Windows.Forms.MessageBox.Show("Please select a valid vector or raster layer.");
                }
            }
            catch (System.Exception ex)
            {
                // log an error, unable to open in dialog
                log.Error(ex);
            }
        }

        protected override void OnUpdate()
        {
        }

        public static bool isLayerValid(ESRI.ArcGIS.Carto.ILayer layer)
        {
            try
            {
                // determine if there is a selected layer, it is valid, and
                // it is a feature class or raster type layer
                if(layer != null && layer.Valid
                    && (layer is ESRI.ArcGIS.Carto.IFeatureLayer 
                    || layer is ESRI.ArcGIS.Carto.IRasterLayer))
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
