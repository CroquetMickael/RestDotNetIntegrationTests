using TechTalk.SpecFlow.Assist;
using TechTalk.SpecFlow;

[Binding]
public static class CustomValueRetrievers
{
    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        Service.Instance.ValueRetrievers.Register(new DateOnlyValueRetriever());
    }
}