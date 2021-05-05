using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Sight : MonoBehaviour
{
	[Range(0,1)]
	public float intensity;
	public float intensityDecreaseFromMiniGames;
	public Color color;
	public float blindingSpeed;
	public Texture2D cutOff;
	private Material material;

	//Creates a private material used to the effect
	void Awake()
	{
		Camera cam = GetComponent<Camera>();
		cam.depthTextureMode = DepthTextureMode.DepthNormals;
		material = new Material(Shader.Find("Custom/Sight"));
		//Debug.Log("Material" + material.ToString());
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			if (intensity < 1)
			{
				intensity += Time.deltaTime * (blindingSpeed * 0.01f);
			}
			else if (intensity > 1)
			{
				intensity = 1;
			}
		}
		else
		{
			intensity = 0;
		}
	}

	//Postprocess the image
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetFloat("_Intensity", intensity);
		material.SetColor("_Color", color);
		material.SetTexture("_CutTex", cutOff);
		Graphics.Blit(source, destination, material);
	}

	public void IncreaseSight()
	{
		if (intensity > intensityDecreaseFromMiniGames)
		{
			intensity -= intensityDecreaseFromMiniGames;
		}
		else
		{
			intensity = 0;
		}
		
	}

    public void DecreaseSight()
    {
        if (intensity < 1 - intensityDecreaseFromMiniGames)
        {
            intensity += intensityDecreaseFromMiniGames;
        }
        else
        {
            intensity = 1;
        }

    }

}

