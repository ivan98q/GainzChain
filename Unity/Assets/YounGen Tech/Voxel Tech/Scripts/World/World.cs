using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace YounGenTech.VoxelTech {
    public class World : MonoBehaviour {

        [SerializeField]
        WorldThreading _worldThreadingComponent;

        [SerializeField]
        VectorI3 _defaultWorldSize = new VectorI3(3, 3, 3);

        [SerializeField]
        ChunkSpawnStyle _chunkSpawning = ChunkSpawnStyle.Square;

        [SerializeField]
        VectorI3 _defaultChunkSize = new VectorI3(8, 8, 8);

        [SerializeField]
        GameObject _chunkObjectPrefab;

        [SerializeField]
        Transform _spawnPosition;

        [SerializeField]
        int _poolChunksAtStart = 30;

        [SerializeField]
        BlockDatabase _blockDatabaseAsset;

        [SerializeField]
        float _dropoffChunkDistance = 10;

        [SerializeField]
        bool _hideChunks = false;

        //[SerializeField]
        //AnimationCurve anim = new AnimationCurve();

        Queue<ChunkObject> _chunkPool = new Queue<ChunkObject>();
        Dictionary<VectorI3, ChunkObject> _activeChunks = new Dictionary<VectorI3, ChunkObject>();
        List<ChunkObject> _chunksToRemove = new List<ChunkObject>();

        List<VectorI3> loadChunkBlockList = new List<VectorI3>();
        Queue<VectorI3> loadChunkBlockQueue = new Queue<VectorI3>();
        //Queue<VectorI3> applyQueue = new Queue<VectorI3>();
        Chunk chunkGenerating;

        Coroutine applyCoroutine = null;
        Coroutine loadChunkBlockCoroutine = null;

        #region Properties
        public WorldCoroutine AttachedWorldCoroutine { get; private set; }

        public BlockDatabase BlockDatabaseAsset {
            get { return _blockDatabaseAsset; }
            set { _blockDatabaseAsset = value; }
        }

        public VectorI3 DefaultChunkSize {
            get { return _defaultChunkSize; }
            private set { _defaultChunkSize = value; }
        }

        public VectorI3 DefaultWorldSize {
            get { return _defaultWorldSize; }
            private set { _defaultWorldSize = value; }
        }

        public Transform SpawnPosition {
            get { return _spawnPosition; }
            private set { _spawnPosition = value; }
        }

        public float WorldChunkRadius {
            get { return Mathf.Max(DefaultWorldSize.x, DefaultWorldSize.z); }
        }

        public WorldThreading WorldThreadingComponent {
            get { return _worldThreadingComponent; }
        }
        #endregion

        void Awake() {
            AttachedWorldCoroutine = GetComponent<WorldCoroutine>();
        }

        void Start() {
            StartWorld();
        }

        //void OnGUI() {
        //    GUILayout.Space(200);
        //    GUILayout.BeginVertical();
        //    {
        //        foreach(var chunk in _activeChunks) {
        //            Vector3 finalPosition = chunk.Value.ChunkData.Center;
        //            Vector3 playerPosition = _spawnPosition.position;

        //            if(_chunkSpawning == ChunkSpawnStyle.Square) {
        //                finalPosition.y = 0;
        //                playerPosition.y = 0;
        //            }

        //            Vector3 distance = (finalPosition - playerPosition) / DefaultChunkSize;

        //            GUILayout.BeginHorizontal();
        //            {
        //                GUILayout.Label(string.Format("Distance{0}", distance));
        //                GUILayout.Toggle(Mathf.Max(Mathf.Abs(distance.x), Mathf.Abs(distance.z)) > _dropoffChunkDistance, "Dropoff");
        //            }
        //            GUILayout.EndHorizontal();
        //        }


        //        //foreach(var chunk in _activeChunks) {
        //        //    Vector3 finalPosition = chunk.Value.ChunkData.Center;
        //        //    finalPosition.y = 0;

        //        //    Vector3 playerPosition = _spawnPosition.position;
        //        //    playerPosition.y = 0;

        //        //    GUILayout.Label(string.Format("Size{0} Distance({1})", ((finalPosition - playerPosition) / DefaultChunkSize), ((finalPosition - playerPosition) / DefaultChunkSize).magnitude));
        //        //}
        //    }
        //    GUILayout.EndVertical();
        //}

        //public void AddChunkToApplyQueue(VectorI3 position) {
        //    applyQueue.Enqueue(position);
        //}

        IEnumerator ApplyChunkMeshes_Coroutine() {
            while(true) {
                yield return StartCoroutine(MeshData.WaitToPickup());

                //yield return StartCoroutine("WaitForApply");

                ApplyChunkMeshes();
                //yield return new WaitForSeconds(2);
                //yield return new WaitForEndOfFrame();
            }
        }

        public void ApplyChunkMeshes() {
            //if(applyQueue.Count == 0) return;

            //GameDebug.Current.Add(string.Format("Apply Chunk{0}", MeshData.Current.Position));

            var chunkPosition = MeshData.Current.Position;
            //var chunkPosition = applyQueue.Dequeue();
            var chunk = GetChunkObject(chunkPosition);

            if(chunk == null) {
                MeshData.Current.PickedUp();
                return;
            }

            //chunk.PrepareMesh();
            chunk.ApplyMeshData(MeshData.Current);

            chunk.SetActive(true);
            chunk.ChunkData.SetClean();
            chunk.ChunkData.IsGenerating = false;

            //GameDebug.Current.Add("Finished");
        }

        ChunkObject CreateChunkObject() {
            GameObject gameObject = Instantiate(_chunkObjectPrefab, transform, false);

            //gameObject.transform.SetParent(transform, false);

            if(_hideChunks)
                gameObject.hideFlags = HideFlags.HideInHierarchy;

            var chunkObject = gameObject.GetComponent<ChunkObject>();

            chunkObject.InWorld = this;

            return chunkObject;
        }

        public void EnqueueChunkBuild(ChunkObject chunkObject) {
            chunkObject.ChunkData.IsGenerating = true;
            AttachedWorldCoroutine.AddToQueue(chunkObject.ChunkData);
        }

        public void ForEachBlockPositionInBounds(Bounds bounds, Action<VectorI3> action) {
            VectorI3 startPosition = GetPosition(bounds.min, PositionStyle.Block);
            VectorI3 endPosition = GetPosition(bounds.max, PositionStyle.Block);
            VectorI3 size = (endPosition - startPosition) + 1;

            for(int i = 0; i < size.size; i++)
                action(startPosition + i.FlatTo3DIndex(size));
        }

        public void ForEachChunkNeighbor(VectorI3 position, CubeDirectionFlag directions, System.Action<ChunkNeighbor> action) {
            for(int i = 0; i < 26; i++)
                if(((int)directions & (1 << (i + 1))) != 0) {
                    var newDirection = i.ToCubeDirection();
                    var newPosition = position + (DirectionHelper.cubeDirections[i] * DefaultChunkSize);
                    var chunkObject = GetChunkObject(newPosition);

                    if(chunkObject)
                        action(new ChunkNeighbor(chunkObject, newDirection));
                }
        }

        IEnumerator GenerateChunkMeshes_Coroutine() {
            while(true) {
                //GenerateChunkMeshes();
                //yield return applyCoroutine;
                yield return new WaitForEndOfFrame();
            }
        }

        public void GenerateChunkMeshes() {
            //if(generateQueue.Count == 0) return;

            ////long totalTime = 0;
            ////long longestTime = long.MinValue;
            ////long shortestTime = long.MaxValue;
            ////var trackMeshGeneration = Stopwatch.StartNew();

            //var chunk = generateQueue.Dequeue();

            //chunk.CalculateMesh();
            //applyQueue.Enqueue(chunk);
            ////trackMeshGeneration.Stop();
            ////totalTime += trackMeshGeneration.ElapsedMilliseconds;

            ////if(trackMeshGeneration.ElapsedMilliseconds > longestTime)
            ////    longestTime = trackMeshGeneration.ElapsedMilliseconds;

            ////if(trackMeshGeneration.ElapsedMilliseconds < shortestTime)
            ////    shortestTime = trackMeshGeneration.ElapsedMilliseconds;

            ////chunk.SetActive(true);
            ////chunk.ChunkData.SetClean();
            ////chunk.ChunkData.IsGenerating = false;

            ////break;
            ////}

            ////if(generatedMesh)
            ////    UnityEngine.Debug.Log("Generate Chunk Meshes " + ((totalTime / (float)count)) + "ms\nLongest Time " + longestTime + "ms\nShortest Time " + shortestTime + "ms");
        }

        public Block GetBlock(VectorI3 position) {
            VectorI3 chunkPosition = GetPosition(position, PositionStyle.Chunk);

            if(_activeChunks.ContainsKey(chunkPosition))
                return _activeChunks[chunkPosition].ChunkData.GetBlock(position - chunkPosition);

            return Block.Empty;
        }

        public Block GetBlockNeighbor(VectorI3 position, CubeDirectionFlag direction) {
            return GetBlock(position + direction.ToDirectionVector());
        }

        /// <summary>
        /// Gets blocks that intersect with the bounds
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="blocks"></param>
        /// <returns>3D Size of the flat array</returns>
        public VectorI3 GetBlocksInBounds(Bounds bounds, out BlockPositionData[] blocks) {
            VectorI3 startPosition = GetPosition(bounds.min, PositionStyle.Block);
            VectorI3 endPosition = GetPosition(bounds.max, PositionStyle.Block);
            VectorI3 size = (endPosition - startPosition) + 1;

            blocks = new BlockPositionData[size.size];

            for(int i = 0; i < blocks.Length; i++) {
                VectorI3 index = i.FlatTo3DIndex(size);
                VectorI3 position = startPosition + index;

                blocks[i] = new BlockPositionData(GetBlock(position), position);
            }

            return size;
        }

        ///// <summary>
        ///// Gets intersecting solid blocks in the form of bounds
        ///// </summary>
        //public List<Bounds> GetSolidBlocksInBoundsAsBounds(Bounds bounds) {
        //    List<Bounds> list = new List<Bounds>();

        //    VectorI3 startPosition = GetPosition(bounds.min, PositionStyle.Block);
        //    VectorI3 endPosition = GetPosition(bounds.max, PositionStyle.Block);
        //    VectorI3 size = (endPosition - startPosition) + 1;

        //    for(int i = 0; i < blocks.Length; i++) {
        //        VectorI3 index = i.FlatTo3DIndex(size);
        //        VectorI3 position = startPosition + index;

        //        blocks[i] = new BlockPositionData(GetBlock(position), position);
        //    }

        //    return list;
        //}

        public List<BlockNeighbor> GetBlockNeighbors(VectorI3 position, CubeDirectionFlag directions) {
            List<BlockNeighbor> blocks = new List<BlockNeighbor>();

            for(int i = 0; i < 26; i++)
                if(((int)directions & (1 << (i + 1))) != 0) {
                    var newDirection = i.ToCubeDirection();
                    var block = GetBlockNeighbor(position, newDirection);

                    blocks.Add(new BlockNeighbor(block, newDirection));
                }

            return blocks;
        }

        public ChunkObject GetChunkObject(VectorI3 position) {
            ChunkObject chunkObject;

            _activeChunks.TryGetValue(position, out chunkObject);

            return chunkObject;
        }

        public ChunkObject GetChunkObjectFromBlockDirection(VectorI3 position, CubeDirectionFlag direction) {
            return GetChunkObject(position + direction.ToDirectionVector());
        }

        public ChunkObject GetChunkObjectNeighbor(VectorI3 position, CubeDirectionFlag direction) {
            return GetChunkObject(position + direction.ToDirectionVector() * DefaultChunkSize);
        }

        public List<ChunkNeighbor> GetChunkObjectNeighbors(VectorI3 position, CubeDirectionFlag directions) {
            List<ChunkNeighbor> chunks = new List<ChunkNeighbor>();

            for(int i = 0; i < 26; i++)
                if(((int)directions & (1 << (i + 1))) != 0) {
                    var newDirection = i.ToCubeDirection();
                    var newPosition = position + (DirectionHelper.cubeDirections[i] * DefaultChunkSize);
                    var chunkObject = GetChunkObject(newPosition);

                    if(chunkObject)
                        chunks.Add(new ChunkNeighbor(chunkObject, newDirection));
                }

            return chunks;
        }

        public Vector3 GetPosition(Vector3 position, PositionStyle style, Pivot pivot = Pivot.Corner) {
            switch(style) {
                case PositionStyle.Chunk: return VectorI3.GridFloor(position, DefaultChunkSize) + (pivot == Pivot.Center ? DefaultChunkSize * .5f : Vector3.zero);
                default: return VectorI3.GridFloor(position, VectorI3.one) + (pivot == Pivot.Center ? new Vector3(.5f, .5f, .5f) : Vector3.zero);
            }
        }

        IEnumerator LoadChunkBlocks() {
            while(true) {
                if(loadChunkBlockList.Count > 0) {
                    //if(loadChunkBlockQueue.Count > 0) {
                    //loadChunkBlockQueue = new Queue<VectorI3>(loadChunkBlockQueue.OrderBy(s => { return (SpawnPosition.position - (s + (DefaultChunkSize * .5f))).sqrMagnitude; }));

                    VectorI3 chunkPosition = loadChunkBlockList[0];
                    float lowestDistance = Mathf.Infinity;
                    int index = 0;

                    for(int i = 0; i < loadChunkBlockList.Count; i++) {
                        var position = loadChunkBlockList[i];

                        //foreach(var position in loadChunkBlockList) {
                        float currentDistance = (SpawnPosition.position - (position + (DefaultChunkSize * .5f))).sqrMagnitude;

                        if(currentDistance < lowestDistance) {
                            lowestDistance = currentDistance;
                            chunkPosition = position;
                            index = i;
                        }
                    }

                    loadChunkBlockList.RemoveAt(index);

                    //var chunkPosition = loadChunkBlockQueue.Dequeue();
                    var chunkObject = GetChunkObject(chunkPosition);

                    chunkObject.ChunkData.LoadBlocks(chunkObject.transform.position, NoiseDensity);
                    chunkObject.PrepareGeneration();

                    yield return StartCoroutine("WaitForChunkToGenerate", chunkObject.ChunkData);
                }
                else yield return null;
            }
        }

        public Block NoiseDensity(Vector3 position) {
            float density = SimplexNoise.Noise(position.x * .066f, position.y * .066f, position.z * .066f) * 20.4f;
            float density2 = SimplexNoise.Noise(position.x * .066f, position.y * .066f, position.z * .066f) * 100.4f;
            float density3 = SimplexNoise.Noise(position.x * .066f, position.y * .066f, position.z * .066f) * 50.5f;
            //float density = UnityEngine.Random.Range(-1f, 1f) * 20.4f * 10000;
            //float density2 = UnityEngine.Random.Range(-1f, 1f) * 100.4f * 10000;
            //float density3 = UnityEngine.Random.Range(-1f, 1f) * 50.5f * 10000;

            density += 20;
            density -= Mathf.Max(density2, 0);
            density = position.y - density;

            density3 += 20;
            density3 -= Mathf.Max(density2, 0);
            density3 = position.y - density3;

            //if(density < 0) return new Block((byte)BlockDatabaseAsset.GetBlockData("Grass").ID);
            //else if(density3 <= 0) return new Block((byte)BlockDatabaseAsset.GetBlockData("Stone").ID);
            //else return new Block((byte)BlockDatabaseAsset.GetBlockData("Air").ID);
            return new Block((byte)BlockDatabaseAsset.GetBlockData("Stone").ID);
            //if(density <= 0) return new Block((byte)BlockDatabaseAsset.GetBlockData("Tile").ID);
            //else return new Block((byte)BlockDatabaseAsset.GetBlockData("Air").ID);
        }

        //void OnDrawGizmos() {
        //    return;
        //    Gizmos.color = Color.blue;

        //    switch(_chunkSpawning) {
        //        case ChunkSpawnStyle.Square:
        //            //Gizmos.DrawWireCube(GetPosition(new Vector3(_spawnPosition.position.x, 0, _spawnPosition.position.z) + (new Vector3(DefaultChunkSize.x, 0, DefaultChunkSize.z) * .5f), PositionStyle.Chunk, Pivot.Center) - new Vector3(0, DefaultChunkSize.y*.5f, 0), (DefaultWorldSize * DefaultChunkSize) + Vector3.one * .01f);
        //            Gizmos.DrawWireCube(VectorI3.GridFloor(_spawnPosition.position, DefaultChunkSize) + new Vector3(DefaultChunkSize.x, 0, DefaultChunkSize.z) * .5f, (DefaultWorldSize * DefaultChunkSize) + Vector3.one * .01f);
        //            break;

        //        case ChunkSpawnStyle.Cube:
        //            //Gizmos.DrawWireCube(GetPosition(new Vector3(_spawnPosition.position.x, 0, _spawnPosition.position.z) + (new Vector3(DefaultChunkSize.x, 0, DefaultChunkSize.z) * .5f), PositionStyle.Chunk, Pivot.Center) - new Vector3(0, DefaultChunkSize.y*.5f, 0), (DefaultWorldSize * DefaultChunkSize) + Vector3.one * .01f);
        //            Gizmos.DrawWireCube(VectorI3.GridFloor(_spawnPosition.position, DefaultChunkSize) + DefaultChunkSize * .5f, (DefaultWorldSize * DefaultChunkSize) + Vector3.one * .01f);
        //            break;

        //        case ChunkSpawnStyle.Circle:
        //            Gizmos.DrawWireSphere(VectorI3.GridFloor(_spawnPosition.position, DefaultChunkSize) + new Vector3(DefaultChunkSize.x, 0, DefaultChunkSize.z) * .5f, (WorldChunkRadius * Mathf.Max(DefaultChunkSize.x, DefaultChunkSize.z)) * .5f);
        //            break;

        //        case ChunkSpawnStyle.Sphere:
        //            Gizmos.DrawWireSphere(VectorI3.GridFloor(_spawnPosition.position, DefaultChunkSize) + DefaultChunkSize * .5f, (WorldChunkRadius * Mathf.Max(DefaultChunkSize.x, DefaultChunkSize.y, DefaultChunkSize.z)) * .5f);
        //            break;
        //    }
        //}

        public void PoolChunks() {
            for(int i = 0; i < _poolChunksAtStart; i++)
                _chunkPool.Enqueue(CreateChunkObject());
        }

        public void RepoolChunk(VectorI3 position) {
            ChunkObject chunkObject;

            if(_activeChunks.TryGetValue(position, out chunkObject))
                RepoolChunk(chunkObject);
        }

        public void RepoolChunk(ChunkObject chunkObject) {
            if(_activeChunks.ContainsKey(chunkObject.ChunkData.Position)) {
                chunkObject.SetActive(false);
                chunkObject.ClearChunkMeshObjects();
                chunkObject.ChunkData.HasBeenRemoved = true;
                chunkObject.ChunkData.ClearBlockArray();
                _chunkPool.Enqueue(chunkObject);
                _chunksToRemove.Add(chunkObject);
            }
        }

        public void SetBlock(VectorI3 position, Block block) {
            VectorI3 chunkPosition = GetPosition(position, PositionStyle.Chunk);

            if(_activeChunks.ContainsKey(chunkPosition))
                _activeChunks[chunkPosition].ChunkData.SetBlock(position - chunkPosition, block);
        }

        public ChunkObject SpawnChunk(Vector3 position) {
            ChunkObject chunkObject = _chunkPool.Dequeue();

            if(chunkObject) {
                if(chunkObject.ChunkData == null)
                    chunkObject.ChunkData = new Chunk(this, DefaultChunkSize);
                else
                    chunkObject.ChunkData.ResetBlockArray();

                chunkObject.transform.position = position + (DefaultChunkSize * .5f);
                chunkObject.ChunkData.Position = position;

                //loadChunkBlockQueue.Enqueue(chunkObject.ChunkData.Position);
                loadChunkBlockList.Add(chunkObject.ChunkData.Position);

                //chunkObject.ChunkData.LoadBlocks(chunkObject.transform.position, NoiseDensity);
                //chunkObject.PrepareGeneration();
            }

            return chunkObject;
        }

        public void SpawnChunks() {
            //if(_activeChunks.Count > 0) return;

            Vector3 spawnPosition = _spawnPosition.position;

            switch(_chunkSpawning) {
                case ChunkSpawnStyle.Square:
                    spawnPosition.y = 0;

                    for(int i = 0; i < DefaultWorldSize.size; i++) {
                        VectorI3 position = i.FlatTo3DIndex(DefaultWorldSize) * DefaultChunkSize;
                        VectorI3 realPosition = VectorI3.GridFloor(spawnPosition + (DefaultChunkSize * .5f) + position - (DefaultChunkSize * DefaultWorldSize * .5f), DefaultChunkSize);

                        if(!_activeChunks.ContainsKey(realPosition))
                            _activeChunks[realPosition] = SpawnChunk(realPosition);
                    }

                    break;

                case ChunkSpawnStyle.Cube:
                    for(int i = 0; i < DefaultWorldSize.size; i++) {
                        VectorI3 position = i.FlatTo3DIndex(DefaultWorldSize) * DefaultChunkSize;
                        VectorI3 realPosition = VectorI3.GridFloor(spawnPosition + (DefaultChunkSize * .5f) + position - (DefaultChunkSize * DefaultWorldSize * .5f), DefaultChunkSize);

                        if(!_activeChunks.ContainsKey(realPosition))
                            _activeChunks[realPosition] = SpawnChunk(realPosition);
                    }

                    break;

                case ChunkSpawnStyle.Circle: {
                        float radius = WorldChunkRadius * Mathf.Max(DefaultChunkSize.x, DefaultChunkSize.z) * .5f;
                        float doubleRadius = radius * radius;

                        spawnPosition.y = 0;

                        for(int i = 0; i < DefaultWorldSize.size; i++) {
                            VectorI3 position = i.FlatTo3DIndex(DefaultWorldSize) * DefaultChunkSize;
                            VectorI3 realPosition = VectorI3.GridFloor(spawnPosition + (DefaultChunkSize * .5f) + position - (DefaultChunkSize * DefaultWorldSize * .5f), DefaultChunkSize);
                            Vector3 finalPosition = new Vector3(position.x, 0, position.z) + new Vector3(DefaultChunkSize.x * .5f, 0, DefaultChunkSize.z * .5f) - (new VectorI3(DefaultWorldSize.x, 0, DefaultWorldSize.z) * new VectorI3(DefaultChunkSize.x, 0, DefaultChunkSize.z) * .5f);

                            //if(finalPosition.magnitude <= (WorldChunkRadius * Mathf.Max(DefaultChunkSize.x, DefaultChunkSize.z) * .5f))
                            //    UnityEngine.Debug.DrawRay(realPosition + DefaultChunkSize * .5f, Vector3.up * DefaultChunkSize.y, Color.blue, 10, false);

                            if(!_activeChunks.ContainsKey(realPosition))
                                if(finalPosition.sqrMagnitude <= doubleRadius)
                                    _activeChunks[realPosition] = SpawnChunk(realPosition);
                        }

                        break;
                    }

                case ChunkSpawnStyle.Sphere: {
                        float radius = WorldChunkRadius * Mathf.Max(DefaultChunkSize.x, DefaultChunkSize.y, DefaultChunkSize.z) * .5f;
                        float doubleRadius = radius * radius;

                        spawnPosition.y = 0;

                        for(int i = 0; i < DefaultWorldSize.size; i++) {
                            VectorI3 position = i.FlatTo3DIndex(DefaultWorldSize) * DefaultChunkSize;
                            VectorI3 realPosition = VectorI3.GridFloor(spawnPosition + (DefaultChunkSize * .5f) + position - (DefaultChunkSize * DefaultWorldSize * .5f), DefaultChunkSize);
                            Vector3 finalPosition = position + (DefaultChunkSize * .5f) - (DefaultWorldSize * DefaultChunkSize * .5f);

                            //if(finalPosition.magnitude <= (WorldChunkRadius * Mathf.Max(DefaultChunkSize.x, DefaultChunkSize.z) * .5f))
                            //    UnityEngine.Debug.DrawRay(realPosition + DefaultChunkSize * .5f, Vector3.up * DefaultChunkSize.y, Color.blue, 10, false);

                            if(!_activeChunks.ContainsKey(realPosition))
                                if(finalPosition.sqrMagnitude <= doubleRadius)
                                    _activeChunks[realPosition] = SpawnChunk(realPosition);
                        }

                        break;
                    }
            }
        }

        public void StartWorld() {
            PoolChunks();

            //StartCoroutine("GenerateChunkMeshes_Coroutine");
            applyCoroutine = StartCoroutine("ApplyChunkMeshes_Coroutine");
            loadChunkBlockCoroutine = StartCoroutine("LoadChunkBlocks");

            StartCoroutine("UpdateCoroutine");
            //SpawnChunks();
            //GenerateChunkMeshes();
        }

        IEnumerator UpdateCoroutine() {
            while(true) {
                //float radius = WorldChunkRadius * Mathf.Max(DefaultChunkSize.x, DefaultChunkSize.z) * .5f;
                float dropoffDistance = _dropoffChunkDistance * _dropoffChunkDistance;

                foreach(var chunk in _activeChunks) {
                    Vector3 finalPosition = chunk.Value.ChunkData.Center;
                    Vector3 playerPosition = _spawnPosition.position;

                    switch(_chunkSpawning) {
                        case ChunkSpawnStyle.Square:
                        case ChunkSpawnStyle.Cube:
                            if(_chunkSpawning == ChunkSpawnStyle.Square) {
                                finalPosition.y = 0;
                                playerPosition.y = 0;
                            }

                            Vector3 distance = (finalPosition - playerPosition) / DefaultChunkSize;
                            float[] compare = _chunkSpawning == ChunkSpawnStyle.Square ?
                                new float[] { Mathf.Abs(distance.x), Mathf.Abs(distance.z) } :
                                new float[] { Mathf.Abs(distance.x), Mathf.Abs(distance.y), Mathf.Abs(distance.z) };

                            if(Mathf.Max(compare) > _dropoffChunkDistance)
                                RepoolChunk(chunk.Value);

                            break;

                        case ChunkSpawnStyle.Circle:
                        case ChunkSpawnStyle.Sphere:
                            if(_chunkSpawning == ChunkSpawnStyle.Circle) {
                                finalPosition.y = 0;
                                playerPosition.y = 0;
                            }

                            if(((finalPosition - playerPosition) / DefaultChunkSize).sqrMagnitude > dropoffDistance)
                                RepoolChunk(chunk.Value);

                            break;
                    }

                    if(!chunk.Value.ChunkData.HasBeenRemoved)
                        if(chunk.Value.ChunkData.HasChanged && !chunk.Value.ChunkData.IsGenerating)
                            EnqueueChunkBuild(chunk.Value);
                }

                if(_chunksToRemove.Count > 0) {
                    foreach(var chunk in _chunksToRemove)
                        _activeChunks.Remove(chunk.ChunkData.Position);

                    _chunksToRemove.Clear();
                }

                SpawnChunks();
                //GenerateChunkMeshes();

                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator WaitForChunkToGenerate(Chunk chunk) {
            while(chunk.IsGenerating) {
                if(chunk.HasBeenRemoved) yield break;

                yield return null;
            }
        }

        //IEnumerator WaitForApply() {
        //    while(applyQueue.Count == 0)
        //        yield return null;
        //}

        //void Update() {
        //    anim.AddKey(Time.time, SimplexNoise.Noise(Time.time * .066f * 4, 0, 0) * 2.40f);
        //}
    }

    public struct ChunkNeighbor {
        public ChunkObject chunkObject;
        public CubeDirectionFlag direction;

        public ChunkNeighbor(ChunkObject chunkObject, CubeDirectionFlag direction) {
            this.chunkObject = chunkObject;
            this.direction = direction;
        }
    }

    public enum Pivot {
        Corner,
        Center
    }

    public enum PositionStyle {
        Chunk,
        Block
    }

    public enum ChunkSpawnStyle {
        Square,
        Circle,
        Cube,
        Sphere
    }
}