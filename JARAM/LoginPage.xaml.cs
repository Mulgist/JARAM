using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Popups;
using MySql.Data.MySqlClient; // MySQL API for RT System
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.Web.Http;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace JARAM
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        bool isLogged = false;
        public LoginPage()
        {
            this.InitializeComponent();

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();

                if (statusBar != null)
                {
                    statusBar.BackgroundOpacity = 1;
                    statusBar.BackgroundColor = Colors.White;
                    statusBar.ForegroundColor = Colors.Black;
                }
            }

            if (App.localSettings.Values["saveID"] != null && App.localSettings.Values["savePassword"] != null)
            {
                IDTextBox.Text = App.localSettings.Values["saveID"] as string;
                PasswordInputBox.Password = App.localSettings.Values["savePassword"] as string;
                SaveLoginCheckBox.IsChecked = true;
            }
            else
            {
                SaveLoginCheckBox.IsChecked = false;
            }
        }
        /*
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // 자동 로그인 기능 사용
            if (App.localSettings.Values["autoLoginID"] != null)
            {
                AutoLoginCheckBox.IsChecked = true;
                AutoLoginCheckBox.IsEnabled = false;
                LoginProgressRing.IsActive = true;
                LoginProgressTextBlock.Text = "자동로그인 중입니다.";
                IDTextBox.Text = App.localSettings.Values["autoLoginID"] as string;
                IDTextBox.IsEnabled = false;
                PasswordInputBox.IsEnabled = false;
                var dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
            }
        }
        */

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginProcess();
        }

        private async void MessageBoxOpen(string showString)
        {
            var dialog = new MessageDialog(showString);
            await dialog.ShowAsync();
        }

        private async void ProgressEnable()
        {
            LoginProgressRing.IsActive = true;
            LoginProgressTextBlock.Text = "로그인하는 중입니다";
            this.InitializeComponent();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LoginProgressRing.IsActive = true;
                LoginProgressTextBlock.Text = "로그인하는 중입니다";
            });
        }
        private async void ProgressDisable()
        {
            LoginProgressRing.IsActive = false;
            LoginProgressTextBlock.Text = "";
            this.InitializeComponent();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
              {
                  LoginProgressRing.IsActive = false;
                  LoginProgressTextBlock.Text = "";
              });
        }
        private void IDTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                LoginProcess();
        }

        private void PasswordInputBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                LoginProcess();
        }

        public async void LoginProcess()
        {
            string ID = IDTextBox.Text;
            string password = PasswordInputBox.Password;
            if (IDTextBox.Text == "")
            {
                MessageBoxOpen("ID를 입력하십시오.");
            }
            else if (PasswordInputBox.Password == "")
            {
                MessageBoxOpen("Password를 입력하십시오.");
            }
            else
            {
                string content = "error_return_url=%2Findex.php%3Fact%3DdispMemberLoginForm&mid=home&vid=&ruleset=%40login&success_return_url=http%3A%2F%2Fwww.jaram.net%2Fboard_slIl36&act=procMemberLogin&xe_validator_id=modules%2Fmember%2Fskins&user_id=" + ID + "&password=" + password;
                Uri uri = new Uri("http://www.jaram.net/index.php?act=dispMemberLoginForm");
                IHttpContent httpContent = new HttpStringContent(content, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");
                System.Net.Http.HttpClientHandler handler = new System.Net.Http.HttpClientHandler();
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Referer = uri;
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.RequestUri = uri;
                string httpResponseBody = "";
                try
                {
                    //Send the POST request
                    HttpResponseMessage response = await httpClient.PostAsync(uri, httpContent);
                    response.EnsureSuccessStatusCode();
                    httpResponseBody = await response.Content.ReadAsStringAsync();
                    if (httpResponseBody.IndexOf("잘못된 비밀번호입니다.") != -1)
                    {
                        isLogged = false;
                        MessageBoxOpen("잘못된 비밀번호입니다.");
                    }
                    else if (httpResponseBody.IndexOf("존재하지 않는 회원 아이디입니다.") != -1)
                    {
                        isLogged = false;
                        MessageBoxOpen("존재하지 않는 회원 아이디입니다.");
                    }
                    else if (httpResponseBody.IndexOf("로그인 가능 횟수를 초과했습니다.") != -1)
                    {
                        isLogged = false;
                        MessageBoxOpen("로그인 가능 횟수를 초과했습니다.\n잠시 동안 로그인이 제한됩니다.");
                    }
                    else if (httpResponseBody.IndexOf("아이디의 값은") != -1)
                    {
                        isLogged = false;
                        MessageBoxOpen("로그인 정보가 잘못되었습니다.");
                    }
                    else
                    {
                        isLogged = true;
                    }

                }
                catch (Exception ex)
                {
                    httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
                    isLogged = false;
                }

                if (isLogged == true)
                {
                    App.currentUserInfo.currentUserID = ID;

                    // 아이디 패스워드 저장
                    if (SaveLoginCheckBox.IsChecked == true)
                    {
                        App.localSettings.Values["saveID"] = ID;
                        App.localSettings.Values["savePassword"] = password;
                    }
                    else
                    {
                        App.localSettings.Values["saveID"] = null;
                        App.localSettings.Values["savePassword"] = null;
                    }

                    App.conn.Open();

                    string sql = "SELECT member_srl, user_name, nick_name, email_address FROM xe_member WHERE user_id = \"" + App.currentUserInfo.currentUserID + "\"";
                    MySqlDataReader connreader = new MySqlCommand(sql, App.conn).ExecuteReader();

                    while (connreader.Read())
                    {
                        App.currentUserInfo.currentUserNumber = connreader["member_srl"].ToString();
                        App.currentUserInfo.currentUserName = connreader["user_name"].ToString();
                        App.currentUserInfo.currentUserNickName = connreader["nick_name"].ToString();
                        App.currentUserInfo.currentUserEmail = connreader["email_address"].ToString();
                    }
                    connreader.Close();

                    // 사용자 등급
                    sql = "SELECT s.user_id, d.title FROM xe_member s, xe_member_group d, xe_member_group_member f WHERE s.member_srl = f.member_srl AND f.group_srl = d.group_srl and s.user_id = \"" + App.currentUserInfo.currentUserID + "\"";
                    connreader = new MySqlCommand(sql, App.conn).ExecuteReader();
                    while (connreader.Read())
                    {
                        App.currentUserInfo.currentUserLevel = connreader["title"].ToString();
                    }
                    connreader.Close();
                    App.conn.Close();

                    this.Frame.Navigate(typeof(MainPage));
                }

                
            }
        }
    }
}
