using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IloopActiviyTransferVS_Net
{
    public class IloopActivity
    {
        public static int UserIdFromConfiguration
        {
            get {
                int userId;
                string configurationUserId = ConfigurationManager.AppSettings["IloopUserId"];

                if (int.TryParse(configurationUserId, out userId) == false)
                    throw new Exception(String.Format("Undefined UserId in Config. UserId:{0}", configurationUserId));

                return userId;
            }
        }
        public int ActivityTypeId { get; set; }
        public int ProjectId { get; set; }
        public string Comment { get; set; }
        public int UserId { get; set; }
        public DateTime ActivityDate { get; set; }
        public int Duration { get; set; }
        public int Status { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreateDate { get; set; }
        public int ActivityPDSTask { get; private set; }

        public IloopActivity()
        {
            /*
             intActivityStatus	strDescription
                0	PM Pending
                1	Accepted
                2	Manager Pending
                3	LD Pending
                4	Rejected 
             */
            Status = 1;
            CustomerId = 0;
            ActivityPDSTask = -1;
        }

        public IloopActivity(int day, int month, int year,int duration,string description, string project, string subProject, bool isAboutReport, bool isActivity)
        {
            if (isActivity == false)
                throw new Exception("There is an activity where isActivity equals false!");

            ActivityDate = new DateTime(year,month,day);
            Comment = description;
            ProjectId = GetProjectId(project);
            CustomerId = 0;
            ActivityPDSTask = -1;

            if (ProjectId > 10)
            {
                ActivityTypeId = 19;
                Status = 1;
            }
            else if (ProjectId == 2) //Sales
            {
                ActivityTypeId = 2;
                ProjectId = 0;
                Status = 2;
                CustomerId = 98;
            }
            else if (ProjectId == 1) //Administrative
            {
                ActivityTypeId = 1;
                ProjectId = 0;
                Status = 0;
            }
            else if (ProjectId == 3) //izin
            {
                ActivityTypeId = 18;
                ProjectId = 0;
                Status = 1;
            }
            else
            {
                ActivityTypeId = 11;
                ProjectId = 0;
                Status = 0;
            }

            Duration = duration;
            

            if (String.IsNullOrEmpty(subProject) == false)
            {
                ProjectId = GetProjectId(subProject);
            }

            UserId = UserIdFromConfiguration;
            CreateDate = DateTime.Now;                
        }

        private int GetProjectId(string project)
        {
            int projectId=-1;

            if (string.IsNullOrEmpty(project))
                throw new Exception("Project is empty!");
            else if (project.IndexOf("-") == -1)
                throw new Exception("Project do not have -");

            var projectIdStr=project.Split(new string[] { "-" },StringSplitOptions.None)[1];

            if (int.TryParse(projectIdStr, out projectId) == false)
                throw new Exception(String.Format("ProjectId can not be parsed! Id String: {0}",projectIdStr));
            
            return projectId;
        }
    }
}
