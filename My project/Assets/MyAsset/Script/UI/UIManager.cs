using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public GameObject MenuWindow;
    public GameObject SaveWindow;


    public Button QuitButton;

    public Button SaveButton;



    bool isMenu;

    bool UIButton;

    float UIVertical;

    [SerializeField]
    SaveManager _save;




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        UIButton = Input.GetButtonDown("Menu");

        if(isMenu)
        {
            UIVertical = Input.GetAxisRaw("Vertical");

            if(UIButton)
            {
                MenuWindow.SetActive(false);
                Time.timeScale = 1;
                isMenu = false;
            }

            if (SaveWindow.activeSelf)
            {
                if (Input.GetButton("Cancel"))
                {
                    SaveWindowCancel();
                }
            }
        }
        else if (UIButton)
        {

            MenuWindow.SetActive(true);
            Time.timeScale = 0;
            isMenu = true;
            QuitButton.Select();
        }
    }

    /// <summary>
    /// ÉÅÉjÉÖÅ[ï¬Ç∂ÇÈ
    /// </summary>
    public void MenuEnd()
    {
        MenuWindow.SetActive(false);
        Time.timeScale = 1;
        isMenu = false;
    }

    public void SaveWindowCall()
    {
        SaveWindow.SetActive(true);
        SaveButton.Select();
    }

    public void SaveWindowCancel()
    {
        SaveWindow.SetActive(false);
        QuitButton.Select();
    }

    public async void SaveAct()
    {
        await _save.SaveAsync();

        bl_SceneLoaderManager.LoadScene("Title");
    }




}
