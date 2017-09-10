using DocInfo;
using System;
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
    public sealed partial class DocumentView : Page
    {
        string docsNum = null;
        string title = null;
        string name = null;
        string content = null;
        public DocumentView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameters = e.Parameter as DocumentsInfo;

            string date = null;
            string userID = null;

            docsNum = parameters.docNumber;
            string sql = "SELECT * FROM web.xe_documents WHERE document_srl=\"" + docsNum + "\"";

            App.conn.Open();

            MySqlDataReader connreader = new MySqlCommand(sql, App.conn).ExecuteReader();
            while (connreader.Read())
            {
                title = connreader["title"].ToString();
                name = connreader["nick_name"].ToString();
                date = connreader["last_update"].ToString().Substring(0, 4) + "." + connreader["last_update"].ToString().Substring(4, 2) + "." + connreader["last_update"].ToString().Substring(6, 2);
                content = connreader["content"].ToString();
                userID = connreader["user_id"].ToString();
            }
            TitleTextBlock.Text = title;
            NameTextBlock.Text = name;
            DateTextBlock.Text = date;
            //content HTML -> string
            content = HtmlTo_String(content);
            ContentTextBlock.Text = content;

            connreader.Close();
            App.conn.Close();

            if (userID != App.currentUserInfo.currentUserID)
            {
                EditButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            App.titleStack.Push(App.titleStack.Peek());
            string[] document = new string[] { title, content, docsNum };
            this.Frame.Navigate(typeof(DocumentEditPage), document);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteMessageBox();
        }

        // 작성: 권요한
        public string HtmlTo_String(string html)
        {
            char[] st = new char[html.Length];
            int j = 0;
            int check = 0;
            for (int i = 0; i < html.Length; i++)
            {
                if (html[i] == '<')
                {
                    check = 1;
                    if (html[i + 1] == '/' || html[i + 1] == 'b')
                    {
                        st[j] = '\n';
                        j++;
                    }
                }
                if (check == 0)
                {
                    st[j] = html[i];
                    j++;
                }
                if (html[i] == '>')
                {
                    check = 0;
                }
            }
            string result = new string(st);
            return result;
        }


        private async void DeleteMessageBox()
        {
            // 확인 취소 메세지박스
            var yesCommand = new UICommand("네");
            var noCommand = new UICommand("아니오");

            var dialog = new MessageDialog("삭제 하시겠습니까?");
            dialog.Options = MessageDialogOptions.None;
            dialog.Commands.Add(yesCommand);

            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 0;

            dialog.Commands.Add(noCommand);
            dialog.CancelCommandIndex = (uint)dialog.Commands.Count - 1;

            var command = await dialog.ShowAsync();

            if (command == yesCommand)
            {
                string sql = "DELETE from xe_documents WHERE document_srl=\"" + docsNum + "\"";
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


    }
}
