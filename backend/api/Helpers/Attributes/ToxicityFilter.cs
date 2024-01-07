using System.ComponentModel.DataAnnotations;

namespace api.Helpers.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ToxicityFilter : ValidationAttribute;