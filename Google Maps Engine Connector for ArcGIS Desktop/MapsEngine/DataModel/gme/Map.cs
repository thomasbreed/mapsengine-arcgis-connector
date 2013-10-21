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
     * A Google Maps Engine map entity.
     */
    public class Map
    {
        // An ID used to refer to this map. IDs are unique within a single project.
        private string _id;

        //The name of the map, supplied by a user.
        private string _name;

        // The description of the map, supplied by a user.
        private string _description;

        // The bounds covering all data included in this Map (west, south, east, north).
        private List<double> _bbox = new List<double>();

        // The folders contained at the root level of this Map.
        private List<MapFolder> _folders = new List<MapFolder>();

        // The layers contained at the root level of this Map.
        private List<MapLayer> _layers = new List<MapLayer>();

        public Map() { }

        #region Getters/Setters

        public String id
        {
            get { return this._id; }

            set { if (this._id != value) this._id = value; }
        }

        public String name
        {
            get { return this._name; }

            set { if (this._name != value) this._name = value; }
        }

        public String description
        {
            get { return this._description; }

            set { if (this._description != value) this._description = value; }
        }

        public List<double> bbox
        {
            get { return this._bbox; }

            set { if (this._bbox != value) this._bbox = value; }
        }

        public List<MapFolder> folders
        {
            get { return this._folders; }

            set { if (this._folders != value) this._folders = value; }
        }

        public List<MapLayer> layers
        {
            get { return this._layers; }

            set { if (this._layers != value) this._layers = value; }
        }
        #endregion
    }
}
