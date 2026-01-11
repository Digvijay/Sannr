using System;
using System.Collections.Generic;
using Sannr;
using Xunit;

namespace Sannr.Tests
{
    [SannrReflect]
    public class TestUser
    {
        public int Id { get; set; }

        [Pii]
        public string? Email { get; set; }

        public List<string>? Tags { get; set; }

        public TestAddress? PrimaryAddress { get; set; }

        public List<TestAddress>? OtherAddresses { get; set; }
    }

    [SannrReflect]
    public class TestAddress
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public int ZipCode { get; set; }
    }

    [SannrReflect]
    public class PrimitiveTypesModel
    {
        public bool IsActive { get; set; }
        public double Score { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ReferenceId { get; set; }
    }

    public class StaticReflectionTests
    {
        [Fact]
        public void ShadowType_Metadata_Correct()
        {
            Assert.Equal("TestUser", TestUserShadow.Metadata.TypeName);
            // Id, Email, Tags, PrimaryAddress, OtherAddresses = 5 properties
            Assert.Equal(5, TestUserShadow.Metadata.PropertyCount);
        }

        [Fact]
        public void Accessors_Work()
        {
            var user = new TestUser { Id = 123, Email = "test@example.com" };

            // Getters
            Assert.Equal(123, TestUserShadow.GetId(user));
            Assert.Equal("test@example.com", TestUserShadow.GetEmail(user));

            // Setters
            TestUserShadow.SetId(user, 456);
            Assert.Equal(456, user.Id);

            TestUserShadow.SetEmail(user, "changed@example.com");
            Assert.Equal("changed@example.com", user.Email);
        }

        [Fact]
        public void Pii_Flags_Correct()
        {
            Assert.True(TestUserShadow.IsEmailPii);
            Assert.False(TestUserShadow.IsIdPii);
            Assert.False(TestUserShadow.IsTagsPii);
        }

        [Fact]
        public void DeepClone_Works_Simple()
        {
            var user = new TestUser
            {
                Id = 1,
                Email = "dev@sannr.com",
                Tags = new List<string> { "admin", "beta" }
            };

            var clone = TestUserShadow.DeepClone(user);

            Assert.NotNull(clone);
            Assert.NotSame(user, clone);
            Assert.Equal(user.Id, clone.Id);
            Assert.Equal(user.Email, clone.Email);

            Assert.NotNull(clone.Tags);
            Assert.NotSame(user.Tags, clone.Tags); // List should be a new instance
            Assert.Equal(user.Tags.Count, clone.Tags.Count);
            Assert.Equal("admin", clone.Tags[0]);
            Assert.Equal("beta", clone.Tags[1]);
        }

        [Fact]
        public void DeepClone_Works_NestedObject()
        {
            var user = new TestUser
            {
                Id = 2,
                PrimaryAddress = new TestAddress { Street = "123 Main St", City = "Tech City", ZipCode = 90210 }
            };

            var clone = TestUserShadow.DeepClone(user);

            Assert.NotNull(clone.PrimaryAddress);
            Assert.NotSame(user.PrimaryAddress, clone.PrimaryAddress); // Should be a deep copy
            Assert.Equal("123 Main St", clone.PrimaryAddress.Street);
            Assert.Equal("Tech City", clone.PrimaryAddress.City);
            Assert.Equal(90210, clone.PrimaryAddress.ZipCode);
        }

        [Fact]
        public void DeepClone_Works_ListOfObjects()
        {
            var user = new TestUser
            {
                OtherAddresses = new List<TestAddress>
                {
                    new TestAddress { City = "Oslo" },
                    new TestAddress { City = "Stockholm" }
                }
            };

            var clone = TestUserShadow.DeepClone(user);

            Assert.NotNull(clone.OtherAddresses);
            Assert.NotSame(user.OtherAddresses, clone.OtherAddresses);
            Assert.Equal(2, clone.OtherAddresses.Count);

            Assert.NotSame(user.OtherAddresses[0], clone.OtherAddresses[0]); // Items should be cloned too
            Assert.Equal("Oslo", clone.OtherAddresses[0].City);
            Assert.Equal("Stockholm", clone.OtherAddresses[1].City);
        }

        [Fact]
        public void DeepClone_Handles_Nulls()
        {
            var user = new TestUser { Id = 3 }; // Null email, null lists, null address

            var clone = TestUserShadow.DeepClone(user);

            Assert.Equal(user.Id, clone.Id);
            Assert.Null(clone.Email);
            Assert.Null(clone.Tags);
            Assert.Null(clone.PrimaryAddress);
            Assert.Null(clone.OtherAddresses);
        }

        [Fact]
        public void PrimitiveTypes_AccessorsAndClone()
        {
            var now = DateTime.Now;
            var guid = Guid.NewGuid();
            var model = new PrimitiveTypesModel
            {
                IsActive = true,
                Score = 99.5,
                CreatedAt = now,
                ReferenceId = guid
            };

            // Test Accessors
            Assert.True(PrimitiveTypesModelShadow.GetIsActive(model));
            Assert.Equal(99.5, PrimitiveTypesModelShadow.GetScore(model));
            Assert.Equal(now, PrimitiveTypesModelShadow.GetCreatedAt(model));
            Assert.Equal(guid, PrimitiveTypesModelShadow.GetReferenceId(model));

            // Test Clone
            var clone = PrimitiveTypesModelShadow.DeepClone(model);
            Assert.NotSame(model, clone);
            Assert.True(clone.IsActive);
            Assert.Equal(clone.Score, model.Score);
            Assert.Equal(clone.CreatedAt, model.CreatedAt);
            Assert.Equal(clone.ReferenceId, model.ReferenceId);
        }

        [Fact]
        public void Visit_Works()
        {
            var user = new TestUser { Id = 99, Email = "secret" };
            var visited = new Dictionary<string, object?>();
            var piiMap = new Dictionary<string, bool>();

            TestUserShadow.Visit(user, (name, val, isPii) =>
            {
                visited[name] = val;
                piiMap[name] = isPii;
            });

            Assert.Equal(99, visited["Id"]);
            Assert.Equal("secret", visited["Email"]);
            Assert.Null(visited["Tags"]);

            Assert.True(piiMap["Email"]);
            Assert.False(piiMap["Id"]);
        }
    }
}
