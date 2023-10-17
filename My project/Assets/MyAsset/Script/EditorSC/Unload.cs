using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static CharacterController;
using PathologicalGames;
using Cysharp.Threading.Tasks.Triggers;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Unload : MonoBehaviour
{

    [SerializeField]
    SpawnPool pool;

    [SerializeField]
    int mapNum;

#if UNITY_EDITOR
    [ContextMenu("未使用オブジェクトをアンロード")]
    public async UniTaskVoid UnLoadMethod()
    {
        Debug.Log("アンロード");
        await Resources.UnloadUnusedAssets();
    }


    [ContextMenu("読み込まれてるシーンを確認")]
    public void CheckScene()
    {
        //現在読み込まれているシーン数だけループ
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {

            //読み込まれているシーンを取得し、その名前をログに表示
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name;
            Debug.Log(sceneName);

        }
    }


    [ContextMenu("scene内のイベントオブジェクトにマップ番号とID付与")]
    public void SetID()
    {
        EventObject[] objs = FindObjectsOfType<EventObject>();

        int count = objs.Length;

        int i = 0;

        //現在読み込まれているシーン数だけループ
        foreach(EventObject obj in objs)
        {
            if (obj.BreakableCheck())
            {
                obj.SetID(i,mapNum);
                
                Debug.Log($"ID：{i}");
                i++;
            }
        }

        Debug.Log($"{i}個の破壊可能オブジェクト");
    }
    [ContextMenu("scene内のイベントオブジェクトのID確認")]
    public void ReadID()
    {
        EventObject[] objs = FindObjectsOfType<EventObject>();

        int count = objs.Length;

        int i = 0;

        //現在読み込まれているシーン数だけループ
        foreach (EventObject obj in objs)
        {
            if (obj.BreakableCheck())
            {

                Debug.Log($"ID：{obj.GetID()}");
                i++;
            }
        }
    }


    [ContextMenu("新しいセーブデータを作成")]
    public void NewSave()
    {
        //スコアマネージャーの初期化

        ES3.Save("PlayTime", 0);
        ES3.Save("Score", 0);
        ES3.Save("Life", 5);


        //  キャラコントローラーの初期化
        ES3.Save("NowCondition", new List<EventObject.EventData>());
        ES3.Save("NowEffect", new GimickCondition());
        ES3.Save("PositionImfo", Vector3.zero);

        LevelManager.instance.NewSave();
    }

    [ContextMenu("プレハブの数を教えて")]
    public void DisplayPool()
    {
        //スコアマネージャーの初期化
       int count = pool.prefabPools.Count;
        for (int i = 0;i<count;i++)
        {
         //   Debug.Log($"{i + 1}個めのPoolは{pool.prefabPools[i].totalCount}");
        }
    }


#endif

}
