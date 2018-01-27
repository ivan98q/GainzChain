using UnityEngine;

namespace YounGenTech.VoxelTech {
    public class ChunkMeshObject : MonoBehaviour {

        [SerializeField]
        Mesh _chunkMesh;

        [SerializeField]
        MeshFilter _chunkMeshFilter;

        [SerializeField]
        MeshRenderer _chunkRenderer;

        [SerializeField]
        MeshCollider _chunkCollider;

        #region Properties
        public MeshCollider ChunkCollider {
            get { return _chunkCollider; }
            set { _chunkCollider = value; }
        }

        public Mesh ChunkMesh {
            get { return _chunkMesh; }
            set { _chunkMesh = value; }
        }

        public MeshFilter ChunkMeshFilter {
            get { return _chunkMeshFilter; }
            set { _chunkMeshFilter = value; }
        }

        public MeshRenderer ChunkRenderer {
            get { return _chunkRenderer; }
            set { _chunkRenderer = value; }
        }

        public ChunkObject PartOfChunk { get; set; }
        #endregion

        public void PrepareMesh() {
            if(ChunkMesh == null) {
                ChunkMesh = new Mesh();

                ChunkMesh.MarkDynamic();
                //ChunkMeshFilter.sharedMesh = ChunkMesh;
                ChunkCollider.sharedMesh = ChunkMesh;
                //ChunkMesh.subMeshCount = 1;

                ChunkMeshFilter.sharedMesh = ChunkMesh;
            }
            else {
                ChunkMesh.Clear();
            }
        }
    }
}