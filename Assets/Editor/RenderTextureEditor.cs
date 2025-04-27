using System.IO;
using UnityEngine;
using UnityEditor;
using static OVRPlugin;
public partial class RenderTextureEditor : EditorWindow
{
    const int BaseResolution = 256;
    [MenuItem("LightnGames/CaptureOrhoCubemap")]
    public static void CaptureWindowGUI()
    {
        var window = GetWindow<RenderTextureEditor>("UIElements");
        window.titleContent = new GUIContent("CaptureOrhoCubemap"); // エディタ拡張ウィンドウのタイトル
        window.Show();
    }

    private void SaveTexture(Camera captureCamera, Texture2D cubemap, float x, float y,float aspectRate, int faceIndex)
    {
        float minLength = Mathf.Min(x, y);
        var rt = new RenderTexture((int)(x / minLength * BaseResolution), (int)(y / minLength * BaseResolution), 16, RenderTextureFormat.ARGB32);
        captureCamera.targetTexture = rt;
        captureCamera.orthographicSize = aspectRate * 0.5f;
        captureCamera.Render();

        // Texture2D変換.
        var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
        var tmp = RenderTexture.active;
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        // リサイズ後のサイズを持つRenderTextureを作成して書き込む
        var resizedRT = RenderTexture.GetTemporary(BaseResolution, BaseResolution);
        Graphics.Blit(texture, resizedRT);

        // リサイズ後のサイズを持つTexture2Dを作成してRenderTextureから書き込む
        var preRT = RenderTexture.active;
        RenderTexture.active = resizedRT;
        cubemap.ReadPixels(new Rect(0, 0, resizedRT.width, resizedRT.height), BaseResolution * faceIndex, 0);
        cubemap.Apply();

        captureCamera.targetTexture = null;
        RenderTexture.ReleaseTemporary(resizedRT);
        DestroyImmediate(texture);
        DestroyImmediate(rt);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("キャプチャ"))
        {
            // 既存アセットの取得.
            string assetPath = string.Format("/Scene/ResidentScene/ReflectionProbe-0.exr");
            string assetFullPath = "Assets" + assetPath;
            string assetAbsolutePath = Application.dataPath + assetPath;
            Texture2D cubemap = new Texture2D(BaseResolution * 6, BaseResolution, TextureFormat.RGBA32, false);
            if (!File.Exists(assetAbsolutePath))
            {
                
                AssetDatabase.CreateAsset(cubemap, assetFullPath);
                var importer = AssetImporter.GetAtPath(assetFullPath) as TextureImporter;
               // importer.generateCubemap = TextureImporterGenerateCubemap.FullCubemap;
                importer.textureShape = TextureImporterShape.TextureCube;
                //importer.isReadable = true;
                //importer.npotScale = TextureImporterNPOTScale.None;

                AssetDatabase.SaveAssets();
            }

            Camera captureCamera = GameObject.Find("OrthogonalReflectionProbe").GetComponent<Camera>();
            ReflectionProbe probe = captureCamera.transform.parent.GetComponent<ReflectionProbe>();
            captureCamera.transform.rotation = Quaternion.LookRotation(Vector3.right);
            SaveTexture(captureCamera, cubemap, probe.size.z, probe.size.y, probe.size.y, 0);
            captureCamera.transform.rotation = Quaternion.LookRotation(-Vector3.right);
            SaveTexture(captureCamera, cubemap, probe.size.z, probe.size.y, probe.size.y, 1);
            captureCamera.transform.rotation = Quaternion.LookRotation(Vector3.forward);
            SaveTexture(captureCamera, cubemap, probe.size.x, probe.size.y, probe.size.y, 4);
            captureCamera.transform.rotation = Quaternion.LookRotation(-Vector3.forward);
            SaveTexture(captureCamera, cubemap, probe.size.x, probe.size.y, probe.size.y, 5);

            captureCamera.transform.rotation = Quaternion.LookRotation(Vector3.up);
            SaveTexture(captureCamera, cubemap, probe.size.x, probe.size.z, probe.size.z, 2);
            captureCamera.transform.rotation = Quaternion.LookRotation(-Vector3.up);
            SaveTexture(captureCamera, cubemap, probe.size.x, probe.size.z, probe.size.z, 3);

            // 保存.
            File.WriteAllBytes(assetAbsolutePath, cubemap.EncodeToEXR());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}