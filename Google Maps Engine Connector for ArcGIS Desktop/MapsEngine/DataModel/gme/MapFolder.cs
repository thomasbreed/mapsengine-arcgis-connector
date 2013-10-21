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
     * A Google Maps Engine map folder entity.
     */
    public class MapFolder
    {
        // The name of the folder, supplied by a user.
        private string _name;

        // The folders contained at the root level of this folder.
        private List<MapFolder> _folders = new List<MapFolder>();

        // The layers contained at the root level of this folder.
        private List<MapLayer> _layers = new List<MapLayer>();

        public MapFolder() { }

        #region Getters/Setters

        public String name
        {
            get { return this._name; }

            set { if (this._name != value) this._name = value; }
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
