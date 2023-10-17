using UnityEngine;
using PathologicalGames;
using UnityEngine.AddressableAssets;



/// <summary>
/// オブジェクトプールの生成と破棄などの処理をデリゲートで上書き
/// </summary>
public class MyInstanceDelegate : MonoBehaviour
{
    private void Awake()
    {
        // 早急にGlobal PoolManagerデリゲートを設定する。
        InstanceHandler.InstantiateDelegates = this.InstantiateDelegate;
        InstanceHandler.DestroyDelegates = this.DestroyDelegate;

      //  InstanceHandler.InstantiateDelegates.na
    }

    public  GameObject InstantiateDelegate(GameObject location, Vector3 pos, Quaternion rot)
    {
        Debug.Log("Using my own instantiation delegate on prefab '" + location.name + "'!");

        return Addressables.InstantiateAsync(location.name, pos, rot).WaitForCompletion();
    }

    public void DestroyDelegate(GameObject instance)
    {
        //Debug.Log("Using my own destroy delegate on '" + instance.name + "'!");

        Addressables.ReleaseInstance(instance);
    }



}