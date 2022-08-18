
using UdonSharp;
using UnityEngine;

public class MatchVideoColor : UdonSharpBehaviour
{
#pragma warning disable 0649
    [SerializeField] Light[] lights;
    [SerializeField] Texture2D textureForOverwrite;
    [SerializeField] Camera lightCamera;
    [SerializeField] Renderer outputRender;
#pragma warning restore 0649
    private Color pixel;
    private Color pixelPrev;
    private float timer;
    [SerializeField] private float updateInterval = 1f;

    private void Start()
    {
        timer = updateInterval;
    }


    public Color fadeColor(Color fromColor, Color toColor, float amount)
    {
        if(amount > 1f)
        {
            amount = 1f;
        }else if(amount < 0f)
        {
            amount = 0f;
        }


        Color newColor = Color.white;

        newColor.r = ((toColor.r - fromColor.r) * amount) + fromColor.r;
        newColor.g = ((toColor.g - fromColor.g) * amount) + fromColor.g;
        newColor.b = ((toColor.b - fromColor.b) * amount) + fromColor.b;

        

        return newColor;
    }


    private void Update()
    {
        timer -= Time.deltaTime;
        if(timer > 0f)
        {
            Color tempColor = Color.black;
            tempColor = fadeColor(pixelPrev, pixel, ((updateInterval - timer) / updateInterval));
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].color = tempColor;
            }
        }
    }


    private Color GetAverageColor(Texture2D texture)
    {
        Color color = Color.black;
        int iterations = 10;

        for(int x = 0; x < iterations; x++)
        {
            for (int y = 0; y < iterations; y++)
            {
                color += texture.GetPixel((x * (texture.width / iterations)) + ((texture.width / iterations)/2), (y * (texture.height / iterations)) + ((texture.height / iterations) / 2));
            }
        }

        color /= (iterations* iterations);
        
        return color;
    }



    private void OnPostRender()
    {
        if(timer <= 0)
        {
            timer = updateInterval;
            textureForOverwrite.ReadPixels(new Rect(0,0, lightCamera.pixelWidth, lightCamera.pixelHeight), 0, 0);
            textureForOverwrite.Apply();
            outputRender.material.mainTexture = textureForOverwrite;
            pixelPrev = pixel;
            pixel = GetAverageColor(textureForOverwrite);
        }
    }

}
