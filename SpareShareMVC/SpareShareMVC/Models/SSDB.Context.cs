﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace SpareShareMVC.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Assess> Assess { get; set; }
        public virtual DbSet<Checks> Checks { get; set; }
        public virtual DbSet<Comments> Comments { get; set; }
        public virtual DbSet<Quests> Quests { get; set; }
        public virtual DbSet<Things> Things { get; set; }
        public virtual DbSet<ThingsQuests> ThingsQuests { get; set; }
        public virtual DbSet<Users> Users { get; set; }
    }
}
