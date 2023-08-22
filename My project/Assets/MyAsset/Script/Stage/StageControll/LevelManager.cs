using Cysharp.Threading.Tasks;
using RenownedGames.Apex.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using static bl_SceneLoader;


/// <summary>
/// 現在どのマップのどのセグメントにいるかとかを保持するスクリプト
/// 
/// 考えること
/// 
/// ・起動時に前回最後に立っていたマップからどうやってロードするか
/// （草案:現在立ってるセグメントの起動オブジェクトをプールしておく。そしてそいつの名前からロードする？）
/// （それか現在立ってるオブジェクトのLevelControllerからメソッド呼び出す？コントローラーに前のシーンと次のシーンを持たせておいてさ。これがいいかも）
/// 
/// ・レベルをどうやってアンロードするか？
/// （レベルコントローラーで現在のセグメントが変わったらその時に前のセグメントや次のセグメントを消去するような仕組み作る？）
/// 
/// ・そのマップの最初のセグメント、番号０のセグメントが来たりしたら前のセグメントとかどうする？
/// （前のせぐめんとの名前のところに通路ぶち込んどくか、０の場合はマップ番号から通路を導く感じ？）
/// 
/// ・次のマップとか前のマップにまたがる通路とかだと同時にシーンを複数存在させないとダメだよね。
/// 基本追加シーンは全部オブジェクト無効化しておいて保存するか
/// 
/// 前のレベルに行くには前のシーンを呼び出して一個だけ有効化で処理が変わってくる
/// このコントローラーを起動させる前に
/// 
/// 

/// 
/// </summary>
public class LevelManager : MonoBehaviour,SaveInterface
{
    

    [Serializable]
    public class NextSceneName : SerializableDictionary<Vector2Int, string>
    {
        [SerializeField]
        private List<Vector2Int> keys;

        [SerializeField]
        private List<string> values;

        protected override List<Vector2Int> GetKeys()
        {
            return keys;
        }

        protected override List<string> GetValues()
        {
            return values;
        }

        protected override void SetKeys(List<Vector2Int> keys)
        {
            this.keys = keys;
        }

        protected override void SetValues(List<string> values)
        {
            this.values = values;
        }
    }

        [ES3NonSerializable]
    public static LevelManager instance;

    /// <summary>
    /// 現在どのマップのどのセグメントにいるか
    /// </summary>
    Vector2Int nowPointer;

    /// <summary>
    /// 現在いるマップのデータ
    /// 通路で切り替える、それでいいな
    /// さとるゲーの場合はチェックポイントで
    /// </summary>
 //   [HideInInspector]
    public LevelData nowData;

    [Header("マップの名前")]
    /// <summary>
    /// 現在の位置から繋がる次のマップの名前を取得
    /// </summary>
    [SerializeField]
    [Header("現在地から次のマップの名前を獲得")]
    NextSceneName nameIndex;

    /// <summary>
    /// ロードの挙動管理
    /// </summary>
    AsyncOperation container;


    AsyncOperationHandle<SceneInstance> containerA;

    [SerializeField]
    [Header("Addressablesを使用してシーン管理をするか")]
    bool isAddressable;

    /// <summary>
    /// ゲームロードから初回起動されたセグメントであるかどうか
    /// 一度きり使うやつ
    /// 最初のセグメントはいろいろやることがある
    /// 偽の時だけ最初
    /// </summary>
    [HideInInspector]
    public bool isFirst;

 //   SaveManager _save;

    /// <summary>
    /// レベルデータのリスト
    /// こいつはマップポインターのXと連動するように
    /// </summary>
    [SerializeField]
    [Header("マップデータの格納")]
    LevelData[] dataList;


    // Start is called before the first frame update
    void Awake()
    {

        if (instance == null)
        {
            instance = this;

        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
        Load();
    }



    /// <summary>
    /// 最初のレベルをロードする
    /// レベルデータのマップの名前からシーンをロード
    /// それでレベルデータのセグメントの名前からセグメントをロード
    /// </summary>
    public void FirstLevelLoad()
    {
        //すでにロードされていないならロード
        //この処理はAddressablesも共通
        if (SceneManager.GetSceneByName(nowData.mapName).IsValid() == false)
        {

            if (!isAddressable)
            {
                //ロードするシーン名と同じであるため、gameobject名を使ってシーンをロードする。
                //   container = SceneManager.LoadSceneAsync(nameIndex[nowPointer], LoadSceneMode.Additive);
               // container = SceneManager.LoadSceneAsync("Test!", LoadSceneMode.Additive);

                //今のデータのマップの名前からロード
                container = SceneManager.LoadSceneAsync(nowData.mapName, LoadSceneMode.Additive);

                //We set it to true to avoid loading the scene twice

                //こいつはシーンローダーから許しが出たら可能に
                container.allowSceneActivation = false;
            }
            else
            {
                //activateOnLoad（初期値true）をFalseにすることでActivateメソッド呼ばれるまでは実体化しない。
                containerA = Addressables.LoadSceneAsync(nowData.mapName, LoadSceneMode.Additive, activateOnLoad : false);
                //containerA.Result.ActivateAsync
            }

        }
    }


    /// <summary>
    /// レベルをロードする
    /// ロードするはいいけどどのセグメントを有効にするかは悩むところ
    /// 通路にそういう機能を持たせるか、
    /// ゲームオブジェクト二つ登録してさ
    /// </summary>
    public void LoadLevel()
    {
        //すでにロードされていないならロード
        if (SceneManager.GetSceneByName(nameIndex[nowPointer]).IsValid() == false)
        {
            if (!isAddressable)
            {
                //ロードするシーン名と同じであるため、gameobject名を使ってシーンをロードする。
                SceneManager.LoadSceneAsync(nameIndex[nowPointer], LoadSceneMode.Additive);
                //We set it to true to avoid loading the scene twice

            }
            else
            {
                //ロードするシーン名と同じであるため、gameobject名を使ってシーンをロードする。
                Addressables.LoadSceneAsync(nameIndex[nowPointer], LoadSceneMode.Additive);
                //We set it to true to avoid loading the scene twice
            }


        }
    }

    /// <summary>
    /// レベルをアンロードする、あとロード中ならロード中止
    /// </summary>
    public void UnLoadLevel()
    {
        if (!isAddressable)
        {
            SceneManager.UnloadSceneAsync(nameIndex[nowPointer]);
        }
    }



    /// <summary>
    /// データをアップデートする
    /// トリガーを通り過ぎた時にアップデート
    /// </summary>
    /// <param name="pointer"></param>
    /// <param name="isNext"></param>
    /// <param name="isFinal"></param>
    public void DataUpdate(Vector2Int pointer)
    {
        nowPointer.x = pointer.x;
        nowPointer.y = pointer.y;

    }


    /// <summary>
    /// シーンローダーからサブシーンを呼んであげる
    /// </summary>
    public async UniTask SubSceneCall()
    {
        if (!isAddressable)
        {
            container.allowSceneActivation = true;
           // container = null;
        }
        else
        {
            await containerA.Result.ActivateAsync();
        }
        //ここでセグメントの有効化まで

        await FirstSegmentActive();
    }

    /// <summary>
    /// シーンローダーからサブシーンを呼んであげる
    /// </summary>
    public float SubSceneProgress()
    {
        if (!isAddressable)
        {
            return container.progress / 2;

        }
        else
        {
            return containerA.PercentComplete / 2;
        }
    }

        /// <summary>
        /// セーブ機能
        /// 必ずしも変数いらんね、文字列キーで呼び出して使えばいいし
        /// </summary>
        public void Save()
    {
        //復帰時にやる処理
        //まずデータをマネージャーに戻す。プレイヤーを位置変更（ここまでAwake）

        //マップの今のマップとセグメントデータに従ってマップロード
        //セグメント有効化
        //マップの一つ一つのオブジェクトが今自分が有効かどうかを自分の番号で問い合わせて真偽型で消える（Destroyかdisenaかは自分で決める）
        //ここまでStart

        ES3.Save( "MapImfo",nowData);
        ES3.Save("SegmentImfo", nowPointer);

    }

    /// <summary>
    /// セーブデータをロードする
    /// 
    /// 
    /// </summary>
    public void Load()
    {

        // ES3.Load("MapImfo", nowData); みたいにロードすればデータが存在しないときにnowdata変数の内容を返してくれる。
        //でもセーブデータ作成して始めるはずだからいらないか
       nowPointer = ES3.Load<Vector2Int>("SegmentImfo");
            nowData = dataList[nowPointer.x];

       SetData(ES3.Load<LevelData>("MapImfo"));
    }

    /// <summary>
    /// レベルデータの数値入れ
    /// </summary>
    void SetData(LevelData data)
    {
        if(data == null)
        {
            return;
        }

        int count = nowData.disenableEnemy.Length;

        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                nowData.disenableEnemy[i] = data.disenableEnemy[i];
            }

        }

        count = nowData.disenableObjects.Length;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                nowData.disenableObjects[i] = data.disenableObjects[i];
            }

        }

    }


    /// <summary>
    /// シーンがロードされるまで待ってセグメントを有効にする
    /// 
    /// 流れとしてはシーンローダーでシーンがロードされる、ロードされたらこっちの追加シーンもロード
    /// そしてセーブデータをロード、セグメント有効化する（セグメントからのオブジェクトが無効かどうかの問い合わせもここ）
    /// </summary>
    /// <returns></returns>
    async UniTask FirstSegmentActive()
    {
        if (!isAddressable)
        {
            Debug.Log($"{container ==null}");
            //シーンがロードされるまで待つ
            await UniTask.WaitUntil(() => container.isDone);
            container = null;
        }
        else
        {
            //シーンがロードされるまで待つ
            await UniTask.WaitUntil(() => containerA.IsDone);
        }

        await ScoreManager.instance.gameObject.GetComponent<SaveManager>().LoadAsync();

        //ここでセーブロード読み込み中…ってテキストを出すことも可能
       // await _save.LoadAsync();

        //ロードされたらセグメントを有効に
        //マップデータはAwakeですでに用意してある
        GameObject.Find(nowData.segmentName[nowPointer.y]).SetActive(true);


    }

    /// <summary>
    /// オブジェクトが壊れたらその旨を知らせる
    /// </summary>
    /// <param name="ID"></param>
    public void ObjectBreak(int ID)
    {
        nowData.disenableObjects[ID] = true;
    }

    /// <summary>
    /// そのオブジェクトが壊れてるかどうか知らせる
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    public bool BreakCheck(int ID)
    {
        return nowData.disenableObjects[ID];
    }


    


}
