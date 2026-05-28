using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 InteractiveObjects
    /// TODO: Implement Moon 2 specific logic
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon2InteractiveObjects : MonoBehaviour
    {
        [Header("Moon 2 InteractiveObjects Settings")]
        [SerializeField] bool isActive = true;
        
        void Start()
        {
            Initialize();
        }
        
        void Initialize()
        {
            if (!isActive) return;
            
            Debug.Log("[[Moon2InteractiveObjects]] ✅ Initialized!");
            
            // TODO: Implement Moon 2 InteractiveObjects logic
        }
    }
}
