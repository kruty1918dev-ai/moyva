using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Реалізація сервісу модального підтвердження.
    /// Залежності:
    /// - делегує показ у <see cref="IConfiremationPanel"/>;
    /// - підписується на <see cref="IConfirmationButton"/> для зовнішнього відкриття модалки.
    /// </summary>
    internal sealed class ConformationService : IConfirmationService, IInitializable
    {
        /// <summary>Панель підтвердження, яка фізично відображає UI.</summary>
        [Inject] private IConfiremationPanel _panel;

        /// <summary>Кнопки, які можуть ініціювати показ модалки підтвердження.</summary>
        [Inject] private IConfirmationButton[] _buttons;

        /// <summary>Підписати сервіс на події панелі та кнопок підтвердження.</summary>
        public void Initialize()
        {
            // 1: При підтвердженні автоматично ховаємо модальне вікно.
            GetPanel().OnConfirme += ForeceHide;

            // 2: При скасуванні також ховаємо модальне вікно для консистентного UX.
            GetPanel().OnCancled += ForeceHide;

            // 3: Кожну кнопку-конектор прив'язуємо до методу Show сервісу.
            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].OnClicked += Show;
            }
        }


        /// <summary>Примусово приховати активну панель підтвердження.</summary>
        public void ForeceHide() => GetPanel().ForeceHide();

        /// <summary>Показати модальне вікно підтвердження для переданого запиту.</summary>
        public void Show(ConfirmationRequest request) => GetPanel().Show(request);

        /// <summary>Спробувати отримати поточний активний запит підтвердження.</summary>
        public bool TryGetReqest(out ConfirmationRequest? request) => GetPanel().TryGetReqest(out request);


        /// <summary>Отримати панель підтвердження або залогувати помилку ін'єкції.</summary>
        private IConfiremationPanel GetPanel()
        {
            // 1: Якщо панель не була заінжекчена, логумо помилку і повертаємо null для явної деградації.
            if (_panel == null)
            {
                Debug.LogError("[ConformationService]: The confirmed panel was not injected.");
                return null;
            }

            // 2: Повертаємо валідну панель для подальшої роботи сервісу.
            return _panel;
        }

        /// <summary>Отримати масив кнопок підтвердження або залогувати помилку ін'єкції.</summary>
        private IConfirmationButton[] GetButtons()
        {
            // 1: Перевіряємо, чи були передані кнопки-джерела запитів підтвердження.
            if (_buttons == null)
            {
                Debug.LogError("[ConformationService]: The confirmed buttons was not injected");
                return null;
            }

            // 2: Повертаємо масив кнопок для потенційного використання поза Initialize.
            return _buttons;
        }
    }
}