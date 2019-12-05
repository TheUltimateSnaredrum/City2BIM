﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using City2BIM.GetGeometry;
using City2BIM.RevitBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace City2BIM.Alkis
{
    public class AlkisReader

    {
        public ICollection<Element> SelectAllElements(UIDocument uidoc, Document doc)
        {
            FilteredElementCollector allTopos = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Topography);
            ICollection<Element> allToposList = allTopos.ToElements();
            return allToposList;
        }

        private double ConvertToRevit(string rawValue)
        {
            double val = double.Parse(rawValue, CultureInfo.InvariantCulture);
            double valUnproj = val / GeoRefSettings.ProjScale;
            double valUnprojFt = valUnproj * Prop.feetToM;

            return valUnprojFt;
        }

        private double[] ReadTopoGeom(string[] ssizeTopo)
        {
            double[] sizeTopo = new double[4];

            sizeTopo[0] = ConvertToRevit(ssizeTopo[0]);
            sizeTopo[1] = ConvertToRevit(ssizeTopo[1]);

            sizeTopo[2] = ConvertToRevit(ssizeTopo[2]);
            sizeTopo[3] = ConvertToRevit(ssizeTopo[3]);

            return sizeTopo;
        }

        public Dictionary<string, XNamespace> allns;

        private List<C2BPoint[]> ReadSegments(XElement surfaceExt)
        {
            List<C2BPoint[]> segments = new List<C2BPoint[]>();

            var posLists = surfaceExt.Descendants(allns["gml"] + "posList");

            foreach (XElement posList in posLists)
            {
                var line = ReadLineString(posList);

                segments.AddRange(line);
            }
            return segments;
        }

        private List<List<C2BPoint[]>> ReadInnerSegments(List<XElement> surfaceInt)
        {
            List<List<C2BPoint[]>> innerSegments = new List<List<C2BPoint[]>>();

            foreach (var interior in surfaceInt)
            {
                List<C2BPoint[]> segments = new List<C2BPoint[]>();

                var posLists = interior.Descendants(allns["gml"] + "posList");

                foreach (XElement posList in posLists)
                {
                    var line = ReadLineString(posList);

                    segments.AddRange(line);
                }
                innerSegments.Add(segments);
            }
            return innerSegments;
        }

        private List<C2BPoint[]> ReadLineString(XElement posList)
        {
            List<C2BPoint[]> segments = new List<C2BPoint[]>();

            var coords = posList.Value;
            string[] coord = coords.Split(' ');

            for (var c = 0; c < coord.Length - 3; c += 2)
            {
                C2BPoint start = new C2BPoint(double.Parse(coord[c], CultureInfo.InvariantCulture), double.Parse(coord[c + 1], CultureInfo.InvariantCulture), 0.0);
                C2BPoint end = new C2BPoint(double.Parse(coord[c + 2], CultureInfo.InvariantCulture), double.Parse(coord[c + 3], CultureInfo.InvariantCulture), 0.0);

                segments.Add(new C2BPoint[] { start, end });
            }
            return segments;
        }


        public List<string> parcelTypes = new List<string>
        {
            "AX_Flurstueck"
        };

        public List<string> buildingTypes = new List<string>
        {
            "AX_Gebaeude"
        };

        public List<string> usageTypes = new List<string>
        {
            //group "Siedlung"
            "AX_Wohnbauflaeche",
            "AX_IndustrieUndGewerbeflaeche",
            "AX_Halde",
            "AX_Bergbaubetrieb",
            "AX_TagebauGrubeSteinbruch",
            "AX_FlaecheGemischterNutzung",
            "AX_FlaecheBesondererFunktionalerPraegung",
            "AX_SportFreizeitUndErholungsflaeche",
            "AX_Friedhof",

            //group "Verkehr"
            "AX_Strassenverkehr",
            "AX_Weg",
            "AX_Platz",
            "AX_Bahnverkehr",
            "AX_Flugverkehr",
            "AX_Schiffsverkehr",

            //group "Vegetation"
            "AX_Landwirtschaft",
            "AX_Wald",
            "AX_Gehoelz",
            "AX_Heide",
            "AX_Moor",
            "AX_Sumpf",
            "AX_UnlandVegetationsloseFlaeche",

            //group "Gewaesser"
            "AX_Fliessgewaesser",
            "AX_Hafenbecken",
            "AX_StehendesGewaesser",
            "AX_Meer"
        };

        public AlkisReader(Document doc)
        {
            var import = new City2BIM.FileDialog();
            string path = import.ImportPath(City2BIM.FileDialog.Data.ALKIS);
            List<AX_Object> axObjects = new List<AX_Object>();

            XDocument xDoc = XDocument.Load(path);

            allns = xDoc.Root.Attributes().
                    Where(a => a.IsNamespaceDeclaration).
                    GroupBy(a => a.Name.Namespace == XNamespace.None ? String.Empty : a.Name.LocalName, a => XNamespace.Get(a.Value)).
                    ToDictionary(g => g.Key, g => g.First());

            //read all parcelTypes objects

            foreach (string axObject in parcelTypes)
            {
                var xmlObjType = xDoc.Descendants(allns[""] + axObject);

                foreach (XElement xmlObj in xmlObjType)
                {
                    AX_Object axObj = new AX_Object();
                    axObj.UsageType = axObject;

                    XElement extSeg = xmlObj.Descendants(allns["gml"] + "exterior").SingleOrDefault();
                    axObj.Segments = ReadSegments(extSeg);

                    List<XElement> intSeg = xmlObj.Descendants(allns["gml"] + "interior").ToList();
                    if (intSeg.Any())
                        axObj.InnerSegments = ReadInnerSegments(intSeg);

                    axObj.Group = AX_Object.AXGroup.parcel;
                    axObj.Attributes = new Alkis_Sem_Reader(xDoc, allns).ReadAttributeValuesParcel(xmlObj, Alkis_Semantic.GetParcelAttributes());

                    axObjects.Add(axObj);
                }
            }

            //---------------

            //read all buildingTypes objects

            foreach (string axObject in buildingTypes)
            {
                var xmlObjType = xDoc.Descendants(allns[""] + axObject);

                foreach (XElement xmlObj in xmlObjType)
                {
                    AX_Object axObj = new AX_Object();
                    axObj.UsageType = axObject;

                    XElement extSeg = xmlObj.Descendants(allns["gml"] + "exterior").SingleOrDefault();
                    axObj.Segments = ReadSegments(extSeg);

                    List<XElement> intSeg = xmlObj.Descendants(allns["gml"] + "interior").ToList();
                    if (intSeg.Any())
                        axObj.InnerSegments = ReadInnerSegments(intSeg);

                    axObj.Group = AX_Object.AXGroup.building;

                    axObjects.Add(axObj);
                }
            }

            //---------------

            //read all usageTypes objects

            foreach (string axObject in usageTypes)
            {
                var xmlObjType = xDoc.Descendants(allns[""] + axObject);

                foreach (XElement xmlObj in xmlObjType)
                {
                    AX_Object axObj = new AX_Object();
                    axObj.UsageType = axObject;

                    XElement extSeg = xmlObj.Descendants(allns["gml"] + "exterior").SingleOrDefault();
                    axObj.Segments = ReadSegments(extSeg);

                    List<XElement> intSeg = xmlObj.Descendants(allns["gml"] + "interior").ToList();
                    if (intSeg.Any())
                        axObj.InnerSegments = ReadInnerSegments(intSeg);

                    axObj.Group = AX_Object.AXGroup.usage;

                    axObjects.Add(axObj);
                }
            }

            //---------------

            var semBuilder = new RevitSemanticBuilder(doc);
            semBuilder.CreateParameters(Alkis_Semantic.GetParcelAttributes(), City2BIM.FileDialog.Data.ALKIS);

            var geomBuilder = new RevitAlkisBuilder(doc);
            geomBuilder.CreateTopo(axObjects);
        }
    }
}
