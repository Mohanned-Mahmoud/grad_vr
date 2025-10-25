using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PaperTexture : MonoBehaviour
{
    [SerializeField]
    private Sprite paperTexture;  // Assign PNG or PSD (imported as Sprite) in the Inspector

    [SerializeField]
    private bool preserveAspect = true;  // Maintain texture's aspect ratio

    [SerializeField]
    private Color textureColor = Color.white;  // Color tint (white for no change)

    [SerializeField]
    private bool matchTextureAspectRatio = false;  // Optional: Adjust Panel size to texture's aspect ratio

    private Image panelImage;
    private RectTransform rectTransform;

    void Awake()
    {
        // Get the Image and RectTransform components
        panelImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        if (panelImage == null)
        {
            Debug.LogError("PaperTexture requires an Image component on " + gameObject.name);
            return;
        }

        ApplyTexture();
    }

    private void ApplyTexture()
    {
        if (paperTexture == null)
        {
            Debug.LogWarning($"No paper texture assigned to {gameObject.name}. Ensure a PNG or PSD (imported as Sprite) is assigned.");
            return;
        }

        // Apply the texture (sprite)
        panelImage.sprite = paperTexture;
        panelImage.color = textureColor;
        panelImage.preserveAspect = preserveAspect;

        // Optional: Match Panel's aspect ratio to texture
        if (matchTextureAspectRatio && paperTexture != null)
        {
            Vector2 textureSize = new Vector2(paperTexture.rect.width, paperTexture.rect.height);
            float aspectRatio = textureSize.x / textureSize.y;
            float currentHeight = rectTransform.rect.height;
            rectTransform.sizeDelta = new Vector2(currentHeight * aspectRatio, currentHeight);
        }

        // Ensure Panel stretches to fill Canvas
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    // Optional: Change texture at runtime
    public void SetTexture(Sprite newTexture)
    {
        paperTexture = newTexture;
        ApplyTexture();
    }
}