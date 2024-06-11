using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MegaCorps.Core.Model.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AttackType
    {
        [EnumMember(Value = "trojan")]
        Trojan,
        [EnumMember(Value = "worm")]
        Worm,
        [EnumMember(Value = "DoS")]
        DoS,
        [EnumMember(Value = "scripting")]
        Scripting,
        [EnumMember(Value = "botnet")]
        Botnet,
        [EnumMember(Value = "fishing")]
        Fishing,
        [EnumMember(Value = "spy")]
        Spy
    }
}
