using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEffect : MonoBehaviour
{
    // Width and height of the texture in pixels.
    public int textureWidth = 200;
    public int textureHeight = 200;

    // The origin of the sampled area in the plane.
    public float xOrg;
    public float yOrg;

    public float effectSpeed = 0.5f;

    // The number of cycles of the basic noise pattern that are repeated
    // over the width and height of the texture.
    public float scale = 10.0F;
    
    public Color baseColor;
    public bool useBaseColor = true;

    private Texture2D normalTex;
    private Texture2D colorTex;
    private Color[] normalPix;
    private Color[] colorPix;
    private Renderer rend;
    private float effectTimer;

    void Start()
    {
        Application.targetFrameRate = 60;
        rend = GetComponent<Renderer>();

        effectTimer = 0.0f;

        // Set up the texture and a Color array to hold pixels during processing.
        normalTex = new Texture2D(textureWidth, textureHeight);
        normalPix = new Color[normalTex.width * normalTex.height];
        colorTex = new Texture2D(textureWidth, textureHeight);
        colorPix = new Color[normalTex.width * normalTex.height];
        rend.material.EnableKeyword("_NORMALMAP");
        rend.material.SetTexture("_BumpMap", normalTex);
        rend.material.SetTexture("_MainTex", colorTex);
        CalcNoise();
    }

    void CalcNoise()
    {
        if (useBaseColor) {
            // For each pixel in the texture...
            float y = 0.0F;

            while (y < normalTex.height)
            {
                float x = 0.0F;
                while (x < normalTex.width)
                {
                    float xCoord = (xOrg + x / normalTex.width * scale);
                    float yCoord = (yOrg + y / normalTex.height * scale);
                    float sample = Mathf.Clamp(Mathf.PerlinNoise(xCoord, yCoord), 0f, 1f);
                    normalPix[(int)y * normalTex.width + (int)x] = new Color(sample, 1f-sample, 0.5f);
                    colorPix[(int)y * normalTex.width + (int)x] = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a + sample);
                    x++;
                }
                y++;
            }

            // Copy the pixel data to the texture and load it into the GPU.
            normalTex.SetPixels(normalPix);
            normalTex.Apply();
            colorTex.SetPixels(colorPix);
            colorTex.Apply();
        } else {
            // For each pixel in the texture...
            float y = 0.0F;

            while (y < normalTex.height)
            {
                float x = 0.0F;
                while (x < normalTex.width)
                {
                    float xCoord = (xOrg + x / normalTex.width * scale);
                    float yCoord = (yOrg + y / normalTex.height * scale);
                    float sample = Mathf.Clamp(Mathf.PerlinNoise(xCoord, yCoord), 0f, 1f);
                    normalPix[(int)y * normalTex.width + (int)x] = new Color(sample, 1f-sample, 0.5f);
                    colorPix[(int)y * normalTex.width + (int)x] = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * sample);
                    x++;
                }
                y++;
            }

            // Copy the pixel data to the texture and load it into the GPU.
            normalTex.SetPixels(normalPix);
            normalTex.Apply();
            colorTex.SetPixels(colorPix);
            colorTex.Apply();
        }
    }

    void AnimateNoise() {
        if (useBaseColor) {
            // For each pixel in the texture...
            float y = 0.0F;

            while (y < normalTex.height)
            {
                float x = 0.0F;
                while (x < normalTex.width)
                {
                    float samplePoint = CheckOverflow(normalPix[(int)y * normalTex.width + (int)x].r + (0.00390625f / effectSpeed));
                    normalPix[(int)y * normalTex.width + (int)x].r = samplePoint;
                    normalPix[(int)y * normalTex.width + (int)x].g = 1-samplePoint;
                    colorPix[(int)y * normalTex.width + (int)x].a = baseColor.a + samplePoint;
                    x++;
                }
                y++;
            }

            // Copy the pixel data to the texture and load it into the GPU.
            normalTex.SetPixels(normalPix);
            normalTex.Apply();
            colorTex.SetPixels(colorPix);
            colorTex.Apply();
        } else {
            // For each pixel in the texture...
            float y = 0.0F;

            while (y < normalTex.height)
            {
                float x = 0.0F;
                while (x < normalTex.width)
                {
                    float samplePoint = CheckOverflow(normalPix[(int)y * normalTex.width + (int)x].r + (0.00390625f / effectSpeed));
                    normalPix[(int)y * normalTex.width + (int)x].r = samplePoint;
                    normalPix[(int)y * normalTex.width + (int)x].g = 1-samplePoint;
                    colorPix[(int)y * normalTex.width + (int)x].a = baseColor.a * samplePoint;
                    x++;
                }
                y++;
            }

            // Copy the pixel data to the texture and load it into the GPU.
            normalTex.SetPixels(normalPix);
            normalTex.Apply();
            colorTex.SetPixels(colorPix);
            colorTex.Apply();
        }
    }

    float CheckOverflow(float samplePoint) {
        return samplePoint > 1.0f ? 0.0f : samplePoint < 0.0f ? 1.0f : samplePoint;
    }

    void Update()
    {
        effectTimer += Time.deltaTime;
        if (effectTimer >= effectSpeed/30) {
            AnimateNoise();
            effectTimer = 0.0f;
        }
    }
}
