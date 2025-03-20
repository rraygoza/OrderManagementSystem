﻿using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace OrderService.Data
{
    public class OrderContext: DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options): base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
    }
}
