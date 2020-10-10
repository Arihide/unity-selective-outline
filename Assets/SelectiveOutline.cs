using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SelectiveOutline : MonoBehaviour
{
    public Material emissionMaterial;
    public Material outlineMaterial;

    private new Camera camera;
    private CommandBuffer commandBuffer;

    [SerializeField] private Renderer targetRenderer = null;

    void OnEnable()
    {
        camera = GetComponent<Camera>();

        commandBuffer = new CommandBuffer();
        commandBuffer.name = "Selective Outline";

        SetCommandBuffer();

        // ImageEffects前(OnRenderImageが呼ばれる前)に適用
        camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer);
    }

    void OnDisable()
    {
        camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer);
    }

    void SetCommandBuffer()
    {
        commandBuffer.Clear();

        if (targetRenderer != null)
        {
            // レンダリング結果を格納するテクスチャ作成
            var id = Shader.PropertyToID("_OutlineTex");
            commandBuffer.GetTemporaryRT(id, -1, -1, 24, FilterMode.Bilinear);
            commandBuffer.SetRenderTarget(id);

            // アウトラインを表示させたいメッシュの描画
            commandBuffer.ClearRenderTarget(false, true, Color.clear);
            commandBuffer.DrawRenderer(targetRenderer, emissionMaterial);

            // アウトラインを抽出して合成
            commandBuffer.Blit(id, BuiltinRenderTextureType.CameraTarget, outlineMaterial);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                targetRenderer = hit.transform.GetComponent<Renderer>();
                SetCommandBuffer();
            }
        }
    }
}
