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
using WiringHarnessDetect.Common;
using WiringHarnessDetect.Model;
using WiringHarnessDetect.View.SubView;
using WiringHarnessDetect.ViewModel;

namespace WiringHarnessDetect.View
{
    /// <summary>
    /// ExcelManagerView.xaml 的交互逻辑
    /// </summary>
    public partial class ExcelManagerView : Page
    {
        public ExcelManagerView()
        {
            InitializeComponent();
            Messenger.Default.Register<List<ExcelPin>>(this, "clearEx", p => ChangePaper(p));//清空点
        }


        #region Field
        private Operation Operator;
        private bool mousedown = false;
        private Ellipse tempEp;
        private Point StartPoint;
        private Brush TempBruhes;
        private Model.ExcelPin SelectedpinEx;
        #endregion

        #region OperatorButtonEvent
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Button btn = sender as Button;
            string tag = btn.Tag.ToString().ToLower();
            switch (tag)
            {
                
                case "create": Operator = Operation.Create; break;
                case "move": Operator = Operation.Move; break;
                case "modify":
                    Operator = Operation.Modify;
                    break;
                case "delete": Operator = Operation.Delete; break;
                case "clsall": 
               
                    SQliteDbContext.DeleteAllExPin((App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixture.FixtureType);
                    (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pins.Clear();
                    foreach (var item in canvas.Children)
                    {
                        try
                        {
                            if (item is Ellipse)
                            {
                                var ep = item as Ellipse;
                                canvas.UnregisterName(ep.Name);
                            }


                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine(ex.Message);
                        }

                    }
                    this.canvas.Children.Clear();
                    break;
                case "clear":
                    (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Channels.Clear();break;
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
                int value = 0;
                int.TryParse(txtDia.Text, out value);
                if (cbDia.IsChecked.HasValue && cbDia.IsChecked.Value)
                {
                    ellipse.Height = value;
                    ellipse.Width = value;
                }
                else
                {
                    ellipse.Height = 20;
                    ellipse.Width = 20;
                   
                }
                if (cbDia.IsChecked.HasValue && cbColor.IsChecked.Value)
                    ellipse.Fill = clpk.SelectedBrush;
                else
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
            if (ep.Name != null && ep.Name.Trim().Length > 0)
            {
                string name = ep.Name.Remove(0, 1);
                
                ExcelPin p = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pins.ToList().Find(x => x.PinNO == name);
                if (p == null)
                    return;
                p.PinSX = Canvas.GetLeft(ep);
                p.PinSY = Canvas.GetTop(ep);
                SQliteDbContext.UpdatOneExPin(p);
            }
            if (ep.Name == ""&& Operator == Operation.Create)
                {
                    ExcelPin p = new ExcelPin();
                    p.PinEX = canvas.RenderSize.Width;
                    p.PinEY = canvas.RenderSize.Height;
                    p.PinSX = Canvas.GetLeft(ep);
                    p.PinSY = Canvas.GetTop(ep);
                    p.Radius = ep.ActualHeight;
                    
                    if (ep.Fill.ToString().Trim().Length > 0 && ep.Fill.ToString() != "0")
                        p.DrawColor = ep.Fill.ToString();
                    else
                        p.DrawColor = "#FF0000";

                    if ((App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixture == null)
                    {
                        MessageBox.Show("请选择一个治具型号后再创建!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    p.FixtureType = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixture.FixtureType;
                 var pnm= (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pins.OrderBy(x =>Convert.ToInt16( x.PinNO)).LastOrDefault();
                int pn = 0;
                if (pnm!=null)
                {
                    string pno = pnm.PinNO;
                    int.TryParse(pno, out pn);
                }
                  
                  
                   p.PinNO = (pn + 1).ToString();
                  p.PhysicalChannel = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Channels.FirstOrDefault();
                (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pin = p;
                    ExcelPinWindow codeWindow = new ExcelPinWindow(false);
                    codeWindow.ShowDialog();
                    // tempEp.Name = "P" + p.PinNO;
                    //canvas.RegisterName(tempEp.Name, tempEp);
                if(codeWindow.IsAdd)
                {
                    tempEp = ep;
                    ep.Name = "P" + p.PinNO;

                    canvas.RegisterName(ep.Name, ep);
                }
                else
                {
                    ep.Fill = null;
                }
                    
                }
               

         
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
        {
            Ellipse ep = sender as Ellipse;
            if (mousedown && Operation.Move == Operator)
            {
                double deltav = e.GetPosition(canvas).Y;
                double deltah = e.GetPosition(canvas).X;
                ep.SetValue(Canvas.TopProperty, deltav - ep.Height / 2);
                ep.SetValue(Canvas.LeftProperty, deltah - ep.Height / 2);

            }
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse ep = sender as Ellipse;
            mousedown = true;
            if (Operator != Operation.Create)
            {
                if (tempEp != null)
                    tempEp.Fill = TempBruhes;
                tempEp = ep;
                TempBruhes = ep.Fill;
                ep.Fill = Brushes.LawnGreen;

                ep.CaptureMouse();
            }
            if (Operator == Operation.Delete)
            {
                ep.ReleaseMouseCapture();
                canvas.UnregisterName(ep.Name);
                canvas.Children.Remove(ep);
               
                if (ep.Name != null && ep.Name != "")
                {
                    SQliteDbContext.DeleteOneExPin(ep.Name.Substring(1, ep.Name.Length - 1), (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixture.FixtureType);
                    ExcelPin p = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pins.ToList().Find(x => x.PinNO == ep.Name.Substring(1, ep.Name.Length - 1));
                    (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pins.Remove(p);


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
                    ExcelPin p = new ExcelPin();
                    p.PinEX = canvas.RenderSize.Width;
                    p.PinEY = canvas.RenderSize.Height;
                    p.PinSX = Canvas.GetLeft(tempEp);
                    p.PinSY = Canvas.GetTop(tempEp);
                    p.Radius = tempEp.ActualHeight;
                    if (tempEp.Fill.ToString().Trim().Length > 0 && tempEp.Fill.ToString() != "0")
                        p.DrawColor = tempEp.Fill.ToString();
                    else
                        p.DrawColor = "#FF0000";

                    if((App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixture==null)
                    {
                        MessageBox.Show("请选择一个治具型号后再创建!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    p.FixtureType = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixture.FixtureType;
                    (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pin = p;
                    ExcelPinWindow codeWindow = new ExcelPinWindow(false);
                    codeWindow.ShowDialog();
                   // tempEp.Name = "P" + p.PinNO;
                    //canvas.RegisterName(tempEp.Name, tempEp);
                    tempEp.Fill = TempBruhes;
                }
                else
                {

                    ExcelPin p = SQliteDbContext.GetOneExPin((App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixture.FixtureType, tempEp.Name.Substring(1, tempEp.Name.Length - 1));
                    if (p == null)
                        return;
                    p.PinEX = canvas.RenderSize.Width;
                    p.PinEY = canvas.RenderSize.Height;
                    p.PinSX = Canvas.GetLeft(tempEp);
                    p.PinSY = Canvas.GetTop(tempEp);
                    p.Radius = tempEp.ActualHeight;
                    (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pin = p;


                    ExcelPinWindow codeWindow = new ExcelPinWindow(true);
                  
                  //  SQliteDbContext.UpdatOneExPin(p);
                    codeWindow.ShowDialog();
                    tempEp.Name = "P" + p.PinNO;
                }

            }
        }
        #endregion

        #region ViewModeMethod

        private void ChangePaper(List<ExcelPin> pins)
        {
           
            foreach (var item in canvas.Children)
            {
                try
                {
                    if (item is Ellipse)
                    {
                        var ep = item as Ellipse;
                        canvas.UnregisterName(ep.Name);
                    }
                    
                    
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                }

            }
            this.canvas.Children.Clear();
            foreach (var item in pins)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Height =item.Radius==0?20:item.Radius;
                ellipse.Width = item.Radius==0?20:item.Radius;
                ellipse.Fill = Brushes.Red;
                ellipse.SetValue(Canvas.TopProperty, (item.PinEY / this.canvas.RenderSize.Height) * item.PinSY);
                ellipse.SetValue(Canvas.LeftProperty, (item.PinEX / this.canvas.RenderSize.Width) * item.PinSX);

                ellipse.Name = "P" + item.PinNO;

                canvas.RegisterName(ellipse.Name, ellipse);
                ellipse.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
                ellipse.MouseMove += Ellipse_MouseMove;
                ellipse.MouseUp += Ellipse_MouseUp;

                canvas.Children.Add(ellipse);
            }
        }
        #endregion

   
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tempEp != null)
                tempEp.Fill = TempBruhes;
            SelectedpinEx = (this.paperdata.SelectedItem as ExcelPin);
            if (SelectedpinEx == null)
                return;
            string name = "P" + SelectedpinEx.PinNO;
            // tempEp = canvas.FindName(name) as Ellipse;
            tempEp = GetChildObject<Ellipse>(canvas, name);
            if (tempEp != null)
            {
                TempBruhes = tempEp.Fill;
                tempEp.Fill = Brushes.LawnGreen;
            }

        }
   
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            string name=  (sender as MenuItem).Header.ToString();
            WindowOperation operation=WindowOperation.Add;
            if(name.Contains("车型"))
            {
                CarProject car = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Car;

                switch (name.Substring(0,2))
                {
                    case "新建":
                        cmbCType.SelectedIndex = -1;
                        (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Car = new CarProject();
                        operation = WindowOperation.Add;
                        break;
                    case "修改":
                        if(car==null)
                        {
                            MessageBox.Show("请选择一个车型!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        operation = WindowOperation.Update;
                        break;
                    case "删除":
                        if (car == null)
                        {
                            MessageBox.Show("请选择一个车型!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        operation = WindowOperation.Delete;
                        break;
                    
                    default:
                        break;
                }

                CarTypeWindow carTypeWindow = new CarTypeWindow(operation);
                carTypeWindow.ShowDialog();
            }
            else if(name.Contains("线束"))
            {
                WireType wire = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Wire;
                switch (name.Substring(0, 2))
                {
                    case "新建":
                        partcmb.SelectedIndex = -1;
                        (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Wire = new WireType();
                        operation = WindowOperation.Add;
                        break;
                      
                    case "修改":
                        if (wire == null)
                        {
                            MessageBox.Show("请选择一个线束!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        operation = WindowOperation.Update;
                        break;
                    case "删除":
                        if (wire == null)
                        {
                            MessageBox.Show("请选择一个线束!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        operation = WindowOperation.Delete;
                        break;

                    default:
                        break;
                }
                WireWindow wireWindow = new WireWindow(operation);
                wireWindow.ShowDialog();
            }
            else if(name.Contains("工程"))
            {
                ExcelProject project = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Project;
                switch (name.Substring(0, 2))
                {
                    case "新建":
                        projectcmb.SelectedIndex = -1;
                        (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Project = new ExcelProject();
                        operation = WindowOperation.Add;
                        break;

                    case "修改":
                        if (project == null)
                        {
                            MessageBox.Show("请选择一个工程!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        operation = WindowOperation.Update;
                        break;
                    case "删除":
                        if (project == null)
                        {
                            MessageBox.Show("请选择一个工程!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        operation = WindowOperation.Delete;
                        break;

                    default:
                        break;
                }
                ExcelProjectWindow excelProjectWindow = new ExcelProjectWindow(operation);
                excelProjectWindow.ShowDialog();
            }
            else if(name.Contains("治具"))
            {
                FixtureBase fixture = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixture;
                switch (name.Substring(0, 2))
                {

                    case "新建":
                        cmbFixture.SelectedIndex = -1;
                        (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Project = new ExcelProject();
                        operation = WindowOperation.Add;
                        break;

                    case "修改":
                        if (fixture == null)
                        {
                            MessageBox.Show("请选择一个治具!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        operation = WindowOperation.Update;
                        break;
                    case "删除":
                        if (fixture == null)
                        {
                            MessageBox.Show("请选择一个治具!", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        operation = WindowOperation.Delete;
                        break;

                    default:
                        break;
                }
                ExFixtureWindow fixtureWindow = new ExFixtureWindow(operation);
                fixtureWindow.ShowDialog();
            }
        }

        private void MenuItem_Click_Modify(object sender, RoutedEventArgs e)
        {

                ExcelPin p = SQliteDbContext.GetOneExPin((App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixture.FixtureType, tempEp.Name.Substring(1, tempEp.Name.Length - 1));
                if (p == null)
                    return;
                p.PinEX = canvas.RenderSize.Width;
                p.PinEY = canvas.RenderSize.Height;
                p.PinSX = Canvas.GetLeft(tempEp);
                p.PinSY = Canvas.GetTop(tempEp);
                p.Radius = tempEp.ActualHeight;
                (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pin = p;


                ExcelPinWindow codeWindow = new ExcelPinWindow(true);

                //  SQliteDbContext.UpdatOneExPin(p);
                codeWindow.ShowDialog();
                tempEp.Name = "P" + p.PinNO;
            
        }

        private void MenuItem_Click_Delete(object sender, RoutedEventArgs e)
        {
            if (tempEp.Name != null && tempEp.Name != "")
            {
                canvas.UnregisterName(tempEp.Name);
                this.canvas.Children.Remove(tempEp);
               
                SQliteDbContext.DeleteOneExPin(tempEp.Name.Substring(1, tempEp.Name.Length - 1), (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Fixture.FixtureType);
                ExcelPin p = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pins.ToList().Find(x => x.PinNO == tempEp.Name.Substring(1, tempEp.Name.Length - 1));
                (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Pins.Remove(p);


            }
        }

     
        private void MenuItem_Click_ON(object sender, RoutedEventArgs e)
        {
            byte[] msg = new byte[] { 0xfe, 0xef, 0x30, 0x0c, 0x00,0x02, };
            var pin = paperdata.SelectedItem as ExcelPin;
            if(pin!=null)
            {
                short led =  (short)SQliteDbContext.GetOneFixtureBaseInfo(pin.FixtureType).LEDAddress;
                byte[] addr=  BitConverter.GetBytes(led).Reverse().ToArray();
                List<byte> datas = new List<byte>();
                datas.AddRange(msg);
                datas.AddRange(addr);
                Messenger.Default.Send<byte[]>(datas.ToArray(), "Send");
            }
          
        }

        private void MenuItem_Click_OFF(object sender, RoutedEventArgs e)
        {
            byte[] msg = new byte[] { 0xfe, 0xef, 0x30, 0x0c, 0x00, 0x02, 0x00, 0x00 };
            Messenger.Default.Send<byte[]>(msg, "Send");
        }

        private void MenuItem_Click_AllON(object sender, RoutedEventArgs e)
        {
            byte[] msg = new byte[] { 0xfe, 0xef, 0x30, 0x0c, 0x00, 0x02, 0xff, 0xff };
            Messenger.Default.Send<byte[]>(msg, "Send");
        }


        ///// <summary>
        ///// 编辑零件图纸
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void BtnEditPaper(object sender, RoutedEventArgs e)
        //{
        //    PartWindow partWindow = new PartWindow();
        //    partWindow.ShowDialog();
        //}

        //private void BtnImport(object sender, RoutedEventArgs e)
        //{
        //    string tag = (sender as Button).Tag.ToString();
        //    switch (tag)
        //    {
        //        case "createType":
        //            CarTypeWindow carTypeWindow = new CarTypeWindow();
        //            carTypeWindow.ShowDialog();
        //            break;
        //        case "import":
        //            PartWindow partWindow = new PartWindow();
        //            partWindow.ShowDialog();
        //            break;
        //        case "image":
        //             PartImageWindow partWindowimage = new PartImageWindow();
        //            partWindowimage.ShowDialog();
        //            break;
        //        case "pin":
        //            if(partcmb.SelectedIndex==-1)
        //            {
        //                MessageBox.Show("请选择一个线束", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        //                return;
        //            }
        //            if (cmbCType.SelectedIndex == -1)
        //            {
        //                MessageBox.Show("请选择一个零件图号", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        //                return;
        //            }
        //            ExcelPinWindow pinwindow = new ExcelPinWindow((App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Project, cmbCType.SelectedItem.ToString());
        //            pinwindow.ShowDialog();
        //            break;
        //        default:
        //            break;
        //    }

        //}
    }

}

