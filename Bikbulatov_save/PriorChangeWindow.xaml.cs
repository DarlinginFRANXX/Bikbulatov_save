using System;
using System.Windows;

namespace Bikbulatov_save
{
    public partial class PriorChangeWindow : Window
    {
        public PriorChangeWindow(int maxPriority)
        {
            InitializeComponent();
            TBPriority.Text = maxPriority.ToString();
        }

        private void ChangePrior_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TBPriority.Text, out int newPriority) && newPriority > 0)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Введите целое положительное число", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}