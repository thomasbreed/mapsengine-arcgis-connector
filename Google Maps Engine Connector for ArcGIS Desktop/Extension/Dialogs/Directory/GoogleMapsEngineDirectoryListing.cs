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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using log4net;
using log4net.Config;

namespace com.google.mapsengine.connectors.arcgis.Extension.Dialogs.Directory
{
    public partial class GoogleMapsEngineDirectoryListing : Form
    {
        // setup and configure log4net
        private static log4net.ILog log = LogManager.GetLogger(typeof(GoogleMapsEngineDirectoryListing));

        // an uninitailized refernece to the Extension object
        protected GoogleMapsEngineToolsExtensionForArcGIS ext;

        // An empty set of map objects to be filed
        List<MapsEngine.DataModel.gme.Map> maps = new List<MapsEngine.DataModel.gme.Map>();

        // a text template to be updated with the number of unique projects
        protected string txtSearchSummaryTemplate = "";

        // a polygon representing the entire world (default Shape)
        protected IPolygon worldPolygon;

        // A boolean value to determine if the request was made through the API or not
        protected Boolean isRequestThroughAPI = false;


        /*
         * Set the project selected in the drop-down. Used to preserve selection between multiple openings.
         */
        public String SelectedProject
        {
            get;
            set;
        }

        /* 
         * Constructor for the Directory Listing form
         */
        public GoogleMapsEngineDirectoryListing()
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("GoogleMapsEngineDirectoryListing initializing.");

            // retrieve a reference to the extension
            log.Debug("Retrieiving a reference to the extension object.");
            ext = GoogleMapsEngineToolsExtensionForArcGIS.GetExtension();

            // initialize the form objects
            log.Debug("Initializing the form objects.");
            InitializeComponent();

            // bind the buttons on the form to Resources for localization capabilities
            log.Debug("Binding the buttons to Resources.");
            this.btnSearch.Text = Properties.Resources.dialog_GoogleMapsEngineDirectoryListing_btnSearch_Text;
            this.btnAdd.Text = Properties.Resources.dialog_GoogleMapsEngineDirectoryListing_btnAdd_Text;
            this.btnCancel.Text = Properties.Resources.dialog_GoogleMapsEngineDirectoryListing_btnCancel_Text;

            // bind the search template to the Resources for localization capabilities
            log.Debug("Binding the search text template to Resources.");
            this.txtSearchSummaryTemplate = Properties.Resources.dialog_GoogleMapsEngineDirectoryListing_txtSearchSummaryTemplate;
        }

        /*
         * Loading the form data
         */
        private void GoogleMapsEngineDirectoryListing_Load(object sender, EventArgs e)
        {
            // attempting to download the Google Maps Engine projects for the user
            log.Debug("Attempting to populate the projects drop down list with data.");
            try
            {
                // setup a new API object to query the Google Maps Engine API
                MapsEngine.API.GoogleMapsEngineAPI api = new MapsEngine.API.GoogleMapsEngineAPI(ref log);

                // retrieve a list of projects for the user
                List<MapsEngine.DataModel.gme.Project> projects = api.getProjects(ext.getToken());

                // sort the projects by name
                projects.Sort(delegate(MapsEngine.DataModel.gme.Project firstProject,
                    MapsEngine.DataModel.gme.Project nextProject)
                {
                    return firstProject.name.CompareTo(nextProject.name);
                });

                // set the ddl data source as the newly downloaded project
                log.Debug("Setting the projects object as the data source for the drop down list.");
                // we will be setting this project selected if its present in the cache. 
                // we need to locally cache it as setting the ddlProjects.DataSource will overwrite SelectedProject
                String temporarySelectedProject = this.SelectedProject;
                this.ddlProjects.DataSource = projects;
                this.ddlProjects.DisplayMember = "name";
                this.ddlProjects.ValueMember = "id";

                // if the Projects include our stored SelectedProject from the last time this dialog was open
                // set it as selected now.
                if ((!String.IsNullOrEmpty(temporarySelectedProject)) && (null != projects.FirstOrDefault(x => x.name == temporarySelectedProject)))
                {
                    this.ddlProjects.SelectedIndex = projects.FindIndex(x => x.name == temporarySelectedProject);
                }
            }
            catch (System.Exception ex)
            {
                // an error occured, log
                log.Error(ex);
            }
        }

        /*
         * A function to handle the click event of the Search button
         */
        private void btnSearch_Click(object sender, EventArgs e)
        {
            // attempt to execute a search after the user clicked the button
            log.Debug("Attempting to execute a search.");
            try
            {
                // verifying the search box has text
                log.Debug("Verifying the search box has value.");
                if (this.txtSearch.Text != null && this.txtSearch.Text.Length > 0)
                {
                    // creating a new blank list of maps (to be populated by the filtered results)
                    log.Debug("Creating a new blank filtered map list, to hold the filtered results.");
                    List<MapsEngine.DataModel.gme.Map> mapsfiltered = new List<MapsEngine.DataModel.gme.Map>();

                    // go through each map and determine if it matches the search
                    log.Debug("Iterating through the available maps in the list.");
                    foreach (MapsEngine.DataModel.gme.Map map in maps)
                    {
                        // Determine if this iterated map matches the following criteria:
                        /* 1. Equals the Asset Identifier (Map ID)
                         * 2. Equals the Customer Identifier (cid)
                         * 3. Contains the name of the map
                         * 4. Equals the map description
                         * 5. Equals the map name
                         */
                        log.Debug("Determining if the map matches the filter criteria.");
                        if (map.id.Equals(this.txtSearch.Text)
                            || map.name.Equals(this.txtSearch.Text)
                            || map.name.Contains(this.txtSearch.Text)
                            || map.description.Equals(this.txtSearch.Text)
                            || map.description.Contains(this.txtSearch.Text))
                        {
                            // filter matched, adding to filtered map results list
                            log.Debug("Match, adding map to filtered result.");
                            log.Debug("Map name: " + map.name);
                            log.Debug("Map id: " + map.id);
                            mapsfiltered.Add(map);
                        }
                    }

                    // setting the data source on the grid to the new filtered results
                    log.Debug("Setting the data source on the grid to the new filtered results.");
                    this.dataGridGlobeDirectory.DataSource = mapsfiltered;
                }
                else
                {
                    // the search text box is blank or null, replace with original maps
                    log.Debug("Search text is nothing, replace data with original GME downloaded map results.");
                    this.dataGridGlobeDirectory.DataSource = maps;
                }
            }
            catch (System.Exception ex)
            {
                // an unexpected error occured, log
                log.Warn(ex);
            }
        }

        /*
         * A function to handle the click event after the user clicks 'Cancel'
         */
        private void btnCancel_Click(object sender, EventArgs e)
        {
            // the user has clicked the close botton, close this form
            log.Debug("GoogleMapsEngineDirectoryListing form closing");
            this.Close();
        }

        /*
         * A function to maintain control of the selection of rows in the data grid,
         * enabling the 'Add' button when there is one (= 1) record selected in the
         * data grid.  Disabling the 'Add' button when there are no rows selected.
         */
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // determine if there is one row selected in the data grid
            log.Debug("Determining if there is one row selected in the data grid.");
            if (dataGridGlobeDirectory.SelectedRows.Count == 1)
            {
                // there is only one row selected
                // enable the 'Add' button
                log.Debug("Enable the 'Add' button.");
                this.btnAdd.Enabled = true;
            }
            else
            {
                // there is none or more than 1 row selected
                // disable the 'Add' button
                log.Debug("Disabling the 'Add' button.");
                this.btnAdd.Enabled = false;
            }
        }

        /*
         * A function to handle the click event after the user clicks 'Add'
         */
        private void btnAdd_Click(object sender, EventArgs e)
        {
            // disable the button immediately
            this.btnAdd.Enabled = false;

            // create a download progress dialog and show it
            Dialogs.Processing.ProgressDialog downloadProgress = new Processing.ProgressDialog();
            downloadProgress.Show(this);

            // attempt to add the selected map to the user ArcGIS instance
            log.Debug("Attempting to create a new feature class and add it to the map.");
            try
            {
                // get the key out of the key/value pair object
                log.Debug("Fetching the user selected MapId from the DataGridView.");
                string MapId = ((MapsEngine.DataModel.gme.Map)this.dataGridGlobeDirectory.SelectedRows[0].DataBoundItem).id;
                log.Debug("MapId: " + MapId);

                // create a new empty Feature Class to hold the Google Maps Engine catalog results
                log.Debug("Creating a new empty feature class to hold the Google Maps Enigne catalog results");
                IFeatureClass fc = Extension.Data.GeodatabaseUtilities.createGoogleMapsEngineCatalogFeatureClass(ref log, ref ext);

                // publish a new download event to the extension
                ext.publishRaiseDownloadProgressChangeEvent(false, "Preparing to download map '" + MapId + "'.", 1, 0);
                
                // create a new reference to the Google Maps API.
                log.Debug("Creating a new instance of the Google Maps Engine API object.");
                MapsEngine.API.GoogleMapsEngineAPI api = new MapsEngine.API.GoogleMapsEngineAPI(ref log);

                // create a new map object to be defined by the API or MapRoot
                log.Debug("Creating a new empty Map object.");
                MapsEngine.DataModel.gme.Map map;

                // query Google Maps Engine for the layers within this map
                log.Debug("Fetching the Google Maps Engine layers for this map.");
                map = api.getMapById(ext.getToken(), MapId);

                // publish a new download event to the extension
                ext.publishRaiseDownloadProgressChangeEvent(false, "Building local geodatabase for map '" + MapId + "'.", 1, 0);

                // create a new Feature Class Management object
                Data.GoogleMapsEngineFeatureClassManagement fcMngmt = new Data.GoogleMapsEngineFeatureClassManagement(api);

                // populate a feature for every Google Maps Engine Map
                log.Debug("Populate the feature class with the specific MapId");
                fcMngmt.populateFCWithGoogleMapsEngineMap(ref fc, ref map);

                // publish a new download event to the extension
                ext.publishRaiseDownloadProgressChangeEvent(true, "Adding '" + MapId + "' layer to your map.", 1, 1);

                // add the new feature class to the map (auto selecting type "map")
                ext.addFeatureClassToMapAsLayer(ref fc, Properties.Resources.GeodatabaseUtilities_schema_LayerName
                    , "" + Properties.Resources.GeodatabaseUtilities_schema_AssetType_Name + " = 'map'");

                // retrieve the Google Maps Engine WMS URL
                string url = Properties.Settings.Default.gme_wms_GetCapabilities;

                // replace the map identifier and auth token
                url = url.Replace("{mapId}", MapId);
                url = url.Replace("{authTokenPlusSlash}", ext.getToken().access_token + "/");

                // proactively add the viewable layer (WMS or WMTS) to the map
                ext.addWebMappingServiceToMap(new Uri(url));
            }
            catch (System.Exception ex)
            {
                // log error and warn user
                log.Error(ex);

                // hide the download progress, if it is visible
                downloadProgress.Hide();

                // warn the user of the error
                ext.displayErrorDialog(Properties.Resources.dialog_GoogleMapsEngineDirectoryListing_btnAdd_Click_unknownerror + "\n" + ex.Message);
            }

            // close the dialog immediately
            this.Close();
        }

        
        private void ddlProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            // attempting to download the Google Maps Engine maps
            log.Debug("Attempting to populate the form with data.");
            try
            {
                // setup a new API object to query the Google Maps Engine API
                MapsEngine.API.GoogleMapsEngineAPI api = new MapsEngine.API.GoogleMapsEngineAPI(ref log);

                // retrieve a list of maps for the filtered project identifier
                maps = api.getMapsByProjectId(ext.getToken(), ((MapsEngine.DataModel.gme.Project)this.ddlProjects.SelectedItem).id);

                // sort the maps by name
                maps.Sort(delegate(MapsEngine.DataModel.gme.Map firstMap,
                    MapsEngine.DataModel.gme.Map nextMap)
                {
                    return firstMap.name.CompareTo(nextMap.name);
                });
                
                // store the selected project name so that if we reopen this dialog later it is pre-selected
                this.SelectedProject = ((MapsEngine.DataModel.gme.Project)this.ddlProjects.SelectedItem).name;

                // set the datagrid data source as the newly downloaded maps
                log.Debug("Setting the maps object as the data source for the grid.");
                this.dataGridGlobeDirectory.DataSource = maps;

                // update the boolean value
                isRequestThroughAPI = true;
            }
            catch (System.Exception ex)
            {
                // an error occured, log
                log.Error(ex);
            }
        }
    }
}
