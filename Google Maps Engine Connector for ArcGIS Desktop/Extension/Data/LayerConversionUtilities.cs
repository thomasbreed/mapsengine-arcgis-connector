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

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;

using com.google.mapsengine.connectors.arcgis.MapsEngine.DataModel.gme;
using System.Drawing;

namespace com.google.mapsengine.connectors.arcgis.Extension.Data
{
    /*
     * Utilities used to create NewMapLayers for creating layers in GME.
     */
    internal static class LayerConversionUtilities
    {

        /*
         * Creates a newmaplayer object used for layer creation in GME for a raster layer.
         */
        internal static NewMapLayer convert(String name, String projectId, String imageId, String acl)
        {
            NewMapLayer returnValue = new NewMapLayer();

            returnValue.name = name;
            returnValue.draftAccessList = acl;
            returnValue.datasourceType = "image";
            returnValue.projectId = projectId;
            returnValue.datasources = new List<DataSource>() { new DataSource() { id = imageId } };

            return returnValue;
        }

        /*
         * Converts a vector featurelayer into a newmaplayer used to create new layers in GME.
         */
        internal static NewMapLayer convert(IGeoFeatureLayer featureLayer, String name, String projectId, String tableId, String acl)
        {
            NewMapLayer returnValue = new NewMapLayer();

            returnValue.name = name;
            returnValue.draftAccessList = acl;
            returnValue.datasourceType = "table";
            returnValue.projectId = projectId;
            returnValue.datasources = new List<DataSource>() { new DataSource(){id=tableId} };
            returnValue.style = convert(featureLayer.Renderer);
            if (null == returnValue.style)
            {
                returnValue.style = new Style();
                returnValue.style.type = "displayRule";
                switch (((IFeatureLayer)featureLayer).FeatureClass.ShapeType)
                {
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryMultipoint:
                        returnValue.style.displayRules = new List<DisplayRule>() 
                        { 
                             new DisplayRule() {
                                 name="displayRule",
                                 zoomLevels=new ZoomLevels() {min=0, max=24},
                                 pointOptions=new PointStyle() {
                                     icon=new IconStyle() {
                                         name="gx_measle_grey"
                                     }
                                 }             
                             }                        
                        };
                        break;
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryLine:
                        returnValue.style.displayRules = new List<DisplayRule>() 
                        { 
                             new DisplayRule() {
                                 name="displayRule",
                                 zoomLevels=new ZoomLevels() {min=0, max=24},
                                 lineOptions=new LineStyle() {
                                    stroke = new Stroke(){color="#000099",width=1.0,opacity=DEFAULT_OPACITY}
                                 }             
                             }                        
                        };
                        break;
                    default: // polygon
                        returnValue.style.displayRules = new List<DisplayRule>() 
                        { 
                             new DisplayRule() {
                                 name="displayRule",
                                 zoomLevels=new ZoomLevels() {min=0, max=24},
                                 polygonOptions=new PolygonStyle() {
                                    fill=new MapsEngine.DataModel.gme.Color(){color="#66CCFF",opacity=DEFAULT_OPACITY},
                                    stroke = new Stroke(){color="#000099",width=1.0,opacity=DEFAULT_OPACITY}
                                 }             
                             }                        
                        };
                        break;
                }
            }
            return returnValue;
        }

        /*
         * Converts a renderer from Arc into a Style.
         */
        private static Style convert(IFeatureRenderer renderer)
        {
            if (renderer is IUniqueValueRenderer)
                return convert((IUniqueValueRenderer)renderer);
            else if (renderer is ISimpleRenderer)
                return convert((ISimpleRenderer)renderer);
            else if (renderer is IClassBreaksRenderer)
                return convert((IClassBreaksRenderer)renderer);

            // unsupported renderer type
            return null;
        }

        /*
         * Converts a classbreaksrenderer (quantitative symbolization)
         * into a GME Style.
         */
        private static Style convert(IClassBreaksRenderer renderer)
        {
            Style returnVal = new Style();
            returnVal.type = "displayRule";
            returnVal.displayRules = new List<DisplayRule>();
            double previousBound = renderer.MinimumBreak;
            String column = renderer.Field;
            for (int i = 0; i < renderer.BreakCount; i++)
            {
                ISymbol symbol = renderer.get_Symbol(i);
                DisplayRule displayRule = convert(symbol);
                if (null == displayRule)
                    return null;

                displayRule.filters = new List<Filter>();
                
                // add lower bound
                Filter newFilter = new Filter();
                newFilter.column = column;
                newFilter.OPERATOR_HOLDING_STRING = ">=";
                newFilter.value = previousBound;
                displayRule.filters.Add(newFilter);
                newFilter = new Filter();
                previousBound = renderer.get_Break(i);
                newFilter.column = column;
                newFilter.OPERATOR_HOLDING_STRING = "<";
                newFilter.value = previousBound;
                displayRule.filters.Add(newFilter);
                returnVal.displayRules.Add(displayRule);
            }
            return returnVal;
        }

        /*
         * Converts a uniquevaluerenderer (qualitative symbolization) into
         * a GME Style.
         */
        private static Style convert(IUniqueValueRenderer renderer)
        {
            Style returnVal = new Style();
            returnVal.type = "displayRule";
            returnVal.displayRules = new List<DisplayRule>();
            string column = renderer.get_Field(0);

            for (int i = 0; i < renderer.ValueCount; i++)
            {
                string val = renderer.get_Value(i);
                string referenceValue = val;
                try
                {
                    referenceValue = renderer.get_ReferenceValue(val);
                }
                catch (Exception) { }

                ISymbol symbol = renderer.get_Symbol(referenceValue);
                DisplayRule displayRule = convert(symbol);
                if (null == displayRule)
                    return null;

                displayRule.filters = new List<Filter>();
                Filter newFilter = new Filter();
                newFilter.column = column;
                newFilter.OPERATOR_HOLDING_STRING = "==";
                newFilter.value = val;
                displayRule.filters.Add(newFilter);
                returnVal.displayRules.Add(displayRule);
            }

            return returnVal;
        }

        /*
         * Converts a simple renderer into a GME Style.
         */
        private static Style convert(ISimpleRenderer renderer)
        {
            Style returnVal = new Style();
            returnVal.type = "displayRule";
            DisplayRule displayRule = convert(renderer.Symbol);
            if (null == displayRule) return null;
            returnVal.displayRules = new List<DisplayRule>() { displayRule };
            return returnVal;
        }

        /*
         * Converts a symbol into a displayrule.
         */
        private static DisplayRule convert(ISymbol symbol)
        {
            DisplayRule returnValue = new DisplayRule();
            returnValue.name = "displayRule";
            returnValue.zoomLevels = new ZoomLevels();
            returnValue.zoomLevels.min = 0;
            returnValue.zoomLevels.max = 24;
            if (symbol is ISimpleMarkerSymbol)
                returnValue = updateDisplayRule(returnValue, (ISimpleMarkerSymbol)symbol);
            else if (symbol is IMultiLayerMarkerSymbol)
            {
                if (((IMultiLayerMarkerSymbol)symbol).get_Layer(0) is ISimpleMarkerSymbol)
                    returnValue = updateDisplayRule(returnValue, (ISimpleMarkerSymbol)((IMultiLayerMarkerSymbol)symbol).get_Layer(0));
                else return null;
            }
            else if (symbol is ISimpleLineSymbol)
                returnValue = updateDisplayRule(returnValue, (ISimpleLineSymbol)symbol);
            else if (symbol is IMultiLayerLineSymbol)
            {
                if (((IMultiLayerLineSymbol)symbol).get_Layer(0) is ISimpleLineSymbol)
                    returnValue = updateDisplayRule(returnValue, (ISimpleLineSymbol)((IMultiLayerLineSymbol)symbol).get_Layer(0));
                else return null;
            }
            else if (symbol is IMultiLayerFillSymbol)
            {
                if (((IMultiLayerFillSymbol)symbol).get_Layer(0) is ISimpleFillSymbol)
                    returnValue = updateDisplayRule(returnValue, (ISimpleFillSymbol)((IMultiLayerFillSymbol)symbol).get_Layer(0));
                else return null;
            }
            else if (symbol is ISimpleFillSymbol)
                returnValue = updateDisplayRule(returnValue, (ISimpleFillSymbol)symbol);
            else return null;

            return returnValue;
        }

        /*
         * Updates a displayrule to reflect a point symbol in Arc.
         */
        private static DisplayRule updateDisplayRule(DisplayRule updateMe, ISimpleMarkerSymbol symbol)
        {

            updateMe.pointOptions = new PointStyle();
            updateMe.pointOptions.icon = new IconStyle();
            IRgbColor test = new RgbColor();
            test.RGB = symbol.Color.RGB;
            int r = (int)test.Red;
            int b = (int)test.Blue;
            int g = (int)test.Green;


            if (((r >= 192) && (128 > b) && (128 > g)) ||
                    ((r >= 128) && (64 > g) && (64 > g)))
            {
                updateMe.pointOptions.icon.name = "gx_small_red";
            }
            else if (((g >= 192) && (128 > b) && (128 > r)) ||
                ((g >= 128) && (64 > b) && (64 > r)))
            {
                updateMe.pointOptions.icon.name = "gx_small_green";
            }
            else if (((b >= 192) && (128 > g) && (128 > r)) ||
                ((b >= 128) && (64 > g) && (64 > r)))
            {
                updateMe.pointOptions.icon.name = "gx_small_blue";
            }
            else if (((r >= 192) && (128 > b) && (g >= 192)) ||
            ((r >= 128) && (64 > b) && (g >= 128)))
            {
                updateMe.pointOptions.icon.name = "gx_small_yellow";
            }
            else if (((r >= 192) && (128 > g) && (b >= 192)) ||
            ((r >= 128) && (64 > g) && (b >= 128)))
            {
                updateMe.pointOptions.icon.name = "gx_small_purple";
            }
            else if (((g >= 192) && (128 > r) && (b >= 192)) ||
            ((g >= 128) && (64 > r) && (b >= 128)))
            {
                updateMe.pointOptions.icon.name = "gx_measle_turquoise";
            }

            else if ((192 <= r) && (192 <= b) && (192 <= g))
            {
                updateMe.pointOptions.icon.name = "gx_measle_white";
            }
            else
            {
                updateMe.pointOptions.icon.name = "gx_measle_grey";
            }

            return updateMe;
        }

        /*
         * Updates a displayrule to reflect a line symbol in Arc.
         */
        private static DisplayRule updateDisplayRule(DisplayRule updateMe, ISimpleLineSymbol symbol)
        {
            updateMe.lineOptions = new LineStyle();
            updateMe.lineOptions.stroke = getStroke(symbol);
            switch (symbol.Style)
            {
                case esriSimpleLineStyle.esriSLSDash:
                    updateMe.lineOptions.dash = new List<double>() { 5.0, 2.0 };
                    break;
                case esriSimpleLineStyle.esriSLSDashDot:
                    updateMe.lineOptions.dash = new List<double>() { 5.0, 2.0, 2.0, 2.0 };
                    break;
                case esriSimpleLineStyle.esriSLSDashDotDot:
                    updateMe.lineOptions.dash = new List<double>() { 5.0, 2.0, 2.0, 2.0, 2.0, 2.0 };
                    break;
                case esriSimpleLineStyle.esriSLSDot:
                    updateMe.lineOptions.dash = new List<double>() { 2.0, 2.0 };
                    break;
                default:
                    // do nothing
                    break;
            }
            return updateMe;
        }


        /*
         * Updates a displayrule to reflect a polygon symbol in Arc.
         */
        private static DisplayRule updateDisplayRule(DisplayRule updateMe, IMultiLayerFillSymbol symbol)
        {
            updateMe.polygonOptions = new PolygonStyle();
            updateMe.polygonOptions.fill = new MapsEngine.DataModel.gme.Color();
            updateMe.polygonOptions.fill.color = getColor(symbol.Color);
            updateMe.polygonOptions.fill.opacity = DEFAULT_OPACITY;
            updateMe.polygonOptions.stroke = getStroke(symbol.Outline);

            return updateMe;
        }

        /*
         * Updates a displayrule to reflect the polygon symbol in Arc.
         */
        private static DisplayRule updateDisplayRule(DisplayRule updateMe, ISimpleFillSymbol symbol)
        {
            updateMe.polygonOptions = new PolygonStyle();
            updateMe.polygonOptions.fill = new MapsEngine.DataModel.gme.Color();
            updateMe.polygonOptions.fill.color = getColor(symbol.Color);
            updateMe.polygonOptions.fill.opacity = DEFAULT_OPACITY;
            updateMe.polygonOptions.stroke = getStroke(symbol.Outline);

            return updateMe;
        }

        /*
         * Convert a line symbol into a GME Stroke
         */
        private static Stroke getStroke(ILineSymbol lineSymbol)
        {
            if (lineSymbol.Width == 0)
                return null;

            Stroke returnValue = new Stroke();

            returnValue.color = getColor(lineSymbol.Color);
            returnValue.width = lineSymbol.Width;
            returnValue.opacity = DEFAULT_OPACITY;
            return returnValue;
        }

        /*
         * Transforms the color to an html color usable with a GME Style
         */
        private static String getColor(IColor color)
        {
            IRgbColor test = new RgbColor();
            test.RGB = color.RGB;
            return ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(255, test.Red, test.Green,test.Blue));
        }

        private const double DEFAULT_OPACITY = 0.60;
    }
}
