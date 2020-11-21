using WiringHarnessDetect.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Dapper;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

namespace WiringHarnessDetect
{
    public class SQliteDbContext
    {
        private static string LoadConnectString(string id = "SqliteTest")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }

        #region User
        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static User GetUser(User user)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                return cnn.Query<User>("select * from Users where UserID=@UserID and Password=@Password", user).SingleOrDefault();

            }
        }
        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<User> GetAllUser()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                return cnn.Query<User>("select * from Users" ).ToList();

            }
        }

        public static User GetUser(string empID)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                return cnn.Query<User>("select * from User where EmployeeNumbe=@EmployeeNumbe", new User { UserName = empID }).SingleOrDefault();

            }
        }



        /// <summary>
        /// 更新登录时间
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int UpdateLoginTime(User user)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                return cnn.Execute("update Users set LastLoginTime=@LastLoginTime where UserID=@UserID", user);
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int UpdateUser(User user)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                return cnn.Execute("update Users set Pwd=@Pwd,Authority=@Authority,LastLoginTime=@LastLoginTime where EmployeeNumbe=@EmployeeNumbe", user);
            }
        }

        /// <summary>
        /// 删除用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int DeleteUser(User user)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                return cnn.Execute("Delete from Users  where EmployeeNumbe=@EmployeeNumbe", user);
            }
        }

        /// <summary>
        /// 删除用户信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int DeleteUser(string userID)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                return cnn.Execute($"Delete from Users  where UserID='{userID}'");
            }
        }
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="objUser">添加用户</param>
        /// <returns></returns>
        public static int AddUser(User objUser)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                return cnn.Execute($"Insert into Users(UserID,UserName,Password,Access)Values(@UserID,@UserName,@Password,@Access)",objUser);
            }

        }

        #endregion

        #region Project
        /// <summary>
        /// 获取所有项目
        /// </summary>
        /// <returns></returns>
        public static  List<Project> GetProjects()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                return cnn.Query<Project>($"Select  * from Project ").ToList();
            }
        }

        public static int  AddProject(Project project)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = "Insert into Project(ProjectName,ProjectNo,Author,Description,ProjectType)Values(@ProjectName,@ProjectNo,@Author,@Description,@ProjectType)";
                return cnn.Execute(sql,project);
            }
        }

        public static int DeleteProject(Project project)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = "delete from Project where ProjectNO=@ProjectNO";
                return cnn.Execute(sql, project);
            }
        }

        public static int UpdateProject(Project project)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = "Update  Project set ProjectName=@ProjectName,Author=@Author,Description=@Description,ProjectType=@ProjectType  where ProjectNO=@ProjectNO";
                return cnn.Execute(sql, project);
            }
        }
        #endregion

        #region Paper
        public static  List<CablePaper> GetPapers(string ProjectNO)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                return cnn.Query<CablePaper>($"Select  * from  Fixture  where ProjectNO= '{ProjectNO}' ").ToList();
            }
        }
        public static int DeletePaper(CablePaper paper)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = "delete from Fixture where FixtureCode=@FixtureCode";
                return cnn.Execute(sql, paper);
            }
        }

        public static int AddPaper(CablePaper paper)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = "Insert into Fixture(ProjectNO,FixtureName,FixtureCode,ImagePath,LEDAddress，IsSafeBox)" +
                    "Values(@ProjectNO,@FixtureName,@FixtureCode,@ImagePath,@LEDAddress,@IsSafeBox)";
                return cnn.Execute(sql, paper);
            }
        }

        public static int UpdatePaper(CablePaper paper)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = "Update  Fixture Set ProjectNO=@ProjectNO,FixtureName=@FixtureName,ImagePath=@ImagePath,LEDAddress=@LEDAddress，IsSafeBox=@IsSafeBox Where FixtureCode=@FixtureCode";
                return cnn.Execute(sql, paper);
            }
        }

        public static CablePaper  GetPaper(CablePaper paper)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = "Select * From Fixture Where FixtureCode=@FixtureCode";
                return cnn.Query<CablePaper>(sql, paper).SingleOrDefault();
            }
        }

        public static CablePaper GetPaper(string paperno)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = "Select * From Fixture Where FixtureCode=@FixtureCode";
                return cnn.Query<CablePaper>(sql, new CablePaper { FixtureCode=paperno}).SingleOrDefault();
            }
        }
        #endregion

        #region Pin
        public static int AddPin(Pin pin)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = "Insert into Pin(ProjectNO,FixtureCode,IsSafeBox,PinCode,physicalChannel,RelayControlChannel,SafeBoxName,DrawColor,PinSX,PinSY,PinEX,PinEY,Radius)" +
                    "Values(@ProjectNO,@FixtureCode,@IsSafeBox,@PinCode,@physicalChannel,@RelayControlChannel,@SafeBoxName,@DrawColor,@PinSX,@PinSY,@PinEX,@PinEY,@Radius)";
                return cnn.Execute(sql, pin);
            }
        }
       
        /// <summary>
        /// 获取一个引脚
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Pin GetOnePin(string projectCode, int Address)
        {
            string query = $"SELECT * FROM Pin  WHERE Pin.ProjectNO  = '{projectCode}' AND Pin.PhysicalChannel= {Address}";
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
                {
                    cnn.Open();
                    return cnn.Query<Pin>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取一个引脚
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Pin GetOnePin(string Pincode,string ProjectNO)
        {
            string query = "SELECT * FROM Pin WHERE PinCode = @PinCode and ProjectNO=@ProjectNO";
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                
                return cnn.Query<Pin>(query, new Pin(){ PinCode=Pincode,ProjectNO=ProjectNO}).SingleOrDefault();
            }
        }

        /// <summary>
        /// 获取同一个治具下的所有引脚
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<Pin> GetAllFixturePins( string fixtureCode)
        {
            string query = "SELECT * FROM Pin WHERE FixtureCode=@FixtureCode";
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();

                return cnn.Query<Pin>(query, new Pin() { FixtureCode = fixtureCode }).ToList();
            }
        }
       
        public static List<Pin> GetAllProjectPins(string projectcode)
        {
            string query = "SELECT * FROM Pin WHERE ProjectNO=@ProjectNO";
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();

                return cnn.Query<Pin>(query, new Pin() { ProjectNO = projectcode }).ToList();
            }
        }
        /// <summary>
        /// 删除一个引脚
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int DeleteOnePin(string Pincode, string ProjectNO)
        {
            string query = "Delete  from Pin WHERE PinCode = @PinCode and ProjectNO=@ProjectNO";
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();

                int rs= cnn.Execute(query, new Pin() { PinCode = Pincode, ProjectNO = ProjectNO });
                return rs;
            }
        }

        /// <summary>
        /// 删除所有个引脚
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int DeleteALLPin(string projectno,string FixtureCode)
        {
            string query = "Delete  from Pin WHERE FixtureCode = @FixtureCode and ProjectNO=@ProjectNO";
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();

                return cnn.Execute(query, new Pin() { FixtureCode = FixtureCode, ProjectNO = projectno });
            }
        }

        /// <summary>
        /// 更新一个pin
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        public static int UpdatOnePin(Pin pin)
        {
            string query = "Update  Pin Set PinSX=@PinSX,PinSY=@PinSY,PhysicalChannel=@PhysicalChannel  WHERE PinCode = @PinCode and ProjectNO=@ProjectNO";
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();  
                return cnn.Execute(query, pin);
            }
        }

        public static int CheckSafeBoxNameSingle(Pin pin)
        {
            string sql = $"SELECT * FROM Pin " +
                         $"WHERE SafeBoxName=@SafeBoxName and FixtureCode=@FixtureCode and ProjectNO=@ProjectNO";
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();

                return cnn.Query<Pin>(sql, pin).Count();
            }
        }

        public static bool CheckCodeSingle(Pin pin)
        {
            string sql = $"SELECT * FROM Pin " +
                        $"WHERE FixtureCode=@FixtureCode  and PinCode=@PinCode";
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();

                 int count= cnn.Query<Pin>(sql, pin).Count() ;
                return count > 0;
            }
           
        }
        public static int CheckAddressSingle(Pin pin)
        {
            string sql = $"SELECT * FROM Pin " +
                          $" WHERE physicalChannel=@physicalChannel and  ProjectNO=@ProjectNO";
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();

                return cnn.Query<Pin>(sql, pin).Count();
            }
        }
        public static List<PinEx> GetExPins(string fixtureCode, string projectCode)
        {
            try
            {
                string sql = $"SELECT Fixture.FixtureName,Project.ProjectName,Pin.PinCode,Pin.PhysicalChannel,Pin.FixtureCode,"
                             + "Pin.ProjectNO,Pin.RelayControlChannel,Pin.SafeBoxName,Pin.IsSafeBox FROM Pin "
                             + " INNER JOIN Project ON Pin.ProjectNO = Project.ProjectNO "
                             + "INNER JOIN Fixture ON Fixture.ProjectNO = Project.ProjectNO AND Pin.FixtureCode = Fixture.FixtureCode"+
                               $" Where Pin.ProjectNO='{projectCode}' and Pin.FixtureCode='{fixtureCode}'";
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
                {
                    cnn.Open();
                    var t= cnn.Query<PinEx>(sql);
                    return t.ToList();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion

     
        #region ExcelConnectInfo
        public static List<WireCircuit> GetAllWirCuit(string projectno)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                //cnn.Open();
                //string sql = $"Select * from ExcelConnect where ProjectNO ='{projectno}'";
                //return cnn.Query<ConnectInfo>(sql).GroupBy(r => Regex.Replace(r.WireNum, @"[^\d]*", "")).Select(group => new WireCircuit
                //{
                //    WireNum = group.Key,
                //    Connects = group.Select(r =>
                //          new WirePin
                //          {
                //              CirCuitName = r.CirCuitName,
                //              FixtureCode=r.
                //          }).ToList(),
                //}).ToList();
            }
            return new List<WireCircuit>();
        }

       
        #endregion
        public static int MultiInsert<T>(string sql,List<T> list) where T:class
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                var tran = cnn.BeginTransaction();
                int rows = cnn.Execute(sql, list, tran);
                tran.Commit();
                return rows;
            }
        }

       

        #region Excel 

        #region CarProject


        //添加
        public static int AddCarType(CarProject carType)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Insert into ExcelCarProject(CarName,CarNO,ModifyDate,Author)Values(@CarName,@CarNO,@ModifyDate,@Author)";
                return cnn.Execute(sql, carType);
            }
        }

        //删除
        public static int DeleteCarType(CarProject carType)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"PRAGMA foreign_keys = true;Delete  from  ExcelCarProject  Where CarNO=@CarNO";
                return cnn.Execute(sql, carType);
            }
        }

        //修改
        public static int UpdateCarType(CarProject carType)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"PRAGMA foreign_keys = true;Update ExcelCarProject set CarName=@CarName, ModifyDate=@ModifyDate where CarNO=@CarNO";
                return cnn.Execute(sql, carType);
            }
        }
        //检测存在
        public static bool CheckCarNOExist(string carNO)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select *  from  ExcelCarProject where CarNO='{carNO}'  ";
                return cnn.Query<CarProject>(sql).Count() > 0;
            }
        }
      
        //获取所有
        public static List<CarProject> GetAllCarTypes()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select * from ExcelCarProject";
                return cnn.Query<CarProject>(sql).ToList();
            }
        }

        #endregion

        #region WireProject
        //添加
        public static int AddWireType(WireType wire)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Insert into ExcelWireTyp(WireName,WireNO,ModifyDate,Author)Values(@WireName,@WireNO,@ModifyDate,@Author)";
                return cnn.Execute(sql, wire);
            }
        }

        //删除
        public static int DeleteWireType(WireType WireType)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"PRAGMA foreign_keys = true;Delete  from  ExcelWireTyp  Where WireNO=@WireNO";
                return cnn.Execute(sql, WireType);
            }
        }

        //修改
        public static int UpdateWireType(WireType WireType)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"PRAGMA foreign_keys = true;Update ExcelWireTyp set WireName=@WireName, ModifyDate=@ModifyDate where WireNO=@WireNO";
                return cnn.Execute(sql, WireType);
            }
        }
        //检测存在
        public static bool CheckWireNOExist(string WireNO)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select *  from  ExcelWireTyp where WireNO='{WireNO}'  ";
                 int rs= cnn.Query<WireType>(sql).Count() ;
                return rs>0;
            }
        }

        //获取所有
        public static List<WireType> GetAllWireTypes()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select * from ExcelWireTyp";
                return cnn.Query<WireType>(sql).ToList();
            }
        }
        #endregion

        #region ExcelProject
        //添加
        public static int AddExcelProject(ExcelProject ExcelProject)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Insert into ExcelProject(CarNO,WireNO,ProjectNO,ProjectName,ModifyDate,Author)Values(@CarNO,@WireNO,@ProjectNO,@ProjectName,@ModifyDate,@Author)";
                return cnn.Execute(sql, ExcelProject);
            }
        }

        //删除
        public static int DeleteExcelProject(ExcelProject ExcelProject)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"PRAGMA foreign_keys = true;Delete  from  ExcelProject  Where ProjectNO=@ProjectNO";
                return cnn.Execute(sql, ExcelProject);
            }
        }

        //修改
        public static int UpdateExcelProject(ExcelProject ExcelProject)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"PRAGMA foreign_keys = true;Update ExcelProject set CarNO=@CarNO,WireNO=@WireNO, ProjectName=@ProjectName, ModifyDate=@ModifyDate where CarNO=@CarNO";
                return cnn.Execute(sql, ExcelProject);
            }
        }
        //检测存在
        public static bool CheckProjectNOExist(string projectNO)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select *  from  ExcelProject where projectNO='{projectNO}'  ";
                return cnn.Query<CarProject>(sql).Count() > 0;
            }
        }

        /// <summary>
        /// 获取所有的工程信息
        /// </summary>
        /// <returns></returns>
        public static List<ExcelProject> GetAllExcelProject()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select * from ExcelProject";
                return cnn.Query<ExcelProject>(sql).ToList();
            }
        }

        /// <summary>
        /// 获取某一个线束下的所有工程
        /// </summary>
        /// <returns></returns>
        public static List<ExcelProject> GetAllWireExcelProject(string CarNO,string WireNO)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select * from ExcelProject where CarNO='{CarNO}' and WireNO='{WireNO}'";
                return cnn.Query<ExcelProject>(sql).ToList();
            }
        }
        #endregion

        #region ExcelPart
        public static bool CheckPartNameExist(string projectNO)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select *  from ExcelPart where ProjectNO='{projectNO}'  ";
                return cnn.Query<Part>(sql).Count() > 0;
            }
        }

        public static List<Part> GetAllParts(string projectNO)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select *  from ExcelPart where ProjectNO='{projectNO}'  ";
                return cnn.Query<Part>(sql).ToList();
            }
        }

        public static Part GetAllParts(string projectNO,string PartNO)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select *  from ExcelPart where ProjectNO='{projectNO}' and PartType='{PartNO}' ";
                return cnn.Query<Part>(sql).First();
            }
        }
        #endregion

        #region FixtureBaseInfo

        /// <summary>
        /// 添加治具基本信息
        /// </summary>
        /// <param name="fixture"></param>
        /// <returns></returns>
        public static int AddFixtureBaseInfo(FixtureBase fixture)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Insert into ExcelFixtureBaseInfo(FixtureType,ImagePath,LEDAddress,Coordinate)Values(@FixtureType,@ImagePath,@LEDAddress,@Coordinate) ";
                return cnn.Execute(sql, fixture);
            }
        }

        /// <summary>
        /// 更新治具基本信息
        /// </summary>
        /// <param name="fixture"></param>
        /// <returns></returns>
        public static int UpdateFixtureBaseInfo(FixtureBase fixture)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Update  ExcelFixtureBaseInfo Set ImagePath=@ImagePath,LEDAddress=@LEDAddress,Coordinate=@Coordinate Where FixtureType=@FixtureType ";
                return cnn.Execute(sql, fixture);
            }
        }

        /// <summary>
        /// 删除治具基本信息
        /// </summary>
        /// <param name="fixture"></param>
        /// <returns></returns>
        public static int DeleteFixtureBaseInfo(FixtureBase fixture)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"PRAGMA foreign_keys = true;PRAGMA foreign_keys = true;Delete from ExcelFixtureBaseInfo where FixtureType=@FixtureType";
                return cnn.Execute(sql, fixture);
            }
        }

        /// <summary>
        /// LED地址唯一
        /// </summary>
        /// <param name="fixture"></param>
        /// <returns></returns>
        public static bool CheckLEDAddress(string Address)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select * from ExcelFixtureBaseInfo where LEDAddress={Address} ";
                return cnn.Query<FixtureBase>(sql).Count()>0;
            }
        }

        /// <summary>
        /// 型号唯一性检查
        /// </summary>
        /// <param name="fixture"></param>
        /// <returns></returns>
        public static bool CheckFixturetype(string fixtureType)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select * from ExcelFixtureBaseInfo where FixtureType='{fixtureType}' ";
                return cnn.Query<FixtureBase>(sql).Count() > 0;
            }
        }

        /// <summary>
        /// 获取所有的治具基本信息
        /// </summary>
        /// <returns></returns>
        public static List<FixtureBase> GetAllFixtureBaseInfos()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select * from ExcelFixtureBaseInfo ";
                return cnn.Query<FixtureBase>(sql).ToList();

            }

        }

        /// <summary>
        /// 获取所有的治具基本信息
        /// </summary>
        /// <returns></returns>
        public static List<FixtureBase> GetAllFixtureBaseInfos(string ProjectNO)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"SELECT * FROM ExcelFixtureBaseInfo WHERE FixtureType IN "+
                        $"(SELECT FixtureType FROM  ExcelFixturePinInfo WHERE ExcelFixturePinInfo.ProjectNO = '{ProjectNO}'); ";
                return cnn.Query<FixtureBase>(sql).ToList();

            }

        }

        public static FixtureBase GetOneFixtureBaseInfo(string FixtureType)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"SELECT * FROM ExcelFixtureBaseInfo WHERE FixtureType='{FixtureType}'  ";
                return cnn.Query<FixtureBase>(sql).FirstOrDefault();

            }

        }

        /// <summary>
        /// 获取所有的治具基本信息
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAllFBaseInfos()
        {

            DataTable table = new DataTable();
            using (SQLiteConnection cnn=new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter("SELECT * FROM ExcelFixtureBaseInfo;", cnn);//读数据库
              
                dataAdapter.Fill(table);//填充DataSet控件
                return table;
            }
            
            
         
        }

    
        #endregion

        #region FixturePinInfos
        /// <summary>
        /// 获取所有的治具基本信息
        /// </summary>
        /// <returns></returns>
        public static List<FixtureInfo> GetAFixturePinInfos(string ProjectNO)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select * from ExcelFixturePinInfo where ProjectNO='{ProjectNO}' ";
                return cnn.Query<FixtureInfo>(sql).ToList();

            }

        }
        #endregion

        #region ExcelPins

        /// <summary>
        /// 更新一个引脚
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        public static int UpdatOneExPin(ExcelPin pin)
        {
            string query = "Update ExcelPin Set PinSX=@PinSX,PinSY=@PinSY,PhysicalChannel=@PhysicalChannel,DrawColor=@DrawColor,Radius=@Radius  WHERE PinNO = @PinNO and FixtureType=@FixtureType";
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                return cnn.Execute(query, pin);
            }
        }

        /// <summary>
        /// 删除一个引脚
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int DeleteOneExPin(string PinNO, string FixtureType)
        {
            string query = "Delete  from ExcelPin WHERE PinNO = @PinNO and FixtureType=@FixtureType";
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();

                int rs = cnn.Execute(query, new ExcelPin() { PinNO = PinNO, FixtureType = FixtureType });
                return rs;
            }
        }

        public static int DeleteAllExPin(string FixtureType)
        {
            string query = "Delete  from ExcelPin WHERE  FixtureType=@FixtureType";
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();

                int rs = cnn.Execute(query, new ExcelPin() {  FixtureType = FixtureType });
                return rs;
            }
        }

        //检测存在
        public static bool CheckPinNOExist(string fixtureType,string PinNO)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select *  from  ExcelPin where FixtureType='{fixtureType}' and PinNO='{PinNO}'  ";
                return cnn.Query<CarProject>(sql).Count() > 0;
            }
        }

        public static bool CheckPinAddressExist(string Address)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select *  from  ExcelPin where  PhysicalChannel={Address}  ";
                return cnn.Query<CarProject>(sql).Count() > 0;
            }
        }
        #endregion

        #region Pins
        /// <summary>
        /// 获取一个治具下的所有引脚
        /// </summary>
        /// <returns></returns>
        public static List<ExcelPin> GetOneFixtureExcelPins(string FixtureType)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select * from ExcelPin where FixtureType='{FixtureType}' ";
                return cnn.Query<ExcelPin>(sql).ToList();

            }

        }

        /// <summary>
        /// 增加一个pin
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        public static int AddExPin(ExcelPin pin)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = "Insert into ExcelPin(FixtureType,PinNO,physicalChannel,DrawColor,PinSX,PinSY,PinEX,PinEY,Radius)" +
                    "Values(@FixtureType,@PinNO,@physicalChannel,@DrawColor,@PinSX,@PinSY,@PinEX,@PinEY,@Radius)";
                return cnn.Execute(sql, pin);
            }
        }

        /// <summary>
        /// 获取一个引脚
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ExcelPin GetOneExPin(string FixtureType, string PinNO)
        {
            string query = $"SELECT * FROM ExcelPin  WHERE  PinNO  = '{PinNO}' AND  FixtureType= '{FixtureType}'";
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
                {
                    cnn.Open();
                    return cnn.Query<ExcelPin>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取一个引脚
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ExcelPin GetOneExPin( string PinNO)
        {
            string query = $"SELECT * FROM ExcelPin  WHERE  PinNO  = '{PinNO}'";
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
                {
                    cnn.Open();
                    return cnn.Query<ExcelPin>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<ExcelPin> GetAllExcelPins()
        {
            string query = $"SELECT * FROM ExcelPin  ";
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
                {
                    cnn.Open();
                    return cnn.Query<ExcelPin>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取一个引脚
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ExcelPin GetOneExPin(int Address)
        {
            string query = $"SELECT * FROM ExcelPin  WHERE  PhysicalChannel= '{Address}'";
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
                {
                    cnn.Open();
                    return cnn.Query<ExcelPin>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取所有的治具基本信息
        /// </summary>
        /// <returns></returns>
        public static DataTable GetAllPinsInfo()
        {

            DataTable table = new DataTable();
            using (SQLiteConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter("SELECT FixtureType,PinNO,physicalChannel FROM ExcelPin Order by FixtureType,cast(substr(PinNO,1,5)as int) ", cnn);//读数据库

                dataAdapter.Fill(table);//填充DataSet控件
                return table;
            }



        }
        #endregion

        #region Connection
        public static List<ConnectionInfo> GetExConnectionInfo(string projectNO)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"SELECT ExcelConnect.WireNum,ExcelConnect.ProjectNO, ExcelConnect.CirCuitName, ExcelConnect.FixtureNO, "+
	                           "ExcelConnect.FixtureType, ExcelConnect.PinIndex, ExcelConnect.PinNO, ExcelPin.PhysicalChannel,ExcelFixtureBaseInfo.Coordinate"+
                               " FROM ExcelConnect"+
                               " LEFT JOIN ExcelPin ON  ExcelConnect.FixtureType = ExcelPin.FixtureType and ExcelConnect.PinNO==ExcelPin.PinNO " +
                               " LEFT JOIN ExcelFixtureBaseInfo ON  ExcelConnect.FixtureType = ExcelFixtureBaseInfo.FixtureType"+
                               $" where ExcelConnect.ProjectNO = '{projectNO}'";
                return cnn.Query<ConnectionInfo>(sql).ToList();
            }
        }
        public static List<ConnectionInfo> GetExConnectionInfo(List<int> physicalAddresses)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"SELECT ExcelConnect.WireNum,ExcelConnect.ProjectNO, ExcelConnect.CirCuitName, ExcelConnect.FixtureNO, " +
                               "ExcelConnect.FixtureType, ExcelConnect.PinIndex, ExcelConnect.PinNO, ExcelPin.PhysicalChannel,ExcelFixtureBaseInfo.Coordinate" +
                               " FROM ExcelConnect" +
                               " LEFT JOIN ExcelPin ON ExcelConnect.FixtureType = ExcelPin.FixtureType" +
                               " LEFT JOIN ExcelFixtureBaseInfo ON  ExcelConnect.FixtureType = ExcelFixtureBaseInfo.FixtureType" +
                               $" where ExcelPin.PhysicalChannel in @Ids";
                return cnn.Query<ConnectionInfo>(sql,new { Ids = physicalAddresses.ToArray()} ).ToList();
            }
        }


        public static List<ConnectionFixtures> GetExcelConnFixtures(List<int> physicalAddresses)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"SELECT ExcelPin.FixtureType, ExcelPin.PinNO, ExcelPin.PhysicalChannel,"+ 
	                         "ExcelFixtureBaseInfo.Coordinate FROM ExcelPin INNER JOIN ExcelFixtureBaseInfo"+
                              " ON ExcelPin.FixtureType = ExcelFixtureBaseInfo.FixtureType where " +
                               $"ExcelPin.PhysicalChannel in @Ids";
                return cnn.Query<ConnectionFixtures>(sql, new { Ids = physicalAddresses.ToArray() }).ToList();
            }
        }
        #endregion

        #endregion

        #region GetSampleData

        public static bool IsExitConnectInfos(string ProjectNO)
        {
            string query = "Select * from  PassiveSampleData Where ProjectNO=@ProjectNO";
            
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
                {
                    return cnn.Query<JKSampleData>(query, new JKSampleData { ProjectNO = ProjectNO }).Count() > 0;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public static  List<JKSampleData> GetStep3JkSampleDatas(string projectCode, int detectionStepCode, string deviceName)
        {

            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
                {
                    cnn.Open();
                    string sql = "select * from  JKLineSampleData_Step1_2 where ProjectNO=@ProjectNO and DetectionStepCode=@DetectionStepCode and DeviceName=@deviceName";
                    return cnn.Query<JKSampleData>(sql, new JKSampleData { ProjectNO = projectCode, DetectionStepCode = detectionStepCode, DeviceName = deviceName }).ToList();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public static List<ConnectionData> GetConnectionDatas(string projectNO)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
            {
                cnn.Open();
                string sql = $"Select * from PassiveSampleData where ProjectNO='{projectNO}'";
                return cnn.Query<BaseSampleData>(sql).GroupBy(r => r.ChannelNum).Select(group => new ConnectionData
                {
                    ChannelNum = group.Key,
                    PinCode=group.First().PinCode,
                    FixtureCode= group.First().FixtureCode,
                    ProjectNO = group.First().FixtureCode,
                    Connects = group.Select(r =>
                          r.ConnectTo
                          ).ToList(),
                }).ToList();
            }
        }
        public static int  DeletePassiveConnectInfos(string ProjectNO)
        {
            string query = "delete  from  PassiveConnect Where ProjectNO=@ProjectNO";

            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
                {
                    return cnn.Execute(query, new JKSampleData { ProjectNO = ProjectNO });

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        /// <summary>
        /// 获关联器件
        /// </summary>
        /// <returns></returns>
        public static List<JKSampleData> GetAllKFPin(string ProjectNO, int PhysicalChannel)
        {

            string query = "SELECT * FROM JKLineSampleData_Step1_2 where ProjectNO = @ProjectNO and PhysicalChannel = @PhysicalChannel AND(DeviceName like 'K%' OR DeviceName like 'F%')";
            try
            {
                using (IDbConnection  cnn = new SQLiteConnection(LoadConnectString()))
                {
                    cnn.Open();
                    return cnn.Query<JKSampleData>(query, new JKSampleData { ProjectNO = ProjectNO, PhysicalChannel = PhysicalChannel }).ToList();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public static List<JKSampleData> GetAllJkSampleDatas(string projectCode, int detectionStepCode)
        {
            try
            {
                using (IDbConnection cnn = new SQLiteConnection(LoadConnectString()))
                {
                    string sql = "select * from  JKLineSampleData_Step1_2 where ProjectNO=@ProjectNO and DetectionStepCode=@DetectionStepCode";
                    return cnn.Query<JKSampleData>(sql, new JKSampleData { ProjectNO = projectCode, DetectionStepCode = detectionStepCode }).ToList();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

      
    }
}