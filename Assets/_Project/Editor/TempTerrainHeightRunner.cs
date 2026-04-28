using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class TempTerrainHeightRunner
{
    public static void ComputeTerrainHeights()
    {
        var points = new (float x, float z)[]
        {
            (15f, 10f), (-5f, 20f), (20f, -15f), (-15f, -10f),
            (5f, 30f), (40f, 15f), (-25f, 25f), (10f, -25f)
        };

        var lines = new List<string> { "x\tz\tterrainHeight\tshardY\tdelta" };
        foreach (var p in points)
        {
            float px = p.x;
            float pz = p.z;
            float h = 0f;
            h += Mathf.Sin(px * 0.03f) * Mathf.Cos(pz * 0.04f) * 2f;
            h += Mathf.Sin(px * 0.07f + 1.3f) * Mathf.Sin(pz * 0.06f + 0.8f) * 0.8f;
            h += Mathf.PerlinNoise(px * 0.02f + 100f, pz * 0.02f + 100f) * 3f;
            float distCenter = Mathf.Sqrt(px * px + pz * pz);
            float plazaBlend = Mathf.Clamp01((distCenter - 15f) / 10f);
            h *= plazaBlend;
            float angle = Mathf.Atan2(pz, px);
            float pathWave = Mathf.Abs(Mathf.Sin(angle * 2f));
            if (pathWave < 0.15f && distCenter > 10f)
                h -= 0.3f;

            float delta = 0.8f - h;
            lines.Add(string.Join("\t", new[]
            {
                px.ToString("0.######", CultureInfo.InvariantCulture),
                pz.ToString("0.######", CultureInfo.InvariantCulture),
                h.ToString("0.######", CultureInfo.InvariantCulture),
                0.8f.ToString("0.######", CultureInfo.InvariantCulture),
                delta.ToString("0.######", CultureInfo.InvariantCulture)
            }));
        }

        var projectRoot = Directory.GetParent(Application.dataPath).FullName;
        File.WriteAllLines(Path.Combine(projectRoot, "temp_terrain_heights.tsv"), lines);
        Debug.Log("temp_terrain_heights.tsv written");
    }
}
