using MathCalcPrice.Entity;
using MathCalcPrice.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    /// Логика взаимодействия для LookUpElements.xaml
    /// </summary>
    public partial class LookUpElements : Window
    {
        public LookUpElements(List<GroupWithCount> list, List<Groups> groups)
        {
            InitializeComponent();
            this.Height = 450;
            this.Width = 600;
            var mwvm = new LookUpElementsViewModel();
            mwvm.groupWithCounts = list;
            mwvm.Groups = groups;
            this.DataContext = mwvm;
        }
    }
}
