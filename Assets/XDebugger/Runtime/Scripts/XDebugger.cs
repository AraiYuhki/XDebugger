using UnityEngine;

namespace Xeon.XDebugger
{
    public class XDebugger : MonoBehaviour
    {
        private static readonly int OpenId = Animator.StringToHash("Open");
        private static readonly int CloseId = Animator.StringToHash("Close");
        private static XDebugger instance;

        public static XDebugger Instance => instance;

        [SerializeField]
        private GameObject mainMenu;
        [SerializeField]
        private Animator animator;
        private bool isShow = false;

        [Header("Trigger")]
        [SerializeField]
        private int clickCount = 3;
        [SerializeField]
        private readonly float inputGraceTime = 0.2f;

        private float elapsedTime = 0f;
        private int clickedCount = 0;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            mainMenu.SetActive(false);
            isShow = false;

            DontDestroyOnLoad(gameObject);
        }

        public void Show()
        {
            if (isShow)
                return;
            
            isShow = true;
            mainMenu.SetActive(true);
            animator.Play(OpenId);
        }

        public void Hide()
        {
            if (!isShow)
                return;

            isShow = false;
            animator.Play(CloseId);
        }

        public void OnHiden()
        {
            isShow = false;
            mainMenu.SetActive(false);
        }

        public void OnClickTrigger()
        {
            elapsedTime = 0f;
            clickedCount++;
            if (clickedCount >= clickCount)
            {
                Show();
                clickedCount = 0;
                return;
            }
        }

        private void Update()
        {
            if (elapsedTime > inputGraceTime)
                clickedCount = 0;
            else
                elapsedTime += Time.deltaTime;
        }
    }
}
