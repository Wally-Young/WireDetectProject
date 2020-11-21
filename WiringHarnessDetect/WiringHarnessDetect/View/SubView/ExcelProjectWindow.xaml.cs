using Panuon.UI.Silver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WiringHarnessDetect.Common;
using WiringHarnessDetect.Model;
using WiringHarnessDetect.ViewModel;
using MessageBox = System.Windows.MessageBox;

namespace WiringHarnessDetect.View.SubView
{
    /// <summary>
    /// ExcelProjectWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExcelProjectWindow : WindowX
    {
        public ExcelProjectWindow(WindowOperation operation)
        {
            InitializeComponent();
           
            if (operation != WindowOperation.Add)
            {

                projectNO.IsReadOnly = true;
                GridFile.Visibility = Visibility.Collapsed;


            }

            switch (operation)
            {
                case WindowOperation.Add:
                    btnConfirm.Content = "添加";
                    break;
                case WindowOperation.Update:
                    btnConfirm.Content = "更新";
                    this.Title = "修改工程";
                    
                    break;
                case WindowOperation.Delete:
                    btnConfirm.Content = "删除";
                    this.Title = "删除工程";
                    break;
                default:
                    break;
            }

            this.Operation = operation;
        }

        public WindowOperation Operation { get; set; } //操作类型

        private string filepath = "";


        private bool QualityCheck()
        {
            if (carType.SelectedIndex==-1)
            {
                MessageBox.Show("请选择车型名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                carType.Focus();
                return false;
            }
            if (wireName.SelectedIndex==-1)
            {
                MessageBox.Show("请输入车型编号", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                wireName.Focus();
                return false;
            }
            if(projectName.Text.Trim().Length==0)
            {
                MessageBox.Show("请输入工程名称", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                projectName.Focus();
                return false;
            }
            if (projectNO.Text.Trim().Length == 0)
            {
                MessageBox.Show("请输入工程编号", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                projectName.Focus();
                return false;
            }
            if (Operation == WindowOperation.Add && SQliteDbContext.CheckCarNOExist(projectNO.Text.Trim()))
            {
                MessageBox.Show("车型编号已经存在请重新输入", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                projectName.Focus();
                return false;
            }
            if(filename.Text.Trim().Length==0&& Operation == WindowOperation.Add)
            {
                MessageBox.Show("请导入文件路径", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                projectName.Focus();
                return false;
            }
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as System.Windows.Controls.Button).Tag.ToString().ToLower();
            if (tag == "scan")
            {
                string filePathName = "";//定义图像文件的位置（包括路径及文件名）
                System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog(); //添加打开对话框
                ofd.Filter = "Excel文件|*.xls;*.xlsx";  //设置过滤器
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)//如果确定打开图片，则保存文件的路径及文件名到字符串变量filePathName中
                {
                    filePathName = ofd.FileName;  //包括路径和文件名                                              // filepath =
                    this.filename.Text = ofd.FileName;
                    filepath = ofd.FileName;
                }
            }
            else if (tag == "confirm")
            {
                ExcelProject project = (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Project;
                if (Operation == WindowOperation.Add)
                {
                    if (QualityCheck())
                    {
                        project.Author = (App.Current.Resources["Locator"] as ViewModelLocator).Main.User.UserID;
                        ImportExcel(project.ProjectNO, project.ProjectName, filepath);
                        if(SQliteDbContext.GetAllExcelProject().Exists(x=>x.ProjectNO==project.ProjectNO))
                        {
                            MessageBox.Show("工程编号已经存在!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.projectNO.Focus();
                            return;
                        }
                        int r = SQliteDbContext.AddExcelProject(project);
                        if (r > 0)
                        {
                            MessageBox.Show("添加成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);

                            this.Close();
                        }
                    }

                }
                else if (Operation == WindowOperation.Update)
                {
                    QualityCheck();
                    int r = SQliteDbContext.UpdateExcelProject(project);
                    if (r > 0)
                    {
                        MessageBox.Show("更新成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        this.Close();
                    }
                }
                else if (Operation == WindowOperation.Delete)
                {
                    QualityCheck();
                    int r = SQliteDbContext.DeleteExcelProject(project);
                    if (r > 0)
                    {
                        MessageBox.Show("删除成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                     
                        this.Close();
                    }
                }
                 (App.Current.Resources["Locator"] as ViewModelLocator).Main.ExProjects = new System.Collections.ObjectModel.ObservableCollection<ExcelProject>(SQliteDbContext.GetAllExcelProject());

            }
            else if(tag=="cancel")
            {
                this.Close();
            }

        }


        /// <summary>
        /// 判断文件夹是否存在
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static bool folderExist(string path)
        {
            bool response = false;

            if (Directory.Exists(path))
            {
                response = true;
            }
            return response;
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path">路径</param>
        public static void folderCreate(string path)
        {
            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// 判断字符串是否是数字
        /// </summary>
        public static bool IsNumber(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            const string pattern = "^[0-9]*$";
            Regex rx = new Regex(pattern);
            return rx.IsMatch(s);
        }


        /// <summary>
        /// 导入Excel
        /// </summary>
        private void ImportExcel(string projectNO, string ProjectName, string path)
        {
            DataSet ds = null;
            System.Data.DataTable dt = null;

            //try
            {
                {
                    #region Part
                    string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path + ";Extended Properties='Excel 12.0;HDR=Yes;IMEX=1;'";
                    OleDbConnection myConn = new OleDbConnection(strConn);
                    myConn.Open();
                    OleDbDataAdapter myCommand = null;
                    string strExcel = "select * from [sheet1$]";//这里sheet1对应excel的工作表名称，一定要注意。
                    myCommand = new OleDbDataAdapter(strExcel, strConn);
                    ds = new DataSet();
                    myCommand.Fill(ds, "table1");
                    dt = ds.Tables[0];
                    List<Part> projects = new List<Part>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        for (int i = 1; i < dt.Columns.Count - 1; i++)
                        {
                            if (dr[i].ToString() != ""&&dr[i].ToString().Trim().Length>0)
                            {
                                Part part = new Part() { PartType = dr[0].ToString().Trim(), ProjectNO = projectNO, Circuit = dt.Columns[i].ToString().Trim() };
                                projects.Add(part);
                            }
                        }

                    }
                    var names = projects.Select(x => x.PartType).Distinct().ToList();
                    foreach (var item in names)
                    {
                        if (SQliteDbContext.CheckPartNameExist(item))
                        {
                            System.Windows.MessageBox.Show($"{item}型号已经存在,请删除该零件新后再导入", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }



                    #endregion

                    #region Fixture
                    string sql2 = "select * from [sheet3$]";//这里sheet1对应excel的工作表名称，一定要注意。
                    myCommand = new OleDbDataAdapter(sql2, strConn);
                    ds = new DataSet();
                    myCommand.Fill(ds, "table1");
                    dt = ds.Tables[0];
                    if (dt.Columns.Count < 5)
                    {
                        System.Windows.MessageBox.Show($"Sheet3缺少列,请检测列信息是否完成", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    var fixtures = dt.Rows.Cast<DataRow>().Where(x => x.ItemArray[0].ToString().Trim().Length > 0).Select(r => new FixtureInfo
                    {
                        ProjectNO = projectNO,
                        FixtureType = r[0].ToString().Trim(),
                        PinNO = r[1].ToString().Trim(),
                        FixtureNO = r[3].ToString().Trim(),
                       // PhysicalChannel = r[2].ToString().Trim().Length == 0 ? 0 : Convert.ToInt32(r[1].ToString()),
                        PinIndex = Convert.ToInt32(r[4].ToString()),


                    }
                    ).ToList();

                    #endregion

                    #region Connect 
                    string Sql3 = "select * from [sheet2$]";//这里sheet1对应excel的工作表名称，一定要注意。
                    myCommand = new OleDbDataAdapter(Sql3, strConn);
                    ds = new DataSet();
                    myCommand.Fill(ds, "table1");
                    dt = ds.Tables[0];

                    var data = dt.AsEnumerable().ToList().Select(x => new WireConnect()
                    {
                        Wire = x[1].ToString().Trim(),
                        Option = x[2].ToString().Trim(),
                        From = x[3].ToString().Trim(),
                        FromPin = x[4].ToString().Trim(),
                        To = x[5].ToString().Trim(),
                        ToPin = x[6].ToString().Trim(),
                    }).ToList();
                    List<HashSet<WireConnect>> wireConnects = new List<HashSet<WireConnect>>();
                    for (int i = 0; i < data.Count; i++)
                    {
                        List<WireConnect> wires = new List<WireConnect>();
                        if (data[i].IsDeal)
                            continue;
                        wires.AddRange(data.FindAll(x => x.Equals(data[i])));
                        data[i].IsDeal = true;
                        if (wires.Count > 0)
                        {
                            for (int j = 0; j < wires.Count; j++)
                            {
                                if (wires[j].IsDeal)
                                    continue;
                                wires.AddRange(data.FindAll(x => x.Equals(wires[j])));
                                wires[j].IsDeal = true;
                            }
                            wires.ForEach(x => x.Num = data[i].Wire);
                            wireConnects.Add(new HashSet<WireConnect>(wires));
                        }
                    }

                    //将From to 拆分成单个的两个归属到治具里
                    var ctlst = data.Select(r => new ConnectInfoEx
                    {
                        ProjectNO = projectNO,
                        WireNum = r.Num.Trim(),
                        CirCuitName=r.Option.Trim(),
                        Pins=new List<BasePin>()
                        {
                            new BasePin() { PinIndex = Convert.ToInt32(r.FromPin.ToString().Trim()), FixtureNO = r.From.ToString().Trim()},
                            new BasePin() { PinIndex = Convert.ToInt32(r.ToPin.ToString().Trim()), FixtureNO = r.To.ToString().Trim()},
                        }
                    });

                    //筛选出合并点的信息
                    List<ConnectionInfo> result = new List<ConnectionInfo>();
                    if (!rd.IsChecked.Value)
                    {
                        result = ctlst.SelectMany(p => p.Pins, (p, r) => new ConnectionInfo
                        {
                            CirCuitName = p.CirCuitName,
                            ProjectNO = p.ProjectNO,
                            WireNum = p.WireNum,
                            FixtureNO = r.FixtureNO,
                            PinIndex = r.PinIndex,
                        }).Where(n => !System.Text.RegularExpressions.Regex.IsMatch(n.FixtureNO, @"^[a-zA-Z]{3}")).ToList();
                    }
                    else if (rd.IsChecked.Value && rd.IsChecked.HasValue)
                    {
                        if (patter.Text.Trim().Length == 0)
                        {
                            MessageBox.Show("请输入需要匹配的合点字母", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        result = ctlst.SelectMany(p => p.Pins, (p, r) => new ConnectionInfo
                        {
                            CirCuitName = p.CirCuitName,
                            ProjectNO = p.ProjectNO,
                            WireNum = p.WireNum,
                            FixtureNO = r.FixtureNO,
                            PinIndex = r.PinIndex,


                        }).Where(n => !n.FixtureNO.StartsWith(patter.Text.Trim())).ToList();
                    }
                    //给每个点赋与物理地址
                    result.ForEach(x =>
                    {
                        var rs = fixtures.ToList().Find(r => r.FixtureNO == x.FixtureNO && r.PinIndex == x.PinIndex);
                        x.PhysicalChannel = rs.PhysicalChannel;
                        x.FixtureType = rs.FixtureType;
                        x.PinNO = rs.PinNO;

                    }
                    );

                    #region
                    ////将From to 拆分成单个的两个归属到治具里
                    //var ctlst = dt.Rows.Cast<DataRow>().Where(x => x.ItemArray[1].ToString() != "").Select(r => new ConnectInfoEx
                    //{
                    //    ProjectNO = projectNO,
                    //    WireNum = r[1].ToString().Trim(),
                    //    CirCuitName = r[2].ToString().Trim(),

                    //    Pins = new List<BasePin>() { new BasePin() { PinIndex = Convert.ToInt32(r[4].ToString().Trim()), FixtureNO = r[3].ToString().Trim()},
                    // new BasePin() { PinIndex = Convert.ToInt32(r[6].ToString().Trim()), FixtureNO = r[5].ToString().Trim()},}
                    //}).ToList();



                    #endregion

                    #endregion




                    //实体线束（零件图号）
                    string sql = "Insert into ExcelPart(ProjectNO,PartType,Circuit)Values(@ProjectNO,@PartType,@Circuit)";
                    SQliteDbContext.MultiInsert<Part>(sql, projects);
                    //治具信息
                    string sqlFixtureAdd = "Insert into ExcelFixturePinInfo(ProjectNO,FixtureType,FixtureNO,PinNO, PinIndex)Values(@ProjectNO,@FixtureType,@FixtureNO,@PinNO,@PinIndex)";
                    SQliteDbContext.MultiInsert<FixtureInfo>(sqlFixtureAdd, fixtures);

                    //连接点
                    string sqlconn = "Insert into ExcelConnect(WireNum,CirCuitName,ProjectNO,FixtureNO,FixtureType,PinIndex,PinNO)Values(@WireNum,@CirCuitName,@ProjectNO,@FixtureNO,@FixtureType,@PinIndex,@PinNO)";
                    SQliteDbContext.MultiInsert<ConnectionInfo>(sqlconn, result);
                }
            }


          // catch (Exception ex)
            {

              //  MessageBox.Show($"导入数据出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
        }

        private void WindowX_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (projectName.Text.Trim().Length == 0 || projectNO.Text.Trim().Length == 0)
                (App.Current.Resources["Locator"] as ViewModelLocator).ExcelPaper.Project = null;

        }
    }


    public class WireConnect
    {
        public string Wire { get; set; }
        public string Option { get; set; }
        public string ToPin { get; set; }
        public string FromPin { get; set; }
        public string From { get; set; }

        public string To { get; set; }

        public bool IsDeal { get; set; }

        public string Num { get; set; }
        public override string ToString()
        {
            return $"{ String.Format("{0,10}", Wire)} {String.Format("{0,10}", Option)} {String.Format("{0,10}", ToPin)} {String.Format("{0,10}", FromPin)} {String.Format("{0,10}", From)} {String.Format("{0, 10}", To)}";
        }

        public override bool Equals(object obj)
        {
            if (obj is WireConnect)
            {
                WireConnect wire = obj as WireConnect;
                bool isTrue = false;
                if (Regex.IsMatch(To, "^[A-Za-z]?"))
                {
                    if ((To == wire.To && ToPin == wire.ToPin) || (To == wire.From && ToPin == wire.FromPin))
                        return isTrue = true;
                    else
                        isTrue = false;
                }
                if (Regex.IsMatch(From, "^[A-Za-z]?"))
                {
                    if ((From == wire.To && FromPin == wire.ToPin) || (From == wire.From && FromPin == wire.FromPin))
                        return isTrue = true;
                    else
                        isTrue = false;
                }
                if (Regex.IsMatch(From, "^[A-Za-z]{3}"))
                {
                    if (From == wire.From || From == wire.To)
                        return isTrue = true;
                    else
                        isTrue = false;
                }
                if (Regex.IsMatch(To, "^[A-Za-z]{3}"))
                {
                    if (To == wire.From || To == wire.To)
                        return isTrue = true;
                    else
                        isTrue = false;
                }

                return isTrue;

            }
            else
            {
                return false;
            }

        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        // override object.Equals


    }
}
