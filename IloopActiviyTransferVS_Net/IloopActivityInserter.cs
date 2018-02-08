using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IloopActiviyTransferVS_Net
{
    class IloopActivityInserter
    {
        private List<IloopActivity> iloopActivityList;
        

        public IloopActivityInserter(List<IloopActivity> iloopActivityList)
        {
            this.iloopActivityList = iloopActivityList;
        }


        public bool Insert()
        {
            int month;
            int year;
            int endDayOfMonth;

            if (iloopActivityList.Count() > 0)
            {
                month = iloopActivityList[0].ActivityDate.Month;
                year = iloopActivityList[0].ActivityDate.Year;
                endDayOfMonth = new DateTime(year, month, 1).AddMonths(1).AddDays(-1).Day;
            }
            else
                return false;

            for (int i = 1; i <= endDayOfMonth; i++)
            {
                var activityDate = new DateTime(year,month,i);
                var holiday = CheckIsHoliday(activityDate);

                if (holiday.IsHoliday && holiday.IsHalfDay==false)
                    continue;
                else
                {
                    if (HasAlreadyIloopInserted(activityDate) || activityDate.DayOfWeek== DayOfWeek.Saturday || activityDate.DayOfWeek== DayOfWeek.Sunday)
                        continue;

                    var dailyActivityList = iloopActivityList.Where(q => q.ActivityDate == activityDate);

                    //CheckActivityTotal
                    var activityTotalHour = dailyActivityList.Sum(q => q.Duration);
                    if (holiday.IsHoliday && holiday.IsHalfDay == true && activityTotalHour < (4*60))
                    {
                        throw new Exception(string.Format("{0} tarihli aktiviteler 240 dakikadan az. {1} saat.",
                            activityDate.ToShortDateString(),activityTotalHour));
                    }
                    else if (activityTotalHour < (8 * 60))
                    {
                        throw new Exception(string.Format("{0} tarihli aktiviteler 480 dakikadan az. {1} saat.",
                                activityDate.ToShortDateString(), activityTotalHour));
                    }

                    using (var entities = new ILOOPEntities())
                    {
                        foreach (var activity in dailyActivityList)
                        {
                            tblActivity newActivity = new tblActivity();
                            newActivity.dtActivityDate = activityDate;
                            newActivity.dtCreateDate = DateTime.Now;
                            newActivity.intActivityDuration = activity.Duration;
                            newActivity.intActivityPDSTask = activity.ActivityPDSTask;
                            newActivity.intActivityStatus = (byte) activity.Status;
                            newActivity.intActivityTypeID = activity.ActivityTypeId;
                            newActivity.intCustomerID = activity.CustomerId;
                            newActivity.intProjectID = activity.ProjectId;
                            newActivity.intUserID = activity.UserId;
                            newActivity.strUserActivityComment = activity.Comment;

                            entities.tblActivity.Add(newActivity);
                        }

                        entities.SaveChanges();
                    }

                }
            }

            return true;
        }

        protected Holiday CheckIsHoliday(DateTime checkDate)
        {
            Holiday retval = new Holiday();

            using (var entities = new ILOOPEntities())
            {
                var holidayDate=entities.tblOfficialHolidays.AsNoTracking().Where(q => q.dtHolidayDate == checkDate).FirstOrDefault();
                if (holidayDate == null)
                    retval.IsHoliday = false;
                else
                {
                    retval.IsHoliday = true;
                    retval.IsHalfDay = holidayDate.Duration == 4;
                }
            }

            return retval;
        }

        protected Holiday CheckIsUserInHoliday(DateTime checkDate)
        {
            Holiday retval = new Holiday();

            using (var entities = new ILOOPEntities())
            {
                var checkStartDate = checkDate.AddHours(18);
                var checkEndDate = checkDate.AddHours(9);

                var holidayRequest=entities.tblHolidayRequest.AsNoTracking().Where(q=>q.intUserID==IloopActivity.UserIdFromConfiguration && 
                    q.dtHolidayBeginDate<= checkStartDate && q.dtHolidayEndDate>= checkEndDate).FirstOrDefault();

                if (holidayRequest == null)
                    retval.IsHoliday = false;
                else
                    retval.IsHoliday = true;
            }

            return retval;
        }

        protected bool HasAlreadyIloopInserted(DateTime checkDate)
        {
            bool retval = false;

            using (var entities = new ILOOPEntities())
            {
                var activitiesCount = entities.tblActivity.AsNoTracking().Where(q => q.intUserID == IloopActivity.UserIdFromConfiguration &&
                      q.dtActivityDate==checkDate).Count();

                retval = activitiesCount > 0;
            }

            return retval;
        }

        protected class Holiday
        {
            public bool IsHoliday { get; set; }
            public bool IsHalfDay { get; set; }
            public DateTime Date { get; set; }
        }
    }
}
