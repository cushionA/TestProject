using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyScroll : MonoBehaviour
{

    Material skyMat;

    [SerializeField]
    Transform Player;

    Rigidbody2D rb;
    Vector2 posi;

  //  [SerializeField]
    float rate;

    float fallTime;

    bool falling;

    // Start is called before the first frame update
    void Start()
    {
        skyMat = GetComponent<Renderer>().material;
        rb = Player.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //    posi.Set(0,Player.position.y);
        //    transform.position = posi;

        float speed = rb.velocity.y;

        if(speed < 0 && fallTime == 0)
        {
            fallTime = Time.time;
        }

        if(!falling)
        {
            //rate = speed / 200;
            skyMat.SetFloat("_TextureScrollYSpeed", 0.3f * Mathf.Sign(speed)) ;// * rate);

            if(speed < 0 && Time.time - fallTime > 1.4)
            {
                falling = true;
            }
        }
        else
        {

            skyMat.SetFloat("_TextureScrollYSpeed", (Mathf.SmoothDamp(0, -2,ref rate,1) + -0.3f));
        }

        // skyMat.SetFloat("_TextureScrollYSpeed",(rb.velocity.y / rate));

        if (speed > 0)
        {
            rate = 0;
            falling = false;
            fallTime = 0;
        }
    }
}
