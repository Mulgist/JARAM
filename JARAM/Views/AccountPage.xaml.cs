using Windows.UI.Xaml.Controls;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace JARAM.Views
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class AccountPage : Page
    {
        public AccountPage()
        {
            this.InitializeComponent();

            UserIDTextBlock.Text = App.currentUserInfo.currentUserID;
            UserNameTextBlock.Text = App.currentUserInfo.currentUserName;
            UserNickNameTextBlock.Text = App.currentUserInfo.currentUserNickName;
            UserEmailTextBlock.Text = App.currentUserInfo.currentUserEmail;
            UserLevelTextBlock.Text = App.currentUserInfo.currentUserLevel;
        }
    }
}
