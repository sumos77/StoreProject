using System;
using System.Collections.Generic;
using System.IO;
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

namespace StoreProject
{
    public class Product
    {
        public string Name;
        public string Description;
        public string Price;
        public string Path;
    }

    public class Rebate
    {
        public string Code;
        public string Percent;
    }

    public partial class MainWindow : Window
    {
        public static Product[] Products;
        public const string ProductFilePath = "Products.csv";

        public static Rebate[] Rebates;
        public const string RebateFilePath = "Rebates.csv";

        public static Dictionary<Product, int> Cart = new Dictionary<Product, int>();
        //public const string CartFilePath = @"C:\Windows\Temp\Cart.csv";

        //Instance variables to be reached and changed in all methods in this class
        private StackPanel cartStack;
        private TextBox rebateCodeTextBox;
        private Label sumPurchaseLabel;
        private decimal rebateMultiplier;
        //private Rebate currentRebateCode = null;
        private decimal sumPurchase;

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        private void Start()
        {
            Products = LoadProducts();
            Rebates = LoadRebates();
            //Cart = LoadCart();
            rebateMultiplier = 1;
            sumPurchase = 0;

            // Window options
            Title = "Butik";
            Width = 800;
            Height = 800;
            //SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Scrolling
            ScrollViewer root = new ScrollViewer();
            root.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Content = root;

            // Main grid
            StackPanel mainStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5)
            };
            root.Content = mainStack;

            WrapPanel wrap = new WrapPanel
            {
                Orientation = Orientation.Horizontal
            };

            mainStack.Children.Add(wrap);

            foreach (Product p in Products)
            {
                wrap.Children.Add(CreateProductPanel(p));
                //mainStack.Children.Add(CreateCartGrid(p.Name + " " + p.Price + " kr ", int.Parse(p.Price)));
                //sumPurchase += decimal.Parse(p.Price);
            }

            cartStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5)
            };

            mainStack.Children.Add(cartStack);
            Cart.Add(Products[0], 100);
            Cart.Add(Products[1], 100);
            Cart.Add(Products[2], 100);

            DrawCart();

            Grid cartGrid = new Grid();
            cartGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cartGrid.ColumnDefinitions.Add(new ColumnDefinition());
            cartGrid.ColumnDefinitions.Add(new ColumnDefinition());
            cartGrid.ColumnDefinitions.Add(new ColumnDefinition());
            mainStack.Children.Add(cartGrid);

            Button saveCartButton = CreateButton("Spara kundvagnen");
            cartGrid.Children.Add(saveCartButton);
            Grid.SetRow(saveCartButton, 0);
            Grid.SetColumn(saveCartButton, 0);

            Button emptyCartButton = CreateButton("Töm kundvagnen");
            cartGrid.Children.Add(emptyCartButton);
            Grid.SetRow(emptyCartButton, 0);
            Grid.SetColumn(emptyCartButton, 1);

            Button confirmPurchaseButton = CreateButton("Avsluta köp");
            cartGrid.Children.Add(confirmPurchaseButton);
            Grid.SetRow(confirmPurchaseButton, 0);
            Grid.SetColumn(confirmPurchaseButton, 2);

            Grid rebateCodeGrid = new Grid();
            rebateCodeGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rebateCodeGrid.ColumnDefinitions.Add(new ColumnDefinition());
            rebateCodeGrid.ColumnDefinitions.Add(new ColumnDefinition());
            rebateCodeGrid.ColumnDefinitions.Add(new ColumnDefinition());
            mainStack.Children.Add(rebateCodeGrid);

            Label rebateCodeLabel = CreateLabel("Rabbatkod");
            rebateCodeLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
            rebateCodeGrid.Children.Add(rebateCodeLabel);
            Grid.SetRow(rebateCodeLabel, 0);
            Grid.SetColumn(rebateCodeLabel, 0);

            rebateCodeTextBox = new TextBox
            {
                Margin = new Thickness(5),
            };
            rebateCodeGrid.Children.Add(rebateCodeTextBox);
            Grid.SetRow(rebateCodeTextBox, 0);
            Grid.SetColumn(rebateCodeTextBox, 1);

            Button confirmRebateCodeButton = CreateButton("OK");
            confirmRebateCodeButton.IsDefault = true;
            rebateCodeGrid.Children.Add(confirmRebateCodeButton);
            Grid.SetRow(confirmRebateCodeButton, 0);
            Grid.SetColumn(confirmRebateCodeButton, 2);
            confirmRebateCodeButton.Click += ConfirmRebateCodeButton_Click;

            Grid sumPurchaseGrid = new Grid();
            sumPurchaseGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            sumPurchaseGrid.ColumnDefinitions.Add(new ColumnDefinition());
            mainStack.Children.Add(sumPurchaseGrid);

            //totalRebate = (100 - decimal.Parse(Rebates[4].Percent)) / 100;
            //sumPurchase = sumPurchase * rebateMultiplier;
            sumPurchaseLabel = CreateLabel("Summa: " + Math.Round(sumPurchase, 2) + " kr");
            sumPurchaseLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
            sumPurchaseGrid.Children.Add(sumPurchaseLabel);
            Grid.SetRow(sumPurchaseLabel, 0);
            Grid.SetColumn(sumPurchaseLabel, 0);
        }

        private void ConfirmRebateCodeButton_Click(object sender, RoutedEventArgs e)
        {
            string codeText = rebateCodeTextBox.Text.ToUpper();
            bool codeIsValid = false;
            //This variable is important so the SumPurchase doesn't become the wrong value, if one presses
            //the confirmationbutton (OK), multiple times.
            decimal sumAfterRebate;

            if (codeText.Length > 3 && codeText.Length < 20)
            {
                foreach (Rebate r in Rebates)
                {
                    if (r.Code == codeText)
                    {
                        codeIsValid = true;
                        rebateMultiplier = (100 - decimal.Parse(r.Percent)) / 100;
                        //currentRebateCode = r;
                    }
                }

                if (codeIsValid)
                {
                    sumAfterRebate = sumPurchase * rebateMultiplier;
                    sumPurchaseLabel.Content = "Summa: " + sumAfterRebate + " kr";
                }
                else
                {
                    MessageBox.Show("Fel kod!");
                    sumPurchaseLabel.Content = "Summa: " + sumPurchase + " kr";
                    //currentRebateCode = null;
                }
            }
            else
            {
                MessageBox.Show("Fel antal tecken!");
            }   
        }

        public static Product[] LoadProducts()
        {
            // If the file doesn't exist, stop the program completely.
            if (!File.Exists(ProductFilePath))
            {
                MessageBox.Show(ProductFilePath + " finns inte, eller har inte blivit satt till 'Copy Always'.");
                Environment.Exit(1);
            }

            // Create an empty list of products, then go through each line of the file to fill it.
            List<Product> products = new List<Product>();
            string[] lines = File.ReadAllLines(ProductFilePath);
            foreach (string line in lines)
            {
                try
                {
                    // First, split the line on commas (CSV means "comma-separated values").
                    string[] parts = line.Split(',');

                    // Then create a product with its values set to the different parts of the line.
                    Product p = new Product
                    {
                        Name = parts[0],
                        Description = parts[1],
                        Price = parts[2],
                        Path = parts[3]
                    };
                    products.Add(p);
                }
                catch
                {
                    MessageBox.Show("Fel vid inläsning av en produkt!");
                }
            }

            // The method returns an array rather than a list (because the products are fixed after the program has started), so we need to convert it before returning.
            return products.ToArray();
        }

        public static Rebate[] LoadRebates()
        {
            // If the file doesn't exist, stop the program completely.
            if (!File.Exists(RebateFilePath))
            {
                MessageBox.Show(RebateFilePath + " finns inte, eller har inte blivit satt till 'Copy Always'.");
                Environment.Exit(1);
            }

            // Create an empty list of products, then go through each line of the file to fill it.
            List<Rebate> rebates = new List<Rebate>();
            string[] lines = File.ReadAllLines(RebateFilePath);
            foreach (string line in lines)
            {
                try
                {
                    // First, split the line on commas (CSV means "comma-separated values").
                    string[] parts = line.Split(',');

                    // Then create a product with its values set to the different parts of the line.
                    Rebate r = new Rebate
                    {
                        //ToUpper to make the codes caseinsensitive
                        Code = parts[0].ToUpper(),
                        Percent = parts [1]
                    };
                    rebates.Add(r);
                }
                catch
                {
                    MessageBox.Show("Fel vid inläsning av en produkt!");
                }
            }

            // The method returns an array rather than a list (because the products are fixed after the program has started), so we need to convert it before returning.
            return rebates.ToArray();
        }

        //public static Dictionary<Product, int> LoadCart()
        //{
        //    // A cart is a dictionary (as described earlier), so create an empty one to fill as we read the CSV file.
        //    Dictionary<Product, int> savedCart = new Dictionary<Product, int>();

        //    // Go through each line and split it on commas, as in `LoadProducts`.
        //    string[] lines = File.ReadAllLines(CartFilePath);
        //    foreach (string line in lines)
        //    {
        //        string[] parts = line.Split(',');
        //        string name = parts[0];
        //        int amount = int.Parse(parts[1]);

        //        // We only store the product's code in the CSV file, but we need to find the actual product object with that code.
        //        // To do this, we access the static `products` variable and find the one with the matching code, then grab that product object.
        //        Product current = null;
        //        foreach (Product p in Products)
        //        {
        //            if (p.Name == name)
        //            {
        //                current = p;
        //            }
        //        }

        //        // Now that we have the product object (and not just the code), we can save it in the dictionary.
        //        savedCart[current] = amount;
        //    }

        //    return savedCart;
        //}

        //Creates the panel and GUI for a product
        private StackPanel CreateProductPanel(Product product)
        {
            string addButton = "Lägg till";

            StackPanel productStackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5)
            };

            ImageSource source = new BitmapImage(new Uri(product.Path, UriKind.RelativeOrAbsolute));
            Image image = new Image
            {
                Source = source,
                Width = 130,
                Height = 100,
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };

            productStackPanel.Children.Add(image);
            Label productNameLabel = CreateLabel(product.Name);
            productStackPanel.Children.Add(productNameLabel);
            Label productDescriptionLabel = CreateLabel(product.Description);
            productStackPanel.Children.Add(productDescriptionLabel);
            Label productPriceLabel = CreateLabel(product.Price + " kr");
            productStackPanel.Children.Add(productPriceLabel);
            Button addProductButton = CreateButton(addButton, product);
            productStackPanel.Children.Add(addProductButton);

            addProductButton.Click += AddProductButton_Click;

            return productStackPanel;
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            Product addedProduct = (Product)clickedButton.Tag;
            if (Cart.ContainsKey(addedProduct))
            { 
                Cart[addedProduct]++; 
            }
            else
            {
                Cart.Add(addedProduct, 1);
            }
            DrawCart();

            //cartStack.Children.Add(CreateLabel("Hello"));
            //stack.Children.Add(CreateCartGrid(Products.Name[1] + " " + p.Price + " kr ", int.Parse(p.Price)));
        }

        private void DrawCart()
        {
            if (cartStack.Children.Count > 0)
            {
                cartStack.Children.Clear();
            }
            foreach (KeyValuePair<Product, int> entry in Cart)
            {
                cartStack.Children.Add(CreateCartGrid(entry.Key, entry.Value));
            }
        }
        public Grid CreateCartGrid(Product product, int quantity)
        {
            string plus = "+";
            string minus = "-";
            string delete = "Ta bort";

            Grid addProductGrid = new Grid();
            addProductGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            //A column that will be empty for asthetics
            addProductGrid.ColumnDefinitions.Add(new ColumnDefinition());
            addProductGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            addProductGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            addProductGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            addProductGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            addProductGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Label productNameLabel = CreateLabel(product.Name + " " + product.Price + " kr ");
            addProductGrid.Children.Add(productNameLabel);
            Grid.SetRow(productNameLabel, 0);
            Grid.SetColumn(productNameLabel, 1);

            //A readonly textbox so the user can't change the value in the textbox, as an errorhandling
            TextBox quantityTextBox = new TextBox
            {
                Text = quantity.ToString(),
                IsReadOnly = true,
                Margin = new Thickness(5),
                Padding = new Thickness(5)
            };
            addProductGrid.Children.Add(quantityTextBox);
            Grid.SetRow(quantityTextBox, 0);
            Grid.SetColumn(quantityTextBox, 2);

            Button plusButton = CreateButton(plus);
            addProductGrid.Children.Add(plusButton);
            Grid.SetRow(plusButton, 0);
            Grid.SetColumn(plusButton, 3);

            Button minusButton = CreateButton(minus);
            addProductGrid.Children.Add(minusButton);
            Grid.SetRow(minusButton, 0);
            Grid.SetColumn(minusButton, 4);

            Button deleteButton = CreateButton(delete, product);
            addProductGrid.Children.Add(deleteButton);
            Grid.SetRow(deleteButton, 0);
            Grid.SetColumn(deleteButton, 5);

            deleteButton.Click += DeleteButton_Click;

            return addProductGrid;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            Product addedProduct = (Product)clickedButton.Tag;

            Cart.Remove(addedProduct);
            DrawCart();
        }

        public static Label CreateLabel(string header)
        {
            Label label = new Label
            {
                Content = header,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                //Margin = new Thickness(5, 0, 0, 0),
                Padding = new Thickness(5)
            };
            return label;
        }
        public static TextBlock CreateTextBlock(string header)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = header,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5),
                FontFamily = new FontFamily("Constantia"),
                FontSize = 16,
                TextAlignment = TextAlignment.Center
            };
            return textBlock;
        }
        public static Button CreateButton(string header, Product tag=null)
        {
            Button button = new Button
            {
                Content = header,
                Margin = new Thickness(5),
                Padding = new Thickness(5),
                Tag = tag
            };
            return button;
        }
    }
}
