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
using System.Net;
using System.Threading;
using com.google.mapsengine.connectors.arcgis.MapsEngine.DataModel.gme;
using Newtonsoft.Json;

namespace com.google.mapsengine.connectors.arcgis.MapsEngine.API
{
    /*
     * A wrapper for the Google Maps Engine API
     */
    class GoogleMapsEngineAPI
    {
        // the Google APIs project key
        protected String GOOGLE_API_KEY;

        // the Google Maps Engine API Configuration
        protected String GME_API_PROTOCOL;
        protected String GME_API_DOMAIN;
        protected String GME_API_SERVICE;
        protected String GME_API_VERSION;

        // the log4net reference
        log4net.ILog log;

        protected Guid GoogleMapsEngineAPISessionId;
        protected System.IO.DirectoryInfo debugDirectory;

        // an uninitailized refernece to the Extension object
        protected GoogleMapsEngineToolsExtensionForArcGIS ext;

        /*
         * Constructor for Google Maps API utilities
         */
        public GoogleMapsEngineAPI(ref log4net.ILog log)
        {
            // establish the Google APIs project key
            this.GOOGLE_API_KEY = Properties.Settings.Default.gme_api_key;

            // establish the Google Maps Engine API settings
            this.GME_API_PROTOCOL = Properties.Settings.Default.gme_api_protocol;
            this.GME_API_DOMAIN = Properties.Settings.Default.gme_api_domain;
            this.GME_API_SERVICE = Properties.Settings.Default.gme_api_service;
            this.GME_API_VERSION = Properties.Settings.Default.gme_api_version;

            // set the log
            this.log = log;

            // retrieve a reference to the extension
            log.Debug("Retrieiving a reference to the extension object.");
            ext = GoogleMapsEngineToolsExtensionForArcGIS.GetExtension();

            // create a Google Maps Engine Session Id for this set of sessions
            GoogleMapsEngineAPISessionId = Guid.NewGuid();

            // if debug, create a debug folder to keep track of information
            if (log.IsDebugEnabled)
            {
                // create a temporary folder
                debugDirectory = System.IO.Directory.CreateDirectory(
                    ext.getLocalWorkspaceDirectory() 
                    + "\\GME_API_TMP_" 
                    + GoogleMapsEngineAPISessionId.ToString().Replace("-",""));
            }
        }

        /*
         * Retreives a list of Google Maps Engine projects (accounts) for the 
         * authenticated user.
         */
        public List<Project> getProjects(Extension.Auth.OAuth2Token token)
        {
            // create a new blank list of projects
            log.Debug("Creating a blank set of projects.");
            List<Project> projects = new List<Project>();

            // build the request url to the GME API
            log.Debug("Building the Google Maps Engine API request URL.");
            string apiRequestUrl = GME_API_PROTOCOL 
                + "://" + GME_API_DOMAIN 
                + "/" + GME_API_SERVICE 
                + "/" + GME_API_VERSION 
                + "/" + "projects"
                + "?" + "key=" + GOOGLE_API_KEY;
            log.Debug("Request Url: " + apiRequestUrl);

            // attempt to make the request to the Google Maps Engine API
            try
            {
                // make the Google Maps Engine Request
                log.Debug("Making the Google Maps Engine Request.");
                String jsonResponse = makeGoogleMapsEngineRequest(apiRequestUrl, token.access_token);

                // Deserialize the Json object into a project list object
                log.Debug("Deserializing the Json response from the API into a ProjectList object.");
                projects = JsonConvert.DeserializeObject<ProjectList>(jsonResponse).projects;
            }
            catch (System.Exception ex)
            {
                // an exception has occured, log and throw
                log.Error(ex);
                throw new System.Exception(Properties.Resources.GoogleMapsEngineAPI_getProjects_unknownexception);
            }

            // return the projects list
            return projects;
        }

        /*
         * Retrieves a list of Google Maps Engine maps for the authenticated
         * user and within a specific Google Maps Engine project.
         */
        public List<Map> getMapsByProjectId(Extension.Auth.OAuth2Token token, string projectId)
        {
            // create a new blank list of maps
            log.Debug("Creating a blank set of maps.");
            List<Map> maps = new List<Map>();

            // build the request url to the GME API
            log.Debug("Building the Google Maps Engine API request URL.");
            string apiRequestUrl = GME_API_PROTOCOL
                + "://" + GME_API_DOMAIN
                + "/" + GME_API_SERVICE
                + "/" + GME_API_VERSION
                + "/" + "maps"
                + "?" + "key=" + GOOGLE_API_KEY
                + "&" + "projectId=" + projectId;
            log.Debug("Request Url: " + apiRequestUrl);

            // attempt to make the request to the Google Maps Engine API
            try
            {
                // make the Google Maps Engine Request
                log.Debug("Making the Google Maps Engine Request.");
                String jsonResponse = makeGoogleMapsEngineRequest(apiRequestUrl, token.access_token);

                // Deserialize the Json object into a maps list object
                log.Debug("Deserializing the Json response from the API into a MapList object.");
                MapList mapsList = JsonConvert.DeserializeObject<MapList>(jsonResponse);

                // add the maps to the list
                log.Debug("Adding the maps to the maps list.");
                maps.AddRange(mapsList.maps);

                // determine if there is a next page
                log.Debug("Determining if there is a next page.");
                if (mapsList.nextPageToken != null && mapsList.nextPageToken.Length > 0)
                {
                    // a next page token does exist, fetch it and add it to the list
                    log.Debug("Next page exists, fetching next page.");
                    maps.AddRange(getMapsByProjectId(token, projectId, mapsList.nextPageToken));
                }
            }
            catch (System.Exception ex)
            {
                // an exception has occured, log and throw
                log.Error(ex);
                throw new System.Exception(Properties.Resources.GoogleMapsEngineAPI_getMaps_unknownexception);
            }

            // return the maps list
            return maps;
        }

        /*
         * Retrives the 2-n pages of a Google Maps Engine project request
         */
        private List<Map> getMapsByProjectId(Extension.Auth.OAuth2Token token, string projectId, string nextPageToken)
        {
            // create a new blank list of maps
            log.Debug("Creating a blank set of maps.");
            List<Map> maps = new List<Map>();

            // build the request url to the GME API
            log.Debug("Building the Google Maps Engine API request URL.");
            string apiRequestUrl = GME_API_PROTOCOL
                + "://" + GME_API_DOMAIN
                + "/" + GME_API_SERVICE
                + "/" + GME_API_VERSION
                + "/" + "maps"
                + "?" + "key=" + GOOGLE_API_KEY
                + "&" + "projectId=" + projectId
                + "&" + "pageToken=" + nextPageToken;
            log.Debug("Request Url: " + apiRequestUrl);

            // attempt to make the request to the Google Maps Engine API
            try
            {
                // make the Google Maps Engine Request
                log.Debug("Making the Google Maps Engine Request.");
                String jsonResponse = makeGoogleMapsEngineRequest(apiRequestUrl, token.access_token);

                // Deserialize the Json object into a maps list object
                log.Debug("Deserializing the Json response from the API into a MapList object.");
                MapList mapsList = JsonConvert.DeserializeObject<MapList>(jsonResponse);

                // add the maps to the list
                log.Debug("Adding the maps to the maps list.");
                maps.AddRange(mapsList.maps);

                // determine if there is a next page
                log.Debug("Determining if there is a next page.");
                if (mapsList.nextPageToken != null && mapsList.nextPageToken.Length > 0)
                {
                    // a next page token does exist, fetch it and add it to the list
                    log.Debug("Next page exists, fetching next page.");
                    maps.AddRange(getMapsByProjectId(token, projectId, mapsList.nextPageToken));
                }
            }
            catch (System.Exception ex)
            {
                // an exception has occured, log and throw
                log.Error(ex);
                throw new System.Exception(Properties.Resources.GoogleMapsEngineAPI_getMaps_unknownexception);
            }

            // return the maps list
            return maps;
        }

        /*
         * Retrieves a specific Google Maps Engine project by identifier for the
         * authenticated user.
         */
        public Map getMapById(Extension.Auth.OAuth2Token token, string mapId)
        {
            // build the request url to the GME API
            log.Debug("Building the Google Maps Engine API request URL.");
            string apiRequestUrl = GME_API_PROTOCOL
                + "://" + GME_API_DOMAIN
                + "/" + GME_API_SERVICE
                + "/" + GME_API_VERSION
                + "/" + "maps"
                + "/" + mapId
                + "?" + "key=" + GOOGLE_API_KEY;
            log.Debug("Request Url: " + apiRequestUrl);

            // attempt to make the request to the Google Maps Engine API
            try
            {
                // make the Google Maps Engine Request
                log.Debug("Making the Google Maps Engine Request.");
                String jsonResponse = makeGoogleMapsEngineRequest(apiRequestUrl, token.access_token);

                // Deserialize the Json object into a maps list object
                log.Debug("Deserializing the Json response from the API into a MapList object.");
                Map map = JsonConvert.DeserializeObject<Map>(jsonResponse);

                // return the map
                return map;
            }
            catch (System.Exception ex)
            {
                // an exception has occured, log and throw
                log.Error(ex);
                throw new System.Exception(Properties.Resources.GoogleMapsEngineAPI_getMaps_unknownexception);
            }
        }

        /*
         * Retrieves a specific Google Maps Engine asset by identifier
         * for the authenticated user.
         */
        public Asset getAssetById(Extension.Auth.OAuth2Token token, string assetId)
        {
            // build the request url to the GME API
            log.Debug("Building the Google Maps Engine API request URL.");
            string apiRequestUrl = GME_API_PROTOCOL
                + "://" + GME_API_DOMAIN
                + "/" + GME_API_SERVICE
                + "/" + GME_API_VERSION
                + "/" + "assets"
                + "/" + assetId
                + "?" + "key=" + GOOGLE_API_KEY;
            log.Debug("Request Url: " + apiRequestUrl);

            // attempt to make the request to the Google Maps Engine API
            try
            {
                // make the Google Maps Engine Request
                log.Debug("Making the Google Maps Engine Request.");
                String jsonResponse = makeGoogleMapsEngineRequest(apiRequestUrl, token.access_token);

                // Deserialize the Json object into an Asset object
                log.Debug("Deserializing the Json response from the API into an Asset object.");
                Asset asset = JsonConvert.DeserializeObject<Asset>(jsonResponse);

                // return the asset
                return asset;
            }
            catch (System.Exception ex)
            {
                // an exception has occured, log and throw
                log.Error(ex);
                throw new System.Exception(Properties.Resources.GoogleMapsEngineAPI_getMaps_unknownexception);
            }
        }
        
        /*
         * Makes a request to the Google Maps Engine API and returns the
         * response from the API in a string object.
         */
        private String makeGoogleMapsEngineRequest(String RequestUrl, String access_token)
        {
            // create a random number generator to handle exponential backoff
            Random randomGenerator = new Random();

            // start a look, attempting no more than N attempts
            for (int n = 0; n < 5; ++n)
            {
                // attempt to make request, catching error
                try
                {
                    // setup an HTTP Web Request method
                    log.Debug("Creating a new WebRequest object.");
                    WebRequest apiWebRequest = HttpWebRequest.Create(RequestUrl);

                    // set the request method to GET
                    apiWebRequest.Method = "GET";

                    // set the content type
                    apiWebRequest.ContentType = "application/json";

                    // set the OAuth 2.0 Access token
                    log.Debug("Setting the OAuth 2.0 Authorization Header paramter.");
                    apiWebRequest.Headers.Add("Authorization", "OAuth " + access_token);

                    // set the user agent, for trouble shooting
                    log.Debug("Setting the user agent for this request for identification.");
                    ((HttpWebRequest)apiWebRequest).UserAgent = "Google Maps Engine Connector for ArcGIS";

                    // publish a new download event to the extension (if DEBUG)
                    if (log.IsDebugEnabled)
                        ext.publishRaiseDownloadProgressChangeEvent(false, "Requesting " + RequestUrl);

                    // get the HTTP response
                    log.Debug("Retrieving the HTTP response.");
                    using (WebResponse apiWebResponse = apiWebRequest.GetResponse())
                    {
                        // verify the response status was OK
                        log.Debug("Verifying the Server response was OK.");
                        if (((HttpWebResponse)apiWebResponse).StatusCode == HttpStatusCode.OK)
                        {
                            log.Debug("Server response was OK.");

                            // setup a stream reader to read the response from the server
                            log.Debug("Streaming the server response.");
                            System.IO.StreamReader reader = new System.IO.StreamReader(apiWebResponse.GetResponseStream());

                            // read the response into a local variable
                            log.Debug("Reading the server response to end.");
                            string jsonResponse = reader.ReadToEnd();

                            // close the response stream from the server
                            log.Debug("Closing the server response.");
                            apiWebResponse.Close();

                            // if debug, log the server response
                            if (log.IsDebugEnabled && debugDirectory != null)
                            {
                                try
                                {
                                    // cast the RequestUrl into a URI object
                                    Uri requestUri = new Uri(RequestUrl);

                                    // create a stream writer and new text file
                                    System.IO.StreamWriter sw = System.IO.File.CreateText(
                                        debugDirectory.FullName
                                        + "\\"
                                        + (requestUri.PathAndQuery.Replace("/", "_").Replace("?", "_").Replace("&", "_"))
                                        + ".txt");

                                    // write the json response
                                    sw.Write(jsonResponse);

                                    // flush the writer
                                    sw.Flush();

                                    // close the writer
                                    sw.Close();
                                }
                                catch (Exception) { }
                            }

                            // return the response object
                            log.Debug("Returning the response Json");
                            return jsonResponse;
                        }
                        else if (((HttpWebResponse)apiWebResponse).StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            // the service was not available, attempt to recover
                            //throw new System.Net.WebException("ServiceUnavailable");
                            // Apply exponential backoff.
                            ext.publishRaiseDownloadProgressChangeEvent(false, "Applying exponential backoff (" + ((HttpWebResponse)apiWebResponse).StatusDescription + ")");
                            Thread.Sleep((1 << n) * 1000 + randomGenerator.Next(1001));
                        }
                        else
                        {
                            // an error occured, throw an exception
                            //throw new System.Exception(Properties.Resources.GoogleMapsEngineAPI_webrequest_unknownexception);
                            // Apply exponential backoff.
                            ext.publishRaiseDownloadProgressChangeEvent(false, "Applying exponential backoff (" + ((HttpWebResponse)apiWebResponse).StatusDescription + ")");
                            Thread.Sleep((1 << n) * 1000 + randomGenerator.Next(1001));
                        }
                    }
                }
                catch (Exception ex)
                {
                    // an exception has occured, log and throw
                    log.Error(ex);

                    // throw an exception
                    throw new System.Exception(Properties.Resources.GoogleMapsEngineAPI_webrequest_unknownexception);
                }
            }

            // An error occured, return nothing
            return null;
        }

        /*
         * Makes a request to the Google Maps Engine API and returns the
         * response from the API in a string object.
         */
        private String makeGoogleMapsEnginePostRequest(String RequestUrl, String access_token, String payload)
        {
            // create a random number generator to handle exponential backoff
            Random randomGenerator = new Random();

            // start a look, attempting no more than N attempts
            for (int n = 0; n < 5; ++n)
            {
                // attempt to make request, catching error
                try
                {
                    // setup an HTTP Web Request method
                    log.Debug("Creating a new WebRequest object.");
                    WebRequest apiWebRequest = HttpWebRequest.Create(RequestUrl);

                    // set the request method to POST
                    apiWebRequest.Method = "POST";

                    // set the content type
                    apiWebRequest.ContentType = "application/json";

                    // set the OAuth 2.0 Access token
                    log.Debug("Setting the OAuth 2.0 Authorization Header paramter.");
                    apiWebRequest.Headers.Add("Authorization", "OAuth " + access_token);

                    // set the user agent, for trouble shooting
                    log.Debug("Setting the user agent for this request for identification.");
                    ((HttpWebRequest)apiWebRequest).UserAgent = "Google Maps Engine Connector for ArcGIS";

                    // set the request payload
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] postBytes = encoding.GetBytes(payload);
                    apiWebRequest.ContentLength = postBytes.Length;
                    System.IO.Stream requestStream = apiWebRequest.GetRequestStream();
                    requestStream.Write(postBytes, 0, postBytes.Length);

                    // publish a new download event to the extension (if DEBUG)
                    if (log.IsDebugEnabled)
                        ext.publishRaiseDownloadProgressChangeEvent(false, "Requesting " + RequestUrl);

                    // get the HTTP response
                    log.Debug("Retrieving the HTTP response.");
                    using (WebResponse apiWebResponse = apiWebRequest.GetResponse())
                    {
                        // verify the response status was OK
                        log.Debug("Verifying the Server response was OK.");
                        if (((HttpWebResponse)apiWebResponse).StatusCode == HttpStatusCode.OK)
                        {
                            log.Debug("Server response was OK.");

                            // setup a stream reader to read the response from the server
                            log.Debug("Streaming the server response.");
                            System.IO.StreamReader reader = new System.IO.StreamReader(apiWebResponse.GetResponseStream());

                            // read the response into a local variable
                            log.Debug("Reading the server response to end.");
                            string jsonResponse = reader.ReadToEnd();

                            // close the response stream from the server
                            log.Debug("Closing the server response.");
                            apiWebResponse.Close();

                            // if debug, log the server response
                            if (log.IsDebugEnabled && debugDirectory != null)
                            {
                                try
                                {
                                    // cast the RequestUrl into a URI object
                                    Uri requestUri = new Uri(RequestUrl);

                                    // create a stream writer and new text file
                                    System.IO.StreamWriter sw = System.IO.File.CreateText(
                                        debugDirectory.FullName
                                        + "\\"
                                        + (requestUri.PathAndQuery.Replace("/", "_").Replace("?", "_").Replace("&", "_"))
                                        + ".txt");

                                    // write the json response
                                    sw.Write(jsonResponse);

                                    // flush the writer
                                    sw.Flush();

                                    // close the writer
                                    sw.Close();
                                }
                                catch (Exception) { }
                            }

                            // return the response object
                            log.Debug("Returning the response Json");
                            return jsonResponse;
                        }
                        else if (((HttpWebResponse)apiWebResponse).StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            // the service was not available, attempt to recover
                            //throw new System.Net.WebException("ServiceUnavailable");
                            // Apply exponential backoff.
                            ext.publishRaiseDownloadProgressChangeEvent(false, "Applying exponential backoff (" + ((HttpWebResponse)apiWebResponse).StatusDescription + ")");
                            Thread.Sleep((1 << n) * 1000 + randomGenerator.Next(1001));
                        }
                        else
                        {
                            // an error occured, throw an exception
                            //throw new System.Exception(Properties.Resources.GoogleMapsEngineAPI_webrequest_unknownexception);
                            // Apply exponential backoff.
                            ext.publishRaiseDownloadProgressChangeEvent(false, "Applying exponential backoff (" + ((HttpWebResponse)apiWebResponse).StatusDescription + ")");
                            Thread.Sleep((1 << n) * 1000 + randomGenerator.Next(1001));
                        }
                    }
                }
                catch (Exception ex)
                {
                    // an exception has occured, log and throw
                    log.Error(ex);

                    // throw an exception
                    throw new System.Exception(Properties.Resources.GoogleMapsEngineAPI_webrequest_unknownexception);
                }
            }

            // An error occured, return nothing
            return null;
        }
        
        /*
         * Makes a request to the Google Maps Engine API to generate a new vector table
         * then, returns a reference identifier
         */
        public UploadingAsset createVectorTableAssetForUploading(Extension.Auth.OAuth2Token token, AssetType assetType, String projectId, String name, String sharedAccessList, List<String> fileNames, String description, List<String> tags, String encoding)
        {
            // attempt to call the GME API to make a new vector table reference
            try
            {
                if (assetType == AssetType.table)
                {
                    // build the request url to the GME API
                    log.Debug("Building the Google Maps Engine API request URL.");
                    String RequestUrl = GME_API_PROTOCOL
                        + "://" + GME_API_DOMAIN
                        + "/" + GME_API_SERVICE
                        + "/" + Properties.Settings.Default.gme_api_version_createTT
                        + "/" + "tables"
                        + "/" + "upload"
                        + "?" + "projectId=" + projectId
                        + "&" + "key=" + GOOGLE_API_KEY;
                    log.Debug("Request Url: " + RequestUrl);

                    // serialize the requestAsset into json
                    String payload = JsonConvert.SerializeObject(new UploadableVectorTableAsset(name, description, fileNames, sharedAccessList, encoding, tags));

                    // make the post request, get json response
                    String jsonResponse = makeGoogleMapsEnginePostRequest(RequestUrl, token.access_token, payload);

                    // Deserialize the Json object into a uploading asset object
                    log.Debug("Deserializing the Json response from the API into a UploadingAsset object.");
                    UploadingAsset uploadingAsset = JsonConvert.DeserializeObject<UploadingAsset>(jsonResponse);

                    // return the uploading asset
                    return uploadingAsset;
                }
            }
            catch (System.Exception ex)
            {
                // TODO: handle gracefully
                System.Windows.Forms.MessageBox.Show("Error: " + ex.Message);
            }

            return null;
        }

        /*
         * Makes a request to Google Maps Engine API to generate a new raster asset
         * then, returns a reference identifier
         */
        public UploadingAsset createRasterAssetForUploading(Extension.Auth.OAuth2Token token, String projectId, String name, String sharedAccessList, String attribution, List<String> fileNames, String description, List<String> tags)
        {
            // attempt to call the GME API to make a raster reference
            try
            {
                // build the request url to the GME API
                log.Debug("Building the Google Maps Engine API request URL.");
                String RequestUrl = GME_API_PROTOCOL
                    + "://" + GME_API_DOMAIN
                    + "/" + GME_API_SERVICE
                    + "/" + Properties.Settings.Default.gme_api_version_createTT
                    + "/" + "rasters"
                    + "/" + "upload"
                    + "?" + "projectId=" + projectId
                    + "&" + "key=" + GOOGLE_API_KEY;
                log.Debug("Request Url: " + RequestUrl);

                // serialize the requestAsset into json
                String payload = JsonConvert.SerializeObject(new UploadableRasterAsset(name, description, fileNames, sharedAccessList, attribution, tags));

                // make the post request, get json response
                String jsonResponse = makeGoogleMapsEnginePostRequest(RequestUrl, token.access_token, payload);

                // Deserialize the Json object into a uploading asset object
                log.Debug("Deserializing the Json response from the API into a UploadingAsset object.");
                UploadingAsset uploadingAsset = JsonConvert.DeserializeObject<UploadingAsset>(jsonResponse);

                // return the uploading asset
                return uploadingAsset;
            }
            catch (System.Exception ex)
            {
                // TODO: handle gracefully
                System.Windows.Forms.MessageBox.Show("Error: " + ex.Message);
            }

            return null;
        }

        public void uploadFilesToAsset(Extension.Auth.OAuth2Token token, String assetId, System.IO.FileInfo[] files)
        {
            try
            {
                // raise a processing notification
                ext.publishRaiseDownloadProgressChangeEvent(false, "Starting to upload " + files.Count() + " files.", files.Count(), 0);

                // go through each file to be uploaded
                // TODO: Multithread
                int fileCounter = 0;
                foreach (System.IO.FileInfo file in files)
                {
                    // raise a processing notification
                    ext.publishRaiseDownloadProgressChangeEvent(false, "Uploading " + file.Name + ".", files.Count(), fileCounter);

                    if (!file.Name.EndsWith(".lock"))
                    {
                        // upload the selected file
                        bool wasPostSuccessful = streamingUploadFileToAsset(token, assetId, file);
                    }

                    // incrament
                    fileCounter++;
                }

                // raise a processing notification
                ext.publishRaiseDownloadProgressChangeEvent(false, "All files have been uploaded.", files.Count(), fileCounter);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error: " + e.Message);
            }
        }

        private bool streamingUploadFileToAsset(Extension.Auth.OAuth2Token token, String assetId, System.IO.FileInfo file)
        {
            // create a random number generator to handle exponential backoff
            Random randomGenerator = new Random();

            // start a look, attempting no more than N attempts
            for (int n = 0; n < 5; ++n)
            {
                try
                {
                    // create a request Url
                    String RequestUrl = "https://www.googleapis.com/upload/mapsengine/create_tt/tables/" + assetId + "/files?uploadType=multipart&filename=" + file.Name;

                    string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                    byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

                    // setup an HTTP Web Request method
                    log.Debug("Creating a new WebRequest object.");
                    HttpWebRequest apiWebRequest = (HttpWebRequest)WebRequest.Create(RequestUrl);

                    // set the request method to POST
                    apiWebRequest.Method = "POST";
                    apiWebRequest.KeepAlive = true;

                    // set the content type and include the boundary
                    apiWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;

                    // set the OAuth 2.0 Access token
                    log.Debug("Setting the OAuth 2.0 Authorization Header paramter.");
                    apiWebRequest.Headers.Add("Authorization", "OAuth " + token.access_token);

                    // set the user agent, for trouble shooting
                    log.Debug("Setting the user agent for this request for identification.");
                    ((HttpWebRequest)apiWebRequest).UserAgent = "Google Maps Engine Connector for ArcGIS";

                    // create a request stream
                    System.IO.Stream requestStream = apiWebRequest.GetRequestStream();

                    // write the bytes for a boundary
                    requestStream.Write(boundarybytes, 0, boundarybytes.Length);

                    // construct the HTTP multi-part header for the metadata
                    string formdataTemplate = "Content-Type: {0}; charset={1}\r\n\r\n";
                    string formitem = string.Format(formdataTemplate, "application/json", "UTF-8");
                    formitem += "{";
                    formitem += "\"name\": \"" + file.Name.Split(".".ToCharArray())[0] + "\"";
                    formitem += "}";
                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);

                    requestStream.Write(formitembytes, 0, formitembytes.Length);

                    // write the bytes for a boundary
                    requestStream.Write(boundarybytes, 0, boundarybytes.Length);

                    // construct the HTTP multi-part header for the file
                    string headerTemplate = "Content-Type: {0}\r\n\r\n";
                    string header = string.Format(headerTemplate, "plain/text");
                    byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                    requestStream.Write(headerbytes, 0, headerbytes.Length);

                    // read the input file and set the request payload
                    System.IO.FileStream fileStream = new System.IO.FileStream(file.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    byte[] buffer = new byte[4096];
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        requestStream.Write(buffer, 0, bytesRead);
                    }
                    fileStream.Close();

                    // write the end and close the request stream
                    byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    requestStream.Write(trailer, 0, trailer.Length);
                    requestStream.Close();
                    requestStream = null;

                    // publish a new download event to the extension (if DEBUG)
                    if (log.IsDebugEnabled)
                        ext.publishRaiseDownloadProgressChangeEvent(false, "Requesting " + RequestUrl);

                    // get the HTTP response
                    log.Debug("Retrieving the HTTP response.");
                    using (WebResponse apiWebResponse = apiWebRequest.GetResponse())
                    {
                        // verify the response status was OK
                        log.Debug("Verifying the Server response was NoContent.");
                        if (((HttpWebResponse)apiWebResponse).StatusCode == HttpStatusCode.NoContent)
                        {
                            log.Debug("Server response was OK.");

                            return true;
                        }
                        else if (((HttpWebResponse)apiWebResponse).StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            // the service was not available, attempt to recover
                            //throw new System.Net.WebException("ServiceUnavailable");
                            // Apply exponential backoff.
                            ext.publishRaiseDownloadProgressChangeEvent(false, "Applying exponential backoff (" + ((HttpWebResponse)apiWebResponse).StatusDescription + ")");
                            Thread.Sleep((1 << n) * 1000 + randomGenerator.Next(1001));
                        }
                        else
                        {
                            // an error occured, throw an exception
                            //throw new System.Exception(Properties.Resources.GoogleMapsEngineAPI_webrequest_unknownexception);
                            // Apply exponential backoff.
                            ext.publishRaiseDownloadProgressChangeEvent(false, "Applying exponential backoff (" + ((HttpWebResponse)apiWebResponse).StatusDescription + ")");
                            Thread.Sleep((1 << n) * 1000 + randomGenerator.Next(1001));
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show("Error: " + e.Message);
                }
            }

            return false;
        }

        /*
         * An Asset Type enumeration
         */
        public enum AssetType
        {
            none,
            map,
            layer,
            image,
            rasterCollection,
            table
        }
    }
}
