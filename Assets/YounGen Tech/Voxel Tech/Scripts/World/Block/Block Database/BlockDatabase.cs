using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.VoxelTech {
    [CreateAssetMenu(fileName = "Block Database", menuName = "Voxel Tech/Block Database")]
    public class BlockDatabase : ScriptableObject, ISerializationCallbackReceiver {

        [SerializeField]
        List<BlockData> _blockDataList = new List<BlockData>();
        Dictionary<uint, BlockData> _blockDataDictionary = new Dictionary<uint, BlockData>();

        public int GetUniqueID() {
            int id = 0;

            if(_blockDataList.Count > 0)
                while(_blockDataList.Exists(s => s.ID == id))
                    ++id;

            return id;
        }

        public BlockData GetBlockData(uint id) {
            return _blockDataDictionary[id];
        }
        public BlockData GetBlockData(string name) {
            return _blockDataList.Find(s => s.Name == name);
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() {
            _blockDataDictionary = new Dictionary<uint, BlockData>();

            foreach(var value in _blockDataList)
                _blockDataDictionary[value.ID] = value;
        }
    }
}