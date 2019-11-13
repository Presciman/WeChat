using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
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
using WeChatTest.DAL;

namespace WeChatDemo
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class UserControl1 : UserControl
    {

        public Action<List<string>> SetPicSource;
        readonly string connString = "Data Source=xuyuex.date,1433;Initial Catalog=StarrySkyDB;Persist Security Info=True;User ID=sa;Password=Nich0las";
        public UserControl1()
        {
            InitializeComponent();
        }


        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            passCodeTB.Text = string.Empty;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (passCodeTB.Text.Length > 0)
                passCodeTB.Text = passCodeTB.Text.Substring(0, passCodeTB.Text.Length - 1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string num = btn.Content.ToString();

            passCodeTB.Text += num;

            if (passCodeTB.Text.Length == 6)
            {
                string passCode = passCodeTB.Text.Trim();

                SQLHelper db = new SQLHelper(connString);

                var tmpList = db.GetPicList(passCode).ToList();
                if (tmpList.Count > 0)
                {
                    SetPicSource(tmpList);
                    //Grid parent = this.Parent as Grid;
                    //parent.Children.Remove(this);
                    this.Visibility = Visibility.Collapsed;
                    passCodeTB.Text = string.Empty;
                }
                else
                {
                    MessageBox.Show("无效提取码");
                    passCodeTB.Text = string.Empty;
                }
            }
        }

        public void ShowThis()
        {
            this.Visibility = Visibility.Visible;
        }
    }
}
