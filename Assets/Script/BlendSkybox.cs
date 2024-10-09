using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class SkyboxCycle : MonoBehaviour
{
    [SerializeField] Material daySkybox;    // 昼用のスカイボックス
    [SerializeField] Material eveningSkybox; // 夕方用のスカイボックス
    [SerializeField] Material nightSkybox;   // 夜用のスカイボックス
    [SerializeField, Range(0f, 1f)] float fadeRate = 0.01f;

    // ポストプロセスボリューム
    [SerializeField] PostProcessVolume dayVolume;
    [SerializeField] PostProcessVolume eveningVolume;
    [SerializeField] PostProcessVolume nightVolume;

    private Material[] skyboxes;
    private PostProcessVolume[] volumes;
    private int currentSkyboxIndex;
    private Coroutine fadeCoroutine;

    public enum SkyboxMode
    {
        DayFixed,
        NightFixed,
        TimeBased
    }

    private SkyboxMode currentMode;

    void Start()
    {
        // スカイボックスとポストプロセスボリュームを配列に格納
        skyboxes = new Material[] { daySkybox, eveningSkybox, nightSkybox };
        volumes = new PostProcessVolume[] { dayVolume, eveningVolume, nightVolume };

        // 現在の時刻に基づいて初期スカイボックスを設定
        DateTime currentTime = DateTime.Now;
        SetInitialSkybox(currentTime);

        // 初期設定として、最初のスカイボックスとポストプロセスの重みを設定
        RenderSettings.skybox = skyboxes[currentSkyboxIndex];
        SetPostProcessWeights(currentSkyboxIndex, 1f); // 最初のボリュームをフルに適用
        RenderSettings.skybox.SetFloat("_Blend", 0f);  // スカイボックスのブレンドを0に設定

        currentMode = SkyboxMode.TimeBased; // デフォルトで時間により変動モード
    }

    void Update()
    {
        if (currentMode == SkyboxMode.TimeBased)
        {
            // 時間により変動モードの処理を行う
            DateTime currentTime = DateTime.Now;
            int newSkyboxIndex = (currentTime.Hour >= 5 && currentTime.Hour < 16) ? 0 :
                                 (currentTime.Hour >= 16 && currentTime.Hour < 19) ? 1 : 2;

            if (newSkyboxIndex != currentSkyboxIndex)
            {
                NextSkybox(); // スカイボックスを切り替える
            }
        }
    }

    // 昼固定モードに設定する
    public void SetDayFixedMode()
    {
        currentMode = SkyboxMode.DayFixed;
        StopCurrentFade();
        SetSkyboxAndPostProcess(0); // 0は昼のインデックス
    }

    // 夜固定モードに設定する
    public void SetNightFixedMode()
    {
        currentMode = SkyboxMode.NightFixed;
        StopCurrentFade();
        SetSkyboxAndPostProcess(2); // 2は夜のインデックス
    }

    // 時間により変動するモードに設定する
    public void SetTimeBasedMode()
    {
        currentMode = SkyboxMode.TimeBased;
        StopCurrentFade();
        DateTime currentTime = DateTime.Now;
        SetInitialSkybox(currentTime);
        RenderSettings.skybox = skyboxes[currentSkyboxIndex];
        SetPostProcessWeights(currentSkyboxIndex, 1f);
    }

    // 時刻に基づいてスカイボックスを設定
    void SetInitialSkybox(DateTime currentTime)
    {
        if (currentTime.Hour >= 5 && currentTime.Hour < 16)
        {
            currentSkyboxIndex = 0;  // 昼スカイボックス
        }
        else if (currentTime.Hour >= 16 && currentTime.Hour < 19)
        {
            currentSkyboxIndex = 1;  // 夕方スカイボックス
        }
        else
        {
            currentSkyboxIndex = 2;  // 夜スカイボックス
        }
    }

    public void NextSkybox()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // 次のスカイボックスに切り替え、フェード処理を開始
        int nextSkyboxIndex = (currentSkyboxIndex + 1) % skyboxes.Length;
        fadeCoroutine = StartCoroutine(SkyboxAndPostProcessFadeRoutine(nextSkyboxIndex));
    }

    IEnumerator SkyboxAndPostProcessFadeRoutine(int nextSkyboxIndex)
    {
        Material currentSkybox = RenderSettings.skybox;
        Material nextSkybox = skyboxes[nextSkyboxIndex];
        PostProcessVolume currentVolume = volumes[currentSkyboxIndex];
        PostProcessVolume nextVolume = volumes[nextSkyboxIndex];

        float blend = 0f;

        // ブレンド処理を行うループ
        while (blend < 1f)
        {
            // スカイボックスのブレンド値を更新
            currentSkybox.SetFloat("_Blend", blend);

            // 現在のポストプロセスの重みを減らし、次のポストプロセスの重みを増やす
            currentVolume.weight = Mathf.Lerp(1f, 0f, blend);
            nextVolume.weight = Mathf.Lerp(0f, 1f, blend);

            blend += fadeRate * Time.deltaTime;
            yield return null;
        }

        // 次のスカイボックスに完全に切り替え
        RenderSettings.skybox = nextSkybox;
        RenderSettings.skybox.SetFloat("_Blend", 0f);

        // インデックスを更新
        currentSkyboxIndex = nextSkyboxIndex;
    }

    // 指定されたスカイボックスとポストプロセスを設定する
    private void SetSkyboxAndPostProcess(int index)
    {
        RenderSettings.skybox = skyboxes[index];
        RenderSettings.skybox.SetFloat("_Blend", 0f);
        SetPostProcessWeights(index, 1f);
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

    // 特定のインデックスに対応するポストプロセスボリュームの重みを設定する
    void SetPostProcessWeights(int activeIndex, float activeWeight)
    {
        for (int i = 0; i < volumes.Length; i++)
        {
            volumes[i].weight = (i == activeIndex) ? activeWeight : 0f;
        }
    }
}
