using Cysharp.Threading.Tasks;
using RenownedGames.Apex.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
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
public class LevelManager : SaveMono
{
    

    [Serializable]
    public class NextSceneName : SerializableDictionary<Vector2Int, int>
    {
        [SerializeField]
        private List<Vector2Int> keys;

        [SerializeField]
        private List<int> values;

        protected override List<Vector2Int> GetKeys()
        {
            return keys;
        }

        protected override List<int> GetValues()
        {
            return values;
        }

        protected override void SetKeys(List<Vector2Int> keys)
        {
            this.keys = keys;
        }

        protected override void SetValues(List<int> values)
        {
            this.values = values;
        }
    }

        [ES3NonSerializable]
    public static LevelManager instance;

    [SerializeField]
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


 //   AsyncOperationHandle<SceneInstance> containerA;

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

    /// <summary>
    /// ロード時間はかるよ
    /// </summary>
    float timeChecek;






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
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
    }





    #region マップロードとアンロード




    /// <summary>
    /// レベルをロードする
    /// ロードするはいいけどどのセグメントを有効にするかは悩むところ
    /// 通路にそういう機能を持たせるか、
    /// ゲームオブジェクト二つ登録してさ
    /// </summary>
    public async UniTaskVoid LoadLevel(int num)
    {
        //けすべきマップないならやめとく
        if (nameIndex.ContainsKey(nowPointer) == false || dataList[nameIndex[nowPointer]] == null)
        {
            return;
        }


        string name = dataList[nameIndex[nowPointer]].mapName;
        //すでにロードされていないならロード
        if (SceneManager.GetSceneByName(name).IsValid() == false)
        {

            //ここでAwaitしてマップのロード待つ間見えない壁出したりしてもいいよ
          
            if (!isAddressable)
            {
                //ロードするシーン名と同じであるため、gameobject名を使ってシーンをロードする。
               await SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
                //We set it to true to avoid loading the scene twice

            }
            else
            {
                //ロードするシーン名と同じであるため、gameobject名を使ってシーンをロードする。
                await Addressables.LoadSceneAsync(name, LoadSceneMode.Additive);
                //We set it to true to avoid loading the scene twice
            }

         //    await UniTask.Delay(TimeSpan.FromSeconds(10));


            //セグメント起動
            GameObject.Find($"{name}SceneRoot").transform.Find($"{name}Segment{num}").gameObject.SetActive(true);

 

         //  Debug.Log($"います？{obj != null}　生きてる？{obj.activeSelf}　お名前は{obj.name}");
        }
    }


    /// <summary>
    /// レベルをアンロードする、あとロード中ならロード中止
    /// </summary>
    public void UnLoadLevel()
    {
        if (!isAddressable)
        {
            //けすべきマップないならやめとく
            if (nameIndex.ContainsKey(nowPointer) == false || dataList[nameIndex[nowPointer]] == null)
            {
                return;
            }

            SceneManager.UnloadSceneAsync(dataList[nameIndex[nowPointer]].mapName);
            //使ってないアセットアンロード
            Resources.UnloadUnusedAssets();
        }
    }

    /// <summary>
    /// シーンローダーにサブシーンのロード率を教える
    /// </summary>
    public float SubSceneProgress()
    {
        if (!isAddressable)
        {
            return container.progress + 0.1f / 2;

        }
        else
        {
            return container.progress + 0.1f / 2;
            // return containerA.PercentComplete + 0.1f / 2;
        }
    }

    /// <summary>
    /// シーンローダーからサブシーンを呼んであげる
    /// メインシーンがロードできたらそこにサブシーンロードして
    /// セグメントを有効に
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
            //  Debug.Log($"{containerA.DebugName}");
            //   await containerA.Result.ActivateAsync();
        }
        //ここでセグメントの有効化まで
        Debug.Log($"おわ");
        await FirstSegmentActive();
    }

    /// <summary>
    /// 最初のレベルをロードする
    /// レベルデータのマップの名前からシーンをロード
    /// それでレベルデータのセグメントの名前からセグメントをロード
    /// </summary>
    public void FirstLevelLoad()
    {


        //ろーどはここでやるからマネージャにまとめないでね
        Load();



        //ここで最初のマップは素手に変更が加えられてることに

        nowData.isChanged = true;

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
             //   containerA = Addressables.LoadSceneAsync(nowData.mapName, LoadSceneMode.Additive, activateOnLoad : false);
                //containerA.Result.ActivateAsync

               
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

            //シーンがロードされるまで待つ
            await UniTask.WaitUntil(() => container.isDone);

        }
        else
        {
            //シーンがロードされるまで待つ
            //  await UniTask.WaitUntil(() => containerA.IsDone);
        }



        await ScoreManager.instance.gameObject.GetComponent<SaveManager>().LoadAsync();


        //ここでセーブロード読み込み中…ってテキストを出すことも可能
        // await _save.LoadAsync();

        //ロードされたらセグメントを有効に
        //マップデータはAwakeですでに用意してある
        //SegmentNameが必要な理由は別のマップのセグメント0さんとかが起きちゃう可能性あるから
        //マップ名で良くね
        //   Debug.Log($"{nowData.mapName}Segment{nowPointer.y}");


        //非アクティブなオブジェクトはルートオブジェクトのトランスフォームからたどって探すね
        SegmentSerch(nowPointer).SetActive(true);
            container = null;
        //使ってないアセットアンロード
        await Resources.UnloadUnusedAssets();

    }

    /// <summary>
    /// マップ情報からセグメント探してくる
    /// </summary>
    /// <returns></returns>
    public GameObject SegmentSerch(Vector2Int pointer)
    {
        string name = dataList[pointer.x].mapName;
        return GameObject.Find($"{name}SceneRoot").transform.Find($"{name}Segment{pointer.y}").gameObject;
    }



    #endregion



    #region 位置情報管理

    /// <summary>
    /// データをアップデートする
    /// トリガーを通り過ぎた時にアップデート
    /// </summary>
    /// <param name="pointer"></param>
    /// <param name="isNext"></param>
    /// <param name="isFinal"></param>
    public void DataUpdate(Vector2Int pointer)//,bool final)
    {
        //マップ番号変わってんのなら
        if (nowPointer.x != pointer.x)
        {
            nowData = dataList[pointer.x];

            //後マップ情報に変化アリってことで

            nowData.isChanged = true;
        }

        if (pointer.x == 1 && pointer.y == 0) { 
            Debug.Log($"更新{pointer}");
    }
        nowPointer.x = pointer.x;
        nowPointer.y = pointer.y;

    }

    /// <summary>
    /// 現在どのセグメントにいるかを教える
    /// </summary>
    /// <returns></returns>
    public int GetSegment(bool isHorizon = false)
    {
        if (!isHorizon)
            return nowPointer.y;
        else
            return nowPointer.x;
    }


    #endregion


    #region　データ関連

    /// <summary>
    /// セーブ機能
    /// 必ずしも変数いらんね、文字列キーで呼び出して使えばいいし
    /// </summary>
    public override void Save()
    {
        //復帰時にやる処理
        //まずデータをマネージャーに戻す。プレイヤーを位置変更（ここまでAwake）

        //マップの今のマップとセグメントデータに従ってマップロード
        //セグメント有効化
        //マップの一つ一つのオブジェクトが今自分が有効かどうかを自分の番号で問い合わせて真偽型で消える（Destroyかdisenaかは自分で決める）
        //ここまでStart

        ES3.Save( "MapImfo",dataList);
        ES3.Save("SegmentImfo", nowPointer);

    }

    /// <summary>
    /// 新しいデータを作る
    /// </summary>
    public void NewSave(bool isClear = false)
    {
        if (!isClear)
        {
            ES3.Save<int>("PlayTime", 0);
            ES3.Save<int>("Score", 0);
            ES3.Save<int>("Life", 5);
            ES3.Save<int>("BestScore", 0);

            //  キャラコントローラーの初期化
            ES3.Save("NowCondition", new List<EventObject.EventData>());
            // ES3.Save("NowEffect", new GimickCondition());
            ES3.Save("PositionImfo", Vector3.zero);
        }
        //復帰時にやる処理
        //まずデータをマネージャーに戻す。プレイヤーを位置変更（ここまでAwake）
        ES3.Save("SegmentImfo", Vector2Int.zero);

       //仕様はマップの今のマップとセグメントデータに従ってマップロード
        //セグメント有効化
        //マップの一つ一つのオブジェクトが今自分が有効かどうかを自分の番号で問い合わせて真偽型で消える（Destroyかdisenaかは自分で決める）
        //ここまでStart
        MapDataCleaner(); 
        //スコアマネージャーの初期化
　　　
        dataList[0].isChanged = true;
　ES3.Save("MapImfo",  dataList);
        //ES3.Save("MapImfo", dataList);

    }







    /// <summary>
    /// マップのデータを洗浄
    /// セーブしてゲーム終わる前や、データロードした時に変更があったマップに関してはまっさらに戻しておく
    /// そうするとセーブした時点からの変更を消してまっさらにできる
    /// </summary>
    void MapDataCleaner()
    {
        int count = dataList.Length;

        for (int i = 0; i < count; i++)
        {
            if (dataList[i].isChanged)
            {

                //オブジェクトと敵を再度有効に
                int count2 = dataList[i].disenableObjects.Length;
                for (int s = 0; s < count2; s++)
                {
                    dataList[i].disenableObjects[s] = false;
                }

                count2 = dataList[i].disenableEnemy.Length;
                for (int s = 0; s < count2; s++)
                {
                    dataList[i].disenableEnemy[s] = false;
                }
            }

                dataList[i].isChanged = false;

        }

    }


    /// <summary>
    /// セーブデータをロードする
    /// 
    /// 
    /// </summary>
    public override void Load()
    {
        MapDataCleaner();
        // ES3.Load("MapImfo", nowData); みたいにロードすればデータが存在しないときにnowdata変数の内容を返してくれる。
        //でもセーブデータ作成して始めるはずだからいらないか
        nowPointer = ES3.Load<Vector2Int>("SegmentImfo");
        LevelData[] data = ES3.Load<LevelData[]>("MapImfo");


     SetData(data);
    }

    /// <summary>
    /// レベルデータのロード時の数値入れ
    /// 配列にひとつひとつ入れていく
    /// あとNowDataも仕込む
    /// </summary>
    void SetData(LevelData[] data)
    {
        if(data == null)
        {
            return;
        }

        int dCount = data.Length;

        

        for (int i = 0; i < dCount; i++)
        {
            //使うデータ
            LevelData useData = data[i];


            //データリストの中にセーブしていたデータを入れていく
            int count = useData.disenableEnemy.Length;
            if (count > 0)
            {
                for (int s = 0; s < count; s++)
                {
                    dataList[i].disenableEnemy[s] = useData.disenableEnemy[s];
                }

            }

            //データリストの中にセーブしていたデータを入れていく
            count = useData.disenableObjects.Length;
            if (count > 0)
            {
                for (int s = 0; s < count; s++)
                {
                    dataList[i].disenableObjects[s] = useData.disenableObjects[s];
                }

            }
        }

        //今のデータはポインターの位置で決める
        nowData = dataList[nowPointer.x];

    }


    /// <summary>
    /// オブジェクトが壊れたらその旨を記録する
    /// </summary>
    /// <param name="ID"></param>
    public void ObjectBreak(int ID,int mapNum)
    {
        if (dataList[mapNum].isChanged == false)
        {
            dataList[mapNum].isChanged = true;
        }
        // Debug.Log($"マイID:{ID}");
        dataList[mapNum].disenableObjects[ID] = true;
    }



    /// <summary>
    /// そのオブジェクトが壊れてるかどうか知らせる
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    public bool BreakCheck(int ID,int mapNum)
    {
        if (dataList[mapNum].disenableObjects.Length <= ID)
        {
            Debug.Log($"ｗｗｗｗ{dataList[mapNum]}");
        }

        return dataList[mapNum].disenableObjects[ID];
    }
    #endregion


    #region 死んだときのためのリスポーン処理

    /// <summary>
    /// 今のセグメントの一番下の位置に行って
    /// セグメントのPassを取り消して前のマップやセグメントを起動
    /// やり方はセグメントを番号とマップネームで探して処理をお願いして
    /// プレイヤーの座標をセグメントの位置＋１０に書き変える
    /// でもセグメントのチェックポイント通ってなかったら現在位置が前のセグメントだと誤解されるかも
    /// どうしよう
    /// </summary>
    public void Resporn()
    {
        SegmentSerch(nowPointer).GetComponent<SegmentTrigger>().RespornSegment();
    }


    #endregion

    /// <summary>
    /// 新しいレベルマネージャを生成する
    /// </summary>
    public void NewInstance()
    {
        Destroy(instance);
        Resources.UnloadUnusedAssets();
        this.AddComponent<LevelManager>();
    }


    /// <summary>
    /// こいつはただのロード時間計測用のメソッド
    /// いつか消してもいい
    /// </summary>
    public void TimeStart()
    {
        timeChecek = Time.unscaledTime;
    }
    /// <summary>
    /// 上に同じ
    /// </summary>
    /// <returns></returns>
    public float TimeEnd()
    {
        return Time.unscaledTime - timeChecek;
    }
    


}
