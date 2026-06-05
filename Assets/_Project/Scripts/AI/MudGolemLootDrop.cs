// File: Assets/_Project/Scripts/AI/MudGolemLootDrop.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Tartaria.AI
{
    public class MudGolemLootDrop : MonoBehaviour
    {
        private Vector3 _spawnPosition;

        private void OnEnable()
        {
            _spawnPosition = transform.position;
        }

        public void DropLoot(GameObject killer = null)
        {
            var shardCount = Random.Range(2, 5);
            for (int i = 0; i < shardCount; i++)
            {
                var shard = Instantiate(_LootShardPrefab, _spawnPosition + Vector3.up * 0.5f, Quaternion.identity);
                shard.GetComponent<Rigidbody>().mass = 0.4f;
                shard.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-2, 2), Random.Range(2, 4), Random.Range(-2, 2)), ForceMode.Impulse);
                shard.AddComponent<SphereCollider>();
                shard.GetComponent<SphereCollider>().radius = 0.4f;
            }

            var rsCoin = Instantiate(_LootRSCoinPrefab, _spawnPosition + Vector3.up * 0.5f, Quaternion.identity);
            rsCoin.GetComponent<Rigidbody>().mass = 0.3f;
            rsCoin.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-2, 2), Random.Range(2, 4), Random.Range(-2, 2)), ForceMode.Impulse);
            rsCoin.AddComponent<SphereCollider>();
            rsCoin.GetComponent<SphereCollider>().radius = 0.6f;

            rsCoin.GetComponent<_LootRSCoin>().OnTriggerEnter += OnRSCoinTriggerEnter;
        }

        private void OnRSCoinTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Tartaria.Core.GameEvents.FireRSChange(8f);
            ServiceLocator.Audio?.PlaySFX("RSCollect", _spawnPosition, 0.7f);
            Destroy(rsCoin);
        }

        private GameObject _LootShardPrefab;
        private GameObject _LootRSCoinPrefab;

        private void Start()
        {
            _LootShardPrefab = Resources.Load<GameObject>("Enemies/MudGolem/Blender/Plates/Shards/ClayShard");
            _LootRSCoinPrefab = Resources.Load<GameObject>("Enemies/MudGolem/Blender/Plates/Shards/RSCoin");
        }
    }
}
