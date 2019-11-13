using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WeChatTest.DAL;
using WxPayAPI;

namespace WeChatDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string connString = "Data Source=xuyuex.date,1433;Initial Catalog=StarrySkyDB;Persist Security Info=True;User ID=sa;Password=Nich0las";
        ObservableCollection<string> PicList = new ObservableCollection<string>();
        bool IsScanQR = false;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        UserControl1 method1;
        ScanQR method2;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var tmp = System.Configuration.ConfigurationManager.AppSettings["IsScanQR"];
            bool.TryParse(tmp, out IsScanQR);

            if (IsScanQR)
            {
                method2 = new ScanQR();
                Grid.SetColumnSpan(method2, 2);
                method2.SetPicSource = SetPicSource;
                layout.Children.Add(method2);

            }
            else
            {
                method1 = new UserControl1();
                Grid.SetColumnSpan(method1, 2);
                method1.SetPicSource = SetPicSource;
                layout.Children.Add(method1);
            }

            //step1.SetPicSource = SetPicSource;
            //stepOne.SetPicSource = SetPicSource;

            Log.Info("WxPayApi", "UnfiedOrder request : ");
        }

        //private void GetPic_Click(object sender, RoutedEventArgs e)
        //{
        //    string passCode = passCodeTB.Text.Trim();

        //    SQLHelper db = new SQLHelper(connString);

        //    List<string> list = db.GetPicList(passCode).ToList();

        //    listBox.ItemsSource = list;
        //}

        void SetPicSource(List<string> list)
        {
            listBox.ItemsSource = list;
        }



        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            NativePay nativePay = new NativePay();
            //生成扫码支付模式二url
            //string tmp = priceTB.Text.Trim();
            //double price = 0.01;
            //double.TryParse(tmp, out price);

            int totalFee = 1;

            string out_trade_no = WxPayApi.GenerateOutTradeNo();

            WxPayData data = new WxPayData();
            data.SetValue("body", "张裕DIY流程演示用");//商品描述
            data.SetValue("attach", "张裕DIY流程演示用");//附加数据
            data.SetValue("out_trade_no", out_trade_no);//随机字符串
            data.SetValue("total_fee", totalFee);//总金额
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));//交易起始时间
            data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));//交易结束时间
            data.SetValue("goods_tag", "jjj");//商品标记
            data.SetValue("trade_type", "NATIVE");//交易类型
            data.SetValue("product_id", "123456789");//商品ID

            WxPayData result = WxPayApi.UnifiedOrder(data);//调用统一下单接口
            string url = result.GetValue("code_url").ToString();//获得统一下单接口返回的二维码链接



            //string url2 = nativePay.GetPayUrl("123456789", totalFee);

            //string ss = HttpUtility.UrlEncode(url2);

            //CreateQrCode(url2);

            QrCodeDisplay dis = new WeChatDemo.QrCodeDisplay(out_trade_no);
            dis.Owner = this;
            dis.SetQR(url);
            dis.Show();

        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            listBox.ItemsSource = null;
            //step1.Visibility = Visibility.Visible;

            //stepOne.ShowThis();

            if (IsScanQR)
            {
                method2.ShowThis();
            }
            else
            {
                method1.ShowThis();
            }
        }



        private void Window_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = imgContainer;
            e.Handled = true;
        }

        private void Window_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)e.Source;


            Matrix rectsMatrix = ((MatrixTransform)element.RenderTransform).Matrix;


            rectsMatrix.Translate(e.DeltaManipulation.Translation.X,
                                 e.DeltaManipulation.Translation.Y);
            //Console.WriteLine(e.DeltaManipulation.Translation.X.ToString());

            Point center = new Point(e.ManipulationOrigin.X, e.ManipulationOrigin.Y);
            center = new Point(element.ActualWidth / 2, element.ActualHeight / 2);
            center = rectsMatrix.Transform(center);

            rectsMatrix.ScaleAt(e.DeltaManipulation.Scale.X,
                                e.DeltaManipulation.Scale.Y,
                                center.X,
                                center.Y);


            rectsMatrix.RotateAt(e.DeltaManipulation.Rotation, center.X, center.Y);



            //((MatrixTransform)element.RenderTransform).Matrix = rectsMatrix;
            element.RenderTransform = new MatrixTransform(rectsMatrix);
            //else

            e.Handled = true;
        }

        private void Window_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {

            //e.TranslationBehavior.DesiredDeceleration = 10.0 * 96.0 / (1000.0 * 100.0);

            //e.ExpansionBehavior.DesiredDeceleration = 0.1 * 96 / (1000.0 * 100.0);

            e.Handled = true;

        }

        private void grid_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string sour = listBox.SelectedItem.ToString();
            bg.Source = new BitmapImage(new Uri(sour));
        }
    }
}
