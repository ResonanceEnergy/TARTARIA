using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 MaterialSetup
    /// TODO: Implement Moon 2 specific logic
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon2MaterialSetup : MonoBehaviour
    {
        [Header("Moon 2 MaterialSetup Settings")]
        [SerializeField] bool isActive = true;
        
        void Start()
        {
            Initialize();
        }
        
        void Initialize()
        {
            if (!isActive) return;
            
            Debug.Log("[[Moon2MaterialSetup]] ✅ Initialized!");
            
            // TODO: Implement Moon 2 MaterialSetup logic
        }
    }
}
