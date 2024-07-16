using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Image))]
public class HarmoniousGradientControllerV3 : MonoBehaviour
{
    public Color topColor = Color.red;
    public Color bottomColor = Color.blue;
    public float cornerRadius = 10f;

    private Material material;
    private Image image;

    private void OnValidate()
    {
        Validate();
        Refresh();
    }

    private void OnEnable()
    {
        Validate();
        Refresh();
    }

    private void OnDisable()
    {
        if (image != null)
        {
            image.material = null;
        }
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }
    }

    private void Validate()
    {
        if (material == null)
        {
            material = new Material(Shader.Find("Custom/HarmoniousGradientRoundedCornersV3"));
        }

        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (image != null)
        {
            image.material = material;
        }
    }

    private void Refresh()
    {
        if (material != null)
        {
            material.SetColor("_TopColor", topColor);
            material.SetColor("_BottomColor", bottomColor);
            material.SetFloat("_CornerRadius", cornerRadius);

            var rect = ((RectTransform)transform).rect;
            material.SetVector("_WidthHeightRadius", new Vector4(rect.width, rect.height, cornerRadius, 0));

            if (image != null && image.sprite != null)
            {
                Rect outer = image.sprite.rect;
                Rect inner = image.sprite.textureRect;
                Vector4 uv = new Vector4(
                    inner.xMin / outer.width,
                    inner.yMin / outer.height,
                    inner.xMax / outer.width,
                    inner.yMax / outer.height
                );
                material.SetVector("_OuterUV", uv);
            }
            else
            {
                material.SetVector("_OuterUV", new Vector4(0, 0, 1, 1));
            }
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            Refresh();
        }
    }
}