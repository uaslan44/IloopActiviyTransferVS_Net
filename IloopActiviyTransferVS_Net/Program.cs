using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IloopActiviyTransferVS_Net
{
    class Program
    {
        protected static string ActivityExcellFilePath
        {
            get
            {
                return ConfigurationManager.AppSettings["ActivityExcelFilePath"].ToString();
            }
        }

        static void Main(string[] args)
        {
            try
            {
                ExcelReader excellReader = new ExcelReader();
                var iloopActivityList = excellReader.ReadActivityData(ActivityExcellFilePath);

                IloopActivityInserter activityInserter = new IloopActivityInserter(iloopActivityList);
                activityInserter.Insert();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Uygulama sonlandı!");
            Console.ReadLine();
        }
    }
}
