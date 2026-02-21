using FishFlingers.UI;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ShinyOwl.Common.Utils
{
    public static partial class Utils
    {
        public static class UI
        {
            public static void SimulatePressed(Button button)
            {
                _ = SimulatePressedAsync(button);
            }

            private static async Task SimulatePressedAsync(Button button)
            {
                ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);

                int delay = 100; // ms
                await Task.Delay(delay);

                ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);

                button.onClick?.Invoke();
            }

            public static void StretchToParent(RectTransform rect)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }
    }
}