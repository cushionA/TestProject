using UnityEngine;
using UnityEngine.SceneManagement;

public enum CheckMethod
{
    Distance,
    Trigger
}


/// <summary>
/// ひとつひとつのシーン呼び出す用のゲームオブジェクトにつけてあげる
/// そしてそのオブジェクトの名前のシーンが呼ばれる
/// gameobject.nameでシーンをロードしてるのはそういうこと
/// 
/// これとは別にマネージャー作るか。
/// 今どのマップにいてどのセグメントにいるのかってやつ
/// 今とその前、あと次のレベルを記録する
/// </summary>

public class LevelController : MonoBehaviour
{
    /// <summary>
    /// プレイヤーのポジションとかみんな使いまくるんだからGManagerにプールしとくべきでは？
    /// TransformPosi取得は重いんだって
    /// </summary>
    public Transform player;

    public CheckMethod checkMethod;
    public float loadRange;

    //Scene state
    private bool isLoaded;
    private bool shouldLoad;


    void Start()
    {
        //このシーンがシーンマネージャーに含まれてるかの確認
        if (SceneManager.sceneCount > 0)
        {
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == gameObject.name)
                {
                    isLoaded = true;
                }
            }
        }
    }

    void Update()
    {
        //どの方法を使うかチェック
        if (checkMethod == CheckMethod.Distance)
        {
            DistanceCheck();
        }
        else if (checkMethod == CheckMethod.Trigger)
        {
            TriggerCheck();
        }
    }

    /// <summary>
    /// 距離チェックでマップ起動
    /// </summary>
    void DistanceCheck()
    {
        //Checking if the player is within the range
        if (Vector3.Distance(player.position, transform.position) < loadRange)
        {
            LoadScene();
        }
        else
        {
            UnLoadScene();
        }
    }

    void LoadScene()
    {
        if (!isLoaded)
        {
            //ロードするシーン名と同じであるため、gameobject名を使ってシーンをロードする。
            SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            //We set it to true to avoid loading the scene twice
            isLoaded = true;
        }
    }

    void UnLoadScene()
    {
        if (isLoaded)
        {
            SceneManager.UnloadSceneAsync(gameObject.name);
            isLoaded = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shouldLoad = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shouldLoad = false;
        }
    }

    void TriggerCheck()
    {
        //shouldLoad is set from the Trigger methods
        if (shouldLoad)
        {
            LoadScene();
        }
        else
        {
            UnLoadScene();
        }
    }


}