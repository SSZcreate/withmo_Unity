using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SkyboxCycle : MonoBehaviour
{
    [Header("Skybox Materials")]
    [SerializeField] private Material daySkybox;        // 昼用のスカイボックス
    [SerializeField] private Material eveningSkybox;    // 夕方用のスカイボックス
    [SerializeField] private Material nightSkybox;      // 夜用のスカイボックス
    [SerializeField, Range(0f, 1f)] private float fadeRate = 0.5f; // フェード速度

    [Header("Post-Processing Volumes (URP)")]
    [SerializeField] private Volume dayVolume;
    [SerializeField] private Volume eveningVolume;
    [SerializeField] private Volume nightVolume;

    private Material[] skyboxes;
    private Volume[] volumes;
    private int currentSkyboxIndex = 0;
    private Coroutine fadeCoroutine;

    public enum SkyboxMode
    {
        DayFixed,
        EveningFixed,
        NightFixed
    }

    private SkyboxMode currentMode;

    void Start()
    {
        // スカイボックスとボリュームを配列に格納
        skyboxes = new Material[] { daySkybox, eveningSkybox, nightSkybox };
        volumes = new Volume[] { dayVolume, eveningVolume, nightVolume };

        // 初期スカイボックスとポストプロセスの設定
        RenderSettings.skybox = skyboxes[currentSkyboxIndex];
        SetVolumeWeights(currentSkyboxIndex, 1f); // 最初のボリュームをフルに適用
        RenderSettings.skybox.SetFloat("_Blend", 0f);  // スカイボックスのブレンドを0に設定

        currentMode = SkyboxMode.DayFixed; // デフォルトで昼固定モード
    }

    void Update()
    {
        // TimeBasedモードは削除されたため、Update内の処理は不要です
    }

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

    // 昼に変更する
    public void ChangeDay()
    {
        StopCurrentFade();
        StartFade(0); // 0は昼のインデックス
    }

    // 夕方に変更する
    public void ChangeEvening()
    {
        StopCurrentFade();
        StartFade(1); // 1は夕方のインデックス
    }

    // 夜に変更する
    public void ChangeNight()
    {
        StopCurrentFade();
        StartFade(2); // 2は夜のインデックス
    }

    // フェード処理を開始する
    private void StartFade(int nextSkyboxIndex)
    {
        fadeCoroutine = StartCoroutine(SkyboxAndVolumeFadeRoutine(nextSkyboxIndex));
    }

    IEnumerator SkyboxAndVolumeFadeRoutine(int nextSkyboxIndex)
    {
        Material currentSkybox = RenderSettings.skybox;
        Material nextSkybox = skyboxes[nextSkyboxIndex];
        Volume currentVolume = volumes[currentSkyboxIndex];
        Volume nextVolume = volumes[nextSkyboxIndex];

        float blend = 0f;

        // フェード処理を行うループ
        while (blend < 1f)
        {
            // スカイボックスのブレンド値を更新
            currentSkybox.SetFloat("_Blend", blend);

            // 現在のボリュームの重みを減らし、次のボリュームの重みを増やす
            SetVolumeWeight(currentVolume, Mathf.Lerp(1f, 0f, blend));
            SetVolumeWeight(nextVolume, Mathf.Lerp(0f, 1f, blend));

            blend += fadeRate * Time.deltaTime;
            yield return null;
        }

        // 次のスカイボックスに完全に切り替え
        RenderSettings.skybox = nextSkybox;
        RenderSettings.skybox.SetFloat("_Blend", 0f);

        // インデックスを更新
        currentSkyboxIndex = nextSkyboxIndex;
    }

    // 指定されたスカイボックスとボリュームを設定する
    private void SetSkyboxAndVolume(int index)
    {
        RenderSettings.skybox = skyboxes[index];
        RenderSettings.skybox.SetFloat("_Blend", 0f);
        SetVolumeWeights(index, 1f);
        currentSkyboxIndex = index;
    }

    // 現在のフェード処理を停止する
    private void StopCurrentFade()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }

    // 特定のインデックスに対応するボリュームの重みを設定する
    private void SetVolumeWeights(int activeIndex, float activeWeight)
    {
        for (int i = 0; i < volumes.Length; i++)
        {
            SetVolumeWeight(volumes[i], (i == activeIndex) ? activeWeight : 0f);
        }
    }

    // ボリュームの重みを設定するヘルパーメソッド
    private void SetVolumeWeight(Volume volume, float weight)
    {
        if (volume != null)
        {
            volume.weight = weight;
        }
    }
}
