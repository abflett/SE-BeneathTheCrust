using RichHudFramework.UI;

namespace WkKn
{
    internal sealed class WkSettingsTextField : TextField
    {
        internal WkSettingsTextField(HudParentBase parent) : base(parent)
        {
            // The standalone window runs in CursorOnly mode, without the game's chat box.
            // Explicit input is supported by RHF but must be scoped to the focused field.
            BindInput.InputFilter = SeBlacklistModes.FullWithChat;
            CharFilterFunc = delegate(char value) { return value >= ' ' && value != '\u007f'; };
            FocusHandler.GainedInputFocus += delegate { if (EnableEditing) OpenInput(); };
            FocusHandler.LostInputFocus += delegate { CloseInput(); };
        }

        internal void StopEditing()
        {
            CloseInput();
            if (FocusHandler.HasFocus)
                FocusHandler.ReleaseFocus();
        }
    }
}
