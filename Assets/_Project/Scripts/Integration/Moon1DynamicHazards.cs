using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 DynamicHazards
    /// TODO: Implement Moon 1 specific logic
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon1DynamicHazards : MonoBehaviour
    {
        [Header("Moon 1 DynamicHazards Settings")]
        [SerializeField] bool isActive = true;
        
        void Start()
        {
            Initialize();
        }
        
        void Initialize()
        {
            if (!isActive) return;
            
            Debug.Log("[[Moon1DynamicHazards]] ✅ Initialized!");
            
            // TODO: Implement Moon 1 DynamicHazards logic
        }
    }
}
