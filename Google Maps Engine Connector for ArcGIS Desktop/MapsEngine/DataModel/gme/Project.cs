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
     * A Google Maps Engine project entity.
     */
    public class Project
    {
        // A user provided name for this project.
        private string _id;

        //Type of the resource.
        private string _name;

        // constructor
        public Project() { }

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

        #endregion
    }
}
