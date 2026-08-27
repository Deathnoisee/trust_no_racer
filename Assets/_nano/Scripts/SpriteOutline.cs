using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutline : MonoBehaviour
{
  public Color outlineColor = Color.white;
  [Range(0f, 0.5f)] public float outlineWidth = 0.008f;

  private SpriteRenderer sr;
  private Material outlineMaterial;
  private static Shader outlineShader;

  void Awake()
  {
    sr = GetComponent<SpriteRenderer>();

    if (outlineShader == null)
      outlineShader = Shader.Find("Custom/SpriteOutline");

    // create a unique material instance so per-object color/width don't share state
    outlineMaterial = new Material(outlineShader);
    outlineMaterial.mainTexture = sr.sprite.texture;
    sr.material = outlineMaterial;

    ApplySettings();
  }

  void ApplySettings()
  {
    outlineMaterial.SetColor("_OutlineColor", outlineColor);
    outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);
  }

  public void SetOutline(bool enabled, Color? color = null)
  {
    outlineWidth = enabled ? 0.008f : 0f;
    if (color.HasValue) outlineColor = color.Value;
    ApplySettings();
  }

  void OnValidate()
  {
    if (outlineMaterial != null) ApplySettings();
  }
}