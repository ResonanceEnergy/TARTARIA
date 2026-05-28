using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 DynamicHazards
    /// TODO: Implement Moon 2 specific logic
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon2DynamicHazards : MonoBehaviour
    {
        [Header("Moon 2 DynamicHazards Settings")]
        [SerializeField] bool isActive = true;
        
        void Start()
        {
            Initialize();
        }
        
        void Initialize()
        {
            if (!isActive) return;
            
            Debug.Log("[[Moon2DynamicHazards]] ✅ Initialized!");
            
            // TODO: Implement Moon 2 DynamicHazards logic
        }
    }
}
