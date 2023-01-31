using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers;

public class Connection : Veneer
{
    public Connection(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(veneers,
        name, isCompound, isInternal)
    {
    }

    protected override async Task<dynamic> FirstAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        var currentValue = new {nativeInputs[0].success};

        await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);
        await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);

        return new {veneer = this};
    }

    protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
    {
        var currentValue = new
        {
            @event = additionalInputs[0],
            nativeInputs[0].success
        };

        await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

        if (currentValue.IsDifferentString((object) LastChangedValue))
            await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);

        return new {veneer = this};
    }
}