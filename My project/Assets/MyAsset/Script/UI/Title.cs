using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CharacterController;


//�Z�[�u�f�[�^�m�F����
public class Title : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI tx;

    private void Start()
    {
        int num = ES3.Load<int>("BestScore", 0);
        Debug.Log($"������{num}");
        tx.text = num.ToString();
    }


    public void StartGame()
    {
        //�Z�[�u�f�[�^�Ȃ��Ȃ�쐬
        if (!ES3.FileExists("SaveFile.es3"))
        {

            LevelManager.instance.NewSave();
        //    await UniTask.WaitUntil(()=> ES3.KeyExists("MapImfo"));
        }
        bl_SceneLoaderManager.LoadScene("MainScene");
    }

    public void DeleteData()
    {
        ES3.DeleteFile("SaveFile.es3");
    }


}
