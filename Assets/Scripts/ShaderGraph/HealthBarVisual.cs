using UnityEngine;

public class HealthBarVisual : MonoBehaviour
{
    private Renderer _renderer;
    private Material _material;
    private Bounds _bounds;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _material = _renderer.material;
        _bounds = _renderer.bounds;
    }

    void OnEnable()
    {
        GameManager.Instance.OnHealthChange += HealthBarVisualChange;
    }

    private void Start()
    {
        HealthBarVisualChange(100f);
    }

    void HealthBarVisualChange(float healthVal)
    {

        _material.SetFloat("_HealthValue", _bounds.size.x / 2 * ((2 * healthVal - 100f) / 100f));
    }

    void OnDisable()
    {
        GameManager.Instance.OnHealthChange -= HealthBarVisualChange;
    }
}
