﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
namespace Signalizer.Models;

public partial class TradingPair
{
    public long Id { get; set; }

    public string Base { get; set; }

    public string Quote { get; set; }

    public virtual ICollection<SignalStrategy> SignalStrategies { get; set; } = new List<SignalStrategy>();
}