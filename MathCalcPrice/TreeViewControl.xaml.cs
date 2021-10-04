using MathCalcPrice.Service.OneDriveControllers;
using MathCalcPrice.StaticResources;
using MathCalcPrice.ViewModels.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MathCalcPrice
{
    /// <summary>
    /// Логика взаимодействия для TreeViewControl.xaml
    /// </summary>
    public partial class TreeViewControl : Window
    {
        public MenuItem selectedMenuItem { get; set; }


        OneDriveController _OneDriveController;
        public TreeViewControl()
        {
            InitializeComponent();
            _OneDriveController = new OneDriveController();
            MenuItem Root = Task.Run(() => _OneDriveController.GetDataFromGroupAsync()).Result;

            Root.Items.RemoveAt(0);
            Root.Items.RemoveAt(2);

            trvMenu.Items.Add(Root);
        }

        private void trvMenu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            selectedMenuItem = (MenuItem)e.NewValue;
        }
        private void SelectListCost_Click(object sender, RoutedEventArgs e)
        {
            if(selectedMenuItem.Title.Contains(".xlsx"))
            {
                Cache.SelectedCostViewElement = selectedMenuItem;
                SelectedCost.Text = selectedMenuItem.Title;
            }
            else
            {
                Cache.SelectedCostViewElement = null;
                SelectedCost.Text = "Неверный формат файла";
            }

        }

        private void SelectListJob_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMenuItem.Title.Contains(".xlsx"))
            {
                Cache.SelectedJobViewElement = selectedMenuItem;
                SelectedJob.Text = selectedMenuItem.Title;
            }
            else
            {
                Cache.SelectedJobViewElement = null;
                SelectedJob.Text = "Неверный формат файла";
            }
        }

        private void SelectShablon_Click(object sender, RoutedEventArgs e)
        {
            if (selectedMenuItem.Title.Contains(".xlsx"))
            {
                Cache.SelectedTreeViewElement = selectedMenuItem;
                SelectedShablon.Text = selectedMenuItem.Title;
            }
            else
            {
                Cache.SelectedTreeViewElement = null;
                SelectedShablon.Text = "Неверный формат файла";
            }
        }
    }
}
