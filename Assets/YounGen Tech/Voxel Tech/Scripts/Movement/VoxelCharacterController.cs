using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace YounGenTech.VoxelTech {
    public class VoxelCharacterController : MonoBehaviour {

        [SerializeField]
        World _inWorld;

        [SerializeField]
        MouseLook _mouseLookOptions;

        [SerializeField]
        Camera _characterCamera;

        [SerializeField]
        Vector3 _characterSize = new Vector3(1, 1.8f, 1);

        [SerializeField]
        bool _useGravity = true;

        [SerializeField]
        float _acceleration = 1;

        [SerializeField]
        float _sprintAcceleration = 2;

        [SerializeField]
        float _maxWalkSpeed = 2;

        [SerializeField]
        float _maxSprintSpeed = 2;

        [SerializeField]
        float _drag = 1;

        [SerializeField]
        float _airDrag = .5f;

        [SerializeField]
        float _jumpForce = 2;

        [SerializeField]
        float _extraJumpForce = .1f;

        [SerializeField]
        Timer _jumpTimer = new Timer(.1f) { AutoDisable = true };

        [SerializeField]
        Timer _extraJumpTimer = new Timer(.5f) { AutoDisable = true };

        [SerializeField]
        Timer _groundedTimer = new Timer(.1f) { Active = true };

        [SerializeField]
        Vector3 _position;

        [SerializeField]
        Vector3 _velocity;

        [SerializeField]
        GameObject _visualizeBlockPlacement;

        Vector3 movementInput;

        bool blockWasHit = false;
        BlockHit lastBlockHit = new BlockHit();

        #region Properties
        public float Acceleration {
            get { return _acceleration; }
            set { _acceleration = value; }
        }

        public float AirDrag {
            get { return _airDrag; }
            set { _airDrag = value; }
        }

        public VectorI3 BlockFootPosition {
            get {
                Bounds playerBounds = CharacterBounds;

                return InWorld.GetPosition(new Vector3(playerBounds.center.x, playerBounds.min.y, playerBounds.center.z), PositionStyle.Block);
            }
        }

        public VectorI3 BlockCameraPosition {
            get { return InWorld.GetPosition(CharacterCamera.transform.position, PositionStyle.Block); }
        }

        public VectorI3 BlockCenterPosition {
            get { return InWorld.GetPosition(Position, PositionStyle.Block); }
        }

        public VectorI3 BlockHeadPosition {
            get { return InWorld.GetPosition(Position + new Vector3(0, CharacterSize.y * .5f, 0), PositionStyle.Block); }
        }

        public Camera CharacterCamera {
            get { return _characterCamera; }
            set { _characterCamera = value; }
        }

        public Bounds CharacterBounds {
            get { return new Bounds(Position, CharacterSize); }
        }

        public Vector3 CharacterSize {
            get { return _characterSize; }
            set { _characterSize = value; }
        }

        public VectorI3 ChunkPosition {
            get { return InWorld.GetPosition(transform.position, PositionStyle.Chunk); }
        }

        public float Drag {
            get { return _drag; }
            set { _drag = value; }
        }

        public float ExtraJumpForce {
            get { return _extraJumpForce; }
            set { _extraJumpForce = value; }
        }

        public float JumpForce {
            get { return _jumpForce; }
            set { _jumpForce = value; }
        }

        public World InWorld {
            get { return _inWorld; }
            set { _inWorld = value; }
        }

        public bool IsGrounded { get; private set; }

        public bool IsSprinting { get; private set; }

        public bool IsModifierInput { get; private set; }

        public float MaxSprintSpeed {
            get { return _maxSprintSpeed; }
            set { _maxSprintSpeed = value; }
        }

        public float MaxWalkSpeed {
            get { return _maxWalkSpeed; }
            set { _maxWalkSpeed = value; }
        }

        public MouseLook MouseLookOptions {
            get { return _mouseLookOptions; }
            private set { _mouseLookOptions = value; }
        }

        public Vector3 Position {
            get { return _position; }
            set { _position = value; }
        }

        public float SprintAcceleration {
            get { return _sprintAcceleration; }
            set { _sprintAcceleration = value; }
        }

        public bool UseGravity {
            get { return _useGravity; }
            set { _useGravity = value; }
        }

        public Vector3 Velocity {
            get { return _velocity; }
            set { _velocity = value; }
        }
        #endregion

        void Awake() {
            _groundedTimer.OnElapsed.AddListener(() => IsGrounded = false);
            Position = transform.position;
            //MouseLookOptions.Init(transform, CharacterCamera.transform);
        }

        void Update() {
            //MouseLookOptions.LookRotation(transform, CharacterCamera.transform);
            //MouseLookOptions.UpdateCursorLock();

            UpdateInput();
            UpdateJump();
            UpdateBlock();

            transform.position = Position;
        }

        void FixedUpdate() {
            if(UseGravity)
                Velocity += Physics.gravity * Time.deltaTime;

            if(!IsGrounded) {
                Velocity -= new Vector3(Velocity.x, 0, Velocity.z) * AirDrag * Time.deltaTime;
            }
            else {
                if(movementInput.sqrMagnitude <= .01f)
                    Velocity -= new Vector3(Velocity.x, 0, Velocity.z) * Drag * Time.deltaTime;
            }

            if(_extraJumpTimer.Active && !_jumpTimer.Active)
                Velocity = Velocity.AddForce(Vector3.up * ExtraJumpForce, 1, ForceMode.Force);

            if(_jumpTimer.Active) {
                Velocity = Velocity.AddForce(Vector3.up * JumpForce, 1, ForceMode.VelocityChange);

                _jumpTimer.Reset(true);
            }

            Position += Velocity * Time.deltaTime;
            UpdateMovement();
            UpdateCollision();
        }

        void OnDrawGizmos() {
            Gizmos.color = new Color(.2f, 1, .2f, .6f);
            Gizmos.DrawWireCube(CharacterBounds.center, CharacterBounds.size);
        }

        void OnGUI() {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("Chunk Position " + ChunkPosition);
                GUILayout.Label("Block Position " + BlockFootPosition);
                GUILayout.Label("Velocity " + Velocity.ToString("F3"));
            }
            GUILayout.EndVertical();
        }

        void BuildTo(BlockHit hit) {

        }

        void CheckCollision(VectorI3 blockPosition) {
            var block = InWorld.GetBlock(blockPosition);
            var blockData = InWorld.BlockDatabaseAsset.GetBlockData(block.ID);

            if(blockData.IsSolid) {
                var blockBounds = new Bounds(blockPosition + new Vector3(.5f, .5f, .5f), Vector3.one);

#if UNITY_EDITOR
                IntBounds.DrawWireCube(blockBounds.center, new Vector3(-.49f, -.49f, -.49f), new Vector3(.49f, .49f, .49f), new Color(1, 0, 0, .5f), 0, false);
#endif

                AABBTransformData data = OffsetBoundsFromCollision(CharacterBounds, Velocity, blockBounds);

                Velocity = Vector3.Scale(Velocity, new Vector3(1 - Mathf.Abs(data.fixDirection.x), 1 - Mathf.Abs(data.fixDirection.y), 1 - Mathf.Abs(data.fixDirection.z)));
                Velocity += data.overlap * 2;
                Position = data.position;
            }
        }

        void UpdateBlock() {
            int blockCount = 0;

            foreach(var hit in BlockRaycast.Cast(Camera.main.transform.position, Camera.main.transform.forward)) {
                //foreach(var hit in BlockRaycast.Cast(BlockHeadPosition + new Vector3(.5f, .5f, .5f), Camera.main.transform.forward)) {
                var block = InWorld.GetBlock(hit.position);
                var blockData = InWorld.BlockDatabaseAsset.GetBlockData(block.ID);

                blockCount++;

                if(blockData.IsSolid) {
#if UNITY_EDITOR
                    IntBounds.DrawWireCube(hit.position + new Vector3(.5f, .5f, .5f), new Vector3(-.525f, -.525f, -.525f), new Vector3(.525f, .525f, .525f), new Color(1, .5f, 0), 0, false);
                    IntBounds.DrawWireCube((hit.position + hit.face.ToDirectionVector()) + new Vector3(.5f, .5f, .5f), new Vector3(-.45f, -.45f, -.45f), new Vector3(.45f, .45f, .45f), new Color(1, 1, 1), 0, false);
#endif

                    lastBlockHit = new BlockHit(hit.position, hit.face, block, blockData);
                    blockWasHit = true;

                    break;
                }

                if(blockCount > 50) {
                    blockWasHit = false;
                    break;
                }
            }

            if(_visualizeBlockPlacement) {
                if(blockWasHit) {
                    _visualizeBlockPlacement.transform.position = lastBlockHit.position + new Vector3(.5f, .5f, .5f);
                    _visualizeBlockPlacement.transform.rotation = Quaternion.LookRotation(lastBlockHit.face.ToDirectionVector());
                }

                if(_visualizeBlockPlacement.activeSelf != blockWasHit)
                    _visualizeBlockPlacement.SetActive(blockWasHit);
            }
        }

        void UpdateCollision() {
            InWorld.ForEachBlockPositionInBounds(CharacterBounds, CheckCollision);
            IsGrounded = false;

            if(Velocity.y <= 0) {
                BlockPositionData[] blockPositions;
                VectorI3 size = InWorld.GetBlocksInBounds(CharacterBounds, out blockPositions);

                //Check intersecting blocks for ground
                for(int x = 0; x < size.x; x++) {
                    for(int z = 0; z < size.z; z++) {
                        VectorI3 index = new VectorI3(x, 0, z);

                        if(InWorld.BlockDatabaseAsset.GetBlockData(blockPositions[index.FlatIndex(size)].block.ID).IsSolid) {
                            IsGrounded = true;

#if UNITY_EDITOR
                            IntBounds.DrawWireCube(blockPositions[index.FlatIndex(size)].worldPosition + new Vector3(.5f, .5f, .5f), new Vector3(-.525f, -.525f, -.525f), new Vector3(.525f, .525f, .525f), new Color(1, 0, .5f), 0, false);
#endif

                            goto BecameGrounded;
                        }
                    }
                }

                //Not intersecting any blocks, check feet
                if(!IsGrounded) {
                    if((CharacterBounds.min.y - Mathf.Floor(CharacterBounds.min.y)) < .1f) //Make sure the character's feet is near the block below
                        for(int x = 0; x < size.x; x++) {
                            for(int z = 0; z < size.z; z++) {
                                VectorI3 index = new VectorI3(x, 0, z);
                                Block block = InWorld.GetBlock(blockPositions[index.FlatIndex(size)].worldPosition + VectorI3.down);

                                if(InWorld.BlockDatabaseAsset.GetBlockData(block.ID).IsSolid) {
                                    IsGrounded = true;

#if UNITY_EDITOR
                                    IntBounds.DrawWireCube(blockPositions[index.FlatIndex(size)].worldPosition + new Vector3(.5f, .5f, .5f), new Vector3(-.501f, -.501f, -.501f), new Vector3(.501f, .501f, .501f), new Color(1, 0, .5f), 0, false);
#endif

                                    goto BecameGrounded;
                                }
                            }
                        }
                }
            }

            BecameGrounded:

            if(IsGrounded)
                _groundedTimer.Reset();

            _groundedTimer.Update();
        }

        void UpdateInput() {
            movementInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            if(Cursor.lockState == CursorLockMode.Locked) {
                if(IsModifierInput) {
                    if(Input.GetMouseButton(0))
                        //if(Input.GetButton("Fire1"))
                        if(blockWasHit)
                            InWorld.SetBlock(lastBlockHit.position, Block.Air);

                    if(Input.GetMouseButtonDown(1))
                        //if(Input.GetButtonDown("Fire2"))
                        if(blockWasHit) {
                            Vector3 direction = lastBlockHit.face.ToDirectionVector();
                            Vector3 directionToBlock = lastBlockHit.position - Position;
                            int axisIndex = MaxIndex(Mathf.Abs(direction.x), Mathf.Abs(direction.y), Mathf.Abs(direction.z));
                            int distance = Mathf.Abs(Mathf.CeilToInt(directionToBlock[axisIndex]));

                            VectorI3 blockDirection = Vector3.zero;
                            blockDirection[axisIndex] = direction[axisIndex] < 0 ? -1 : 1;

                            for(int i = 1; i < distance; i++)
                                InWorld.SetBlock(lastBlockHit.position + blockDirection * i, new Block(3));
                        }
                }
                else {
                    if(Input.GetMouseButtonDown(0))
                        //if(Input.GetButtonDown("Fire1"))
                        if(blockWasHit)
                            InWorld.SetBlock(lastBlockHit.position, Block.Air);

                    if(Input.GetMouseButtonDown(1))
                        //if(Input.GetButtonDown("Fire2"))
                        if(blockWasHit)
                            InWorld.SetBlock(lastBlockHit.position + lastBlockHit.face.ToDirectionVector(), new Block(3));
                }
            }

            //IsSprinting = Input.GetButton("Sprint");
            //IsModifierInput = Input.GetButton("Modifier");
            IsSprinting = Input.GetKey(KeyCode.LeftShift);
            IsModifierInput = Input.GetKey(KeyCode.LeftControl);
        }

        void UpdateJump() {
            if(Input.GetButtonDown("Jump")) {
                if(IsGrounded) {
                    _jumpTimer.Start(true);
                    _extraJumpTimer.Start(true);
                    _groundedTimer.CurrentTime = 0;
                    IsGrounded = false;
                }
            }

            if(Input.GetButtonUp("Jump")) {
                if(_jumpTimer.Active)
                    _jumpTimer.Reset(true);

                if(_extraJumpTimer.Active)
                    _extraJumpTimer.Reset(true);
            }

            _jumpTimer.Update();
            _extraJumpTimer.Update();
        }

        void UpdateMovement() {
            if(movementInput.sqrMagnitude > .01f) {
                Vector3 force = Quaternion.LookRotation(Camera.main.transform.right) * new Vector3(-movementInput.z, 0, movementInput.x);

                float acceleration = IsSprinting ? SprintAcceleration : Acceleration;
                float maxSpeed = IsSprinting ? MaxSprintSpeed : MaxWalkSpeed;

                Velocity += Vector3.ClampMagnitude(force, 1) * acceleration * Time.deltaTime;
                Velocity = Vector3.ClampMagnitude(new Vector3(Velocity.x, 0, Velocity.z), maxSpeed) + new Vector3(0, Velocity.y, 0);
            }
        }


        static int MaxIndex(params float[] values) {
            int result = -1;

            if(values.Length > 0) {
                result = 0;

                for(int i = 1; i < values.Length; i++)
                    if(values[i] > values[result])
                        result = i;
            }

            return result;
        }

        static int MinIndex(params float[] values) {
            int result = -1;

            if(values.Length > 0) {
                result = 0;

                for(int i = 1; i < values.Length; i++)
                    if(values[i] < values[result])
                        result = i;
            }

            return result;
        }

        static AABBTransformData OffsetBoundsFromCollision(Bounds origin, Vector3 velocity, Bounds collision) {
            Vector3 position = origin.center;
            Vector3 direction = collision.center - origin.center;
            Vector3 overlap = Vector3.zero;
            VectorI3 fixDirection = VectorI3.zero;

            overlap.x = (origin.extents.x + collision.extents.x) - Mathf.Abs(direction.x);

            if(overlap.x > 0) {
                overlap.y = (origin.extents.y + collision.extents.y) - Mathf.Abs(direction.y);

                if(overlap.y > 0) {
                    overlap.z = (origin.extents.z + collision.extents.z) - Mathf.Abs(direction.z);

                    if(overlap.z > 0) {
                        int axisIndex = MinIndex(overlap.x, overlap.y, overlap.z);

                        if(Mathf.Abs(velocity[axisIndex]) == 0) {
                            overlap = Vector3.zero;
                        }
                        else {
                            fixDirection[axisIndex] = direction[axisIndex] < 0 ? -1 : 1;

                            if(Mathf.Sign(velocity[axisIndex]) == fixDirection[axisIndex]) {
                                overlap *= fixDirection;
                                position -= overlap;
                            }
                            else {
                                overlap = Vector3.zero;
                                fixDirection = VectorI3.zero;
                            }
                        }

                        return new AABBTransformData() { position = position, overlap = overlap, fixDirection = fixDirection };
                    }
                }
            }

            return new AABBTransformData() { position = position };
        }

        struct BlockHit {
            public VectorI3 position;
            public CubeDirectionFlag face;
            public Block block;
            public BlockData blockData;

            public BlockHit(VectorI3 position, CubeDirectionFlag face, Block block, BlockData blockData) {
                this.position = position;
                this.face = face;
                this.block = block;
                this.blockData = blockData;
            }
        }

        struct AABBTransformData {
            public Vector3 position;
            public Vector3 overlap;
            public VectorI3 fixDirection;
        }
    }
}