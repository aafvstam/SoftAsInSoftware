﻿using Microsoft.AspNetCore.Components;
using softasinsoftware.Shared.Models;

namespace softasinsoftware.Web.Pages
{
    public partial class GearCard
    {
		[Parameter]
		public GearItem GearItem { get; set; } = default!;
	}
}
