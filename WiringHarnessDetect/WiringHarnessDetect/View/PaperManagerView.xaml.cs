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
    /// PaperManager.xaml 的交互逻辑
    /// </summary>
    public partial class PaperManagerView : Page
    {
        public PaperManagerView()
        {
            InitializeComponent();
            Messenger.Default.Register<List<Pin>>(this,"clear",p=>ChangePaper(p));//清空点

            
        }

        

        #region Field
        private Operation Operator;
        private bool mousedown = false;
        private Ellipse tempEp;
        private Point StartPoint;
        private Brush TempBruhes;
        private PinEx SelectedpinEx;
        #endregion

        #region OperatorButtonEvent
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Button btn = sender as Button;
            string tag = btn.Tag.ToString().ToLower();
            switch (tag)
            {
                case "select": Operator = Operation.Select; break;
                case "create": Operator = Operation.Create;break;
                case "move": Operator = Operation.Move; break;
                case "modify":Operator = Operation.Modify;               
                    break;
                case "delete": Operator = Operation.Delete; break;
                case "clear": (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Channels.Clear(); break;
                default:
                    break;
            }
        }

        #endregion

        #region CanvasEvent
        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var item in canvas.Children)
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

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
            StartPoint = e.GetPosition(canvas);
            if (Operator == Operation.Create)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Height = 12;
                ellipse.Width = 12;
                ellipse.Fill = Brushes.Red;
                ellipse.SetValue(Canvas.TopProperty, StartPoint.Y - ellipse.Height / 2);
                ellipse.SetValue(Canvas.LeftProperty, StartPoint.X - ellipse.Height / 2);
                ellipse.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
                ellipse.MouseMove += Ellipse_MouseMove;
                ellipse.MouseUp += Ellipse_MouseUp;
                
                canvas.Children.Add(ellipse);
                
            }
            
        }

      
        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Operator = Operation.None;
            if (tempEp != null)
                tempEp.Fill = TempBruhes;
        }
        #endregion

        #region EllipseEvent
        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Ellipse ep = sender as Ellipse;

            mousedown = false;

            ep.ReleaseMouseCapture();
            if (tempEp != null && (Operator == Operation.Move))
            {
                tempEp.Fill = TempBruhes;
                Operator = Operation.None;
               
            }
            if(ep.Name!=null&&ep.Name.Trim().Length>0)
            {
                string name = ep.Name.Remove(0,1);
                Pin p= (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Pins.ToList().Find(x => x.PinCode ==name );
                if (p == null)
                    return;
                p.PinSX = Canvas.GetLeft(ep);
                p.PinSY = Canvas.GetTop(ep);
                SQliteDbContext.UpdatOnePin(p);
            }
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            Ellipse ep = sender as Ellipse;
            if (mousedown && Operation.Move == Operator)
            {
                double deltav = e.GetPosition(canvas).Y;
                double deltah = e.GetPosition(canvas).X;
                ep.SetValue(Canvas.TopProperty, deltav-ep.Height/2);
                ep.SetValue(Canvas.LeftProperty, deltah- ep.Height / 2);

            }
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse ep = sender as Ellipse;
            mousedown = true;
            if (Operator != Operation.Create)
            {
                tempEp = ep;
                TempBruhes = ep.Fill;
                ep.Fill = Brushes.LawnGreen;
               
                ep.CaptureMouse();
            }
            if (Operator == Operation.Delete)
            {
                ep.ReleaseMouseCapture();
                canvas.Children.Remove(ep);
                if(ep.Name!=null&&ep.Name!="")
                {
                    SQliteDbContext.DeleteOnePin(ep.Name.Substring(1, ep.Name.Length - 1), (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Project.ProjectNO);
                    PinEx p= (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Pins.ToList().Find(x => x.PinCode == ep.Name.Substring(1, ep.Name.Length - 1));
                    (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Pins.Remove(p);


                }
            }
            if (Operator == Operation.Modify)
            {
                if (tempEp != null)
                {
                    int value = 0;
                    int.TryParse(txtDia.Text, out value);
                    if (cbDia.IsChecked.HasValue && cbDia.IsChecked.Value)
                    {
                        tempEp.Height = value;
                        tempEp.Width = value;
                    }
                    if (cbDia.IsChecked.HasValue && cbColor.IsChecked.Value)
                        tempEp.Fill = clpk.SelectedBrush;

                }
                if (tempEp.Name == "")
                {
                    Pin p = new Pin();
                    p.PinEX = canvas.RenderSize.Width;
                    p.PinEY = canvas.RenderSize.Height;
                    p.PinSX = Canvas.GetLeft(tempEp);
                    p.PinSY = Canvas.GetTop(tempEp);
                    p.Radius = tempEp.ActualHeight;
                    if (tempEp.Fill.ToString().Trim().Length > 0 && tempEp.Fill.ToString() != "0")
                        p.DrawColor = tempEp.Fill.ToString();
                    else
                        p.DrawColor = "#FF0000";
                    bool isrelay = (App.Current.Resources["Locator"] as ViewModelLocator).Paper.CablePaper.IsSafeBox == 1;
                    p.ProjectNO = (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Project.ProjectNO;
                    p.FixtureCode = (App.Current.Resources["Locator"] as ViewModelLocator).Paper.CablePaper.FixtureCode;
                    (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Pin = p;
                    CodeWindow codeWindow = new CodeWindow(isrelay);
                    codeWindow.ShowDialog();
                    tempEp.Name ="P"+ p.PinCode;
                    //canvas.RegisterName(tempEp.Name, tempEp);
                    tempEp.Fill = TempBruhes;
                }
                else
                {
                    string projectno= (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Project.ProjectNO;
                    Pin p = SQliteDbContext.GetOnePin(tempEp.Name.Substring(1, tempEp.Name.Length - 1),projectno);
                    if (p == null)
                        return;
                    p.PinEX = canvas.RenderSize.Width;
                    p.PinEY = canvas.RenderSize.Height;
                  
                    p.PinSX = Canvas.GetLeft(tempEp);
                    p.PinSY = Canvas.GetTop(tempEp);

                    p.Radius = tempEp.ActualHeight;
                    (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Pin = p;
                    bool isrelay = (App.Current.Resources["Locator"] as ViewModelLocator).Paper.CablePaper.IsSafeBox == 1;
                    CodeWindow codeWindow = new CodeWindow(isrelay);
                    codeWindow.IsModify = true;
                    codeWindow.ShowDialog();
                   // tempEp.Name ="P"+ p.PinCode;
                }

            }
        }
        #endregion

        #region ViewModeMethod

        private void ChangePaper(List<Pin> pins)
        {
            this.canvas.Children.Clear();
            foreach (var item in pins)
            {
                try
                {
                    canvas.UnregisterName("P" + item.PinCode);
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                }
                
            }
            foreach (var item in pins)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Height = item.Radius == 0 ? 12 : item.Radius;
                ellipse.Width = item.Radius == 0 ? 12 : item.Radius;
                ellipse.Fill = Brushes.Red;
                ellipse.SetValue(Canvas.TopProperty, (item.PinEY / this.canvas.RenderSize.Height) * item.PinSY);
                ellipse.SetValue(Canvas.LeftProperty, (item.PinEX / this.canvas.RenderSize.Width) * item.PinSX);
                
                ellipse.Name = "P" + item.PinCode;
                
                canvas.RegisterName(ellipse.Name, ellipse);
                ellipse.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
                ellipse.MouseMove += Ellipse_MouseMove;
                ellipse.MouseUp += Ellipse_MouseUp;

                canvas.Children.Add(ellipse);
            }
        }
        #endregion

        /// <summary>
        /// 操作菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string header = (sender as MenuItem).Header.ToString();
            Messenger.Default.Send<string>(header,"MenuClick");
        }

        /// <summary>
        /// 清空列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            string pno=(App.Current.Resources["Locator"] as ViewModelLocator).Paper.Project.ProjectNO;
            string fixno = (App.Current.Resources["Locator"] as ViewModelLocator).Paper.CablePaper.FixtureCode;
            SQliteDbContext.DeleteALLPin(pno, fixno);
            (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Pins.Clear();
        }

       
       
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            MenuItem item = (sender as MenuItem);
            string header= item.Header.ToString();
            switch (header)
            {
                case "选择模型": Operator = Operation.Select; break;
                case "修改地址":
                    Operator = Operation.Modify;
                    break;
                case "删除模型": Operator = Operation.Delete; break;
                default:
                    break;
            }
        }

        #region DataGridEvent

        private void Paperdata_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            PinEx pin = e.Row.Item as PinEx;
            if ((App.Current.Resources["Locator"] as ViewModelLocator).Paper.Pins.Count(x=>x.PhysicalChannel==pin.PhysicalChannel)>1)
            {
                MessageBox.Show("物理地址不能重复!", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Cancel = true;

             
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tempEp != null)
                tempEp.Fill = TempBruhes;
            SelectedpinEx = (this.paperdata.SelectedItem as PinEx);
            if (SelectedpinEx == null)
                return;
            string name = "P" + SelectedpinEx.PinCode;
            // tempEp = canvas.FindName(name) as Ellipse;
            tempEp = GetChildObject<Ellipse>(canvas, name);
            if (tempEp != null)
            {
                TempBruhes = tempEp.Fill;
                tempEp.Fill = Brushes.LawnGreen;
            }

        }

        private void MenuItem_Delete(object sender, RoutedEventArgs e)
        {
            int rs = SQliteDbContext.DeleteOnePin(SelectedpinEx.PinCode, SelectedpinEx.ProjectNO);
            if (rs > 0)
            {
                this.canvas.Children.Remove(tempEp);
                (App.Current.Resources["Locator"] as ViewModelLocator).Paper.Pins.Remove(SelectedpinEx);
                tempEp = null;
                SelectedpinEx = null;
            }
        }

        private void MenuItem_Update(object sender, RoutedEventArgs e)
        {
            int count = SQliteDbContext.CheckAddressSingle(SelectedpinEx);
            int ct = SQliteDbContext.GetOnePin(SelectedpinEx.PinCode, SelectedpinEx.ProjectNO).physicalChannel;
            if (count >= 1 && ct != SelectedpinEx.physicalChannel)
            {
                MessageBox.Show("物理地址已经被占用!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }
            int rs = SQliteDbContext.UpdatOnePin(SelectedpinEx);

        }

        #endregion

        public T GetChildObject<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject child = null;
            T grandChild = null;

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);

                if (child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)child;
                }
                else
                {
                    grandChild = GetChildObject<T>(child, name);
                    if (grandChild != null)
                        return grandChild;
                }
            }
            return null;
        }


    }

    public enum Operation
    {
        None = 0,
        Create = 1,
        Modify = 2,
        Move = 3,
        Select = 4,
        Delete = 5,
    }
}
