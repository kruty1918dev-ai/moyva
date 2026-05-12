using UnityEngine;
using Zenject;
using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Тестовий скрипт для перевірки роботи WorldInfoPanel.
    /// Додай цей скрипт на GameObject зі SceneContext.
    /// 
    /// Натиснення клавіш:
    /// - Space: Відкрити панель з тестовими даними
    /// - ESC: Закрити панель
    /// 
    /// Це допоможе перевірити що система встановлена правильно.
    /// </summary>
    public sealed class InfoPanelTestHelper : MonoBehaviour
    {
        private SignalBus _signalBus;

        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TestOpenPanel();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TestClosePanel();
            }
        }

        private void TestOpenPanel()
        {
            var signal = new WorldInfoPanelRequestedSignal
            {
                Title = "Test Building",
                Subtitle = "Tavern",
                Content = "Wood: 100\nStone: 50\nGold: 25"
            };

            _signalBus.Fire(signal);
            Debug.Log("[InfoPanelTestHelper] ✓ Панель відкрита (Space для закриття)");
        }

        private void TestClosePanel()
        {
            _signalBus.Fire(new WorldInfoPanelClosedSignal());
            Debug.Log("[InfoPanelTestHelper] ✓ Панель закрита (Space для відкриття)");
        }
    }
}
