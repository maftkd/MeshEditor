using UnityEngine;

public class DrawSelectionBox : MonoBehaviour
{
    public Shader shader;
    private Material _mat;
    // Start is called before the first frame update
    void Start()
    {
        _mat = new Material(shader);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(src, dest, _mat);
    }
}
