using DocInfo;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using MySql.Data.MySqlClient; // MySQL API for RT System

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace JARAM.Views
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class DocumentsListPage : Page
    {
        public DocumentsListPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var parameter = e.Parameter as string;
            string sql = "SELECT title, nick_name, last_update, is_notice, document_srl FROM web.xe_documents WHERE module_srl=\"" + parameter + "\" ORDER BY update_order ASC";

            App.conn.Open();
            MySqlDataReader connreader = new MySqlCommand(sql, App.conn).ExecuteReader();
            DocumentsInfo docsInfo;
            ObservableCollection<DocumentsInfo> docsInfoList = new ObservableCollection<DocumentsInfo>();

            while (connreader.Read())
            {
                docsInfo = new DocumentsInfo
                {
                    title = connreader["title"].ToString(),
                    name = connreader["nick_name"].ToString(),
                    date = connreader["last_update"].ToString().Substring(0, 4) + "." + connreader["last_update"].ToString().Substring(4, 2) + "." + connreader["last_update"].ToString().Substring(6, 2),
                    notice = connreader["is_notice"].ToString(),
                    docNumber = connreader["document_srl"].ToString()
                };
                docsInfoList.Add(docsInfo);
            }

            SeminarList.ItemsSource = docsInfoList;

            connreader.Close();
            App.conn.Close();
        }

        private void SeminarList_ItemClick(object sender, ItemClickEventArgs e)
        {
            DocumentsInfo info = e.ClickedItem as DocumentsInfo;
            string docsNum = info.docNumber;
            string sql = "SELECT * FROM web.xe_documents WHERE document_srl=\"" + docsNum + "\"";

            var parameter = info;

            App.titleStack.Push(App.titleStack.Peek());
            this.Frame.Navigate(typeof(DocumentView), parameter);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            App.titleStack.Push(App.titleStack.Peek());
            this.Frame.Navigate(typeof(DocumentEditPage));
        }
    }
}
