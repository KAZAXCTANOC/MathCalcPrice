using System.Collections.ObjectModel;

namespace MathCalcPrice.ViewModels.Entity
{
    public class MenuItem
    {
        public MenuItem()
        {
            this.Items = new ObservableCollection<MenuItem>();
        }
        public string Id { get; set; }
        public string Title { get; set; }
        public ObservableCollection<MenuItem> Items { get; set; }
    }
}
