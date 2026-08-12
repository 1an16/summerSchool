using UnityEngine;

public class DestoryObject : MonoBehaviour
{
    public int maxHit = 3;

    public float hitCooldown = 0.5f;

    private int hitCount = 0;

    private float timer = 0;


    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }


    public void TakeHit()
    {
        // 冷却期间不计数
        if (timer > 0)
            return;


        hitCount++;

        Debug.Log(
            gameObject.name +
            " 被撞击 " + hitCount + "/" + maxHit
        );


        timer = hitCooldown;


        if (hitCount >= maxHit)
        {
            Destroy(gameObject);
        }
    }
}