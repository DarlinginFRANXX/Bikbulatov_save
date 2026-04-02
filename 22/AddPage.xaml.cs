using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Galiev_Глазки_save
{
    public partial class AddPage : Page
    {
        private Agent _currentAgent;
        private GalievSaveEntities _context;
        private List<Product> _allProducts;
        private List<ProductSale> _sales;

        public AddPage(Agent agent)
        {
            InitializeComponent();

            _context = new GalievSaveEntities();
            _allProducts = _context.Product.ToList();
            ProductCombo.ItemsSource = _allProducts;

            if (agent != null && agent.ID != 0)
            {
                _currentAgent = _context.Agent
                    .Include(a => a.ProductSale.Select(ps => ps.Product))
                    .FirstOrDefault(a => a.ID == agent.ID);

                if (_currentAgent == null)
                {
                    _currentAgent = new Agent { ProductSale = new List<ProductSale>() };
                    DeleteBtn.Visibility = Visibility.Hidden;
                }
                else
                {
                    ComboType.SelectedIndex = _currentAgent.AgentTypeID - 1;
                    DeleteBtn.Visibility = Visibility.Visible;
                    _sales = _currentAgent.ProductSale.ToList();
                }
            }
            else
            {
                _currentAgent = new Agent { ProductSale = new List<ProductSale>() };
                DeleteBtn.Visibility = Visibility.Hidden;
                _sales = new List<ProductSale>();
            }

            DataContext = _currentAgent;
            SalesListView.ItemsSource = _sales;

            // Поиск в ComboBox
            ProductCombo.Loaded += (s, e) =>
            {
                var textBox = ProductCombo.Template.FindName("PART_EditableTextBox", ProductCombo) as TextBox;
                if (textBox != null)
                    textBox.TextChanged += ProductCombo_TextChanged;
            };
        }

        private void ProductCombo_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = ((TextBox)sender).Text;
            ProductCombo.ItemsSource = string.IsNullOrEmpty(filter)
                ? _allProducts
                : _allProducts.Where(p => p.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            ProductCombo.IsDropDownOpen = true;
        }

        private void ChangePictureBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp" };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string agentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agents");
                    Directory.CreateDirectory(agentsFolder);

                    string destPath = Path.Combine(agentsFolder, Path.GetFileName(dialog.FileName));
                    int count = 1;
                    while (File.Exists(destPath))
                    {
                        string name = Path.GetFileNameWithoutExtension(dialog.FileName);
                        string ext = Path.GetExtension(dialog.FileName);
                        destPath = Path.Combine(agentsFolder, $"{name}_{count}{ext}");
                        count++;
                    }
                    File.Copy(dialog.FileName, destPath);

                    _currentAgent.Logo = Path.GetFileName(destPath);
                    LogoImage.Source = ImageHelper.LoadLogo(_currentAgent.Logo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка копирования: " + ex.Message);
                }
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentAgent.Title)) errors.AppendLine("Наименование");
            if (string.IsNullOrWhiteSpace(_currentAgent.Address)) errors.AppendLine("Адрес");
            if (string.IsNullOrWhiteSpace(_currentAgent.DirectorName)) errors.AppendLine("ФИО директора");
            if (ComboType.SelectedItem == null) errors.AppendLine("Тип агента");
            else _currentAgent.AgentTypeID = ComboType.SelectedIndex + 1;
            if (_currentAgent.Priority <= 0) errors.AppendLine("Приоритет (положительное число)");
            if (string.IsNullOrWhiteSpace(_currentAgent.INN)) errors.AppendLine("ИНН");
            if (string.IsNullOrWhiteSpace(_currentAgent.KPP)) errors.AppendLine("КПП");
            if (string.IsNullOrWhiteSpace(_currentAgent.Phone)) errors.AppendLine("Телефон");
            else if (_currentAgent.Phone.Count(char.IsDigit) != 11) errors.AppendLine("Телефон (11 цифр)");
            if (string.IsNullOrWhiteSpace(_currentAgent.Email)) errors.AppendLine("Email");

            if (errors.Length > 0)
            {
                MessageBox.Show("Заполните поля:\n" + errors);
                return;
            }

            if (_currentAgent.ID == 0) _context.Agent.Add(_currentAgent);

            try
            {
                _context.SaveChanges();
                MessageBox.Show("Сохранено");
                // ВАЖНО: создаём новую страницу AgentPage, чтобы обновить список
                Manager.MainFrame.Navigate(new AgentPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAgent.SalesForYear > 0)
            {
                MessageBox.Show("Невозможно удалить – есть продажи");
                return;
            }

            if (MessageBox.Show("Удалить агента?", "Внимание", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Agent.Remove(_currentAgent);
                    _context.SaveChanges();
                    // ВАЖНО: после удаления тоже создаём новую страницу
                    Manager.MainFrame.Navigate(new AgentPage());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void AddSaleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ProductCombo.SelectedItem == null) { MessageBox.Show("Выберите продукт"); return; }
            if (!int.TryParse(ProductCountBox.Text, out int count) || count <= 0) { MessageBox.Show("Кол-во > 0"); return; }
            if (SaleDatePicker.SelectedDate == null) { MessageBox.Show("Выберите дату"); return; }

            var product = (Product)ProductCombo.SelectedItem;
            var sale = new ProductSale
            {
                ProductID = product.ID,
                ProductCount = count,
                SaleDate = SaleDatePicker.SelectedDate.Value,
                Product = product
            };
            _currentAgent.ProductSale.Add(sale);
            _sales.Add(sale);
            SalesListView.Items.Refresh();

            ProductCountBox.Clear();
            SaleDatePicker.SelectedDate = null;
            ProductCombo.SelectedItem = null;
        }

        private void DeleteSaleBtn_Click(object sender, RoutedEventArgs e)
        {
            var sale = (sender as Button)?.Tag as ProductSale;
            if (sale != null)
            {
                _context.ProductSale.Remove(sale);
                _currentAgent.ProductSale.Remove(sale);
                _sales.Remove(sale);
                SalesListView.Items.Refresh();
            }
        }
    }
}