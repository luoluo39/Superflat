using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Superflat
{
    public class Block
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int Count { get; set; }

        public string IdString => Count == 1 ? Id : $"{Count}*{Id}";
        public string TString => ToString();

        public override string ToString()
        {
            return Count < 2 ? Name : $"{Count}×{Name}";
        }

        public Block WithCount(int val)
        {
            return new Block
            {
                Name = Name,
                Id = Id,
                Count = val
            };
        }
    }

    public class Biome
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class DragAndDropListBox : ListBox
    {
        private Point _dragStartPoint;

        private static P FindVisualParent<P>(DependencyObject child)
            where P : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            if (parentObject is P parent)
                return parent;

            return FindVisualParent<P>(parentObject);
        }

        public DragAndDropListBox()
        {
            PreviewMouseMove += ListBox_PreviewMouseMove;

            var style = new Style(typeof(ListBoxItem));

            style.Setters.Add(new Setter(AllowDropProperty, true));

            style.Setters.Add(
                new EventSetter(
                    PreviewMouseLeftButtonDownEvent,
                    new MouseButtonEventHandler(ListBoxItem_PreviewMouseLeftButtonDown)));

            ItemContainerStyle = style;
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(null);
            Vector diff = _dragStartPoint - point;
            if (e.LeftButton != MouseButtonState.Pressed ||
                (!(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance) &&
                 !(Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))) return;
            var lbi = FindVisualParent<ListBoxItem>(((DependencyObject)e.OriginalSource));
            if (lbi != null)
            {
                DragDrop.DoDragDrop(lbi, lbi.DataContext, DragDropEffects.Move);
            }
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public BindingList<Block> Blocks { get; } = new BindingList<Block>();
        public BindingList<Biome> Biomes { get; } = new BindingList<Biome>();
        public ReorderableList<Block> SelectedBlocks { get; }
        public ConfigBuilder Builder { get; set; }

        public MainWindow()
        {
            DataContext = this;

            foreach (var line in File.ReadAllLines("blocks.txt"))
            {
                var split = line.Split(' ');
                Blocks.Add(new Block { Id = split[1], Name = split[0] });
            }

            foreach (var line in File.ReadAllLines("biomes.txt"))
            {
                var split = line.Split(' ');
                Biomes.Add(new Biome { Id = split[1], Name = split[0] });
            }
            Builder = new ConfigBuilder();

            InitializeComponent();

            SelectedBlocks = new ReorderableList<Block>(selectedBlockList, "TString");
            UpdateString();
        }

        private void UpdateString()
        {
            Builder.Layers = SelectedBlocks.ToArray();


            resultBox.Text = Builder.GetString();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (blockList.SelectedItem != null)
            {
                var item = (Block)blockList.SelectedItem;
                SelectedBlocks.Add(item.WithCount(int.Parse(countBox.Text)));
                UpdateString();
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = new object[selectedBlockList.SelectedItems.Count];
            selectedBlockList.SelectedItems.CopyTo(selected, 0);
            foreach (var block in selected)
            {
                SelectedBlocks.Remove((Block)block);
            }
            selectedBlockList.UnselectAll();
            UpdateString();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedBlocks.Clear();
            UpdateString();
        }


        private void RemoveDrop(object sender, DragEventArgs e)
        {
            var source = (Block)e.Data.GetData(typeof(Block));
            if (source.Count != 0)
            {
                SelectedBlocks.Remove(source);
                UpdateString();
            }
        }

        private void AddDrop(object sender, DragEventArgs e)
        {
            var source = (Block)e.Data.GetData(typeof(Block));
            if (source.Count == 0)
            {
                SelectedBlocks.Add(source.WithCount(int.Parse(countBox.Text)));
                UpdateString();
            }
        }

        private void countBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = !(e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 || e.Key == Key.Delete || e.Key == Key.Back);
            if ((e.Key == Key.Delete || e.Key == Key.Back) && ((TextBox)sender).Text.Length == 1)
            {
                ((TextBox)sender).Text = "1";
                e.Handled = true;
            }
        }

        private void UpdateCallback(object sender, EventArgs e)
        {
           UpdateString();
        }

        private void RemoveSelected(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete) return;

            var selected = new object[selectedBlockList.SelectedItems.Count];
            selectedBlockList.SelectedItems.CopyTo(selected, 0);
            foreach (var block in selected)
            {
                SelectedBlocks.Remove((Block)block);
            }
            selectedBlockList.UnselectAll();
            UpdateString();
        }
    }
}
