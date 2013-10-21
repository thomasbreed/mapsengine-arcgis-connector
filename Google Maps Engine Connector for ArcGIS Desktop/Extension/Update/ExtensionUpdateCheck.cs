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
using log4net;
using log4net.Config;
using System.Net;
using Newtonsoft.Json;
using System.Reflection;

namespace com.google.mapsengine.connectors.arcgis.Extension.Update
{
    public class ExtensionUpdateCheck
    {
        // setup and configure log4net
        protected static readonly ILog log = LogManager.GetLogger(typeof(ExtensionUpdateCheck));

        protected UpdateStatusOverview updatestatus;
        protected Version serverVersion;

        public ExtensionUpdateCheck()
        {
            // initialize and configure log4net, reading from Xml .config file
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));

            log.Info("ExtensionUpdateCheck initializing.");

            updatestatus = null;
        }

        public bool isUpdateAvailable()
        {
            log.Debug("isUpdateAvailable check starting.");

            try
            {
                // retrieving the Update Check URI from the settings
                log.Debug("Retreiving the Update Check URI from the application settings.");
                string updateapi = Properties.Settings.Default.extension_update_check_uri;

                // setup a query string
                log.Debug("Adding the current version to the update URI");
                log.Debug("av=" + Assembly.GetExecutingAssembly().GetName().Version);
                string query = "av=" + Assembly.GetExecutingAssembly().GetName().Version;

                // setup an HTTP Web Request method
                log.Debug("Setting up the HTTP request with the URI + query");
                WebRequest statusUpdateServerRequest = HttpWebRequest.Create(updateapi + "?" + query);

                // set the request method to GET
                log.Debug("Setting the request type to GET");
                statusUpdateServerRequest.Method = "GET";

                // set the content type
                log.Debug("Setting the appropriate content type for the request.");
                statusUpdateServerRequest.ContentType = "application/x-www-form-urlencoded";

                // set the user agent, for trouble shooting
                log.Debug("Setting the user agent for this request for identification.");
                ((HttpWebRequest)statusUpdateServerRequest).UserAgent = "Google Maps Engine Connector for ArcGIS";

                // get the HTTP response
                log.Debug("Starting to retrieve the HTTP response.");
                using (WebResponse statusUpdateServerResponse = statusUpdateServerRequest.GetResponse())
                {
                    // verify the response status was OK
                    log.Debug("Checking the response for a status of OK");
                    log.Debug("StatusCode: " + ((HttpWebResponse)statusUpdateServerResponse).StatusCode);
                    if (((HttpWebResponse)statusUpdateServerResponse).StatusCode == HttpStatusCode.OK)
                    {
                        // setup a stream reader to read the response from the server
                        log.Debug("Setting up a stream reader to retrieve the HTTP response from the update server.");
                        System.IO.StreamReader reader = new System.IO.StreamReader(statusUpdateServerResponse.GetResponseStream());

                        // read the response into a local variable
                        log.Debug("Reading to the end of the stream.");
                        string response = reader.ReadToEnd();

                        // close the response stream from the server
                        log.Debug("Closing the stream.");
                        statusUpdateServerResponse.Close();

                        // setup a token object (decode from JSON to object)
                        log.Debug("Deserializing the HTTP respose JSON object.");
                        log.Debug("Response: " + response);
                        updatestatus = JsonConvert.DeserializeObject<UpdateStatusOverview>(response);
                        log.Debug("Deserialized: " + updatestatus);

                        // check to make sure the status of the object is not null and it is currently active
                        log.Debug("Verifying the deserialized object is not null and it has an active status.");
                        if (updatestatus != null && updatestatus.activeVersions != null && updatestatus.activeVersions.Length > 0)
                        {
                            // get the version for ArcGIS 10
                            log.Debug("Retreive the update value for ArcGIS Desktop 10.");
                            // TODO: Make this dymaic
                            UpdateStatusVersion ags10v = updatestatus.activeVersions.Single(q => q.platform.vendor.Equals("Esri") && q.platform.name.Equals("ArcGIS Desktop") && q.platform.version.Equals("10.0"));

                            // create a Version to define the server's state
                            log.Debug("Creating a version object to compare the user's version to the server's version");
                            serverVersion = new Version(ags10v.AddinInstallationVersion);

                            // compare
                            log.Debug("Comparing versions.");
                            log.Debug("User version: " + Assembly.GetExecutingAssembly().GetName().Version);
                            log.Debug("Server version: " + serverVersion);
                            if (Assembly.GetExecutingAssembly().GetName().Version.CompareTo(serverVersion) < 0)
                            {
                                // update available
                                log.Debug("Server version is more up-to-date than the user's version.  Returning true to update.");
                                return true;
                            }
                            else
                            {
                                // no update available
                                log.Debug("Versions match or the user's version is more advanced.  Returning false to update.");
                                return false;
                            }
                        }
                        else
                        {
                            // update was unsuccessful or inactive
                            log.Warn("Update was not successful or the response JSON object was not active.");
                            // TODO: Do something here...
                            return false;
                        }
                    }
                    else
                    {
                        log.Warn("HTTP status was not OK.");
                        throw new System.Exception("...");
                    }
                }
            }
            catch (System.Exception ex)
            {
                log.Error("An error occured during the check for an update.", ex);
                throw ex;
            }
        }

        public string getUpdateURI()
        {
            if (updatestatus != null && updatestatus.activeVersions != null && updatestatus.activeVersions.Length > 0)
            {
                // get the version for ArcGIS 10
                UpdateStatusVersion ags10v = updatestatus.activeVersions.Single(q => q.platform.vendor.Equals("Esri") && q.platform.name.Equals("ArcGIS Desktop") && (q.platform.version.Equals("10.0") || q.platform.version.Equals("10.1")));

                return ags10v.AddinInstallationURL;
            }
            else
                return "";
        }

        public Version getLocalVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        public Version getServerVersion()
        {
            if (this.serverVersion != null)
                return this.serverVersion;
            else
                throw new System.Exception("An update check is required before the server version is available.");
        }
    }
}
