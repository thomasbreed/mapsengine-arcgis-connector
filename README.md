Google Maps Engine Connector for ArcGIS
=====================================

Introduction
------------
Google Maps Engine Connector for ArcGIS Desktop(tm) allows your to authenticate with your
Google Account, list maps you have access to and view and interact with them
within ArcGIS Desktop.

[Learn more about Google Maps Engine](
http://www.google.com/enterprise/mapsearth/products/mapsengine.html)

The connector is provided by Google for public use and modification,
but is not covered under the Google Maps Engine
[service level agreement](
http://www.google.com/enterprise/earthmaps/legal/us/gme_sla.html) or
[technical support services](
http://www.google.com/enterprise/earthmaps/legal/us/gme_tssg.html).

Installation
------------
To install the Google Maps Engine Connector for ArcGIS Desktop(tm), download the .esriAddin file (link TBD) to a local Windows machine running ArcGIS Desktop 10.0, 10.1, or 10.2.  Click on the .esriAddin file to install the addon.  Once installed, open ArcMap and enable the Google Maps Engine Connector for ArcGIS Desktop extension from the menu.


Overview of the Tools
---------------------
| Tool | Desciption |
| ---- | ---------- |
| ![Sign In](/Images/private-16.png) | Sign in or out of your Maps Engine Account. |
| ![Search](/Images/search-16.png) | Search for a map from a Google Maps Engine account. Once you select a map and click OK, a bounding box layer is added to the QGIS canvas.* |
| ![Search in Gallery](/Images/gallery-16.png) | Search for a Google Maps Engine map in the Google Earth Gallery.* |
| ![WMS](/Images/overlay-16.png) | View a layer from the selected map as a WMS overlay in QGIS. ** |
| ![View in GME](/Images/maps_engine-16.png) | View the selected map or layer in Google Maps Engine in a new browser tab. ** |
| ![View in Google Maps](/Images/maps-16.png) | View the selected map in a Google Maps viewer in a new browser tab. The url includes a short-lived access token allowing access to private maps. ** |
| ![Copy to clipboard](/Images/link-16.png) | Copy the link to the WMS service url to clipboard. The url includes a short-lived access token allowing access to private maps. ** |
| ![Upload](/Images/upload_item-16.png) | Upload the selected vector or raster layer to Google Maps Engine. *** |
| More | Access *Advanced Settings* and *About dialog*. |

\* *Enabled after a successful login*.

** *Enabled after a successful map search.*

*** *Enabled once a vector layer is selected.*

Updates
-------
The Google Maps Engine Connector for ArcGIS Desktop will inform you via a dialog when opening ArcMap.  You can download the new version of the connector by clicking on the
notification.


License
-------
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
