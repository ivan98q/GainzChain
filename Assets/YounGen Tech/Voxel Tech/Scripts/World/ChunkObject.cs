using System;
using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.VoxelTech {
    public class ChunkObject : MonoBehaviour {

        static Material debugMaterial;

        [SerializeField]
        ChunkMeshObject _prefabChunkMeshObject;

        List<ChunkMeshObject> _createdChunkMeshObjects = new List<ChunkMeshObject>();
        //List<BlockBounds> gizmos = new List<BlockBounds>();

        #region Properties
        public Chunk ChunkData { get; set; }

        public World InWorld { get; set; }

        public List<ChunkMeshObject> CreatedChunkMeshObjects {
            get { return _createdChunkMeshObjects; }
        }
        #endregion

        public void ApplyMeshData(MeshData meshData) {
            //meshData.ApplyTo(ChunkMesh);
            //meshData.ApplyTo(ChunkRenderer);
            meshData.ApplyTo(this, CreatedChunkMeshObjects);
            meshData.PickedUp();

            //ChunkMeshFilter.sharedMesh = ChunkMesh;
        }

        //public void CalculateMesh() {
        //    var meshData = SetupMeshData();

        //    if(meshData == null) return;

        //    ChunkGenerationMethodSpan(meshData);
        //    //ApplyMeshData(meshData);

        //    //ChunkMesh.RecalculateNormals();
        //}

        public void ChunkGenerationMethodBlock(MeshData meshData) {
            for(int i = 0; i < ChunkData.TotalSize; i++) {
                var block = GetBlock(i);
                var currentBlockData = InWorld.BlockDatabaseAsset.GetBlockData(block.ID);

                if(currentBlockData.BuildType == BlockData.BlockBuildType.BlockObject) {
                    var gameObject = Instantiate(currentBlockData.OriginalBlockObject.gameObject, transform.position + i.FlatTo3DIndex(ChunkData.Size) - (ChunkData.Size * .5f), Quaternion.identity, transform) as GameObject;
                    var blockObject = gameObject.GetComponent<BlockObject>();

                    blockObject.BuildBlockObject(InWorld, GetBlockNeighbors(i, CubeDirectionFlag.Faces).ToArray());
                }
                else if(currentBlockData.BuildType == BlockData.BlockBuildType.Material) {
                    if(currentBlockData.IsSolid) {
                        var blockPosition = i.FlatTo3DIndex(ChunkData.Size);
                        var generateMeshDirections = CubeDirectionFlag.None;
                        var blockNeighbors = GetBlockNeighbors(i, CubeDirectionFlag.Faces);

                        foreach(var blockNeighbor in blockNeighbors) {
                            var data = InWorld.BlockDatabaseAsset.GetBlockData(blockNeighbor.block.ID);

                            if(!data.IsSolid || !data.IsOpaque)
                                generateMeshDirections |= blockNeighbor.direction;
                        }

                        if(generateMeshDirections != CubeDirectionFlag.None)
                            CubeData.QuickCube(meshData, blockPosition - (ChunkData.Size * .5f) + (Vector3.one * .5f), generateMeshDirections);
                    }
                }
            }
        }

        public void ChunkGenerationMethodSpan(MeshData meshData) {
            //var islands = ChunkData.SpanBlocks();

            //foreach(var island in islands) {
            //    int startIndex = meshData.vertices.Count;

            //    meshData.colors.AddRange(island.GetColorArray(Color.white));
            //    meshData.vertices.AddRange(island.GetVertexArray());
            //    meshData.uv.AddRange(island.GetUVArray());
            //    meshData.normals.AddRange(island.GetNormalsArray());

            //    var blockData = InWorld.BlockDatabaseAsset.GetBlockData(island.block.ID);
            //    var blockFaceMaterial = blockData.GetFaceMaterial(island.faceIndex);

            //    if(!meshData.matIndices.ContainsKey(blockFaceMaterial))
            //        meshData.matIndices[blockFaceMaterial] = new List<int>();

            //    List<int> indices = meshData.matIndices[blockFaceMaterial];

            //    indices.AddRange(island.GetIndexArray(startIndex, !island.counterClockwise));
            //}
        }

        public Block GetBlock(int index) {
            return ChunkData.GetBlock(index);
        }
        public Block GetBlock(VectorI3 index) {
            return ChunkData.GetBlock(index);
        }

        public VectorI3 GetBlockLocalPosition(int index) {
            return ChunkData.GetBlockLocalPosition(index);
        }

        public Block GetBlockNeighbor(int index, CubeDirectionFlag direction) {
            return ChunkData.GetBlockNeighbor(index, direction);
        }
        public Block GetBlockNeighbor(int index, VectorI3 direction) {
            return ChunkData.GetBlockNeighbor(index, direction);
        }

        public List<BlockNeighbor> GetBlockNeighbors(int index, CubeDirectionFlag directions) {
            return ChunkData.GetBlockNeighbors(index, directions);
        }

        public ChunkObject GetChunkNeighbor(CubeDirectionFlag direction) {
            return InWorld.GetChunkObjectNeighbor(ChunkData.Position, direction);
        }

        public List<ChunkNeighbor> GetChunkObjectNeighbors(VectorI3 position, CubeDirectionFlag directions) {
            return InWorld.GetChunkObjectNeighbors(ChunkData.Position, directions);
        }

        public VectorI3 GetBlockWorldPosition(int index) {
            return ChunkData.GetBlockWorldPosition(index);
        }

        //void OnGUI() {
        //    GUILayout.Space(100);
        //    GUILayout.BeginVertical();
        //    {
        //        foreach(var island in gizmos) {
        //            GUILayout.Label("Size " + island.bounds.size);
        //        }
        //    }
        //    GUILayout.EndVertical();
        //}

        //void OnRenderObject() {
        //    return;
        //    CreateDebugMaterial();
        //    debugMaterial.SetPass(0);

        //    //GL.PushMatrix();
        //    //GL.MultMatrix(transform.localToWorldMatrix);

        //    GL.Begin(GL.QUADS);
        //    int index = 0;
        //    foreach(var island in gizmos) {
        //        UnityEngine.Random.seed = index;
        //        Color randomColor = UnityEngine.Random.ColorHSV(0, 1, .1f, 1);
        //        randomColor.a = .9f;
        //        GL.Color(randomColor);
        //        Vector3 offset = island.normal * .01f;

        //        GL.Vertex(island.VertexBottomLeft + offset);
        //        GL.Vertex(island.VertexTopLeft + offset);
        //        GL.Vertex(island.VertexTopRight + offset);
        //        GL.Vertex(island.VertexBottomRight + offset);

        //        index++;
        //    }
        //    GL.End();

        //    //GL.PopMatrix();
        //}

        void OnDrawGizmos() {
            //int index = 0;
            //foreach(var island in gizmos) {
            //    UnityEngine.Random.seed = index;
            //    Color randomColor = UnityEngine.Random.ColorHSV(0, 1, .1f, 1);
            //    Gizmos.color = randomColor;
            //    Gizmos.DrawWireCube(island.bounds.center, island.bounds.size);
            //    Gizmos.color = new Color(randomColor.r, randomColor.g, randomColor.b, 1);
            //    Gizmos.DrawCube(island.bounds.center, island.bounds.size + (Vector3.one * .01f));
            //    index++;
            //}
            return;
            if(!Application.isPlaying) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, ChunkData.Size);
        }

        public ChunkMeshObject AddChunkMeshObject() {
            //var gameObject = Instantiate(_prefabChunkMeshObject, transform.position, Quaternion.identity);
            var gameObject = Instantiate(_prefabChunkMeshObject, transform, false);
            var chunkMeshObject = gameObject.GetComponent<ChunkMeshObject>();

            CreatedChunkMeshObjects.Add(chunkMeshObject);

            return chunkMeshObject;
        }

        public void ClearChunkMeshObjects() {
            foreach(var chunkMeshObject in CreatedChunkMeshObjects)
                Destroy(chunkMeshObject.gameObject);

            CreatedChunkMeshObjects.Clear();
        }

        public void PrepareGeneration() {
            SetDirty();
            SetNeighborsDirty();
        }

        public void SetActive(bool value) {
            gameObject.SetActive(value);
        }

        public void SetBlock(int index, Block block) {
            ChunkData.SetBlock(index, block);
        }

        public void SetDirty() {
            ChunkData.SetDirty();
        }

        public void SetNeighborsDirty() {
            InWorld.ForEachChunkNeighbor(ChunkData.Position, CubeDirectionFlag.All, SetNeighborDirtyMethod);
        }
        public void SetNeighborsDirty(VectorI3 blockPosition) {
            for(int i = 0; i < 26; i++) {
                var newDirection = i.ToCubeDirection();
                var chunkObject = InWorld.GetChunkObjectFromBlockDirection(blockPosition, newDirection);

                if(chunkObject)
                    chunkObject.SetDirty();
            }
        }

        void SetNeighborDirtyMethod(ChunkNeighbor chunk) {
            chunk.chunkObject.SetDirty();
        }


        static void CreateDebugMaterial() {
            if(!debugMaterial) {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                debugMaterial = new Material(shader);
                debugMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                debugMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                debugMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                debugMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                debugMaterial.SetInt("_ZWrite", 0);
            }
        }
    }

    internal struct SpanData {
        public CubeDirectionFlag drawableFaces;
        public Block block;

        public SpanData(CubeDirectionFlag drawableFaces, Block block) {
            this.drawableFaces = drawableFaces;
            this.block = block;
        }
    }
}