using System.Collections.Generic;
using UnityEngine;

namespace SlipUp
{
    public class TrackGenerator : MonoBehaviour
    {
        private const int MinGameplayChunks = 8;
        private const int MaxGameplayChunks = 15;

        private readonly List<Chunk> _spawned = new List<Chunk>();
        private readonly List<int> _compatible = new List<int>();
        private ChunkFactory _factory;
        private bool _legacyHidden;

        internal int Seed { get; private set; }

        internal Vector3 SpawnPosition { get; private set; }

        internal Quaternion SpawnRotation { get; private set; }

        internal float KillHeight { get; private set; }

        internal Chunk[] BuildPair(Transform parent)
        {
            ChunkFactory factory = new ChunkFactory(parent);
            Chunk start = factory.CreateStart();
            Chunk next = factory.CreateStraightB();
            start.transform.SetPositionAndRotation(parent.position, parent.rotation);
            start.transform.localScale = Vector3.one;
            ChunkPlacement.AlignToExit(next.transform, next.Entry, start.Exit);
            return new[] { start, next };
        }

        internal Chunk[] Build(int seed)
        {
            HideLegacy();
            ClearSpawned();
            Seed = seed;
            _factory = new ChunkFactory(transform);
            System.Random random = new System.Random(seed);
            int gameplayCount = random.Next(MinGameplayChunks, MaxGameplayChunks + 1);
            Chunk start = Place(_factory.CreateStart(), transform.position, transform.rotation);
            _spawned.Add(start);
            Chunk previous = start;

            for (int i = 0; i < gameplayCount; i++)
            {
                Chunk next = PlaceCompatible(previous, random, false);
                _spawned.Add(next);
                previous = next;
            }

            Chunk finish = PlaceCompatible(previous, random, true);
            _spawned.Add(finish);
            RememberSpawn(start);
            RememberKillHeight();
            Chunk[] chunks = _spawned.ToArray();
            return chunks;
        }

        private void OnDestroy()
        {
            ClearSpawned();
            _factory = null;
        }

        private Chunk PlaceCompatible(Chunk previous, System.Random random, bool isFinish)
        {
            if (isFinish)
            {
                if (previous.ExitType != ConnectorType.Straight8)
                {
                    Chunk adapter = PlaceRecipe(previous, 4);
                    previous = adapter;
                    _spawned.Add(adapter);
                }

                Chunk finish = _factory.CreateFinish();
                ChunkPlacement.AlignToExit(finish.transform, finish.Entry, previous.Exit);
                return finish;
            }

            CollectCompatible(previous.ExitType);

            if (_compatible.Count == 0)
            {
                CollectCompatible(ConnectorType.Straight8);
            }

            int recipe = _compatible[random.Next(_compatible.Count)];
            return PlaceRecipe(previous, recipe);
        }

        private Chunk PlaceRecipe(Chunk previous, int recipe)
        {
            Chunk chunk = _factory.CreateGameplay(recipe);
            ChunkPlacement.AlignToExit(chunk.transform, chunk.Entry, previous.Exit);
            return chunk;
        }

        private void CollectCompatible(ConnectorType exitType)
        {
            _compatible.Clear();

            for (int recipe = 0; recipe < ChunkFactory.GameplayRecipeCount; recipe++)
            {
                ChunkFactory.GetRecipeConnectors(recipe, out ConnectorType entryType, out ConnectorType _);

                if (ChunkPlacement.AreCompatible(exitType, entryType) == false)
                {
                    continue;
                }

                _compatible.Add(recipe);
            }
        }

        private Chunk Place(Chunk chunk, Vector3 position, Quaternion rotation)
        {
            chunk.transform.SetPositionAndRotation(position, rotation);
            chunk.transform.localScale = Vector3.one;
            return chunk;
        }

        private void RememberSpawn(Chunk start)
        {
            Transform spawn = start.SpawnPoint;

            if (spawn == null)
            {
                SpawnPosition = start.Entry.position + Vector3.up * 1.8f + start.Entry.forward * 2f;
                SpawnRotation = start.Entry.rotation;
                return;
            }

            SpawnPosition = spawn.position;
            SpawnRotation = spawn.rotation;
        }

        private void RememberKillHeight()
        {
            float minY = transform.position.y;

            for (int i = 0; i < _spawned.Count; i++)
            {
                Transform exit = _spawned[i].Exit;

                if (exit.position.y < minY)
                {
                    minY = exit.position.y;
                }
            }

            KillHeight = minY - 8f;
        }

        private void HideLegacy()
        {
            if (_legacyHidden)
            {
                return;
            }

            _legacyHidden = true;

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void ClearSpawned()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] == null)
                {
                    continue;
                }

                DestroyChunk(_spawned[i].gameObject);
            }

            _spawned.Clear();

            if (_factory == null)
            {
                return;
            }

            _factory.ReleaseOwnedMaterials();
            _factory = null;
        }

        private static void DestroyChunk(GameObject chunk)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(chunk);
                return;
            }

            Object.DestroyImmediate(chunk);
        }
    }
}
