using GalaSoft.MvvmLight.Messaging;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WiringHarnessDetect.Model;
using WiringHarnessDetect.ViewModel;


namespace WiringHarnessDetect.View
{
    /// <summary>
    /// ExcelDetectView.xaml 的交互逻辑
    /// </summary>
    public partial class ExcelDetectView : Page
    {
        public ExcelDetectView()
        {
            InitializeComponent();
            Messenger.Default.Register<Dictionary<string, List<ExcelPin>>> (this, "UpdateExPCanvas", p => UpdatePins(p));//更新图
        }

        private void UpdatePins(Dictionary<string,List<ExcelPin>> p)
        {

            if (p.ContainsKey("Left"))
                ChangePaper(p["Left"],this.Left);
            if (p.ContainsKey("Right"))
                ChangePaper(p["Right"], this.Right);

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lst = (sender as ListView);
            if (lst.SelectedItem == null)
            {
                return;
            }
            string tag = lst.SelectedItem.ToString();
            var info = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPassive.UpdatePicture(tag);
            UpdatePins(info);
            
            lst.SelectedIndex = -1;
        }



        private void ChangePaper(List<ExcelPin> pins, Canvas canvas)
        {
            canvas.Children.Clear();
            foreach (var item in pins)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Height = 12;
                ellipse.Width = 12;
                try
                {
                    ellipse.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.DrawColor));
                }
                catch (Exception)
                {

                    ellipse.Fill = Brushes.Red;
                }
                ellipse.SetValue(Canvas.TopProperty, (canvas.RenderSize.Height / item.PinEY) * item.PinSY);
                ellipse.SetValue(Canvas.LeftProperty, (canvas.RenderSize.Width / item.PinEX) * item.PinSX);
                ellipse.Name = "P" + item.PinNO;

                canvas.Children.Add(ellipse);
            }
        }

        private void ListView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ListView lst = (sender as ListView);
            if (lst.SelectedItem == null)
            {
                return;
            }
        }

        private void Left_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var item in Left.Children)
            {
                if (item is Ellipse)
                {
                    Ellipse ep = item as Ellipse;
                    double hight = Canvas.GetTop(ep);
                    double with = Canvas.GetLeft(ep);
                    ep.SetValue(Canvas.TopProperty, (e.NewSize.Height / e.PreviousSize.Height) * hight);
                    ep.SetValue(Canvas.LeftProperty, (e.NewSize.Width / e.PreviousSize.Width) * with);
                }

            }
        }

        private void Right_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var item in Right.Children)
            {
                if (item is Ellipse)
                {
                    Ellipse ep = item as Ellipse;
                    double hight = Canvas.GetTop(ep);
                    double with = Canvas.GetLeft(ep);
                    ep.SetValue(Canvas.TopProperty, (e.NewSize.Height / e.PreviousSize.Height) * hight);
                    ep.SetValue(Canvas.LeftProperty, (e.NewSize.Width / e.PreviousSize.Width) * with);
                }

            }
        }

        private List<string> GetCodes(string tag)
        {
            List<string> result = new List<string>();


            int first = tag.IndexOf('(');
            int start = tag.IndexOf("F:");
            int end = tag.IndexOf(',', start);
            int mid = tag.IndexOf('>');
            if (mid != -1)
            {

                int fir = tag.IndexOf(',');
                int sec = tag.IndexOf(":");
                string axis = tag.Substring(tag.IndexOf(":") + 1, fir - sec - 1);
                string FNO = tag.Substring(start + 2, mid - start - 3);
                string PNO = tag.Substring(mid + 1, end - mid - 1);
                string PAddress = tag.Substring(0, first);
                result.AddRange(new string[] { PAddress, axis, FNO, PNO });
            }
            else
            {
                result.AddRange(new string[] { "", "", "", "" });
            }


            int sStart = tag.IndexOf("V:");

            int sEnd = tag.IndexOf(')');
            int sMid = tag.IndexOf("->", sStart);
            if (sMid != -1)
            {
                string VFNO = tag.Substring(sStart + 2, sMid - sStart - 2);
                string VIndex = tag.Substring(sMid + 2, sEnd - sMid - 2);
                result.AddRange(new string[] { VFNO, VIndex });
            }
            else
            {
                result.AddRange(new string[] { "", "" });
            }

            return result;
        }

        private void MenuItem_ON(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            var cm = mi.Parent as ContextMenu;
            var lstItem = cm.PlacementTarget as ListViewItem;
           
            if (lstItem == null)
            {
                return;
               
            }
           
            string tag = lstItem.Content.ToString();
            List<byte> msg = new List<byte> { 0xfe, 0xef, 0x30, 0x0c };
            List<string> leftCodes = GetCodes(tag.Split('+')[0]);
            List<string> rightCodes = GetCodes(tag.Split('+')[1]);
           
            List<byte> datas = new List<byte>();
            if (leftCodes[2] != ""&& leftCodes[2].Trim().Length!=0)
            {
              
                 short led = (short)SQliteDbContext.GetOneFixtureBaseInfo(leftCodes[2]).LEDAddress;
                 byte[] addr = BitConverter.GetBytes(led).Reverse().ToArray();
                 datas.AddRange(addr);
            }
            if (rightCodes[2] != "" && rightCodes[2].Trim().Length != 0)
            {
                short led = (short)SQliteDbContext.GetOneFixtureBaseInfo(rightCodes[2]).LEDAddress;
                byte[] addr = BitConverter.GetBytes(led).Reverse().ToArray();
                datas.AddRange(addr);
            }
            msg.AddRange(BitConverter.GetBytes((short)datas.Count).Reverse());
            msg.AddRange(datas);
            Messenger.Default.Send<byte[]>(msg.ToArray(), "Send");
        }

        private void MenuItem_OFF(object sender, RoutedEventArgs e)
        {
            byte[] msg = new byte[] { 0xfe, 0xef, 0x30, 0x0c, 0x00, 0x02, 0x00, 0x00 };
            Messenger.Default.Send<byte[]>(msg, "Send");
        }
    }
}
