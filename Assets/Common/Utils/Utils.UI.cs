using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

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
        }
    }
}