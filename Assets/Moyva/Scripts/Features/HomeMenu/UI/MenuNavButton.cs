using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Утилітарний компонент, що прив'язує кнопку до однієї з операцій
    /// <see cref="HomeMenuNavigationController"/>. Вибирається через Inspector —
    /// дозволяє зробити повну навігацію без написання коду.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class MenuNavButton : MonoBehaviour
    {
        public enum ActionKind
        {
            /// <summary>Відкрити підпанель як новий рівень у стеку.</summary>
            Push,
            /// <summary>Замінити поточну підпанель на нову (без додавання в стек).</summary>
            Replace,
            /// <summary>Повернутись на один рівень назад.</summary>
            Back,
            /// <summary>Закрити всі відкриті панелі (правий бік екрану стає порожнім).</summary>
            CloseAll,
            /// <summary>Запустити ігрову сцену через <see cref="HomeMenuNavigationController.LaunchGameplay"/>.</summary>
            LaunchGameplay
        }

        [SerializeField] private HomeMenuNavigationController navigation;
        [SerializeField] private ActionKind action = ActionKind.Push;
        [Tooltip("Ціль для Push/Replace — підпанель, яку треба відкрити.")]
        [SerializeField] private GameObject target;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null) _button.onClick.AddListener(Execute);
        }

        private void OnDestroy()
        {
            if (_button != null) _button.onClick.RemoveListener(Execute);
        }

        private void Execute()
        {
            if (navigation == null)
            {
                Debug.LogWarning("[MenuNavButton] navigation не призначено.", this);
                return;
            }

            switch (action)
            {
                case ActionKind.Push:           navigation.Push(target);           break;
                case ActionKind.Replace:        navigation.Replace(target);        break;
                case ActionKind.Back:           navigation.Back();                 break;
                case ActionKind.CloseAll:       navigation.CloseAll();             break;
                case ActionKind.LaunchGameplay: navigation.LaunchGameplay();       break;
            }
        }
    }
}
