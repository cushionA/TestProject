using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThroughWall : MonoBehaviour
{
    [SerializeField]
    BoxCollider2D col;
    float posi;

    bool isEnd;

    private void Start()
    {
        posi = transform.position.y;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isEnd)
        {
            return;
        }

        if(collision.tag == "Player" && ScoreManager.instance.PlayerPosi.y > posi)
        {
            col.enabled = true;
            isEnd = true;
        }
    }

}
