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
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.GISClient;
using ESRI.ArcGIS.Geoprocessor;

namespace com.google.mapsengine.connectors.arcgis.Extension.Dialogs.Interact
{
    public partial class UploadToGoogleMapsEngine : Form
    {
        // setup and configure log4net
        private static log4net.ILog log = LogManager.GetLogger(typeof(UploadToGoogleMapsEngine));

        // establish a link back to the Extension object
        GoogleMapsEngineToolsExtensionForArcGIS ext = null;

        // create a boolean to track if the layer is valid
        Boolean isLayerValidated = false;

        // create an object to track what type of dataset is being uploaded
        String uploadType = "vector";

        public UploadToGoogleMapsEngine(ref GoogleMapsEngineToolsExtensionForArcGIS ext)
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("OAuth2AuthForm initializing.");

            // initialize the local extension object
            log.Debug("Initializing the reference to the Extension.");
            this.ext = ext;

            // initialize form components
            InitializeComponent();

            // establish a reference to the running ArcMap instance
            ESRI.ArcGIS.ArcMapUI.IMxDocument mxDoc = (ESRI.ArcGIS.ArcMapUI.IMxDocument)ArcMap.Application.Document;

            // retrieve a reference to the selected layer
            ESRI.ArcGIS.Carto.ILayer selectedLayer = mxDoc.SelectedLayer;

            // determine if the selected layer is valid
            if (!Tools.ToolsCommand_UploadToGoogleMapsEngine.isLayerValid(selectedLayer))
            {
                // Handle invalid layer
                System.Windows.Forms.MessageBox.Show("The selected layer does not meet the minimum requirements or is invalid.");

                // close the dialog
                this.Close();
            }
            else
            {
                this.isLayerValidated = true;
            }
        }

        public static void ExportLayerToShapefile(string shapePath, string shapeName, ILayer source)
        {
            try
            {
                // cast the selected/requested layer into a feature layer
                ESRI.ArcGIS.Carto.IFeatureLayer featureLayer = (IFeatureLayer)source;
                
                // create a new feature class to feature class converter utility
                ESRI.ArcGIS.ConversionTools.FeatureClassToFeatureClass fc2fc 
                    = new ESRI.ArcGIS.ConversionTools.FeatureClassToFeatureClass();

                // set the input feature layer
                fc2fc.in_features = featureLayer;

                // set the output path and Shapefile name
                fc2fc.out_path = shapePath;
                fc2fc.out_name = shapeName + ".shp";

                // create a new GeoProcessor
                ESRI.ArcGIS.Geoprocessor.Geoprocessor geoprocessor = new ESRI.ArcGIS.Geoprocessor.Geoprocessor();
                geoprocessor.TemporaryMapLayers = true;

                // execute the FeatureClassToFeatureClass
                geoprocessor.Execute(fc2fc, null);

                // export a copy of the layer file definition too for upload/style at a later time
                ESRI.ArcGIS.DataManagementTools.SaveToLayerFile saveToLayerFile 
                    = new ESRI.ArcGIS.DataManagementTools.SaveToLayerFile();
                saveToLayerFile.in_layer = source;
                saveToLayerFile.out_layer = shapePath + "\\" + shapeName + ".lyr";

                // execute the FeatureClassToFeatureClass
                geoprocessor.Execute(saveToLayerFile, null);

                // remove reference to the tool and geoprocessor
                fc2fc = null;
                geoprocessor = null;
            }
            catch (Exception ex)
            {
                // an error occured
                System.Windows.Forms.MessageBox.Show("Error: " + ex.Message);
            }
        }

        public static void ExportLayerToUploadableRaster(string outputFilePath, string name, ILayer source)
        {
            try
            {
                // cast the selected/requested layer into a raster layer
                ESRI.ArcGIS.Carto.IRasterLayer rasterLayer = (IRasterLayer)source;
                
                // create a new raster to raster converter utility
                ESRI.ArcGIS.DataManagementTools.CopyRaster raster2raster
                    = new ESRI.ArcGIS.DataManagementTools.CopyRaster();

                // set the input raster layer
                raster2raster.in_raster = rasterLayer;

                // set the output path and Shapefile name
                /* The name and location of the raster dataset to be created.
                 * .bil—Esri BIL
                 * .bip—Esri BIP
                 * .bmp—BMP
                 * .bsq—Esri BSQ
                 * .dat—ENVI DAT
                 * .gif—GIF
                 * .img—ERDAS IMAGINE
                 * .jpg—JPEG
                 * .jp2—JPEG 2000
                 * .png—PNG
                 * .tif—TIFF
                 * no extension for Esri Grid
                 */
                raster2raster.out_rasterdataset = outputFilePath + "\\" + name + ".tif";

                // create a new GeoProcessor
                Geoprocessor geoprocessor = new Geoprocessor();
                geoprocessor.TemporaryMapLayers = true;

                // execute the RasterToOtherFormat
                geoprocessor.Execute(raster2raster, null);
            }
            catch (Exception ex)
            {
                // an error occured
                System.Windows.Forms.MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void UploadToGoogleMapsEngine_Load(object sender, EventArgs e)
        {
            // establish a reference to the running ArcMap instance
            ESRI.ArcGIS.ArcMapUI.IMxDocument mxDoc = (ESRI.ArcGIS.ArcMapUI.IMxDocument)ArcMap.Application.Document;

            // retrieve a reference to the selected layer
            ESRI.ArcGIS.Carto.ILayer selectedLayer = mxDoc.SelectedLayer;

            // get the data layer of this feature layer
            ESRI.ArcGIS.Carto.IDataLayer2 dataLayer
                = (ESRI.ArcGIS.Carto.IDataLayer2)selectedLayer;
            ESRI.ArcGIS.Geodatabase.IDatasetName datasetName
                = (ESRI.ArcGIS.Geodatabase.IDatasetName)dataLayer.DataSourceName;

            // add a script manager to the browser
            // in order to capture select events
            webBrowser.ObjectForScripting = new BrowerScriptManager(this);

            // fetch the upload_dialog_html resource
            log.Debug("fetch the upload_dialog_html resource");
            string uploadhtml = Properties.Resources.upload_dialog_html;

            // populate the source dataset information
            uploadhtml = uploadhtml.Replace("{sourcename}", datasetName.Name);
            uploadhtml = uploadhtml.Replace("{sourceworkspace}", datasetName.WorkspaceName.PathName);

            // configure raster/vector specific information
            if (selectedLayer is ESRI.ArcGIS.Carto.IFeatureLayer)
            {
                // cast the selected layer into a feature layer
                ESRI.ArcGIS.Carto.IFeatureLayer featureLayer 
                    = (ESRI.ArcGIS.Carto.IFeatureLayer)selectedLayer;

                // set the source vector type
                uploadhtml = uploadhtml.Replace("{sourcetype}", featureLayer.DataSourceType);

                // set the variable upload type to vector
                uploadType = "vector";
            } 
            else if (selectedLayer is ESRI.ArcGIS.Carto.IRasterLayer)
            {
                // cast the selected layer into a raster layer
                ESRI.ArcGIS.Carto.IRasterLayer rasterLayer 
                    = (ESRI.ArcGIS.Carto.IRasterLayer)selectedLayer;

                // set the source raster type
                uploadhtml = uploadhtml.Replace("{sourcetype}", "Raster: "
                    + rasterLayer.RowCount + " rows, "  + rasterLayer.ColumnCount + " columns, " 
                    + rasterLayer.BandCount + " bands");

                // set the variable upload type to raster
                uploadType = "raster";
            }

            // set the upload html
            log.Debug("Setting the HTML to the web browser on the dialog.");
            this.webBrowser.DocumentText = uploadhtml;
        }

        protected void uploadButtonClicked(String projectId, String name, String description, String tags, String draftAccessList, String attribution, String acquisitionTime, String maskType)
        {
            try
            {
                // create a new Processing Dialog
                Dialogs.Processing.ProgressDialog processingDialog = new Processing.ProgressDialog();

                // check to see if the layer is valid 
                // and destination is configured correctly
                if (isLayerValidated
                    && projectId != null && projectId.Length > 0
                    && name != null && name.Length > 0
                    && draftAccessList != null && draftAccessList.Length > 0
                    && uploadType.Equals("vector") || (uploadType.Equals("raster") && attribution != null && attribution.Length > 0))
                {
                    // show the processing dialog
                    processingDialog.Show();

                    // create a reference to the Google Maps Engine API
                    MapsEngine.API.GoogleMapsEngineAPI api = new MapsEngine.API.GoogleMapsEngineAPI(ref log);

                    // establish a reference to the running ArcMap instance
                    ESRI.ArcGIS.ArcMapUI.IMxDocument mxDoc = (ESRI.ArcGIS.ArcMapUI.IMxDocument)ArcMap.Application.Document;

                    // retrieve a reference to the selected layer
                    ESRI.ArcGIS.Carto.ILayer selectedLayer = mxDoc.SelectedLayer;

                    // create a temporary working directory
                    System.IO.DirectoryInfo tempDir
                        = System.IO.Directory.CreateDirectory(ext.getLocalWorkspaceDirectory() + "\\" + System.Guid.NewGuid());

                    // determine what type of layer is selected
                    if (selectedLayer is ESRI.ArcGIS.Carto.IFeatureLayer)
                    {
                        // export a copy of the feature class to a "uploadable" Shapefile
                        // raise a processing notification
                        ext.publishRaiseDownloadProgressChangeEvent(false, "Extracting a copy of '" + selectedLayer.Name + "' (feature class) for data upload.");
                        ExportLayerToShapefile(tempDir.FullName, selectedLayer.Name, selectedLayer);

                        processingDialog.Update();
                        processingDialog.Focus();

                        // create a list of files in the temp directory
                        List<String> filesNames = new List<String>();
                        for (int k = 0; k < tempDir.GetFiles().Count(); k++)
                            if (!tempDir.GetFiles()[k].Name.EndsWith(".lock"))
                                filesNames.Add(tempDir.GetFiles()[k].Name);

                        // create a Google Maps Engine asset record
                        ext.publishRaiseDownloadProgressChangeEvent(false, "Requesting a new Google Maps Engine asset be created.");
                        MapsEngine.DataModel.gme.UploadingAsset uploadingAsset
                            = api.createVectorTableAssetForUploading(ext.getToken(),
                            MapsEngine.API.GoogleMapsEngineAPI.AssetType.table,
                            projectId,
                            name,
                            draftAccessList,
                            filesNames,
                            description,
                            tags.Split(",".ToCharArray()).ToList<String>(),
                            "UTF-8");

                        // Initiate upload of file(s)
                        ext.publishRaiseDownloadProgressChangeEvent(false, "Starting to upload files...");
                        api.uploadFilesToAsset(ext.getToken(), uploadingAsset.id, "tables", tempDir.GetFiles());

                        // launch a web browser
                        ext.publishRaiseDownloadProgressChangeEvent(false, "Launching a web browser.");
                        System.Diagnostics.Process.Start("https://mapsengine.google.com/admin/#RepositoryPlace:cid=" + projectId +"&v=DETAIL_INFO&aid=" + uploadingAsset.id);
                    }
                    else if (selectedLayer is ESRI.ArcGIS.Carto.IRasterLayer)
                    {
                        // export a copy of the raster to a format that can be uploaded
                        // raise a processing notification
                        ext.publishRaiseDownloadProgressChangeEvent(false, "Extracting a copy of '" + selectedLayer.Name + "' (raster) for data upload.");
                        ExportLayerToUploadableRaster(tempDir.FullName, selectedLayer.Name, selectedLayer);

                        processingDialog.Update();
                        processingDialog.Focus();

                        // create a list of files in the temp directory
                        List<String> filesNames = new List<String>();
                        for (int k = 0; k < tempDir.GetFiles().Count(); k++)
                            if (!tempDir.GetFiles()[k].Name.EndsWith(".lock"))
                                filesNames.Add(tempDir.GetFiles()[k].Name);

                        // create a Google Maps Engine asset record
                        ext.publishRaiseDownloadProgressChangeEvent(false, "Requesting a new Google Maps Engine asset be created.");
                        MapsEngine.DataModel.gme.UploadingAsset uploadingAsset
                            = api.createRasterAssetForUploading(ext.getToken(),
                            projectId,
                            name,
                            draftAccessList,
                            attribution, // attribution
                            filesNames,
                            description,
                            tags.Split(",".ToCharArray()).ToList<String>(),
                            acquisitionTime,
                            maskType);

                        // Initiate upload of file(s)
                        ext.publishRaiseDownloadProgressChangeEvent(false, "Starting to upload files...");
                        api.uploadFilesToAsset(ext.getToken(), uploadingAsset.id, "rasters", tempDir.GetFiles());

                        // launch a web browser
                        ext.publishRaiseDownloadProgressChangeEvent(false, "Launching a web browser.");
                        System.Diagnostics.Process.Start("https://mapsengine.google.com/admin/#RepositoryPlace:cid="+ projectId +"&v=DETAIL_INFO&aid=" + uploadingAsset.id);
                    }

                    // Ask the user if the temporary files should be deleted
                    DialogResult dialogResult = MessageBox.Show(
                        String.Format(Properties.Resources.dialog_dataUpload_tempFileCleanupMessage,
                            tempDir.GetFiles().Count(), tempDir.FullName),
                        ext.getAddinName() + " Temporary File Clean-up", 
                        MessageBoxButtons.YesNo);

                    // if the user wishes to delete the temporary files, delete them
                    if (dialogResult == DialogResult.Yes)
                    {
                        try
                        {
                            // remove the temporary layer from the ArcMap session
                            ext.removeLayerByName((uploadType.Equals("vector") ? selectedLayer.Name : selectedLayer.Name + ".tif"), tempDir.FullName);

                            // Delete temporary directory and containing files
                            tempDir.Delete(true);
                        }
                        catch (Exception) { }
                    }

                    // close the processing dialog
                    ext.publishRaiseDownloadProgressChangeEvent(true, "Finished");
                    processingDialog.Close();
                }
                else
                {
                    // display an error dialog to the user
                    System.Windows.Forms.MessageBox.Show("The selected layer is invalid or the destination configuration has not been properly set.");
                }

            }
            catch (System.Exception ex)
            {
                // Ops.
                System.Windows.Forms.MessageBox.Show("An error occured. " + ex.Message);
            }

            // close the dialog
            this.Close();
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
            private UploadToGoogleMapsEngine mForm;

            // Constructor.
            public BrowerScriptManager(UploadToGoogleMapsEngine form)
            {
                // Save the form so it can be referenced later.
                mForm = form;
            }

            // This method can be called from JavaScript.
            public void handleUploadClickEvent(String projectId, String name, String description, String tags, String draftAccessList, String attribution, String acquisitionTime, String maskType)
            {
                // need to insure tags is not null to avoid exceptions when splitting (cannot be set from javascript as empty strings marshall as a null).
                if (null == tags)
                {
                    tags = "";
                }
                // Call a method on the form.
                mForm.uploadButtonClicked(projectId, name, description, tags, draftAccessList, attribution, acquisitionTime, maskType);
            }

            // This method can be called from JavaScript.
            public void handleCancelClickEvent()
            {
                // Call a method on the form.
                mForm.cancelButtonClicked();
            }
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // create an object array to house the required parameters
            System.Object[] o = new System.Object[4];
            o[0] = (System.Object)Properties.Settings.Default.oauth2_settings_clientId; // projectId
            o[1] = (System.Object)Properties.Settings.Default.gme_api_key; // projectKey
            o[2] = (System.Object)ext.getToken().access_token; // access_token
            o[3] = (System.Object)uploadType; // dataset upload type

            // call the JavaScript setConfiguration function to set API info
            webBrowser.Document.InvokeScript("setConfiguration", o);
        }
    }
}
