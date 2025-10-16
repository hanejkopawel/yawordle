using UnityEngine.UIElements;

namespace Yawordle.Presentation.Views
{
    public static class UIUtils
    {
        public static void OpenModal(VisualElement overlay, VisualElement content, VisualElement panel) {
            if (overlay == null || content == null || panel == null) return;
            content.AddToClassList("is-active");
            content.BringToFront(); 
            overlay.AddToClassList("modal--is-open");
            panel.schedule.Execute(() => panel.AddToClassList("modal__panel--is-visible"));
        }

        public static void CloseModal(VisualElement overlay, VisualElement content, VisualElement panel, System.Action onClosed = null) {
            
            if (overlay == null || content == null || panel == null) {
                onClosed?.Invoke();
                return;
            }
            
            void OnEnd(TransitionEndEvent e) {
                if (e.target != panel) return;
                panel.UnregisterCallback<TransitionEndEvent>(OnEnd);
                overlay.RemoveFromClassList("modal--is-open");
                content.RemoveFromClassList("is-active"); 
                onClosed?.Invoke();
            }
            
            panel.RegisterCallback<TransitionEndEvent>(OnEnd);
            panel.RemoveFromClassList("modal__panel--is-visible");
        }
    }
}