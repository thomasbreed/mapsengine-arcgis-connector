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
    class ProjectFilterChangeEventArgs : EventArgs
    {
        public ProjectFilterChangeEventArgs(bool isFilterApplied, String projectId)
        {
            _isFilterApplied = isFilterApplied;
            _projectId = projectId;
        }
        private bool _isFilterApplied;

        private String _projectId;

        public bool isFilterApplied
        {
            get { return _isFilterApplied; }
            set { _isFilterApplied = value; }
        }

        public String projectId
        {
            get { return _projectId; }
            set { _projectId = value; }
        }
    }
}
