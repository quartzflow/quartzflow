using System;
using System.Collections.Generic;
using Quartz;

namespace QuartzFlow.QuartzExtensions
{
    public static class JobExtensions
    {
        public static bool RequiresTrigger(this IJobDetail job)
        {
            string runAt = (string)job.JobDataMap[Constants.FieldNames.RunAt];
            return !string.IsNullOrEmpty(runAt);
        }

        public static string GetNextRunAtMessages(this IJobDetail job, HashSet<ITrigger> triggers)
        {
            string targetTimeZone = (string)job.JobDataMap[Constants.FieldNames.Timezone];
            TimeZoneInfo timezone = TimeZoneInfo.Local;
            if (!string.IsNullOrEmpty(targetTimeZone))
            {
                timezone = TimeZoneInfo.FindSystemTimeZoneById(targetTimeZone);
            }

            string runPlan = string.Empty;

            foreach (ITrigger t in triggers)
            {
                if (t.GetNextFireTimeUtc().HasValue)
                {
                    var nextFireTimeUtc = t.GetNextFireTimeUtc().Value;
                    var nextFireTimeInTargetTimeZone =
                        TimeZoneInfo.ConvertTimeFromUtc(nextFireTimeUtc.DateTime, timezone);
                    var nextFireTimeInLocalTimeZone =
                        TimeZoneInfo.ConvertTimeFromUtc(nextFireTimeUtc.DateTime, TimeZoneInfo.Local);

                    string timeZoneName = timezone.IsDaylightSavingTime(nextFireTimeUtc)
                        ? timezone.DaylightName
                        : timezone.StandardName;
                    string localTimeZoneName =
                        TimeZone.CurrentTimeZone.IsDaylightSavingTime(nextFireTimeInTargetTimeZone)
                            ? TimeZone.CurrentTimeZone.DaylightName
                            : TimeZone.CurrentTimeZone.StandardName;

                    if (timeZoneName != localTimeZoneName)
                    {
                        runPlan += $"{nextFireTimeInTargetTimeZone:F} {timeZoneName} ({nextFireTimeInLocalTimeZone:F} {localTimeZoneName}){Environment.NewLine}";
                    }
                    else
                    {
                        runPlan += $"{nextFireTimeInTargetTimeZone:F} {timeZoneName}{Environment.NewLine}";
                    }                  
                }
                else
                {
                    runPlan = $"Job does not have a next run time set";
                }
            }

            if (string.IsNullOrEmpty(runPlan))
                runPlan = $"Job does not have a specific trigger";

            return runPlan;
        }

        public static HashSet<ITrigger> CreateTriggers(this IJobDetail job)
        {
            string runAt = (string)job.JobDataMap[Constants.FieldNames.RunAt];
            string runOnDays = (string)job.JobDataMap[Constants.FieldNames.RunDays];
            string runCalendar = (string)job.JobDataMap[Constants.FieldNames.RunCalendar];
            string excludeCalendar = (string)job.JobDataMap[Constants.FieldNames.ExclusionCalendar];
            string targetTimeZone = (string)job.JobDataMap[Constants.FieldNames.Timezone];

            var triggers = new HashSet<ITrigger>();
            string[] runAtTimes = runAt.Split(',');
            int runAtIndex = 0;
            foreach (var runAtTime in runAtTimes)
            {
                string triggerName = $"Trigger_{runAtIndex++}_for_{job.Key}";
                var trigger = CreateTriggerForSpecificRunTime(triggerName, runAtTime, runOnDays, excludeCalendar, targetTimeZone);
                triggers.Add(trigger);
            }

            return triggers;
        }

        private static ITrigger CreateTriggerForSpecificRunTime(string triggerName, string runAtTime, string runOnDays, string excludeCalendar, string targetTimeZone)
        {
            ITrigger trigger = null;

            TimeZoneInfo timezone = TimeZoneInfo.Local;
            if (!string.IsNullOrEmpty(targetTimeZone))
            {
                timezone = TimeZoneInfo.FindSystemTimeZoneById(targetTimeZone);
            }

            if (!string.IsNullOrEmpty(runOnDays))
            {
                var daysOfWeek = ScheduleStringHelper.GetDaysOfWeekToRunOn(runOnDays);
                var runAtTuple = ScheduleStringHelper.GetHoursAndMinutes(runAtTime);

                trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerName)
                    .WithDescription($"Run at: {runAtTime}, run days: {runOnDays}, exclusion calendar: {excludeCalendar}")
                    .WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(runAtTuple.Item1, runAtTuple.Item2, daysOfWeek)
                        .InTimeZone(timezone))
                    .ModifiedByCalendar(excludeCalendar)
                    .StartNow()
                    .Build();
            }
            //else if (!string.IsNullOrEmpty(runCalendar))
            //{
            //    trigger = TriggerBuilder.Create()
            //                            .WithIdentity(string.Format("Trigger_for_{0}", job.Key))
            //                            .WithDescription(string.Format("Run at: {0}, run calendar: {1}, exclusion calendar: {2}", runAt, runCalendar, excludeCalendar))
            //                            .WithSchedule(CronScheduleBuilder.)
            //                            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(hour, minute))
            //                            .ModifiedByCalendar(runCalendar)
            //                            .StartNow()
            //                            .Build();
            //}

            return trigger;
        }

      
    }
}
