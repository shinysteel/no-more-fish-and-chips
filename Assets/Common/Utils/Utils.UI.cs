using NoMoreFishAndChips.UI;
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
                // Invoking onClick at the end was more trouble than it was worth, so we're doing it at the start instead
                button.onClick?.Invoke();

                ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);

                int delay = 100; // ms
                await Task.Delay(delay);
                
                ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);
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