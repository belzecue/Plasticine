﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plasticine {

    //
    // Basic class handling geometric transformations
    //
    public class PointList : IEnumerable<Vector3> {

        //
        // Points are explicitely represented
        //
        private List<Vector3> m_points = new List<Vector3>();

        //
        // UV's are implicitly represented (default is (0,0) everywhere)
        //
        private IUVMapper m_uvMapper = new ZeroMapper();

        //
        // Keep a unique identifier for each unique point, share id when needed
        //
        private List<int> m_uids = new List<int>();

        //
        // Generator of unique identifiers
        //
        private static int m_uidCounter = 0;
        private static int NextUid() {
            return m_uidCounter++;
        }

        //
        // Add a new point
        //
        public void Add(Vector3 point) {
            m_points.Add (point);
            m_uids.Add (NextUid ());
        }

        public void Add(float x, float y, float z) {
            m_points.Add (new Vector3(x, y, z));
            m_uids.Add (NextUid ());
        }

        //
        // Copy a point from another PointList and preserve point uid, see Bridge(), Reverse(), Shift()  methods
        //
        public void Copy(PointList list, int index) {
            m_points.Add (list.m_points[index]);
            m_uids.Add (list.m_uids[index]);
        }

        //
        // Operator [] : get the point at a given index
        //
        public Vector3 this [int index] {
            get {
                return m_points [index];
            }
        }

        //
        // Unique identifier of point at a given index
        //
        public int Uid(int index) {
            return m_uids [index];
        }

        //
        // Get the number of points in the list 
        //
        public int Count
        {
            get {
                return m_points.Count;
            }
        }

        //
        // Implementation of IEnumerable<> interface
        //
        public IEnumerator<Vector3> GetEnumerator() {
            return m_points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return m_points.GetEnumerator();
        }

        //
        // UV Mapper access
        //
        public IUVMapper UVMapper 
        {
            get {
                return m_uvMapper;
            }
            set {
                m_uvMapper = value;
            }
        }

        //
        // Empty the list
        //
        public void Clear() {
            m_points.Clear ();
            m_uids.Clear ();
        }

        //
        // Create a clone with points having same coordinates but different uids
        //
        public PointList Duplicate()
        {
            PointList points = new PointList();
            for (int i = 0; i < this.Count; i++) {
                points.Add (this [i]);
            }
            points.UVMapper = this.UVMapper;
            return points;
        }

        // ---------------------------------------------------------

        //
        // Barycenter
        //
        public Vector3 ComputeBarycenter() {
            Vector3 center = new Vector3 ();
            foreach (Vector3 point in this) {
                center += point;
            }
            return center*(float)(1f/this.Count);
        }

        //
        // Normal, warning : it is not normalized
        //
        public Vector3 ComputeNormal() {
            if (Count > 2) {
                return Vector3.Cross (this [1] - this [0], this [2] - this [0]);
            } else {
                return Vector3.zero;
            }
        }

        //
        // Reverse
        //
        public PointList Reverse() {
            PointList result = new PointList();
            for (int i=this.Count-1; i>=0; i--) {
                result.Copy (this, i);
            }
            return result;
        }

        //
        // Shift
        //
        public PointList Shift(int shift = 1) {
            PointList result = new PointList();
            for (int i=0; i<this.Count; i++) {
                int index = (i + shift) % this.Count;
                result.Copy (this, index);
            }
            return result;
        }

        //
        // Min
        //
        public float Min(Axis axis)
        {
            float min = float.MaxValue;
            switch(axis) {
            case Axis.XAxis:
                for (int i = 0; i < Count; i++) {
                    min = Mathf.Min (this [i].x, min);
                }
                break;
            case Axis.YAxis:
                for (int i = 0; i < Count; i++) {
                    min = Mathf.Min (this [i].y, min);
                }
                break;
            case Axis.ZAxis:
                for (int i = 0; i < Count; i++) {
                    min = Mathf.Min (this [i].z, min);
                }
                break;
            }
            return min;
        }

        //
        // Translate
        //
        public PointList Translate(Vector3 direction) {
            PointList result = new PointList();
            foreach (Vector3 point in this) {
                result.Add (new Vector3 (point.x+direction.x, point.y+direction.y, point.z+direction.z));
            }
            return result;
        }

        public PointList Translate(float x, float y, float z) {
            return Translate (new Vector3 (x, y, z));
        }

        //
        // Bridge : return each side as a PointList
        //

        public enum BridgeMode
        {
            Open,
            CloseReuse,    // Reuse first point to close the loop
            CloseDuplicate // Duplicate first points to close the loop
        }

        public List<PointList> Bridge (PointList pointsB, BridgeMode mode = BridgeMode.Open) {
            List<PointList> list = new List<PointList> ();

            if (Count != pointsB.Count) {
                
                Debug.LogError ("PointList : Cannot Bridge PointLists of different size (" + Count + " / " + pointsB.Count + ")");
            
            } else {

                // Add top points
                int iMax = this.Count - 1;
                if (mode == BridgeMode.CloseReuse) {
                    iMax++;
                }

                for (int i = 0; i < iMax; i++) {
                    // Add side points
                    PointList points = new PointList ();
                    points.Copy (this, i);
                    points.Copy (this, (i + 1) % this.Count);
                    points.Copy (pointsB, (i + 1) % this.Count);
                    points.Copy (pointsB, i);
                    list.Add (points);
                }

                if (mode == BridgeMode.CloseDuplicate) {
                    PointList points = new PointList ();
                    points.Copy (this, iMax);
                    points.Add (this [0]);
                    points.Add (pointsB [0]);
                    points.Copy (pointsB, iMax);
                    list.Add (points);
                }
            }

            return list;
        }

        //
        // Use origin, TODO : new origin should be returned !
        //
        public PointList Extrude(Vector3 origin, Vector3 direction, Vector3 nextDirection)
        {
            PointList result = new PointList();

            // Project origin
            Vector3 newOrigin = origin + direction;

            // Plane normal
            Vector3 planeNormal = Vector3.Lerp (direction.normalized, nextDirection.normalized, 0.5f);

            foreach (Vector3 point in this) {
                Vector3 newPoint = LinePlaneIntersection (point, direction, newOrigin, planeNormal);
                result.Add (newPoint);
            }

            return result;
        }

        //
        // Line / Plane intersection
        //
        private static Vector3 LinePlaneIntersection(Vector3 linePoint, Vector3 lineVec, Vector3 planePoint, Vector3 planeNormal) {

            Vector3 intersection = linePoint;

            // Compute distance between linePoint and the line-plane intersection point
            float dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
            float dotDenominator = Vector3.Dot(lineVec.normalized, planeNormal);

            // line and plane are not parallel
            if(dotDenominator != 0.0f)
            {
                float length =  dotNumerator / dotDenominator;
                intersection = linePoint + length * lineVec.normalized;
            }

            return intersection;
        }

        //
        // 3 axis scale
        //
        public PointList Scale(Vector3 origin, float alpha)
        {
            PointList result = new PointList();

            foreach (Vector3 point in this) {
                Vector3 newPoint = alpha * point + (1f - alpha) * origin;
                result.Add (newPoint);
            }

            return result;
        }

        //
        // Uniform scale
        //
        public PointList Scale (float alpha) {
            return Scale(ComputeBarycenter(), alpha);
        }

        //
        // Only works for 4 sides
        //
        public List<PointList> Divide(int n, int m) {
            List<PointList> list = new List<PointList> ();

            if (this.Count != 4)
                return list;

            Vector3 vA = (this [1] - this [0])/n;
            Vector3 vB = (this [3] - this [0])/m;

            for (int i = 0; i < n; i++) {
                for (int j = 0; j < m; j++) {
                    PointList pointsA = new PointList();
                    pointsA.Add (this[0] + i*vA + j*vB);
                    pointsA.Add (this[0] + (i+1)*vA + j*vB);
                    pointsA.Add (this[0] + (i+1)*vA + (j+1)*vB);
                    pointsA.Add (this[0] + i*vA + (j+1)*vB);
                    list.Add(pointsA);
                }
            }

            return list;
        }

        //
        // alpha = 0 means object as is
        // alpha = 1 means sphere of given radius
        //
        public PointList Inflate(Vector3 center, float radius, float alpha) {
            PointList result = new PointList();

            foreach (Vector3 point in this) {
                Vector3 v = point - center;
                Vector3 spherePoint = center + radius * v.normalized;
                Vector3 newPoint = alpha * spherePoint + (1 - alpha) * point;
                result.Add (newPoint);
            }

            return result;
        }

    }

}