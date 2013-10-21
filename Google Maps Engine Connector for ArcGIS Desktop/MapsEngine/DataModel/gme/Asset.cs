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
     * A Google Maps Engine Asset entity
     */
    public class Asset
    {
        // An ID used to refer to this resource. IDs are unique within a single project.
        private string _id;

        // The name of the resource, supplied by a user.
        private string _name;

        // The description of the resource, supplied by a user.
        private string _description;

        // The type of the resource.
        private string _type;

        // A URL from which the resource can be downloaded.
        private string _url;

        // The geographic bounds of the resource. If the resource does not have a spatial extent, this field will not be returned.
        private List<double> _bbox = new List<double>();

        // constructor
        public Asset() { }

        #region Getters/Setters

        public string id
        {
            get { return this._id; }

            set { if (this._id != value) this._id = value; }
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

        public string type
        {
            get { return this._type; }

            set { if (this._type != value) this._type = value; }
        }

        public string url
        {
            get { return this._url; }

            set { if (this._url != value) this._url = value; }
        }

        public List<double> bbox
        {
            get { return this._bbox; }

            set { if (this._bbox != value) this._bbox = value; }
        }

        #endregion
    }
}