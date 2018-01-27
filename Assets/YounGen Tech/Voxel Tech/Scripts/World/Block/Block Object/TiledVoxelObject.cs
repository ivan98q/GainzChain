using System;
using UnityEngine;

namespace YounGenTech.VoxelTech {
    public class TiledVoxelObject : BlockObject {
        [SerializeField]
        GameObject _left;

        [SerializeField]
        GameObject _right;

        [SerializeField]
        GameObject _down;

        [SerializeField]
        GameObject _up;

        [SerializeField]
        GameObject _back;

        [SerializeField]
        GameObject _forward;

        #region Properties
        public GameObject Back {
            get { return _back; }
            set { _back = value; }
        }

        public GameObject Down {
            get { return _down; }
            set { _down = value; }
        }

        public GameObject Forward {
            get { return _forward; }
            set { _forward = value; }
        }

        public GameObject Left {
            get { return _left; }
            set { _left = value; }
        }

        public GameObject Right {
            get { return _right; }
            set { _right = value; }
        }

        public GameObject Up {
            get { return _up; }
            set { _up = value; }
        }
        #endregion

        public override void BuildBlockObject(World world, params BlockNeighbor[] neighbors) {
            CubeDirectionFlag flags = CubeDirectionFlag.None;

            foreach(var neighbor in neighbors)
                if(world.BlockDatabaseAsset.GetBlockData(neighbor.block.ID).IsOpaque)
                    flags |= neighbor.direction;

            Left.SetActive(!flags.HasDirection(CubeDirectionFlag.Left));
            Right.SetActive(!flags.HasDirection(CubeDirectionFlag.Right));

            Down.SetActive(!flags.HasDirection(CubeDirectionFlag.Down));
            Up.SetActive(!flags.HasDirection(CubeDirectionFlag.Up));

            Back.SetActive(!flags.HasDirection(CubeDirectionFlag.Back));
            Forward.SetActive(!flags.HasDirection(CubeDirectionFlag.Forward));
        }
    }
}