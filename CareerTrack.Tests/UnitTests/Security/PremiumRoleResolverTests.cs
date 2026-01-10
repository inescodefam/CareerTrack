using CareerTrack.Models;
using CareerTrack.Security;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerTrack.Tests.UnitTests.Security
{
    public class PremiumRoleResolverTests
    {
        [Fact]
        public void ResolveRole_ShouldReturnAdmin_WhenIsAdminIsTrue()
        {
            //Arrange
            PremiumRoleResolver resolver = new PremiumRoleResolver();
            User user = new User { IsAdmin = true };

            //Act
            string role = resolver.ResolveRole(user);

            //Assert
            role.Should().Be("Admin");
        }

        [Fact]
        public void ResolveRole_ShouldReturnPremiumUser_WhenEmailEndsWithPremiumCom()
        {
            PremiumRoleResolver resolver = new PremiumRoleResolver();

            User user = new User { Email = "mail@premium.com" };

            //Act
            string role = resolver.ResolveRole(user);

            //Assert
            role.Should().Be("PremiumUser");
        }

        [Fact]
        public void ResolveRole_ShouldReturnUser_WhenEmailNotEndsWithPremiumCom_And_IsAdminOsFalse()
        {
            PremiumRoleResolver resolver = new PremiumRoleResolver();

            User user = new User
            {
                Email = "mail@mail.com" ,
                IsAdmin = false
            };

            //Act
            string role = resolver.ResolveRole(user);

            //Assert
            role.Should().Be("User");
        }
    }
}
