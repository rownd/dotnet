using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RowndASPTestApp.Models;

public class RowndUserProfile
{
    public string? Id { get; set; }

    [Display(Name = "Name")]
    public string? Name { get; set; }
}