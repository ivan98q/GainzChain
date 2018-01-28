using UnityEngine;

namespace YounGenTech.VoxelTech {
    [System.Serializable]
    public class BlockData {

        [SerializeField]
        string _name;

        [SerializeField]
        uint _id;

        [SerializeField]
        BlockBuildType _buildType = BlockBuildType.Material;

        [SerializeField]
        bool _isSolid;

        [SerializeField]
        bool _isOpaque;

        [SerializeField]
        Material _faceMaterialLeft, _faceMaterialRight, _faceMaterialDown, _faceMaterialUp, _faceMaterialBack, _faceMaterialForward;

        [SerializeField]
        BlockObject _originalBlockObject;

        #region Properties
        public BlockBuildType BuildType {
            get { return _buildType; }
            set { _buildType = value; }
        }

        public uint ID {
            get { return _id; }
            set { _id = value; }
        }

        public bool IsSolid {
            get { return _isSolid; }
            set { _isSolid = value; }
        }

        public bool IsOpaque {
            get { return _isOpaque; }
            set { _isOpaque = value; }
        }

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public BlockObject OriginalBlockObject {
            get { return _originalBlockObject; }
            set { _originalBlockObject = value; }
        }
        #endregion

        public Material GetFaceMaterial(int face) {
            switch(face) {
                default: return null;
                case 0: return _faceMaterialLeft;
                case 1: return _faceMaterialRight;
                case 2: return _faceMaterialDown;
                case 3: return _faceMaterialUp;
                case 4: return _faceMaterialBack;
                case 5: return _faceMaterialForward;
            }
        }

        public enum BlockBuildType {
            Material,
            BlockObject
        }
    }

    public interface IBlockObjectBuildRules {
        void BuildBlockObject(World world, params BlockNeighbor[] neighbors);
    }
}