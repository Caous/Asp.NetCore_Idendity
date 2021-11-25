using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using WebApi.Domain;

namespace WebApi.Repository
{
    public class Context : IdentityDbContext<User, Role, int,
                                                IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public Context(DbContextOptions<Context> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<UserRole>(x =>
            {
                x.HasKey(ur => new { ur.UserId, ur.RoleId });
                x.HasOne(ur => ur.Role)
                    .WithMany(r => r.UsersRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
                x.HasOne(ur => ur.User)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.UserId)
                  .IsRequired();

            });

            builder.Entity<Organization>(org =>
            {
                org.ToTable("Organizations");
                org.HasKey(x => x.Id);

                org.HasMany<User>()
                .WithOne().HasForeignKey(x => x.OrgId).IsRequired(false);
            });
        }
    }
}
