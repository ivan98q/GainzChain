using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YounGenTech.VoxelTech {
    public class WorldCoroutine : MonoBehaviour {

        public static WorldCoroutine Current { get; private set; }

        [SerializeField]
        BlockDatabase _blockDatabaseAsset;

        [SerializeField]
        Material _defaultMaterial;

        Dictionary<VectorI3, ChunkData> chunkDataDictionary = new Dictionary<VectorI3, ChunkData>();
        List<ChunkData> chunkData = new List<ChunkData>();
        Queue<Chunk> buildQueue = new Queue<Chunk>();

        #region Properties
        public World AttachedWorld { get; set; }

        public BlockDatabase BlockDatabaseAsset {
            get { return _blockDatabaseAsset; }
            set { _blockDatabaseAsset = value; }
        }

        public Vector3 Center {
            get { return AttachedWorld.SpawnPosition.position; }
        }

        public VectorI3 DefaultChunkSize { get; set; }
        #endregion

        void OnEnable() {
            Current = this;
        }

        void Awake() {
            AttachedWorld = GetComponent<World>();
            DefaultChunkSize = AttachedWorld.DefaultChunkSize;

            chunkData = new List<ChunkData>();

            for(int i = 0; i < 27; i++)
                chunkData.Add(new ChunkData(this, DefaultChunkSize));
        }

        void Start() {
            //GameDebug.Current.Add("", () => { return string.Format("Build Queue {0}", buildQueue.Count); });
            //GameDebug.Current.Add("", () => { return string.Format("Chunk Dictionary {0}", chunkDataDictionary.Count); });
            //GameDebug.Current.Add("", () => { return MeshData.Current != null ? string.Format("Waiting For Pickup {0}", MeshData.Current.WaitingForPickup) : "No MeshData"; });

            StartCoroutine("BuildCoroutine");
        }

        public void AddToQueue(Chunk chunk) {
            //GameDebug.Current.Add(string.Format("Enqueing Chunk{0}", chunk.Position));

            buildQueue.Enqueue(chunk);
        }

        IEnumerator BuildCoroutine() {
            while(true) {
                if(buildQueue.Count != 0) {
                    DequeueNextChunk();

                    yield return StartCoroutine("WaitForChunks");

                    //GameDebug.Current.Add("Starting Build");

                    yield return StartCoroutine("StartBuild");
                }

                yield return new WaitForEndOfFrame();
            }
        }

        void Cleanup() {
            chunkDataDictionary.Clear();
        }

        void DequeueNextChunk() {
            Chunk chunk = null;

            if(buildQueue.Count == 0) return;

            buildQueue = new Queue<Chunk>(buildQueue.OrderBy(s => Vector3.Distance(s.Position + s.Size * .5f, Center)));

            while(buildQueue.Count > 0) {
                chunk = buildQueue.Dequeue();

                if(!chunk.HasBeenRemoved) break;
            }

            if(chunk == null) return;
            
            List<Chunk> setChunks = new List<Chunk>();

            setChunks.Add(chunk);
            
            foreach(var neighbor in chunk.InWorld.GetChunkObjectNeighbors(chunk.Position, CubeDirectionFlag.All)) 
                setChunks.Add(neighbor.chunkObject.ChunkData);

            SetChunks(setChunks.ToArray());
        }

        Block GetBlock(VectorI3 position) {
            VectorI3 chunkPosition = GetPosition(position, PositionStyle.Chunk);

            if(chunkDataDictionary.ContainsKey(chunkPosition))
                return chunkDataDictionary[chunkPosition].GetBlock(position - chunkPosition);

            return Block.Empty;
        }

        public ChunkData GetChunk(VectorI3 position) {
            ChunkData chunkData;

            chunkDataDictionary.TryGetValue(position, out chunkData);

            return chunkData;
        }

        public ChunkData GetChunkNeighbor(VectorI3 position, CubeDirectionFlag direction) {
            return GetChunk(position + direction.ToDirectionVector() * DefaultChunkSize);
        }

        public List<ChunkDataNeighbor> GetChunkNeighbors(VectorI3 position, CubeDirectionFlag directions) {
            List<ChunkDataNeighbor> chunks = new List<ChunkDataNeighbor>();

            for(int i = 0; i < 26; i++)
                if(((int)directions & (1 << (i + 1))) != 0) {
                    var newDirection = i.ToCubeDirection();
                    var newPosition = position + (DirectionHelper.cubeDirections[i] * DefaultChunkSize);
                    var chunkData = GetChunk(newPosition);

                    if(chunkData != null)
                        chunks.Add(new ChunkDataNeighbor(chunkData, newDirection));
                }

            return chunks;
        }

        Vector3 GetPosition(Vector3 position, PositionStyle style, Pivot pivot = Pivot.Corner) {
            switch(style) {
                case PositionStyle.Chunk: return VectorI3.GridFloor(position, DefaultChunkSize) + (pivot == Pivot.Center ? DefaultChunkSize * .5f : Vector3.zero);
                default: return VectorI3.GridFloor(position, VectorI3.one) + (pivot == Pivot.Center ? new Vector3(.5f, .5f, .5f) : Vector3.zero);
            }
        }

        void SetChunks(params Chunk[] chunks) {
            chunkDataDictionary.Clear();

            for(int i = 0; i < 27 && i < chunks.Length; i++) {
                chunkData[i].SetChunkData(chunks[i]);
                chunkDataDictionary[chunkData[i].Position] = chunkData[i];
            }
        }

        IEnumerator StartBuild() {
            var chunk = chunkData[0];
            var islands = chunk.SpanBlocks(BlockDatabaseAsset);

            yield return StartCoroutine(MeshData.WaitOnPickedUp());
            //yield return new WaitForSeconds(1); // TODO Remove this

            var meshData = MeshData.Setup();

            foreach(var island in islands) {
                int startIndex = meshData.vertices.Count;

                //meshData.colors.AddRange(island.GetColorArray(Color.white));
                //meshData.vertices.AddRange(island.GetVertexArray());
                //meshData.uv.AddRange(island.GetUVArray());
                //meshData.normals.AddRange(island.GetNormalsArray());

                island.AddVertexArray(meshData.vertices);
                island.AddUVArray(meshData.uv);
                island.AddNormalsArray(meshData.normals);

                var blockData = BlockDatabaseAsset.GetBlockData(island.block.ID);
                var blockFaceMaterial = blockData.GetFaceMaterial(island.faceIndex);
                //var blockFaceMaterial = _defaultMaterial;

                if(!meshData.matIndices.ContainsKey(blockFaceMaterial))
                    meshData.matIndices[blockFaceMaterial] = new List<int>();

                List<int> indices = meshData.matIndices[blockFaceMaterial];

                island.AddIndexArray(indices, startIndex, !island.counterClockwise);
                //indices.AddRange(island.GetIndexArray(startIndex, !island.counterClockwise));
            }

            MeshData.Current.FlagForPickup(chunk.Position);

            Cleanup();
        }

        IEnumerator WaitForBool(Func<bool> check) {
            while(check())
                yield return null;
        }

        IEnumerator WaitForChunks() {
            while(chunkDataDictionary.Count == 0)
                yield return null;
        }

        //IEnumerator WaitForPickup() {
        //    while(MeshData.Current != null && MeshData.Current.WaitingForPickup)
        //        yield return null;
        //}

        public class ChunkData {
            Block[] _blocks;
            VectorI3 _size;

            #region Properties
            public Vector3 Center {
                get { return Position + (Size * .5f); }
            }

            public WorldCoroutine InWorld { get; set; }

            public VectorI3 Position { get; set; }

            public VectorI3 Size {
                get { return _size; }
                set { _size = value; }
            }

            public int TotalSize { get; private set; }
            #endregion

            public ChunkData(WorldCoroutine world, VectorI3 size) {
                InWorld = world;
                Size = size;
            }

            public CubeDirectionFlag BlockNeighborKeep(int index, CubeDirectionFlag directions, BlockDatabase blockDatabase) {
                return Chunk.BlockNeighborKeep(index, directions, GetBlockNeighbor, s => Chunk.TestForAir(s, blockDatabase));
            }

            public Block GetBlock(int index) {
                return _blocks[index];
            }
            public Block GetBlock(VectorI3 index) {
                return _blocks[index.FlatIndex(Size)];
            }

            public VectorI3 GetBlockLocalPosition(int index) {
                return index.FlatTo3DIndex(Size);
            }

            public Block GetBlockNeighbor(int index, CubeDirectionFlag direction) {
                return GetBlockNeighbor(index, direction.ToDirectionVector());
            }
            public Block GetBlockNeighbor(int index, VectorI3 direction) {
                var position = index.FlatTo3DIndex(Size) + direction;
                int neighborIndex = position.FlatIndex(Size);

                return !position.ArrayOutOfBounds(Size) ?
                    _blocks[neighborIndex] :
                    InWorld.GetBlock(Position + position);
            }

            public void SetChunkData(Chunk chunk) {
                _blocks = chunk.CloneBlockArray();
                Position = chunk.Position;

                //GameDebug.Current.Add("Set Chunk " + Position);
            }

            public List<BlockBounds> SpanBlocks(BlockDatabase blockDatabase) {
                return Chunk.SpanBlocks(Size, blockDatabase, GetBlock, s => BlockNeighborKeep(s, CubeDirectionFlag.Faces, blockDatabase));
            }
        }

        public struct ChunkDataNeighbor {
            public ChunkData chunkData;
            public CubeDirectionFlag direction;

            public ChunkDataNeighbor(ChunkData chunkData, CubeDirectionFlag direction) {
                this.chunkData = chunkData;
                this.direction = direction;
            }
        }
    }
}