﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StockAPI.Context;

#nullable disable

namespace StockAPI.Migrations
{
    [DbContext(typeof(StockDBContext))]
    [Migration("20240222213513_mig1")]
    partial class mig1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("StockAPI.Models.Stock", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Stocks");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Count = 200,
                            ProductId = 1
                        },
                        new
                        {
                            Id = 2,
                            Count = 300,
                            ProductId = 2
                        },
                        new
                        {
                            Id = 3,
                            Count = 50,
                            ProductId = 3
                        },
                        new
                        {
                            Id = 4,
                            Count = 10,
                            ProductId = 4
                        },
                        new
                        {
                            Id = 5,
                            Count = 60,
                            ProductId = 5
                        });
                });
#pragma warning restore 612, 618
        }
    }
}