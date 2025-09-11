using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.UI;

public class CameraSimulator : MonoBehaviour
{
    [Header("Dials")]
    public DialInteractable leftModeDial;   // ���� ���
    public DialInteractable rightMainDial;  // ���� �ϴ�

    [Header("Post Processing")]
    public Volume volume;

    [Header("LCD UI")]
    public TMP_Text apertureText;
    public TMP_Text shutterText;
    public TMP_Text isoText;
    public Image apertureHL, shutterHL, isoHL; // ���� ���̶���Ʈ(������ ����α�)

    DepthOfField dof;
    MotionBlur motionBlur;
    FilmGrain filmGrain;
    ColorAdjustments exposure;

    public enum Active { Aperture, Shutter, ISO }
    public Active active = Active.Aperture;

    int apIdx = 3, shIdx = 7, isoIdx = 0;

    public float[] apertures = { 1.4f, 1.8f, 2f, 2.8f, 4f, 5.6f, 8f, 11f, 16f, 22f };
    public float[] shutterSec = { 1f / 8000f, 1f / 4000f, 1f / 2000f, 1f / 1000f, 1f / 500f, 1f / 250f, 1f / 125f, 1f / 60f, 1f / 30f, 1f / 15f, 1f / 8f, 1f / 4f, 1f / 2f, 1f, 2f, 4f };
    public int[] isoValues = { 100, 200, 400, 800, 1600, 3200, 6400, 12800 };

    void Awake()
    {
        volume.profile.TryGet(out dof);
        volume.profile.TryGet(out motionBlur);
        volume.profile.TryGet(out filmGrain);
        volume.profile.TryGet(out exposure);
    }

    void OnEnable()
    {
        if (leftModeDial) leftModeDial.OnTick.AddListener(OnModeTick);
        if (rightMainDial) rightMainDial.OnTick.AddListener(OnMainTick);
        ApplyPostFX();
        UpdateLCD();
    }
    void OnDisable()
    {
        if (leftModeDial) leftModeDial.OnTick.RemoveListener(OnModeTick);
        if (rightMainDial) rightMainDial.OnTick.RemoveListener(OnMainTick);
    }

    // �»�� ���̾�: � �׸��� �������� ��ȯ ����
    void OnModeTick(int dir)
    {
        int i = (int)active;
        i = (i + dir) % 3; if (i < 0) i += 3;
        active = (Active)i;
        UpdateLCD();
    }

    // ���ϴ� ���̾�: ���� ���õ� �׸� ���� ��/��
    void OnMainTick(int dir)
    {
        switch (active)
        {
            case Active.Aperture: apIdx = Mathf.Clamp(apIdx + dir, 0, apertures.Length - 1); break;
            case Active.Shutter: shIdx = Mathf.Clamp(shIdx + dir, 0, shutterSec.Length - 1); break;
            case Active.ISO: isoIdx = Mathf.Clamp(isoIdx + dir, 0, isoValues.Length - 1); break;
        }
        ApplyPostFX();
        UpdateLCD();
    }

    void ApplyPostFX()
    {
        float f = apertures[apIdx];
        float s = shutterSec[shIdx];
        int iso = isoValues[isoIdx];

        // ISO -> ���� + �׷���
        if (exposure) exposure.postExposure.value = Mathf.Log10(Mathf.Max(iso, 1) / 100f);
        if (filmGrain) filmGrain.intensity.value = Mathf.Clamp01((iso - 100f) / 12800f);

        // ���� -> ��Ǻ�(�������� ũ��)
        if (motionBlur)
        {
            float blur = Mathf.InverseLerp(1f / 8000f, 1f / 4f, s); // ����0 �� ����1
            motionBlur.intensity.value = blur;
        }

        // ������ -> �ɵ�
        if (dof)
        {
            dof.mode.value = DepthOfFieldMode.Bokeh;
            dof.aperture.value = f;       // f �� �ݿ�
            // �ʿ��ϸ� dof.focusDistance.value �� ���� �Ÿ��� ����
        }
    }

    void UpdateLCD()
    {
        if (apertureText) apertureText.text = "F" + (apertures[apIdx] % 1f == 0 ? apertures[apIdx].ToString("0") : apertures[apIdx].ToString("0.0"));
        if (shutterText) shutterText.text = FormatShutter(shutterSec[shIdx]);
        if (isoText) isoText.text = isoValues[isoIdx].ToString();

        if (apertureHL) apertureHL.enabled = (active == Active.Aperture);
        if (shutterHL) shutterHL.enabled = (active == Active.Shutter);
        if (isoHL) isoHL.enabled = (active == Active.ISO);
    }

    string FormatShutter(float s)
    {
        if (s >= 1f) return s.ToString("0") + "s";
        int denom = Mathf.RoundToInt(1f / Mathf.Max(0.000001f, s));
        return "1/" + denom;
    }
}
