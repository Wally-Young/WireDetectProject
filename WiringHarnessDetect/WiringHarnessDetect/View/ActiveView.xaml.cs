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

namespace WiringHarnessDetect.View
{
    /// <summary>
    /// PassiveView.xaml 的交互逻辑
    /// </summary>
    public partial class ActiveView : Page
    {
        public ActiveView()
        {
            InitializeComponent();
            Messenger.Default.Register<string>(this, "NextDisable", p => DisableNextStep(p));//更新图
            Messenger.Default.Register<Dictionary<string, List<Pin>>>(this, "UpdateCanvaPinA", p => UpdatePins(p));//更新图
        }

        private void UpdatePins(Dictionary<string, List<Pin>> p)
        {
            if (p.ContainsKey("DLeft"))
                ChangePaper(p["DLeft"], this.DLCanva);
            if (p.ContainsKey("DRight"))
                ChangePaper(p["DRight"], this.DRCanva);
            if (p.ContainsKey("LLeft"))
                ChangePaper(p["LLeft"], this.LLCanva);
            if (p.ContainsKey("LRight"))
                ChangePaper(p["LRight"], this.LRCanva);
        }

        private void ChangePaper(List<Pin> pins, Canvas canvas)
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
                ellipse.Name = "P" + item.PinCode;

                canvas.Children.Add(ellipse);
            }
        }
        private void DisableNextStep(string p)
        {
            if (p == "enable")
                this.nextstep.IsEnabled = true;
            else if(p=="disenable")
                this.nextstep.IsEnabled = false;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.learntab.Visibility = Visibility.Visible;
            this.detecttab.Visibility = Visibility.Collapsed;
            this.tabcontrol.SelectedIndex = 0;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.learntab.Visibility = Visibility.Collapsed;
            this.detecttab.Visibility = Visibility.Visible;
            this.tabcontrol.SelectedIndex = 1;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lst = (sender as ListView);
            string tag = lst.SelectedItem.ToString();

            Messenger.Default.Send<string>(tag, "UpdatePicture");
        }
    }
}
