using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 PowerUps
    /// TODO: Implement Moon 1 specific logic
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon1PowerUps : MonoBehaviour
    {
        [Header("Moon 1 PowerUps Settings")]
        [SerializeField] bool isActive = true;
        
        void Start()
        {
            Initialize();
        }
        
        void Initialize()
        {
            if (!isActive) return;
            
            Debug.Log("[[Moon1PowerUps]] ✅ Initialized!");
            
            // TODO: Implement Moon 1 PowerUps logic
        }
    }
}
