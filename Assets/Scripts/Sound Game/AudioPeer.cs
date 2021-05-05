using System.Collections;
using UnityEngine;

[RequireComponent (typeof (AudioSource))]
public class AudioPeer : MonoBehaviour
{
	public AudioSource _audioSourceDelayed;
	public AudioSource _audioSource;
	public static float[] _Samples = new float[512];
	public static float[] _SamplesDelayed = new float[512];
	public static float[] _freq4Band = new float[4];
	public static float[] _freq4BandDelayed = new float[4];
	public float _delayTime;
	public static float[] _bandBuffer = new float[4];
	float[] _bufferDecrease = new float[4];

	// Use this for initialization
	IEnumerator Start ()
	{
        _audioSource.Play();

        WaitForSeconds i = new WaitForSeconds(_delayTime);
        yield return i;

        _audioSourceDelayed.Play();
    }

	void BandBuffer()
	{
		for (int g = 0; g < 4; ++g)
		{
			if (_freq4BandDelayed[g] > _bandBuffer[g])
			{
				_bandBuffer[g] = _freq4BandDelayed[g];
				_bufferDecrease[g] = 0.005f;
			}
			if (_freq4BandDelayed[g] < _bandBuffer[g])
			{
				_bandBuffer[g] -= _bufferDecrease[g];
				_bufferDecrease[g] *= 1.2f;
			}
		}

	}
	
	// Update is called once per frame
	void Update ()
	{
        GetSpectrumAudioSource();
		Make4FrequencyBands();
		Make4FrequencyBandsDelayed();
		BandBuffer();
	}

	void GetSpectrumAudioSource()
	{
		_audioSource.GetSpectrumData(_Samples, 0, FFTWindow.Blackman);
		_audioSourceDelayed.GetSpectrumData(_SamplesDelayed, 0, FFTWindow.Blackman);
	}

	void Make4FrequencyBands()
	{
		int count = 0;
		for (int i = 0; i < 4; i++)
		{
			float average = 0;

			int sampleCount = (int)Mathf.Pow(2, i) * 2;

			if (i == 3)
			{
				sampleCount += 2;
			}
			for (int j = 0; j < sampleCount; j++)
			{
				average += _Samples[count] * (count + 1);
				count++;
			}

			average /= count;

			_freq4Band[i] = average * 10;
		}
	}

	void Make4FrequencyBandsDelayed()
	{
		int count = 0;
		for (int i = 0; i < 4; i++)
		{
			float average = 0;

			int sampleCount = (int)Mathf.Pow(2, i) * 2;

			if (i == 3)
			{
				sampleCount += 2;
			}
			for (int j = 0; j < sampleCount; j++)
			{
				average += _SamplesDelayed[count] * (count + 1);
				count++;
			}

			average /= count;

			_freq4BandDelayed[i] = average * 10;
		}
	}
}
