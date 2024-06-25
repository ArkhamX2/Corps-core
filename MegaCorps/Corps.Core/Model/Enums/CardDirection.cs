using System.Runtime.Serialization;

namespace MegaCorps.Core.Model.Enums
{
    public enum CardDirection
    {
        [EnumMember(Value = "left")]
        Left,
        [EnumMember(Value = "right")]
        Right,
        [EnumMember(Value = "all")]
        All,
        [EnumMember(Value = "allbutnotme")]
        Allbutnotme
    }
}
