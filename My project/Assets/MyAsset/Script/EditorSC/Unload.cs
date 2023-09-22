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
    [ContextMenu("���g�p�I�u�W�F�N�g���A�����[�h")]
    public async UniTaskVoid UnLoadMethod()
    {
        Debug.Log("�A�����[�h");
        await Resources.UnloadUnusedAssets();
    }


    [ContextMenu("�ǂݍ��܂�Ă�V�[�����m�F")]
    public void CheckScene()
    {
        //���ݓǂݍ��܂�Ă���V�[�����������[�v
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {

            //�ǂݍ��܂�Ă���V�[�����擾���A���̖��O�����O�ɕ\��
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).name;
            Debug.Log(sceneName);

        }
    }


    [ContextMenu("scene���̃C�x���g�I�u�W�F�N�g��ID�t�^")]
    public void SetID()
    {
        EventObject[] objs = FindObjectsOfType<EventObject>();

        int count = objs.Length;

        int i = 0;

        //���ݓǂݍ��܂�Ă���V�[�����������[�v
        foreach(EventObject obj in objs)
        {
            if (obj.BreakableCheck())
            {
                obj.SetID(i);
                i++;
            }
        }

        Debug.Log($"{i}�̔j��\�I�u�W�F�N�g");
    }

#endif

}
