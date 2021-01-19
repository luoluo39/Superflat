using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// <summary>
    /// NullableBox.xaml 的交互逻辑
    /// </summary>
    public partial class NullableBox : UserControl
    {
        public string Title
        {
            get => check.Content as string;
            set => check.Content = value;
        }

        public bool NumberOnly { get; set; } = false;
        public bool AllowDecimal { get; set; } = false;

        public event EventHandler ValueChanged;

        public static readonly DependencyProperty ValueIntProperty = DependencyProperty.Register("ValueInt", typeof(XValue<int>), typeof(NullableBox));
        public XValue<int> ValueInt
        {
            get => (XValue<int>)GetValue(ValueIntProperty);
            set
            {
                check.IsChecked = value.HasValue;
                text.Text = value.Value.ToString();
                SetValue(ValueIntProperty, value);
            }
        }
        public static readonly DependencyProperty ValueFloatProperty = DependencyProperty.Register("ValueFloat", typeof(XValue<float>), typeof(NullableBox));
        public XValue<float> ValueFloat
        {
            get => (XValue<float>)GetValue(ValueFloatProperty);
            set
            {
                check.IsChecked = value.HasValue;
                text.Text = value.Value.ToString();
                SetValue(ValueFloatProperty, value);
            }
        }

        public NullableBox()
        {
            InitializeComponent();
        }

        private void SetEnabled(bool value)
        {
            var vi = ValueInt;
            vi.HasValue = value;
            SetValue(ValueIntProperty, vi);

            var vf = ValueFloat;
            vf.HasValue = value;
            SetValue(ValueFloatProperty, vf);
            text.IsEnabled = value;

            ValueChanged?.Invoke(this, null);
        }

        private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!NumberOnly) return;

            var box = (TextBox)sender;
            e.Handled = !(AllowDecimal && e.Key == Key.Decimal && !box.Text.Contains('.') || e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 || e.Key == Key.Delete || e.Key == Key.Back);
            if ((e.Key == Key.Delete || e.Key == Key.Back) && ((TextBox)sender).Text.Length == 1)
            {
                ((TextBox)sender).Text = "1";
                e.Handled = true;
            }
        }

        private void check_Checked(object sender, RoutedEventArgs e)
        {
            SetEnabled(true);
        }

        private void check_Unchecked(object sender, RoutedEventArgs e)
        {
            SetEnabled(false);
        }

        private void text_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValueInt = new XValue<int> { HasValue = check.IsChecked == true, Value = int.Parse(text.Text) };
            ValueFloat = new XValue<float> { HasValue = check.IsChecked == true, Value = float.Parse(text.Text) };

            ValueChanged?.Invoke(this, null);
        }
    }
}
