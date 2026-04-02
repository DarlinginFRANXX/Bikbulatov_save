using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Bikbulatov_save
{
    public partial class AddEditPage : Page
    {
        private Agent _currentAgent;
        private Bikbulatov_saveEntities _context;
        private List<Product> _allProducts;
        private List<ProductSale> _sales;

        public AddEditPage()
        {
            InitializeComponent();

            // Создаём папку agents, если её нет
            string agentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agents");
            if (!Directory.Exists(agentsFolder))
                Directory.CreateDirectory(agentsFolder);

            _context = Bikbulatov_saveEntities.GetContext();
            _allProducts = _context.Product.ToList();
            ProductCombo.ItemsSource = _allProducts;

            _currentAgent = new Agent();
            _currentAgent.ProductSale = new List<ProductSale>();
            _sales = new List<ProductSale>();
            DataContext = _currentAgent;
            SalesListView.ItemsSource = _sales;
            DeleteBtn.Visibility = Visibility.Hidden;
        }

        public AddEditPage(Agent agent) : this()
        {
            if (agent != null && agent.ID != 0)
            {
                _currentAgent = _context.Agent
                    .Include(a => a.ProductSale.Select(ps => ps.Product))
                    .FirstOrDefault(a => a.ID == agent.ID);

                if (_currentAgent != null)
                {
                    ComboType.SelectedIndex = _currentAgent.AgentTypeID - 1;
                    DeleteBtn.Visibility = Visibility.Visible;
                    _sales = _currentAgent.ProductSale.ToList();
                    SalesListView.ItemsSource = _sales;
                    DataContext = _currentAgent;
                }
            }
        }

        private void ChangePictureBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string agentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agents");
                    string fileName = Path.GetFileName(openFileDialog.FileName);
                    string destPath = Path.Combine(agentsFolder, fileName);
                    int counter = 1;
                    while (File.Exists(destPath))
                    {
                        string name = Path.GetFileNameWithoutExtension(fileName);
                        string ext = Path.GetExtension(fileName);
                        destPath = Path.Combine(agentsFolder, $"{name}_{counter}{ext}");
                        counter++;
                    }
                    File.Copy(openFileDialog.FileName, destPath);
                    _currentAgent.Logo = Path.GetFileName(destPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка копирования: " + ex.Message);
                }
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentAgent.Title))
                errors.AppendLine("Наименование");

            if (string.IsNullOrWhiteSpace(_currentAgent.Address))
                errors.AppendLine("Адрес");

            if (string.IsNullOrWhiteSpace(_currentAgent.DirectorName))
                errors.AppendLine("ФИО директора");

            if (ComboType.SelectedItem == null)
                errors.AppendLine("Тип агента");
            else
                _currentAgent.AgentTypeID = ComboType.SelectedIndex + 1;

            if (_currentAgent.Priority < 0)
                errors.AppendLine("Приоритет (положительное число)");


            if (string.IsNullOrWhiteSpace(_currentAgent.INN))
                errors.AppendLine("ИНН");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(_currentAgent.INN, @"^\d{10}$|^\d{12}$"))
                errors.AppendLine("ИНН (10 или 12 цифр)");


            if (string.IsNullOrWhiteSpace(_currentAgent.KPP))
                errors.AppendLine("КПП");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(_currentAgent.KPP, @"^\d{9}$"))
                errors.AppendLine("КПП (9 цифр)");


            if (string.IsNullOrWhiteSpace(_currentAgent.Phone))
                errors.AppendLine("Телефон");
            else if (_currentAgent.Phone.Count(char.IsDigit) != 11)
                errors.AppendLine("Телефон (11 цифр)");


            if (string.IsNullOrWhiteSpace(_currentAgent.Email))
                errors.AppendLine("Email");
            else
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(_currentAgent.Email);
                    if (addr.Address != _currentAgent.Email)
                        errors.AppendLine("Email (неверный формат)");
                }
                catch
                {
                    errors.AppendLine("Email (неверный формат)");
                }
            }
            if (errors.Length > 0)
            {
                MessageBox.Show("Заполните корректно:\n" + errors,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (_currentAgent.ID == 0)
                _context.Agent.Add(_currentAgent);

            try
            {
                _context.SaveChanges();

                MessageBox.Show("Сохранено",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAgent.SaleForYear > 0)
            {
                MessageBox.Show("Невозможно удалить – есть продажи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Удалить агента?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Agent.Remove(_currentAgent);
                    _context.SaveChanges();
                    Manager.MainFrame.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddSaleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ProductCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите продукт");
                return;
            }
            if (!int.TryParse(ProductCountBox.Text, out int count) || count <= 0)
            {
                MessageBox.Show("Введите положительное целое число");
                return;
            }
            if (SaleDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату");
                return;
            }

            Product selectedProduct = (Product)ProductCombo.SelectedItem;
            ProductSale newSale = new ProductSale
            {
                ProductID = selectedProduct.ID,
                ProductCount = count,
                SaleDate = SaleDatePicker.SelectedDate.Value,
                AgentID = _currentAgent.ID,
                Product = selectedProduct
            };

            if (_currentAgent.ID != 0)
                _context.ProductSale.Add(newSale);

            _currentAgent.ProductSale.Add(newSale);
            _sales.Add(newSale);
            SalesListView.Items.Refresh();

            ProductCountBox.Text = "";
            SaleDatePicker.SelectedDate = null;
            ProductCombo.SelectedItem = null;
        }

        private void DeleteSaleBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            ProductSale sale = btn.Tag as ProductSale;
            if (sale != null)
            {
                if (sale.ID != 0)
                    _context.ProductSale.Remove(sale);
                _currentAgent.ProductSale.Remove(sale);
                _sales.Remove(sale);
                SalesListView.Items.Refresh();
            }
        }
    }
}