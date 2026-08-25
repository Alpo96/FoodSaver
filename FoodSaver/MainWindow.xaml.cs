using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace FoodSaver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private shopping_cart shopping_Cart;
        private MainViewModel mainViewModel;
        private ShoppingHistory ShoppingHistory = new ShoppingHistory();
        LinqToSqlDataContext DataCon = new LinqToSqlDataContext();
        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = ((App)Application.Current).SharedMainViewModel;
            shopping_Cart = new shopping_cart(this, mainViewModel);
            ShoppingHistory.Reference(this);    //végtelen példányosítás kikerülése.
            CoinText.Text = mainViewModel.Coin.ToString();
            DataContext = mainViewModel;
            OnceGetCoin();
            GetData();
        }

        public void GetData()
        {
            try
            {
                //string Kereses = "SELECT FoodShopRelation.Id, ShopLocation.Location, Food.Name, ImagesTable.ImagePath, ValueTable.FoodValue FROM FoodShopRelation INNER JOIN ShopLocation ON FoodShopRelation.ShopLocationID = ShopLocation.Id INNER JOIN Food ON FoodShopRelation.FoodID = Food.Id INNER JOIN ImagesTable ON FoodShopRelation.ImagesTableID = ImagesTable.Id INNER JOIN ValueTable ON FoodShopRelation.ValueTableID = ValueTable.Id";
                var query = from foodshoprelation in DataCon.FoodShopRelations
                            join location in DataCon.ShopLocations on foodshoprelation.ShopLocationID equals location.Id
                            join food in DataCon.Foods on foodshoprelation.FoodID equals food.Id
                            join image in DataCon.ImagesTables on foodshoprelation.ImagesTableID equals image.Id
                            join value in DataCon.ValueTables on foodshoprelation.ValueTableID equals value.Id
                            join quality in DataCon.FoodQualities on foodshoprelation.FoodQualityID equals quality.Id
                            select new FoodShopRelationInfo
                            {
                                Id = foodshoprelation.Id,
                                Location = location.Location,
                                Name = food.Name,
                                ImagePath = image.ImagePath,
                                FoodValue = value.FoodValue,
                                Quality = quality.Quality
                            };
                DTGrid.ItemsSource = query.ToList();

                UpdateCoin();
                ShoppingCartImageChange();
                ShoppingHistory.GetData();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateCoin()
        {
            try
            {
                var UserCoinQuery2 = DataCon.LoginTables.FirstOrDefault(item => item.Username == FistLogin.Username);

                if (UserCoinQuery2 != null)
                {
                    UserCoinQuery2.UserCoin = mainViewModel.Coin;
                    DataCon.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private async void Buy(object sender, RoutedEventArgs e)
        {
            BuyButton.Source = new BitmapImage(new Uri("Images/buybuttonup_640.png", UriKind.Relative));
            await Task.Delay(100);

            try
            {
                if (DTGrid.SelectedItem != null)
                {
                    FoodShopRelationInfo selectedRow = DTGrid.SelectedItem as FoodShopRelationInfo;
                    if (selectedRow != null)
                    {
                        int id = selectedRow.Id;
                        var foodValueQuery = from foodshoprelation in DataCon.FoodShopRelations
                                             join valueTable in DataCon.ValueTables on foodshoprelation.ValueTableID equals valueTable.Id
                                             where foodshoprelation.Id == id
                                             select valueTable.FoodValue;

                        int searchValue = 0;
                        await Task.Run( () =>
                        {
                            int Value = foodValueQuery.FirstOrDefault();
                            if (Value != 0)
                            {
                                searchValue = Value;
                                Debug.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " For select FoodValue");
                            }
                        });

                        if (searchValue <= mainViewModel.Coin)
                        {
                            mainViewModel.Coin -= searchValue;
                            //UpdateUserCoin(searchValue);
                            ShoppingCart.Visibility = Visibility.Visible;

                            FoodShopRelation InsertRow = DataCon.FoodShopRelations.FirstOrDefault(item => item.Id == id);

                            var UserNameIDQueary = from logintable in DataCon.LoginTables
                                             where logintable.Username == FistLogin.Username
                                             select logintable.Id;
                            int UserID = 0;
                            if (InsertRow != null)
                            {
                                await Task.Run( () =>
                                {
                                    UserID = UserNameIDQueary.FirstOrDefault();
                                    FoodShopRelationBuy foodShopRelationBuy = new FoodShopRelationBuy
                                    {
                                        FoodID = InsertRow.FoodID,
                                        ShopLocationID = InsertRow.ShopLocationID,
                                        ImagesTableID = InsertRow.ImagesTableID,
                                        ValueTableID = InsertRow.ValueTableID,
                                        FoodQualityID = InsertRow.FoodQualityID,
                                        LoginTableID = UserID
                                    };

                                    DataCon.FoodShopRelationBuys.InsertOnSubmit(foodShopRelationBuy);
                                    DataCon.SubmitChanges();
                                    Debug.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " For INSERT INTO FoodShopRelationBuy");
                                });

                                await Task.Run( () =>
                                {
                                    History(InsertRow, UserID);
                                    if (InsertRow != null)
                                    {
                                        DataCon.FoodShopRelations.DeleteOnSubmit(InsertRow);
                                        DataCon.SubmitChanges();
                                        Debug.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " For DELETE FROM FoodShopRelation");
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            if (shopping_Cart != null)
                                            {
                                                shopping_Cart.GetData();
                                            }
                                            GetData();
                                        });                                       
                                    }
                                    else
                                    {
                                        MessageBox.Show("Failed to delete!");
                                    }
                                });
                                
                            }
                            else
                            {
                                MessageBox.Show("Failed to Insert!");
                            }
                            
                        }
                        else
                        {
                            MessageBox.Show("Not enought money for it!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void History(FoodShopRelation InsertRow, int UserID)
        {
            try
            {
                if (InsertRow != null)
                {
                    History history = new History
                    {
                        FoodID = InsertRow.FoodID,
                        ShopLocationID = InsertRow.ShopLocationID,
                        ImagesTableID = InsertRow.ImagesTableID,
                        ValueTableID = InsertRow.ValueTableID,
                        FoodQualityID = InsertRow.FoodQualityID,
                        LoginTableID = UserID,
                        Refund = "Buy",
                        Time = DateTime.Now
                    };
                    DataCon.Histories.InsertOnSubmit(history);
                    DataCon.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /*
        public void UpdateUserCoin(int searchValue)
        {
            try
            {
                var UserCoinQuery = DataCon.LoginTables.FirstOrDefault(item => item.Username == FistLogin.Username);

                if (UserCoinQuery != null)
                {
                    DataCon.Refresh(RefreshMode.KeepCurrentValues, UserCoinQuery);
                    UserCoinQuery.UserCoin -= searchValue;
                    DataCon.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        */
        private void ShoppingCart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (shopping_Cart != null && !shopping_Cart.IsVisible)
            {
                shopping_Cart.Visibility = Visibility.Visible;
                shopping_Cart.Show();
                shopping_Cart.SpentCoinText.Text = mainViewModel.SpentCoin.ToString();
            }
        }

        private void DTGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                if (DTGrid.SelectedItem != null)
                {
                    FoodShopRelationInfo selectedRow = DTGrid.SelectedItem as FoodShopRelationInfo;
                    if (selectedRow != null)
                    {
                        int id = selectedRow.Id;
                        var foodValueQuery = from foodshoprelation in DataCon.FoodShopRelations
                                             join valueTable in DataCon.ValueTables on foodshoprelation.ValueTableID equals valueTable.Id
                                             where foodshoprelation.Id == id
                                             select valueTable.FoodValue;

                        int foodvalue = foodValueQuery.FirstOrDefault();

                        if (foodvalue != 0)
                        {
                            if (foodvalue <= mainViewModel.Coin)
                            {
                                BuyButton.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                BuyButton.Visibility = Visibility.Hidden;
                                MessageBox.Show("You don't have enought money for this: " + selectedRow.Name);
                            }
                        }
                    }
                }
                else
                {
                    BuyButton.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ShoppingCartImageChange()
        {
            Dictionary<int, String> ShoppingcartImages = new Dictionary<int, String>();
            ShoppingcartImages.Add(0, "Images/shoppingcart0_1280.png");
            ShoppingcartImages.Add(1, "Images/shoppingcart1_1280.png");
            ShoppingcartImages.Add(2, "Images/shoppingcart2_1280.png");
            ShoppingcartImages.Add(3, "Images/shoppingcart3_1280.png");

            int ListNumber = shopping_Cart.DTGrid.Items.Count;

            if (ListNumber < ShoppingcartImages.Count)
            {
                Uri Uri = new Uri(ShoppingcartImages[ListNumber], UriKind.Relative);
                ImageSource imageSource = new BitmapImage(Uri);
                ShoppingCart.Source = imageSource;
            }
            else
            {
                Uri Uri = new Uri(ShoppingcartImages[3], UriKind.Relative);
                ImageSource imageSource = new BitmapImage(Uri);
                ShoppingCart.Source = imageSource;
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

        private void ImageLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BuyButton.Source = new BitmapImage(new Uri("Images/buybuttondown_640.png", UriKind.Relative));
        }

        private void GoHistory_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ShoppingHistory != null && !ShoppingHistory.IsVisible)
            {
                ShoppingHistory.Visibility = Visibility.Visible;
                ShoppingHistory.Show();

                Uri Uri = new Uri("Images/historyicon_640.png", UriKind.Relative);
                ImageSource imageSource = new BitmapImage(Uri);
                GoHistory.Source = imageSource;
            }
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {

        private int coin;
        public int Coin
        {
            get { return coin; }
            set
            {
                coin = value;
                OnPropertyChanged(nameof(Coin));
            }
        }

        private int spentcoin;
        public int SpentCoin
        {
            get { return spentcoin; }
            set
            {
                spentcoin = value;
                OnPropertyChanged(nameof(SpentCoin));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
