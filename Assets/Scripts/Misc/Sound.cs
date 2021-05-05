using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

[ExecuteInEditMode]
public class Sound : MonoBehaviour
{
	public float intensity;
	public float intensityDecreaseFromMiniGames;
	public float deafeningSpeed;
	public AudioMixer masterMixer;

	private void Update()
	{
		if (Application.isPlaying)
		{
			if (intensity > -25)
			{
				intensity -= Time.deltaTime * (deafeningSpeed * 0.01f);
			}
			else if (intensity < -25)
			{
				intensity = -25;
			}
			masterMixer.SetFloat("Enemy", intensity);
		}
		else
		{
			intensity = 0;
		}
	}

	public void IncreaseSound()
	{
		if (intensity < 0 - intensityDecreaseFromMiniGames)
		{
			intensity += intensityDecreaseFromMiniGames;
		}
		else
		{
			intensity = 1;
		}
		
	}

    public void DecreaseSound()
    {
        if (intensity > -25 - intensityDecreaseFromMiniGames)
        {
            intensity -= intensityDecreaseFromMiniGames;
        }
        else
        {
            intensity = -25;
        }

    }
}

