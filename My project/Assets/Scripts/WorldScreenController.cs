using UnityEngine;
using System.IO;

public class WorldPhotoScreen : MonoBehaviour
{
    public MeshRenderer screenRenderer;   // Quad MeshRenderer
    public Camera captureCamera;          // DSLR 모델 안의 Camera
    public string saveFolder = "Photos";

    public void TakePhoto()
    {
        // 1. RenderTexture 생성
        RenderTexture rt = new RenderTexture(1024, 1024, 24);
        captureCamera.targetTexture = rt;

        // 2. 카메라 화면 → Texture2D 캡처
        Texture2D photo = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        captureCamera.Render();
        RenderTexture.active = rt;
        photo.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        photo.Apply();

        // 3. 리소스 정리
        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // 4. Quad에 텍스처 표시
        if (screenRenderer != null)
        {
            screenRenderer.material.mainTexture = photo;
        }

        // 5. (선택) 파일로 저장
        string folderPath = Path.Combine(Application.persistentDataPath, saveFolder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fileName = "Photo_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        File.WriteAllBytes(Path.Combine(folderPath, fileName), photo.EncodeToPNG());
        Debug.Log("DSLR 촬영 완료: " + fileName);
    }
}
