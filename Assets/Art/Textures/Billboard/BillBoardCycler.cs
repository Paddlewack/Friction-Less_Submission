using System.Collections;
using UnityEngine;

public class BillboardCycler : MonoBehaviour
{
    [Header("Textures to cycle through")]
    public Texture2D[] textures;

    [Header("Timing")]
    public float intervalSeconds = 5f;

    [Header("Glitch on switch")]
    public bool glitchOnSwitch = true;
    public float glitchDuration = 0.12f;
    public float glitchIntensity = 0.8f;

    private Renderer _renderer;
    private Material _mat;
    private int _index = 0;
    private float _timer = 0f;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _mat = _renderer.material; // creates a per-instance copy
        if (textures.Length > 0)
            _mat.SetTexture("_MainTex", textures[0]);
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= intervalSeconds)
        {
            _timer = 0f;
            _index = (_index + 1) % textures.Length;
            _mat.SetTexture("_MainTex", textures[_index]);
            if (glitchOnSwitch)
                StartCoroutine(GlitchFlash());
        }
    }

    IEnumerator GlitchFlash()
    {
        _mat.SetFloat("_GlitchStrength", glitchIntensity);
        yield return new WaitForSeconds(glitchDuration);
        _mat.SetFloat("_GlitchStrength", 0f);
    }
}