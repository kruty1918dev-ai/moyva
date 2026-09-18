namespace Kruty1918.Moyva.UIActions.API
{
    public static class UiActionIds
    {
        public static class Construction
        {
            public static readonly UiActionId Open =
                new("ui.construction.open");

            public static readonly UiActionId Close =
                new("ui.construction.close");

            public static readonly UiActionId Toggle =
                new("ui.construction.toggle");

            public static readonly UiActionId SelectBuilding =
                new("ui.construction.select-building");

            public static readonly UiActionId ConfirmPlacement =
                new("ui.construction.placement.confirm");

            public static readonly UiActionId CancelPlacement =
                new("ui.construction.placement.cancel");

            public static readonly UiActionId RotatePlacement =
                new("ui.construction.placement.rotate");

            public static readonly UiActionId UndoPlacement =
                new("ui.construction.placement.undo");

            public static readonly UiActionId RedoPlacement =
                new("ui.construction.placement.redo");
        }

        public static class Recruitment
        {
            public static readonly UiActionId Open =
                new("ui.recruitment.open");

            public static readonly UiActionId Close =
                new("ui.recruitment.close");

            public static readonly UiActionId Enqueue =
                new("ui.recruitment.enqueue");
            public static readonly UiActionId Cancel = new("ui.recruitment.cancel");
            public static readonly UiActionId Deploy = new("ui.recruitment.deploy");
        }

        public static class Combat
        {
            public static readonly UiActionId CaptureSelection =
                new("ui.combat.capture-selection");
        }

        public static class Deployment
        {
            public static readonly UiActionId Confirm =
                new("ui.deployment.confirm");

            public static readonly UiActionId Cancel =
                new("ui.deployment.cancel");
        }

        public static class Logistics
        {
            public static readonly UiActionId Transfer = new("ui.logistics.transfer");
            public static readonly UiActionId StartRoute = new("ui.logistics.route.start");
            public static readonly UiActionId StopRoute = new("ui.logistics.route.stop");
            public static readonly UiActionId FoundSettlement = new("ui.logistics.found-settlement");
        }

        public static class Pause
        {
            public static readonly UiActionId Open =
                new("game.pause.open");

            public static readonly UiActionId Close =
                new("game.pause.close");
        }

        public static class Diagnostics
        {
            public static readonly UiActionId InputEscape =
                new("input.escape");

            public static readonly UiActionId TextUnfocus =
                new("ui.text.unfocus");

            public static readonly UiActionId PanelClose =
                new("ui.panel.close");

            public static readonly UiActionId ModalClose =
                new("ui.modal.close");
        }

        public static readonly UiActionId EndTurn =
            new("game.turn.end");

        public static readonly UiActionId ClearSelection =
            new("selection.clear");
    }
}
