using System.Windows;
using System.Windows.Input;

namespace Ogur.Terraria.Manager.Devexpress.Views;

public partial class InputDialog : Window
{
    public string Prompt { get; set; }
    public string InputValue { get; set; } = "";
    public bool WasConfirmed { get; private set; }

    public InputDialog(string title, string prompt)
    {
        InitializeComponent();
        Title = title;
        Prompt = prompt;
        DataContext = this;

        Loaded += (s, e) => InputTextBox.Focus();

        KeyDown += InputDialog_KeyDown;
    }

    private void InputDialog_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            WasConfirmed = true;
            Close();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            WasConfirmed = false;
            Close();
            e.Handled = true;
        }
    }

    private void Execute_Click(object sender, RoutedEventArgs e)
    {
        WasConfirmed = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        WasConfirmed = false;
        Close();
    }
}