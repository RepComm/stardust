
using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Numerics;
using Vector3 = Godot.Vector3;

public partial class Chunk : Node3D
{

  Array<MeshInstance3D> blockMeshes;

  ChunkData data = new ChunkData();

  MeshInstance3D meshChild = new();
  RigidBody3D rigidBody;

  Array<CollisionShape3D> colliders = new();

  public override void _Ready()
  {
    this.rigidBody = GetParent<RigidBody3D>();
    if (this.rigidBody is not RigidBody3D) {
      throw new Exception("Chunk.GetParent did not return a RigidBody3D..");
    }

    Material mat = GD.Load<Material>("res://voxels.tres");
    this.meshChild.MaterialOverride = mat;

    this.blockMeshes = GetChildMeshes();

    foreach (var mesh in blockMeshes)
    {
      GD.Print("Block mesh", mesh.Name);
    }

    Vector3 v = new();
    for (int x = 0; x < data.dimensionSize; x++)
    {
      for (int y = 0; y < data.dimensionSize; y++)
      {
        v.X = x;
        v.Y = 0;
        v.Z = y;
        if (
          (x == 0 ||
          x == data.dimensionSize - 1) ||
          (y == 0 ||
          y == data.dimensionSize - 1)
        )
        {
          data.readBlockData[0] = 1;
        }
        else
        {
          data.readBlockData[0] = 2;
        }
        data.writeBlockAtPos(v);
      }
    }

    data.readBlockData[0] = 1;
    for (int x = 0; x < data.dimensionSize; x++)
    {
      for (int y = 1; y < data.dimensionSize; y++)
      {
        v.X = data.dimensionSize - 1;
        v.Y = y;
        v.Z = x;
        data.writeBlockAtPos(v);
      }
    }

    //clear the existing dynamic mesh data
    this.clearMesh();

    //generate dynamic mesh based on chunk data (clears dynamic data first)
    this.generateMesh();

    this.clearCollisionMesh();
    this.generateCollisionMesh();
    this.assignCollisionMesh();

    //assign to the visual node so it becomes visible in game
    this.assignMesh(
      //compile the dynamic mesh data into a ArrayMesh
      this.buildMesh()
    );

    // Add the mesh instance to the scene
    AddChild(this.meshChild);
  }

  public void generateCollisionMesh () {
    var pos = new Vector3();
    for (int x = 0; x < this.data.dimensionSize; x++)
    {
      for (int y = 0; y < this.data.dimensionSize; y++)
      {
        for (int z = 0; z < this.data.dimensionSize; z++){
          pos.X = x;
          pos.Y = y;
          pos.Z = z;

          this.data.readBlockAtPos(pos);
          var blockType = (int)this.data.readBlockData[0];
          if (blockType > 0)
          {
            var mesh = this.blockMeshes[blockType - 1];
            var aabb = mesh.GetAabb();

            var box = new BoxShape3D();
            box.Size = aabb.Size;
            var boxCollider = new CollisionShape3D();
            boxCollider.Shape = box;
            this.colliders.Add(boxCollider);
            boxCollider.Position = pos + aabb.Position;

          }

        }
      }
    }
  }

  public void generateMesh()
  {
    var pos = new Vector3();
    for (int x = 0; x < this.data.dimensionSize; x++)
    {
      for (int y = 0; y < this.data.dimensionSize; y++)
      {
        for (int z = 0; z < this.data.dimensionSize; z++)
        {
          pos.X = x;
          pos.Y = y;
          pos.Z = z;

          this.data.readBlockAtPos(pos);
          var blockType = (int)this.data.readBlockData[0];
          if (blockType > 0)
          {
            var mesh = this.blockMeshes[blockType - 1];
            this.appendMeshAt(
              mesh,
              pos
            );
          }

        }
      }
    }
  }

  public void clearMesh()
  {
    this.vertices.Clear();
  }

  public void clearCollisionMesh () {
    foreach (var col in this.colliders) {
      // rigidBody.RemoveChild(col);
      rigidBody.CallDeferred(RigidBody3D.MethodName.RemoveChild, col);
    }
    this.colliders.Clear();
  }

  public void assignCollisionMesh () {
    foreach (var col in this.colliders) {
      // rigidBody.AddChild(col);
      rigidBody.CallDeferred(RigidBody3D.MethodName.AddChild, col);
    }
  }

  public Array<MeshInstance3D> GetChildMeshes()
  {
    var nodes = GetChildrenOfType<Node3D>(this);
    var results = new Godot.Collections.Array<MeshInstance3D>();
    foreach (var node in nodes)
    {
      var meshes = GetChildrenOfType<MeshInstance3D>(node);
      foreach (var mesh in meshes)
      {
        results.Add(mesh);
      }
    }
    return results;
  }
#pragma warning disable GD0302
  public static Array<T> GetChildrenOfType<T>(Node3D parent) where T : Node3D
  {
    var results = new Godot.Collections.Array<T>();
#pragma warning restore
    var children = parent.GetChildren();
    foreach (var child in children)
    {
      if (child is T d)
      {
        results.Add(d);
      }
    }
    return results;
  }

  public Array<Vector3> vertices = new();

  public ArrayMesh buildMesh()
  {
    var vs = this.vertices.ToArray();
    //mesh
    var arrMesh = new ArrayMesh();

    //data
    var arrays = new Godot.Collections.Array();

    //don't allow too large of an array
    arrays.Resize((int)Mesh.ArrayType.Max);

    var normals = CalculateNormals(vs);

    //assign verts
    arrays[(int)Mesh.ArrayType.Vertex] = vs;
    arrays[(int)Mesh.ArrayType.Normal] = normals;

    // Create the Mesh.
    arrMesh.AddSurfaceFromArrays(
      Mesh.PrimitiveType.Triangles,
      arrays
    );
    return arrMesh;
  }

  public void assignMesh(ArrayMesh mesh)
  {
    this.meshChild.Mesh = mesh;
  }

  private void appendVerts(Vector3[] verts)
  {
    this.vertices.AddRange(verts);
  }

  private void appendVertsAt(Vector3[] verts, Vector3 pos)
  {
    for (int i = 0; i < verts.Length; i++)
    {
      var vert = verts[i];
      // vert.X += pos.X;
      // vert.Y += pos.Y;
      // vert.Z += pos.Z;
      vert += pos;
      verts[i] = vert;
    }
    this.vertices.AddRange(verts);
  }

  // Function to create a cube mesh
  private void appendMesh(MeshInstance3D mesh)
  {
    this.appendVerts(mesh.Mesh.GetFaces());
  }

  private void appendMeshAt(MeshInstance3D mesh, Vector3 pos)
  {
    this.appendVertsAt(mesh.Mesh.GetFaces(), pos);
  }

  private Vector3[] CalculateNormals(Vector3[] vertices)
  {
    var normals = new Vector3[vertices.Length];

    // Initialize normals to zero
    for (int i = 0; i < normals.Length; i++)
    {
      normals[i] = Vector3.Zero;
    }

    // Calculate face normals and accumulate to vertex normals
    for (int i = 0; i < vertices.Length; i += 3)
    {
      Vector3 v1 = vertices[i];
      Vector3 v2 = vertices[i + 1];
      Vector3 v3 = vertices[i + 2];

      Vector3 faceNormal = CalculateFaceNormal(v1, v2, v3);

      normals[i] -= faceNormal;
      normals[i + 1] -= faceNormal;
      normals[i + 2] -= faceNormal;
    }

    // Normalize vertex normals
    for (int i = 0; i < normals.Length; i++)
    {
      normals[i] = normals[i].Normalized();
    }

    return normals;
  }

  private static Vector3 CalculateFaceNormal(Vector3 v1, Vector3 v2, Vector3 v3)
  {
    // Cross product of two edges of the face gives the face normal
    Vector3 edge1 = v2 - v1;
    Vector3 edge2 = v3 - v1;
    return edge1.Cross(edge2).Normalized();
  }

}