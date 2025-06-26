using System.Collections.Generic;
using UnityEngine;

public class HeatmapGenerator : MonoBehaviour
{
    [Header("Input")]
    public TextAsset csvFile;
    public int textureWidth = 256;
    public int textureHeight = 256;

    [Header("Store Dimensions (world units)")]
    [Tooltip("The total width of the store (x-axis); CSV x values are expected between -storeWidth/2 and storeWidth/2")]
    public float storeWidth = 40f;
    [Tooltip("The total height of the store (z-axis); CSV z values are expected between -storeHeight/2 and storeHeight/2")]
    public float storeHeight = 25f;

    [Header("Position Offset (world units)")]
    [Tooltip("Offset to add to the CSV x (x component) and CSV z (y component) positions before mapping")]
    public Vector2 positionOffset = Vector2.zero;

    [Header("Rendering")]
    public Gradient heatmapGradient;
    public Renderer targetRenderer;

    [Header("Heatmap Settings")]
    [Tooltip("Adjust contrast for visual clarity; lower values exaggerate differences.")]
    [Range(0.1f, 2f)]
    public float heatmapContrast = 0.5f;

    [ContextMenu("Generate Heatmap")]
    public void GenerateHeatmap()
    {
        if (csvFile == null)
        {
            Debug.LogError("CSV file not assigned!");
            return;
        }

        // Initialize a heatmap grid, one entry per pixel.
        int[,] heatmap = new int[textureWidth, textureHeight];
        string[] lines = csvFile.text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        // Calculate center pixel positions.
        int centerX = textureWidth / 2;
        int centerY = textureHeight / 2;

        // Loop through each line (each line should be "x,y,z")
        foreach (string line in lines)
        {
            // Skip any header lines (if they exist)
            if (line.StartsWith("Shopper"))
                continue;

            string[] parts = line.Trim().Split(',');
            if (parts.Length != 3)
                continue;

            // Parse x and z from CSV (ignoring y)
            if (float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[2], out float z))
            {
                // Apply the offset to the x and z values.
                x += positionOffset.x;
                z += positionOffset.y;

                // Map positions: (0,0) in CSV becomes center of texture.
                int px = centerX + Mathf.FloorToInt(x * textureWidth / storeWidth);
                int py = centerY + Mathf.FloorToInt(z * textureHeight / storeHeight);

                // Clamp to ensure indices remain valid.
                px = Mathf.Clamp(px, 0, textureWidth - 1);
                py = Mathf.Clamp(py, 0, textureHeight - 1);

                heatmap[px, py]++;
            }
        }

        // Determine the maximum count in the heatmap for normalization.
        int maxCount = 1;
        for (int i = 0; i < textureWidth; i++)
        {
            for (int j = 0; j < textureHeight; j++)
            {
                if (heatmap[i, j] > maxCount)
                    maxCount = heatmap[i, j];
            }
        }

        // Create the texture and color it based on heatmap data.
        Texture2D texture = new Texture2D(textureWidth, textureHeight);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        // Loop over each pixel (no need to flip vertically as mapping is center based)
        for (int i = 0; i < textureWidth; i++)
        {
            for (int j = 0; j < textureHeight; j++)
            {
                // Normalize and adjust with power curve for contrast
                float normalized = (float)heatmap[i, j] / maxCount;
                float adjusted = Mathf.Pow(normalized, heatmapContrast);
                Color pixelColor = heatmapGradient.Evaluate(adjusted);
                texture.SetPixel(i, j, pixelColor);
            }
        }

        texture.Apply();

        if (targetRenderer != null)
        {
            targetRenderer.material.mainTexture = texture;
        }
        Debug.Log("Heatmap generated. Max count: " + maxCount);
    }
}
