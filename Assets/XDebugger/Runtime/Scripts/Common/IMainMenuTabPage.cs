using System;
using Xeon.XDebugger.Model;
using Xeon.XDebugger.UI;

namespace Xeon.XDebugger.Control
{
    public interface IMainMenuTabPage
    {
        event Action<PageModel> OnPageChanged;

        string Title { get; }

        void Initialize();

        void OpenPage<T>(T model = null) where T : PageModel, new();

        void RefreshCurrentPage(bool doRecreate);

        PageModel GetCurrentPage();

        void ClosePage(PageModel target);

        void ClearPageStack();
    }
}
