using api.Helpers.Attributes;

namespace api.Models.DbModels;

/**
 * Model not instantiated, but here to convey DB model structure
 */
public class UserRoomJunctions
{
    [EnforceName("user")] public int user { get; set; }

    [EnforceName("room")] public int room { get; set; }
}