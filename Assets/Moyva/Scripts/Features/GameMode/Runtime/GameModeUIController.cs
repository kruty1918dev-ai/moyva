using System;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    /// <summary>
    /// Керує UI кнопками для переключення між режимами гри.
    /// Слухає GameModeChangedSignal та показує/ховає кнопки входу/виходу залежно від режиму.
    /// 
    /// ФУНКЦІОНАЛЬНІСТЬ:
    /// — Показує кнопку "Будівництво" коли режим = Normal.
    /// — Показує кнопку "Вихід" коли режим = Construction.
    /// — Видає GameModeChangeRequestedSignal при кліку на кнопку.
    /// 
    /// ЯК ПІДКЛЮЧИТИ В UNITY:
    /// 1. Додай компонент до GameObject кореневого UI (обов'язково у сцені).
    /// 2. Призначи enterConstructionButton та exitConstructionButton в Inspector.
    /// 3. Додай GameModeInstaller до SceneContext.
    /// </summary>
    public class GameModeUIController : MonoBehaviour, IInitializable, IDisposable
    {
        [Header("Кнопки переключення режимів (перетягни в Inspector)")]
        [Tooltip("Кнопка входу в режим будівництва. Видима коли режим = Normal.")]
        [SerializeField] private GameObject enterConstructionButton;

        [Tooltip("Кнопка виходу з режиму будівництва. Видима коли режим = Construction.")]
        [SerializeField] private GameObject exitConstructionButton;

        // --- Інжектується Zenject ---
        private SignalBus _signalBus;

        // --- Внутрішній стан ---
        private GameModeType _currentMode = GameModeType.Normal;

        /// <summary>Точка ін'єкції Zenject. Не викликати вручну.</summary>
        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        /// <summary>Викликається Zenject після ін'єкції. Підписується на сигнали.</summary>
        public void Initialize()
        {
            if (_signalBus == null)
            {
                Debug.LogError("[GameModeUIController] Zenject не інʼєктував SignalBus. Перевір SceneContext installers для Signals та GameMode.", this);
                return;
            }

            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);

            // Підключаємо кнопки до кліків
            if (enterConstructionButton != null)
            {
                var button = enterConstructionButton.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                    button.onClick.AddListener(OnEnterConstructionClicked);
                else
                    Debug.LogWarning("[GameModeUIController] Кнопка 'enterConstructionButton' не має компонента Button.", this);
            }
            else
            {
                Debug.LogWarning("[GameModeUIController] Кнопка 'enterConstructionButton' не призначена.", this);
            }

            if (exitConstructionButton != null)
            {
                var button = exitConstructionButton.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                    button.onClick.AddListener(OnExitConstructionClicked);
                else
                    Debug.LogWarning("[GameModeUIController] Кнопка 'exitConstructionButton' не має компонента Button.", this);
            }
            else
            {
                Debug.LogWarning("[GameModeUIController] Кнопка 'exitConstructionButton' не призначена.", this);
            }

            // Ініціалізуємо видимість кнопок за поточним режимом
            RefreshButtonVisibility();
        }

        /// <summary>Викликається Zenject при знищенні. Відписується від сигналів.</summary>
        public void Dispose()
        {
            if (_signalBus != null)
                _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);

            if (enterConstructionButton != null)
            {
                var button = enterConstructionButton.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                    button.onClick.RemoveListener(OnEnterConstructionClicked);
            }

            if (exitConstructionButton != null)
            {
                var button = exitConstructionButton.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                    button.onClick.RemoveListener(OnExitConstructionClicked);
            }
        }

        // -----------------------------------------------------------------------
        // Обробники кліків на кнопки
        // -----------------------------------------------------------------------

        private void OnEnterConstructionClicked()
        {
            RequestModeChange(GameModeType.Construction);
        }

        private void OnExitConstructionClicked()
        {
            RequestModeChange(GameModeType.Normal);
        }

        // -----------------------------------------------------------------------
        // Обробники сигналів
        // -----------------------------------------------------------------------

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _currentMode = signal.NewMode;
            RefreshButtonVisibility();
        }

        // -----------------------------------------------------------------------
        // Допоміжні методи
        // -----------------------------------------------------------------------

        private void RefreshButtonVisibility()
        {
            bool isNormalMode = _currentMode == GameModeType.Normal;

            if (enterConstructionButton != null)
                enterConstructionButton.SetActive(isNormalMode);

            if (exitConstructionButton != null)
                exitConstructionButton.SetActive(!isNormalMode);
        }

        private void RequestModeChange(GameModeType requestedMode)
        {
            if (_signalBus == null)
                return;

            _signalBus.Fire(new GameModeChangeRequestedSignal { RequestedMode = requestedMode });
        }
    }
}
