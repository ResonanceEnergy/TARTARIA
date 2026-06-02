using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

// NOTE: namespace is intentionally Tartaria.Core.ContentLoading (not .Addressables)
// to avoid shadowing UnityEngine.AddressableAssets.Addressables for sibling files
// in namespace Tartaria.Core (e.g. AddressableAssetLoader.cs which uses the
// unqualified `Addressables` identifier). Folder name is organizational only.
namespace Tartaria.Core.ContentLoading
{
    /// <summary>
    /// AddressableContentLoader --- thin task-based loader for Moon 1 hero content
    /// with a Resources.Load fallback so callers never hard-fail when an address
    /// is unmapped (e.g. before the Moon1Content Addressables group is authored).
    ///
    /// PRIMARY: UnityEngine.AddressableAssets.Addressables.LoadAssetAsync&lt;GameObject&gt;(id)
    /// FALLBACK: Resources.Load&lt;GameObject&gt;($"Moon1/Heroes/{id}")
    ///
    /// Cowork follow-up (NOT in this PR --- requires Editor work on .asset files):
    ///   Window &gt; Asset Management &gt; Addressables &gt; Groups
    ///     - Create group "Moon1Content"
    ///     - Drag Assets/_Project/Prefabs/Moon1/** into it
    ///     - Set each entry's address to its hero id (e.g. "Cathedral", "Fountain", "Spire")
    ///     - Build content (Build &gt; New Build &gt; Default Build Script)
    ///
    /// See also: Tartaria.Core.AddressableAssetLoader (broader group/label wrapper,
    /// streaming-ring helpers). This class is intentionally minimal --- one method,
    /// one fallback --- so spawner sites can swap Resources.Load() one-for-one.
    /// </summary>
    public static class AddressableContentLoader
    {
        const string FALLBACK_PATH_PREFIX = "Moon1/Heroes/";

        /// <summary>
        /// Load a Moon 1 hero prefab by id. Tries Addressables first; on any
        /// failure (unmapped key, addressables not initialized, package issue),
        /// falls back to Resources.Load. Returns null only if both paths fail.
        /// </summary>
        /// <param name="id">Hero id, e.g. "Cathedral", "Fountain", "Spire".
        /// Used as both the Addressables key and the Resources subpath.</param>
        public static async Task<GameObject> LoadHero(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("[AddressableContentLoader] LoadHero called with null/empty id.");
                return null;
            }

            // PRIMARY: Addressables
            try
            {
                AsyncOperationHandle<GameObject> handle =
                    UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(id);
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    Debug.Log($"[AddressableContentLoader] LoadHero('{id}') resolved via Addressables group");
                    return handle.Result;
                }
                Debug.LogWarning(
                    $"[AddressableContentLoader] Addressables miss for id='{id}' " +
                    $"(status={handle.Status}). Falling back to Resources.Load.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[AddressableContentLoader] Addressables threw for id='{id}': " +
                    $"{ex.GetType().Name}: {ex.Message}. Falling back to Resources.Load.");
            }

            // FALLBACK: Resources
            string resourcesPath = FALLBACK_PATH_PREFIX + id;
            GameObject fallback = Resources.Load<GameObject>(resourcesPath);
            if (fallback == null)
            {
                Debug.LogError($"[AddressableContentLoader] LoadHero('{id}') returned null --- id not registered anywhere");
                Debug.LogError(
                    $"[AddressableContentLoader] LoadHero('{id}') failed --- neither " +
                    $"Addressables key nor Resources/{resourcesPath} resolved. " +
                    $"Cowork: create Moon1Content Addressables group or place a " +
                    $"prefab at Assets/_Project/Resources/{resourcesPath}.prefab");
            }
            else
            {
                Debug.LogWarning($"[AddressableContentLoader] LoadHero('{id}') fell back to Resources.Load --- Addressables group entry missing for this id");
            }
            return fallback;
        }

        /// <summary>
        /// Synchronous best-effort variant for legacy call sites that cannot easily
        /// await. Blocks on the Addressables task; prefer LoadHero() (async) in new code.
        /// Falls through to Resources.Load on any exception.
        /// </summary>
        public static GameObject LoadHeroBlocking(string id)
        {
            try
            {
                // NOTE: LoadHero() emits the canonical route logs
                // ("resolved via Addressables group" / "fell back to Resources.Load" /
                // "returned null --- id not registered anywhere") --- LoadHeroBlocking
                // only adds the exception-path fallback below.
                Task<GameObject> task = LoadHero(id);
                task.Wait();
                return task.Result;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[AddressableContentLoader] LoadHeroBlocking('{id}') exception: " +
                    $"{ex.GetType().Name}: {ex.Message}. Trying Resources directly.");
                GameObject fallback = Resources.Load<GameObject>(FALLBACK_PATH_PREFIX + id);
                if (fallback == null)
                {
                    Debug.LogError($"[AddressableContentLoader] LoadHero('{id}') returned null --- id not registered anywhere");
                }
                else
                {
                    Debug.LogWarning($"[AddressableContentLoader] LoadHero('{id}') fell back to Resources.Load --- Addressables group entry missing for this id");
                }
                return fallback;
            }
        }
    }
}
