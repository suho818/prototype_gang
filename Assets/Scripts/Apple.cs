
using UnityEngine;
using System.Collections;



public class Apple : MonoBehaviour
{
    public int number; // 사과의 숫자 정보
    public int id; // 사과의 고유 번호
    public int remainingApple;
    public int[] coordinate = new int[2]; // 사과의 현재 grid상 좌표

    private Coroutine fallCoroutine;
    public float fallSpeed = 4f; // 한 칸 내려오는 속도

    public float launchForce = 300f; // 위로 발사하는 힘
    public float horizontalOffset = 0.5f; // 좌우로 튈 수 있는 최대 거리
    private Rigidbody2D rb;

    public bool isSelected = false;
    public GameObject selectedEffect;

    void Start()
    {
        selectedEffect = transform.Find("apple_image").Find("as_img").gameObject;
        rb = gameObject.AddComponent<Rigidbody2D>(); // Rigidbody2D 컴포넌트 추가
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0; // 중력 스케일 설정

    }
    // 아래로 이동하는 코루틴 시작
    public void StartFalling(Vector3 targetPosition)
    {
        if (fallCoroutine != null)
        {
            StopCoroutine(fallCoroutine); // 이미 실행 중인 코루틴이 있다면 중지
        }
        fallCoroutine = StartCoroutine(FallToPosition(targetPosition));
    }

    // 서서히 아래로 이동하는 코루틴
    private IEnumerator FallToPosition(Vector3 targetPosition)
    {
        
        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        fallCoroutine = null; // 이동이 완료되면 코루틴을 해제
    }

    public void ShootAndDestroy()
    {
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Dynamic;
        Destroy(gameObject.GetComponent<CircleCollider2D>());
        transform.position += new Vector3(0, 0, -1); // Z축 값을 낮춰 위에 표시되도록 설정
        // 위로 발사할 때 약간의 좌우 랜덤 오프셋 추가
        float randomHorizontalForce = Random.Range(-horizontalOffset, horizontalOffset) * launchForce;
        Vector2 launchDirection = new Vector2(randomHorizontalForce, launchForce);
        rb.AddForce(launchDirection);
        rb.gravityScale = 3;
        // 일정 시간 후 사과를 삭제
        Destroy(gameObject, 2f); // 2초 후에 삭제
    }

    public void SetSelected()
    {
        if (selectedEffect != null)
        {
            selectedEffect.SetActive(isSelected); // 선택 여부에 따라 광선 활성화
        }
    }
}