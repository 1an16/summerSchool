using UnityEngine;

public class RotateShootReturn2D : MonoBehaviour
{
    public float rotateSpeed = 120f;
    public float moveSpeed = 8f;
    public float moveDistance = 5f;
    public float returnSpeed = 10f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private bool rotating = true;
    private bool moving = false;
    private bool returning = false;

    private float currentAngle = 0f;
    private bool clockwise = true;


    void Start()
    {
        startPosition = transform.position;
    }


    void Update()
    {
        // 旋转阶段
        if (rotating)
        {
            RotateFront();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartMove();
            }
        }


        // 向尖头方向移动
        if (moving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );


            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                StartReturn();
            }
        }


        // 返回
        if (returning)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                startPosition,
                returnSpeed * Time.deltaTime
            );


            if (Vector3.Distance(transform.position, startPosition) < 0.05f)
            {
                transform.position = startPosition;

                returning = false;
                rotating = true;
            }
        }
    }



    // 前方180度旋转
    void RotateFront()
    {
        if (clockwise)
            currentAngle += rotateSpeed * Time.deltaTime;
        else
            currentAngle -= rotateSpeed * Time.deltaTime;


        if (currentAngle >= 90f)
        {
            currentAngle = 90f;
            clockwise = false;
        }


        if (currentAngle <= -90f)
        {
            currentAngle = -90f;
            clockwise = true;
        }


        transform.localRotation =
            Quaternion.Euler(0, 0, currentAngle);
    }



    // 开始移动
    void StartMove()
    {
        rotating = false;
        moving = true;


        targetPosition =
            transform.position +
            transform.up * moveDistance;
    }



    // 开始返回
    void StartReturn()
    {
        moving = false;
        returning = true;
    }



    // ==========================
    // 碰撞检测
    // ==========================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (moving && collision.CompareTag("Destory"))
        {
            // 销毁碰到的目标
            Destroy(collision.gameObject);

            // 立刻回到起点
            transform.position = startPosition;

            // 重置状态
            moving = false;
            returning = false;
            rotating = true;
        }
    }
}