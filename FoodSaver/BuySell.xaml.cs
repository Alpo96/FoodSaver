using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace FoodSaver
{
    /// <summary>
    /// Interaction logic for BuySell.xaml
    /// </summary>
    public partial class BuySell : Window
    {
        SqlConnection sqlConnection = new SqlConnection();
        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
        SqlCommand sqlCommand = new SqlCommand();
        string TextString = "Please Enter the new line";
        LinqToSqlDataContext DataCon = new LinqToSqlDataContext();
        MainViewModel mainViewModel = new MainViewModel();
        public BuySell()
        {
            InitializeComponent();
            string connectionString = ConfigurationManager.ConnectionStrings["FoodSaver.Properties.Settings.FoodDBConnectionString"].ConnectionString;
            sqlConnection = new SqlConnection(connectionString);
            ShowShop();
            ShowProduct();
            ShowValue();
            //ShowFoodShopRelation();
            ShowImages();
            ShowQuality();
            OnceGetCoin();
        }

        public void OnceGetCoin()
        {
            try
            {
                var UserCoinQuery = from logintable in DataCon.LoginTables
                                    where logintable.Username == FistLogin.Username
                                    select logintable.UserCoin;

                int uservalue = UserCoinQuery.FirstOrDefault();

                if (uservalue != 0)
                {
                    mainViewModel.Coin = uservalue;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void CheckBoxShowImagesQuality(object sender, RoutedEventArgs e)
        {
            ShowImages();
            ShowQuality();
        }

        private void ShowShop()
        {
            try
            {
                var shopLocationQuery = from shoplocation in DataCon.ShopLocations
                                        select new ShopInfo
                                        {
                                            Id = shoplocation.Id,
                                            Location = shoplocation.Location
                                        };

                ListShop.DisplayMemberPath = "Location";
                ListShop.SelectedValuePath = "Id";
                ListShop.ItemsSource = shopLocationQuery.ToList();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowProduct()
        {
            try
            {
                var foodQuery = from food in DataCon.Foods
                                select new FoodInfo
                                {
                                    Id = food.Id,
                                    Name = food.Name
                                };

                ListProduct.DisplayMemberPath = "Name";
                ListProduct.SelectedValuePath = "Id";
                ListProduct.ItemsSource = foodQuery.ToList();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowValue()
        {
            try
            {
                var foodValueQuery = from foodvalue in DataCon.ValueTables
                                     select new ValueInfo
                                     {
                                         Id = foodvalue.Id,
                                         FoodValue = foodvalue.FoodValue
                                     };

                ListValue.DisplayMemberPath = "FoodValueCoin";
                ListValue.SelectedValuePath = "Id";
                ListValue.ItemsSource = foodValueQuery.ToList();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowFoodShopRelation()
        {
            try
            {
                string Kereses = "Select * from FoodShopRelation";
                sqlDataAdapter = new SqlDataAdapter(Kereses, sqlConnection);

                using (sqlDataAdapter)
                {
                    DataTable ShopTable = new DataTable();

                    sqlDataAdapter.Fill(ShopTable);

                    ListFoodShopRelation.DisplayMemberPath = "FoodID";
                    ListFoodShopRelation.ItemsSource = ShopTable.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowImages()
        {
            try
            {
                if (DamagedCheckBox.IsChecked == false)
                {
                    var imageQuery = from image in DataCon.ImagesTables
                                     where !image.ImagePath.Contains("Damaged")
                                     select new ImageInfo
                                     {
                                         Id = image.Id,
                                         ImagePath = image.ImagePath
                                     };
                    ListImages.SelectedValuePath = "Id";
                    ListImages.ItemsSource = imageQuery.ToList();
                }
                else
                {
                    var imageQuery = from image in DataCon.ImagesTables
                                     where image.ImagePath.Contains("Damaged")
                                     select new ImageInfo
                                     {
                                         Id = image.Id,
                                         ImagePath = image.ImagePath
                                     };
                    ListImages.SelectedValuePath = "Id";
                    ListImages.ItemsSource = imageQuery.ToList();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowQuality()
        {
            try
            {
                if (DamagedCheckBox.IsChecked == false)
                {
                    var foodQualityQuery = from quality in DataCon.FoodQualities
                                           where quality.Quality > 74
                                           select new QualityInfo
                                           {
                                               Id = quality.Id,
                                               Quality = quality.Quality
                                           };

                    ListQuality.DisplayMemberPath = "QualityPercent";
                    ListQuality.SelectedValuePath = "Id";
                    ListQuality.ItemsSource = foodQualityQuery.ToList();
                }
                else
                {
                    var foodQualityQuery = from quality in DataCon.FoodQualities
                                           where quality.Quality <= 74
                                           select new QualityInfo
                                           {
                                               Id = quality.Id,
                                               Quality = quality.Quality
                                           };

                    ListQuality.DisplayMemberPath = "QualityPercent";
                    ListQuality.SelectedValuePath = "Id";
                    ListQuality.ItemsSource = foodQualityQuery.ToList();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddItemToFoodShopRelation(object sender, RoutedEventArgs e)
        {
            Upload.Source = new BitmapImage(new Uri("Images/uploadbuttonup_640.png", UriKind.Relative));
            await Task.Delay(100);
            try
            {
                //string Kereses = "INSERT INTO FoodShopRelation (ImagesTableID, ShopLocationID, FoodID, ValueTableID) VALUES (@ImagesTableId, @ShopLocationId, @FoodId, @ValueTableId)";
                ImageInfo selectedImage = ListImages.SelectedItem as ImageInfo;
                ShopInfo selectedShop = ListShop.SelectedItem as ShopInfo;
                FoodInfo selectedProduct = ListProduct.SelectedItem as FoodInfo;
                ValueInfo selectedValue = ListValue.SelectedItem as ValueInfo;
                QualityInfo selectedQuality = ListQuality.SelectedItem as QualityInfo;

                if (selectedImage != null && selectedShop != null && selectedProduct != null && selectedValue != null && selectedQuality != null)
                {
                    FoodShopRelation foodShopRelation = new FoodShopRelation
                    {
                        FoodID = selectedProduct.Id,
                        ShopLocationID = selectedShop.Id,
                        ImagesTableID = selectedImage.Id,
                        ValueTableID = selectedValue.Id,
                        FoodQualityID = selectedQuality.Id
                    };

                    DataCon.FoodShopRelations.InsertOnSubmit(foodShopRelation);
                    DataCon.SubmitChanges();

                    mainViewModel.Coin += selectedValue.FoodValue;
                    UpdateUserCoin();
                    //ShowFoodShopRelation();
                    MessageBox.Show("Food is loaded successfully, and you earned " + selectedValue.FoodValue + " Coin!");
                    ListImages.SelectedValue = null;
                    ListShop.SelectedValue = null;
                    ListProduct.SelectedValue = null;
                    ListValue.SelectedValue = null;
                    ListQuality.SelectedValue = null;
                }
                else
                {
                    if (selectedImage == null)
                    {
                        MessageBox.Show("You have not selected Image!");
                    }
                    if (selectedShop == null)
                    {
                        MessageBox.Show("You have not selected Shop!");
                    }
                    if (selectedProduct == null)
                    {
                        MessageBox.Show("You have not selected Food!");
                    }
                    if (selectedValue == null)
                    {
                        MessageBox.Show("You have not selected Value!");
                    }
                    if (selectedQuality == null)
                    {
                        MessageBox.Show("You have not selected FoodQuality!");
                    }
                }
            }
            catch (Exception r)
            {
                MessageBox.Show(r.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateUserCoin()
        {
            try
            {
                var UserCoinQuery = DataCon.LoginTables.FirstOrDefault(item => item.Username == FistLogin.Username);

                if (UserCoinQuery != null)
                {
                    //UserCoinQuery.UserCoin += SelectedValue;
                    UserCoinQuery.UserCoin = mainViewModel.Coin;
                    DataCon.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddNewShop(object sender, RoutedEventArgs e)
        {
            AddShop.Source = new BitmapImage(new Uri("Images/shopbuttonup_640.png", UriKind.Relative));
            await Task.Delay(200);
            try
            {
                if (NewLine.Text != "" && NewLine.Text != TextString)
                {
                    bool InsertShop = DataCon.ShopLocations.Any(item => item.Location == NewLine.Text);
                    if (InsertShop == false)
                    {
                        ShopLocation shopLocation = new ShopLocation
                        {
                            Location = NewLine.Text
                        };

                        DataCon.ShopLocations.InsertOnSubmit(shopLocation);
                        DataCon.SubmitChanges();
                        ShowShop();
                    }
                    else
                    {
                        MessageBox.Show("The location is already listed!");
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a shop!");
                }

                NewLine.Text = "";
            }
            catch (Exception r)
            {
                MessageBox.Show(r.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddNewProduct(object sender, RoutedEventArgs e)
        {
            AddProduct.Source = new BitmapImage(new Uri("Images/productbuttonup_640.png", UriKind.Relative));
            await Task.Delay(200);
            try
            {
                if (NewLine.Text != "" && NewLine.Text != TextString)
                {
                    bool InsertFood = DataCon.Foods.Any(item => item.Name == NewLine.Text);
                    if (InsertFood == false)
                    {
                        Food food = new Food
                        {
                            Name = NewLine.Text
                        };
                        DataCon.Foods.InsertOnSubmit(food);
                        DataCon.SubmitChanges();
                        ShowProduct();
                    }
                    else
                    {
                        MessageBox.Show("The Food is already listed!");
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a food!");
                }
                NewLine.Text = "";
            }
            catch (Exception r)
            {
                MessageBox.Show(r.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddNewValue(object sender, RoutedEventArgs e)
        {
            AddValue.Source = new BitmapImage(new Uri("Images/valuebuttonup_640.png", UriKind.Relative));
            await Task.Delay(200);
            try
            {
                if (NewLine.Text != "" && NewLine.Text != TextString)
                {
                    bool InsertFoodValue = DataCon.ValueTables.Any(item => item.FoodValue.Equals(Convert.ToInt32(NewLine.Text)));
                    if (InsertFoodValue == false)
                    {
                        ValueTable value = new ValueTable
                        {
                            FoodValue = (Convert.ToInt32(NewLine.Text))
                        };
                        DataCon.ValueTables.InsertOnSubmit(value);
                        DataCon.SubmitChanges();
                        ShowValue();
                    }
                    else
                    {
                        MessageBox.Show("The price of the food is already listed!");
                    }

                }
                else
                {
                    MessageBox.Show("Please enter a food value!");
                }

                NewLine.Text = "";
            }
            catch (Exception r)
            {
                MessageBox.Show(r.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddNewImage(object sender, RoutedEventArgs e)
        {
            AddImage.Source = new BitmapImage(new Uri("Images/imagebuttonup_640.png", UriKind.Relative));
            await Task.Delay(200);
            try
            {
                if (NewLine.Text != "" && NewLine.Text != TextString)
                {
                    bool InsertImage = DataCon.ImagesTables.Any(item => item.ImagePath == NewLine.Text);
                    if (InsertImage == false)
                    {
                        if (DamagedCheckBox.IsChecked == false && !NewLine.Text.Contains("Damaged"))
                        {
                            ImagesTable Image = new ImagesTable
                            {
                                ImagePath = NewLine.Text
                            };
                            DataCon.ImagesTables.InsertOnSubmit(Image);
                            DataCon.SubmitChanges();
                            ShowImages();
                        }
                        else if (DamagedCheckBox.IsChecked == true && NewLine.Text.Contains("Damaged"))
                        {
                            ImagesTable Image = new ImagesTable
                            {
                                ImagePath = NewLine.Text
                            };
                            DataCon.ImagesTables.InsertOnSubmit(Image);
                            DataCon.SubmitChanges();
                            ShowImages();
                        }
                        else if (DamagedCheckBox.IsChecked == false && NewLine.Text.Contains("Damaged"))
                        {
                            MessageBox.Show("The quality of the food in the picture is too low, but the CheckBox is unchecked, please check it and try again!");
                        }
                        else if (DamagedCheckBox.IsChecked == true && !NewLine.Text.Contains("Damaged"))
                        {
                            MessageBox.Show("The quality of the food in the picture is too good, but the CheckBox is checked, please uncheck it and try again!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("The Image is already listed!");
                    }
                }
                else
                {
                    MessageBox.Show("Please enter an image!");
                }

                NewLine.Text = "";
            }
            catch (Exception r)
            {
                MessageBox.Show(r.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddNewQuality(object sender, MouseButtonEventArgs e)
        {
            AddQuality.Source = new BitmapImage(new Uri("Images/qualitybuttonup_640.png", UriKind.Relative));
            await Task.Delay(200);
            try
            {
                if (NewLine.Text != "" && NewLine.Text != TextString)
                {
                    bool InsertFoodQuality = DataCon.FoodQualities.Any(item => item.Quality.Equals(Convert.ToInt32(NewLine.Text)));

                    if ((74 < Convert.ToInt32(NewLine.Text) && Convert.ToInt32(NewLine.Text) <= 100) && DamagedCheckBox.IsChecked == false)
                    {
                        if (InsertFoodQuality == false)
                        {
                            FoodQuality quality = new FoodQuality
                            {
                                Quality = (Convert.ToInt32(NewLine.Text))
                            };
                            DataCon.FoodQualities.InsertOnSubmit(quality);
                            DataCon.SubmitChanges();
                            ShowQuality();
                        }
                        else
                        {
                            MessageBox.Show("The quality of the food is already listed!");
                        }
                    }
                    else if ((29 < Convert.ToInt32(NewLine.Text) && Convert.ToInt32(NewLine.Text) <= 74) && DamagedCheckBox.IsChecked == true)
                    {
                        if (InsertFoodQuality == false)
                        {
                            FoodQuality quality = new FoodQuality
                            {
                                Quality = (Convert.ToInt32(NewLine.Text))
                            };
                            DataCon.FoodQualities.InsertOnSubmit(quality);
                            DataCon.SubmitChanges();
                            ShowQuality();
                        }
                        else
                        {
                            MessageBox.Show("The quality of the food is already listed!");
                        }
                    }
                    else if ((74 < Convert.ToInt32(NewLine.Text) && Convert.ToInt32(NewLine.Text) <= 100) && DamagedCheckBox.IsChecked == true)
                    {
                        MessageBox.Show("The given high food quality number is: " + Convert.ToInt32(NewLine.Text) + "% (75% - 100%) but the CheckBox is checked, please uncheck it and try again!");
                    }
                    else if ((29 < Convert.ToInt32(NewLine.Text) && Convert.ToInt32(NewLine.Text) <= 74) && DamagedCheckBox.IsChecked == false)
                    {
                        MessageBox.Show("The given low food quality number is: " + Convert.ToInt32(NewLine.Text) + "% (30% - 74%) but the CheckBox is unchecked, please check it and try again!");
                    }
                    else
                    {
                        MessageBox.Show("The value of food quality number must be between 30% - 100%!");
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a quality value!");
                }

                NewLine.Text = "";
            }
            catch (Exception r)
            {
                MessageBox.Show(r.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BackButton.Source = new BitmapImage(new Uri("Images/buttondown_640.png", UriKind.Relative));
        }

        private async void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            BackButton.Source = new BitmapImage(new Uri("Images/buttonup_640.png", UriKind.Relative));
            await Task.Delay(500);
            App app = (App)Application.Current;
            app.ShowWindow(new Login());
        }

        private void AddImageButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddImage.Source = new BitmapImage(new Uri("Images/imagebuttondown_640.png", UriKind.Relative));
        }

        private void AddShopButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddShop.Source = new BitmapImage(new Uri("Images/shopbuttondown_640.png", UriKind.Relative));
        }

        private void AddProductButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddProduct.Source = new BitmapImage(new Uri("Images/productbuttondown_640.png", UriKind.Relative));
        }

        private void AddValueButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddValue.Source = new BitmapImage(new Uri("Images/valuebuttondown_640.png", UriKind.Relative));
        }

        private void UploadButtonDown(object sender, MouseButtonEventArgs e)
        {
            Upload.Source = new BitmapImage(new Uri("Images/uploadbuttondown_640.png", UriKind.Relative));
        }

        private void AddQualityButtonDown(object sender, MouseButtonEventArgs e)
        {
            AddQuality.Source = new BitmapImage(new Uri("Images/qualitybuttondown_640.png", UriKind.Relative));
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            NewLine.Text = "";
        }
    }
}
