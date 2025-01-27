// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using EdFi.Ods.AdminApp.Management.Database.Commands;
using Moq;
using NUnit.Framework;
using Shouldly;
using System.Linq;
using EdFi.Admin.DataAccess.Contexts;
using EdFi.Admin.DataAccess.Models;
using EdFi.Ods.AdminApp.Web.Helpers;
using VendorUser = EdFi.Admin.DataAccess.Models.User;
using static EdFi.Ods.AdminApp.Management.Tests.Testing;

namespace EdFi.Ods.AdminApp.Management.Tests.Database.Commands
{
    [TestFixture]
    class EditVendorCommandTests : PlatformUsersContextTestBase
    {
        private int _vendorId;
        private int _vendorWithNoNameSpaceId;
        private const string OriginalVendorNamespacePrefix = "old namespace prefix";

        [SetUp]
        public void Init()
        {
            var originalVendor = new Vendor
            {
                VendorName = "old vendor name",
                VendorNamespacePrefixes = new List<VendorNamespacePrefix> { new VendorNamespacePrefix { NamespacePrefix = OriginalVendorNamespacePrefix } },
            };
            var originalVendorWithNoNameSpace = new Vendor
            {
                VendorName = "old vendor name",
                VendorNamespacePrefixes = new List<VendorNamespacePrefix>()
            };
            var originalVendorContact = new VendorUser
            {
                FullName = "old contact name",
                Email = "old contact email",
                Vendor = originalVendor
            };
            originalVendor.Users.Add(originalVendorContact);
            originalVendorWithNoNameSpace.Users.Add(originalVendorContact);

            Save(originalVendor, originalVendorWithNoNameSpace);
            _vendorId = originalVendor.VendorId;
            _vendorWithNoNameSpaceId = originalVendorWithNoNameSpace.VendorId;
        }

        [Test]
        public void ShouldEditVendorWithContact()
        {
            var newVendorData = new Mock<IEditVendor>();
            newVendorData.Setup(v => v.VendorId).Returns(_vendorId);
            newVendorData.Setup(v => v.Company).Returns("new vendor name");
            newVendorData.Setup(v => v.NamespacePrefixes).Returns("new namespace prefix");
            newVendorData.Setup(v => v.ContactName).Returns("new contact name");
            newVendorData.Setup(v => v.ContactEmailAddress).Returns("new contact email");

            Scoped<IUsersContext>(usersContext =>
            {
                var editVendorCommand = new EditVendorCommand(usersContext);
                editVendorCommand.Execute(newVendorData.Object);
            });
            
            Transaction(usersContext =>
            {
                var changedVendor = usersContext.Vendors.Single(v => v.VendorId == _vendorId);
                changedVendor.VendorName.ShouldBe("new vendor name");
                changedVendor.VendorNamespacePrefixes.First().NamespacePrefix.ShouldBe("new namespace prefix");
                changedVendor.Users.First().FullName.ShouldBe("new contact name");
                changedVendor.Users.First().Email.ShouldBe("new contact email");
            });
        }

        [Test]
        public void ShouldEditVendorWithNoNameSpacePrefix()
        {
            var newVendorData = new Mock<IEditVendor>();
            newVendorData.Setup(v => v.VendorId).Returns(_vendorId);
            newVendorData.Setup(v => v.Company).Returns("new vendor name");
            newVendorData.Setup(v => v.NamespacePrefixes).Returns(string.Empty);
            newVendorData.Setup(v => v.ContactName).Returns("new contact name");
            newVendorData.Setup(v => v.ContactEmailAddress).Returns("new contact email");

            Scoped<IUsersContext>(usersContext =>
            {
                var editVendorCommand = new EditVendorCommand(usersContext);
                editVendorCommand.Execute(newVendorData.Object);
            });

            Transaction(usersContext =>
            {
                var changedVendor = usersContext.Vendors.Single(v => v.VendorId == _vendorId);
                changedVendor.VendorName.ShouldBe("new vendor name");
                changedVendor.VendorNamespacePrefixes.ShouldBeEmpty();
                changedVendor.Users.First().FullName.ShouldBe("new contact name");
                changedVendor.Users.First().Email.ShouldBe("new contact email");
            });
        }

        [Test]
        public void ShouldEditVendorByAddingNewNameSpacePrefix()
        {
            var newVendorData = new Mock<IEditVendor>();
            newVendorData.Setup(v => v.VendorId).Returns(_vendorWithNoNameSpaceId);
            newVendorData.Setup(v => v.Company).Returns("new vendor name");
            newVendorData.Setup(v => v.NamespacePrefixes).Returns("new namespace prefix");
            newVendorData.Setup(v => v.ContactName).Returns("new contact name");
            newVendorData.Setup(v => v.ContactEmailAddress).Returns("new contact email");

            Scoped<IUsersContext>(usersContext =>
            {
                var editVendorCommand = new EditVendorCommand(usersContext);
                editVendorCommand.Execute(newVendorData.Object);
            });

            Transaction(usersContext =>
            {
                var changedVendor = usersContext.Vendors.Single(v => v.VendorId == _vendorWithNoNameSpaceId);
                changedVendor.VendorName.ShouldBe("new vendor name");
                changedVendor.VendorNamespacePrefixes.First().NamespacePrefix.ShouldBe("new namespace prefix");
                changedVendor.Users.First().FullName.ShouldBe("new contact name");
                changedVendor.Users.First().Email.ShouldBe("new contact email");
            });
        }

        [Test]
        public void ShouldEditVendorByAddingMultipleNameSpacePrefixes()
        {
            var newVendorData = new Mock<IEditVendor>();
            Transaction(usersContext =>
            {
                var originalVendor = usersContext.Vendors.Single(v => v.VendorId == _vendorId);
                originalVendor.VendorNamespacePrefixes.Single().NamespacePrefix.ShouldBe(OriginalVendorNamespacePrefix);
            });
            var newNamespacePrefixes = new List<string>
            {
                
                "http://www.test1.com/",
                "http://www.test2.com/",
                "http://www.test3.com/"
            };
            newVendorData.Setup(v => v.VendorId).Returns(_vendorId);
            newVendorData.Setup(v => v.Company).Returns("new vendor name");
            newVendorData.Setup(v => v.NamespacePrefixes).Returns(newNamespacePrefixes.ToDelimiterSeparated());
            newVendorData.Setup(v => v.ContactName).Returns("new contact name");
            newVendorData.Setup(v => v.ContactEmailAddress).Returns("new contact email");

            Scoped<IUsersContext>(usersContext =>
            {
                var editVendorCommand = new EditVendorCommand(usersContext);
                editVendorCommand.Execute(newVendorData.Object);
            });

            Transaction(usersContext =>
            {
                var changedVendor = usersContext.Vendors.Single(v => v.VendorId == _vendorId);
                changedVendor.VendorName.ShouldBe("new vendor name");
                changedVendor.VendorNamespacePrefixes.Select(x => x.NamespacePrefix).ShouldBe(newNamespacePrefixes);
                changedVendor.Users.First().FullName.ShouldBe("new contact name");
                changedVendor.Users.First().Email.ShouldBe("new contact email");
            });
        }

        [Test]
        public void ShouldEditVendorByRemovingNameSpacePrefix()
        {
            var newVendorData = new Mock<IEditVendor>();

            Transaction(usersContext =>
            {
                var originalVendor = usersContext.Vendors.Single(v => v.VendorId == _vendorId);
                originalVendor.VendorNamespacePrefixes.Single().NamespacePrefix.ShouldBe(OriginalVendorNamespacePrefix);
            });

            newVendorData.Setup(v => v.VendorId).Returns(_vendorId);
            newVendorData.Setup(v => v.Company).Returns("new vendor name");
            newVendorData.Setup(v => v.NamespacePrefixes).Returns("");
            newVendorData.Setup(v => v.ContactName).Returns("new contact name");
            newVendorData.Setup(v => v.ContactEmailAddress).Returns("new contact email");

            Scoped<IUsersContext>(usersContext =>
            {
                var editVendorCommand = new EditVendorCommand(usersContext);
                editVendorCommand.Execute(newVendorData.Object);
            });

            Transaction(usersContext =>
            {
                var changedVendor = usersContext.Vendors.Single(v => v.VendorId == _vendorId);
                changedVendor.VendorName.ShouldBe("new vendor name");
                changedVendor.VendorNamespacePrefixes.ShouldBeEmpty();
                changedVendor.Users.First().FullName.ShouldBe("new contact name");
                changedVendor.Users.First().Email.ShouldBe("new contact email");
            });
        }
    }
}
