﻿using System;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        private string ParseReason(string data)
        {
            const string errorMessage = "Nemůžu bezdůvodně odebrat přístup. Uveď důvod (`unverify {time} {reason} [{tags}]`)";

            if (data.StartsWith("<@"))
                throw new ArgumentException(errorMessage);

            var reason = data.Split("<@", StringSplitOptions.RemoveEmptyEntries)[0].Trim();

            if (string.IsNullOrEmpty(reason))
                throw new ArgumentException(errorMessage);

            return reason;
        }
    }
}
