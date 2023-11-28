using System.ComponentModel.DataAnnotations;

namespace core.Models.WebsocketTransferObjects;

public class ClientWantsToEnterRoom : BaseTransferObject
{

    [Required, Range(1, int.MaxValue)]
    public int roomId { get; set; }
}