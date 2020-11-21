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
    /// PassiveView.xaml 的交互逻辑
    /// </summary>
    public partial class PassiveView : Page
    {
        public PassiveView()
        {
            InitializeComponent();
            Messenger.Default.Register<Dictionary<string,List<Pin>>>(this, "UpdateCanvaPin", p => UpdatePins(p));//更新图
        }

        private void UpdatePins(Dictionary<string,List<Pin>> p)
        {
            if(p.ContainsKey("Left"))
                ChangePaper( p["Left"], this.leftCanva);
            if(p.ContainsKey("Right"))
                ChangePaper(p["Right"], this.rightCanva);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lst = (sender as ListView);
            if (lst.SelectedItem == null)
            {
                return;
            }
            string tag= lst.SelectedItem.ToString();
            var info= (App.Current.Resources["Locator"] as ViewModelLocator).Passive.UpdatePicture(tag);
            UpdatePins(info);
        }

     

        private void ChangePaper(List<Pin> pins,Canvas canvas)
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
                ellipse.SetValue(Canvas.TopProperty, (canvas.RenderSize.Height/item.PinEY  ) * item.PinSY);
                ellipse.SetValue(Canvas.LeftProperty, (canvas.RenderSize.Width/item.PinEX  ) * item.PinSX);
                ellipse.Name = "P" + item.PinCode;
                
                canvas.Children.Add(ellipse);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
