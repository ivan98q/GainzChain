using UnityEngine;

namespace YounGenTech.VoxelTech {
    [System.Serializable]
    public struct Block {

        public static Block Empty { get { return new Block(0); } }
        public static Block Air { get { return new Block(1); } }
        public static Block Solid { get { return new Block(2); } }
        public static Block Gold { get { return new Block(3);  } }

        [SerializeField]
        byte _id;

        #region Properties
        public byte ID {
            get { return _id; }
            set { _id = value; }
        }
        #endregion

        public Block(byte id) {
            _id = id;
        }

        public override bool Equals(object obj) {
            if(!(obj is Block))
                return false;
            else {
                Block block = (Block)obj;

                return block.ID == ID;
            }
        }

        public override int GetHashCode() {
            return ID;
        }

        public static bool operator ==(Block a, Block b) {
            return a.ID == b.ID;
        }

        public static bool operator !=(Block a, Block b) {
            return !(a == b);
        }
    }
}