using System;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Editor
{
    internal interface IUnitDesignerIdentityFacade
    {
        void DrawSection(SerializedProperty unit);
        void DrawDangerSection(SerializedProperty unit);
    }

    internal interface IUnitDesignerPrefabFacade
    {
        void DrawSection(SerializedProperty unit, string typeId);
        void DrawComponentsSection(SerializedProperty unit);
    }

    internal interface IUnitDesignerAnimationFacade
    {
        void DrawSection(SerializedProperty unit);
    }

    internal interface IUnitDesignerPreviewFacade
    {
        void DrawPanel(params GUILayoutOption[] options);
    }

    internal interface IUnitDesignerCombatFacade
    {
        void Initialize();
        void Dispose();
        bool IsWorkspaceActive();
        void OnWorkspaceSelected();
        void DrawWorkspace();
        void DrawCompactSection(SerializedProperty unit);
    }

    internal sealed class UnitDesignerIdentityFacade : IUnitDesignerIdentityFacade
    {
        private readonly Action<SerializedProperty> _drawSection;
        private readonly Action<SerializedProperty> _drawDangerSection;

        public UnitDesignerIdentityFacade(
            Action<SerializedProperty> drawSection,
            Action<SerializedProperty> drawDangerSection)
        {
            _drawSection = drawSection ?? throw new ArgumentNullException(nameof(drawSection));
            _drawDangerSection = drawDangerSection ?? throw new ArgumentNullException(nameof(drawDangerSection));
        }

        public void DrawSection(SerializedProperty unit)
        {
            _drawSection(unit);
        }

        public void DrawDangerSection(SerializedProperty unit)
        {
            _drawDangerSection(unit);
        }
    }

    internal sealed class UnitDesignerPrefabFacade : IUnitDesignerPrefabFacade
    {
        private readonly Action<SerializedProperty, string> _drawSection;
        private readonly Action<SerializedProperty> _drawComponentsSection;

        public UnitDesignerPrefabFacade(
            Action<SerializedProperty, string> drawSection,
            Action<SerializedProperty> drawComponentsSection)
        {
            _drawSection = drawSection ?? throw new ArgumentNullException(nameof(drawSection));
            _drawComponentsSection = drawComponentsSection ?? throw new ArgumentNullException(nameof(drawComponentsSection));
        }

        public void DrawSection(SerializedProperty unit, string typeId)
        {
            _drawSection(unit, typeId);
        }

        public void DrawComponentsSection(SerializedProperty unit)
        {
            _drawComponentsSection(unit);
        }
    }

    internal sealed class UnitDesignerAnimationFacade : IUnitDesignerAnimationFacade
    {
        private readonly Action<SerializedProperty> _drawSection;

        public UnitDesignerAnimationFacade(Action<SerializedProperty> drawSection)
        {
            _drawSection = drawSection ?? throw new ArgumentNullException(nameof(drawSection));
        }

        public void DrawSection(SerializedProperty unit)
        {
            _drawSection(unit);
        }
    }

    internal sealed class UnitDesignerPreviewFacade : IUnitDesignerPreviewFacade
    {
        private readonly Action<GUILayoutOption[]> _drawPanel;

        public UnitDesignerPreviewFacade(Action<GUILayoutOption[]> drawPanel)
        {
            _drawPanel = drawPanel ?? throw new ArgumentNullException(nameof(drawPanel));
        }

        public void DrawPanel(params GUILayoutOption[] options)
        {
            _drawPanel(options);
        }
    }

    internal sealed class UnitDesignerCombatFacade : IUnitDesignerCombatFacade
    {
        private readonly Action _initialize;
        private readonly Action _dispose;
        private readonly Func<bool> _isWorkspaceActive;
        private readonly Action _onWorkspaceSelected;
        private readonly Action _drawWorkspace;
        private readonly Action<SerializedProperty> _drawCompactSection;

        public UnitDesignerCombatFacade(
            Action initialize,
            Action dispose,
            Func<bool> isWorkspaceActive,
            Action onWorkspaceSelected,
            Action drawWorkspace,
            Action<SerializedProperty> drawCompactSection)
        {
            _initialize = initialize ?? throw new ArgumentNullException(nameof(initialize));
            _dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
            _isWorkspaceActive = isWorkspaceActive ?? throw new ArgumentNullException(nameof(isWorkspaceActive));
            _onWorkspaceSelected = onWorkspaceSelected ?? throw new ArgumentNullException(nameof(onWorkspaceSelected));
            _drawWorkspace = drawWorkspace ?? throw new ArgumentNullException(nameof(drawWorkspace));
            _drawCompactSection = drawCompactSection ?? throw new ArgumentNullException(nameof(drawCompactSection));
        }

        public void Initialize()
        {
            _initialize();
        }

        public void Dispose()
        {
            _dispose();
        }

        public bool IsWorkspaceActive()
        {
            return _isWorkspaceActive();
        }

        public void OnWorkspaceSelected()
        {
            _onWorkspaceSelected();
        }

        public void DrawWorkspace()
        {
            _drawWorkspace();
        }

        public void DrawCompactSection(SerializedProperty unit)
        {
            _drawCompactSection(unit);
        }
    }
}
