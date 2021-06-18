using System;
using Hangfire;
using ntbs_service.DataAccess;
using Serilog;

namespace ntbs_service.Jobs
{
    public class TestJob : HangfireJobContext
    {
        public TestJob(NtbsContext ntbsContext) : base(ntbsContext)
        {
        }

        public void Run(IJobCancellationToken token)
        {
            Log.Information($"Starting test job");
            Fail();
            Log.Information($"Finishing test job");
        }

        private static void Fail()
        {
            throw new Exception("This is a test. We want to see if the exception reaches Sentry");
        }
    }
}
