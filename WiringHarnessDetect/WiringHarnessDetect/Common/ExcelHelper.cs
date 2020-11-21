using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiringHarnessDetect.Model;

namespace WiringHarnessDetect.Common
{
    public class ExcelHelper
    {

        /// <summary>
        /// 导出无线源数据
        /// </summary>
        /// <param name="result"></param>
        public static void WritePassive(Dictionary<string,List<string>> result)
        {
            var workBook = new HSSFWorkbook();
            #region 设置样式
            IFont font = workBook.CreateFont();
            font.FontHeightInPoints = 11;
            font.FontName = "Arial";
            font.IsBold = true;

            var cellMidStyle = workBook.CreateCellStyle();
            cellMidStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;//设置水平居中
            cellMidStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;//设置垂直居中
            cellMidStyle.SetFont(font);
            #endregion


            List<string> headers = new List<string>() { "From", "Connection", "To" };
            foreach (var item in result)
            {
                var sheet = workBook.CreateSheet(item.Key);//创建表格
                var row = sheet.CreateRow(0);
                //创建列头部
                for (int i = 0; i < headers.Count; i++)//创建表头
                {
                    var cell = row.CreateCell(i);
                    cell.SetCellValue(headers[i]);
                    cell.CellStyle = cellMidStyle;//设置单元格样式
                }
               
                //填写数据
                foreach (var record in item.Value)
                {
                    int rowIndex = 1;
                    row = sheet.CreateRow(rowIndex);
                    List<string> values = new List<string>();
                    //分解数据
                    if(record.Contains('+'))
                    {
                        values.AddRange(record.Split('+'));
                        values.Insert(1, "+");
                    }
                    else if(record.Contains('<'))
                    {
                        values.AddRange(record.Split('<'));
                        values.Insert(1, "<");
                    }

                    //创建每一行数据
                    for (int i = 0; i < values.Count; i++)
                    {
                        row.CreateCell(i).SetCellValue(values[i]);//给rowIndex行的第1列的单元格赋值
                       
                    }
                    rowIndex++;
                }
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "xls files(*.xls)|*.xls";
            DialogResult sdg = saveFileDialog.ShowDialog();
            if (sdg == DialogResult.OK)
            {
                //获得文件路径
                var localFilePath = saveFileDialog.FileName;
                using (FileStream fs = new FileStream(localFilePath, FileMode.Create))
                {
                    workBook.Write(fs);
                }
                MessageBox.Show("保存成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        /// <summary>
        /// 导出无线源数据
        /// </summary>
        /// <param name="result"></param>
        public static void WriteActive(Dictionary<string, List<JKSampleData>> activeresult,Dictionary<string, List<string>> passiveresult)
        {
            var workBook = new HSSFWorkbook();
            #region 设置样式
            IFont font = workBook.CreateFont();
            font.FontHeightInPoints = 11;
            font.FontName = "Arial";
            font.IsBold = true;

            var cellMidStyle = workBook.CreateCellStyle();
            cellMidStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;//设置水平居中
            cellMidStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;//设置垂直居中
            cellMidStyle.SetFont(font);
            #endregion


            List<string> headers = new List<string>() { "From", "Connection", "To" };
            foreach (var item in passiveresult)
            {
                var sheet = workBook.CreateSheet(item.Key);//创建表格
                var row = sheet.CreateRow(0);
                //创建列头部
                for (int i = 0; i < headers.Count; i++)//创建表头
                {
                    var cell = row.CreateCell(i);
                    cell.SetCellValue(headers[i]);
                    cell.CellStyle = cellMidStyle;//设置单元格样式
                }

                //填写数据
                foreach (var record in item.Value)
                {
                    int rowIndex = 1;
                    row = sheet.CreateRow(rowIndex);
                    List<string> values = new List<string>();
                    //分解数据
                    if (record.Contains('+'))
                    {
                        values.AddRange(record.Split('+'));
                        values.Insert(1, "+");
                    }
                    else if (record.Contains('<'))
                    {
                        values.AddRange(record.Split('<'));
                        values.Insert(1, "<");
                    }

                    //创建每一行数据
                    for (int i = 0; i < values.Count; i++)
                    {
                        row.CreateCell(i).SetCellValue(values[i]);//给rowIndex行的第1列的单元格赋值

                    }
                    rowIndex++;
                }
            }


            List<string> headersa = new List<string>() { "检测步骤", "引脚电压", "治具名称","治具编号","引脚号","物理通道","器件名称" };
           
            foreach (var item in activeresult)
            {
                var sheet = workBook.CreateSheet(item.Key);//创建表格
                var row = sheet.CreateRow(0);
                //创建列头部
                for (int i = 0; i < headersa.Count; i++)//创建表头
                {
                    var cell = row.CreateCell(i);
                    cell.SetCellValue(headersa[i]);
                    cell.CellStyle = cellMidStyle;//设置单元格样式
                }
              
                //填写数据
                foreach (var record in item.Value)
                {
                    int rowIndex = 1;
                    row = sheet.CreateRow(rowIndex);
                    row.CreateCell(0).SetCellValue(record.DetectionStepCode);//检测步骤
                    row.CreateCell(1).SetCellValue(record.PinVoltageCode);//电压
                    row.CreateCell(2).SetCellValue(record.FixtureName);//治具名称
                    row.CreateCell(3).SetCellValue(record.FixtureCode);//治具编号 
                    row.CreateCell(4).SetCellValue(record.PinNO);//引脚编号
                    row.CreateCell(5).SetCellValue(record.PhysicalChannel);//物理通道 
                    row.CreateCell(5).SetCellValue(record.DeviceName);//器件名称
                    rowIndex++;
                }
               
            }



            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "xls files(*.xls)|*.xls";


            DialogResult sdg = saveFileDialog.ShowDialog();
            if (sdg == DialogResult.OK)
            {
                //获得文件路径
                var localFilePath = saveFileDialog.FileName;
                using (FileStream fs = new FileStream(localFilePath, FileMode.Create))
                {
                    workBook.Write(fs);
                }
                MessageBox.Show("保存成功!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
