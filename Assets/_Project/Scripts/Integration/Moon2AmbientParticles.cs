using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Integration
{
    [DefaultExecutionOrder(-81)]
    public class Moon2AmbientParticles : MonoBehaviour
    {
        [SerializeField] GameObject crystalSparkPrefab;
        [SerializeField] GameObject caveMistPrefab;
        [SerializeField] GameObject biolumSporePrefab;
        
        [SerializeField] int maxCrystalSparks = 40;
        [SerializeField] int maxMistPatches = 25;
        [SerializeField] int maxSpores = 30;
        
        readonly List<GameObject> _particles = new();
        
        void Start()
        {
            SpawnCrystalSparks();
            SpawnCaveMist();
            SpawnBiolumSpores();
            
            Debug.Log($"[Moon2AmbientParticles] ✅ {_particles.Count} particle systems spawned");
        }
        
        void SpawnCrystalSparks()
        {
            for (int i = 0; i < maxCrystalSparks; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-50f, 50f), Random.Range(0f, 20f), Random.Range(-50f, 50f));
                GameObject spark = crystalSparkPrefab != null ?
                    Instantiate(crystalSparkPrefab, pos, Quaternion.identity, transform) :
                    CreateProceduralSpark(pos);
                _particles.Add(spark);
            }
        }
        
        void SpawnCaveMist()
        {
            for (int i = 0; i < maxMistPatches; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-50f, 50f), 0.5f, Random.Range(-50f, 50f));
                GameObject mist = caveMistPrefab != null ?
                    Instantiate(caveMistPrefab, pos, Quaternion.identity, transform) :
                    CreateProceduralMist(pos);
                _particles.Add(mist);
            }
        }
        
        void SpawnBiolumSpores()
        {
            for (int i = 0; i < maxSpores; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-50f, 50f), Random.Range(2f, 15f), Random.Range(-50f, 50f));
                GameObject spore = biolumSporePrefab != null ?
                    Instantiate(biolumSporePrefab, pos, Quaternion.identity, transform) :
                    CreateProceduralSpore(pos);
                _particles.Add(spore);
            }
        }
        
        GameObject CreateProceduralSpark(Vector3 pos)
        {
            GameObject obj = new GameObject("CrystalSpark");
            obj.transform.position = pos;
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.6f, 0.2f, 0.9f);
            main.startLifetime = 2f;
            main.startSpeed = 0.5f;
            main.startSize = 0.05f;
            var emission = ps.emission;
            emission.rateOverTime = 3f;
            return obj;
        }
        
        GameObject CreateProceduralMist(Vector3 pos)
        {
            GameObject obj = new GameObject("CaveMist");
            obj.transform.position = pos;
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(0.2f, 0.2f, 0.3f, 0.3f);
            main.startLifetime = 5f;
            main.startSpeed = 0.2f;
            main.startSize = 3f;
            var emission = ps.emission;
            emission.rateOverTime = 2f;
            return obj;
        }
        
        GameObject CreateProceduralSpore(Vector3 pos)
        {
            GameObject obj = new GameObject("BiolumSpore");
            obj.transform.position = pos;
            Light light = obj.AddComponent<Light>();
            light.color = new Color(0.2f, 0.8f, 0.6f);
            light.range = 3f;
            light.intensity = 0.8f;
            return obj;
        }
    }
}
