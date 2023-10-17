using Cysharp.Threading.Tasks;
using UnityEngine;

public class EndTrigger : MonoBehaviour
{


    float posi;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            posi = transform.position.y;
            EndWait().Forget();
        }
    }

    async UniTaskVoid EndWait()
    {
        await UniTask.WaitUntil(() => ScoreManager.instance.PlayerPosi.y > posi, cancellationToken: destroyCancellationToken);
        ScoreManager.instance.isGoal = true;
    }

}
