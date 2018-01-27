using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

//***Here tag contains thread detours to make them not work as threads anymore. Temporarily fixes the problem

public class World : MonoBehaviour {

	System.ComponentModel.BackgroundWorker ChunkMeshGenerationWorker = new System.ComponentModel.BackgroundWorker();

	public TQueue<ChunkData> chunkMeshQueue;

	public static World world;

	public static VectorI3 chunkSize {
		get { return world.worldData.chunkSize; }
		set { world.worldData.chunkSize = value; }
	}
	public static VectorI3 chunkDistance {
		get { return world.worldData.chunkDistance; }
		set { world.worldData.chunkDistance = value; }
	}
	public static VectorI3 worldSize {
		get { return world.worldData.worldSize; }
	}

	public static volatile Chunk[,,] chunks;
	public static byte[] blocks;
	public static int speed;

	public Material terrainMaterial;

	public WorldData worldData = new WorldData() {
		chunkSize = new VectorI3(16, 16, 16),
		chunkDistance = new VectorI3(3, 3, 3)
	};

	public delegate void MoveEventHandler(Vector3 previousPosition, Vector3 position);
	public event MoveEventHandler OnPlayerMoved;

	public Vector3 _playerPosition;
	public Vector3 playerPosition {
		get { return _playerPosition; }

		set {
			Vector3 playerPos = Chunk.Position(_playerPosition);
			bool changeLastChunk = false;

			if(Chunk.Position(value) != playerPos) changeLastChunk = true;

			_playerPosition = value;

			currentChunkPosition = Chunk.Position(_playerPosition);
			if(changeLastChunk) lastPlayerChunk = playerPos;
		}
	}

	public Vector3 currentChunkPosition;
	public Vector3 lastPlayerChunkPosition;
	public Vector3 lastPlayerChunk {
		get { return lastPlayerChunkPosition; }

		set {
			if(OnPlayerMoved != null && value != currentChunkPosition) OnPlayerMoved(value, currentChunkPosition);//playerMoved = true;
			//if(OnPlayerMoved != null) OnPlayerMoved(currentChunkPosition);//playerMoved = true;

			lastPlayerChunkPosition = value;
		}
	}

	public bool shiftingChunks;
	public bool chunksGenerating;

	public Vector3 chunkPosition;

#if UNITY_EDITOR
	public bool debugChunksGeneratingBlocks;
	public bool debugChunksGeneratingMesh;
#endif

	public World() { world = this; }

	void Awake() {
		world = this;

		chunkMeshQueue = new TQueue<ChunkData>();

		blocks = new byte[worldData.worldSize.size];
		chunks = new Chunk[chunkDistance.x, chunkDistance.y, chunkDistance.z];

		foreach(VectorI3 a in ChunkLoop()) {
			Chunk chunk = GetChunk(a);
			chunk.permaIndex = a;
			chunk.position = ArrayIndexToPosition(chunk.permaIndex);
			SetChunk(a, chunk);
		}

		GameObject mover = GameObject.Find("WorldMover");

		playerPosition = mover.transform.position;
		//lastPlayerChunk = Chunk.Position(playerPosition);
		//playerMoved = false;

		//***Here
		//StartCoroutine("ChunkCoroutineUpdate");
		//StartCoroutine("ChunkApplyMeshes");

		//BGWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(BGWorker_DoWork);
		//BGWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(BGWorker_RunWorkerCompleted);

		//BGWorker.RunWorkerAsync(vector);
	}

	void Start() {
		OnPlayerMoved += new MoveEventHandler(World_OnPlayerMoved);
		OnPlayerMoved(lastPlayerChunk, currentChunkPosition);

		ChunkMeshGenerationWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(ChunkMeshGenerationWorker_DoWork);
		ChunkMeshGenerationWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(ChunkMeshGenerationWorker_RunWorkerCompleted);
	}

	#region Chunk Generation Shiz
	void ChunkMeshGenerationWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
		//***Here
		ChunkData chunkData = (ChunkData)sender;
		//ChunkData chunkData = (ChunkData)e.Argument;


		Chunk chunk = chunkData.chunk;
		byte[] blocks = chunkData.blocks;

		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		List<Color> colors = new List<Color>();
		//List<Vector3> normals = new List<Vector3>();

		int triangleIndex = 0;

		//Debug.Log(chunk.index + " " + (chunk.localIndex));

		foreach(VectorI3 localIndex in chunk.BlockLoopNoAir()) {
			VectorI3 blockIndex = localIndex + (chunk.localIndex * chunkSize);
			int flatIndex = MathHelper.FlatIndex(blockIndex, worldSize);

			if(blocks[flatIndex] == 0) continue;

			bool[] neighbors = GetNeighbors(blockIndex);

			if(Chunk.SkipBlockLoop(neighbors)) continue;

			foreach(int f in Chunk.BlockFaceLoop(blockIndex)) {
				Vector3[] vertexVectors = MathHelper.OffsetVector3(Block.vertices, BlockHelper.faceVertices[f], Chunk.BlockLocalPosition(localIndex.x, localIndex.y, localIndex.z));

				//if(WorldManager.enableBlockDeformation) vertexVectors = DeformVertices(f, vertexVectors, deformMultiplier, neighbors);

				//vertexVectors[4] = (vertexVectors[0] + vertexVectors[1] + vertexVectors[2] + vertexVectors[3] + vertexVectors[4]) / 5f;
				vertexVectors[4] = (vertexVectors[0] + vertexVectors[1] + vertexVectors[2] + vertexVectors[3]) / 4f;

				vertices.AddRange(vertexVectors);
				triangles.AddRange(MathHelper.OffsetInt(Block.triangles * 3, BlockHelper.triangles, triangleIndex));
				uvs.AddRange(MathHelper.OffsetVector2(Block.vertices, BlockHelper.uvs, BlockUV.GetUV((BlockType)System.Enum.Parse(typeof(BlockType), blocks[flatIndex].ToString()))));

				Color blockColor = Color.Lerp(Chunk.BlockColor(blocks[flatIndex], 1), Color.white, .2f);

				Color[] ambientColors = Ambient.ChangeLight(f, neighbors);

				for(int v = 0; v < ambientColors.Length; v++) {
					//ambientColors[v] = Color.Lerp(blockColor, ambientColors[v], 0.6f);
					ambientColors[v] = blockColor * ambientColors[v];

					//	normals.Add(MathHelper.direction[f]);
				}

				colors.AddRange(ambientColors);

				triangleIndex += Block.vertices;
			}
		}

		chunkData.vertices = vertices;
		chunkData.triangles = triangles;
		chunkData.uvs = uvs;
		chunkData.colors = colors;

		ChunkMeshGenerationWorker_RunWorkerCompleted(chunkData, null);
		//e.Result = chunkData;
	}

	void ChunkMeshGeneration(ChunkData chunkData) {
		Chunk chunk = chunkData.chunk;
		byte[] blocks = chunkData.blocks;

		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		List<Color> colors = new List<Color>();
		//List<Vector3> normals = new List<Vector3>();

		int triangleIndex = 0;

		foreach(VectorI3 localIndex in chunk.BlockLoopNoAir()) {
			VectorI3 blockIndex = localIndex + (chunk.localIndex * chunkSize);
			int flatIndex = MathHelper.FlatIndex(blockIndex, worldSize);

			if(blocks[flatIndex] == 0) continue;

			bool[] neighbors = GetNeighbors(blockIndex);

			if(Chunk.SkipBlockLoop(neighbors)) continue;

			foreach(int f in Chunk.BlockFaceLoop(blockIndex)) {
				Vector3[] vertexVectors = MathHelper.OffsetVector3(Block.vertices, BlockHelper.faceVertices[f], Chunk.BlockLocalPosition(localIndex.x, localIndex.y, localIndex.z));

				//if(WorldManager.enableBlockDeformation) vertexVectors = DeformVertices(f, vertexVectors, deformMultiplier, neighbors);

				//vertexVectors[4] = (vertexVectors[0] + vertexVectors[1] + vertexVectors[2] + vertexVectors[3] + vertexVectors[4]) / 5f;
				vertexVectors[4] = (vertexVectors[0] + vertexVectors[1] + vertexVectors[2] + vertexVectors[3]) / 4f;

				vertices.AddRange(vertexVectors);
				triangles.AddRange(MathHelper.OffsetInt(Block.triangles * 3, BlockHelper.triangles, triangleIndex));
				uvs.AddRange(MathHelper.OffsetVector2(Block.vertices, BlockHelper.uvs, BlockUV.GetUV((BlockType)System.Enum.Parse(typeof(BlockType), blocks[flatIndex].ToString()))));

				Color blockColor = Color.Lerp(Chunk.BlockColor(blocks[flatIndex], 1), Color.white, .2f);

				Color[] ambientColors = Ambient.ChangeLight(f, neighbors);

				for(int v = 0; v < ambientColors.Length; v++) {
					//ambientColors[v] = Color.Lerp(blockColor, ambientColors[v], 0.6f);
					ambientColors[v] = blockColor * ambientColors[v];

					//	normals.Add(MathHelper.direction[f]);
				}

				colors.AddRange(ambientColors);

				triangleIndex += Block.vertices;
			}
		}

		chunkData.vertices = vertices;
		chunkData.triangles = triangles;
		chunkData.uvs = uvs;
		chunkData.colors = colors;

		chunkMeshQueue.Enqueue(chunkData);
	}

	void ChunkMeshGenerationWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) {
		ChunkData chunkData = (ChunkData)sender;
		//ChunkData chunkData = (ChunkData)e.Result;

		chunkMeshQueue.Enqueue(chunkData);
	}

	void ChunkApplyMesh(ChunkData chunkData) {
		//Stopwatch watch = new Stopwatch();
		//watch.Start();

		Chunk chunk = chunkData.chunk;

		GameObject chunkObject = GameObject.Find(chunk.name);

		if(chunkObject) {
			Mesh mesh = chunkObject.GetComponent<MeshFilter>().sharedMesh;
			mesh.Clear();

			mesh.vertices = chunkData.vertices.ToArray();
			mesh.triangles = chunkData.triangles.ToArray();
			mesh.uv = chunkData.uvs.ToArray();
			mesh.colors = chunkData.colors.ToArray();

			mesh.RecalculateNormals();

			//mesh.normals = normals.ToArray();

			MeshCollider meshCollider = chunkObject.GetComponent<MeshCollider>();
			meshCollider.sharedMesh = null;
			meshCollider.sharedMesh = mesh;
		}

		chunk.generatingMesh = false;

		VectorI3 index = chunk.index;

		//if(!index.ArrayOutOfBounds(chunkDistance)) {
		SetChunk(index, chunk);
		//Debug.Log(
		//    index.ToString()
		//    + "\n    generatingBlocks = " + chunk.generatingBlocks
		//    + "\n    generatingMesh = " + chunk.generatingMesh
		//    + "\n    queueBlockGeneration = " + chunk.queueBlockGeneration
		//    + "\n    queueMeshGeneration = " + chunk.queueMeshGeneration
		//    );
		//}

		//watch.Stop();
		//UnityEngine.Debug.Log("ApplyMesh Time = " + watch.Elapsed);
	}

	IEnumerator ChunkApplyMeshes() {
		while(true) {
			if(chunkMeshQueue.Count > 0 && !shiftingChunks) {
				ChunkData chunkData = chunkMeshQueue.Dequeue();

				ChunkApplyMesh(chunkData);
			}

			yield return new WaitForEndOfFrame();
		}
	}

	IEnumerator ChunkCoroutineUpdate() {
		while(true) {
			foreach(VectorI3 index in ChunkLoop()) {
				Chunk chunk = GetChunk(index);

				if(chunk.exists) {
					if(!chunk.TooFar()) {
						if(chunk.queueBlockGeneration && !chunk.generatingBlocks) {
							chunk.generatingBlocks = true;
							chunk.queueBlockGeneration = false;

							SetChunk(index, chunk);

							chunk = GenerateBlocks(chunk);
						}

						if(chunk.queueMeshGeneration && !chunk.generatingMesh && !ChunkMeshGenerationWorker.IsBusy && !(chunkMeshQueue.Count > 0)) {
							chunk.generatingMesh = true;
							chunk.queueMeshGeneration = false;

							SetChunk(index, chunk);

							ChunkMeshGenerationWorker_DoWork(new ChunkData(chunk, blocks), null);
							//***HereChunkMeshGenerationWorker.RunWorkerAsync(new ChunkData(chunk, blocks));
						}
					}
				}

				SetChunk(index, chunk);
			}

			yield return new WaitForEndOfFrame();
		}
	}
	#endregion

	void Update() {
		foreach(VectorI3 index in ChunkLoop()) {
			Chunk chunk = GetChunk(index);

			if(chunk.exists) {
				if(chunk.queueBlockGeneration && !chunk.generatingBlocks) {
					chunk.generatingBlocks = true;
					chunk.queueBlockGeneration = false;

					SetChunk(index, chunk);

					chunk = GenerateBlocks(chunk);
				}

				if(chunk.queueMeshGeneration && !chunk.generatingMesh && !ChunkMeshGenerationWorker.IsBusy /*&& !(chunkMeshQueue.Count > 0)*/) {
					chunk.generatingMesh = true;
					chunk.queueMeshGeneration = false;

					SetChunk(index, chunk);

					ChunkMeshGeneration(new ChunkData(chunk, blocks));
				}
			}

			SetChunk(index, chunk);
		}

		while(chunkMeshQueue.Count > 0) {
			ChunkData chunkData = chunkMeshQueue.Dequeue();

			ChunkApplyMesh(chunkData);
		}

		//if(Input.GetKeyDown(KeyCode.Space)) {
		//Debug.Log(GetNeighbor(VectorI3.zero, VectorI3.left).ToString());
		//}

		//if(Input.GetMouseButtonDown(0)) {
		//RaycastHit hit;

		//if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100)) {
		//    foreach(VectorI3 index in ChunkLoop()) {
		//        if(chunks[index.x, index.y, index.z].position == hit.collider.transform.position) {
		//            chunks[index.x, index.y, index.z].Remove();
		//            break;
		//        }
		//    }
		//}
		//Debug.Log("Block Here = " + blocks[MathHelper.FlatIndex(new VectorI3(0, 0, 0), worldSize)]);
		//Debug.Log("Block Neighbor " + GetNeighbor(new VectorI3(0, 0, 0), new VectorI3(0, 1, 0)));
		//}

		bool add = Input.GetKeyDown(KeyCode.T);
		//bool remove = Input.GetKeyDown(KeyCode.R);

		if(add)
			foreach(VectorI3 index in ChunkLoop()) {
				Chunk chunk = GetChunk(index);

				//if(!chunk.exists) {
				//	if(add) chunk.Add();
				//}
				//else {
				//    if(remove && chunk.tooFarAway) chunk.Remove();
				//}

				//if(chunk.exists) {
				chunk.queueBlockGeneration = true;
				//chunk.queueMeshGeneration = true;
				//}

				SetChunk(index, chunk);
			}

		//chunksGenerating = false;

		//foreach(Chunk a in chunks) {
		//    if(a.exists) {
		//        if(a.queueBlockGeneration || a.generatingBlocks || a.queueMeshGeneration || a.generatingMesh) {
		//            chunksGenerating = true; break;
		//        }
		//    }
		//}
	}

	void World_OnPlayerMoved(Vector3 previousPosition, Vector3 position) {
		shiftingChunks = true;
		//StartCoroutine("QueuedMove", new KeyValuePair<Vector3, Vector3>(previousPosition, position));
		QueuedMove(new KeyValuePair<Vector3, Vector3>(previousPosition, position));
	}

	//IEnumerator QueuedMove(KeyValuePair<Vector3, Vector3> data) {
	void QueuedMove(KeyValuePair<Vector3, Vector3> data) {
		//while(ChunkMeshGenerationWorker.IsBusy) yield return new WaitForEndOfFrame();

		//ThreadPool.QueueUserWorkItem(ChunkUpdate, data);
		ChunkUpdate(data);
		//StartCoroutine("ChunkUpdate", data);

		shiftingChunks = false;
	}

	public static GameObject GetChunkObject(string name) {
		GameObject chunkObject = null;

		for(int i = 0; i < world.transform.childCount; i++) {
			if(world.transform.GetChild(i).name.Contains(name)) chunkObject = world.transform.GetChild(i).gameObject;
		}

		return chunkObject;
	}

	public Chunk GetChunk(VectorI3 index) {
		return chunks[index.x, index.y, index.z];
	}

	public void SetChunk(VectorI3 index, Chunk chunk) {
		chunks[index.x, index.y, index.z] = chunk;
	}

	//void ChunkUpdate(Vector3 previousPosition, Vector3 playerPosition) {
	//void ChunkUpdate(KeyValuePair<Vector3, Vector3> data) {
	void ChunkUpdate(object data) {
		Vector3 previousPosition = ((KeyValuePair<Vector3, Vector3>)data).Key;
		Vector3 playerPosition= ((KeyValuePair<Vector3, Vector3>)data).Value;

		#region Remove
		foreach(VectorI3 index in ChunkLoop()) {
			Chunk chunk = GetChunk(index);

			if(chunk.exists) {
				if(chunk.TooFar()) {
					chunk.Remove();
					SetChunk(index, chunk);
				}
			}
		}
		#endregion

		#region Chunk Shifting
		byte[] temp = new byte[worldData.worldSize.size];

		foreach(VectorI3 index in ChunkLoop()) {
			Chunk chunk = GetChunk(index);

			if(chunk.exists) {
				chunk.position = ArrayIndexToPosition(chunk.permaIndex);

				if(!chunk.generatingBlocks && !chunk.generatingMesh) {
					if(!chunk.TooFar()) {
						foreach(VectorI3 localIndex in Chunk.BlockLoop()) {
							VectorI3 blockIndex = localIndex + (LocalArrayIndex(chunk.position, previousPosition) * chunkSize);
							int flatIndex = blockIndex.FlatIndex(worldSize);

							byte block = blocks[flatIndex];

							blockIndex = localIndex + (chunk.localIndex * chunkSize);
							flatIndex = blockIndex.FlatIndex(worldSize);
							temp[flatIndex] = block;
						}

						chunk.queueMeshGeneration = true;
					}

					//chunk.queueMeshGeneration = true;
				}
			}

			SetChunk(index, chunk);
		}

		blocks = temp;
		#endregion

		#region Add
		foreach(VectorI3 index in ChunkLoop()) {
			Vector3 direction = (new Vector3(index.x, index.y, index.z) - (chunkDistance * .5f)) * chunkSize;
			Chunk chunk = GetChunk(index);

			if(!chunk.exists) {
				chunk.position = ArrayIndexToPosition(chunk.permaIndex, playerPosition);
				chunk.Add();
				SetChunk(index, chunk);
			}
		}
		#endregion
	}

	Chunk GenerateBlocks(Chunk chunk) {
		//Debug.LogError("Generating blocks for Chunk" + chunk.position + " LocalIndex" + chunk.localIndex + " Index" + chunk.index);
		//Debug.DrawLine(chunk.position + Vector3.down, chunk.position + Vector3.up);

		foreach(VectorI3 i in Chunk.BlockLoop()) {
			VectorI3 blockIndex = i + (chunk.localIndex * chunkSize);

			int flatIndex = blockIndex.FlatIndex(worldSize);
			float noise = GetNoise(i + chunk.position);
			//float noise = Random.Range(.2f, .7f);

			if(noise > 0.2f)
				blocks[flatIndex] = 1;

			if(noise > .3f) {
				blocks[flatIndex] = 3;
			}

			if(noise > .6f)
				blocks[flatIndex] = 2;

			if(noise > .8f)
				blocks[flatIndex] = 0;
		}

		chunk.queueBlockGeneration = false;
		chunk.generatingBlocks = false;
		chunk.queueMeshGeneration = true;
		chunk.blocksGenerated = true;

		return chunk;
	}

	Chunk GenerateMesh(Chunk chunk) {
		//Vector3 index = (chunk.index - (chunkDistance * .5f)) * chunkSize;
		GameObject chunkObject = GetChunkObject(chunk.nameLeft);// GameObject.Find(chunk.name);

		if(chunkObject) {
			List<Vector3> vertices = new List<Vector3>();
			List<int> triangles = new List<int>();
			List<Vector2> uvs = new List<Vector2>();
			List<Color> colors = new List<Color>();
			//List<Vector3> normals = new List<Vector3>();

			int triangleIndex = 0;

			//Debug.Log(chunk.index + " " + (chunk.localIndex));

			foreach(VectorI3 localIndex in chunk.BlockLoopNoAir()) {
				VectorI3 blockIndex = localIndex + (chunk.localIndex * chunkSize);
				int flatIndex = MathHelper.FlatIndex(blockIndex, worldSize);

				if(blocks[flatIndex] == 0) continue;

				bool[] neighbors = GetNeighbors(blockIndex);

				if(Chunk.SkipBlockLoop(neighbors)) continue;

				foreach(int f in Chunk.BlockFaceLoop(blockIndex)) {
					Vector3[] vertexVectors = MathHelper.OffsetVector3(Block.vertices, BlockHelper.faceVertices[f], Chunk.BlockLocalPosition(localIndex.x, localIndex.y, localIndex.z));

					//if(WorldManager.enableBlockDeformation) vertexVectors = DeformVertices(f, vertexVectors, deformMultiplier, neighbors);

					//vertexVectors[4] = (vertexVectors[0] + vertexVectors[1] + vertexVectors[2] + vertexVectors[3] + vertexVectors[4]) / 5f;
					vertexVectors[4] = (vertexVectors[0] + vertexVectors[1] + vertexVectors[2] + vertexVectors[3]) / 4f;

					vertices.AddRange(vertexVectors);
					triangles.AddRange(MathHelper.OffsetInt(Block.triangles * 3, BlockHelper.triangles, triangleIndex));
					uvs.AddRange(MathHelper.OffsetVector2(Block.vertices, BlockHelper.uvs, BlockUV.GetUV((BlockType)System.Enum.Parse(typeof(BlockType), blocks[flatIndex].ToString()))));

					Color blockColor = Chunk.BlockColor(blocks[flatIndex], 1);

					Color[] ambientColors = Ambient.ChangeLight(f, neighbors);

					for(int v = 0; v < ambientColors.Length; v++) {
						//ambientColors[v] = Color.Lerp(blockColor, ambientColors[v], 0.6f);
						ambientColors[v] = blockColor * ambientColors[v];

						//	normals.Add(MathHelper.direction[f]);
					}

					colors.AddRange(ambientColors);

					triangleIndex += Block.vertices;
				}
			}

			Mesh mesh = chunkObject.GetComponent<MeshFilter>().sharedMesh;
			mesh.Clear();

			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.uv = uvs.ToArray();
			mesh.colors = colors.ToArray();

			mesh.RecalculateNormals();

			//mesh.normals = normals.ToArray();

			MeshCollider meshCollider = chunkObject.GetComponent<MeshCollider>();
			meshCollider.sharedMesh = null;
			meshCollider.sharedMesh = mesh;
		}

		return chunk;
	}

#if UNITY_EDITOR

	void OnGUI() {
		//GUILayout.Box("Generating mesh? " + ChunkMeshGenerationWorker.IsBusy);

		//int chunkAmount = 0;
		//int chunksQueuedBlocks = 0;
		int chunksQueuedMesh = 0;
		//int chunksGeneratingBlocks = 0;
		int chunksGeneratingMesh = 0;

		foreach(Chunk a in chunks) {
			//    if(a.exists) {
			//        chunkAmount++;
			//        if(a.queueBlockGeneration) chunksQueuedBlocks++;
			if(a.queueMeshGeneration) chunksQueuedMesh++;
			//        if(a.generatingBlocks) chunksGeneratingBlocks++;
			if(a.generatingMesh) chunksGeneratingMesh++;

			//        Vector3 pos = Camera.main.WorldToScreenPoint(a.position);
			//        Vector2 size = GUI.skin.box.CalcSize(new GUIContent(a.position.ToString()));

			//        Rect rect = new Rect(0, 0, size.x, size.y);
			//        rect.center = new Vector2(pos.x, Screen.height - pos.y);
			//        //GUI.Box(rect, a.position.ToString());
			//        //GUI.Box(rect, a.index.ToString());
			//        //GUI.Box(rect, a.blocksGenerated.ToString());
			//    }
		}

		GUILayout.Space(100);

		//GUILayout.Box("Chunks = " + chunkAmount, GUILayout.ExpandWidth(false));
		//GUILayout.Box("Chunks Queued To Generate Blocks = " + chunksQueuedBlocks, GUILayout.ExpandWidth(false));
		GUILayout.Box("Chunks Queued To Generate Mesh = " + chunksQueuedMesh, GUILayout.ExpandWidth(false));
		//GUILayout.Box("Chunks Generating Blocks = " + chunksGeneratingBlocks, GUILayout.ExpandWidth(false));
		GUILayout.Box("Chunks Generating Mesh = " + chunksGeneratingMesh, GUILayout.ExpandWidth(false));
	}


	void OnDrawGizmos() {
		if(Application.isPlaying) {
			//foreach(Chunk a in chunks) {
			//        if(debugChunksGeneratingBlocks && (a.queueBlockGeneration || a.generatingBlocks)) {
			//            Gizmos.color = new Color(.2f, .2f, 1, a.generatingBlocks ? .6f : .3f);
			//            Gizmos.DrawCube(a.position, chunkSize);
			//        }

			//        //if(debugChunksGeneratingMesh && (a.queueMeshGeneration || a.generatingMesh)) {
			//        if(debugChunksGeneratingMesh && a.generatingMesh) {
			//            Gizmos.color = new Color(1, .5f, 0, a.generatingMesh ? .6f : .3f);
			//            Gizmos.DrawCube(a.position, chunkSize);

			//            //Gizmos.DrawLine(a.position, GetChunkObject(a.name).transform.position);
			//        }

			//        if(a.blocksGenerated) {
			//            Gizmos.color = Color.black;
			//            Gizmos.DrawWireSphere(a.position, chunkSize.x * .5f);
			//        }

			//float chunkAmount = chunkDistance.size;

			//Gizmos.color = Color.Lerp(Color.black, Color.white, a.permaIndex.FlatIndex(chunkDistance) / (chunkAmount - 1));
			//Gizmos.DrawCube(a.position, chunkSize);
			//}

			//    //Vector3 chunkPos = ArrayIndexToPosition(new Vector3(0, 0, 0), playerPosition);
			//    //Gizmos.color = Color.green;
			//    //Gizmos.DrawCube(chunkPos, chunkSize);
		}

		bool gizmosToggle = false;

		if(Application.isPlaying && gizmosToggle) {
			VectorI3 chunkIndex = chunkDistance;
			VectorI3 size = chunkSize;

			for(int x = 0; x < chunkIndex.x; x++)
				for(int y = 0; y < chunkIndex.y; y++)
					for(int z = 0; z < chunkIndex.z; z++) {
						Vector3 index = (new Vector3(x, y, z) - (chunkDistance * .5f)) * chunkSize;
						Chunk chunk = GetChunk(new VectorI3(x, y, z));

						Vector3 player = PlayerChunkPosition() + (chunkSize * .5f);
						Vector3 position = player + Chunk.Position(index);

						//if(chunk.exists) {
						Vector3 cSize = size;

						Gizmos.color = new Color((x + 1f) / chunkIndex.x, (y + 1f) / chunkIndex.y, (z + 1f) / chunkIndex.z);
						Gizmos.DrawCube(chunk.position, cSize * .6f);

						VectorI3 ind = LocalArrayIndex(chunk.position);
						Gizmos.color = new Color((ind.x + 1f) / chunkIndex.x, (ind.y + 1f) / chunkIndex.y, (ind.z + 1f) / chunkIndex.z);
						Gizmos.DrawWireCube(chunk.position, size);

						//}
						//else {
						//Gizmos.color = new Color(1, .5f, 0);
						//Gizmos.DrawWireCube(position, size);
						//}

						//Gizmos.color = new Color(0, 0, 1, .3f);
						//Gizmos.DrawCube(chunk.position + (index / chunkDistance) + ((chunkSize / chunkDistance) * .5f), chunkSize / chunkDistance);

						////Gizmos.color = new Color(0, 1, 0, .3f);
						////Gizmos.DrawWireCube(chunkPosition2, chunkSize * 1.1f);

						//////Draw 0,0,0
						////Gizmos.color = new Color(.6f, .6f, 1, .5f);
						////Gizmos.DrawCube(ArrayIndexToPosition(Vector3.zero), chunkSize * .95f);
					}

			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(PlayerChunkPosition(), chunkSize);
			Gizmos.color = Color.blue;
			Gizmos.DrawWireCube(PlayerPositionRounded(), chunkSize * .9f);

		}
	}
#endif

	public static float GetNoise(Vector3 position) {
		float positionZ = position.z / 8f;

		float noise = Mathf.PerlinNoise((position.x / 32) + (positionZ / 2), (position.y / 32) + (positionZ / 2));
		noise += Mathf.PerlinNoise((position.x / 16) + (positionZ / 16), (position.y / 16) + (positionZ / 16));
		noise -= Mathf.PerlinNoise((position.x / 8) + (positionZ / 8), (position.y / 8) + (positionZ / 8));

		if(position.y > 0) {
			float noise2 = Mathf.PerlinNoise((position.x / 64) + (position.y / 32), (position.z / 64) + (position.y / 32));

			if(position.y > noise2 * 32) noise = 0;
		}
		//float noise = Mathf.PerlinNoise((position.x / 8f) + positionZ, (position.y / 8f) + positionZ);
		//noise += Mathf.PerlinNoise((position.x / 16f) + positionZ, (position.y / 16f) + positionZ);
		//noise += Mathf.PerlinNoise(((position.x * 16) / 8) + positionZ, ((position.y * 16) / 8) + positionZ);

		return noise;
	}

	static void Loop(object state) {
		//bool update = true;

		//while(update) {
		//    foreach(Chunk a in chunks) {
		//        foreach(VectorI3 index in WorldBlockLoop()) {
		//            int x = index.x, y = index.y, z = index.z;
		//            int blockIndex = WorldBlockFlatIndex(index);

		//            blocks[blockIndex] = 1;
		//        }
		//    }

		//    Thread.Sleep(100);
		//}
	}

	public static byte GetNeighbor(VectorI3 blockIndex, VectorI3 direction) {
		//direction.y = -direction.y;
		if(MathHelper.ArrayOutOfBounds(blockIndex + direction, worldSize)) return 0;

		return blocks[MathHelper.FlatIndex(blockIndex + direction, worldSize)];
	}
	public static byte GetNeighbor(VectorI3 blockIndex, int directionIndex) {
		VectorI3 direction = MathHelper.direction[directionIndex];
		//direction.y = -direction.y;

		if(MathHelper.ArrayOutOfBounds(blockIndex + direction, worldSize)) return 0;

		return blocks[MathHelper.FlatIndex(blockIndex + direction, worldSize)];
	}

	public static bool[] GetNeighbors(VectorI3 index) {
		bool[] neighbors = new bool[26];

		for(int i = 0; i < 26; i++) {
			//if(!FindNeighborBlock(new Vector3(index.x, index.y, index.z), i)) neighbors[i] = true;
			if(GetNeighbor(index, i) != 0) neighbors[i] = true;
		}

		return neighbors;
	}

	public static VectorI3 LocalArrayIndex(Vector3 position) {
		return LocalArrayIndex(position, World.world.playerPosition);
	}
	public static VectorI3 LocalArrayIndex(Vector3 position, Vector3 worldPosition) {
		worldPosition = Chunk.Position(worldPosition);
		position -= worldPosition;// +(chunkSize * .5f);
		position /= chunkSize;
		position = VectorI3.Round(position) + new VectorI3(chunkDistance / 2);

		return position;
	}

	public static VectorI3 GlobalArrayIndex(Vector3 position) {
		position /= chunkSize;
		position *= 2;
		position = VectorI3.Round(position) + new VectorI3(chunkDistance);

		position = MathHelper.Wrap3DIndex((chunkSize / 2f) - position, chunkDistance);

		return position;
	}

	public static Vector3 ArrayIndexToPosition(VectorI3 index) {
		return ArrayIndexToPosition(index, World.world.playerPosition);
	}
	public static Vector3 ArrayIndexToPosition(VectorI3 index, Vector3 playerPosition) {
		playerPosition = Chunk.Position(playerPosition / chunkSize, Vector3.one);
		Vector3 position = playerPosition;

		VectorI3 distance = chunkDistance;// -Vector3.one;
		Vector3 size = distance;
		Vector3 halfSize = size * .5f;

		position -= index + Vector3.one;
		position = Chunk.Position(position, Vector3.one);
		position = VectorI3.GridCeil(position, size);
		position -= VectorI3.Floor(halfSize);
		position += index;
		position = Chunk.Position(position, Vector3.one);
		position *= chunkSize;

		return position;
	}
	//public static Vector3 ArrayIndexToPosition(VectorI3 index, Vector3 playerPosition) {
	//    Vector3 distance = chunkDistance * chunkSize;
	//    Vector3 position = (-distance * .5f) + (index * chunkSize) + (chunkSize * .5f);

	//    return Chunk.Position(playerPosition, distance) + position;
	//}
	/*public static Vector3 ArrayIndexToPosition(VectorI3 index, int x) {
		Vector3 playerPosition = Chunk.Position(World.world.playerPosition);
		Vector3 position = playerPosition;

		VectorI3 distance = chunkDistance;// -Vector3.one;
		Vector3 size = distance * chunkSize;
		Vector3 halfSize = size * .5f;

		position -= chunkSize * (index + Vector3.one);
		position = Chunk.Position(position);
		position = VectorI3.GridCeil(position, size);
		position -= VectorI3.Floor(halfSize);
		position += chunkSize * index;
		position = Chunk.Position(position);

		return position;
	}*/
	public static VectorI3 GetStartBlockIndex(VectorI3 chunkIndex) {
		return chunkIndex * chunkSize;
	}

	public Vector3 PlayerChunkPosition() {
		return Chunk.Position(playerPosition);
	}

	public Vector3 PlayerPositionRounded() {
		return Chunk.Position(playerPosition + (chunkSize * .5f));
	}

	public static int WorldBlockFlatIndex(VectorI3 index) {
		return MathHelper.FlatIndex(index, worldSize);
	}

	public static IEnumerable WorldBlockLoop() {
		for(int x = 0; x < worldSize.x; x++)
			for(int y = 0; y < worldSize.y; y++)
				for(int z = 0; z < worldSize.z; z++)
					yield return new VectorI3(x, y, z);
	}

	public static IEnumerable ChunkLoop() {
		VectorI3 chunkIndex = chunkDistance;

		for(int x = 0; x < chunkIndex.x; x++)
			for(int y = 0; y < chunkIndex.y; y++)
				for(int z = 0; z < chunkIndex.z; z++)
					yield return new VectorI3(x, y, z);
	}
}

[System.Serializable]
public class WorldData {
	public VectorI3 chunkSize;
	public VectorI3 chunkDistance;
	public VectorI3 worldSize {
		get { return chunkSize * chunkDistance; }
	}
}
