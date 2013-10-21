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
     * A list of Google Maps Engine map entities.
     */
    public class MapList
    {
        // Resources returned.
        private List<Map> _maps = new List<Map>();

        // Next page token.
        private String _nextPageToken;

        // constructor
        public MapList() { }

        #region Getters/Setters

        public List<Map> maps
        {
            get { return this._maps; }

            set { if (this._maps != value) this._maps = value; }
        }

        public string nextPageToken
        {
            get { return this._nextPageToken; }

            set { if (this._nextPageToken != value) this._nextPageToken = value; }
        }

        #endregion
    }
}
