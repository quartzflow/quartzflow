namespace QuartzFlowHost.HttpApi
{
    public class HttpApiConstants
    {
        public static class FormFieldNames
        {
            public const string ActionToTake = "actionToTake";
        }

        public static class JobAction
        {
            public const string Pause = "pause";
            public const string Resume = "resume";
            public const string Start = "start";
            public const string Kill = "kill";
        }
    }
}
