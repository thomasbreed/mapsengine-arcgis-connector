/*
Copyright 2014 Google Inc

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
     * A new layer to be created in GME.
     */
    public class NewMapLayer
    {
        /*
         * The type of datasource this layer styles. Must be either
         * "table" or "image."
         */
        public String datasourceType
        {
            get;
            set;
        }

        /*
         * The name for the ACL able to access the draft layer.
         */
        public String draftAccessList
        {
            get;
            set;
        }

        /*
         * Name of the layer.
         */
        public String name
        {
            get;
            set;
        }

        /*
         * Name of the project id.
         */
        public String projectId
        {
            get;
            set;
        }

        /*
         * A collection of datasources that the layer styles. In the case
         * of vector tables there should be only one.
         */
        public List<DataSource> datasources
        {
            get;
            set;
        }

        /*
         * The style to apply to the features in the datasource.
         */
        public Style style
        {
            get;
            set;
        }
    }

    /*
     * Represents a vector table or image in GME.
     */
    public class DataSource
    {
        /*
         * The AssetId of the table or image.
         */
        public String id
        {
            get;
            set;
        }
    }
    
    /*
     * Represents a style
     */
    public class Style
    {
        /*
         * The type of display rule. Must be "displayRule."
         */
        public String type
        {
            get;
            set;
        }

        /*
         * Collection of display rules used to style features.
         */
        public List<DisplayRule> displayRules
        {
            get;
            set;
        }
    }

    /*
     * The set of rules to apply when associated filters and 
     * zoom levels are matched.
     */
    public class DisplayRule
    {
        /*
         * A range of LODs that the style appears at.
         */
        public ZoomLevels zoomLevels
        {
            get;
            set;
        }

        /*
         * Name of the display rule.
         */
        public String name
        {
            get;
            set;
        }

        /*
         * Style to apply if vector is line. Only one of polygonOptions, pointOptions, or
         * lineOptions should be present.
         */
        public LineStyle lineOptions
        {
            get;
            set;
        }

        /*
         * Style to apply if vector is polygon. Only one of polygonOptions, pointOptions, or
         * lineOptions should be present.
         */
        public PolygonStyle polygonOptions
        {
            get;
            set;
        }

        /*
         * Style to apply if vector is point. Only one of polygonOptions, pointOptions, or
         * lineOptions should be present.
         */
        public PointStyle pointOptions
        {
            get;
            set;
        }

        /*
         * Collection of filters that a feature must match in order to apply a style.
         */
        public List<Filter> filters
        {
            get;
            set;
        }
    }

    /*
     * A range of LODs that the style appears at.
     */
    public class ZoomLevels
    {
        public int max
        {
            get;
            set;
        }
        public int min
        {
            get;
            set;
        }
    }

    /*
     * Represents a filter to identify a set of features.
     */
    public class Filter
    {
        /*
         * The table column on which to apply the filter.
         */
        public String column
        {
            get;
            set;
        }

        /*
         * Holds the operator (e.g. "==", "<=" etc.)
         * It cannot be named operator and should be renamed
         * to "operator" once this object is serialized.
         */
        public String OPERATOR_HOLDING_STRING
        {
            get;
            set;
        }

        /*
         * The value to compare using the operator.
         */
        public Object value
        {
            get;
            set;
        }
    }

    /*
     * Describes a point style.
     */
    public class PointStyle
    {
        public IconStyle icon
        {
            get;
            set;
        }
    }

    /*
     * Information about an icon to use.
     */
    public class IconStyle
    {
        /*
         * Name of an stock icon. Either this or id but not both
         * should be provided.
         */
        public string name
        {
            get;
            set;
        }

        /*
         * Id of a custom icon. Either this or id but not both
         * should be provided.
         */
        public string id
        {
            get;
            set;
        }
    }

    /*
     * Provides information about a color.
     */
    public class Color
    {
        /*
         * HTML format color string.
         */
        public String color
        {
            get;
            set;
        }

        /*
         * Opacity of the color ranging from 0.0 to 1.0.
         */
        public double opacity
        {
            get;
            set;
        }
    }

    /*
     * A polygon style.
     */
    public class PolygonStyle
    {
        /*
         * Describes the filling of a polygon.
         */
        public Color fill
        {
            get;
            set;
        }

        /*
         * The outline of the polygon.
         */
        public Stroke stroke
        {
            get;
            set;
        }

    }

    /*
     * Captures the style of a linear element, whether
     * an outline or a line proper.
     */
    public class Stroke
    {
        public String color
        {
            get;
            set;
        }
        public double opacity
        {
            get;
            set;
        }

        public double width
        {
            get;
            set;
        }
    }

    /*
     * Captures the style of a line.
     */
    public class LineStyle
    {
        public List<double> dash
        {
            get;
            set;
        }
        public Stroke stroke
        {
            get;
            set;
        }
    }
}
