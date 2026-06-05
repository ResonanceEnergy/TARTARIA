// File: Assets/_Project/Scripts/Integration/Moon1ExcavationSites.cs
using System;
using UnityEngine;

namespace Tartaria.Integration
{
    public class Moon1ExcavationSites : MonoBehaviour
    {
        private const string WORKSHOP_FBX = "anvil.fbx";
        private const string ARCHITECTURE_FBX = "blueprint.fbx";
        private const string TOOL_CACHE_FBX = "chisel.fbx";
        private const string CEREMONIAL_FBX = "anvil.fbx";

        private const float SITE_RADIUS = 2.5f;
        private const float SITE_CIRCLE_RADIUS = 1.5f;
        private const float SITE_TRIGGER_RADIUS = 4f;

        private const float Y_ROTATION_RANGE = 90f;
        private const float TILT_RANGE = 0.3f;

        private void Start()
        {
            if (transform.childCount > 0) return;

            var workshopPos = new Vector3(-12, 0, 8);
            var architectTablePos = new Vector3(8, 0, 12);
            var toolCachePos = new Vector3(15, 0, -8);
            var ceremonialPos = new Vector3(-10, 0, -14);

            SpawnSite(workshopPos, WORKSHOP_FBX);
            SpawnSite(architectTablePos, ARCHITECTURE_FBX);
            SpawnSite(toolCachePos, TOOL_CACHE_FBX);
            SpawnSite(ceremonialPos, CEREMONIAL_FBX);
        }

        private void SpawnSite(Vector3 pos, string fbxName)
        {
            var excavationSite = Instantiate<GameObject>(transform);
            excavationSite.name = $"Excavation_{pos.x}_{pos.y}_{pos.z}_theme";

            var circleCollider = new GameObject("CircleCollider");
            circleCollider.transform.SetParent(excavationSite.transform);
            circleCollider.AddComponent<CircleCollider>();
            circleCollider.radius = SITE_CIRCLE_RADIUS;

            var triggerCollider = new GameObject("TriggerCollider");
            triggerCollider.transform.SetParent(excavationSite.transform);
            triggerCollider.AddComponent<SphereCollider>();
            triggerCollider.radius = SITE_TRIGGER_RADIUS;

            var dirtCircle = Instantiate<GameObject>("DirtCircle");
            dirtCircle.transform.SetParent(excavationSite.transform);
            dirtCircle.AddComponent<CircleCollider>();
            dirtCircle.radius = SITE_CIRCLE_RADIUS;
            dirtCircle.GetComponent<Renderer>().material.color = new Color(0.30f, 0.22f, 0.14f);

            var props = new GameObject("Props");
            props.transform.SetParent(excavationSite.transform);
            props.AddComponent<Collider>();

            SpawnProp(props.transform, fbxName, Vector3.zero, Random.Range(-Y_ROTATION_RANGE, Y_ROTATION_RANGE), Random.Range(-TILT_RANGE, TILT_RANGE));
            SpawnProp(props.transform, fbxName, Vector3.zero, Random.Range(-Y_ROTATION_RANGE, Y_ROTATION_RANGE), Random.Range(-TILT_RANGE, TILT_RANGE));
            SpawnProp(props.transform, fbxName, Vector3.zero, Random.Range(-Y_ROTATION_RANGE, Y_ROTATION_RANGE), Random.Range(-TILT_RANGE, TILT_RANGE));
            SpawnProp(props.transform, fbxName, Vector3.zero, Random.Range(-Y_ROTATION_RANGE, Y_ROTATION_RANGE), Random.Range(-TILT_RANGE, TILT_RANGE));
        }
    }
}
