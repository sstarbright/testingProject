using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEffect : MonoBehaviour
{
    // The width and height (pixels) of the noise map (HEAVILY IMPACTS PERFORMANCE IF HIGHER THAN LIKE 300)
    public int textureWidth = 200;
    public int textureHeight = 200;

    // The origin position of the noise map, can be used to randomize how the water will look at runtime?
    public float xOrg;
    public float yOrg;

    // Alters the speed at which the water animates. Lower values are actually faster.
    //(Can have an effect on performance if moving faster due to it having to calculate more pixels every second)
    public float effectSpeed = 0.5f;

    // The size of the noise generated (used to zoom in and out of a noise map, shouldn't have much impact on performance if any)
    public float scale = 5.0F;
    
    // The base color you might want to add to your water
    public Color baseColor;

    // Check this off in the unity editor if you want the base color to be used
    public bool useBaseColor = true;

    // The textures that store both the normal map and the alpha texture.
    private Texture2D normalTex;
    private Texture2D alphaTex;

    // A list of colors that are translated into the normal map and alpha textures once changes have been made to them.
    private Color[] normalPix;
    private Color[] alphaPix;

    // The renderer of the target object that will have the ripple effect.
    private Renderer rend;

    // The timer that manages the speed that the effect runs at.
    private float effectTimer;

    void Start()
    {
        Application.targetFrameRate = 60;
        rend = GetComponent<Renderer>();

        //sets the base of the effect timer
        effectTimer = 0.0f;

        // Sets up the normal and alpha color textures and color arrays for future use
        normalTex = new Texture2D(textureWidth, textureHeight);
        normalPix = new Color[normalTex.width * normalTex.height];
        alphaTex = new Texture2D(textureWidth, textureHeight);
        alphaPix = new Color[normalTex.width * normalTex.height];

        // Applies the normal and alpha color textures to the object's material
        rend.material.EnableKeyword("_NORMALMAP");
        rend.material.SetTexture("_BumpMap", normalTex);
        rend.material.SetTexture("_MainTex", alphaTex);

        CalcNoise();
    }

    // Calculates the noise map to be used during animation
    void CalcNoise()
    {
        // Decides how to calculate color based on your choice
        if (useBaseColor) {
            // iterates over each pixel on the texture and updates it
            float y = 0.0F;
            while (y < normalTex.height)
            {
                float x = 0.0F;
                while (x < normalTex.width)
                {
                    // Uses the noise origin specified by user to create a perlin noise map
                    float xCoord = (xOrg + x / normalTex.width * scale);
                    float yCoord = (yOrg + y / normalTex.height * scale);
                    // Clamps the value (forces the value to be between 0f and 1f)
                    float sample = Mathf.Clamp(Mathf.PerlinNoise(xCoord, yCoord), 0f, 1f);
                    // uses the sample value from 0 to 1 to create a normal map and alpha pixel.
                    normalPix[(int)y * normalTex.width + (int)x] = new Color(sample, 1f-sample, 1f);
                    alphaPix[(int)y * normalTex.width + (int)x] = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a + sample);
                    x++;
                }
                y++;
            }

            // Adds the pixels to the textures
            normalTex.SetPixels(normalPix);
            // Updates all changes to the textures
            normalTex.Apply();
            alphaTex.SetPixels(alphaPix);
            alphaTex.Apply();
        } else {
            float y = 0.0F;

            while (y < normalTex.height)
            {
                float x = 0.0F;
                while (x < normalTex.width)
                {
                    float xCoord = (xOrg + x / normalTex.width * scale);
                    float yCoord = (yOrg + y / normalTex.height * scale);
                    float sample = Mathf.Clamp(Mathf.PerlinNoise(xCoord, yCoord), 0f, 1f);
                    normalPix[(int)y * normalTex.width + (int)x] = new Color(sample, 1f-sample, 1f);
                    alphaPix[(int)y * normalTex.width + (int)x] = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * sample);
                    x++;
                }
                y++;
            }

            normalTex.SetPixels(normalPix);
            normalTex.Apply();
            alphaTex.SetPixels(alphaPix);
            alphaTex.Apply();
        }
    }

    void AnimateNoise() {
        if (useBaseColor) {
            float y = 0.0F;

            while (y < normalTex.height)
            {
                float x = 0.0F;
                while (x < normalTex.width)
                {
                    // Animates water on a loop, shifting each pixel's value every time we animate (then loops back around)
                    float samplePoint = CheckOverflow(normalPix[(int)y * normalTex.width + (int)x].r + (0.00390625f / effectSpeed));
                    normalPix[(int)y * normalTex.width + (int)x].r = samplePoint;
                    normalPix[(int)y * normalTex.width + (int)x].g = 1-samplePoint;
                    alphaPix[(int)y * normalTex.width + (int)x].a = baseColor.a + samplePoint;
                    x++;
                }
                y++;
            }

            normalTex.SetPixels(normalPix);
            normalTex.Apply();
            alphaTex.SetPixels(alphaPix);
            alphaTex.Apply();
        } else {
            float y = 0.0F;

            while (y < normalTex.height)
            {
                float x = 0.0F;
                while (x < normalTex.width)
                {
                    float samplePoint = CheckOverflow(normalPix[(int)y * normalTex.width + (int)x].r + (0.00390625f / effectSpeed));
                    normalPix[(int)y * normalTex.width + (int)x].r = samplePoint;
                    normalPix[(int)y * normalTex.width + (int)x].g = 1-samplePoint;
                    alphaPix[(int)y * normalTex.width + (int)x].a = baseColor.a * samplePoint;
                    x++;
                }
                y++;
            }

            normalTex.SetPixels(normalPix);
            normalTex.Apply();
            alphaTex.SetPixels(alphaPix);
            alphaTex.Apply();
        }
    }

    // function for keeping values between the max and min
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
