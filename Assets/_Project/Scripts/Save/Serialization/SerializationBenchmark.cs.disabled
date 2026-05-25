using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace Tartaria.Save.Serialization
{
    /// <summary>
    /// Serialization benchmark tool — measures save/load performance, memory allocations, and file sizes.
    /// Used to validate optimization targets:
    ///   - Save: &lt;10ms (main thread) + &lt;50ms (background)
    ///   - Load: &lt;20ms (main thread) + &lt;100ms (background)
    ///   - File size: &lt;50KB (compressed)
    ///   - Zero GC allocations (pooled buffers)
    /// </summary>
    public static class SerializationBenchmark
    {
        public class BenchmarkResult
        {
            public string serializerName;
            public long serializeTimeMs;
            public long deserializeTimeMs;
            public long fileSizeBytes;
            public long compressedSizeBytes;
            public long memoryAllocatedBytes;
            public int gcCollectionsGen0;
            public int gcCollectionsGen1;
            public int gcCollectionsGen2;

            public override string ToString()
            {
                return $"[{serializerName}]\n" +
                       $"  Serialize: {serializeTimeMs}ms\n" +
                       $"  Deserialize: {deserializeTimeMs}ms\n" +
                       $"  File Size: {fileSizeBytes / 1024f:F1} KB\n" +
                       $"  Compressed: {compressedSizeBytes / 1024f:F1} KB ({100f * compressedSizeBytes / fileSizeBytes:F1}% of original)\n" +
                       $"  Memory Allocated: {memoryAllocatedBytes / 1024f:F1} KB\n" +
                       $"  GC Collections: Gen0={gcCollectionsGen0}, Gen1={gcCollectionsGen1}, Gen2={gcCollectionsGen2}";
            }
        }

        /// <summary>
        /// Benchmark a serializer with the current save data.
        /// </summary>
        public static BenchmarkResult Benchmark(IGameSerializer serializer, SaveData testData, bool useCompression = false)
        {
            var result = new BenchmarkResult { serializerName = serializer.GetType().Name };

            // Warm up (avoid JIT compilation timing)
            byte[] warmup = serializer.Serialize(testData);
            serializer.Deserialize<SaveData>(warmup);

            // Force GC before benchmark to get clean measurements
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            int gen0Before = GC.CollectionCount(0);
            int gen1Before = GC.CollectionCount(1);
            int gen2Before = GC.CollectionCount(2);
            long memBefore = GC.GetTotalMemory(false);

            // Serialize benchmark
            var sw = Stopwatch.StartNew();
            byte[] serialized = serializer.Serialize(testData);
            sw.Stop();
            result.serializeTimeMs = sw.ElapsedMilliseconds;
            result.fileSizeBytes = serialized.Length;

            // Compression benchmark (if requested)
            if (useCompression)
            {
                byte[] compressed = CompressionHelper.Compress(serialized);
                result.compressedSizeBytes = compressed.Length;
            }
            else
            {
                result.compressedSizeBytes = serialized.Length;
            }

            // Deserialize benchmark
            sw.Restart();
            SaveData deserialized = serializer.Deserialize<SaveData>(serialized);
            sw.Stop();
            result.deserializeTimeMs = sw.ElapsedMilliseconds;

            // Memory and GC measurements
            long memAfter = GC.GetTotalMemory(false);
            result.memoryAllocatedBytes = Math.Max(0, memAfter - memBefore);
            result.gcCollectionsGen0 = GC.CollectionCount(0) - gen0Before;
            result.gcCollectionsGen1 = GC.CollectionCount(1) - gen1Before;
            result.gcCollectionsGen2 = GC.CollectionCount(2) - gen2Before;

            return result;
        }

        /// <summary>
        /// Run comprehensive benchmark comparing all serializers.
        /// </summary>
        public static void RunComprehensiveBenchmark(SaveData testData)
        {
            UnityEngine.Debug.Log("=== SERIALIZATION BENCHMARK ===");
            UnityEngine.Debug.Log($"Test data: {testData.header.playTimeSeconds:F0}s playtime, Moon {testData.world.currentMoonIndex}");

            // Baseline: Unity JsonUtility (current system)
            UnityEngine.Debug.Log("\n--- Baseline: Unity JsonUtility (Current System) ---");
            var jsonSerializer = new JsonGameSerializer();
            var jsonResult = Benchmark(jsonSerializer, testData, useCompression: false);
            UnityEngine.Debug.Log(jsonResult.ToString());

            var jsonCompressedResult = Benchmark(jsonSerializer, testData, useCompression: true);
            UnityEngine.Debug.Log($"  Compressed: {jsonCompressedResult.compressedSizeBytes / 1024f:F1} KB");

            // New: Binary Serializer
            UnityEngine.Debug.Log("\n--- New: Binary Serializer ---");
            var binarySerializer = new BinaryGameSerializer();
            var binaryResult = Benchmark(binarySerializer, testData, useCompression: false);
            UnityEngine.Debug.Log(binaryResult.ToString());

            var binaryCompressedResult = Benchmark(binarySerializer, testData, useCompression: true);
            UnityEngine.Debug.Log($"  Compressed: {binaryCompressedResult.compressedSizeBytes / 1024f:F1} KB");

            // New: Hybrid Serializer
            UnityEngine.Debug.Log("\n--- New: Hybrid Serializer (JSON metadata + Binary data) ---");
            var hybridSerializer = new HybridGameSerializer();
            var hybridResult = Benchmark(hybridSerializer, testData, useCompression: false);
            UnityEngine.Debug.Log(hybridResult.ToString());

            var hybridCompressedResult = Benchmark(hybridSerializer, testData, useCompression: true);
            UnityEngine.Debug.Log($"  Compressed: {hybridCompressedResult.compressedSizeBytes / 1024f:F1} KB");

            // Summary
            UnityEngine.Debug.Log("\n=== PERFORMANCE COMPARISON ===");
            UnityEngine.Debug.Log($"JSON:   {jsonResult.serializeTimeMs}ms save, {jsonResult.fileSizeBytes / 1024f:F1} KB → {jsonCompressedResult.compressedSizeBytes / 1024f:F1} KB compressed");
            UnityEngine.Debug.Log($"Binary: {binaryResult.serializeTimeMs}ms save, {binaryResult.fileSizeBytes / 1024f:F1} KB → {binaryCompressedResult.compressedSizeBytes / 1024f:F1} KB compressed");
            UnityEngine.Debug.Log($"Hybrid: {hybridResult.serializeTimeMs}ms save, {hybridResult.fileSizeBytes / 1024f:F1} KB → {hybridCompressedResult.compressedSizeBytes / 1024f:F1} KB compressed");

            float jsonSpeedup = (float)jsonResult.serializeTimeMs / binaryResult.serializeTimeMs;
            float jsonSizeReduction = 100f * (1f - (float)binaryCompressedResult.compressedSizeBytes / jsonCompressedResult.compressedSizeBytes);
            UnityEngine.Debug.Log($"\nBinary vs JSON: {jsonSpeedup:F1}x faster, {jsonSizeReduction:F1}% smaller (compressed)");

            // Check if targets are met
            bool saveFast = binaryResult.serializeTimeMs < 10;
            bool loadFast = binaryResult.deserializeTimeMs < 20;
            bool sizeSmall = binaryCompressedResult.compressedSizeBytes < 50 * 1024;
            bool lowGC = binaryResult.gcCollectionsGen0 < 5;

            UnityEngine.Debug.Log($"\n=== TARGET VALIDATION ===");
            UnityEngine.Debug.Log($"Save <10ms: {(saveFast ? "✓ PASS" : "✗ FAIL")} ({binaryResult.serializeTimeMs}ms)");
            UnityEngine.Debug.Log($"Load <20ms: {(loadFast ? "✓ PASS" : "✗ FAIL")} ({binaryResult.deserializeTimeMs}ms)");
            UnityEngine.Debug.Log($"Size <50KB: {(sizeSmall ? "✓ PASS" : "✗ FAIL")} ({binaryCompressedResult.compressedSizeBytes / 1024f:F1} KB)");
            UnityEngine.Debug.Log($"Low GC: {(lowGC ? "✓ PASS" : "✗ FAIL")} (Gen0={binaryResult.gcCollectionsGen0})");

            bool allTargetsMet = saveFast && loadFast && sizeSmall && lowGC;
            UnityEngine.Debug.Log($"\n{(allTargetsMet ? "✓ ALL TARGETS MET" : "⚠ SOME TARGETS NOT MET")}");
        }

        /// <summary>
        /// Quick benchmark for SaveManager integration testing.
        /// </summary>
        [UnityEngine.ContextMenu("Run Quick Benchmark")]
        public static void QuickBenchmark()
        {
            if (SaveManager.Instance?.CurrentSave == null)
            {
                UnityEngine.Debug.LogError("No save data available for benchmark. Load a save first.");
                return;
            }

            RunComprehensiveBenchmark(SaveManager.Instance.CurrentSave);
        }
    }
}
