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
    public class DefaultRoleResolverTests
    {
        [Fact]
        public void ResolveRole_ShouldReturnUser_WhenIsAdminIsFalse()
        {
            //Arrange
            DefaultRoleResolver resolver = new DefaultRoleResolver();

            User user = new User { IsAdmin = false };

            //Act 
            string role = resolver.ResolveRole(user);

            //Assert
            role.Should().Be("User");
        }

        [Fact]
        public void ResolveRole_ShouldReturnAdmin_WhenIsAdminIsTrue()
        {
            //Arrange
            DefaultRoleResolver resolver = new DefaultRoleResolver();

            User user = new User { IsAdmin = true };

            //Act
            string role = resolver.ResolveRole(user);

            //Assert
            role.Should().Be("Admin");
        }
    }
}
