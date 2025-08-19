using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xeon.XDebugger.Model;

namespace Xeon.XDebugger
{
    public class XDebugger : MonoBehaviour
    {
        private static readonly int OpenId = Animator.StringToHash("Open");
        private static readonly int CloseId = Animator.StringToHash("Close");
        private static XDebugger instance;

        public static XDebugger Instance => instance;
        private static PageModel initialPage;

        [SerializeField]
        private GameObject mainMenu;
        [SerializeField]
        private TMP_Text titleLabel;
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private Button backButton;
        
        private bool isShow = false;

        [Header("Trigger")]
        [SerializeField]
        private int clickCount = 3;
        [SerializeField]
        private Transform content;
        [SerializeField]
        private readonly float inputGraceTime = 0.2f;

        private float elapsedTime = 0f;
        private int clickedCount = 0;
        private PageModel currentPage;

        private List<PageModel> pageStack = new ();


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

        public static PageModel GetOrCreateInitialPage()
        {
            initialPage ??= new PageModel("Initial Page");
            return initialPage;
        }

        public void Show()
        {
            if (isShow)
                return;
            
            backButton.gameObject.SetActive(false);
            isShow = true;
            mainMenu.SetActive(true);
            animator.Play(OpenId);
            if (currentPage == null)
                OpenPage(GetOrCreateInitialPage());
            else
                currentPage.Refresh();
        }

        public void Hide()
        {
            if (!isShow)
                return;

            isShow = false;
            animator.Play(CloseId);
        }

        public void Back()
        {
            if (pageStack.Count > 1)
                ClosePage(currentPage);
            backButton.gameObject.SetActive(pageStack.Count > 1);
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
            }
        }

        private void Update()
        {
            if (elapsedTime > inputGraceTime)
                clickedCount = 0;
            else
                elapsedTime += Time.deltaTime;
        }

        public void OpenPage<T>(T model = null) where T : PageModel, new()
        {
            if (currentPage != null)
            {
                currentPage.Hide(() => CreatePage(model));
                return;
            }
            CreatePage(model);
        }

        private void CreatePage<T>(T model) where T : PageModel, new()
        {
            model ??= new T();
            model.Initialize();
            model.OpenPage(content);
            pageStack.Add(model);
            currentPage = model;
            titleLabel.text = currentPage.Title;
            backButton.gameObject.SetActive(pageStack.Count > 1);
        }

        public void ClosePage(PageModel target)
        {
            pageStack.Remove(target);
            if (currentPage != target)
                return;
            currentPage = null;
            target.Close(() =>
            {
                currentPage = pageStack.LastOrDefault();
                currentPage.Show(true);
                titleLabel.text = currentPage.Title;
            });
            backButton.gameObject.SetActive(pageStack.Count > 1);
        }
    }
}
