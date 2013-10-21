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
using System.Linq;
using System.Text;
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

namespace com.google.mapsengine.connectors.arcgis.Extension.Data
{
    /*
     * An assortment of File Geodatabase utilities
     */
    public static class GeodatabaseUtilities
    {
        /*
         * A utility class to create a File Geodatabase Workspace
         */
        public static IWorkspace createFileGeodatabaseWorkspace(ref log4net.ILog log, string workspaceDirectory, string workspaceFolderName)
        {
            log.Debug("Starting the process to create a new local File Geodatabase workspace.");

            // Instantiate a file geodatabase workspace factory and create a new file geodatabase.
            // The Create method returns a workspace name object.
            log.Debug("Initializing a new workspace factory to establish the FGB.");
            IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();
            IWorkspaceName workspaceName = null;

            // verify the workspace does not already exist
            log.Debug("Determining if the workspace apready exists or not.");
            if (System.IO.Directory.Exists(workspaceDirectory + "\\" + workspaceFolderName))
            {
                // file geodatabase workspace already exist, throw warning
                log.Warn("File Geodatabase workspace already exists.");
                throw new System.Exception(Properties.Resources.fgdb_GeodatabaseUtilities_createFileGeodatabaseWorkspace_exists);
            }
            else
            {
                // workspace doens't exist, good
                log.Debug("File Geodatabase does not exist.");

                // verify the directory exists
                log.Debug("Checking to see if the workspace directory exists or not (parent of the FGD).");
                if (!System.IO.Directory.Exists(workspaceDirectory))
                {
                    // workspace directory doens't exist, create the directory
                    log.Debug("Workspace folder/parent doesn't exist, creating the directory.");
                    System.IO.Directory.CreateDirectory(workspaceDirectory);
                }

                // create the workspace
                log.Debug("Creating the File Geodatabase workspace.");
                workspaceName = workspaceFactory.Create(workspaceDirectory, workspaceFolderName, null, 0);

                // Cast the workspace name object to the IName interface and open the workspace.
                log.Debug("Retreiving the workspace name.");
                IName name = (IName)workspaceName;

                // return the workspace
                log.Debug("Opening the workspace and returning it to the requestor.");
                return (IWorkspace)name.Open();
            }
        }

        /*
         * A utility function to open the File Geodatabase workspace
         */
        public static IWorkspace openFileGeodatabaseWorkspace(ref log4net.ILog log, string workspaceDirectory, string workspaceFolderName)
        {
            // attempting to open the file geodatabase
            log.Debug("Attempting to open the File Geodatabase.");
            log.Debug("Workspace: " + workspaceDirectory);
            log.Debug("Directory: " + workspaceFolderName);
            try
            {
                // verify the workspace does already exist
                log.Debug("Checking to see if the referenced File Geodatabase exists or not.");
                if (System.IO.Directory.Exists(workspaceDirectory + "\\" + workspaceFolderName))
                {
                    // File Geodatabase exist, create/configure a properties set
                    log.Debug("Creating a property set object.");
                    IPropertySet propertySet = new PropertySetClass();

                    // set the database value
                    log.Debug("Setting the DATABASE value of the property set.");
                    propertySet.SetProperty("DATABASE", workspaceDirectory + "\\" + workspaceFolderName);

                    // create a new workspace factory to open the FDGB
                    log.Debug("Creating a new workspace factory to open the file geodatabase.");
                    IWorkspaceFactory workspaceFactory = new FileGDBWorkspaceFactoryClass();

                    // Open the FDGB and return it
                    log.Debug("Opening and returning the workspace.");
                    return workspaceFactory.Open(propertySet, 0);
                }
                else
                {
                    // Referenced File Geodatabase (FGDB) does ont exist
                    log.Warn("Referenced File Geodatabase (FGDB) does not exist");
                    throw new System.Exception(Properties.Resources.fgdb_GeodatabaseUtilities_openFileGeodatabaseWorkspace_notfound);
                }
            }
            catch (System.Exception ex)
            {
                // an error occured, log and throw
                log.Error(ex);
                throw new System.Exception(Properties.Resources.fgdb_GeodatabaseUtilities_openFileGeodatabaseWorkspace_unknown);
            }
        }

        /*
         * A utility class to delete a File Geodatabase (FGDB).
         */
        public static bool deleteFileGeodatabaseWorkspace(ref log4net.ILog log, string workspaceDirectory, string workspaceFolderName)
        {
            // attempt to delete the referenced File Geodatabase (FGDB)
            log.Debug("Attempting to delete the referenced File Geodatabase (FGDB).");
            log.Debug("workspaceDirectory: " + workspaceDirectory);
            log.Debug("workspaceFolderName: " + workspaceFolderName);
            try
            {
                // verify the workspace does already exist
                log.Debug("Verifying if the File Geodatabase exists.");
                if (!System.IO.Directory.Exists(workspaceDirectory + "\\" + workspaceFolderName))
                {
                    // the File Geodatabase does not exist
                    log.Warn("File Geodatabase does not exist.  Unable to delete.");
                    throw new System.Exception(Properties.Resources.fgdb_GeodatabaseUtilities_deleteFileGeodatabaseWorkspace_notfound);
                }
                else
                {
                    // File Geodatabase exists, create a new DirecotryInfo object to represent the <name>.gdb
                    log.Debug("File Geodatabase exists, creating a new DirectoryInfo object to represent the .gdb.");
                    System.IO.DirectoryInfo dinfo = new System.IO.DirectoryInfo(workspaceDirectory + "\\" + workspaceFolderName);

                    // once again, verifying the folder itself exists
                    log.Debug("Checking again to see if the folder itself exists.");
                    if (dinfo.Exists)
                    {
                        // the File Geodatabase folder exists.  Attempt to delete it.
                        log.Debug("File Geodatabase folder exists, attempt to delete it.");
                        dinfo.Delete(true);

                        // return true
                        log.Debug("Returning true/successful.");
                        return true;
                    }
                    else
                    {
                        // the <name>.gdb folder does not exist, return false
                        log.Debug("File Geodatabase folder does not exist, return false.");
                        return false;
                    }
                }
            }
            catch (System.Exception ex)
            {
                // an unknown error occured, log and throw
                log.Error(ex);
                throw new System.Exception(Properties.Resources.fgdb_GeodatabaseUtilities_deleteFileGeodatabaseWorkspace_unknown);
            }
        }


        public static IFeatureClass createGoogleMapsEngineCatalogFeatureClass(ref log4net.ILog log, ref GoogleMapsEngineToolsExtensionForArcGIS ext)
        {
            try
            {
                // temporary directory to store workspace 
                string workspacedirectory = ext.getLocalWorkspaceDirectory().FullName;

                // add the directory to the cleanup list
                // TODO: Replace with scratch
                ext.addTemporaryDirectory(new System.IO.DirectoryInfo(workspacedirectory));

                // determine the workspace name for the geodatabase
                //string workspacefoldername = Properties.Settings.Default.extension_gdb_workspacename;
                // TODO: Use sctach workspace instead of creating a temporary one
                string workspacefoldername = "GME_Data_" + System.Guid.NewGuid().ToString().Replace("-", "");

                // define a workspace to do work
                IWorkspace workspace = null;

                // attempt to open or create the workspace
                try
                {
                    // check to see if the workspace already exists, if so, open it
                    if (System.IO.Directory.Exists(workspacedirectory + "\\" + workspacefoldername))
                    {
                        workspace = Extension.Data.GeodatabaseUtilities.openFileGeodatabaseWorkspace(ref log, workspacedirectory, workspacefoldername);
                        ESRI.ArcGIS.Geodatabase.IFeatureWorkspace featureWorkspace = (ESRI.ArcGIS.Geodatabase.IFeatureWorkspace)workspace;
                        ESRI.ArcGIS.Geodatabase.IFeatureClass featureClass = featureWorkspace.OpenFeatureClass(Properties.Resources.GeodatabaseUtilities_schema_FeatureClassName);
                        ESRI.ArcGIS.Geodatabase.IDataset pdataset = (ESRI.ArcGIS.Geodatabase.IDataset)featureClass;
                        if (pdataset.CanDelete())
                            pdataset.Delete();

                        pdataset = null;
                        featureClass = null;
                        featureWorkspace = null;

                        // TODO: Open instead of delete/replace
                        //if (arcgis.ext.gdb.GeodatabaseUtilities.deleteFileGeodatabaseWorkspace(workspacedirectory, workspacefoldername))
                        //workspace = arcgis.ext.gdb.GeodatabaseUtilities.createFileGeodatabaseWorkspace(workspacedirectory, workspacefoldername);
                    }
                    else
                    {
                        // workspace doesn't exist, create the workspace
                        workspace = Extension.Data.GeodatabaseUtilities.createFileGeodatabaseWorkspace(ref log, workspacedirectory, workspacefoldername);
                    }
                }
                catch (System.Exception ex)
                {
                    // unable to create the fgdb or unable to delete the fc within the fgdb
                    log.Error(ex);
                    System.Windows.Forms.MessageBox.Show("Unable to create or delete an existing feature class.");
                }

                // verify the workspace is open
                if (workspace != null)
                {
                    // create a new feature workspace to work spatially
                    IFeatureWorkspace featureWorkspace = workspace as IFeatureWorkspace;

                    // create a spatial reference for the Google Earth Builder data (always in 4326)
                    SpatialReferenceEnvironment sRefEnvGEB = new SpatialReferenceEnvironment();
                    ISpatialReference sGEBRef = sRefEnvGEB.CreateGeographicCoordinateSystem(4326);

                    // for this feature class, create and determine the field
                    IFields fields = new FieldsClass();
                    IFieldsEdit fieldsEdit = (IFieldsEdit)fields;
                    fieldsEdit.FieldCount_2 = 10;

                    //Create the Object ID field.
                    IField fusrDefinedField = new Field();
                    IFieldEdit fusrDefinedFieldEdit = (IFieldEdit)fusrDefinedField;
                    fusrDefinedFieldEdit.Name_2 = Properties.Resources.GeodatabaseUtilities_schema_OBJECTID_Name;
                    fusrDefinedFieldEdit.AliasName_2 = Properties.Resources.GeodatabaseUtilities_schema_OBJECTID_AliasName;
                    fusrDefinedFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
                    fieldsEdit.set_Field(0, fusrDefinedField);

                    //Create the CustomerId field.
                    fusrDefinedField = new Field();
                    fusrDefinedFieldEdit = (IFieldEdit)fusrDefinedField;
                    fusrDefinedFieldEdit.Name_2 = Properties.Resources.GeodatabaseUtilities_schema_CustomerId_Name;
                    fusrDefinedFieldEdit.AliasName_2 = Properties.Resources.GeodatabaseUtilities_schema_CustomerId_AliasName;
                    fusrDefinedFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                    fieldsEdit.set_Field(1, fusrDefinedField);

                    //Create the MapAssetId field.
                    fusrDefinedField = new Field();
                    fusrDefinedFieldEdit = (IFieldEdit)fusrDefinedField;
                    fusrDefinedFieldEdit.Name_2 = Properties.Resources.GeodatabaseUtilities_schema_MapAssetId_Name;
                    fusrDefinedFieldEdit.AliasName_2 = Properties.Resources.GeodatabaseUtilities_schema_MapAssetId_AliasName;
                    fusrDefinedFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                    fieldsEdit.set_Field(2, fusrDefinedField);

                    //Create the AssetId field.
                    fusrDefinedField = new Field();
                    fusrDefinedFieldEdit = (IFieldEdit)fusrDefinedField;
                    fusrDefinedFieldEdit.Name_2 = Properties.Resources.GeodatabaseUtilities_schema_AssetId_Name;
                    fusrDefinedFieldEdit.AliasName_2 = Properties.Resources.GeodatabaseUtilities_schema_AssetId_AliasName;
                    fusrDefinedFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                    fieldsEdit.set_Field(3, fusrDefinedField);

                    //Create the ParentAssetId field.
                    fusrDefinedField = new Field();
                    fusrDefinedFieldEdit = (IFieldEdit)fusrDefinedField;
                    fusrDefinedFieldEdit.Name_2 = Properties.Resources.GeodatabaseUtilities_schema_ParentAssetId_Name;
                    fusrDefinedFieldEdit.AliasName_2 = Properties.Resources.GeodatabaseUtilities_schema_ParentAssetId_AliasName;
                    fusrDefinedFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                    fieldsEdit.set_Field(4, fusrDefinedField);

                    //Create the AssetType field.
                    fusrDefinedField = new Field();
                    fusrDefinedFieldEdit = (IFieldEdit)fusrDefinedField;
                    fusrDefinedFieldEdit.Name_2 = Properties.Resources.GeodatabaseUtilities_schema_AssetType_Name;
                    fusrDefinedFieldEdit.AliasName_2 = Properties.Resources.GeodatabaseUtilities_schema_AssetType_AliasName;
                    fusrDefinedFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                    fieldsEdit.set_Field(5, fusrDefinedField);

                    //Create the AssetName field.
                    fusrDefinedField = new Field();
                    fusrDefinedFieldEdit = (IFieldEdit)fusrDefinedField;
                    fusrDefinedFieldEdit.Name_2 = Properties.Resources.GeodatabaseUtilities_schema_AssetName_Name;
                    fusrDefinedFieldEdit.AliasName_2 = Properties.Resources.GeodatabaseUtilities_schema_AssetName_AliasName;
                    fusrDefinedFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                    fieldsEdit.set_Field(6, fusrDefinedField);

                    //Create the AssetDescription field.
                    fusrDefinedField = new Field();
                    fusrDefinedFieldEdit = (IFieldEdit)fusrDefinedField;
                    fusrDefinedFieldEdit.Name_2 = Properties.Resources.GeodatabaseUtilities_schema_AssetDescription_Name;
                    fusrDefinedFieldEdit.AliasName_2 = Properties.Resources.GeodatabaseUtilities_schema_AssetDescription_AliasName;
                    fusrDefinedFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                    fieldsEdit.set_Field(7, fusrDefinedField);

                    //Create the MapSharedWith field.
                    fusrDefinedField = new Field();
                    fusrDefinedFieldEdit = (IFieldEdit)fusrDefinedField;
                    fusrDefinedFieldEdit.Name_2 = Properties.Resources.GeodatabaseUtilities_schema_MapSharedWith_Name;
                    fusrDefinedFieldEdit.AliasName_2 = Properties.Resources.GeodatabaseUtilities_schema_MapSharedWith_AliasName;
                    fusrDefinedFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                    fieldsEdit.set_Field(8, fusrDefinedField);

                    // Create the Shape field.
                    fusrDefinedField = new Field();
                    fusrDefinedFieldEdit = (IFieldEdit)fusrDefinedField;
                    // Set up the geometry definition for the Shape field.
                    IGeometryDef geometryDef = new GeometryDefClass();
                    IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                    geometryDefEdit.GeometryType_2 = ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon;
                    // By setting the grid size to 0, you're allowing ArcGIS to determine the appropriate grid sizes for the feature class. 
                    // If in a personal geodatabase, the grid size will be 1000. If in a file or ArcSDE geodatabase, the grid size
                    // will be based on the initial loading or inserting of features.
                    geometryDefEdit.GridCount_2 = 1;
                    geometryDefEdit.set_GridSize(0, 0);
                    geometryDefEdit.HasM_2 = false;
                    geometryDefEdit.HasZ_2 = false;
                    //Assign the spatial reference that was passed in, possibly from
                    //IGeodatabase.SpatialReference for the containing feature dataset.
                    geometryDefEdit.SpatialReference_2 = sGEBRef;
                    // Set standard field properties.
                    fusrDefinedFieldEdit.Name_2 = "SHAPE";
                    fusrDefinedFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                    fusrDefinedFieldEdit.GeometryDef_2 = geometryDef;
                    fusrDefinedFieldEdit.IsNullable_2 = true;
                    fusrDefinedFieldEdit.Required_2 = true;
                    fieldsEdit.set_Field(9, fusrDefinedField);

                    // Create a feature class description object to use for specifying the CLSID and EXTCLSID.
                    IFeatureClassDescription fcDesc = new FeatureClassDescriptionClass();
                    IObjectClassDescription ocDesc = (IObjectClassDescription)fcDesc;

                    IFeatureClass fc = featureWorkspace.CreateFeatureClass(
                        Properties.Resources.GeodatabaseUtilities_schema_FeatureClassName, // Feature Class Name
                        fields, // Feature Class Fields (defined above)
                        ocDesc.InstanceCLSID, 
                        ocDesc.ClassExtensionCLSID, 
                        esriFeatureType.esriFTSimple, 
                        fcDesc.ShapeFieldName, // Shape Field Name
                        "" // Keyword Configurations
                        );

                    // return the feature class
                    return fc;
                }
                else
                {
                    // end gracefully, maybe prompt the user that the toolbar wasn't able to create a workspcae
                    throw new Exception("Unable to open local geodatabase.");
                }
            }
            catch (System.Exception ex)
            {
                // an error occured
                log.Error(ex);

                // throw an exception
                throw new Exception("An unknown exception occured while attempting to create a local feature class.");
            }
        }
    
    }
}
