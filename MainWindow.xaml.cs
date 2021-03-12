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

        public const string CartFilePath = @"C:\Windows\Temp\Cart.csv";

        public static Dictionary<Product, int> Cart = new Dictionary<Product, int>();
        //public const string CartFilePath = @"C:\Windows\Temp\Cart.csv";

        //Instance variables to be reached and changed in all methods in this class
        private StackPanel cartStack;
        private StackPanel receiptProductPanel;
        private TextBox rebateCodeTextBox;
        private Label sumPurchaseLabel;
        private decimal rebateMultiplier;
        private Rebate currentRebateCode;

        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        private void Start()
        {
            //Loads the products from the CSV-file
            Products = LoadProducts();
            //Loads the rebates from the CSV-file
            Rebates = LoadRebates();
            //Loads the cart at startup if it exists
            //Cart = LoadCart();
            rebateMultiplier = 1;
            currentRebateCode = null;

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

            foreach (Product product in Products)
            {
                wrap.Children.Add(CreateProductPanel(product));
            }

            cartStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5)
            };

            mainStack.Children.Add(cartStack);
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

            saveCartButton.Click += SaveCartButton_Click;

            Button emptyCartButton = CreateButton("Töm kundvagnen");
            cartGrid.Children.Add(emptyCartButton);
            Grid.SetRow(emptyCartButton, 0);
            Grid.SetColumn(emptyCartButton, 1);

            emptyCartButton.Click += EmptyCartButton_Click;

            Button confirmPurchaseButton = CreateButton("Avsluta köp");
            cartGrid.Children.Add(confirmPurchaseButton);
            Grid.SetRow(confirmPurchaseButton, 0);
            Grid.SetColumn(confirmPurchaseButton, 2);

            confirmPurchaseButton.Click += ConfirmPurchaseButton_Click;

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
            sumPurchaseLabel = CreateLabel("Summa: " + Math.Round(TotalSumWithRebate(), 2) + " kr");
            sumPurchaseLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
            sumPurchaseGrid.Children.Add(sumPurchaseLabel);
            Grid.SetRow(sumPurchaseLabel, 0);
            Grid.SetColumn(sumPurchaseLabel, 0);

            //#################################################
            // KVITTO
            // ################################################
            receiptProductPanel = new StackPanel
            {
                Orientation = Orientation.Vertical

            };
            mainStack.Children.Add(receiptProductPanel);

        }

        private void ConfirmPurchaseButton_Click(object sender, RoutedEventArgs e)
        {   
            if(receiptProductPanel.Children.Count > 0)
            {
                receiptProductPanel.Children.Clear();
            }
            DrawReceipt();
            Cart.Clear();
            DrawCart();
        }

        private void DrawReceipt()
        {
            Grid.SetRow(receiptProductPanel, 0);
            Grid.SetColumn(receiptProductPanel, 0);

            #region Header Grid
            Grid receiptHeaderGrid = new Grid();
            receiptHeaderGrid.RowDefinitions.Add(new RowDefinition());
            receiptHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            receiptHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            receiptHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            receiptHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            receiptProductPanel.Children.Add(receiptHeaderGrid);

            Label receiptNameLabel = CreateBorderdLabel("Produkt");
            receiptHeaderGrid.Children.Add(receiptNameLabel);
            Grid.SetRow(receiptNameLabel, 0);
            Grid.SetColumn(receiptNameLabel, 0);
            Label receiptDescriptionLabel = CreateBorderdLabel("Antal");
            receiptHeaderGrid.Children.Add(receiptDescriptionLabel);
            Grid.SetRow(receiptDescriptionLabel, 0);
            Grid.SetColumn(receiptDescriptionLabel, 1);
            Label receiptPriceLabel = CreateBorderdLabel("Styckpris");
            receiptHeaderGrid.Children.Add(receiptPriceLabel);
            Grid.SetRow(receiptPriceLabel, 0);
            Grid.SetColumn(receiptPriceLabel, 2);
            Label receiptTotPriceLabel = CreateBorderdLabel("TotalPris");
            receiptHeaderGrid.Children.Add(receiptTotPriceLabel);
            Grid.SetRow(receiptTotPriceLabel, 0);
            Grid.SetColumn(receiptTotPriceLabel, 3);
            #endregion Header

            foreach (KeyValuePair<Product, int> entry in Cart)
            {
                string productName = entry.Key.Name;
                string productPrice = entry.Key.Price;
                int quantity = entry.Value;
                receiptProductPanel.Children.Add(creatReceiptProductGrid(productName, quantity, int.Parse(productPrice)));
            }

            #region Receipt Sum

            Grid receiptSumGrid = new Grid();
            receiptSumGrid.Margin = new Thickness(0, 5, 0, 0);
            receiptSumGrid.RowDefinitions.Add(new RowDefinition());
            receiptSumGrid.RowDefinitions.Add(new RowDefinition());
            receiptSumGrid.RowDefinitions.Add(new RowDefinition());
            receiptSumGrid.RowDefinitions.Add(new RowDefinition());
            receiptSumGrid.ColumnDefinitions.Add(new ColumnDefinition());
            receiptSumGrid.ColumnDefinitions.Add(new ColumnDefinition());
            receiptProductPanel.Children.Add(receiptSumGrid);

            #region Receipt Colum 0
            Label recepitRebateCodeLabel = CreateBorderdLabel("Rabattkod");
            receiptSumGrid.Children.Add(recepitRebateCodeLabel);
            Grid.SetRow(recepitRebateCodeLabel, 0);
            Grid.SetColumn(recepitRebateCodeLabel, 0);
            Label receiptSumLabel = CreateBorderdLabel("Summa");
            receiptSumGrid.Children.Add(receiptSumLabel);
            Grid.SetRow(receiptSumLabel, 1);
            Grid.SetColumn(receiptSumLabel, 0);
            Label rebateCodeLabel = CreateBorderdLabel("Rabatt");
            receiptSumGrid.Children.Add(rebateCodeLabel);
            Grid.SetRow(rebateCodeLabel, 2);
            Grid.SetColumn(rebateCodeLabel, 0);
            Label receiptRebateSumLabel = CreateBorderdLabel("Summa efter rabatt");
            receiptSumGrid.Children.Add(receiptRebateSumLabel);
            Grid.SetRow(receiptRebateSumLabel, 3);
            Grid.SetColumn(receiptRebateSumLabel, 0);
            #endregion Receipt Colum 0

            #region Receipt Colum 1

            decimal sumTotal = TotalSum();
            decimal sumTotalWithRebate = TotalSumWithRebate();
            decimal rebateAmount = sumTotal - sumTotalWithRebate;
            string rebateCodePrecent = "0";
            //Label recepitRebateCodeLabel = CreateBorderdLabel(rebateCodeTextBox.Text);
            Label recepitRebateCode = new Label();
            Label rebateCode = new Label();
            if (currentRebateCode != null)
            {
                recepitRebateCode = CreateBorderdLabel(currentRebateCode.Code);
                rebateCodePrecent = currentRebateCode.Percent;
            }
            else
            {
                recepitRebateCode = CreateBorderdLabel("Ingen rabattkod");
            }
            receiptSumGrid.Children.Add(recepitRebateCode);
            Grid.SetRow(recepitRebateCode, 0);
            Grid.SetColumn(recepitRebateCode, 1);
            //Vet inte hur jag skulle göra på denna så alla efter kommer att utgå från denna
            Label receiptSum = CreateBorderdLabel(Math.Round(sumTotal,2) + " kr");
            receiptSumGrid.Children.Add(receiptSum);
            Grid.SetRow(receiptSum, 1);
            Grid.SetColumn(receiptSum, 1);
            //Tog 20% som ett exempel
            rebateCode = CreateBorderdLabel(rebateAmount + " kr (" + rebateCodePrecent + " %)");
            receiptSumGrid.Children.Add(rebateCode);
            Grid.SetRow(rebateCode, 2);
            Grid.SetColumn(rebateCode, 1);
            Label receiptRebateSum = CreateBorderdLabel(Math.Round(sumTotalWithRebate,2) + " kr");
            receiptSumGrid.Children.Add(receiptRebateSum);
            Grid.SetRow(receiptRebateSum, 3);
            Grid.SetColumn(receiptRebateSum, 1);
            #endregion Receipt Colum 1
            #endregion Receipt Sum
        }

        public static Grid creatReceiptProductGrid(string productName, int quantity, int price)
        {
            Grid receiptProductGrid = new Grid();
            receiptProductGrid.RowDefinitions.Add(new RowDefinition());
            receiptProductGrid.ColumnDefinitions.Add(new ColumnDefinition());
            receiptProductGrid.ColumnDefinitions.Add(new ColumnDefinition());
            receiptProductGrid.ColumnDefinitions.Add(new ColumnDefinition());
            receiptProductGrid.ColumnDefinitions.Add(new ColumnDefinition());

            Label productNameLabel = CreateBorderdLabel(productName);
            receiptProductGrid.Children.Add(productNameLabel);
            Grid.SetRow(productNameLabel, 0);
            Grid.SetColumn(productNameLabel, 0);

            Label receiptquantityLabel = CreateBorderdLabel(quantity.ToString());
            receiptProductGrid.Children.Add(receiptquantityLabel);
            Grid.SetRow(receiptquantityLabel, 0);
            Grid.SetColumn(receiptquantityLabel, 1);

            Label receiptPriceLabel = CreateBorderdLabel(price.ToString());
            receiptProductGrid.Children.Add(receiptPriceLabel);
            Grid.SetRow(receiptPriceLabel, 0);
            Grid.SetColumn(receiptPriceLabel, 2);

            Label recepitTotPriceLabel = CreateBorderdLabel((price * quantity).ToString());
            receiptProductGrid.Children.Add(recepitTotPriceLabel);
            Grid.SetRow(recepitTotPriceLabel, 0);
            Grid.SetColumn(recepitTotPriceLabel, 3);


            return receiptProductGrid;
        }
        public static Label CreateBorderdLabel(string header)
        {
            Label label = new Label
            {
                Content = header,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(2),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };
            return label;
        }


        private void SaveCartButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> cartLines = new List<string>();
            
            foreach(KeyValuePair<Product, int> entry in Cart)
            {
                Product product = entry.Key;
                int quantity = entry.Value;
                string line = product.Name + "," + product.Description + "," + product.Price + "," + quantity.ToString();
                cartLines.Add(line);
            }

            File.WriteAllLines(CartFilePath, cartLines);
            MessageBox.Show("Varukorg har sparats!");
        }

        //Empties and clears the cart dictionary and the cart GUi
        private void EmptyCartButton_Click(object sender, RoutedEventArgs e)
        {
            Cart.Clear();
            DrawCart();
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
                        currentRebateCode = r;
                    }
                }

                if (codeIsValid)
                {
                    sumAfterRebate = TotalSumWithRebate();
                    DrawSumLabel();
                }
                else
                {
                    MessageBox.Show("Fel kod!");
                    currentRebateCode = null;
                    DrawSumLabel();
                }
            }
            else
            {
                MessageBox.Show("Fel antal tecken!");
                currentRebateCode = null;
                DrawSumLabel();
            }   
        }

        private void DrawSumLabel()
        {
            decimal sum = TotalSumWithRebate();
            if (currentRebateCode != null)
            {
                sumPurchaseLabel.Content = "Giltig rabattkod: " + currentRebateCode.Code + " Summa: " + Math.Round(sum, 2) + " kr";
            }
            else
            {
                sumPurchaseLabel.Content = "Summa: " + Math.Round(sum, 2) + " kr";
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
                    Product product = new Product
                    {
                        Name = parts[0],
                        Description = parts[1],
                        Price = parts[2],
                        Path = parts[3]
                    };
                    products.Add(product);
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
                    Rebate rebate = new Rebate
                    {
                        //ToUpper to make the codes caseinsensitive
                        Code = parts[0].ToUpper(),
                        Percent = parts [1]
                    };
                    rebates.Add(rebate);
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

        //public static void SaveCart()
        //{
        //    // Create an empty list of text lines that we will fill with strings and then write to a textfile using `WriteAllLines`.
        //    List<string> lines = new List<string>();
        //    foreach (KeyValuePair<Product, int> pair in Cart)
        //    {
        //        Product p = pair.Key;
        //        int amount = pair.Value;

        //        // For each product, we only save the code and the amount.
        //        // The other info (name, price, description) is already in "Products.csv" and we can look it up when we load the cart.
        //        lines.Add(p.Code + "," + amount);
        //    }
        //    File.WriteAllLines(CartFilePath, lines);

        //    Console.WriteLine("Din varukorg har sparats: ");
        //    Console.WriteLine();
        //    Console.WriteLine(CartToString());
        //}

        //Creates the panel for a productobject
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
        }

        //Draws the cart and the sum
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
            
            if (sumPurchaseLabel != null) 
            {
                DrawSumLabel();
            }
        }
        
        //Creates the cart GUI and handles the button clicks
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

            Button plusButton = CreateButton(plus, product);
            addProductGrid.Children.Add(plusButton);
            Grid.SetRow(plusButton, 0);
            Grid.SetColumn(plusButton, 3);

            plusButton.Click += PlusButton_Click;

            Button minusButton = CreateButton(minus, product);
            addProductGrid.Children.Add(minusButton);
            Grid.SetRow(minusButton, 0);
            Grid.SetColumn(minusButton, 4);

            minusButton.Click += MinusButton_Click;

            Button deleteButton = CreateButton(delete, product);
            addProductGrid.Children.Add(deleteButton);
            Grid.SetRow(deleteButton, 0);
            Grid.SetColumn(deleteButton, 5);

            deleteButton.Click += DeleteButton_Click;

            return addProductGrid;
        }

        private decimal TotalSum()
        {
            decimal sum = 0;

            foreach (KeyValuePair<Product, int> entry in Cart)
            {
                decimal productPrice = decimal.Parse(entry.Key.Price);
                int quantity = entry.Value;
                sum += productPrice * quantity;
            }

            return sum;
        }

        private decimal TotalSumWithRebate()
        {
            decimal sum = 0;

            foreach (KeyValuePair<Product, int> entry in Cart)
            {
                decimal productPrice = decimal.Parse(entry.Key.Price);
                int quantity = entry.Value;
                sum += productPrice * quantity;
            }
            if (currentRebateCode != null)
            {
                rebateMultiplier = (100 - decimal.Parse(currentRebateCode.Percent)) / 100;
                sum = sum * rebateMultiplier;
            }

            return sum;
        }

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            Product addedProduct = (Product)clickedButton.Tag;

            if (Cart[addedProduct] > 0)
            {
                Cart[addedProduct]--;
            }
            DrawCart();
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            Product addedProduct = (Product)clickedButton.Tag;

            Cart[addedProduct]++;
            DrawCart();
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

        //Creates a button and gives it a tag
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
