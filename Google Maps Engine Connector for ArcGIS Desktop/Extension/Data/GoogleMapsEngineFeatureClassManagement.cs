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
     * The GoogleMapsEngineFeatureClassManagement class is used to save
     * Google Maps Engine API responses (asset for maps and layers) into a 
     * local FileGeodatabase for further use by the Connector.
     */
    class GoogleMapsEngineFeatureClassManagement
    {
        // setup and configure log4net
        private static log4net.ILog log = LogManager.GetLogger(typeof(GoogleMapsEngineFeatureClassManagement));

        // an uninitailized refernece to the Extension object
        protected GoogleMapsEngineToolsExtensionForArcGIS ext;

        // a polygon representing the entire world (default Shape)
        protected IPolygon worldPolygon;

        // a reference to the Google Maps Engine API
        protected MapsEngine.API.GoogleMapsEngineAPI api;

        // the constructor for the Feature Class Management class
        public GoogleMapsEngineFeatureClassManagement(MapsEngine.API.GoogleMapsEngineAPI api)
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("GoogleMapsEngineFeatureClassManagement initializing.");

            // retrieve a reference to the extension
            log.Debug("Retrieiving a reference to the extension object.");
            ext = GoogleMapsEngineToolsExtensionForArcGIS.GetExtension();

            // initiate the Google Maps Enigne API object
            log.Debug("Setting the GME API object.");
            this.api = api;

            // generate a worldwide polygon, for default (spatial geometry undetermined or undefined) assets
            log.Debug("Establishing a default worldwide polygon object.");
            IPoint pUL = new Point();
            pUL.X = -180;
            pUL.Y = 90;
            IPoint pLL = new Point();
            pLL.X = -180;
            pLL.Y = -90;
            IPoint pLR = new Point();
            pLR.X = 180;
            pLR.Y = -90;
            IPoint pUR = new Point();
            pUR.X = 180;
            pUR.Y = 90;

            // add the points to the point collection
            IPointCollection pPtColl = new Polygon();
            pPtColl.AddPoint(pUL, Type.Missing, Type.Missing);
            pPtColl.AddPoint(pUR, Type.Missing, Type.Missing);
            pPtColl.AddPoint(pLR, Type.Missing, Type.Missing);
            pPtColl.AddPoint(pLL, Type.Missing, Type.Missing);
            pPtColl.AddPoint(pUL, Type.Missing, Type.Missing);

            // define the polygon as a list of points, then close the polygon
            worldPolygon = (IPolygon)pPtColl;
            worldPolygon.Close();
        }

        /*
         * A function to populate a referenced FeatureClass (fc) with the contents of a Google Maps Engine map
         */
        public void populateFCWithGoogleMapsEngineMap(ref IFeatureClass fc, ref MapsEngine.DataModel.gme.Map map)
        {
            // create a feature object
            log.Debug("Creating a new Feature object to be used later in populating the FC.");
            IFeature feature = fc.CreateFeature();

            log.Debug("Creating feature for map " + map.id);

            // create a projectId value from the MapId
            String projectId = map.id.Split("-".ToCharArray())[0];

            // Update the values for this feature
            feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_CustomerId_Name), projectId);
            log.Debug(Properties.Resources.GeodatabaseUtilities_schema_CustomerId_Name + ": " + projectId);
            feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_AssetId_Name), map.id);
            log.Debug(Properties.Resources.GeodatabaseUtilities_schema_AssetId_Name + ": " + map.id);
            feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_MapAssetId_Name), map.id);
            log.Debug(Properties.Resources.GeodatabaseUtilities_schema_MapAssetId_Name + ": " + map.id);
            feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_AssetType_Name), "map");
            log.Debug(Properties.Resources.GeodatabaseUtilities_schema_AssetType_Name + ": " + "map");
            feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_AssetName_Name), map.name);
            log.Debug(Properties.Resources.GeodatabaseUtilities_schema_AssetName_Name + ": " + map.name);
            feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_ParentAssetId_Name), projectId);
            log.Debug(Properties.Resources.GeodatabaseUtilities_schema_ParentAssetId_Name + ": " + projectId);

            // attempt to set the description
            try
            {
                // attempt to set the feature description (truncating if necessary)
                feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_AssetDescription_Name)
                    , map.description.Length > 256
                    ? map.description.Substring(0, 252) + "..."
                    : map.description);
                log.Debug(Properties.Resources.GeodatabaseUtilities_schema_AssetDescription_Name + ": " + map.description);
            }
            catch (System.Exception ex)
            {
                // warn
                log.Warn(ex);
            }

            // check to see if the bbox object is not null
            log.Debug("Adding spatial representation if available.");
            if (map.bbox != null && map.bbox.Count() == 4)
            {
                // deterine the maximum and minimum bounds of all layers within this map
                // 0=West, 1=South, 2=East, 3=North
                double XMAX = map.bbox[2];
                log.Debug("XMAX: " + XMAX);
                double YMAX = map.bbox[3];
                log.Debug("YMAX: " + YMAX);
                double XMIN = map.bbox[0];
                log.Debug("XMIN: " + XMIN);
                double YMIN = map.bbox[1];
                log.Debug("YMIN: " + YMIN);


                // determine the map extent based on the layers maximum extent
                IPoint pExtentNE = new Point();
                pExtentNE.X = XMAX;
                pExtentNE.Y = YMAX;
                IPoint pExtentSW = new Point();
                pExtentSW.X = XMIN;
                pExtentSW.Y = YMIN;
                IPoint pExtentNW = new Point();
                pExtentNW.X = XMIN;
                pExtentNW.Y = YMAX;
                IPoint pExtentSE = new Point();
                pExtentSE.X = XMAX;
                pExtentSE.Y = YMIN;

                // define the polygon bounding box (NE/SW) as a point collection
                log.Debug("Building polygon object.");
                IPointCollection pExtentPointCol = new Polygon();
                pExtentPointCol.AddPoint(pExtentNE, Type.Missing, Type.Missing);
                pExtentPointCol.AddPoint(pExtentSE, Type.Missing, Type.Missing);
                pExtentPointCol.AddPoint(pExtentSW, Type.Missing, Type.Missing);
                pExtentPointCol.AddPoint(pExtentNW, Type.Missing, Type.Missing);

                // create a polygon, p, from the point collection, then close the polygon
                IPolygon pExtent = (IPolygon)pExtentPointCol;
                pExtent.Close();

                // add the polygon, p, as the new feature's geometry
                if (pExtent != null)
                {
                    log.Debug("Setting feature's geometry.");
                    feature.Shape = pExtent;
                }
                else
                {
                    log.Warn("Polygon is not valid, setting feature geometry to default worldwide.");
                    feature.Shape = worldPolygon;
                }
            }
            else
            {
                // no spatial information contained, use world
                log.Warn("No spatial representation, setting feature geometry to default worldwide.");
                //feature.Shape = worldPolygon;
            }

            // Commit the new feature to fc
            log.Debug("Storing feature.");
            feature.Store();

            // add all child layers within this map
            log.Debug("Verifying the map has layers.");
            if (map.layers != null)
            {
                // go through each layer in the map
                log.Debug("Going through each layer in the map.");
                foreach (MapsEngine.DataModel.gme.MapLayer layer in map.layers)
                {
                    // populate the features in the layer
                    log.Debug("Processing layer " + layer.id);
                    populateFCWithGoogleMapsEngineLayer(ref fc, map.id, map.id, layer);
                }
            }

            // Verifying the map has a folders object
            log.Debug("Verifying the map has a folders object.");
            if (map.folders != null && map.folders.Count() > 0)
            {
                // populate the features in the folder
                log.Debug("Processing folders");
                populateFCWithGoogleMapsEngineFolders(ref fc, map.id, map.id, map.folders);
            }
        }

        /*
         * A function to populate a referenced Feature Class (fc) with the contents of a Google Maps Engine map layer
         */
        protected void populateFCWithGoogleMapsEngineLayer(ref IFeatureClass fc, string mapId, string parentId, MapsEngine.DataModel.gme.MapLayer layer)
        {
            // create a new feature
            IFeature feature;

            log.Debug("Creating a feature for layer " + layer.id);

            // attempt to process the assets within a the layer
            try
            {
                // fetch the layer asset object from the API
                log.Debug("Fetching an asset object for the layer.");
                MapsEngine.DataModel.gme.Asset layerAsset = api.getAssetById(ext.getToken(), layer.id);

                // create a new feature
                log.Debug("Creating a new feature.");
                feature = fc.CreateFeature();

                // Update the values for this feature
                feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_CustomerId_Name), mapId.Split("-".ToCharArray())[0]);
                log.Debug(Properties.Resources.GeodatabaseUtilities_schema_CustomerId_Name + ": " + mapId.Split("-".ToCharArray())[0]);
                feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_AssetId_Name), layer.id);
                log.Debug(Properties.Resources.GeodatabaseUtilities_schema_AssetId_Name + ": " + layer.id);
                feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_MapAssetId_Name), mapId);
                log.Debug(Properties.Resources.GeodatabaseUtilities_schema_MapAssetId_Name + ": " + mapId);
                feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_AssetType_Name), layerAsset.type);
                log.Debug(Properties.Resources.GeodatabaseUtilities_schema_AssetType_Name + ": " + layerAsset.type);
                feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_AssetName_Name), layerAsset.name);
                log.Debug(Properties.Resources.GeodatabaseUtilities_schema_AssetName_Name + ": " + layerAsset.name);
                feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_ParentAssetId_Name), parentId);
                log.Debug(Properties.Resources.GeodatabaseUtilities_schema_ParentAssetId_Name + ": " + parentId);

                // attempt to set the description object
                try
                {
                    // set the layer description value (truncate if necessary)
                    feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_AssetDescription_Name)
                        , layerAsset.description.Length > 256
                        ? layerAsset.description.Substring(0, 252) + "..."
                        : layerAsset.description);
                    log.Debug(Properties.Resources.GeodatabaseUtilities_schema_AssetDescription_Name + ": " + layerAsset.description);
                }
                catch (System.Exception ex)
                {
                    // log warning
                    log.Warn(ex);
                }

                // verify the layer has a bbox and has two valid points
                if (layerAsset.bbox != null && layerAsset.bbox.Count() == 4)
                {
                    // deterine the maximum and minimum bounds of all layers within this map
                    // 0=West, 1=South, 2=East, 3=North
                    double XMAX = layerAsset.bbox[2];
                    log.Debug("XMAX: " + XMAX);
                    double YMAX = layerAsset.bbox[3];
                    log.Debug("YMAX: " + YMAX);
                    double XMIN = layerAsset.bbox[0];
                    log.Debug("XMIN: " + XMIN);
                    double YMIN = layerAsset.bbox[1];
                    log.Debug("YMIN: " + YMIN);

                    // determine the map extent based on the layers maximum extent
                    IPoint pExtentNE = new Point();
                    pExtentNE.X = XMAX;
                    pExtentNE.Y = YMAX;
                    IPoint pExtentSW = new Point();
                    pExtentSW.X = XMIN;
                    pExtentSW.Y = YMIN;
                    IPoint pExtentNW = new Point();
                    pExtentNW.X = XMIN;
                    pExtentNW.Y = YMAX;
                    IPoint pExtentSE = new Point();
                    pExtentSE.X = XMAX;
                    pExtentSE.Y = YMIN;

                    // define the polygon bounding box (NE/SW) as a point collection
                    IPointCollection pExtentPointCol = new Polygon();
                    pExtentPointCol.AddPoint(pExtentNE, Type.Missing, Type.Missing);
                    pExtentPointCol.AddPoint(pExtentSE, Type.Missing, Type.Missing);
                    pExtentPointCol.AddPoint(pExtentSW, Type.Missing, Type.Missing);
                    pExtentPointCol.AddPoint(pExtentNW, Type.Missing, Type.Missing);

                    // create a polygon, p, from the point collection, then close the polygon
                    IPolygon pExtent = (IPolygon)pExtentPointCol;
                    pExtent.Close();

                    // add the polygon, p, as the new feature's geometry
                    if (pExtent != null)
                    {
                        // set the shape geometry
                        log.Debug("Setting the feature's geometry as the polygon.");
                        feature.Shape = pExtent;
                    }
                    else
                    {
                        // set the feature's goemetry as the default world
                        log.Debug("Invalid spatial representation, setting feature goemtry as the world.");
                        feature.Shape = worldPolygon;
                    }
                }
                else
                {
                    // the layer does not have spatial information
                    log.Warn("Layer has no bbox information");
                    //feature.Shape = worldPolygon;
                }

                // Commit the new feature to fc
                log.Debug("Storing feature.");
                feature.Store();
            }
            catch (Exception ex)
            {
                // log the warning
                log.Warn(ex);
            }
        }

        /*
         * A function to populate a referenced Feature Class (fc) with the contents of a Google Mpas Engine map folder
         */
        protected void populateFCWithGoogleMapsEngineFolders(ref IFeatureClass fc, string mapId, string parentId, List<MapsEngine.DataModel.gme.MapFolder> folders)
        {
            // establish a feature object
            IFeature feature;

            // add all child folder within this map
            for (int f = 0; f < folders.Count(); f++)
            {
                // grap the current folder
                MapsEngine.DataModel.gme.MapFolder folder = folders[f];

                // add all child layers within this map
                foreach (MapsEngine.DataModel.gme.MapLayer layer in folder.layers)
                {
                    populateFCWithGoogleMapsEngineLayer(ref fc, mapId, "Folder" + f, layer);
                }

                // create a new feature
                feature = fc.CreateFeature();

                // Update the values for this feature
                feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_CustomerId_Name), mapId.Split("-".ToCharArray())[0]);
                feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_AssetId_Name), "Folder" + f);
                feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_MapAssetId_Name), mapId);
                feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_AssetType_Name), "folder");
                feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_AssetName_Name), "Folder " + f);
                feature.set_Value(fc.FindField(Properties.Resources.GeodatabaseUtilities_schema_ParentAssetId_Name), parentId);

                // Commit the new feature to fc
                feature.Store();

                // if the folder contains subfolders, add them
                if (folder.folders != null && folder.folders.Count() > 0)
                {
                    populateFCWithGoogleMapsEngineFolders(ref fc, mapId, "Folder" + f, folder.folders);
                }
            }
        }
    }
}