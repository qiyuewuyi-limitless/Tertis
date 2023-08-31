using UnityEngine;

public class NormalsReplacementShader : MonoBehaviour
{
    [SerializeField]
    Shader normalsShader;

    private RenderTexture renderTexture;
    private new Camera camera;

    private void Start()
    {
        Camera thisCamera = GetComponent<Camera>();

        // Create a render texture matching the main camera's current dimensions.
        renderTexture = new RenderTexture(thisCamera.pixelWidth, thisCamera.pixelHeight, 24);
        // Surface the render texture as a global variable, available to all shaders.
        Shader.SetGlobalTexture("_CameraNormalsTexture", renderTexture); //设置全局属性_CameraNormalsTexture的值为renderTexture

        // Setup a copy of the camera to render the scene using the normals shader.
        GameObject copy = new GameObject("Normals camera");
        camera = copy.AddComponent<Camera>();
        camera.CopyFrom(thisCamera);
        camera.transform.SetParent(transform);
        camera.targetTexture = renderTexture; // 更换camera的输出： screen - rednerTexture
        camera.SetReplacementShader(normalsShader, "RenderType"); // 替换被渲染物体使用的着色器
        camera.depth = thisCamera.depth - 1;
    }
}
