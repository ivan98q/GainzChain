using System;
using UnityEngine;

namespace YounGenTech.VoxelTech {
    public abstract class BlockObject : MonoBehaviour, IBlockObjectBuildRules {
        public abstract void BuildBlockObject(World world, params BlockNeighbor[] neighbors);
    }
}