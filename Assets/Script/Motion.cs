using UnityEngine;

public class Motion : StateMachineBehaviour
{
    [SerializeField]
    private float _timeUntilMotion; // モーションが開始されるまでの待ち時間
    [SerializeField]
    private int _numberOfMotion; // ランダムに選択されるモーションの数
    private float _idleTime; // アイドル状態の経過時間
    private int motionAnimation; // 選択されたモーションの番号
    private float _motionStartTime; // モーション開始時間
    private float _timetomotion = 4.0f;
    private Vector2 _startMotion; // モーションの開始位置
    private Vector2 _targetMotion; // モーションの目標位置

    private bool _tomotion;
    private bool _toreset;

    // OnStateEnter は、遷移が開始され、ステートマシンがこのステートの評価を開始するときに呼び出されます
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // モーション開始時に初期位置を設定
        animator.SetFloat("idelMotionX", 0);
        animator.SetFloat("idelMotionY", 0);
        ResetIdle();
    }


    // OnStateUpdate は、OnStateEnter と OnStateExit のコールバックの間の各 Update フレームに呼び出されます
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // アイドル状態の経過時間をカウント
        _idleTime += Time.deltaTime;
        // モーションを開始する条件を満たしたかどうかを確認
        if (_idleTime > _timeUntilMotion && _tomotion == false && _toreset == false)
        {
            // ランダムなモーションを選択
            motionAnimation = Random.Range(1, _numberOfMotion + 1);
            //Debug.Log("change to " + motionAnimation + " , " + _idleTime);

            // 選択されたモーションに応じて目標位置を設定
            switch (motionAnimation)
            {
                case 1:
                    _startMotion = new Vector2(animator.GetFloat("idelMotionX"), animator.GetFloat("idelMotionY"));
                    _targetMotion = new Vector2(1f, 0.1f);
                    break;
                case 2:
                    _startMotion = new Vector2(animator.GetFloat("idelMotionX"), animator.GetFloat("idelMotionY"));
                    _targetMotion = new Vector2(-1f, 0.1f);
                    break;
                case 3:
                    _startMotion = new Vector2(animator.GetFloat("idelMotionX"), animator.GetFloat("idelMotionY"));
                    _targetMotion = new Vector2(-0.1f, 1f);
                    break;
            }

            // モーションの開始時間を記録
            _motionStartTime = Time.time;
            _tomotion = true;

        }
        else if (_tomotion == true && _toreset == false)
        {
            //Debug.Log("Tomotion" + motionAnimation);
            // モーションを進行させる
            float elapsedTime = Time.time - _motionStartTime;
            float progress = Mathf.Clamp01(elapsedTime / _timetomotion); // 1.5秒かけて変更

            // モーションを補間して更新
            Vector2 currentMotion = Vector2.Lerp(_startMotion, _targetMotion, progress);
            animator.SetFloat("idelMotionX", currentMotion.x);
            animator.SetFloat("idelMotionY", currentMotion.y);
            //Debug.Log("TomotioncurrentMotion " + currentMotion);

            // モーションが完了したら、リセットフラグを立てる
            if (elapsedTime >= _timetomotion && stateInfo.normalizedTime % 1 > 0.97)
            {
                //Debug.Log("GOToReset" + motionAnimation);
                _idleTime = 0;
                _tomotion = false;
                _toreset = true;
                _motionStartTime = Time.time;
            }
        }
        if (_tomotion == false && _toreset == true)
        {
            //Debug.Log("ToReset" + motionAnimation);
            // モーションを進行させる
            float elapsedTime = Time.time - _motionStartTime;
            float progress = Mathf.Clamp01(elapsedTime / _timetomotion);
            //Debug.Log("elapsedTime" + elapsedTime);

            // モーションを補間して更新
            _startMotion = new Vector2(animator.GetFloat("idelMotionX"), animator.GetFloat("idelMotionY"));
            _targetMotion = new Vector2(-0.01f, -0.01f);
            //Debug.Log("_startMotion " + _startMotion + "_targetMotion" + _targetMotion);

            Vector2 currentMotion = Vector2.Lerp(_startMotion, _targetMotion, progress);
            //Debug.Log("currentMotion " + currentMotion);
            animator.SetFloat("idelMotionX", currentMotion.x);
            animator.SetFloat("idelMotionY", currentMotion.y);

            // モーションが完了したら、リセットフラグを立てる
            if (elapsedTime >= _timetomotion)//elapsedTimeが
            {
                Debug.Log("GOToMotion" + motionAnimation);
                _idleTime = 0;
                _toreset = false;
                _tomotion = false;
            }
        }
    }
    // アイドル状態をリセットする
    private void ResetIdle()
    {
        _idleTime = 0;
        motionAnimation = 0;
        _tomotion = false;
        _toreset = false;
    }
}
