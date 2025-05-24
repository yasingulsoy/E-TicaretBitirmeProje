﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETicaret.Core.Entities
{
    public class OrderLine:IEntity
    {
        public int Id { get; set; }
        [Display(Name = "Sipariş numarası")]
        public int OrderId { get; set; }
        [Display(Name = "Sipariş")]
        public Order? Order { get; set; }
        [Display(Name = "Ürün Numarası")]
        public int ProductId { get; set; }
        [Display(Name = "Ürün")]
        public Product? Product { get; set; }
        [Display(Name = "Ürün Miktarı")]
        public int Quantity { get; set; }
        [Display(Name = "Birim Fiyatı")]
        public decimal UnitPrice { get; set; }

    }
}
