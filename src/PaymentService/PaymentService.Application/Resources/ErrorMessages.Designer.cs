using System.Globalization;
using System.Resources;

namespace PaymentService.Application.Resources;

internal static class ErrorMessages
{
    private static ResourceManager? _resourceManager;

    internal static ResourceManager ResourceManager
    {
        get
        {
            if (_resourceManager == null)
            {
                _resourceManager = new ResourceManager("PaymentService.Application.Resources.ErrorMessages", typeof(ErrorMessages).Assembly);
            }
            return _resourceManager;
        }
    }

    internal static string GetString(string name, CultureInfo? culture = null)
    {
        return ResourceManager.GetString(name, culture) ?? name;
    }
}
