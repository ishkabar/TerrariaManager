using System;
using System.Windows;

namespace Ogur.Terraria.Manager.Devexpress.Views;

public partial class LicenseExpiredWindow : Window
{
    public LicenseExpiredWindow(DateTime expirationDate)
    {
        InitializeComponent();
        ExpirationDateText.Text = $"Data wygaśnięcia: {expirationDate:yyyy-MM-dd}";
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}