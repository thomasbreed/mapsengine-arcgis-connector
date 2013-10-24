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

using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GISClient;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace com.google.mapsengine.connectors.arcgis
{
    /*
     * The class defining the ArcGIS Extension
     */
    public class GoogleMapsEngineToolsExtensionForArcGIS : ESRI.ArcGIS.Desktop.AddIns.Extension
    {
        #region Global Variables

        // setup an extension object for use by other internal components
        private static GoogleMapsEngineToolsExtensionForArcGIS s_extension;

        // setup a map object to house a reference to the focused ArcMap instance
        private IMap m_map;

        // setup and configure log4net
        private static log4net.ILog log = LogManager.GetLogger(typeof(GoogleMapsEngineToolsExtensionForArcGIS));

        // establish a global object to track the OAuth2 token
        protected Extension.Auth.OAuth2Token token;

        // Declare an Extension State Change Event using EventHandler<T>
        internal event EventHandler<Extension.StateChangeEventArgs> RaiseExtensionStateChangeEvent;

        // Declare an Authentication State Change Event using EventHandler<T>
        internal event EventHandler<Extension.Auth.AuthenticationStateChangeEventArgs> RaiseAuthenticationStateChangeEvent;

        // Declare an Selection Change Event using EventHandler<T>
        internal event EventHandler<Extension.SelectionChangeEventArgs> RaiseSelectionChangeEvent;

        // Declare an Project Filter Change Event using EventHandler<T>
        internal event EventHandler<Extension.ProjectFilterChangeEventArgs> RaiseProjectFilterChangeEvent;

        // Declare an Download Change Event using EventHandler<T>
        internal event EventHandler<Extension.DownloadProgressChangeEventArgs> RaiseDownloadProgressChangeEvent;

        // Declare an Map Layer State Change Event using EventHandler<T>
        internal event EventHandler<Extension.MapLayerStateChangeEventArgs> RaiseMapLayerStateChangeEvent;

        // Declare a list of temporary files to clean-up at shutdown (TODO: Replace with scratch)
        internal List<System.IO.DirectoryInfo> tempDirs = new List<DirectoryInfo>();

        #endregion

        #region Initialize Extension, Start/End
        public GoogleMapsEngineToolsExtensionForArcGIS()
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("The extension is initializing.");

            // set the extension object to this
            log.Debug("Setting the extension object to this.");
            s_extension = this;

            // The Google Maps Engine Tools Extension for ArcMap is initializing
            // This occures when the extension is loaded or referenced for the first time
            // The extension is set for delayed loading, so this may happen at Map Load or later, for instance
            // when the user goes to the Tools > Extension menu

            // initialize the OAuth2 token object
            log.Debug("Initializing the OAuth2 token object.");
            token = null;

            // Step 1: Check to see if the extension is enabled
            log.Debug("Checking to see if the extension is enabled.");
            if (this.State == ESRI.ArcGIS.Desktop.AddIns.ExtensionState.Enabled)
            {
                log.Debug("The extension is enabled.");

                // raise an extension changed event
                OnRaiseExtensionStateChangeEvent(new Extension.StateChangeEventArgs(true));

                // check to see if there is a valid token stored for this user
                log.Debug("Checking to see if the token object exists in the user's profile.");
                if (Extension.Auth.OAuth2Utilities.doesTokenExist(ref log))
                {
                    log.Debug("Token exists.");

                    // retrieve token from the user's profile for use
                    log.Debug("Getting token object from user's proflie.");
                    token = Extension.Auth.OAuth2Utilities.getToken(ref log);
                }

                // check to see if there is an update to the Extension
                //TODO: Spin off a seperate thread to do this
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
            else
            {
                // the extension is not enabled or is unavailable
                log.Debug("The extension is disabled or unavailable (" + this.State + ").");
            }
        }

        protected override void OnStartup()
        {
            // the extension is starting up for the first time
            log.Info("The extension is starting up.");

            // raise an extension changed event
            log.Debug("Raising an extension event for those who are listensing.");
            OnRaiseExtensionStateChangeEvent(new Extension.StateChangeEventArgs(isExtensionEnabled()));

            // wire up ArcMap events and begin to listen to them
            log.Debug("Wiring up ArcMap document events.");
            ArcMap.Events.NewDocument += InitializeArcMap;
            ArcMap.Events.OpenDocument += InitializeArcMap;
        }

        protected override void OnShutdown()
        {
            log.Info("Extension is shutting down.");

            // go through each layer and remove any temporary layers
            for (int k = 0; k < m_map.LayerCount; k++)
            {
                if (m_map.get_Layer(k).Name == "Google Maps Engine Asset Catalog")
                {
                    ILayer layer = m_map.get_Layer(k);
                    m_map.DeleteLayer(layer);
                    Release(layer);
                }
            }

            // clean-up Google Maps Engine temporary files
            foreach (System.IO.DirectoryInfo dir in tempDirs)
            {
                try
                {
                    // TODO: Do not delete so abruptly, handle better
                    //dir.Delete(true);
                }
                catch (System.Exception)
                { }
            }

            // uninitialize ArcMap events
            log.Debug("Uninitializing ArcMap event handlers.");
            UninitializeArcMap();

            // wire up ArcMap events and stop listening to them
            log.Debug("Un-wiring up ArcMap document events.");
            ArcMap.Events.NewDocument -= InitializeArcMap;
            ArcMap.Events.OpenDocument -= InitializeArcMap;
            
            // set the map and extension to null to clear up
            log.Debug("Setting the arcmap and extension reference objects to null.");
            m_map = null;
            s_extension = null;

            // shutdown
            base.OnShutdown();
        }

        protected override bool OnSetState(ESRI.ArcGIS.Desktop.AddIns.ExtensionState state)
        {
            try
            {
                // change the state of the extension
                log.Debug("State of extension changing from " + this.State.ToString() + " to " + state.ToString());
                this.State = state;

                // raise the state change event
                log.Info("Raising a state change event for all subecribers.");
                OnRaiseExtensionStateChangeEvent(new Extension.StateChangeEventArgs(isExtensionEnabled()));

                // raise a selection state change event too, for buttons dependent on selection
                OnRaiseSelectionChangeEvent(new Extension.SelectionChangeEventArgs(isExtensionEnabled()));

                // returning true
                log.Debug("Returning true");
                return true;
            }
            catch (System.Exception ex)
            {
                // an error occured, log error and return false
                log.Error(ex);
                log.Debug("Returning true");
                return false;
            }
        }

        #endregion

        
        #region Internal Functions
        internal void Release(object comObj)
        {
            int refsLeft = 0;
            do
            {
                refsLeft = System.Runtime.InteropServices.Marshal.ReleaseComObject(comObj);
            }
            while (refsLeft > 0);
        }

        internal static GoogleMapsEngineToolsExtensionForArcGIS GetExtension()
        {
            // Extension loads just in time, call FindExtension to load it.
            if (s_extension == null)
            {
                ESRI.ArcGIS.esriSystem.UID extID = new ESRI.ArcGIS.esriSystem.UIDClass();
                extID.Value = ThisAddIn.IDs.GoogleMapsEngineToolsExtensionForArcGIS;
                ArcMap.Application.FindExtensionByCLSID(extID);
            }
            return s_extension;
        }

        internal bool isExtensionEnabled()
        {
            log.Debug("Checking if the extension is in an enabled state.");
            log.Debug("State == " + this.State.ToString());
            if (this.State == ESRI.ArcGIS.Desktop.AddIns.ExtensionState.Enabled)
                return true;
            else return false;
        }

        internal bool isAuthorizationAvailable()
        {
            return (this.token != null);
        }

        internal Extension.Auth.OAuth2Token getToken()
        {
            try
            {
                // check to see if the token is valid
                if (!Extension.Auth.OAuth2Utilities.isTokenExpired(ref log, token))
                {
                    // returning global token
                    log.Debug("Returning global token object, as it is valid.");
                    return this.token;
                }
                else
                {
                    // exchange OAuth2 code for new token, as it is invalid
                    log.Debug("Exchanging the OAuth2 code for a new token, as it has expired.");
                    this.token = Extension.Auth.OAuth2Utilities.refreshToken(ref log, token);

                    // return the new token
                    log.Debug("Returning new, refreshed token.");
                    return this.token;
                }
            }
            catch (System.Exception ex)
            {
                // an error occured and was not able to get token
                log.Error(ex);
                System.Windows.Forms.MessageBox.Show("Unable to retrieve a short-lived authentication token from your Google Account.  Please sign-in and try again.");
                this.clearToken();
                return null;
            }
        }

        internal void setToken(Extension.Auth.OAuth2Token token)
        {
            // replace the global token object
            log.Debug("Replacing the global token object with a new one");
            this.token = token;
        }

        internal void clearToken()
        {
            // clear the OAuth2 Token from the user's profile
            Extension.Auth.OAuth2Utilities.clearToken(ref log);

            // clear the global token
            this.token = null;

            // raise an Authentication event
            OnRaiseAuthenticationStateChangeEvent(new Extension.Auth.AuthenticationStateChangeEventArgs(false, true, null));

            // raise a selection state change event too, for buttons dependent on selection
            OnRaiseSelectionChangeEvent(new Extension.SelectionChangeEventArgs(false));
        }

        internal string getAddinName()
        {
            return ThisAddIn.Name;
        }

        internal string getAddinDate()
        {
            return ThisAddIn.Date;
        }

        internal string getAddinDescription()
        {
            return ThisAddIn.Description;
        }

        internal string getAddinVersion()
        {
            return ThisAddIn.Version;
        }

        internal bool hasSelectedFeature()
        {

            return false;
        }

        internal System.IO.DirectoryInfo getLocalWorkspaceDirectory()
        {
            // establish a new directory info object
             System.IO.DirectoryInfo tempDirectoryInfo;

            // determine if the properties location has a value
            if (Properties.Settings.Default.temp_storage_location != null && Properties.Settings.Default.temp_storage_location.Length > 0)
            {
                // set the text box string as the properties value
                tempDirectoryInfo = new DirectoryInfo(Properties.Settings.Default.temp_storage_location);
            }
            else
            {
                // the properties value does not exist, use default
                tempDirectoryInfo = new DirectoryInfo(System.IO.Path.GetTempPath() + @"\Google\Google Maps Engine");
            }

            // create the directory if it doesn't exist
            if (!tempDirectoryInfo.Exists)
                tempDirectoryInfo.Create();

            // return the Directory Info object
            return tempDirectoryInfo;
        }

        internal void addFeatureClassToMapAsLayer(ref IFeatureClass fc, string LayerName)
        {
            try
            {
                // define a new layer for the asset catalog
                IFeatureLayer player = new FeatureLayer();
                player.FeatureClass = fc;
                player.Name = LayerName;

                // TODO: Specifically set the Spatial Reference

                //Create a GxLayer to stylize the layer.
                ESRI.ArcGIS.Catalog.IGxLayer gxLayerCls = new ESRI.ArcGIS.Catalog.GxLayer();
                ESRI.ArcGIS.Catalog.IGxFile gxFile = (ESRI.ArcGIS.Catalog.IGxFile)gxLayerCls;
                //Explicit Cast.

                // create an assembly file object to determine where on the system this is running
                log.Debug("Attempting to locate the executing extension assembly.");
                Uri assemblyUri = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                System.IO.FileInfo assemblyFileInfo
                    = new System.IO.FileInfo(Path.GetDirectoryName(assemblyUri.LocalPath));
                log.Debug("Executing from: " + assemblyFileInfo.FullName);

                //Set the path for where the layer file is located on disk.
                gxFile.Path = assemblyFileInfo.FullName + @"\GME_Catalog_Style.lyr";
                log.Debug("lyr Path: " + gxFile.Path);

                // validate the symbology layers exist
                if (!(gxLayerCls.Layer == null))
                {
                    // create a geo feature layer for the master symbology
                    IGeoFeatureLayer symbologyGeoFeatureLayer = gxLayerCls.Layer as IGeoFeatureLayer;

                    // create a geo feature layer for the new features
                    IGeoFeatureLayer pGeoFeatureLayer = player as IGeoFeatureLayer;

                    // apply the renderer from the template to the new feature layer
                    pGeoFeatureLayer.Renderer = symbologyGeoFeatureLayer.Renderer;
                }

                // add the layer to the map
                m_map.AddLayer(player);
            }
            catch (System.Exception ex)
            {
                // an error occured
                log.Error(ex);
                System.Windows.Forms.MessageBox.Show("Unable to add feature class to the map.");
            }
        }

        internal void addFeatureClassToMapAsLayer(ref IFeatureClass fc, string LayerName, string whereClause)
        {
            try
            {
                // define a new layer for the asset catalog
                IFeatureLayer player = new FeatureLayer();
                player.FeatureClass = fc;
                player.Name = LayerName;

                // TODO: Specifically set the spatial reference

                // Set up the query
                ESRI.ArcGIS.Geodatabase.IQueryFilter queryFilter = new ESRI.ArcGIS.Geodatabase.QueryFilterClass();
                queryFilter.WhereClause = whereClause;

                // create a feature selection object
                ESRI.ArcGIS.Carto.IFeatureSelection featureSelection 
                    = player as ESRI.ArcGIS.Carto.IFeatureSelection; // Dynamic Cast

                // Perform the selection
                featureSelection.SelectFeatures(queryFilter, ESRI.ArcGIS.Carto.esriSelectionResultEnum.esriSelectionResultNew, false);

                //Create a GxLayer to stylize the layer.
                ESRI.ArcGIS.Catalog.IGxLayer gxLayerCls = new ESRI.ArcGIS.Catalog.GxLayer();
                ESRI.ArcGIS.Catalog.IGxFile gxFile = (ESRI.ArcGIS.Catalog.IGxFile)gxLayerCls;
                //Explicit Cast.

                // create an assembly file object to determine where on the system this is running
                log.Debug("Attempting to locate the executing extension assembly.");
                Uri assemblyUri = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                System.IO.FileInfo assemblyFileInfo
                    = new System.IO.FileInfo(Path.GetDirectoryName(assemblyUri.LocalPath));
                log.Debug("Executing from: " + assemblyFileInfo.FullName);

                //Set the path for where the layer file is located on disk.
                gxFile.Path = assemblyFileInfo.FullName + "\\GME_Catalog_Style.lyr";
                log.Debug("lyr Path: " + gxFile.Path);

                // validate the symbology layers exist
                if (!(gxLayerCls.Layer == null))
                {
                    // create a geo feature layer for the master symbology
                    IGeoFeatureLayer symbologyGeoFeatureLayer = gxLayerCls.Layer as IGeoFeatureLayer;

                    // create a geo feature layer for the new features
                    IGeoFeatureLayer pGeoFeatureLayer = player as IGeoFeatureLayer;

                    // apply the renderer from the template to the new feature layer
                    pGeoFeatureLayer.Renderer = symbologyGeoFeatureLayer.Renderer;
                }

                // add the layer to the map
                m_map.AddLayer(player);

                // Flag the new selection
                ArcMap.Document.ActiveView.PartialRefresh(ESRI.ArcGIS.Carto.esriViewDrawPhase.esriViewGeoSelection, null, null);

                // manually trigger the selection has changed event to determine if the buttons should re-enable
                ArcMap_SelectionChanged();
            }
            catch (System.Exception ex)
            {
                // an error occured
                log.Error(ex);
                System.Windows.Forms.MessageBox.Show("Unable to add feature class to the map.");
            }
        }


        internal void addWebMappingServiceToMap(Uri WMS_URI)
        {
            try
            {
                // create a new WMS layer for the ArcMap session
                IWMSGroupLayer wmsMapLayer = new WMSMapLayer() as IWMSGroupLayer;
                IWMSConnectionName connName = new WMSConnectionName();

                // set the property of the WMS layer to the Google Maps Engine
                // WMS GetCapabilities URL
                IPropertySet propSet = new PropertySet();
                propSet.SetProperty("URL", WMS_URI.ToString());

                connName.ConnectionProperties = propSet;

                // create a new data layer for the WMS layer to use
                IDataLayer dataLayer = wmsMapLayer as IDataLayer;
                dataLayer.Connect((IName)connName);

                // create a new ArcMap layer and add the WMS.  Set the name.
                IWMSServiceDescription serviceDesc = wmsMapLayer.WMSServiceDescription;
                ILayer layer = wmsMapLayer as ILayer;
                layer.Name = serviceDesc.WMSTitle;

                // add the WMS layer to the ArcMap session
                m_map.AddLayer(layer);
            }
            catch (System.Exception ex)
            {
                // an error occured and was not able to add the WMS layer to the map
                log.Error(ex);
                System.Windows.Forms.MessageBox.Show("Unable to add a WMS layer to the map.");
            }
        }

        internal List<string> getSelectedMapAssetIds()
        {
            // setup a list of asset identifiers to populate by searching selected features
            List<string> assetIds = new List<string>();

            try
            {
                // establish a featured layer and featured selection
                IFeatureLayer featureLayer;
                IFeatureSelection featSel;

                // go through each layer and determine if a feature is selected
                for (int k = 0; k < m_map.LayerCount; k++)
                {
                    // if the layer is contained is feature selection, look at each feature, else skip
                    if (m_map.get_Layer(k) is IFeatureSelection)
                    {
                        // get the feature layer from the map layer
                        featureLayer = m_map.get_Layer(k) as IFeatureLayer;

                        // if it doesn't exist or is null, break out of this sequence
                        // the ArcMap session may return a null layer reference
                        // if there is an issue getting the layer  using (m_map.get_Layer)
                        if (featureLayer == null)
                            break;

                        // get the selected features from the feature layre
                        featSel = featureLayer as IFeatureSelection;

                        // if the selected features set is null, skip
                        if (featSel.SelectionSet != null)
                        {
                            // establish a cursor to go through each selected feature
                            ICursor cursor;

                            // search through the feature set
                            featSel.SelectionSet.Search(null, false, out cursor);

                            // go through, row by row, to determine if the selected feature
                            // includes an MapAssetId
                            IRow row = cursor.NextRow();
                            while (row != null)
                            {
                                // look only at MapAssetId only, if it doesn't have that field...skip
                                int findex = row.Fields.FindField(Properties.Resources.GeodatabaseUtilities_schema_AssetId_Name);
                                if (findex != -1)
                                    assetIds.Add(row.get_Value(findex).ToString());

                                // go to next row
                                row = cursor.NextRow();
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
                // TODO: Fail gracefully
            }

            // return the Asset Identifiers
            return assetIds.Distinct().ToList();
        }

        internal void displayErrorDialog(string message)
        {
            System.Windows.Forms.MessageBox.Show(
                message
                , "Google Maps Engine Tools for ArcMap"
                , System.Windows.Forms.MessageBoxButtons.OK
                , System.Windows.Forms.MessageBoxIcon.Error);
        }

        internal void addTemporaryDirectory(System.IO.DirectoryInfo dir)
        {
            this.tempDirs.Add(dir);
        }

        internal bool hasAtLeastOneLayer()
        {
            return (m_map.LayerCount > 0);
        }

        #endregion

        #region Custom Event Handlers
        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        internal virtual void OnRaiseExtensionStateChangeEvent(Extension.StateChangeEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<Extension.StateChangeEventArgs> handler = RaiseExtensionStateChangeEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        internal virtual void OnRaiseAuthenticationStateChangeEvent(Extension.Auth.AuthenticationStateChangeEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<Extension.Auth.AuthenticationStateChangeEventArgs> handler = RaiseAuthenticationStateChangeEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        internal virtual void OnRaiseSelectionChangeEvent(Extension.SelectionChangeEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<Extension.SelectionChangeEventArgs> handler = RaiseSelectionChangeEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        internal virtual void OnRaiseProjectFilterChangeEvent(Extension.ProjectFilterChangeEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<Extension.ProjectFilterChangeEventArgs> handler = RaiseProjectFilterChangeEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        internal virtual void OnRaiseDownloadProgressChangeEvent(Extension.DownloadProgressChangeEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<Extension.DownloadProgressChangeEventArgs> handler = RaiseDownloadProgressChangeEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
        internal virtual void OnRaiseMapLayerStateChangeEvent(Extension.MapLayerStateChangeEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<Extension.MapLayerStateChangeEventArgs> handler = RaiseMapLayerStateChangeEvent;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        internal void publishRaiseAuthenticationStateChangeEvent(bool isAuthorized, bool isViewOnly, Extension.Auth.OAuth2Token token)
        {
            if(isAuthorized)
                OnRaiseAuthenticationStateChangeEvent(new Extension.Auth.AuthenticationStateChangeEventArgs(isAuthorized, isViewOnly, token));
            else
                OnRaiseAuthenticationStateChangeEvent(new Extension.Auth.AuthenticationStateChangeEventArgs(isAuthorized, isViewOnly));
        }

        internal void publishRaiseProjectFilterChangeEvent(bool isFilterApplied, String projectId)
        {
            OnRaiseProjectFilterChangeEvent(new Extension.ProjectFilterChangeEventArgs(isFilterApplied, projectId));
        }

        internal void publishRaiseDownloadProgressChangeEvent(bool isComplete, string message)
        {
            OnRaiseDownloadProgressChangeEvent(new Extension.DownloadProgressChangeEventArgs(isComplete, message));
        }
        internal void publishRaiseDownloadProgressChangeEvent(bool isComplete, string message, int total, int index)
        {
            OnRaiseDownloadProgressChangeEvent(new Extension.DownloadProgressChangeEventArgs(isComplete, message, total, index));
        }
        internal void publishRaiseDownloadProgressChangeEvent(bool isLayerSelected)
        {
            OnRaiseMapLayerStateChangeEvent(new Extension.MapLayerStateChangeEventArgs(isLayerSelected));
        }

        #endregion

        #region ArcMap Event Handlers and Setup
        private void InitializeArcMap()
        {
            // If the extension hasn't been started yet or it's been turned off, bail
            log.Debug("Initializing ArcMap, checking to see if the extension is enabled.");
            if (s_extension == null || this.State != ExtensionState.Enabled)
                return;

            // Reset event handlers
            log.Debug("Setting up and attaching ArcMap event handlers.");
            IActiveViewEvents_Event avEvent = ArcMap.Document.FocusMap as IActiveViewEvents_Event;
            avEvent.SelectionChanged += ArcMap_SelectionChanged;
            avEvent.ItemAdded += ArcMap_ItemAddedDeleted;
            avEvent.ItemDeleted += ArcMap_ItemAddedDeleted;

            // Update the UI
            log.Debug("Setting a reference to this ArcMap session.");
            m_map = ArcMap.Document.FocusMap;
        }

        private void UninitializeArcMap()
        {
            // verify the extension is not null
            log.Debug("On uninitialization of ArcMap, verifying the extension is not null.");
            if (s_extension == null)
                return;

            // Detach event handlers
            log.Debug("Detaching ArcMap event handlers.");
            IActiveViewEvents_Event avEvent = m_map as IActiveViewEvents_Event;
            avEvent.SelectionChanged -= ArcMap_SelectionChanged;
            avEvent.ItemAdded -= ArcMap_ItemAddedDeleted;
            avEvent.ItemDeleted -= ArcMap_ItemAddedDeleted;

            avEvent = null;
        }

        private void ArcMap_SelectionChanged()
        {
            // setup a total selected features
            int totalSelectedFeatures = 0;

            IFeatureLayer featureLayer;
            IFeatureSelection featSel;

            // go through each layer and determine if a feature is selected
            for (int k = 0; k < m_map.LayerCount; k++)
            {
                if (m_map.get_Layer(k) is IFeatureSelection)
                {
                    featureLayer = m_map.get_Layer(k) as IFeatureLayer;
                    if (featureLayer == null)
                        break;

                    featSel = featureLayer as IFeatureSelection;

                    int count = 0;
                    if (featSel.SelectionSet != null)
                        count = featSel.SelectionSet.Count;

                    totalSelectedFeatures += count;
                }
            }

            // if there are selected features, raise event
            if(totalSelectedFeatures > 0)
                OnRaiseSelectionChangeEvent(new Extension.SelectionChangeEventArgs(true, totalSelectedFeatures));
            else
                OnRaiseSelectionChangeEvent(new Extension.SelectionChangeEventArgs(false));
        }

        private void ArcMap_ItemAddedDeleted(System.Object Item)
        {
            if (m_map.LayerCount > 0)
            {
                OnRaiseMapLayerStateChangeEvent(new Extension.MapLayerStateChangeEventArgs(true));
            }
            else
            {
                OnRaiseMapLayerStateChangeEvent(new Extension.MapLayerStateChangeEventArgs(false));
            }
        }
        #endregion
    
    }

}
