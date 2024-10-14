using Microsoft.Maui.Controls;

namespace lab01
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent(); // Ініціалізація компонентів з App.xaml
            MainPage = new MainPage(); // Встановлення головної сторінки
        }
    }
}