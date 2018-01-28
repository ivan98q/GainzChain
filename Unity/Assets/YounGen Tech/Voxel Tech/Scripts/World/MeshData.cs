using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YounGenTech.VoxelTech {
    public class MeshData {
        static MeshData _current = null;

        public static MeshData Current {
            get { return _current; }
            set { _current = value; }
        }

        #region Properties
        public List<Color> colors { get; set; }
        public Dictionary<int, List<int>> indices { get; set; }
        public Dictionary<Material, List<int>> matIndices { get; set; }
        public List<Vector3> normals { get; set; }
        public List<Vector2> uv { get; set; }
        public List<Vector3> vertices { get; set; }

        public VectorI3 Position { get; set; }

        public bool WaitingForPickup { get; set; }
        #endregion

        public void ApplyTo(ChunkObject chunkObject, List<ChunkMeshObject> chunkMeshObjects) {
            int splits = (vertices.Count / 65532) + 1; //65532 is divisble by 4

            if(splits > 1)
                Debug.LogError(string.Format("{0} splits with {1} vertices", splits, vertices.Count), chunkObject);

            if(chunkObject.CreatedChunkMeshObjects.Count < splits)
                for(int i = 0; i < (splits - chunkObject.CreatedChunkMeshObjects.Count); i++)
                    chunkObject.AddChunkMeshObject();

            for(int i = 0; i < splits && i < 1; i++) {
                var chunkMeshObject = chunkObject.CreatedChunkMeshObjects[i];

                chunkMeshObject.PrepareMesh();

                if(splits == 1) {
                    chunkMeshObject.ChunkMesh.SetVertices(vertices);
                    //chunkMeshObject.ChunkMesh.SetColors(colors);
                    chunkMeshObject.ChunkMesh.SetNormals(normals);

                    //if(indices != null)
                    //    foreach(var index in indices)
                    //        chunkMeshObject.ChunkMesh.SetIndices(index.Value.ToArray(), MeshTopology.Triangles, index.Key);

                    if(matIndices != null) {
                        int iterator = 0;

                        chunkMeshObject.ChunkMesh.subMeshCount = matIndices.Count;
                        //int vertexCount = vertices.Count;

                        foreach(var index in matIndices) {
                            //int highestCount = -1;

                            //for(int a = 0; a < index.Value.Count; a++)
                            //    if(index.Value[a] > highestCount)
                            //        highestCount = index.Value[a];

                            //Debug.Log(string.Format("Vertices {0}\nIndices {1} Material({2}) Highest {3}", vertices.Count, index.Value.Count, index.Key.name, highestCount));
                            chunkMeshObject.ChunkMesh.SetIndices(index.Value.ToArray(), MeshTopology.Triangles, iterator);
                            iterator++;
                        }
                    }

                    chunkMeshObject.ChunkMesh.SetUVs(0, uv);
                    chunkMeshObject.ChunkRenderer.sharedMaterials = matIndices.Keys.ToArray();
                }
                else {

                }

                chunkMeshObject.gameObject.SetActive(true);
            }
        }

        public void ApplyTo(Mesh mesh) {
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetNormals(normals);

            if(indices != null)
                foreach(var index in indices)
                    mesh.SetIndices(index.Value.ToArray(), MeshTopology.Triangles, index.Key);

            if(matIndices != null) {
                int i = 0;

                mesh.subMeshCount = matIndices.Count;

                foreach(var index in matIndices) {
                    mesh.SetIndices(index.Value.ToArray(), MeshTopology.Triangles, i);
                    i++;
                }
            }

            mesh.SetUVs(0, uv);
        }

        public void ApplyTo(MeshRenderer meshRenderer) {
            meshRenderer.sharedMaterials = matIndices.Keys.ToArray();
            //matIndices.Keys.CopyTo(meshRenderer.sharedMaterials, 0);
        }

        public void ClearData() {
            if(colors != null) colors.Clear();
            if(indices != null) indices.Clear();
            if(matIndices != null) matIndices.Clear();
            if(normals != null) normals.Clear();
            if(uv != null) uv.Clear();
            if(vertices != null) vertices.Clear();
        }

        public void FlagForPickup(VectorI3 position) {
            WaitingForPickup = true;
            Position = position;
        }

        public void PickedUp() {
            WaitingForPickup = false;
        }
        // TODO Remove null checks here
        public static IEnumerator WaitToPickup() {
            while(!(Current != null && Current.WaitingForPickup))
                yield return null;
        }

        public static IEnumerator WaitOnPickedUp() {
            while(Current != null && Current.WaitingForPickup)
                yield return null;
        }

        public static MeshData Setup() {
            if(Current != null) {
                Current.ClearData();
            }
            else {
                Current = new MeshData() {
                    vertices = new List<Vector3>(),
                    colors = new List<Color>(),
                    normals = new List<Vector3>(),
                    //indices = new Dictionary<int, List<int>>(),
                    //indices = null,
                    matIndices = new Dictionary<Material, List<int>>(),
                    uv = new List<Vector2>()
                };
            }

            return Current;
        }
    }

    public static class CubeData {
        public static void QuickCube(MeshData meshData, Vector3 position, CubeDirectionFlag directions) {
            if(directions.HasDirection(CubeDirectionFlag.Left)) CubeFace(meshData, position, CubeDirectionFlag.Left);
            if(directions.HasDirection(CubeDirectionFlag.Right)) CubeFace(meshData, position, CubeDirectionFlag.Right);
            if(directions.HasDirection(CubeDirectionFlag.Down)) CubeFace(meshData, position, CubeDirectionFlag.Down);
            if(directions.HasDirection(CubeDirectionFlag.Up)) CubeFace(meshData, position, CubeDirectionFlag.Up);
            if(directions.HasDirection(CubeDirectionFlag.Back)) CubeFace(meshData, position, CubeDirectionFlag.Back);
            if(directions.HasDirection(CubeDirectionFlag.Forward)) CubeFace(meshData, position, CubeDirectionFlag.Forward);
        }

        public static void CubeFace(MeshData meshData, Vector3 position, CubeDirectionFlag direction) {
            int startIndex = meshData.vertices.Count;
            Quaternion rotation = Quaternion.identity;

            switch(direction) {
                case CubeDirectionFlag.Left: rotation = Quaternion.LookRotation(Vector3.left); break;
                case CubeDirectionFlag.Right: rotation = Quaternion.LookRotation(Vector3.right); break;
                case CubeDirectionFlag.Down: rotation = Quaternion.LookRotation(Vector3.down); break;
                case CubeDirectionFlag.Up: rotation = Quaternion.LookRotation(Vector3.up); break;
                case CubeDirectionFlag.Back: rotation = Quaternion.LookRotation(Vector3.back); break;
                case CubeDirectionFlag.Forward: rotation = Quaternion.LookRotation(Vector3.forward); break;
            }

            meshData.colors.AddRange(new Color[] {
            Color.white,
            Color.white,
            Color.white,
            Color.white
        });

            meshData.vertices.AddRange(new Vector3[] {
            (rotation * new Vector3(-.5f, -.5f, .5f)) + position,
            (rotation * new Vector3(.5f, -.5f, .5f)) + position,
            (rotation * new Vector3(-.5f, .5f, .5f)) + position,
            (rotation * new Vector3(.5f, .5f, .5f)) + position,
        });


            meshData.uv.AddRange(new Vector2[] {
            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(0,1),
            new Vector2(1,1)
        });

            Vector3 normal = direction.ToDirectionVector();

            meshData.normals.AddRange(new Vector3[] { normal, normal, normal, normal });

            //if(!meshData.indices.ContainsKey((int)direction))
            //    meshData.indices[(int)direction] = new List<int>();

            //List<int> indices = meshData.indices[(int)direction];        

            if(!meshData.indices.ContainsKey(0))
                meshData.indices[0] = new List<int>();

            List<int> indices = meshData.indices[0];

            indices.AddRange(new int[] {
            startIndex + 0,
            startIndex + 1,
            startIndex + 2,
            startIndex + 3,
            startIndex + 2,
            startIndex + 1,
        });
        }
    }
}