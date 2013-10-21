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

namespace com.google.mapsengine.connectors.arcgis.Extension
{
    public class DownloadProgressChangeEventArgs : EventArgs
    {
        private bool _isComplete;
        private String _message;

        private int _index = -1;
        private int _total = -1;

        public DownloadProgressChangeEventArgs(bool isComplete, string message)
        {
            this._isComplete = isComplete;
            this._message = message;
            this._total = -1;
            this._index = -1;
        }

        public DownloadProgressChangeEventArgs(bool isComplete, string message, int total, int index)
        {
            this._isComplete = isComplete;
            this._message = message;
            this._total = total;
            this._index = index;
        }

        public bool isComplete
        {
            get { return this._isComplete; }
            set { _isComplete = value; }
        }

        public String message
        {
            get { return this._message; }
            set { _message = value; }
        }

        public int index
        {
            get { return this._index; }
            set { _index = value; }
        }

        public int total
        {
            get { return this._total; }
            set { _total = value; }
        }
    }
}
