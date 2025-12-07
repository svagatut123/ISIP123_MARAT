using System;
using System.Windows;
using System.Windows.Controls;

namespace PizzaOrder
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            QuantitySlider.ValueChanged += QuantitySlider_ValueChanged;
        }

        private void QuantitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int quantity = (int)QuantitySlider.Value;
            QuantityText.Text = $"{quantity} шт";
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            string size = "";
            if (SmallPizza.IsChecked == true) size = "Маленькая";
            if (MediumPizza.IsChecked == true) size = "Средняя";
            if (BigPizza.IsChecked == true) size = "Большая";

            string toppings = "";
            if (CheeseCheckBox.IsChecked == true) toppings += "Сыр, ";
            if (MushroomsCheckBox.IsChecked == true) toppings += "Грибы, ";
            if (PepperoniCheckBox.IsChecked == true) toppings += "Пепперони, ";

            if (toppings.Length > 0)
                toppings = toppings.TrimEnd(',', ' ');
            else
                toppings = "Без добавок";

            string message = $"Заказ оформлен!\n\n" +
                            $"Размер: {size}\n" +
                            $"Добавки: {toppings}\n" +
                            $"Количество: {QuantityText.Text}\n" +
                            $"Комментарий: {(string.IsNullOrWhiteSpace(CommentTextBox.Text) ? "Нет" : CommentTextBox.Text)}";

            MessageBox.Show(message, "Информация о заказе",
                           MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}