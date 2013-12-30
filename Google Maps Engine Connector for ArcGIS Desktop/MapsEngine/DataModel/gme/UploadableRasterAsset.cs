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

namespace com.google.mapsengine.connectors.arcgis.MapsEngine.DataModel.gme
{
    /*
     * A Google Maps Engine Uploadable Asset entity
     */
    public class UploadableRasterAsset
    {
        // The Google Maps Engine project identifier
        private string _projectId;

        // The name of the resource, supplied by a user.
        private string _name;

        // The description of the resource, supplied by a user.
        private string _description;

        // files
        private List<UploadableFileName> _files = new List<UploadableFileName>();

        // The acquisition time object.
        // All values must be supplied in RFC 3339 date-time format (YYYY-MM-DDTHH:MM:SSZ)
        private String _acquisitionTime;

        // Access Control List for Map Editors
        private string _draftAccessList;

        private string _attribution;

        private List<String> _tags = new List<String>();

        //  The image masking mode. Default is autoMask.
        // Valid values are: autoMask, alphaChannelMask, noMask, imageMask
        private String _maskType;

        // constructor
        public UploadableRasterAsset(String projectId, String name, String description,
            List<String> files, string draftAccessList,
            string attribution, List<String> tags, String acquisitionTime, String maskType)
        {
            this._projectId = projectId;
            this._name = name;
            this._description = description;
            for(int k=0; k<files.Count; k++)
                this._files.Add(new UploadableFileName(files[k]));
            this._draftAccessList = draftAccessList;
            this._attribution = attribution;
            this._tags = tags;
            this._acquisitionTime = acquisitionTime;
            this._maskType = maskType;
        }

        #region Getters/Setters

        public string projectId
        {
            get { return this._projectId; }

            set { if (this._projectId != value) this._projectId = value; }
        }

        public string name
        {
            get { return this._name; }

            set { if (this._name != value) this._name = value; }
        }

        public string description
        {
            get { return this._description; }

            set { if (this._description != value) this._description = value; }
        }
        public List<UploadableFileName> files
        {
            get { return this._files; }

            set { if (this._files != value) this._files = value; }
        }
        public string draftAccessList
        {
            get { return this._draftAccessList; }

            set { if (this._draftAccessList != value) this._draftAccessList = value; }
        }
        public string attribution
        {
            get { return this._attribution; }

            set { if (this._attribution != value) this._attribution = value; }
        }
        public string acquisitionTime
        {
            get { return this._acquisitionTime; }

            set { if (this._acquisitionTime != value) this._acquisitionTime = value; }
        }
        public List<String> tags
        {
            get { return this._tags; }

            set { if (this._tags != value) this._tags = value; }
        }

        public string maskType
        {
            get { return this._maskType; }

            set { if (this._maskType != value) this._maskType = value; }
        }

        #endregion
    }
}
