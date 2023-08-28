using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length, startpos;
    public Transform player;
    public float parallaxEffect;

    public GameObject subback;
    public Transform assistBack;
    public float multi;

    Vector3 posi;



    Transform controller;
    Transform nowMain;

    bool isUp;
    bool isDown;

    void Start()
    {
        // ”wŒi‰æ‘œ‚ÌxÀ•W
        startpos = transform.position.y;
        // ”wŒi‰æ‘œ‚Ìx²•ûŒü‚Ì•
        length = GetComponent<SpriteRenderer>().bounds.size.y;
        controller = subback.transform;
        nowMain = this.gameObject.transform;
    }

    private void FixedUpdate()
    {


        //‚Ü‚¾·‚ª‰æ‘œ‚Ì’·‚³“à‚È‚ç
        if (Mathf.Abs(player.position.y - nowMain.position.y) < length*multi)
        {
            if (player.position.y > nowMain.position.y && !isUp)
            {
                posi.Set(nowMain.position.x,nowMain.position.y + length,nowMain.position.z);
                controller.position = posi;
                posi.Set(nowMain.position.x, nowMain.position.y + length * 2, nowMain.position.z);
                assistBack.transform.position = posi;
                isUp = true;
                isDown = false;
            }
            if (player.position.y < nowMain.position.y && !isDown)
            {
                posi.Set(nowMain.position.x, nowMain.position.y - length, nowMain.position.z);
                controller.position = posi;
                posi.Set(nowMain.position.x, nowMain.position.y - length * 2, nowMain.position.z);
                assistBack.transform.position = posi;
                isUp = false;
                isDown = true;
            }

        }
        //‹——£‚ª‰ß‚¬‚½‚ç“ü‚ê‘Ö‚¦
        else
        {
            isUp = false;
            isDown = false;
            Transform save = nowMain;

            nowMain = controller;
            controller = save;

        }
    }

}
