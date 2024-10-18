using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SkyboxCycle : MonoBehaviour
{
    [Header("Skybox Materials")]
    [SerializeField] private Material daySkybox;        // 昼用のスカイボックス
    [SerializeField] private Material eveningSkybox;    // 夕方用のスカイボックス
    [SerializeField] private Material nightSkybox;      // 夜用のスカイボックス
    [SerializeField, Min(0.1f)] private float fadeDuration = 2f; // フェードの持続時間（秒）

    [Header("Post-Processing Volumes (URP)")]
    [SerializeField] private Volume dayVolume;
    [SerializeField] private Volume eveningVolume;
    [SerializeField] private Volume nightVolume;

    [Header("Lighting Settings")]
    [SerializeField] private Light mainLight;            // メインの方向性ライト

    // 指定されたライトの色と強度
    [SerializeField] private Color dayLightColor = new Color(1f, 0.9608f, 0.8902f);         // 昼のライト色 #FFF5E3
    [SerializeField] private float dayLightIntensity = 1.11f;                             // 昼のライト強度

    [SerializeField] private Color eveningLightColor = new Color(1f, 0.9608f, 0.8902f);     // 夕方のライト色 #FFF5E3
    [SerializeField] private float eveningLightIntensity = 0.64f;                       // 夕方のライト強度

    [SerializeField] private Color nightLightColor = new Color(0.5686f, 0.5765f, 0.6784f);  // 夜のライト色 #9193AD
    [SerializeField] private float nightLightIntensity = 1.67f;                         // 夜のライト強度

    [Header("ColorChanger Settings")]
    [SerializeField] private Material colorChangerMaterial;        // ColorChangerマテリアル
    [SerializeField] private float dayLevel = 0f;                  // 昼のLevel値（0または1）
    [SerializeField] private float eveningLevel = 0.3f;            // 夕方のLevel値
    [SerializeField] private float nightLevel = 0.75f;             // 夜のLevel値

    private Material[] skyboxes;
    private Volume[] volumes;
    private int currentSkyboxIndex = 0;
    private Coroutine fadeCoroutine;

    // ライトの色と強度を格納する配列
    private Color[] lightColors;
    private float[] lightIntensities;

    // Level値を格納する配列
    private float[] levels;

    public enum SkyboxMode
    {
        DayFixed,
        EveningFixed,
        NightFixed
    }

    private SkyboxMode currentMode;

    void Start()
    {
        // スカイボックスとポストプロセスボリュームを配列に格納
        skyboxes = new Material[] { daySkybox, eveningSkybox, nightSkybox };
        volumes = new Volume[] { dayVolume, eveningVolume, nightVolume };

        // ライトの色と強度を配列に格納
        lightColors = new Color[] { dayLightColor, eveningLightColor, nightLightColor };
        lightIntensities = new float[] { dayLightIntensity, eveningLightIntensity, nightLightIntensity };

        // Level値を配列に格納
        levels = new float[] { dayLevel, eveningLevel, nightLevel };

        // バリデーション
        if (skyboxes.Length != volumes.Length ||
            skyboxes.Length != lightColors.Length ||
            skyboxes.Length != lightIntensities.Length ||
            skyboxes.Length != levels.Length)
        {
            Debug.LogError("Skyboxes, volumes, light colors, light intensities, and levels arrays must have the same length.");
            enabled = false;
            return;
        }

        if (mainLight == null)
        {
            Debug.LogError("Main Light is not assigned.");
            enabled = false;
            return;
        }

        if (colorChangerMaterial == null)
        {
            Debug.LogError("ColorChanger Material is not assigned.");
            enabled = false;
            return;
        }

        // 初期スカイボックスとポストプロセスの設定
        RenderSettings.skybox = skyboxes[currentSkyboxIndex];
        RenderSettings.skybox.SetFloat("_Blend", 0f); // スカイボックスのブレンドを0に設定
        SetVolumeWeights(currentSkyboxIndex, 1f);   // 最初のボリュームをフルに適用

        // 初期ライトの設定
        mainLight.color = lightColors[currentSkyboxIndex];
        mainLight.intensity = lightIntensities[currentSkyboxIndex];

        // 初期Levelの設定
        colorChangerMaterial.SetFloat("_Level", levels[currentSkyboxIndex]);

        currentMode = SkyboxMode.DayFixed; // デフォルトで昼固定モード
    }

    // Updateメソッドは不要です

    #region Mode Setting Methods

    // 昼固定モードに設定する
    public void SetDayFixedMode()
    {
        currentMode = SkyboxMode.DayFixed;
        StopCurrentFade();
        SetSkyboxAndVolume(0); // 0は昼のインデックス
    }

    // 夕方固定モードに設定する
    public void SetEveningFixedMode()
    {
        currentMode = SkyboxMode.EveningFixed;
        StopCurrentFade();
        SetSkyboxAndVolume(1); // 1は夕方のインデックス
    }

    // 夜固定モードに設定する
    public void SetNightFixedMode()
    {
        currentMode = SkyboxMode.NightFixed;
        StopCurrentFade();
        SetSkyboxAndVolume(2); // 2は夜のインデックス
    }

    #endregion

    #region Change Mode Methods

    // 昼に変更する
    public void ChangeDay()
    {
        StartFadeTransition(0); // 0は昼のインデックス
    }

    // 夕方に変更する
    public void ChangeEvening()
    {
        StartFadeTransition(1); // 1は夕方のインデックス
    }

    // 夜に変更する
    public void ChangeNight()
    {
        StartFadeTransition(2); // 2は夜のインデックス
    }

    #endregion

    /// <summary>
    /// フェードトランジションを開始する。
    /// </summary>
    /// <param name="targetIndex">目標スカイボックスのインデックス</param>
    private void StartFadeTransition(int targetIndex)
    {
        if (targetIndex == currentSkyboxIndex)
        {
            Debug.LogWarning("Next skybox is the same as the current one. Fade aborted.");
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeRoutine(targetIndex));
    }

    /// <summary>
    /// スカイボックス、ポストプロセスボリューム、ライト、Levelのフェードルーチン。
    /// </summary>
    /// <param name="targetIndex">目標スカイボックスのインデックス</param>
    /// <returns></returns>
    private IEnumerator FadeRoutine(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= skyboxes.Length)
        {
            Debug.LogError("Invalid skybox index for fade.");
            yield break;
        }

        // 現在のスカイボックスと目標スカイボックスの取得
        Material currentSkybox = RenderSettings.skybox;
        Material targetSkybox = skyboxes[targetIndex];
        Volume currentVolume = volumes[currentSkyboxIndex];
        Volume targetVolume = volumes[targetIndex];

        // フェードの開始準備
        // 現在のスカイボックスの _Blend を 0 から 1 に変更
        float elapsed = 0f;

        // 特殊な遷移かどうかを判定
        bool isNightToDay = (currentMode == SkyboxMode.NightFixed) && (targetModeFromIndex(targetIndex) == SkyboxMode.DayFixed);

        while (elapsed < fadeDuration)
        {
            float blend = Mathf.Clamp01(elapsed / fadeDuration);

            // 現在のスカイボックスの _Blend を 0 から 1 に変更
            currentSkybox.SetFloat("_Blend", blend);

            // ボリュームの重みを調整
            SetVolumeWeight(currentVolume, 1f - blend);
            SetVolumeWeight(targetVolume, blend);

            // ライトの色と強度を補間
            mainLight.color = Color.Lerp(lightColors[currentSkyboxIndex], lightColors[targetIndex], blend);
            mainLight.intensity = Mathf.Lerp(lightIntensities[currentSkyboxIndex], lightIntensities[targetIndex], blend);

            // Levelの処理
            if (isNightToDay)
            {
                // 夜から昼への遷移時は Level を 0.75 から 1 にのみ変更
                float specialLevel = Mathf.Lerp(0.75f, 1f, blend);
                colorChangerMaterial.SetFloat("_Level", specialLevel);
            }
            else
            {
                // その他の遷移では通常通り Lerp
                float currentLevel = Mathf.Lerp(levels[currentSkyboxIndex], levels[targetIndex], blend);
                colorChangerMaterial.SetFloat("_Level", currentLevel);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // フェード完了後の設定
        // スカイボックスを次に切り替え、次のスカイボックスの _Blend を 0 にリセット
        RenderSettings.skybox = targetSkybox;
        RenderSettings.skybox.SetFloat("_Blend", 0f);

        // ボリュームの重みを調整
        SetVolumeWeight(currentVolume, 0f);
        SetVolumeWeight(targetVolume, 1f);

        // ライトの最終設定
        mainLight.color = lightColors[targetIndex];
        mainLight.intensity = lightIntensities[targetIndex];

        // Levelの最終設定
        if (isNightToDay)
        {
            // 夜から昼への遷移後に Level を 1 に設定
            colorChangerMaterial.SetFloat("_Level", 1f);
            // その後 Level を 0 にリセット
            colorChangerMaterial.SetFloat("_Level", 0f);
        }
        else
        {
            colorChangerMaterial.SetFloat("_Level", levels[targetIndex]);
        }

        // 現在のスカイボックスインデックスを更新
        currentSkyboxIndex = targetIndex;

        // 現在のモードを更新
        currentMode = targetModeFromIndex(targetIndex);

        // フェードコルーチンをクリア
        fadeCoroutine = null;
    }

    /// <summary>
    /// スカイボックスのインデックスからSkyboxModeを取得する。
    /// </summary>
    /// <param name="index">スカイボックスのインデックス</param>
    /// <returns>対応するSkyboxMode</returns>
    private SkyboxMode targetModeFromIndex(int index)
    {
        switch (index)
        {
            case 0:
                return SkyboxMode.DayFixed;
            case 1:
                return SkyboxMode.EveningFixed;
            case 2:
                return SkyboxMode.NightFixed;
            default:
                return currentMode;
        }
    }

    /// <summary>
    /// 指定されたインデックスのスカイボックスと関連設定を即時に設定する。
    /// 固定モード時に使用。
    /// </summary>
    /// <param name="index">スカイボックスのインデックス</param>
    private void SetSkyboxAndVolume(int index)
    {
        if (index < 0 || index >= skyboxes.Length ||
            index >= volumes.Length ||
            index >= lightColors.Length ||
            index >= lightIntensities.Length ||
            index >= levels.Length)
        {
            Debug.LogError($"Invalid index {index} for skyboxes, volumes, lighting settings, or levels.");
            return;
        }

        // スカイボックスを設定
        RenderSettings.skybox = skyboxes[index];
        RenderSettings.skybox.SetFloat("_Blend", 0f);

        // ボリュームの重みを設定
        SetVolumeWeights(index, 1f);

        // ライトの色と強度を設定
        mainLight.color = lightColors[index];
        mainLight.intensity = lightIntensities[index];

        // Levelを設定
        colorChangerMaterial.SetFloat("_Level", levels[index]);

        // スカイボックスインデックスを更新
        currentSkyboxIndex = index;
    }

    /// <summary>
    /// 現在のフェード処理を停止する。
    /// </summary>
    private void StopCurrentFade()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }

    /// <summary>
    /// 指定されたインデックスのボリュームに重みを設定し、他をリセットする。
    /// </summary>
    /// <param name="activeIndex">アクティブなボリュームのインデックス</param>
    /// <param name="activeWeight">アクティブなボリュームの重み</param>
    private void SetVolumeWeights(int activeIndex, float activeWeight)
    {
        for (int i = 0; i < volumes.Length; i++)
        {
            if (i == activeIndex)
            {
                SetVolumeWeight(volumes[i], activeWeight);
            }
            else
            {
                SetVolumeWeight(volumes[i], 0f);
            }
        }
    }

    /// <summary>
    /// ボリュームの重みを設定するヘルパーメソッド。
    /// </summary>
    /// <param name="volume">対象のボリューム</param>
    /// <param name="weight">設定する重み</param>
    private void SetVolumeWeight(Volume volume, float weight)
    {
        if (volume != null)
        {
            volume.weight = Mathf.Clamp01(weight);
        }
    }
}
