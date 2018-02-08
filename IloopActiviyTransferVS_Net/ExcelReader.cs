using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;


namespace IloopActiviyTransferVS_Net
{
    class ExcelReader
    {
        public class ActivityExcelRow
        {
            public static readonly string ActivitySheetName;
            public static readonly string DefinitionsSheetName;

            #region Properties

            public bool IsEmpty { get; private set; }

            protected int day;
            public int Day
            {
                get
                {
                    return day;
                }
            }

            protected int duration;
            public int Duration
            {
                get {
                    return duration;
                }
            }

            protected string description;
            public string Description
            {
                get {
                    return description;
                }
            }

            private string project;
            public string Project
            {
                get {
                    return project;
                }
            }


            private string subProject;
            public string SubProject
            {
                get
                {
                    return subProject;
                }
            }


            private bool isAboutReport;
            public bool IsAboutReport
            {
                get
                {
                    return isAboutReport;
                }
            }


            private bool isActivity;
            public bool IsActivity
            {
                get
                {
                    return isActivity;
                }
            }

            #endregion

            static ActivityExcelRow()
            {
                ActivitySheetName = "Activities";
                DefinitionsSheetName = "Definitions";
            }

            public ActivityExcelRow(ExcelWorksheet excelWorkSheet,int rowIndex)
            {
                object dayValue = excelWorkSheet.Cells[rowIndex, ActivitiesCellIndex.Day].Value;
                IsEmpty = (excelWorkSheet.Cells[rowIndex, ActivitiesCellIndex.Day].Value == null) ||
                    (dayValue.GetType() ==typeof(string) && String.IsNullOrEmpty(excelWorkSheet.Cells[rowIndex, ActivitiesCellIndex.Day].Value as string));

                if (IsEmpty == false)
                {
                    day = ExcelReader.CInt(dayValue,"Day");
                    duration = ExcelReader.CInt(excelWorkSheet.Cells[rowIndex, ActivitiesCellIndex.Duration].Value,"Duration");
                    description = excelWorkSheet.Cells[rowIndex, ActivitiesCellIndex.Description].Value as string;
                    project = excelWorkSheet.Cells[rowIndex, ActivitiesCellIndex.Project].Value as string;
                    subProject= excelWorkSheet.Cells[rowIndex, ActivitiesCellIndex.SubProject].Value as string;
                    isAboutReport = ExcelReader.CBool(excelWorkSheet.Cells[rowIndex, ActivitiesCellIndex.IsAboutReport].Value, "IsAboutReport");
                    isActivity = ExcelReader.CBool(excelWorkSheet.Cells[rowIndex, ActivitiesCellIndex.IsActivity].Value, "IsAboutReport");
                }
            }

            protected struct ActivitiesCellIndex
            {
                public static readonly int Day = 1;
                public static readonly int Duration = 2;
                public static readonly int Description = 3;
                public static readonly int Project = 4;
                public static readonly int SubProject = 5;
                public static readonly int IsAboutReport = 6;
                public static readonly int IsActivity = 7;
            }

            public struct DefinitionsCellIndex
            {
                public static readonly int MonthRow = 2;
                public static readonly int MonthColumn = 2;
                public static readonly int YearRow = 2;
                public static readonly int YearColumn = 3;
            }
        }

        public List<IloopActivity> ReadActivityData(string fileUrl)
        {
            try
            {
                List<IloopActivity> iloopActivityList = new List<IloopActivity>();

                var fileInfo = new FileInfo(fileUrl);

                using (var p = new ExcelPackage(fileInfo))
                {
                    var definitionsSheet = p.Workbook.Worksheets[ActivityExcelRow.DefinitionsSheetName];
                    int year = (int)(double) definitionsSheet.Cells[ActivityExcelRow.DefinitionsCellIndex.YearRow, ActivityExcelRow.DefinitionsCellIndex.YearColumn].Value;
                    int month = (int)(double)definitionsSheet.Cells[ActivityExcelRow.DefinitionsCellIndex.MonthRow, ActivityExcelRow.DefinitionsCellIndex.MonthColumn].Value;

                    int readStartRowIndex = 2;
                    var activitySheet = p.Workbook.Worksheets[ActivityExcelRow.ActivitySheetName];
                    ActivityExcelRow activityExcelRow = new ActivityExcelRow(activitySheet, readStartRowIndex);
                   
                    while (activityExcelRow.IsEmpty==false)
                    {
                        if (activityExcelRow.IsActivity)
                        {
                            iloopActivityList.Add(new IloopActivity(activityExcelRow.Day, month, year, activityExcelRow.Duration, activityExcelRow.Description,
                                activityExcelRow.Project, activityExcelRow.SubProject, activityExcelRow.IsAboutReport, activityExcelRow.IsActivity));
                        }

                        readStartRowIndex++;
                        activityExcelRow = new ActivityExcelRow(activitySheet, readStartRowIndex);
                    }
                 
                }

                return iloopActivityList;
            }
            catch (Exception ex)
            {
                throw new Exception("Excel Read Error",ex);
            }
        }

        public static int CInt(object value, string columnName)
        {
            int retval = 0;

            try
            { 
                retval = (int)(double)value;
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Integer cast error, Colunm Name:{0}", columnName));
            }
            
            return retval;
        }

        public static bool CBool(object value, string columnName)
        {
            bool retval = false;

            if (value==null)
            {
                retval = false;
            }
            else
            {
                try
                {
                    var intValue = (int)(double)value;

                    if (intValue == 1) retval = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("Bool cast error, Colunm Name:{0}", columnName));
                }
            }

            return retval;
        }


    }
}
