using System;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface INavigation
    {
        void Close(string menuName);
        void CloseForce(string menuName);
        Task CloseIf(string menuName, Func<Task<bool>> condition);
        void Open(string menuName);
        void OpenForce(string menuName);
        void OpenLast();
        void OpenLastForce();
        Task OpenIfAsync(string menuName, Func<Task<bool>> condition);

        void CloseLast();
        void CloseLastForce();

        string CurrentMenu { get; }

        event Action<NavigationChangeEventArgs> OnMenuChanged;
    }
}