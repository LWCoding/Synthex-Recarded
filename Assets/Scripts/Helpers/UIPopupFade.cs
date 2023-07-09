using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIPopupFade : MonoBehaviour
{

    [HideInInspector] public float timeAlive;
    [HideInInspector] public float lastUpdate;
    private float _maxTime;
    private float _speedAmplifier;
    private float _xDiff;
    private bool _xMove;
    private TextMeshProUGUI _tmp;
    private Color _initialColor;
    private Color _targetColor;

    public void Initialize(float sAmplifier, float mTime, bool shouldXMove)
    {
        _xDiff = Random.Range(-0.8f, 0.8f);
        _xMove = shouldXMove;
        _speedAmplifier = sAmplifier;
        _maxTime = mTime;
        timeAlive = 0;
        lastUpdate = 0;
        _tmp = GetComponent<TextMeshProUGUI>();
        _initialColor = _tmp.color;
        _targetColor = new Color(_initialColor.r, _initialColor.g, _initialColor.b, 0);
    }

    private void Update()
    {
        timeAlive += Time.deltaTime;
        lastUpdate += Time.deltaTime;
        _tmp.color = Color.Lerp(_initialColor, _targetColor, timeAlive / _maxTime);
        if (lastUpdate > 0.01f)
        {
            lastUpdate = 0;
            if (timeAlive < _maxTime * 0.3f)
            {
                transform.Translate(((_xMove) ? _xDiff : 0), 1.2f * _speedAmplifier * (timeAlive * 2 / _maxTime), 0);
            }
            else
            {
                transform.Translate(((_xMove) ? _xDiff : 0) * 0.5f, -0.8f * _speedAmplifier, 0);
            }
        }
    }

}
