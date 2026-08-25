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
using System.Windows.Shapes;

namespace FoodSaver
{
    /// <summary>
    /// Interaction logic for ShoppingHistory.xaml
    /// </summary>
    public partial class ShoppingHistory : Window
    {
        LinqToSqlDataContext DataCon = new LinqToSqlDataContext();
        public ShoppingHistory()
        {
            InitializeComponent();
            GetData();
        }

        public void GetData()
        {
            try
            {
                var query = from history in DataCon.Histories
                            join location in DataCon.ShopLocations on history.ShopLocationID equals location.Id
                            join food in DataCon.Foods on history.FoodID equals food.Id
                            join image in DataCon.ImagesTables on history.ImagesTableID equals image.Id
                            join value in DataCon.ValueTables on history.ValueTableID equals value.Id
                            join quality in DataCon.FoodQualities on history.FoodQualityID equals quality.Id
                            join logintable in DataCon.LoginTables on history.LoginTableID equals logintable.Id
                            select new HistoryInfo
                            {
                                Id = history.Id,
                                Location = location.Location,
                                Name = food.Name,
                                ImagePath = image.ImagePath,
                                FoodValue = value.FoodValue,
                                Quality = quality.Quality,
                                Username = logintable.Username,
                                Refund = history.Refund,
                                Time = history.Time
                            };
                DTGrid.ItemsSource = query.ToList();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteHistoryButtonDown(object sender, MouseButtonEventArgs e)
        {
            DeleteHistory.Source = new BitmapImage(new Uri("Images/clearhistorybuttondown_640.png", UriKind.Relative));
        }

        private async void DeleteHistoryButtonUp(object sender, MouseButtonEventArgs e)
        {
            DeleteHistory.Source = new BitmapImage(new Uri("Images/clearhistorybuttonup_640.png", UriKind.Relative));
            await Task.Delay(100);
            try
            {
                if (DTGrid.Items.Count > 0)
                {
                    List<History> deleteRows = DataCon.Histories.ToList();
                   
                    if (deleteRows != null)
                    {
                        DataCon.Histories.DeleteAllOnSubmit(deleteRows);
                        DataCon.SubmitChanges();
                        GetData();                  
                    }
                    else
                    {
                        MessageBox.Show("failed to delete!");
                    }
                }
                else
                {
                    MessageBox.Show("The history is empty!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private MainWindow mainWindow;  //végtelen példányosítás kikerülése.
        public void Reference(MainWindow main)
        {
            mainWindow = main;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Uri Uri = new Uri("Images/historytable0_640.png", UriKind.RelativeOrAbsolute);
            ImageSource imageSource = new BitmapImage(Uri);
            mainWindow.GoHistory.Source = imageSource;
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
    }
}
