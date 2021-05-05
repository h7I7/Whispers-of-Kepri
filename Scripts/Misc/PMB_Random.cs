using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 Minimal random number generator of Park and Miller with Bays - Durham shuffle and added safeguards.
 Returns a uniform random deviate between 0.0 and 1.0 (exclusive of the endpoint values). 
 RNMX should approximate the largest floating value that is less than 1.
 */

public class PMB_Random
{


    private static long IA = 16807;
    private static long IM = 2147483647;
    private static double AM = (1.0 / IM);
    private static long IQ = 127773;
    private static long IR = 2836;
    private static long NTAB = 32;
    private static long NDIV = (1 + (IM - 1) / NTAB);
    private static double EPS = 1.2e-7;
    private static double RNMX = (1.0 - EPS);

    private static long iy = 0;
    private static long[] iv = new long[NTAB];

    private static long _seed = 0;
    

    public static void Seed(long a_seed)
    {
        iy = 0;
        _seed = a_seed;
    }
    

    public static float Random()
    {
        float rand = 0f;
        int j = 0;
        long k = 0L;
       

        if ( iy == 0)
        {
            //If we've not set the seed value then set the seed value now
            if (_seed == 0)
            {
                _seed = System.Environment.TickCount;
            }

            for (j = (int)NTAB + 7; j >= 0; j--)
            {
                /* 
                 * Load the shuffle table (after 8 warm - ups).
                 */
                k = (_seed) / IQ;
                _seed = IA * (_seed - k * IQ) - IR * k;
                if (_seed < 0)
                {
                    _seed += IM;
                }
                if (j < NTAB)
                {
                    iv[j] = _seed;
                }
            }

            iy = iv[0];
        }

        /* 
         * Start here when not initializing.
         */

        k = (_seed) / IQ;

        /*	
         Compute	idum = (IA * idum) % IM	without overflows by Schrage's method.
         */
        _seed = IA * (_seed - k * IQ) - IR * k;
        if (_seed < 0)
        {
            _seed += IM;
        }

        /*
         * Will be in the range 0..NTAB -1.
         */
        j = (int)(iy / NDIV);

        /*	
         * Output previously stored value and refill the shuffle table
         */
        iy = iv[j];
        iv[j] = _seed;

        /* 
         * Because users don't expect endpoint values
         */

        if ((rand = (float)(AM * iy)) > RNMX)
        {
            return (float)RNMX;
        }
        else
        {
            return rand;
        }
    }

    public static float Range( float min, float max )
    {
        float rand = Random();
        rand = min + (rand * (max - min));        
        return rand;
    }

    //With a normal distribution values within the range of -1 to 1 will occur with a normalised distribution
    //implementation details from https://en.wikipedia.org/wiki/Marsaglia_polar_method
    static float r0;
    static bool generate = true;

    public static float MarsagliaNormalDistribution(float mean = 0f, float standardDev = 1f )
    {
        float u, v, s;
        generate = !generate;
        if (generate)
            return r0 * standardDev + mean;
        do
        {
            u = 2.0f * Random() - 1.0f;
            v = 2.0f * Random() - 1.0f;
            s = u * u + v * v;
        }
        while (s >= 1.0f || s == 0f);

        float fac = Mathf.Sqrt(-2.0f * Mathf.Log(s) / s);
        r0 = v * fac;

        
        return u * fac * standardDev + mean;
    }

    //Box-Muller Distribution
    //Source adapted from https://en.wikipedia.org/wiki/Box%E2%80%93Muller_transform
    static float z0;
    static bool bmGen = true;
    public static float BoxMullerNormalDistribution(float mean = 0f, float stdDev = 1f)
    {
        const float two_pi = Mathf.PI * 2f;

        bmGen = !bmGen;
        if(bmGen)
        {
            return z0 * stdDev + mean;
        }

        float u, v;
        u = 1f - Random();
        v = 1f - Random();
        
        float s = Mathf.Sqrt(-2f * Mathf.Log(u));
        z0 = s * Mathf.Sin(two_pi * v);
        float z1 = s * Mathf.Cos(two_pi * v);

        return z1 * stdDev + mean;

        
    }

    public static void RandomPointInCircle( float a_radius, out float x, out float y )
    {
        float t = 2f * Mathf.PI * Random();
        float u = Random() + Random();
        float r = (u > 1) ? 2 - u : u;
        x = a_radius * r * Mathf.Cos(t);
        y = a_radius * r * Mathf.Sin(t);
    }

    public static void RandomPointInQuad(Vector2 a_min, Vector2 a_max, out Vector2 a_PointInQuad)
    {
        float u = Random() + Random();
        float v = Random() + Random();
        float r = (u > 1) ? 2 - u : u;
        float s = (v > 1) ? 2 - v : v;
        a_PointInQuad = new Vector2(r * (a_max.x - a_min.x) + a_min.x, s * (a_max.y - a_min.y) + a_min.y);

    }

    public static void RandomPointInEllipse(float a_width, float a_height, out Vector2 a_pointInEllipse)
    {
        float t = 2f * Mathf.PI * Random();
        float u = Random() + Random();
        float r = (u > 1) ? 2 - u : u;
        a_pointInEllipse = new Vector2(a_width * r * Mathf.Cos(t), a_height * r * Mathf.Sin(t));
    }


}