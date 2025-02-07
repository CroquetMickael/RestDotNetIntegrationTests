using TechTalk.SpecFlow.Assist;
using TechTalk.SpecFlow;
using MyApi.WebApi.Tests.CustomValueRetrievers;

namespace MyApi.WebApi.Tests.Hooks;

[Binding]
public static class CustomValueRetrievers
{
    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        Service.Instance.ValueRetrievers.Register(new DateOnlyValueRetriever());
    }
}
