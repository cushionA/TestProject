using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TNRD;
using UnityEngine;
using static Destructible2D.D2dSplitter;
using UnityEngine.SocialPlatforms.Impl;
using static CharacterController;

/// <summary>
///　セーブするときにスクリプトから情報を吸い出す
///　Awakeではデータを使う処理はしない
///　スタートでそういう処理をする、というルールを徹底
///　
/// 必要な情報
/// ・キャラクターコントローラーから状態異常とメイン状態
/// ・スコアマネージャーからプレイヤーの体力、プレイ時間
/// ・レベルマネージャーから現在の位置
/// ・マップデータから現在有効なオブジェクトリスト（レベルマネージャーに持たせる？）
/// ・いや、データはスクリプタブルだから保持されてるんだ・レベルマネージャーに持たせておくか（初期化されてるかみたいなフラグを作って初期化されてるレベルデータは処理をスキップ）
/// 
/// ES3Settings.defaultSettings.path = "MyNewFile.es3";　これでセーブデータの保存先を変える、複数データを持てるか

/// </summary>
public class SaveManager : MonoBehaviour
{

    [SerializeField]
    SaveMono[] _useList;
    

    /// <summary>
    /// シングルトンや静的インスタンスのこともあるし
    /// こいつのAwake処理は実行順の最後に置いた方がよくね
    /// </summary>
    // Start is called before the first frame update
    void Awake()
    {
        //この辺の処理はレべマネからの呼び出しに委託する
        //_useList = (SaveInterface[])_saveList;
        // Load();


    }



    /// <summary>
    /// UIのイベントでも呼べるみょん
    /// </summary>
    bool Save()
    {
        int count = _useList.Length;
        for (int i = 0;i < count; i++)
        {
            _useList[i].Save();

        }

        //ロードがタイミング違うので配列にまとめない
        LevelManager.instance.Save();
        
        return true;
    }





    /// <summary>
    /// ロードはAwakeにさせるべきじゃないの？
    /// それは本当にそう
    /// このスクリプトのAwakeでロードするか
    /// </summary>
    bool Load()
    {
        int count = _useList.Length;
        for (int i = 0; i < count; i++)
        {
            _useList[i].Load();
        }
        return true;
    }


    /// <summary>
    /// 非同期のロード処理
    /// </summary>
    /// <returns></returns>
    public async UniTask LoadAsync()
    {
        //からのロード
        bool end = false;


        end = Load();

        await UniTask.WaitUntil(() => end == true);
    }

    /// <summary>
    /// 非同期のセーブ処理
    /// </summary>
    /// <returns></returns>
    public async UniTask SaveAsync()
    {
        bool end = false;

        float s;
        Debug.Log($"dfセーブ中");

        s = Time.unscaledTime;
        end = Save();
        
        await UniTask.WaitUntil(() => end == true);



        //からのセーブ
        //Save();
        Debug.Log($"ssセーブ完了{Time.unscaledTime - s}");
    }



    public void ClearSave()
    {
        //スコアマネージャーの初期化

        ES3.Save<int>("PlayTime", 0);
        ES3.Save<int>("Score", 0);
        ES3.Save<int>("Life", 5);
        Debug.Log($"べｓｔ{ScoreManager.instance.BestScore}");
        ES3.Save<int>("BestScore", ScoreManager.instance.BestScore);

        //  キャラコントローラーの初期化
        ES3.Save("NowCondition", new List<EventObject.EventData>());
        ES3.Save("NowEffect", new GimickCondition());
        ES3.Save("PositionImfo", Vector3.zero);



        LevelManager.instance.NewSave(true);
    }

}
