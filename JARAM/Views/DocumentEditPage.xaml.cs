using System;
using System.Net;
using System.Net.Sockets;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MySql.Data.MySqlClient; // MySQL API for RT System
using Windows.UI.Popups;

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace JARAM.Views
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class DocumentEditPage : Page
    {
        // 수정인지 추가인지 구분하는 bool
        bool isEdit = false;
        string docsNum;
        string myIPAddress;
        string content;
        public DocumentEditPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter == null)
            {
                isEdit = false;
            }
            else
            {
                var parameter = e.Parameter as string[];
                isEdit = true;
                TitleTextBox.Text = parameter[0];
                ContentTextBox.Text = parameter[1];
                docsNum = parameter[2];
            }
            GetClientIPAddress();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveMessageBox();
        }

        private async void SaveMessageBox()
        {
            // 확인 취소 메세지박스
            var yesCommand = new UICommand("네");
            var noCommand = new UICommand("아니오");

            var dialog = new MessageDialog("저장 하시겠습니까?");
            dialog.Options = MessageDialogOptions.None;
            dialog.Commands.Add(yesCommand);

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 0;

            dialog.Commands.Add(noCommand);
            dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;

            var command = await dialog.ShowAsync();

            if (command == yesCommand)
            {
                DateTime now = DateTime.Now;
                string sql = "SELECT min(update_order) min_order FROM xe_documents";
                int minOrder = 0;
                string nowTime = now.Year + string.Format("{0:D2}", now.Month) + string.Format("{0:D2}", now.Day) + string.Format("{0:D2}", now.Hour) + string.Format("{0:D2}", now.Minute) + string.Format("{0:D2}", now.Second);
                // string -> HTML
                content = StringTo_Html(ContentTextBox.Text);
                App.conn.Open();
                MySqlDataReader connreader = new MySqlCommand(sql, App.conn).ExecuteReader();

                while (connreader.Read())
                {
                    minOrder = int.Parse(connreader["min_order"].ToString());
                }
                minOrder--;
                connreader.Close();
                App.conn.Close();

                if (isEdit == true)
                    // 수정할 시의 SQL문
                    sql = "UPDATE xe_documents SET title=\"" + TitleTextBox.Text + "\", content=\"" + content
                        + "\", last_update=\"" + nowTime + "\", update_order = \"" + minOrder + "\" WHERE document_srl=\"" + docsNum + "\"";
                else
                    // 새로 작성할 시의 SQL문
                    sql = "INSERT INTO xe_documents VALUES (\"" + (-minOrder) + "\", \"" + GetDocumentClassNum(App.titleStack.Peek()) 
                        + "\", \"0\", \"ko\", \"N\", \"" + TitleTextBox.Text + "\", \"N\", \"N\", \"" + content
                        + "\", \"0\", \"0\", \"0\", \"0\", \"0\", \"0\", null, \"" + App.currentUserInfo.currentUserID
                        + "\", \"" + App.currentUserInfo.currentUserName
                        + "\", \"" + App.currentUserInfo.currentUserNickName + "\", \"" + App.currentUserInfo.currentUserNumber
                        + "\", \"" + App.currentUserInfo.currentUserEmail + "\", \"\", null, \"N;\", \"" + nowTime + "\", \"" + nowTime
                        + "\", null, \"" + myIPAddress + "\", \"" + minOrder + "\", \"" + minOrder + "\", \"N\", \"N\", \"PUBLIC\", \"ALLOW\")";

                // sql 커맨드 입력할 때
                MySqlCommand cmd = new MySqlCommand(sql, App.conn);
                App.conn.Open();
                // cmd 해제
                cmd.ExecuteNonQuery();
                App.conn.Close();

                // 페이지 뒤로 가기
                App.titleStack.Pop();
                this.Frame.GoBack();
            }
        }

        public int GetDocumentClassNum(string documentClassName)
        {
            int documentClassNum;

            switch (documentClassName)
            {
                case "공지사항":
                    documentClassNum = 164;
                    break;
                case "세미나":
                    documentClassNum = 173;
                    break;
                case "플레이스토밍":
                    documentClassNum = 345;
                    break;
                case "워크샵":
                    documentClassNum = 248;
                    break;
                case "스터디":
                    documentClassNum = 145;
                    break;
                case "재학생 게시판":
                    documentClassNum = 158;
                    break;
                default :
                    documentClassNum = 0;
                    break;
            }
            return documentClassNum;
        }

        public async void GetClientIPAddress()
        {
            IPHostEntry host = await Dns.GetHostEntryAsync(Dns.GetHostName());
            string ClientIP = string.Empty;
            for (int i = 0; i < host.AddressList.Length; i++)
            {
                if (host.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    ClientIP = host.AddressList[i].ToString();
                }
            }
            myIPAddress = ClientIP;
        }

        //작성: 권요한
        public string StringTo_Html(string content)
        {
            char[] ht = new char[5 * content.Length];
            int j = 0;
            ht[0] = '<';
            ht[1] = 'p';
            ht[2] = '>';
            j = j + 3;
            for (int i = 0; i < content.Length; i++)
            {

                if (content[i] == '\r')
                {
                    ht[j] = '<';
                    ht[j + 1] = 'b';
                    ht[j + 2] = 'r';
                    ht[j + 3] = '/';
                    ht[j + 4] = '>';

                    j = j + 5;
                }
                else
                {
                    ht[j] = content[i];
                    j++;
                }
            }
            ht[j] = '<';
            ht[j + 1] = 'p';
            ht[j + 2] = '/';
            ht[j + 3] = '>';

            string result = new string(ht);
            return result;
        }

        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            EraseMessageBox();
        }

        private async void EraseMessageBox()
        {
            // 확인 취소 메세지박스
            var yesCommand = new UICommand("네");
            var noCommand = new UICommand("아니오");

            var dialog = new MessageDialog("본문 내용을 전부 지우시겠습니까?");
            dialog.Options = MessageDialogOptions.None;
            dialog.Commands.Add(yesCommand);

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 0;

            dialog.Commands.Add(noCommand);
            dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;

            var command = await dialog.ShowAsync();

            if (command == yesCommand)
            {
                ContentTextBox.Text = "";
            }
        }
    }
}
