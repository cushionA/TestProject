using UnityEngine;
using PathologicalGames;
using UnityEngine.AddressableAssets;



/// <summary>
/// �I�u�W�F�N�g�v�[���̐����Ɣj���Ȃǂ̏������f���Q�[�g�ŏ㏑��
/// </summary>
public class MyInstanceDelegate : MonoBehaviour
{
    private void Awake()
    {
        // ���}��Global PoolManager�f���Q�[�g��ݒ肷��B
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