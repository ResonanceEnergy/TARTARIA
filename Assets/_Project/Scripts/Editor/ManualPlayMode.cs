using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    public static class ManualPlayMode
    {
        [MenuItem("Tartaria/ENTER PLAY MODE %#p", priority = 0)]
        public static void EnterPlayMode()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.Log("[ManualPlayMode] Entering Play mode via menu...");
                EditorApplication.isPlaying = true;
            }
            else
            {
                Debug.Log("[ManualPlayMode] Already in Play mode");
            }
        }
    }
}
