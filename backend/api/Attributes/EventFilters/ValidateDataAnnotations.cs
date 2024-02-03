using System.ComponentModel.DataAnnotations;
using Fleck;
using lib;

namespace api.Attributes.EventFilters;

public class ValidateDataAnnotations : BaseEventFilter
{
    public override Task Handle<T>(IWebSocketConnection socket, T dto)
    {
        
            var validationContext = new ValidationContext(dto ?? throw new ArgumentNullException(nameof(dto)));
            Validator.ValidateObject(dto, validationContext, true);
            return Task.CompletedTask;
    }
}