﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plasticine {

//
// Put this script on an empty node
//
public class TestBasicShapes : MonoBehaviour {

    void Start () {

        // 3 sides
        CreateTriangle (-2f, -2f);
        CreateTriangleUp(-2f, 0f);
        CreateTriangleHop(-2f, 2f);

        // 4 sides
        CreateUnitTile (0, -2f);
        CreateUnitTileUp (0, 0f);
        CreateUnitTileHop (0, 2f);

        // 5 sides
        CreatePentagon (2f, -2f);
        CreatePentagonUp (2f, 0f);
        CreatePentagonHop (2f, 2f);
    }

    //
    //
    //
    private GameObject CreateUnitTile(float x, float z)
    {
        GameObject obj = CreateChild (x, z);

        PointList points = PrimitiveBuilder.CreateUnitTile ();
        MeshBuilder builder = new MeshBuilder ();
        builder.Cap (points);
        Mesh mesh = builder.Build ();

        NodeBuilder.SetMesh (obj, mesh);
        return obj;
    }

    private GameObject CreateUnitTileUp(float x, float z)
    {
        GameObject obj = CreateChild (x, z);

        PointList pointsA = PrimitiveBuilder.CreateUnitTile ();
        PointList pointsB = pointsA.Translate (Vector3.up);

        List<PointList> list = pointsA.Bridge (pointsB, PointList.BridgeMode.CloseReuse);

        MeshBuilder builder = new MeshBuilder ();
        builder.Cap (list);
        builder.Cap (pointsB);
        builder.Cap (pointsA.Reverse());
        Mesh mesh = builder.Build ();

        NodeBuilder.SetMesh (obj, mesh);
        return obj;
    }

    private GameObject CreateUnitTileHop(float x, float z)
    {
        GameObject obj = CreateChild (x, z);

        MeshBuilder builder = new MeshBuilder ();
        PointList pointsA = PrimitiveBuilder.CreateUnitTile ();
        PointList pointsB = pointsA.Translate (Vector3.up);
        List<PointList> list = pointsA.Bridge(pointsB, PointList.BridgeMode.CloseReuse);
        list.Add (pointsA.Reverse ());
        list.Add (pointsB);
        foreach(PointList points in list) {
            Extend (points, builder);
        }
        Mesh mesh = builder.Build ();

        NodeBuilder.SetMesh (obj, mesh);
        return obj;
    }

    // -------------------------------------

    private GameObject CreateTriangle(float x, float z)
    {
        GameObject obj = CreateChild (x, z);
        
        MeshBuilder builder = new MeshBuilder ();
        builder.Cap (PrimitiveBuilder.CreateUnitPolygon (3));
        Mesh mesh = builder.Build ();

        NodeBuilder.SetMesh (obj, mesh);
        return obj;
    }

    private GameObject CreateTriangleUp(float x, float z)
    {
        GameObject obj = CreateChild (x, z);
        
        MeshBuilder builder = new MeshBuilder ();
        PointList pointsA = PrimitiveBuilder.CreateUnitPolygon (3);
        PointList pointsB = pointsA.Translate(new Vector3(0f, 1f, 0f));
        builder.Cap ( pointsA.Bridge(pointsB, PointList.BridgeMode.CloseReuse) );
        builder.Cap ( pointsB );
        builder.Cap ( pointsA.Reverse() );
        Mesh mesh = builder.Build ();

        NodeBuilder.SetMesh (obj, mesh);
        return obj;
    }

    private GameObject CreateTriangleHop(float x, float z)
    {
        GameObject obj = CreateChild (x, z);
        
        MeshBuilder builder = new MeshBuilder ();
        PointList pointsA = PrimitiveBuilder.CreateUnitPolygon (3);
        PointList pointsB = pointsA.Translate(new Vector3(0f, 1f, 0f));
        List<PointList> list = pointsA.Bridge(pointsB, PointList.BridgeMode.CloseReuse);
        list.Add( pointsB );
        list.Add ( pointsA.Reverse() );
        foreach(PointList points in list) {
            Extend (points, builder);
        }
        Mesh mesh = builder.Build ();

        NodeBuilder.SetMesh (obj, mesh);
        return obj;
    }

    // -------------------------------------

    private GameObject CreatePentagon(float x, float z)
    {
        GameObject obj = CreateChild (x, z);
        
        MeshBuilder builder = new MeshBuilder ();
        builder.Cap (PrimitiveBuilder.CreateUnitPolygon (5));
        Mesh mesh = builder.Build ();

        NodeBuilder.SetMesh (obj, mesh);
        return obj;
    }

    private GameObject CreatePentagonUp(float x, float z)
    {
        GameObject obj = CreateChild (x, z);
        
        MeshBuilder builder = new MeshBuilder ();
        PointList pointsA = PrimitiveBuilder.CreateUnitPolygon (5);
        PointList pointsB = pointsA.Translate(new Vector3(0f, 1f, 0f));
        builder.Cap ( pointsA.Bridge(pointsB, PointList.BridgeMode.CloseReuse) );
        builder.Cap ( pointsB );
        builder.Cap ( pointsA.Reverse() );
        Mesh mesh = builder.Build ();

        NodeBuilder.SetMesh (obj, mesh);
        return obj;
    }

    private GameObject CreatePentagonHop(float x, float z)
    {
        GameObject obj = CreateChild (x, z);
        
        MeshBuilder builder = new MeshBuilder ();
        PointList pointsA = PrimitiveBuilder.CreateUnitPolygon (5);
        PointList pointsB = pointsA.Translate(new Vector3(0f, 1f, 0f));
        List<PointList> list = pointsA.Bridge(pointsB, PointList.BridgeMode.CloseReuse);
        list.Add( pointsB );
        list.Add ( pointsA.Reverse() );
        foreach(PointList points in list) {
            Extend (points, builder);
        }
        Mesh mesh = builder.Build ();

        NodeBuilder.SetMesh (obj, mesh);
        return obj;
    }

    // -------------------------------------

    void Extend(PointList points, MeshBuilder builder) {
        Vector3 n = points.ComputeNormal ();
        PointList pointsB = points.Translate (0.4f*n.normalized);
        builder.Cap(points.Bridge(pointsB, PointList.BridgeMode.CloseReuse));
        DigHole (pointsB, builder);
    }

    void DigHole(PointList points, MeshBuilder builder) {
        Vector3 n = points.ComputeNormal ();
        PointList pointsC = points.Scale (0.5f);
        PointList pointsD = pointsC.Translate (-0.1f*n.normalized);
        builder.Cap(points.Bridge(pointsC, PointList.BridgeMode.CloseReuse));
        builder.Cap(pointsC.Bridge(pointsD, PointList.BridgeMode.CloseReuse));
        builder.Cap (pointsD);
    }

    // -------------------------------------

    //
    // Create a new node under current node
    // 
    private GameObject CreateChild(float x, float z)
    {
        GameObject obj = new GameObject ();
        obj.transform.SetParent (transform);
        obj.transform.localPosition = new Vector3 (x, 0, z);
        return obj;
    }

}

}
