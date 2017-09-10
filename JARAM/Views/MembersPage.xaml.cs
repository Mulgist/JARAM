using Windows.UI.Xaml.Controls;
using MySql.Data.MySqlClient; // MySQL API for RT System

// 빈 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace JARAM.Views
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MembersPage : Page
    {
        public MembersPage()
        {
            this.InitializeComponent();

            string content = null;
            string sql = "SELECT * FROM web.xe_documents WHERE document_srl=163";

            App.conn.Open();

            MySqlDataReader connreader = new MySqlCommand(sql, App.conn).ExecuteReader();

            while (connreader.Read())
            {
                content = connreader["content"].ToString();
            }

            content = HtmlTo_String(content);
            ContentTextBlock.Text = content;

            connreader.Close();
            App.conn.Close();
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
    }
}
