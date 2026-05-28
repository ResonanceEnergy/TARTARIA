using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Collectibles
    /// TODO: Implement Moon 1 specific logic
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon1Collectibles : MonoBehaviour
    {
        [Header("Moon 1 Collectibles Settings")]
        [SerializeField] bool isActive = true;
        
        void Start()
        {
            Initialize();
        }
        
        void Initialize()
        {
            if (!isActive) return;
            
            Debug.Log("[[Moon1Collectibles]] ✅ Initialized!");
            
            // TODO: Implement Moon 1 Collectibles logic
        }
    }
}
