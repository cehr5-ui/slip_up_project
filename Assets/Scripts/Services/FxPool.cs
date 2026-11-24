using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace SlipUp
{
    public class FxPool : MonoBehaviour
    {
        private const int PoolSize = 16;
        private const float Lifetime = 0.6f;

        internal static FxPool Current { get; private set; }

        private ObjectPool<ParticleSystem> _pool;

        private void Awake()
        {
            Current = this;
            _pool = new ObjectPool<ParticleSystem>(Create, OnGet, OnRelease, OnDestroyPooled, true, PoolSize);
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }

            if (_pool == null)
            {
                return;
            }

            _pool.Dispose();
            _pool = null;
        }

        internal void Play(Vector3 position, Color color, int count = 14)
        {
            ParticleSystem system = _pool.Get();
            system.transform.position = position;
            ParticleSystem.MainModule main = system.main;
            main.startColor = color;
            ParticleSystem.EmissionModule emission = system.emission;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, (short)count));
            system.Play(true);
            StartCoroutine(ReleaseAfter(system));
        }

        private IEnumerator ReleaseAfter(ParticleSystem system)
        {
            yield return new WaitForSeconds(Lifetime);
            _pool.Release(system);
        }

        private ParticleSystem Create()
        {
            GameObject gameObject = new GameObject("Fx");
            gameObject.transform.SetParent(transform, false);
            ParticleSystem system = gameObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = system.main;
            main.duration = Lifetime;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 0.45f;
            main.startSpeed = 4.5f;
            main.startSize = 0.16f;
            main.gravityModifier = 0.6f;
            main.maxParticles = 24;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 14) });
            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;
            ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateParticleMaterial();
            gameObject.SetActive(false);
            return system;
        }

        private static void OnGet(ParticleSystem system)
        {
            system.gameObject.SetActive(true);
        }

        private static void OnRelease(ParticleSystem system)
        {
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            system.gameObject.SetActive(false);
        }

        private static void OnDestroyPooled(ParticleSystem system)
        {
            if (system == null)
            {
                return;
            }

            Destroy(system.gameObject);
        }

        private static Material CreateParticleMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

            if (shader == null)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            Material material = new Material(shader);
            material.color = Color.white;
            return material;
        }
    }
}
