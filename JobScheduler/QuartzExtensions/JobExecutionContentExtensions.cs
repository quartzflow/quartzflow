using System;
using Quartz;

namespace JobScheduler.QuartzExtensions
{
    public static class JobExecutionContextExtensions
    {
        public static void AppendToOutputBuffer(this IJobExecutionContext context, string message)
        {
            var contents = context.Get(Constants.FieldNames.StandardOutput);
            contents += message;
            contents += Environment.NewLine;
            context.Put(Constants.FieldNames.StandardOutput, contents);
        }

        public static void ClearOutputBuffer(this IJobExecutionContext context)
        {
            context.Put(Constants.FieldNames.StandardOutput, string.Empty);
        }

        public static string ReadOutputBufferContents(this IJobExecutionContext context)
        {
            return context.Get(Constants.FieldNames.StandardOutput).ToString();
        }
    }
}