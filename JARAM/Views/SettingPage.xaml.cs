using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Contacts;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace JARAM.Views
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public SettingPage()
        {
            this.InitializeComponent();

            // 종료 설정 예외처리문
            try
            {
                if (ExitToggleSwitch != null)
                    if ((bool)(App.localSettings.Values["IsBackExit"]) == true)
                        ExitToggleSwitch.IsOn = true;
                    else
                        ExitToggleSwitch.IsOn = false;
                else
                    ExitToggleSwitch.IsOn = false;
            }
            catch
            {
                ExitToggleSwitch.IsOn = false;
            }


            // 앱 버전 string
            string appVersion = string.Format("버전: {0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);
            AppVersionTextBlock.Text = appVersion;
        }

        private void ExitToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
                if (toggleSwitch.IsOn == true)
                    App.localSettings.Values["IsBackExit"] = true;
                else
                    App.localSettings.Values["IsBackExit"] = false;
        }

        // 이메일 주소 구성
        private async void EmailLink_Click(object sender, RoutedEventArgs e)
        {
            Contact emailContact = new Contact();
            ContactEmail email = new ContactEmail();
            email.Address = "good9797@outlook.com";
            email.Description = "JARAM 29기 회원, JARAM UWP앱 개발자";
            emailContact.Emails.Add(email);
            await ComposeEmail(emailContact, "앱 피드백 - JARAM", "앱 피드백 - JARAM\n\n");
        }

        // 이메일 보내기 구성
        private async Task ComposeEmail(Contact recipient, string messageSubject ,string messageBody)
        {
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.Subject = messageSubject;
            emailMessage.Body = messageBody;

            var email = recipient.Emails.FirstOrDefault();
            if (email != null)
            {
                var emailRecipient = new Windows.ApplicationModel.Email.EmailRecipient(email.Address);
                emailMessage.To.Add(emailRecipient);
            }

            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }
    }
}
