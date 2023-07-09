using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// I totally didn't steal this from the internet what are you talking about
public class CameraFigureEightHover : MonoBehaviour
{
    private float m_Speed = 1.4f;
    private float m_XScale = 0.1f;
    private float m_YScale = 0.025f;
    private Vector3 m_Pivot;
    private Vector3 m_PivotOffset;
    private float m_Phase;
    private bool m_Invert = false;
    private float m_2PI = Mathf.PI * 2;

    void Start()
    {
        m_Pivot = transform.position;
    }

    void Update()
    {
        m_PivotOffset = Vector3.right * 2 * m_XScale;

        m_Phase += m_Speed * Time.deltaTime;
        if (m_Phase > m_2PI)
        {
            m_Invert = !m_Invert;
            m_Phase -= m_2PI;
        }
        if (m_Phase < 0) m_Phase += m_2PI;

        transform.position = m_Pivot + (m_Invert ? m_PivotOffset : Vector3.zero);
        transform.position += new Vector3(Mathf.Cos(m_Phase) * (m_Invert ? -1 : 1) * m_XScale, Mathf.Sin(m_Phase) * m_YScale, 0);
    }
}
