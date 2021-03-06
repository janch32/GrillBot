﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grillbot.Database.Entity
{
    [Table("Birthdays")]
    public class Birthday
    {
        [Key]
        [Column]
        [Required]
        public string ID { get; set; }

        [NotMapped]
        public ulong IDSnowflake
        {
            get => Convert.ToUInt64(ID);
            set => ID = value.ToString();
        }

        [Column]
        [Required]
        public DateTime Date { get; set; }

        [Column]
        [Required]
        public bool AcceptAge { get; set; }

        [Column]
        [Required]
        public string GuildID { get; set; }

        [NotMapped]
        public ulong GuildIDSnowflake
        {
            get => Convert.ToUInt64(GuildID);
            set => GuildID = value.ToString();
        }

        public int ComputeAge()
        {
            var today = DateTime.Today;
            var age = today.Year - Date.Year;
            if (Date.Date > today.AddYears(-age)) age--;

            return age;
        }
    }
}
