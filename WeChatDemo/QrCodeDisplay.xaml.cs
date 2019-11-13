using Gma.QrCodeNet.Encoding;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using WxPayAPI;

namespace WeChatDemo
{
    /// <summary>
    /// QrCodeDisplay.xaml 的交互逻辑
    /// </summary>
    public partial class QrCodeDisplay : Window
    {
        string out_trade_no;
        public QrCodeDisplay(string orderid)
        {
            InitializeComponent();
            this.out_trade_no = orderid;
            Loaded += QrCodeDisplay_Loaded;
            this.Closing += QrCodeDisplay_Closing;
        }

        private void QrCodeDisplay_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
        }

        DispatcherTimer timer;
        private void QrCodeDisplay_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            WxPayData date = new WxPayAPI.WxPayData();
            date.SetValue("out_trade_no", out_trade_no);//公众账号ID

            WxPayData result = WxPayApi.OrderQuery(date);
            if (result.GetValue("return_code").ToString().ToUpper().Equals("SUCCESS"))
            {
                if (result.GetValue("result_code").ToString().ToUpper().Equals("SUCCESS") &&
                    result.GetValue("trade_state").ToString().ToUpper().Equals("SUCCESS"))
                {
                    CloseThis();
                    MessageBox.Show("付款成功");
                }
            }
        }

        void CloseThis()
        {
            timer.Stop();
            this.Close();
        }

        public void SetQR(string qrContent)
        {
            var geometry = CreateQrCode(qrContent);
            qrDrawing.Geometry = geometry;
        }

        StreamGeometry CreateQrCode(string qrContent)
        {
            try
            {
                QrEncoder qrEncoder = new QrEncoder(ErrorCorrectionLevel.H);
                QrCode qrCode = new QrCode();
                qrEncoder.TryEncode(qrContent, out qrCode);

                //StreamGeometry geometry = PathRender.DrawEllipseGeometry(qrCode.Matrix, 1, 1, false);
                //drawing1.Geometry = geometry;

                //double w = geometry.Bounds.Width / 4;
                //double h = geometry.Bounds.Height / 4;

                //double x = geometry.Bounds.Width / 2 - w / 2;
                //double y = geometry.Bounds.Height / 2 - h / 2;

                //drawIcon1.Rect = new Rect(x, y, w, h);


                var geometry = PathRender.DrawRectGeometry(qrCode.Matrix, 1, 1, false);
                //qrDrawing.Geometry = geometry;
                return geometry;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return null;
        }

    }
}
