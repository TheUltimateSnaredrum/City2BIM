﻿using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using static City2BIM.Prop;

namespace City2BIM.GetGeometry
{
    public class C2BSolid
    {
        private Dictionary<string, C2BPlane> planes = new Dictionary<string, C2BPlane>();
        private Dictionary<string, C2BPlane> planesCopy = new Dictionary<string, C2BPlane>();
        private List<C2BVertex> vertices = new List<C2BVertex>();
        private List<C2BEdge> edges = new List<C2BEdge>();

        public List<C2BVertex> Vertices
        {
            get { return vertices; }
        }

        public Dictionary<string, C2BPlane> Planes
        {
            get { return planes; }
        }

        public Dictionary<string, C2BPlane> PlanesCopy
        {
            get { return planesCopy; }
        }

        public List<C2BEdge> Edges { get => edges; set => edges = value; }

        public void AddPlane(string id, List<C2BPoint> polygon, List<List<C2BPoint>> innerPolygons = null)
        {
            if (polygon.Count < 4)
            {
                Log.Error("Not enough points for valid plane: " + polygon.Count);
            }

            C2BPoint normal = new C2BPoint(0, 0, 0);
            C2BPoint centroid = new C2BPoint(0, 0, 0);

            List<C2BEdge> locEdges = new List<C2BEdge>();

            ////list for vertex-integers (size = polygon-points.Length)
            //List<int> verts = new List<int>(polygon.Count);
            //int currentInt = 0;

            ////Loop over all polygon points (starts with 1 for no redundant point detecting)
            //for (int i = 1; i < polygon.Count; i++)
            //{
            //    int beforeInt = currentInt;

            //    //bool for decision if new vertex must be created
            //    bool notmatched = true;

            //    //inner loop over all vertices (list per whole building (!), if vertices contains objects)
            //    for (int j = 0; j < Vertices.Count; j++)
            //    {
            //        //calclation of distance between current polygon point and current vertex in list 
            //        double dist = C2BPoint.DistanceSq(polygon[i], vertices[j].Position);

            //        //case: distance smaller than setted Distolsq (= points are topological identical --> Vertex)
            //        //if points are identical, an equivalent vertex is still existinng in vertices list
            //        if (dist < Distolsq)
            //        {
            //            currentInt = j;

            //            //add plane id to current vertex in list
            //            vertices[j].AddPlane(id);
            //            //add vertex-iterator to verts list (for later identification)
            //            verts.Add(j);
            //            notmatched = false;
            //            break;
            //        }
            //    }

            //    //no match --> a new vertex needs to create
            //    if (notmatched)
            //    {
            //        currentInt = vertices.Count;

            //        C2BVertex v = new C2BVertex(polygon[i], id);
            //        //list of verts gets a new number at the end of list
            //        verts.Add(vertices.Count);
            //        //Vertex bldg list gets new Vertex
            //        vertices.Add(v);
            //    }

            //    //------------------------------------------------------------------------------

            //    //adds normal value (normal of plane which current point and the point before span) 
            //    normal += C2BPoint.CrossProduct(polygon[i - 1], polygon[i]);

            //    //adds current coordinates to centroid variable for later centroid calculation
            //    centroid += polygon[i];

            //    //edge needs start- and end vertex, and also normal between points for later identification of similar planes

            //}

            List<int> extVerts = CalculateVertexCoords(id, polygon, true, ref normal, ref centroid);
            List<List<int>> intVertsList = new List<List<int>>();

            foreach (List<C2BPoint> inPoly in innerPolygons)
            {
                List<int> intVerts = CalculateVertexCoords(id, inPoly, false, ref normal, ref centroid);
                intVertsList.Add(intVerts);
            }

            C2BPoint planeNormal = C2BPoint.Normalized(normal);

            for (var v = 0; v < extVerts.Count; v++)
            {
                int beforeInt = 0;

                if (v == 0)
                    beforeInt = extVerts.Last();
                else
                    beforeInt = extVerts[v - 1];

                var edge = new C2BEdge(beforeInt, extVerts[v], id, planeNormal);

                locEdges.Add(edge);
                Edges.Add(edge);
            }

            //create plane..
            //with plane normal (via normalization of spanned normals of the poly points) 
            //with centroid dependent of number of poly points
            var plane = new C2BPlane(id, extVerts, intVertsList, planeNormal, centroid / ((double)extVerts.Count), locEdges);

            Planes.Add(id, plane);

        }

        private List<int> CalculateVertexCoords(string id, List<C2BPoint> polygon, bool exterior, ref C2BPoint normal, ref C2BPoint centroid)
        {
            //list for vertex-integers (size = polygon-points.Length)
            List<int> verts = new List<int>(polygon.Count);
            int currentInt = 0;

            //Loop over all polygon points (starts with 1 for no redundant point detecting)
            for (int i = 1; i < polygon.Count; i++)
            {
                int beforeInt = currentInt;

                //bool for decision if new vertex must be created
                bool notmatched = true;

                //inner loop over all vertices (list per whole building (!), if vertices contains objects)
                for (int j = 0; j < Vertices.Count; j++)
                {
                    //calclation of distance between current polygon point and current vertex in list 
                    double dist = C2BPoint.DistanceSq(polygon[i], vertices[j].Position);

                    //case: distance smaller than setted Distolsq (= points are topological identical --> Vertex)
                    //if points are identical, an equivalent vertex is still existinng in vertices list
                    if (dist < Distolsq)
                    {
                        currentInt = j;

                        //add plane id to current vertex in list
                        vertices[j].AddPlane(id);
                        //add vertex-iterator to verts list (for later identification)
                        verts.Add(j);
                        notmatched = false;
                        break;
                    }
                }

                //no match --> a new vertex needs to create
                if (notmatched)
                {
                    currentInt = vertices.Count;

                    C2BVertex v = new C2BVertex(polygon[i], id);
                    //list of verts gets a new number at the end of list
                    verts.Add(vertices.Count);
                    //Vertex bldg list gets new Vertex
                    vertices.Add(v);
                }

                //------------------------------------------------------------------------------

                if (exterior)
                {
                //adds normal value (normal of plane which current point and the point before span) 
                normal += C2BPoint.CrossProduct(polygon[i - 1], polygon[i]);

                //adds current coordinates to centroid variable for later centroid calculation
                centroid += polygon[i];
                }
            }

            return verts;
        }


        public void IdentifySimilarPlanes()
        {
            //----------------------------------------------------
            //PlaneyCopy list needed for later address at level cuts
            //original list will not be corrupted because original surface geometry should be imported (not combined surfaces)
            planesCopy = new Dictionary<string, C2BPlane>();

            foreach (var pl in Planes)
            {
                planesCopy.Add(pl.Key, pl.Value);
            }
            //----------------------------------------------------

            //cases for similar planes
            //1.) simple case: similar plane normal and one shared edge
            //2.) special cases: more than two planes with similar plane normal and respectively one shared edge (case 1 applied multiple times)
            //the possibility of case 2 requires combining of two planes and a new search for similar planes after each combination

            bool similarPlanes = true;

            while (similarPlanes)
            {
                AggregatePlanes(ref similarPlanes);
            }

            RemoveNoCornerVertices();
        }


        private void AggregatePlanes(ref bool simPlanes)
        {
            double locDistolsq = 0.0025; //entspricht 5 cm

            for (var i = 0; i < Edges.Count; i++)
            {
                for (var j = i + 1; j < Edges.Count; j++)
                {
                    double distNorm = C2BPoint.DistanceSq(Edges[i].PlaneNormal, Edges[j].PlaneNormal);

                    if (Edges[i].Start == Edges[j].End && Edges[i].End == Edges[j].Start &&
                        distNorm < locDistolsq)
                    {
                        C2BPlane plane1 = planesCopy[Edges[i].PlaneId];
                        C2BPlane plane2 = planesCopy[Edges[j].PlaneId];

                        int cursorEdgeA = plane1.Edges.IndexOf(Edges[i]);
                        int cursorEdgeB = plane2.Edges.IndexOf(Edges[j]);

                        for (var k = 0; k < cursorEdgeB; k++)
                        {
                            var edge = plane2.Edges[0];
                            plane2.Edges.RemoveAt(0);
                            plane2.Edges.Add(edge);
                        }

                        List<C2BEdge> cpdEdgeList = new List<C2BEdge>();

                        cpdEdgeList.AddRange(plane1.Edges);
                        cpdEdgeList.RemoveAt(cursorEdgeA);
                        cpdEdgeList.InsertRange(cursorEdgeA, plane2.Edges);
                        cpdEdgeList.Remove(Edges[j]);

                        var newVerts = cpdEdgeList.Select(e => e.End).ToList();
                        var newID = Edges[i].PlaneId + "_" + Edges[j].PlaneId;


                        var changeVert1 = from v in Vertices
                                          where v.Planes.Contains(Edges[i].PlaneId)
                                          select v;

                        var changeVert2 = from v in Vertices
                                          where v.Planes.Contains(Edges[j].PlaneId)
                                          select v;

                        foreach (var v in changeVert1)
                        {
                            v.Planes.Remove(Edges[i].PlaneId);
                            v.Planes.Add(newID);
                        }

                        foreach (var v in changeVert2)
                        {
                            v.Planes.Remove(Edges[j].PlaneId);
                            v.Planes.Add(newID);
                        }

                        var currentID1 = Edges[i].PlaneId;
                        var currentID2 = Edges[j].PlaneId;

                        foreach (var e in Edges)
                        {
                            //logPlanes.Information(e.PlaneId);

                            if (e.PlaneId == currentID1 || e.PlaneId == currentID2)
                            {
                                e.PlaneId = newID;
                            }
                        }

                        Edges[i].PlaneId = null;
                        Edges[j].PlaneId = null;

                        //calc logic for new plane normal and new plane centroid

                        C2BPoint planeNormal = plane1.Normal;
                        C2BPoint planeCentroid = plane1.Centroid;

                        List<int[]> innerVerts = plane1.InnerVertices;

                        innerVerts.AddRange(plane2.InnerVertices);

                        UpdatePlaneParameters(newID, newVerts, ref planeNormal, ref planeCentroid);

                        planesCopy.Remove(plane1.ID);
                        planesCopy.Remove(plane2.ID);
                        planesCopy.Add(newID, new C2BPlane(newID, newVerts, innerVerts, planeNormal, planeCentroid, cpdEdgeList));

                        break;
                    }
                    else
                        simPlanes = false;
                }
            }
        }

        private void UpdatePlaneParameters(string planeID, List<int> newVerts, ref C2BPoint planeNormal, ref C2BPoint planeCentroid)
        {
            C2BPoint norm = new C2BPoint(0, 0, 0);
            C2BPoint centr = new C2BPoint(0, 0, 0);

            for (var i = 0; i < newVerts.Count; i++)
            {
                int lastVert = newVerts.Last();

                if (i != 0)
                    lastVert = newVerts[i - 1];

                int currVert = newVerts[i];

                C2BPoint lastPt = Vertices[lastVert].Position;
                C2BPoint thisPt = Vertices[currVert].Position;

                centr += thisPt;

                norm += C2BPoint.CrossProduct(lastPt, thisPt);

                Vertices[newVerts[i]].Planes.Add(planeID);

            }

            planeNormal = C2BPoint.Normalized(norm);
            planeCentroid = centr / (double)newVerts.Count;

            //UpdateVertexPositions(newVerts, planeNormal, planeCentroid);
        }

        private void RemoveNoCornerVertices()
        {
            var remVerts = Vertices.Where(v => v.Planes.Count < 3);

            foreach (var v in remVerts)
            {
                int vIndex = Vertices.IndexOf(v);

                var planesInd = from p in PlanesCopy
                                where p.Value.Vertices.Contains(vIndex) 
                                select p.Value;

                foreach (var pl in planesInd)
                {
                    pl.Vertices = pl.Vertices.Where(i => i != vIndex).ToArray();
                }

                var planesIndInt = from p in PlanesCopy
                                   where p.Value.InnerVertices.Any()
                                   select p.Value;

                foreach (var plane in planesIndInt)
                {
                    for (int j = 0; j < plane.InnerVertices.Count; j++)
                    {
                        if (plane.InnerVertices[j].Contains(vIndex))
                            plane.InnerVertices[j] = plane.InnerVertices[j].Where(i => i != vIndex).ToArray();
                    }
                }
            }
        }

        //private void UpdateVertexPositions(List<int> newVerts, C2BPoint planeNormal, C2BPoint planeCentroid)
        //{
        //    var projectedVerts = new List<C2BPoint>();

        //    for (var i = 0; i < newVerts.Count; i++)
        //    {
        //        C2BPoint thisPt = Vertices[newVerts[i]].Position;

        //        var vecPtCent = thisPt - planeCentroid;
        //        var d = C2BPoint.ScalarProduct(vecPtCent, planeNormal);

        //        var vecLotCent = new C2BPoint(d * planeNormal.X, d * planeNormal.Y, d * planeNormal.Z);
        //        var vertNew = thisPt - vecLotCent;

        //        Vertices[newVerts[i]].Position = vertNew;
        //    }
        //}

        public void CalculatePositions()
        {
            Dictionary<int, string[]> planesToSplit = new Dictionary<int, string[]>();

            for (var v = 0; v < vertices.Count; v++)
            {
                Log.Debug("Calculation for " + v);

                if (vertices[v].Planes.Count < 3)     //cases of removed vertices before, no longer consideration but no removement of Vertex-List because of consistency
                {
                    Log.Debug("Skip because < 3 Planes!");
                    continue;
                }

                if (vertices[v].Planes.Count == 3)    //optimal wished case --> no danger of non planar curve loops
                {
                    Log.Debug("Optimal case: 3 Planes!");

                    C2BPoint vertex = new C2BPoint(0, 0, 0);

                    string[] vplanes = vertices[v].Planes.ToArray<string>();

                    //vertices[v].Position = CalculateLevelCut(planes[vplanes[0]], planes[vplanes[1]], planes[vplanes[2]]);
                    vertices[v].Position = CalculateLevelCut(planesCopy[vplanes[0]], planesCopy[vplanes[1]], planesCopy[vplanes[2]]);
                }

                if (vertices[v].Planes.Count > 3)
                {
                    Log.Debug("Dangerous case: " + vertices[v].Planes.Count + " Planes!");

                    string[] vplanes = vertices[v].Planes.ToArray<string>();
                    string[] splitPlanes = new string[vplanes.Count() - 3];

                    int first = 0, second = 0, third = 0;
                    int firstSub = 0, secondSub = 0, thirdSub = 0;
                    double d = 100;
                    C2BPoint origPos = vertices[v].Position;
                    C2BPoint calcPos = origPos;
                    bool divisible = false;

                    for (var i = 0; i < vplanes.Length - 2; i++)
                    {
                        for (var j = i + 1; j < vplanes.Length - 1; j++)
                        {
                            for (var k = j + 1; k < vplanes.Length; k++)
                            {
                                C2BPoint currPos = CalculateLevelCut(planesCopy[vplanes[i]], planesCopy[vplanes[j]], planesCopy[vplanes[k]]);

                                if (currPos == null)
                                    continue;

                                double dOld = d;
                                double dNew = C2BPoint.DistanceSq(origPos, currPos);

                                if (dNew < dOld)
                                {
                                    firstSub = i;
                                    secondSub = j;
                                    thirdSub = k;

                                    //extension:
                                    //not the best case if other planes have to much vertices (difficult to split later)

                                    string[] otherPlanes = vplanes.Where(w => w != vplanes[i] && w != vplanes[j] && w != vplanes[k]).ToArray();

                                    int[] vertsCt = new int[otherPlanes.Count()];

                                    for (int n = 0; n < otherPlanes.Length; n++)
                                    {
                                        vertsCt[n] = planesCopy[otherPlanes[n]].Vertices.Count();
                                    }

                                    int greater5verts = vertsCt.Where(m => m > 4).Count();

                                    if (greater5verts == 0 && dNew < 10 /*0.05*/)       
                                    {
                                        first = i;
                                        second = j;
                                        third = k;

                                        calcPos = currPos;
                                        d = dNew;

                                        divisible = true;

                                        splitPlanes = otherPlanes;
                                    }
                                }
                            }
                        }
                    }

                    vertices[v].Position = calcPos;

                    planesToSplit.Add(v, splitPlanes);

                    if (!divisible)
                    {
                        Log.Debug("Komplizierter Schnitt nötig!");
                    }
                    else
                    {
                        Log.Debug("Kein komplizierter Schnitt nötig!");
                    }
                }
            }
            //Achtung: nur für SplitPlanes mit 4 Vertices (=Vierecke)

            foreach (var vPl in planesToSplit)
            {
                Log.Debug("Split at " + vPl.Key + " for:");

                try
                {

                    foreach (var p in vPl.Value)
                    {
                        Log.Debug(p);

                        var sPlane = planesCopy[p];
                        var sVerts = sPlane.Vertices;

                        if (sVerts.Length == 3)     //no split neccessary because of triangle plane
                            continue;

                        var index = Array.IndexOf(sVerts, vPl.Key);

                        int vertBefore = 0;
                        int vertNext = 0;
                        int vertOpposite = 0;
                        
                        switch (index)
                        {
                            case 0:
                                {
                                    vertBefore = sVerts[3];
                                    vertNext = sVerts[1];
                                    vertOpposite = sVerts[2];
                                    break;
                                }
                            case 1:
                                {
                                    vertBefore = sVerts[0];
                                    vertNext = sVerts[2];
                                    vertOpposite = sVerts[3];
                                    break;
                                }
                            case 2:
                                {
                                    vertBefore = sVerts[1];
                                    vertNext = sVerts[3];
                                    vertOpposite = sVerts[0];
                                    break;
                                }
                            case 3:
                                {
                                    vertBefore = sVerts[2];
                                    vertNext = sVerts[0];
                                    vertOpposite = sVerts[1];
                                    break;
                                }
                        }

                        //PlanesToSplit enthalten bestenfalls nur noch Planes, mit 4 Vertices
                        //Achtung: Löcher (InnerVerts werden in SplitPlanes übernommen --> funktioniert nur, wenn wirlich keine Löcher vorhanden sind --> TO DO!

                        //Normale ist hier alte nicht exakte Normale, centroid und edges sind def falsch (sollte aber alles nicht mehr relevant sein)
                        C2BPlane splitPlaneTri1 =
                            new C2BPlane(p + "_split1", new List<int>() { vertBefore, vPl.Key, vertOpposite }, sPlane.InnerVertices, sPlane.Normal, sPlane.Centroid, sPlane.Edges);

                        //Rest des Planes (Dreieck an Vertex abgeschnitten), beachte falsche Edges, (Normale), Centroid
                        C2BPlane splitPlaneTri2 =
                            new C2BPlane(p + "_split2", new List<int>() { vertOpposite, vPl.Key, vertNext }, sPlane.InnerVertices, sPlane.Normal, sPlane.Centroid, sPlane.Edges);

                        planesCopy.Add("SPLITTED_01" + splitPlaneTri1.ID, splitPlaneTri1);
                        planesCopy.Add("SPLITTED_02" + splitPlaneTri2.ID, splitPlaneTri2);
                        planesCopy.Remove(p);
                    }
                }
                catch
                {
                    continue;
                }

            }
        }

        private C2BPoint CalculateLevelCut(C2BPlane plane1, C2BPlane plane2, C2BPlane plane3)
        {
            double determinant = 0;
            determinant = C2BPoint.ScalarProduct(plane1.Normal, C2BPoint.CrossProduct(plane2.Normal, plane3.Normal));

            if (Math.Abs(determinant) > Determinanttol)
            {
                C2BPoint pos = (C2BPoint.CrossProduct(plane2.Normal, plane3.Normal) * C2BPoint.ScalarProduct(plane1.Centroid, plane1.Normal) +
                           C2BPoint.CrossProduct(plane3.Normal, plane1.Normal) * C2BPoint.ScalarProduct(plane2.Centroid, plane2.Normal) +
                           C2BPoint.CrossProduct(plane1.Normal, plane2.Normal) * C2BPoint.ScalarProduct(plane3.Centroid, plane3.Normal)) /
                           determinant;
                return pos;
            }
            else
                return null;
        }
    }
}