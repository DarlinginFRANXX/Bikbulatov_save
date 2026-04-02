using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.Entity;

namespace Galiev_Глазки_save
{
    public partial class AgentPage : Page
    {
        private List<Agent> _filteredAgents;
        private int pageSize = 10;
        private int currentPage = 1;

        public AgentPage()
        {
            InitializeComponent();
            CBSort.SelectedIndex = 0;
            CBFilt.SelectedIndex = 0;
            UpdateAgents();
        }

        public void UpdateAgents()
        {
            using (var context = new GalievSaveEntities())
            {
                var currentAgents = context.Agent
                    .Include(a => a.AgentType)
                    .Include(a => a.ProductSale.Select(ps => ps.Product))   // ← добавляем Include для продуктов
                    .ToList();

                // Фильтрация по типу
                string filterType = (CBFilt.SelectedItem as TextBlock)?.Text;
                if (filterType != "Все типы")
                {
                    currentAgents = currentAgents.Where(a => a.AgentType.Title == filterType).ToList();
                }

                // Поиск
                string searchText = TBSearch.Text.ToLower();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string cleanedSearchPhone = searchText
                        .Replace("+", "").Replace("(", "").Replace(")", "")
                        .Replace("-", "").Replace(" ", "").Replace("8", "7");

                    currentAgents = currentAgents.Where(a =>
                        (a.Title?.ToLower().Contains(searchText) == true) ||
                        (a.Email?.ToLower().Contains(searchText) == true) ||
                        (a.Phone?.Replace("+", "").Replace("(", "").Replace(")", "")
                                .Replace("-", "").Replace(" ", "").Replace("8", "7")
                                .Contains(cleanedSearchPhone) == true)
                    ).ToList();
                }

                // Сортировка
                int sortIndex = CBSort.SelectedIndex;
                switch (sortIndex)
                {
                    case 1: currentAgents = currentAgents.OrderBy(a => a.Title).ToList(); break;
                    case 2: currentAgents = currentAgents.OrderByDescending(a => a.Title).ToList(); break;
                    case 3: currentAgents = currentAgents.OrderBy(a => a.Discount).ToList(); break;
                    case 4: currentAgents = currentAgents.OrderByDescending(a => a.Discount).ToList(); break;
                    case 5: currentAgents = currentAgents.OrderBy(a => a.Priority).ToList(); break;
                    case 6: currentAgents = currentAgents.OrderByDescending(a => a.Priority).ToList(); break;
                    default: break;
                }

                _filteredAgents = currentAgents;
                currentPage = 1;
                ChangePage();
            }
        }

        private void ChangePage()
        {
            PageListBox.Items.Clear();
            int totalPages = (_filteredAgents.Count + pageSize - 1) / pageSize;
            for (int i = 1; i <= totalPages; i++)
                PageListBox.Items.Add(i);

            PageListBox.SelectedItem = currentPage;

            var agentsPage = _filteredAgents
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            AgentListView.ItemsSource = agentsPage;
        }

        private void AgentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangePriorityBtn.Visibility = AgentListView.SelectedItems.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e) => UpdateAgents();
        private void CBSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateAgents();
        private void CBFilt_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateAgents();

        private void ChangePriorityBtn_Click(object sender, RoutedEventArgs e)
        {
            int maxPriority = AgentListView.SelectedItems.Cast<Agent>().Max(a => a.Priority);
            var priorWindow = new PriorChangeWindow(maxPriority);
            priorWindow.Owner = Window.GetWindow(this);
            priorWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            priorWindow.ShowDialog();

            if (int.TryParse(priorWindow.TBPriority.Text, out int newPriority))
            {
                foreach (Agent agent in AgentListView.SelectedItems)
                    agent.Priority = newPriority;

                try
                {
                    GalievSaveEntities.GetContext().SaveChanges();
                    AgentListView.SelectedItems.Clear();
                    UpdateAgents();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void AddAgentBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddPage(null));
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1) { currentPage--; ChangePage(); }
        }

        private void RightDirButton_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (_filteredAgents.Count + pageSize - 1) / pageSize;
            if (currentPage < totalPages) { currentPage++; ChangePage(); }
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (PageListBox.SelectedItem is int page && page != currentPage)
            {
                currentPage = page;
                ChangePage();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddPage(AgentListView.SelectedItem as Agent));
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateAgents();
        }
    }
}