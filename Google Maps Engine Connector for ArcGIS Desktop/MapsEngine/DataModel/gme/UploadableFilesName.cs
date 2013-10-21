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
    public class UploadableFileName
    {
        // The filename of the resource.
        private string _filename;

        // constructor
        public UploadableFileName(string filename)
        {
            this._filename = filename;
        }

        #region Getters/Setters

        public string filename
        {
            get { return this._filename; }

            set { if (this._filename != value) this._filename = value; }
        }

        #endregion
    }
}
