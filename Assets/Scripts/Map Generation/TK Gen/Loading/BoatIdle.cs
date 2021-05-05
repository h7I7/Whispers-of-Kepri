using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BoatIdle : MonoBehaviour {

    [SerializeField]
    private float m_waveAmount;
    [SerializeField]
    private float m_waveSpeed;
    [SerializeField]
    private float m_boatSpeed;

    private float m_imgWidth;
    private float m_canvasWidth;
    private Vector3 m_initialPosition;

	// Use this for initialization
	void Start () {
        m_imgWidth = GetComponent<Image>().rectTransform.rect.width;
        m_canvasWidth = transform.parent.GetComponent<CanvasScaler>().referenceResolution.x;
        if (m_initialPosition == null)
            m_initialPosition = transform.localPosition;
        else
            transform.localPosition = m_initialPosition;
    }
	
	void Update () {

        Vector3 newPos = transform.localPosition;

        if (newPos.x < -((m_canvasWidth + m_imgWidth) * 0.5f))
            newPos.x = ((m_canvasWidth + m_imgWidth) * 0.5f);

        newPos.y += m_waveAmount * Mathf.Sin(Time.time * m_waveSpeed * Time.deltaTime * 100f);
        newPos.x -= (m_boatSpeed * Time.deltaTime * 100f);

        transform.localPosition = newPos;

	}
}
