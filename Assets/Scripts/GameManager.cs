using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{
    public GameObject[] applePrefabs; // apple1 ~ apple9 프리팹을 담을 배열
    public GameObject appleSelected;
    
    private int[,] grid = new int[17, 9];
    private int[] verticalAppleNum = new int[17];
    private int[] verticalRemovedAppleNum = new int[17];
    private List<GameObject> appleObjects = new List<GameObject>();
    private List<IEnumerator> appleCoroutine = new List<IEnumerator>();
    public float fallSpeed = 0.5f; // 사과가 한 칸 내려오는데 걸리는 시간

    public int gameMethod = 1; // 0 = 드래그 , 1 = 터치
    public int touchNum = 0;

    private float width;
    private float height;

    public RectTransform selectionBox; // 드래그 영역 UI 요소
    private Vector2 startPosition;
    private Vector2 endPosition;
    private List<GameObject> selectedApples = new List<GameObject>();

    public TextMeshProUGUI scoreText;
    private int score = 0;
    public TextMeshProUGUI timeText;
    private float time = 0f;

    public int gameMode = 0; //0: 기본모드, 1: 무한모드



    void Start()
    {
        Vector3 screenBottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        Vector3 screenTopRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.nearClipPlane));

        width = screenTopRight.x - screenBottomLeft.x;
        height = screenTopRight.y - screenBottomLeft.y;

        InitializeGrid();
        UpdateScoreUI();
    }

    public void RestartGame()
    {
       
        foreach (var apple in appleObjects)
        {
            Destroy(apple);
        }
        InitializeGrid();
        score = 0;
        UpdateScoreUI();
    }

    void InitializeGrid()
    {
        verticalAppleNum = new int[17];
        verticalRemovedAppleNum = new int[17];

        for (int x = 0; x < 17; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                grid[x, y] = UnityEngine.Random.Range(1, 10); // 1~9 랜덤 숫자 배정
                verticalAppleNum[x] += 1;
                verticalRemovedAppleNum[x] += 1;
            }
        }
        DisplayGrid();
    }

    void DisplayGrid()
    {
        appleObjects.Clear();
        for (int x = 0; x < 17; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                int number = grid[x, y] - 1; // 1~9를 0~8 인덱스로 맞춤
                GameObject apple = Instantiate(applePrefabs[number], new Vector3(x-17/2, y-9/2, 0), Quaternion.identity);
                apple.name = $"Apple_{x}_{y}"; // 오브젝트 이름 설정

                GameObject as_img = Instantiate(appleSelected, new Vector3(0,0,0), Quaternion.identity);
                as_img.transform.SetParent(apple.transform.Find("apple_image"), false);
                as_img.gameObject.SetActive(false);
                as_img.name = "as_img";

                appleObjects.Add(apple);
                // Apple 스크립트를 추가하고 숫자 설정
                Apple appleComponent = apple.GetComponent<Apple>();
                if (appleComponent == null)
                {
                    appleComponent = apple.AddComponent<Apple>();
                }
                appleComponent.id = y * 17 + x;
                appleComponent.number = number + 1; // 실제 숫자를 설정
                appleComponent.coordinate = new int[] {x, y};
                appleComponent.remainingApple = y;
                
                
            }
        }
    }
   
   
        void Update()
        {
        // R 키를 누르면 재시작
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }

        if (IsTouchInput())
            {
                HandleTouchInput();
            
            }
            else
            {
           //     HandleMouseInput();
           
        }
        }

        // 터치 입력 확인
        bool IsTouchInput()
        {
            return Input.touchCount > 0;
        }

        // 터치 입력 처리
        void HandleTouchInput()
        {
            Touch touch = Input.GetTouch(0); // 첫 번째 터치 입력

        if (gameMethod == 0)
        {
            Debug.Log("터치0");
            switch (touch.phase)
            {
                case TouchPhase.Began: // 터치 시작
                    selectedApples.Clear();
                    startPosition = touch.position;
                    endPosition = touch.position;
                    UpdateSelectionBox();
                    selectionBox.gameObject.SetActive(true);
                    break;
                case TouchPhase.Stationary: // 터치 지속중
                    UpdateSelectionBox();
                    break;


                case TouchPhase.Moved: // 터치 이동 중
                    endPosition = touch.position;
                    UpdateSelectionBox();
                    break;

                case TouchPhase.Ended: // 터치 종료
                    SelectApplesInArea();
                    selectionBox.gameObject.SetActive(false);
                    CheckAndRemoveApples();
                    break;
            }
        }
        else if (gameMethod == 1)
                {
            Debug.Log("터치1");

            if (touchNum == 0)
            {
                switch (touch.phase)
                {
                    case TouchPhase.Began: // 터치 시작
                        selectedApples.Clear();
                        startPosition = touch.position;
                        endPosition = touch.position;
                        UpdateSelectionBox();
                        selectionBox.gameObject.SetActive(true);
                        break;

                    }
                touchNum = 1;
            }
            else
            {if (touchNum == 1)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        endPosition = touch.position;
                        UpdateSelectionBox();
                        selectionBox.gameObject.SetActive(true);
                        touchNum = 2;

                    }
                }
            else if (touchNum == 2)
                {
                    if (touch.phase == TouchPhase.Ended)
                    {
                        UpdateSelectionBox();
                        SelectApplesInArea();

                        selectionBox.gameObject.SetActive(false);
                        CheckAndRemoveApples();
                        touchNum = 0;
                    }
                }
             
               
            }
        }
        }

        // 마우스 입력 처리
        void HandleMouseInput()
        {
            // 드래그 시작
            if (Input.GetMouseButtonDown(0))
            {
                selectedApples.Clear();
                startPosition = Input.mousePosition;
                selectionBox.gameObject.SetActive(true);
            }

            // 드래그 중
            if (Input.GetMouseButton(0))
            {
                endPosition = Input.mousePosition;
                UpdateSelectionBox();
            }

            // 드래그 종료
            if (Input.GetMouseButtonUp(0))
            {
                SelectApplesInArea();
                selectionBox.gameObject.SetActive(false);
                CheckAndRemoveApples();
            }
        }

        // 드래그 직사각형 영역을 갱신
        void UpdateSelectionBox()
    {
        Vector2 boxStart = startPosition;
        Vector2 boxSize = endPosition - startPosition;

        selectionBox.anchoredPosition = boxStart + boxSize / 2;
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(boxSize.x), Mathf.Abs(boxSize.y));

        Vector2 min = Camera.main.ScreenToWorldPoint(Vector3.Min(startPosition, endPosition));
        Vector2 max = Camera.main.ScreenToWorldPoint(Vector3.Max(startPosition, endPosition));


        foreach(var apple in appleObjects)
        {
            apple.GetComponent<Apple>().isSelected = false;
            apple.GetComponent<Apple>().SetSelected();

        }

        Collider2D[] colliders = Physics2D.OverlapAreaAll(min, max);

        foreach (var collider in colliders)
        {
            if (collider.gameObject.CompareTag("Apple"))
            {
                collider.gameObject.GetComponent<Apple>().isSelected=true;
                collider.gameObject.GetComponent<Apple>().SetSelected();
            }
        }
    }

    // 영역 내 사과를 선택
    void SelectApplesInArea()
    {
        foreach (var apple in appleObjects)
        {
            apple.GetComponent<Apple>().isSelected = false;
            apple.GetComponent<Apple>().SetSelected();

        }

        Vector2 min = Camera.main.ScreenToWorldPoint(Vector3.Min(startPosition, endPosition));
        Vector2 max = Camera.main.ScreenToWorldPoint(Vector3.Max(startPosition, endPosition));
       

        Collider2D[] colliders = Physics2D.OverlapAreaAll(min, max);
        
        foreach (var collider in colliders)
        {
            if (collider.gameObject.CompareTag("Apple"))
            {
                selectedApples.Add(collider.gameObject);
            }
        }
    }

    // 선택된 사과들의 합이 10인지 확인 후 제거
    void CheckAndRemoveApples()
    {
        int sum = 0;
        foreach (var apple in selectedApples)
        {

            Apple appleComponent = apple.GetComponent<Apple>();
            
            sum += appleComponent.number; // Apple 스크립트에 숫자 정보가 있다고 가정
        }

        if (sum == 10)
        {
            foreach (var apple in selectedApples)
            {
                
                Vector2 pos = apple.transform.position;
                grid[(int)pos.x + 8, (int)pos.y + 4] = 0;// 사과 제거 시 0으로 표시
                Apple AP = apple.GetComponent<Apple>();
                AP.ShootAndDestroy();
                appleObjects.Remove(apple);
                foreach (var ap in appleObjects.FindAll(a => a.GetComponent<Apple>().coordinate[0] == AP.coordinate[0] && a.GetComponent<Apple>().coordinate[1] > AP.coordinate[1]))
                {
                    ap.GetComponent<Apple>().remainingApple -= 1;
                }
                verticalRemovedAppleNum[AP.coordinate[0]] -= 1;
               
                CreateApple(AP.coordinate[0]);
          
                
                for (int id_num = AP.id; id_num < 17 * verticalAppleNum[AP.coordinate[0]]; id_num += 17)
                {
                    ApplyGravity(id_num);
                    PrintGrid();
                }

                
            }
            score += selectedApples.Count;
            UpdateScoreUI();
        }
    }

        void ApplyGravity(int id)
        {
            GameObject apple = FindAppleAtId(id);
            if (apple != null)
            {
                Vector3 targetPosition = new Vector3(apple.transform.position.x, apple.GetComponent<Apple>().remainingApple - 4, 0);
                apple.GetComponent<Apple>().StartFalling(targetPosition);
            }

        }
        void PrintGrid()
        {
            string gridOutput = "";

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 17; x++)
                {
                    //gridOutput += gridAppleRemaining[x, y] + "\t"; // 각 요소 사이에 탭으로 구분
                }
                gridOutput += "\n"; // 한 행이 끝날 때 줄 바꿈
            }
}

        GameObject FindAppleAtCoordinate(int x, int y)
        {
            return appleObjects.Find(a => a.GetComponent<Apple>().coordinate[0] == x && a.GetComponent<Apple>().coordinate[1] == y);
        }
        GameObject FindAppleAtId(int id)
        {
            return appleObjects.Find(a => a.GetComponent<Apple>().id == id);
        }
    private void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }

    
    void CreateApple(int x)
    {
        verticalAppleNum[x] += 1;
        verticalRemovedAppleNum[x] += 1;
        int y = verticalAppleNum[x] - 1;
        int number = UnityEngine.Random.Range(1, 10)-1;
        float Y = appleObjects.Where(apple => apple.GetComponent<Apple>().coordinate[0] == x)
            .OrderByDescending(apple => apple.GetComponent<Apple>().coordinate[1])
            .FirstOrDefault().transform.position.y+1;
        
        Y = Y > 5 ? Y : 5;
      
        GameObject apple = Instantiate(applePrefabs[number], new Vector3(x - 17 / 2, Y, 0), Quaternion.identity);
        apple.name = $"Apple_{x}_{y}"; // 오브젝트 이름 설정

        GameObject as_img = Instantiate(appleSelected, new Vector3(0, 0, 0), Quaternion.identity);
        as_img.transform.SetParent(apple.transform.Find("apple_image"), false);
        as_img.gameObject.SetActive(false);
        as_img.name = "as_img";

        appleObjects.Add(apple);
        // Apple 스크립트를 추가하고 숫자 설정
        Apple appleComponent = apple.GetComponent<Apple>();
        if (appleComponent == null)
        {
            appleComponent = apple.AddComponent<Apple>();
        }
        appleComponent.id = y * 17 + x;
        appleComponent.number = number + 1; // 실제 숫자를 설정
        appleComponent.coordinate = new int[] { x, y };
        appleComponent.remainingApple = verticalRemovedAppleNum[x] - 1;
    }

}