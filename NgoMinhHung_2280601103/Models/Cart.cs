﻿namespace NgoMinhHung_2280601103.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
