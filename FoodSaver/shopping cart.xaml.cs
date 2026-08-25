using System;
using System.Collections.Generic;
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
    /// Interaction logic for shopping_cart.xaml
    /// </summary>
    public partial class shopping_cart : Window
    {
        private MainWindow mainWindow;
        private MainViewModel mainViewModel;
        DispatcherTimer timer1;
        TimeSpan timeSpan;
        private ShoppingHistory ShoppingHistory = new ShoppingHistory();
        LinqToSqlDataContext DataCon = new LinqToSqlDataContext();
        public int CurrentUserID = 0;
        public shopping_cart(MainWindow main, MainViewModel viewModel)
        {
            InitializeComponent();
            mainWindow = main;
            mainViewModel = viewModel;
            GetData();
            GetCurrentUserID();
            DataContext = mainViewModel;
            timer1 = new DispatcherTimer();
            DateTime now = DateTime.Now;
            DateTime when = new DateTime(now.Year, now.Month, now.Day, 22, 00, 00);
            timeSpan = when - now;
            timer1.Interval = TimeSpan.FromSeconds(1);
            timer1.Tick += timer_Update;
            if (timeSpan > TimeSpan.Zero)
            {
                timer1.Start();
            }
        }

        void timer_Update(object sender, EventArgs e)
        {
            if (DTGrid.Items.Count > 0)
            {
                tbTime.Visibility = Visibility.Visible;
                timeSpan = timeSpan.Add(TimeSpan.FromSeconds(-1));

                int hours = timeSpan.Hours;
                int minutes = timeSpan.Minutes;
                int seconds = timeSpan.Seconds;
                tbTime.Content = "Delivery time of the goods, until closing: " + hours.ToString("D2") + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2");

                if (timeSpan <= TimeSpan.Zero)
                {
                    tbTime.Content = "The time is up, " + "you lost: " + mainViewModel.SpentCoin + " Money!";
                    timer1.Stop();
                    try
                    {
                        var deleteRowsQuery = from foodshoprelationbuy in DataCon.FoodShopRelationBuys
                                              join logintable in DataCon.LoginTables on foodshoprelationbuy.LoginTableID equals logintable.Id
                                              where logintable.Username == FistLogin.Username
                                              select foodshoprelationbuy;
                        List<FoodShopRelationBuy> DeleteRow = deleteRowsQuery.ToList();
                        
                        DataCon.FoodShopRelationBuys.DeleteAllOnSubmit(DeleteRow);

                        if (DeleteRow != null)
                        {
                            DataCon.SubmitChanges();
                            GetData();
                            SpentMoney();
                            History_DeleteAll();
                            mainWindow.ShoppingCartImageChange();
                        }
                        else
                        {
                            MessageBox.Show("failed to delete!");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                tbTime.Visibility = Visibility.Hidden;
            }
        }

        public void GetCurrentUserID()
        {
            var CurrentUserIDQuery = from logintable in DataCon.LoginTables
                                     where logintable.Username == FistLogin.Username
                                     select logintable.Id;
            CurrentUserID = CurrentUserIDQuery.FirstOrDefault();
        }

        public void GetData()
        {
            try
            {
                var query = from foodshoprelationbuy in DataCon.FoodShopRelationBuys
                            join location in DataCon.ShopLocations on foodshoprelationbuy.ShopLocationID equals location.Id
                            join food in DataCon.Foods on foodshoprelationbuy.FoodID equals food.Id
                            join image in DataCon.ImagesTables on foodshoprelationbuy.ImagesTableID equals image.Id
                            join value in DataCon.ValueTables on foodshoprelationbuy.ValueTableID equals value.Id
                            join quality in DataCon.FoodQualities on foodshoprelationbuy.FoodQualityID equals quality.Id
                            join logintable in DataCon.LoginTables on foodshoprelationbuy.LoginTableID equals logintable.Id
                            where logintable.Username == FistLogin.Username
                            select new FoodShopRelationInfo
                            {
                                Id = foodshoprelationbuy.Id,
                                Location = location.Location,
                                Name = food.Name,
                                ImagePath = image.ImagePath,
                                FoodValue = value.FoodValue,
                                Quality = quality.Quality
                            };
                DTGrid.ItemsSource = query.ToList();
                SpentMoney();
                ShoppingHistory.GetData();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteAll_Order(object sender, RoutedEventArgs e)
        {
            DeleteAll.Source = new BitmapImage(new Uri("Images/deleteallbuttonup_640.png", UriKind.Relative));
            await Task.Delay(200);
            try
            {
                if (DTGrid.Items.Count > 0)
                {
                    //List<FoodShopRelationBuy> DeleteRow = DataCon.FoodShopRelationBuys.Where(item => item.LoginTableID == CurrentUserID).ToList();     Method syntax  
                    var deleteRowsQuery = from foodshoprelationbuy in DataCon.FoodShopRelationBuys                                                      //Query syntax
                                          join logintable in DataCon.LoginTables on foodshoprelationbuy.LoginTableID equals logintable.Id
                                          where logintable.Username == FistLogin.Username
                                          select foodshoprelationbuy;
                    List<FoodShopRelationBuy> DeleteRow = deleteRowsQuery.ToList();
                    History_DeleteAll();
                    
                    DataCon.FoodShopRelationBuys.DeleteAllOnSubmit(DeleteRow);

                    if (DeleteRow != null)
                    {
                        DataCon.SubmitChanges();
                        GetData();
                        mainWindow.ShoppingCartImageChange();
                    }
                    else
                    {
                        MessageBox.Show("failed to delete!");
                    }
                }
                else
                {
                    MessageBox.Show("Your shopping cart is empty!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void History_DeleteAll()
        {
            try
            {
                List<History> historyUpdate = DataCon.Histories.Where(history => history.LoginTableID == CurrentUserID && history.Refund == "Buy").ToList();

                foreach (History item in historyUpdate)
                {
                    item.Refund = "Delete";
                    item.Time = DateTime.Now;
                    DataCon.SubmitChanges();
                }
                mainWindow.GetData();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Cancel_Order(object sender, RoutedEventArgs e)
        {
            Cancel.Source = new BitmapImage(new Uri("Images/cancelproductbuttonup_640.png", UriKind.Relative));
            await Task.Delay(100);
            try
            {
                if (DTGrid.SelectedItem != null)
                {
                    FoodShopRelationInfo selectedRow = DTGrid.SelectedItem as FoodShopRelationInfo;
                    if (selectedRow != null)
                    {
                        //string Search = "SELECT ValueTable.FoodValue FROM FoodShopRelationBuy INNER JOIN ValueTable ON FoodShopRelationBuy.ValueTableID = ValueTable.Id WHERE FoodShopRelationBuy.Id = @Id";
                        int id = selectedRow.Id;
                        var foodValueQuery = from foodshoprelationbuy in DataCon.FoodShopRelationBuys
                                             join valueTable in DataCon.ValueTables on foodshoprelationbuy.ValueTableID equals valueTable.Id
                                             where foodshoprelationbuy.Id == id
                                             select new ValueInfo
                                             {
                                                 FoodValue = valueTable.FoodValue
                                             };
                        int SearchValue = 0;

                        await Task.Run(() =>
                       {
                           ValueInfo Value = foodValueQuery.FirstOrDefault();
                           if (Value != null)
                           {
                               SearchValue = Value.FoodValue;
                               Debug.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " For select FoodValue");
                           }
                       });

                        if (SearchValue > 0)
                        {
                            mainViewModel.Coin += SearchValue;
                            //UpdateUserCoin1(SearchValue);

                            FoodShopRelationBuy RefundRow = DataCon.FoodShopRelationBuys.FirstOrDefault(item => item.Id == id);
                            if (RefundRow != null)
                            {
                                await Task.Run(() =>
                               {
                                   FoodShopRelation foodShopRelation = new FoodShopRelation
                                   {
                                       FoodID = RefundRow.FoodID,
                                       ShopLocationID = RefundRow.ShopLocationID,
                                       ImagesTableID = RefundRow.ImagesTableID,
                                       ValueTableID = RefundRow.ValueTableID,
                                       FoodQualityID = RefundRow.FoodQualityID
                                   };

                                   DataCon.FoodShopRelations.InsertOnSubmit(foodShopRelation);
                                   DataCon.SubmitChanges();
                                   Debug.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " For INSERT INTO FoodShopRelation");
                               });

                                await Task.Run(() =>
                               {
                                   if (RefundRow != null)
                                   {
                                       History_Refund(RefundRow);
                                       DataCon.FoodShopRelationBuys.DeleteOnSubmit(RefundRow);
                                       DataCon.SubmitChanges();
                                       Debug.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " For DELETE FROM FoodShopRelationBuy");
                                       Application.Current.Dispatcher.Invoke(() =>
                                       {
                                           GetData();
                                           mainWindow.GetData();
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
                            MessageBox.Show("error occurred with the item!");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("You have not selected anything!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void History_Refund(FoodShopRelationBuy RefundRow)
        {
            try
            {
                History historyUpdate = DataCon.Histories.FirstOrDefault(history => history.FoodID == RefundRow.FoodID
                && history.ShopLocationID == RefundRow.ShopLocationID && history.ImagesTableID == RefundRow.ImagesTableID
                && history.ValueTableID == RefundRow.ValueTableID && history.FoodQualityID == RefundRow.FoodQualityID 
                && history.LoginTableID == RefundRow.LoginTableID && history.Refund == "Buy");

                if (historyUpdate != null)
                {
                    historyUpdate.Refund = "Refund";
                    historyUpdate.Time = DateTime.Now;
                    DataCon.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /*
        public void UpdateUserCoin1(int SearchValue)
        {
            try
            {
                var UserCoinQuery = DataCon.LoginTables.FirstOrDefault(item => item.Username == FistLogin.Username);

                if (UserCoinQuery != null)
                {
                    DataCon.Refresh(RefreshMode.KeepCurrentValues, UserCoinQuery);
                    UserCoinQuery.UserCoin += SearchValue;
                    DataCon.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        */
        private async void Cancel_AllOrder(object sender, RoutedEventArgs e)
        {
            CancelAll.Source = new BitmapImage(new Uri("Images/cancelallbuttonup_640.png", UriKind.Relative));
            await Task.Delay(100);
            try
            {
                if (DTGrid != null && DTGrid.Items.Count > 0)
                {
                    int SearchValue = 0;

                    await Task.Run(() =>
                   {
                       var foodValueQuery = from foodshoprelationbuy in DataCon.FoodShopRelationBuys
                                            join valueTable in DataCon.ValueTables on foodshoprelationbuy.ValueTableID equals valueTable.Id
                                            join logintable in DataCon.LoginTables on foodshoprelationbuy.LoginTableID equals logintable.Id
                                            where logintable.Username == FistLogin.Username
                                            select valueTable.FoodValue;
                       SearchValue = foodValueQuery.Sum();
                       Debug.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " For select FoodValue");
                   });

                    if (SearchValue > 0)
                    {
                        mainViewModel.Coin += SearchValue;
                        //UpdateUserCoin2(SearchValue);
                        List<FoodShopRelationBuy> RefundAllRows = new List<FoodShopRelationBuy>();

                        await Task.Run(() =>
                       {
                           RefundAllRows = DataCon.FoodShopRelationBuys.Where(item => item.LoginTableID == CurrentUserID).ToList();

                            foreach (FoodShopRelationBuy item in RefundAllRows)
                           {
                               FoodShopRelation foodShopRelation = new FoodShopRelation
                               {
                                   FoodID = item.FoodID,
                                   ShopLocationID = item.ShopLocationID,
                                   ImagesTableID = item.ImagesTableID,
                                   ValueTableID = item.ValueTableID,
                                   FoodQualityID = item.FoodQualityID
                               };
                               DataCon.FoodShopRelations.InsertOnSubmit(foodShopRelation);
                           }
                           DataCon.SubmitChanges();
                           Debug.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " For INSERT INTO FoodShopRelation");
                       });

                        if (RefundAllRows.Count > 0)
                        {
                            await Task.Run(() =>
                           {
                               History_RefundAll();
                               
                               DataCon.FoodShopRelationBuys.DeleteAllOnSubmit(RefundAllRows);
                               Debug.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " For DELETE FROM FoodShopRelationBuy");
                               if (RefundAllRows != null)
                               {
                                   DataCon.SubmitChanges();
                                   Application.Current.Dispatcher.Invoke(() =>
                                   {
                                       GetData();
                                       mainWindow.GetData();
                                   });
                               }
                               else
                               {
                                   MessageBox.Show("failed to delete!");
                               }
                           });
                        }
                        else
                        {
                            MessageBox.Show("Failed to Instert!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("error occurred with the item!");
                    }
                }
                else
                {
                    MessageBox.Show("Your shopping cart is empty!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void History_RefundAll()
        {
            try
            {
                List<History> historyUpdate = DataCon.Histories.Where(history => history.LoginTableID == CurrentUserID && history.Refund == "Buy").ToList();

                foreach (History item in historyUpdate)
                {
                    item.Refund = "Refund";
                    item.Time = DateTime.Now;
                    DataCon.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /*
        public void UpdateUserCoin2(int SearchValue)
        {
            try
            {
                var UserCoinQuery = DataCon.LoginTables.FirstOrDefault(item => item.Username == FistLogin.Username);

                if (UserCoinQuery != null)
                {
                    DataCon.Refresh(RefreshMode.KeepCurrentValues, UserCoinQuery);
                    UserCoinQuery.UserCoin += SearchValue;
                    DataCon.SubmitChanges();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        */
        public void SpentMoney()
        {
            if (DTGrid.Items.Count > 0)
            {
                var foodValueQuery = from foodshoprelationbuy in DataCon.FoodShopRelationBuys
                                     join valueTable in DataCon.ValueTables on foodshoprelationbuy.ValueTableID equals valueTable.Id
                                     join logintable in DataCon.LoginTables on foodshoprelationbuy.LoginTableID equals logintable.Id
                                     where logintable.Username == FistLogin.Username
                                     select valueTable.FoodValue;
                mainViewModel.SpentCoin = foodValueQuery.Sum();
            }         
            else
            {
                mainViewModel.SpentCoin = 0;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void CancelProductButtonDown(object sender, MouseButtonEventArgs e)
        {
            Cancel.Source = new BitmapImage(new Uri("Images/cancelproductbuttondown_640.png", UriKind.Relative));
        }

        private void CancelAllProductButtonDown(object sender, MouseButtonEventArgs e)
        {
            CancelAll.Source = new BitmapImage(new Uri("Images/cancelallbuttondown_640.png", UriKind.Relative));
        }

        private void DeleteAllProductButtonDown(object sender, MouseButtonEventArgs e)
        {
            DeleteAll.Source = new BitmapImage(new Uri("Images/deleteallbuttondown_640.png", UriKind.Relative));
        }
    }
}
