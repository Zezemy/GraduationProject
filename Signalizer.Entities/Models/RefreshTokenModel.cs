﻿namespace Signalizer.Entities.Models
{
    public class RefreshTokenModel
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string RefreshToken { get; set; }
        public virtual UserModel User { get; set; }

    }
}