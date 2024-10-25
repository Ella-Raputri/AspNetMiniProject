using System;
using Microsoft.EntityFrameworkCore;
using RentalCarBack.Model;


namespace RentalCarBack.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){
    }

    public DbSet<MsCar> MsCar{get; set; }
    public DbSet<MsCustomer> MsCustomer{get; set; }
    public DbSet<MsCarImages> MsCarImages{get; set; }
    public DbSet<TrPayment> TrPayment{get; set; }
    public DbSet<TrRental> TrRental{get; set; }
}
