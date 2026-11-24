using UnityEngine;

namespace SlipUp
{
    public sealed class ChunkFactory
    {
        public const float PassageWidth = 8f;
        public const float TightWidth = 3f;
        public const float PassageHeight = 3f;
        public const float FloorThickness = 0.2f;
        public const float DefaultLength = 10f;

        private readonly Transform _parent;
        private readonly Material _floorMaterial;
        private readonly Material _accentMaterial;

        public ChunkFactory(Transform parent)
        {
            _parent = parent;
            _floorMaterial = CreateMaterial(new Color(0.35f, 0.7f, 0.42f, 1f));
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

        public Chunk CreateStraight(string name, float length, float width, ConnectorType type, bool hasSpawn, bool hasFinish)
        {
            GameObject root = CreateRoot(name);
            Transform entry = CreateMarker(root.transform, "Entry", Vector3.zero, Quaternion.identity);
            Transform exit = CreateMarker(root.transform, "Exit", new Vector3(0f, 0f, length), Quaternion.identity);
            CreateBox(
                root.transform,
                "Floor",
                new Vector3(0f, -FloorThickness * 0.5f, length * 0.5f),
                new Vector3(width, FloorThickness, length),
                _floorMaterial);

            Transform spawn = null;

            if (hasSpawn)
            {
                spawn = CreateMarker(root.transform, "Spawn", new Vector3(0f, 1.8f, 2f), Quaternion.identity);
            }

            FinishGate finish = null;

            if (hasFinish)
            {
                finish = CreateFinishGate(root.transform, length);
            }

            Chunk chunk = root.AddComponent<Chunk>();
            chunk.Bind(entry, exit, type, type, finish, spawn);
            return chunk;
        }

        public void ReleaseOwnedMaterials()
        {
            Object.Destroy(_floorMaterial);
            Object.Destroy(_accentMaterial);
        }

        internal Material FloorMaterial => _floorMaterial;

        internal Material AccentMaterial => _accentMaterial;

        internal GameObject CreateRoot(string name)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(_parent, false);
            root.transform.localScale = Vector3.one;
            return root;
        }

        internal Transform CreateMarker(Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
        {
            GameObject marker = new GameObject(name);
            Transform transform = marker.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            transform.localScale = Vector3.one;
            return transform;
        }

        internal GameObject CreateBox(Transform parent, string name, Vector3 localPosition, Vector3 size, Material material)
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

        internal FinishGate CreateFinishGate(Transform parent, float length)
        {
            GameObject trigger = new GameObject("Finish");
            trigger.transform.SetParent(parent, false);
            trigger.transform.localPosition = new Vector3(0f, PassageHeight * 0.5f, length - 0.5f);
            trigger.transform.localScale = Vector3.one;
            BoxCollider collider = trigger.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(PassageWidth, PassageHeight, 1f);
            return trigger.AddComponent<FinishGate>();
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
