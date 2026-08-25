using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
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
    /// Interaction logic for FistLogin.xaml
    /// </summary>
    public partial class FistLogin : Window
    {
        SqlConnection sqlConnection = new SqlConnection();
        SqlCommand sqlCommand = new SqlCommand();
        string Userb = "Please Enter a Username";
        string Passwordb = "12345678910";
        Login login = new Login();
        public static string Username = "";
        MainViewModel mainViewModel = new MainViewModel();
        
        public FistLogin()
        {
            InitializeComponent();
            string connectionString = ConfigurationManager.ConnectionStrings["FoodSaver.Properties.Settings.FoodDBConnectionString"].ConnectionString;
            sqlConnection = new SqlConnection(connectionString);
        }
        private async void SearchLogin(object sender, MouseButtonEventArgs e)
        {
            LoginButton.Source = new BitmapImage(new Uri("Images/signinbuttonup_640.png", UriKind.Relative));
            await Task.Delay(100);
            try
            {
                string Search = "SELECT LoginTable.Id, LoginTable.Username, LoginTable.Password FROM LoginTable WHERE LoginTable.Username = @Username AND LoginTable.Password = @Password";
                object searchValue = null;
                Username = UserBox.Text;
                string Password = PasswordBox.Password;
                if (UserBox.Text != null && PasswordBox.Password != null && UserBox.Text != Userb && PasswordBox.Password != Passwordb)
                {
                    await Task.Run(async () =>
                    {
                        using (sqlCommand = new SqlCommand(Search, sqlConnection))
                        {
                            await sqlConnection.OpenAsync();
                            sqlCommand.Parameters.AddWithValue("@Username", Username);
                            sqlCommand.Parameters.AddWithValue("@Password", Password);
                            searchValue = await sqlCommand.ExecuteScalarAsync();
                            Debug.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " For SELECT Username and Password FROM LoginTable");

                            if (searchValue != null)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    Username = UserBox.Text;
                                    if (login != null && !login.IsVisible)
                                    {
                                        App app = (App)Application.Current;
                                        app.ShowWindow(new Login());
                                        this.Hide();
                                    }
                                });
                            }
                            else
                            {
                                MessageBox.Show("The username or password is incorrect!");
                            }
                        }
                    });
                }
                else
                {
                    MessageBox.Show("Please enter a username and password");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private async void Restistration(object sender, MouseButtonEventArgs e)
        {
            RegisterButton.Source = new BitmapImage(new Uri("Images/registrationbuttonup_640.png", UriKind.Relative));
            await Task.Delay(100);
            try
            {
                string Search = "SELECT LoginTable.Id, LoginTable.Username, LoginTable.Password FROM LoginTable WHERE LoginTable.Username = @Username AND LoginTable.Password = @Password";
                object searchValue = null;
                string User = UserBox.Text;
                string Password = PasswordBox.Password;
                if (!string.IsNullOrEmpty(UserBox.Text) && !string.IsNullOrEmpty(PasswordBox.Password) && UserBox.Text != Userb && PasswordBox.Password != Passwordb)
                {
                    await Task.Run(async () =>
                    {
                        using (sqlCommand = new SqlCommand(Search, sqlConnection))
                        {
                            await sqlConnection.OpenAsync();
                            sqlCommand.Parameters.AddWithValue("@Username", User);
                            sqlCommand.Parameters.AddWithValue("@Password", Password);
                            searchValue = await sqlCommand.ExecuteScalarAsync();
                            Debug.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " For SELECT Username and Password FROM LoginTable");
                        }
                    });
                    if (searchValue == null)
                    {
                        int affectedRows = 0;
                        Random r = new Random();
                        mainViewModel.Coin = r.Next(4000, 10000);
                        await Task.Run(async () =>
                        {
                            string Search2 = "INSERT INTO LoginTable (Username, Password, UserCoin) VALUES (@Username, @Password, @UserCoin)";
                            using (sqlCommand = new SqlCommand(Search2, sqlConnection))
                            {
                                sqlCommand.Parameters.AddWithValue("@Username", User);
                                sqlCommand.Parameters.AddWithValue("@Password", Password);
                                sqlCommand.Parameters.AddWithValue("@UserCoin", mainViewModel.Coin);
                                Debug.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " For Insert Username and Password into LoginTable");
                                affectedRows = await sqlCommand.ExecuteNonQueryAsync();

                                if (affectedRows > 0)
                                {
                                    MessageBox.Show("Registration was successful!");
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        UserBox.Text = null;
                                        PasswordBox.Password = null;
                                    });

                                }
                            }
                        });
                    }
                    else
                    {
                        MessageBox.Show("This Username and Password is already exist!");
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a username and password");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                sqlConnection.Close();
            }
        }

        private void LoginButtonDown(object sender, MouseButtonEventArgs e)
        {
            LoginButton.Source = new BitmapImage(new Uri("Images/signinbuttondown_640.png", UriKind.Relative));
        }

        private void RegisterButtonDown(object sender, MouseButtonEventArgs e)
        {
            RegisterButton.Source = new BitmapImage(new Uri("Images/registrationbuttondown_640.png", UriKind.Relative));
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UserBox.Text = null;
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = null;
        }
    }
}
