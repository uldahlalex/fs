using System.ComponentModel.DataAnnotations;
using api.Helpers.ExtensionMethods;
using api.Models._3rdPartyTransferModels;
using Serilog;

namespace api.Helpers.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ToxicityFilter : ValidationAttribute
{
//in externalities
}