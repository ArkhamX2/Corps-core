﻿using Corps.Server.DTO;
using MegaCorps.Core.Model.Enums;
using Newtonsoft.Json;

public class CardDescriptionInfo
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
}

public class AttackCardDescriptionInfo : CardDescriptionInfo
{
    [JsonProperty("attack_type")]
    public AttackType AttackType { get; set; }
    [JsonProperty("attack_type_name")]
    public required string AttackTypeName { get; set; }
}
public class DefenceCardDescriptionInfo : CardDescriptionInfo
{
    [JsonProperty("attack_types")]
    public required List<AttackType> AttackTypeList { get; set; }
}

public class DeveloperCardDescriptionInfo : CardDescriptionInfo
{
}

public class CardDirectionInfo
{
    [JsonProperty("card_direction")]
    public CardDirection Direction { get; set; }
    public required string Title { get; set; }
}
