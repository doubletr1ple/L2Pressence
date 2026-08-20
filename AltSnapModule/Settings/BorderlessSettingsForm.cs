using System.Drawing;
using System.Windows.Forms;
using L2Presence.AltSnapModule.Input;

namespace L2Presence.AltSnapModule.Settings;

internal sealed class BorderlessSettingsForm : Form
{
    private readonly ComboBox _modifierBox;
    private readonly ComboBox _buttonBox;

    public BorderlessSettings Settings { get; private set; }

    public BorderlessSettingsForm(BorderlessSettings settings)
    {
        Settings = settings.Clone();

        Text = "Borderless hotkey";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(420, 145);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            ColumnCount = 2,
            RowCount = 3
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(CreateHeader("Modifier keys"), 0, 0);
        layout.Controls.Add(CreateHeader("Mouse button"), 1, 0);

        _modifierBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        _buttonBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        _modifierBox.Items.AddRange(CreateModifierChoices().Cast<object>().ToArray());
        _buttonBox.Items.AddRange(CreateButtonChoices().Cast<object>().ToArray());
        Select(_modifierBox, settings.ToggleShortcut.Modifiers);
        Select(_buttonBox, settings.ToggleShortcut.Button);
        layout.Controls.Add(_modifierBox, 0, 1);
        layout.Controls.Add(_buttonBox, 1, 1);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };
        var saveButton = new Button { Text = "Save", AutoSize = true };
        var cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
        var defaultsButton = new Button { Text = "Defaults", AutoSize = true };
        saveButton.Click += (_, _) => SaveAndClose();
        defaultsButton.Click += (_, _) => LoadShortcut(new BorderlessShortcut());
        buttons.Controls.Add(saveButton);
        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(defaultsButton);
        layout.SetColumnSpan(buttons, 2);
        layout.Controls.Add(buttons, 0, 2);

        AcceptButton = saveButton;
        CancelButton = cancelButton;
        Controls.Add(layout);
    }

    internal static string FormatShortcut(BorderlessShortcut shortcut)
        => $"{FormatModifiers(shortcut.Modifiers)} + {FormatButton(shortcut.Button)}";

    private void SaveAndClose()
    {
        Settings.ToggleShortcut = new BorderlessShortcut
        {
            Modifiers = ((Choice<HotkeyModifiers>)_modifierBox.SelectedItem!).Value,
            Button = ((Choice<MouseButtonKind>)_buttonBox.SelectedItem!).Value
        };
        DialogResult = DialogResult.OK;
        Close();
    }

    private void LoadShortcut(BorderlessShortcut shortcut)
    {
        Select(_modifierBox, shortcut.Modifiers);
        Select(_buttonBox, shortcut.Button);
    }

    private static Label CreateHeader(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Font = new Font(SystemFonts.MessageBoxFont ?? Control.DefaultFont, FontStyle.Bold),
        Anchor = AnchorStyles.Left
    };

    private static IEnumerable<Choice<HotkeyModifiers>> CreateModifierChoices()
    {
        for (var value = 1; value < 16; value++)
        {
            var modifiers = (HotkeyModifiers)value;
            yield return new Choice<HotkeyModifiers>(modifiers, FormatModifiers(modifiers));
        }
    }

    private static IEnumerable<Choice<MouseButtonKind>> CreateButtonChoices()
    {
        yield return new Choice<MouseButtonKind>(MouseButtonKind.Left, "Left");
        yield return new Choice<MouseButtonKind>(MouseButtonKind.Right, "Right");
        yield return new Choice<MouseButtonKind>(MouseButtonKind.Middle, "Middle");
        yield return new Choice<MouseButtonKind>(MouseButtonKind.XButton1, "Mouse 4");
        yield return new Choice<MouseButtonKind>(MouseButtonKind.XButton2, "Mouse 5");
    }

    private static string FormatModifiers(HotkeyModifiers modifiers)
    {
        var names = new List<string>();
        if (modifiers.HasFlag(HotkeyModifiers.Control)) names.Add("Ctrl");
        if (modifiers.HasFlag(HotkeyModifiers.Alt)) names.Add("Alt");
        if (modifiers.HasFlag(HotkeyModifiers.Shift)) names.Add("Shift");
        if (modifiers.HasFlag(HotkeyModifiers.Windows)) names.Add("Win");
        return string.Join(" + ", names);
    }

    private static string FormatButton(MouseButtonKind button) => button switch
    {
        MouseButtonKind.Left => "Left Mouse",
        MouseButtonKind.Right => "Right Mouse",
        MouseButtonKind.Middle => "Middle Mouse",
        MouseButtonKind.XButton1 => "Mouse 4",
        MouseButtonKind.XButton2 => "Mouse 5",
        _ => button.ToString()
    };

    private static void Select<T>(ComboBox comboBox, T value)
    {
        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (comboBox.Items[index] is Choice<T> choice && EqualityComparer<T>.Default.Equals(choice.Value, value))
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }

        comboBox.SelectedIndex = 0;
    }

    private sealed record Choice<T>(T Value, string Text)
    {
        public override string ToString() => Text;
    }
}
