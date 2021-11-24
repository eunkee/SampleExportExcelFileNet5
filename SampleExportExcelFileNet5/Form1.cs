using ICG.NetCore.Utilities.Spreadsheet;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SampleExportExcelFileNet5
{
    //ICG netcore.utilities.spreadsheet
    //https://github.com/IowaComputerGurus/netcore.utilities.spreadsheet
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private bool IsRunExport = false;

        public Form1()
        {
            InitializeComponent();
        }

        //그리드 선택 안되게
        private void GridViewMaterial_Click(object sender, EventArgs e)
        {
            GridViewMaterial.ClearSelection();
        }

        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            GridViewMaterial.Rows.Clear();
            for(int i = 0; i < 7; i++)
            {
                var rnd = new Random(Guid.NewGuid().GetHashCode());
                var year = rnd.Next(DateTime.Now.Year, DateTime.Now.Year + 1);
                var month = rnd.Next(1, 13);
                var days = rnd.Next(1, DateTime.DaysInMonth(year, month) + 1);

                DateTime dateTime = new(year, month, days,
                    rnd.Next(0, 24), rnd.Next(0, 60), rnd.Next(0, 60), rnd.Next(0, 1000));

                string chars = "ABCDEFG";
                int sellerNumber = rnd.Next(1, chars.Length);
                string seller = $"Company{chars[sellerNumber]}";
                int count = rnd.Next(1, 100);

                DataGridViewRow AddRow = new();
                AddRow.CreateCells(GridViewMaterial);
                AddRow.Cells[0].Value = dateTime.ToString();
                AddRow.Cells[1].Value = seller;
                AddRow.Cells[2].Value = count.ToString();
                AddRow.Height = 25;
                GridViewMaterial.Rows.Add(AddRow);
            }

            GridViewMaterial.ClearSelection();
        }

        private void ButtonExport_Click(object sender, EventArgs e)
        {
            //실행 중일 때 간략 예외처리
            if (IsRunExport)
            {
                MessageBox.Show("Alerady Export", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //데이터 없을 때 예외처리
            if (GridViewMaterial.Rows.Count <= 0)
            {
                MessageBox.Show("No Load Data", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nowDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string savePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string pathFilename = $"{savePath}\\excelexport_{nowDateTime}.xlsx";

            //사용자가 FileDialog를 사용하게 하려면
            {
                SaveFileDialog saveFile = new()
                {
                    Title = "Save Excel File",
                    FileName = $"excelexport_{nowDateTime}.xlsx",
                    DefaultExt = "xlsx",
                    Filter = "Xlsx files(*.xlsx)|*.xlsx"
                };
                // OK버튼을 눌렀을때의 동작
                if (saveFile.ShowDialog() == DialogResult.OK)
                {
                    // 경로와 파일명을 fileName에 저장
                    pathFilename = saveFile.FileName.ToString();
                }
                else
                {
                    return;
                }
            }

            List<SampleData> sampleDatas = new();
            foreach (DataGridViewRow row in GridViewMaterial.Rows)
            {
                if (!decimal.TryParse(row.Cells[2].Value.ToString(), out decimal count))
                {
                    count = 0;
                }
                SampleData sample = new()
                {
                    날짜 = row.Cells[0].Value.ToString(),
                    거래처 = row.Cells[1].Value.ToString(),
                    입고수량 = count
                };
                sampleDatas.Add(sample);
            }

            var services = new ServiceCollection();
            services.UseIcgNetCoreUtilitiesSpreadsheet();
            var provider = services.BuildServiceProvider();

            //Get our generator and export
            var exportGenerator = provider.GetService<ISpreadsheetGenerator>();
            var exportDefinition = new SpreadsheetConfiguration<SampleData>
            {
                RenderTitle = true,
                DocumentTitle = "Export Excel File Title",
                RenderSubTitle = true,
                DocumentSubTitle = "Showing options",
                ExportData = sampleDatas,
                WorksheetName = "Sample Sheet Name"
            };
            var fileContent = exportGenerator.CreateSingleSheetSpreadsheet(exportDefinition);
            System.IO.File.WriteAllBytes(pathFilename, fileContent);

            //var multiSheet = new List<ISpreadsheetConfiguration<object>>();
            //multiSheet.Add(exportDefinition);
            //multiSheet.Add(new SpreadsheetConfiguration<SampleData>
            //{
            //    RenderTitle = true,
            //    DocumentTitle = "Sample Second of 50 Records",
            //    RenderSubTitle = true,
            //    DocumentSubTitle = "Showing the full options",
            //    ExportData = sampleDatas,
            //    WorksheetName = "Additional"
            //});
            //var multiFileContent = exportGenerator.CreateMultiSheetSpreadsheet(multiSheet);
            //System.IO.File.WriteAllBytes("Sample-Multi.xlsx", multiFileContent);
            MessageBox.Show("Complete Export.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            IsRunExport = false;
        }
    }
}
