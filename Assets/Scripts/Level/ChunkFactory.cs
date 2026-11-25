using UnityEngine;

namespace SlipUp
{
    public sealed class ChunkFactory
    {
        public const int GameplayRecipeCount = 32;
        public const float PassageWidth = 8f;
        public const float TightWidth = 3f;
        public const float PassageHeight = 3f;
        public const float FloorThickness = 0.2f;
        public const float DefaultLength = 10f;

        private readonly Transform _parent;
        private readonly Material _floorMaterial;
        private readonly Material _accentMaterial;
        private readonly Material _altFloorMaterial;

        public ChunkFactory(Transform parent)
        {
            _parent = parent;
            _floorMaterial = CreateMaterial(new Color(0.35f, 0.7f, 0.42f, 1f));
            _altFloorMaterial = CreateMaterial(new Color(0.28f, 0.55f, 0.72f, 1f));
            _accentMaterial = CreateMaterial(new Color(0.95f, 0.35f, 0.2f, 1f));
        }

        public Chunk CreateStart()
        {
            return CreateStraight("Chunk_StartA", DefaultLength, PassageWidth, ConnectorType.Straight8, true, false);
        }

        public Chunk CreateStraightB()
        {
            return CreateStraight("Chunk_StraightB", DefaultLength, PassageWidth, ConnectorType.Straight8, false, false);
        }

        public Chunk CreateFinish()
        {
            return CreateStraight("Chunk_Finish", DefaultLength, PassageWidth, ConnectorType.Straight8, false, true);
        }

        public Chunk CreateGameplay(int recipe)
        {
            int kind = recipe % GameplayRecipeCount;

            switch (kind)
            {
                case 0:
                    return CreateStraight("Straight", DefaultLength, PassageWidth, ConnectorType.Straight8, false, false);
                case 1:
                    return CreateStraight("StraightLong", 16f, PassageWidth, ConnectorType.Straight8, false, false);
                case 2:
                    return CreateStraight("Tight", DefaultLength, TightWidth, ConnectorType.Tight3, false, false);
                case 3:
                    return CreateAdapter("WideToTight", PassageWidth, TightWidth, ConnectorType.Straight8, ConnectorType.Tight3);
                case 4:
                    return CreateAdapter("TightToWide", TightWidth, PassageWidth, ConnectorType.Tight3, ConnectorType.Straight8);
                case 5:
                    return CreateGap();
                case 6:
                    return CreateDashWall();
                case 7:
                    return CreateBumper("BumperCenter", 0f);
                case 8:
                    return CreateBumperPair();
                case 9:
                    return CreateSpinnerBar();
                case 10:
                    return CreateSpinnerCross();
                case 11:
                    return CreateSweep();
                case 12:
                    return CreateRamp("RampUp", 3f);
                case 13:
                    return CreateRamp("RampDown", -3f);
                case 14:
                    return CreateStairs("StairsUp", 3f);
                case 15:
                    return CreateStairs("StairsDown", -3f);
                case 16:
                    return CreateTurn("TurnRight", 90f, PassageWidth, ConnectorType.Straight8);
                case 17:
                    return CreateTurn("TurnLeft", -90f, PassageWidth, ConnectorType.Straight8);
                case 18:
                    return CreateTurn("TurnRightLong", 90f, PassageWidth, ConnectorType.Straight8, 12f);
                case 19:
                    return CreateTurn("TurnLeftLong", -90f, PassageWidth, ConnectorType.Straight8, 12f);
                case 20:
                    return CreateTurn("TightTurnLeft", -90f, TightWidth, ConnectorType.Tight3);
                case 21:
                    return CreateBeam();
                case 22:
                    return CreateOffsetPlatforms();
                case 23:
                    return CreatePendulum();
                case 24:
                    return CreateLowBar();
                case 25:
                    return CreateDashLane();
                case 26:
                    return CreateZigZagBumpers();
                case 27:
                    return CreateDualSpinners();
                case 28:
                    return CreateRampTurn();
                case 29:
                    return CreateDrop();
                case 30:
                    return CreateSnake();
                default:
                    return CreateHexSteps();
            }
        }

        public static void GetRecipeConnectors(int recipe, out ConnectorType entryType, out ConnectorType exitType)
        {
            int kind = recipe % GameplayRecipeCount;

            if (kind == 2 || kind == 20)
            {
                entryType = ConnectorType.Tight3;
                exitType = ConnectorType.Tight3;
                return;
            }

            if (kind == 3)
            {
                entryType = ConnectorType.Straight8;
                exitType = ConnectorType.Tight3;
                return;
            }

            if (kind == 4)
            {
                entryType = ConnectorType.Tight3;
                exitType = ConnectorType.Straight8;
                return;
            }

            entryType = ConnectorType.Straight8;
            exitType = ConnectorType.Straight8;
        }

        public Chunk CreateStraight(string name, float length, float width, ConnectorType type, bool hasSpawn, bool hasFinish)
        {
            GameObject root = CreateRoot(name);
            Transform entry = CreateMarker(root.transform, "Entry", Vector3.zero, Quaternion.identity);
            Transform exit = CreateMarker(root.transform, "Exit", new Vector3(0f, 0f, length), Quaternion.identity);
            CreateFloor(root.transform, width, length, 0f);
            Transform spawn = hasSpawn ? CreateMarker(root.transform, "Spawn", new Vector3(0f, 1.8f, 2f), Quaternion.identity) : null;
            FinishGate finish = hasFinish ? CreateFinishGate(root.transform, length, width) : null;
            return BindChunk(root, entry, exit, type, type, finish, spawn);
        }

        public void ReleaseOwnedMaterials()
        {
            DestroyMaterial(_floorMaterial);
            DestroyMaterial(_accentMaterial);
            DestroyMaterial(_altFloorMaterial);
        }

        private static void DestroyMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(material);
                return;
            }

            Object.DestroyImmediate(material);
        }

        private Chunk CreateAdapter(string name, float entryWidth, float exitWidth, ConnectorType entryType, ConnectorType exitType)
        {
            float length = DefaultLength;
            GameObject root = CreateRoot(name);
            Transform entry = CreateMarker(root.transform, "Entry", Vector3.zero, Quaternion.identity);
            Transform exit = CreateMarker(root.transform, "Exit", new Vector3(0f, 0f, length), Quaternion.identity);
            CreateBox(root.transform, "Floor", new Vector3(0f, -FloorThickness * 0.5f, length * 0.5f), new Vector3((entryWidth + exitWidth) * 0.5f, FloorThickness, length), _floorMaterial);
            CreateBox(root.transform, "WallL", new Vector3(-entryWidth * 0.5f, 1f, 2f), new Vector3(0.4f, 2f, 4f), _accentMaterial);
            CreateBox(root.transform, "WallR", new Vector3(entryWidth * 0.5f, 1f, 2f), new Vector3(0.4f, 2f, 4f), _accentMaterial);
            return BindChunk(root, entry, exit, entryType, exitType);
        }

        private Chunk CreateGap()
        {
            float length = 12f;
            GameObject root = CreateRoot("Gap");
            Transform entry = CreateMarker(root.transform, "Entry", Vector3.zero, Quaternion.identity);
            Transform exit = CreateMarker(root.transform, "Exit", new Vector3(0f, 0f, length), Quaternion.identity);
            CreateBox(root.transform, "FloorStart", new Vector3(0f, -FloorThickness * 0.5f, 2f), new Vector3(PassageWidth, FloorThickness, 4f), _floorMaterial);
            CreateBox(root.transform, "FloorEnd", new Vector3(0f, -FloorThickness * 0.5f, 10f), new Vector3(PassageWidth, FloorThickness, 4f), _floorMaterial);
            return BindChunk(root, entry, exit, ConnectorType.Straight8, ConnectorType.Straight8);
        }

        private Chunk CreateDashWall()
        {
            Chunk chunk = CreateStraight("DashWall", DefaultLength, PassageWidth, ConnectorType.Straight8, false, false);
            GameObject wall = CreateBox(chunk.transform, "DashWall", new Vector3(0f, 1.5f, 5f), new Vector3(PassageWidth, 3f, 0.4f), _accentMaterial);
            wall.layer = PlayerMover.DashWallLayerIndex;
            return chunk;
        }

        private Chunk CreateDashLane()
        {
            Chunk chunk = CreateStraight("DashLane", 14f, PassageWidth, ConnectorType.Straight8, false, false);
            GameObject wallA = CreateBox(chunk.transform, "DashWallA", new Vector3(-1.5f, 1.5f, 5f), new Vector3(1.2f, 3f, 0.4f), _accentMaterial);
            GameObject wallB = CreateBox(chunk.transform, "DashWallB", new Vector3(1.5f, 1.5f, 9f), new Vector3(1.2f, 3f, 0.4f), _accentMaterial);
            wallA.layer = PlayerMover.DashWallLayerIndex;
            wallB.layer = PlayerMover.DashWallLayerIndex;
            return chunk;
        }

        private Chunk CreateBumper(string name, float x)
        {
            Chunk chunk = CreateStraight(name, DefaultLength, PassageWidth, ConnectorType.Straight8, false, false);
            AddBumper(chunk.transform, new Vector3(x, 0.6f, 5f), new Vector3(1.6f, 1.2f, 1.6f));
            return chunk;
        }

        private Chunk CreateBumperPair()
        {
            Chunk chunk = CreateStraight("BumperPair", DefaultLength, PassageWidth, ConnectorType.Straight8, false, false);
            AddBumper(chunk.transform, new Vector3(-2.2f, 0.6f, 4f), new Vector3(1.4f, 1.2f, 1.4f));
            AddBumper(chunk.transform, new Vector3(2.2f, 0.6f, 7f), new Vector3(1.4f, 1.2f, 1.4f));
            return chunk;
        }

        private Chunk CreateZigZagBumpers()
        {
            Chunk chunk = CreateStraight("ZigZagBumpers", 14f, PassageWidth, ConnectorType.Straight8, false, false);
            AddBumper(chunk.transform, new Vector3(-2.4f, 0.6f, 3.5f), new Vector3(2.4f, 1.2f, 1.2f));
            AddBumper(chunk.transform, new Vector3(2.4f, 0.6f, 7f), new Vector3(2.4f, 1.2f, 1.2f));
            AddBumper(chunk.transform, new Vector3(-2.4f, 0.6f, 10.5f), new Vector3(2.4f, 1.2f, 1.2f));
            return chunk;
        }

        private Chunk CreateSpinnerBar()
        {
            Chunk chunk = CreateStraight("SpinnerBar", DefaultLength, PassageWidth, ConnectorType.Straight8, false, false);
            AddSpinner(chunk.transform, "Bar", new Vector3(0f, 1f, 5f), new Vector3(7.5f, 0.5f, 0.5f), Vector3.up, 70f);
            return chunk;
        }

        private Chunk CreateSpinnerCross()
        {
            Chunk chunk = CreateStraight("SpinnerCross", DefaultLength, PassageWidth, ConnectorType.Straight8, false, false);
            Transform hub = CreateMarker(chunk.transform, "Hub", new Vector3(0f, 1f, 5f), Quaternion.identity);
            Rigidbody hubBody = hub.gameObject.AddComponent<Rigidbody>();
            hubBody.isKinematic = true;
            hubBody.useGravity = false;
            CreateBox(hub, "ArmA", Vector3.zero, new Vector3(7.5f, 0.45f, 0.45f), _accentMaterial);
            CreateBox(hub, "ArmB", Vector3.zero, new Vector3(0.45f, 0.45f, 7.5f), _accentMaterial);
            hub.gameObject.AddComponent<ChunkSpinner>().Bind(Vector3.up, 55f);
            return chunk;
        }

        private Chunk CreateDualSpinners()
        {
            Chunk chunk = CreateStraight("DualSpinners", 14f, PassageWidth, ConnectorType.Straight8, false, false);
            AddSpinner(chunk.transform, "BarA", new Vector3(0f, 1f, 4.5f), new Vector3(6.5f, 0.45f, 0.45f), Vector3.up, 80f);
            AddSpinner(chunk.transform, "BarB", new Vector3(0f, 1f, 9.5f), new Vector3(6.5f, 0.45f, 0.45f), Vector3.up, -65f);
            return chunk;
        }

        private Chunk CreatePendulum()
        {
            Chunk chunk = CreateStraight("Pendulum", DefaultLength, PassageWidth, ConnectorType.Straight8, false, false);
            AddSpinner(chunk.transform, "Hammer", new Vector3(0f, 2.2f, 5f), new Vector3(0.5f, 3.2f, 0.5f), Vector3.forward, 90f);
            return chunk;
        }

        private Chunk CreateSweep()
        {
            Chunk chunk = CreateStraight("Sweep", DefaultLength, PassageWidth, ConnectorType.Straight8, false, false);
            GameObject sweeper = CreateKinematicBox(chunk.transform, "Sweeper", new Vector3(-3f, 0.7f, 5f), new Vector3(2.2f, 1.4f, 1.4f), _accentMaterial);
            ChunkSlider slider = sweeper.AddComponent<ChunkSlider>();
            slider.Bind(new Vector3(-3f, 0.7f, 5f), new Vector3(3f, 0.7f, 5f), 2.2f);
            return chunk;
        }

        private Chunk CreateRamp(string name, float height)
        {
            float length = 12f;
            GameObject root = CreateRoot(name);
            Transform entry = CreateMarker(root.transform, "Entry", Vector3.zero, Quaternion.identity);
            Transform exit = CreateMarker(root.transform, "Exit", new Vector3(0f, height, length), Quaternion.identity);
            float angle = Mathf.Atan2(height, length) * Mathf.Rad2Deg;
            float rampLength = Mathf.Sqrt(length * length + height * height);
            CreateBox(
                root.transform,
                "Ramp",
                new Vector3(0f, height * 0.5f - FloorThickness * 0.5f, length * 0.5f),
                new Vector3(PassageWidth, FloorThickness, rampLength),
                _altFloorMaterial).transform.localRotation = Quaternion.Euler(-angle, 0f, 0f);
            return BindChunk(root, entry, exit, ConnectorType.Straight8, ConnectorType.Straight8);
        }

        private Chunk CreateStairs(string name, float height)
        {
            float length = 10f;
            int steps = 5;
            GameObject root = CreateRoot(name);
            Transform entry = CreateMarker(root.transform, "Entry", Vector3.zero, Quaternion.identity);
            Transform exit = CreateMarker(root.transform, "Exit", new Vector3(0f, height, length), Quaternion.identity);
            float stepLength = length / steps;
            float stepHeight = height / steps;

            for (int i = 0; i < steps; i++)
            {
                float y = stepHeight * (i + 1) - FloorThickness * 0.5f;
                float z = stepLength * (i + 0.5f);
                CreateBox(root.transform, "Step" + i, new Vector3(0f, y, z), new Vector3(PassageWidth, FloorThickness + Mathf.Abs(stepHeight), stepLength), _floorMaterial);
            }

            return BindChunk(root, entry, exit, ConnectorType.Straight8, ConnectorType.Straight8);
        }

        private Chunk CreateTurn(string name, float yaw, float width, ConnectorType type, float size = 8f)
        {
            GameObject root = CreateRoot(name);
            Transform entry = CreateMarker(root.transform, "Entry", Vector3.zero, Quaternion.identity);
            float sign = Mathf.Sign(yaw);
            Vector3 exitPosition = new Vector3(sign * size, 0f, size);
            Transform exit = CreateMarker(root.transform, "Exit", exitPosition, Quaternion.Euler(0f, yaw, 0f));
            CreateBox(root.transform, "FloorA", new Vector3(0f, -FloorThickness * 0.5f, size * 0.5f), new Vector3(width, FloorThickness, size), _floorMaterial);
            CreateBox(root.transform, "FloorB", new Vector3(sign * size * 0.5f, -FloorThickness * 0.5f, size), new Vector3(size, FloorThickness, width), _floorMaterial);
            return BindChunk(root, entry, exit, type, type);
        }

        private Chunk CreateRampTurn()
        {
            GameObject root = CreateRoot("RampTurn");
            Transform entry = CreateMarker(root.transform, "Entry", Vector3.zero, Quaternion.identity);
            Transform exit = CreateMarker(root.transform, "Exit", new Vector3(8f, 2.5f, 8f), Quaternion.Euler(0f, 90f, 0f));
            CreateBox(root.transform, "FloorA", new Vector3(0f, 0.6f, 4f), new Vector3(PassageWidth, FloorThickness, 8f), _altFloorMaterial);
            CreateBox(root.transform, "FloorB", new Vector3(4f, 2.2f, 8f), new Vector3(8f, FloorThickness, PassageWidth), _altFloorMaterial);
            return BindChunk(root, entry, exit, ConnectorType.Straight8, ConnectorType.Straight8);
        }

        private Chunk CreateBeam()
        {
            GameObject root = CreateRoot("Beam");
            Transform entry = CreateMarker(root.transform, "Entry", Vector3.zero, Quaternion.identity);
            Transform exit = CreateMarker(root.transform, "Exit", new Vector3(0f, 0f, 12f), Quaternion.identity);
            CreateBox(root.transform, "Start", new Vector3(0f, -FloorThickness * 0.5f, 1.5f), new Vector3(PassageWidth, FloorThickness, 3f), _floorMaterial);
            CreateBox(root.transform, "Beam", new Vector3(0f, -FloorThickness * 0.5f, 6f), new Vector3(1.2f, FloorThickness, 6f), _altFloorMaterial);
            CreateBox(root.transform, "End", new Vector3(0f, -FloorThickness * 0.5f, 10.5f), new Vector3(PassageWidth, FloorThickness, 3f), _floorMaterial);
            return BindChunk(root, entry, exit, ConnectorType.Straight8, ConnectorType.Straight8);
        }

        private Chunk CreateOffsetPlatforms()
        {
            GameObject root = CreateRoot("OffsetPlatforms");
            Transform entry = CreateMarker(root.transform, "Entry", Vector3.zero, Quaternion.identity);
            Transform exit = CreateMarker(root.transform, "Exit", new Vector3(0f, 0f, 14f), Quaternion.identity);
            CreateBox(root.transform, "A", new Vector3(-2f, -FloorThickness * 0.5f, 2f), new Vector3(4f, FloorThickness, 4f), _floorMaterial);
            CreateBox(root.transform, "B", new Vector3(2f, -FloorThickness * 0.5f, 7f), new Vector3(4f, FloorThickness, 4f), _altFloorMaterial);
            CreateBox(root.transform, "C", new Vector3(-1f, -FloorThickness * 0.5f, 12f), new Vector3(4.5f, FloorThickness, 4f), _floorMaterial);
            return BindChunk(root, entry, exit, ConnectorType.Straight8, ConnectorType.Straight8);
        }

        private Chunk CreateLowBar()
        {
            Chunk chunk = CreateStraight("LowBar", DefaultLength, PassageWidth, ConnectorType.Straight8, false, false);
            CreateBox(chunk.transform, "Bar", new Vector3(0f, 1.35f, 5f), new Vector3(PassageWidth, 0.4f, 0.4f), _accentMaterial);
            return chunk;
        }

        private Chunk CreateDrop()
        {
            GameObject root = CreateRoot("Drop");
            Transform entry = CreateMarker(root.transform, "Entry", Vector3.zero, Quaternion.identity);
            Transform exit = CreateMarker(root.transform, "Exit", new Vector3(0f, -3f, 12f), Quaternion.identity);
            CreateBox(root.transform, "High", new Vector3(0f, -FloorThickness * 0.5f, 3f), new Vector3(PassageWidth, FloorThickness, 6f), _floorMaterial);
            CreateBox(root.transform, "Low", new Vector3(0f, -3f - FloorThickness * 0.5f, 9.5f), new Vector3(PassageWidth, FloorThickness, 5f), _altFloorMaterial);
            return BindChunk(root, entry, exit, ConnectorType.Straight8, ConnectorType.Straight8);
        }

        private Chunk CreateSnake()
        {
            GameObject root = CreateRoot("Snake");
            Transform entry = CreateMarker(root.transform, "Entry", Vector3.zero, Quaternion.identity);
            Transform exit = CreateMarker(root.transform, "Exit", new Vector3(0f, 0f, 16f), Quaternion.identity);
            CreateBox(root.transform, "A", new Vector3(-2f, -FloorThickness * 0.5f, 2.5f), new Vector3(4f, FloorThickness, 5f), _floorMaterial);
            CreateBox(root.transform, "B", new Vector3(2f, -FloorThickness * 0.5f, 8f), new Vector3(4f, FloorThickness, 5f), _altFloorMaterial);
            CreateBox(root.transform, "C", new Vector3(-1.5f, -FloorThickness * 0.5f, 13.5f), new Vector3(5f, FloorThickness, 5f), _floorMaterial);
            return BindChunk(root, entry, exit, ConnectorType.Straight8, ConnectorType.Straight8);
        }

        private Chunk CreateHexSteps()
        {
            GameObject root = CreateRoot("HexSteps");
            Transform entry = CreateMarker(root.transform, "Entry", Vector3.zero, Quaternion.identity);
            Transform exit = CreateMarker(root.transform, "Exit", new Vector3(0f, 1.5f, 12f), Quaternion.identity);
            CreateBox(root.transform, "A", new Vector3(0f, -FloorThickness * 0.5f, 2f), new Vector3(4f, FloorThickness, 4f), _floorMaterial);
            CreateBox(root.transform, "B", new Vector3(0f, 0.5f, 6f), new Vector3(3.2f, FloorThickness, 3.2f), _altFloorMaterial);
            CreateBox(root.transform, "C", new Vector3(0f, 1.5f, 10f), new Vector3(4.5f, FloorThickness, 4f), _floorMaterial);
            AddSpinner(root.transform, "Bar", new Vector3(0f, 2.2f, 6f), new Vector3(5.5f, 0.4f, 0.4f), Vector3.up, 100f);
            return BindChunk(root, entry, exit, ConnectorType.Straight8, ConnectorType.Straight8);
        }

        private void CreateFloor(Transform parent, float width, float length, float y)
        {
            CreateBox(parent, "Floor", new Vector3(0f, y - FloorThickness * 0.5f, length * 0.5f), new Vector3(width, FloorThickness, length), _floorMaterial);
        }

        private void AddBumper(Transform parent, Vector3 localPosition, Vector3 size)
        {
            GameObject bumper = CreateKinematicBox(parent, "Bumper", localPosition, size, _accentMaterial);
            Pusher pusher = bumper.AddComponent<Pusher>();
            pusher.BindVisual(bumper.transform);
        }

        private void AddSpinner(Transform parent, string name, Vector3 localPosition, Vector3 size, Vector3 axis, float speed)
        {
            GameObject bar = CreateKinematicBox(parent, name, localPosition, size, _accentMaterial);
            ChunkSpinner spinner = bar.AddComponent<ChunkSpinner>();
            spinner.Bind(axis, speed);
        }

        private GameObject CreateRoot(string name)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(_parent, false);
            root.transform.localScale = Vector3.one;
            return root;
        }

        private Transform CreateMarker(Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
        {
            GameObject marker = new GameObject(name);
            Transform transform = marker.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            transform.localScale = Vector3.one;
            return transform;
        }

        private GameObject CreateBox(Transform parent, string name, Vector3 localPosition, Vector3 size, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPosition;
            box.transform.localRotation = Quaternion.identity;
            box.transform.localScale = size;
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
            return box;
        }

        private GameObject CreateKinematicBox(Transform parent, string name, Vector3 localPosition, Vector3 size, Material material)
        {
            GameObject box = CreateBox(parent, name, localPosition, size, material);
            Rigidbody body = box.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            return box;
        }

        private FinishGate CreateFinishGate(Transform parent, float length, float width)
        {
            GameObject trigger = new GameObject("Finish");
            trigger.transform.SetParent(parent, false);
            trigger.transform.localPosition = new Vector3(0f, PassageHeight * 0.5f, length - 0.5f);
            trigger.transform.localScale = Vector3.one;
            BoxCollider collider = trigger.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(width, PassageHeight, 1f);
            return trigger.AddComponent<FinishGate>();
        }

        private static Chunk BindChunk(
            GameObject root,
            Transform entry,
            Transform exit,
            ConnectorType entryType,
            ConnectorType exitType,
            FinishGate finish = null,
            Transform spawn = null)
        {
            Chunk chunk = root.AddComponent<Chunk>();
            chunk.Bind(entry, exit, entryType, exitType, finish, spawn);
            return chunk;
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            Material material = new Material(shader);
            material.color = color;

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            return material;
        }
    }
}
