using System.Dynamic;
using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class FocasPerf : Veneer
{
    public FocasPerf(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(
        veneers, name, isCompound, isInternal)
    {
    }

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        // TODO : review extension usage
        //  https://stackoverflow.com/questions/69917366/net6-morelinq-call-is-ambiguous-between-system-linq-enumerable-distinctby-a

        var max = MoreEnumerable.MaxBy((List<dynamic>) additionalInputs[0].focas_invocations, o => o.invocationMs)
            .First();

        var min = MoreEnumerable.MinBy((List<dynamic>) additionalInputs[0].focas_invocations, o => o.invocationMs)
            .First();

        var avg = (int) ((List<dynamic>) additionalInputs[0].focas_invocations).Average(o => (int) o.invocationMs);

        var sum = ((List<dynamic>) additionalInputs[0].focas_invocations).Sum(o => (int) o.invocationMs);

        var failedMethods = ((List<dynamic>) additionalInputs[0].focas_invocations)
            .Where(o => o.rc != 0)
            .Select(o => new {o.method, o.rc});

        dynamic currentValue = new ExpandoObject();
        currentValue.sweep_ms = additionalInputs[0].sweepMs;
        currentValue.invocation = new ExpandoObject();
        currentValue.invocation.count = additionalInputs[0].focas_invocations.Count;
        currentValue.invocation.max_method = max.method;
        currentValue.invocation.max_ms = max.invocationMs;
        currentValue.invocation.min_ms = min.invocationMs;
        currentValue.invocation.avg_ms = avg;
        currentValue.invocation.sum_ms = sum;
        currentValue.invocation.failed_methods = failedMethods;
        
        await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

        return new {veneer = this};
    }
}