using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 InteractiveObjects
    /// TODO: Implement Moon 1 specific logic
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon1InteractiveObjects : MonoBehaviour
    {
        [Header("Moon 1 InteractiveObjects Settings")]
        [SerializeField] bool isActive = true;
        
        void Start()
        {
            Initialize();
        }
        
        void Initialize()
        {
            if (!isActive) return;
            
            Debug.Log("[[Moon1InteractiveObjects]] ✅ Initialized!");
            
            // TODO: Implement Moon 1 InteractiveObjects logic
        }
    }
}
