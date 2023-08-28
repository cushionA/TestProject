using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Unload : MonoBehaviour
{

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

#endif

}
