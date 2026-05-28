using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 NPCDialogues
    /// TODO: Implement Moon 1 specific logic
    /// </summary>
    [DefaultExecutionOrder(-81)]
    public class Moon1NPCDialogues : MonoBehaviour
    {
        [Header("Moon 1 NPCDialogues Settings")]
        [SerializeField] bool isActive = true;
        
        void Start()
        {
            Initialize();
        }
        
        void Initialize()
        {
            if (!isActive) return;
            
            Debug.Log("[[Moon1NPCDialogues]] ✅ Initialized!");
            
            // TODO: Implement Moon 1 NPCDialogues logic
        }
    }
}
